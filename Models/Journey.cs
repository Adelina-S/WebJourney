using System.ComponentModel.DataAnnotations.Schema;

namespace WebJourney.Models
{
    public class Journey
    { 
        public int Id { get; set; }
        public User User { get; set; }
        public string Title { get; set; }
        public List<DayTrip> Trips { get; set; }
        public Journey()
        {
            Trips=new List<DayTrip>();
        }
        public Journey(string title, User user):this()
        {
            Title = title;
            User = user;
        }
        [NotMapped]
        public int AddedTrips { get => Trips.Count - 1; }
        [NotMapped]
        public Dictionary<string, double> TotalSpents { get => Trips.SelectMany(t => t.Expenses).GroupBy(t => t.Currency).ToDictionary(k => k.Key, v => v.Sum(t => t.Value)); }
        [NotMapped]
        public int VisitedMarks { get => Trips.Where(t => t.IsStartTrip == false).SelectMany(t => t.Locations).Count(); }
        [NotMapped]
        public int TotalPhotos { get=>Trips.SelectMany(t=>t.Photos).Count(); }
        [NotMapped]
        public double AverageRating
        {
            get
            {
                var ratings = Trips.SelectMany(t => t.Ratings).ToList();
                return Math.Round(((double)ratings.Sum(t => t.Mark) / ratings.Count), 1);
            }
        }
    }
}
