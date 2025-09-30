using System.ComponentModel.DataAnnotations;

namespace WebApi.MinimalApi.Models.Requests;

public class CreateUserResponse
{
    [Required]
    public string Login { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}