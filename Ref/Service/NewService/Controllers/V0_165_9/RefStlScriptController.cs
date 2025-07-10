using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common;
using Microsoft.AspNetCore.Mvc;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Controllers;

public partial class RefStlScriptController : DataSetControllerBase<Models.RefStlScript>
{
	public override IEnumerable<DataSetVersion> GetAvailableDataSetTimestamps([FromQuery] string version = null)
	{
		var dataSetVersions = GetMinimumSupportedDataSetVersionsByContract(version, 165)
			?.Select(x => new DataSetVersion(x.Name, x.Timestamp, x.Name.Equals("RefStlScript", StringComparison.OrdinalIgnoreCase) ? 0 : 1));
		return dataSetVersions;
	}
}
