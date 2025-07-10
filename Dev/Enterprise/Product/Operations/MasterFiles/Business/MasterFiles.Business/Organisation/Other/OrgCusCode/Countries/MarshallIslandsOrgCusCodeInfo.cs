using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class MarshallIslandsOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.EIN, Res.GetString("EFC03B09-C439-42EE-BA76-3A3A590ED35F", "Employer Identification Number"));
			list.AddPair(OrgCusCodes.SSN, Res.GetString("1E416219-336E-4951-A63C-A2D392F310F0", "Social Security Number"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.MarshallIslands);
			result.Add(OrgCusCodes.EIN);
			result.Add(OrgCusCodes.SSN);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.MarshallIslands);
			result.Add(OrgCusCodes.EIN);
			return result;
		}

		public static class OrgCusCodes
		{
			public const string EIN = "EIN";
			public const string SSN = "SSN";
		}
	}
}
