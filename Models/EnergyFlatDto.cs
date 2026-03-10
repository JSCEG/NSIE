namespace NSIE.Models
{
    public class EnergyFlatDto
    {
        public int ParentId { get; set; }
        public string ParentName { get; set; }
        public string ParentDescription { get; set; }
        public string ParentColor { get; set; }

        public int ChildId { get; set; }
        public string ChildName { get; set; }
        public string Tipo { get; set; }
        public string ChildDescription { get; set; }
        public string ChildColor { get; set; }

        public int Year { get; set; }
        public double Value { get; set; }
    }

}