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
        private HashSet<int> _processedPages; // Track processed pages
        private const int MAX_PAGES = 150; // Safety limit

        public SkillListScraper(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _skillNameToIdMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _processedPages = new HashSet<int>();
        }

        public async Task InitializeSkillListAsync()
        {
            _skillNameToIdMap.Clear();
            int totalSkillsMapped = 0;
            int currentPage = 1;
            bool hasNextPage = true;

            while (currentPage <= MAX_PAGES && hasNextPage)
            {
                if (_processedPages.Contains(currentPage))
                {
                    Console.WriteLine($"Page {currentPage} has already been processed. Skipping.");
                    break;
                }

                var url = $"https://cobodex.eu/all_skills?page={currentPage}";
                Console.WriteLine($"Fetching page {currentPage}: {url}");
                var response = await _httpClient.GetStringAsync(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(response);

                // Log HTML content for debugging
                Console.WriteLine($"HTML content of page {currentPage}: {doc.DocumentNode.InnerHtml.Substring(0, Math.Min(doc.DocumentNode.InnerHtml.Length, 500))}...");

                var skillNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'view_button_view_button__dLwDP')]");
                if (skillNodes == null || !skillNodes.Any())
                {
                    Console.WriteLine($"No skills found on page {currentPage}. Stopping.");
                    break;
                }

                Console.WriteLine($"Processing page {currentPage}...");

                int skillsOnThisPage = 0;
                foreach (var skillNode in skillNodes)
                {
                    var skillName = skillNode.SelectSingleNode(".//span/span")?.InnerText.Trim();
                    var skillId = skillNode.SelectSingleNode(".//a")?.GetAttributeValue("href", "").Replace("/skills/", "").Trim();

                    if (!string.IsNullOrEmpty(skillName) && !string.IsNullOrEmpty(skillId))
                    {
                        if (_skillNameToIdMap.ContainsKey(skillName))
                        {
                            Console.WriteLine($"Duplicate skill found: {skillName}. Skipping.");
                        }
                        else
                        {
                            _skillNameToIdMap[skillName] = skillId;
                            skillsOnThisPage++;
                            totalSkillsMapped++;
                            Console.WriteLine($"Mapped Skill: {skillName} -> {skillId}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Invalid skill entry found on page {currentPage}. Skipping.");
                    }
                }

                Console.WriteLine($"Page {currentPage}: Mapped {skillsOnThisPage} skills. Total skills mapped: {totalSkillsMapped}");

                // Add current page to processed pages
                _processedPages.Add(currentPage);

                // Check for next page
                var nextPageButton = doc.DocumentNode.SelectSingleNode("//button[@aria-label='Go to next page']");
                hasNextPage = nextPageButton != null && !nextPageButton.GetAttributeValue("disabled", "").Equals("disabled", StringComparison.OrdinalIgnoreCase);

                if (!hasNextPage)
                {
                    Console.WriteLine("Last page reached or pagination broken. Stopping.");
                    break;
                }

                currentPage++;
            }

            Console.WriteLine($"Finished mapping. Total skills mapped: {totalSkillsMapped}");
        }

        public string GetSkillIdByName(string skillName)
        {
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
            return _skillNameToIdMap.Keys
                .Where(name => name.Contains(partialName, StringComparison.OrdinalIgnoreCase))
                .Take(5); // Limit to 5 suggestions
        }
    }
}
