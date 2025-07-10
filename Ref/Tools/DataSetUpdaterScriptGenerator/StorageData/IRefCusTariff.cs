using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public partial interface IRefCusTariff : IDataSetStorage
	{
		Guid ZZ1_PK { get; set; }
		Guid ZZ1_ZZI_TariffType { get; set; }
		string ZZ1_TariffCode { get; set; }
		short ZZ1_IAMUnique { get; set; }
		string ZZ1_Description { get; set; }
		DateTime ZZ1_StartDate { get; set; }
		DateTime ZZ1_EndDate { get; set; }
		string ZZ1_ZZF_NKTaxOrFeeCode { get; set; }
		string ZZ1_CompositeKeyOnZZ5 { get; set; }
		string ZZ1_ZZZ_NKDataGrouping { get; set; }
		Nullable<DateTime> ZZ1_PublishedDate { get; set; }

		IEnumerable<IRefCusTariffRelationship> RefCusTariffRelationships { get; }
		IEnumerable<IRefCusTariffUOM> RefCusTariffUOMs { get; }
		IEnumerable<IRefCusRate> RefCusRates { get; }
		IEnumerable<IRefCusTariffAttribute> RefCusTariffAttribute { get; }
		IEnumerable<IRefCusCondition> RefCusCondition { get; }
		IEnumerable<IRefCusTariffNationalCode> RefCusTariffNationalCode { get; }
		IEnumerable<IRefCusVATApplicability> RefCusVATApplicability { get; }
		IEnumerable<IRefCusTariffLanguage> RefCusTariffLanguage { get; }
		IEnumerable<IRefCusTariffAdditionalCode> RefCusTariffAdditionalCode { get; }
		IEnumerable<IRefCusTariffBRCharacteristic> RefCusTariffBRCharacteristic { get; }
	}
}
