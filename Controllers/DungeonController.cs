using Microsoft.AspNetCore.Mvc;
using Elsword_API.Services;
using System.Threading.Tasks;

namespace Elsword_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DungeonController : ControllerBase
    {
        private readonly DungeonScraper _dungeonScraper;

        public DungeonController(DungeonScraper dungeonScraper)
        {
            _dungeonScraper = dungeonScraper;
        }

        [HttpGet("{dungeonId}")]
        public async Task<IActionResult> GetDungeon(string dungeonId)
        {
            var dungeon = await _dungeonScraper.ScrapeDungeonsAsync(dungeonId);
            if (dungeon == null)
            {
                return NotFound("Dungeon not found.");
            }
            return Ok(dungeon);
        }
    }
}