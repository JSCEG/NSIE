using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NSIE.Models;
using NSIE.Servicios.Interfaces;
using System;
using System.Threading.Tasks;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class SIILController : Controller
    {
        private readonly ILogger<SIILController> _logger;
        private readonly IRepositorioSIIL _repositorioSIIL;
        private readonly string _connectionString;

        public SIILController(
            ILogger<SIILController> logger,
            IRepositorioSIIL repositorioSIIL,
            IConfiguration configuration)
        {
            _logger = logger;
            _repositorioSIIL = repositorioSIIL;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Obtener datos para KPIs del dashboard
                var muestras = await _repositorioSIIL.ObtenerTodos<RegistroMuestra>("Registro_Muestras");
                var pronosticos = await _repositorioSIIL.ObtenerTodos<PronosticoPozo>("Pronostico_Pozos");

                // Calcular KPIs
                var pozosPrioritarios = pronosticos.Count(p =>
                    p.PosibilidadIntervalo != null &&
                    (p.PosibilidadIntervalo.Contains("Muy alto") ||
                     p.PosibilidadIntervalo.Contains("Alto") ||
                     p.PosibilidadIntervalo.StartsWith("1.") ||
                     p.PosibilidadIntervalo.StartsWith("2."))
                );

                // Por ahora, asumimos que todas las muestras están analizadas
                // En el futuro, esto vendría de la tabla Calculo_Litio_Probable
                var muestrasAnalizadas = muestras.Count(m => m.IdPozo_Pronostico_FK != null);

                // Obtener perfil de usuario
                var perfilJson = HttpContext.Session.GetString("PerfilUsuario");
                var perfilUsuario = Newtonsoft.Json.JsonConvert.DeserializeObject<PerfilUsuario>(perfilJson);

                var modelo = new DashboardSIILViewModel
                {
                    TotalMuestras = muestras.Count,
                    PozosPrioritarios = pozosPrioritarios,
                    MuestrasAnalizadas = muestrasAnalizadas,
                    NombreUsuario = perfilUsuario?.Nombre ?? "Usuario",
                    RolUsuario = perfilUsuario?.Rol ?? "Consulta",
                    PronosticosPozos = pronosticos
                };

                return View(modelo);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al cargar dashboard SIIL: {ex.Message}");
                // Retornar vista con datos vacíos en caso de error
                return View(new DashboardSIILViewModel());
            }
        }

        /// <summary>
        /// Vista para crear una nueva muestra (Formulario 1)
        /// GET: /SIIL/CrearMuestra
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CrearMuestra()
        {
            try
            {
                // Cargar pronósticos disponibles para el dropdown
                var pronosticos = await _repositorioSIIL.ObtenerTodos<PronosticoPozo>("Pronostico_Pozos");
                ViewBag.PronosticosPozos = pronosticos;
                var modelo = new RegistroMuestra();

                var perfilJson = HttpContext.Session.GetString("PerfilUsuario");
                var perfilUsuario = string.IsNullOrWhiteSpace(perfilJson)
                    ? null
                    : Newtonsoft.Json.JsonConvert.DeserializeObject<PerfilUsuario>(perfilJson);

                if (perfilUsuario != null)
                {
                    modelo.ResponsableRegistro = perfilUsuario.Correo;
                    ViewData["NombreUsuario"] = perfilUsuario.Nombre;
                }

                return View(modelo);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al cargar formulario de muestra: {ex.Message}");
                ViewBag.PronosticosPozos = new List<PronosticoPozo>();
                return View(new RegistroMuestra());
            }
        }

        /// <summary>
        /// Endpoint de ejemplo para generar un ID de muestra
        /// GET: /SIIL/GenerarIdMuestra?fuente=Arcilla&origen=Barrenación
        /// </summary>
        [HttpGet]
        public IActionResult GenerarIdMuestra(string fuente, string origen)
        {
            try
            {
                var idGenerado = _repositorioSIIL.GenerarIdMuestra(fuente, origen, DateTime.Now);
                return Ok(new
                {
                    success = true,
                    idMuestra = idGenerado,
                    fuente = fuente,
                    origen = origen,
                    timestamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm")
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Procesar formulario de nueva muestra (Formulario 1)
        /// POST: /SIIL/CrearMuestra
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearMuestra(RegistroMuestra modelo)
        {
            try
            {
                // 1. Validar modelo básico
                if (string.IsNullOrEmpty(modelo.Fuente))
                {
                    ModelState.AddModelError("Fuente", "La fuente es requerida");
                    ViewBag.PronosticosPozos = await _repositorioSIIL.ObtenerTodos<PronosticoPozo>("Pronostico_Pozos");
                    return View(modelo);
                }

                // 2. Generar ID único automáticamente
                var idMuestra = _repositorioSIIL.GenerarIdMuestra(
                    fuente: modelo.Fuente,
                    origen: modelo.Origen ?? "Prospectiva",
                    fecha: DateTime.Now
                );

                // 3. Asignar valores al modelo
                modelo.IdMuestra = idMuestra;
                modelo.FechaRegistro = DateTime.Now;
                modelo.FechaCreacion = DateTime.Now;
                modelo.FechaActualizacion = DateTime.Now;

                // 4. Insertar en base de datos (ignora el resultado ya que IdMuestra es string)
                try
                {
                    await _repositorioSIIL.Insertar<RegistroMuestra>("Registro_Muestras", modelo);
                    _logger.LogInformation($"Muestra creada exitosamente: {idMuestra}");
                }
                catch (Exception dbEx)
                {
                    _logger.LogError($"Error SQL al insertar muestra: {dbEx.Message}");
                    _logger.LogError($"Stack trace: {dbEx.StackTrace}");
                    throw new Exception($"Error en base de datos: {dbEx.Message}", dbEx);
                }

                TempData["SuccessMessage"] = $"Muestra registrada exitosamente con ID: {idMuestra}";
                return RedirectToAction("Index");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Error de validación al crear muestra: {ex.Message}");
                ModelState.AddModelError("", ex.Message);
                ViewBag.PronosticosPozos = await _repositorioSIIL.ObtenerTodos<PronosticoPozo>("Pronostico_Pozos");
                return View(modelo);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear muestra: {ex.Message}");
                ModelState.AddModelError("", "Error interno al procesar la solicitud");
                ViewBag.PronosticosPozos = await _repositorioSIIL.ObtenerTodos<PronosticoPozo>("Pronostico_Pozos");
                return View(modelo);
            }
        }

        /// <summary>
        /// Obtener todas las muestras registradas
        /// GET: /SIIL/ObtenerMuestras
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerMuestras()
        {
            try
            {
                var muestras = await _repositorioSIIL.ObtenerTodos<RegistroMuestra>("Registro_Muestras");
                return Ok(new
                {
                    success = true,
                    total = muestras.Count,
                    muestras = muestras
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener muestras: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Error al obtener muestras"
                });
            }
        }

        /// <summary>
        /// Obtener todos los pronósticos registrados
        /// GET: /SIIL/ObtenerPronosticos
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerPronosticos()
        {
            try
            {
                var pronosticos = await _repositorioSIIL.ObtenerTodos<PronosticoPozo>("Pronostico_Pozos");
                return Ok(new
                {
                    success = true,
                    total = pronosticos.Count,
                    pronosticos = pronosticos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener pronósticos: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Error al obtener pronósticos"
                });
            }
        }
    }
}
