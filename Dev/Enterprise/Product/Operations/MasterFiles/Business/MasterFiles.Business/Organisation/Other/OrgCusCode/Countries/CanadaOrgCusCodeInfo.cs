using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class CanadaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CACodeTypes.BusinessNumberForGoodsServicesHarmonizedSalesTax, Res.GetString("Organisation|CustomsCodes|BRTCodeCanada", "Business Number For Goods and Services Tax/Harmonized Sales Tax")); // Accounting consumption code

			list.OverridePair(OrgCusCode.CodeTypes.CarrierCode, Res.GetString("OrgCusCode.CodeTypes.CarrierCodeStandard", "Standard Carrier Alpha Code"));

			list.AddPair(OrgCusCode.CACodeTypes.AuthorizationID, Res.GetString("OrgCusCode.CACodeTypes.AuthorizationID", "Authorization ID"));
			list.AddPair(OrgCusCode.CACodeTypes.ExportLicenceNumber, Res.GetString("OrgCusCode.CACodeTypes.ExportLicenceNumber", "Exporter License Number"));
			list.AddPair(OrgCusCode.CACodeTypes.BusinessNumberCustomsBroker, Res.GetString("OrgCusCode.CACodeTypes.BusinessNumberCustomsBroker", "Business Number Customs Broker"));
			list.AddPair(OrgCusCode.CACodeTypes.BusinessNumberForCorporateIncomeTax, Res.GetString("OrgCusCode.CACodeTypes.BusinessNumberForCorporateIncomeTax", "Business Number For Corporate Income Tax"));
			list.AddPair(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, Res.GetString("OrgCusCode.CACodeTypes.BusinessNumberForImportExport", "Business Number For Import/Export"));
			list.AddPair(OrgCusCode.CACodeTypes.BusinessNumberForExport, Res.GetString("OrgCusCode.CACodeTypes.BusinessNumberForExport", "Business Number For Export"));
			list.AddPair(OrgCusCode.CACodeTypes.BusinessNumberForLowValueShipments, Res.GetString("OrgCusCode.CACodeTypes.BusinessNumberForLowValueShipments", "Business Number For Low Value Shipments"));
			list.AddPair(OrgCusCode.CACodeTypes.BusinessNumberForPayrollDeductions, Res.GetString("OrgCusCode.CACodeTypes.BusinessNumberForPayrollDeductions", "Business Number For Payroll Deductions"));
			list.AddPair(OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial, Res.GetString("OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial", "Business Number Importer Commercial"));
			list.AddPair(OrgCusCode.CACodeTypes.BusinessNumberImporterNonCommercial, Res.GetString("OrgCusCode.CACodeTypes.BusinessNumberImporterNonCommercial", "Business Number Importer Non-Commercial"));
			list.AddPair(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, Res.GetString("OrgCusCode.CACodeTypes.WarehouseControlledPremisesID", "Customs Controlled Premises - Warehouse"));
			list.AddPair(OrgCusCode.CACodeTypes.CustomsOfficeCode, Res.GetString("OrgCusCode.CACodeTypes.CustomsOfficeCode", "Customs Office Code"));
			list.AddPair(OrgCusCode.CACodeTypes.CSAReferenceID, Res.GetString("OrgCusCode.CACodeTypes.CSAReferenceID", "CSA Reference ID"));
			list.AddPair(OrgCusCode.CACodeTypes.SocialInsuranceNumber, Res.GetString("OrgCusCode.CACodeTypes.SocialInsuranceNumber", "Social Insurance Number"));
			list.AddPair(OrgCusCode.CACodeTypes.QuebecSalesTaxID, Res.GetString("bbc51809-5e04-4aa2-8d7d-4f60ed274a6c", "Quebec Sales Tax Registration Number"));
			list.AddPair(OrgCusCode.CACodeTypes.AccountSecurityCode, Res.GetString("023DC8CF-1DB1-4B46-94B1-03DB65B8F01F", "Account Security Code"));
			list.AddPair(OrgCusCode.CACodeTypes.CFIAAccountNumber, Res.GetString("90be09df-4348-45c4-9438-c17c3f45e3b0", "CFIA Account Number"));
			list.AddPair(OrgCusCode.CACodeTypes.WorldManufacturerIdentifier, Res.GetString("8c2a9e16-0581-415d-aaa0-12cb555e8e54", "World Manufacturer Identifier"));
			list.AddPair(OrgCusCode.CACodeTypes.NuclearSafetyCommissionLicenseNumber, Res.GetString("aaceaa40-570c-45f0-9659-921d0ec71077", "Nuclear Safety Commission License Number"));
			list.AddPair(OrgCusCode.CACodeTypes.SafeFoodForCanadiansLicense, Res.GetString("B10AE6C7-11E1-4CBE-9E93-F678A3B8B32F", "Safe Food For Canadians License"));
			list.AddPair(OrgCusCode.CACodeTypes.ECCCAuthorizationNumber, Res.GetString("5463A901-F400-4ED4-9363-11061F3C49E9", "ECCC Authorization Number"));
			list.RemoveCode(OrgCusCode.CodeTypes.GovBusinessCode);
			list.RemoveCode(OrgCusCode.CodeTypes.CorporationCode);
			list.RemoveCode(OrgCusCode.CodeTypes.TaxFileCode);
			list.RemoveCode(OrgCusCode.CodeTypes.CustomsClientCode);
			list.RemoveCode(OrgCusCode.CodeTypes.SupplierCode);
			list.RemoveCode(OrgCusCode.CodeTypes.BrokerageRegistration);
			list.RemoveCode(OrgCusCode.CodeTypes.BrokerageSiteID);
			list.RemoveCode(OrgCusCode.CodeTypes.BrokeragePrinter);
			list.RemoveCode(OrgCusCode.CodeTypes.ManifestProviderID);
			list.RemoveCode(OrgCusCode.CodeTypes.DeliveranceCode);
			list.RemoveCode(OrgCusCode.CodeTypes.ContainerChainCommunityCode);
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Canada);
			result.Add(OrgCusCode.CACodeTypes.BusinessNumberForCorporateIncomeTax);
			result.Add(OrgCusCode.CACodeTypes.BusinessNumberForGoodsServicesHarmonizedSalesTax);
			result.Add(OrgCusCode.CACodeTypes.BusinessNumberForLowValueShipments);
			result.Add(OrgCusCode.CACodeTypes.BusinessNumberForPayrollDeductions);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Canada);
			result.Add(OrgCusCode.CACodeTypes.BusinessNumberForExport);
			result.Add(OrgCusCode.CACodeTypes.BusinessNumberForLowValueShipments);
			result.Add(OrgCusCode.CACodeTypes.BusinessNumberForCorporateIncomeTax);
			result.Add(OrgCusCode.CACodeTypes.BusinessNumberForGoodsServicesHarmonizedSalesTax);
			result.Add(OrgCusCode.CACodeTypes.BusinessNumberForPayrollDeductions);
			result.Add(OrgCusCode.CACodeTypes.AuthorizationID);
			result.Add(OrgCusCode.CACodeTypes.ExportLicenceNumber);
			return result;
		}
	}
}
