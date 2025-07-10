using System.Collections.Generic;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;

namespace CargoWise.RefDbRepo.TRReferenceData.Business
{
	public static class TRCodeListGenerateHelper
	{
		public static Dictionary<string, List<RefCusCodeListExcelConfig>> GetRefCusCodeListExcelConfig()
		{
			var result = new Dictionary<string, List<RefCusCodeListExcelConfig>>();

			result[Constants.RefCusCodeType.TariffAdditionalCodeListCode] = new List<RefCusCodeListExcelConfig> {
				new RefCusCodeListExcelConfig(1, "ZZD_Code", "Additional Code"),
				new RefCusCodeListExcelConfig(2, "ZZD_Description", "Description"),
				new RefCusCodeListExcelConfig(3, "ZZD_StartDate", "Start Date"),
				new RefCusCodeListExcelConfig(4, "ZZD_EndDate", "End Date"),
			};

			result[Constants.RefCusCodeType.WarehouseCodes] = new List<RefCusCodeListExcelConfig> {
				new RefCusCodeListExcelConfig(1, "ZZD_Code", "Warehouse Code"),
				new RefCusCodeListExcelConfig(2, "ZZD_Description", "Warehouse Name"),
				new RefCusCodeListExcelConfig(5, "ZZD_StartDate", "Start Date"),
				new RefCusCodeListExcelConfig(6, "ZZD_EndDate", "End Date"),
			};

			result[Constants.RefCusCodeType.SupportingDocumentsCodes] = new List<RefCusCodeListExcelConfig> {
				new RefCusCodeListExcelConfig(1, "ZZD_ZZK_NKCodeType", "ZZD_ZZK_NKCodeType"),
				new RefCusCodeListExcelConfig(2, "ZZD_Code", "ZZD_Code"),
				new RefCusCodeListExcelConfig(3, "ZZD_Description", "ZZD_Description"),
				new RefCusCodeListExcelConfig(4, "ZZD_StartDate", "ZZD_StartDate"),
				new RefCusCodeListExcelConfig(5, "ZZD_EndDate", "ZZD_EndDate"),
			};

			result[Constants.RefCusCodeType.ExportUnionCountryCodes] = new List<RefCusCodeListExcelConfig> {
				new RefCusCodeListExcelConfig(1, "ZZD_Code", "Country Code"),
				new RefCusCodeListExcelConfig(2, "ZZD_Description", "Country Name"),
				new RefCusCodeListExcelConfig(3, "ZZD_StartDate", "Start Date"),
				new RefCusCodeListExcelConfig(4, "ZZD_EndDate", "End Date"),
			};

			return result;
		}
	}
}
