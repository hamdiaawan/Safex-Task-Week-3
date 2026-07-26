using System.Collections.Generic;
using System.Threading.Tasks;
using SafeXChat.Models;

namespace SafeXChat.Services
{
    public interface IJobService
    {
        Task<IEnumerable<Job>> GetApprovedJobsAsync();
        Task<IEnumerable<Job>> GetPendingJobsAsync();
        Task<Job?> GetJobByIdAsync(int id);
        Task<Job> CreateJobAsync(JobCreateViewModel model, string uploadsFolder);
        Task<bool> ApproveJobAsync(int id);
    }
}
