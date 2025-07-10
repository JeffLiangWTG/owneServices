using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class JamaicaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.JamaicaCodeTypes.GCT, Res.GetString("5c26f97b-6af7-46a6-8ad9-6252602c06bb", "VAT (GCT) Business Registration Number")); // Accounting consumption code

			list.AddPair(OrgCusCode.JamaicaCodeTypes.TRN, Res.GetString("92A236DD-3CF3-4E9B-A558-E3C29A1B656A", "Taxpayer Registration Number / Tax Identification Number"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Jamaica);
			result.Add(OrgCusCode.JamaicaCodeTypes.TRN);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Jamaica);
			return result;
		}
	}
}
