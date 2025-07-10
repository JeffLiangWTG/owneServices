using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class SurinameOrgCusCodeInfo :
		OrgCusCodeInfo,
		IOrgCusCodeProvider
	{
		public static class OrgCusCodes
		{
			public const string BTW = "BTW";
		}

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.BTW, Res.GetString("3ae91f4a-30fe-44fc-bb09-ae8fcf8ef0d6", "VAT (BTW) Business Registration Number")); // Accounting consumption code

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

		ZString CountryCode => Core.Constants.CountryCodes.Suriname;
	}
}
