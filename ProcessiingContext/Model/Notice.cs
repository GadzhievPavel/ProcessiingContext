using DeveloperUtilsLibrary;
using ProcessiingContext.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.References;
using TFlex.DOCs.Model.References.Modifications;
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
            if (this.configuration != null)
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
                foreach (var modification in modificationsReferenceObject)
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
        /// Возвращает список исходных подключений
        /// </summary>
        /// <returns></returns>
        public List<ComplexHierarchyLink> GetAllSourceHierarchyLinks()
        {
            List<ComplexHierarchyLink> sourceLinks = new List<ComplexHierarchyLink>();
            this.Modifications.ForEach(modification =>
            {
                modification.UsingAreas.ForEach(usingArea =>
                    usingArea.Matches.ForEach(match => sourceLinks.Add(match.SourceHierarhyLink)));
            });
            return sourceLinks;
        }
        ///// <summary>
        ///// Возвращает конфигурацию для просмотра ИИ 
        ///// </summary>
        ///// <returns></returns>
        //public ConfigurationSettings GetConfigModifications()
        //{
        //    HashSet<DesignContextObject> designContextObjects = new HashSet<DesignContextObject>();
        //    foreach( var item in modifications)
        //    {
        //        designContextObjects.Add(item.DesignContextObject);
        //    }
        //    if (designContextObjects.Count == 1)
        //    {
        //        return new ConfigurationSettings(connection)
        //        {
        //            DesignContext = designContextObjects.ElementAt(0),
        //            ApplyDesignContext = true,
        //            ShowDeletedInDesignContextLinks = true
        //        };
        //    }
        //    return null;
        //}

        /// <summary>
        /// Возвращает конфигурацию для работы с ии в контексте, в котором на текущий момент находятся изменения
        /// </summary>
        /// <param name="ii">Извещение об изменении</param>
        /// <param name="serverConnection">подключение сервера</param>
        /// <returns>конфигурация</returns>
        /// <exception cref="Exception">Ошибка, если отсутствует контекст в изменении по связи.
        /// Ошибка, не все изменения находятся в одном контексте проектирования.
        /// Ошибка, множество контекстов проектирования пусто
        /// </exception>
        public static ConfigurationSettings GetConfigModifications(ReferenceObject ii, ServerConnection serverConnection)
        {
            HashSet<DesignContextObject> designContextObjects = new HashSet<DesignContextObject>();
            var modifications = ii.GetObjects(Guids.NotifyReference.Link.Modifications);
            foreach (var item in modifications)
            {

                var designContext = item.GetObject(ModificationReferenceObject.RelationKeys.DesignContext) as DesignContextObject;
                if (designContext is null)
                {
                    throw new Exception($"В изменении {item} отсутствует контекст проектирования по связи {ModificationReferenceObject.RelationKeys.DesignContext}");
                }
                designContextObjects.Add(designContext);
            }

            if (designContextObjects.Count > 1)
            {
                throw new Exception("Не все изменения находятся в одном контексте проектирования");
            }
            else if (designContextObjects.Count == 0)
            {
                throw new Exception("Множество контекстов проектирования пусто");
            }
            else
            {
                var designContext = designContextObjects.First();
                var config = new ConfigurationSettings(serverConnection)
                {
                    DesignContext = designContext,
                    ApplyDesignContext = true,
                    ShowDeletedInDesignContextLinks = true
                };
                return config;
            }
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
