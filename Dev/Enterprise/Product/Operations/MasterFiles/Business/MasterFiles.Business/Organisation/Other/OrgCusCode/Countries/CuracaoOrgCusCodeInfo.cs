using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class CuracaoOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		public static class OrgCusCodes
		{
			public const string OB = "OB";
			public const string CRB = "CRB";
		}

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.CRB, string.Format((NoResString)"Curacao Recording Information Belastingen (CRIB) / {0}", Res.GetString("85e97abe-79ac-457d-9c61-b2e7fd47989c", "Business VAT (OB) Registration Number"))); // Accounting consumption code

			list.AddPair(OrgCusCode.CuracaoCodeTypes.CCR, Res.GetString("B46E5545-E0AA-4B6B-945D-A54F12D2D68C", "Chamber of Commerce Registration Number"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Curacao);
			result.Add(OrgCusCode.CuracaoCodeTypes.CCR);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Curacao);
			return result;
		}
	}
}
