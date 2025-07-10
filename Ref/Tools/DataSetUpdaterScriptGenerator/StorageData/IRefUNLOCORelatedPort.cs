using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefUNLOCORelatedPort : IDataSetStorage
	{
		Guid RLR_PK { get; set; }
		string RLR_RL_NKRelatedPort { get; set; }
		short RLR_GroupNumber { get; set; }
	}
}
