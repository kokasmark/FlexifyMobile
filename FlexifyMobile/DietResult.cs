using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexifyMobile
{

    public class DietResult
    {
        public Diet_Json json { get; set; }
        public bool success { get; set; }
    }

    public class Diet_Json
    {
        public Meal[] breakfast { get; set; }
        public Meal[] lunch { get; set; }
        public Meal[] dinner { get; set; }
        public Meal[] snacks { get; set; }
    }

    public class Meal
    {
        public string name { get; set; }
        public int calories { get; set; }
    }


}
