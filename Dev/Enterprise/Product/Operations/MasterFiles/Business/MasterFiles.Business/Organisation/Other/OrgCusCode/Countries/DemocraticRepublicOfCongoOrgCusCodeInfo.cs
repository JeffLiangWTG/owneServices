using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class DemocraticRepublicOfCongoOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Country.GetDefaultTaxCodeDescription(OrgCusCode.CodeTypes.VATCode)); // Accounting consumption code

			list.AddPair(OrgCusCode.CodeTypes.CompanyNumber, Res.GetString("OrgCusCode.CodeTypes.CompanyNumber", "Company Number"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = OrgCusCodeInfo.GetPrimaryCusCodes(Core.Constants.CountryCodes.DemocraticRepublicOfCongo);
			result.Add(OrgCusCodes.NIU);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = OrgCusCodeInfo.GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.DemocraticRepublicOfCongo);
			result.Remove(OrgCusCode.CodeTypes.CorporationCode);

			return result;
		}

		public static class OrgCusCodes
		{
			public const string NIU = "NIU";
		}
	}
}
