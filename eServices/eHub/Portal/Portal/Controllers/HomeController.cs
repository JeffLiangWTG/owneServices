using System.Web.Mvc;

namespace CargoWise.eHub.Portal.Controllers
{
	public class HomeController : ControllerBase
    {
        public ActionResult Index()
        {
            ViewData["Message"] = "Welcome to the eHub Management Portal!";

            return View();
        }

		public ActionResult Setup()
		{
			return RedirectToAction("Index", "TransformationSet");
		}

        public ActionResult About()
        {
            return View();
        }
    }
}
