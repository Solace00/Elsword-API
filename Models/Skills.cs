using System.Collections.Generic;
using System.Diagnostics;

namespace Elsword_API.Models
{
    public class Skills
    {
        public required string Name { get; set; }
        public required string Type { get; set; }
        public required string MpCost { get; set; }
        public required string Cooldown { get; set; }
        public required string Description { get; set; }
        public required string Effect { get; set; }
        public required string ImageUrl { get; set; }
        public required List<Trait> Traits { get; set; } = new List<Trait>();
    }

    public class Trait
    {
        public required string Type { get; set; }
        public required string Description { get; set; }
    }
}
