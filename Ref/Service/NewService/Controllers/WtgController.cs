using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CargoWise.RefDbRepo.NewService
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
