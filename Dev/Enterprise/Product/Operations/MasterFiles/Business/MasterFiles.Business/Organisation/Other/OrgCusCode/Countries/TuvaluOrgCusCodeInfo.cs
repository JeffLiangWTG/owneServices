using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class TuvaluOrgCusCodeInfo :
		OrgCusCodeInfo,
		IOrgCusCodeProvider
	{
		public static class OrgCusCodes
		{
			public const string TCT = "TCT";
			public const string TIN = "TIN";
		}

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.TCT, Res.GetString("4def5ca5-bd03-49d0-902b-7373e5563066", "TCT Business Registration Number (TIN)")); // Accounting consumption code

			list.AddPair(OrgCusCodes.TIN, Res.GetString("dfc83b80-eb91-4dba-a6d9-d33b837d3cd1", "Taxpayer Identification Number"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(CountryCode);
			result.Add(OrgCusCodes.TIN);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(CountryCode);
			result.Add(OrgCusCode.CodeTypes.CorporationCode);
			result.Add(OrgCusCodes.TIN);
			return result;
		}

		ZString CountryCode => Core.Constants.CountryCodes.Tuvalu;
	}
}
