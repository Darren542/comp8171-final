using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TranscriptionGateway.Api.Models;

namespace TranscriptionGateway.Api.Controllers;

[ApiController]
[Route("api/gpu")]
public class GpuProxyController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GpuApiOptions _gpuApi;

    public GpuProxyController(
        IHttpClientFactory httpClientFactory,
        IOptions<GpuApiOptions> gpuApiOptions)
    {
        _httpClientFactory = httpClientFactory;
        _gpuApi = gpuApiOptions.Value;
    }

    [HttpGet("config")]
    [AllowAnonymous]
    public IActionResult GetConfig()
    {
        return Ok(new
        {
            _gpuApi.BaseUrl,
            _gpuApi.StatusWsUrl,
            _gpuApi.TranscriptBaseUrl
        });
    }

    [HttpGet("transcript/{jobId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTranscript(string jobId, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("gpu");
        var url = $"{_gpuApi.TranscriptBaseUrl}{jobId}";

        var response = await client.GetAsync(url, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return NotFound($"Transcript not found for jobId '{jobId}'.");

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        return Content(body, "text/plain");
    }

    [HttpPost("upload")]
    [AllowAnonymous]
    public async Task<IActionResult> Upload(IFormFile file, [FromForm] string jobId, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest("file is required");

        if (string.IsNullOrWhiteSpace(jobId))
            return BadRequest("jobId is required");

        var client = _httpClientFactory.CreateClient("gpu");
        using var form = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();

        var fileContent = new StreamContent(stream);
        form.Add(fileContent, "file", file.FileName);
        form.Add(new StringContent(jobId), "jobid");

        var response = await client.PostAsync($"{_gpuApi.BaseUrl.TrimEnd('/')}/upload", form, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        return StatusCode((int)response.StatusCode, body);
    }
}