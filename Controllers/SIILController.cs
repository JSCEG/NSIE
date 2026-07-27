using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NSIE.Models;
using NSIE.Servicios.Interfaces;
using System;
using System.Threading.Tasks;
using System.Diagnostics;

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

                var existeBarreno = await _repositorioSIIL.BuscarBarreno();

                ViewBag.ExisteBarreno = existeBarreno;

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

                if (string.IsNullOrWhiteSpace(perfilJson))
                return Unauthorized();

                var perfilUsuario = string.IsNullOrWhiteSpace(perfilJson)
                    ? null
                    : Newtonsoft.Json.JsonConvert.DeserializeObject<PerfilUsuario>(perfilJson);

                if (perfilUsuario != null)
                {
                    modelo.ResponsableRegistro = perfilUsuario.Correo;
                    ViewData["NombreUsuario"] = perfilUsuario.Nombre;
                }

                // 🔐 Obtener datos reales desde base de datos
                var usuario = await _repositorioSIIL.ObtenerUsuarioPorCorreo(perfilUsuario.Correo);

                if (usuario == null)
                {
                    return Unauthorized();
                }

                // 🔐 Obtener rol del usuario
                var rol = await _repositorioSIIL.ObtenerRolPorUsuarioId(usuario.IdUsuario);

                // 🔐 Asignar valores seguros (IGNORANDO lo que venga del form)
                modelo.NombreResponsableRegistro = usuario.Nombre;
                modelo.ResponsableRegistro = usuario.Correo;
                modelo.Institucion = rol.Rol_Clave; // o el campo que represente la dependencia

                // Cargar estados y municipios para los dropdowns (pueden ser estáticos o desde BD)
                var estados = await _repositorioSIIL.ObtenerTodos();
                ViewBag.Estados = estados;

                // Cargar Barrenos
                var barrenos = await _repositorioSIIL.ObtenerBarrenos();
                ViewBag.Barrenos = barrenos;

                return View(modelo);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al cargar formulario de muestra: {ex.Message}");
                ViewBag.PronosticosPozos = new List<PronosticoPozo>();
                return View(new RegistroMuestra());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Barrenacion(string proyecto, string estado, string municipio)
        {
            ViewBag.Proyecto = proyecto;
            ViewBag.Estado = estado;
            ViewBag.Municipio = municipio;
            
            var model = new BarrenacionViewModel();
            return View(model);
        }

       [HttpGet]
        public async Task<IActionResult> ObtenerSiguienteConsecutivo(string proyecto, string zona)
        {
            try
            {
                int siguiente = await _repositorioSIIL.ObtenerSiguienteConsecutivo(proyecto, zona);

                return Ok(new
                {
                    success = true,
                    consecutivo = siguiente.ToString("D3")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener consecutivo: {ex.Message}");

                return StatusCode(500, new
                {
                    success = false,
                    error = "Error al obtener consecutivo"
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerSiguienteConsecutivoCaja(string barrenoID)
        {
            try
            {
                int siguiente = await _repositorioSIIL.ObtenerSiguienteConsecutivoCaja(barrenoID);

                return Ok(new
                {
                    success = true,
                    consecutivo = siguiente.ToString("D3")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener consecutivo de caja: {ex.Message}");

                return StatusCode(500, new
                {
                    success = false,
                    error = "Error al obtener consecutivo de caja"
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Barrenacion(BarrenacionViewModel model)
        {
            Console.WriteLine("ENTRÓ AL POST DE BARRENACIÓN");
            Console.WriteLine($"ModelState válido: {ModelState.IsValid}");
            // =========================
            // VALIDACIONES EXTRA
            // =========================

            // Validar que la fecha de inicio no sea futura
            if (model.FechaInicio.HasValue)
            {
                if (model.FechaInicio.Value.Date > DateTime.Now.Date)
                {
                    ModelState.AddModelError("FechaInicio",
                        "La fecha de inicio no puede ser posterior a la fecha del registro.");
                }
            }

            // 1️⃣ Validar fechas
            if (model.FechaInicio.HasValue && model.FechaFinalizacion.HasValue)
            {
                if (model.FechaFinalizacion < model.FechaInicio)
                {
                    ModelState.AddModelError("FechaFinalizacion",
                        "La fecha de finalización no puede ser menor que la fecha de inicio.");
                }
            }

            // 2️⃣ Validar recuperación de núcleo
            if (model.LongitudRecuperada.HasValue && model.LongitudPerforada.HasValue)
            {
                if (model.LongitudRecuperada > model.LongitudPerforada)
                {
                    ModelState.AddModelError("LongitudRecuperada",
                        "La longitud recuperada no puede ser mayor que la longitud perforada.");
                }
            }

            if (model.Intervalos == null || !model.Intervalos.Any())
            {
                ModelState.AddModelError("Intervalos",
                    "Debe registrar al menos un intervalo.");
            }

            // 3️⃣ Validar intervalos
            if (model.Intervalos != null)
            {
                foreach (var i in model.Intervalos)
                {
                    if (i.Hasta <= i.Desde)
                    {
                        ModelState.AddModelError("Intervalos",
                            $"El intervalo {i.Nombre} tiene valores inválidos.");
                    }
                }
            }

            if (model.LongitudRecuperada.HasValue && model.LongitudPerforada.HasValue)
            {
                var tcrCalculado = (model.LongitudRecuperada.Value / model.LongitudPerforada.Value) * 100m;

                if (model.TCR.HasValue && Math.Abs((double)(model.TCR.Value - tcrCalculado)) > 0.5)
                {
                    ModelState.AddModelError("TCR",
                        "El TCR no coincide con las longitudes registradas.");
                }
                if (model.TCR.HasValue && model.TCR < 50)
                {
                    if (string.IsNullOrWhiteSpace(model.TCRNotas))
                    {
                        ModelState.AddModelError("TCRNotas",
                            "Debe agregar una nota cuando el TCR es menor a 50%.");
                    }
                }
            }

            if (model.TipoBarrenacion == "Corte Diamante" &&
                string.IsNullOrWhiteSpace(model.RQD))
            {
                ModelState.AddModelError("RQD",
                    "El RQD es obligatorio para Corte Diamante.");
            }

            if (model.TipoBarrenacion != "Corte Diamante" && !string.IsNullOrWhiteSpace(model.RQD))
            {
                ModelState.AddModelError("RQD",
                    "El RQD solo aplica para barrenación con corte diamante.");
            }

            if (model.TipoBarrenacion == "OTRO")
            {
                if (string.IsNullOrWhiteSpace(model.TipoBarrenacionOtro))
                {
                    ModelState.AddModelError("TipoBarrenacion",
                        "Debe especificar el tipo de barrenación.");
                }
                else
                {
                    model.TipoBarrenacion = model.TipoBarrenacionOtro;
                }
            }

            if (model.ArchivoDescripcionNucleo == null)
            {
                ModelState.AddModelError("ArchivoDescripcionNucleo",
                    "Debe anexar el archivo de descripción del núcleo.");
            }

            if (model.FotografiasNucleo == null || !model.FotografiasNucleo.Any())
            {
                ModelState.AddModelError("FotografiasNucleo",
                    "Debe anexar al menos una fotografía del núcleo.");
            }

            // =========================
            // VALIDAR ARCHIVO EXCEL
            // =========================

            if (model.ArchivoDescripcionNucleo != null)
            {
                var extension = Path.GetExtension(model.ArchivoDescripcionNucleo.FileName).ToLower();

                if (extension != ".xlsx" && extension != ".csv")
                {
                    ModelState.AddModelError("ArchivoDescripcionNucleo",
                        "El archivo debe ser Excel (.xlsx) o CSV.");
                }

                if (model.ArchivoDescripcionNucleo.Length > 10 * 1024 * 1024)
                {
                    ModelState.AddModelError("ArchivoDescripcionNucleo",
                        "El archivo no debe superar los 10MB.");
                }
            }

            // =========================
            // VALIDAR FOTOS
            // =========================

            if (model.FotografiasNucleo != null && model.FotografiasNucleo.Any())
            {
                foreach (var foto in model.FotografiasNucleo)
                {
                    var extension = Path.GetExtension(foto.FileName).ToLower();

                    if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                    {
                        ModelState.AddModelError("FotografiasNucleo",
                            "Solo se permiten imágenes JPG o PNG.");
                        break;
                    }

                    if (foto.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("FotografiasNucleo",
                            "Cada imagen debe ser menor a 5MB.");
                        break;
                    }
                }
            }

            foreach (var error in ModelState)
            {
                foreach (var subError in error.Value.Errors)
                {
                    Console.WriteLine($"Campo: {error.Key} | Error: {subError.ErrorMessage}");
                }
            }

            Console.WriteLine("Validaciones completadas. Estado del modelo: " + ModelState.IsValid);

            // =========================
            // VALIDAR MODELO
            // =========================

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // =========================
                // VALIDAR BARRENO EXISTENTE
                // =========================

                bool existe = await _repositorioSIIL.ExisteBarreno(model.BarrenoID);

                if (existe)
                {
                    ModelState.AddModelError("BarrenoID", "Ya existe un barreno con ese ID.");
                    return View(model);
                }

                var carpetaFotos = Path.Combine(
                    "wwwroot/img/SILL/Barrenacion",
                    model.BarrenoID
                );

                if (!Directory.Exists(carpetaFotos))
                {
                    Directory.CreateDirectory(carpetaFotos);
                }

                var carpetaExcel = Path.Combine(
                    "wwwroot/archivos/SILL/Barrenacion",
                    model.BarrenoID
                );

                if (!Directory.Exists(carpetaExcel))
                {
                    Directory.CreateDirectory(carpetaExcel);
                }

                // =========================
                // GUARDAR ARCHIVO EXCEL
                // =========================

                string rutaExcel = null;

                if (model.ArchivoDescripcionNucleo != null)
                {
                    var nombreArchivo = $"{Guid.NewGuid()}_{model.ArchivoDescripcionNucleo.FileName}";

                    var ruta = Path.Combine(carpetaExcel, nombreArchivo);

                    using (var stream = new FileStream(ruta, FileMode.Create))
                    {
                        await model.ArchivoDescripcionNucleo.CopyToAsync(stream);
                    }

                    rutaExcel = $"/archivos/SILL/Barrenacion/{model.BarrenoID}/{nombreArchivo}";
                }

                Console.WriteLine("Archivos guardados correctamente. Ruta Excel: " + rutaExcel);

                // =========================
                // MAPEAR VIEWMODEL → ENTIDAD
                // =========================

                var entidad = new Barrenacion
                {
                    AnomaliaGravimetrica = model.AnomaliaGravimetrica,
                    Anomalia1 = model.Anomalia1,
                    Anomalia2 = model.Anomalia2,
                    Anomalia3 = model.Anomalia3,

                    Accesibilidad = model.Accesibilidad,
                    TipoTerreno = model.TipoTerreno,

                    BarrenoID = model.BarrenoID,
                    Perforista = model.Perforista,
                    Responsable = model.Responsable,
                    ResponsableNucleo = model.ResponsableNucleo, // ✅ NUEVO

                    Latitud = model.Latitud,
                    Longitud = model.Longitud,
                    Altitud = model.Altitud,

                    Azimut = model.Azimut,
                    Inclinacion = model.Inclinacion,

                    TipoBarrenacion = model.TipoBarrenacion,
                    FechaInicio = model.FechaInicio,
                    FechaFinalizacion = model.FechaFinalizacion,

                    LongitudPerforada = model.LongitudPerforada,
                    LongitudRecuperada = model.LongitudRecuperada,

                    Diametro = model.Diametro,
                    NumeroCajas = model.NumeroCajas,

                    RQD = model.RQD,
                    TCR = model.TCR,
                    TCRNotas = model.TCRNotas, // ✅ NUEVO

                    ArchivoDescripcionRuta = rutaExcel,
                    Observaciones = model.Observaciones,

                    FechaCreacion = DateTime.Now
                };

                // =========================
                // INSERTAR EN BD
                // =========================

                var barrenacionId = await _repositorioSIIL.Insertar<Barrenacion>(
                    "Barrenaciones",
                    entidad
                );

                entidad.Id = barrenacionId;

                foreach (var intervalo in model.Intervalos)
                {
                    var entidadIntervalo = new BarrenacionIntervalo
                    {
                        BarrenacionId = barrenacionId,
                        Nombre = intervalo.Nombre,
                        Desde = intervalo.Desde,
                        Hasta = intervalo.Hasta,
                        EsInteres = intervalo.EsInteres
                    };

                    await _repositorioSIIL.Insertar("BarrenacionIntervalos", entidadIntervalo);
                }

                for (int i = 1; i <= model.NumeroCajas; i++)
                {
                    var consecutivo = i.ToString("D3");

                    var cajaID = $"{model.BarrenoID}_C.{consecutivo}";

                    var caja = new BarrenacionCaja
                    {
                        BarrenacionId = barrenacionId,
                        CajaID = cajaID,
                        Consecutivo = i
                    };

                    await _repositorioSIIL.Insertar("BarrenacionCajas", caja);
                }

                // =========================
                // GUARDAR FOTOS
                // =========================

                if (model.FotografiasNucleo != null)
                {
                    foreach (var foto in model.FotografiasNucleo)
                    {
                        var nombreFoto = $"{Guid.NewGuid()}_{foto.FileName}";

                        var rutaFoto = Path.Combine(
                            carpetaFotos,
                            nombreFoto
                        );

                        using (var stream = new FileStream(rutaFoto, FileMode.Create))
                        {
                            await foto.CopyToAsync(stream);
                        }

                        var rutaBD = $"/img/SILL/Barrenacion/{model.BarrenoID}/{nombreFoto}";

                        await _repositorioSIIL.Insertar<BarrenacionFoto>(
                            "BarrenacionFotos",
                            new BarrenacionFoto
                            {
                                BarrenacionId = entidad.Id, // Asumiendo que el ID se genera al insertar la barrenación
                                RutaFoto = rutaBD
                            }
                        );
                    }
                }

                TempData["Success"] = "Registro de barrenación guardado correctamente.";

                return RedirectToAction("CrearMuestra", "SIIL");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completo al guardar barrenación");

                ModelState.AddModelError("",
                    $"Error real: {ex.Message}");

                return View(model);
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

            if (modelo.Fuente != "Arcilla" && modelo.Fuente != "Salmuera")
            {
                ModelState.AddModelError("Fuente", "Fuente inválida.");
            }

            if (modelo.Fuente == "Arcilla" && string.IsNullOrEmpty(modelo.Origen))
            {
                ModelState.AddModelError("Origen", "Debe seleccionar el origen.");
            }

            if (modelo.Fuente == "Arcilla" && modelo.FamiliaRoca == null)
            {
                ModelState.AddModelError("FamiliaRoca", "Debe seleccionar la familia de roca.");
            }
            
            if (modelo.Fuente == "Arcilla" &&
                modelo.Origen == "Barrenación")
            {
                if (modelo.Azimut == null || modelo.Azimut < 0 || modelo.Azimut > 360)
                    ModelState.AddModelError("Azimut", "Azimut debe estar entre 0 y 360.");

                if (modelo.Inclinacion == null || modelo.Inclinacion < 0 || modelo.Inclinacion > 90)
                    ModelState.AddModelError("Inclinacion", "Inclinación debe estar entre 0 y 90.");

                if (modelo.Largo == null || modelo.Largo <= 0)
                    ModelState.AddModelError("Largo", "El largo debe ser mayor a 0.");

                if (modelo.Diametro == null || modelo.Diametro <= 0)
                    ModelState.AddModelError("Diametro", "El diámetro debe ser mayor a 0.");

                if (string.IsNullOrEmpty(modelo.RQD))
                    ModelState.AddModelError("RQD", "Debe seleccionar la calidad de roca.");
            }

            if (modelo.Fuente == "Salmuera")
            {
                ModelState.Remove("Origen");

                if (modelo.PH == null || modelo.PH < 0 || modelo.PH > 14)
                    ModelState.AddModelError("PH", "El pH debe estar entre 0 y 14.");

                if (modelo.Temperatura == null || modelo.Temperatura < -10 || modelo.Temperatura > 200)
                    ModelState.AddModelError("Temperatura", "Temperatura fuera de rango lógico.");

                if (modelo.Conductividad == null || modelo.Conductividad <= 0)
                    ModelState.AddModelError("Conductividad", "La conductividad debe ser mayor a 0.");

                if (modelo.ProfundidadPozo == null || modelo.ProfundidadPozo <= 0)
                    ModelState.AddModelError("ProfundidadPozo", "La profundidad debe ser mayor a 0.");

                // 🔒 Forzar origen nulo
                modelo.Origen = null;
            }

            // Validar Estado y Municipio
            if (!int.TryParse(modelo.Estado, out int estadoId))
            {
                ModelState.AddModelError("Estado", "Estado inválido.");
            }
            else
            {
                var municipiosDelEstado = await _repositorioSIIL
                    .ObtenerMunicipiosPorEstado(estadoId);

                var municipioNormalizado = modelo.Municipio?.TrimStart('0');

                bool municipioExiste = municipiosDelEstado.Any(m =>
                    m.MunicipioID.ToString() == municipioNormalizado
                );

                if (!municipioExiste)
                {
                    ModelState.AddModelError("Municipio",
                        "El municipio no pertenece al estado seleccionado.");
                }
            }

            foreach (var error in ModelState)
            {
                var key = error.Key;
                var errors = error.Value.Errors;

                foreach (var e in errors)
                {
                    Console.WriteLine($"Error en {key}: {e.ErrorMessage}");
                }
            }

            if (!ModelState.IsValid)
            {
                ModelState.Remove("Estado");
                modelo.Estado = "0"; // o null si así lo manejas

                ViewBag.PronosticosPozos = await _repositorioSIIL
                    .ObtenerTodos<PronosticoPozo>("Pronostico_Pozos");

                ViewBag.Estados = await _repositorioSIIL.ObtenerTodos();

                return View(modelo);
            }
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
                    Console.WriteLine($"Intentando insertar muestra con ID: {idMuestra}");
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

        [HttpGet]
        public async Task<IActionResult> ObtenerMunicipiosPorEstado(int estadoId)
        {
            var municipios = await _repositorioSIIL.ObtenerMunicipiosPorEstado(estadoId);

            return Ok(municipios);
        }
    }
}
