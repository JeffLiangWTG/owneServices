using System;
using Microsoft.AspNetCore.Mvc;

namespace CargoWise.RefDbRepo.Staging.NewService.Controllers
{
	[ApiController]
	[Route("api/[controller]/[action]")]
	public class GuidGeneratorController : ControllerBase
	{
		[HttpGet]
		public Guid Get()
		{
			return Guid.NewGuid();
		}
	}
}
