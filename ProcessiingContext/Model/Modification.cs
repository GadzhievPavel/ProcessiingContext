using DeveloperUtilsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.References;
using TFlex.DOCs.Model.References.Modifications;
using TFlex.DOCs.Model.References.Nomenclature;
using TFlex.DOCs.Resources.Strings;

namespace ProcessiingContext.Model
{
    /// <summary>
    /// Представление объекта типа "Изменения"
    /// </summary>
    public class Modification
    {
        /// <summary>
        /// Изменение в DOCs
        /// </summary>
        private ReferenceObject modification;
        /// <summary>
        /// контекст проектирования в изменени
        /// </summary>
        private DesignContextObject designContext;
        /// <summary>
        /// Список областей применения
        /// </summary>
        private List<UsingArea> usingAreas;

        private ConfigurationSettings currentConfiguration;

        private ServerConnection serverConnection;
        /// <summary>
        /// Ссылка на объект Изменение в системе
        /// </summary>
        public ReferenceObject ModificationObject
        {
            get { return modification; }
            //set { modification = value; }
        }
        /// <summary>
        /// Контекст проектирования в котором находится изменение
        /// </summary>
        public DesignContextObject DesignContextObject
        {
            get { return designContext; }
            //set { designContext = value; }
        }
        /// <summary>
        /// Список объектов области применения
        /// </summary>
        public List<UsingArea> UsingAreas
        {
            get { return usingAreas; }
            //set { usingAreas = value; }
        }

        private List<ComplexHierarchyLink> usingAreaLinksInContext;
        /// <summary>
        /// Создает представление изменения в DOCs в контексте, указанном в изменении modification
        /// </summary>
        /// <param name="modification">изменение в DOCs</param>
        /// <param name="connection">подключение</param>
        public Modification(ReferenceObject modification, ServerConnection connection, ConfigurationSettings configurationSettings)
        {
            this.serverConnection = connection;
            this.modification = modification;
            this.usingAreaLinksInContext = new List<ComplexHierarchyLink>();
            this.usingAreas = new List<UsingArea>();
            this.currentConfiguration = configurationSettings;
            this.designContext = modification.GetObject(ModificationReferenceObject.RelationKeys.DesignContext) as DesignContextObject;
            fillUsingArea();
        }

        /// <summary>
        /// Заполнение области применения
        /// </summary>
        private void fillUsingArea()
        {
            using (modification.Reference.ChangeAndHoldConfigurationSettings(currentConfiguration))
            {
                this.modification.Reference.Refresh();
                this.modification.Reload();

                var usingAreaObjects = modification.GetObjects(ModificationReferenceObject.RelationKeys.UsingArea);
                foreach (var itemUsingArea in usingAreaObjects)
                {
                    this.usingAreas.Add(new UsingArea(itemUsingArea, currentConfiguration, this.serverConnection));
                }
            }
        }
        /// <summary>
        /// Перенос измененения и подключения в заданный контекст
        /// </summary>
        /// <param name="designContext">контекст, в который надо перенести</param>
        public void MoveHierarchyLinks(DesignContextObject targetDesignContext)
        {
            var config = new ConfigurationSettings(this.serverConnection)
            {
                DesignContext = targetDesignContext,
                ApplyDesignContext = true,
                ShowDeletedInDesignContextLinks = true
            };
            List<PairConnections> connections = new List<PairConnections>();
            foreach (var usingArea in usingAreas)
            {
                usingArea.UsingAreaObject.StartUpdate();
                foreach (var match in usingArea.Matches)
                {
                    var newConnections = match.CopyComplexHierarhyLInkInContext(targetDesignContext);
                    connections.Add(newConnections);
                    //newMatch.UpdateReferenceObject();
                    match.DeleteComplexHierarhyLinkInContext(this.DesignContextObject);
                    //match.SetLinkConnections(newConnections, config);
                }
                usingArea.UsingAreaObject.EndUpdate("обновление подключений");
            }

            using (modification.Reference.ChangeAndHoldConfigurationSettings(config))
            {
                modification.Reference.Refresh();
                modification.Reload();
                foreach(var connection in connections)
                {
                    var findedMatch = FindMatch(connection.Match, true);
                    UpdateMatch(findedMatch,connection);
                }
                Save();
            }
            setDesignContext(targetDesignContext);
        }

        /// <summary>
        /// Поиск соответствия подключений
        /// </summary>
        /// <param name="match">искомое подключение</param>
        /// <param name="editIt">брать на редактирование найденный объект</param>
        /// <returns></returns>
        private ReferenceObject FindMatch(ReferenceObject match, bool editIt)
        {
            var usingAreas = modification.GetObjects(ModificationReferenceObject.RelationKeys.UsingArea);
            foreach (var usingArea in usingAreas)
            {
                var matches = usingArea.GetObjects(Guids.NotifyReference.Link.MatchesConnection);
                foreach (var selectMatch in matches)
                {
                    if (selectMatch.Equals(match))
                    {
                        if (editIt)
                        {
                            this.modification.StartUpdate();
                            usingArea.StartUpdate();
                            selectMatch.StartUpdate();
                        }
                        return selectMatch;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// сохранение изменения и связанных объектов
        /// </summary>
        private void Save()
        {
            var usingAreas = modification.GetObjects(ModificationReferenceObject.RelationKeys.UsingArea);
            foreach (var usingArea in usingAreas)
            {
                var matches = usingArea.GetObjects(Guids.NotifyReference.Link.MatchesConnection);
                foreach (var selectMatch in matches)
                {
                    selectMatch.EndUpdate("сохраняем соотетствие");
                }
                usingArea.EndUpdate("сохраняем область применения");
            }
            this.modification.EndUpdate("сохраняем изменение");
        }

        /// <summary>
        /// обновление подключений в соответствии
        /// </summary>
        /// <param name="match">соответствие</param>
        /// <param name="pairConnections">набор подключений</param>
        private void UpdateMatch(ReferenceObject match, PairConnections pairConnections)
        {
            match.Links.ToOneToComplexHierarchy[Guids.NotifyReference.Link.RemoveHierarchyLink].SetLinkedComplexLink(pairConnections.RemoveLink);
            match.Links.ToOneToComplexHierarchy[Guids.NotifyReference.Link.AddHierarchyLink].SetLinkedComplexLink(pairConnections.AddLink);
        }
        /// <summary>
        /// задать контекст проектирования
        /// </summary>
        /// <param name="designContext"></param>
        private void setDesignContext(DesignContextObject designContext)
        {
            this.modification.StartUpdate();
            this.modification.SetLinkedObject(ModificationReferenceObject.RelationKeys.DesignContext, designContext);
            this.modification.EndUpdate("изменение контекста проектирования");
            this.designContext = designContext;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"[Изменение]: {modification}");
            stringBuilder.AppendLine($"[Конфигурация]: {this.currentConfiguration.DesignContext}");
            //stringBuilder.AppendLine($"[Конфигурация просмотра DesignContext]: {currentConfiguration.DesignContext}");
            foreach (var item in usingAreas)
            {
                stringBuilder.Append(item.ToString());
                stringBuilder.Append("\n");
            }
            return stringBuilder.ToString();
        }
    }
}
