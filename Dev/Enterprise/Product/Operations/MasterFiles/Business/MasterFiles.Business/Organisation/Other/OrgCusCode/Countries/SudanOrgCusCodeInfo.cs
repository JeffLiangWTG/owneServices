using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class SudanOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Res.GetString("C682B74D-CB2A-4DDA-9511-52833582D6E6", "{0} Business Registration Number", OrgCusCode.CodeTypes.VATCode)); // Accounting consumption code

			list.RemoveCode(OrgCusCode.CodeTypes.GSTCode);
			list.AddPair("TIN", Res.GetString("65613A6B-349D-4E92-B444-8EDCFA0F3A2F", "Tax Identification Number"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			return OrgCusCodeInfo.GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Sudan);
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			return OrgCusCodeInfo.GetPrimaryCusCodes(Core.Constants.CountryCodes.Sudan);
		}

		public static class OrgCusCodes
		{
			public const string TIN = "TIN";
		}
	}
}
