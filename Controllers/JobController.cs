using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SafeXChat.Models;
using SafeXChat.Services;

namespace SafeXChat.Controllers
{
    [Route("Job")]
    public class JobController : Controller
    {
        private readonly IJobService _jobService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public JobController(IJobService jobService, IWebHostEnvironment webHostEnvironment)
        {
            _jobService = jobService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var approvedJobs = await _jobService.GetApprovedJobsAsync();
            return View(approvedJobs);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var job = await _jobService.GetJobByIdAsync(id);
            if (job == null)
            {
                return NotFound();
            }
            return View(job);
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View(new JobCreateViewModel());
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "jobs");
            await _jobService.CreateJobAsync(model, uploadsFolder);

            TempData["SuccessMessage"] = "Job submitted successfully! It is pending administrator approval.";
            return RedirectToAction(nameof(Index));
        }
    }
}
