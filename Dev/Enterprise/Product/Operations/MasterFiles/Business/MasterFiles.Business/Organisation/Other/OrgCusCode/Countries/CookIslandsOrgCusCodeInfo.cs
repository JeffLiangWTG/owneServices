using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class CookIslandsOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Res.GetString("4f1c3cdb-554b-46cb-8be2-f85abeca2b56", "VAT Business Registration Number (RMD)")); // Accounting consumption code

			list.AddPair(OrgCusCodes.RMD, Res.GetString("c4548a11-9526-499e-90b6-13911519088a", "RMD Registration Number"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.CookIslands);
			result.Add(OrgCusCodes.RMD);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.CookIslands);
			result.Add(OrgCusCodes.RMD);
			result.Remove(OrgCusCode.CodeTypes.CorporationCode);
			return result;
		}

		public static class OrgCusCodes
		{
			public const string RMD = "RMD";
		}
	}
}

