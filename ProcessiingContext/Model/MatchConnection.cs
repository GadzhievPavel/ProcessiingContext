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

        private Dictionary<ComplexHierarchyLink, Boolean> dictLinks;
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
            this.dictLinks = new Dictionary<ComplexHierarchyLink, Boolean>();
        }

        public MatchConnection(ReferenceObject match, ConfigurationSettings configurationSettings)
        {
            this.nomenclature = match.GetObject(Guids.NotifyReference.Link.ObjectPDM) as NomenclatureObject;
            var srcHierarchyManager = match.Links.ToOneToComplexHierarchy[Guids.NotifyReference.Link.SourceHierarchyLink];
            using (srcHierarchyManager.LinkReference.ChangeAndHoldConfigurationSettings(configurationSettings))
            {
                this.sourceHierarhyLink = srcHierarchyManager.LinkedComplexLink;
            }
            var addHierarhyManager = match.Links.ToOneToComplexHierarchy[Guids.NotifyReference.Link.AddHierarchyLink];
            using (addHierarhyManager.LinkReference.ChangeAndHoldConfigurationSettings(configurationSettings))
            {
                this.addHierarhyLink = addHierarhyManager.LinkedComplexLink;
            }
            var removeHierarhyManager = match.Links.ToOneToComplexHierarchy[Guids.NotifyReference.Link.RemoveHierarchyLink];
            using (removeHierarhyManager.LinkReference.ChangeAndHoldConfigurationSettings(configurationSettings))
            {
                this.removeHierarhyLink = removeHierarhyManager.LinkedComplexLink;
            }
            this.dictLinks = new Dictionary<ComplexHierarchyLink, Boolean>();
        }

        public void DeleteComplexHierarhyLinkInContext(DesignContextObject designContext)
        {
            IReadOnlyCollection<ComplexHierarchyLink> readonlyCol = dictLinks.Keys;
            designContext.DeleteChangesAsync(readonlyCol);
        }

        public void CopyComplexHierarhyLInkInContext(DesignContextObject designContext)
        {
            dictLinks.Add(this.addHierarhyLink, true);
            dictLinks.Add(this.removeHierarhyLink, true);
            designContext.CopyMoveChangesAsync(dictLinks);
        }

        public override string ToString()
        {
            return $"Совпадение {match} номенклатура {nomenclature} исходное подключение {sourceHierarhyLink}" +
                $" добавленное подключение {addHierarhyLink} удаляемое подключение {removeHierarhyLink}";
        }
    }
}
