﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Tangy_Common;
using Tangy_DataAccess;
using Tangy_Models;
using TangyWeb_API.Helper;

namespace TangyWeb_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly APISettings _apiSettings;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<APISettings> options)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _apiSettings = options.Value;
        }

        [HttpPost]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequestDTO signUpRequestDTO)
        {
            if (signUpRequestDTO == null || !ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = new ApplicationUser
            {
                UserName = signUpRequestDTO.Email,
                Email = signUpRequestDTO.Email,
                Name = signUpRequestDTO.Name,
                PhoneNumber = signUpRequestDTO.PhoneNumber,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, signUpRequestDTO.Password);

            if(!result.Succeeded)
            {
                return BadRequest(new SignUpResponseDTO()
                {
                    IsRegistrationSuccessful = false,
                    Errors = result.Errors.Select(u => u.Description)
                });
            }

            var roleResult = await _userManager.AddToRoleAsync(user, SD.Role_Customer);

            if (!roleResult.Succeeded)
            {
                return BadRequest(new SignUpResponseDTO()
                {
                    IsRegistrationSuccessful = false,
                    Errors = result.Errors.Select(u => u.Description)
                });
            }

            return StatusCode(201);
        }

        [HttpPost]
        public async Task<IActionResult> SignIn([FromBody] SignInRequestDTO signInRequestDTO)
        {
            if (signInRequestDTO == null || !ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await _signInManager.PasswordSignInAsync(signInRequestDTO.UserName, signInRequestDTO.Password, false, false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(signInRequestDTO.UserName);
                if(user == null)
                {
                    return Unauthorized(new SignInResponseDTO()
                    {
                        IsAuthSuccessful = false,
                        ErrorMessage = "Invalid Authentication"
                    });
                }

                // everything is valid and need to login
                var signingCredentials = GetSigningCredentials();
                var claims = await GetClaims(user);
                var tokenOptions = new JwtSecurityToken(
                    issuer: _apiSettings.ValidIssuer,
                    audience: _apiSettings.ValidAudience,
                    claims: claims,
                    expires: DateTime.Now.AddDays(30),
                    signingCredentials: signingCredentials );
                var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
                return Ok(new SignInResponseDTO()
                {
                    IsAuthSuccessful = true,
                    Token = token,
                    User = new UserDTO()
                    {
                        Name = user.UserName,
                        Id = user.Id,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber
                    }
                });
            }
            else
            {
                return Unauthorized(new SignInResponseDTO()
                {
                    IsAuthSuccessful = false,
                    ErrorMessage = "Invalid Authentication"
                });
            }

            return StatusCode(201);
        }

        private SigningCredentials GetSigningCredentials()
        {
            var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_apiSettings.SecretKey));

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        private async Task<List<Claim>> GetClaims(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("Id", user.Id),
            };

            var roles = await _userManager.GetRolesAsync(await _userManager.FindByEmailAsync(user.Email));
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }
    }
}
