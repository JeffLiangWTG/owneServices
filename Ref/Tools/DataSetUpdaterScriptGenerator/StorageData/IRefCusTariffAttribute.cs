using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusTariffAttribute : IDataSetStorage
	{
		Guid ZZ3_PK { get; set; }
		Nullable<Guid> ZZ3_ZZ1_Tariff { get; set; }
		string ZZ3_Name { get; set; }
		string ZZ3_Value { get; set; }
		Nullable<Guid> ZZ3_ZZW_TariffNationalCode { get; set; }
	}
}
