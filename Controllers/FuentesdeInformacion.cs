using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSIE.Models;
using NSIE.Servicios;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class FuentesdeInformacion : Controller
    {
        private readonly IRepositorioFuentesdeInformacion repositorioFuentesdeInformacion;
        private readonly IRepositorioBitacora repositorioBitacora;
        private readonly ILogger<FuentesdeInformacion> _logger;

        public FuentesdeInformacion(IRepositorioFuentesdeInformacion repositorioFuentesdeInformacion,
                                   IRepositorioBitacora repositorioBitacora,
                                   ILogger<FuentesdeInformacion> logger)
        {
            this.repositorioFuentesdeInformacion = repositorioFuentesdeInformacion;
            this.repositorioBitacora = repositorioBitacora;
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

        // Nueva vista para gestionar fuentes de una entidad específica
        public IActionResult GestionarFuentes(string entidad)
        {
            if (string.IsNullOrEmpty(entidad))
            {
                return RedirectToAction("Fuentes");
            }

            ViewBag.Entidad = entidad;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarEntidad([FromBody] ActualizarEntidadRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Json(new { success = false, error = "Datos de solicitud requeridos." });
                }

                if (string.IsNullOrWhiteSpace(request.EntidadOriginal) || string.IsNullOrWhiteSpace(request.EntidadNueva))
                {
                    return Json(new { success = false, error = "Los nombres de entidad son requeridos." });
                }

                if (request.EntidadOriginal == request.EntidadNueva)
                {
                    return Json(new { success = false, error = "El nuevo nombre debe ser diferente al actual." });
                }

                var resultado = await repositorioFuentesdeInformacion.ActualizarEntidadAsync(request.EntidadOriginal, request.EntidadNueva);

                if (resultado)
                {
                    return Json(new { success = true, message = $"Entidad actualizada de '{request.EntidadOriginal}' a '{request.EntidadNueva}'" });
                }
                else
                {
                    return Json(new { success = false, error = "No se encontró la entidad a actualizar." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar entidad de {request?.EntidadOriginal} a {request?.EntidadNueva}");
                return Json(new { success = false, error = "Error interno del servidor al actualizar entidad." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EliminarEntidad([FromBody] EliminarEntidadRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Json(new { success = false, error = "Datos de solicitud requeridos." });
                }

                if (string.IsNullOrWhiteSpace(request.Entidad))
                {
                    return Json(new { success = false, error = "El nombre de la entidad es requerido." });
                }

                var resultado = await repositorioFuentesdeInformacion.EliminarEntidadAsync(request.Entidad);

                if (resultado)
                {
                    return Json(new { success = true, message = $"Entidad '{request.Entidad}' eliminada exitosamente" });
                }
                else
                {
                    return Json(new { success = false, error = "No se encontró la entidad a eliminar." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar entidad: {request?.Entidad}");
                return Json(new { success = false, error = "Error interno del servidor al eliminar entidad." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearEntidad([FromBody] CrearEntidadRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Json(new { success = false, error = "Datos de solicitud requeridos." });
                }

                if (string.IsNullOrWhiteSpace(request.Nombre))
                {
                    return Json(new { success = false, error = "El nombre de la entidad es requerido." });
                }

                if (request.Nombre.Length < 3)
                {
                    return Json(new { success = false, error = "El nombre debe tener al menos 3 caracteres." });
                }

                var resultado = await repositorioFuentesdeInformacion.CrearEntidadAsync(request.Nombre);

                if (resultado)
                {
                    return Json(new { success = true, message = $"Entidad '{request.Nombre}' creada exitosamente" });
                }
                else
                {
                    return Json(new { success = false, error = "La entidad ya existe o no se pudo crear." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al crear entidad: {request?.Nombre}");
                return Json(new { success = false, error = "Error interno del servidor al crear entidad." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearFuente([FromBody] FuenteInformacionModel nuevaFuente)
        {
            try
            {
                if (nuevaFuente == null)
                {
                    return Json(new { success = false, error = "Los datos de la fuente son requeridos." });
                }

                // --- INICIO: Validar sesión de usuario ---
                var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
                if (string.IsNullOrEmpty(perfilUsuarioJson))
                {
                    return Json(new { success = false, error = "Sesión de usuario no encontrada. Por favor, inicie sesión de nuevo." });
                }
                var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);
                // --- FIN: Validar sesión de usuario ---

                // Validaciones básicas
                if (string.IsNullOrWhiteSpace(nuevaFuente.Entidad))
                {
                    return Json(new { success = false, error = "La entidad es requerida." });
                }

                if (string.IsNullOrWhiteSpace(nuevaFuente.Tipo))
                {
                    return Json(new { success = false, error = "El tipo es requerido." });
                }

                if (string.IsNullOrWhiteSpace(nuevaFuente.Rubro))
                {
                    return Json(new { success = false, error = "El rubro es requerido." });
                }

                if (string.IsNullOrWhiteSpace(nuevaFuente.Etiqueta))
                {
                    return Json(new { success = false, error = "La etiqueta es requerida." });
                }

                // Crear la fuente en la base de datos
                var idNuevaFuente = await repositorioFuentesdeInformacion.CrearFuenteAsync(nuevaFuente);

                if (idNuevaFuente > 0)
                {
                    // Registrar la actividad en la bitácora centralizada
                    await repositorioBitacora.RegistrarActividadAsync(
                        userId: perfilUsuario.IdUsuario.ToString(),
                        userName: perfilUsuario.Nombre,
                        actionName: "Crear",
                        controllerName: "FuentesdeInformacion",
                        pageName: "Fuentes de Información",
                        tipo: "Entidad",
                        elemento: "Fuente",
                        idElemento: idNuevaFuente.ToString(),
                        valor: nuevaFuente.Etiqueta,
                        additionalData: JsonConvert.SerializeObject(new
                        {
                            Entidad = nuevaFuente.Entidad,
                            Tipo = nuevaFuente.Tipo,
                            Rubro = nuevaFuente.Rubro
                        })
                    );

                    return Json(new
                    {
                        success = true,
                        message = $"Fuente '{nuevaFuente.Etiqueta}' creada exitosamente en la entidad '{nuevaFuente.Entidad}'",
                        id = idNuevaFuente
                    });
                }
                else
                {
                    return Json(new { success = false, error = "No se pudo crear la fuente." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear fuente: {Etiqueta}", nuevaFuente?.Etiqueta ?? "null");
                return Json(new { success = false, error = "Error interno del servidor al crear fuente." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerHistorialFuente(int id)
        {
            try
            {
                var historial = await repositorioBitacora.ObtenerHistorialPorElementoAsync(
                    pageName: "Fuentes de Información",
                    idElemento: id.ToString()
                );

                return Json(new { success = true, data = historial });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de fuente: {Id}", id);
                return Json(new { success = false, error = "Error interno del servidor al obtener historial." });
            }
        }
    }

    // DTOs para las requests
    public class ActualizarEntidadRequest
    {
        public string EntidadOriginal { get; set; }
        public string EntidadNueva { get; set; }
    }

    public class EliminarEntidadRequest
    {
        public string Entidad { get; set; }
    }

    public class CrearEntidadRequest
    {
        public string Nombre { get; set; }
    }
}
