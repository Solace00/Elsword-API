using Microsoft.AspNetCore.Mvc;
using Elsword_API.Services;
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

        // Handle requests with path parameters like /api/search/Dungeon/15849414
        [HttpGet("{idOrAlias}")]
        public async Task<IActionResult> GetDungeonByIdOrAlias(string idOrAlias)
        {
            string dungeonId = ResolveDungeonId(idOrAlias);
            if (string.IsNullOrEmpty(dungeonId))
            {
                return NotFound("Dungeon not found.");
            }

            var dungeon = await _dungeonScraper.ScrapeDungeonsAsync(dungeonId);
            if (dungeon == null)
            {
                return NotFound("Dungeon not found.");
            }

            return Ok(dungeon);
        }

        private string ResolveDungeonId(string idOrAlias)
        {
            if (!string.IsNullOrEmpty(idOrAlias))
            {
                // Check if idOrAlias is a direct ID
                if (_mappingService.Dungeons.ContainsKey(idOrAlias))
                {
                    return idOrAlias;
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
                        return kvp.Key;
                    }
                }
            }

            return null;
        }
    }
}