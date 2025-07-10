using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class ZimbabweOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		public static class OrgCusCodes
		{
			public const string BPN = "BPN";
			public const string TIN = "TIN";
		}

		ZString CountryCode => Core.Constants.CountryCodes.Zimbabwe;

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Res.GetString("3509D539-2240-4789-9151-461322995AE3", "VAT Business Registration Number")); // Accounting consumption code

			list.AddPair(OrgCusCodes.BPN, Res.GetString("9d718b33-f53a-4451-b3e2-e02f86357c2c", "Business Partner/Identification Number"));
			list.AddPair(OrgCusCodes.TIN, Res.GetString("7404BE02-9D3C-4EC5-BE7A-27A38EEE8BE2", "Tax Identification Number"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(CountryCode);
			result.Add(OrgCusCodes.BPN);
			result.Add(OrgCusCodes.TIN);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(CountryCode);
			result.Add(OrgCusCodes.TIN);
			result.Remove(OrgCusCode.CodeTypes.CorporationCode);

			return result;
		}
	}
}
