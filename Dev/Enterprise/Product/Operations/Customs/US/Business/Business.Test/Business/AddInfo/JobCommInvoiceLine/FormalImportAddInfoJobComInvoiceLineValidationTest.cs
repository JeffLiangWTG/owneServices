using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FormalImportAddInfoJobComInvoiceLineValidationTest : CommonImportAddInfoComInvoiceLineValidationTest
	{
		public void TestCheckUS_TaxRate()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "22000000";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.CompoundSpecificAndAdValoremFirstQuantity;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_Column1RateSpecific = 0.44m;
			invoiceLine.US_UC_NKCountryOfOrigin = "TT";
			invoiceLine.JI_LinePrice = 10000.20m;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_CustomsQuantity = 2.523m;
			invoiceLine.US_TaxApply = "O";
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Specify;
			invoiceLine.US_TaxRate = 0.25m;
			AssertNoMessageError(invoiceLine.US_TaxRateInfo, USAddInfoValidation.TaxRateShouldBeEntered);
			invoiceLine.US_TaxRate = 0m;
			AssertHasMessageError(invoiceLine.US_TaxRateInfo, USAddInfoValidation.TaxRateShouldBeEntered);
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.CBMAEligible;
			invoiceLine.US_TaxRate = 0.2544m;
			AssertNoMessageError(invoiceLine.US_TaxRateInfo, USAddInfoValidation.TaxRateShouldBeEntered);
			invoiceLine.US_TaxRate = 0m;
			AssertHasMessageError(invoiceLine.US_TaxRateInfo, USAddInfoValidation.TaxRateShouldBeEntered);
		}

		public void TestCheckUS_TaxRateS_ProductClaimC()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.CBMAEligible;
			AssertHasMessageErrorContaining(invoiceLine.US_TaxRateSInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			invoiceLine.AddInfoValidation.ValidateUS_TaxRateS();
			AssertNoMessageErrorContaining(invoiceLine.US_TaxRateSInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_TTBRateDesignationCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Wines_3;
			AssertNoMessageErrorContaining(invoiceLine.US_TTBRateDesignationCodeInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.US_TTBRateDesignationCode = "W01010";
			invoiceLine.AddInfoValidation.ValidateUS_TTBRateDesignationCode();
			AssertNoMessageErrorContaining(invoiceLine.US_TTBRateDesignationCodeInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			invoiceLine.US_TTBRateDesignationCode = "W01010";
			invoiceLine.AddInfoValidation.ValidateUS_TTBRateDesignationCode();
			AssertHasMessageErrorContaining(invoiceLine.US_TTBRateDesignationCodeInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.US_TTBRateDesignationCode = "W02010";
			invoiceLine.AddInfoValidation.ValidateUS_TTBRateDesignationCode();
			AssertNoMessageErrorContaining(invoiceLine.US_TaxRateSInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestPortIsExceptFromIRTaxesNWhenPREntryPort()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "89089019";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeFlag = "1";

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4915", "Test PR Port", startDate, endDate);
			var attributeNameState = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.State, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			var attributeState = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameState.ZXE_Name, USStateList.Codes.PuertoRico);
			newFactory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_SchDEntry = "4915";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "89089019";
			AssertEquals(TaxApplyList.Codes.No, invoiceLine.US_TaxApply);
			invoiceLine.JI_Description = "Goods";
			AssertEquals("Should default 'N'", "N", invoiceLine.US_TaxApply);
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			AssertHasMessageError("Entry Port is in PR", invoiceLine.US_TaxApplyInfo, FormalImportAddInfoJobComInvoiceLineValidation.PortIsExceptFromIRTaxes);
			invoiceLine.US_TaxApply = TaxApplyList.Codes.No;
			AssertNoMessageError("Entry Port is in PR", invoiceLine.US_TaxApplyInfo, FormalImportAddInfoJobComInvoiceLineValidation.PortIsExceptFromIRTaxes);
			invoiceLine.US_TaxApply = ZString.Empty;
			AssertNoMessageError("Entry Port is in PR", invoiceLine.US_TaxApplyInfo, FormalImportAddInfoJobComInvoiceLineValidation.PortIsExceptFromIRTaxes);
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			AssertHasMessageError("Entry Port is in PR", invoiceLine.US_TaxApplyInfo, FormalImportAddInfoJobComInvoiceLineValidation.PortIsExceptFromIRTaxes);
			invoiceLine.JI_Tariff = "";
			declaration.US_SchDEntry = "3901";
			invoiceLine.JI_Tariff = "89089019";
			AssertNoMessageError("Entry Port is in PR", invoiceLine.US_TaxApplyInfo, FormalImportAddInfoJobComInvoiceLineValidation.PortIsExceptFromIRTaxes);
		}

		public void TestInnermostPackageUQOnTheFrontTabForFTZWeeklyEstimateIntegrationEnabled()
		{
			var importer = Factory.New<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var orgAddress1 = importer.Addresses.AddNew();
			var cusCode1 = importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "A000", "US");
			cusCode1.OK_OA_PremisesAddress = orgAddress1.PK;
			var warehouseOrg = Factory.New<OrgHeader>();
			warehouseOrg.OH_IsWarehouseClient = true;
			var orgAddress2 = warehouseOrg.Addresses.AddNew();
			invoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.Declaration.US_EnableCRL = true;
			invoiceLine.Declaration.US_EntryDateElectionCode = ZString.Empty;
			invoiceLine.Declaration.JE_OH_Importer = importer.PK;
			invoiceLine.Declaration.WarehouseDocAddress.E2_OA_Address = orgAddress2.PK;
			invoiceLine.US_ManifestQty = 57;
			invoiceLine.US_ManifestUQ = "~";
			AssertHasMessageError(invoiceLine.US_ManifestUQInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_ManifestUQ = "BAG";
			AssertNoMessageError(invoiceLine.US_ManifestUQInfo, ListValidation.InvalidCodeMessageError);
			importer.CompanyData.OB_IMUsedBondedWhs = false;
			invoiceLine.US_ManifestUQ = "~";
			AssertHasWarning(invoiceLine.US_ManifestUQInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.US_ManifestUQ = "*";
			AssertNoWarning(invoiceLine.US_ManifestUQInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_ADDCVDCaseNo()
		{
			var addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "ADD111";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase.U5_CaseStatusDate = ZDateTime.Today;
			addCase.U5_ISOCountryCode = "CA";
			addCase.CaseTariffs.AddNew().U9_TariffNumber = "22030060";
			var cvdCase = Factory.New<USCACCase>();
			cvdCase.U5_CaseNumber = "CVD111";
			cvdCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			cvdCase.U5_CaseStatusDate = ZDateTime.Today;
			cvdCase.U5_ISOCountryCode = "CA";
			cvdCase.CaseTariffs.AddNew().U9_TariffNumber = "22030060";
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.US_ADDCaseNo = "ADD111";
			invoiceLine.US_CVDCaseNo = "CVD111";
			AssertNotNull("PreCondition(AntidumpingDutyCase_ACE)", invoiceLine.AntidumpingDutyCase);
			AssertNotNull("PreCondition(CountervailingDutyCase_ACE)", invoiceLine.CountervailingDutyCase);
			AssertHasMessageErrorContaining(invoiceLine.US_ADDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.CaseNoAgainstCountryOfOriginPartialMessage);
			AssertHasMessageErrorContaining(invoiceLine.US_CVDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.CaseNoAgainstCountryOfOriginPartialMessage);
			invoiceLine.US_UC_NKCountryOfOrigin = "XC";
			AssertNoMessageErrorContaining(invoiceLine.US_ADDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.CaseNoAgainstCountryOfOriginPartialMessage);
			AssertNoMessageErrorContaining(invoiceLine.US_CVDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.CaseNoAgainstCountryOfOriginPartialMessage);
		}

		public void TestAddInfoWhenSeverityLevelOfInvoiceLineValidationERRWRN()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.FillWithValidTestData();
			consignor.OH_Code = "ORGAUSYD";
			consignor.OH_IsConsignor = ZBool.True;
			consignor.OH_RL_NKClosestPort = "AUSYD";
			var consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.OH_Code = "ORGUSCHI";
			consignee.OH_IsConsignee = ZBool.True;
			consignee.OH_RL_NKClosestPort = "USCHI";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "WEIGHTTEST";
			product.OP_Desc = "WEIGHTTEST";
			product.RelatedOrganisations.AddOwner(consignee);
			product.OP_Weight = 10;
			product.OP_WeightUQ = Core.Constants.Weight.Kilograms;
			product.OP_NetWeight = 9;
			product.OP_StockKeepingUnit = "UNT";

			var supplierRelation = product.RelatedOrganisations.AddSupplier(consignor);
			product.RelatedOrganisations.AddOwner(consignee);

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "9101000010";
			pivot.CI_SupplementalTariff = "9101000011";
			pivot.CD_PerUnitCost = 0m;
			pivot.CD_RX_NKPerUnitCostCurr = "USD";
			pivot.CD_ADDApplicable = true;
			pivot.CD_ADDCaseNo = "1122";
			pivot.CD_CVDApplicable = true;
			pivot.CD_CVDCaseNo = "3344";
			pivot.CD_UC_NKCountryOfOrigin = "HK";
			pivot.CD_SPI = "Y";
			AssertEquals(true, pivot.CD_ADDApplicable);

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice1 = declaration1.Invoices.AddNew();
			invoice1.JZ_OH_Buyer = consignee.PK;
			invoice1.JZ_OH_Supplier = consignor.PK;

			var invoiceLine1 = invoice1.InvoiceLines.AddNew();

			var msgSPI = "SPI does not match product code file.";
			var msgCOO = "Country Of Origin does not match product code file.";
			var msgADDCaseNo = "ADD Case No does not match product code file.";
			var msgCVDCaseNo = "CVD Case No does not match product code file.";
			var msgADDApplicable = "US_ADD_NA does not match product code file.";

			invoiceLine1.JI_PartNo = product.OP_PartNum;
			AssertNotNull(invoiceLine1.Part);
			using (CustomsDataRegistry.Instance.SeverityLevelOfInvoiceLineValidationAgainstProductData.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, "WRN"))
			{
				invoiceLine1.US_ADD_NA = false;
				AssertHasWarning(invoiceLine1.US_ADD_NAInfo, msgADDApplicable);
				invoiceLine1.US_ADD_NA = true;
				AssertNoWarning(invoiceLine1.US_ADD_NAInfo, msgADDApplicable);

				invoiceLine1.US_SPI = "N";
				invoiceLine1.US_UC_NKCountryOfOrigin = "FR";
				AssertHasWarning(invoiceLine1.US_SPIInfo, msgSPI);
				AssertHasWarning(invoiceLine1.US_UC_NKCountryOfOriginInfo, msgCOO);

				invoiceLine1.US_SPI = "Y";
				invoiceLine1.US_UC_NKCountryOfOrigin = "HK";
				AssertNoWarning(invoiceLine1.US_SPIInfo, msgSPI);
				AssertNoWarning(invoiceLine1.US_UC_NKCountryOfOriginInfo, msgCOO);

				invoiceLine1.US_ADDCaseNo = "";
				AssertHasWarning(invoiceLine1.US_ADDCaseNoInfo, msgADDCaseNo);
				invoiceLine1.US_CVDCaseNo = "";
				AssertHasWarning(invoiceLine1.US_CVDCaseNoInfo, msgCVDCaseNo);

				invoiceLine1.US_ADDCaseNo = "1122";
				invoiceLine1.US_CVDCaseNo = "3344";
				AssertNoWarning(invoiceLine1.US_ADDCaseNoInfo, msgADDCaseNo);
				AssertNoWarning(invoiceLine1.US_CVDCaseNoInfo, msgCVDCaseNo);
			}

			using (CustomsDataRegistry.Instance.SeverityLevelOfInvoiceLineValidationAgainstProductData.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, "ERR"))
			{
				invoiceLine1.US_ADD_NA = false;
				AssertHasMessageError(invoiceLine1.US_ADD_NAInfo, msgADDApplicable);
				invoiceLine1.US_ADD_NA = true;
				AssertNoMessageError(invoiceLine1.US_ADD_NAInfo, msgADDApplicable);

				invoiceLine1.US_SPI = "N";
				invoiceLine1.US_UC_NKCountryOfOrigin = "FR";
				AssertHasMessageError(invoiceLine1.US_SPIInfo, msgSPI);
				AssertHasMessageError(invoiceLine1.US_UC_NKCountryOfOriginInfo, msgCOO);

				invoiceLine1.US_SPI = "Y";
				invoiceLine1.US_UC_NKCountryOfOrigin = "HK";
				AssertNoMessageError(invoiceLine1.US_SPIInfo, msgSPI);
				AssertNoMessageError(invoiceLine1.US_UC_NKCountryOfOriginInfo, msgCOO);

				invoiceLine1.US_ADDCaseNo = "";
				AssertHasMessageError(invoiceLine1.US_ADDCaseNoInfo, msgADDCaseNo);
				invoiceLine1.US_CVDCaseNo = "";
				AssertHasMessageError(invoiceLine1.US_CVDCaseNoInfo, msgCVDCaseNo);

				invoiceLine1.US_ADDCaseNo = "1122";
				invoiceLine1.US_CVDCaseNo = "3344";
				AssertNoMessageError(invoiceLine1.US_ADDCaseNoInfo, msgADDCaseNo);
				AssertNoMessageError(invoiceLine1.US_CVDCaseNoInfo, msgCVDCaseNo);
			}
		}

		public void TestCheckUS_TSCAODSCertIndividual()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableCRL = true;
			declaration.US_EnableENS = false;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCAODSCertIndividual = "XZ";
			AssertHasMessageErrorContaining(invoiceLine.US_TSCAODSCertIndividualInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_TSCAODSCertIndividual = PartyTypeList.Codes.Importer;
			AssertNoMessageErrorContaining(invoiceLine.US_TSCAODSCertIndividualInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(invoiceLine.US_TSCAODSCertIndividualInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.US_TSCAODSCertIndividual = "";
			AssertHasMessageErrorContaining(invoiceLine.US_TSCAODSCertIndividualInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_TSCACertification()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableCRL = true;
			declaration.US_EnableENS = false;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCACertification = "Y";
			AssertHasMessageErrorContaining(invoiceLine.US_TSCACertificationInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_TSCACertification = TSCAIndicatorList.Codes.TSCANegative;
			AssertNoMessageErrorContaining(invoiceLine.US_TSCACertificationInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_TSCACertification = TSCAIndicatorList.Codes.TSCAPositive;
			AssertNoMessageErrorContaining(invoiceLine.US_TSCACertificationInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_TSCACertification = string.Empty;
			AssertHasMessageErrorContaining(invoiceLine.US_TSCACertificationInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_CottonCertificateNoWithCOTDocument()
		{
			var startDate = ZDateTime.Today.AddMonths(-1);
			var endDate = ZDateTime.Today.AddMonths(1);
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "3333333333";
			tariff.UE_DateFrom = startDate;
			tariff.UE_DateTo = endDate;

			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Cotton;
			dutyRate.UD_TaxFeeFlag = "1";

			var declaration = new DeclarationTestHelper().CreateSimpleImportDeclaration(Factory);
			var importer = Factory.New<OrgHeader>();
			var nafDoc = importer.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.Cotton);
			declaration.IOROrgPK = importer.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.DoMerge();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			nafDoc.EQ_ValidToDate = ZDateTime.Now.AddDays(10);
			nafDoc.EQ_DocNumber = "123456";
			invoiceLine.US_CottonCertificateNo = nafDoc.EQ_DocNumber;
			AssertNoMessageError("The cotton certificate is valid", invoiceLine.US_CottonCertificateNoInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckCottonCertificate);
			nafDoc.EQ_ValidToDate = ZDateTime.Now.AddDays(-10);
			nafDoc.EQ_DocNumber = "789021";
			invoiceLine.US_CottonCertificateNo = "123";
			AssertNoMessageError("The cotton certificate number does't match document number", invoiceLine.US_CottonCertificateNoInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckCottonCertificate);
			invoiceLine.US_CottonCertificateNo = nafDoc.EQ_DocNumber;
			AssertHasMessageError("The cotton certificate is not valid", invoiceLine.US_CottonCertificateNoInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckCottonCertificate);
			nafDoc.EQ_DocNumber = "abc12345";
			invoiceLine.US_CottonCertificateNo = "ABC12345";
			AssertHasMessageError("The cotton certificate is not valid", invoiceLine.US_CottonCertificateNoInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckCottonCertificate);
		}

		public void TestCheckUS_ZoneStatusForACECargoRelease()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableCRL = true;
			declaration.US_EnableENS = false;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_ZoneStatus = "";
			AssertHasMessageErrorContaining(invoiceLine.US_ZoneStatusInfo, FormalImportAddInfoJobComInvoiceLineValidation.ZoneStatusShouldBeEntered);
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			AssertNoMessageError(invoiceLine.US_ZoneStatusInfo, FormalImportAddInfoJobComInvoiceLineValidation.ZoneStatusShouldBeEntered);
		}

		public void TestCheckUS_DateOfExport()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_QuotaIndicator = true;
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			dec.US_EnableENS = true;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = dec.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000000000";
			invoiceLine.US_DateOfExport = ZDateTime.Today;
			invoiceLine.US_DateOfExport = ZDateTime.Empty;
			AssertHasWarningContaining(invoiceLine.US_DateOfExportInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.US_DateOfExport = ZDateTime.Today;
			AssertNoWarningContaining(invoiceLine.US_DateOfExportInfo, MandatoryValidation.YouHaveNotEntered);
			dec.JE_DateOfArrival = ZDateTime.Today;
			invoiceLine.US_DateOfExport = ZDateTime.Today.AddDays(1);
			AssertHasMessageErrorContaining(invoiceLine.US_DateOfExportInfo, "Date of Arrival can not be before the Export Date");
			dec.JE_DateOfArrival = ZDateTime.Empty;
			invoiceLine.US_DateOfExport = ZDateTime.Empty;
			invoiceLine.US_DateOfExport = ZDateTime.Today.AddDays(1);
			AssertNoMessageErrors(invoiceLine.US_DateOfExportInfo);
		}

		public void TestCheckUS_SupTariff()
		{
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "0910200000";
			AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, FormalImportAddInfoJobComInvoiceLineValidation.TIBEntryMustUseChapter9813Tariffs);
			invoiceLine.US_SupTariff = "9813000520";
			AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, FormalImportAddInfoJobComInvoiceLineValidation.TIBEntryMustUseChapter9813Tariffs);
			invoiceLine.US_SupTariff = "0404100500";
			AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, FormalImportAddInfoJobComInvoiceLineValidation.TIBEntryMustUseChapter9813Tariffs);
			invoiceLine.US_SupTariff = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.US_SupTariffInfo, FormalImportAddInfoJobComInvoiceLineValidation.EmptyTIBTariff);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.US_SupTariff = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.US_SupTariffInfo, FormalImportAddInfoJobComInvoiceLineValidation.EmptyTIBTariff);
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceLine1.AddSecondaryInvoiceLine();
			invoiceLine1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			AssertEquals("PreCondition", invoiceLine1, invoiceLine2.ParentTariffLine);
			invoiceLine2.US_SupTariff = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine2.US_SupTariffInfo, FormalImportAddInfoJobComInvoiceLineValidation.EmptyTIBTariff);
			var testHelper = new Chapter98HelperTest();
			testHelper.ParentLine.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;
			testHelper.ChildLine.US_SupTariff = testHelper.Test9813Tariff.UE_Tariff;
			testHelper.Charpter98Job.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			testHelper.ParentLine.AddInfoValidation.ValidateUS_SupTariff();
			AssertNoMessageError(testHelper.ParentLine.US_SupTariffInfo, FormalImportAddInfoJobComInvoiceLineValidation.TIBEntryMustUseChapter9813Tariffs);
		}

		public void TestAdviseToSelectOverrideAndIndicateTaxCodeAndRate()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.US_EnableENS = true;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = dec.InvoiceLines.AddNew();
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			invoiceLine.AddInfoValidation.ValidateUS_TaxApply();
			AssertHasMessageError(invoiceLine.US_TaxApplyInfo, FormalImportAddInfoJobComInvoiceLineValidation.SelectOverrideAndIndicateTaxCode);
			invoiceLine.US_TaxCode = "016";
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			invoiceLine.AddInfoValidation.ValidateUS_TaxApply();
			AssertHasMessageError(invoiceLine.US_TaxApplyInfo, FormalImportAddInfoJobComInvoiceLineValidation.SelectOverrideAndIndicateTaxRate);
			dec.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.BulkLiquorDeferred;
			invoiceLine.JI_Tariff = "2203.00.0090";
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			invoiceLine.AddInfoValidation.ValidateUS_TaxApply();
			AssertHasMessageError(invoiceLine.US_TaxApplyInfo, FormalImportAddInfoJobComInvoiceLineValidation.BulkLiquorNoTaxApply);
			invoiceLine.US_TaxApply = TaxApplyList.Codes.No;
			invoiceLine.AddInfoValidation.ValidateUS_TaxApply();
			AssertNoMessageError(invoiceLine.US_TaxApplyInfo, FormalImportAddInfoJobComInvoiceLineValidation.BulkLiquorNoTaxApply);
		}

		public void TestValidateCustomsValueFor9802Tariff()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.US_EnableENS = true;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = dec.InvoiceLines.AddNew();
			var parentInvoiceLine = dec.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "7412200025";
			invoiceLine.US_SupTariff = "9802005060";
			Factory.Save();
			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertHasMessageErrorContaining(invoiceLine.US_98GoodsValueInfo, ValidationConstants.EntrySummary.MinimumAssemblyOperationCostFor9802Products);
			AssertHasMessageErrorContaining(invoiceLine.US_98ValueInvCurrInfo, ValidationConstants.EntrySummary.MinimumAssemblyOperationCostFor9802Products);
			AssertHasMessageErrorContaining(invoiceLine.JI_LinePriceInfo, ValidationConstants.EntrySummary.MinimumAssemblyOperationCostFor9802Products);
			invoiceLine.JI_LinePrice = 1m;
			invoiceLine.US_98GoodsValue = 1m;
			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNoMessageErrorContaining(invoiceLine.US_98GoodsValueInfo, ValidationConstants.EntrySummary.MinimumAssemblyOperationCostFor9802Products);
			AssertNoMessageErrorContaining(invoiceLine.US_98ValueInvCurrInfo, ValidationConstants.EntrySummary.MinimumAssemblyOperationCostFor9802Products);
			AssertNoMessageErrorContaining(invoiceLine.JI_LinePriceInfo, ValidationConstants.EntrySummary.MinimumAssemblyOperationCostFor9802Products);
			invoiceLine.JI_LinePrice = 0.3m;
			invoiceLine.US_98GoodsValue = 9999.70m;
			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertHasMessageErrorContaining(invoiceLine.JI_LinePriceInfo, ValidationConstants.EntrySummary.MinimumAssemblyOperationCostFor9802Products);
			invoiceLine.JI_LinePrice = 1m;
			invoiceLine.US_98GoodsValue = 0.3m;
			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertHasMessageErrorContaining(invoiceLine.US_98GoodsValueInfo, ValidationConstants.EntrySummary.MinimumAssemblyOperationCostFor9802Products);
			AssertHasMessageErrorContaining(invoiceLine.US_98ValueInvCurrInfo, ValidationConstants.EntrySummary.MinimumAssemblyOperationCostFor9802Products);
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.JI_ParentID = parentInvoiceLine.PK;
			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNoMessageErrorContaining(invoiceLine.JI_LinePriceInfo, ValidationConstants.EntrySummary.MinimumAssemblyOperationCostFor9802Products);
		}

		public void TestCheckUS_CottonCertificateNo()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_CottonCertificateNo = "1212";
			AssertHasMessageError(invoiceLine.US_CottonCertificateNoInfo, string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.LicenseAndPermit));
		}

		public void TestCheckUS_LumberExportCharges()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_LumberExportCharges = 1m;
			AssertHasMessageError(invoiceLine.US_LumberExportChargesInfo, string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, FormalImportAddInfoJobComInvoiceLineValidation.Constants.OGA.SoftwoodLumber));
		}

		[TestDate(2010, 08, 05)]
		public void TestTaxCodeAndRateValidation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2403.10.2050";
			AssertEquals("", invoiceLine.US_TaxCode);
			AssertNoMessageErrors(invoiceLine.US_TaxCodeInfo);
			AssertEquals(true, invoiceLine.US_TaxCodeInfo.ReadOnly);
			AssertEquals("", invoiceLine.US_TaxRateS);
			AssertNoMessageErrors(invoiceLine.US_TaxRateSInfo);
			AssertEquals(true, invoiceLine.US_TaxRateSInfo.ReadOnly);
			invoiceLine.US_TaxApply = TaxApplyList.Codes.No;
			AssertEquals("", invoiceLine.US_TaxCode);
			AssertNoMessageErrors(invoiceLine.US_TaxCodeInfo);
			AssertEquals(true, invoiceLine.US_TaxCodeInfo.ReadOnly);
			AssertEquals("", invoiceLine.US_TaxRateS);
			AssertNoMessageErrors(invoiceLine.US_TaxRateSInfo);
			AssertEquals(true, invoiceLine.US_TaxRateSInfo.ReadOnly);
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			AssertEquals("Code should default from Tariff", "018", invoiceLine.US_TaxCode);
			AssertNoMessageErrors(invoiceLine.US_TaxCodeInfo);
			AssertEquals("Code remains read only", false, invoiceLine.US_TaxCodeInfo.ReadOnly);
			AssertEquals("Rate should default from Tariff", "$2.41822574/KG", invoiceLine.US_TaxRateS);
			AssertNoMessageErrors(invoiceLine.US_TaxRateSInfo);
			AssertEquals("Rate remains read only", false, invoiceLine.US_TaxRateSInfo.ReadOnly);
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.AddInfoValidation.ValidateUS_TaxApply();
			AssertNoMessageErrors(invoiceLine.US_TaxCodeInfo);
			AssertEquals("Code should now be enterable", false, invoiceLine.US_TaxCodeInfo.ReadOnly);
			invoiceLine.AddInfoValidation.ValidateUS_TaxRateS();
			AssertNoMessageErrors(invoiceLine.US_TaxRateSInfo);
			AssertEquals("Rate should now be enterable", false, invoiceLine.US_TaxRateSInfo.ReadOnly);
			invoiceLine.JI_Tariff = "1901.10.0500";
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.AddInfoValidation.ValidateUS_TaxCode();
			AssertHasMessageErrorContaining(invoiceLine.US_TaxCodeInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.US_TaxRateS = "";
			invoiceLine.AddInfoValidation.ValidateUS_TaxRateS();
			AssertHasMessageErrorContaining(invoiceLine.US_TaxRateSInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			AssertEquals("Code should be blank", "", invoiceLine.US_TaxCode);
			invoiceLine.AddInfoValidation.ValidateUS_TaxCode();
			AssertNoMessageErrorContaining(invoiceLine.US_TaxCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertEquals("Code remains read only", false, invoiceLine.US_TaxCodeInfo.ReadOnly);
			AssertEquals("Rate should be blank", "", invoiceLine.US_TaxRateS);
			AssertEquals("Rate remains read only", false, invoiceLine.US_TaxRateSInfo.ReadOnly);
		}

		public void TestCheckUS_TaxRateQuantity()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = "K";
			invoiceLine.JI_CustomsSecondUnitQty = "";
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxCode = "016";
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Wines_1; //"15.3389c/L";
			AssertHasMessageError(invoiceLine.US_TaxQtyInfo, string.Format(USAddInfoValidation.UnitOfQuantityDoesNotMatch, "L"));
			invoiceLine.US_TaxQty = 120m;
			AssertNoMessageError(invoiceLine.US_TaxQtyInfo, string.Format(USAddInfoValidation.UnitOfQuantityDoesNotMatch, "L"));
		}

		[TestDate(2008, 9, 11)]
		public void TestCottonFeeExemptWhenOrganicCertificateNoIsEntered()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6104220040";
			Assert("PreCondition", invoiceLine.ImportTariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Cotton));
			invoiceLine.US_CottonFeeExempt = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.US_CottonFeeExemptInfo, FormalImportAddInfoJobComInvoiceLineValidation.CottonFeeApplicable);
			invoiceLine.US_CottonCertificateNo = "007894812";
			invoiceLine.US_CottonFeeExempt = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.US_CottonFeeExemptInfo, FormalImportAddInfoJobComInvoiceLineValidation.CottonFeeApplicable);
		}

		[TestDate(2008, 9, 11)]
		public void TestCottonFeeExemptWhenOrganicCertificateNoIsEnteredForACE()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6104220040";
			Assert("PreCondition", invoiceLine.ImportTariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Cotton));
			invoiceLine.US_CottonFeeExempt = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.US_CottonFeeExemptInfo, FormalImportAddInfoJobComInvoiceLineValidation.CottonFeeApplicable);
			invoiceLine.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._12, "007894812");
			invoiceLine.US_CottonFeeExempt = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.US_CottonFeeExemptInfo, FormalImportAddInfoJobComInvoiceLineValidation.CottonFeeApplicable);
		}

		public void TestADD_CVDCaseNumberEnteredForLowerDutyRateLine()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8211100000";
			invoiceLine.JI_InvoiceQuantity = 4000m;
			invoiceLine.JI_CustomsQuantity = 4000m;
			invoiceLine.JI_LinePrice = 0m;
			AssertEquals("PreCondition", ComputationCodeList.Codes.Derived, invoiceLine.ImportTariff.UE_DutyComputationCode);
			JobComInvoiceLine line2 = invoiceLine.AddSecondaryInvoiceLine();
			line2.JI_Tariff = "8211929045"; // .4c per unit + 6.1% = 20000 *.004 + 24000 * .061 = $1544
			line2.JI_CustomsQuantity = 16000m;
			line2.JI_LinePrice = 16000m;
			line2.US_ADDCaseNo = "A123456";
			JobComInvoiceLine line3 = invoiceLine.AddSecondaryInvoiceLine();
			line3.JI_Tariff = "8211930030"; // .3c per unit + 5.4% = (20000 *.03) + (24000 * .054) = $1896
			line3.JI_CustomsQuantity = 4000m;
			line3.JI_LinePrice = 8000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("PreCondition:Highest duty rate is 8211930030", "8211930030", line2.CusEntryLine.CL_AdValoremTariff);
			AssertHasMessageError(line2.US_ADDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.ThisTariffWillBeIgnoredDueToDerivedDutyCalculationAndADD_CVDDetailsNotValid);
			line2.US_ADDCaseNo = "";
			AssertNoMessageError(line2.US_ADDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.ThisTariffWillBeIgnoredDueToDerivedDutyCalculationAndADD_CVDDetailsNotValid);
			line3.US_CVDCaseNo = "C123456";
			AssertHasMessageError(line3.US_CVDDepositValueInfo, FormalImportAddInfoJobComInvoiceLineValidation.DepositValueShouldBeEnteredForDerivedDutyCalculation);
			line3.US_CVDDepositValue = 10m;
			AssertNoMessageError(line3.US_CVDDepositValueInfo, FormalImportAddInfoJobComInvoiceLineValidation.DepositValueShouldBeEnteredForDerivedDutyCalculation);
		}

		public void TestMoreThanOneADDDetails()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_ADDCaseNo = "A475819004";
			JobComInvoiceLine secondaryLine = invoiceLine.AddSecondaryInvoiceLine();
			secondaryLine.US_ADDCaseNo = "A475819004";
			AssertHasMessageError(secondaryLine.US_ADDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.SecondaryADD_CVDCannotBeSentWhenParentLineHasADDOrCVD);
			invoiceLine.US_ADDCaseNo = "";
			secondaryLine.US_ADDCaseNo = "A475819004";
			AssertNoMessageError(secondaryLine.US_ADDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.SecondaryADD_CVDCannotBeSentWhenParentLineHasADDOrCVD);
			JobComInvoiceLine secondaryLine2 = invoiceLine.AddSecondaryInvoiceLine();
			secondaryLine2.US_ADDCaseNo = "A475819004";
			AssertHasMessageError(secondaryLine2.US_ADDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.AnotherSecondaryHasADD_CVD);
			secondaryLine2.US_ADDCaseNo = "";
			AssertNoMessageError(secondaryLine2.US_ADDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.AnotherSecondaryHasADD_CVD);
		}

		public void TestMoreThanOneCVDDetails()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_CVDCaseNo = "C475819004";
			JobComInvoiceLine secondaryLine = invoiceLine.AddSecondaryInvoiceLine();
			secondaryLine.US_CVDCaseNo = "C475819004";
			AssertHasMessageError(secondaryLine.US_CVDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.SecondaryADD_CVDCannotBeSentWhenParentLineHasADDOrCVD);
			invoiceLine.US_CVDCaseNo = "";
			secondaryLine.US_CVDCaseNo = "C475819004";
			AssertNoMessageError(secondaryLine.US_CVDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.SecondaryADD_CVDCannotBeSentWhenParentLineHasADDOrCVD);
			JobComInvoiceLine secondaryLine2 = invoiceLine.AddSecondaryInvoiceLine();
			secondaryLine2.US_CVDCaseNo = "C475819004";
			AssertHasMessageError(secondaryLine2.US_CVDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.AnotherSecondaryHasADD_CVD);
			secondaryLine2.US_CVDCaseNo = "";
			AssertNoMessageError(secondaryLine2.US_CVDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.AnotherSecondaryHasADD_CVD);
		}

		public void TestCheckFASClaimExceptionWarning()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0306230020";
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.US_SPI = "Z";
			AssertHasMessageErrorContaining(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.InvalidSPIIndicatorForCountryOfOrigin);
			invoiceLine.US_UC_NKCountryOfOrigin = "FM";
			AssertNoMessageErrorContaining(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.InvalidSPIIndicatorForCountryOfOrigin);
			AssertHasWarning(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckFASClaimExceptionOnChapter2To52AgriculturalProductWarning);
			invoiceLine.JI_Tariff = "";
			AssertNoWarning(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckFASClaimExceptionOnChapter2To52AgriculturalProductWarning);
		}

		public void TestCheckUS_SPIWhenOnlyProvTariffIsDutyFree()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "2933394100";
			tariff1.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff1.UE_Column1RateSpecific = 1.05m;
			tariff1.UE_Column1RateOther = 0.95m;
			tariff1.UE_Column1RateAdValorem = 1m;
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.Today.AddYears(1);

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff2.UE_Column1RateSpecific = 1.05m;
			tariff2.UE_Column1RateOther = 0.95m;
			tariff2.UE_Column1RateAdValorem = 1m;
			tariff2.UE_Tariff = "99030120";
			tariff2.UE_QuotaIndicator = true;
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.Today.AddYears(1);

			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_DutyComputationCode = ComputationCodeList.Codes.Free;
			tariff3.UE_Tariff = "99030123";
			tariff3.UE_QuotaIndicator = true;
			tariff3.UE_DateFrom = ZDateTime.Today;
			tariff3.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff3.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff3.UE_DateTo = ZDateTime.Today.AddYears(1);

			var tariff4 = Factory.New<USCTariff>();
			tariff4.UE_DutyComputationCode = ComputationCodeList.Codes.Free;
			tariff4.UE_Tariff = "3303109000";
			tariff4.UE_QuotaIndicator = true;
			tariff4.UE_DateFrom = ZDateTime.Today;
			tariff4.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff4.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff4.UE_DateTo = ZDateTime.Today.AddYears(1);

			Factory.Save();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.US_SPI = "K";

			invoiceLine.JI_Tariff = tariff4.UE_Tariff;
			invoiceLine.US_SupTariff = tariff3.UE_Tariff;

			AssertHasMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.MayNotBeSpecifiedForDutyFree);
			invoiceLine.JI_Tariff = tariff1.UE_Tariff;
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.MayNotBeSpecifiedForDutyFree);
			invoiceLine.US_SupTariff = tariff2.UE_Tariff;
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.MayNotBeSpecifiedForDutyFree);
			invoiceLine.US_SupTariff = tariff3.UE_Tariff;
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.MayNotBeSpecifiedForDutyFree);
		}

		public void TestCheckUS_SPINotEnteredWhenThereAreApplicableSPIs()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_SPICode = "D R P AUBHCACLILJ+JOMAMXSG";
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = "";
			AssertHasMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.ThereAreSPIsApplicableNonEntered);
			invoiceLine.US_SPI = "AU";
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.ThereAreSPIsApplicableNonEntered);
			USCustomsDataRegistry.Instance.SPIValidation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, SPIValidationTypeList.Codes.WRN);
			invoiceLine.US_SPI = "";
			AssertHasWarning(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.ThereAreSPIsApplicableNonEntered);
			invoiceLine.US_SPI = "AU";
			AssertNoWarning(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.ThereAreSPIsApplicableNonEntered);
		}

		public void TestCheckUS_IsBondedADD()
		{
			invoiceLine.US_IsBondedADD = true;
			AssertHasMessageError(invoiceLine.US_IsBondedADDInfo, FormalImportAddInfoJobComInvoiceLineValidation.BondedADD_CVDRequiresSuretyCode);
			invoiceLine.US_IsBondedCVD = true;
			AssertHasMessageError(invoiceLine.US_IsBondedCVDInfo, FormalImportAddInfoJobComInvoiceLineValidation.BondedADD_CVDRequiresSuretyCode);
			declaration.US_ADDCVDSuretyCode = "ASD";
			invoiceLine.US_IsBondedADD = true;
			AssertNoMessageError(invoiceLine.US_IsBondedADDInfo, FormalImportAddInfoJobComInvoiceLineValidation.BondedADD_CVDRequiresSuretyCode);
			invoiceLine.US_IsBondedCVD = true;
			AssertNoMessageError(invoiceLine.US_IsBondedCVDInfo, FormalImportAddInfoJobComInvoiceLineValidation.BondedADD_CVDRequiresSuretyCode);
			USCACCase addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "AXXAAABBB";
			USCACCaseBondCash bondCash = addCase.BondCashIndicators.AddNew();
			bondCash.U8_Indicator = BondCashIndicatorList.Codes.Cash;
			bondCash.U8_InactivatedDate = ZDateTime.Empty;
			bondCash.U8_EffectiveDate = invoiceLine.DateForADD_CVD;
			AssertEquals(true, ((IACCase)addCase).IsCashRequired(invoiceLine.DateForADD_CVD));
			invoiceLine.US_ADDCaseNo = "AXXAAABBB";
			invoiceLine.US_CVDCaseNo = ZString.Empty;
			invoiceLine.US_IsBondedADD = false;
			AssertNoMessageError(invoiceLine.US_IsBondedADDInfo, FormalImportAddInfoJobComInvoiceLineValidation.CashRequirdADD_CVDCasesCannotBeBonded);
			invoiceLine.US_IsBondedADD = true;
			AssertHasMessageError(invoiceLine.US_IsBondedADDInfo, FormalImportAddInfoJobComInvoiceLineValidation.CashRequirdADD_CVDCasesCannotBeBonded);
			bondCash.U8_InactivatedDate = invoiceLine.DateForADD_CVD;
			AssertEquals(false, ((IACCase)addCase).IsCashRequired(invoiceLine.DateForADD_CVD));
			invoiceLine.US_IsBondedADD = false;
			AssertNoMessageError(invoiceLine.US_IsBondedADDInfo, FormalImportAddInfoJobComInvoiceLineValidation.CashRequirdADD_CVDCasesCannotBeBonded);
			invoiceLine.US_IsBondedADD = true;
			AssertNoMessageError(invoiceLine.US_IsBondedADDInfo, FormalImportAddInfoJobComInvoiceLineValidation.CashRequirdADD_CVDCasesCannotBeBonded);
			bondCash.U8_InactivatedDate = ZDateTime.Empty;
			AssertEquals(true, ((IACCase)addCase).IsCashRequired(invoiceLine.DateForADD_CVD));
			invoiceLine.US_ADDCaseNo = ZString.Empty;
			invoiceLine.US_CVDCaseNo = "AXXAAABBB";
			invoiceLine.US_IsBondedCVD = false;
			AssertNoMessageError(invoiceLine.US_IsBondedCVDInfo, FormalImportAddInfoJobComInvoiceLineValidation.CashRequirdADD_CVDCasesCannotBeBonded);
			invoiceLine.US_IsBondedCVD = true;
			AssertHasMessageError(invoiceLine.US_IsBondedCVDInfo, FormalImportAddInfoJobComInvoiceLineValidation.CashRequirdADD_CVDCasesCannotBeBonded);
			bondCash.U8_InactivatedDate = invoiceLine.DateForADD_CVD;
			AssertEquals(false, ((IACCase)addCase).IsCashRequired(invoiceLine.DateForADD_CVD));
			invoiceLine.US_IsBondedCVD = false;
			AssertNoMessageError(invoiceLine.US_IsBondedCVDInfo, FormalImportAddInfoJobComInvoiceLineValidation.CashRequirdADD_CVDCasesCannotBeBonded);
			invoiceLine.US_IsBondedCVD = true;
			AssertNoMessageError(invoiceLine.US_IsBondedCVDInfo, FormalImportAddInfoJobComInvoiceLineValidation.CashRequirdADD_CVDCasesCannotBeBonded);
		}

		public void TestCheckUS_LumberExportPrice()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			invoiceLine.JI_Tariff = "4407100115";
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Canada;
			AssertEquals(true, invoiceLine.IsSoftwoodLumberSection804FarmBillRequirement);
			invoiceLine.US_LumberImporterDeclaration = YesNoDefaultList.Codes.Yes;
			invoiceLine.US_LumberExportPrice = ZDecimal.Zero;
			AssertHasMessageError(invoiceLine.US_LumberExportPriceInfo, FormalImportAddInfoJobComInvoiceLineValidation.SoftWoodLumberExportPriceMandatory);
			invoiceLine.US_LumberExportPrice = 10m;
			AssertNoMessageError(invoiceLine.US_LumberExportPriceInfo, FormalImportAddInfoJobComInvoiceLineValidation.SoftWoodLumberExportPriceMandatory);
			invoiceLine.US_LumberImporterDeclaration = YesNoDefaultList.Codes.No;
			invoiceLine.US_LumberExportPrice = ZDecimal.Zero;
			AssertNoMessageError(invoiceLine.US_LumberExportPriceInfo, FormalImportAddInfoJobComInvoiceLineValidation.SoftWoodLumberExportPriceMandatory);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			AssertEquals(true, invoiceLine.IsSoftwoodLumberSection804FarmBillRequirement);
			invoiceLine.US_LumberImporterDeclaration = YesNoDefaultList.Codes.Yes;
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Canada;
			invoiceLine.US_LumberExportPrice = ZDecimal.Zero;
			AssertHasMessageError(invoiceLine.US_LumberExportPriceInfo, FormalImportAddInfoJobComInvoiceLineValidation.SoftwoodLumberExportPriceRequired);
			declaration.US_EntryType = EntryTypeList.Codes.Baggage;
			AssertEquals(false, invoiceLine.IsSoftwoodLumberSection804FarmBillRequirement);
			invoiceLine.US_LumberExportPrice = ZDecimal.Zero;
			AssertNoMessageError(invoiceLine.US_LumberExportPriceInfo, FormalImportAddInfoJobComInvoiceLineValidation.SoftwoodLumberExportPriceRequired);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			AssertEquals(true, invoiceLine.IsSoftwoodLumberSection804FarmBillRequirement);
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.SouthAfrica;
			invoiceLine.US_LumberExportPrice = ZDecimal.Zero;
			AssertNoMessageError(invoiceLine.US_LumberExportPriceInfo, FormalImportAddInfoJobComInvoiceLineValidation.SoftwoodLumberExportPriceRequired);
			invoiceLine.US_LumberImporterDeclaration = YesNoDefaultList.Codes.No;
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Canada;
			invoiceLine.US_LumberExportPrice = ZDecimal.Zero;
			AssertNoMessageError(invoiceLine.US_LumberExportPriceInfo, FormalImportAddInfoJobComInvoiceLineValidation.SoftwoodLumberExportPriceRequired);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_LumberExportPrice = 10m;
			AssertHasMessageError(invoiceLine.US_LumberExportPriceInfo, string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, FormalImportAddInfoJobComInvoiceLineValidation.Constants.OGA.SoftwoodLumber));
		}

		public void TestCheckUS_LumberExportPriceIsRequired()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			invoiceLine.JI_Tariff = "4407100115";
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Canada;
			AssertEquals(true, invoiceLine.IsSoftwoodLumberSection804FarmBillRequirement);
			invoiceLine.US_LumberExportPrice = 10m;
			invoiceLine.US_LumberImporterDeclaration = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_LumberImporterDeclarationInfo, FormalImportAddInfoJobComInvoiceLineValidation.ExportPriceSoftwoodLumberRequired);
			invoiceLine.US_LumberImporterDeclaration = YesNoDefaultList.Codes.Yes;
			AssertNoMessageError(invoiceLine.US_LumberImporterDeclarationInfo, FormalImportAddInfoJobComInvoiceLineValidation.ExportPriceSoftwoodLumberRequired);
			invoiceLine.US_LumberExportPrice = ZDecimal.Zero;
			AssertHasMessageError(invoiceLine.US_LumberExportPriceInfo, FormalImportAddInfoJobComInvoiceLineValidation.SoftWoodLumberExportPriceMandatory);
			invoiceLine.US_LumberExportPrice = 100m;
			AssertNoMessageError(invoiceLine.US_LumberExportPriceInfo, FormalImportAddInfoJobComInvoiceLineValidation.SoftWoodLumberExportPriceMandatory);
		}

		public void TestCheckUS_LumberImporterDeclaration()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			invoiceLine.JI_Tariff = "4407100115";
			AssertEquals(true, invoiceLine.IsSoftwoodLumberSection804FarmBillRequirement);
			invoiceLine.US_LumberImporterDeclaration = "D";
			AssertHasMessageErrorContaining(invoiceLine.US_LumberImporterDeclarationInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_LumberImporterDeclaration = YesNoDefaultList.Codes.No;
			AssertNoMessageErrorContaining(invoiceLine.US_LumberImporterDeclarationInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(invoiceLine.US_LumberImporterDeclarationInfo, FormalImportAddInfoJobComInvoiceLineValidation.SoftwoodLumberImporterDeclarationRequired);
			invoiceLine.US_LumberImporterDeclaration = YesNoDefaultList.Codes.Yes;
			AssertNoMessageErrorContaining(invoiceLine.US_LumberImporterDeclarationInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(invoiceLine.US_LumberImporterDeclarationInfo, FormalImportAddInfoJobComInvoiceLineValidation.SoftwoodLumberImporterDeclarationRequired);
			invoiceLine.US_LumberImporterDeclaration = "";
			AssertNoMessageErrorContaining(invoiceLine.US_LumberImporterDeclarationInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(invoiceLine.US_LumberImporterDeclarationInfo, FormalImportAddInfoJobComInvoiceLineValidation.SoftwoodLumberImporterDeclarationRequired);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_LumberImporterDeclaration = YesNoDefaultList.Codes.Yes;
			AssertHasMessageError(invoiceLine.US_LumberImporterDeclarationInfo, string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, FormalImportAddInfoJobComInvoiceLineValidation.Constants.OGA.SoftwoodLumber));
		}

		[TestDate(2008, 9, 11)]
		public void TestCottonFeeExemptNotRequiredForEnsemble()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6104220040";
			Assert("PreCondition", invoiceLine.ImportTariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Cotton));
			JobComInvoiceLine secondaryLine = invoiceLine.AddSecondaryInvoiceLine();
			secondaryLine.JI_Tariff = "6104622028";
			secondaryLine.JI_CustomsQuantity = 200m;
			Assert("PreCondition", secondaryLine.ImportTariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Cotton));
			invoiceLine.US_CottonFeeExempt = ZString.Empty;
			secondaryLine.US_CottonFeeExempt = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.US_CottonFeeExemptInfo, FormalImportAddInfoJobComInvoiceLineValidation.CottonFeeApplicable);
			AssertHasMessageErrorContaining(secondaryLine.US_CottonFeeExemptInfo, FormalImportAddInfoJobComInvoiceLineValidation.CottonFeeApplicable);
			invoiceLine.JI_Tariff = "6104.22.0010";
			Assert("PreCondition", !invoiceLine.ImportTariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Cotton));
			secondaryLine.US_CottonFeeExempt = ZString.Empty;
			AssertHasMessageErrorContaining(secondaryLine.US_CottonFeeExemptInfo, FormalImportAddInfoJobComInvoiceLineValidation.CottonFeeApplicable);
			secondaryLine.US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;
			AssertNoMessageErrorContaining(secondaryLine.US_CottonFeeExemptInfo, FormalImportAddInfoJobComInvoiceLineValidation.CottonFeeApplicable);
		}

		public void TestCottonFeeExemptForXLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.CottonFeeApplicable;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			Assert("PreCondition", invoiceLine.ImportTariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Cotton));
			var vLine = invoiceLine.AddSecondaryInvoiceLine();
			vLine.JI_Tariff = USCTariff.CottonFeeApplicable;
			vLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			Assert("PreCondition", vLine.ImportTariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Cotton));
			var vLine2 = invoiceLine.AddSecondaryInvoiceLine();
			vLine2.JI_Tariff = USCTariff.CottonFeeApplicable;
			vLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine.US_CottonFeeExempt = ZString.Empty;
			Assert("No message error saying that it is required", !invoiceLine.US_CottonFeeExemptInfo.HasMessageErrors());
			vLine.US_CottonFeeExempt = ZString.Empty;
			AssertHasMessageError(vLine.US_CottonFeeExemptInfo, FormalImportAddInfoJobComInvoiceLineValidation.CottonFeeApplicable);
			vLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			AssertNoMessageError(vLine.US_CottonFeeExemptInfo, FormalImportAddInfoJobComInvoiceLineValidation.CottonFeeApplicable);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;
			AssertHasMessageError(invoiceLine.US_CottonFeeExemptInfo, string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.LicenseAndPermit));
			invoiceLine.US_CottonCertificateNo = "007894812";
			AssertHasMessageError(invoiceLine.US_CottonCertificateNoInfo, string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.LicenseAndPermit));
		}

		[TestDate(2008, 9, 11)]
		public void TestCottonFeeExemptCertificateShouldNotBeEnteredForExemptTariff()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6206900040";
			invoiceLine.US_UC_NKCountryOfOrigin = "TW";
			invoiceLine.US_UC_NKCountryOfExport = "TW";
			invoiceLine.JI_CustomsSecondQuantity = 500m; //affects the cotton fee (Second UQ: KG)
			AssertEquals("PreCondition:Exempt Indicator is empty", ZString.Empty, invoiceLine.US_CottonFeeExempt);
			AssertHasMessageErrorContaining(invoiceLine.US_CottonFeeExemptInfo, FormalImportAddInfoJobComInvoiceLineValidation.CottonFeeApplicable);
			invoiceLine.US_CottonFeeExempt = "~";
			AssertHasMessageErrorContaining(invoiceLine.US_CottonFeeExemptInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_Tariff = "6206900040";
			JobComInvoiceLine line2 = invoiceLine.InvoiceHeader.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "6206900040";
			line2.US_UC_NKCountryOfOrigin = "TW";
			line2.US_UC_NKCountryOfExport = "TW";
			line2.JI_CustomsSecondQuantity = 500m; //affects the cotton fee (Second UQ: KG)
			line2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			line2.JI_Tariff = USCTariff.FCCMayBeApplicable;
			line2.US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;
			AssertEquals("PreCondition:CottonFeeNotApplicable", false, line2.ImportTariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Cotton));
			AssertHasMessageError(line2.US_CottonFeeExemptInfo, FormalImportAddInfoJobComInvoiceLineValidation.CottonFeeNotApplicable);
			line2.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			AssertHasMessageError(line2.US_CottonFeeExemptInfo, FormalImportAddInfoJobComInvoiceLineValidation.CottonFeeNotApplicable);
			line2.US_CottonFeeExempt = ZString.Empty;
			AssertNoMessageError(line2.US_CottonFeeExemptInfo, FormalImportAddInfoJobComInvoiceLineValidation.CottonFeeNotApplicable);
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			JobComInvoiceLine secondaryLine1 = invoiceLine.AddSecondaryInvoiceLine();
			secondaryLine1.US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;
			AssertHasMessageError(secondaryLine1.US_CottonFeeExemptInfo, FormalImportAddInfoJobComInvoiceLineValidation.LineCannotBeSentWhenMixedExemptionIndicators);
			secondaryLine1.US_CottonFeeExempt = ZString.Empty;
			AssertNoMessageError(secondaryLine1.US_CottonFeeExemptInfo, FormalImportAddInfoJobComInvoiceLineValidation.LineCannotBeSentWhenMixedExemptionIndicators);
			secondaryLine1.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			AssertNoMessageError(secondaryLine1.US_CottonFeeExemptInfo, FormalImportAddInfoJobComInvoiceLineValidation.LineCannotBeSentWhenMixedExemptionIndicators);
		}

		public void TestCountryOfOriginShouldBeACanadianProvinceForPrimarySPI_B()
		{
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.B;
			AssertHasMessageError(invoiceLine.US_SPIInfo, FormalImportAddInfoJobComInvoiceLineValidation.CountryOfOriginShouldBeACanadianProvinceForPrimarySPI_B);
			invoiceLine.US_UC_NKCountryOfOrigin = "CA";
			AssertHasMessageError(invoiceLine.US_SPIInfo, FormalImportAddInfoJobComInvoiceLineValidation.CountryOfOriginShouldBeACanadianProvinceForPrimarySPI_B);
			invoiceLine.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XA;
			AssertNoMessageError(invoiceLine.US_SPIInfo, FormalImportAddInfoJobComInvoiceLineValidation.CountryOfOriginShouldBeACanadianProvinceForPrimarySPI_B);
		}

		public void TestVisaNumber_4CU()
		{
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.F;
			invoiceLine.US_VisaNo = "TEST";
			AssertEquals(false, invoiceLine.US_VisaNoInfo.HasMessageError(FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.MayNotBeEnteredWhereSPISecondaryM));
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.M;
			invoiceLine.US_VisaNo = "";
			AssertEquals(false, invoiceLine.US_VisaNoInfo.HasMessageError(FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.MayNotBeEnteredWhereSPISecondaryM));
			invoiceLine.US_VisaNo = "TEST";
			AssertEquals(true, invoiceLine.US_VisaNoInfo.HasMessageError(FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.MayNotBeEnteredWhereSPISecondaryM));
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_VisaNo = "TEST";
			AssertHasMessageError(invoiceLine.US_VisaNoInfo, string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.TextileCLassificationDetails));
		}

		public void TestCountryOfOriginAndExportTheSameForSPIY()
		{
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.SouthAfrica;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.Y;
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.InsularPossessionRequiresSameCountry);
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.SouthAfrica;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.Z;
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.InsularPossessionRequiresSameCountry);
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Russia;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.Y;
			AssertHasMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.InsularPossessionRequiresSameCountry);
			invoiceLine.US_UC_NKCountryOfExport = ZString.Empty;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Honduras;
			declaration.JE_RL_NKPortOfLoading = "HNGJA";
			invoiceLine.AddInfoValidation.ValidateUS_SPI();
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.InsularPossessionRequiresSameCountry);
		}

		public void TestCheckUS_VisaUQ()
		{
			invoiceLine.US_VisaUQ = "~";
			AssertHasMessageError(invoiceLine.US_VisaUQInfo, FormalImportAddInfoJobComInvoiceLineValidation.VisaUQShouldBeInList);
			invoiceLine.US_VisaUQ = invoiceLine.AddInfoLookups.US_UnitOfMeasureList[0].Code;
			AssertNoMessageError(invoiceLine.US_VisaUQInfo, FormalImportAddInfoJobComInvoiceLineValidation.VisaUQShouldBeInList);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_VisaUQ = invoiceLine.AddInfoLookups.US_UnitOfMeasureList[0].Code;
			AssertHasMessageError(invoiceLine.US_VisaUQInfo, string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.TextileCLassificationDetails));
		}

		[TestDate(2008, 9, 11)]
		public void TestUS_MiscPermitNo_NotRequired()
		{
			invoiceLine.JI_Tariff = "3824710000";
			AssertEquals(true, invoiceLine.ImportTariff.UE_PermitLicenseIndicator.IsEmpty);
			invoiceLine.US_MiscPermitNo = "AAA";
			AssertHasWarning(invoiceLine.US_MiscPermitNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.MiscPermitNoIsNotRequired);
			invoiceLine.US_MiscPermitNo = "";
			AssertNoWarning(invoiceLine.US_MiscPermitNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.MiscPermitNoIsNotRequired);
		}

		public void TestUS_MiscPermitNo_Required()
		{
			const string tariff = "98206225";
			USCTariff tariff_textile = Factory.New<USCTariff>();
			tariff_textile.UE_Tariff = tariff;
			tariff_textile.UE_DateFrom = new ZDate(2000, 1, 1);
			tariff_textile.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff_textile.UE_PermitLicenseIndicator = "13"; //no specific validator exists
			invoiceLine.JI_Tariff = tariff;
			invoiceLine.US_MiscPermitNo = "123";
			AssertNoMessageError(invoiceLine.US_MiscPermitNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.MiscPermitNoRequired);
			invoiceLine.US_MiscPermitNo = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_MiscPermitNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.MiscPermitNoRequired);
		}

		public void TestUS_MiscPermitNo_Steel()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._01, "Steel Import License", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			invoiceLine.JI_Tariff = "7219320042";
			invoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			Assert(invoiceLine.US_MiscPermitNoInfo.GetMessageErrors().GetFirstMessage().Contains("Steel"));
		}

		public void TestUS_MiscPermitNo_Beef()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._05, "Beef Export Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			invoiceLine.JI_Tariff = "0202201000";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Argentina;
			invoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			Assert(invoiceLine.US_MiscPermitNoInfo.GetMessageErrors().GetFirstMessage().Contains("The Beef Export Certificate number is required."));
		}

		public void TestUS_MiscPermitNo_Diamond()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._06, "Diamond Certificate", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			invoiceLine.JI_Tariff = "7102211010";
			invoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			Assert(invoiceLine.US_MiscPermitNoInfo.GetMessageErrors().GetFirstMessage().Contains("The Diamond Certificate number is required."));
		}

		public void TestUS_MiscPermitNo_MXCement()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._09, "Mexican Cement Import License", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			invoiceLine.JI_Tariff = "2523100000";
			invoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			var msgErrors = invoiceLine.US_MiscPermitNoInfo.GetMessageErrors();
			AssertEquals(0, msgErrors.Count());
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			invoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			var permitError = invoiceLine.US_MiscPermitNoInfo.GetMessageErrors().GetFirstMessage();
			Assert(permitError.Contains("Mexican Cement"));
		}

		public void TestCheckUS_TSCAIndicator()
		{
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(invoiceLine.US_TSCAIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			AssertHasMessageError(invoiceLine.US_TSCAIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
			invoiceLine.US_TSCAInd = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_TSCAIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_CargoReleaseType = ZString.Empty;
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(invoiceLine.US_TSCAIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			AssertHasMessageError(invoiceLine.US_TSCAIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
			invoiceLine.US_TSCAInd = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_TSCAIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
		}

		public void TestCheckVisaFormat()
		{
			declaration.JE_ExportDate = new ZDateTime(2007, 1, 1);
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Lesotho;
			invoiceLine.US_DateOfExportFromCountryOfOrigin = ZDateTime.Empty;
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			invoiceLine.US_VisaNo = "ABB";
			AssertHasMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaNumberNot9Characters);
			AssertHasMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.FirstCharacterVisaNumberShouldBeANumber);
			AssertHasMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.SecondThirdLettersVisaNumbersShouldBeCountryOfOrigin);
			invoiceLine.US_VisaNo = "2LS456789";
			AssertNoMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaNumberNot9Characters);
			AssertHasMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.FirstCharacterVisaNumberShouldBeANumber);
			AssertNoMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.SecondThirdLettersVisaNumbersShouldBeCountryOfOrigin);
			invoiceLine.US_VisaNo = "7LS456789";
			AssertNoMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.FirstCharacterVisaNumberShouldBeANumber);
			invoiceLine.US_DateOfExportFromCountryOfOrigin = new ZDateTime(2007, 1, 1);
			invoiceLine.US_VisaNo = "2LS456789";
			AssertHasMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.FirstCharacterVisaNumberShouldBeANumber);
			invoiceLine.US_VisaNo = "7LS456789";
			AssertNoMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.FirstCharacterVisaNumberShouldBeANumber);
			invoiceLine.US_VisaNo = "7LS456O89";
			AssertHasMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.Last6PositionsShouldBeNumbers);
			invoiceLine.US_VisaNo = "7LS4567l9";
			AssertHasMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.Last6PositionsShouldBeNumbers);
			invoiceLine.US_VisaNo = "7LS456739";
			AssertNoMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.Last6PositionsShouldBeNumbers);
			invoiceLine.US_SupTariff = FormalImportAddInfoJobComInvoiceLineValidation.CBTPAGoodsBrassiereTariff98201115;
			invoiceLine.US_VisaNo = "FCB123456";
			AssertHasMessageErrorContaining(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.InvalidFormatForBrassiereTariff98201115);
			invoiceLine.US_VisaNo = "1CB123O56";
			AssertHasMessageErrorContaining(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.InvalidFormatForBrassiereTariff98201115);
			invoiceLine.US_VisaNo = "1CB123l56";
			AssertHasMessageErrorContaining(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.InvalidFormatForBrassiereTariff98201115);
			invoiceLine.US_VisaNo = "1RT231568";
			AssertHasMessageErrorContaining(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.InvalidFormatForBrassiereTariff98201115);
			invoiceLine.US_VisaNo = "1CB23156";
			AssertHasMessageErrorContaining(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.InvalidFormatForBrassiereTariff98201115);
			invoiceLine.US_VisaNo = "1CB523456";
			AssertHasMessageErrorContaining(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.InvalidFormatForBrassiereTariff98201115);
			invoiceLine.US_VisaNo = "1CB123456";
			AssertNoMessageErrorContaining(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.InvalidFormatForBrassiereTariff98201115);
			invoiceLine.US_SupTariff = "";
			invoiceLine.JI_Tariff = "1";
			invoiceLine.US_VisaNo = "FCB123456";
			AssertNoMessageErrorContaining(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.InvalidFormatForBrassiereTariff98201115);
		}

		public void TestVisaNumberNotRequired()
		{
			var fdaPNRequiredTariff = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, USCTariff.FDAPriorNoticeRequiredTariff)).LastOrDefault();
			if (fdaPNRequiredTariff == null)
			{
				fdaPNRequiredTariff = Factory.New<USCTariff>();
				fdaPNRequiredTariff.UE_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
				fdaPNRequiredTariff.UE_DutyComputationCode = "7";
				fdaPNRequiredTariff.UE_Column1RateAdValorem = 0.25m;
				fdaPNRequiredTariff.UE_DateFrom = ZDateTime.Today;
				fdaPNRequiredTariff.UE_DateTo = ZDateTime.Today.AddDays(30);
				fdaPNRequiredTariff.UE_PGACodes = "FD3";
			}

			Factory.Save();
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "FW1EP3EP5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_ExportDate = new ZDateTime(2007, 1, 1);
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Lesotho;
			invoiceLine.US_DateOfExportFromCountryOfOrigin = ZDateTime.Empty;
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			invoiceLine.US_VisaNo = "ABB";
			invoiceLine.AddInfoValidation.ValidateUS_VisaNo();
			AssertHasMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaNumberNotRequired);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			invoiceLine.AddInfoValidation.ValidateUS_VisaNo();
			AssertNoMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaNumberNotRequired);
		}

		public void TestCheckUS_VisaNoForConsumptionQuery()
		{
			USCVisa visa = Factory.New<USCVisa>();
			visa.UO_TextileCategoryNo = "123";
			visa.UO_UC_NKOriginCountry = "KR";
			visa.UO_BeginDate = ZDateTime.BrettsBirthday;
			visa.UO_EndDate = ZDateTime.Today;
			invoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			invoiceLine.Declaration.JE_ExportDate = ZDateTime.BrettsBirthday;
			invoiceLine.US_DateOfExportFromCountryOfOrigin = ZDateTime.Empty;
			invoiceLine.US_TextileCategoryNo = "123";
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			AssertHasMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaRequiredForTariffOriginAndCategory);
			invoiceLine.US_UC_NKCountryOfOrigin = "JP";
			AssertNoMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaRequiredForTariffOriginAndCategory);
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			AssertHasMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaRequiredForTariffOriginAndCategory);
			invoiceLine.US_TextileCategoryNo = "234";
			AssertNoMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaRequiredForTariffOriginAndCategory);
			invoiceLine.US_TextileCategoryNo = "123";
			AssertHasMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaRequiredForTariffOriginAndCategory);
			invoiceLine.US_VisaNo = "7ZM123456";
			AssertNoMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaRequiredForTariffOriginAndCategory);
			invoice.US_DateOfExport = ZDateTime.BrettsBirthday;
			invoiceLine.US_VisaNo = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaRequiredForTariffOriginAndCategory);
			invoiceLine.US_VisaNo = "7ZM123456";
			AssertNoMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaRequiredForTariffOriginAndCategory);
			invoiceLine.US_DateOfExportFromCountryOfOrigin = new ZDateTime(2007, 1, 1);
			invoiceLine.US_VisaNo = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaRequiredForTariffOriginAndCategory);
			invoiceLine.US_VisaNo = "7ZM123456";
			AssertNoMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaRequiredForTariffOriginAndCategory);
		}

		public void TestVisaNoIsEnteredWhenTextileExportDateIsEntered()
		{
			invoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			invoiceLine.US_TextileCategoryNo = "123";
			invoiceLine.US_UC_NKCountryOfOrigin = "IT";
			invoiceLine.US_DateOfExportFromCountryOfOrigin = ZDateTime.Today;
			AssertHasMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaNoRequired);
			invoiceLine.US_VisaNo = "123456789";
			AssertNoMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.VisaNumber.VisaNoRequired);
		}

		public void TestTextileExportDate()
		{
			invoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			invoiceLine.US_TextileCategoryNo = "123";
			invoiceLine.US_UC_NKCountryOfOrigin = "IT";
			invoiceLine.US_UC_NKCountryOfExport = "IT";
			invoiceLine.InvoiceHeader.US_DateOfExport = ZDateTime.Today.AddDays(1);
			invoiceLine.US_DateOfExportFromCountryOfOrigin = ZDateTime.Today;
			AssertHasMessageError(invoiceLine.US_DateOfExportFromCountryOfOriginInfo, FormalImportAddInfoJobComInvoiceLineValidation.OriginAndExportDateMustBeTheSame);
			invoiceLine.InvoiceHeader.US_DateOfExport = ZDateTime.Now;
			invoiceLine.US_DateOfExportFromCountryOfOrigin = ZDateTime.Now;
			AssertNoMessageError(invoiceLine.US_DateOfExportFromCountryOfOriginInfo, FormalImportAddInfoJobComInvoiceLineValidation.OriginAndExportDateMustBeTheSame);
			invoiceLine.US_UC_NKCountryOfOrigin = "FR";
			invoiceLine.InvoiceHeader.US_DateOfExport = ZDateTime.Today.AddDays(-1);
			invoiceLine.US_DateOfExportFromCountryOfOrigin = ZDateTime.Today;
			AssertHasMessageError(invoiceLine.US_DateOfExportFromCountryOfOriginInfo, FormalImportAddInfoJobComInvoiceLineValidation.TextileExportDateNotLater);
			invoiceLine.InvoiceHeader.US_DateOfExport = ZDateTime.Today.AddDays(3);
			invoiceLine.US_DateOfExportFromCountryOfOrigin = ZDateTime.Today;
			AssertNoMessageError(invoiceLine.US_DateOfExportFromCountryOfOriginInfo, FormalImportAddInfoJobComInvoiceLineValidation.TextileExportDateNotLater);
		}

		public void TestTextileCategoryNo()
		{
			AssertUS_TexileCategoryNo(invoiceLine);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertTExtileCategoryNoAganstXLine(invoiceLine);
		}

		public void TestTextileCategoryNoIsNotSentForCertain9802Tariffs()
		{
			//	0LK		NOTE:  98020050/60 ENTRIES DO NOT REQUIRE CATEGORY NUMBERS.
			invoiceLine.JI_Tariff = "5407820090";
			invoiceLine.US_SupTariff = "9802008040";
			invoiceLine.US_TextileCategoryNo = "519";
			AssertNoMessageError(invoiceLine.US_TextileCategoryNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.TextileCategoryIsNotRequiredForThisTariff);
			invoiceLine.US_SupTariff = "9802005010";
			invoiceLine.US_TextileCategoryNo = "639";
			AssertHasMessageError(invoiceLine.US_TextileCategoryNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.TextileCategoryIsNotRequiredForThisTariff);
			invoiceLine.US_TextileCategoryNo = "";
			AssertNoMessageError(invoiceLine.US_TextileCategoryNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.TextileCategoryIsNotRequiredForThisTariff);
		}

		public void TestTextileCategoryForSPI()
		{
			invoiceLine.JI_Tariff = "5407820090";
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.G;
			invoiceLine.US_TextileCategoryNo = "228";
			AssertHasMessageError(invoiceLine.US_TextileCategoryNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.TextileCategoryForSPI);
			invoiceLine.US_TextileCategoryNo = "443";
			AssertNoMessageError(invoiceLine.US_TextileCategoryNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.TextileCategoryForSPI);
			invoiceLine.US_TextileCategoryNo = "444";
			AssertNoMessageError(invoiceLine.US_TextileCategoryNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.TextileCategoryForSPI);
			invoiceLine.US_TextileCategoryNo = "643";
			AssertNoMessageError(invoiceLine.US_TextileCategoryNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.TextileCategoryForSPI);
			invoiceLine.US_TextileCategoryNo = "644";
			AssertNoMessageError(invoiceLine.US_TextileCategoryNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.TextileCategoryForSPI);
			invoiceLine.US_TextileCategoryNo = "843";
			AssertNoMessageError(invoiceLine.US_TextileCategoryNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.TextileCategoryForSPI);
			invoiceLine.US_TextileCategoryNo = "844";
			AssertNoMessageError(invoiceLine.US_TextileCategoryNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.TextileCategoryForSPI);
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.M;
			invoiceLine.US_TextileCategoryNo = "228";
			AssertNoMessageError(invoiceLine.US_TextileCategoryNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.TextileCategoryForSPI);
		}

		public void TestCheckUS_PIRPRulingNoAndType()
		{
			invoiceLine.US_PIRPRulingType = PIRPRulingTypeList.Codes.Preclassification;
			AssertHasWarning(invoiceLine.US_PIRPRulingTypeInfo, FormalImportAddInfoJobComInvoiceLineValidation.PIRPRulingTypeEnteredWithoutNo);
			invoiceLine.US_PIRPRulingNo = "123";
			AssertNoWarning(invoiceLine.US_PIRPRulingTypeInfo, FormalImportAddInfoJobComInvoiceLineValidation.PIRPRulingTypeEnteredWithoutNo);
			invoiceLine.US_PIRPRulingType = "";
			AssertHasMessageError(invoiceLine.US_PIRPRulingNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.PIRPRulingNoEnteredWithoutType);
			invoiceLine.US_PIRPRulingType = "~";
			AssertNoMessageError(invoiceLine.US_PIRPRulingNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.PIRPRulingNoEnteredWithoutType);
			AssertHasMessageError(invoiceLine.US_PIRPRulingTypeInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_PIRPRulingType = PIRPRulingTypeList.Codes.Preclassification;
			AssertNoMessageError(invoiceLine.US_PIRPRulingTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			invoiceLine.US_PIRPRulingNo = "INVREQ";
			AssertHasMessageError(invoiceLine.US_PIRPRulingNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.PIRPRulingNumberCannotBeINVREQIfTruck);
			invoiceLine.US_PIRPRulingNo = "";
			AssertNoMessageError(invoiceLine.US_PIRPRulingNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.PIRPRulingNumberCannotBeINVREQIfTruck);
			invoiceLine.US_PIRPRulingNo = "INVREQ";
			invoiceLine.US_PIRPRulingType = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_PIRPRulingNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.PIRPRulingNumberCannotBeINVREQIfTruck);
			invoiceLine.US_PIRPRulingNo = ZString.Empty;
			invoiceLine.US_PIRPRulingType = PIRPRulingTypeList.Codes.CommercialDescription;
			AssertNoWarning(invoiceLine.US_PIRPRulingTypeInfo, FormalImportAddInfoJobComInvoiceLineValidation.PIRPRulingTypeEnteredWithoutNo);
			invoiceLine.US_PIRPRulingType = PIRPRulingTypeList.Codes.CommercialDescription;
			invoiceLine.US_PIRPRulingNo = "GTV34";
			AssertHasMessageError(invoiceLine.US_PIRPRulingNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.PIRPRulingNumberShouldBeEmptyForCommercialDescr);
			invoiceLine.US_PIRPRulingNo = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_PIRPRulingNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.PIRPRulingNumberShouldBeEmptyForCommercialDescr);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_PIRPRulingType = PIRPRulingTypeList.Codes.BindingRulings;
			AssertHasMessageError(invoiceLine.US_PIRPRulingTypeInfo, string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.LicenseAndPermit));
			invoiceLine.US_PIRPRulingNo = "GTV";
			AssertHasMessageError(invoiceLine.US_PIRPRulingNoInfo, string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.LicenseAndPermit));
		}

		public void TestCheckUS_DestinationState()
		{
			invoiceLine.US_DestinationState = "";
			AssertHasMessageError(invoiceLine.US_DestinationStateInfo, FormalImportAddInfoJobComInvoiceLineValidation.DestinationStateIsRequired);
			invoiceLine.InvoiceHeader.US_DestinationState = "IL";
			invoiceLine.US_DestinationState = "";
			AssertNoMessageError(invoiceLine.US_DestinationStateInfo, FormalImportAddInfoJobComInvoiceLineValidation.DestinationStateIsRequired);
			invoiceLine.InvoiceHeader.US_DestinationState = "";
			invoiceLine.US_DestinationState = "";
			AssertHasMessageError(invoiceLine.US_DestinationStateInfo, FormalImportAddInfoJobComInvoiceLineValidation.DestinationStateIsRequired);
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			invoiceLine.US_DestinationState = "";
			AssertNoMessageError(invoiceLine.US_DestinationStateInfo, FormalImportAddInfoJobComInvoiceLineValidation.DestinationStateIsRequired);
			invoiceLine.US_DestinationState = "~";
			AssertHasMessageError(invoiceLine.US_DestinationStateInfo, ValidationConstants.Declaration.DestinationStateShouldBeInList);
			invoiceLine.US_DestinationState = USStatesList.Codes.Alabama;
			AssertNoMessageError(invoiceLine.US_DestinationStateInfo, ValidationConstants.Declaration.DestinationStateShouldBeInList);
		}

		public void TestCheckUS_OA_ManufacturerAddress()
		{
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.US_EnableINB = true;
			AssertUS_OA_ManufacturerAddressForMID(invoiceLine);
			declaration.ValidationModes = ValidationModes.CargoRelease;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			AssertUS_OA_ManufacturerAddressForMID(invoiceLine);
			JobDeclaration declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration2.US_EnableENS = true;
			JobComInvoiceHeader invoice2 = declaration2.Invoices.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			AssertUS_OA_ManufacturerAddressForMIDAgainstCanadianProvince(invoiceLine2);
		}

		public void TestValidateADDCVDCases()
		{
			USCACCase addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "AXXAAABBB";
			addCase.U5_ISOCountryCode = "AU";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.IC;
			addCase.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			addCase.CaseTariffs.AddNew().U9_TariffNumber = "221133445";
			USCACCaseLiqSuspension liqSuspension = addCase.LiqSuspensions.AddNew();
			liqSuspension.UN_InactivatedDate = ZDateTime.Empty;
			liqSuspension.UN_EffectiveDate = ZDateTime.BrettsBirthday;
			liqSuspension.UN_Action = "START";
			Factory.Save();
			invoiceLine.US_UC_NKCountryOfOrigin = "NZ";
			invoiceLine.JI_Tariff = "221133445";
			invoiceLine.US_ADDCaseNo = "AXXAAABBB";
			AssertHasMessageErrorContaining(invoiceLine.US_ADDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.CaseNoAgainstCountryOfOriginPartialMessage);
			AssertHasMessageErrorContaining(invoiceLine.US_ADDCaseNoInfo, string.Format(FormalImportAddInfoJobComInvoiceLineValidation.ADD_CVDIsNotEffective, ACCaseStatusList.Codes.IC, ACCaseStatusList.Descriptions.IC));
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_ADDCaseNo = "AXXAAABBB";
			AssertNoMessageErrorContaining(invoiceLine.US_ADDCaseNoInfo, string.Format(FormalImportAddInfoJobComInvoiceLineValidation.ADD_CVDIsNotEffective, ACCaseStatusList.Codes.IC, ACCaseStatusList.Descriptions.IC));
			AssertNoMessageErrorContaining(invoiceLine.US_ADDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.CaseNoAgainstCountryOfOriginPartialMessage);
			invoiceLine.US_CVDCaseNo = "AXXAAABBB";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.ID;
			invoiceLine.AddInfoValidation.ValidateUS_ADDCaseNo();
			AssertHasMessageErrorContaining(invoiceLine.US_ADDCaseNoInfo, string.Format(FormalImportAddInfoJobComInvoiceLineValidation.ADD_CVDIsNotEffective, ACCaseStatusList.Codes.ID, ACCaseStatusList.Descriptions.ID));
			invoiceLine.AddInfoValidation.ValidateUS_CVDCaseNo();
			AssertHasMessageErrorContaining(invoiceLine.US_CVDCaseNoInfo, string.Format(FormalImportAddInfoJobComInvoiceLineValidation.ADD_CVDIsNotEffective, ACCaseStatusList.Codes.ID, ACCaseStatusList.Descriptions.ID));
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.IF;
			invoiceLine.AddInfoValidation.ValidateUS_ADDCaseNo();
			AssertHasMessageErrorContaining(invoiceLine.US_ADDCaseNoInfo, string.Format(FormalImportAddInfoJobComInvoiceLineValidation.ADD_CVDIsNotEffective, ACCaseStatusList.Codes.IF, ACCaseStatusList.Descriptions.IF));
			invoiceLine.AddInfoValidation.ValidateUS_CVDCaseNo();
			AssertHasMessageErrorContaining(invoiceLine.US_CVDCaseNoInfo, string.Format(FormalImportAddInfoJobComInvoiceLineValidation.ADD_CVDIsNotEffective, ACCaseStatusList.Codes.IF, ACCaseStatusList.Descriptions.IF));
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.IO;
			invoiceLine.AddInfoValidation.ValidateUS_ADDCaseNo();
			invoiceLine.AddInfoValidation.ValidateUS_CVDCaseNo();
			AssertNoMessageErrorContaining(invoiceLine.US_ADDCaseNoInfo, string.Format(FormalImportAddInfoJobComInvoiceLineValidation.ADD_CVDIsNotEffective, ACCaseStatusList.Codes.IF, ACCaseStatusList.Descriptions.IF));
			AssertNoMessageErrorContaining(invoiceLine.US_CVDCaseNoInfo, string.Format(FormalImportAddInfoJobComInvoiceLineValidation.ADD_CVDIsNotEffective, ACCaseStatusList.Codes.IF, ACCaseStatusList.Descriptions.IF));
			AssertNoMessageErrorContaining(invoiceLine.US_ADDCaseNoInfo, string.Format(FormalImportAddInfoJobComInvoiceLineValidation.ADD_CVDIsNotEffective, ACCaseStatusList.Codes.ID, ACCaseStatusList.Descriptions.ID));
			AssertNoMessageErrorContaining(invoiceLine.US_CVDCaseNoInfo, string.Format(FormalImportAddInfoJobComInvoiceLineValidation.ADD_CVDIsNotEffective, ACCaseStatusList.Codes.ID, ACCaseStatusList.Descriptions.ID));
			addCase.U5_CaseNumber = "CXXAAABBB";
			addCase.U5_ManufacturerMID = "MXFTR345F";
			OrgHeader manufacturer = Factory.New<OrgHeader>();
			invoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			invoiceLine.US_CVDCaseNo = "CXXAAABBB";
			string errorMessage1 = string.Format(FormalImportAddInfoJobComInvoiceLineValidation.CVDADDManufacturerIDDiffer, "Countervailing");
			string errorMessage2 = string.Format(FormalImportAddInfoJobComInvoiceLineValidation.CVDADDManufacturerIDDiffer, "Antidumping");
			AssertNoWarningContaining(invoiceLine.US_CVDCaseNoInfo, errorMessage1);
			AssertNoWarningContaining(invoiceLine.US_ADDCaseNoInfo, errorMessage2);
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "HXFTR345F");
			invoiceLine.US_CVDCaseNo = "CXXAAABBB";
			invoiceLine.US_ADDCaseNo = "CXXAAABBB";
			AssertHasWarningContaining(invoiceLine.US_ADDCaseNoInfo, errorMessage2);
			AssertHasWarningContaining(invoiceLine.US_CVDCaseNoInfo, errorMessage1);
			manufacturer.MainAddress.CustomsCodes.DeleteAll();
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "MXFTR345F");
			invoiceLine.US_CVDCaseNo = "CXXAAABBB";
			invoiceLine.US_ADDCaseNo = "CXXAAABBB";
			AssertNoWarningContaining(invoiceLine.US_CVDCaseNoInfo, errorMessage1);
			AssertNoWarningContaining(invoiceLine.US_ADDCaseNoInfo, errorMessage2);
			addCase.U5_ManufacturerMID = ZString.Empty;
			invoiceLine.US_CVDCaseNo = "CXXAAABBB";
			invoiceLine.US_ADDCaseNo = "CXXAAABBB";
			AssertNoWarningContaining(invoiceLine.US_ADDCaseNoInfo, errorMessage2);
			AssertNoWarningContaining(invoiceLine.US_CVDCaseNoInfo, errorMessage1);
		}

		public void TestCheckADD_CVDAgainstTariffNumber()
		{
			invoiceLine.US_SupTariff = "98020040";
			invoiceLine.US_CVDCaseNo = "CXXAAABBB";
			invoiceLine.US_ADDCaseNo = "CXXAAABBB";
			AssertHasMessageError(invoiceLine.US_ADDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.ADD_CVDNotAllowedForRepairOrAlteration);
			AssertHasMessageError(invoiceLine.US_CVDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.ADD_CVDNotAllowedForRepairOrAlteration);
			invoiceLine.US_CVDCaseNo = "";
			invoiceLine.US_ADDCaseNo = "";
			AssertNoMessageError(invoiceLine.US_ADDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.ADD_CVDNotAllowedForRepairOrAlteration);
			AssertNoMessageError(invoiceLine.US_CVDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.ADD_CVDNotAllowedForRepairOrAlteration);
		}

		public void TestValidateXAndVLine()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			AssertHasMessageError(invoiceLine.US_SecondarySPIInfo, FormalImportAddInfoJobComInvoiceLineValidation.NoComponentLinesForSetHeaderLine);
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine2.JI_ParentID = invoiceLine.PK;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			AssertNoMessageError(invoiceLine.US_SecondarySPIInfo, FormalImportAddInfoJobComInvoiceLineValidation.NoComponentLinesForSetHeaderLine);
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			AssertHasMessageError(invoiceLine.US_SecondarySPIInfo, FormalImportAddInfoJobComInvoiceLineValidation.NoParentLineForAComponentLine);
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine2.JI_ParentID = ZGuid.Empty;
			invoiceLine.JI_ParentID = invoiceLine2.PK;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			AssertNoMessageError(invoiceLine.US_SecondarySPIInfo, FormalImportAddInfoJobComInvoiceLineValidation.NoParentLineForAComponentLine);
		}

		public void TestCheckUS_ManifestQty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			line.US_ManifestQty = 0;
			AssertHasMessageErrorContaining(line.US_ManifestQtyInfo, FormalImportAddInfoJobComInvoiceLineValidation.FTZPackQtyIsRequired);
			line.US_ManifestQty = 10;
			AssertNoMessageErrorContaining(line.US_ManifestQtyInfo, FormalImportAddInfoJobComInvoiceLineValidation.FTZPackQtyIsRequired);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			line.US_ManifestQty = 0;
			AssertNoMessageErrorContaining(line.US_ManifestQtyInfo, FormalImportAddInfoJobComInvoiceLineValidation.FTZPackQtyIsRequired);
		}

		public void TestCheckUS_ZoneStatus()
		{
			JobComInvoiceLine secondLine = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine.US_ZoneStatus = "~";
			AssertNoMessageError(invoiceLine.US_ZoneStatusInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.US_ZoneStatus = "~";
			AssertHasMessageError(invoiceLine.US_ZoneStatusInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_ZoneStatus = "";
			AssertHasMessageErrorContaining(invoiceLine.US_ZoneStatusInfo, FormalImportAddInfoJobComInvoiceLineValidation.ZoneStatusShouldBeEntered);
			AssertNoMessageErrorContaining(secondLine.US_ZoneStatusInfo, FormalImportAddInfoJobComInvoiceLineValidation.ZoneStatusShouldBeEntered);
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			AssertNoMessageErrors(invoiceLine.US_ZoneStatusInfo);
			AssertNoMessageErrorContaining(secondLine.US_ZoneStatusInfo, FormalImportAddInfoJobComInvoiceLineValidation.ZoneStatusShouldBeEntered);
			invoiceLine.US_PrivilegedStatusDate = ZDateTime.Invalid;
			AssertHasNotifications("Preconditions: US_PrivilegedStatusDate validation should add at least 1 notification when invalid date was set in US_PrivilegedStatusDate", invoiceLine.US_PrivilegedStatusDateInfo);
			for (int i = 0; i < invoiceLine.AddInfoLookups.US_ZoneStatusList.Count; i++)
			{
				invoiceLine.US_PrivilegedStatusDate = ZDateTime.Invalid;
				invoiceLine.US_PrivilegedStatusDateInfo.ClearAllNotifications();
				invoiceLine.US_ZoneStatus = invoiceLine.AddInfoLookups.US_ZoneStatusList[i].Code;
				if (invoiceLine.AddInfoLookups.US_ZoneStatusList[i].Code == ZoneStatusList.Codes.PrivilegedForeign)
				{
					AssertHasNotifications("ValidateUS_PrivilegedStatusDate should be called here", invoiceLine.US_PrivilegedStatusDateInfo);
				}
				else
				{
					AssertNoNotifications("ValidateUS_PrivilegedStatusDate should be called here", invoiceLine.US_PrivilegedStatusDateInfo);
				}
			}
		}

		public void TestAgricultureNo()
		{
			#region Setup Ref Db Test Data

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._14, "Agricultural License", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatMask, @"[0-9]{1}-([a-z]{2}|[a-z]{1}\s)-[0-9]{3}-[0-9]{1}");
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USPerLicFormatErrorText, "'N-AA-NNN-N' or 'N-AB-NNN-N' where 'N'-numeric, 'A'-alphabetic, AND 'B'-space.");
			Factory.Save();

			#endregion

			var agricultureLicenseNumberInvalidFormatText = "The Agricultural License number is invalid. The format should be 'N-AA-NNN-N' or 'N-AB-NNN-N' where 'N'-numeric, 'A'-alphabetic, AND 'B'-space.";
			invoiceLine.US_AgricultureLicNo = "5-SS-444-1";
			declaration.US_EntryType = EntryTypeList.Codes.ImmediateExportation;
			AssertEquals(false, invoiceLine.LightValidationIsValid);
			invoiceLine.RunPreSaveValidation();
			AssertHasMessageErrorContaining(invoiceLine.US_AgricultureLicNoInfo, AgriculturalLicenseValidator.AgricultureLicenseNoInvalidEntryType);
			invoiceLine.US_AgricultureLicNoInfo.ClearAllNotifications();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			AssertNoMessageErrorContaining(invoiceLine.US_AgricultureLicNoInfo, AgriculturalLicenseValidator.AgricultureLicenseNoInvalidEntryType);
			invoiceLine.US_AgricultureLicNo = "5-SS-444-K";
			invoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertHasMessageError(invoiceLine.US_AgricultureLicNoInfo, agricultureLicenseNumberInvalidFormatText);
			invoiceLine.US_AgricultureLicNo = "5-SS-444-7";
			invoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertNoMessageError(invoiceLine.US_AgricultureLicNoInfo, agricultureLicenseNumberInvalidFormatText);
			invoiceLine.US_AgricultureLicNo = "5-S-444-7";
			AssertHasMessageError(invoiceLine.US_AgricultureLicNoInfo, agricultureLicenseNumberInvalidFormatText);
			invoiceLine.US_AgricultureLicNo = "G-SS-444-4";
			AssertHasMessageError(invoiceLine.US_AgricultureLicNoInfo, agricultureLicenseNumberInvalidFormatText);
			invoiceLine.US_AgricultureLicNo = "5-3E-444-4";
			AssertHasMessageError(invoiceLine.US_AgricultureLicNoInfo, agricultureLicenseNumberInvalidFormatText);
			invoiceLine.US_AgricultureLicNo = "5-E4-444-4";
			AssertHasMessageError(invoiceLine.US_AgricultureLicNoInfo, agricultureLicenseNumberInvalidFormatText);
			invoiceLine.US_AgricultureLicNo = "5-ER-H44-4";
			AssertHasMessageError(invoiceLine.US_AgricultureLicNoInfo, agricultureLicenseNumberInvalidFormatText);
			invoiceLine.US_AgricultureLicNo = "5-E4-4H4-4";
			AssertHasMessageError(invoiceLine.US_AgricultureLicNoInfo, agricultureLicenseNumberInvalidFormatText);
			invoiceLine.US_AgricultureLicNo = "5-E4-44H-4";
			AssertHasMessageError(invoiceLine.US_AgricultureLicNoInfo, agricultureLicenseNumberInvalidFormatText);
			invoiceLine.US_AgricultureLicNo = "5-E4-444-L";
			AssertHasMessageError(invoiceLine.US_AgricultureLicNoInfo, agricultureLicenseNumberInvalidFormatText);
			invoiceLine.US_VisaNo = "223";
			invoiceLine.US_AgricultureLicNo = "5-SS-444-7";
			AssertHasMessageError(invoiceLine.US_AgricultureLicNoInfo, AgriculturalLicenseValidator.AgricultureLicenseNoCannotBeEnteredIfTextileInfoEntered);
			invoiceLine.US_VisaNo = "";
			invoiceLine.US_TextileCategoryNo = "REE";
			invoiceLine.US_AgricultureLicNo = "5-SS-444-7";
			AssertHasMessageError(invoiceLine.US_AgricultureLicNoInfo, AgriculturalLicenseValidator.AgricultureLicenseNoCannotBeEnteredIfTextileInfoEntered);
			invoiceLine.US_TextileCategoryNo = "";
			invoiceLine.US_DateOfExportFromCountryOfOrigin = ZDateTime.Today;
			invoiceLine.US_AgricultureLicNo = "5-SS-444-7";
			AssertHasMessageError(invoiceLine.US_AgricultureLicNoInfo, AgriculturalLicenseValidator.AgricultureLicenseNoCannotBeEnteredIfTextileInfoEntered);
			invoiceLine.US_DateOfExportFromCountryOfOrigin = ZDateTime.Empty;
			invoiceLine.US_AgricultureLicNo = "5-SS-444-7";
			AssertNoMessageError(invoiceLine.US_AgricultureLicNoInfo, AgriculturalLicenseValidator.AgricultureLicenseNoCannotBeEnteredIfTextileInfoEntered);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_AgricultureLicNo = "5-SS-444-7";
			AssertHasMessageError(invoiceLine.US_AgricultureLicNoInfo, string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.LicenseAndPermit));
		}

		public void TestCAFTAOriginatingClaim()
		{
			var hnCountry = Factory.LoadTop1<USCCountry>(new ZQuery(USCCountrySchema.UC_ISOCountryCode, "HN"));
			if (hnCountry == null)
			{
				hnCountry = Factory.New<USCCountry>();
				hnCountry.UC_ISOCountryCode = "HN";
			}

			hnCountry.UC_GSPIndicator = true;
			hnCountry.UC_GSPBeginDate = ZDateTime.BrettsBirthday;
			hnCountry.UC_GSPEndDate = ZDateTime.Today.AddYears(1);
			hnCountry.UC_MiscellaneousSPIIndicator = "P";
			hnCountry.UC_MiscellaneousSPIBeginDate = ZDateTime.BrettsBirthday;
			hnCountry.UC_MiscellaneousSPIEndDate = ZDateTime.Today.AddYears(1);
			Factory.Save();
			invoiceLine.US_UC_NKCountryOfOrigin = "HN";
			invoiceLine.US_UC_NKCountryOfExport = "HN";
			invoiceLine.CountryOfOrigin_US.UC_MiscellaneousSPIEndDate = ZDateTime.Empty;
			invoiceLine.CountryOfExport_US.UC_MiscellaneousSPIEndDate = ZDateTime.Empty;
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.P;
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CAFTAOriginatingClaim);
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CAFTAOriginatingClaim);
			invoiceLine.US_UC_NKCountryOfExport = "DE";
			AssertHasMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CAFTAOriginatingClaim);
		}

		public void TestFASClaim()
		{
			invoiceLine.US_UC_NKCountryOfOrigin = "HN";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.Z;
			AssertHasMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.FASClaimRequiresSameOriginExport);
			declaration.JE_RL_NKPortOfLoading = "HNGJA";
			invoiceLine.AddInfoValidation.ValidateUS_SPI();
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.FASClaimRequiresSameOriginExport);
		}

		public void TestGSPOrigin()
		{
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.A;
			AssertHasMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.GSPCountryOfOrigin);
			invoiceLine.US_UC_NKCountryOfOrigin = "VU";
			invoiceLine.AddInfoValidation.ValidateUS_SPI();
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.GSPCountryOfOrigin);
		}

		public void TestGSPExport()
		{
			invoice.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.A;
			AssertHasMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.GSPCountryOfExport);
			invoice.US_UC_NKCountryOfExport = "VU";
			invoiceLine.AddInfoValidation.ValidateUS_SPI();
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.GSPCountryOfExport);
		}

		public void TestCheckUS_PrivilegedStatusDate()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableENS = true;
			invoiceLine.US_PrivilegedStatusDate = ZDateTime.Today.AddDays(1);
			AssertHasMessageError(invoiceLine.US_PrivilegedStatusDateInfo, FormalImportAddInfoJobComInvoiceLineValidation.PrivilegedStatusDateCannotBeFuture);
			invoiceLine.US_PrivilegedStatusDate = ZDateTime.Today;
			AssertNoMessageError(invoiceLine.US_PrivilegedStatusDateInfo, FormalImportAddInfoJobComInvoiceLineValidation.PrivilegedStatusDateCannotBeFuture);
			for (int i = 0; i < invoiceLine.AddInfoLookups.US_ZoneStatusList.Count; i++)
			{
				invoiceLine.US_ZoneStatus = invoiceLine.AddInfoLookups.US_ZoneStatusList[i].Code;
				invoiceLine.US_PrivilegedStatusDate = ZDateTime.Empty;
				if (invoiceLine.US_ZoneStatus == ZoneStatusList.Codes.PrivilegedForeign)
				{
					AssertHasMessageErrorContaining(invoiceLine.US_PrivilegedStatusDateInfo, FormalImportAddInfoJobComInvoiceLineValidation.PrivilegedStatusDateShouldBeEntered);
				}
				else
				{
					AssertNoMessageErrorContaining(invoiceLine.US_PrivilegedStatusDateInfo, FormalImportAddInfoJobComInvoiceLineValidation.PrivilegedStatusDateShouldBeEntered);
				}
			}

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			invoiceLine.US_PrivilegedStatusDate = ZDateTime.Empty;
			AssertNoMessageErrorContaining(invoiceLine.US_PrivilegedStatusDateInfo, FormalImportAddInfoJobComInvoiceLineValidation.PrivilegedStatusDateShouldBeEntered);
		}

		public void TestADDorCVDDetailsForDomesticMerchandise()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.Domestic;
			AssertEquals("precondition", true, invoiceLine.IsDomesticMerchandise);
			invoiceLine.US_ADDCaseNo = "A9802008";
			invoiceLine.US_CVDCaseNo = "C9802008";
			AssertHasMessageError(invoiceLine.US_ADDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.NoADDOrCVDForDomesticMerchandise);
			AssertHasMessageError(invoiceLine.US_CVDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.NoADDOrCVDForDomesticMerchandise);
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.NonPrivilegedForeign;
			AssertNoMessageError(invoiceLine.US_ADDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.NoADDOrCVDForDomesticMerchandise);
			AssertNoMessageError(invoiceLine.US_CVDCaseNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.NoADDOrCVDForDomesticMerchandise);
		}

		public void TestValidatePrimarySPIAgainstCountryOfExportAndOrigin()
		{
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.D;
			AssertHasMessageErrorContaining(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.InvalidSPIIndicatorForCountryOfExport);
			AssertHasMessageErrorContaining(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.InvalidSPIIndicatorForCountryOfOrigin);
			foreach (var cv in Factory.Load<USCCountry>(new ZQuery(USCCountrySchema.UC_Code, new ZString[] { "CV", "ST" })))
			{
				cv.UC_SpecialTradeProgramsBeginDate = ZDateTime.Today.AddYears(-1);
				cv.UC_SpecialTradeProgramsEndDate = ZDateTime.Today.AddYears(1);
			}

			invoiceLine.US_UC_NKCountryOfExport = "CV";
			AssertNoMessageErrorContaining(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.InvalidSPIIndicatorForCountryOfExport);
			invoiceLine.US_UC_NKCountryOfOrigin = "ST";
			AssertNoMessageErrorContaining(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.InvalidSPIIndicatorForCountryOfOrigin);
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.R;
			AssertHasMessageErrorContaining(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.InvalidSPIIndicatorForCountryOfExport);
			AssertHasMessageErrorContaining(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.InvalidSPIIndicatorForCountryOfOrigin);
			invoiceLine.US_UC_NKCountryOfExport = "TT";
			AssertNoMessageErrorContaining(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.InvalidSPIIndicatorForCountryOfExport);
			invoiceLine.US_UC_NKCountryOfOrigin = "JM";
			AssertNoMessageErrorContaining(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.InvalidSPIIndicatorForCountryOfOrigin);
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.J;
			AssertHasMessageErrorContaining(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.InvalidSPIIndicatorForCountryOfExport);
			AssertHasMessageErrorContaining(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.InvalidSPIIndicatorForCountryOfOrigin);
			USCCountry ec = Factory.LoadFromNaturalKey<USCCountry>(ZArchitecture.Schema.USCCountrySchema.UC_Code, "EC");
			ec.UC_SPIEndDate = ZDateTime.Empty;
			invoiceLine.US_UC_NKCountryOfExport = "EC";
			AssertNoMessageErrorContaining(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.InvalidSPIIndicatorForCountryOfExport);
			USCCountry pe = Factory.LoadFromNaturalKey<USCCountry>(ZArchitecture.Schema.USCCountrySchema.UC_Code, "PE");
			pe.UC_SPIEndDate = ZDateTime.Empty;
			invoiceLine.US_UC_NKCountryOfOrigin = "PE";
			AssertNoMessageErrorContaining(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.InvalidSPIIndicatorForCountryOfOrigin);
			invoiceLine.US_SPI = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.InvalidSPIIndicatorForCountryOfExport);
			AssertNoMessageErrorContaining(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.InvalidSPIIndicatorForCountryOfOrigin);
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.K;
			AssertNoMessageErrorContaining(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.InvalidSPIIndicatorForCountryOfExport);
			AssertNoMessageErrorContaining(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.InvalidSPIIndicatorForCountryOfOrigin);
		}

		public void TestCheckUS_SecondarySPI()
		{
			AssertUS_SecondarySPI(invoiceLine);
		}

		public void TestCheckUS_SWPMIndicator()
		{
			invoiceLine.US_SWPMIndicator = "~";
			AssertHasMessageError(invoiceLine.US_SWPMIndicatorInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_SWPMIndicator = SWPMList.Codes._1;
			AssertNoMessageError(invoiceLine.US_SWPMIndicatorInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine.US_SWPMIndicator = SWPMList.Codes._2;
			AssertHasWarning(invoiceLine.US_SWPMIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.SWPMIndicatorForCN_HK);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_SWPMIndicator = SWPMList.Codes._1;
			AssertHasMessageError(invoiceLine.US_SWPMIndicatorInfo, string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.LicenseAndPermit));
		}

		[TestDate(2009, 12, 12)]
		public void TestCheckUS_SPIWithSPICountryCode()
		{
			invoiceLine.US_SPI = SpecialProgramList.Codes.JPlus;
			AssertHasMessageError(invoiceLine.US_SPIInfo, "Andean Trade Promotion and Drug Eradication Act (ATPDEA) is only supported when importing goods of an origin of Colombia, Bolivia, Ecuador or Peru.");
			invoiceLine.US_UC_NKCountryOfOrigin = "PE";
			AssertNoMessageError(invoiceLine.US_SPIInfo, "Andean Trade Promotion and Drug Eradication Act (ATPDEA) is only supported when importing goods of an origin of Colombia, Bolivia, Ecuador or Peru.");
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000111122";
			tariff.UE_SPICode = "AU";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(100);
			invoiceLine.JI_Tariff = "0000111122";
			invoiceLine.US_SPI = "IL";
			AssertHasMessageErrorContaining(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.NotEligibleForSPI);
			invoiceLine.US_SPI = "AU";
			AssertNoMessageErrorContaining(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.NotEligibleForSPI);
		}

		public void TestIsraelFTAWithEgyptCountryOfExport()
		{
			declaration.Invoices[0].US_UC_NKCountryOfExport = "EG";
			invoiceLine.US_UC_NKCountryOfOrigin = "EG";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.N;
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.IsraelFTAExportError);
			declaration.Invoices[0].US_UC_NKCountryOfExport = "AU";
			invoiceLine.AddInfoValidation.ValidateUS_SPI();
			AssertHasMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.IsraelFTAExportError);
		}

		public void TestIsraelFTAWithEgyptCountryOfOrigin()
		{
			declaration.JE_RL_NKOrigin = "ILAAA";
			invoiceLine.US_UC_NKCountryOfOrigin = "EG";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.N;
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.IsraelFTAOriginError);
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.AddInfoValidation.ValidateUS_SPI();
			AssertHasMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.IsraelFTAOriginError);
		}

		public void TestCountervailingCaseNoWhenEmpty()
		{
			USCACCase case1 = Factory.New<USCACCase>();
			case1.U5_CaseNumber = "C9085290";
			case1.U5_ISOCountryCode = "KR";
			case1.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			case1.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var tariff1 = case1.CaseTariffs.AddNew();
			tariff1.U9_TariffNumber = "0000000000";
			USCTariff importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000000000";
			importTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			importTariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			importTariff.UE_CountervailingDutyFlag = true;
			Factory.Save();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000000000";
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			AssertHasMessageError(invoiceLine.US_CVDCaseNoInfo, string.Format(ADD_CVDLiabilityChecker.MayBeSubjectToADD_CVD, "CVD", "C9085290"));
			invoiceLine.US_CVD_NA = true;
			AssertNoMessageError(invoiceLine.US_ADDCaseNoInfo, string.Format(ADD_CVDLiabilityChecker.MayBeSubjectToADD_CVD, "CVD", "C9085290"));
		}

		public void TestCountervailingDepositValue()
		{
			invoiceLine.US_CVDDepositValue = 123m;
			AssertHasMessageError(invoiceLine.US_CVDDepositValueInfo, FormalImportAddInfoJobComInvoiceLineValidation.ADD_CVDDetailsEnteredWithoutCaseNumber);
			invoiceLine.US_CVDCaseNo = "C";
			AssertNoMessageError(invoiceLine.US_CVDDepositValueInfo, FormalImportAddInfoJobComInvoiceLineValidation.ADD_CVDDetailsEnteredWithoutCaseNumber);
		}

		[TestDate(2009, 12, 12)]
		public void TestUS_UC_NKCountryOfOriginForBorderCargoRelease()
		{
			declaration.JE_TransportMode = declaration.TransportModeRoadCodeForTesting;
			declaration.ValidationModes = ValidationModes.CargoRelease;
			AssertUS_UC_NKCountryOfOrigin(invoiceLine);
			AssertOnCountryInRelationToSpecialTradeAgreement(invoiceLine.US_UC_NKCountryOfOriginInfo);
		}

		[TestDate(2009, 12, 12)]
		public void TestUS_UC_NKCountryOfOrigin()
		{
			AssertEquals("IsCargoReleaseValidationMode", true, declaration.IsCargoReleaseValidationMode);
			AssertUS_UC_NKCountryOfOrigin(invoiceLine);
			AssertOnCountryInRelationToSpecialTradeAgreement(invoiceLine.US_UC_NKCountryOfOriginInfo);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			invoiceLine.US_UC_NKCountryOfOrigin = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.US_UC_NKCountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Albania;
			AssertNoMessageErrorContaining(invoiceLine.US_UC_NKCountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCountryOfExportAndOriginShouldBeSameForCBTPA()
		{
			invoiceLine.US_SupTariff = USCTariff.CBTPABenefitsApplicable;
			invoiceLine.US_UC_NKCountryOfExport = "";
			invoiceLine.InvoiceHeader.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "NZ";
			AssertHasMessageError(invoiceLine.US_UC_NKCountryOfOriginInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.CountryOfExportAndOriginShouldBeSame);
			AssertHasMessageError(invoiceLine.US_UC_NKCountryOfExportInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.CountryOfExportAndOriginShouldBeSame);
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			AssertNoMessageError(invoiceLine.US_UC_NKCountryOfOriginInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.CountryOfExportAndOriginShouldBeSame);
			AssertNoMessageError(invoiceLine.US_UC_NKCountryOfExportInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.CountryOfExportAndOriginShouldBeSame);
			invoiceLine.US_UC_NKCountryOfExport = "NZ";
			AssertHasMessageError(invoiceLine.US_UC_NKCountryOfOriginInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.CountryOfExportAndOriginShouldBeSame);
			AssertHasMessageError(invoiceLine.US_UC_NKCountryOfExportInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.CountryOfExportAndOriginShouldBeSame);
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			AssertNoMessageError(invoiceLine.US_UC_NKCountryOfOriginInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.CountryOfExportAndOriginShouldBeSame);
			AssertNoMessageError(invoiceLine.US_UC_NKCountryOfExportInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.CountryOfExportAndOriginShouldBeSame);
		}

		[TestDate(2009, 12, 12)]
		public void TestCheckUS_UC_NKCountryOfExport()
		{
			AssertUS_UC_NKCountryOfExport(invoiceLine);
			AssertOnCountryInRelationToSpecialTradeAgreement(invoiceLine.US_UC_NKCountryOfExportInfo);
		}

		public void TestManufacturerIDAgainstCountryOfOrigin()
		{
			OrgHeader manufacturer = Factory.New<OrgHeader>();
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AU11167");
			OrgHeader canadianManufacturer = Factory.New<OrgHeader>();
			canadianManufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "XA11167");
			canadianManufacturer.MiscServ.OM_RN_NKEXDefaultCntryOfOrigin = Core.Constants.CountryCodes.Canada;
			invoiceLine.InvoiceHeader.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			invoiceLine.US_UC_NKCountryOfExport = "CA";
			invoiceLine.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XT;
			invoiceLine.AddInfoValidation.ValidateUS_UC_NKCountryOfOrigin();
			AssertHasMessageError(invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.Canadian);
			invoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			invoiceLine.US_UC_NKCountryOfExport = "CA";
			invoiceLine.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XT;
			invoiceLine.AddInfoValidation.ValidateUS_UC_NKCountryOfOrigin();
			AssertHasMessageError(invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.Canadian);
			invoiceLine.JI_OA_ManufacturerAddress = canadianManufacturer.MainAddress.PK;
			invoiceLine.US_UC_NKCountryOfExport = "CA";
			invoiceLine.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XT;
			invoiceLine.AddInfoValidation.ValidateUS_UC_NKCountryOfOrigin();
			AssertNoMessageError(invoiceLine.US_UC_NKCountryOfOriginInfo, ManufacturerIDValidator.Constants.Canadian);
		}

		[TestDate(2007, 2, 12)]
		public void TestTariffForUnsupportedGSP()
		{
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Colombia;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Colombia;
			invoiceLine.JI_Tariff = "0603107010";
			invoiceLine.US_SPI = "A";
			AssertHasMessageErrorContaining(invoiceLine.US_SPIInfo, "is excluded for GSP for the selected tariff");
			invoiceLine.JI_Tariff = "9201900000";
			invoiceLine.US_SPI = "A";
			AssertNoMessageErrorContaining(invoiceLine.US_SPIInfo, "is excluded for GSP for the selected tariff");
		}

		// Rule 386
		public void TestGSPForAssociatedCountries()
		{
			invoiceLine.US_SPI = "A";
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Bolivia;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Botswana;
			invoiceLine.JI_Tariff = "0603107010";
			AssertHasMessageErrorContaining(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.OriginAndExportMustBeTheSameForGSP);
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Colombia;
			AssertNoMessageErrorContaining(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.OriginAndExportMustBeTheSameForGSP);
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Bolivia;
			AssertNoMessageErrorContaining(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.OriginAndExportMustBeTheSameForGSP);
		}

		public void TestCheckAPlusSPI()
		{
			invoiceLine.JI_Tariff = "2008.20.0090";
			AssertEquals("PreCondition:A+ is in the list of SPIs", true, invoiceLine.ImportTariff.HasSpecialProgramsIndicator("A+"));
			invoiceLine.US_UC_NKCountryOfOrigin = "RU";
			invoiceLine.US_SPI = "A";
			AssertHasMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.TariffValidOnlyForColumn3Country);
			invoiceLine.US_UC_NKCountryOfOrigin = "UG";
			AssertNoMessageError(invoiceLine.US_SPIInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.TariffValidOnlyForColumn3Country);
		}

		[TestDate(2008, 9, 11)]
		public void TestNotApplicablePrimarySPI()
		{
			invoiceLine.JI_Tariff = "0603107010";
			invoiceLine.US_SPI = "R";
			AssertHasMessageError(invoiceLine.US_SPIInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestWhenNoDutyCalculationFormulaAvailable()
		{
			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "0000000000";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.Now;
			tariff1.UE_DutyComputationCode = ComputationCodeList.Codes.NoComputationFormulaAvailable;
			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "0000000001";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.Now;
			tariff2.UE_DutyComputationCode = ComputationCodeList.Codes.NoComputationFormulaAvailable;
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_RuleCode = TariffRuleList.Codes.RepairTariffs;
			tariffRule.U1_Tariff = "0000000001";
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			invoiceLine.US_OverrideDuty = false;
			invoiceLine.JI_Tariff = tariff1.UE_Tariff;
			AssertHasWarning(invoiceLine.US_OverrideDutyInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.NoDutyComputationFormulaAvailableCheckDutyAmount);
			invoiceLine.US_OverrideDuty = true;
			AssertNoWarning(invoiceLine.US_OverrideDutyInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.NoDutyComputationFormulaAvailableCheckDutyAmount);
			invoiceLine.US_OverrideDuty = false;
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			AssertNoWarning(invoiceLine.US_OverrideDutyInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.NoDutyComputationFormulaAvailableCheckDutyAmount);
			invoiceLine.US_OverrideDuty = true;
			AssertNoWarning(invoiceLine.US_OverrideDutyInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.NoDutyComputationFormulaAvailableCheckDutyAmount);

			invoiceLine.US_SupTariff = tariff1.UE_Tariff;
			AssertHasWarning(invoiceLine.US_OverrideSupDutyInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.NoDutyComputationFormulaAvailableCheckDutyAmount);
			invoiceLine.US_OverrideSupDuty = true;
			AssertNoWarning(invoiceLine.US_OverrideSupDutyInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.NoDutyComputationFormulaAvailableCheckDutyAmount);
			invoiceLine.US_OverrideSupDuty = false;
			invoiceLine.US_SupTariff = tariff2.UE_Tariff;
			AssertNoWarning(invoiceLine.US_OverrideSupDutyInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.NoDutyComputationFormulaAvailableCheckDutyAmount);
			invoiceLine.US_OverrideSupDuty = true;
			AssertNoWarning(invoiceLine.US_OverrideSupDutyInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.NoDutyComputationFormulaAvailableCheckDutyAmount);
		}

		[TestDate(2008, 2, 17)]
		public void TestSilkWithCategory733()
		{
			invoiceLine.JI_Tariff = "5007106030";
			invoiceLine.US_TextileCategoryNo = "733";
			invoiceLine.US_VisaNo = "123";
			AssertNoMessageErrorContaining(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.SilkFromChina);
			invoiceLine.US_UC_NKCountryOfExport = "CN";
			AssertHasMessageError(invoiceLine.US_VisaNoInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.SilkFromChina);
		}

		public void TestFDAIndicator()
		{
			//FDA Admissibility Required
			invoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			invoiceLine.US_FDAIndicator = "~";
			AssertHasMessageError(invoiceLine.US_FDAIndicatorInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_FDAIndicatorInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_FDAIndicator = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDARequiredButBlank);
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDARequiredButBlank);
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDARequiredButDisclaimed);
			Assert(!invoiceLine.US_FDAIndicatorInfo.HasMessageError(CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDADisclaimedInvalid));
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDARequiredButDisclaimed);
			AssertHasMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDALineRequired);
			invoiceLine.FDAs.AddNew();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDALineRequired);
			invoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewMayBeRequiredTariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDALineNotAllowed);
			Assert(!invoiceLine.US_FDAIndicatorInfo.HasMessageError(CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDADisclaimedInvalid));
			invoiceLine.FDAs.RemoveAndDeleteAll();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertNoMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDALineNotAllowed);
			//Do Not Submit
			invoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewDONOTSUBMITTariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDAMustNotBeSubmitted);
			Assert(!invoiceLine.US_FDAIndicatorInfo.HasMessageError(CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDADisclaimedInvalid));
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDAMustNotBeSubmitted);
			invoiceLine.US_FDAIndicator = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDAMustNotBeSubmitted);
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_DomesticCargo = true;
			invoiceLine.US_FDAIndicator = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDARequiredButBlank);
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertNoMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDARequiredButDisclaimed);
			AssertNoMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDALineNotAllowed);
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDALineRequired);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			invoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			invoiceLine.ImportTariff.UE_PGACodes = invoiceLine.ImportTariff.UE_OGACodes;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			string message = AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedIsNotAllowedForRequiredTariff;
			AssertHasMessageError(invoiceLine.US_FDAIndicatorInfo, message);
			var req = (OGAAgencyRequirement)invoiceLine.OGAAgencyRequirements.FirstOrDefault(x => ((OGAAgencyRequirement)x).AgencyCode == GovernmentAgencyProgramCodeList.Codes.FDA);
			AssertHasMessageError(req.IndicatorInfo, message);
			invoiceLine.US_FDAIndicator = ZString.Empty;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_DomesticCargo = true;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertNoMessageError(invoiceLine.US_FDAIndicatorInfo, message);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.US_FTZCurrentTariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(invoiceLine.US_FDAIndicatorInfo, message);
			invoiceLine.US_FTZCurrentTariff = ZString.Empty;
			invoiceLine.JI_Tariff = "4407100115";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			var warningMessage = "The tariff does not indicate that FDA reporting is required.";
			AssertHasWarning(invoiceLine.US_FDAIndicatorInfo, warningMessage);
			req = (OGAAgencyRequirement)invoiceLine.OGAAgencyRequirements.FirstOrDefault(x => ((OGAAgencyRequirement)x).AgencyCode == GovernmentAgencyProgramCodeList.Codes.FDA);
			AssertHasWarning(req.IndicatorInfo, warningMessage);
			invoiceLine.US_FDAIndicator = "";
			AssertNoMessageError(invoiceLine.US_FDAIndicatorInfo, message);
			AssertNoWarning(invoiceLine.US_FDAIndicatorInfo, warningMessage);
			AssertNoWarning(req.IndicatorInfo, warningMessage);
			message = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequiredIfNotApplicable, "FDA", EntryTypeList.Codes.WarehouseWithdrawalConsumption);
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_CertifyCargoRelease = false;
			invoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasMessageErrorContaining(invoiceLine.US_FDAIndicatorInfo, message);
			invoiceLine.FDAs.RemoveAndDeleteAll();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasMessageError(invoiceLine.US_FDAIndicatorInfo, string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, "FDA"));
		}

		public void TestFDADisclaimReason()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "FD3AL2";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.JI_Tariff = "1000000001";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_FDADisclaimReason = PGADisclaimReasonList.Codes.F;
			AssertHasMessageError(invoiceLine.US_FDADisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_FDADisclaimReason = "";
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			invoiceLine.US_FDADisclaimReason = PGADisclaimReasonList.Codes.F;
			AssertNoMessageError(invoiceLine.US_FDADisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestDOTIndicator()
		{
			invoiceLine.JI_Tariff = USCTariff.DOTIsApplicable;
			invoiceLine.US_DOTIndicator = "~";
			AssertHasMessageError(invoiceLine.US_DOTIndicatorInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_DOTIndicatorInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_DOTIndicator = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTRequiredButBlank);
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			invoiceLine.US_DOTIndicator = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTRequiredButBlank);
			invoiceLine.Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertNoMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTRequiredButBlank);
			AssertHasMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTRequiredButDisclaimed);
			AssertNoMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTDisclaimedInvalid);
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTRequiredButDisclaimed);
			AssertHasMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTLineRequired);
			AssertNoWarning(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTNotRequired);
			invoiceLine.DOTs.AddNew();
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTLineRequired);
			invoiceLine.JI_Tariff = USCTariff.DOTMayBeApplicable;
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTLineNotAllowed);
			AssertNoMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTDisclaimedInvalid);
			var req = (OGAAgencyRequirement)invoiceLine.OGAAgencyRequirements.FirstOrDefault(x => ((OGAAgencyRequirement)x).AgencyCode == GovernmentAgencyProgramCodeList.Codes.DOT);
			AssertHasMessageError(req.IndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTLineNotAllowed);
			invoiceLine.DOTs.RemoveAndDeleteAll();
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertNoMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTLineNotAllowed);
			AssertNoMessageError(req.IndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTLineNotAllowed);
			invoiceLine.JI_Tariff = "4407100115";
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTDisclaimedInvalid);
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTNotRequired);
			invoiceLine.US_DOTIndicator = "";
			AssertNoMessageError(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTDisclaimedInvalid);
			AssertNoWarning(invoiceLine.US_DOTIndicatorInfo, FormalImportAddInfoJobComInvoiceLineValidation.Constants.DOT.DOTNotRequired);
			invoiceLine.DOTs.RemoveAndDeleteAll();
			invoiceLine.JI_Tariff = USCTariff.DOTIsApplicable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasMessageError(invoiceLine.US_DOTIndicatorInfo, string.Format(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.DataIsNotNeededForXLine, "DOT"));
		}

		public void TestCheckUS_98GoodsValue()
		{
			JobComInvoiceLine parentInvoiceLine = invoice.InvoiceLines.AddNew();
			parentInvoiceLine.US_SupTariff = "9822005060";
			parentInvoiceLine.US_98GoodsValue = 100m;
			AssertHasMessageError(parentInvoiceLine.US_98GoodsValueInfo, FormalImportAddInfoJobComInvoiceLineValidation._98GoodsValueExpectedToBeEnteredOnlyFor9801And9802);
			parentInvoiceLine.US_98GoodsValue = 0m;
			AssertNoMessageError(parentInvoiceLine.US_98GoodsValueInfo, FormalImportAddInfoJobComInvoiceLineValidation._98GoodsValueExpectedToBeEnteredOnlyFor9801And9802);
			parentInvoiceLine.US_SupTariff = "9802";
			parentInvoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			var childVLine = parentInvoiceLine.AddSecondaryInvoiceLine();
			childVLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			childVLine.US_98GoodsValue = 200m;
			parentInvoiceLine.US_98GoodsValue = 100m;
			AssertHasWarning(parentInvoiceLine.US_98GoodsValueInfo, ImportLinePriceValidator.LinePriceOfXWillBeIgnoredFor7501);
			parentInvoiceLine.US_98GoodsValue = 200m;
			AssertNoWarning(parentInvoiceLine.US_98GoodsValueInfo, ImportLinePriceValidator.LinePriceOfXWillBeIgnoredFor7501);
		}

		//delete TestCheckUS_TSCAInd
		public void TestCheckUS_TSCAInd()
		{
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "FW1EP8";
			invoiceLine.JI_Tariff = "1000000001";
			invoiceLine.US_TSCAInd = "~";
			AssertHasMessageError(invoiceLine.US_TSCAIndInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_TSCAInd = "";
			var warnText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGARequiredButBlank, "TSCA");
			AssertHasMessageError(invoiceLine.US_TSCAIndInfo, warnText);
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_TSCAIndInfo, warnText);
			invoiceLine.US_TSCAInd = ZString.Empty;
			AssertNoWarning(invoiceLine.US_TSCAIndInfo, warnText);
		}

		public void TestCheckUS_OMCInd()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0407000020";
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff.UE_PGACodes = "OM2";
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "4801000020";
			tariff2.UE_DateFrom = ZDateTime.Today;
			tariff2.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff2.UE_PGACodes = "OM1";
			Factory.Save();
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USOMC, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				invoiceLine.US_OMCInd = "~";
				AssertHasMessageError(invoiceLine.US_OMCIndInfo, ListValidation.InvalidCodeMessageError);
				invoiceLine.JI_Tariff = tariff.UE_Tariff;
				invoiceLine.US_OMCInd = OGAIndicatorList.Codes.Declared;
				var errorText2 = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequired, "OMC");
				AssertHasMessageError(invoiceLine.US_OMCIndInfo, errorText2);
				var omcHeader = invoiceLine.OMCHeaders.AddNew();
				invoiceLine.AddInfoValidation.ValidateUS_OMCInd();
				AssertNoMessageError(invoiceLine.US_OMCIndInfo, errorText2);
				invoiceLine.OMCHeaders.RemoveAndDeleteAll();
				invoiceLine.JI_Tariff = tariff2.UE_Tariff;
				invoiceLine.US_OMCInd = OGAIndicatorList.Codes.Declared;
				errorText2 = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequired, "OMC");
				AssertHasMessageError(invoiceLine.US_OMCIndInfo, errorText2);
				var header2 = invoiceLine.OMCHeaders.AddNew();
				invoiceLine.AddInfoValidation.ValidateUS_OMCInd();
				AssertNoMessageError(invoiceLine.US_OMCIndInfo, errorText2);
				errorText2 = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequiredIfNotApplicable, "OMC", EntryTypeList.Codes.Warehouse);
				declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
				declaration.US_EnableCRL = false;
				declaration.US_CertifyCargoRelease = true;
				invoiceLine.US_OMCInd = OGAIndicatorList.Codes.Declared;
				AssertHasMessageErrorContaining(invoiceLine.US_OMCIndInfo, errorText2);
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
				invoiceLine.US_OMCInd = OGAIndicatorList.Codes.Disclaimed;
				AssertHasMessageError(invoiceLine.US_OMCIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
				invoiceLine.US_OMCInd = OGAIndicatorList.Codes.Declared;
				AssertHasMessageError(invoiceLine.US_OMCIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
				invoiceLine.US_OMCInd = ZString.Empty;
				AssertNoMessageError(invoiceLine.US_OMCIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
			}
		}

		[TestDate(2016, 9, 18)]
		public void TestCheckUS_AMSDisclaimProgramWithAB()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "7201100000";
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			var tariffRule2 = Factory.New<USCTariffRule>();
			tariffRule2.U1_RuleCode = TariffRuleList.Codes.HTSExemptFromAMSMO8ProgramRequirement;
			tariffRule2.U1_Tariff = "7201100000";
			tariffRule2.U1_DateFrom = ZDateTime.Now.AddYears(-1);
			Factory.Save();
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
			tariffRule2.U1_RuleCode = TariffRuleList.Codes.HTSExemptFromAMSMO8ProgramRequirement;
			tariffRule2.Factory.Save();
			invoiceLine.US_AMSDisclaimReason = PGADisclaimReasonList.Codes.B;
			invoiceLine.US_AMSDisclaimProgram = "MO8";
			AssertNoMessageError(invoiceLine.US_AMSDisclaimProgramInfo, FormalImportAddInfoJobComInvoiceLineValidation.AMSDisclaimedReasonShouldBeB);
			invoiceLine.US_AMSDisclaimProgram = "";
			invoiceLine.US_AMSDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.US_AMSDisclaimProgram = "MO8";
			AssertHasMessageError(invoiceLine.US_AMSDisclaimProgramInfo, FormalImportAddInfoJobComInvoiceLineValidation.AMSDisclaimedReasonShouldBeB);
		}

		[TestDate(2016, 9, 23)]
		public void TestCheckUS_AMSDisclaimProgramWithMO8()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0805100040";
			tariff.UE_DateFrom = ZDateTime.Now.AddYears(-2);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(2);
			var tariffRule2 = Factory.New<USCTariffRule>();
			tariffRule2.U1_Tariff = "0805100040";
			tariffRule2.U1_RuleCode = TariffRuleList.Codes.HTSExemptFromAMSMO7ProgramRequirement;
			tariffRule2.U1_DateFrom = ZDateTime.Now.AddYears(-1);
			tariffRule2.U1_DateTo = ZDateTime.Now.AddYears(1);
			Factory.Save();
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			CombineAssertions(() =>
			{
				invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.JI_Tariff = tariff.UE_Tariff;
				invoiceLine.US_AMSDisclaimProgram = "MO8";
				invoiceLine.US_AMSDisclaimReason = PGADisclaimReasonList.Codes.B;
				invoiceLine.AddInfoValidation.ValidateUS_AMSDisclaimProgram();
				AssertNoMessageError(invoiceLine.US_AMSDisclaimProgramInfo, FormalImportAddInfoJobComInvoiceLineValidation.AMSDisclaimedReasonShouldBeB);
				invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
				invoiceLine.US_AMSDisclaimReason = PGADisclaimReasonList.Codes.A;
				invoiceLine.AddInfoValidation.ValidateUS_AMSDisclaimProgram();
				AssertHasMessageError(invoiceLine.US_AMSDisclaimProgramInfo, FormalImportAddInfoJobComInvoiceLineValidation.AMSDisclaimedReasonShouldBeB);
			});
		}

		[TestDate(2016, 7, 23)]
		public void TestCheckUS_AMSDisclaimProgram()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_AMSDisclaimProgram = "";
			invoiceLine.AddInfoValidation.ValidateUS_AMSDisclaimProgram();
			AssertHasMessageErrorContaining(invoiceLine.US_AMSDisclaimProgramInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.US_AMSDisclaimProgram = "~";
			AssertHasMessageErrorContaining(invoiceLine.US_AMSDisclaimProgramInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_AMSDisclaimProgram = "MO8";
			AssertNoMessageErrorContaining(invoiceLine.US_AMSDisclaimProgramInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(invoiceLine.US_AMSDisclaimProgramInfo, ListValidation.InvalidCodeMessageError);
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "7201100000";
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			var tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_RuleCode = TariffRuleList.Codes.HTSExemptFromAMSMO7ProgramRequirement;
			tariffRule.U1_Tariff = "7201100000";
			tariffRule.U1_DateFrom = ZDateTime.Now.AddYears(-1);
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_AMSDisclaimProgram = "MO8";
			AssertHasMessageError(invoiceLine.US_AMSDisclaimProgramInfo, string.Format(FormalImportAddInfoJobComInvoiceLineValidation.AMSDisclaimedNotRequired, "MO8", TariffRuleList.Codes.HTSExemptFromAMSMO8ProgramRequirement));
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "7201100001";
			tariff2.UE_DateFrom = ZDateTime.Today;
			tariff2.UE_DateTo = ZDateTime.Today.AddYears(1);
			var tariffRule2 = Factory.New<USCTariffRule>();
			tariffRule2.U1_RuleCode = TariffRuleList.Codes.HTSExemptFromAMSMO8ProgramRequirement;
			tariffRule2.U1_Tariff = "7201100001";
			tariffRule2.U1_DateFrom = ZDateTime.Now.AddYears(-1);
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_AMSDisclaimProgram = "MO8";
			AssertNoMessageError(invoiceLine.US_AMSDisclaimProgramInfo, string.Format(FormalImportAddInfoJobComInvoiceLineValidation.AMSDisclaimedNotRequired, "MO8", TariffRuleList.Codes.HTSExemptFromAMSMO8ProgramRequirement));
		}

		public void TestCheckUS_AMSDisclaimProgramWithEG1()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0805100040";
			tariff.UE_DateFrom = ZDateTime.Now.AddYears(-2);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(2);
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_AMSDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.US_AMSDisclaimProgram = AMSProgramList.Codes.EG1;
			AssertNoMessageErrors(invoiceLine.US_AMSDisclaimProgramInfo);
			invoiceLine.US_AMSDisclaimProgram = AMSProgramList.Codes.MO8;
			AssertHasMessageError(invoiceLine.US_AMSDisclaimProgramInfo, FormalImportAddInfoJobComInvoiceLineValidation.AMSDisclaimedReasonShouldBeB);
		}

		public void TestCheckUS_PSTDisclaimProgram()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;

			invoiceLine.US_PSTDisclaimProgram = "";
			invoiceLine.AddInfoValidation.ValidateUS_PSTDisclaimProgram();
			AssertHasMessageErrorContaining(invoiceLine.US_PSTDisclaimProgramInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.US_PSTDisclaimProgram = "~";
			invoiceLine.AddInfoValidation.ValidateUS_PSTDisclaimProgram();
			AssertHasMessageErrorContaining(invoiceLine.US_PSTDisclaimProgramInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.US_PSTDisclaimProgram = "PS1";
			invoiceLine.AddInfoValidation.ValidateUS_PSTDisclaimProgram();
			AssertNoMessageErrorContaining(invoiceLine.US_PSTDisclaimProgramInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(invoiceLine.US_PSTDisclaimProgramInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.US_PSTDisclaimProgram = "PS2";
			invoiceLine.AddInfoValidation.ValidateUS_PSTDisclaimProgram();
			AssertNoMessageErrorContaining(invoiceLine.US_PSTDisclaimProgramInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(invoiceLine.US_PSTDisclaimProgramInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_CPSCInd()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0407000020";
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff.UE_PGACodes = "CP2";
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "4801000020";
			tariff2.UE_DateFrom = ZDateTime.Today;
			tariff2.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff2.UE_PGACodes = "CP1";
			Factory.Save();
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USCPSC, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				invoiceLine.US_CPSCInd = "~";
				AssertHasMessageError(invoiceLine.US_CPSCIndInfo, ListValidation.InvalidCodeMessageError);
				invoiceLine.JI_Tariff = tariff.UE_Tariff;
				invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Declared;
				var errorText2 = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequired, "CPSC");
				AssertHasMessageError(invoiceLine.US_CPSCIndInfo, errorText2);
				var cpscHeader = invoiceLine.CPSCHeaders.AddNew();
				invoiceLine.AddInfoValidation.ValidateUS_CPSCInd();
				AssertNoMessageError(invoiceLine.US_CPSCIndInfo, errorText2);
				invoiceLine.CPSCHeaders.RemoveAndDeleteAll();
				invoiceLine.JI_Tariff = tariff2.UE_Tariff;
				invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Declared;
				errorText2 = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequired, "CPSC");
				AssertHasMessageError(invoiceLine.US_CPSCIndInfo, errorText2);
				var header2 = invoiceLine.CPSCHeaders.AddNew();
				invoiceLine.AddInfoValidation.ValidateUS_CPSCInd();
				AssertNoMessageError(invoiceLine.US_CPSCIndInfo, errorText2);
				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa;
				declaration.US_EnableCRL = false;
				declaration.US_CertifyCargoRelease = true;
				invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Declared;
				AssertNoMessageErrors(invoiceLine.US_CPSCIndInfo);
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
				invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Disclaimed;
				AssertHasMessageError(invoiceLine.US_CPSCIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
				invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Declared;
				AssertHasMessageError(invoiceLine.US_CPSCIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
				invoiceLine.US_CPSCInd = ZString.Empty;
				AssertNoMessageError(invoiceLine.US_CPSCIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);

				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				invoiceLine.CPSCHeaders.AddNew();
				invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Disclaimed;
				var errorText3 = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedButEntered, "CPSC");
				AssertHasMessageError(invoiceLine.US_CPSCIndInfo, errorText3);
				invoiceLine.US_CPSCDisclaimReason = PGADisclaimReasonList.Codes.A;
				invoiceLine.AddInfoValidation.ValidateUS_CPSCInd();
				AssertNoMessageError(invoiceLine.US_CPSCIndInfo, errorText3);
			}
		}

		public void TestCheckUS_HFCInd()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0407000020";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-30);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff.UE_PGACodes = "EH2";
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "4801000020";
			tariff2.UE_DateFrom = ZDateTime.Today.AddDays(-30);
			tariff2.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff2.UE_PGACodes = "EH1";
			Factory.Save();

			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.US_HFCInd = "~";
			AssertHasMessageError(invoiceLine.US_HFCIndInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Declared;
			var errorText2 = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequired, "HFC");
			AssertHasMessageError(invoiceLine.US_HFCIndInfo, errorText2);
			var hfcHeader = invoiceLine.USHFCHeaders.AddNew();
			invoiceLine.AddInfoValidation.ValidateUS_HFCInd();
			AssertNoMessageError(invoiceLine.US_HFCIndInfo, errorText2);
			invoiceLine.USHFCHeaders.RemoveAndDeleteAll();
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Declared;
			errorText2 = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequired, "HFC");
			AssertHasMessageError(invoiceLine.US_HFCIndInfo, errorText2);
			var header2 = invoiceLine.USHFCHeaders.AddNew();
			invoiceLine.AddInfoValidation.ValidateUS_HFCInd();
			AssertNoMessageError(invoiceLine.US_HFCIndInfo, errorText2);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa;
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = true;
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Declared;
			AssertNoMessageErrors(invoiceLine.US_HFCIndInfo);
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(invoiceLine.US_HFCIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Declared;
			AssertHasMessageError(invoiceLine.US_HFCIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
			invoiceLine.US_HFCInd = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_HFCIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			invoiceLine.USHFCHeaders.AddNew();
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Disclaimed;
			var errorText3 = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedButEntered, "HFC");
			AssertHasMessageError(invoiceLine.US_HFCIndInfo, errorText3);
			invoiceLine.US_HFCDisclaimReason = "";
			invoiceLine.AddInfoValidation.ValidateUS_HFCDisclaimReason();
			AssertHasMessageError(invoiceLine.US_HFCDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			invoiceLine.US_HFCDisclaimReason = "!";
			AssertHasMessageError(invoiceLine.US_HFCDisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_HFCDisclaimReason = PGADisclaimReasonList.Codes.A;
			AssertNoMessageError(invoiceLine.US_HFCDisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(invoiceLine.US_HFCDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
		}

		public void TestCheckUS_AMSInd()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.AMSEGG, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				var tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = "0407000020";
				tariff.UE_DateFrom = ZDateTime.Today;
				tariff.UE_DateTo = ZDateTime.Today.AddDays(30);
				tariff.UE_PGACodes = "AM2";
				var tariff2 = Factory.New<USCTariff>();
				tariff2.UE_Tariff = "4801000020";
				tariff2.UE_DateFrom = ZDateTime.Today;
				tariff2.UE_DateTo = ZDateTime.Today.AddDays(30);
				tariff2.UE_PGACodes = "AM4";
				var tariff3 = Factory.New<USCTariff>();
				tariff3.UE_Tariff = "4861000020";
				tariff3.UE_DateFrom = ZDateTime.Today;
				tariff3.UE_DateTo = ZDateTime.Today.AddDays(30);
				tariff3.UE_PGACodes = "";
				Factory.Save();
				invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				invoiceLine.US_AMSInd = "~";
				AssertHasMessageError(invoiceLine.US_AMSIndInfo, ListValidation.InvalidCodeMessageError);
				invoiceLine.JI_Tariff = tariff3.UE_Tariff;
				invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
				var warningText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, "AMS", "Disclaimed");
				AssertHasWarningContaining(invoiceLine.US_AMSIndInfo, warningText);
				invoiceLine.JI_Tariff = tariff.UE_Tariff;
				invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
				var errorText2 = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequired, "AMS");
				AssertNoWarningContaining(invoiceLine.US_AMSIndInfo, warningText);
				AssertHasMessageError(invoiceLine.US_AMSIndInfo, errorText2);
				var amsLine = invoiceLine.AMSLines.AddNew();
				amsLine.US_Program = AMSProgramList.Codes.EG1;
				invoiceLine.AddInfoValidation.ValidateUS_AMSInd();
				AssertNoMessageError(invoiceLine.US_AMSIndInfo, errorText2);
				invoiceLine.AMSLines.RemoveAndDeleteAll();
				invoiceLine.JI_Tariff = tariff2.UE_Tariff;
				invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
				invoiceLine.AddInfoValidation.ValidateUS_AMSInd();
				errorText2 = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequired, "AMS");
				AssertNoWarningContaining(invoiceLine.US_AMSIndInfo, warningText);
				AssertHasMessageError(invoiceLine.US_AMSIndInfo, errorText2);
				var amsLine2 = invoiceLine.AMSLines.AddNew();
				amsLine2.US_Program = AMSProgramList.Codes.MO1;
				invoiceLine.AddInfoValidation.ValidateUS_AMSInd();
				AssertNoMessageError(invoiceLine.US_AMSIndInfo, errorText2);
				errorText2 = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequiredIfNotApplicable, "AMS", EntryTypeList.Codes.ReWarehouse);
				declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
				declaration.US_EnableCRL = false;
				declaration.US_CertifyCargoRelease = true;
				invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
				invoiceLine.AddInfoValidation.ValidateUS_AMSInd();
				AssertHasMessageErrorContaining(invoiceLine.US_AMSIndInfo, errorText2);
				declaration.US_EntryType = ZString.Empty;
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
				invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
				AssertHasMessageError(invoiceLine.US_AMSIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
				invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
				AssertHasMessageError(invoiceLine.US_AMSIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
				invoiceLine.US_AMSInd = ZString.Empty;
				AssertNoMessageError(invoiceLine.US_AMSIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
			}
		}

		public void TestCheckUS_NOPInd()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0902101015";
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff.UE_PGACodes = "AM8";
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "0910995000";
			tariff2.UE_DateFrom = ZDateTime.Today;
			tariff2.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff2.UE_PGACodes = "AM7";
			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "4861000020";
			tariff3.UE_DateFrom = ZDateTime.Today;
			tariff3.UE_DateTo = ZDateTime.Today.AddDays(30);
			tariff3.UE_PGACodes = "";
			Factory.Save();
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.US_NOPInd = "~";
			AssertHasMessageError(invoiceLine.US_NOPIndInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_Tariff = tariff3.UE_Tariff;
			invoiceLine.US_NOPInd = OGAIndicatorList.Codes.Disclaimed;
			var warningText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, "NOP", "Disclaimed");
			AssertHasWarningContaining(invoiceLine.US_NOPIndInfo, warningText);
			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGARequiredButBlank, "NOP");
			var errorText2 = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedButEntered, "NOP");
			var errorText3 = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedIsNotAllowedForRequiredTariff, "NOP");
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_NOPInd = ZString.Empty;
			invoiceLine.US_NOPInd = OGAIndicatorList.Codes.Disclaimed;
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.OR1;
			AssertNoWarningContaining(invoiceLine.US_NOPIndInfo, warningText);
			invoiceLine.AddInfoValidation.ValidateUS_NOPInd();
			AssertHasMessageErrorContaining(invoiceLine.US_NOPIndInfo, errorText2);
			AssertHasMessageErrorContaining(invoiceLine.US_NOPIndInfo, errorText3);
			amsLine.US_Program = AMSProgramList.Codes.OR2;
			AssertNoWarningContaining(invoiceLine.US_NOPIndInfo, warningText);
			invoiceLine.AddInfoValidation.ValidateUS_NOPInd();
			AssertHasMessageErrorContaining(invoiceLine.US_NOPIndInfo, errorText2);
			AssertHasMessageErrorContaining(invoiceLine.US_NOPIndInfo, errorText3);
			invoiceLine.AMSLines.RemoveAndDeleteAll();
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			invoiceLine.US_NOPInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.AddInfoValidation.ValidateUS_NOPInd();
			AssertNoWarningContaining(invoiceLine.US_NOPIndInfo, warningText);
			AssertNoMessageErrorContaining(invoiceLine.US_NOPIndInfo, errorText2);
			var amsLine2 = invoiceLine.AMSLines.AddNew();
			amsLine2.US_Program = AMSProgramList.Codes.OR1;
			invoiceLine.AddInfoValidation.ValidateUS_NOPInd();
			AssertHasMessageErrorContaining(invoiceLine.US_NOPIndInfo, errorText2);
			errorText2 = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequiredIfNotApplicable, "NOP", EntryTypeList.Codes.ReWarehouse);
			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = true;
			invoiceLine.US_NOPInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.AddInfoValidation.ValidateUS_NOPInd();
			AssertHasMessageErrorContaining(invoiceLine.US_NOPIndInfo, errorText2);
			declaration.US_EntryType = ZString.Empty;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			invoiceLine.US_NOPInd = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(invoiceLine.US_NOPIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
			invoiceLine.US_NOPInd = OGAIndicatorList.Codes.Declared;
			AssertHasMessageError(invoiceLine.US_NOPIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
			invoiceLine.US_NOPInd = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_NOPIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AMSNOP, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				invoiceLine.JI_Tariff = tariff.UE_Tariff;
				invoiceLine.US_NOPInd = OGAIndicatorList.Codes.Declared;
				AssertNoMessageErrors(invoiceLine.US_NOPIndInfo);
				invoiceLine.US_NOPInd = ZString.Empty;
				AssertNoMessageError(invoiceLine.US_NOPIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AMSNOP, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				invoiceLine.JI_Tariff = tariff.UE_Tariff;
				invoiceLine.US_NOPInd = OGAIndicatorList.Codes.Declared;
				AssertNoWarnings(invoiceLine.US_NOPIndInfo);
			}
		}

		[TestDate(2017, 5, 17)]
		public void TestValidatePGAWhenNotEffective()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.AMSEGG, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				var tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = "1000000001";
				tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
				tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
				tariff.UE_PGACodes = "FW1EP1";
				invoiceLine.JI_Tariff = tariff.UE_Tariff;
				invoiceLine.US_TTBInd = ZString.Empty;
				AssertNoNotifications(invoiceLine.US_TTBIndInfo);
				invoiceLine.US_FWSInd = ZString.Empty;
				AssertNoNotifications(invoiceLine.US_FWSIndInfo);
				invoiceLine.US_FSISInd = ZString.Empty;
				AssertNoWarning(invoiceLine.US_FSISIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGASubmittedBy);
				tariff.UE_PGACodes = "FW1EP1FS3";
				invoiceLine.US_FSISInd = ZString.Empty;
				AssertHasWarning(invoiceLine.US_FSISIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGASubmittedBy);
				tariff.UE_PGACodes = "FW1EP1FS4";
				invoiceLine.US_FSISInd = ZString.Empty;
				AssertHasWarning(invoiceLine.US_FSISIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGASubmittedBy);
				invoiceLine.US_ODSInd = ZString.Empty;
				var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGARequiredButBlank, "ODS");
				AssertHasMessageError(invoiceLine.US_ODSIndInfo, errorText);
				invoiceLine.US_TSCAInd = ZString.Empty;
				AssertNoNotifications(invoiceLine.US_TSCAIndInfo);
				invoiceLine.US_AMSInd = ZString.Empty;
				AssertNoNotifications(invoiceLine.US_AMSIndInfo);
				invoiceLine.US_VNEInd = ZString.Empty;
				AssertNoNotifications(invoiceLine.US_VNEIndInfo);
				invoiceLine.US_PSTIndicator = ZString.Empty;
				AssertNoNotifications(invoiceLine.US_PSTIndicatorInfo);
			}
		}

		public void TestODSIndicator()
		{
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "FW1EP1";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGARequiredButBlank, "ODS");
			invoiceLine.AddInfoValidation.ValidateUS_ODSInd();
			AssertHasMessageError(invoiceLine.US_ODSIndInfo, errorText);
			invoiceLine.US_ODSInd = "~";
			AssertHasMessageError(invoiceLine.US_ODSIndInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_ODSIndInfo, ListValidation.InvalidCodeMessageError);
			tariff.UE_PGACodes = "FW1EP2";
			invoiceLine.US_ODSInd = ZString.Empty;
			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGARequiredButBlank, "ODS");
			AssertHasMessageError(invoiceLine.US_ODSIndInfo, errorText);
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_ODSIndInfo, errorText);
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.US_SupTariff = ZString.Empty;
			invoiceLine.US_FTZCurrentTariff = tariff.UE_Tariff;
			invoiceLine.US_ODSInd = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_ODSIndInfo, errorText);
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_ODSIndInfo, errorText);
			invoiceLine.US_FTZCurrentTariff = ZString.Empty;
			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_ODSInd = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_ODSIndInfo, errorText);
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var req = (OGAAgencyRequirement)invoiceLine.OGAAgencyRequirements.FirstOrDefault(x => ((OGAAgencyRequirement)x).AgencyCode == GovernmentAgencyProgramCodeList.Codes.ODS);
			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, GovernmentAgencyProgramCodeList.Codes.ODS, "Declared");
			tariff.UE_PGACodes = ZString.Empty;
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(invoiceLine.US_ODSIndInfo, errorText);
			tariff.UE_PGACodes = "EP1";
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;
			AssertNoWarning(invoiceLine.US_ODSIndInfo, errorText);
			invoiceLine.US_ODSInd = ZString.Empty;
			AssertNoWarning(invoiceLine.US_ODSIndInfo, errorText);
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Disclaimed;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(invoiceLine.US_ODSIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;
			AssertHasMessageError(invoiceLine.US_ODSIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
			invoiceLine.US_ODSInd = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_ODSIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
		}

		public void TestVNEIndicator()
		{
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "FW1EP3EP8";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGARequiredButBlank, "VNE");
			invoiceLine.AddInfoValidation.ValidateUS_VNEInd();
			AssertHasMessageError(invoiceLine.US_VNEIndInfo, errorText);
			invoiceLine.US_VNEInd = "~";
			AssertHasMessageError(invoiceLine.US_VNEIndInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_VNEIndInfo, ListValidation.InvalidCodeMessageError);
			var req = (OGAAgencyRequirement)invoiceLine.OGAAgencyRequirements.FirstOrDefault(x => ((OGAAgencyRequirement)x).AgencyCode == GovernmentAgencyProgramCodeList.Codes.VNE);
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			tariff.UE_PGACodes = "FW1EP4";
			invoiceLine.US_VNEInd = ZString.Empty;
			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGARequiredButBlank, "VNE");
			AssertHasMessageError(invoiceLine.US_VNEIndInfo, errorText);
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_VNEIndInfo, errorText);
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.US_SupTariff = ZString.Empty;
			invoiceLine.US_FTZCurrentTariff = tariff.UE_Tariff;
			invoiceLine.US_VNEInd = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_VNEIndInfo, errorText);
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_VNEIndInfo, errorText);
			invoiceLine.US_FTZCurrentTariff = ZString.Empty;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_VNEInd = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_VNEIndInfo, errorText);
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequired, "VNE");
			AssertHasMessageError(invoiceLine.US_VNEIndInfo, errorText);
			invoiceLine.VehicleLines.AddNew();
			invoiceLine.AddInfoValidation.ValidateUS_VNEInd();
			AssertNoMessageError(invoiceLine.US_VNEIndInfo, errorText);
			tariff.UE_OGACodes = "FW1";
			invoiceLine.US_SupTariff = ZString.Empty;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Disclaimed;
			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedButEntered, "VNE");
			AssertHasMessageError(invoiceLine.US_VNEIndInfo, errorText);
			invoiceLine.VehicleLines.RemoveAndDeleteAll();
			invoiceLine.US_VNEInd = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_VNEIndInfo, errorText);
			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, GovernmentAgencyProgramCodeList.Codes.VNE, "Declared");
			tariff.UE_PGACodes = ZString.Empty;
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(invoiceLine.US_VNEIndInfo, errorText);
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Disclaimed;
			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, GovernmentAgencyProgramCodeList.Codes.VNE, "Disclaimed");
			AssertHasWarning(invoiceLine.US_VNEIndInfo, errorText);
			invoiceLine.US_VNEInd = ZString.Empty;
			AssertNoWarning(invoiceLine.US_VNEIndInfo, errorText);
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(invoiceLine.US_VNEIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			AssertHasMessageError(invoiceLine.US_VNEIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
			invoiceLine.US_VNEInd = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_VNEIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
		}

		public void TestFSISIndicator()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "FW1EP5FS3";
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			invoiceLine.Declaration.US_CertifyCargoRelease = true;
			AssertFSISIndicator(tariff);
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Disclaimed;
			Assert(!invoiceLine.US_FSISIndInfo.HasMessageError(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode));
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			Assert(!invoiceLine.US_FSISIndInfo.HasMessageError(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode));
			invoiceLine.US_FSISInd = ZString.Empty;
			Assert(!invoiceLine.US_FSISIndInfo.HasMessageError(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode));
			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequired, "FSIS", EntryTypeList.Codes.TemporaryImportationBond);
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			invoiceLine.Declaration.US_EnableCRL = false;
			invoiceLine.Declaration.US_CertifyCargoRelease = true;
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			AssertHasMessageErrorContaining(invoiceLine.US_FSISIndInfo, errorText);
		}

		public void TestCheckUS_FSISIndicatorForMaybeRequired()
		{
			var tariffWithFS3 = Factory.New<USCTariff>();
			tariffWithFS3.UE_Tariff = "1000000001";
			tariffWithFS3.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffWithFS3.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariffWithFS3.UE_PGACodes = "FW1EP5FS3"; //may be required
			var tariffWithFS4 = Factory.New<USCTariff>();
			tariffWithFS4.UE_Tariff = "1000000002";
			tariffWithFS4.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffWithFS4.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariffWithFS4.UE_PGACodes = "FW1EP5FS4"; //required
			var tariffWithNone = Factory.New<USCTariff>();
			tariffWithNone.UE_Tariff = "1000000003";
			tariffWithNone.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffWithNone.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariffWithNone.UE_PGACodes = "FW1EP5"; //nothing
												   // FSIS is effective
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PGAFSIS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true);
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			invoiceLine.Declaration.US_CertifyCargoRelease = true;
			invoiceLine.JI_Tariff = tariffWithFS4.UE_Tariff;
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(invoiceLine.US_FSISIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedIsNotAllowedForRequiredTariff);
			invoiceLine.JI_Tariff = tariffWithFS3.UE_Tariff;
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Disclaimed;
			AssertNoMessageError(invoiceLine.US_FSISIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedIsNotAllowedForRequiredTariff);
			invoiceLine.JI_Tariff = tariffWithNone.UE_Tariff;
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			var errorMessage = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, "FSIS", "Declared");
			AssertHasWarning(invoiceLine.US_FSISIndInfo, errorMessage);
			invoiceLine.JI_Tariff = tariffWithFS3.UE_Tariff;
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			AssertNoWarning(invoiceLine.US_FSISIndInfo, errorMessage);
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Disclaimed;
			AssertNoWarning(invoiceLine.US_FSISIndInfo, errorMessage);
			invoiceLine.JI_Tariff = tariffWithFS4.UE_Tariff;
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			AssertNoWarning(invoiceLine.US_FSISIndInfo, errorMessage);
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Disclaimed;
			AssertNoWarning(invoiceLine.US_FSISIndInfo, errorMessage);
		}

		public void TestACE_FDAIndicatorAndDisclaim()
		{
			var tariffWithFD1 = Factory.New<USCTariff>();
			tariffWithFD1.UE_Tariff = "1000000001";
			tariffWithFD1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffWithFD1.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariffWithFD1.UE_PGACodes = "FW1FD1"; //may be required
			var tariffWithFD2 = Factory.New<USCTariff>();
			tariffWithFD2.UE_Tariff = "1000000002";
			tariffWithFD2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffWithFD2.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariffWithFD2.UE_PGACodes = "FW1FD2"; //required
			var tariffWithFD3 = Factory.New<USCTariff>();
			tariffWithFD3.UE_Tariff = "1000000003";
			tariffWithFD3.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffWithFD3.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariffWithFD3.UE_PGACodes = "FW1FD3"; //may be required
			var tariffWithFD4 = Factory.New<USCTariff>();
			tariffWithFD4.UE_Tariff = "1000000004";
			tariffWithFD4.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffWithFD4.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariffWithFD4.UE_PGACodes = "FW1FD4"; //required
			var tariffWithNone = Factory.New<USCTariff>();
			tariffWithNone.UE_Tariff = "1000000005";
			tariffWithNone.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffWithNone.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariffWithNone.UE_PGACodes = "FW1"; //nothing
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			invoiceLine.Declaration.US_CertifyCargoRelease = true;
			invoiceLine.JI_Tariff = tariffWithFD2.UE_Tariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			var errorMessage = AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedIsNotAllowedForRequiredTariff;
			AssertHasMessageError(invoiceLine.US_FDAIndicatorInfo, errorMessage);
			invoiceLine.JI_Tariff = tariffWithFD1.UE_Tariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertNoMessageError(invoiceLine.US_FDAIndicatorInfo, errorMessage);
			invoiceLine.JI_Tariff = tariffWithNone.UE_Tariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			errorMessage = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, "FDA", "Declared");
			AssertHasWarning(invoiceLine.US_FDAIndicatorInfo, errorMessage);
			invoiceLine.JI_Tariff = tariffWithFD1.UE_Tariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoWarning(invoiceLine.US_FDAIndicatorInfo, errorMessage);
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertNoWarning(invoiceLine.US_FDAIndicatorInfo, errorMessage);
			invoiceLine.JI_Tariff = tariffWithFD2.UE_Tariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoWarning(invoiceLine.US_FDAIndicatorInfo, errorMessage);
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertNoWarning(invoiceLine.US_FDAIndicatorInfo, errorMessage);
			invoiceLine.US_FDADisclaimReason = "";
			invoiceLine.AddInfoValidation.ValidateUS_FDADisclaimReason();
			AssertHasMessageError(invoiceLine.US_FDADisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			invoiceLine.US_FDADisclaimReason = "!";
			AssertHasMessageError(invoiceLine.US_FDADisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_FDADisclaimReason = PGADisclaimReasonList.Codes.A;
			AssertNoMessageError(invoiceLine.US_FDADisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(invoiceLine.US_FDADisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
		}

		public void TestCheckUS_ATFInd()
		{
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			invoiceLine.Declaration.US_CertifyCargoRelease = true;
			invoiceLine.US_ATFInd = "~";
			AssertHasMessageError(invoiceLine.US_ATFIndInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_ATFInd = "D";
			AssertNoMessageError(invoiceLine.US_ATFIndInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_ATFInd = "";
			var aftLine = invoiceLine.ATFLines.AddNew();
			aftLine.US_CategoryCode = "API";
			invoiceLine.AddInfoValidation.ValidateUS_ATFInd();
			var errorMessage = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedButEntered, "ATF");
			AssertHasMessageError(invoiceLine.US_ATFIndInfo, errorMessage);
			invoiceLine.US_ATFInd = "D";
			AssertNoMessageError(invoiceLine.US_ATFIndInfo, errorMessage);
			invoiceLine.ATFLines.RemoveAndDeleteAll();
			invoiceLine.AddInfoValidation.ValidateUS_ATFInd();
			errorMessage = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequired, "ATF");
			AssertHasMessageError(invoiceLine.US_ATFIndInfo, errorMessage);
			invoiceLine.US_ATFInd = "";
			AssertNoMessageError(invoiceLine.US_ATFIndInfo, errorMessage);
			errorMessage = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequired, "ATF", EntryTypeList.Codes.ConsumptionQuotaVisa);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_EnableCRL = false;
			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
			AssertHasMessageErrorContaining(invoiceLine.US_ATFIndInfo, errorMessage);
		}

		public void TestPSTIndicator()
		{
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "FW1EP3EP5";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGARequiredButBlank, "PST");
			invoiceLine.AddInfoValidation.ValidateUS_PSTIndicator();
			AssertHasMessageError(invoiceLine.US_PSTIndicatorInfo, errorText);
			invoiceLine.US_PSTIndicator = "~";
			AssertHasMessageError(invoiceLine.US_PSTIndicatorInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_LaceyIndicator = "~";
			AssertHasMessageError(invoiceLine.US_LaceyIndicatorInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_PSTIndicatorInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_LaceyIndicatorInfo, ListValidation.InvalidCodeMessageError);
			tariff.UE_PGACodes = "FW1EP6";
			invoiceLine.US_PSTIndicator = ZString.Empty;
			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGARequiredButBlank, "PST");
			AssertHasMessageError(invoiceLine.US_PSTIndicatorInfo, errorText);
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_PSTIndicatorInfo, errorText);
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.US_SupTariff = ZString.Empty;
			invoiceLine.US_FTZCurrentTariff = tariff.UE_Tariff;
			invoiceLine.US_PSTIndicator = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_PSTIndicatorInfo, errorText);
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_PSTIndicatorInfo, errorText);
			invoiceLine.US_FTZCurrentTariff = ZString.Empty;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_PSTIndicator = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_PSTIndicatorInfo, errorText);
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			tariff.UE_PGACodes = "FW1EP6";
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			var req = (OGAAgencyRequirement)invoiceLine.OGAAgencyRequirements.FirstOrDefault(x => ((OGAAgencyRequirement)x).AgencyCode == GovernmentAgencyProgramCodeList.Codes.PST);
			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequired, "PST");
			AssertHasMessageError(invoiceLine.US_PSTIndicatorInfo, errorText);
			AssertHasMessageError(req.IndicatorInfo, errorText);
			invoiceLine.PSTLines.AddNew();
			invoiceLine.AddInfoValidation.ValidateUS_PSTIndicator();
			AssertNoMessageError(invoiceLine.US_PSTIndicatorInfo, errorText);
			tariff.UE_OGACodes = "FW1";
			invoiceLine.US_SupTariff = ZString.Empty;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;
			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedButEntered, "PST");
			AssertHasMessageError(invoiceLine.US_PSTIndicatorInfo, errorText);
			invoiceLine.PSTLines.RemoveAndDeleteAll();
			invoiceLine.US_PSTIndicator = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_PSTIndicatorInfo, errorText);
			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, GovernmentAgencyProgramCodeList.Codes.PST, "Declared");
			tariff.UE_PGACodes = ZString.Empty;
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(invoiceLine.US_PSTIndicatorInfo, errorText);
			invoiceLine.US_PSTIndicator = ZString.Empty;
			AssertNoWarning(invoiceLine.US_PSTIndicatorInfo, errorText);
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(invoiceLine.US_PSTIndicatorInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasMessageError(invoiceLine.US_PSTIndicatorInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
			invoiceLine.US_PSTIndicator = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_PSTIndicatorInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGANotApplicableForCertificationMode);
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
		}

		public void TestDisclaimReason()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000002";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "EP1EP3EP5EP7AM1AM3DT1";
			invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_PSTDisclaimReason = "";
			AssertHasMessageError(invoiceLine.US_PSTDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			invoiceLine.US_PSTDisclaimReason = PGADisclaimReasonList.Codes.B;
			var messageErrorText = ListValidation.InvalidCodeMessageError;
			AssertNoMessageError(invoiceLine.US_PSTDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			AssertHasMessageError(invoiceLine.US_PSTDisclaimReasonInfo, messageErrorText);
			invoiceLine.US_PSTDisclaimReason = PGADisclaimReasonList.Codes.A;
			AssertNoMessageError(invoiceLine.US_PSTDisclaimReasonInfo, messageErrorText);
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_TSCADisclaimReason = "";
			AssertHasMessageError(invoiceLine.US_TSCADisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			invoiceLine.US_TSCADisclaimReason = PGADisclaimReasonList.Codes.C;
			AssertNoMessageError(invoiceLine.US_TSCADisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			invoiceLine.US_TSCADisclaimReason = PGADisclaimReasonList.Codes.A;
			messageErrorText = ListValidation.InvalidCodeMessageError;
			AssertNoMessageError(invoiceLine.US_TSCADisclaimReasonInfo, messageErrorText);
			invoiceLine.US_TSCADisclaimReason = PGADisclaimReasonList.Codes.C;
			AssertNoMessageError(invoiceLine.US_TSCADisclaimReasonInfo, messageErrorText);
			invoiceLine.US_TSCADisclaimReason = PGADisclaimReasonList.Codes.B;
			AssertNoMessageError(invoiceLine.US_TSCADisclaimReasonInfo, messageErrorText);
			invoiceLine.US_TSCADisclaimReason = PGADisclaimReasonList.Codes.D;
			AssertNoMessageError(invoiceLine.US_TSCADisclaimReasonInfo, messageErrorText);
			invoiceLine.US_TSCADisclaimReason = "~";
			AssertHasMessageError(invoiceLine.US_TSCADisclaimReasonInfo, messageErrorText);
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_ODSDisclaimReason = "";
			AssertHasMessageError(invoiceLine.US_ODSDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			invoiceLine.US_ODSDisclaimReason = PGADisclaimReasonList.Codes.A;
			messageErrorText = ListValidation.InvalidCodeMessageError;
			AssertNoMessageError(invoiceLine.US_ODSDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			AssertNoMessageError(invoiceLine.US_ODSDisclaimReasonInfo, messageErrorText);
			invoiceLine.US_ODSDisclaimReason = PGADisclaimReasonList.Codes.C;
			AssertNoMessageError(invoiceLine.US_ODSDisclaimReasonInfo, messageErrorText);
			invoiceLine.US_ODSDisclaimReason = PGADisclaimReasonList.Codes.B;
			AssertNoMessageError(invoiceLine.US_ODSDisclaimReasonInfo, messageErrorText);
			invoiceLine.US_ODSDisclaimReason = PGADisclaimReasonList.Codes.D;
			AssertNoMessageError(invoiceLine.US_ODSDisclaimReasonInfo, messageErrorText);
			invoiceLine.US_ODSDisclaimReason = "~";
			AssertHasMessageError(invoiceLine.US_ODSDisclaimReasonInfo, messageErrorText);
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_VNEDisclaimReason = "";
			AssertHasMessageError(invoiceLine.US_VNEDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			invoiceLine.US_VNEDisclaimReason = PGADisclaimReasonList.Codes.A;
			messageErrorText = ListValidation.InvalidCodeMessageError;
			AssertNoMessageError(invoiceLine.US_VNEDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			AssertNoMessageError(invoiceLine.US_VNEDisclaimReasonInfo, messageErrorText);
			invoiceLine.US_VNEDisclaimReason = PGADisclaimReasonList.Codes.B;
			AssertNoMessageError(invoiceLine.US_VNEDisclaimReasonInfo, messageErrorText);
			invoiceLine.US_VNEDisclaimReason = "~";
			AssertHasMessageError(invoiceLine.US_VNEDisclaimReasonInfo, messageErrorText);
			// FSIS is not yet effective
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PGAFSIS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false);
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_FSISDisclaimReason = "";
			AssertNoMessageError(invoiceLine.US_FSISDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			// FSIS is effective
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PGAFSIS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true);
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_FSISDisclaimReason = "";
			AssertHasMessageError(invoiceLine.US_FSISDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			invoiceLine.US_FSISDisclaimReason = PGADisclaimReasonList.Codes.A;
			AssertNoMessageError(invoiceLine.US_FSISDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			invoiceLine.US_FSISDisclaimReason = "~";
			AssertHasMessageError(invoiceLine.US_FSISDisclaimReasonInfo, messageErrorText);
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_AMSDisclaimReason = "";
			AssertHasMessageError(invoiceLine.US_AMSDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			invoiceLine.US_AMSDisclaimReason = PGADisclaimReasonList.Codes.A;
			AssertNoMessageError(invoiceLine.US_AMSDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			AssertNoMessageError(invoiceLine.US_AMSDisclaimReasonInfo, messageErrorText);
			invoiceLine.US_AMSDisclaimReason = PGADisclaimReasonList.Codes.B;
			AssertNoMessageError(invoiceLine.US_AMSDisclaimReasonInfo, messageErrorText);
			invoiceLine.US_AMSDisclaimReason = PGADisclaimReasonList.Codes.C;
			AssertHasMessageError(invoiceLine.US_AMSDisclaimReasonInfo, messageErrorText);
			invoiceLine.US_AMSDisclaimReason = PGADisclaimReasonList.Codes.D;
			AssertHasMessageError(invoiceLine.US_AMSDisclaimReasonInfo, messageErrorText);
			invoiceLine.US_NOPInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_NOPDisclaimReason = "";
			AssertHasMessageError(invoiceLine.US_NOPDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			invoiceLine.US_NOPDisclaimReason = PGADisclaimReasonList.Codes.A;
			AssertNoMessageError(invoiceLine.US_NOPDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_NHTDisclaimReason = "";
			AssertHasMessageError(invoiceLine.US_NHTDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			invoiceLine.US_NHTDisclaimReason = PGADisclaimReasonList.Codes.A;
			AssertNoMessageError(invoiceLine.US_NHTDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			AssertNoMessageError(invoiceLine.US_NHTDisclaimReasonInfo, messageErrorText);
			invoiceLine.US_NHTDisclaimReason = PGADisclaimReasonList.Codes.B;
			AssertHasMessageError(invoiceLine.US_NHTDisclaimReasonInfo, messageErrorText);
			invoiceLine.US_NHTDisclaimReason = PGADisclaimReasonList.Codes.C;
			AssertHasMessageError(invoiceLine.US_NHTDisclaimReasonInfo, messageErrorText);
			invoiceLine.US_NHTDisclaimReason = PGADisclaimReasonList.Codes.D;
			AssertHasMessageError(invoiceLine.US_NHTDisclaimReasonInfo, messageErrorText);
		}

		public void TestNHTSAIndicator()
		{
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1000000001";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);
			tariff.UE_PGACodes = "FW1EP3DT2";
			invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGARequiredButBlank, "NHTSA");
			Assert(!invoiceLine.US_NHTSAIndicatorInfo.HasMessageError(errorText));
			invoiceLine.US_NHTSAIndicator = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_NHTSAIndicatorInfo, errorText);
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.US_SupTariff = ZString.Empty;
			invoiceLine.US_FTZCurrentTariff = tariff.UE_Tariff;
			invoiceLine.US_NHTSAIndicator = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_NHTSAIndicatorInfo, errorText);
			invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_NHTSAIndicatorInfo, errorText);
			invoiceLine.US_FTZCurrentTariff = ZString.Empty;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_NHTSAIndicator = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_NHTSAIndicatorInfo, errorText);
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Disclaimed;
			errorText = AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedIsNotAllowedForRequiredTariff;
			AssertHasMessageError(invoiceLine.US_NHTSAIndicatorInfo, errorText);
			invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_NHTSAIndicatorInfo, errorText);
			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequired, "NHTSA");
			AssertHasMessageError(invoiceLine.US_NHTSAIndicatorInfo, errorText);
			invoiceLine.NHTSALines.AddNew();
			invoiceLine.AddInfoValidation.ValidateUS_NHTSAIndicator();
			AssertNoMessageError(invoiceLine.US_NHTSAIndicatorInfo, errorText);
			tariff.UE_PGACodes = "FW1";
			invoiceLine.US_SupTariff = ZString.Empty;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Disclaimed;
			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedButEntered, "NHTSA");
			AssertHasMessageError(invoiceLine.US_NHTSAIndicatorInfo, errorText);
			invoiceLine.NHTSALines.RemoveAndDeleteAll();
			invoiceLine.US_NHTSAIndicator = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_NHTSAIndicatorInfo, errorText);
			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGANotRequired, "NHTSA", "Declared");
			tariff.UE_PGACodes = ZString.Empty;
			invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Declared;
			AssertHasWarning(invoiceLine.US_NHTSAIndicatorInfo, errorText);
			invoiceLine.US_NHTSAIndicator = ZString.Empty;
			AssertNoWarning(invoiceLine.US_NHTSAIndicatorInfo, errorText);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		void AssertOnCountryInRelationToSpecialTradeAgreement(ZPropertyInfo countryInfo)
		{
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Assert("Pre-condition", invoiceLine.Declaration.US_EntryType == EntryTypeList.Codes.ConsumptionFreeDutiable);
			countryInfo.Value = (ZString)"CA";
			invoiceLine.US_SupTariff = "9802008042";
			AssertNotNull("PreCondition:Import tariff should not be null", invoiceLine.ImportSupTariff);
			AssertEquals("IsEligibleForAGOATextile", true, invoiceLine.ImportSupTariff.IsEligibleForAGOATextileBenefits(invoiceLine.EffectiveDateForDutyRate));
			AssertHasMessageError(countryInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.NoAGOACountry);
			countryInfo.Value = (ZString)"ZA";
			AssertNoMessageError(countryInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.NoAGOACountry);
			invoiceLine.US_SupTariff = "9802008044";
			AssertNotNull("PreCondition:Import tariff should not be null", invoiceLine.ImportSupTariff);
			AssertEquals("IsEligibleForAGOATextile", true, invoiceLine.ImportSupTariff.IsEligibleForCBTPATextileBenefits(invoiceLine.EffectiveDateForDutyRate));
			AssertHasMessageError(countryInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.NoCBTPACountry);
			countryInfo.Value = (ZString)"JM";
			AssertNoMessageError(countryInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.NoCBTPACountry);
			invoiceLine.US_SupTariff = "9802008048";
			AssertNotNull("PreCondition:Import tariff should not be null", invoiceLine.ImportSupTariff);
			AssertEquals("IsEligibleForATPDEA", true, invoiceLine.ImportSupTariff.IsEligibleForATPDEATextileAndTunaClaims(invoiceLine.EffectiveDateForDutyRate));
			AssertHasMessageError(countryInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.NoATPDEACountry);
			countryInfo.Value = (ZString)"EC";
			AssertNoMessageError(countryInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.NoATPDEACountry);
			invoiceLine.US_SupTariff = USCTariff.CAFTABenefitsApplicable;
			AssertNotNull("PreCondition:Import tariff should not be null", invoiceLine.ImportSupTariff);
			AssertEquals("IsEligibleForATPDEA", true, invoiceLine.ImportSupTariff.IsEligibleForCAFTAClaims(invoiceLine.EffectiveDateForDutyRate));
			AssertHasMessageError(countryInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.NoCAFTACountry);
			countryInfo.Value = (ZString)Core.Constants.CountryCodes.ElSalvador;
			AssertNoMessageError(countryInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.CountryOfExport.NoCAFTACountry);
		}

		void AssertFSISIndicator(USCTariff tariff)
		{
			// FSIS is not yet effective
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PGAFSIS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false);
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.AddInfoValidation.ValidateUS_FSISInd();
			AssertHasWarning(invoiceLine.US_FSISIndInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGASubmittedBy);
			invoiceLine.US_FSISInd = "~";
			AssertHasMessageError(invoiceLine.US_FSISIndInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			AssertNoMessageError(invoiceLine.US_FSISIndInfo, ListValidation.InvalidCodeMessageError);
			var warningText = AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedIsNotAllowedForRequiredTariff;
			tariff.UE_PGACodes = "FW1EP5FS4";
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(invoiceLine.US_FSISIndInfo, warningText);
			tariff.UE_PGACodes = "FW1FS3";
			invoiceLine.US_FSISInd = ZString.Empty;
			invoiceLine.AddInfoValidation.ValidateUS_FSISInd();
			AssertNoMessageError(invoiceLine.US_FSISIndInfo, warningText);
			// FSIS is effective
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PGAFSIS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true);
			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			invoiceLine.AddInfoValidation.ValidateUS_FSISInd();
			var errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGARequiredButBlank, "FSIS");
			AssertHasMessageError(invoiceLine.US_FSISIndInfo, errorText);
			invoiceLine.US_FSISInd = "D";
			AssertNoMessageError(invoiceLine.US_FSISIndInfo, errorText);
			invoiceLine.US_FSISInd = ZString.Empty;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.US_SupTariff = ZString.Empty;
			invoiceLine.US_FTZCurrentTariff = tariff.UE_Tariff;
			AssertHasMessageError(invoiceLine.US_FSISIndInfo, errorText);
			invoiceLine.US_FSISInd = "D";
			AssertNoMessageError(invoiceLine.US_FSISIndInfo, errorText);
			invoiceLine.US_FTZCurrentTariff = ZString.Empty;
			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			var req = (OGAAgencyRequirement)invoiceLine.OGAAgencyRequirements.FirstOrDefault(x => ((OGAAgencyRequirement)x).AgencyCode == GovernmentAgencyProgramCodeList.Codes.FSIS);
			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGALineRequired, "FSIS");
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Declared;
			AssertHasMessageError(invoiceLine.US_FSISIndInfo, errorText);
			AssertHasMessageError(req.IndicatorInfo, errorText);
			invoiceLine.FSISLines.AddNew();
			invoiceLine.AddInfoValidation.ValidateUS_FSISInd();
			AssertNoMessageError(invoiceLine.US_FSISIndInfo, errorText);
			tariff.UE_OGACodes = "FW1";
			invoiceLine.US_SupTariff = ZString.Empty;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_FSISInd = OGAIndicatorList.Codes.Disclaimed;
			errorText = string.Format(AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedButEntered, "FSIS");
			AssertHasMessageError(invoiceLine.US_FSISIndInfo, errorText);
			// FSIS is not yet effective
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PGAFSIS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, false);
			invoiceLine.AddInfoValidation.ValidateUS_FSISInd();
			AssertHasMessageError(invoiceLine.US_FSISIndInfo, errorText);
			// FSIS is effective
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PGAFSIS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true);
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.AddInfoValidation.ValidateUS_FSISInd();
			AssertNoMessageError(invoiceLine.US_FSISIndInfo, errorText);
			invoiceLine.FSISLines.RemoveAndDeleteAll();
			invoiceLine.AddInfoValidation.ValidateUS_FSISInd();
			AssertNoMessageError(invoiceLine.US_FSISIndInfo, errorText);
		}
	}
}
