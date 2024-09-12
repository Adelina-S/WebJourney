namespace WebJourney.Models
{
    public class Photo
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public PhotoFile File { get; set; }
    }
}
