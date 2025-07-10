using System.Collections.Generic;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Controllers;

public partial class RefVesselZZController : DataSetControllerBase<Models.RefVesselZZ>
{
	protected override IEnumerable<Models.RefVesselZZ> GetData(DataBlockKey<Models.RefVesselZZ> key, string version)
	{
		var versionTuple = Adaptor.IsSRDbVersion(version) ? new System.Tuple<int, int, int>(0, 34, 9) : Adaptor.ParseVersion(version);
		var requestVersion = versionTuple.Item2;
		if (requestVersion <= 33)
		{
			var dataSetId = ((RefVesselZZService)TypedService).GetDataSetId(key.DataSet);
			return ((RefVesselZZService)TypedService).GetDataUpToV33(key.LowerTimestamp, key.UpperTimestamp, CheckpointHelper.TryGet(key.Checkpoint), BlockSize, dataSetId);
		}

		return base.GetData(key, version);
	}
}
