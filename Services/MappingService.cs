using Elsword_API.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Elsword_API.Services
{
    public class MappingService
    {
        public Dictionary<string, DungeonMeta> Dungeons { get; private set; }

        public MappingService(IConfiguration configuration)
        {
            var dungeonsFilePath = configuration["DungeonsFilePath"];
            if (File.Exists(dungeonsFilePath))
            {
                var json = File.ReadAllText(dungeonsFilePath);
                Dungeons = JsonConvert.DeserializeObject<Dictionary<string, DungeonMeta>>(json);
            }
            else
            {
                Dungeons = new Dictionary<string, DungeonMeta>();
            }
        }
    }
}