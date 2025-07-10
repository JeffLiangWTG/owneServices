using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using static Enterprise.Core.Constants;
using CodeType = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code;
using RefCusCodeListTypeCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;

namespace Enterprise.Customs.PL.Business;

public static class CusCodesHelper
{
	public static ICodeDescription FindCusCodeDescription(this BusinessObjectFactory factory, string codeType, ZString code, ZString grouping = default, ZDateTime? date = default)
	{
		if (code.IsEmpty)
		{
			return null;
		}
		if (grouping.IsEmpty)
		{
			grouping = CountryCodes.Poland;
		}
		date ??= ZDateTime.Today;

		return ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(factory, grouping, codeType, date.Value)
			.FirstOrDefault(x => x.ZZD_Code == code);
	}

	public static ZString GetOfficeCodeWithDescription(this BusinessObjectFactory factory, ZString officeCode)
	{
		if (officeCode.IsEmpty | officeCode.Length < 2)
		{
			return officeCode;
		}

		var officeCountryGrouping = officeCode.Substring(0, 2);
		return factory.GetCodeWithDescription(RefCusCodeListTypeCodes.CustomsOffice, officeCode, grouping: officeCountryGrouping);
	}

	public static ZString GetNoReleaseMotivationCodeWithDescription(this BusinessObjectFactory factory, ZString noReleaseMotivationCode)
		=> factory.GetCodeWithDescription(CodeType.Code_CL211, noReleaseMotivationCode);

	public static ZString GetIncidentCodeWithDescription(this BusinessObjectFactory factory, ZString incidentCode)
		=> factory.GetCodeWithDescription(CodeType.Code_CL019, incidentCode);

	public static ZString GetCodeWithDescription(this BusinessObjectFactory factory, string codeType, ZString code, ZString grouping = default, ZDateTime? date = default, string separator = null, bool lookInParents = false)
	{
		var cusCodeDescription = factory.FindCusCodeDescription(codeType, code, grouping, date);

		separator = separator switch
		{
			null => "- ",
			"" => string.Empty,
			_ => separator + " "
		};

		return string.IsNullOrEmpty(cusCodeDescription?.Description) ? code : $"{code} {separator}{cusCodeDescription.Description}";
	}
}
