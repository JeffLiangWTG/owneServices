using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class PalauOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.RemoveCode(OrgCusCode.CodeTypes.GSTCode);
			list.AddPair(OrgCusCode.PalauCodeTypes.EIN, Res.GetString("621bb230-97e6-4ed0-a95f-00c55c541952", "Employer Identification Number"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Palau);
			result.Add(OrgCusCode.PalauCodeTypes.EIN);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Palau);
			return result;
		}
	}
}
