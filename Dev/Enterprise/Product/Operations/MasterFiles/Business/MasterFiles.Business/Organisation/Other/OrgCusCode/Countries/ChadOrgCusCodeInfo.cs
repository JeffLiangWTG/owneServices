using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class ChadOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.ChadCodeTypes.NIF, string.Format((NoResString)"Numéro d’Identification Fiscale / {0}", Res.GetString("92e4a0c8-7cab-4373-8cf8-e460c403dfe0", "Fiscal Identification Number"))); // Accounting consumption code

			list.RemoveCode(OrgCusCode.CodeTypes.GSTCode);
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Chad);
			result.Add(OrgCusCode.CodeTypes.GSTCode);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Chad);
			return result;
		}
	}
}
