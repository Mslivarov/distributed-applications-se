using System.Net.Http.Headers;
using System.Text.Json;
using WoWDashboard.Models;

namespace WoWDashboard.Services
{
    public class RaiderIOService
    {
        private readonly HttpClient _httpClient;

        public RaiderIOService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<(double Score, RaidProgression RaidProgression)> GetRaiderIoProfileAsync(string name, string realm, string region)
        {
            var realmSlug = realm.ToLower().Replace(" ", "-");
            var characterName = name.ToLower();
            var characterRegion = region.ToLower();
            var _accessToken= "RIO6iNmsRzWBUfFBEhfkZ4XsK";

            var requestUrl = $"https://raider.io/api/v1/characters/profile?region={characterRegion}&realm={realmSlug}&name={characterName}&fields=mythic_plus_scores_by_season%3Acurrent,raid_progression";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
                return (0, null);

            var content = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(content);

            var score = data.GetProperty("mythic_plus_scores_by_season")[0]
                            .GetProperty("scores")
                            .GetProperty("all")
                            .GetDouble();

            var raidProgressionData = data.GetProperty("raid_progression");
            var firstRaid = raidProgressionData.EnumerateObject().First();

            var progression = new RaidProgression
            {
                RaidName = firstRaid.Name.ToUpper().Replace("-", " "),
                Summary = firstRaid.Value.GetProperty("summary").GetString(),
            };

            return (score, progression);
        }
    }
}
