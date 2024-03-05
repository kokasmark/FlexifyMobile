using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexifyMobile
{

    public class WorkoutTemplate
    {
        public Template[] templates { get; set; }
        public bool success { get; set; }
    }

    public class Template
    {
        public int id { get; set; }
        public string name { get; set; }
        public Json[] json { get; set; }
    }

    public class Json
    {
        public object exercise_id { get; set; }
        public Set_Data[] set_data { get; set; }
        public string name { get; set; }
    }

    public class Set_Data
    {
        public object reps { get; set; }
        public object weight { get; set; }
        public object time { get; set; }
    }

}
