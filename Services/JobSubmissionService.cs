using DagOrchestrator.Models;

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

        private readonly PythonComService _pythonComService;
        public Dictionary<string, JobDefinition> DagResults = new();
        private IDatabase _db;

        public JobSubmissionService(PythonComService pythonComService, IConnectionMultiplexer cache)
        {
            _pythonComService = pythonComService;
            _db = cache.GetDatabase();
        }

        public bool IsJobForDagRunning(string job)
        {
            return RunningJobs.Select(x => x.JobID).Contains(job);
        }

        public void SubmitJob(JobDefinition job)
        {
            RunningJobs.Add(job);
        }

        internal void SetJobStatusFailed(string? jobID, string image_response)
        {
            foreach (var item in RunningJobs.Where(x => jobID == x.JobID))
            {
                item.Status = JobStatus.Failed;
                _pythonComService.ClearData(jobID);
                item.JobOutput = image_response;    
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
                item.JobOutput = jobResult;
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

        internal bool HasJobsInQueue()
        {
            return RunningJobs.Count() > 0;
        }
    }



    public class JobDefinition
    {
        public string DagID { get; set; }
        public string JobID { get; set; }
        public List<DagNode> Nodes { get; set; }
        public string JobOutput { get; set; }
        public JobStatus Status;
        public DagNode LazyNode { get; set; }

        public JobDefinition(string job,string dagid, List<DagNode> nodes)
        {
            JobID = job;
            DagID = dagid;
            Status = JobStatus.Pending;
            Nodes = nodes;
        }
    }
}
