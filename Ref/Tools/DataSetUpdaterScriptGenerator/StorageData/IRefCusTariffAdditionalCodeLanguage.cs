using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusTariffAdditionalCodeLanguage : IDataSetStorage
	{
		Guid ZY4_PK { get; set; }
		string ZY4_ZX6_NKLanguage { get; set; }
		Guid ZY4_ZY2_TariffAdditionalCode { get; set; }
		string ZY4_Description { get; set; }
	}
}
