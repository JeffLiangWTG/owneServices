using System.Collections.Generic;
using System.Globalization;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class GabonOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.TVACode, Res.GetString("D4FD3C81-49AF-403B-B08D-DF2C1C8D32E1", "VAT (TVA) Business Registration Number")); // Accounting consumption code

			list.AddPair(OrgCusCodes.NIF, string.Format(CultureInfo.InvariantCulture, (NoResString)"Numéro d'Identification Fiscal / {0}", Res.GetString("B049F489-9594-4ABA-AF28-F33A04052645", "Tax Identification Number")));
			list.AddPair(OrgCusCodes.RCM, string.Format(CultureInfo.InvariantCulture, (NoResString)"RCCM - Registre de Commerce et du Crédit Mobilier / {0}", Res.GetString("22458030-8E31-44CA-BDD1-D319F734E77B", "Commercial Registration Number")));

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = OrgCusCodeInfo.GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Gabon);

			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = OrgCusCodeInfo.GetPrimaryCusCodes(Core.Constants.CountryCodes.Gabon);
			result.Add(OrgCusCodes.RCM);

			return result;
		}

		public static class OrgCusCodes
		{
			public const string NIF = "NIF";
			public const string RCM = "RCM";
		}
	}
}
