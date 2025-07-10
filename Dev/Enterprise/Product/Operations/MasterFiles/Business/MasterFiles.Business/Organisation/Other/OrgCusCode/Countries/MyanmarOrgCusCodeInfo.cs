using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class MyanmarOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.MyanmarCodeTypes.CMT, Country.GetDefaultTaxCodeDescription(OrgCusCode.MyanmarCodeTypes.CMT)); // Accounting consumption code

			list.AddPair(OrgCusCode.CodeTypes.CompanyRegistrationNumber, Res.GetString("OrgCusCode.CodeTypes.CompanyRegistrationNumber", "Company Registration Number"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Myanmar);
			result.Add(OrgCusCode.CodeTypes.CompanyRegistrationNumber);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Myanmar);
			return result;
		}
	}
}
