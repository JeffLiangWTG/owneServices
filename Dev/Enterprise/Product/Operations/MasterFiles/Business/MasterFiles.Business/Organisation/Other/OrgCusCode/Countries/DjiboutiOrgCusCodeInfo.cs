using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class DjiboutiOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.NIF, OrgCusCodeDescription.NIF); // Accounting consumption code

			list.RemoveCode(OrgCusCode.CodeTypes.GSTCode);
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Djibouti);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Djibouti);
			result.Remove(OrgCusCode.CodeTypes.CorporationCode);
			return result;
		}

		public static class OrgCusCodes
		{
			public const string NIF = "NIF";
		}

		public static class OrgCusCodeDescription
		{
			public static string NIF => FormattableString.Invariant($"Numéro d'Identification Fiscale / {(Res.GetString("a95160bb-248d-4a53-9bb0-c6b32868f42c", "Tax and VAT Identification Number"))}");
		}
	}
}
