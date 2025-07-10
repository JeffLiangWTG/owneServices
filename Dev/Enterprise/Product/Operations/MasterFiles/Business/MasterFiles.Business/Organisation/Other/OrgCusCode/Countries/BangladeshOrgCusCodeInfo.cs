using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class BangladeshOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Res.GetString("07eefddc-86d5-43cc-9934-2e0ef11bfa2f", "VAT Registration BIN (Business Identification Number)")); // Accounting consumption code

			list.AddPair(OrgCusCode.CodeTypes.BusinessRegistrationNumber, Res.GetString("OrgCusCode.CodeTypes.BusinessRegistrationNumber", "Business Registration Number"));
			list.AddPair(OrgCusCode.CodeTypes.TaxIDNumber, Res.GetString("OrgCusCode.CodeTypes.TaxIDNumberBG", "Tax Registration Number"));
			list.AddPair(OrgCusCode.BangladeshCodeTypes.AIN, Res.GetString("OrgCusCode.BangladeshCodeTypes.AIN", "Agent Identification Number"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Bangladesh);
			result.Add(OrgCusCode.CodeTypes.BusinessRegistrationNumber);
			result.Add(OrgCusCode.CodeTypes.TaxIDNumber);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Bangladesh);
			result.Add(OrgCusCode.CodeTypes.TaxIDNumber);
			result.Add(OrgCusCode.CodeTypes.VATCode);
			result.Add(OrgCusCode.BangladeshCodeTypes.AIN);
			return result;
		}
	}
}
