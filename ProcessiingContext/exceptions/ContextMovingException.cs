using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessiingContext.exceptions
{
    public class ContextMovingException:Exception
    {
        public ContextMovingException(string message): base(message){ 
            
        }
    }
}
