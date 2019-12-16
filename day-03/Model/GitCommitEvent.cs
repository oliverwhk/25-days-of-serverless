using System.Collections.Generic;

namespace day_03.Model
{
    public class GitCommitEvent
    {
        public IEnumerable<Commit> Commits { get; set; }
        public Repository Repository { get; set; }
    }
}
