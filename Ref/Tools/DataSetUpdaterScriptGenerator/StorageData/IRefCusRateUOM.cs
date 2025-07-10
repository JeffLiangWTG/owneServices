using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusRateUOM : IDataSetStorage
	{
		Guid ZXG_PK { get; set; }
		Guid ZXG_ZZ2_Rate { get; set; }
		string ZXG_UOM { get; set; }
	}
}
