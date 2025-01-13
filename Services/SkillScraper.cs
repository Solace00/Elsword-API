using Elsword_API.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Elsword_API.Services
{
    public class SkillScraper
    {
        private readonly HttpClient _httpClient;
        private readonly SkillListScraper _skillListScraper;

        public SkillScraper(HttpClient httpClient, SkillListScraper skillListScraper)
        {
            _httpClient = httpClient;
            _skillListScraper = skillListScraper;
        }

        public async Task<Skills> ScrapeSkillInfoAsync(string skillNameOrId)
        {
            string skillId = int.TryParse(skillNameOrId, out _)
                ? skillNameOrId
                : _skillListScraper.GetSkillIdByName(skillNameOrId);

            if (string.IsNullOrEmpty(skillId))
            {
                var possibleMatches = _skillListScraper.GetPossibleMatches(skillNameOrId);
                throw new Exception($"Skill '{skillNameOrId}' not found. Did you mean: {string.Join(", ", possibleMatches)}?");
            }

            var url = $"https://cobodex.eu/skills/{skillId}";
            var response = await _httpClient.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(response);

            var mainNode = doc.DocumentNode.SelectSingleNode("//main[contains(@class, 'skill_page_main__JLgLG')]");

            if (mainNode == null)
            {
                throw new Exception($"Unable to find main node for skill ID '{skillId}'. Verify that the HTML structure has not changed.");
            }

            var skill = new Skills
            {
                Name = ExtractSkillName(mainNode),
                Type = ExtractSkillType(mainNode),
                MpCost = ExtractMpCost(mainNode),
                Cooldown = ExtractCooldown(mainNode),
                Description = ExtractSkillDescription(mainNode),
                ImageUrl = ExtractSkillImageUrl(mainNode),
                Effect = ExtractSkillEffect(mainNode),
                Traits = ExtractSkillTraits(mainNode)
            };

            return skill;
        }

        private string ExtractSkillName(HtmlNode mainNode)
        {
            var nameNode = mainNode.SelectSingleNode(".//h1");
            return nameNode?.InnerText.Trim() ?? "Skill name not found.";
        }

        private string ExtractSkillType(HtmlNode mainNode)
        {
            var typeNode = mainNode.SelectSingleNode(".//div[@class='skill_page_info__EcwTI']/strong");
            return typeNode?.InnerText.Trim() ?? "Skill type not found.";
        }

        private string ExtractMpCost(HtmlNode mainNode)
        {
            var mpCostNode = mainNode.SelectSingleNode(".//div[@class='skill_page_info__EcwTI']/p[contains(text(), 'MP')]");
            return mpCostNode?.InnerText.Trim().Replace(" MP", "") ?? "Not applicable";
        }

        private string ExtractCooldown(HtmlNode mainNode)
        {
            var cooldownNode = mainNode.SelectSingleNode(".//div[@class='skill_page_info__EcwTI']/p[contains(text(), 'Cooldown')]");
            if (cooldownNode != null)
            {
                var cooldownText = cooldownNode.InnerText.Trim();
                var cooldownValue = cooldownText.Split(new[] { "Cooldown" }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault()?.Trim();
                return !string.IsNullOrEmpty(cooldownValue) ? cooldownValue : "Not applicable";
            }
            return "Not applicable";
        }

        private string ExtractSkillDescription(HtmlNode mainNode)
        {
            var descriptionNode = mainNode.SelectSingleNode(".//div[@class='skill_page_main_desc__lmun8']");
            return descriptionNode?.InnerText.Trim() ?? "Skill description not found.";
        }

        private string ExtractSkillImageUrl(HtmlNode mainNode)
        {
            var imageNode = mainNode.SelectSingleNode(".//div[contains(@class, 'skill_skill__9coMp')]//img");
            return imageNode?.GetAttributeValue("src", "Image not found") ?? "Image URL not found.";
        }

        private string ExtractSkillEffect(HtmlNode mainNode)
        {
            var effectNode = mainNode.SelectSingleNode(".//div[@class='skill_page_effective_desc__wMJEc']/p");
            return effectNode?.InnerText.Trim() ?? "Skill effect not found.";
        }

        private List<Trait> ExtractSkillTraits(HtmlNode mainNode)
        {
            var traits = new List<Trait>();
            var traitNodes = mainNode.SelectNodes(".//div[@class='skill_page_traits__zqk9y']/div");
            if (traitNodes != null)
            {
                foreach (var traitNode in traitNodes)
                {
                    var trait = new Trait
                    {
                        Type = traitNode.SelectSingleNode(".//span")?.InnerText.Trim() ?? "Trait type not found.",
                        Description = traitNode.SelectSingleNode(".//p")?.InnerText.Trim() ?? "Trait description not found."
                    };
                    traits.Add(trait);
                }
            }
            return traits;
        }
    }
}
