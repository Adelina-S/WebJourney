using System.ComponentModel.DataAnnotations;

namespace WebJourney.Models
{
    public class Country
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string OtherInfo { get; set; }
    }

}
