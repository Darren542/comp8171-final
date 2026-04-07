namespace TranscriptionGateway.Api.Models;

public class TranscriptionJob
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;
    public ApplicationUser? User { get; set; }

    public string JobId { get; set; } = null!;
    public string OriginalFileName { get; set; } = null!;

    public string Status { get; set; } = "Created";
    public string? UploadResponseJson { get; set; }
    public string? TranscriptUrl { get; set; }
    public string? TranscriptText { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedUtc { get; set; }
    public DateTime? CompletedUtc { get; set; }
}