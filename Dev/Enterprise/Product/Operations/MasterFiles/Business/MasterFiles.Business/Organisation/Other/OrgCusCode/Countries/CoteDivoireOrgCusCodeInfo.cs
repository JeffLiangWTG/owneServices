using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class CoteDivoireOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CoteDivoireCodeTypes.NCC, string.Format((NoResString)"Numéro de Compte Contribuable / {0}", Res.GetString("1b653a69-391d-455c-87b2-a1d7676ce288", "Taxpayer Account Number"))); // Accounting consumption code

			list.RemoveCode(OrgCusCode.CodeTypes.TaxFileCode);
			list.AddPair(OrgCusCode.CoteDivoireCodeTypes.NRC, string.Format((NoResString)"Numéro de Registre de Commerce / {0}", Res.GetString("6eeea5bf-3c88-46a5-9c1a-cce7461a5ea9", "Commercial Register Number")));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.CoteDivoire);
			result.Add(OrgCusCode.CodeTypes.TaxFileCode);
			result.Add(OrgCusCode.CoteDivoireCodeTypes.NRC);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.CoteDivoire);
			return result;
		}
	}
}
