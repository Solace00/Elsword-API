using Microsoft.AspNetCore.Mvc;
using Elsword_API.Services;
using Elsword_API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elsword_API.Controllers
{
    [ApiController]
    [Route("api/search/Dungeon")]
    public class DungeonController : ControllerBase
    {
        private readonly DungeonScraper _dungeonScraper;
        private readonly MappingService _mappingService;

        public DungeonController(DungeonScraper dungeonScraper, MappingService mappingService)
        {
            _dungeonScraper = dungeonScraper;
            _mappingService = mappingService;
        }

        [HttpGet("{idOrAlias}")]
        public async Task<IActionResult> GetDungeonByIdOrAlias(string idOrAlias)
        {
            var dungeonIds = ResolveDungeonIds(idOrAlias);
            if (dungeonIds == null || !dungeonIds.Any())
            {
                return NotFound("Dungeon not found.");
            }

            var dungeons = new List<Dungeons>();
            var allAliases = new List<string>();

            foreach (var dungeonId in dungeonIds)
            {
                var dungeonMeta = _mappingService.Dungeons[dungeonId];
                if (dungeonMeta.OtherAliases != null)
                {
                    allAliases.AddRange(dungeonMeta.OtherAliases);
                }

                var dungeon = await _dungeonScraper.ScrapeDungeonsAsync(dungeonId);
                if (dungeon != null)
                {
                    dungeons.Add(dungeon);
                }
            }

            allAliases = allAliases.Distinct().ToList();

            if (!dungeons.Any())
            {
                return NotFound("Dungeon not found.");
            }

            return Ok(new { Dungeons = dungeons, Aliases = allAliases });
        }

        private List<string> ResolveDungeonIds(string idOrAlias)
        {
            var matchedIds = new List<string>();

            if (!string.IsNullOrEmpty(idOrAlias))
            {
                // Check if idOrAlias is a direct ID
                if (_mappingService.Dungeons.ContainsKey(idOrAlias))
                {
                    matchedIds.Add(idOrAlias);
                }

                // Check if idOrAlias matches any alias
                foreach (var kvp in _mappingService.Dungeons)
                {
                    var dungeon = kvp.Value;
                    var aliases = dungeon.OtherAliases ?? new List<string>();
                    if (string.Equals(dungeon.Name, idOrAlias, System.StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(dungeon.ShortName, idOrAlias, System.StringComparison.OrdinalIgnoreCase) ||
                        aliases.Any(a => string.Equals(a, idOrAlias, System.StringComparison.OrdinalIgnoreCase)))
                    {
                        matchedIds.Add(kvp.Key);
                    }
                }
            }

            return matchedIds;
        }
    }
}