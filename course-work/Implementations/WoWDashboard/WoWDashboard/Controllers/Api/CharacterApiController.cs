using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WoWDashboard.Data;
using WoWDashboard.Dtos;
using WoWDashboard.Mappers;
using WoWDashboard.Models;

namespace WoWDashboard.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CharacterApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CharacterApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CharacterDto>>> GetCharacters(
        int page = 1,
        int pageSize = 10,
        string? name = null,
        string? characterClass = null,
        string? sortBy = null,
        bool ascending = true)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var query = _context.Characters
                .Where(c => c.UserId == userId)
                .Include(c => c.GearItems)
                .Include(c => c.RaidProgression)
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(c => c.Name.Contains(name));
            }

            if (!string.IsNullOrEmpty(characterClass))
            {
                query = query.Where(c => c.CharacterClass.Contains(characterClass));
            }

            query = (sortBy?.ToLower()) switch
            {
                "name" => ascending ? query.OrderBy(c => c.Name) : query.OrderByDescending(c => c.Name),
                "level" => ascending ? query.OrderBy(c => c.Level) : query.OrderByDescending(c => c.Level),
                "raiderioscore" => ascending ? query.OrderBy(c => c.RaiderIoScore) : query.OrderByDescending(c => c.RaiderIoScore),
                "race" => ascending ? query.OrderBy(c => c.Race) : query.OrderByDescending(c => c.Race),
                "realm" => ascending ? query.OrderBy(c => c.Realm) : query.OrderByDescending(c => c.Realm),
                _ => query.OrderBy(c => c.Id) // Default sorting
            };

            var characters = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return characters.Select(CharacterMapper.ToDto).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CharacterDto>> GetCharacter(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var character = await _context.Characters
                .Include(c => c.GearItems)
                .Include(c => c.RaidProgression)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (character == null)
                return NotFound();

            return CharacterMapper.ToDto(character);
        }

        [HttpPost]
        public async Task<ActionResult<CharacterDto>> PostCharacter(CharacterDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var character = CharacterMapper.ToModel(dto);
            character.UserId = userId;

            _context.Characters.Add(character);
            await _context.SaveChangesAsync();

            var resultDto = CharacterMapper.ToDto(character);
            return CreatedAtAction(nameof(GetCharacter), new { id = character.Id }, resultDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCharacter(int id, CharacterDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var existingCharacter = await _context.Characters
                .Include(c => c.GearItems)
                .Include(c => c.RaidProgression)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (existingCharacter == null)
                return NotFound();

            existingCharacter.Name = dto.Name;
            existingCharacter.Realm = dto.Realm;
            existingCharacter.Region = dto.Region;
            existingCharacter.Level = dto.Level;
            existingCharacter.Race = dto.Race;
            existingCharacter.Guild = dto.Guild;
            existingCharacter.CharacterClass = dto.CharacterClass;
            existingCharacter.RaiderIoScore = dto.RaiderIoScore;

            existingCharacter.GearItems = dto.GearItems.Select(g => new GearItem
            {
                Slot = g.Slot,
                Name = g.Name,
                Rarity = g.Rarity,
                ItemLevel = g.ItemLevel,
                ItemId = g.ItemId
            }).ToList();

            existingCharacter.RaidProgression = new RaidProgression
            {
                RaidName = dto.RaidProgression.RaidName,
                Summary = dto.RaidProgression.Summary
            };

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCharacter(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var character = await _context.Characters
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (character == null)
                return NotFound();

            _context.Characters.Remove(character);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
