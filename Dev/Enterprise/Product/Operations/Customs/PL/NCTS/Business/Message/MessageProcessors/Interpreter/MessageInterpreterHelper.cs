using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.PL.NCTS.Business;

internal static class MessageInterpreterHelper
{
	public static string GetOfficeCodeWithDescription(BusinessObjectFactory factory, string code)
	{
		const string codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;

		var grouping = !string.IsNullOrEmpty(code) && code.Length > 1 ? code.Substring(0, 2) : string.Empty;
		var description = !string.IsNullOrEmpty(grouping)
			? ZZRefCusCodeListCombined.Loader
				.LoadAndFallbackToParentDataGroupingIfNotFound(factory, grouping, codeType, ZDateTime.Today)
				.FirstOrDefault(x => x.ZZD_Code == code)?.ZZD_Description.ToString()
			: string.Empty;

		return string.IsNullOrEmpty(description) ? code : $"{code} - {description}";
	}

	public static string GetTypeOfControlDescription(this BusinessObjectFactory factory, string code, ZDateTime? date = default) =>
		ZZRefCusCodeListCombined.Loader
			.LoadAndFallbackToParentDataGroupingIfNotFound(factory, Core.Constants.CountryCodes.Poland, RefCusCodeListType.Code.Code_CL716, date ?? ZDateTime.Today)
			.FirstOrDefault(x => x.ZZD_Code == code)?.ZZD_Description.ToString();
}
