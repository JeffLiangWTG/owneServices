using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class BurkinaFasoOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		ZString CountryCode => Core.Constants.CountryCodes.BurkinaFaso;

		public static class OrgCusCodes
		{
			public const string IFU = "IFU";
			public const string RCM = "RCM";
		}

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.IFU, FormattableString.Invariant($"Identifiant Fiscal Unique / {(Res.GetString("40421F9C-6E32-4C0A-89CF-45EDEC1C9B87", "VAT (TVA) Business Registration Number"))}")); // Accounting consumption code

			list.AddPair(OrgCusCodes.RCM, FormattableString.Invariant($"RCCM Registre du Commerce et du Crédit Mobilier / {(Res.GetString("31A6C15E-4F8B-430D-9A54-336E4B4A33F9", "Commercial Registration Code"))}"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			return GetPrimaryCusCodes(CountryCode);
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(CountryCode);
			result.Add(OrgCusCode.CodeTypes.CorporationCode);

			return result;
		}
	}
}

