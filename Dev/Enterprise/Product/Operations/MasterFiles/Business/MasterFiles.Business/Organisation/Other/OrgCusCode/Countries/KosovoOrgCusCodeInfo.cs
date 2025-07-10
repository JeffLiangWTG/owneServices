using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class KosovoOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		public static class OrgCusCodes
		{
			public const string TVS = "TVS";
			public const string TVSH = "TVSH";
		}

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.TVS, Res.GetString("80de3b71-ba61-4191-8b21-a8289094216d", "VAT (TVSH) Business Registration Number")); // Accounting consumption code

			list.AddPair(OrgCusCode.KosovoCodeTypes.NFK, string.Format((NoResString)"Numrin Fiskal / {0}", Res.GetString("8D27997C-2233-42EE-A9F0-82729A2FCAD8", "Fiscal Registration Number")));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Kosovo);
			result.Add(OrgCusCode.KosovoCodeTypes.NFK);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Kosovo);
			return result;
		}
	}
}
