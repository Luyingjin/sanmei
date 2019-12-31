using System.Collections.Generic;
using Sanmei_AirConditioner.Roles.Dto;

namespace Sanmei_AirConditioner.Web.Models.Roles
{
    public class RoleListViewModel
    {
        public IReadOnlyList<RoleDto> Roles { get; set; }

        public IReadOnlyList<PermissionDto> Permissions { get; set; }
    }
}