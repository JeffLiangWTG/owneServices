using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class SpainOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider, IOrgCusCodeCustomsRegNoValidationProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.SpainCodeTypes.NIF, Country.TaxCodeDescriptions.VATCodeTemplate(OrgCusCode.SpainCodeTypes.NIF)); // Accounting consumption code

			list.AddPair(OrgCusCodes.DNI, Res.GetString("OrgCusCode.SpainCodeTypes.TaxRegistrationofIndividual", "Tax Registration of Individual (non-business)"));
			list.AddPair(OrgCusCodes.IGC, Res.GetString("OrgCusCode.SpainCodeTypes.CanariasIGICRegistrationNumber", "Canarias IGIC (NIF) Business Registration Number"));
			list.AddPair(OrgCusCodes.SII, Res.GetString("e3252f5a-35ce-4eaa-88a5-db0c4bc70fe1", "SII Registration (NIF Number)"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = OrgCusCodeInfo.GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Spain);
			result.Add(OrgCusCodes.NIF);
			result.Add(OrgCusCode.CodeTypes.CorporationCode);

			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = OrgCusCodeInfo.GetPrimaryCusCodes(Core.Constants.CountryCodes.Spain);
			result.Add(OrgCusCodes.NIF);
			result.Add(OrgCusCodes.IGC);
			return result;
		}

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode orgCusCode)
		{
			if (orgCusCode.OK_CodeType == OrgCusCode.SpainCodeTypes.NIF)
			{
				new ESNIFCodeValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
			}
			else if (orgCusCode.OK_CodeType == OrgCusCodes.SII)
			{
				new ESSIICodeValidator().Validate(orgCusCode);
			}

			OrgCusCodeValidation.ValidateCustomsCodeForEU(orgCusCode);
			OrgCusCodeValidation.ValidateCustomsCodeEORI(orgCusCode);
		}

		public static class OrgCusCodes
		{
			public const string NIF = "NIF";
			public const string DNI = "DNI";
			public const string IGC = "IGC";
			public const string SII = "SII";
		}
	}
}
