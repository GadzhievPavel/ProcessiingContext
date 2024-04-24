using DeveloperUtilsLibrary;
using ProcessiingContext.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFlex.DOCs.Model;
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
        private ServerConnection connection;
        private ConfigurationSettings configSettings;

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

        public MatchConnection(ReferenceObject match, ConfigurationSettings configurationSettings, ServerConnection connection)
        {
            this.match = match;
            this.configSettings = configurationSettings;
            this.connection = connection;
            using (match.Reference.ChangeAndHoldConfigurationSettings(configSettings))
            {
                match.Reference.Refresh();
                match.Reload();
                this.nomenclature = match.GetObject(Guids.NotifyReference.Link.ObjectPDM) as NomenclatureObject;
                this.sourceHierarhyLink = match.Links.ToOneToComplexHierarchy[Guids.NotifyReference.Link.SourceHierarchyLink].LinkedComplexLink;
                this.addHierarhyLink = match.Links.ToOneToComplexHierarchy[Guids.NotifyReference.Link.AddHierarchyLink].LinkedComplexLink;
                this.removeHierarhyLink = match.Links.ToOneToComplexHierarchy[Guids.NotifyReference.Link.RemoveHierarchyLink].LinkedComplexLink;
            }
            this.dictLinks = new Dictionary<ComplexHierarchyLink, Boolean>();
        }

        /// <summary>
        /// Удаление подключений в заднном контексте
        /// </summary>
        /// <param name="designContext">контекст, в котором будет проводиться удаление подключений</param>
        public void DeleteComplexHierarhyLinkInContext(DesignContextObject designContext)
        {
            IReadOnlyCollection<ComplexHierarchyLink> readonlyCol = dictLinks.Keys;
            designContext.DeleteChangesAsync(readonlyCol);
        }

        /// <summary>
        /// копирование подключений в целевой контекст
        /// </summary>
        /// <param name="targetContext">целевой контекст</param>
        /// <returns></returns>
        public PairConnections CopyComplexHierarhyLInkInContext(DesignContextObject targetContext)
        {
            if (addHierarhyLink != null)
            {
                dictLinks.Add(this.addHierarhyLink, true);
            }
            if (removeHierarhyLink != null)
            {
                dictLinks.Add(this.removeHierarhyLink, true);
            }

            targetContext.CopyMoveChangesAsync(dictLinks);
            NomenclatureHandler nomenclatureHandler = new NomenclatureHandler(this.connection);
            var copyAddHierarchyLink = nomenclatureHandler.FindComplexHierarhyLink(addHierarhyLink, targetContext);
            var copyRemoveHierarchyLink = nomenclatureHandler.FindComplexHierarhyLink(removeHierarhyLink, targetContext);
            var pairConnections = new PairConnections()
            {
                AddLink = copyAddHierarchyLink,
                RemoveLink = copyRemoveHierarchyLink,
                Match = this.match
            };
            return pairConnections;
        }


        /// <summary>
        /// Обновляет соответствие подключений в T-FLEX DOCs в соответствии с параметрами, хранящимися в объекте представления
        /// </summary>
        public void UpdateReferenceObject()
        {
            using (match.Reference.ChangeAndHoldConfigurationSettings(this.configSettings))
            {
                match.Reference.Refresh();
                match.Reload();
                this.match.StartUpdate();
                match.Links.ToOneToComplexHierarchy[Guids.NotifyReference.Link.AddHierarchyLink].SetLinkedComplexLink(this.addHierarhyLink);
                match.Links.ToOneToComplexHierarchy[Guids.NotifyReference.Link.RemoveHierarchyLink].SetLinkedComplexLink(this.removeHierarhyLink);
                match.Links.ToOneToComplexHierarchy[Guids.NotifyReference.Link.SourceHierarchyLink].SetLinkedComplexLink(this.SourceHierarhyLink);
                this.match.EndChanges();
            }
        }

        public override string ToString()
        {
            var matchStr = match == null ? "null" : match.ToString();
            var nomStr = nomenclature == null ? "null" : nomenclature.ToString();
            var sourceStr = sourceHierarhyLink == null ? "null" : $"{sourceHierarhyLink} {sourceHierarhyLink.Id}";
            var addLink = addHierarhyLink == null ? "null" : $"{addHierarhyLink} {addHierarhyLink.Id}";
            var removeLink = removeHierarhyLink == null ? "null" : $"{removeHierarhyLink} {removeHierarhyLink.Id}";
            return $"[Совпадение]: {matchStr}\n" +
                $" [номенклатура]: {nomStr}\n" +
                $" [исходное подключение]: {sourceStr}\n" +
                $" [добавленное подключение]: {addLink}\n" +
                $" [удаляемое подключение]: {removeLink}\n";
            //$" [конфигурация DesignContext]:/*{configDesign}*/\n";
        }
    }
}
