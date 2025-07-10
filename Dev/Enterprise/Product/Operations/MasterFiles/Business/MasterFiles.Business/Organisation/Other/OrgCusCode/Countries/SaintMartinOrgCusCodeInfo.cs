using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class SaintMartinOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		ZString CountryCode => Core.Constants.CountryCodes.SaintMartin;

		public static class OrgCusCodes
		{
			public const string SRT = "SRT";
			public const string SRN = "SRN";
			public const string NIF = "NIF";
			public const string TGC = "TGC";
			public const string TGCA = "TGCA";
		}

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.TGC, string.Format((NoResString)"TGCA Numéro d’Identification/VAT Business Registration Number")); // Accounting consumption code

			list.AddPair(OrgCusCodes.SRT, Res.GetString("OrgCusCodes.SIRETBusinessRegistration", "SIRET Business Registration"));
			list.AddPair(OrgCusCodes.SRN, Res.GetString("OrgCusCodes.SIRENEstablishmentIdentifier", "SIREN Establishment Identifier"));
			list.AddPair(OrgCusCodes.NIF, string.Format((NoResString)"Numéro d’Identification Fiscale"));

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(CountryCode);
			result.Add(OrgCusCodes.NIF);

			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(CountryCode);
			result.Add(OrgCusCodes.NIF);
			result.Remove(OrgCusCode.CodeTypes.CorporationCode);

			return result;
		}
	}
}
