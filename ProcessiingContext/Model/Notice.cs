using ProcessiingContext.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFlex.DOCs.Model.References;

namespace ProcessiingContext
{
    /// <summary>
    /// Представление объекта ИИ
    /// </summary>
    public class Notice
    {

        private ReferenceObject notice;

        private List<Modification> modifications;
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

        public Notice(ReferenceObject notice)
        {
            this.notice = notice;
            var list = notice.GetObjects(Guids.NotifyReference.Link.Modifications);
            this.modifications = new List<Modification>();
            foreach (var item in list)
            {
                modifications.Add(new Modification(item));
            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in modifications)
            {
                stringBuilder.Append(item.ToString());
                stringBuilder.Append('\n');
            }
            return notice.ToString() + "\n" + stringBuilder.ToString();
        }
    }
}
