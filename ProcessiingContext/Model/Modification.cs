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

        private ReferenceObject modification;

        private DesignContextObject designContext;

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

        public Modification(ReferenceObject modification, ServerConnection connection)
        {
            this.modification = modification;
            this.designContext = modification.GetObject(ModificationReferenceObject.RelationKeys.DesignContext) as DesignContextObject;
            this.usingAreas = new List<UsingArea>();
            var list = modification.GetObjects(ModificationReferenceObject.RelationKeys.UsingArea);
            foreach (var item in list)
            {
                this.usingAreas.Add(new UsingArea(item.GetObjects(Guids.NotifyReference.Link.MatchesConnection)));
            }
            this.currentConfiguration = new ConfigurationSettings(connection)
            {
                DesignContext = this.designContext,
                ApplyDesignContext = true,
                Date = Texts.TodayText,
                ApplyDate = true
            };
        }

        public void MoveHierarchyLinks()
        {
            foreach(var usingArea in  usingAreas)
            {

            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in usingAreas)
            {
                stringBuilder.Append(item.ToString());
                stringBuilder.Append("\n");
            }
            return modification.ToString() + "\n" + designContext.ToString() + "\n" + stringBuilder.ToString();
        }
    }
}
