using System.Web.Mvc;
using Abp.Web.Mvc.Authorization;

namespace Sanmei_AirConditioner.Web.Controllers
{
    [AbpMvcAuthorize]
    public class HomeController : Sanmei_AirConditionerControllerBase
    {
        public ActionResult Index()
        {
            return View();
        }
	}
}