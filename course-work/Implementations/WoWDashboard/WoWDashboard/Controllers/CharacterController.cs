using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;
using WoWDashboard.Data;
using WoWDashboard.Models;
using WoWDashboard.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WoWDashboard.Controllers
{
    public class CharacterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BlizzardService _blizzardService;
        private readonly RaiderIOService _raiderIOService;

        public CharacterController(ApplicationDbContext context, BlizzardService blizzardService, RaiderIOService raiderIOService)
        {
            _blizzardService = blizzardService;
            _raiderIOService = raiderIOService;
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> SavedCharacters(string searchTerm)
        {
            
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            int userId = int.Parse(userIdClaim);

            
            var userCharacters = _context.UserCharacters
            .Where(uc => uc.UserId == userId)
            .Include(uc => uc.Character)  
            .Select(uc => uc.Character);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var terms = searchTerm.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (var term in terms)
                {
                    userCharacters = userCharacters.Where(c =>
                        c.Name.ToLower().Contains(term) ||
                        c.Realm.ToLower().Contains(term) ||
                        c.CharacterClass.ToLower().Contains(term) ||
                        c.Level.ToString() == term
                    );
                }

                ViewData["SearchTerm"] = searchTerm;
            }

            return View(await userCharacters.ToListAsync());
        }

        [Authorize]
        public IActionResult GoToIndex()
        {
            return View("Index");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Details(string name, string realm, string region)
        {
            var character = await _blizzardService.GetCharacterInfoAsync(name, realm, region);

            if (character == null)
            {
                return View("Error");
            }

            var (score, progression) = await _raiderIOService.GetRaiderIoProfileAsync(name, realm, region);
            character.RaiderIoScore = score;
            character.RaidProgression = progression;

            var equipedItems = await _blizzardService.GetCharacterEquipmentAsync(name, realm, region);
            character.GearItems = equipedItems;

            var avatarUrl = await _blizzardService.GetCharacterAvatartAsync(name, realm, region);
            character.AvatarUrl = avatarUrl;

            return View(character);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> SavedCharacterDetails(int id)
        {
           
            var character = await _context.Characters
                .Include(c => c.GearItems) 
                .Include(c => c.RaidProgression)
                .FirstOrDefaultAsync(c => c.Id == id);  

            if (character == null)
            {
                return NotFound();
            }

            return View("Details", character);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SaveCharacter(string name, string realm, string region)
        {
            var character = await _blizzardService.GetCharacterInfoAsync(name, realm, region);

            int userIdClaim = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userIdClaim == 0)
            {
                return Unauthorized();
            }

            character.UserId = userIdClaim;
            var existingCharacter = await _context.Characters
                .FirstOrDefaultAsync(c => c.Name == character.Name && c.Realm == character.Realm && c.Region == character.Region);

            if (existingCharacter == null)
            {
                _context.Characters.Add(character);
                await _context.SaveChangesAsync();
                existingCharacter = character;
            }

            var alreadyLinked = await _context.UserCharacters
                .AnyAsync(uc => uc.UserId == userIdClaim && uc.CharacterId == existingCharacter.Id);

            if (!alreadyLinked)
            {
                var userCharacter = new UserCharacter
                {
                    UserId = userIdClaim,
                    CharacterId = existingCharacter.Id
                };
                _context.UserCharacters.Add(userCharacter);
                await _context.SaveChangesAsync();
            }

            var (score, progression) = await _raiderIOService.GetRaiderIoProfileAsync(name, realm, region);
            var equipedItems = await _blizzardService.GetCharacterEquipmentAsync(name, realm, region);
            var avatarUrl = await _blizzardService.GetCharacterAvatartAsync(name, realm, region);
            existingCharacter.OriginalName = character.Name;
            existingCharacter.OriginalRealm = character.Realm;
            existingCharacter.OriginalRegion = character.Region;
            existingCharacter.AvatarUrl = avatarUrl;
            existingCharacter.GearItems = equipedItems;
            existingCharacter.RaiderIoScore = score;
            existingCharacter.RaidProgression = progression;

            _context.Characters.Update(existingCharacter);
            await _context.SaveChangesAsync();

            return View("Index");
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var character = await _context.Characters.FindAsync(id);
            if (character == null)
            {
                return NotFound();
            }
            return View(character);
        }
     
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var character = await _context.Characters.FindAsync(id);
            if (character != null)
            {
                _context.Characters.Remove(character);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(SavedCharacters));
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var character = await _context.Characters.FindAsync(id);
            if (character == null)
            {
                return NotFound();
            }
            return View(character);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFinal(int id, string name, string realm, string characterClass, string race, int level, string region, string guild, int raiderIoScore, string avatarUrl)
        {
            var character = await _context.Characters.FindAsync(id);
            if (character == null)
            {
                return NotFound();
            }
            if (string.IsNullOrEmpty(character.OriginalName))
            {
                character.OriginalName = character.Name;
                character.OriginalRealm = character.Realm;
                character.OriginalRegion = character.Region;
            }
            character.Name = name;
            character.Realm = realm;
            character.CharacterClass = characterClass;
            character.Race = race;
            character.Level = level;
            character.Region = region;
            character.Guild = guild;
            character.RaiderIoScore = raiderIoScore;
            character.AvatarUrl = avatarUrl;

            _context.Update(character);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(SavedCharacters));
        }

        [Authorize]
        public async Task<IActionResult> UpdateCharacter(int id)
        {
            var character = await _context.Characters.FindAsync(id);
            if (character == null)
            {
                return NotFound();
            }

            var updatedCharacter = await _blizzardService.GetCharacterInfoAsync(character.OriginalName, character.OriginalRealm, character.OriginalRegion);
            var updatedAvatarUrl = await _blizzardService.GetCharacterAvatartAsync(character.OriginalName, character.OriginalRealm, character.OriginalRegion);
            var (score, progression) = await _raiderIOService.GetRaiderIoProfileAsync(character.OriginalName, character.OriginalRealm, character.OriginalRegion);

            if (updatedCharacter == null)
            {
                TempData["ErrorMessage"] = "Failed to fetch updated character data from Blizzard API.";
                return RedirectToAction("SavedCharacters");
            }

            character.Realm = updatedCharacter.Realm;
            character.Region = updatedCharacter.Region;
            character.RaiderIoScore = score;
            character.AvatarUrl = updatedAvatarUrl;
            character.Name = updatedCharacter.Name;
            character.Level = updatedCharacter.Level;
            character.Guild = updatedCharacter.Guild;
            character.Race = updatedCharacter.Race;
            character.CharacterClass = updatedCharacter.CharacterClass;

            _context.Update(character);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{character.Name}'s information was updated.";
            return RedirectToAction("SavedCharacters");
        }
    }
}
