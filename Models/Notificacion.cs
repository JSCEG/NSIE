namespace NSIE.Models
{
    public class Notificacion
    {
        public int ID { get; set; }
        public Guid ID_Notificacion { get; set; }
        public string Titulo_Notificacion { get; set; }
        public string Mensaje { get; set; }
        public DateTime Fecha_Notificacion { get; set; }
        public string Link { get; set; }
        public int ID_Usuario { get; set; }
        public bool Visto { get; set; }
        public DateTime? Fecha_Visto { get; set; }
        public string Imagen { get; set; } // Nueva propiedad para imagenes

        // Propiedad calculada para el tiempo transcurrido
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.Now - Fecha_Notificacion;
                if (timeSpan.TotalDays > 1)
                    return $"{(int)timeSpan.TotalDays} días ago";
                else if (timeSpan.TotalHours > 1)
                    return $"{(int)timeSpan.TotalHours} hrs ago";
                else if (timeSpan.TotalMinutes > 1)
                    return $"{(int)timeSpan.TotalMinutes} mins ago";
                else
                    return "Just now";
            }
        }
    }
}