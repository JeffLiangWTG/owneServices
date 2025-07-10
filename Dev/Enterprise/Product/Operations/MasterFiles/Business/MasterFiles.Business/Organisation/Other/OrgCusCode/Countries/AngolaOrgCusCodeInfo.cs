using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class AngolaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.IVA, Country.TaxCodeDescriptions.VATCodeTemplate(OrgCusCode.CodeTypes.IVA)); // Accounting consumption code

			list.AddPair(OrgCusCode.AngolaCodeTypes.NumeroDeIdentificacioFiscal, Res.GetString("OrgCusCode.AngolaCodeTypes.NumeroDeIdentificacioFiscal", "NIF / Tax Identification Number"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Angola);
			result.Add(OrgCusCode.AngolaCodeTypes.NumeroDeIdentificacioFiscal);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Angola);
			return result;
		}
	}
}
