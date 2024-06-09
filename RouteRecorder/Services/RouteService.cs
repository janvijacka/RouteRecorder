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

            var metadata = gpxDocument.Element("metadata");
            var activity = gpxDocument.Element("trk").Element("type")?.Value ?? "Unknown";
            var dateTimeString = metadata.Element("time").Value.Split("T")[0];
            var dateTime = DateTime.ParseExact(dateTimeString, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var route = new Models.Route
            {
                Activity = activity,
                Date = dateTime,
                Person = "Default",
                Records = new List<Record>()
            };

            var records = gpxDocument.Element("trk").Element("trkseg").Elements("trkpt");
            foreach ( var recordValue in records )
            {
                var latitude = double.Parse(recordValue.Attribute("lat").Value);
                var longtitude = double.Parse(recordValue.Attribute("lon").Value);
                var elevation = double.Parse(recordValue.Element("ele").Value);
                var recordTime = DateTime.Parse((string)recordValue.Element("time"));

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
