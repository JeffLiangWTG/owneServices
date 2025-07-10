using System;
using System.Data;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public class ExciseTaxTariffDataRow
	{
		ExciseTaxTariffDataRow(DataRow dr)
		{
			ZZ1_ZZI_TariffType = ExcelHelper.GetValueAsString(dr[0]);
			ZZ1_TariffCode = ExcelHelper.GetValueAsString(dr[1]);
			ZZ1_Description = ExcelHelper.GetValueAsString(dr[2]);
			if(DateTime.TryParse(ExcelHelper.GetValueAsString(dr[3]), out var startDate))
			{
				ZZ1_StartDate = startDate;
			}

			if(DateTime.TryParse(ExcelHelper.GetValueAsString(dr[4]), out var endDate))
			{
				ZZ1_EndDate = endDate;
			}

			ExciseTaxTariffUOMDataRow = ExciseTaxTariffUOMDataRow.New(ExcelHelper.GetValueAsString(dr[6]), ExcelHelper.GetValueAsString(dr[7]));
		}

		public string ZZ1_ZZI_TariffType { get; }

		public string ZZ1_TariffCode { get; }

		public string ZZ1_Description { get; }

		public DateTime ZZ1_StartDate { get; }

		public DateTime ZZ1_EndDate { get; }

		public ExciseTaxTariffUOMDataRow ExciseTaxTariffUOMDataRow { get; }

		bool IsValid
		{
			get
			{
				return !string.IsNullOrEmpty(ZZ1_ZZI_TariffType)
					&& !string.IsNullOrEmpty(ZZ1_TariffCode)
					&& !string.IsNullOrEmpty(ZZ1_Description)
					&& ZZ1_StartDate != default
					&& ZZ1_EndDate != default;
			}
		}

		public static ExciseTaxTariffDataRow New(DataRow dr)
		{
			var instance = new ExciseTaxTariffDataRow(dr);
			return instance.IsValid ? instance : null;
		}
	}
}
