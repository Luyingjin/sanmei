using System.Web.Mvc;

namespace Sanmei_AirConditioner.Web.Controllers
{
    public class AboutController : Sanmei_AirConditionerControllerBase
    {
        public ActionResult Index()
        {
            return View();
        }
	}
}