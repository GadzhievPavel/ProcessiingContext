using DeveloperUtilsLibrary;
using ProcessiingContext.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.References;
using TFlex.DOCs.Model.References.Nomenclature;
using TFlex.DOCs.Resources.Strings;

namespace ProcessiingContext
{
    /// <summary>
    /// Представление объекта ИИ
    /// </summary>
    public class Notice
    {
        /// <summary>
        /// Извещение в DOCs
        /// </summary>
        private ReferenceObject notice;
        /// <summary>
        /// Список изменений
        /// </summary>
        private List<Modification> modifications;
        /// <summary>
        /// подключение к DOCs
        /// </summary>
        private ServerConnection connection;
        /// <summary>
        /// текущий контекст ИИ
        /// </summary>
        private DesignContextObject currentDesignContext;
        /// <summary>
        /// Ссылка на объект ИИ в системе
        /// </summary>
        public ReferenceObject NoticeObject
        {
            get { return notice; }
            //set { notice = value; }
        }
        /// <summary>
        /// Список объектов Изменений в ИИ
        /// </summary>
        public List<Modification> Modifications
        {
            get { return modifications; }
            //set { modifications = value; }
        }

        public Notice(ReferenceObject notice, ServerConnection serverConnection, DesignContextObject designContext)
        {
            ConfigurationSettings configSettings = null;
            if (designContext != null)
            {
                configSettings = new ConfigurationSettings(serverConnection)
                {
                    DesignContext = designContext,
                    ApplyDesignContext = true,
                    Date = Texts.TodayText,
                    ApplyDate = true
                };
            }

            this.notice = notice;
            this.connection = serverConnection;
            this.currentDesignContext = designContext;
            this.modifications = new List<Modification>();

            var modificationsReferenceObject = new List<ReferenceObject>();
            if(configSettings != null)
            {
                using (notice.Reference.ChangeAndHoldConfigurationSettings(configSettings))
                {
                    notice.Reference.Refresh();
                    notice.Reload();
                    modificationsReferenceObject = notice.GetObjects(Guids.NotifyReference.Link.Modifications);
                }
            }

            if (modificationsReferenceObject.Any())
            {
                foreach(var modification in modificationsReferenceObject)
                {
                    this.modifications.Add(new Modification(modification, connection, currentDesignContext));
                }
            }
            else
            {
                foreach(var modification in notice.GetObjects(Guids.NotifyReference.Link.Modifications))
                {
                    this.modifications.Add(new Modification(modification, connection, currentDesignContext));
                }
            }
        }

        public void MoveHierarchyLinks(DesignContextObject designContext)
        {
            foreach (var item in modifications)
            {
                item.ModificationObject.StartUpdate();
                item.MoveHierarchyLinks(designContext);
                item.ModificationObject.EndUpdate($"Завершено перемещение изменения в контекст {designContext}");
            }
        }
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"ИИ {notice}:\n");
            foreach (var item in modifications)
            {
                stringBuilder.Append(item.ToString());
                stringBuilder.Append('\n');
            }
            return stringBuilder.ToString();
        }
    }
}
