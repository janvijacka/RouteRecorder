using System.Diagnostics.CodeAnalysis;

namespace RouteRecorder.ViewModels
{
    public class RouteViewModel
    {
        public int RouteId { get; set; }
        [NotNull]
        public string? Activity { get; set; }
        [NotNull]
        public string? Person { get; set; }
        public DateOnly Date { get; set; }
        public int Distance { get; set; }
        public TimeSpan Time { get; set; }
        public double AvgSpeed { get; set; }
    }
}
