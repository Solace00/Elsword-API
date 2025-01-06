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
    public class ElwikiInfoScraper
    {
        private readonly HttpClient _httpClient;

        public ElwikiInfoScraper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ElwikiInfo> ScrapeElwikiInfoAsync()
        {
            var url = "https://elwiki.net/w/Main_Page";
            var response = await _httpClient.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(response);

            var elwikiInfo = new ElwikiInfo
            {
                WallTime = ExtractWallTime(doc),
                PlegasTime = ExtractPlegasTime(doc),
                ServerTime = ExtractServerTime(doc),
                HenirChallenge = ExtractHenirChallenge(doc),
                SeasonalIceBurner = ExtractSeasonalIceBurner(doc),
                SeasonalFurniture = ExtractSeasonalFurniture(doc)
            };
            return elwikiInfo;
        }

        private string ExtractWallTime(HtmlDocument doc)
        {
            try
            {
                // Select the countdown timer for The Great Steel Wall
                var wallTimeNode = doc.DocumentNode.SelectSingleNode("//div[@class='boss-timer boss-timer-NA']//p[@id='boss1-localization-NA']/following-sibling::div[@class='boss-timer-caption']//div[@class='boss-timer-countdown']");
                var wallTime = wallTimeNode?.InnerText.Trim() ?? "Wall time not found.";
                Console.WriteLine($"Extracted Wall Time: {wallTime}"); // Log the extracted value
                return wallTime;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting Wall Time: {ex.Message}");
                return "Wall time extraction error.";
            }
        }

        private string ExtractPlegasTime(HtmlDocument doc)
        {
            try
            {
                // Select the countdown timer for Plegas's Labyrinth
                var plegasTimeNode = doc.DocumentNode.SelectSingleNode("//div[@class='boss-timer boss-timer-NA']//p[@id='boss2-localization-NA']/following-sibling::div[@class='boss-timer-caption']//div[@class='boss-timer-countdown']");
                var plegasTime = plegasTimeNode?.InnerText.Trim() ?? "Plegas time not found.";
                Console.WriteLine($"Extracted Plegas Time: {plegasTime}"); // Log the extracted value
                return plegasTime;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting Plegas Time: {ex.Message}");
                return "Plegas time extraction error.";
            }
        }

        private string ExtractServerTime(HtmlDocument doc)
        {
            try
            {
                var serverTimeNode = doc.DocumentNode.SelectSingleNode("//div[@class='server-time-wrap']//div[@class='server-time']");
                var serverTime = serverTimeNode?.InnerText.Trim() ?? "Server time not found.";
                Console.WriteLine($"Extracted Server Time: {serverTime}"); // Log the extracted value
                return serverTime;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting Server Time: {ex.Message}");
                return "Server time extraction error.";
            }
        }

        private string ExtractHenirChallenge(HtmlDocument doc)
        {
            try
            {
                var henirChallengeTitleNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'henir-caption')]");
                var henirChallengeTitle = henirChallengeTitleNode?.InnerText.Trim() ?? "Henir Challenge not found.";

                var henirBossNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'henir-stage')]//span[contains(@class, 'text')]");
                var henirBosses = new List<string>();
                if (henirBossNodes != null)
                {
                    foreach (var bossNode in henirBossNodes)
                    {
                        henirBosses.Add(bossNode.InnerText.Trim());
                    }
                }
                var henirChallenge = $"{henirChallengeTitle} - Bosses: {string.Join(", ", henirBosses)}";
                Console.WriteLine($"Extracted Henir Challenge: {henirChallenge}"); // Log the extracted value
                return henirChallenge;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting Henir Challenge: {ex.Message}");
                return "Henir Challenge extraction error.";
            }
        }

        private string ExtractSeasonalIceBurner(HtmlDocument doc)
        {
            var seasonalIceBurnerNode = doc.DocumentNode.SelectSingleNode("//div[@class='tabber-content']//div[@class='heading-base border-color']//a[@title='Empire Reign']");
            var dateNode = seasonalIceBurnerNode?.ParentNode?.ParentNode?.SelectSingleNode(".//div[@class='date']");
            return seasonalIceBurnerNode?.InnerText.Trim() + " " + dateNode?.InnerText.Trim() ?? "Seasonal Ice Burner not found.";
        }

        private string ExtractSeasonalFurniture(HtmlDocument doc)
        {
            var seasonalFurnitureNode = doc.DocumentNode.SelectSingleNode("//div[@class='tabber-content' and .//a[@title='El House Furniture/Midnight Romance']]");
            var dateNode = seasonalFurnitureNode?.ParentNode?.SelectSingleNode(".//div[@class='date']");
            return seasonalFurnitureNode?.InnerText.Trim() + " " + dateNode?.InnerText.Trim() ?? "Seasonal Furniture not found.";
        }

    }
}
