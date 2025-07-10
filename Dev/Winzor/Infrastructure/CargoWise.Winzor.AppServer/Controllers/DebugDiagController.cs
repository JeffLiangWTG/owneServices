using Microsoft.AspNetCore.Mvc;

namespace CargoWise.Winzor.AppServer.Controllers
{
	public class DebugDiagController : Controller
	{
		[Route("DebugDiag")]
		public IActionResult Index() => Json(new { ProcessId = System.Environment.ProcessId.ToString() });
	}
}
