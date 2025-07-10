using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class MauritiusOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Country.TaxCodeDescriptions.VATBusinessRegistrationNumber); // Accounting consumption code

			list.AddPair(OrgCusCode.CodeTypes.BusinessRegistrationNumber, Res.GetString("OrgCusCode.CodeTypes.BusinessRegistrationNumber", "Business Registration Number"));
			list.RemoveCode(OrgCusCode.CodeTypes.GovBusinessCode);
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Mauritius);
			result.Add(OrgCusCode.CodeTypes.BusinessRegistrationNumber);
			result.Remove(OrgCusCode.CodeTypes.GovBusinessCode);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Mauritius);
			result.Add(OrgCusCode.CodeTypes.BusinessRegistrationNumber);
			result.Remove(OrgCusCode.CodeTypes.GovBusinessCode);
			result.Remove(OrgCusCode.CodeTypes.CorporationCode);

			return result;
		}
	}
}

