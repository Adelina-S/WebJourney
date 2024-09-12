using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.JSInterop.Implementation;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using WebJourney.DAO;
using WebJourney.Models;

namespace WebJourney.Controllers
{
    public class HomeController : Controller
    {
        ApplicationContext Database;
        private User User { get; set; }
        public HomeController(ApplicationContext database)
        {
            Database = database;
        }
        private void CheckUser()
        {
            if (HttpContext == null) return;
            var login = HttpContext.User?.Identity?.Name;
            if (login != null)
            {
                User = Database.GetUser(login);
                if (User != null)
                {
                    ViewData["Name"] = User.Name;
                    ViewData["IsHaveUser"] = true;
                    //ViewData["Messages"] = Database.GetMessages(User).Count;
                }
            }
            if (login == null || User == null)
                ViewData["IsHaveUser"] = false;
        }
        public IActionResult Index()
        {
            CheckUser();
            var journeys = Database.GetJourneys();
            return View(journeys);
        }
        [HttpGet, Authorize]
        public IActionResult NewJourney()
        {
            CheckUser();
            return View("Views/Home/NewJourney.cshtml");
        }
        [HttpPost]
        public string GetCountriesList()
        {
            var countries = Database.GetCountries();
            List<string> result = new List<string>();
            foreach (var country in countries)
            {
                //result.Add($"<option data-countryId='{country.Id}'>{country.Name}</option>");
                result.Add($"<option data-countryName='{country.Name}' data-countryId='{country.Id}'>{country.Name}</option>");
            }
            return string.Join('\n', result);
        }
        [HttpPost]
        public string GetCitiesList(int countryId)
        {
            var cities = Database.GetCities(countryId);
            List<string> result = new List<string>();
            //result.Add($"<option data-cityId='0'>Не выбран</option>");
            foreach (var city in cities)
            {
                result.Add($"<option  data-cityName='{city.Name}' data-cityId='{city.Id}'>{city.Name}</option>");
            }
            return string.Join('\n', result);
        }
        [HttpPost, Authorize]
        public string AddJourney(string json)
        {
            CheckUser();
            try
            {
                var htmlJourney = System.Text.Json.JsonSerializer.Deserialize<HtmlJourney>(json);
                var Journey = new Journey();
                if (string.IsNullOrEmpty(htmlJourney.Title))
                    return "Нет названия";
                Journey.Title = htmlJourney.Title;
                Journey.User = User;
                var countries = Database.GetCountries(htmlJourney.Items.Select(t => t.CountryIdInt).Distinct().ToList());
                var cities = Database.GetCities(htmlJourney.Items.Where(t => t.CityIdInt != null).Select(t => t.CityIdInt.Value).Distinct().ToList());
                DayTrip starTrip = new DayTrip() { IsStartTrip = true, Title = Journey.Title };
                Journey.Trips.Add(starTrip);
                foreach (var item in htmlJourney.Items)
                {
                    var country = countries.First(t => t.Id == item.CountryIdInt);
                    City city = null;
                    if (item.CityIdInt != null)
                        city = cities.First(t => t.Id == item.CityIdInt.Value);
                    var location = new Location(country, city);
                    starTrip.Locations.Add(location);
                }
                foreach (var expenses in htmlJourney.Expenses)
                {
                    starTrip.Expenses.Add(new Expenses(expenses.Type, expenses.Value, expenses.Currency));
                }
                Database.AddJourney(Journey);
            }
            catch (Exception e)
            {
                return "Не удалось добавить путешествие";
            }
            return "";
        }
        [HttpGet, Authorize]
        public IActionResult MyJourneys()
        {
            CheckUser();
            var journeys = Database.GetJourneys(User);
            return View("Views/Home/MyJourneys.cshtml", journeys);
        }
        [HttpGet]
        public IActionResult ShowJourney(int journeyId)
        {
            CheckUser();
            var journey = Database.GetJourney(journeyId);
            var cities = journey.Trips.First(t => t.IsStartTrip).Locations.Where(t => t.City != null).ToList();
            ViewData["Cities"] = cities.Select(t => new string[] { t.City.Name, t.City.Latitude.ToString().Replace(',', '.'), t.City.Longitude.ToString().Replace(',', '.') });
            return View("Views/Home/JourneyInfo.cshtml", journey);
        }
        [HttpPost]
        public string GetMarks(int tripId)
        {
            DayTrip trip= Database.GetTrip(tripId);
            List<string[]> result = null;
            if (trip.IsStartTrip)
            {
                var cities = trip.Locations.Where(t => t.City != null).ToList();
                result = cities.Select(t => new string[] { t.City.Name, t.City.Latitude.ToString().Replace(',', '.'), t.City.Longitude.ToString().Replace(',', '.') }).ToList();
            }
            else
            {
                result = new List<string[]>();
                for (int i = 0; i < trip.Locations.Count; i++)
                    result.Add(new string[] { (i + 1).ToString(), trip.Locations[i].Latitude.ToString().Replace(',', '.'), trip.Locations[i].Longitude.ToString().Replace(',', '.') });
            }
            return System.Text.Json.JsonSerializer.Serialize(result);
        }
        [HttpGet, Authorize]
        public IActionResult AddTrip(int journeyId)
        {
            CheckUser();
            var journey = Database.GetJourney(User, journeyId);
            if (journey == null)
            {
                ViewData["ErrorText"] = "Путешествие не найдено";
                return Redirect("Views/Home/Error.cshtml");
            }
            else
            {
                Location firstLoc = journey.Trips.First(t => t.IsStartTrip).Locations.FirstOrDefault(t => t.City != null);
                ViewData["StartLat"] = (firstLoc?.City.Latitude.ToString() ?? "0").Replace(',','.');
                ViewData["StartLong"] = (firstLoc?.City.Longitude.ToString() ?? "0").Replace(',','.');
                ViewData["StartCity"] = firstLoc?.City.Id.ToString() ?? "0";
                ViewData["StartCountry"] = firstLoc?.Country.Id.ToString() ?? journey.Trips.First(t => t.IsStartTrip).Locations.First().Country.Id.ToString();
                return View("Views/Home/AddTrip.cshtml", journey);
            }
        }
        [HttpPost, Authorize]
        public string LoadPhotos()
        {
            CheckUser();
            var photos = HttpContext.Request.Form.Files;
            var values=new Dictionary<string, string>();
            foreach (var key in HttpContext.Request.Form.Keys)
                values.Add(key, HttpContext.Request.Form[key]);
            var journey = Database.GetJourney(User, Int32.Parse(values["journeyId"]));
            //if (journey == null) return "Путешествие не найдено";
            var trip = new DayTrip();
            trip.Title = values["where"];
            trip.Description = values["description"];
            DateOnly date = DateOnly.Parse(values["date"]);
            TimeOnly start = TimeOnly.Parse(values["start"]);
            TimeOnly end= TimeOnly.Parse(values["end"]);
            trip.StartTime = date.ToDateTime(start);
            trip.EndTime = date.ToDateTime(end);
            trip.Ratings.Add(new Rating() { Name = "Удобство проезда", Mark = Int32.Parse(values["starRoad"]) });
            trip.Ratings.Add(new Rating() { Name = "Растительность", Mark = Int32.Parse(values["starFlora"]) });
            trip.Ratings.Add(new Rating() { Name = "Безопасность", Mark = Int32.Parse(values["starSafe"]) });
            trip.Ratings.Add(new Rating() { Name = "Общие впечатления", Mark = Int32.Parse(values["starSumm"]) });
            for (int i=0; ;i++)
            {
                if (values.ContainsKey($"spentType{i}") == false) break;
                trip.Expenses.Add(new Expenses(values[$"spentType{i}"], Double.Parse(values[$"spentVal{i}"]), values[$"spentCurr{i}"]));
            }
            for (int i=0; ;i++)
            {
                if (values.ContainsKey($"markLat{i}") == false) break;
                var countryId = Int32.Parse(values[$"markCountry{i}"]);
                var cityId = Int32.Parse(values[$"markCity{i}"]);
                var lat = Double.Parse(values[$"markLat{i}"].Replace('.',','));
                var lng = Double.Parse(values[$"markLong{i}"].Replace('.', ','));
                var zoom = Int32.Parse(values[$"markZoom{i}"]);
                var country = Database.GetCountry(countryId);
                var city = Database.GetCity(cityId);
                trip.Locations.Add(new Location(country, city, lng, lat));
            }
            foreach (var photo in photos)
            {
                using (var memoryStream = new MemoryStream())
                {
                    photo.CopyTo(memoryStream);
                    byte[] fileBytes = memoryStream.ToArray();
                    trip.Photos.Add(new Photo() { FileName = photo.FileName, File = new PhotoFile() { Bytes = fileBytes } });
                    // Здесь вы можете сохранить файл, обработать его или что-то еще
                }
            }
            Database.AddTrip(journey, trip);
            return "";
        }
        [HttpGet]
        public IActionResult ShowPhoto(int id)
        {
                var photo = Database.GetPhoto(id);
                if (photo != null)
                {
                    ImageResult result = new ImageResult(photo.File.Bytes, "image/png", photo.FileName);
                    return result;
                }
                else return null;
        }
        [HttpGet]
        public IActionResult Login()
        {
            CheckUser();
            return View();
        }
        [HttpPost]
        public async Task<string> Login(string login, string password)
        {
            CheckUser();
            User result = Database.GetUser(login);
            if (result == null || result.Password != password)
                return "Неверный логин или пароль";
            else
            {
                List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, result.Login),
                };
                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(claimsPrincipal);
                return "";
            }
        }
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            CheckUser();
            if (User != null)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            return Redirect("/Home/Index");
        }
        [HttpGet]
        public IActionResult Register()
        {
            CheckUser();
            return View();
        }
        [HttpPost]
        public string Register(string login, string password, string name)
        {
            CheckUser();
            return Database.AddUser(name, password, login);
        }
    }
    class HtmlJourney()
    {
        public string Title { get; set; }
        public HtmlLocation[] Items { get; set; }
        public HtmlExpenses[] Expenses { get; set; }
    }
    /*class HtmlLocation
    {
        public string Country { get; set; }
        public string CountryId { get; set; }
        public string City { get; set; }
        public string CityId { get; set; }
    }*/
    class HtmlLocation
    {
        public int CountryIdInt { get; set; }
        [NotMapped]
        public string CountryId { get => CountryIdInt.ToString(); set { CountryIdInt = Int32.Parse(value); } }
        public int? CityIdInt { get; set; }
        [NotMapped]
        public string CityId { get => CityIdInt.ToString(); set { if (value!=null) CityIdInt = Int32.Parse(value); } }
    }
    class HtmlExpenses
    {
        public string Type { get; set; }
        public double Value { get; set; }
        //public string Value { get => ValueDouble.ToString(); set => ValueDouble = Double.Parse(value.Replace(',', '.')); }
        public string Currency { get; set; }
    }
}
