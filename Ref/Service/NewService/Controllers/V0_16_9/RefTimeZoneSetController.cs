using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;
using Microsoft.AspNetCore.Mvc;
using ContractModel = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Controllers;

public partial class RefTimeZoneSetController : DataSetControllerBase<ContractModel.RefTimeZoneSet>
{
	public override IEnumerable<DataSetVersion> GetAvailableDataSetTimestamps([FromQuery] string version = null)
	{
		return GetMinimumSupportedDataSetVersionsByContract(version, 16);
	}
}
