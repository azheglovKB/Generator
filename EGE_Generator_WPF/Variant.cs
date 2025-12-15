using System.Collections.Generic;

namespace EgeGenerator.Models
{
    public class Variant
    {
        public string Name { get; set; } = string.Empty;
        public List<EgeTask> Tasks { get; set; } = new List<EgeTask>();
        public string Path { get; set; } = string.Empty;
        public bool IsComplete => Tasks.Count == 27;
    }
}