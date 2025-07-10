using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class HongKongOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.HKCodeTypes.KnownConsignorNumber, Res.GetString("OrgCusCode.HKCodeTypes.KnownConsignorNumber", "Known Consignor Number"));
			list.AddPair(OrgCusCode.CodeTypes.VGMRegistrationNumber, Res.GetString("OrgCusCode.CodeTypes.VGMRegistrationNumber", "VGM Registration Number"));
			list.AddPair(OrgCusCode.HKCodeTypes.AEO, Res.GetString("OrgCusCode.HKCodeTypes.AEO", "Authorized Economic Operator"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.HongKong);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.HongKong);
			return result;
		}
	}
}
