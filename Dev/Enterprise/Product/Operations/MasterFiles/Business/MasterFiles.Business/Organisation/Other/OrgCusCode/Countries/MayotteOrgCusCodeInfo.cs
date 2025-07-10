using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class MayotteOrgCusCodeInfo :
		OrgCusCodeInfo,
		IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.TVACode, Res.GetString("54223d2b-86dc-4638-9588-4179a7a8ec0d", "VAT (TVA) Business Registration Number")); // Accounting consumption code

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			return GetMainOrganizationNumberTypes(CountryCode);
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			return GetPrimaryCusCodes(CountryCode);
		}

		ZString CountryCode => Core.Constants.CountryCodes.Mayotte;
	}
}
