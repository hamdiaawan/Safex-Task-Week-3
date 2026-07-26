using System;
using System.ComponentModel.DataAnnotations;

namespace SafeXChat.Models
{
    public class Job
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string RequiredSkills { get; set; } = string.Empty; // Store as comma-separated string

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Budget { get; set; }

        [Required]
        public string Duration { get; set; } = string.Empty;

        [Required]
        public string ExperienceLevel { get; set; } = string.Empty;

        [Required]
        public string WorkMode { get; set; } = string.Empty; // "Remote" or "Onsite"

        [Required]
        public DateTime Deadline { get; set; }

        public string? AttachmentPath { get; set; }

        [Required]
        public JobStatus Status { get; set; } = JobStatus.PendingAdminApproval;
    }
}
