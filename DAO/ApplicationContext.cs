using Microsoft.EntityFrameworkCore;
using System.Text;
using WebJourney.Models;

namespace WebJourney.DAO
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Journey> Journeys { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Expenses> Expenses { get; set; }
        public DbSet<DayTrip> DayTrips { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<PhotoFile> PhotoFiles { get; set; }
        public ApplicationContext()
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=base.db");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            User adelina = new User("Adelina", "Synergy", "Аделина") { Id = 1};
            User jack = new User("Jack", "123", "Евгений") { Id = 2 };
            User vika = new User("Vika", "123", "Виктория") { Id = 3 };
            modelBuilder.Entity<User>().HasData(new User[] { adelina, jack, vika });
            var countries = LoadCountries();
            modelBuilder.Entity<Country>().HasData(countries);
            var cities = LoadCities(3159);
             modelBuilder.Entity<City>().HasData(cities);

            modelBuilder.Entity<City>().Navigation(t => t.Country).AutoInclude();
            modelBuilder.Entity<Location>().Navigation(t=>t.Country).AutoInclude();
            modelBuilder.Entity<Location>().Navigation(t => t.City).AutoInclude();
            modelBuilder.Entity<Journey>().Navigation(t=>t.User).AutoInclude();    
            modelBuilder.Entity<Journey>().Navigation(t=>t.Trips).AutoInclude();
            modelBuilder.Entity<DayTrip>().Navigation(t=>t.Locations).AutoInclude();
            modelBuilder.Entity<DayTrip>().Navigation(t=>t.Expenses).AutoInclude();
            modelBuilder.Entity<DayTrip>().Navigation(t=>t.Ratings).AutoInclude();
            //modelBuilder.Entity<DayTrip>().Navigation(t=>t.Journey).AutoInclude();
            modelBuilder.Entity<DayTrip>().Navigation(t=>t.Photos).AutoInclude();
        }
        Country[] LoadCountries()
        {
            List<Country> result = new List<Country>();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var encoding = Encoding.GetEncoding(1251);
            var data = File.ReadAllLines("C://123//country.txt", encoding);
            foreach (var line in data)
            {
                var parts = line.Split(';');
                parts = parts.Select(t => t.Trim('\\').Trim('"')).ToArray();
                int id = 0;
                if (Int32.TryParse(parts[0], out id) == false)
                    continue;
                Country country = new Country() { Id = id, Name = parts[2] };
                result.Add(country);
            }
            return result.ToArray();
        }
        City[] LoadCities(int countryId)
        {
            List<City> result = new List<City>();
            var data = File.ReadAllLines("C://123//cities.csv");
            int id = 0;
            foreach (var line in data)
            {
                var parts = line.Split(',');
                int index = 0;
                if (Int32.TryParse(parts[0], out index) == false)
                    continue;
                id++;
                double longitude = Double.Parse(parts[18].Replace('.',','));
                double latitude = Double.Parse(parts[17].Replace('.', ','));
                City city = new City() { Id = id, Name = parts[6], Longitude=longitude, Latitude=latitude, CountryId=countryId };
                result.Add(city);
            }
            result.Add(new City() { Id = 2000, CountryId = 3159, Latitude = 55.753995, Longitude = 37.614069, Name = "Москва" });
            result.Add(new City() { Id = 2001, CountryId = 3159, Latitude = 59.93428, Longitude = 30.3351, Name = "Санкт-Петербург" });
            return result.ToArray();
        }
        public User GetUser(string login) => Users.FirstOrDefault(t => t.Login == login);
        public List<Country> GetCountries() => Countries.ToList();
        public List<Country> GetCountries(List<int> list)=>Countries.Where(t=>list.Contains(t.Id)).ToList();
        public Country GetCountry(int id)=>Countries.FirstOrDefault(t=>t.Id == id);
        public List<City> GetCities(int CountryId)=>Cities.Where(t=>t.Country.Id == CountryId).ToList();
        public List<City> GetCities(List<int> list) => Cities.Where(t => list.Contains(t.Id)).ToList();
        public City GetCity(int id) => Cities.FirstOrDefault(t => t.Id == id);
        public List<Journey> GetJourneys(User user)=>Journeys.Where(t=>t.User == user).ToList();
        public List<Journey> GetJourneys()=>Journeys.ToList();
        public Journey GetJourney(User user, int journeyId) => Journeys.FirstOrDefault(t => t.User == user && t.Id == journeyId);
        public Journey GetJourney(int journeyId) => Journeys.FirstOrDefault(t => t.Id == journeyId);
        public DayTrip GetTrip(int tripId)=>DayTrips.FirstOrDefault(t=>t.Id==tripId);
        public Photo GetPhoto(int id) => Photos.Include(t => t.File).FirstOrDefault(t => t.Id == id);
        public void AddJourney(Journey journey)
        {
            Journeys.Add(journey);
            SaveChanges();
        }
        public void AddTrip(Journey journey, DayTrip trip)
        {
            journey.Trips.Add(trip);
            SaveChanges();
        }
        public string AddUser(string name, string password, string login)
        {
            if (GetUser(login) != null) return "Пользователь с такой учётной записью уже зарегистрирован";
            User user = new User(login, password, name);
            Users.Add(user);
            SaveChanges();
            return "";
        }
    }
}
