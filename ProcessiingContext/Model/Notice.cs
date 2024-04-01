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
        /// Конфигурация для просмотра
        /// </summary>
        private ConfigurationSettings configuration;
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

        public Notice(ReferenceObject notice, ServerConnection serverConnection, ConfigurationSettings configurationSettings) 
        {
            this.notice = notice;
            this.connection = serverConnection;
            this.modifications = new List<Modification>();
            this.configuration = configurationSettings;
            var modificationsReferenceObject = new List<ReferenceObject>();
            if(this.configuration != null)
            {
                using (notice.Reference.ChangeAndHoldConfigurationSettings(configurationSettings))
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
                    this.modifications.Add(new Modification(modification, connection, configurationSettings));
                }
            }
        }

        /// <summary>
        /// Перенос изменения и подключений в другой контекст
        /// </summary>
        /// <param name="designContext">контекст в который надо перенести</param>
        public void MoveHierarchyLinks(DesignContextObject designContext)
        {
            foreach (var item in modifications)
            {
                item.ModificationObject.StartUpdate();
                item.MoveHierarchyLinks(designContext);
                item.ModificationObject.EndUpdate($"Завершено перемещение изменения в контекст {designContext}");
            }
        }

        /// <summary>
        /// Возвращает конфигурацию для просмотра ИИ 
        /// </summary>
        /// <returns></returns>
        public ConfigurationSettings GetConfigModifications()
        {
            HashSet<DesignContextObject> designContextObjects = new HashSet<DesignContextObject>();
            foreach( var item in modifications)
            {
                designContextObjects.Add(item.DesignContextObject);
            }
            if (designContextObjects.Count == 1)
            {
                return new ConfigurationSettings(connection)
                {
                    DesignContext = designContextObjects.ElementAt(0),
                    ApplyDesignContext = true,
                    ShowDeletedInDesignContextLinks = true
                };
            }
            return null;
        }
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"[Выбранная конфигурация]: {configuration.DesignContext}\n");
            stringBuilder.Append($"ИИ: {notice}\n ");
            foreach (var item in modifications)
            {
                stringBuilder.Append(item.ToString());
                stringBuilder.Append('\n');
            }
            return stringBuilder.ToString();
        }
    }
}
