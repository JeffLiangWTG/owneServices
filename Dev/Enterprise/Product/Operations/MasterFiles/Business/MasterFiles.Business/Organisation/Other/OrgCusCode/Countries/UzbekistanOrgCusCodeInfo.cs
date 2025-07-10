using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class UzbekistanOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.QQS, Res.GetString("379ee54b-73f3-400c-94c3-3590907345f7", "VAT (QQS) Business Registration Number")); // Accounting consumption code

			list.AddPair(OrgCusCodes.STR, FormattableString.Invariant($"STIR Soliq Toʼlovchining Identifikatsiya Raqami / {(Res.GetString("a80f1298-1ffa-434f-ad9b-7fb306e0a528", "Taxpayer Identification Number"))}"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			return OrgCusCodeInfo.GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Uzbekistan);
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			return OrgCusCodeInfo.GetPrimaryCusCodes(Core.Constants.CountryCodes.Uzbekistan);
		}

		public static class OrgCusCodes
		{
			public const string QQS = "QQS";
			public const string STR = "STR";
		}
	}
}
