using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class IcelandOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.IcelandCodeTypes.VSK, Res.GetString("Organisation|CustomsCodes|VSKIceland", "VAT (VSK) Business Registration Number")); // Accounting consumption code

			list.AddPair(OrgCusCode.IcelandCodeTypes.Kennitala, Res.GetString("OrgCusCode.IcelandCodeTypes.Kennitala", "Kennitala / Identification Number"));
			list.AddPair(OrgCusCode.IcelandCodeTypes.CustomsOfficeCode, Res.GetString("OrgCusCode.IcelandCodeTypes.CustomsOfficeCode", "Customs Office Code"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Iceland);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Iceland);
			return result;
		}
	}
}
