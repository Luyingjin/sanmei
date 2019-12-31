using System.Collections.Generic;
using System.Linq;
using Sanmei_AirConditioner.Roles.Dto;
using Sanmei_AirConditioner.Users.Dto;

namespace Sanmei_AirConditioner.Web.Models.Users
{
    public class EditUserModalViewModel
    {
        public UserDto User { get; set; }

        public IReadOnlyList<RoleDto> Roles { get; set; }

        public bool UserIsInRole(RoleDto role)
        {
            return User.Roles != null && User.Roles.Any(r => r == role.Name);
        }
    }
}