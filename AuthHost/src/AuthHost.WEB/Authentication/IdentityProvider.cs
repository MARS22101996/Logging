using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using AuthHost.BLL.DTO;
using AuthHost.BLL.Infrastructure.Exceptions;
using AuthHost.BLL.Interfaces;
using AuthHost.WEB.Authentication.Interfaces;
using AuthHost.WEB.Controllers;
using AuthHost.WEB.Models.AccountApiModels;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace AuthHost.WEB.Authentication
{
    public class IdentityProvider : IIdentityProvider
    {
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;
        private readonly ILogger<IdentityProvider> _logger;

        public IdentityProvider(IAccountService accountService, IMapper mapper, ILogger<IdentityProvider> logger)
        {
            _accountService = accountService;
            _mapper = mapper;
            _logger = logger;
        }

        public List<Claim> GetIdentity(LoginApiModel loginApiModel)
        {
            var loginModelDto = _mapper.Map<LoginModelDto>(loginApiModel);
            UserDto userDto;

            try
            {
                userDto = _accountService.Login(loginModelDto);
            }
            catch (ServiceException)
            {
                return null;
            }

            var role = userDto.Roles.FirstOrDefault();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.Email, loginApiModel.Email),
                new Claim("userId", userDto.Id.ToString())
            };

            _logger.LogInformation($"Claims was successfuly added for user with email: {loginApiModel.Email}");

            return claims;
        }
    }
}