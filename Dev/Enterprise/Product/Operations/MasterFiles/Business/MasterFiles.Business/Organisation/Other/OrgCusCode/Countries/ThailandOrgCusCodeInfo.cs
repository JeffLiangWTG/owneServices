using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class ThailandOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Country.GetDefaultTaxCodeDescription(OrgCusCode.CodeTypes.VATCode)); // Accounting consumption code

			list.AddPair(OrgCusCode.CodeTypes.CompanyRegistrationNumber, Res.GetString("OrgCusCode.CodeTypes.CompanyRegistrationNumber", "Company Registration Number"));
			list.AddPair(OrgCusCode.CodeTypes.TaxIDNumber, Res.GetString("OrgCusCode.CodeTypes.TaxIDNumber", "Tax ID Number"));
			list.AddPair(OrgCusCode.ThailandCodeTypes.BID, Res.GetString("OrgCusCode.ThailandCodeTypes.BID", "Business Head Office or Branch Identifier"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Thailand);
			result.Add(OrgCusCode.CodeTypes.CompanyRegistrationNumber);
			result.Add(OrgCusCode.CodeTypes.TaxIDNumber);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Thailand);
			result.Add(OrgCusCode.CodeTypes.VATCode);
			result.Add(OrgCusCode.CodeTypes.TaxIDNumber);
			return result;
		}
	}
}
