using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class VanuatuOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, OrgCusCodeDescription.TIN); // Accounting consumption code

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Vanuatu);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Vanuatu);
			return result;
		}

		public static class OrgCusCodes
		{
			public const string TIN = "TIN";
		}

		public static class OrgCusCodeDescription
		{
			public static string TIN => Res.GetString("d0b1cf4e-3683-4d66-8a8b-8fbeab854c06", "Tax Identification Number");
		}
	}
}
