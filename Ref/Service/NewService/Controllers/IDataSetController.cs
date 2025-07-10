using System;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common;
using Microsoft.AspNetCore.Mvc;

namespace CargoWise.RefDbRepo.NewService.Controllers
{
	public interface IDataSetController
	{
		DateTime GetServerTimestamp();
		Task<string> Report([FromQuery] string clientId, [FromQuery] string clientTimestamp, [FromQuery] string checkpoint, [FromQuery] string dataset, [FromQuery] string systemType = null, [FromQuery] bool isInUse = true);
		Task<IActionResult> GetDataStreamV2([FromBody] DataSetGet dataSetGet);
		IActionResult GetDataStream([FromBody] DataSetGet dataSetGet);
	}
}
