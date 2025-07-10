using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class KiribatiOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.KiribatiCodeTypes.TaxIdentificationNumber, Res.GetString("bd5c0b30-86f8-41af-af55-239bfab0e1b6", "Taxpayer Identification Number / Business VAT Registration Number")); // Accounting consumption code

			list.RemoveCode(OrgCusCode.CodeTypes.GSTCode);
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Kiribati);
			result.Add(OrgCusCode.CodeTypes.GSTCode);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Kiribati);
			return result;
		}
	}
}
