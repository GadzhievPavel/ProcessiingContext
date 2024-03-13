using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFlex.DOCs.Model.References;

namespace ProcessiingContext.Model
{
    public class UsingArea
    {
        private List<MatchConnection> matches;
        public List<MatchConnection> Matches
        {
            get { return matches; }
            set { matches = value; }
        }

        public UsingArea(List<MatchConnection> matches)
        {
            this.matches = matches;
        }

        public UsingArea(List<ReferenceObject> matches)
        {
            this.matches = new List<MatchConnection>();
            matches.ForEach(c => this.matches.Add(new MatchConnection(c)));

        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Область применения:\n");
            matches.ForEach(x => stringBuilder.Append(x.ToString()).AppendLine());
            return stringBuilder.ToString();
        }
    }
}
