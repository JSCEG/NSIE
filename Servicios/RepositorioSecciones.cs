using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NSIE.Controllers;
using NSIE.Models;
using System.Text.Json;
using System.Collections.Generic;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Data;
using System.ComponentModel;

namespace NSIE.Servicios
{
    using NSIE.Models;

    public interface IRepositorioSecciones
    {
        Task<List<SeccionConModulos>> ObtenerTodasLasSeccionesAsync();
        Task<SeccionConModulos> ObtenerSeccionPorIdAsync(int id);
        Task CrearSeccionAsync(SeccionConModulos seccion);
        Task ActualizarSeccionAsync(SeccionConModulos seccion);
        Task EliminarSeccionAsync(int id);

        Task AgregarModuloAsync(Modulo modulo);
        Task ActualizarModuloAsync(Modulo modulo);
        Task EliminarModuloAsync(int id);

        Task<Modulo> ObtenerModuloPorIdAsync(int id);

    }
    public class RepositorioSecciones : IRepositorioSecciones
    {
        private readonly string _connectionString;

        public RepositorioSecciones(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<SeccionConModulos>> ObtenerTodasLasSeccionesAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            var secciones = await connection.QueryAsync<SeccionConModulos>("SELECT * FROM Secciones");
            foreach (var seccion in secciones)
            {
                var modulos = await connection.QueryAsync<Modulo>(
                    "SELECT * FROM Modulos WHERE SeccionId = @SeccionId",
                    new { SeccionId = seccion.Id });
                seccion.Modulos = modulos.ToList();
            }

            return secciones.ToList();
        }

        public async Task<SeccionConModulos> ObtenerSeccionPorIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var seccion = await connection.QueryFirstOrDefaultAsync<SeccionConModulos>(
                "SELECT * FROM Secciones WHERE Id = @Id",
                new { Id = id });

            if (seccion != null)
            {
                var modulos = await connection.QueryAsync<Modulo>(
                    "SELECT * FROM Modulos WHERE SeccionId = @SeccionId",
                    new { SeccionId = seccion.Id });
                seccion.Modulos = modulos.ToList();
            }

            return seccion;
        }

        public async Task CrearSeccionAsync(SeccionConModulos seccion)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO Secciones (Titulo, Articulos, FundamentoLegal, Descripcion, Ayuda, Objetivo, ResponsableNormativo, PublicoObjetivo, Activo)
                    VALUES (@Titulo, @Articulos, @FundamentoLegal, @Descripcion, @Ayuda, @Objetivo, @ResponsableNormativo, @PublicoObjetivo, @Activo)";

            await connection.ExecuteAsync(sql, seccion);
        }

        public async Task ActualizarSeccionAsync(SeccionConModulos seccion)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE Secciones SET 
                    Titulo = @Titulo,
                    Articulos = @Articulos,
                    FundamentoLegal = @FundamentoLegal,
                    Descripcion = @Descripcion,
                    Ayuda = @Ayuda,
                    Objetivo = @Objetivo,
                    ResponsableNormativo = @ResponsableNormativo,
                    PublicoObjetivo = @PublicoObjetivo,
                    Activo = @Activo
                    WHERE Id = @Id";

            await connection.ExecuteAsync(sql, seccion);
        }

        public async Task EliminarSeccionAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            // Primero eliminamos los módulos asociados
            await connection.ExecuteAsync("DELETE FROM Modulos WHERE SeccionId = @Id", new { Id = id });

            // Luego eliminamos la sección
            await connection.ExecuteAsync("DELETE FROM Secciones WHERE Id = @Id", new { Id = id });
        }

        public async Task AgregarModuloAsync(Modulo modulo)
        {
            Console.WriteLine(">>> Insertando módulo con título: " + modulo.Title);
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO Modulos (SeccionId, Title, FundamentoLegalModulo, Roles, NombresRoles, Perfiles, Etapa,
                    JustificacionOrden, AyudaContextual, Controller, Action, [Desc], Img, Btn, ElementosUI, AyudaVista, Orden, Activo)
                    VALUES (@SeccionId, @Title, @FundamentoLegalModulo, @Roles, @NombresRoles, @Perfiles, @Etapa,
                    @JustificacionOrden, @AyudaContextual, @Controller, @Action, @Desc, @Img, @Btn, @ElementosUI, @AyudaVista, @Orden, @Activo)";

            await connection.ExecuteAsync(sql, new
            {
                modulo.SeccionId,
                modulo.Title,
                modulo.FundamentoLegalModulo,
                modulo.Roles,
                modulo.NombresRoles,
                modulo.Perfiles,
                modulo.Etapa,
                modulo.JustificacionOrden,
                modulo.AyudaContextual,
                modulo.Controller,
                modulo.Action,
                modulo.Desc,
                modulo.Img,
                modulo.Btn,
                modulo.ElementosUI,
                modulo.AyudaVista,
                modulo.Orden,
                modulo.Activo
            });

        }

        public async Task ActualizarModuloAsync(Modulo modulo)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE Modulos SET
                    Title = @Title,
                    FundamentoLegalModulo = @FundamentoLegalModulo,
                    Roles = @Roles,
                    NombresRoles = @NombresRoles,
                    Perfiles = @Perfiles,
                    Etapa = @Etapa,
                    JustificacionOrden = @JustificacionOrden,
                    AyudaContextual = @AyudaContextual,
                    Controller = @Controller,
                    Action = @Action,
                   [Desc] = @Desc,
                    Img = @Img,
                    Btn = @Btn,
                    ElementosUI = @ElementosUI,
                    AyudaVista = @AyudaVista,
                    Orden = @Orden,
                    Activo = @Activo
                    WHERE Id = @Id";

            await connection.ExecuteAsync(sql, modulo);
        }

        public async Task EliminarModuloAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync("DELETE FROM Modulos WHERE Id = @Id", new { Id = id });
        }


        public async Task<Modulo> ObtenerModuloPorIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "SELECT * FROM Modulos WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<Modulo>(sql, new { Id = id });
        }

    }

}










