using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class UnitedArabEmiratesOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Country.TaxCodeDescriptions.VATBusinessRegistrationNumber); // Accounting consumption code

			list.AddPair(OrgCusCode.UnitedArabEmiratesCodeTypes.AEO, Res.GetString("OrgCusCode.UnitedArabEmiratesCodeTypes.AEO", "Authorized Economic Operator"));
			list.AddPair(OrgCusCode.UnitedArabEmiratesCodeTypes.CBLSNumber, Res.GetString("OrgCusCode.UnitedArabEmiratesCodeTypes.CBL", "CBLS number issued by UAE Ministry of Economy"));
			list.AddPair(OrgCusCode.UnitedArabEmiratesCodeTypes.MPCINumber, Res.GetString("OrgCusCode.UnitedArabEmiratesCodeTypes.MPC", "MPCI Party ID, as provided by NAIC upon Party Registration"));
			list.AddPair(OrgCusCode.UnitedArabEmiratesCodeTypes.IDNumber, Res.GetString("OrgCusCode.UnitedArabEmiratesCodeTypes.IDO", "ID Number"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.UnitedArabEmirates);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.UnitedArabEmirates);
			return result;
		}
	}
}
