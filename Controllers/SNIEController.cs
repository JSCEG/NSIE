using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using NSIE.Models;
using NSIE.Servicios;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Configuration;




namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]


    public class SNIEController : Controller
    {
        public IActionResult AltaColaboradores()
        {
            return View();
        }

        public IActionResult CargaInformacion()
        {
            return View();
        }

        public IActionResult ValidacionFirma()
        {
            return View();
        }

        public IActionResult Semaforo()
        {
            return View();
        }
        public IActionResult BuzonNotificaciones()
        {
            // Aquí implementarías la lógica para obtener las notificaciones
            // desde tu base de datos o servicio
            return View();
        }

        public IActionResult BalanceNacional()
        {
            // Aquí implementarías la lógica para obtener las notificaciones
            // desde tu base de datos o servicio
            return View();
        }


    }


}
