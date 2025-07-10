using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class BosniaAndHerzegovinaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.BosniaAndHerzegovinaCodeTypes.PDV, Country.TaxCodeDescriptions.VATCodeTemplate(OrgCusCode.BosniaAndHerzegovinaCodeTypes.PDV)); // Accounting consumption code

			list.RemoveCode(OrgCusCode.CodeTypes.GSTCode);
			list.AddPair(OrgCusCode.BosniaAndHerzegovinaCodeTypes.IDB, string.Format((NoResString)"Identifikacioni Broj / {0}", Res.GetString("OrgCusCode.CodeTypes.BusinessRegistrationNumber", "Business Registration Number")));
			list.AddPair(OrgCusCode.BosniaAndHerzegovinaCodeTypes.JMB, string.Format((NoResString)"Jedinstvenom Maticnom Broju / {0}", Res.GetString("f996e02d-db7d-452b-824f-6cbd91dc9798", "Unique Personal Identification Number")));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.BosniaAndHerzegovina);
			result.Add(OrgCusCode.CodeTypes.GSTCode);
			result.Add(OrgCusCode.CodeTypes.BusinessRegistrationNumber);
			result.Add(OrgCusCode.BosniaAndHerzegovinaCodeTypes.IDB);
			result.Add(OrgCusCode.BosniaAndHerzegovinaCodeTypes.JMB);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.BosniaAndHerzegovina);
			return result;
		}
	}
}
