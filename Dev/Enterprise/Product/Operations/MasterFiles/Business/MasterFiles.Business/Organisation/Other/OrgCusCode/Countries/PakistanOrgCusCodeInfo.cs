using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class PakistanOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Res.GetString("6b26e4a5-882a-434a-811a-b0cae450f090", "STRN (VAT) Sales Tax Registration Number")); // Accounting consumption code

			list.AddPair(OrgCusCodes.NTN, Res.GetString("22a65f60-ff93-4164-af8e-cb1e31b87cf5", "National Tax Number"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			return OrgCusCodeInfo.GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Pakistan);
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			return OrgCusCodeInfo.GetPrimaryCusCodes(Core.Constants.CountryCodes.Pakistan);
		}

		public static class OrgCusCodes
		{
			public const string NTN = "NTN";
		}
	}
}
