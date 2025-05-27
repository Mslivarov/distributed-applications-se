namespace WoWDashboard.Models
{
    public class Character
    {
        public string OriginalName { get; set; } = string.Empty;
        public string OriginalRealm { get; set; } = string.Empty;
        public string OriginalRegion { get; set; } = string.Empty;
        public int Id { get; set; } 
        public string Name { get; set; } = string.Empty;
        public string Realm { get; set; } = string.Empty;
        public string Region {  get; set; } = string.Empty;
        public int Level { get; set; }
        public string Race { get; set; } = string.Empty;
        public string Guild { get; set; } = string.Empty;
        public string CharacterClass { get; set; } = string.Empty;
        public List<GearItem> GearItems { get; set; } = new List<GearItem>();
        public double RaiderIoScore { get; set; }
        public RaidProgression RaidProgression { get; set; } = new RaidProgression();
        public string AvatarUrl { get; set; } = string.Empty;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public ICollection<UserCharacter> UserCharacters { get; set; }
    }
}
