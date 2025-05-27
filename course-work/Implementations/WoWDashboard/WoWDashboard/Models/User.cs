// Models/User.cs
using System.ComponentModel.DataAnnotations;

namespace WoWDashboard.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }

        public ICollection<UserCharacter> UserCharacters { get; set; }
    }
}
