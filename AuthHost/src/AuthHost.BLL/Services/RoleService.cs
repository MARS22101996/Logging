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
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<RoleService> _logger;

        public RoleService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<RoleService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }


        public RoleDto Get(string name)
        {
            var role = _unitOfWork.Roles
                .Find(r => r.Name.Equals(name))
                .FirstOrDefault();

            if (role == null)
            {
                throw new EntityNotFoundException(
                    $"Role with such name does not exist. Name: {name}",
                    "Role");
            }

            var roleDto = _mapper.Map<RoleDto>(role);

            _logger.LogInformation($"Get role by name: {name}");

            return roleDto;
        }

        public async Task CreateAsync(string name)
        {
            var role = _unitOfWork.Roles
                .Find(r => r.Name.Equals(name))
                .FirstOrDefault();

            if (role != null)
            {
                throw new EntityExistsException(
                    $"Role with such name already exists. Name: {name}",
                    "Role");
            }

            var roleToCreate = new Role { Name = name };

            await _unitOfWork.Roles.CreateAsync(roleToCreate);

            _logger.LogInformation($"New role {name} created");
        }
    }
}