using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class NigerOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.NigerCodeTypes.NIF, string.Format((NoResString)"Numéro d’identification Fiscal / {0}", Res.GetString("dcde8fb7-1850-4cfb-9cc2-f66d86503e76", "Tax & VAT Identification Number"))); // Accounting consumption code

			list.RemoveCode(OrgCusCode.CodeTypes.TaxFileCode);
			list.AddPair(OrgCusCode.NigerCodeTypes.RCC, string.Format((NoResString)"RCCM - Registre du Commerce et du Crédit Mobilier / {0}", Res.GetString("b5a240bf-9d52-4e2d-b6de-673f6984fb3c", "Commercial Registration")));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Niger);
			result.Add(OrgCusCode.CodeTypes.TaxFileCode);
			result.Add(OrgCusCode.NigerCodeTypes.RCC);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Niger);
			return result;
		}
	}
}
