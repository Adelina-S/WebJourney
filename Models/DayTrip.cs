namespace WebJourney.Models
{
    public class DayTrip
    {
        public int Id { get; set; }
        //public Journey Journey { get; set; }
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Description { get; set; }
        public List<Expenses> Expenses { get; set; }
        public List<Location> Locations { get; set; }
        public bool IsStartTrip { get; set; }
        public List<Rating> Ratings { get; set; }
        public List<Photo> Photos { get; set; }
        public DayTrip()
        {
            Expenses= new List<Expenses>();
            Locations= new List<Location>();
            Ratings= new List<Rating>();
            Photos = new List<Photo>();
        }
    }
}
