using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class CameroonOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CameroonCodeTypes.NIU, string.Format((NoResString)"Numéro Identifiant Unique / {0}", Res.GetString("80be794b-6886-430e-a304-90c187a28518", "Unique Identification Number (Tax Registration)"))); // Accounting consumption code

			list.RemoveCode(OrgCusCode.CodeTypes.TaxFileCode);
			list.AddPair(OrgCusCode.CameroonCodeTypes.NRC, string.Format((NoResString)"Numéro de Registre de Commerce / {0}", Res.GetString("6eeea5bf-3c88-46a5-9c1a-cce7461a5ea9", "Commercial Register Number")));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Cameroon);
			result.Add(OrgCusCode.CodeTypes.TaxFileCode);
			result.Add(OrgCusCode.CameroonCodeTypes.NRC);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Cameroon);
			return result;
		}
	}
}
