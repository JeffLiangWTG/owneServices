using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusTariffAdditionalCode : IDataSetStorage
	{
		Guid ZY2_PK { get; set; }
		Nullable<Guid> ZY2_ZZ1_Tariff { get; set; }
		Nullable<Guid> ZY2_ZZW_NationalCode { get; set; }
		string ZY2_AdditionalCode { get; set; }
		string ZY2_Description { get; set; }
		string ZY2_ZY3_NKCategory { get; set; }
		string ZY2_ParentAdditionalCode { get; set; }
		string ZY2_ZY3_NKParentCategory { get; set; }
		Nullable<bool> ZY2_IsMandatory { get; set; }
		string ZY2_ZZZ_NKDataGrouping { get; set; }
		DateTime ZY2_StartDate { get; set; }
		DateTime ZY2_EndDate { get; set; }

		IEnumerable<IRefCusApplicability> RefCusApplicability { get; }
		IEnumerable<IRefCusTariffAdditionalCodeLanguage> RefCusTariffAdditionalCodeLanguage { get; }
	}
}
