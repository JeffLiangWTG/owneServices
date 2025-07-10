using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class BurundiOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.TVACode, Res.GetString("6394675d-20da-4bb0-9c39-ebef77d1a762", "VAT (TVA) Business Registration Number")); // Accounting consumption code

			list.AddPair(OrgCusCodes.NIF, FormattableString.Invariant($"Numéro d'Identification Fiscale / {(Res.GetString("bc2ab4c5-5e44-47a7-b1b2-b6746afec778", "Tax Identification Number"))}"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			return OrgCusCodeInfo.GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Burundi);
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			return OrgCusCodeInfo.GetPrimaryCusCodes(Core.Constants.CountryCodes.Burundi);
		}

		public static class OrgCusCodes
		{
			public const string NIF = "NIF";
		}
	}
}
