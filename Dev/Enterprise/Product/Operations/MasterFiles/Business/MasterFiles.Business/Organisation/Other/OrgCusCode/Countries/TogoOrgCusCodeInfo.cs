using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class TogoOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.TogoCodeTypes.NIF, string.Format((NoResString)"Numero d’Identification Fiscale / {0}", Res.GetString("ad60360b-98b6-47a5-ad47-73c3abb5ac95", "VAT (TVA) Business Registration Number"))); // Accounting consumption code

			list.AddPair(OrgCusCode.TogoCodeTypes.NIC, string.Format((NoResString)"Numero d’Immatriculation au Registre du Commerce / {0}", Res.GetString("3A79710E-EFF1-402A-88A4-574A90D5CF15", "Commercial Registration Number")));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Togo);
			result.Add(OrgCusCode.TogoCodeTypes.NIC);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Togo);
			return result;
		}
	}
}
