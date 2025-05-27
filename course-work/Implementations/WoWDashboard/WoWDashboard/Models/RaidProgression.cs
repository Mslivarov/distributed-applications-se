namespace WoWDashboard.Models
{
    public class RaidProgression
    {
        public int Id { get; set; } 
        public string RaidName { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public int CharacterId { get; set; }
        public Character Character { get; set; } = null!;
    }
}
    
