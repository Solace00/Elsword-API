using Elsword_API.Models;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Elsword_API.Services
{
    public class SkillScraper
    {
        private readonly HttpClient _httpClient;

        public SkillScraper(HttpClient httpClient)
        {
            _httpClient = new HttpClient();
        }

        public async Task<Skills> ScrapeSkillInfoAsync(string skillId)
        {
            var url = $"https://cobodex.eu/skills/{skillId}";
            var response = await _httpClient.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(response);

            var skill = new Skills()
            {
                Name = ExtractSkillName(doc),
                Type = ExtractSkillType(doc),
                MpCost = ExtractMpCost(doc),
                Cooldown = ExtractCooldown(doc),
                Description = ExtractSkillDescription(doc),
                ImageUrl = ExtractSkillImageUrl(doc),
                Effect = ExtractSkillEffect(doc),
                Traits = ExtractSkillTraits(doc)
            };

            return skill;
        }

        private string ExtractSkillName(HtmlDocument doc)
        {
            var nameNode = doc.DocumentNode.SelectSingleNode("//h1");
            return nameNode?.InnerText.Trim() ?? "Skill name not found.";
        }

        private string ExtractSkillType(HtmlDocument doc)
        {
            var typeNode = doc.DocumentNode.SelectSingleNode("//strong");
            return typeNode?.InnerText.Trim() ?? "Skill type not found.";
        }

        private string ExtractMpCost(HtmlDocument doc)
        {
            var mpCostNode = doc.DocumentNode.SelectSingleNode("//div[@class='skill_page_info__EcwTI']/p[contains(text(), 'MP')]");
            return mpCostNode?.InnerText.Trim().Replace(" MP", "") ?? "Not applicable";
        }

        private string ExtractCooldown(HtmlDocument doc)
        {
            var cooldownNode = doc.DocumentNode.SelectSingleNode("//div[@class='skill_page_info__EcwTI']/p[2]");
            if (cooldownNode != null)
            {
                var cooldownValueNode = cooldownNode.SelectSingleNode(".//span[2]/span[1]");
                if (cooldownValueNode != null)
                {
                    return cooldownValueNode.InnerText.Trim() + " sec";
                }
            }
            return "Not applicable";
        }

        private string ExtractSkillDescription(HtmlDocument doc)
        {
            var descriptionNode = doc.DocumentNode.SelectSingleNode("//div[@class='skill_page_main_desc__lmun8']");
            return descriptionNode?.InnerText.Trim() ?? "Skill description not found.";
        }

        private string ExtractSkillImageUrl(HtmlDocument doc)
        {
            var imageNode = doc.DocumentNode.SelectSingleNode("//div[@class='skill_skill__9coMp']//img");
            return imageNode?.GetAttributeValue("src", "Image not found");
        }

        private string ExtractSkillEffect(HtmlDocument doc)
        {
            var effectNode = doc.DocumentNode.SelectSingleNode("//div[@class='skill_page_effective_desc__wMJEc']/p");
            return effectNode?.InnerText.Trim() ?? "Skill effect not found.";
        }

        private List<Trait> ExtractSkillTraits(HtmlDocument doc)
        {
            var traits = new List<Trait>();
            var traitNodes = doc.DocumentNode.SelectNodes("//div[@class='skill_page_traits__zqk9y']/div");
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
