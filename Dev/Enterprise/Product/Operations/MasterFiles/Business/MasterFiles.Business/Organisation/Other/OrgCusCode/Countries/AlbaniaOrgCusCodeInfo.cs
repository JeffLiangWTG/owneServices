using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AlbaniaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.NIT, string.Format((NoResString)"NIPT Numri Identifikues i Personit të Tatueshëm / {0}", Res.GetString("42150ce9-cb1b-447b-8187-56b24ea30930", "Tax and VAT Registration"))); // Accounting consumption code

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Albania);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Albania);
			return result;
		}

		public static class OrgCusCodes
		{
			public const string NIT = "NIT";
			public const string TVSH = "TVSH";
			public const string NIPT = "NIPT";
		}
	}
}

