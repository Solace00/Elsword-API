﻿using System.Collections.Generic;
using System.Diagnostics;

namespace Elsword_API.Models
{
    public class Skills
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string MpCost { get; set; }
        public string Cooldown { get; set; }
        public string Description { get; set; }
        public string Effect { get; set; }
        public string ImageUrl { get; set; }
        public List<Trait> Traits { get; set; } = new List<Trait>();
    }

    public class Trait
    {
        public string Type { get; set; }
        public string Description { get; set; }
    }
}
