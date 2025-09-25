using DagOrchestrator.Models;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;

namespace DagOrchestrator.Services
{
    public enum JobStatus
    {
        Completed,
        Failed,
        Running,
        Pending
    }

    public class JobSubmissionService
    {
        private List<JobDefinition> RunningJobs = new();
        private List<JobDefinition> PendingJobs = new();

        private readonly PythonComService _pythonComService;
        public Dictionary<string, JobDefinition> DagResults = new();

        public JobSubmissionService(PythonComService pythonComService)
        {
            _pythonComService = pythonComService;
        }

        public bool IsJobForDagRunning(string job)
        {
            return RunningJobs.Select(x => x.JobID).Contains(job);
        }

        public void SubmitJob(JobDefinition job)
        {
            job.Status = JobStatus.Running;
            RunningJobs.Add(job);
        }

        internal void SetJobStatusFailed(string? jobID, string image_response)
        {
            foreach (var item in RunningJobs.Where(x => jobID == x.JobID))
            {
                item.Status = JobStatus.Failed;
                _pythonComService.ClearData(jobID);

                item.JobOutput = JObject.Parse( image_response );    
            }
            var job = RunningJobs.Where(x => jobID == x.JobID).FirstOrDefault();
            RunningJobs.Remove(job);

        }

        internal void SetJobStatusRunning(string jobID)
        {
            foreach (var item in RunningJobs.Where(x => jobID == x.JobID))
            {
                item.Status = JobStatus.Running;
                _pythonComService.ClearData(jobID);
            }

        }

        internal void SetJobStatusCompleted(string jobID)
        {
            foreach (var item in RunningJobs.Where(x => jobID == x.JobID))
            {
                item.Status = JobStatus.Completed;
            }
            var job = RunningJobs.Where(x => jobID == x.JobID).FirstOrDefault();
            RunningJobs.Remove(job);

        }

        internal void SetJobResult(string jobID, string jobResult)
        {
            foreach (var item in RunningJobs.Where(x => jobID == x.JobID))
            {
                if (jobResult != string.Empty)
                {
                    item.JobOutput = JObject.Parse(jobResult);
                }
                if(!DagResults.TryAdd(item.DagID, item ))
                {
                    DagResults[item.DagID] = item;
                }
                
            }
        }



        internal JobDefinition GetJobResult(string jobID)
        {
           return DagResults[jobID];
        }

        internal JobDefinition DequeueJob()
        {
            var job = RunningJobs.FirstOrDefault();
            return job;
        }

        internal void SetLazyNodes(DagNode node)
        {
            var job = RunningJobs.Where(x => node.JobID== x.JobID).FirstOrDefault();
            job.LazyNode = node;
            RunningJobs.Remove(job);
        }

        internal bool HasJobsInQueue(string dagid)
        {
            int num_jobs_with_dagId = RunningJobs.Where(x => x.DagID== dagid).Count();  
            return num_jobs_with_dagId > 0;
        }

        internal JobDefinition? HasPendingPipeline(string dagId, int detectionindex)
        {
            return PendingJobs.FirstOrDefault(x => x.DagID == dagId && x.DetectionIndex == detectionindex);
        }

        internal void RemovePendingJob(JobDefinition pending_job)
        {
            PendingJobs.Remove(pending_job);
        }

        internal void AddPendingJob(JobDefinition new_pending_job)
        {
            PendingJobs.Add(new_pending_job);
        }
    }



    public class JobDefinition
    {
        public string DagID { get; set; }
        public string JobID { get; set; }
        public int DetectionIndex { get; set; }
        public int ReceivedImages { get; set; } = 0;
        public int MaxImages { get; set; } = 0;
        public List<DagNode> Nodes { get; set; }
        public JObject JobOutput { get; set; }
        public JobStatus Status;
        public DagNode LazyNode { get; set; }

        public JobDefinition(string job,string dagid, int detectionindex, int nimageswithdetectionindex, List<DagNode> nodes)
        {
            JobID = job;
            DagID = dagid;
            DetectionIndex = detectionindex;
            MaxImages = nimageswithdetectionindex;  
            Status = JobStatus.Pending;
            Nodes = nodes;
        }
    }
}
