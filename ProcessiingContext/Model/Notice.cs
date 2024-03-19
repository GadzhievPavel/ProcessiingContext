﻿using ProcessiingContext.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.References;

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

        public Notice(ReferenceObject notice, ServerConnection serverConnection)
        {
            this.notice = notice;
            this.connection = serverConnection;
            var list = notice.GetObjects(Guids.NotifyReference.Link.Modifications);
            this.modifications = new List<Modification>();
            foreach (var item in list)
            {
                modifications.Add(new Modification(item, connection));
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
