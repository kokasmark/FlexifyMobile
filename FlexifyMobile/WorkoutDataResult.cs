using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexifyMobile
{


    public class WorkoutDataResult
    {
        public Datum[] data { get; set; }
        public bool success { get; set; }
    }

    public class Datum
    {
        public int id { get; set; }
        public string name { get; set; }
        public string json { get; set; }
        public string time { get; set; }
        public int isFinished { get; set; }
    }


}
