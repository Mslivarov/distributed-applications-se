using WoWDashboard.Models;
using WoWDashboard.Dtos;

namespace WoWDashboard.Mappers
{
    public static class CharacterMapper
    {
        public static CharacterDto ToDto(Character character)
        {
            return new CharacterDto
            {
                Id = character.Id,
                Name = character.Name,
                Realm = character.Realm,
                Region = character.Region,
                Level = character.Level,
                Race = character.Race,
                Guild = character.Guild,
                CharacterClass = character.CharacterClass,
                RaiderIoScore = character.RaiderIoScore,
                GearItems = character.GearItems.Select(g => new GearItemDto
                {
                    Slot = g.Slot,
                    Name = g.Name,
                    Rarity = g.Rarity,
                    ItemLevel = g.ItemLevel,
                    ItemId = g.ItemId
                }).ToList(),
                RaidProgression = new RaidProgressionDto
                {
                    RaidName = character.RaidProgression.RaidName,
                    Summary = character.RaidProgression.Summary
                }
            };
        }

        public static Character ToModel(CharacterDto dto)
        {
            return new Character
            {
                Id = dto.Id,
                Name = dto.Name,
                Realm = dto.Realm,
                Region = dto.Region,
                Level = dto.Level,
                Race = dto.Race,
                Guild = dto.Guild,
                CharacterClass = dto.CharacterClass,
                RaiderIoScore = dto.RaiderIoScore,
                GearItems = dto.GearItems.Select(g => new GearItem
                {
                    Slot = g.Slot,
                    Name = g.Name,
                    Rarity = g.Rarity,
                    ItemLevel = g.ItemLevel,
                    ItemId = g.ItemId
                }).ToList(),
                RaidProgression = new RaidProgression
                {
                    RaidName = dto.RaidProgression.RaidName,
                    Summary = dto.RaidProgression.Summary
                }
            };
        }
    }
}
