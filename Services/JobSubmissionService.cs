using DagOrchestrator.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Principal;

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

        public JobSubmissionService()
        {
        }

        public bool IsJobForDagRunning(string job)
        {
            return RunningJobs.Select(x => x.JobID).Contains(job);
        }

        public void SubmitJob(string job)
        {
            RunningJobs.Add(new JobDefinition(job));
        }

        internal void SetJobStatusFailed(string? jobID, string image_response)
        {
            foreach (var item in RunningJobs.Where(x => jobID == x.JobID))
            {
                item.Status = JobStatus.Failed;
                item.JobOutput = image_response;    
            }
            ;
        }

        internal void SetJobStatusRunning(string jobID)
        {
            foreach (var item in RunningJobs.Where(x => jobID == x.JobID))
            {
                item.Status = JobStatus.Running;
            }
        }

        internal void SetJobStatusCompleted(string jobID)
        {
            foreach (var item in RunningJobs.Where(x => jobID == x.JobID))
            {
                item.Status = JobStatus.Completed;
            }
        }

        internal void SetJobResult(string jobID, string jobResult)
        {
            foreach (var item in RunningJobs.Where(x => jobID == x.JobID))
            {
                item.JobOutput = jobResult;
            }
        }

        internal JobDefinition GetJobResult(string jobID)
        {
            var job = RunningJobs.Where(x => jobID == x.JobID).FirstOrDefault();
            return job;
        }
    }



    public class JobDefinition
    {
        public string JobID { get; set; }
        public string JobOutput { get; set; }
        public JobStatus Status;

        public JobDefinition(string job)
        {
            JobID = job;
            Status = JobStatus.Pending;
        }
    }
}
