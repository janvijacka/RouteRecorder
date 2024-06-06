using System.Diagnostics.CodeAnalysis;

namespace RouteRecorder.Models
{
    public class Route
    {
        public int RouteId { get; set; }
        [NotNull]
        public string? Activity { get; set; }
        [NotNull]
        public string? Person { get; set; }
        public DateTime Date { get; set; }
        [NotNull]
        public List<Record>? Records { get; set; }
    }
}
