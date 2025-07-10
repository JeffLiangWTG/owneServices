using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public static class EuEoriResolver
{
	public static ZString GetRegNoWithCountryCode(OrgAddress address)
	{
		var eoriCusCodes = address?.Header.GetEuEoriOrgCusCodes();

		var addressCusCode = eoriCusCodes?.FirstOrDefault(x => x.OK_OA_PremisesAddress == address.PK);
		var orgCusCode = addressCusCode ?? eoriCusCodes?.FirstOrDefault(x => !x.OK_OA_PremisesAddress.IsValid);

		return orgCusCode?.GetRegNoWithCountryCode() ?? ZString.Empty;
	}

	public static ZString GetRegNoWithCountryCode(OrgHeader header)
	{
		var orgCusCode = header?.GetEuEoriOrgCusCodes().FirstOrDefault();

		return orgCusCode?.GetRegNoWithCountryCode() ?? ZString.Empty;
	}

	static IEnumerable<OrgCusCode> GetEuEoriOrgCusCodes(this OrgHeader orgHeader)
	{
		var provider = ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>();

		return orgHeader.CustomsCodes
			.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori)
			.Where(code => provider.IsMemberOfEU(code.OK_RN_NKCodeCountry.ToString()));
	}

	static ZString GetRegNoWithCountryCode(this OrgCusCode code)
	{
		var regNumber = code?.OK_CustomsRegNo ?? ZString.Empty;
		var countryCode = code?.OK_RN_NKCodeCountry ?? ZString.Empty;
		return regNumber.StartsWith(countryCode) ? regNumber.ToString() : countryCode + regNumber;
	}
}
