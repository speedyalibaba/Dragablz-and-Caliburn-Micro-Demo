using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Links.Contract.Models
{
    public class LayoutStructureBranch
    {
        public LayoutStructureBranch(Guid id, Guid? childFirstBranchId, Guid? childSecondBranchId, Guid? childFirstTabSetId, Guid? childSecondTabSetId, Orientation orientation, double ratio)
        {
            Id = id;
            ChildFirstBranchId = childFirstBranchId;
            ChildSecondBranchId = childSecondBranchId;
            ChildFirstTabSetId = childFirstTabSetId;
            ChildSecondTabSetId = childSecondTabSetId;
            Orientation = orientation;
            Ratio = ratio;
        }

        public Guid Id { get; }

        public Guid? ChildFirstBranchId { get; }

        public Guid? ChildSecondBranchId { get; }

        public Guid? ChildFirstTabSetId { get; }

        public Guid? ChildSecondTabSetId { get; }

        public Orientation Orientation { get; }

        public double Ratio { get; }
    }
}
