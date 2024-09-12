using WebJourney.Common;

namespace WebJourney.Models
{
    public class Location
    {
        public int Id { get; set; }
        public Country Country { get; set; }
        public City City { get; set; }
        public LocationType LocationType { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public Location()
        {

        }
        public Location(Country country)
        {
            Country = country;
            LocationType = LocationType.Country;
        }
        public Location(Country country, City city)
        {
            Country = country;
            City= city;
            LocationType = LocationType.City;
        }
        public Location(Country country, City city, double lon, double lat)
        {
            Country = country;
            City = city;
            Longitude = lon;
            Latitude = lat;
            LocationType = LocationType.Point;
        }
    }
}
