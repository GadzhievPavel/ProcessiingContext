using DeveloperUtilsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFlex.DOCs.Common;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.References;
using TFlex.DOCs.Model.References.Modifications;
using TFlex.DOCs.Model.References.Nomenclature;
using TFlex.DOCs.Resources.Strings;

namespace ProcessiingContext
{
    public class ProcessingNoticeInDesignContext
    {
        DesignContextObject CurrentDesignContext;
        ReferenceObject notice;
        List<ReferenceObject> modifications;
        ServerConnection connection;
        Notice myNotice;

        public ProcessingNoticeInDesignContext(ServerConnection connection, DesignContextObject currentDesignContext, ReferenceObject notice)
        {
            this.connection = connection;
            CurrentDesignContext = currentDesignContext;
            this.notice = notice;
            this.modifications = notice.GetObjects(ModificationReferenceObject.RelationKeys.ModificationNotice);
        }

        public ProcessingNoticeInDesignContext(ReferenceObject noticeReferenceObject)
        {
            //this.myNotice = new Notice(not);
        }
        public void MoveToContext(DesignContextObject targetDesignContext)
        {
            foreach (var modification in modifications)
            {
                var configSettings = new ConfigurationSettings(connection.ConfigurationSettings)
                {
                    DesignContext = targetDesignContext,
                };

                using (notice.Reference.ChangeAndHoldConfigurationSettings(configSettings))
                {
                    //MoveHierarchyLinks();
                }
            }
        }

        private void MoveHierarchyLinks(ReferenceObject modification, DesignContextObject context)
        {
            var currentDesignContext = (DesignContextObject)modification.GetObject(ModificationReferenceObject.RelationKeys.DesignContext);
            if (currentDesignContext is null)
            {
                return;
            }

            var newConfigurationSettings = new ConfigurationSettings(connection.ConfigurationSettings)
            {
                DesignContext = currentDesignContext,
                ApplyDesignContext = true,
                Date = Texts.TodayText,
                ApplyDate = true
            };

            var usingAreaLinksInContext = new List<ComplexHierarchyLink>();
            var usingAreaObjects = modification.GetObjects(ModificationReferenceObject.RelationKeys.UsingArea);
            foreach (var usingAreaObject in usingAreaObjects)
            {
                var hierarchyLinksMatches = usingAreaObject.GetObjects(ModificationUsingAreaReferenceObject.RelationKeys.HierarchyLinkMatches);
                foreach (var matches in hierarchyLinksMatches)
                {
                    FillUsingAreaLinksInContext(matches, newConfigurationSettings, currentDesignContext, ref usingAreaLinksInContext,
                        Guids.UsingAreaAddedLink);
                    FillUsingAreaLinksInContext(matches, newConfigurationSettings, currentDesignContext, ref usingAreaLinksInContext,
                        Guids.UsingAreaDeletedLink);
                }


                if (!usingAreaLinksInContext.IsNullOrEmpty())
                {
                    usingAreaLinksInContext = usingAreaLinksInContext.Distinct().ToList();
                    var dictLinks = new Dictionary<ComplexHierarchyLink, Boolean>();
                    usingAreaLinksInContext.ForEach(link => dictLinks.Add(link, true));
                    currentDesignContext.CopyMoveChangesAsync(dictLinks);
                    modification.StartUpdate();
                    usingAreaObject.StartUpdate();

                }
            }
        }

        private void FillUsingAreaLinksInContext(ReferenceObject matches, ConfigurationSettings settings, DesignContextObject currentContext,
            ref List<ComplexHierarchyLink> listUsingAreaLinks, Guid linkGuid)
        {
            var links = matches.Links.ToOneToComplexHierarchy[linkGuid];
            using (links.LinkReference.ChangeAndHoldConfigurationSettings(settings))
            {
                links.Reload();
                var linkedComplexLink = links.LinkedComplexLink;
                if (linkedComplexLink != null && linkedComplexLink.SystemFields.DesignContextId == currentContext.Id)
                {
                    listUsingAreaLinks.Add(linkedComplexLink);
                }
            }
        }

        private void UpdateLinksInMatches(List<ComplexHierarchyLink> usingAreaLinksInContext, DesignContextObject newDesignContext,
            DesignContextObject currentContext, ReferenceObject usingAreaObject)
        {
            foreach (var link in usingAreaLinksInContext)
            {
                //var matches =
            }
        }

        //private ReferenceObject FindMatchesByLinkInUsingAreaLinksInContext(ComplexHierarchyLink hierarchyLink, Guid linkGuid,
        //    ReferenceObject usingAreaObject, DesignContextObject currentDesignContext)
        //{
        //    var ConfigurationSettings = new ConfigurationSettings(connection)
        //    {
        //        DesignContext = currentDesignContext,
        //        ApplyDesignContext = true,
        //        Date = Texts.TodayText,
        //        ApplyDate = true
        //    };

        //    ModificationReference modificationReference = new ModificationReference(connection);
        //    ReferenceObject matches = null;
        //    using (modificationReference.ChangeAndHoldConfigurationSettings(ConfigurationSettings))
        //    {
        //        usingAreaObject.Reload();
        //        usingAreaObject.Refresh(usingAreaObject);
        //        //matches = usingAreaObject.GetObjects(ModificationUsingAreaReferenceObject.RelationKeys.HierarchyLinkMatches).Where(matche => matches.Links.ToOneToComplexHierarchy[])
        //    }
        //}
    }
}
