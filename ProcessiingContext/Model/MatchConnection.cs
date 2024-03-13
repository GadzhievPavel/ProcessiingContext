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
    public class MatchConnection
    {
        private ReferenceObject match;
        private NomenclatureObject nomenclature;
        private ComplexHierarchyLink sourceHierarhyLink;
        private ComplexHierarchyLink addHierarhyLink;
        private ComplexHierarchyLink removeHierarhyLink;

        public ReferenceObject Match
        {
            get { return match; }
            //set { match = value; }
        }

        public NomenclatureObject Nomenclature
        {
            get { return nomenclature; }
            //set { nomenclature = value; }
        }

        public ComplexHierarchyLink SourceHierarhyLink
        {
            get { return sourceHierarhyLink; }
            //set { sourceHierarhyLink = value; }
        }

        public ComplexHierarchyLink AddHierarhyLink
        {
            get { return addHierarhyLink; }
            //set { addHierarhyLink = value; }
        }

        public ComplexHierarchyLink RemoveHierarhyLink
        {
            get { return removeHierarhyLink; }
            //set { removeHierarhyLink = value; }
        }

        public MatchConnection(ReferenceObject nomenclature, ComplexHierarchyLink SourceHierarchyLink,
            ComplexHierarchyLink AddHierarchyLink, ComplexHierarchyLink RemoveHierarchyLink)
        {
            this.nomenclature = nomenclature as NomenclatureObject;
            this.sourceHierarhyLink = SourceHierarchyLink;
            this.addHierarhyLink = AddHierarchyLink;
            this.removeHierarhyLink = RemoveHierarchyLink;
        }

        public MatchConnection(ReferenceObject match)
        {
            this.nomenclature = match.GetObject(Guids.NotifyReference.Link.ObjectPDM) as NomenclatureObject;
            this.sourceHierarhyLink = match.Links.ToOneToComplexHierarchy[Guids.NotifyReference.Link.SourceHierarchyLink].LinkedComplexLink;
            this.addHierarhyLink = match.Links.ToOneToComplexHierarchy[Guids.NotifyReference.Link.AddHierarchyLink].LinkedComplexLink;
            this.removeHierarhyLink = match.Links.ToOneToComplexHierarchy[Guids.NotifyReference.Link.RemoveHierarchyLink].LinkedComplexLink;
        }

        public override string ToString()
        {
            return $"Совпадение {match} номенклатура {nomenclature} исходное подключение {sourceHierarhyLink}" +
                $" добавленное подключение {addHierarhyLink} удаляемое подключение {removeHierarhyLink}";
        }
    }
}
