using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class BahrainOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Country.TaxCodeDescriptions.VATBusinessRegistrationNumber); // Accounting consumption code

			list.RemoveCode(OrgCusCode.CodeTypes.GSTCode);
			list.RemoveCode(OrgCusCode.CodeTypes.TaxFileCode);
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Bahrain);
			result.Add(OrgCusCode.CodeTypes.GSTCode);
			result.Add(OrgCusCode.CodeTypes.TaxFileCode);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Bahrain);
			return result;
		}
	}
}
