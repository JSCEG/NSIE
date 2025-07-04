using Microsoft.AspNetCore.Mvc;
using NSIE.Models;
using NSIE.Servicios;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Dapper;
using System.Data;

namespace NSIE.Controllers
{
    [ServiceFilter(typeof(ValidacionInputFiltro))]
    public class SNIERController : Controller
    {
        private readonly IRepositorioSNIER _repositorioSNIER;
        private readonly string _connectionString;
        private readonly ILogger<SNIERController> _logger;
        public SNIERController(IRepositorioSNIER repositorioSNIER, IConfiguration configuration, ILogger<SNIERController> logger)
        {
            _repositorioSNIER = repositorioSNIER;
            _connectionString = configuration.GetConnectionString("DefaultConnection");

            _logger = logger;
        }

        public IActionResult Enviar()
        {
            return View();
        }

        // 🧬 SNIEr - 🌱 El inventario y el potencial de los recursos renovables
        public IActionResult SNIEr_Inventario_AZEL()
        {
            return View();
        }

        // 🧬 SNIEr - 📚 Registros con Información Estadística sobre
        public IActionResult SNIEr_Registros_OrigenDestinoEnergia()
        {
            return View();
        }

        public IActionResult SNIEr_Registros_MercadoElectricoMayorista()
        {
            return View();
        }

        public IActionResult SNIEr_Registros_SeguimientoPlaneacion()
        {
            return View();
        }

        public IActionResult SNIEr_Registros_FOTEASE()
        {
            return View();
        }

        public IActionResult SNIEr_Registros_FSUE()
        {
            return View();
        }

        public IActionResult SNIEr_Registros_FondosID()
        {
            return View();
        }

        // 🧬 SNIEr - ♻️ Registro de Público de Certificados de Energías Limpias
        public IActionResult SNIEr_CEL_OtorgamientoCEL()
        {
            return View();
        }

        public IActionResult SNIEr_CEL_FactorEmisiones()
        {
            return View();
        }

        // 🧬 SNIEr - 📁 Catálogo de tecnologías
        public IActionResult SNIEr_Catalogo_TecnologiasGeneracion()
        {
            return View();
        }

        // 🧬 SNIEr - 🧭 Escenarios del sector energético
        public IActionResult SNIEr_Escenarios_ReferenciaYProyecciones()
        {
            return View();
        }

        // 🧬 SNIEr - 🧾 Información en Materia de Eficiencia Energética
        public IActionResult SNIEr_Eficiencia_ListaCombustibles()
        {
            return View();
        }

        public IActionResult SNIEr_Eficiencia_CatalogoEquipos()
        {
            return View();
        }

        public IActionResult SNIEr_Eficiencia_RegistroInstalaciones()
        {
            return View();
        }

        public IActionResult SNIEr_Eficiencia_AhorrosEnergeticos()
        {
            return View();
        }

        // 🏛️ Consejo - 🗓️ Convocatoria y Orden del Día
        public IActionResult Consejo_Convocatoria_ProgramacionSesiones()
        {
            return View();
        }

        // 🏛️ Consejo - 📑 Documentación soporte
        public IActionResult Consejo_Documentacion_MaterialesReferencia()
        {
            return View();
        }

        // 🏛️ Consejo - 🗣️ Sesión
        public IActionResult Consejo_Sesion_DesarrolloReuniones()
        {
            return View();
        }

        // 🏛️ Consejo - 📝 Registro de Acuerdos
        public IActionResult Consejo_Acuerdos_RegistroDecisiones()
        {
            return View();
        }

        // 🏛️ Consejo - 📌 Seguimiento de Acuerdos
        public IActionResult Consejo_Acuerdos_SeguimientoCumplimiento()
        {
            return View();
        }

        // 🏛️ Consejo - 📘 Informe Anual de Actividades
        public IActionResult Consejo_Reportes_InformeAnualActividades()
        {
            return View();
        }

        // 🏛️ Consejo - 📋 Programa Anual de Trabajo
        public IActionResult Consejo_Planeacion_ProgramaAnualTrabajo()
        {
            return View();
        }

        // 🧠 Modelos - 📝 Obtención y Registro de Información
        public IActionResult Modelos_Informacion_ObtencionYRegistro()
        {
            return View();
        }

        // 🧠 Modelos - ⚙️ Normalización de Información
        public IActionResult Modelos_Procesamiento_NormalizacionDatos()
        {
            return View();
        }

        // 🧠 Modelos - 🧠 Ejecución de Modelos
        public IActionResult Modelos_Ejecucion_ProcesamientoModelos()
        {
            return View();
        }

        // 🧠 Modelos - 🗂️ Cartera y Prospectiva de Proyectos
        public IActionResult Modelos_Proyectos_CarteraYProspectiva()
        {
            return View();
        }

        // 🗂️ SIE - 📦 Envío de Series / Información
        public IActionResult SIE_Series_EnvioInformacion()
        {
            return View();
        }

        // 🗂️ SIE - 🔍 Revisión de Series / Información
        public IActionResult SIE_Series_RevisionValidacion()
        {
            return View();
        }

        // 🗂️ SIE - 🤝 Validación de Información con Emisor
        public IActionResult SIE_Validacion_CoordinacionEmisores()
        {
            return View();
        }

        // 🗂️ SIE - 🔒 Carga de Información Definitiva
        public IActionResult SIE_Carga_InformacionDefinitiva()
        {
            return View();
        }

        // 🗂️ SIE - 📰 Publicación de Información
        public IActionResult SIE_Publicacion_VersionPublica()
        {
            return View();
        }

        // 🗂️ SIE - ☯ Balance Nacional de Energía
        public IActionResult SIE_Balance_BalanceNacionalEnergia()
        {
            return View();
        }

        // 🧬 SNIEr - 📊 Prospectivas Energéticas
        public IActionResult SNIEr_Prospectivas_SectorElectrico()
        {
            return View();
        }
    }
}
