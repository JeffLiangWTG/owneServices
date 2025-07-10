using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class MicronesiaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		ZString CountryCode => Core.Constants.CountryCodes.Micronesia;

		public static class OrgCusCodes
		{
			public const string EIN = "EIN";
		}

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.EIN, Res.GetString("01995a1f-7c4c-4966-a44b-098db97425f7", "Employer Identification Number"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(CountryCode);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(CountryCode);
			return result;
		}
	}
}
