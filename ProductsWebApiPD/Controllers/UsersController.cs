using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProductsWebApiPD.DataTransfer;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ProductsWebApiPD.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private UserManager<IdentityUser<int>> users;
        private readonly SignInManager<IdentityUser<int>> signIn;

        public UsersController(UserManager<IdentityUser<int>> users, SignInManager<IdentityUser<int>> signIn)
        {
            this.users = users;
            this.signIn = signIn;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDto)
        {
            var user = await users.FindByNameAsync(loginDto.Username);
            if (user is null)
            {
                return NotFound("Неверное имя или пароль");
            }
            if (!await users.CheckPasswordAsync(user, loginDto.Password))
            {
                return NotFound("Неверное имя пользователя или пароль");
            }
            var principal=await signIn.CreateUserPrincipalAsync(user);
            return Ok(getToken(principal));
        }


        [HttpPost("registration")]
        public async Task<IActionResult> Registration(RegistrationDTO dto)
        {
            var user = new IdentityUser<int>
            {
                UserName=dto.Username,
                Email=dto.Email
            };
            var result=await users.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            result = await users.AddToRoleAsync(user, "client");
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return Ok("Success");
        }



        private string getToken(ClaimsPrincipal principal)
        {
            List<Claim> claims = principal.Claims.ToList();
            SigningCredentials credentials=new SigningCredentials(
                new SymmetricSecurityKey(KeyProvider.Key),
                SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: "ects",
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddHours(12),
                claims: claims,
                signingCredentials: credentials);
            var handler=new JwtSecurityTokenHandler();
            string result=handler.WriteToken(token);
            return result;
        }
    }
}
