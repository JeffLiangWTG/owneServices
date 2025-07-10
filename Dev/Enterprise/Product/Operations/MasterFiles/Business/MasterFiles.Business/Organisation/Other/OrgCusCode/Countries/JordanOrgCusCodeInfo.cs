using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class JordanOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		public static class OrgCusCodes
		{
			public const string BusinessActivityNumber = "BAN";
		}

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.GSTCode, Country.GetDefaultTaxCodeDescription(OrgCusCode.CodeTypes.GSTCode)); // Accounting consumption code
			list.AddPair(OrgCusCode.CodeTypes.IdentityCardNumber, Res.GetString("97dba989-f3e1-4392-b78e-3c8fa2755364", "Identity Card Number"));
			list.AddPair(OrgCusCodes.BusinessActivityNumber, Res.GetString("65DD32BB-21F3-4C0E-9CE1-C701566CD6FC", "Business Activity Number"));

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Jordan);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Jordan);
			return result;
		}
	}
}
