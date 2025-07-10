using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class SenegalOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.SenegalCodeTypes.NIN, string.Format((NoResString)"NINEA - Numéro d'Identification National des Entreprises et des Associations / {0}", Res.GetString("af0a3422-0814-4469-9ada-8e33e35be061", "Business & Tax Registration Number"))); // Accounting consumption code

			list.RemoveCode(OrgCusCode.CodeTypes.TaxFileCode);
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Senegal);
			result.Add(OrgCusCode.CodeTypes.TaxFileCode);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Senegal);
			return result;
		}
	}
}
