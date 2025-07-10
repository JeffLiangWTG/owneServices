using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class FrenchGuianaOrgCusCodeInfo :
		OrgCusCodeInfo,
		IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.TVACode, Res.GetString("f693ecc3-9237-49d0-80dd-c4530fb7d975", "VAT (TVA) Business Registration Number")); // Accounting consumption code

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

		ZString CountryCode => Core.Constants.CountryCodes.FrenchGuyana;
	}
}
