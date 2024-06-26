﻿using DeveloperUtilsLibrary;
using ProcessiingContext.exceptions;
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
        /// Перемещение изменения между контекстами
        /// </summary>
        /// <param name="targetDesignContext">целевой контекст проектирования</param>
        public void MoveToContext(DesignContextObject targetDesignContext)
        {
            var currentConfig = GetConfigModifications(notice, connection);
            if (currentConfig.DesignContext.Equals(targetDesignContext))
            {
                throw new ContextMovingException($"Изменения уже находятся в контексте {targetDesignContext}");
            }
            foreach (var item in modifications)
            {
                item.ModificationObject.StartUpdate();
                var pairConnections = item.MoveToContext(targetDesignContext);
                item.Update(targetDesignContext, pairConnections);
                item.ModificationObject.EndUpdate($"Завершено перемещение изменения в контекст {targetDesignContext}");
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
        /// <summary>
        /// Проводит поиск исходных подключений в целевом контексте
        /// </summary>
        /// <param name="targetContext">целевой контекст</param>
        /// <returns>словарь, true - подключение найдено, false - подключение в целевом контексте отсутствует</returns>
        private Dictionary<ComplexHierarchyLink,Boolean> GetSourceComplexHierarchyLink(DesignContextObject targetContext)
        {
            DesignContextsReference designContextsReference = new DesignContextsReference(connection);
            var designContextObject = designContextsReference.Find("Основной") as DesignContextObject;

            var mainConfig = new ConfigurationSettings(connection)
            {
                DesignContext = designContextObject,
                ApplyDesignContext = true,
                ShowDeletedInDesignContextLinks = true
            };

            var noticeInMain = new Notice(this.notice, this.connection, mainConfig);
            var sourceLinks = noticeInMain.GetAllSourceHierarchyLinks();

            var targetConfig = new ConfigurationSettings(connection)
            {
                DesignContext = targetContext,
                ApplyDesignContext = true,
                ShowDeletedInDesignContextLinks = true
            };

            var noticeInTagret = new Notice(this.notice, this.connection, targetConfig);

            Dictionary<ComplexHierarchyLink, Boolean> findedLinks = new Dictionary<ComplexHierarchyLink, bool>();
            foreach (var link in sourceLinks)
            {
                findedLinks.Add(link, false);
                foreach (var modificationInTarget in noticeInTagret.modifications)
                {
                    var findSourceComplexHierarchyLink = modificationInTarget.GetSourceComplexHierarchyLink(link);
                    if (findSourceComplexHierarchyLink != null)
                    {
                        findedLinks[link] = true;
                    }
                }
            }

            return findedLinks;
        }

        /// <summary>
        /// Разрешает перенос в целевой контекст, если исходные подключения в нем не изменены
        /// </summary>
        /// <param name="targetContext">целевой контекст</param>
        /// <returns></returns>
        public bool IsEnableMoveInContext(DesignContextObject targetContext)
        {
            var findedLinks = GetSourceComplexHierarchyLink(targetContext);
            return findedLinks.All(link => link.Value == true);
        }
        /// <summary>
        /// Возвращает текст о можножности переноса в целевой контекст изменений
        /// </summary>
        /// <param name="targetContext">целевой контест</param>
        /// <returns></returns>
        public string GetInfoIsEnableMovingInContext(DesignContextObject targetContext)
        {
            var findedLinks = GetSourceComplexHierarchyLink(targetContext);
            var noFindedLinks = findedLinks.Where(link => link.Value == false).ToList();
            StringBuilder stringBuilder = new StringBuilder();

            if (!noFindedLinks.Any()) {
                stringBuilder.AppendLine($"Извещение возможно перенести в контекст проектирования {targetContext}");
                return stringBuilder.ToString();
            }
            foreach (var link in noFindedLinks)
            {
                stringBuilder.AppendLine($"В контексте проектирования {targetContext} не было найдено или было изменено подключение между объектом" +
                    $" {link.Key.ParentObject} и входящей в него {link.Key.ChildObject}");
                stringBuilder.AppendLine();
            }
            return stringBuilder.ToString();
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
