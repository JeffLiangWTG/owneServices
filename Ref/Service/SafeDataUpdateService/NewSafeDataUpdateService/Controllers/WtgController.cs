using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Controllers
{
	[ApiController]
	[Route("[controller]/[action]")]
	public class WtgController : ControllerBase
	{
		[HttpGet]
		[HttpHead]
		[AllowAnonymous]
		public IActionResult Ready()
		{
			return Ok();
		}
	}
}
