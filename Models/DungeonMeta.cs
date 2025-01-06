using System.Collections.Generic;
using Newtonsoft.Json;

namespace Elsword_API.Models
{
    public class DungeonMeta
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("short_name")]
        public string ShortName { get; set; }

        [JsonProperty("other_aliases")]
        public List<string> OtherAliases { get; set; }
    }
}