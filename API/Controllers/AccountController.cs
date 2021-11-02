using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IMapper mapper)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.UserName)) return BadRequest("Username is already taken");

            var user = _mapper.Map<AppUser>(registerDto);

            user.UserName = registerDto.UserName.ToLower();
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            var roleResult =  await _userManager.AddToRoleAsync(user,"Member");
            if(!roleResult.Succeeded) return BadRequest(result.Errors);
            
            return new UserDto
            {
                Username = user.UserName,
                KnownAs = user.KnownAs,
                Token =  await _tokenService.CreateToken(user),
                Gender = user.Gender
            };
        }

        private async Task<bool> UserExists(string userName)
        {
            return await _userManager.Users.AnyAsync(x => x.UserName.ToLower() == userName.ToLower());
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.Users.Include(p => p.Photos)
            .SingleOrDefaultAsync(x => x.UserName.ToLower() == loginDto.UserName.ToLower());
            if (user == null) return Unauthorized("Invalid Username");
            // using var hmac = new HMACSHA512(user.PasswordSalt);
            // var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(login.Password));
            // for (int i = 0; i < computedHash.Length; i++)
            // {
            //     if (computedHash[i] != user.PasswordHash[i])
            //     {
            //         return Unauthorized("Invalid Password");
            //     }
            // }

            var result = await _signInManager.CheckPasswordSignInAsync(user,loginDto.Password,false);
            if(!result.Succeeded) 
                return Unauthorized();
            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                PhotoUrl = user.Photos?.FirstOrDefault(x => x.IsMain == true)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }
    }
}