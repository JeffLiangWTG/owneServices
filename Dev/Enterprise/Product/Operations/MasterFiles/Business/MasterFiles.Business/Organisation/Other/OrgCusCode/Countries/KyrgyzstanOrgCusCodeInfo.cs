using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class KyrgyzstanOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(KyrgyzstanOrgCusCodeInfo.OrgCusCodes.TIN, Res.GetString("b0ed38e6-82c3-4cc4-863b-8c523502d8fb", "Business VAT Tax Identification Number")); // Accounting consumption code

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Kyrgyzstan);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Kyrgyzstan);
			return result;
		}

		public static class OrgCusCodes
		{
			public const string TIN = "TIN";
		}
	}
}
