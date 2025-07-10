using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class LesothoOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Country.TaxCodeDescriptions.VATBusinessRegistrationNumber); // Accounting consumption code

			list.AddPair(OrgCusCodes.TIN, Res.GetString("8fdd606f-0d16-42de-bda3-fbc43060d4c5", "Tax Identification Number"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			return OrgCusCodeInfo.GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Lesotho);
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			return OrgCusCodeInfo.GetPrimaryCusCodes(Core.Constants.CountryCodes.Lesotho);
		}

		public static class OrgCusCodes
		{
			public const string TIN = "TIN";
		}
	}
}
