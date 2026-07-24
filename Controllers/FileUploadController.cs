using Microsoft.AspNetCore.Mvc;

namespace SafeXChat.Controllers
{
    [ApiController]
    [Route("api/chat/upload")]
    public class FileUploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf", ".docx", ".zip", ".txt" };
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB, adjust if needed

        public FileUploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("no file received");

            if (file.Length > MaxFileSizeBytes)
                return BadRequest("file too large, max 10MB");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                return BadRequest("file type not allowed");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            // prefix with guid so two people uploading "resume.pdf" don't clash
            var storedFileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadsFolder, storedFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new
            {
                fileUrl = $"/uploads/{storedFileName}",
                fileName = file.FileName,
                fileSize = file.Length
            });
        }
    }
}
