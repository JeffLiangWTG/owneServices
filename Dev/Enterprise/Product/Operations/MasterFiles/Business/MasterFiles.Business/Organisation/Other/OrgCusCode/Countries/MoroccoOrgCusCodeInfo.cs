using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class MoroccoOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.TVACode, Country.TaxCodeDescriptions.VATCodeTemplate(OrgCusCode.CodeTypes.TVACode)); // Accounting consumption code

			list.AddPair(OrgCusCode.MoroccoCodeTypes.ICE, MultilingualString.Join(" / ", (NoResString)"Identifiant Commun de l’Entreprise", ResString.GetMultilingualString("ec04eaa4-a050-441c-8ad9-4d2a94c04ff7", "Enterprise Registration Number")));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Morocco);
			result.Add(OrgCusCode.MoroccoCodeTypes.ICE);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Morocco);
			return result;
		}
	}
}
