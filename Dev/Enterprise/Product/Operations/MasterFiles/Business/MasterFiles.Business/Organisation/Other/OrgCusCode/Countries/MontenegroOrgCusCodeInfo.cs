using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class MontenegroOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.PDV, Res.GetString("e165dd4d-959a-4251-b9cf-d6278d3d6970", "VAT (PDV) Business Registration Number")); // Accounting consumption code

			list.AddPair(OrgCusCodes.PIB, FormattableString.Invariant($"Porezni Identifikacijski Brojevi / {(Res.GetString("d61e659d-18e1-4af2-9064-0a963936b5ff", "Tax Identification Number"))}"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			return OrgCusCodeInfo.GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Montenegro);
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			return OrgCusCodeInfo.GetPrimaryCusCodes(Core.Constants.CountryCodes.Montenegro);
		}

		public static class OrgCusCodes
		{
			public const string PDV = "PDV";
			public const string PIB = "PIB";
		}
	}
}
