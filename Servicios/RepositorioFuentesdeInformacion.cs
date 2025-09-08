using Microsoft.Data.SqlClient;
using NSIE.Models;
using Dapper;
using System.Data;
using NuGet.Protocol.Plugins;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // Agrega este using

namespace NSIE.Servicios
{
    public interface IRepositorioFuentesdeInformacion
    {
        Task<List<FuenteInformacionModel>> ObtenerFuentesAsync(string filtro = null);
        Task<List<FuenteTotalModel>> ObtenerTotalesPorFuenteAsync();
        Task<FuenteInformacionModel> ObtenerFuentePorIdAsync(int id);
    }



    public class RepositorioFuentesdeInformacion : IRepositorioFuentesdeInformacion
    {
        private readonly string connectionString;
        private readonly ILogger<RepositorioFuentesdeInformacion> _logger; // Agrega logger

        public RepositorioFuentesdeInformacion(IConfiguration configuration, ILogger<RepositorioFuentesdeInformacion> logger)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public async Task<List<FuenteInformacionModel>> ObtenerFuentesAsync(string filtro = null)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                _logger.LogInformation("Conexión abierta para ObtenerFuentesAsync con filtro: {Filtro}", filtro);

                var query = @"SELECT ID, Entidad, Tipo, Rubro, Etiqueta, Dato_Información AS Dato_Informacion,
                            Desagregación AS Desagregacion, Sub_desagregación AS Sub_desagregacion, Unidades,
                            Periodicidad_Corte_de_Información AS Periodicidad_Corte_de_Informacion, Fuente_Link, Comentario
                      FROM dbo.FuentesdeInformacion
                      WHERE (@Filtro IS NULL OR Entidad LIKE '%' + @Filtro + '%' OR Rubro LIKE '%' + @Filtro + '%')";
                var resultado = await connection.QueryAsync<FuenteInformacionModel>(query, new { Filtro = filtro });
                _logger.LogInformation("Consulta ejecutada. Registros obtenidos: {Count}", resultado.Count());
                return resultado.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ObtenerFuentesAsync: {Message}", ex.Message);
                throw;
            }
        }

        // Totales por fuente
        public async Task<List<FuenteTotalModel>> ObtenerTotalesPorFuenteAsync()
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"SELECT Entidad, COUNT(*) AS Total FROM dbo.FuentesdeInformacion GROUP BY Entidad";
                var resultado = await connection.QueryAsync<FuenteTotalModel>(query);
                return resultado.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ObtenerTotalesPorFuenteAsync: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<FuenteInformacionModel> ObtenerFuentePorIdAsync(int id)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"SELECT ID, Entidad, Tipo, Rubro, Etiqueta, Dato_Información AS Dato_Informacion,
                            Desagregación AS Desagregacion, Sub_desagregación AS Sub_desagregacion, Unidades,
                            Periodicidad_Corte_de_Información AS Periodicidad_Corte_de_Informacion, Fuente_Link, Comentario
                      FROM dbo.FuentesdeInformacion WHERE ID = @Id";
                var resultado = await connection.QueryFirstOrDefaultAsync<FuenteInformacionModel>(query, new { Id = id });
                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ObtenerFuentePorIdAsync: {Message}", ex.Message);
                throw;
            }
        }

    }

    public class FuenteTotalModel
    {
        public string Entidad { get; set; }
        public int Total { get; set; }
    }
}

