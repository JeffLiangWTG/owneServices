using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class CostaRicaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeCustomsRegNoValidationProvider, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(CostaRicaOrgCusCodeInfo.OrgCusCodes.BusinessIdentificationNumber, Country.GetDefaultTaxCodeDescription(CostaRicaOrgCusCodeInfo.OrgCusCodes.BusinessIdentificationNumber)); // Accounting consumption code

			list.RemoveCode(CostaRicaOrgCusCodeInfo.OrgCusCodes.BusinessIdentificationNumber);
			list.RemoveCode(OrgCusCode.CodeTypes.TaxFileCode);
			list.AddPair(CostaRicaOrgCusCodeInfo.OrgCusCodes.TaxFileCode, MultilingualString.Join(" / ", (NoResString)"Registro de Contribuyentes", ResString.GetMultilingualString("8e092620-1210-4e4e-b1d1-544e999b4a06", "Tax Registration Number")));
			list.AddPair(CostaRicaOrgCusCodeInfo.OrgCusCodes.IndividualIdentificationNumber, MultilingualString.Join(" / ", (NoResString)"Cédula Persona Física", ResString.GetMultilingualString("968a2a46-7cbf-43a4-b3ca-5d7698b2ea38", "Individual Identification Number")));
			list.AddPair(CostaRicaOrgCusCodeInfo.OrgCusCodes.BusinessIdentificationNumber, MultilingualString.Join(" / ", (NoResString)"Cédula Persona Jurídica", ResString.GetMultilingualString("538c3157-6a97-4e7e-a8cd-d01bf66dc4fe", "Business Identification Number")));
			list.AddPair(CostaRicaOrgCusCodeInfo.OrgCusCodes.DIMEXDocumentIdentificationNumber, MultilingualString.Join(" / ", (NoResString)"DIMEX Documento de Identidad Migratoria para Extranjeros", ResString.GetMultilingualString("c9d6fdef-15b3-43a8-8130-722281776acb", "Foreigner Tax Registration")));
			list.AddPair(CostaRicaOrgCusCodeInfo.OrgCusCodes.EACEconomicActivityCode, MultilingualString.Join(" / ", (NoResString)"Código de Actividad Económica", ResString.GetMultilingualString("FF283B06-9C6E-4B4C-9CFB-AB34328977D6", "Economic Activity Code")));
			list.AddPair(CostaRicaOrgCusCodeInfo.OrgCusCodes.NITIdentificationNumber, MultilingualString.Join(" / ", (NoResString)"NITE Número de Identificación Fiscal Especial", ResString.GetMultilingualString("040b4b7e-df7a-4348-806e-abae361ddcea", "Special Tax Registration")));
			list.AddPair(CostaRicaOrgCusCodeInfo.OrgCusCodes.AuthorizedEconomicOperator, ResString.GetMultilingualString("OrgCusCode.CostaRica.AuthorizedEconomicOperator", "Authorized Economic Operator"));
			list.AddPair(CostaRicaOrgCusCodeInfo.OrgCusCodes.UBILocacionCode, ResString.GetMultilingualString("06DE4AEE-E4F3-4F8D-B39B-F7EB47031255", "Location Code"));
			list.AddPair(CostaRicaOrgCusCodeInfo.OrgCusCodes.PYMPYME, ResString.GetMultilingualString("0B0E8ED6-D48E-4304-ADEC-927FDCE53198", "PYME"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.CostaRica);
			result.Add(OrgCusCode.CodeTypes.TaxFileCode);
			result.Add(CostaRicaOrgCusCodeInfo.OrgCusCodes.BusinessIdentificationNumber);
			result.Add(CostaRicaOrgCusCodeInfo.OrgCusCodes.IndividualIdentificationNumber);
			result.Add(CostaRicaOrgCusCodeInfo.OrgCusCodes.DIMEXDocumentIdentificationNumber);
			result.Add(CostaRicaOrgCusCodeInfo.OrgCusCodes.EACEconomicActivityCode);
			result.Add(CostaRicaOrgCusCodeInfo.OrgCusCodes.NITIdentificationNumber);
			result.Add(CostaRicaOrgCusCodeInfo.OrgCusCodes.UBILocacionCode);
			result.Add(CostaRicaOrgCusCodeInfo.OrgCusCodes.PYMPYME);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.CostaRica);
			result.Add(CostaRicaOrgCusCodeInfo.OrgCusCodes.IndividualIdentificationNumber);
			result.Add(CostaRicaOrgCusCodeInfo.OrgCusCodes.TaxFileCode);
			result.Add(CostaRicaOrgCusCodeInfo.OrgCusCodes.DIMEXDocumentIdentificationNumber);
			result.Add(CostaRicaOrgCusCodeInfo.OrgCusCodes.NITIdentificationNumber);
			result.Remove(OrgCusCode.CodeTypes.CorporationCode);
			return result;
		}

		public static class OrgCusCodes
		{
			public const string AuthorizedEconomicOperator = "AEO";
			public const string TaxFileCode = "GTX";
			public const string IndividualIdentificationNumber = "CID";
			public const string BusinessIdentificationNumber = "CIJ";
			public const string DIMEXDocumentIdentificationNumber = "DIM";
			public const string EACEconomicActivityCode = "EAC";
			public const string NITIdentificationNumber = "NIT";
			public const string UBILocacionCode = "UBI";
			public const string PYMPYME = "PYM";
		}

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode customsRegNo)
		{
			switch (customsRegNo.OK_CodeType)
			{
				case OrgCusCodes.AuthorizedEconomicOperator:
					if (!Regex.IsMatch(customsRegNo.OK_CustomsRegNo, "^[0-9]{12}$", RegexOptions.IgnoreCase))
					{
						customsRegNo.OK_CustomsRegNoInfo.AddError(Res.GetString("9649849c-3d1d-49cd-85db-ce32725f5ce1", "CR AEO number should consist of 12 numeric characters."));
					}
					break;

				case OrgCusCodes.BusinessIdentificationNumber:
					new CRCIJCodeValidator().Validate(customsRegNo.OK_CustomsRegNoInfo);
					break;

				case OrgCusCodes.DIMEXDocumentIdentificationNumber:
					new CRDIMCodeValidator().Validate(customsRegNo.OK_CustomsRegNoInfo);
					break;

				case OrgCusCodes.EACEconomicActivityCode:
					new CREACCodeValidator().Validate(customsRegNo.OK_CustomsRegNoInfo);
					break;

				case OrgCusCodes.IndividualIdentificationNumber:
					new CRCIDCodeValidator().Validate(customsRegNo.OK_CustomsRegNoInfo);
					break;

				case OrgCusCodes.NITIdentificationNumber:
					new CRNITCodeValidator().Validate(customsRegNo.OK_CustomsRegNoInfo);
					break;

				case OrgCusCodes.UBILocacionCode:
					new CRUBICodeValidator().Validate(customsRegNo.OK_CustomsRegNoInfo);
					break;
			}
		}
	}
}
