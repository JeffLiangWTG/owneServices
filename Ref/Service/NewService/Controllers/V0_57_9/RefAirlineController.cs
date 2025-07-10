using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common;
using Microsoft.AspNetCore.Mvc;
using ContractModel = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Controllers;

public partial class RefAirlineController : DataSetControllerBase<ContractModel.RefAirline>
{
	public override IEnumerable<DataSetVersion> GetAvailableDataSetTimestamps([FromQuery] string version = null)
	{
		var dataSetVersions = GetMinimumSupportedDataSetVersionsByContract(version, 57)
			?.Select(x => new DataSetVersion(x.Name, x.Timestamp, x.Name.Equals("RefAirline", StringComparison.OrdinalIgnoreCase) ? 0 : 1));
		return dataSetVersions;
	}
}
