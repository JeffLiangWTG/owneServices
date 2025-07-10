using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class RussiaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Country.GetDefaultTaxCodeDescription(OrgCusCode.CodeTypes.VATCode)); // Accounting consumption code

			list.AddPair(OrgCusCode.RussiaCodeTypes.OGRN, Res.GetString("d6d5a7f0-7a91-4950-bc91-dbccf1d49d8c", "OGRN - Main State Registration Number"));
			list.AddPair(OrgCusCode.RussiaCodeTypes.KPP, Res.GetString("9da5c943-4b40-4de8-a384-8dc6d2fadfba", "Tax Registration Reason Code"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Russia);
			result.Add(OrgCusCode.RussiaCodeTypes.KPP);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Russia);
			return result;
		}
	}
}
