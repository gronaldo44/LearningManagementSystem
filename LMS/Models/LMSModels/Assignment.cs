using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Assignment
    {
        public Assignment()
        {
            Submissions = new HashSet<Submission>();
        }

        public string Name { get; set; } = null!;
        public uint? MaxPoints { get; set; }
        public string? Contents { get; set; }
        public DateTime? Due { get; set; }
        public uint AId { get; set; }
        public uint CategId { get; set; }

        public virtual AssignmentCategory Categ { get; set; } = null!;
        public virtual ICollection<Submission> Submissions { get; set; }
    }
}
