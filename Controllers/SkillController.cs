using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Elsword_API.Services;
using Elsword_API.Models;
using System;

namespace Elsword_API.Controllers
{
    [ApiController]
    [Route("api/search/[controller]")]
    public class SkillController : ControllerBase
    {
        private readonly SkillScraper _skillScraper;
        private readonly SkillListScraper _skillListScraper;

        public SkillController(SkillScraper skillScraper, SkillListScraper skillListScraper)
        {
            _skillScraper = skillScraper;
            _skillListScraper = skillListScraper;
        }

        [HttpGet("{skillNameOrId}")]
        public async Task<IActionResult> GetSkillInfo(string skillNameOrId)
        {
            try
            {
                // Ensure the skill list is initialized
                await _skillListScraper.InitializeSkillListAsync();

                var skillInfo = await _skillScraper.ScrapeSkillInfoAsync(skillNameOrId);

                if (skillInfo == null)
                {
                    var possibleMatches = _skillListScraper.GetPossibleMatches(skillNameOrId);
                    return NotFound(new { message = $"Skill '{skillNameOrId}' not found. Did you mean: {string.Join(", ", possibleMatches)}?" });
                }

                return Ok(skillInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
