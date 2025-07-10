using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class BelizeOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		ZString CountryCode => Core.Constants.CountryCodes.Belize;

		public static class OrgCusCodes
		{
			public const string TIN = "TIN";
		}

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.GSTCode, Res.GetString("6F5680B9-70CF-4E43-B813-568277E1211B", "Business GST Registration TIN")); // Accounting consumption code

			list.AddPair(OrgCusCodes.TIN, Res.GetString("1A1B8BEE-30A8-4789-B6DC-B53272C15669", "Tax Identification Number"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = OrgCusCodeInfo.GetPrimaryCusCodes(CountryCode);
			result.Add(OrgCusCodes.TIN);

			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = OrgCusCodeInfo.GetMainOrganizationNumberTypes(CountryCode);
			result.Add(OrgCusCodes.TIN);
			result.Remove(OrgCusCode.CodeTypes.CorporationCode);

			return result;
		}
	}
}
