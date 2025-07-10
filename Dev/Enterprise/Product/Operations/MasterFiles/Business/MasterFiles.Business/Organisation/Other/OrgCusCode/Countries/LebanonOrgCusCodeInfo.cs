using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class LebanonOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Country.TaxCodeDescriptions.VATBusinessRegistrationNumber); // Accounting consumption code

			list.RemoveCode(OrgCusCode.CodeTypes.TaxFileCode);
			list.AddPair(OrgCusCode.LebanonCodeTypes.CRN, Res.GetString("66af4fc7-e2b6-472a-aa04-9feddc51a64d", "Commercial Registration Number"));
			list.AddPair(OrgCusCode.LebanonCodeTypes.TIN, Res.GetString("b44eca60-a132-4bdb-aa59-436241518324", "Tax Identification Number"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Lebanon);
			result.Add(OrgCusCode.CodeTypes.TaxFileCode);
			result.Add(OrgCusCode.LebanonCodeTypes.TIN);
			result.Add(OrgCusCode.LebanonCodeTypes.CRN);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Lebanon);
			return result;
		}
	}
}
