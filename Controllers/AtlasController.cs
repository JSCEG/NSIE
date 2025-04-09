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
    public class AtlasController : Controller
    {
        private readonly IRepositorioAtlas repositorioAtlas;


        public AtlasController(IRepositorioAtlas repositorioAtlas)
        {

            this.repositorioAtlas = repositorioAtlas;
        }




        public IActionResult AZEL()
        {
            return View();
        }




    }
}
