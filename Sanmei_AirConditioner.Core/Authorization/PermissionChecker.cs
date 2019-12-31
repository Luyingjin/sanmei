using Abp.Authorization;
using Sanmei_AirConditioner.Authorization.Roles;
using Sanmei_AirConditioner.Authorization.Users;

namespace Sanmei_AirConditioner.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {

        }
    }
}
