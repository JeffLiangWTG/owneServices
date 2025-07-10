using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusTaxOrFeeLanguage : IDataSetStorage
	{
		Guid ZXU_PK { get; set; }
		Guid ZXU_ZZF_TaxOrFee { get; set; }
		string ZXU_ZX6_NKLanguage { get; set; }
		string ZXU_Description { get; set; }
	}
}
