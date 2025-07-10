using System.Data;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public class ExciseTaxTariffRelationshipDataRow
	{
		ExciseTaxTariffRelationshipDataRow(DataRow dr)
		{
			ZZ1_ZZI_TariffType = ExcelHelper.GetValueAsString(dr[0]);
			ZZ1_TariffCode = ExcelHelper.GetValueAsString(dr[1]);
			ZZ1_Description = ExcelHelper.GetValueAsString(dr[2]);
			ZZH_TariffCode = ExcelHelper.GetValueAsString(dr[3]);
		}

		public string ZZ1_ZZI_TariffType { get; }

		public string ZZ1_TariffCode { get; }

		public string ZZ1_Description { get; }

		public string ZZH_TariffCode { get; }

		bool IsValid
		{
			get
			{
				return !string.IsNullOrEmpty(ZZ1_ZZI_TariffType)
					&& !string.IsNullOrEmpty(ZZ1_TariffCode)
					&& !string.IsNullOrEmpty(ZZ1_Description)
					&& !string.IsNullOrEmpty(ZZH_TariffCode);
			}
		}

		public static ExciseTaxTariffRelationshipDataRow New(DataRow dr)
		{
			var instance = new ExciseTaxTariffRelationshipDataRow(dr);
			return instance.IsValid ? instance : null;
		}
	}
}
