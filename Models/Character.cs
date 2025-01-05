namespace Elsword_API.Models
{
    public class Character
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Skill> Skills { get; set; }
    }

    public class Skill
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
