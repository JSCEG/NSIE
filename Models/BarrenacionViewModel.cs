using System.ComponentModel.DataAnnotations;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Http;

namespace NSIE.Models
{
    public class BarrenacionViewModel
    {
        // =========================
        // CONTEXTO
        // =========================

        [Required(ErrorMessage = "Debe seleccionar la litología.")]
        public string LitologiaLocal { get; set; }

        public decimal? AnomaliaGravimetrica { get; set; }

        public string Anomalia1 { get; set; }
        public string Anomalia2 { get; set; }
        public string Anomalia3 { get; set; }

        [Required]
        public string Accesibilidad { get; set; }

        [Required]
        public string TipoTerreno { get; set; }


        // =========================
        // BARRENO
        // =========================

        [Required]
        public string BarrenoID { get; set; }

        public string Perforista { get; set; }

        public string Responsable { get; set; }


        // =========================
        // COORDENADAS
        // =========================

        [Range(14,33,ErrorMessage="Latitud fuera de rango")]
        public decimal Latitud { get; set; }

        [Range(-118.5,-86,ErrorMessage="Longitud fuera de rango")]
        public decimal Longitud { get; set; }

        [Range(0,10000)]
        public decimal? Altitud { get; set; }

        [Range(0,360)]
        public int? Azimut { get; set; }

        [Range(-90,0)]
        public int? Inclinacion { get; set; }


        // =========================
        // PROGRAMA
        // =========================

        public string TipoBarrenacion { get; set; }

        public DateTime? FechaInicio { get; set; }

        public DateTime? FechaFinalizacion { get; set; }

        [Range(0,10000)]
        public decimal? LongitudPerforada { get; set; }


        // =========================
        // NUCLEO
        // =========================

        public decimal? LongitudRecuperada { get; set; }

        public int? Diametro { get; set; }

        public int? NumeroCajas { get; set; }

        public string NombreCajas { get; set; }

        public string RQD { get; set; }

        [Range(0,1)]
        public decimal? TCR { get; set; }

        public int? Intervalos { get; set; }

        public string IntervalosInteres { get; set; }

        // =========================
        // ARCHIVOS
        // =========================

        public IFormFile ArchivoDescripcionNucleo { get; set; }

        public List<IFormFile> FotografiasNucleo { get; set; }

        // =========================
        // NOTAS
        // =========================
        public string Observaciones { get; set; }
    }

    public class Barrenacion
    {
        public int Id { get; set; }

        // CONTEXTO
        public string LitologiaLocal { get; set; }
        public decimal? AnomaliaGravimetrica { get; set; }

        public string Anomalia1 { get; set; }
        public string Anomalia2 { get; set; }
        public string Anomalia3 { get; set; }

        public string Accesibilidad { get; set; }
        public string TipoTerreno { get; set; }

        // BARRENO
        public string BarrenoID { get; set; }
        public string Perforista { get; set; }
        public string Responsable { get; set; }

        // COORDENADAS
        public decimal Latitud { get; set; }
        public decimal Longitud { get; set; }
        public decimal? Altitud { get; set; }

        public int? Azimut { get; set; }
        public int? Inclinacion { get; set; }

        // PROGRAMA
        public string TipoBarrenacion { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFinalizacion { get; set; }

        public decimal? LongitudPerforada { get; set; }

        // NUCLEO
        public decimal? LongitudRecuperada { get; set; }
        public int? Diametro { get; set; }
        public int? NumeroCajas { get; set; }
        public string NombreCajas { get; set; }

        public string RQD { get; set; }
        public decimal? TCR { get; set; }

        public int? Intervalos { get; set; }
        public string IntervalosInteres { get; set; }

        // ARCHIVOS
        public string ArchivoDescripcionRuta { get; set; }

        // NOTAS
        public string Observaciones { get; set; }

        // AUDITORIA
        public DateTime FechaCreacion { get; set; }
    }
}