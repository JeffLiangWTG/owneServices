using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class EgyptOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Country.GetDefaultTaxCodeDescription(OrgCusCode.CodeTypes.VATCode)); // Accounting consumption code

			list.AddPair(OrgCusCode.EgyptCodeTypes.CommercialRegistrationNumber, Res.GetString("OrgCusCode.EgyptCodeTypes.CommercialRegistrationNumber", "Commercial Registration Number"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Egypt);
			result.Add(OrgCusCode.EgyptCodeTypes.CommercialRegistrationNumber);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Egypt);
			return result;
		}
	}
}
