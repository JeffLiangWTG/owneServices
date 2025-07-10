using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusTariffNationalCode : IDataSetStorage
	{
		Guid ZZW_PK { get; set; }
		Guid ZZW_ZZ1_Tariff { get; set; }
		string ZZW_NationalCode { get; set; }
		string ZZW_Description { get; set; }
		string ZZW_ZZF_NKTaxOrFeeCode { get; set; }
		DateTime ZZW_StartDate { get; set; }
		DateTime ZZW_EndDate { get; set; }
		string ZZW_ZZZ_NKDataGrouping { get; set; }
		Nullable<DateTime> ZZW_PublishedDate { get; set; }

		IEnumerable<IRefCusRate> RefCusRate { get; }
		IEnumerable<IRefCusTariffAttribute> RefCusTariffAttribute { get; }
		IEnumerable<IRefCusVATApplicability> RefCusVATApplicability { get; }
		IEnumerable<IRefCusTariffUOM> RefCusTariffUOM { get; }
		IEnumerable<IRefCusTariffAdditionalCode> RefCusTariffAdditionalCode { get; }
	}
}
