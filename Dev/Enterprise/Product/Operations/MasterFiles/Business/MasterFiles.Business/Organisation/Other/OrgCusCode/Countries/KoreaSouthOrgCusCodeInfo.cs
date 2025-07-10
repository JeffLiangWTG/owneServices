using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo;

namespace Enterprise.MasterFiles.Business
{
	public class KoreaSouthOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider, IOrgCusCodeCustomsRegNoValidationProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Country.GetDefaultTaxCodeDescription(OrgCusCode.CodeTypes.VATCode)); // Accounting consumption code

			list.AddPair(CodeTypes.KBT, Res.GetString("e1bf8eb0-900a-4711-bbf1-d72befa42264", "Korea Business Principal Activity Type"));
			list.AddPair(CodeTypes.KBC, Res.GetString("a8248d49-521c-40fc-92e2-731dc11bc6c4", "Korea Business Principal Industry Category"));
			list.AddPair(CodeTypes.AEO, Res.GetString("OrgCusCode.KoreaCodeTypes.AEO", "Authorized Economic Operator"));
			list.AddPair(CodeTypes.UnipassIDForOrganization, Res.GetString("OrgCusCode.KoreaCodeTypes.UnipassIDForOrganization", "UNIPASS Registration Number for Business"));
			list.AddPair(CodeTypes.ForeignCompanyID, Res.GetString("OrgCusCode.KoreaCodeTypes.ForeignCompanyID", "Foreign Company ID"));
			list.AddPair(CodeTypes.UnipassIDForIndividual, Res.GetString("OrgCusCode.KoreaCodeTypes.UnipassIDForIndividual", "UNIPASS Registration Number for Individual"));
			list.AddPair(CodeTypes.KoreanRegNoForResident, Res.GetString("OrgCusCode.KoreaCodeTypes.KoreanRegNoForResident", "Citizen Registration Number"));
			list.AddPair(CodeTypes.KoreanRegNoForForeigner, Res.GetString("OrgCusCode.KoreaCodeTypes.KoreanRegNoForForeigner", "Foreigner Registration Number"));
			list.AddPair(CodeTypes.OfficeID, Res.GetString("OrgCusCode.KoreaCodeTypes.OfficeID", "Office ID"));
			list.AddPair(CodeTypes.IndustrialParkCode, Res.GetString("OrgCusCode.KoreaCodeTypes.IPC", "Industrial Park Code"));
			list.AddPair(CodeTypes.RoadNameCode, Res.GetString("OrgCusCode.KoreaCodeTypes.RoadNameCode", "Road Name Code"));
			list.AddPair(CodeTypes.BuildingNumber, Res.GetString("OrgCusCode.KoreaCodeTypes.BuildingNumberCode", "Building Number"));
			list.AddPair(CodeTypes.ECommerceCompanyID, Res.GetString("OrgCusCode.KoreaCodeTypes.ECommerceCompanyID", "E Commerce Company ID"));
			list.AddPair(CodeTypes.CourierCompanyID, Res.GetString("OrgCusCode.KoreaCodeTypes.CourierCompanyID", "Courier Company ID"));
			list.AddPair(CodeTypes.CertificateOfOriginExporterNumber, Res.GetString("OrgCusCode.KoreaCodeTypes.CertificateOfOriginExporterNumber", "Customs Approved COO Exporter Number"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = OrgCusCodeInfo.GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.KoreaSouth);
			result.Add(OrgCusCode.CodeTypes.CorporationCode);
			result.Add(CodeTypes.KoreanRegNoForResident);
			result.Add(CodeTypes.KoreanRegNoForForeigner);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = OrgCusCodeInfo.GetPrimaryCusCodes(Core.Constants.CountryCodes.KoreaSouth);
			result.Add(CodeTypes.KoreanRegNoForResident);
			result.Add(CodeTypes.KoreanRegNoForForeigner);
			return result;
		}

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode customsRegNo)
		{
			MandatoryValidation.CheckEntered(customsRegNo.OK_CustomsRegNoInfo);
			switch (customsRegNo.OK_CodeType)
			{
				case OrgCusCode.CodeTypes.VATCode:
					KoreaSouthRegistrationNumberValidator.ValidateVATNumber(customsRegNo.OK_CustomsRegNoInfo);
					break;
				case KoreaSouthComplianceInfo.CodeTypes.OfficeID:
					KoreaSouthRegistrationNumberValidator.ValidateOfficeID(customsRegNo.OK_CustomsRegNoInfo);
					break;
				case KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForResident:
					KoreaSouthRegistrationNumberValidator.ValidateKoreanRegNoForResident(customsRegNo.OK_CustomsRegNoInfo);
					break;
				case KoreaSouthComplianceInfo.CodeTypes.KBT:
					KoreaSouthRegistrationNumberValidator.ValidateKBT(customsRegNo.OK_CustomsRegNoInfo);
					break;
				case KoreaSouthComplianceInfo.CodeTypes.KBC:
					KoreaSouthRegistrationNumberValidator.ValidateKBC(customsRegNo.OK_CustomsRegNoInfo);
					break;
				case KoreaSouthComplianceInfo.CodeTypes.AEO:
					KoreaSouthRegistrationNumberValidator.ValidateAEO(customsRegNo.OK_CustomsRegNoInfo);
					break;
			}
		}
	}
}
