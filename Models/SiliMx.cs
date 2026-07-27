// Models/SiliMx/SilimxRegistroUsuario.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NSIE.Models
{
    public class SilimxRegistroUsuario
    {
        public int IdRegistro { get; set; }
        public int IdUsuario { get; set; }

        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string Nombres { get; set; }
        public string TipoInstitucion { get; set; }
        public string Institucion { get; set; }
        public string Puesto { get; set; }
        public string CorreoInstitucional { get; set; }
        public string TelefonoContacto { get; set; }
        public string PerfilSolicitado { get; set; }
        public string UsuarioID { get; set; }

        public string Estatus { get; set; }
        public string PerfilAsignado { get; set; }
        public int? IdAprobador { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public string ComentarioAprobacion { get; set; }

        public int Vigente { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }
    public class SilimxRegistroViewModel
    {
        public int IdRegistro { get; set; }
        public int IdUsuario { get; set; }

        [Display(Name = "Apellido Paterno")]
        [Required(ErrorMessage = "El apellido paterno es obligatorio")]
        public string ApellidoPaterno { get; set; }

        [Display(Name = "Apellido Materno")]
        public string ApellidoMaterno { get; set; }

        [Display(Name = "Nombre(s)")]
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombres { get; set; }

        [Display(Name = "Tipo de Institución")]
        [Required(ErrorMessage = "Selecciona el tipo de institución")]
        public string TipoInstitucion { get; set; }

        [Display(Name = "Institución")]
        [Required(ErrorMessage = "La institución es obligatoria")]
        public string Institucion { get; set; }

        [Display(Name = "Puesto")]
        [Required(ErrorMessage = "El puesto es obligatorio")]
        public string Puesto { get; set; }

        [Display(Name = "Correo institucional")]
        [Required(ErrorMessage = "El correo institucional es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo no válido")]
        public string CorreoInstitucional { get; set; }

        [Display(Name = "Teléfono de contacto")]
        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone(ErrorMessage = "Formato de teléfono no válido")]
        public string TelefonoContacto { get; set; }

        [Display(Name = "Perfil solicitado")]
        [Required(ErrorMessage = "Selecciona el perfil solicitado")]
        public string PerfilSolicitado { get; set; }

        // Para llenar el <select> de Perfil solicitado
        public List<SelectListItem> PerfilesDisponibles { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Directivo", Text = "Directivo" },
            new SelectListItem { Value = "Operador capturista", Text = "Operador capturista" }
        };

        // Para llenar el <select> de Tipo de Institución
        public List<SelectListItem> TiposInstitucionDisponibles { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Pública / Federal", Text = "Pública / Federal" },
            new SelectListItem { Value = "Pública / Estatal", Text = "Pública / Estatal" },
            new SelectListItem { Value = "Pública / Municipal", Text = "Pública / Municipal" },
            new SelectListItem { Value = "Privada", Text = "Privada" },
            new SelectListItem { Value = "Consorcio (Pública-Privada)", Text = "Consorcio (Pública-Privada)" },
            new SelectListItem { Value = "Investigador independiente / sociedad civil", Text = "Investigador independiente / sociedad civil" }
        };
    }

    public class SilimxRegistroRequest
    {
        public int IdUsuario { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string Nombres { get; set; }
        public string TipoInstitucion { get; set; }
        public string Institucion { get; set; }
        public string Puesto { get; set; }
        public string CorreoInstitucional { get; set; }
        public string TelefonoContacto { get; set; }
        public string PerfilSolicitado { get; set; }
    }

    public class SilimxProyecto
    {
        public int IdProyecto { get; set; }
        public int IdUsuario { get; set; }
        public string TipoProyecto { get; set; }
        public string ProyectoID { get; set; }
        public string NombreProyecto { get; set; }
        public string DescripcionObjetivo { get; set; }
        public string InstitucionEmpresa { get; set; }
        public string TipoInstitucion { get; set; }
        public string Financiamiento { get; set; }
        public string Responsable { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string EstadoActual { get; set; }
        public int? Avance { get; set; }
        public string EntidadFederativa { get; set; }
        public string Municipio { get; set; }
        public string Localidad { get; set; }
        public string NotasFinales { get; set; }
        public string Evidencias { get; set; }
        public string Estatus { get; set; }
        public int Vigente { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public string IdVisible { get; set; }
        public string ResumenAvances { get; set; }
    }

    public class SilimxProyectoPermiso
    {
        public int IdPermiso { get; set; }
        public int IdProyecto { get; set; }
        public string Institucion { get; set; }
        public string PermisosSeleccionados { get; set; }
        public string OtroEspecifique { get; set; }
    }

    // DTO completo que llega del wizard por fetch
    public class SilimxProyectoRequest
    {
        public string TipoProyecto { get; set; }
        public string ProyectoID { get; set; }
        public string NombreProyecto { get; set; }
        public string DescripcionObjetivo { get; set; }
        public string InstitucionEmpresa { get; set; }
        public string TipoInstitucion { get; set; }
        public string Financiamiento { get; set; }
        public string Responsable { get; set; }
        public string FechaInicio { get; set; }
        public string FechaFin { get; set; }
        public string EstadoActual { get; set; }
        public int? Avance { get; set; }
        public string EntidadFederativa { get; set; }
        public string Municipio { get; set; }
        public string Localidad { get; set; }
        public string NotasFinales { get; set; }
        public string Evidencias { get; set; }
        public List<SilimxPermisoRequest> Permisos { get; set; } = new();
        public string IdVisible { get; set; }
        public string ResumenAvances { get; set; }
    }

    public class SilimxPermisoRequest
    {
        public string Institucion { get; set; }
        public List<string> PermisosSeleccionados { get; set; } = new();
        public string OtroEspecifique { get; set; }
    }

    public class SilimxProyectoCatalogo
    {
        public int IdProyecto { get; set; }
        public string TipoProyecto { get; set; }     // EE, BE, MA, EC
        public string ProyectoID { get; set; }        // nombre que el usuario registró
        public string UsuarioID { get; set; }         // el AAMMDDL asignado al usuario
        public string NombreProyecto { get; set; }
        public string EntidadFederativa { get; set; }
        public string Municipio { get; set; }
        public string Localidad { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string InstitucionEmpresa { get; set; }
        public string TipoInstitucion { get; set; }
        public string Financiamiento { get; set; }
        public string EstadoActual { get; set; }
        public int? Avance { get; set; }
        public string Estatus { get; set; }           // Pendiente / Completo / Borrador
        public string IdVisible { get; set; }         // TipoProyecto-ProyectoID-UsuarioID
    }

    // Modelo ligero para el selector del dropdown
    public class SilimxProyectoSelector
    {
        public int IdProyecto { get; set; }
        public string IdVisible { get; set; }   // ya viene de BD
        public string NombreProyecto { get; set; }
        public string Estatus { get; set; }
    }

    // Modelo completo con permisos precargados (para el wizard de actualización)
    public class SilimxProyectoCompleto
    {
        public SilimxProyecto Proyecto { get; set; }
        public IEnumerable<SilimxProyectoPermiso> Permisos { get; set; }
    }

    // DTO para la petición de actualización
    public class SilimxActualizacionRequest
    {
        public int IdProyecto { get; set; }           // ID del proyecto original
        public SilimxProyectoRequest DatosProyecto { get; set; }
    }

    public class SilimxBarreno
    {
        public int IdBarreno { get; set; }
        public int IdUsuario { get; set; }
        public int? IdProyecto { get; set; }
        public string ProyectoEEID { get; set; }
        public string BarrenoID { get; set; }
        public string BarrenoIDVisible { get; set; }
        public string Responsable { get; set; }
        public string EmpresaPerforista { get; set; }
        public string ResponsableDescNucleo { get; set; }
        public string Estado { get; set; }
        public string Municipio { get; set; }
        public string Localidad { get; set; }
        public decimal? LatitudN { get; set; }
        public decimal? LongitudO { get; set; }
        public decimal? Altitud { get; set; }
        public int? Azimut { get; set; }
        public string Inclinacion { get; set; }
        public string TipoBarrenacion { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public decimal? LongitudPerforada { get; set; }
        public decimal? LongitudRecuperada { get; set; }
        public int? Diametro { get; set; }
        public string RQD { get; set; }
        public int? NumeroCajas { get; set; }
        public string NombrePrimeraCaja { get; set; }
        public decimal? Gravimetria { get; set; }
        public string NotasFinales { get; set; }
        public string Evidencias { get; set; }
        public string Estatus { get; set; }
        public int Vigente { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }

    public class SilimxBarrenoIntervalo
    {
        public int IdIntervalo { get; set; }
        public int IdBarreno { get; set; }
        public string Nombre { get; set; }
        public decimal DesdeM { get; set; }
        public decimal HastaM { get; set; }
        public bool DeInteres { get; set; }
    }

    public class SilimxBarrenoRequest
    {
        // Paso 1
        public int? IdProyecto { get; set; }
        public string ProyectoEEID { get; set; }
        public string BarrenoID { get; set; }
        public string Responsable { get; set; }
        public string EmpresaPerforista { get; set; }
        public string ResponsableDescNucleo { get; set; }
        public string Estado { get; set; }
        public string Municipio { get; set; }
        public string Localidad { get; set; }
        // Paso 2
        public decimal? LatitudN { get; set; }
        public decimal? LongitudO { get; set; }
        public decimal? Altitud { get; set; }
        public int? Azimut { get; set; }
        public string Inclinacion { get; set; }
        public string TipoBarrenacion { get; set; }
        public string FechaInicio { get; set; }
        public string FechaFin { get; set; }
        public decimal? LongitudPerforada { get; set; }
        public decimal? LongitudRecuperada { get; set; }
        public int? Diametro { get; set; }
        public string RQD { get; set; }
        public int? NumeroCajas { get; set; }
        public string NombrePrimeraCaja { get; set; }
        public decimal? Gravimetria { get; set; }
        // Paso 3
        public List<SilimxBarrenoIntervaloRequest> Intervalos { get; set; } = new();
        // Paso 4
        public string NotasFinales { get; set; }
        public string Evidencias { get; set; }
    }

    public class SilimxBarrenoIntervaloRequest
    {
        public string Nombre { get; set; }
        public decimal DesdeM { get; set; }
        public decimal HastaM { get; set; }
        public bool DeInteres { get; set; }
    }

    // Para el catálogo
    public class SilimxBarrenoCatalogo
    {
        public int IdBarreno { get; set; }
        public string BarrenoIDVisible { get; set; }
        public string ProyectoEEID { get; set; }
        public string IdVisible { get; set; }   // del proyecto padre
        public decimal? LongitudRecuperada { get; set; }
        public string RQD { get; set; }
        public string Estado { get; set; }
        public string Municipio { get; set; }
        public string Localidad { get; set; }
        public DateTime? FechaFin { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string InstitucionEmpresa { get; set; }
        public string Estatus { get; set; }
    }

    public class SilimxMuestra
    {
        public int IdMuestra { get; set; }
        public int IdUsuario { get; set; }
        public int? IdProyecto { get; set; }
        public string ProyectoEEID { get; set; }
        public string MuestraID { get; set; }
        public string MuestraIDVisible { get; set; }
        public decimal? TamanoMuestra { get; set; }
        public string ResponsableMuestreo { get; set; }
        public DateTime? FechaMuestreo { get; set; }
        public string Estado { get; set; }
        public string Municipio { get; set; }
        public string Localidad { get; set; }
        public string Fuente { get; set; }      // Arcilla | Salmuera
        public string TipoCampo { get; set; }   // Petrolero | Geotermico | Superficial | Profunda
        // SP
        public string SP_Campo { get; set; }
        public string SP_Pozo { get; set; }
        public decimal? SP_LatitudN { get; set; }
        public decimal? SP_LongitudO { get; set; }
        public decimal? SP_Altitud { get; set; }
        public decimal? SP_Profundidad { get; set; }
        public decimal? SP_IntervaloInicio { get; set; }
        public decimal? SP_IntervaloFin { get; set; }
        public int? SP_CorteAgua { get; set; }
        public decimal? SP_Presion { get; set; }
        public int? SP_Temperatura { get; set; }
        public decimal? SP_pH { get; set; }
        public decimal? SP_OxigenoDisuelto { get; set; }
        // SG
        public string SG_Campo { get; set; }
        public string SG_PuntoMuestra { get; set; }
        public decimal? SG_LatitudN { get; set; }
        public decimal? SG_LongitudO { get; set; }
        public decimal? SG_Altitud { get; set; }
        public decimal? SG_Profundidad { get; set; }
        public int? SG_Temperatura { get; set; }
        public decimal? SG_pH { get; set; }
        public decimal? SG_OxigenoDisuelto { get; set; }
        // AS
        public decimal? AS_LatitudN { get; set; }
        public decimal? AS_LongitudO { get; set; }
        public decimal? AS_Altitud { get; set; }
        // AP
        public string AP_BarrenoID { get; set; }
        public string AP_IntervaloID { get; set; }
        public decimal? AP_DesdeM { get; set; }
        public decimal? AP_HastaM { get; set; }
        public string AP_Estructuras { get; set; }
        // Descripción técnica (Arcilla)
        public string AR_Litologia { get; set; }
        public string AR_Color { get; set; }
        public string AR_Textura { get; set; }
        public string AR_NotasDescripcion { get; set; }
        // Final
        public string NotasFinales { get; set; }
        public string Evidencias { get; set; }
        public string Estatus { get; set; }
        public int Vigente { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }

    public class SilimxMuestraRequest
    {
        // Paso 1
        public int? IdProyecto { get; set; }
        public string ProyectoEEID { get; set; }
        public string MuestraID { get; set; }
        public decimal? TamanoMuestra { get; set; }
        public string ResponsableMuestreo { get; set; }
        public string FechaMuestreo { get; set; }
        public string Estado { get; set; }
        public string Municipio { get; set; }
        public string Localidad { get; set; }
        // Paso 2
        public string Fuente { get; set; }
        // Paso 3
        public string TipoCampo { get; set; }
        // SP
        public string SP_Campo { get; set; }
        public string SP_Pozo { get; set; }
        public decimal? SP_LatitudN { get; set; }
        public decimal? SP_LongitudO { get; set; }
        public decimal? SP_Altitud { get; set; }
        public decimal? SP_Profundidad { get; set; }
        public decimal? SP_IntervaloInicio { get; set; }
        public decimal? SP_IntervaloFin { get; set; }
        public int? SP_CorteAgua { get; set; }
        public decimal? SP_Presion { get; set; }
        public int? SP_Temperatura { get; set; }
        public decimal? SP_pH { get; set; }
        public decimal? SP_OxigenoDisuelto { get; set; }
        // SG
        public string SG_Campo { get; set; }
        public string SG_PuntoMuestra { get; set; }
        public decimal? SG_LatitudN { get; set; }
        public decimal? SG_LongitudO { get; set; }
        public decimal? SG_Altitud { get; set; }
        public decimal? SG_Profundidad { get; set; }
        public int? SG_Temperatura { get; set; }
        public decimal? SG_pH { get; set; }
        public decimal? SG_OxigenoDisuelto { get; set; }
        // AS
        public decimal? AS_LatitudN { get; set; }
        public decimal? AS_LongitudO { get; set; }
        public decimal? AS_Altitud { get; set; }
        // AP
        public string AP_BarrenoID { get; set; }
        public string AP_IntervaloID { get; set; }
        public decimal? AP_DesdeM { get; set; }
        public decimal? AP_HastaM { get; set; }
        public string AP_Estructuras { get; set; }
        // Descripción técnica
        public string AR_Litologia { get; set; }
        public string AR_Color { get; set; }
        public string AR_Textura { get; set; }
        public string AR_NotasDescripcion { get; set; }
        // Final
        public string NotasFinales { get; set; }
        public string Evidencias { get; set; }
    }

    public class SilimxMuestraCatalogo
    {
        public int IdMuestra { get; set; }
        public string MuestraIDVisible { get; set; }
        public string ProyectoEEID { get; set; }
        public string IdVisible { get; set; }
        public string Estado { get; set; }
        public string Municipio { get; set; }
        public string Localidad { get; set; }
        public DateTime? FechaMuestreo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string InstitucionEmpresa { get; set; }
        public string Estatus { get; set; }
    }

    public class SilimxBarrenoSelector
    {
        public int IdBarreno { get; set; }
        public string BarrenoIDVisible { get; set; }
        // Intervalos de este barreno
        public string IntervalosJson { get; set; }
    }
}