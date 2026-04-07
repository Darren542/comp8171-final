using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TranscriptionGateway.Api.Data;
using TranscriptionGateway.Api.Models;
using TranscriptionGateway.Api.Services;

namespace TranscriptionGateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TranscriptionsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IGpuTranscriptionClient _gpuClient;

    public TranscriptionsController(AppDbContext db, IGpuTranscriptionClient gpuClient)
    {
        _db = db;
        _gpuClient = gpuClient;
    }

    [HttpPost("submit")]
    [RequestSizeLimit(500_000_000)]
    public async Task<IActionResult> Submit(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is required.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var jobId = Guid.NewGuid().ToString("N");

        await using var stream = file.OpenReadStream();
        var uploadResponse = await _gpuClient.SubmitAudioAsync(stream, file.FileName, jobId, cancellationToken);

        var job = new TranscriptionJob
        {
            UserId = userId,
            JobId = jobId,
            OriginalFileName = file.FileName,
            Status = "Submitted",
            UploadResponseJson = uploadResponse,
            TranscriptUrl = $"https://s3-aged-water-5651.fly.dev/transcriptions/{jobId}",
            UpdatedUtc = DateTime.UtcNow
        };

        _db.TranscriptionJobs.Add(job);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            job.Id,
            job.JobId,
            job.Status,
            job.TranscriptUrl
        });
    }

    [HttpGet("mine")]
    public async Task<IActionResult> Mine(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var jobs = await _db.TranscriptionJobs
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedUtc)
            .ToListAsync(cancellationToken);

        return Ok(jobs);
    }

    [HttpGet("filenames")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFileNames(CancellationToken cancellationToken)
    {
        try
        {
            var fileNames = await _db.TranscriptionJobs
                .OrderBy(x => x.OriginalFileName)
                .Select(x => x.OriginalFileName)
                .ToListAsync(cancellationToken);

            return Ok(fileNames);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }
    }

    [HttpGet("files")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFiles(CancellationToken cancellationToken)
    {
        var files = await _db.TranscriptionJobs
            .OrderBy(x => x.OriginalFileName)
            .Select(x => new
            {
                x.JobId,
                x.OriginalFileName
            })
            .ToListAsync(cancellationToken);

        return Ok(files);
    }
}