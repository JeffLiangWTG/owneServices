using System;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common;
using Microsoft.AspNetCore.Mvc;

namespace CargoWise.RefDbRepo.NewService.Controllers;

public partial class RefUNLOCOUtcOffsetController : ControllerBase, IDataSetController
{
	[HttpPost]
	public Task<IActionResult> GetDataStreamV2(DataSetGet dataSetGet)
	{
		return Task.FromResult(GetDataStream(dataSetGet));
	}

	[HttpPost]
	public IActionResult GetDataStream(DataSetGet dataSetGet)
	{
		return new EmptyResult();
	}

	[HttpGet]
	public DateTime GetServerTimestamp()
	{
		return DateTime.MinValue;
	}

	[HttpPost]
	public Task<string> Report([FromQuery] string clientId, [FromQuery] string clientTimestamp, [FromQuery] string checkpoint, [FromQuery] string dataset, [FromQuery] string systemType = null, [FromQuery] bool isInUse = true)
	{
		return Task.FromResult(string.Empty);
	}
}
