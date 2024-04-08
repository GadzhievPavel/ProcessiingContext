using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.References;
using TFlex.DOCs.Model.References.Nomenclature;
using TFlex.DOCs.Resources.Strings;

namespace ProcessiingContext.Handler
{
    public class NomenclatureHandler
    {
        private NomenclatureReference nomenclatureReference;
        private ServerConnection serverConnection;
        public NomenclatureHandler(ServerConnection connection) {
            this.nomenclatureReference = new NomenclatureReference(connection);
            this.serverConnection = connection;
        }

        /// <summary>
        /// Поиск подключение в ЭСИ на основе родительского и дочернего объекта в определенном контексте
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="children"></param>
        /// <param name="CurrentDesignContext"></param>
        /// <returns></returns>
        public ComplexHierarchyLink FindComplexHierarhyLink(ComplexHierarchyLink complexLink,  DesignContextObject CurrentDesignContext)
        {
            ReferenceObject parent = complexLink.ParentObject;
            ReferenceObject children = complexLink.ChildObject;

            var ConfigurationSettings = new ConfigurationSettings(serverConnection)
            {
                DesignContext = CurrentDesignContext,
                ApplyDesignContext = true,
                Date = Texts.TodayText,
                ApplyDate = true
            };

            ComplexHierarchyLink findedHierarchyLink = null;

            using (nomenclatureReference.ChangeAndHoldConfigurationSettings(ConfigurationSettings))
            {
                nomenclatureReference.Refresh();
                var childInOtherContext = nomenclatureReference.Find(children.Id);
                childInOtherContext.Parents.Reload();

                findedHierarchyLink = childInOtherContext.Parents.GetHierarchyLinks().Where(link=> link.ParentObjectId == parent.Id).FirstOrDefault();
            }
            return findedHierarchyLink;
        }
    }
}
