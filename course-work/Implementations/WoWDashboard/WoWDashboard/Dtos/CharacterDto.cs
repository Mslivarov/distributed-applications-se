namespace WoWDashboard.Dtos
{
    public class CharacterDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Realm { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public int Level { get; set; }
        public string Race { get; set; } = string.Empty;
        public string Guild { get; set; } = string.Empty;
        public string CharacterClass { get; set; } = string.Empty;
        public double RaiderIoScore { get; set; }

        public List<GearItemDto> GearItems { get; set; } = new();
        public RaidProgressionDto RaidProgression { get; set; } = new();
    }
}
