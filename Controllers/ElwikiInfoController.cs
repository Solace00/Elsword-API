using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Elsword_API.Services;
using Elsword_API.Models;
using System;

namespace Elsword_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ElwikiInfoController : Controller
    {
        private readonly ElwikiInfoScraper _elwikiScraper;

        public ElwikiInfoController(ElwikiInfoScraper elwikiScraper)
        {
            _elwikiScraper = elwikiScraper;
        }

        // Action method to fetch the Elwiki information
        [HttpGet("Info")]
        public async Task<IActionResult> GetElwikiInfo()
        {
            try
            {
                var elwikiInfo = await _elwikiScraper.ScrapeElwikiInfoAsync();
                return Ok(elwikiInfo); // Return the ElwikiInfo object as JSON
            }
            catch (Exception ex)
            {
                // Handle the exception as needed
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
