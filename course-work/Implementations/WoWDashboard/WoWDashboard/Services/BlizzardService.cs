using System.Net.Http.Headers;
using System.Text.Json;
using WoWDashboard.Data;
using WoWDashboard.Models;

namespace WoWDashboard.Services
{
    public class BlizzardService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly ApplicationDbContext _context;
        private DateTime _tokenExpiration;

        public BlizzardService(ApplicationDbContext context, IConfiguration configuration)
        {
            _clientId = configuration["Blizzard:ClientId"] ?? throw new ArgumentNullException("Blizzard ClientId is not set");
            _clientSecret = configuration["Blizzard:ClientSecret"] ?? throw new ArgumentNullException("Blizzard ClientSecret is not set");
            _context = context;
        }

        private async Task<string> AuthenticateAsync()
        {

            var authRequest = new HttpRequestMessage(HttpMethod.Post, "https://eu.oauth.battle.net/token");
            authRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" }
            });

            var byteArray = System.Text.Encoding.ASCII.GetBytes($"{_clientId}:{_clientSecret}");
            authRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var response = await _httpClient.SendAsync(authRequest);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            var tokenData = JsonSerializer.Deserialize<JsonElement>(content);
            var _accessToken = tokenData.GetProperty("access_token").GetString();
            _tokenExpiration = DateTime.UtcNow.AddSeconds(tokenData.GetProperty("expires_in").GetInt32() - 60);

            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiration)
                return _accessToken;

            else return "";
        }


          public async Task<Character?> GetCharacterInfoAsync(string name, string realm, string region)
          {
              var _accessToken = await AuthenticateAsync();

              if (string.IsNullOrEmpty(_accessToken))
              {
                  throw new Exception("Access token is missing! Authentication failed.");
              }

              var realmSlug = realm.ToLower().Replace(" ", "-");
              var characterName = name.ToLower();
              var characterRegion = region.ToLower();

              var requestUrl = $"https://{characterRegion}.api.blizzard.com/profile/wow/character/{realmSlug}/{characterName}?namespace=profile-{characterRegion}&locale=en_US&access_token={_accessToken}";
              _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

              HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);

              if (!response.IsSuccessStatusCode)
                  return null;

              var content = await response.Content.ReadAsStringAsync();
              var data = JsonSerializer.Deserialize<JsonElement>(content);
              var extractedRegion = region.ToLower();
              var extractedId = data.GetProperty("id").GetInt32();
              var extractedName = data.GetProperty("name").GetString();
              var extractedRealm = data.GetProperty("realm").GetProperty("name").GetString();
              var ectractedLevel = data.GetProperty("level").GetInt32();
              string guild = data.TryGetProperty("guild", out var guildProp) && guildProp.TryGetProperty("name", out var guildNameProp) ? guildNameProp.GetString() : "NONE";
              var race = data.GetProperty("race").GetProperty("name").GetString();
              var characterClass = data.GetProperty("character_class").GetProperty("name").GetString();

              if (string.IsNullOrEmpty(extractedName) || string.IsNullOrEmpty(extractedRealm) ||
                  string.IsNullOrEmpty(race) || string.IsNullOrEmpty(characterClass))
              {
                  throw new Exception("Character data is incomplete from Blizzard API.");
              }

            var character = new Character
            {
                Id = extractedId,
                Name = extractedName,
                Realm = extractedRealm,
                Region = extractedRegion,
                Level = ectractedLevel,
                Race = race,
                CharacterClass = characterClass,
                Guild = guild

              };
              return character;
          }
        public async Task<List<GearItem>> GetCharacterEquipmentAsync(string name, string realm, string region)
        {
            var _accessToken = await AuthenticateAsync();

            if (string.IsNullOrEmpty(_accessToken))
            {
                throw new Exception("Access token is missing! Authentication failed.");
            }

            var realmSlug = realm.ToLower().Replace(" ", "-");
            var characterName = name.ToLower();
            var characterRegion = region.ToLower();

            var requestUrl = $"https://{characterRegion}.api.blizzard.com/profile/wow/character/{realmSlug}/{characterName}/equipment?namespace=profile-{characterRegion}&locale=en_US&access_token={_accessToken}";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);

            var equipmentItems = new List<GearItem>();
            int totalCharacterIlvl = 0;

            foreach (var item in document.RootElement.GetProperty("equipped_items").EnumerateArray())
            {
                var equipmentItem = new GearItem
                {
                    Slot = item.GetProperty("slot").GetProperty("type").GetString(),
                    Name = item.GetProperty("name").GetString(),
                    Rarity = item.GetProperty("quality").GetProperty("type").GetString(),
                    ItemLevel = item.GetProperty("level").GetProperty("value").GetInt32(),
                    ItemId = item.GetProperty("item").GetProperty("id").GetInt32(),
                };
                equipmentItems.Add(equipmentItem);
            }
            
            return equipmentItems;
        }
        public async Task<String> GetCharacterAvatartAsync(string name, string realm, string region)
        {
            var _accessToken = await AuthenticateAsync();

              if (string.IsNullOrEmpty(_accessToken))
              {
                  throw new Exception("Access token is missing! Authentication failed.");
              }

              var realmSlug = realm.ToLower().Replace(" ", "-");
              var characterName = name.ToLower();
              var characterRegion = region.ToLower();

            var requestUrl = $"https://{characterRegion}.api.blizzard.com/profile/wow/character/{realmSlug}/{characterName}/character-media?namespace=profile-{characterRegion}&locale=en_US&access_token={_accessToken}";
              _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

              HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);

              if (!response.IsSuccessStatusCode)
                  return null;

              string jsonString = await response.Content.ReadAsStringAsync();
            JsonDocument doc = JsonDocument.Parse(jsonString);

            var assets = doc.RootElement.GetProperty("assets");

            string avatarUrl = null;

            foreach (var asset in assets.EnumerateArray())
            {
                if (asset.GetProperty("key").GetString() == "avatar")
                {
                    avatarUrl = asset.GetProperty("value").GetString();
                    break;
                }
            }
            return avatarUrl;
        }

    }
}
    

