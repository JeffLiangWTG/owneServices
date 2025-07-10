using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class RomaniaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeCustomsRegNoValidationProvider, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.RomaniaCodeTypes.TVA, Country.TaxCodeDescriptions.VATCodeTemplate(OrgCusCode.RomaniaCodeTypes.TVA)); // Accounting consumption code

			list.AddPair(OrgCusCode.RomaniaCodeTypes.CNP, Res.GetString("803ce184-36a2-47c2-a043-1cd421b2ff1c", "Cod Numeric Personal/Personal Identification (Individual)"));
			list.AddPair(OrgCusCode.RomaniaCodeTypes.CIF, string.Format((NoResString)"Codul de Identificare Fiscală/{0}", Res.GetString("e31ed926-a66d-4ccd-a007-7a99394dcceb", "Business Identification Number")));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Romania);
			result.Add(OrgCusCode.RomaniaCodeTypes.CIF);
			result.Add(OrgCusCode.RomaniaCodeTypes.CNP);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Romania);
			result.Add(OrgCusCode.RomaniaCodeTypes.TVA);
			result.Add(OrgCusCode.RomaniaCodeTypes.CIF);
			result.Add(OrgCusCode.RomaniaCodeTypes.CNP);
			return result;
		}

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode orgCusCode)
		{
			if (orgCusCode.OK_CodeType == OrgCusCode.RomaniaCodeTypes.TVA)
			{
				new ROTVACodeValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
			}

			OrgCusCodeValidation.ValidateCustomsCodeForEU(orgCusCode);
			OrgCusCodeValidation.ValidateCustomsCodeEORI(orgCusCode);
		}
	}
}
