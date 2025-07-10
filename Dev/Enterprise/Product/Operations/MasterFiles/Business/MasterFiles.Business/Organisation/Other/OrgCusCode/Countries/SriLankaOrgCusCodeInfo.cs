using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class SriLankaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Country.GetDefaultTaxCodeDescription(OrgCusCode.CodeTypes.VATCode)); // Accounting consumption code

			list.AddPair(OrgCusCode.SriLankaCodeTypes.SVATBusinessRegistrationNumber, (NoResString)"SVAT - Sri Lanka Business Registration Number");
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.SriLanka);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.SriLanka);
			return result;
		}

		public static class OrgCusCodes
		{
			public const string SVT = "SVT";
		}
	}
}
