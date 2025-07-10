using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefUNLOCO : IDataSetStorage
	{
		Guid RL_PK { get; set; }
		string RL_Code { get; set; }
	}
}
