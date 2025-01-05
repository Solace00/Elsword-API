using System.Collections.Generic;

namespace Elsword_API.Models
{
    public class Dungeons
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string CombatPowerRequired { get; set; }
        public string LevelRequirement { get; set; }
        public string RegionDebuff { get; set; }
        public string NumberOfPlayers { get; set; }
        public string ResurrectionLimit { get; set; }

        public List<string> FeaturedDrops { get; set; }
        public List<MonsterStats> BossStats { get; set; }
    }

    public class MonsterStats
    {
        public string Name { get; set; }
        public string HP { get; set; }
        public string PhysicalDefense { get; set; }
        public string MagicalDefense { get; set; }
        public string FireResistance { get; set; }
        public string WaterResistance { get; set; }
        public string NatureResistance { get; set; }
        public string WindResistance { get; set; }
        public string LightResistance { get; set; }
        public string DarkResistance { get; set; }
    }

    public class  BossStats : MonsterStats
    {
        public string FreezeDuration { get; set; }
        public string PetrifyDuration { get; set; }
    }
}
