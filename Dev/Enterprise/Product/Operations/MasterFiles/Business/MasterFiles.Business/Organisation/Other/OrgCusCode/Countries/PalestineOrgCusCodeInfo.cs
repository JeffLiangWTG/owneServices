using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class PalestineOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Country.TaxCodeDescriptions.VATBusinessRegistrationNumber); // Accounting consumption code

			list.AddPair(OrgCusCodes.TIN, Res.GetString("dd33f826-a137-4da3-9666-e1782b53456a", "Tax Identification Number"));

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = OrgCusCodeInfo.GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.PalestinianTerritory);

			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = OrgCusCodeInfo.GetPrimaryCusCodes(Core.Constants.CountryCodes.PalestinianTerritory);
			result.Add(OrgCusCodes.TIN);

			return result;
		}

		public static class OrgCusCodes
		{
			public const string TIN = "TIN";
		}
	}
}
