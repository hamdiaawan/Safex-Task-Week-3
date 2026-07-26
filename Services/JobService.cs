using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SafeXChat.Data;
using SafeXChat.Models;

namespace SafeXChat.Services
{
    public class JobService : IJobService
    {
        private readonly ApplicationDbContext _context;

        public JobService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Job>> GetApprovedJobsAsync()
        {
            return await _context.Jobs
                .Where(j => j.Status == JobStatus.Approved)
                .OrderByDescending(j => j.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<Job>> GetPendingJobsAsync()
        {
            return await _context.Jobs
                .Where(j => j.Status == JobStatus.PendingAdminApproval)
                .OrderByDescending(j => j.Id)
                .ToListAsync();
        }

        public async Task<Job?> GetJobByIdAsync(int id)
        {
            return await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task<Job> CreateJobAsync(JobCreateViewModel model, string uploadsFolder)
        {
            string? attachmentPath = null;

            if (model.Attachment != null && model.Attachment.Length > 0)
            {
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Create a unique filename
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.Attachment.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Attachment.CopyToAsync(fileStream);
                }

                attachmentPath = "/uploads/jobs/" + uniqueFileName;
            }

            var job = new Job
            {
                JobTitle = model.JobTitle,
                Description = model.Description,
                RequiredSkills = string.Join(", ", model.SelectedSkills),
                Budget = model.Budget,
                Duration = model.Duration,
                ExperienceLevel = model.ExperienceLevel,
                WorkMode = model.WorkMode,
                Deadline = model.Deadline,
                AttachmentPath = attachmentPath,
                Status = JobStatus.PendingAdminApproval // default status
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            return job;
        }

        public async Task<bool> ApproveJobAsync(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
            {
                return false;
            }

            job.Status = JobStatus.Approved;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
