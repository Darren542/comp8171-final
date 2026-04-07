using System.Net.Http.Headers;

namespace TranscriptionGateway.Api.Services;

public class GpuTranscriptionClient : IGpuTranscriptionClient
{
    private readonly HttpClient _httpClient;

    public GpuTranscriptionClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> SubmitAudioAsync(Stream fileStream, string fileName, string jobId, CancellationToken cancellationToken = default)
    {
        using var form = new MultipartFormDataContent();

        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        form.Add(fileContent, "file", fileName);
        form.Add(new StringContent(jobId), "jobid");

        var response = await _httpClient.PostAsync("upload", form, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}