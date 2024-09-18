using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    /// <summary>
    /// This is Assignment Categories
    /// It is a object on the database
    /// </summary>
    public partial class AssignmentCategory
    {
        public AssignmentCategory()
        {
            Assignments = new HashSet<Assignment>();
        }

        public uint? GradingWeight { get; set; }
        public string Name { get; set; } = null!;
        public uint ClassId { get; set; }
        public uint CategId { get; set; }

        public virtual Class Class { get; set; } = null!;
        public virtual ICollection<Assignment> Assignments { get; set; }
    }
}
