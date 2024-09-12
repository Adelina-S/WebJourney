using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebJourney.Models
{
    public class City
    {
        public int Id { get; set; } 
        public Country Country { get; set; }
        [ForeignKey("Country")]
        public int CountryId { get; set; }
        [Required]
        public string Name { get; set;}
        public double Longitude { get; set;}
        public double Latitude { get; set;}
        public string OtherInfo { get; set; }
    }
}
