using Elsword_API.Models;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
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
            var nameNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'dungeon_page_dungeon_title__0wRCr')]");
            return nameNode?.InnerText.Trim() ?? "Name not found.";
        }

        private string ExtractDungeonDescription(HtmlDocument doc)
        {
            var descriptionNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'dungeon_page_description_iUzFm')]");
            return descriptionNode.InnerText ?? "Description not found.";
        }

        private string ExtractCombatPowerRequired(HtmlDocument doc)
        {
            var combatPowerNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'dungeon_page_combat_power__lIwnu')]");
            return combatPowerNode?.InnerText.Trim() ?? "Combat power not found.";
        }

        private string ExtractLevelRequirement(HtmlDocument doc)
        {
            var levelRequirementNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'dungeon_page_entry_data__31fok')]//div[contains(@class, 'fa-angles-up')]/following-sibling::strong");
            return levelRequirementNode?.InnerText.Trim() ?? "Level requirement not found.";
        }

        private string ExtractRegionDebuff(HtmlDocument doc)
        {
            var debuffNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'dungeon_page_entry_data__31fok')]//div[contains(@class, 'fa-virus')]/following-sibling::strong");
            return debuffNode?.InnerText.Trim() ?? "Region debuff not found.";
        }

        private string ExtractNumberOfPlayers(HtmlDocument doc)
        {
            var playersNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'dungeon_page_entry_data__31fok')]//div[contains(@class, 'fa-person')]/following-sibling::span/strong");
            return playersNode?.InnerText.Trim() ?? "Number of players not found.";
        }

        private string ExtractResurrectionLimit(HtmlDocument doc)
        {
            var resurrectionNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'dungeon_page_entry_data__31fok')]//div[contains(@class, 'fa-skull-crossbones')]/following-sibling::strong");
            return resurrectionNode?.InnerText.Trim() ?? null; // Return null if not found
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
                    var bossTypeNode = node.SelectSingleNode(".//div[contains(@class, 'dungeon_page_header__WRZgB')]//img[@class='dungeon_page_mob_icon__PcD5M']");
                    var bossType = bossTypeNode?.GetAttributeValue("alt", null);

                    // Only process nodes where the alt text is "Boss"
                    if (bossType == "Boss")
                    {
                        var bossNameNode = node.SelectSingleNode(".//div[contains(@class, 'dungeon_page_header__WRZgB')]/span");
                        var bossImageNode = node.SelectSingleNode(".//div[contains(@class, 'dungeon_page_header__WRZgB')]/img");

                        var bossStatsItem = new BossStats
                        {
                            Name = bossNameNode?.InnerText.Trim(),
                            ImageUrl = bossImageNode?.GetAttributeValue("src", null),
                            HP = ExtractBossStatValue(node, "heart"),
                            PhysicalDefense = ExtractBossStatValue(node, "shield-halved", 0),
                            MagicalDefense = ExtractBossStatValue(node, "shield-halved", 1),
                            FreezeDuration = ExtractBossStatValue(node, "snowflake"),
                            PetrifyDuration = ExtractBossStatValue(node, "lock"),
                            FireResistance = ExtractBossResistance(node, "fire"),
                            WaterResistance = ExtractBossResistance(node, "water"),
                            WindResistance = ExtractBossResistance(node, "wind"),
                            NatureResistance = ExtractBossResistance(node, "seedling"),
                            LightResistance = ExtractBossResistance(node, "bolt"),
                            DarkResistance = ExtractBossResistance(node, "moon")
                        };

                        bossStats.Add(bossStatsItem);
                    }
                }
            }
            return bossStats;
        }

        private string ExtractBossStatValue(HtmlNode node, string iconName, int occurrence = 0)
        {
            var statNode = node.SelectNodes($".//svg[contains(@data-icon, '{iconName}')]/following-sibling::span[@class='dungeon_page_stat_value__a0Hkk']")[occurrence];
            return statNode?.InnerText.Trim();
        }

        private string ExtractBossResistance(HtmlNode node, string iconName)
        {
            var resistanceNode = node.SelectSingleNode($".//svg[contains(@data-icon, '{iconName}')]/following-sibling::div[@class='dungeon_page_bar_wrapper__3OYSu']//span[@class='dungeon_page_stat_value__a0Hkk']");
            return resistanceNode?.InnerText.Trim();
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
