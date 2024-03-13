using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFlex.DOCs.Model.References;
using TFlex.DOCs.Model.References.Modifications;
using TFlex.DOCs.Model.References.Nomenclature;

namespace ProcessiingContext.Model
{
    public class Modification
    {
        private ReferenceObject modification;
        private DesignContextObject designContext;
        private List<UsingArea> usingAreas;

        public ReferenceObject ModificationObject
        {
            get { return modification; }
            set { modification = value; }
        }

        public DesignContextObject DesignContextObject
        {
            get { return designContext; }
            set { designContext = value; }
        }

        public List<UsingArea> UsingAreas
        {
            get { return usingAreas; }
            set { usingAreas = value; }
        }

        public Modification(ReferenceObject modification)
        {
            this.modification = modification;
            this.designContext = modification.GetObject(ModificationReferenceObject.RelationKeys.DesignContext) as DesignContextObject;
            this.usingAreas = new List<UsingArea>();
            var list = modification.GetObjects(ModificationReferenceObject.RelationKeys.UsingArea);
            foreach (var item in list)
            {
                this.usingAreas.Add(new UsingArea(item.GetObjects(Guids.NotifyReference.Link.MatchesConnection)));
            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Изменение {modification} в контексте {designContext}:\n");
            foreach (var item in usingAreas)
            {
                stringBuilder.Append(item.ToString());
                stringBuilder.Append("\n");
            }
            return stringBuilder.ToString();
        }
    }
}
