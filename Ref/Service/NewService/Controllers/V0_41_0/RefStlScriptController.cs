using System.Collections.Generic;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Controllers;

public partial class RefStlScriptController : DataSetControllerBase<Models.RefStlScript>
{
	protected override IEnumerable<Models.RefStlScript> GetData(DataBlockKey<Models.RefStlScript> key, string version)
	{
		var versionTuple = Adaptor.IsSRDbVersion(version) ? new System.Tuple<int, int, int>(0, 42, 9) : Adaptor.ParseVersion(version);
		var requestVersion = versionTuple.Item2;
		if (requestVersion < 41)
		{
			var dataSetId = ((RefStlScriptService)TypedService).GetDataSetId(key.DataSet);
			return ((RefStlScriptService)TypedService).GetDataUpToV40(key.LowerTimestamp, key.UpperTimestamp, CheckpointHelper.TryGet(key.Checkpoint), BlockSize, dataSetId);
		}

		return base.GetData(key, version);
	}
}
