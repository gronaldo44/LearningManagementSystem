using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Class
    {
        public Class()
        {
            AssignmentCategories = new HashSet<AssignmentCategory>();
            Enrolleds = new HashSet<Enrolled>();
        }

        public uint Year { get; set; }
        public string Season { get; set; } = null!;
        public string? Location { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public uint CId { get; set; }
        public uint ClassId { get; set; }
        public string? TaughtBy { get; set; }

        public virtual Course CIdNavigation { get; set; } = null!;
        public virtual Professor? TaughtByNavigation { get; set; }
        public virtual ICollection<AssignmentCategory> AssignmentCategories { get; set; }
        public virtual ICollection<Enrolled> Enrolleds { get; set; }
    }
}
