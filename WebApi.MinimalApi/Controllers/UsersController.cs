using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApi.MinimalApi.Domain;
using WebApi.MinimalApi.Models;
using WebApi.MinimalApi.Models.Requests;

namespace WebApi.MinimalApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : Controller
{
    // Чтобы ASP.NET положил что-то в userRepository требуется конфигурация
    private readonly IUserRepository userRepository;
    private readonly IMapper mapper;
    public UsersController(IUserRepository userRepository, IMapper mapper)
    {
        this.userRepository = userRepository;
        this.mapper = mapper;
    }

    [HttpGet("{userId}", Name = nameof(GetUserById))]
    [HttpHead("{userId}")]
    [Produces("application/json", "application/xml")]
    public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
    {
        var user = userRepository.FindById(userId);
        if (user == null)
            return NotFound();

        var response = mapper.Map<UserEntity, UserDto>(user);

        if (HttpMethods.IsHead(Request.Method))
        {
            Response.ContentType = "application/json; charset=utf-8";
            return StatusCode(200);
        }

        return Ok(response);
    }

    [HttpPost]
    [Produces("application/json", "application/xml")]
    public IActionResult CreateUser([FromBody] CreateUserRequest user)
    {
        if (user == null)
            return BadRequest();

        if (!(user.Login == null))
        {
            if (user.Login.Any(letter => !char.IsLetterOrDigit(letter)))
            {
                ModelState.AddModelError("login", "Login should contain only letters or digits");
            }
        }
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);
        
        var userEntity = mapper.Map<CreateUserRequest, UserEntity>(user);
        var result = userRepository.Insert(userEntity);
        return CreatedAtRoute(
            nameof(GetUserById),
            new { userId = result.Id },
             result.Id);
    }

    [HttpPut("{userId}")]
    [Produces("application/json", "application/xml")]
    public IActionResult UpsertUser(
        string userId, 
        [FromBody] UpsertUserRequest user)
    {
        if (user == null || !Guid.TryParse(userId, out Guid userGuid))
            return BadRequest();
        
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);

        var userEntity = new UserEntity(userGuid);
        mapper.Map(user, userEntity);

        bool result;
        userRepository.UpdateOrInsert(userEntity, out result);

        if (result)
            return CreatedAtRoute(
                nameof(GetUserById),
                new { userId = userId },
                userId);
        
        return NoContent();
    }
}