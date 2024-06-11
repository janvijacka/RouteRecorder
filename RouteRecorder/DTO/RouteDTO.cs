using RouteRecorder.Models;
using System.Diagnostics.CodeAnalysis;

namespace RouteRecorder.DTO
{
    public class RouteDTO
    {
        public int RouteId { get; set; }
        [NotNull]
        public string? Activity { get; set; }
        [NotNull]
        public string? Person { get; set; }
        public DateOnly Date { get; set; }
        [NotNull]
        public List<RecordDTO>? Records { get; set; }
    }
}
