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

        public async Task SaveRouteFromGpx(Stream gpxFileStream)
        {
            XDocument gpxDocument = XDocument.Load(gpxFileStream);
            XNamespace ns = "http://www.topografix.com/GPX/1/1";
            var metadata = gpxDocument.Root.Element(ns + "metadata");
            var trk = gpxDocument.Root.Element(ns + "trk");
            var activity = trk.Element(ns + "type")?.Value ?? "Unknown";
            var dateTimeString = metadata.Element(ns + "time").Value.Split("T")[0];
            var dateTime = DateTime.ParseExact(dateTimeString, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var route = new Models.Route
            {
                Activity = activity,
                Date = dateTime,
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
