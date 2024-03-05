using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexifyMobile
{

    public class SetData
    {
        public Set[] Property1 { get; set; }
    }

    public class Set
    {
        public string time { get; set; }
        public int weight { get; set; }
        public int reps { get; set; }
    }

}
