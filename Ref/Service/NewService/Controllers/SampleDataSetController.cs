using CargoWise.RefDbRepo.Common;
using CargoWise.RefDbRepo.Common.Argument;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.NewService.Controllers
{
	[ApiController]
	[Route("[controller]/[action]")]
	public class SampleDataSetController : ControllerBase
	{
		[HttpPost]
		public IActionResult GetServerResponse([FromBody] DataSetGet dataSetGet)
		{
			Argument.NotNull(dataSetGet, nameof(dataSetGet));
			var message = JsonConvert.SerializeObject(dataSetGet);
			return Ok(message);
		}
	}
}
