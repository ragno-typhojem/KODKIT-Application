using System.Collections.Generic;

namespace KodKit.Models
{
    public class Project
    {
        public required string Name { get; set; } = string.Empty;
        public List<Block> Blocks { get; set; } = new();
    }
}
