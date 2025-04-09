using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using NSIE.Models;
using NSIE.Servicios;
using Newtonsoft.Json;


using System.Security.Cryptography;
using System.Text;


using System.Data;




namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class UsuariosController : Controller
    {

        private readonly IRepositorioUsuarios repositorioUsuarios;
        private readonly IRepositorioAcceso repositorioAcceso;

        public UsuariosController(IRepositorioUsuarios repositorioUsuarios, IRepositorioAcceso repositorioAcceso)
        {

            this.repositorioUsuarios = repositorioUsuarios;
            this.repositorioAcceso = repositorioAcceso;
        }


        //Obtiene la Lista Completa de los Usuarios de la Plataforma
        public async Task<IActionResult> AdministrarUsuarios()
        {
            var ListadeUsuarios = await repositorioUsuarios.ObtenerListadeUsuarios();

            if (ListadeUsuarios == null || !ListadeUsuarios.Any())
            {
                return NotFound(); // Manejar el caso en que regrese null o una lista vacía
            }
            return View(ListadeUsuarios);
        }



        //Obtiene el detalle del Usuario por ID para editarlo
        public async Task<IActionResult> Edit(int id)
        {
            var user = await repositorioUsuarios.ObtenerUsuarioPorId(id);

            if (user == null)
            {
                // Manejar el caso donde no se encuentra el usuario
                return NotFound();
            }


            // Mapear los datos del usuario al modelo de vista
            var model = new EditUserViewModel
            {
                IdUsuario = user.IdUsuario,
                Correo = user.Correo,
                Clave = user.Clave,
                Nombre = user.Nombre,
                Unidad_de_Adscripcion = user.Unidad_de_Adscripcion,
                Cargo = user.Cargo,
                SesionActiva = user.SesionActiva,
                UltimaActualizacion = user.UltimaActualizacion,
                RFC = user.RFC,
                Vigente = user.Vigente,
                ClaveEmpleado = user.ClaveEmpleado,
                HoraInicioSesion = user.HoraInicioSesion,
                // Rol_Id_RU = user.Rol_Id_RU,
                Mercado_ID = user.Mercado_ID,
                RolUsuario_Vigente = user.RolUsuario_Vigente,
                RolUsuario_QuienRegistro = user.RolUsuario_QuienRegistro,
                RolUsuario_FechaMod = user.RolUsuario_FechaMod,
                RolUsuario_Comentarios = user.RolUsuario_Comentarios,
                Rol_ID = user.Rol,
                Rol_Nombre = user.Rol_Nombre,
                Rol_Vigente = user.Rol_Vigente,
                Rol_FechaMod = user.Rol_FechaMod,
                Rol_Comentario = user.Rol_Comentario,
                Mercado_ID_M = user.Mercado_ID_M,
                Mercado_Nombre = user.Mercado_Nombre,
                Mercado_Vigente = user.Mercado_Vigente,
                Mercado_FechaMod = user.Mercado_FechaMod,
                Mercado_Comentario = user.Mercado_Comentario
            };

            // Obteniendo todos los roles desde tu repositorio
            var roles = await repositorioUsuarios.ObtenerTodosLosRoles();
            // Crear una lista de SelectListItems para el dropdown en la vista
            ViewBag.Roles = roles.Select(r => new SelectListItem
            {
                Value = r.Rol_ID.ToString(),
                Text = r.Rol_Nombre,
                Selected = r.Rol_ID == model.Rol_ID// Aquí se elige el rol actual
            }).ToList();

            // Obteniendo todos los mercados desde tu repositorio
            var mercados = await repositorioUsuarios.ObtenerTodosLosMercados(); // Asume que tienes un método así.
                                                                                // Crear una lista de SelectListItems para el dropdown en la vista
            ViewBag.Mercados = mercados.Select(m => new SelectListItem
            {
                Value = m.Mercado_ID.ToString(),
                Text = m.Mercado_Nombre,
                Selected = m.Mercado_ID == model.Mercado_ID_M // Aquí se elige el mercado actual.
            }).ToList();

            // Enviar el modelo de vista a la vista de edición
            return View(model);
        }



        // POST: /Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Mapear tu EditUserViewModel a UserViewModel y RolUsuarioViewModel 
            var user = new UserViewModel
            {
                // Asegúrate de mapear todas las propiedades necesarias...
                IdUsuario = model.IdUsuario,
                Nombre = model.Nombre,
                RFC = model.RFC,
                Correo = model.Correo,
                Cargo = model.Cargo,
                Unidad_de_Adscripcion = model.Unidad_de_Adscripcion,
                ClaveEmpleado = model.ClaveEmpleado,
                SesionActiva = model.SesionActiva,
                Vigente = model.Vigente,
                // ...
            };

            var rolUsuario = new RolesUsuarioViewModel
            {
                // Asegúrate de mapear todas las propiedades necesarias...
                IdUsuario = model.IdUsuario,
                Rol_ID = model.Rol_ID,
                Mercado_ID = model.Mercado_ID,
                RolUsuario_Comentarios = model.RolUsuario_Comentarios
                // ...
            };

            var userUpdateSuccess = await repositorioUsuarios.ActualizarUsuario(user);
            var rolUsuarioUpdateSuccess = await repositorioUsuarios.ActualizarRolUsuario(rolUsuario);

            if (userUpdateSuccess && rolUsuarioUpdateSuccess)
            {
                return RedirectToAction("AdministrarUsuarios");
            }
            else
            {
                // Manejar el caso en que la actualización falla...
                ModelState.AddModelError(string.Empty, "Hubo un error al actualizar la información del usuario.");
                return View(model);
            }
        }

        //Obtiene el detalle del Usuario por ID para editarlo
        public async Task<IActionResult> ModeloCuentaCompuesto(int id)
        {
            var user = await repositorioUsuarios.ObtenerUsuarioPorId(id);


            if (user == null)
            {
                // Manejar el caso donde no se encuentra el usuario
                return NotFound();
            }


            // Mapear los datos del usuario al modelo de vista
            var model = new ModeloCuentaCompuesto
            {
                CuentaUsuario = new Cuenta_UsuarioViewModel
                {
                    IdUsuario = user.IdUsuario,
                    Correo = user.Correo,
                    Clave = user.Clave,
                    Nombre = user.Nombre,
                    Unidad_de_Adscripcion = user.Unidad_de_Adscripcion,
                    Cargo = user.Cargo,
                    SesionActiva = user.SesionActiva,
                    UltimaActualizacion = user.UltimaActualizacion,
                    RFC = user.RFC,
                    Vigente = user.Vigente,
                    ClaveEmpleado = user.ClaveEmpleado,
                    HoraInicioSesion = user.HoraInicioSesion,
                    // Rol_Id_RU = user.Rol_Id_RU,
                    Mercado_ID = user.Mercado_ID,
                    RolUsuario_Vigente = user.RolUsuario_Vigente,
                    RolUsuario_QuienRegistro = user.RolUsuario_QuienRegistro,
                    RolUsuario_FechaMod = user.RolUsuario_FechaMod,
                    RolUsuario_Comentarios = user.RolUsuario_Comentarios,
                    Rol_ID = user.Rol,
                    Rol_Nombre = user.Rol_Nombre,
                    Rol_Vigente = user.Rol_Vigente,
                    Rol_FechaMod = user.Rol_FechaMod,
                    Rol_Comentario = user.Rol_Comentario,
                    Mercado_ID_M = user.Mercado_ID_M,
                    Mercado_Nombre = user.Mercado_Nombre,
                    Mercado_Vigente = user.Mercado_Vigente,
                    Mercado_FechaMod = user.Mercado_FechaMod,
                    Mercado_Comentario = user.Mercado_Comentario
                }
            };

            // Obteniendo todos los roles desde tu repositorio
            var roles = await repositorioUsuarios.ObtenerTodosLosRoles();
            // Crear una lista de SelectListItems para el dropdown en la vista
            ViewBag.Roles = roles.Select(r => new SelectListItem
            {
                Value = r.Rol_ID.ToString(),
                Text = r.Rol_Nombre,
                Selected = r.Rol_ID == model.CuentaUsuario.Rol_ID// Aquí se elige el rol actual
            }).ToList();

            // Obteniendo todos los mercados desde tu repositorio
            var mercados = await repositorioUsuarios.ObtenerTodosLosMercados(); // Asume que tienes un método así.
                                                                                // Crear una lista de SelectListItems para el dropdown en la vista
            ViewBag.Mercados = mercados.Select(m => new SelectListItem
            {
                Value = m.Mercado_ID.ToString(),
                Text = m.Mercado_Nombre,
                Selected = m.Mercado_ID == model.CuentaUsuario.Mercado_ID_M // Aquí se elige el mercado actual.
            }).ToList();

            // Enviar el modelo de vista a la vista de edición
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModeloCuentaCompuesto(ModeloCuentaCompuesto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Mapear tu EditUserViewModel a UserViewModel y RolUsuarioViewModel 
            var user = new UserViewModel
            {
                // Asegúrate de mapear todas las propiedades necesarias...
                IdUsuario = model.CuentaUsuario.IdUsuario,
                Nombre = model.CuentaUsuario.Nombre,
                RFC = model.CuentaUsuario.RFC,
                Correo = model.CuentaUsuario.Correo,
                Cargo = model.CuentaUsuario.Cargo,
                Unidad_de_Adscripcion = model.CuentaUsuario.Unidad_de_Adscripcion,
                ClaveEmpleado = model.CuentaUsuario.ClaveEmpleado,
                SesionActiva = model.CuentaUsuario.SesionActiva,
                // ...
            };

            var rolUsuario = new RolesUsuarioViewModel
            {
                // Asegúrate de mapear todas las propiedades necesarias...
                IdUsuario = model.CuentaUsuario.IdUsuario,
                Rol_ID = model.CuentaUsuario.Rol_ID,
                Mercado_ID = model.CuentaUsuario.Mercado_ID,
                RolUsuario_Comentarios = model.CuentaUsuario.RolUsuario_Comentarios
                // ...
            };

            var userUpdateSuccess = await repositorioUsuarios.ActualizarUsuario(user);
            var rolUsuarioUpdateSuccess = await repositorioUsuarios.ActualizarRolUsuario(rolUsuario);

            if (userUpdateSuccess && rolUsuarioUpdateSuccess)
            {
                return RedirectToAction("AdministrarUsuarios");
            }
            else
            {
                // Manejar el caso en que la actualización falla...
                ModelState.AddModelError(string.Empty, "Hubo un error al actualizar la información del usuario.");
                return View(model);
            }
        }

        public async Task<IActionResult> MonitoreoUsuario(int id, string nombre)
        {
            // Lógica para obtener los datos del usuario
            var userViewModel = await repositorioUsuarios.ObtenerUsuarioPorId(id); // Suponiendo que tienes una manera de obtener el usuario por ID
            var NombreUsuario = nombre;
            var usuario = ConvertToUsuario(userViewModel); // Convertir UserViewModel a Usuario

            // Lógica para obtener los datos de monitoreo
            var modeloCompuesto = await ObtenerModeloCompuesto(usuario, nombre);

            return Json(modeloCompuesto);
        }

        private Usuario ConvertToUsuario(UserViewModel userViewModel)
        {
            // Lógica para convertir UserViewModel a Usuario
            // Por ejemplo:
            var usuario = new Usuario
            {
                IdUsuario = userViewModel.IdUsuario,
                Correo = userViewModel.Correo,
                // Añade el resto de las propiedades necesarias...
            };

            return usuario;
        }

        private async Task<ModeloCuentaCompuesto> ObtenerModeloCompuesto(Usuario usuario, string nombre)
        {
            var totalAccesos = await repositorioAcceso.GetTotalAccessCountAsync();
            var fechaInicio = new DateTime(2023, 1, 1);
            var fechaFin = new DateTime(2030, 12, 31);
            var detallesAcceso = await repositorioAcceso.GetDetallesAccesoPorUsuarioAsync(nombre, fechaInicio, fechaFin);
            var totalAccesosPorTipo = await repositorioAcceso.GetTotalAccessCountByTypeAsync(fechaInicio, fechaFin);
            var ultimoAccesoPorUsuario = detallesAcceso
                .GroupBy(da => da.Nombre)
                .Select(g => new UltimoAccesoUsuario
                {
                    Nombre = g.Key,
                    UltimoAcceso = g.Max(x => x.FechaHoraLocal)
                })
                .ToList();


            // Construir el objeto ViewModel compuesto
            var modeloCompuesto = new ModeloCuentaCompuesto
            {
                CuentaUsuario = new Cuenta_UsuarioViewModel
                {
                    IdUsuario = usuario.IdUsuario,
                    Correo = usuario.Correo,
                    // Añade el resto de las propiedades necesarias...
                },
                CuentaMonitoreo = new Cuenta_MonitoreoViewModel
                {
                    TotalAccesos = totalAccesos,
                    DetallesAcceso = detallesAcceso,
                    TotalAccesosPorTipo = totalAccesosPorTipo,
                    UltimoAccesoPorUsuario = ultimoAccesoPorUsuario
                }
            };
            Console.WriteLine($"Datos Acceso: {modeloCompuesto}");

            return modeloCompuesto;
        }


        // Notificaciones a los Usuarios

        [HttpGet]
        public async Task<IActionResult> GetNotifications(int userId)
        {
            var totalUnreadCount = await repositorioUsuarios.GetUnreadNotificationsCountAsync(userId);
            var notifications = await repositorioUsuarios.GetNotificationsByUserIdAsync(userId);
            return Json(new { totalUnreadCount, notifications });
        }

        [HttpPost]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            try
            {
                bool success = await repositorioUsuarios.MarkNotificationAsReadAsync(notificationId);
                if (success)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo actualizar la notificación." });
                }
            }
            catch (Exception ex)
            {
                // Loguear el error (opcional)
                // _logger.LogError(ex, "Error al marcar la notificación como leída");
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult NotificationDetails(string titulo, string mensaje, DateTime fecha, string link)
        {
            var model = new NotificationDetails
            {
                Titulo = titulo,
                Mensaje = mensaje,
                Fecha = fecha,
                Link = link
            };

            return View(model);
        }

        public async Task<IActionResult> AllNotifications(int userId)
        {
            var notificaciones = await repositorioUsuarios.GetAllNotificationsAsync(userId);
            return View(notificaciones);
        }

        //PRUEBA DE CREACIÓN DE NOTIFICACIÓN
        [HttpPost]
        public async Task<IActionResult> GenerateTestNotification(int userId)
        {
            try
            {
                bool success = await repositorioUsuarios.GenerateNotificationsScriptAsync();

                return Json(new { success });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        // Muestra el Detalle de una notificación
        // Acción para mostrar los detalles de una notificación
        // Acción para mostrar los detalles de una notificación
        public async Task<IActionResult> DetalleNotificacion(int id)
        {
            var notification = await repositorioUsuarios.ObtenerNotificacionPorId(id);
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        //private readonly UserManager<UsuarioApp> userManager;

        //public UsuariosController(UserManager<UsuarioApp> userManager)
        //{
        //    this.userManager = userManager;
        //}

        // public IActionResult ListaUsuarios()
        // {
        //     return View();

        // }
        // public IActionResult Registro() 
        // { 
        //   return View();

        // }
        // [HttpPost]
        // public async Task<IActionResult> Registro(RegistroViewModel model)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         return View(model);
        //     }

        //     var usuario = new UsuarioApp() { Usuario = model.Usuario };
        //     var resultado = await userManager.CreateAsync(usuario, password: model.Contraseña);

        //     if (!resultado.Succeeded)
        //     {
        //         return RedirectToAction("Index", "Home");
        //     }
        //     else
        //     {
        //         foreach (var error in resultado.Errors)
        //         {
        //             ModelState.AddModelError(string.Empty, error.Description);
        //         }
        //         return View(model);
        //     }
        // }

        private async Task PoblarDropdowns()
        {
            var roles = await repositorioUsuarios.ObtenerTodosLosRoles();
            ViewBag.Roles = roles.Select(r => new SelectListItem
            {
                Value = r.Rol_ID.ToString(),
                Text = r.Rol_Nombre,
            }).ToList();

            var mercados = await repositorioUsuarios.ObtenerTodosLosMercados();
            ViewBag.Mercados = mercados.Select(m => new SelectListItem
            {
                Value = m.Mercado_ID.ToString(),
                Text = m.Mercado_Nombre,
            }).ToList();
        }



        //Registrar un Nuevo Usuario

        public async Task<IActionResult> NuevoUsuario()
        {

            await PoblarDropdowns();

            // Enviar el modelo de vista a la vista de edición
            return View();


        }



        [HttpPost]
        public async Task<IActionResult> NuevoUsuario(UserViewModel nuevoUsuario, int IDUsuario, int RolUsuario)
        {
            // Asegúrate de que las contraseñas no son nulas o cadenas vacías.
            if (string.IsNullOrWhiteSpace(nuevoUsuario.Clave) || string.IsNullOrWhiteSpace(nuevoUsuario.ConfirmarClave))
            {
                ViewData["Mensaje"] = "La contraseña no puede estar vacía.";
                return View();
            }

            // Validando Contraseñas.
            if (nuevoUsuario.Clave == nuevoUsuario.ConfirmarClave)
            {
                // Si los passwords son idénticos se procede a encriptarlos por seguridad.
                nuevoUsuario.Clave = ConvertirSha256(nuevoUsuario.Clave);
            }
            else
            {
                ViewData["Mensaje"] = "Las contraseñas no coinciden";
                await PoblarDropdowns();
                return View();
            }

            if (ModelState.IsValid)
            {
                // Crear usuario y obtener el ID generado.
                int newUserId = await repositorioUsuarios.RegistraUsuario(nuevoUsuario);

                // Verificar si el ID es válido.
                if (newUserId > 0)
                {
                    // Crear RolUsuario con el nuevo ID de usuario.
                    var rolUsuario = new RolesUsuarioViewModel
                    {
                        IdUsuario = newUserId,
                        Rol_ID = nuevoUsuario.Rol_ID,
                        Mercado_ID = nuevoUsuario.Mercado_ID,
                        RolUsuario_Comentarios = nuevoUsuario.RolUsuario_Comentarios,
                        RolUsuario_Vigente = RolUsuario,  // Asumiendo que es un entero.
                        RolUsuario_QuienRegistro = IDUsuario, // Asumiendo que es un entero.
                        RolUsuario_FechaMod = DateTime.Now // Fecha y hora actuales.
                    };

                    bool isRolUsuarioCreated = await repositorioUsuarios.RegistraRolUsuario(rolUsuario);

                    if (isRolUsuarioCreated)
                    {
                        // Redirigir o mostrar vista correspondiente si ambos registros son exitosos.
                        return RedirectToAction("AdministrarUsuarios");
                    }
                }
            }

            // Manejar fallas en la creación y mostrar errores al usuario.
            ModelState.AddModelError(string.Empty, "Error al crear usuario y/o rol de usuario.");
            await PoblarDropdowns();
            return View(nuevoUsuario);
        }



        public static string ConvertirSha256(string texto)
        {
            // using System.Text;
            // USAR LA REFERENCIA DE "System.Security.Cryptography"

            StringBuilder Sb = new StringBuilder();
            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(texto));
                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));

            }
            return Sb.ToString();

        }



        //Elimina Un Usuario
        public async Task<IActionResult> Eliminar(int id)
        {
            var wasDeleted = await repositorioUsuarios.EliminarUsuario(id);

            if (wasDeleted)
            {
                TempData["UserMessage"] = "Usuario eliminado exitosamente!";
                TempData["IsSuccess"] = true;
            }
            else
            {
                TempData["UserMessage"] = "Hubo un problema y el usuario no se pudo eliminar.";
                TempData["IsSuccess"] = false;
            }

            return RedirectToAction("AdministrarUsuarios"); // o la acción/vista que prefieras
        }


        //Ayuda

        public IActionResult Ayuda()
        {
            return View();
        }
        public async Task<IActionResult> Creditos()
        {
            var creditos = await repositorioUsuarios.ObtenerCreditos();
            return View(creditos.ToList()); // Asegúrate de que sea una lista
        }

        public IActionResult Guia_Consulta_Publica()
        {
            return View();
        }


        public async Task<IActionResult> Resena(int id)
        {
            var credito = await repositorioUsuarios.ObtenerCreditoPorId(id);
            if (credito == null)
            {
                return NotFound();
            }
            return View(credito);
        }


        public IActionResult Encuesta()
        {
            return View();
        }

        [ServiceFilter(typeof(ValidacionInputFiltro))]
        [HttpPost]
        public IActionResult EnviarEncuesta(Encuesta encuesta)
        {

            // Ahora sí verifica si el modelo es válido
            if (!ModelState.IsValid)
            {
                // Si el modelo no es válido, vuelve a mostrar la vista original con los errores
                return View(encuesta);
            }

            else
            {
                repositorioUsuarios.InsertarEncuesta(encuesta);
                return RedirectToAction("Gracias");
            }
        }


        public IActionResult Gracias()
        {
            return View();
        }

    }
}
