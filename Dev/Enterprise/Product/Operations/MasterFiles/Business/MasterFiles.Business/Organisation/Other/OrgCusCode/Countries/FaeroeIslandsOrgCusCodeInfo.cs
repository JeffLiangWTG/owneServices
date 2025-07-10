using System.Collections.Generic;
using System.Globalization;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class FaeroeIslandsOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		public static class OrgCusCodes
		{
			public const string MVG = "MVG";
		}

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.MVG, string.Format(CultureInfo.InvariantCulture, Res.GetString("B56A4102-C7A3-4B37-B418-22197D59F3E3", "VAT (MVG) Business Registration Number ({0})"), (NoResString)"V-tal"));

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.FaeroeIslands);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.FaeroeIslands);
			return result;
		}
	}
}
