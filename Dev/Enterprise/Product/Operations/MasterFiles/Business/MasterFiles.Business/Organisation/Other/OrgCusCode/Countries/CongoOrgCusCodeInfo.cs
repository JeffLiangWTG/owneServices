using System.Collections.Generic;
using System.Globalization;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class CongoOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.NIU, string.Format(CultureInfo.InvariantCulture, (NoResString)"Numéro d'Identification Unique / {0}", Res.GetString("b247522b-60ce-4c44-bee0-0bfbfbc4d713", "Tax Identification Number"))); // Accounting consumption code

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = OrgCusCodeInfo.GetPrimaryCusCodes(Core.Constants.CountryCodes.Congo);
			result.Add(OrgCusCodes.NIU);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = OrgCusCodeInfo.GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Congo);
			result.Remove(OrgCusCode.CodeTypes.CorporationCode);

			return result;
		}

		public static class OrgCusCodes
		{
			public const string NIU = "NIU";
		}
	}
}
