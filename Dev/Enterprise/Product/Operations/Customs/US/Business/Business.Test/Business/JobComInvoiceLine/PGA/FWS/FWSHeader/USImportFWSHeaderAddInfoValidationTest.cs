using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USImportFWSHeaderAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestFWSValidateState()
		{
			var refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "KNZ";
			refCountryStates1.RW_RN_NKCountryCode = "MX";
			refCountryStates1.RW_Description = "KNZTEST";
			Factory.Save();

			var header = InvoiceLine.FWSHeaders.AddNew();

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			var address = orgHeader.Addresses.AddNew(OrgAddressType.Delivery, true);
			address.OA_State = "XXXX";
			address.OA_RL_NKRelatedPortCode = "USABV";
			header.US_OA_FWSExporterAddress = address.PK;
			header.AddInfoValidation.ValidateUS_OA_FWSExporterAddress();
			AssertHasMessageErrorContaining(header.US_OA_FWSExporterAddressInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			header.US_OA_FWSExporterAddress = address.PK;
			header.AddInfoValidation.ValidateUS_OA_FWSExporterAddress();
			AssertNoMessageErrorContaining(header.US_OA_FWSExporterAddressInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			header.US_OA_FWSExporterAddress = address.PK;
			header.AddInfoValidation.ValidateUS_OA_FWSExporterAddress();
			AssertNoMessageErrorContaining(header.US_OA_FWSExporterAddressInfo, "The state is not a valid");

			address.OA_State = "XXXX";
			address.OA_RL_NKRelatedPortCode = "USABV";
			header.US_OA_FWSImporterAddress = address.PK;
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertHasMessageErrorContaining(header.US_OA_FWSImporterAddressInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			header.US_OA_FWSImporterAddress = address.PK;
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertNoMessageErrorContaining(header.US_OA_FWSImporterAddressInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			header.US_OA_FWSImporterAddress = address.PK;
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertNoMessageErrorContaining(header.US_OA_FWSImporterAddressInfo, "The state is not a valid");
		}
		public void TestCheckUS_ProcessingCode()
		{
			var header = InvoiceLine.FWSHeaders.AddNew();
			header.US_ProcessingCode = "#";
			AssertHasMessageErrorContaining(header.US_ProcessingCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(header.US_ProcessingCodeInfo, MandatoryValidation.YouHaveNotEntered);
			header.US_ProcessingCode = ZString.Empty;
			AssertNoMessageErrorContaining(header.US_ProcessingCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(header.US_ProcessingCodeInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertNoMessageErrorContaining(header.US_ProcessingCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(header.US_ProcessingCodeInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertNoMessageErrorContaining(header.US_ProcessingCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(header.US_ProcessingCodeInfo, MandatoryValidation.YouHaveNotEntered);
			foreach (ICodeDescription pair in header.AddInfoLookups.ProcessingCodes)
			{
				header.US_ProcessingCode = pair.Code;
				AssertNoMessageErrorContaining(header.US_ProcessingCodeInfo, ListValidation.InvalidCodeMessageError);
			}

			header.US_ProcessingCode = "EDS";
			AssertHasMessageErrorContaining(header.US_ProcessingCodeInfo, ValidationConstants.FWS.FWLLicenseTypeIsRequiredForWildlifeCommercialPurpose);

			var license = header.Licenses.AddNew();
			license.US_Type = FWSLicenseTypeList.Codes.FWSImportExportLicense;
			header.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertNoMessageErrorContaining(header.US_ProcessingCodeInfo, ValidationConstants.FWS.FWLLicenseTypeIsRequiredForWildlifeCommercialPurpose);

			header.US_ProcessingCode = "LDS";
			AssertHasMessageErrorContaining(header.US_ProcessingCodeInfo, ValidationConstants.FWS.FWCLicenseTypeIsRequiredForLDS);

			license.US_Type = FWSLicenseTypeList.Codes.FWSeDecsConfirmationNumber;
			license.US_Number = "123456";
			header.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertNoMessageErrorContaining(header.US_ProcessingCodeInfo, ValidationConstants.FWS.FWCLicenseTypeIsRequiredForLDS);
		}

		public void TestCheckUS_ProductType()
		{
			var header = InvoiceLine.FWSHeaders.AddNew();
			header.US_ProductType = "!";
			AssertHasMessageErrorContaining(header.US_ProductTypeInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in new GlobalUniqueProductCodeQualifierList())
			{
				header.US_ProductType = pair.Code;
				AssertNoMessageErrorContaining(header.US_ProductTypeInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckUS_ProductNumber()
		{
			var header = InvoiceLine.FWSHeaders.AddNew();
			header.US_ProductType = "!";
			header.US_ProductNumber = "SDF";
			AssertNoMessageError(header.US_ProductNumberInfo, ValidationConstants.FWS.ProductNumberIsRequiredWhenTypeIsEntered);
			header.US_ProductNumber = ZString.Empty;
			AssertHasMessageError(header.US_ProductNumberInfo, ValidationConstants.FWS.ProductNumberIsRequiredWhenTypeIsEntered);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_ProductNumber();
			AssertNoMessageError(header.US_ProductNumberInfo, ValidationConstants.FWS.ProductNumberIsRequiredWhenTypeIsEntered);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_ProductNumber();
			AssertHasMessageError(header.US_ProductNumberInfo, ValidationConstants.FWS.ProductNumberIsRequiredWhenTypeIsEntered);
			header.US_ProductType = ZString.Empty;
			AssertNoMessageError(header.US_ProductNumberInfo, ValidationConstants.FWS.ProductNumberIsRequiredWhenTypeIsEntered);
		}

		public void TestCheckUS_ScientificGenusName()
		{
			var header = InvoiceLine.FWSHeaders.AddNew();
			header.US_ScientificGenusName = "!";
			header.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage("Scientific Genus Name");
			AssertNoMessageError(header.US_ScientificGenusNameInfo, messageError);
			header.US_ScientificGenusName = ZString.Empty;
			AssertHasMessageError(header.US_ScientificGenusNameInfo, messageError);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_ScientificGenusName();
			AssertNoMessageError(header.US_ScientificGenusNameInfo, messageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_ScientificGenusName();
			AssertHasMessageError(header.US_ScientificGenusNameInfo, messageError);
		}

		public void TestCheckUS_ScientificSpeciesName()
		{
			var header = InvoiceLine.FWSHeaders.AddNew();
			header.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			header.US_ScientificSpeciesName = "!";
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage("Scientific Species Name");
			AssertNoMessageError(header.US_ScientificSpeciesNameInfo, messageError);
			header.US_ScientificSpeciesName = ZString.Empty;
			AssertHasMessageError(header.US_ScientificSpeciesNameInfo, messageError);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_ScientificSpeciesName();
			AssertNoMessageError(header.US_ScientificSpeciesNameInfo, messageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_ScientificSpeciesName();
			AssertHasMessageError(header.US_ScientificSpeciesNameInfo, messageError);
		}

		public void TestCheckUS_Scientific2GenusName()
		{
			var header = InvoiceLine.FWSHeaders.AddNew();
			header.US_Hybrid = FWSHybridTypeList.Codes.Intergeneric;
			header.US_Scientific2GenusName = "!";
			header.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage("2nd Scientific Genus Name");
			AssertNoMessageError(header.US_Scientific2GenusNameInfo, messageError);
			header.US_Scientific2GenusName = ZString.Empty;
			AssertHasMessageError(header.US_Scientific2GenusNameInfo, messageError);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_Scientific2GenusName();
			AssertNoMessageError(header.US_Scientific2GenusNameInfo, messageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_Scientific2GenusName();
			AssertHasMessageError(header.US_Scientific2GenusNameInfo, messageError);
			header.US_Hybrid = ZString.Empty;
			AssertNoMessageError(header.US_Scientific2GenusNameInfo, messageError);
			header.US_Hybrid = FWSHybridTypeList.Codes.Interspecific;
			AssertHasMessageError(header.US_Scientific2GenusNameInfo, messageError);
		}

		public void TestCheckUS_Scientific2SpeciesName()
		{
			var header = InvoiceLine.FWSHeaders.AddNew();
			header.US_Hybrid = FWSHybridTypeList.Codes.Intergeneric;
			header.US_Scientific2SpeciesName = "!";
			header.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage("2nd Scientific Species Name");
			header.AddInfoValidation.ValidateUS_Scientific2SpeciesName();
			AssertNoMessageError(header.US_Scientific2SpeciesNameInfo, messageError);
			header.US_Scientific2SpeciesName = ZString.Empty;
			AssertHasMessageError(header.US_Scientific2SpeciesNameInfo, messageError);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_Scientific2SpeciesName();
			AssertNoMessageError(header.US_Scientific2SpeciesNameInfo, messageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_Scientific2SpeciesName();
			AssertHasMessageError(header.US_Scientific2SpeciesNameInfo, messageError);
			header.US_Hybrid = ZString.Empty;
			AssertNoMessageError(header.US_Scientific2SpeciesNameInfo, messageError);
		}

		public void TestCheckUS_WildlifeCategoryCode()
		{
			var header = InvoiceLine.FWSHeaders.AddNew();
			header.US_WildlifeCategoryCode = "!";
			AssertHasMessageErrorContaining(header.US_WildlifeCategoryCodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_WildlifeCategoryCode();
			AssertNoMessageErrors(header.US_WildlifeCategoryCodeInfo);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_WildlifeCategoryCode();
			AssertHasMessageErrorContaining(header.US_WildlifeCategoryCodeInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in new FWSWildlifeCategoryCodesList())
			{
				header.US_WildlifeCategoryCode = pair.Code;
				AssertNoMessageErrors(header.US_WildlifeCategoryCodeInfo);
			}
		}

		public void TestCheckUS_SpeciesOrigin()
		{
			var header = InvoiceLine.FWSHeaders.AddNew();
			header.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			header.US_SpeciesOrigin = ZString.Empty;
			header.AddInfoValidation.ValidateUS_SpeciesOrigin();
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage("Species Origin");
			AssertHasMessageError(header.US_SpeciesOriginInfo, messageError);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_SpeciesOrigin();
			AssertNoMessageErrors(header.US_SpeciesOriginInfo);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_SpeciesOrigin();
			AssertHasMessageError(header.US_SpeciesOriginInfo, messageError);
		}

		public void TestCheckUS_WildlifeSource()
		{
			var header = InvoiceLine.FWSHeaders.AddNew();
			header.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			header.AddInfoValidation.ValidateUS_WildlifeSource();
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage("Wildlife Source");
			AssertHasMessageError(header.US_WildlifeSourceInfo, messageError);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_WildlifeSource();
			AssertNoMessageErrors(header.US_WildlifeSourceInfo);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_WildlifeSource();
			AssertHasMessageError(header.US_WildlifeSourceInfo, messageError);
			foreach (ICodeDescription pair in FWSWildlifeSourceList.GetList(Factory))
			{
				header.US_WildlifeSource = pair.Code;
				AssertNoMessageErrors(pair.Code, header.US_WildlifeSourceInfo);
			}
			header.US_WildlifeSource = "!";
			AssertHasMessageErrorContaining(header.US_WildlifeSourceInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_Hybrid()
		{
			var header = InvoiceLine.FWSHeaders.AddNew();
			header.US_Hybrid = "~";
			AssertHasMessageErrorContaining(header.US_HybridInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in new FWSHybridTypeList())
			{
				header.US_Hybrid = pair.Code;
				AssertNoMessageErrors(header.US_HybridInfo);
			}
			header.US_Hybrid = ZString.Empty;
			AssertNoMessageErrors(header.US_HybridInfo);
		}

		public void TestCheckUS_CommodityGeneralName()
		{
			var header = InvoiceLine.FWSHeaders.AddNew();
			header.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage("Commodity General Name");
			header.AddInfoValidation.ValidateUS_CommodityGeneralName();
			AssertHasMessageError(header.US_CommodityGeneralNameInfo, messageError);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_CommodityGeneralName();
			AssertNoMessageErrors(header.US_CommodityGeneralNameInfo);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_CommodityGeneralName();
			AssertHasMessageError(header.US_CommodityGeneralNameInfo, messageError);
			header.US_CommodityGeneralName = "SD";
			AssertNoMessageErrors(header.US_CommodityGeneralNameInfo);
		}

		public void TestCheckUS_CartonQty()
		{
			var header = InvoiceLine.FWSHeaders.AddNew();
			header.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage("Carton Qty");
			header.AddInfoValidation.ValidateUS_CartonQty();
			AssertHasMessageError(header.US_CartonQtyInfo, messageError);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_CartonQty();
			AssertNoMessageErrors(header.US_CartonQtyInfo);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_CartonQty();
			AssertHasMessageError(header.US_CartonQtyInfo, messageError);
			header.US_CartonQty = 10;
			AssertNoMessageErrors(header.US_CartonQtyInfo);
		}

		public void TestCheckUS_WildlifeDescriptionCode()
		{
			var header = InvoiceLine.FWSHeaders.AddNew();
			header.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage("Wildlife Description Code");
			header.AddInfoValidation.ValidateUS_WildlifeDescriptionCode();
			AssertHasMessageError(header.US_WildlifeDescriptionCodeInfo, messageError);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_WildlifeDescriptionCode();
			AssertNoMessageErrors(header.US_WildlifeDescriptionCodeInfo);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_WildlifeDescriptionCode();
			AssertHasMessageError(header.US_WildlifeDescriptionCodeInfo, messageError);
			header.US_WildlifeDescriptionCode = "!";
			AssertHasMessageErrorContaining(header.US_WildlifeDescriptionCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_OA_FWSExporterAddress()
		{
			var header = InvoiceLine.FWSHeaders.AddNew();
			header.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			var messageError = ValidationConstants.FWS.FWSExporterAddressIsRequired;
			header.AddInfoValidation.ValidateUS_OA_FWSExporterAddress();
			AssertHasMessageError(header.US_OA_FWSExporterAddressInfo, messageError);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_OA_FWSExporterAddress();
			AssertNoMessageErrors(header.US_OA_FWSExporterAddressInfo);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_OA_FWSExporterAddress();
			AssertHasMessageError(header.US_OA_FWSExporterAddressInfo, messageError);

			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "USLAX";
			header.US_OA_FWSExporterAddress = org.MainAddress.PK;
			AssertNoMessageError(header.US_OA_FWSExporterAddressInfo, messageError);
			AssertHasMessageError(header.US_OA_FWSExporterAddressInfo, ValidationConstants.FWS.FWSExporterAddresMustNotBeUS);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_OA_FWSExporterAddress();
			AssertNoMessageErrors(header.US_OA_FWSExporterAddressInfo);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_OA_FWSExporterAddress();
			AssertNoMessageError(header.US_OA_FWSExporterAddressInfo, messageError);
			AssertHasMessageError(header.US_OA_FWSExporterAddressInfo, ValidationConstants.FWS.FWSExporterAddresMustNotBeUS);
			org.OH_RL_NKClosestPort = "AUSYD";
			header.AddInfoValidation.ValidateUS_OA_FWSExporterAddress();
			AssertNoMessageErrors(header.US_OA_FWSExporterAddressInfo);
		}

		public void TestCheckUS_OA_FWSImporterAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();

			var header = InvoiceLine.FWSHeaders.AddNew();
			header.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			var messageError = ValidationConstants.FWS.FWSImporterAddressIsRequired;
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertHasMessageError(header.US_OA_FWSImporterAddressInfo, messageError);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertNoMessageError(header.US_OA_FWSImporterAddressInfo, messageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertHasMessageError(header.US_OA_FWSImporterAddressInfo, messageError);
			header.US_ProcessingCode = ZString.Empty;
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertNoMessageError(header.US_OA_FWSImporterAddressInfo, messageError);

			header.US_OA_FWSImporterAddress = org.MainAddress.PK;
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertHasMessageError(header.US_OA_FWSImporterAddressInfo, ValidationConstants.FWS.FWEMustBeSetupAgainstImporterOrCompanyOrgProxyOrBranchOrgProxy);

			Declaration.Company.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.FWSeDecsAccountNumber, "FWE132123", Core.Constants.CountryCodes.UnitedStates);
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertNoMessageError(header.US_OA_FWSImporterAddressInfo, ValidationConstants.FWS.FWEMustBeSetupAgainstImporterOrCompanyOrgProxyOrBranchOrgProxy);
			Declaration.Company.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();

			AssertNoMessageError(header.US_OA_FWSImporterAddressInfo, messageError);
			AssertHasMessageError(header.US_OA_FWSImporterAddressInfo, ValidationConstants.FWS.FWSImporterAddresMustBeUS);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertNoMessageError(header.US_OA_FWSImporterAddressInfo, messageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertNoMessageError(header.US_OA_FWSImporterAddressInfo, messageError);
			AssertHasMessageError(header.US_OA_FWSImporterAddressInfo, ValidationConstants.FWS.FWSImporterAddresMustBeUS);
			org.OH_RL_NKClosestPort = "USLAX";
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertNoMessageError(header.US_OA_FWSImporterAddressInfo, messageError);

			AssertHasMessageError(header.US_OA_FWSImporterAddressInfo, ValidationConstants.FWS.FWEMustBeSetupAgainstImporterOrCompanyOrgProxyOrBranchOrgProxy);

			Declaration.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.FWSeDecsAccountNumber, "FWE132123", Core.Constants.CountryCodes.UnitedStates);
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertNoMessageError(header.US_OA_FWSImporterAddressInfo, ValidationConstants.FWS.FWEMustBeSetupAgainstImporterOrCompanyOrgProxyOrBranchOrgProxy);
			Declaration.Branch.OrgProxy.CustomsCodes.RemoveAndDeleteAll();

			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.FWSeDecsAccountNumber, "FWE132123", Core.Constants.CountryCodes.UnitedStates);
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertNoMessageError(header.US_OA_FWSImporterAddressInfo, ValidationConstants.FWS.FWEMustBeSetupAgainstImporterOrCompanyOrgProxyOrBranchOrgProxy);

			Declaration.Company.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			Declaration.Branch.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			org.CustomsCodes.RemoveAndDeleteAll();
			header = InvoiceLine.FWSHeaders.AddNew();
			header.US_ProcessingCode = FWSProcessingCodeList.Codes.LDS;
			header.US_OA_FWSImporterAddress = org.MainAddress.PK;

			org.OH_RL_NKClosestPort = "AUSYD";
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertNoMessageError(header.US_OA_FWSImporterAddressInfo, ValidationConstants.FWS.FWEMustBeSetupAgainstImporterOrCompanyOrgProxyOrBranchOrgProxy);

			org.OH_RL_NKClosestPort = "USLAX";
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertNoMessageError(header.US_OA_FWSImporterAddressInfo, ValidationConstants.FWS.FWEMustBeSetupAgainstImporterOrCompanyOrgProxyOrBranchOrgProxy);
		}

		public void TestCheckUS_Value()
		{
			var header = InvoiceLine.FWSHeaders.AddNew();
			header.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			var invoiceLine = header.InvoiceLine;
			invoiceLine.JI_LinePrice = 100.10m;
			var declaration = invoiceLine.Declaration;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			AssertEquals("ENS enabled", true, declaration.US_EnableENS);
			AssertEquals("CRL enabled", false, declaration.US_EnableCRL);

			declaration.US_EntryFilerCode = "XJ5";
			header.US_InvCurrPGAValue = 30;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			header.AddInfoValidation.ValidateUS_Value();
			AssertNoMessageErrors(header.US_ValueInfo);

			var invoice = invoiceLine.InvoiceHeader;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			header.US_InvCurrPGAValue = 130;
			declaration.DoMerge();
			header.AddInfoValidation.ValidateUS_Value();
			AssertNoMessageErrorContaining(header.US_ValueInfo, USImportFWSHeaderAddInfoValidation.FWSValuesShouldBeLessThanTenBillion);

			header.US_InvCurrPGAValue = 99999999999m;
			declaration.DoMerge();
			header.AddInfoValidation.ValidateUS_Value();
			AssertHasMessageErrorContaining(header.US_ValueInfo, USImportFWSHeaderAddInfoValidation.FWSValuesShouldBeLessThanTenBillion);
			AssertNoMessageErrorContaining(header.US_ValueInfo, USImportFWSHeaderAddInfoValidation.FWSValueCannotBeZero);

			header.US_InvCurrPGAValue = 0.48m;
			declaration.DoMerge();
			header.AddInfoValidation.ValidateUS_Value();
			AssertNoMessageErrorContaining(header.US_ValueInfo, USImportFWSHeaderAddInfoValidation.FWSValuesShouldBeLessThanTenBillion);
			AssertHasMessageErrorContaining(header.US_ValueInfo, USImportFWSHeaderAddInfoValidation.FWSValueCannotBeZero);
		}

		public void TestCheckUS_InvCurrPGAValue()
		{
			InvoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
			var fwsHeader = InvoiceLine.FWSHeaders.AddNew();
			fwsHeader.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			var invoiceLine = fwsHeader.InvoiceLine;
			invoiceLine.JI_LinePrice = 100m;
			var declaration = invoiceLine.Declaration;
			declaration.US_EnableENS = true;
			var exceededMessageError = "The total of Inv. Curr. Value (200) from FWS data should be less than the line price (100) (including child lines).";

			fwsHeader.US_InvCurrPGAValue = 0m;
			AssertHasMessageErrorContaining(fwsHeader.US_InvCurrPGAValueInfo, ValidationConstants.FDA.OGAInvValue);

			fwsHeader.US_ProcessingCode = FWSProcessingCodeList.Codes.LDS;
			fwsHeader.AddInfoValidation.ValidateUS_InvCurrPGAValue();
			AssertNoMessageErrorContaining(fwsHeader.US_InvCurrPGAValueInfo, ValidationConstants.FDA.OGAInvValue);

			fwsHeader.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			fwsHeader.US_InvCurrPGAValue = 200m;
			invoiceLine.Validation.ValidateJI_LinePrice();
			AssertHasMessageErrorContaining(invoiceLine.JI_LinePriceInfo, exceededMessageError);

			fwsHeader.US_ProcessingCode = FWSProcessingCodeList.Codes.LDS;
			invoiceLine.Validation.ValidateJI_LinePrice();
			AssertNoMessageErrorContaining(invoiceLine.JI_LinePriceInfo, exceededMessageError);

			fwsHeader.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			fwsHeader.US_InvCurrPGAValue = -1m;
			AssertHasMessageErrorContaining(fwsHeader.US_InvCurrPGAValueInfo, ValidationConstants.NegativeAmountNotAllowed);

			fwsHeader.US_ProcessingCode = FWSProcessingCodeList.Codes.LDS;
			fwsHeader.AddInfoValidation.ValidateUS_InvCurrPGAValue();
			AssertNoMessageErrorContaining(fwsHeader.US_InvCurrPGAValueInfo, ValidationConstants.NegativeAmountNotAllowed);
		}

		public void TestCheckUS_NetCommodity()
		{
			var header = InvoiceLine.FWSHeaders.AddNew();
			header.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage("Net Commodity");
			header.AddInfoValidation.ValidateUS_NetCommodity();
			AssertHasMessageError(header.US_NetCommodityInfo, messageError);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_NetCommodity();
			AssertNoMessageErrors(header.US_NetCommodityInfo);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_NetCommodity();
			AssertHasMessageError(header.US_NetCommodityInfo, messageError);
			header.US_NetCommodity = 100m;
			AssertNoMessageErrors(header.US_NetCommodityInfo);
		}

		public void TestCheckUS_NetCommodityUQ()
		{
			var header = InvoiceLine.FWSHeaders.AddNew();
			header.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			header.US_NetCommodity = 100m;
			header.AddInfoValidation.ValidateUS_NetCommodityUQ();
			AssertHasMessageErrorContaining(header.US_NetCommodityUQInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_NetCommodityUQ();
			AssertNoMessageErrors(header.US_NetCommodityUQInfo);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_NetCommodityUQ();
			AssertHasMessageErrorContaining(header.US_NetCommodityUQInfo, MandatoryValidation.YouHaveNotEntered);
			header.US_NetCommodityUQ = "!";
			AssertNoMessageErrorContaining(header.US_NetCommodityUQInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(header.US_NetCommodityUQInfo, ListValidation.InvalidCodeMessageError);
			foreach (ICodeDescription pair in new FWSUnitOfMeasureList())
			{
				header.US_NetCommodityUQ = pair.Code;
				AssertNoMessageErrors(header.US_NetCommodityUQInfo);
			}
		}

		public void TestCheckUS_FIRMS()
		{
			var firmsHelper = new UniversalReferenceTestDataHelper(Factory);
			firmsHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			firmsHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "H99!", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var header = InvoiceLine.FWSHeaders.AddNew();
			header.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage("FIRMS");
			header.AddInfoValidation.ValidateUS_FIRMS();
			AssertHasMessageError(header.US_FIRMSInfo, messageError);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_FIRMS();
			AssertNoMessageErrors(header.US_FIRMSInfo);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_FIRMS();
			AssertHasMessageError(header.US_FIRMSInfo, messageError);
			header.US_FIRMS = "S!";
			AssertNoMessageError(header.US_FIRMSInfo, messageError);
			AssertHasMessageErrorContaining(header.US_FIRMSInfo, ListValidation.InvalidCodeMessageError);
			header.US_FIRMS = "H99!";
			AssertNoMessageErrors(header.US_FIRMSInfo);
		}

		public void TestCheckContactName()
		{
			var header = InvoiceLine.FWSHeaders.AddNew();
			header.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			header.US_OA_FWSImporterAddress = org.MainAddress.PK;
			var messageError = string.Format(OrganisationValidation.ContactInformationForCustomsIsMissing, "Name");
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertHasMessageErrorContaining(header.US_OA_FWSImporterAddressInfo, messageError);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertNoMessageErrorContaining(header.US_OA_FWSImporterAddressInfo, messageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertHasMessageErrorContaining(header.US_OA_FWSImporterAddressInfo, messageError);
			header.US_ProcessingCode = ZString.Empty;
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertNoMessageErrorContaining(header.US_OA_FWSImporterAddressInfo, messageError);
		}

		public void TestCheckContactPhoneNo()
		{
			var header = InvoiceLine.FWSHeaders.AddNew();
			header.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			header.US_OA_FWSImporterAddress = org.MainAddress.PK;
			var messageError = string.Format(OrganisationValidation.ContactInformationForCustomsIsMissing, "Work Phone");
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertHasMessageErrorContaining(header.US_OA_FWSImporterAddressInfo, messageError);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertNoMessageErrorContaining(header.US_OA_FWSImporterAddressInfo, messageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertHasMessageErrorContaining(header.US_OA_FWSImporterAddressInfo, messageError);
			header.US_ProcessingCode = ZString.Empty;
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertNoMessageErrorContaining(header.US_OA_FWSImporterAddressInfo, messageError);
		}

		public void TestCheckContactEmail()
		{
			var header = InvoiceLine.FWSHeaders.AddNew();
			header.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			header.US_OA_FWSImporterAddress = org.MainAddress.PK;
			var messageError = string.Format(OrganisationValidation.ContactInformationForCustomsIsMissing, "Email/Fax");
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertHasMessageErrorContaining(header.US_OA_FWSImporterAddressInfo, messageError);
			Declaration.ValidationModes = ValidationModes.None;
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertNoMessageErrorContaining(header.US_OA_FWSImporterAddressInfo, messageError);
			Declaration.RecalculateValidationModesOnDeclaration();
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertHasMessageErrorContaining(header.US_OA_FWSImporterAddressInfo, messageError);
			header.US_ProcessingCode = ZString.Empty;
			header.AddInfoValidation.ValidateUS_OA_FWSImporterAddress();
			AssertNoMessageErrorContaining(header.US_OA_FWSImporterAddressInfo, messageError);
		}

		#region Implementation
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		JobComInvoiceHeader Invoice
		{
			get { return invoice ?? (invoice = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoice;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Invoice.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;
		#endregion
	}
}
