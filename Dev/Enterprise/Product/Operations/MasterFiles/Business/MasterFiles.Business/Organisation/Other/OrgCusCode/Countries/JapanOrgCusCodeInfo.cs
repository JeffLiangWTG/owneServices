using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class JapanOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		public static class OrgCusCodes
		{
			public const string CCP = "CCP";
		}

		ZString CountryCode => Core.Constants.CountryCodes.Japan;

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.JapanCodeTypes.CON, Country.GetDefaultTaxCodeDescription(OrgCusCode.JapanCodeTypes.CON)); // Accounting consumption code

			list.AddPair(OrgCusCode.JapanCodeTypes.CIE, Res.GetString("B212226F-F895-433F-8C70-37A8DF036CIE", "Customs Importer/Exporter Code"));
			list.AddPair(OrgCusCode.JapanCodeTypes.FSB, Res.GetString("B212226F-F895-433F-8C70-37A8DF036CSC", "Foreign Supplier/Buyer Code"));
			list.AddPair(OrgCusCode.JapanCodeTypes.JAS, Res.GetString("B212226F-F895-433F-8C70-37A8DF036JAS", "JASTPRO Code"));
			list.AddPair(OrgCusCode.JapanCodeTypes.LPC, Res.GetString("B212226F-F895-433F-8C70-37A8DF036LPC", "Legal Person Code"));
			list.AddPair(OrgCusCode.JapanCodeTypes.NUC, Res.GetString("B212226F-F895-433F-8C70-37A8DF036NUC", "NACCS User Code"));
			list.AddPair(OrgCusCode.JapanCodeTypes.AAL, Res.GetString("B212226F-F895-433F-8C70-37A8DF036AAL", "Air Cargo Agent Location Code"));
			list.OverridePair(OrgCusCodes.CCP, Res.GetString("OrgCusCodes.CCP", "Bonded Location Code"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(CountryCode);
			result.Add(OrgCusCode.JapanCodeTypes.CON);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(CountryCode);
			result.Add(OrgCusCode.JapanCodeTypes.CON);
			result.Remove(OrgCusCode.CodeTypes.CorporationCode);
			return result;
		}
	}
}
