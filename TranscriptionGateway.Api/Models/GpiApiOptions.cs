namespace TranscriptionGateway.Api.Models;

public class GpuApiOptions
{
    public string BaseUrl { get; set; } = "";
    public string StatusWsUrl { get; set; } = "";
    public string TranscriptBaseUrl { get; set; } = "";
}