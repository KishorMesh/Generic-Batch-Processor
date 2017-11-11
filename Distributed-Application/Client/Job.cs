using System;
using API;

namespace Client
{
    internal class Job
    {
        public int ID { get; private set; }
        public string Description { get; private set; }
        public string MachineNode { get; set; }
        public JobStatus Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public long Duration { get; set; }

        internal Job(int taskId, string taskDescription)
        {
            ID = taskId;
            Description = taskDescription;
            Status = JobStatus.NotStarted;
        }
    }
}
