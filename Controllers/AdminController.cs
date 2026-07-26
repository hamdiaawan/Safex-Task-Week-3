using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SafeXChat.Services;

namespace SafeXChat.Controllers
{
    [Route("Admin")]
    public class AdminController : Controller
    {
        private readonly IJobService _jobService;

        public AdminController(IJobService jobService)
        {
            _jobService = jobService;
        }

        [HttpGet("PendingJobs")]
        public async Task<IActionResult> PendingJobs()
        {
            var pendingJobs = await _jobService.GetPendingJobsAsync();
            return View(pendingJobs);
        }

        [HttpPost("Approve")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _jobService.ApproveJobAsync(id);
            if (!result)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Job approved successfully!";
            return RedirectToAction(nameof(PendingJobs));
        }
    }
}
