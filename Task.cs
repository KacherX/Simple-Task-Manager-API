using System.ComponentModel.DataAnnotations;

namespace TaskHandler;

public class Task
{
    public Guid Id { get; set; } // Unique identifier
    [Required]
    public string Title { get; set; } = string.Empty; // Required
    public bool IsCompleted { get; set; }
    public string? Description { get; set; } // Optional
}