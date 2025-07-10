using System;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.NewService
{
	public static class CheckpointHelper
	{
		public static ICheckpoint TryGet(string checkpointString)
		{
			return !string.IsNullOrEmpty(checkpointString) ? JsonConvert.DeserializeObject<CheckpointV2>(checkpointString) : null;
		}

		public static ICheckpoint Create(Guid dataSetPK)
		{
			return new CheckpointV2(dataSetPK);
		}
	}
}
