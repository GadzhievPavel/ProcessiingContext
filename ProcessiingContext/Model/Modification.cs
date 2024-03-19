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
        /// контекст проектирования
        /// </summary>
        private DesignContextObject designContext;
        /// <summary>
        /// Список областей применения
        /// </summary>
        private List<UsingArea> usingAreas;

        private ConfigurationSettings currentConfiguration;
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
        public Modification(ReferenceObject modification, ServerConnection connection, DesignContextObject designContext = null)
        {
            this.modification = modification;

            if(designContext != null)
            {
                this.designContext = modification.GetObject(ModificationReferenceObject.RelationKeys.DesignContext) as DesignContextObject;
            }

            this.currentConfiguration = new ConfigurationSettings(connection)
            {
                DesignContext = this.designContext,
                ApplyDesignContext = true,
                Date = Texts.TodayText,
                ApplyDate = true
            };

            this.usingAreaLinksInContext = new List<ComplexHierarchyLink>();
            this.usingAreas = new List<UsingArea>();
            fillUsingArea(currentConfiguration);
        }

        private void fillUsingArea(ConfigurationSettings configurationSettings)
        {
            using (modification.Reference.ChangeAndHoldConfigurationSettings(currentConfiguration))
            {
                var usingAreaObjects = modification.GetObjects(ModificationReferenceObject.RelationKeys.UsingArea);
                foreach (var itemUsingArea in usingAreaObjects)
                {
                    this.usingAreas.Add(new UsingArea(itemUsingArea.GetObjects(Guids.NotifyReference.Link.MatchesConnection),
                        itemUsingArea, configurationSettings));
                }
            }
        }

        public void MoveHierarchyLinks(DesignContextObject designContext)
        {
            foreach(var usingArea in  usingAreas)
            {
                foreach(var match in usingArea.Matches)
                {
                    match.CopyComplexHierarhyLInkInContext(designContext);

                }
            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Изменение {modification} в контексте {designContext}:\n");
            foreach (var item in usingAreas)
            {
                stringBuilder.Append(item.ToString());
                stringBuilder.Append("\n");
            }
            return stringBuilder.ToString();
        }
    }
}
