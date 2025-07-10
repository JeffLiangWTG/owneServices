using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusTariffLanguage : IDataSetStorage
	{
		Guid ZX7_PK { get; set; }
		string ZX7_ZX6_NKLanguage { get; set; }
		Guid ZX7_ZZ1_Tariff { get; set; }
		string ZX7_Description { get; set; }
	}
}
