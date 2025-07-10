using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class SaudiArabiaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		public static class OrgCusCodes
		{
			public const string NAT = "NAT";
			public const string TIN = "TIN";
		}

		ZString CountryCode => Core.Constants.CountryCodes.SaudiArabia;

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Country.TaxCodeDescriptions.VATBusinessRegistrationNumber); // Accounting consumption code

			list.AddPair(OrgCusCode.CodeTypes.CompanyRegistrationNumber, Res.GetString("4B79B006-74B2-4806-A615-81D6A0F2F166", "Company Registration Number"));
			list.AddPair(OrgCusCodes.NAT, Res.GetString("2B156565-F813-4564-8757-5D116C9FEDA4", "National ID"));
			list.AddPair(OrgCusCodes.TIN, Res.GetString("D231BA88-F79F-47C3-A0E1-BFC50B3FF244", "Tax Identification Number"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(CountryCode);
			result.Add(OrgCusCode.CodeTypes.CompanyRegistrationNumber);
			result.Add(OrgCusCode.CodeTypes.CorporationCode);
			result.Add(OrgCusCode.CodeTypes.VATCode);
			result.Add(OrgCusCodes.NAT);
			result.Add(OrgCusCodes.TIN);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(CountryCode);
			result.Add(OrgCusCode.CodeTypes.VATCode);
			result.Add(OrgCusCode.CodeTypes.CorporationCode);
			return result;
		}
	}
}
