using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace WebJourney.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
        public User()
        {

        }
        public User(string login, string password, string name) : this()
        {
            Name = name;
            Login = login;
            Password = password;
        }
    }
}
