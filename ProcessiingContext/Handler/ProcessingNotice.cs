using DeveloperUtilsLibrary;
using ProcessiingContext.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFlex.DOCs.Common;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.References;
using TFlex.DOCs.Model.References.Nomenclature;
using TFlex.DOCs.Resources.Strings;

namespace ProcessiingContext.Handler
{
    public class ProcessingNotice
    {
        private Notice notice;
        private ServerConnection connection;
        public ProcessingNotice(ReferenceObject _notice, ServerConnection connection, ConfigurationSettings configurationSettings)
        {
            this.notice = new Notice(_notice, connection, configurationSettings);
            this.connection = connection;
        }

        public void MoveToContext()
        {
            foreach (var modification in notice.Modifications)
            {
                var configSettiings = new ConfigurationSettings(connection.ConfigurationSettings)
                {
                    DesignContext = modification.DesignContextObject
                };
                using (notice.NoticeObject.Reference.ChangeAndHoldConfigurationSettings(configSettiings))
                {
                    //to do move
                }
            }
        }

        private void MoveHierarchyLinks(Modification modification, DesignContextObject contextDesign)
        {
            var designContext = modification.DesignContextObject;
            if (designContext is null)
            {
                return;
            }

            var newConfigurationSettings = new ConfigurationSettings(connection)
            {
                DesignContext = designContext,
                ApplyDesignContext = true,
                Date = Texts.TodayText,
                ApplyDate = true
            };

            var usingAreaLinksInContext = new List<ComplexHierarchyLink>();

            foreach (var usingAreaObject in modification.UsingAreas)
            {
                foreach (var match in usingAreaObject.Matches)
                {
                    FillUsingAreaLinksInContext(match, newConfigurationSettings, designContext, ref usingAreaLinksInContext, Guids.UsingAreaAddedLink);
                    FillUsingAreaLinksInContext(match, newConfigurationSettings, designContext, ref usingAreaLinksInContext, Guids.UsingAreaDeletedLink);
                }
                if (!usingAreaLinksInContext.IsNullOrEmpty())
                {
                    usingAreaLinksInContext = usingAreaLinksInContext.Distinct().ToList();
                    var dictLinks = new Dictionary<ComplexHierarchyLink, Boolean>();
                    usingAreaLinksInContext.ForEach(link => dictLinks.Add(link, true));
                    contextDesign.CopyMoveChangesAsync(dictLinks);

                    modification.ModificationObject.StartUpdate();
                    //usingAreaObject.
                }
            }
        }

        private void FillUsingAreaLinksInContext(MatchConnection match, ConfigurationSettings settings, DesignContextObject context,
            ref List<ComplexHierarchyLink> listUsingArea, Guid linkGuid)
        {
            var links = match.Match.Links.ToOneToComplexHierarchy[linkGuid];
            using (links.LinkReference.ChangeAndHoldConfigurationSettings(settings))
            {
                links.Reload();
                var linkedComplexLink = links.LinkedComplexLink;
                if (linkedComplexLink != null && linkedComplexLink.SystemFields.DesignContextId == context.Id)
                {
                    listUsingArea.Add(linkedComplexLink);
                }
            }
        }
    }
}
