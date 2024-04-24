using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.References;

namespace ProcessiingContext.Model
{
    /// <summary>
    /// Класс представляющий объект типа "Область применения"
    /// </summary>
    public class UsingArea
    {

        private ReferenceObject usingAreaObject;
        private List<MatchConnection> matches;
        private ServerConnection serverConnection;
        /// <summary>
        /// Список объектов типа "Соответствия подключений"
        /// </summary>
        public List<MatchConnection> Matches
        {
            get { return matches; }
            //set { matches = value; }
        }

        /// <summary>
        /// Возвращает ссылку на область объект области применения
        /// </summary>
        public ReferenceObject UsingAreaObject
        {
            get { return usingAreaObject; }
            //set { this.usingAreaObject = value; }
        }

        public UsingArea(List<MatchConnection> matches, ReferenceObject usingAreaObject)
        {
            this.matches = matches;
            this.usingAreaObject = usingAreaObject;
        }

        public UsingArea(ReferenceObject usingAreaObject, ConfigurationSettings configurationSettings, ServerConnection serverConnection)
        {
            this.usingAreaObject = usingAreaObject;
            this.serverConnection = serverConnection;
            this.matches = new List<MatchConnection>();
            using(usingAreaObject.Reference.ChangeAndHoldConfigurationSettings(configurationSettings))
            {
                usingAreaObject.Reference.Refresh();
                usingAreaObject.Reload();
                var tempMatches = usingAreaObject.GetObjects(Guids.NotifyReference.Link.MatchesConnection);
                tempMatches.ForEach(c => this.matches.Add(new MatchConnection(c, configurationSettings, serverConnection)));
            }
        }
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Область применения:\n");
            matches.ForEach(x => stringBuilder.Append(x.ToString()).AppendLine());
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Поиск исходного подключения по имеющемуся подключению
        /// </summary>
        /// <param name="sourceLink">имеющеся подключение</param>
        /// <returns></returns>
        public ComplexHierarchyLink GetSourceComplexHierarchyLink(ComplexHierarchyLink sourceLink)
        {
            foreach(MatchConnection match in matches) {
                if (match.SourceHierarhyLink.Equals(sourceLink))
                {
                    return match.SourceHierarhyLink;
                }
            }
            return null;
        }
    }
}
