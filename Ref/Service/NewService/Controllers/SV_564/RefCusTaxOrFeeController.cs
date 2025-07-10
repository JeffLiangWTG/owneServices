using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;
using Microsoft.AspNetCore.Mvc;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Controllers;

#if DEBUG
/// <summary>
/// This is a sample to use GetMinimumSupportedDataSetVersionsBySRDbVersion.
/// </summary>
public partial class RefCusTaxOrFeeController : DataSetControllerBase<Models.RefCusTaxOrFee>
{
	public override IEnumerable<DataSetVersion> GetAvailableDataSetTimestamps([FromQuery] string version = null)
	{
		return GetMinimumSupportedDataSetVersionsBySRDbVersion(version, 564);
	}
}
#endif
