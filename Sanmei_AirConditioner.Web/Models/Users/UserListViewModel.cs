using System.Collections.Generic;
using Sanmei_AirConditioner.Roles.Dto;
using Sanmei_AirConditioner.Users.Dto;

namespace Sanmei_AirConditioner.Web.Models.Users
{
    public class UserListViewModel
    {
        public IReadOnlyList<UserDto> Users { get; set; }

        public IReadOnlyList<RoleDto> Roles { get; set; }
    }
}