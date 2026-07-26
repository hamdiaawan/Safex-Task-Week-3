using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SafeXChat.Models
{
    public class JobCreateViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Job Title is required.")]
        [StringLength(100, ErrorMessage = "Job Title cannot exceed 100 characters.")]
        public string JobTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "At least one skill must be selected.")]
        public List<string> SelectedSkills { get; set; } = new();

        [Required(ErrorMessage = "Budget is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Budget must be greater than 0.")]
        public decimal Budget { get; set; }

        [Required(ErrorMessage = "Duration is required.")]
        public string Duration { get; set; } = string.Empty;

        [Required(ErrorMessage = "Experience Level is required.")]
        public string ExperienceLevel { get; set; } = string.Empty;

        [Required(ErrorMessage = "Work Mode is required.")]
        public string WorkMode { get; set; } = string.Empty; // "Remote" or "Onsite"

        [Required(ErrorMessage = "Deadline is required.")]
        [DataType(DataType.Date)]
        public DateTime Deadline { get; set; } = DateTime.Today.AddDays(7);

        public IFormFile? Attachment { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Deadline <= DateTime.Today)
            {
                yield return new ValidationResult("Deadline must be in the future.", new[] { nameof(Deadline) });
            }

            if (SelectedSkills == null || SelectedSkills.Count == 0)
            {
                yield return new ValidationResult("At least one required skill must be selected.", new[] { nameof(SelectedSkills) });
            }
        }
    }
}
