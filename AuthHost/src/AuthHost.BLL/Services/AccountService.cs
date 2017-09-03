using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthHost.BLL.DTO;
using AuthHost.BLL.Infrastructure.Exceptions;
using AuthHost.BLL.Interfaces;
using AuthHost.DAL.Entities;
using AuthHost.DAL.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace AuthHost.BLL.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRoleService _roleService;
        private readonly ICryptoProvider _cryptoProvider;
        private readonly IMapper _mapper;
        private const string AdminEmail = "admin@mail.com";
        private const string AdminPassword = "Pass1";
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            IUnitOfWork unitOfWork, 
            IRoleService roleService, 
            ICryptoProvider cryptoProvider, 
            IMapper mapper, ILogger<AccountService> logger)
        {
            _unitOfWork = unitOfWork;
            _roleService = roleService;
            _cryptoProvider = cryptoProvider;
            _mapper = mapper;
            _logger = logger;
        }

        public UserDto Login(LoginModelDto loginModel)
        {
            CreateAdminIfNotExists();

            var user = _unitOfWork.Users.Find(u => u.Email.Equals(loginModel.Email)).FirstOrDefault();

            if (user == null || !_cryptoProvider.VerifyHash(loginModel.Password, user.PasswordHash))
            {
                throw new ServiceException("Invalid password", "Password");
            }

            var userDto = _mapper.Map<UserDto>(user);

            _logger.LogInformation($"User login with email {loginModel.Email}");

            return userDto;
        }

        private async void  CreateAdminIfNotExists()
        {
            var user = _unitOfWork.Users.Find(u => u.Email.Equals(AdminEmail)).FirstOrDefault();

            if (user == null)
            {
                var userdmin = new User
                {
                    Email = AdminEmail,
                    Name = AdminEmail,
                    PasswordHash = _cryptoProvider.GetHash(AdminPassword),
                    Roles = new List<string>()
                };

                AppendRole(userdmin, "admin");

                await _unitOfWork.Users.CreateAsync(userdmin);

                _logger.LogInformation($"Admin user was register with email {AdminEmail}");
            }
        }
        public async Task RegisterAsync(RegisterModelDto registerModelDto)
        {
            ValidateEmail(registerModelDto.Email);

            var user = _mapper.Map<User>(registerModelDto);

            AppendRole(user, "user");

            user.PasswordHash = _cryptoProvider.GetHash(registerModelDto.Password);

            await _unitOfWork.Users.CreateAsync(user);

            _logger.LogInformation($"User register with email {registerModelDto.Email}");
        }

        private void ValidateEmail(string email)
        {
            if (_unitOfWork.Users.Find(user => user.Email.Equals(email)).Any())
            {
                throw new EntityExistsException($"User with such email is already exists. Email: {email}", "Email");
            }
        }

        private void AppendRole(User user, string role)
        {
            try
            {
                _roleService.Get(role);
            }
            catch (EntityNotFoundException)
            {
                _roleService.CreateAsync(role);
            }

            user.Roles = user.Roles.Append(role);
        }
    }
}