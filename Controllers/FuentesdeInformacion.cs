using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSIE.Models;
using NSIE.Servicios;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class FuentesdeInformacion : Controller
    {
        private readonly IRepositorioFuentesdeInformacion repositorioFuentesdeInformacion;
        private readonly ILogger<FuentesdeInformacion> _logger;

        public FuentesdeInformacion(IRepositorioFuentesdeInformacion repositorioFuentesdeInformacion, ILogger<FuentesdeInformacion> logger)
        {
            this.repositorioFuentesdeInformacion = repositorioFuentesdeInformacion;
            _logger = logger;
        }

        // Vista principal del tablero
        public IActionResult Fuentes()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerFuentes(string filtro)
        {
            try
            {
                var datos = await repositorioFuentesdeInformacion.ObtenerFuentesAsync(filtro);
                return Json(datos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener fuentes con filtro: {Filtro}", filtro);
                return Json(new { error = "Error interno del servidor al obtener fuentes." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTotalesPorFuente()
        {
            try
            {
                var totales = await repositorioFuentesdeInformacion.ObtenerTotalesPorFuenteAsync();
                return Json(totales);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener totales por fuente");
                return Json(new { error = "Error interno del servidor al obtener totales." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerFuentePorId(int id)
        {
            try
            {
                var fuente = await repositorioFuentesdeInformacion.ObtenerFuentePorIdAsync(id);
                return Json(fuente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener fuente por ID: {Id}", id);
                return Json(new { error = "Error interno del servidor al obtener fuente." });
            }
        }
    }
}
