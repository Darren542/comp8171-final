namespace TranscriptionGateway.Api.Services;

public interface IGpuTranscriptionClient
{
    Task<string> SubmitAudioAsync(Stream fileStream, string fileName, string jobId, CancellationToken cancellationToken = default);
}