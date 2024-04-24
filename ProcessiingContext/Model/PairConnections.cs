using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFlex.DOCs.Model.References;

namespace ProcessiingContext.Model
{
    /// <summary>
    /// Набор подключений соответствия подключений
    /// </summary>
    public class PairConnections
    {
        /// <summary>
        /// Добавляемое подключение
        /// </summary>
        public ComplexHierarchyLink AddLink { get; set; }
        /// <summary>
        /// Удаляемое подключение
        /// </summary>
        public ComplexHierarchyLink RemoveLink { get; set; }
        /// <summary>
        /// Соответстивие
        /// </summary>
        public ReferenceObject Match {  get; set; }
    }
}
