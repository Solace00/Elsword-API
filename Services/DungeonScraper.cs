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
    public class DungeonScraper
    {
        private readonly HttpClient _httpclient;

        public DungeonScraper(HttpClient httpclient)
        {
            _httpclient = httpclient;
        }

        public async Task<Dungeons> ScrapeDungeonsAsync(string dungeonId)
        {
            var url = $"https://cobodex.eu/dungeon/{dungeonId}";
            var response = await _httpclient.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(response);

            var dungeon = new Dungeons
            {
                Name = ExtractDungeonName(doc),
                Description = ExtractDungeonDescription(doc),
                CombatPowerRequired = ExtractCombatPowerRequired(doc),
                LevelRequirement = ExtractLevelRequirement(doc),
                RegionDebuff = ExtractRegionDebuff(doc),
                NumberOfPlayers = ExtractNumberOfPlayers(doc),
                ResurrectionLimit = ExtractResurrectionLimit(doc),
                FeaturedDrops = ExtractFeaturedDrops(doc).Select(drop => drop.Description).ToList(),
                BossStats = ExtractBossStats(doc).Cast<MonsterStats>().ToList(),
            };

            return dungeon;
        }

        private string ExtractDungeonName(HtmlDocument doc)
        {
            var nameNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'dungeon_page_dungeon_title__0wRCr')]//h1");
            return nameNode?.InnerText.Trim() ?? "Name not found.";
        }

        private string ExtractDungeonDescription(HtmlDocument doc)
        {
            var descriptionNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'dungeon_page_description__iUzFm')]");
            if (descriptionNode != null)
            {
                var paragraphs = descriptionNode.SelectNodes(".//p/span[@style='white-space: pre-line;']")
                    ?.Select(p => p.InnerText.Trim());
                return paragraphs != null ? string.Join(" ", paragraphs) : "Description not found.";
            }
            return "Description not found.";
        }

        private string ExtractCombatPowerRequired(HtmlDocument doc)
        {
            var combatPowerNode = doc.DocumentNode.SelectSingleNode("//div[svg[@data-icon='burst']]/span[contains(@class, 'dungeon_page_combat_power__lIwnu')]");
            return combatPowerNode?.InnerText.Trim() ?? "Combat power not found.";
        }

        private string ExtractLevelRequirement(HtmlDocument doc)
        {
            var levelRequirementNode = doc.DocumentNode.SelectSingleNode("//div[svg[@data-icon='angles-up']]/strong");
            return levelRequirementNode?.InnerText.Trim() ?? "Level requirement not found.";
        }

        private string ExtractRegionDebuff(HtmlDocument doc)
        {
            var debuffNode = doc.DocumentNode.SelectSingleNode("//div[svg[@data-icon='virus']]/strong");
            return debuffNode?.InnerText.Trim() ?? "Region debuff not found.";
        }

        private string ExtractNumberOfPlayers(HtmlDocument doc)
        {
            var playersNode = doc.DocumentNode.SelectSingleNode("//div[svg[@data-icon='person']]/span/strong");
            return playersNode?.InnerText.Trim() ?? "Number of players not found.";
        }

        private string ExtractResurrectionLimit(HtmlDocument doc)
        {
            var resurrectionNode = doc.DocumentNode.SelectSingleNode("//div[svg[@data-icon='skull-crossbones']]/strong");
            return resurrectionNode?.InnerText.Trim() ?? string.Empty; // Return empty string if not found
        }

        private static List<FeaturedDrops> ExtractFeaturedDrops(HtmlDocument doc)
        {
            var featuredDrops = new List<FeaturedDrops>();
            var dropNodes = doc.DocumentNode.SelectNodes("//div[@class='dungeon_page_drop_list__FFBcg']//div[@class='dungeon_page_item_icon__h3Rfa']//img");

            if (dropNodes != null)
            {
                foreach (var dropNode in dropNodes)
                {
                    var imageUrl = dropNode.GetAttributeValue("src", null);
                    var description = dropNode.GetAttributeValue("alt", null);
                    featuredDrops.Add(new FeaturedDrops
                    {
                        ImageUrl = imageUrl,
                        Description = description
                    });
                }
            }

            return featuredDrops;
        }

        private List<BossStats> ExtractBossStats(HtmlDocument doc)
        {
            var bossStats = new List<BossStats>();
            var bossNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'dungeon_page_npc_card__omeU0')]");

            if (bossNodes != null)
            {
                foreach (var node in bossNodes)
                {
                    var bossTypeNode = node.SelectSingleNode(".//img[contains(@class, 'dungeon_page_mob_icon__PcD5M')]");
                    var bossType = bossTypeNode?.GetAttributeValue("alt", null);

                    // Only process nodes where the alt text is "Boss"
                    if (bossType == "Boss")
                    {
                        var bossNameNode = node.SelectSingleNode(".//span");
                        var bossImageNode = node.SelectSingleNode(".//img[contains(@class, 'dungeon_page_boss_icon__zuUfy')]");

                        var bossStatsItem = new BossStats
                        {
                            Name = bossNameNode?.InnerText.Trim() ?? "Name not found.",
                            ImageUrl = bossImageNode?.GetAttributeValue("src", null) ?? string.Empty,
                            HP = ExtractBossStatValue(node, "heart") ?? "HP not found.",
                            PhysicalDefense = ExtractBossStatValue(node, "shield-halved", "rgb(186, 20, 11)") ?? "Physical defense not found.",
                            MagicalDefense = ExtractBossStatValue(node, "shield-halved", "rgb(31, 165, 237)") ?? "Magical defense not found.",
                            FreezeDuration = ExtractBossStatValue(node, "snowflake") ?? "Freeze duration not found.",
                            PetrifyDuration = ExtractBossStatValue(node, "lock") ?? "Petrify duration not found.",
                            FireResistance = ExtractBossStatValue(node, "fire") ?? "Fire resistance not found.",
                            WaterResistance = ExtractBossStatValue(node, "water") ?? "Water resistance not found.",
                            WindResistance = ExtractBossStatValue(node, "wind") ?? "Wind resistance not found.",
                            NatureResistance = ExtractBossStatValue(node, "seedling") ?? "Nature resistance not found.",
                            LightResistance = ExtractBossStatValue(node, "bolt") ?? "Light resistance not found.",
                            DarkResistance = ExtractBossStatValue(node, "moon") ?? "Dark resistance not found."
                        };

                        bossStats.Add(bossStatsItem);
                    }
                }
            }
            return bossStats;
        }

        private string ExtractBossStatValue(HtmlNode node, string iconName, string color = null)
        {
            // Target the specific <li> element that contains the SVG with the specified iconName
            string xpath = $".//li[div/svg[contains(@data-icon, '{iconName}')]]";

            // Get the value within the span with class 'dungeon_page_stat_value__a0Hkk'
            xpath += "//span[@class='dungeon_page_stat_value__a0Hkk']";

            var valueNode = node.SelectSingleNode(xpath);
            return valueNode?.InnerText.Trim();
        }

        public class FeaturedDrops
        {
            public string ImageUrl { get; set; }
            public string Description { get; set; }
        }

        public class BossStats : MonsterStats
        {
            public string ImageUrl { get; set; }
            public string FreezeDuration { get; set; }
            public string PetrifyDuration { get; set; }
            public string FireResistance { get; set; }
            public string WaterResistance { get; set; }
            public string WindResistance { get; set; }
            public string NatureResistance { get; set; }
            public string LightResistance { get; set; }
            public string DarkResistance { get; set; }
        }
    }
}

