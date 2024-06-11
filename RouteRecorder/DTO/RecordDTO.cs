using System.Diagnostics.CodeAnalysis;

namespace RouteRecorder.DTO
{
    public class RecordDTO
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Elevation { get; set; }
        public DateTime Time { get; set; }
    }
}
