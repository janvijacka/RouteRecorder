using RouteRecorder.DTO;
using RouteRecorder.Models;
using System.Globalization;
using System.Xml.Linq;

namespace RouteRecorder.Services
{
    public class RouteService
    {
        private ApplicationDbContext _context;

        public RouteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<RouteDTO> GetRoutes()
        {
            var allRoutes = _context.Routes;
            var allRoutesDtos = new List<RouteDTO>();
            foreach (var route in allRoutes)
            {
                allRoutesDtos.Add(RouteModelToDto(route));
            }
            return allRoutesDtos;
        }

        internal async Task<RouteDTO> GetByIdAsync(int id)
        {
            var route = await VerifyExistenceAsync(id);
            if (route == null)
            {
                return null;
            }
            return RouteModelToDto(route);
        }

        private async Task<Models.Route> VerifyExistenceAsync(int id)
        {
            var route = _context.Routes.FirstOrDefault(route => route.RouteId == id);
            if (route == null)
            {
                return null;
            }
            return route;
        }

        internal async Task DeleteAsync(int id)
        {
            var routeToDelete = _context.Routes.FirstOrDefault(route => route.RouteId == id);
            List<Record> recordsToDelete = _context.Records.Where(record => record.RouteId == id).ToList();
            foreach (var record in recordsToDelete)
            {
                _context.Records.Remove(record);
            }
            _context.Routes.Remove(routeToDelete);
            await _context.SaveChangesAsync();
        }

        private static RouteDTO RouteModelToDto(Models.Route route)
        {
            return new RouteDTO
            {
                RouteId = route.RouteId,
                Activity = route.Activity,
                Person = route.Person,
                Date = route.Date,
                Records = route.Records != null && route.Records.Count > 0 ? RecordModelToDto(route.Records) : new List<RecordDTO>()
            };
        }

        private static List<RecordDTO> RecordModelToDto(List<Record> records)
        {
            var allRecordsDtos = new List<RecordDTO>();
            foreach (var record in records)
            {
                allRecordsDtos.Add(new RecordDTO
                {
                    Latitude = record.Latitude,
                    Longitude = record.Longitude,
                    Elevation = record.Elevation,
                    Time = record.Time,
                }); 
            }
            return allRecordsDtos;
        }

        public async Task SaveRouteFromGpx(Stream gpxFileStream)
        {
            XDocument gpxDocument = XDocument.Load(gpxFileStream);
            XNamespace ns = "http://www.topografix.com/GPX/1/1";
            var metadata = gpxDocument.Root.Element(ns + "metadata");
            var trk = gpxDocument.Root.Element(ns + "trk");
            var activity = trk.Element(ns + "type")?.Value ?? "Unknown";
            var dateTimeString = metadata.Element(ns + "time").Value.Split("T")[0];
            var date = DateOnly.ParseExact(dateTimeString, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var route = new Models.Route
            {
                Activity = activity,
                Date = date,
                Person = "Default",
                Records = new List<Record>()
            };

            var trkseg = trk.Element(ns + "trkseg");
            var records = trkseg.Elements(ns + "trkpt");
            foreach ( var recordValue in records )
            {
                var latitude = double.Parse(recordValue.Attribute("lat").Value, CultureInfo.InvariantCulture);
                var longtitude = double.Parse(recordValue.Attribute("lon").Value, CultureInfo.InvariantCulture);
                var elevation = double.Parse(recordValue.Element(ns +"ele").Value, CultureInfo.InvariantCulture);
                var recordTime = DateTime.Parse((string)recordValue.Element(ns +"time"));

                var record = new Record
                {
                    Latitude = latitude,
                    Longitude = longtitude,
                    Elevation = elevation,
                    Time = recordTime
                };

                route.Records.Add(record);
            }
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();
        }
    }
}
