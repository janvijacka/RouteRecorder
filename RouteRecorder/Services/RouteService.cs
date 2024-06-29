using GeoCoordinatePortable;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.IO;
using RouteRecorder.DTO;
using RouteRecorder.Models;
using RouteRecorder.ViewModels;
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

        public IEnumerable<RouteViewModel> GetRoutes()
        {
            var allRoutes = _context.Routes;
            var allRoutesViewModels = new List<RouteViewModel>();
            foreach (var route in allRoutes)
            {
                allRoutesViewModels.Add(RouteModelToViewModel(route));
            }
            return allRoutesViewModels;
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
            var route = _context.Routes
                .Include(r => r.Records)
                .FirstOrDefault(route => route.RouteId == id);
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

        private static RouteViewModel RouteModelToViewModel(Models.Route route)
        {
            return new RouteViewModel
            {
                RouteId = route.RouteId,
                Activity = route.Activity,
                Person = route.Person,
                Date = route.Date,
                Distance = route.Distance,
                Time = route.Time,
                AvgSpeed = route.AvgSpeed,
            };
        }

        private static RouteDTO RouteModelToDto(Models.Route route)
        {
            return new RouteDTO
            {
                RouteId = route.RouteId,
                Activity = route.Activity,
                Person = route.Person,
                Date = route.Date,
                Distance = route.Distance,
                Time = route.Time,
                AvgSpeed = route.AvgSpeed,
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

        private static Models.Route RouteDtoToModel(RouteDTO routeDTO)
        {
            return new Models.Route
            {
                RouteId = routeDTO.RouteId,
                Activity = routeDTO.Activity,
                Person = routeDTO.Person,
                Date = routeDTO.Date,
                Distance = routeDTO.Distance,
                Time = routeDTO.Time,
                AvgSpeed = routeDTO.AvgSpeed,
                Records = routeDTO.Records != null && routeDTO.Records.Count > 0 ? RecordDtoToModel(routeDTO.Records) : new List<Record>()
            };
        }

        private static List<Record> RecordDtoToModel(List<RecordDTO> recordsDtos)
        {
            var allRecords = new List<Record>();
            foreach (var record in recordsDtos)
            {
                allRecords.Add(new Record
                {
                    Latitude = record.Latitude,
                    Longitude = record.Longitude,
                    Elevation = record.Elevation,
                    Time = record.Time,
                });
            }
            return allRecords;
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

            var route = new RouteDTO
            {
                Activity = activity,
                Date = date,
                Person = "Default",
                Records = new List<RecordDTO>()
            };

            var trkseg = trk.Element(ns + "trkseg");
            var records = trkseg.Elements(ns + "trkpt");
            GeoCoordinate previousPoint = null;
            double totalDistance = 0;

            foreach ( var recordValue in records )
            {
                var latitude = double.Parse(recordValue.Attribute("lat").Value, CultureInfo.InvariantCulture);
                var longtitude = double.Parse(recordValue.Attribute("lon").Value, CultureInfo.InvariantCulture);
                var elevation = double.Parse(recordValue.Element(ns +"ele").Value, CultureInfo.InvariantCulture);
                var recordTime = DateTime.Parse((string)recordValue.Element(ns +"time"));

                var currentPoint = new GeoCoordinate(latitude, longtitude, elevation);
                if (previousPoint != null) 
                {
                    var distance = previousPoint.GetDistanceTo(currentPoint);
                    totalDistance += distance;
                }

                previousPoint = currentPoint;

                var record = new RecordDTO
                {
                    Latitude = latitude,
                    Longitude = longtitude,
                    Elevation = elevation,
                    Time = recordTime
                };

                route.Records.Add(record);
            }

            route.Distance = (int)Math.Round(totalDistance);
            route.Time = route.Records[route.Records.Count - 1].Time - route.Records[0].Time;
            route.AvgSpeed = Math.Round((totalDistance / 1000) / ((double)route.Time.TotalHours), 2);

            _context.Routes.Add(RouteDtoToModel(route));
            await _context.SaveChangesAsync();
        }

        public List<object> GetPoints(RouteDTO routeDto)
        {
            var points = new List<object>();
            foreach (var record in routeDto.Records) 
            {
                var point = new {latitude = record.Latitude, longitude = record.Longitude};
                points.Add(point);
            }
            return points;
        }
    }
}
