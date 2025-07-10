using System.Collections.Generic;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Controllers;

public partial class RefCusCodeTypeController : DataSetControllerBase<Models.RefCusCodeType>
{
	protected override IEnumerable<Models.RefCusCodeType> GetData(DataBlockKey<Models.RefCusCodeType> key, string version)
	{
		var versionTuple = Adaptor.IsSRDbVersion(version) ? new System.Tuple<int, int, int>(0, 27, 9) : Adaptor.ParseVersion(version);
		var requestVersion = versionTuple.Item2;
		if (requestVersion <= 26)
		{
			var dataSetId = ((RefCusCodeTypeService)TypedService).GetDataSetId(key.DataSet);
			return ((RefCusCodeTypeService)TypedService).GetDataUpToV26(key.LowerTimestamp, key.UpperTimestamp, CheckpointHelper.TryGet(key.Checkpoint), BlockSize, dataSetId);
		}

		return base.GetData(key, version);
	}
}
