using System.Collections.Generic;
using System.Globalization;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class MauritaniaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.TVACode, Res.GetString("D4FD3C81-49AF-403B-B08D-DF2C1C8D32E1", "VAT (TVA) Business Registration Number")); // Accounting consumption code

			list.AddPair(OrgCusCodes.NIF, string.Format(CultureInfo.InvariantCulture, (NoResString)"Numéro d'Identification Fiscal / {0}", Res.GetString("B049F489-9594-4ABA-AF28-F33A04052645", "Tax Identification Number")));
			list.RemoveCode(OrgCusCode.CodeTypes.GSTCode);

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = OrgCusCodeInfo.GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Mauritania);
			result.Add(OrgCusCode.CodeTypes.TVACode);

			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = OrgCusCodeInfo.GetPrimaryCusCodes(Core.Constants.CountryCodes.Mauritania);
			result.Add(OrgCusCode.CodeTypes.TVACode);

			return result;
		}

		public static class OrgCusCodes
		{
			public const string NIF = "NIF";
		}
	}
}
