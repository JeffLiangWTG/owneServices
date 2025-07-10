using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IUNDGVersion : IDataSetStorage
	{
		Guid DV_PK { get; set; }
		string DV_Name { get; set; }
		string DV_Standard { get; set; }
		bool DV_IsActive { get; set; }
	}
}
