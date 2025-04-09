using System.Data;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NSIE.Models;
using NSIE.Servicios;
using Newtonsoft.Json.Linq;
using System.Net;
using Newtonsoft.Json;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    [AutorizacionFiltro]
    public class HomeController : Controller

    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRepositorioProyectos repositorioProyectos;
        private readonly IServicioEmail servicioEmail;
        private readonly string connectionString;
        private readonly HttpClient _client;
        private readonly IRepositorioHome repositorioHome;



        public HomeController(ILogger<HomeController> logger, IRepositorioProyectos repositorioProyectos, IConfiguration configuration, IServicioEmail servicioEmail, IRepositorioHome repositorioHome)
        {
            _logger = logger;
            this.repositorioProyectos = repositorioProyectos;
            this.servicioEmail = servicioEmail;
            connectionString = configuration.GetConnectionString("DefaultConnection");
            _client = new HttpClient();
            this.repositorioHome = repositorioHome;
        }



        public IActionResult ListaUsuarios()
        {
            try
            {

                #region Obtener DataTable de SQL - Lista Usuarios

                //string cadena = "Data Source=cre-db-srv-1.cre.gob.mx;Initial Catalog=cre-db-2;Persist Security Info=True; User ID=dgpe;Password=059ff$5k-zPWksW*";
                //string cadenap = "Data Source=DESKTOP-9F04CH6;Initial Catalog=cre-db-2;Persist Security Info=True;User ID=sa;Password=Javiereg32";
                SqlConnection con = new SqlConnection(connectionString);//cadenap
                string consulta = "SELECT [id],[Usuario],[Email],[EmailNormalizado],[PasswordHash] FROM[cre-db-2].[dbo].[Usuarios]";
                SqlDataAdapter da = new SqlDataAdapter(consulta, con);

                DataTable dt = new DataTable();
                da.Fill(dt);



                #endregion

                TempData["MSG"] = "Estos son todos los Usuarios";
                return View("ListaUsuarios", dt);

            }
            catch (Exception)
            {
                TempData["ERROR"] = "";

                return View();
            }
        }


        /// </returns>

        public IActionResult Index()
        {


            var perfilUsuarioJson = HttpContext.Session.GetString("PerfilUsuario");
            var perfilUsuario = JsonConvert.DeserializeObject<PerfilUsuario>(perfilUsuarioJson);

            // Utilizar los datos del usuario para personalizar la vista
            ViewData["NombreUsuario"] = perfilUsuario.Nombre;
            ViewData["RolUsuario"] = perfilUsuario.Rol;


            return View();

        }



        #region API Indicadores Financieros

        public async Task<IActionResult> GetStockData(string symbol)
        {
            var alphaVantageApiKey = "WXDNQ18QMQFOQLIM";
            var queryUrl = $"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={symbol}&apikey={alphaVantageApiKey}";

            Uri queryUri = new Uri(queryUrl);

            using (WebClient client = new WebClient())
            {
                string response = await client.DownloadStringTaskAsync(queryUri);
                var data = JObject.Parse(response);

                // Comprobamos si "Global Quote" está vacío
                if (data["Global Quote"] == null || !data["Global Quote"].HasValues)
                {
                    return Json(new { error = "No quote data returned. Please check if the stock symbol is correct and try again later." });
                }

                // Devolvemos todos los datos de "Global Quote" en lugar de solo el precio
                return Json(data["Global Quote"]);
            }
        }


        #endregion




        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Map()
        {

            var Usuarios = repositorioProyectos.ObtenerU().Take(3).ToList();
            var modelin = new HomeIndex()
            {
                AccesosLocales = Usuarios,
            };

            return View(modelin);
        }
        public IActionResult Mapas_por_Mercado()
        {
            return View();
        }

        public IActionResult Hidrocarburos()
        {
            return View();
        }


        public IActionResult Mapa_SEM()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region Electricidad

        public IActionResult Electricidad()
        {
            return View();
        }



        #endregion


        #region Resultados
        public IActionResult MEP()
        {
            return View();
        }


        #endregion

        #region Votaciones
        public IActionResult Votaciones()
        {
            return View();
        }


        #endregion


        #region Contacto

        [HttpGet]
        public IActionResult Contacto()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Contacto(ContactoViewModel contactoViewModel)
        //        public async Task<IActionResult> Contacto(ContactoViewModel contactoViewModel)

        {
            // await servicioEmail.Enviar(contactoViewModel);
            return RedirectToAction("Gracias");
        }
        #endregion


        #region Gracias

        public IActionResult Gracias()
        {
            return View();
        }
        #endregion


        #region En construcción

        public IActionResult EnConstrucción()
        {
            return View();
        }

        #endregion








    }
}

