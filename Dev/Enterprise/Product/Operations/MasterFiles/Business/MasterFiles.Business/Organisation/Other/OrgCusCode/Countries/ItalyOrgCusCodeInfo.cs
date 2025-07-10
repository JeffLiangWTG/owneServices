using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class ItalyOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeCustomsRegNoValidationProvider, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.IVA, Country.TaxCodeDescriptions.VATCodeTemplate(OrgCusCode.CodeTypes.IVA)); // Accounting consumption code

			list.AddPair(ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale, (NoResString)"Codice Fiscale - " + Res.GetString("5ae76d61-2647-4abe-9bce-ab8881bc42f3", "Italian Registration Number"));
			list.AddPair(ItalyOrgCusCodeInfo.OrgCusCodes.NBO, (NoResString)"NBO - " + Res.GetString("OrgCusCode.ItalyCodeTypes.NBO", "Do Not Charge {0}", (NoResString)"Bollo"));
			list.AddPair(ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail, Res.GetString("9bdba0a6-7b9e-4a79-801c-589b9a173ab7", "{0} / Certified Email", "Posta Elettronica Certificata"));
			list.AddPair(ItalyOrgCusCodeInfo.OrgCusCodes.CodiceAttive, Res.GetString("OrgCusCode.ItalyCodeTypes.CAT", "{0} / Primary Business Activity", "Codici Attività (ATECO)"));
			list.AddPair(ItalyOrgCusCodeInfo.OrgCusCodes.CUU, Res.GetString("0d8253a1-2d37-404d-96cc-3c13e6fdc36c", "{0} / Public Administration Code", "Codice Univoco Ufficio"));
			list.AddPair(ItalyOrgCusCodeInfo.OrgCusCodes.DefermentApprovaNumberForTrieste, Res.GetString("2EBB4F8C-0312-4962-95C8-3242667D2FAB", "{0} / Deferment Approval Number for Trieste", " Deferment Approval Number for Trieste"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Italy);
			result.Add(ItalyOrgCusCodeInfo.OrgCusCodes.NBO);
			result.Add(ItalyOrgCusCodeInfo.OrgCusCodes.CodiceAttive);
			result.Add(ItalyOrgCusCodeInfo.OrgCusCodes.CUU);
			result.Add(ItalyOrgCusCodeInfo.OrgCusCodes.DefermentApprovaNumberForTrieste);

			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Italy);

			return result;
		}

		public static class OrgCusCodes
		{
			public const string CodiceFiscale = "COD";
			public const string NBO = "NBO";
			public const string CodiceAttive = "CAT";
			public const string CUU = "CUU";
			public const string CertifiedEmail = "PEC";
			public const string DefermentApprovaNumberForTrieste = "DAT";
		}

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode customsRegNo)
		{
			switch (customsRegNo.OK_CodeType)
			{
				case OrgCusCodes.CertifiedEmail:
					MandatoryValidation.CheckEntered(customsRegNo.OK_CustomsRegNoInfo);
					EmailAddressValidation.ValidateEmailAddress(customsRegNo.OK_CustomsRegNoInfo);
					break;

				case OrgCusCodes.CodiceAttive:
					if (!customsRegNo.OK_CustomsRegNo.IsNumbersOnlyOrEmpty || customsRegNo.OK_CustomsRegNo.Length != 6)
					{
						var errorMessage = Res.GetString("a80b3639-362c-4131-9ffd-25c2cefe817c", "The CAT registration code must be a six digits numeric code (NNNNNN).");
						customsRegNo.OK_CustomsRegNoInfo.AddErrorIfEnforced(errorMessage, OrganisationRegistry.RegistrationNumberFormatFields.ITCAT);
					}
					break;

				case OrgCusCodes.CodiceFiscale:
					new ITCODValidator().Validate(customsRegNo.OK_CustomsRegNoInfo);
					break;

				case OrgCusCode.CodeTypes.IVA:
					new ITIVAValidator().Validate(customsRegNo.OK_CustomsRegNoInfo);
					break;
			}

			if (customsRegNo.CountryIsMemberOf(EconomicGroupList.Codes.EuropeanUnion))
			{
				OrgCusCodeValidation.ValidateCustomsCodeForEU(customsRegNo);
			}
			OrgCusCodeValidation.ValidateCustomsCodeEORI(customsRegNo);
		}
	}
}
