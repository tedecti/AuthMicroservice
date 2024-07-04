using System.ComponentModel.DataAnnotations;

namespace AuthMicroservice.Models.Users;

public class User
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; }
    [Required]
    [MinLength(8)]
    public string Password { get; set; }
    [Required]
    [AllowedValues("Seller", "Customer")]
    public string UserType { get; set; }
}