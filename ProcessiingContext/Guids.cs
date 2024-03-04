using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessiingContext
{
    public class Guids
    {
        /// <summary>
        /// Добавленное подключение связь на подключение объектов в ЭСИ
        /// </summary>
        public static readonly Guid UsingAreaAddedLink = new Guid("bac1b7b8-bd2e-4198-beb0-3580ee077534");
        /// <summary>
        /// Удаленное подключение связи на подключенные объектов в ЭСИ
        /// </summary>
        public static readonly Guid UsingAreaDeletedLink = new Guid("37210965-e2fc-4a44-b0ce-87dde109c458");
        /// <summary>
        /// Связь на объект в справочнике Контексты проектирования
        /// </summary>
        public static readonly Guid LinkContext = new Guid("42d45355-203a-4352-b6f1-303dc2d3fd87");
        /// <summary>
        /// Cвязь на исходное подключение объекта в основном контексте ЭСИ
        /// </summary>
        public static readonly Guid SourceHierarchyLink = new Guid("b0a49e16-0a7e-49a3-a897-b7fdbf3fccee");

        public static class NotifyReference
        {
            public static class Link
            {
                /// <summary>
                /// Cвязь на список изменений от извещения об изменении
                /// </summary>
                public static readonly Guid Modifications = new Guid("5e46670a-400c-4e36-bb37-d4d651bdf692");
                /// <summary>
                /// Связь на список объектов "Соответствия подключений"
                /// </summary>
                public static readonly Guid MatchesConnection = new Guid("40a11b88-666f-4273-a850-f205eb170d28");
                /// <summary>
                /// Связь на номенклатуру из объекта списка соответствия подключений
                /// </summary>
                public static readonly Guid ObjectPDM = new Guid("00f158e5-eab7-4006-b672-14b6b6f4c92f");
                public static readonly Guid SourceHierarchyLink = new Guid("b0a49e16-0a7e-49a3-a897-b7fdbf3fccee");
                public static readonly Guid AddHierarchyLink = new Guid("bac1b7b8-bd2e-4198-beb0-3580ee077534");
                public static readonly Guid RemoveHierarchyLink = new Guid("37210965-e2fc-4a44-b0ce-87dde109c458");
            }
        }

    }
}
