using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class TanzaniaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.TanzaniaCodeTypes.VRN, Country.TaxCodeDescriptions.VATBusinessRegistrationNumber); // Accounting consumption code

			list.AddPair(OrgCusCode.TanzaniaCodeTypes.TIN, Res.GetString("71371b7d-684c-41ca-a55c-f563387f537c", "Tax Identification Number"));
			list.RemoveCode(OrgCusCode.CodeTypes.TaxFileCode);
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Tanzania);
			result.Add(OrgCusCode.CodeTypes.TaxFileCode);
			result.Add(OrgCusCode.TanzaniaCodeTypes.TIN);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Tanzania);
			result.Add(OrgCusCode.TanzaniaCodeTypes.TIN);
			result.Remove(OrgCusCode.CodeTypes.CorporationCode);
			return result;
		}
	}
}
