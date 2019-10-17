using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//app implementation database task model

namespace TaskManager.DataModel
{
    class TTask
    {
        public int Id { get; set; }
        public string TaskName { get; set; }
        public string TaskContent { get; set; }
        public string TaskDate { get; set; }
        public Nullable<int> PriorityId { get; set; }
        public Nullable<int> StatusId { get; set; }

        public string TaskPriority { get; set; }
        public string TaskStatus { get; set; }
    }
}
