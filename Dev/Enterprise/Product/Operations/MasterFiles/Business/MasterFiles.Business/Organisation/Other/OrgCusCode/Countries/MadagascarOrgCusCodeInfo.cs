using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class MadagascarOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.MadagascarCodeTypes.TIN, Country.TaxCodeDescriptions.VATCodeTemplate(OrgCusCode.CodeTypes.TVACode)); // Accounting consumption code

			list.RemoveCode(OrgCusCode.MadagascarCodeTypes.TIN);
			list.AddPair(OrgCusCode.MadagascarCodeTypes.TIN, string.Format((NoResString)"Numéro d'immatriculation Fiscale (NIF) / {0}", Res.GetString("2D1FB479-E764-496A-A505-38696CEC9CF3", "VAT (TVA) Business Registration")));
			list.AddPair(OrgCusCode.MadagascarCodeTypes.NIS, string.Format((NoResString)"Numéro d’identification Statistique / {0}", Res.GetString("5E3559B9-A297-4CC9-8722-C42A02A09072", "Statistical Identification Number")));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Madagascar);
			result.Add(OrgCusCode.MadagascarCodeTypes.NIS);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Madagascar);
			return result;
		}
	}
}
