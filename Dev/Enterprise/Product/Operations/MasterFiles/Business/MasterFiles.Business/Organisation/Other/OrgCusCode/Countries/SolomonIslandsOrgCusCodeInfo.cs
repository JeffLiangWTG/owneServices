using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class SolomonIslandsOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(SolomonIslandsOrgCusCodeInfo.OrgCusCodes.TIN, Res.GetString("dbf570ea-cc14-45b6-944b-b57b2f74704e", "Tax Identification Number")); // Accounting consumption code

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.SolomonIslands);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.SolomonIslands);
			return result;
		}

		public static class OrgCusCodes
		{
			public const string TIN = "TIN";
			public const string TAX = "TAX";
		}
	}
}
