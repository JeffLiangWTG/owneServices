using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AlgeriaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		ZString CountryCode => Core.Constants.CountryCodes.Algeria;

		public static class OrgCusCodes
		{
			public const string NIF = "NIF";
			public const string NRC = "NRC";
			public const string NIS = "NIS";
		}

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.NIF, FormattableString.Invariant($"Numéro d’Identification Fiscale / {(Res.GetString("640a0a7d-65d3-47e5-b628-6a7720d1cfcc", "TVA (VAT) and Business Tax Registration Number (TIN)"))}")); // Accounting consumption code

			list.AddPair(OrgCusCodes.NIS, FormattableString.Invariant($"Numéro d'Identification Statistique / {(Res.GetString("7743747e-4264-4d2b-b334-2c1c4e2fa776", "Statistical ID Number"))}"));
			list.AddPair(OrgCusCodes.NRC, FormattableString.Invariant($"Numéro de Registre de Commerce / {(Res.GetString("02608b00-9952-4a01-a432-d283124a210f", "Business Trade Register Number"))}"));
			list.RemoveCode(OrgCusCode.CodeTypes.TaxFileCode);

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(CountryCode);
			result.Add(OrgCusCodes.NRC);
			result.Add(OrgCusCodes.NIS);
			result.Remove(OrgCusCode.CodeTypes.TaxFileCode);

			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(CountryCode);
			result.Add(OrgCusCode.CodeTypes.CorporationCode);

			return result;
		}
	}
}

