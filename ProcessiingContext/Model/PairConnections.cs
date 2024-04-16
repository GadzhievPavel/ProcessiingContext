using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFlex.DOCs.Model.References;

namespace ProcessiingContext.Model
{
    public class PairConnections
    {
        public ComplexHierarchyLink AddLink { get; set; }
        public ComplexHierarchyLink RemoveLink { get; set; }
        public ReferenceObject Match {  get; set; }
    }
}
