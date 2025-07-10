using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCarrierCodeAttribute : IDataSetStorage
	{
		Guid ZZG_PK { get; set; }
		Guid ZZG_ZZ4_CarrierCode { get; set; }
		string ZZG_Name { get; set; }
		string ZZG_Value { get; set; }
	}
}
