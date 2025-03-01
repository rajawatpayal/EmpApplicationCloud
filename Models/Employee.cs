using System.ComponentModel.DataAnnotations;

public class Employee
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string Position { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string ContactInfo { get; set; } = string.Empty;
}