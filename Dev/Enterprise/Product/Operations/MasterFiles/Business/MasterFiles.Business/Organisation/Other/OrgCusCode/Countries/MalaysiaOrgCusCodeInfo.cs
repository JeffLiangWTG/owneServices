using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class MalaysiaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider, IOrgCusCodeCustomsRegNoValidationProvider, IOrgCusCodeStandardIndustrialClassificationNoValidationProvider
	{
		ZString CountryCode => Core.Constants.CountryCodes.Malaysia;

		public static class OrgCusCodes
		{
			public const string RegistrarOfBusiness = "ROB";
			public const string RegistrarOfCompany = "ROC";
			public const string ImportCustomsAgentBusinessCode = "IAG";
			public const string ExportCustomsAgentBusinessCode = "EAG";
			public const string OtherBusinessCode = "OTH";
			public const string CustomsStation = "CST";
			public const string CFSBondedPackUnpack = "CFS";
			public const string SER = "SER";
			public const string SAL = "SAL";
			public const string PersonalIdentificationCardNumber = "PIC";
			public const string TaxIdentificationNumber = "TIN";
		}

		// Code descriptions also referenced by registry entry descriptions
		public static class OrgCusCodesDescription
		{
			public static string OtherBusinessCode => Res.GetString("A17F3EC5-7BF4-406B-AB86-EF22C7546E9A", "Other Business Code");
			public static string PersonalIdentificationCardNumber => Res.GetString("CC6FB66A-6EA8-4865-B914-5E0D1F33E7D3", "Personal Identification Card Number");
			public static string TaxIdentificationNumber => Res.GetString("F60B12D8-61A5-403E-B1E4-C1FDA691EC65", "Tax Identification Number");
		}

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.SER, Country.GetDefaultTaxCodeDescription((NoResString)"SER")); // Accounting consumption code

			list.AddPair(OrgCusCode.CodeTypes.GSTCode, Res.GetString("A045A628-8160-4496-BCB0-F401611586F7", "Goods and Services Tax Registration Number"));
			list.OverridePair(OrgCusCodes.SER, Res.GetString("03D8E958-8FC2-4A7F-B33A-78D737627400", "Government Service Tax Registration Number"));
			list.AddPair(OrgCusCodes.SAL, Res.GetString("E67ED958-2CA8-484E-BDC6-87556296AE77", "Government Sales Tax Registration Number"));
			list.AddPair(OrgCusCodes.RegistrarOfBusiness, Res.GetString("099F2DC9-756E-4FC4-8354-293DFAC5E6A7", "Registrar of Business No"));
			list.AddPair(OrgCusCodes.RegistrarOfCompany, Res.GetString("5A1D7729-B070-4C6C-B3FE-97D50A963856", "Registrar of Company No"));
			list.AddPair(OrgCusCodes.ImportCustomsAgentBusinessCode, Res.GetString("A39A4D51-2186-4771-BF58-CB9EC011688F", "Import Customs Agent Code"));
			list.AddPair(OrgCusCodes.ExportCustomsAgentBusinessCode, Res.GetString("4670E97A-02BC-4B57-90E8-FA7F4BB5A325", "Export Customs Agent Code"));
			list.AddPair(OrgCusCodes.OtherBusinessCode, OrgCusCodesDescription.OtherBusinessCode);
			list.AddPair(OrgCusCodes.CFSBondedPackUnpack, Res.GetString("591035FE-6F13-4347-93DB-71EE7DF50DDC", "Container Freight Station (Bonded Pack / Unpack)"));
			list.OverridePair(OrgCusCode.CodeTypes.CarrierCode, Res.GetString("81A2E647-EB80-4CC2-BCCC-3AC12DEB933B", "Registered Shipping Agent Code"));
			list.OverridePair(OrgCusCode.CodeTypes.ControlledPremisesID, Res.GetString("F8ADE503-0211-48C8-93F3-B4AD60C67A91", "Port Operator Code"));
			list.AddPair(OrgCusCodes.CustomsStation, Res.GetString("B79EE05B-2D60-412C-AEF6-D12314C433CF", "Customs Station"));
			list.AddPair(OrgCusCodes.PersonalIdentificationCardNumber, OrgCusCodesDescription.PersonalIdentificationCardNumber);
			list.AddPair(OrgCusCodes.TaxIdentificationNumber, OrgCusCodesDescription.TaxIdentificationNumber);

			list.RemoveCode(OrgCusCode.CodeTypes.BrokeragePrinter);
			list.RemoveCode(OrgCusCode.CodeTypes.BrokerageRegistration);
			list.RemoveCode(OrgCusCode.CodeTypes.BrokerageSiteID);
			list.RemoveCode(OrgCusCode.CodeTypes.ContainerChainCommunityCode);
			list.RemoveCode(OrgCusCode.CodeTypes.CustomsClientCode);
			list.RemoveCode(OrgCusCode.CodeTypes.ManifestProviderID);
			list.RemoveCode(OrgCusCode.CodeTypes.SupplierCode);
			list.RemoveCode(OrgCusCode.CodeTypes.GovBusinessCode);
			list.RemoveCode(OrgCusCode.CodeTypes.CorporationCode);
			list.RemoveCode(OrgCusCode.CodeTypes.TaxFileCode);

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(CountryCode);
			result.Add(RefCountry.LoadFromCountryCode(new ReadOnlyBusinessObjectFactory(), CountryCode).LocalBusinessRegNoCodeType);
			result.Add(OrgCusCodes.SAL);

			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(CountryCode);
			result.Remove(OrgCusCode.CodeTypes.GovBusinessCode);
			result.Remove(OrgCusCode.CodeTypes.CorporationCode);
			result.Remove(OrgCusCode.CodeTypes.TaxFileCode);
			result.Add(OrgCusCode.CodeTypes.GSTCode);
			result.Add(OrgCusCodes.RegistrarOfBusiness);
			result.Add(OrgCusCodes.RegistrarOfCompany);
			result.Add(OrgCusCodes.SAL);
			result.Add(OrgCusCodes.TaxIdentificationNumber);

			return result;
		}

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode customsRegNo)
		{
			switch (customsRegNo.OK_CodeType)
			{
				case OrgCusCodes.OtherBusinessCode:
					MalaysiaRegistrationNumberValidator.ValidateOtherBusinessCode(customsRegNo.OK_CustomsRegNoInfo);
					break;
				case OrgCusCodes.PersonalIdentificationCardNumber:
					MalaysiaRegistrationNumberValidator.ValidatePersonalIdentificationCardNumber(customsRegNo.OK_CustomsRegNoInfo);
					break;
				case OrgCusCodes.TaxIdentificationNumber:
					MalaysiaRegistrationNumberValidator.ValidateTIN(customsRegNo.OK_CustomsRegNoInfo);
					break;
			}
		}

		void IOrgCusCodeStandardIndustrialClassificationNoValidationProvider.ValidateStandardIndustrialClassification(OrgCusCode customsRegNo)
		{
			if (!AccountingMasterFilesUtils.IsValidSIC(CountryCode, customsRegNo.OK_CustomsRegNo))
			{
				customsRegNo.OK_CustomsRegNoInfo.AddErrorIfEnforced(Res.GetString("96F5443F-AF23-4305-9CD5-03C1B8803817", "The MY SIC number '{0}' is invalid. It should be a valid MSIC code in format 'NNNNN'.", new string[] { customsRegNo.OK_CustomsRegNo }), OrganisationRegistry.RegistrationNumberFormatFields.MYSIC);
			}
		}
	}
}

