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
        private NomenclatureHandler nomenclatureHandler;
        public ProcessingNotice(ReferenceObject _notice, ServerConnection connection, ConfigurationSettings configurationSettings)
        {
            this.notice = new Notice(_notice, connection, configurationSettings);
            this.connection = connection;
            this.nomenclatureHandler = new NomenclatureHandler(connection);
        }

        /// <summary>
        /// Проверяет возможность переноса текущего ИИ в заданный контекст
        /// </summary>
        /// <param name="targetContext">Целевой контекст</param>
        /// <returns>true если можно перенести</returns>
        public bool IsEnableMoveInContext(DesignContextObject targetContext)
        {
            var sourceLinks = notice.GetAllSourceHierarchyLinks();
            foreach (var sourceLink in sourceLinks)
            {
                var findSourceLink = nomenclatureHandler.FindComplexHierarhyLink(sourceLink, targetContext);
                if (findSourceLink is null)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Возвращает текстовое сообщение с информацией о возможности переноса в указанный контекст
        /// </summary>
        /// <param name="targetContext">целевой контекст</param>
        /// <returns>отчет</returns>
        public String GetReportMoveInContext(DesignContextObject targetContext)
        {
            StringBuilder stringBuilder = new StringBuilder();
            var sourceLinks = notice.GetAllSourceHierarchyLinks();
            foreach (var sourceLink in sourceLinks)
            {
                var findSourceLink = nomenclatureHandler.FindComplexHierarhyLink(sourceLink, targetContext);
                if(findSourceLink is null)
                {
                    stringBuilder.AppendLine($"Не было найдено подключение c id: {sourceLink.Id} в контексте {targetContext} между {sourceLink.ParentObject} и {sourceLink.ChildObject}\n");
                }
            }
            if (stringBuilder.Length == 0)
            {
                stringBuilder.AppendLine($"ИИ можно перенести в {targetContext}");
            }
            return stringBuilder.ToString();
        }

        public void MovingToDesignContext(DesignContextObject targetDesignContext)
        {
            List<PairConnections> connections = new List<PairConnections>();

            foreach (var modification in notice.Modifications)
            {
                modification.ModificationObject.StartUpdate();
                var pairConnections = modification.MoveToContext(targetDesignContext);
                modification.Update(targetDesignContext, pairConnections);
                modification.ModificationObject.StartUpdate();
            }

            
        }
    }
}
