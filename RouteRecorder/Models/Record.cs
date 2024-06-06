using System.Diagnostics.CodeAnalysis;

namespace RouteRecorder.Models
{
    public class Record
    {
        public int RecordId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Elevation { get; set; }
        public DateTime Time { get; set; }
        public int RouteId { get; set; }
        [NotNull]
        public Route? Route { get; set; }
    }
}
