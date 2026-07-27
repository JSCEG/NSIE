// Controllers/SiliMxController.cs
using Dapper;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSIE.Models;
using NSIE.Servicios;
using Newtonsoft.Json;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class SiliMxController : Controller
    {
        private readonly IRepositorioSiliMx repositorioSiliMx;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public SiliMxController(IRepositorioSiliMx repositorioSiliMx, IWebHostEnvironment environment, IConfiguration configuration)
        {
            this.repositorioSiliMx = repositorioSiliMx;
            this._environment = environment;
            this._configuration = configuration;
        }

        // GET: /SiliMx/Index  ← punto de entrada al módulo
        public async Task<IActionResult> Index()
        {
            try
            {
                int idUsuario = ObtenerIdUsuarioSesion();
                var registro = await repositorioSiliMx.ObtenerRegistroPorUsuario(idUsuario);

                if (registro == null)
                {
                    // Sin registro → va a completar su perfil SILiMx
                    return RedirectToAction("Registro");
                }

                // Con registro → va a Bienvenida con sus datos
                return RedirectToAction("Bienvenida");
            }
            catch (Exception)
            {
                return RedirectToAction("Registro");
            }
        }

        // GET: /SiliMx/Registro
        public IActionResult Registro()
        {
            try
            {
                int idUsuario = ObtenerIdUsuarioSesion();
                ViewBag.IdUsuario = idUsuario;
            }
            catch (Exception)
            {
                // Si por algún motivo no se puede obtener el usuario,
                // AutorizacionFiltro ya debería haber redirigido antes de llegar aquí.
            }

            return View();
        }

        // POST: /SiliMx/GuardarRegistro
        [HttpPost]
        public async Task<IActionResult> GuardarRegistro([FromBody] SilimxRegistroRequest request)
        {
            try
            {
                int idUsuario = ObtenerIdUsuarioSesion();
                request.IdUsuario = idUsuario; // se fuerza desde la sesión, no se confía en el body

                var registroExistente = await repositorioSiliMx.ObtenerRegistroPorUsuario(idUsuario);

                if (registroExistente == null)
                {
                    string usuarioIdGenerado = GenerarUsuarioID();
                    var idRegistro = await repositorioSiliMx.GuardarRegistro(request, usuarioIdGenerado);

                    return Json(new { success = true, idRegistro, usuarioId = usuarioIdGenerado, esNuevo = true });
                }
                else
                {
                    await repositorioSiliMx.ActualizarRegistro(registroExistente.IdRegistro, request);

                    return Json(new { success = true, idRegistro = registroExistente.IdRegistro, esNuevo = false });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET: /SiliMx/ObtenerMiRegistro
        [HttpGet]
        public async Task<IActionResult> ObtenerMiRegistro()
        {
            try
            {
                int idUsuario = ObtenerIdUsuarioSesion();
                var registro = await repositorioSiliMx.ObtenerRegistroPorUsuario(idUsuario);

                if (registro == null)
                {
                    var datosBase = await repositorioSiliMx.ObtenerDatosBaseUsuario(idUsuario);
                    return Json(new { existe = false, datosBase });
                }

                return Json(new { existe = true, registro });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET: /SiliMx/Bienvenida
        public async Task<IActionResult> Bienvenida()
        {
            try
            {
                int idUsuario = ObtenerIdUsuarioSesion();
                var registro = await repositorioSiliMx.ObtenerRegistroPorUsuario(idUsuario);

                if (registro == null)
                {
                    return RedirectToAction("Registro");
                }

                // Obtenemos también los datos base para mostrar correo/nombre/institución
                var datosBase = await repositorioSiliMx.ObtenerDatosBaseUsuario(idUsuario);

                ViewBag.Correo = datosBase?.Correo ?? registro.CorreoInstitucional;
                ViewBag.Nombre = $"{registro.Nombres} {registro.ApellidoPaterno} {registro.ApellidoMaterno}".Trim();
                ViewBag.Institucion = registro.Institucion;
                ViewBag.Estatus = registro.Estatus;
            }
            catch (Exception)
            {
                return RedirectToAction("Registro");
            }

            return View();
        }

        // --------- Helper de sesión, alineado al patrón real del proyecto ---------

        private int ObtenerIdUsuarioSesion()
        {
            var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");

            if (string.IsNullOrEmpty(perfilUsuarioJson))
                throw new InvalidOperationException("No hay sesión activa.");

            var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);

            if (!int.TryParse(perfilUsuario.IdUsuario, out int idUsuario))
                throw new InvalidOperationException("El IdUsuario almacenado en sesión no es válido.");

            return idUsuario;
        }

        private string GenerarUsuarioID()
        {
            string fecha = DateTime.Now.ToString("yyMMdd");
            string letra = ((char)new Random().Next('A', 'Z' + 1)).ToString();
            return fecha + letra;
        }

        // Renombrar el Index portero que ya tenías — no cambia nada, solo añadimos Dashboard:

        // GET: /SiliMx/Dashboard  ← Pantalla de Inicio con el acordeón
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                int idUsuario = ObtenerIdUsuarioSesion();
                var registro = await repositorioSiliMx.ObtenerRegistroPorUsuario(idUsuario);

                if (registro == null)
                    return RedirectToAction("Registro");

                var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
                var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);

                ViewBag.NombreUsuario = $"{registro.Nombres} {registro.ApellidoPaterno}".Trim();
                ViewBag.EsDirectivo = registro?.PerfilAsignado?.ToLower().Contains("directivo") ?? false;
                // ↑ ajusta "PerfilAsignado" al nombre real de la propiedad en tu clase PerfilUsuario

                var estados =
                    await repositorioSiliMx.ObtenerTodos();

                ViewBag.Estados = estados;
            }
            catch (Exception)
            {
                return RedirectToAction("Registro");
            }

            return View();
        }

        [HttpGet]
        public async Task<JsonResult>
            ObtenerMunicipiosPorEstado(int estadoId)
        {
            var municipios =
                await repositorioSiliMx
                    .ObtenerMunicipiosPorEstado(estadoId);

            return Json(municipios);
        }

        // POST: /SiliMx/GuardarProyecto
        [HttpPost]
        public async Task<IActionResult> GuardarProyecto()
        {
            try
            {
                var form = Request.Form;

                var request = new SilimxProyectoRequest
                {
                    TipoProyecto = form["TipoProyecto"],
                    ProyectoID = form["ProyectoID"],
                    NombreProyecto = form["NombreProyecto"],
                    DescripcionObjetivo = form["DescripcionObjetivo"],
                    InstitucionEmpresa = form["InstitucionEmpresa"],
                    TipoInstitucion = form["TipoInstitucion"],
                    Financiamiento = form["Financiamiento"],
                    Responsable = form["Responsable"],
                    FechaInicio = form["FechaInicio"],
                    FechaFin = form["FechaFin"],
                    EstadoActual = form["EstadoActual"],
                    Avance = int.TryParse(form["Avance"], out var avance)
                        ? avance
                        : 0,
                    EntidadFederativa = form["EntidadFederativa"],
                    Municipio = form["Municipio"],
                    Localidad = form["Localidad"],
                    NotasFinales = form["NotasFinales"]
                };

                request.Permisos = string.IsNullOrEmpty(form["PermisosJson"])
                    ? new List<SilimxPermisoRequest>()
                    : JsonConvert.DeserializeObject<List<SilimxPermisoRequest>>(form["PermisosJson"]);

                int idUsuario = ObtenerIdUsuarioSesion();
                var registro = await repositorioSiliMx.ObtenerRegistroPorUsuario(idUsuario);

                if (registro == null)
                {
                    return BadRequest(new
                    {
                        message = "No existe registro SILiMx para el usuario."
                    });
                }

                string usuarioID = registro.UsuarioID; // o UsuarioID real

                string carpetaUsuario = Path.Combine(
                    _environment.WebRootPath,
                    "archivos",
                    "SILiMx",
                    usuarioID
                );

                Directory.CreateDirectory(carpetaUsuario);

                List<string> rutasArchivos = new();

                foreach (var archivo in Request.Form.Files)
                {
                    if (archivo.Length == 0)
                        continue;

                    string extension = Path.GetExtension(archivo.FileName).ToLower();

                    var extensionesPermitidas = new[]
                    {
                        ".xlsx",
                        ".xls"
                    };

                    if (!extensionesPermitidas.Contains(extension))
                    {
                        return BadRequest(new
                        {
                            message = $"Archivo no permitido: {archivo.FileName}"
                        });
                    }

                    string nombreArchivo =
                        $"{Guid.NewGuid()}{extension}";

                    string rutaCompleta =
                        Path.Combine(carpetaUsuario, nombreArchivo);

                    using var stream =
                        new FileStream(rutaCompleta, FileMode.Create);

                    await archivo.CopyToAsync(stream);

                    rutasArchivos.Add(
                        $"/archivos/SILiMx/{usuarioID}/{nombreArchivo}"
                    );
                }

                request.Evidencias =
                    string.Join("|", rutasArchivos);

                var idProyecto = await repositorioSiliMx.GuardarProyecto(request, idUsuario);

                if (request.Permisos != null && request.Permisos.Any())
                {
                    await repositorioSiliMx.GuardarPermisosProyecto(idProyecto, request.Permisos);
                }

                // Generar ID visible tipo XX-XXXX-XXXXXXX
                string idVisible = $"{request.TipoProyecto}-{request.ProyectoID.Trim()}-{usuarioID}";

                await repositorioSiliMx.ActualizarIdVisibleProyecto(idProyecto, idVisible);

                return Json(new { success = true, idProyecto, idVisible });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET: /SiliMx/CatalogoProyectos
        public async Task<IActionResult> CatalogoProyectos()
        {
            try
            {
                int idUsuario = ObtenerIdUsuarioSesion();
                var proyectos = await repositorioSiliMx.ObtenerCatalogoProyectos(idUsuario);

                return View(proyectos);
            }
            catch (Exception)
            {
                return RedirectToAction("Dashboard");
            }
        }

        // POST: /SiliMx/InhabilitarProyecto
        [HttpPost]
        public async Task<IActionResult> InhabilitarProyecto([FromBody] int idProyecto)
        {
            try
            {
                int idUsuario = ObtenerIdUsuarioSesion();
                await repositorioSiliMx.InhabilitarProyecto(idProyecto, idUsuario);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET: /SiliMx/ObtenerSelectorProyectos
        [HttpGet]
        public async Task<IActionResult> ObtenerSelectorProyectos()
        {
            try
            {
                int idUsuario = ObtenerIdUsuarioSesion();
                var proyectos = await repositorioSiliMx.ObtenerSelectorProyectos(idUsuario);
                return Json(proyectos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET: /SiliMx/ObtenerProyectoCompleto?idProyecto=X
        [HttpGet]
        public async Task<IActionResult> ObtenerProyectoCompleto(int idProyecto)
        {
            try
            {
                int idUsuario = ObtenerIdUsuarioSesion();
                var resultado = await repositorioSiliMx.ObtenerProyectoCompleto(idProyecto, idUsuario);

                if (resultado == null)
                    return NotFound(new { message = "Proyecto no encontrado." });

                return Json(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // POST: /SiliMx/GuardarActualizacion
        [HttpPost]
        public async Task<IActionResult> GuardarActualizacion()
        {
            try
            {
                int idUsuario = ObtenerIdUsuarioSesion();

                var form = Request.Form;

                int idProyecto = int.Parse(form["IdProyecto"]);

                var request = new SilimxActualizacionRequest
                {
                    IdProyecto = idProyecto,
                    DatosProyecto = new SilimxProyectoRequest
                    {
                        TipoProyecto = form["TipoProyecto"],
                        ProyectoID = form["ProyectoID"],
                        NombreProyecto = form["NombreProyecto"],
                        DescripcionObjetivo = form["DescripcionObjetivo"],
                        InstitucionEmpresa = form["InstitucionEmpresa"],
                        TipoInstitucion = form["TipoInstitucion"],
                        Financiamiento = form["Financiamiento"],
                        Responsable = form["Responsable"],
                        FechaInicio = form["FechaInicio"],
                        FechaFin = form["FechaFin"],
                        EstadoActual = form["EstadoActual"],
                        Avance = int.TryParse(form["Avance"], out var avance)
                        ? avance
                        : 0,
                        EntidadFederativa = form["EntidadFederativa"],
                        Municipio = form["Municipio"],
                        Localidad = form["Localidad"],
                        ResumenAvances = form["ResumenAvances"],
                        NotasFinales = form["NotasFinales"]
                    }
                };

                request.DatosProyecto.Permisos =
                    string.IsNullOrEmpty(form["PermisosJson"])
                        ? new List<SilimxPermisoRequest>()
                        : JsonConvert.DeserializeObject<List<SilimxPermisoRequest>>(form["PermisosJson"]);

                var registro = await repositorioSiliMx.ObtenerRegistroPorUsuario(idUsuario);

                if (registro == null)
                {
                    return BadRequest(new
                    {
                        message = "No existe registro SILiMx para el usuario."
                    });
                }

                string usuarioID = registro.UsuarioID;

                string carpetaUsuario = Path.Combine(
                    _environment.WebRootPath,
                    "archivos",
                    "SILiMx",
                    usuarioID
                );

                Directory.CreateDirectory(carpetaUsuario);

                List<string> rutasArchivos = new();

                foreach (var archivo in Request.Form.Files)
                {
                    if (archivo.Length == 0)
                        continue;

                    string extension = Path.GetExtension(archivo.FileName).ToLower();

                    var extensionesPermitidas = new[]
                    {
                        ".xlsx",
                        ".xls"
                    };

                    if (!extensionesPermitidas.Contains(extension))
                        return BadRequest(new
                        {
                            message = $"Archivo no permitido: {archivo.FileName}"
                        });

                    string nombre = $"{Guid.NewGuid()}{extension}";

                    string ruta = Path.Combine(carpetaUsuario, nombre);

                    using var stream = new FileStream(ruta, FileMode.Create);

                    await archivo.CopyToAsync(stream);

                    rutasArchivos.Add(
                        $"/archivos/SILiMx/{usuarioID}/{nombre}"
                    );
                }

                request.DatosProyecto.Evidencias =
                    string.Join("|", rutasArchivos);

                // Obtener el proyecto original para construir el nuevo IdVisible con prefijo "A."
                var original = await repositorioSiliMx.ObtenerProyectoCompleto(request.IdProyecto, idUsuario);
                if (original == null)
                    return NotFound(new { message = "Proyecto no encontrado." });

                string nuevoIdVisible = $"A.{original.Proyecto.IdVisible}";
                request.DatosProyecto.IdVisible = nuevoIdVisible;

                var idNuevo = await repositorioSiliMx.DuplicarYActualizarProyecto(
                    request.DatosProyecto, request.IdProyecto, idUsuario);

                // Guardar permisos del duplicado si los hay
                if (request.DatosProyecto.Permisos?.Any() == true)
                    await repositorioSiliMx.GuardarPermisosProyecto(idNuevo, request.DatosProyecto.Permisos);

                return Json(new { success = true, idProyecto = idNuevo, idVisible = nuevoIdVisible });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET: /SiliMx/ExploracionExplotacion
        public async Task<IActionResult> ExploracionExplotacion()
        {
            try
            {
                int idUsuario = ObtenerIdUsuarioSesion();
                var registro = await repositorioSiliMx.ObtenerRegistroPorUsuario(idUsuario);
                if (registro == null) return RedirectToAction("Registro");
            }
            catch (Exception)
            {
                return RedirectToAction("Dashboard");
            }

            return View();
        }

        // GET: /SiliMx/Exploracion
        public async Task<IActionResult> Exploracion()
        {
            try
            {
                int idUsuario = ObtenerIdUsuarioSesion();
                var registro = await repositorioSiliMx.ObtenerRegistroPorUsuario(idUsuario);
                if (registro == null) return RedirectToAction("Registro");
            }
            catch (Exception)
            {
                return RedirectToAction("Dashboard");
            }

            return View();
        }

        // Subsecciones de Exploración
        // GET: /SiliMx/Barrenos
        public async Task<IActionResult> Barrenos()
        {
            try
            {
                int idUsuario = ObtenerIdUsuarioSesion();
                // Pasar proyectos EE del usuario para el selector
                var proyectos = await repositorioSiliMx.ObtenerSelectorProyectos(idUsuario);
                ViewBag.Proyectos = proyectos;
                ViewBag.ProyectosEE = proyectos
                    .Where(p => p.IdVisible != null &&
                                (p.IdVisible.StartsWith("EE") || p.IdVisible.StartsWith("A.EE")))
                    .ToList();
            }
            catch (Exception)
            {
                return RedirectToAction("Exploracion");
            }

            var estados =
                    await repositorioSiliMx.ObtenerTodos();

                ViewBag.Estados = estados;

            return View();
        }

        // POST: /SiliMx/GuardarBarreno
        [HttpPost]
        public async Task<IActionResult> GuardarBarreno()
        {
            try
            {
                var form = Request.Form;

                var request = new SilimxBarrenoRequest
                {
                    IdProyecto = int.TryParse(form["b-IdProyecto"], out var idProyecto)
                        ? idProyecto
                        : 0,

                    ProyectoEEID = form["b-ProyectoEEID"],
                    BarrenoID = form["b-BarrenoID"],
                    Responsable = form["b-Responsable"],
                    EmpresaPerforista = form["b-EmpresaPerforista"],
                    ResponsableDescNucleo = form["b-ResponsableDescNucleo"],

                    Estado = form["b-Estado"],
                    Municipio = form["b-Municipio"],
                    Localidad = form["b-Localidad"],

                    LatitudN = decimal.TryParse(form["b-LatitudN"], out var lat)
                        ? lat
                        : null,

                    LongitudO = decimal.TryParse(form["b-LongitudO"], out var lon)
                        ? lon
                        : null,

                    Altitud = decimal.TryParse(form["b-Altitud"], out var alt)
                        ? alt
                        : null,

                    Azimut = int.TryParse(form["b-Azimut"], out var az)
                        ? az
                        : null,

                    Inclinacion = form["b-Inclinacion"],

                    TipoBarrenacion = form["b-TipoBarrenacion"],

                    FechaInicio = form["b-FechaInicio"],
                    FechaFin = form["b-FechaFin"],

                    LongitudPerforada =
                        decimal.TryParse(form["b-LongitudPerforada"], out var lp)
                            ? lp
                            : null,

                    LongitudRecuperada =
                        decimal.TryParse(form["b-LongitudRecuperada"], out var lr)
                            ? lr
                            : null,

                    Diametro =
                        int.TryParse(form["b-Diametro"], out var diam)
                            ? diam
                            : null,

                    RQD = form["b-RQD"],

                    NumeroCajas =
                        int.TryParse(form["b-NumeroCajas"], out var cajas)
                            ? cajas
                            : null,

                    NombrePrimeraCaja = form["b-NombrePrimeraCaja"],

                    Gravimetria =
                        decimal.TryParse(form["b-Gravimetria"], out var grav)
                            ? grav
                            : null,

                    NotasFinales = form["b-NotasFinales"]
                };

                request.Intervalos =
                    string.IsNullOrEmpty(form["b-Intervalos"])
                        ? new List<SilimxBarrenoIntervaloRequest>()
                        : JsonConvert.DeserializeObject<List<SilimxBarrenoIntervaloRequest>>
                            (form["b-Intervalos"]);

                int idUsuario = ObtenerIdUsuarioSesion();

                // Obtener UsuarioID del registro SILiMx
                var registro = await repositorioSiliMx.ObtenerRegistroPorUsuario(idUsuario);
                string usuarioID = registro?.UsuarioID ?? idUsuario.ToString();

                // Generar BarrenoIDVisible: B.BarrenoID-UsuarioID
                string barrenoIdVisible = $"B.{request.BarrenoID}-{usuarioID}";

                string carpetaUsuario = Path.Combine(
                    _environment.WebRootPath,
                    "archivos",
                    "SILiMx",
                    usuarioID,
                    "Barrenos"
                );

                Directory.CreateDirectory(carpetaUsuario);

                List<string> rutas = new();

                foreach (var archivo in Request.Form.Files)
                {
                    if (archivo.Length == 0)
                        continue;

                    string extension =
                        Path.GetExtension(archivo.FileName).ToLower();

                    var permitidas = new[]
                    {
                        ".xlsx",
                        ".xls",
                        ".png",
                        ".jpg",
                        ".jpeg"
                    };

                    if (!permitidas.Contains(extension))
                    {
                        return BadRequest(new
                        {
                            message = $"Archivo no permitido: {archivo.FileName}"
                        });
                    }

                    string nombre =
                        $"{Guid.NewGuid()}{extension}";

                    string ruta =
                        Path.Combine(carpetaUsuario, nombre);

                    using var stream =
                        new FileStream(ruta, FileMode.Create);

                    await archivo.CopyToAsync(stream);

                    rutas.Add(
                        $"/archivos/SILiMx/{usuarioID}/Barrenos/{nombre}"
                    );
                }

                request.Evidencias = string.Join("|", rutas);

                var idVisible = await repositorioSiliMx.GuardarBarreno(
                    request, idUsuario, barrenoIdVisible);

                // Recuperar el IdBarreno recién insertado para guardar los intervalos
                var barrenos = await repositorioSiliMx.ObtenerCatalogoBarrenos(idUsuario);
                var barrenoNuevo = barrenos.FirstOrDefault(b => b.BarrenoIDVisible == barrenoIdVisible);

                if (barrenoNuevo != null && request.Intervalos?.Any() == true)
                {
                    await repositorioSiliMx.GuardarIntervalosBarreno(
                        barrenoNuevo.IdBarreno, request.Intervalos);
                }

                return Json(new { success = true, barrenoIdVisible });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET: /SiliMx/CatalogoBarrenos
        public async Task<IActionResult> CatalogoBarrenos()
        {
            try
            {
                int idUsuario = ObtenerIdUsuarioSesion();
                var barrenos = await repositorioSiliMx.ObtenerCatalogoBarrenos(idUsuario);
                return View(barrenos);
            }
            catch (Exception)
            {
                return RedirectToAction("Exploracion");
            }
        }

        // POST: /SiliMx/InhabilitarBarreno
        [HttpPost]
        public async Task<IActionResult> InhabilitarBarreno([FromBody] int idBarreno)
        {
            try
            {
                int idUsuario = ObtenerIdUsuarioSesion();
                await repositorioSiliMx.InhabilitarBarreno(idBarreno, idUsuario);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        // GET: /SiliMx/Muestras
        public async Task<IActionResult> Muestras()
        {
            try
            {
                int idUsuario = ObtenerIdUsuarioSesion();
                var proyectos = await repositorioSiliMx.ObtenerSelectorProyectos(idUsuario);
                var barrenos = await repositorioSiliMx.ObtenerSelectorBarrenos(idUsuario);
                ViewBag.Proyectos = proyectos;
                ViewBag.ProyectosEE = proyectos
                    .Where(p => p.IdVisible != null &&
                        (p.IdVisible.StartsWith("EE") || p.IdVisible.StartsWith("A.EE")))
                    .ToList();
                ViewBag.Barrenos = barrenos.ToList();

            }
            catch (Exception)
            {
                return RedirectToAction("Exploracion");
            }

            var estados =
                    await repositorioSiliMx.ObtenerTodos();

            ViewBag.Estados = estados;

            return View();
        }

        // POST: /SiliMx/GuardarMuestra
        [HttpPost]
        public async Task<IActionResult> GuardarMuestra()
        {
            try
            {
                int idUsuario = ObtenerIdUsuarioSesion();

                var form = Request.Form;

                var request = new SilimxMuestraRequest
                {
                    IdProyecto = int.TryParse(form["m-IdProyecto"], out var idProyecto)
                        ? idProyecto
                        : (int?)null,

                    ProyectoEEID = form["m-ProyectoEEID"],
                    MuestraID = form["m-MuestraID"],
                    TamanoMuestra = decimal.TryParse(form["m-TamanoMuestra"], out var tam)
                        ? tam
                        : (decimal?)null,

                    ResponsableMuestreo = form["m-ResponsableMuestreo"],
                    FechaMuestreo = form["m-FechaMuestreo"],
                    Estado = form["m-Estado"],
                    Municipio = form["m-Municipio"],
                    Localidad = form["m-Localidad"],

                    Fuente = form["m-Fuente"],
                    TipoCampo = form["m-TipoCampo"],

                    //=========================
                    // SALMUERA PETROLERO
                    //=========================

                    SP_Campo = form["sp-Campo"],
                    SP_Pozo = form["sp-Pozo"],
                    SP_LatitudN = decimal.TryParse(form["sp-LatitudN"], out var spLat)
                        ? spLat
                        : (decimal?)null,

                    SP_LongitudO = decimal.TryParse(form["sp-LongitudO"], out var spLon)
                        ? spLon
                        : (decimal?)null,

                    SP_Altitud = decimal.TryParse(form["sp-Altitud"], out var spAlt)
                        ? spAlt
                        : (decimal?)null,

                    SP_Profundidad = decimal.TryParse(form["sp-Profundidad"], out var spProf)
                        ? spProf
                        : (decimal?)null,

                    SP_IntervaloInicio = decimal.TryParse(form["sp-IntervaloInicio"], out var spIni)
                        ? spIni
                        : (decimal?)null,

                    SP_IntervaloFin = decimal.TryParse(form["sp-IntervaloFin"], out var spFin)
                        ? spFin
                        : (decimal?)null,

                    SP_CorteAgua = int.TryParse(form["sp-CorteAgua"], out var spCorte)
                        ? spCorte
                        : (int?)null,

                    SP_Presion = decimal.TryParse(form["sp-Presion"], out var spPresion)
                        ? spPresion
                        : (decimal?)null,

                    SP_Temperatura = int.TryParse(form["sp-Temperatura"], out var spTemp)
                        ? spTemp
                        : (int?)null,

                    SP_pH = decimal.TryParse(form["sp-pH"], out var sppH)
                        ? sppH
                        : (decimal?)null,

                    SP_OxigenoDisuelto = decimal.TryParse(form["sp-OxigenoDisuelto"], out var spOD)
                        ? spOD
                        : (decimal?)null,

                    //=========================
                    // SALMUERA GEOTÉRMICO
                    //=========================

                    SG_Campo = form["sg-Campo"],
                    SG_PuntoMuestra = form["sg-PuntoMuestra"],

                    SG_LatitudN = decimal.TryParse(form["sg-LatitudN"], out var sgLat)
                        ? sgLat
                        : (decimal?)null,

                    SG_LongitudO = decimal.TryParse(form["sg-LongitudO"], out var sgLon)
                        ? sgLon
                        : (decimal?)null,

                    SG_Altitud = decimal.TryParse(form["sg-Altitud"], out var sgAlt)
                        ? sgAlt
                        : (decimal?)null,

                    SG_Profundidad = decimal.TryParse(form["sg-Profundidad"], out var sgProf)
                        ? sgProf
                        : (decimal?)null,

                    SG_Temperatura = int.TryParse(form["sg-Temperatura"], out var sgTemp)
                        ? sgTemp
                        : (int?)null,

                    SG_pH = decimal.TryParse(form["sg-pH"], out var sgpH)
                        ? sgpH
                        : (decimal?)null,

                    SG_OxigenoDisuelto = decimal.TryParse(form["sg-OxigenoDisuelto"], out var sgOD)
                        ? sgOD
                        : (decimal?)null,

                    //=========================
                    // ARCILLA SUPERFICIAL
                    //=========================

                    AS_LatitudN = decimal.TryParse(form["as-LatitudN"], out var asLat)
                        ? asLat
                        : (decimal?)null,

                    AS_LongitudO = decimal.TryParse(form["as-LongitudO"], out var asLon)
                        ? asLon
                        : (decimal?)null,

                    AS_Altitud = decimal.TryParse(form["as-Altitud"], out var asAlt)
                        ? asAlt
                        : (decimal?)null,

                    //=========================
                    // ARCILLA PROFUNDA
                    //=========================

                    AP_BarrenoID = form["ap-BarrenoID"],
                    AP_IntervaloID = form["ap-IntervaloID"],

                    AP_DesdeM = decimal.TryParse(form["ap-DesdeM"], out var apDesde)
                        ? apDesde
                        : (decimal?)null,

                    AP_HastaM = decimal.TryParse(form["ap-HastaM"], out var apHasta)
                        ? apHasta
                        : (decimal?)null,

                    AP_Estructuras = form["ap-Estructuras"],

                    //=========================
                    // DESCRIPCIÓN ROCA
                    //=========================

                    AR_Litologia = form["ar-Litologia"],
                    AR_Color = form["ar-Color"],
                    AR_Textura = form["ar-Textura"],
                    AR_NotasDescripcion = form["ar-NotasDescripcion"],

                    //=========================
                    // FINAL
                    //=========================

                    NotasFinales = form["m-NotasFinales"]
                };

                // Obtener UsuarioID del registro SILiMx
                var registro = await repositorioSiliMx.ObtenerRegistroPorUsuario(idUsuario);
                string usuarioID = registro?.UsuarioID ?? idUsuario.ToString();

                //====================================================
                // SUBIR ARCHIVOS
                //====================================================

                string carpetaUsuario = Path.Combine(
                    _environment.WebRootPath,
                    "archivos",
                    "SILiMx",
                    usuarioID);

                Directory.CreateDirectory(carpetaUsuario);

                List<string> rutasArchivos = new();

                foreach (var archivo in Request.Form.Files)
                {
                    if (archivo.Length == 0)
                        continue;

                    string extension = Path.GetExtension(archivo.FileName).ToLower();

                    var permitidas = new[]
                    {
                        ".xlsx",
                        ".xls",
                        ".png",
                        ".jpg",
                        ".jpeg"
                    };

                    if (!permitidas.Contains(extension))
                    {
                        return BadRequest(new
                        {
                            message = $"Archivo no permitido: {archivo.FileName}"
                        });
                    }

                    string nombre = $"{Guid.NewGuid()}{extension}";

                    string ruta = Path.Combine(carpetaUsuario, nombre);

                    using var stream = new FileStream(ruta, FileMode.Create);

                    await archivo.CopyToAsync(stream);

                    rutasArchivos.Add(
                        $"/archivos/SILiMx/{usuarioID}/{nombre}"
                    );
                }

                request.Evidencias = string.Join("|", rutasArchivos);

                //====================================================
                // GENERAR ID VISIBLE
                //====================================================

                string sigla = (request.Fuente, request.TipoCampo) switch
                {
                    ("Salmuera", "Petrolero") => "SP",
                    ("Salmuera", "Geotermico") => "SG",
                    ("Arcilla", "Superficial") => "AS",
                    ("Arcilla", "Profunda") => "AP",
                    _ => "XX"
                };

                string muestraIdVisible =
                    $"{sigla}-{request.MuestraID}-{usuarioID}";

                var (idMuestra, idVisible) =
                    await repositorioSiliMx.GuardarMuestra(
                        request,
                        idUsuario,
                        muestraIdVisible);

                return Json(new
                {
                    success = true,
                    idMuestra,
                    muestraIdVisible = idVisible
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }

        // GET: /SiliMx/CatalogoMuestras
        public async Task<IActionResult> CatalogoMuestras()
        {
            try
            {
                int idUsuario = ObtenerIdUsuarioSesion();
                var muestras = await repositorioSiliMx.ObtenerCatalogoMuestras(idUsuario);
                return View(muestras);
            }
            catch (Exception)
            {
                return RedirectToAction("Exploracion");
            }
        }

        // POST: /SiliMx/InhabilitarMuestra
        [HttpPost]
        public async Task<IActionResult> InhabilitarMuestra([FromBody] int idMuestra)
        {
            try
            {
                int idUsuario = ObtenerIdUsuarioSesion();
                await repositorioSiliMx.InhabilitarMuestra(idMuestra, idUsuario);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET: /SiliMx/ObtenerIntervalosBarreno?idBarreno=X
        [HttpGet]
        public async Task<IActionResult> ObtenerIntervalosBarreno(int idBarreno)
        {
            try
            {
                int idUsuario = ObtenerIdUsuarioSesion();
                using var connection = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                var intervalos = await connection.QueryAsync(@"
                    SELECT i.Nombre, i.DesdeM, i.HastaM
                    FROM [dbo].[SILIMX_BARRENO_INTERVALO] i
                    INNER JOIN [dbo].[SILIMX_BARRENO] b ON i.IdBarreno = b.IdBarreno
                    WHERE i.IdBarreno = @IdBarreno AND b.IdUsuario = @IdUsuario",
                    new { IdBarreno = idBarreno, IdUsuario = idUsuario });

                return Json(intervalos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        public IActionResult Analisis() => View();

        // Catálogos de subsecciones
        public IActionResult CatalogoAnalisis() => View();

        // Placeholder Explotacion (la construimos después)
        public IActionResult Explotacion() => View();

        // Placeholder para los otros módulos (los construimos después)
        public IActionResult Beneficio() => View();
        public IActionResult Manufactura() => View();
        public IActionResult EconomiaCircular() => View();

    }
}