using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class GuyanaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Country.TaxCodeDescriptions.VATBusinessRegistrationNumber); // Accounting consumption code

			list.AddPair(OrgCusCodes.TIN, Res.GetString("78c48dc7-9658-4b96-9fe1-dba3c4df8676", "Tax Identification Number"));
			list.RemoveCode(OrgCusCode.CodeTypes.GSTCode);

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = OrgCusCodeInfo.GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Guyana);
			result.Add(OrgCusCode.CodeTypes.VATCode);

			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = OrgCusCodeInfo.GetPrimaryCusCodes(Core.Constants.CountryCodes.Guyana);
			result.Add(OrgCusCode.CodeTypes.VATCode);

			return result;
		}

		public static class OrgCusCodes
		{
			public const string TIN = "TIN";
		}
	}
}
