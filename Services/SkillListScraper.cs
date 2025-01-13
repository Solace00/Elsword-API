using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Elsword_API.Services
{
    public class SkillListScraper
    {
        private readonly HttpClient _httpClient;
        private Dictionary<string, string> _skillNameToIdMap;

        public SkillListScraper(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _skillNameToIdMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public async Task InitializeSkillListAsync()
        {
            _skillNameToIdMap.Clear(); // Clear existing mappings
            int page = 1;
            bool hasNextPage = true;
            int totalSkillsMapped = 0;

            while (hasNextPage)
            {
                var url = $"https://cobodex.eu/all_skills?page={page}";
                var response = await _httpClient.GetStringAsync(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(response);

                var skillNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'view_button_view_button__dLwDP')]");
                if (skillNodes == null || !skillNodes.Any())
                {
                    Console.WriteLine($"No skills found on page {page}. Stopping.");
                    break;
                }

                int skillsOnThisPage = 0;
                foreach (var skillNode in skillNodes)
                {
                    var skillName = skillNode.SelectSingleNode(".//span/span")?.InnerText.Trim();
                    var skillId = skillNode.SelectSingleNode(".//a")?.GetAttributeValue("href", "").Replace("/skills/", "").Trim();

                    if (!string.IsNullOrEmpty(skillName) && !string.IsNullOrEmpty(skillId))
                    {
                        _skillNameToIdMap[skillName] = skillId;
                        skillsOnThisPage++;
                        totalSkillsMapped++;
                        Console.WriteLine($"Mapped Skill: {skillName} -> {skillId}");
                    }
                }

                Console.WriteLine($"Page {page}: Mapped {skillsOnThisPage} skills. Total skills mapped: {totalSkillsMapped}");

                var nextPageLink = doc.DocumentNode.SelectSingleNode("//a[contains(@class, 'next') or contains(text(), 'Next')]");
                hasNextPage = nextPageLink != null;
                Console.WriteLine($"Next page link found: {hasNextPage}");
                page++;
            }

            Console.WriteLine($"Finished mapping. Total skills mapped: {totalSkillsMapped}");
        }

        public string GetSkillIdByName(string skillName)
        {
            if (_skillNameToIdMap == null || !_skillNameToIdMap.Any())
            {
                throw new Exception("Skill mapping is empty. Ensure InitializeSkillListAsync() has been called.");
            }

            if (_skillNameToIdMap.TryGetValue(skillName, out var skillId))
            {
                return skillId;
            }

            var partialMatch = _skillNameToIdMap
                .FirstOrDefault(kvp => kvp.Key.Contains(skillName, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(partialMatch.Key))
            {
                return partialMatch.Value;
            }

            Console.WriteLine($"No match found for skill name: {skillName}");
            return null;
        }

        public IEnumerable<string> GetPossibleMatches(string partialName)
        {
            if (_skillNameToIdMap == null || !_skillNameToIdMap.Any())
            {
                throw new Exception("Skill mapping is empty. Ensure InitializeSkillListAsync() has been called.");
            }

            return _skillNameToIdMap.Keys
                .Where(name => name.Contains(partialName, StringComparison.OrdinalIgnoreCase))
                .Take(5); // Limit to 5 suggestions
        }
    }
}
