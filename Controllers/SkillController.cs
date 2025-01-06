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

        public SkillController(SkillScraper skillScraper)
        {
            _skillScraper = skillScraper;
        }

        [HttpGet("{skillId}")]
        public async Task<IActionResult> GetSkillInfo(string skillId)
        {
            var skillInfo = await _skillScraper.ScrapeSkillInfoAsync(skillId);

            if (skillInfo == null)
            {
                return NotFound();
            }

            return Ok(skillInfo);
        }
    }
}