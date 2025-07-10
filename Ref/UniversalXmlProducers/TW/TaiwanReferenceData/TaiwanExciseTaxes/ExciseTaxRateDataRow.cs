using System;
using System.Data;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public class ExciseTaxRateDataRow
	{
		ExciseTaxRateDataRow(DataRow dr)
		{
			ZZ1_ZZI_TariffType = ExcelHelper.GetValueAsString(dr[0]);
			ZZ2_ZY1_ZZR_NKRateType = ExcelHelper.GetValueAsString(dr[1]);
			ZZ2_ZY1_NKRateCode = ExcelHelper.GetValueAsString(dr[2]);
			ZZ1_TariffCode = ExcelHelper.GetValueAsString(dr[3]);
			ZZ1_Description = ExcelHelper.GetValueAsString(dr[4]);
			ZZ2_RateFormula = ExcelHelper.GetValueAsString(dr[5]);
			ZZ2_RateFormulaDerivedFrom = ExcelHelper.GetValueAsString(dr[6]);
			if (DateTime.TryParse(ExcelHelper.GetValueAsString(dr[7]), out var startDate))
			{
				ZZ2_StartDate = startDate;
			}

			if (DateTime.TryParse(ExcelHelper.GetValueAsString(dr[8]), out var endDate))
			{
				ZZ2_EndDate = endDate;
			}
			ZZT_ZZA_NKTradeGroup = ExcelHelper.GetValueAsString(dr[9]);
		}

		public string ZZ1_ZZI_TariffType { get; }

		public string ZZ2_ZY1_ZZR_NKRateType { get;}

		public string ZZ2_ZY1_NKRateCode { get; }

		public string ZZ1_TariffCode { get; }

		public string ZZ1_Description { get; }

		public string ZZ2_RateFormula { get; }

		public string ZZ2_RateFormulaDerivedFrom { get; }

		public DateTime ZZ2_StartDate { get; }

		public DateTime ZZ2_EndDate { get; }

		public string ZZT_ZZA_NKTradeGroup { get; }

		bool IsValid
		{
			get
			{
				return !string.IsNullOrEmpty(ZZ1_ZZI_TariffType)
					&& !string.IsNullOrEmpty(ZZ2_ZY1_ZZR_NKRateType)
					&& !string.IsNullOrEmpty(ZZ2_ZY1_NKRateCode)
					&& !string.IsNullOrEmpty(ZZ1_TariffCode)
					&& !string.IsNullOrEmpty(ZZ1_Description)
					&& !string.IsNullOrEmpty(ZZ2_RateFormula)
					&& !string.IsNullOrEmpty(ZZ2_RateFormulaDerivedFrom)
					&& !string.IsNullOrEmpty(ZZT_ZZA_NKTradeGroup)
					&& ZZ2_StartDate != default
					&& ZZ2_EndDate != default;
			}
		}

		public static ExciseTaxRateDataRow New(DataRow dr)
		{
			var instance = new ExciseTaxRateDataRow(dr);
			return instance.IsValid ? instance : null;
		}
	}
}
