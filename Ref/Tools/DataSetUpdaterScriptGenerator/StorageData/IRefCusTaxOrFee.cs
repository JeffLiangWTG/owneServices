using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusTaxOrFee : IDataSetStorage
	{
		Guid ZZF_PK { get; set; }
		string ZZF_Code { get; set; }
		string ZZF_Description { get; set; }
		decimal ZZF_Value { get; set; }
		DateTime ZZF_StartDate { get; set; }
		DateTime ZZF_EndDate { get; set; }
		string ZZF_ZZZ_NKDataGrouping { get; set; }
		decimal ZZF_Minimum { get; set; }
		decimal ZZF_Maximum { get; set; }
		decimal ZZF_Threshold { get; set; }
		string ZZF_ZX0_NKTaxOrFeeType { get; set; }

		IEnumerable<IRefCusTaxOrFeeLanguage> RefCusTaxOrFeeLanguage { get; }
	}
}
