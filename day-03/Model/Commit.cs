using System.Collections.Generic;

namespace day_03.Model
{
    public class Commit
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public IEnumerable<string> Added { get; set; }
    }
}