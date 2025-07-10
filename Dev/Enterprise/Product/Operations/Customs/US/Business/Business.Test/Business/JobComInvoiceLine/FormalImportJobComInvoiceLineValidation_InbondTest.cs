using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FormalImportJobComInvoiceLineValidation_InbondTest : CommonImportJobComInvoiceLineValidationTest
	{
		[TestDate(2020, 11, 06)]
		public void TestCheckSTNRule()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "9101299010";
			tariff1.UE_DateFrom = new ZDateTime(2020, 11, 01);
			tariff1.UE_DateTo = new ZDateTime(2020, 12, 31);

			var tariffRule1 = Factory.New<USCTariffRule>();
			tariffRule1.U1_RuleCode = TariffRuleList.Codes.EligibleForSecondaryTariffNumbers;
			tariffRule1.U1_Tariff = tariff1.UE_Tariff;
			tariffRule1.U1_DateFrom = new ZDateTime(2020, 11, 01);

			var ruleSecondaryTariff1 = Factory.New<USCRuleSecondaryTariff>();
			ruleSecondaryTariff1.U3_U1 = tariffRule1.PK;
			ruleSecondaryTariff1.U3_TariffFrom = "9101299020";
			ruleSecondaryTariff1.U3_DateFrom = new ZDateTime(2020, 11, 01);
			ruleSecondaryTariff1.U3_DateTo = new ZDateTime(2020, 12, 31);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "9101297000", new ZDateTime(2020, 11, 01), new ZDateTime(2020, 12, 31));
			var attribute = helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, UniversalReferenceConstants.TariffAttributeTypes.Values.WatchStrap, tariff);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var parentLine = invoice.JobComInvoiceLines.AddNew();
			parentLine.JI_Tariff = "9101299010";
			var childLine = invoice.JobComInvoiceLines.AddNew();
			childLine.JI_Tariff = "9101297000";
			childLine.JI_ParentID = parentLine.PK;
			parentLine.US_IsParent = true;

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.US_DutyCalcDate = new ZDateTime(2020, 11, 06);
			Factory.Save();

			childLine.Validation.ValidateJI_Tariff();
			AssertHasMessageError(childLine.JI_TariffInfo, ValidationConstants.InvoiceLine.ImportTariff.ChildLineTariffNotFoundInRuleTariffs);

			var newFactory = new BusinessObjectFactory();
			var newHelper = new UniversalReferenceTestDataHelper(newFactory);
			var relationShip = newHelper.CreateTariffRelationship(tariff.PK, tariffType.PK, "91012990");
			var loadedLine = newFactory.Load<JobComInvoiceLine>(childLine.PK);
			loadedLine.Validation.ValidateJI_Tariff();
			AssertNoMessageError(loadedLine.JI_TariffInfo, ValidationConstants.InvoiceLine.ImportTariff.ChildLineTariffNotFoundInRuleTariffs);

			var loadedParentLine = newFactory.Load<JobComInvoiceLine>(parentLine.PK);
			loadedParentLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrors(loadedParentLine.JI_TariffInfo);
		}

		public void TestTariffValidationIsActiveOnlyWhenThereIsDateForDutyRate()
		{
			var reconDeclaration = new DeclarationTestHelper().GetDutiableReconDeclaration(Factory);
			reconDeclaration.InvoiceLines[0].JI_Tariff = "0000000000";
			AssertHasMessageErrorContaining(reconDeclaration.InvoiceLines[0].JI_TariffInfo, "Tariff unable to be found. ");

			reconDeclaration.OriginalEntries[0].US_R_DutyRateDate = ZDateTime.Empty;
			reconDeclaration.InvoiceLines[0].JI_Tariff = "0000000000";
			AssertNoMessageErrorContaining(reconDeclaration.InvoiceLines[0].JI_TariffInfo, "Tariff unable to be found. ");

			Factory.Save();
			AssertEquals("No Reference file request should have been sent", 0, reconDeclaration.Messages.Count);
			reconDeclaration.OriginalEntries[0].US_R_DutyRateDate = ZDateTime.Today;
			reconDeclaration.InvoiceLines[0].JI_Tariff = "1111234521";
			Factory.Save();
			AssertEquals("One Reference file request should have been sent", 1, reconDeclaration.Messages.Count);
		}

		public void TestCheckJI_TariffForQuota()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_QuotaIndicator = true;

			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "0000000001";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.Today.AddDays(1);

			JobComInvoiceLine invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_Tariff = "0000000000";

			AssertHasWarningContaining(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.QuotaMayBeApplicable);
			AssertNoWarningContaining(invoiceLine.JI_TariffInfo, "There is a query record which indicates that the quota for this tariff was");

			USCQuota quota = Factory.New<USCQuota>();
			quota.UT_BeginDate = ZDateTime.BrettsBirthday;
			quota.UT_EndDate = ZDateTime.Today.AddDays(1);
			quota.UT_PeriodProcessDateIndicator = PeriodProcessingDateIndicatorList.Codes.PresentationDate;
			quota.UT_Code = "0000000000";

			invoiceLine.JI_Tariff = "0000000000";

			AssertHasWarningContaining(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.QuotaMayBeApplicable);
			AssertHasWarningContaining(invoiceLine.JI_TariffInfo, "There is a query record which indicates that the quota for this tariff was");

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			MQEDIMessage message = (MQEDIMessage)invoiceLine.CusEntryLine.Header.Messages.AddNew(typeof(MQEDIMessage));
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryQuota;

			invoiceLine.JI_Tariff = "0000000000";

			AssertHasWarningContaining(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.QuotaMayBeApplicable);
			AssertHasWarningContaining(invoiceLine.JI_TariffInfo, "There is a query record which indicates that the quota for this tariff was");

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			invoiceLine.JI_Tariff = "000000000";
			AssertNoWarningContaining(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.QuotaMayBeApplicable);

			quota.UT_QuotaStatus = QuotaStatusList.Codes.Banned;
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			invoiceLine.JI_Tariff = "0000000000";
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.BannedImportMessage);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.BannedImportMessage);
		}

		public void TestCheckJI_TariffForDutyFreeGoods()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = true;

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "2208601000";
			tariff.UE_OGACodes = "FD4TL2";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_QuotaIndicator = false;

			JobComInvoiceLine invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "BOOZE";

			invoiceLine.US_TaxRate = 0m;
			invoiceLine.JI_Tariff = "2208601000";
			AssertHasWarningContaining(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.DutyFreeGoodsShouldNotBeBonded);

			invoiceLine.US_TaxRate = 1m;
			invoiceLine.JI_Tariff = "2208601000";
			AssertNoWarningContaining(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.DutyFreeGoodsShouldNotBeBonded);
		}

		public void TestCheckJI_TariffIsValidWhenItIsNotEmptyWithPossibleQuota()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = true;

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "2208601000";
			tariff.UE_OGACodes = "FD4TL2";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);

			JobComInvoiceLine invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "BOOZE";
			invoiceLine.US_TaxRate = 0m;

			tariff.UE_QuotaIndicator = false;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertHasWarningContaining(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.DutyFreeGoodsShouldNotBeBonded);

			tariff.UE_QuotaIndicator = true;
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertNoWarningContaining(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.DutyFreeGoodsShouldNotBeBonded);
		}

		public void TestCheckJI_TariffIsValidWhenItIsNotEmpty()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "2208601000";
			tariff.UE_OGACodes = "DT2";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);

			JobComInvoiceLine invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "BOOZE";
			invoiceLine.JI_Tariff = "2208601000";

			AssertNoMessageErrors(invoiceLine.JI_TariffInfo);

			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "9808003000";
			tariff2.UE_OGACodes = "DT2";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.Today.AddDays(1);

			var invoiceLine2 = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "9808003000";
			AssertNoWarningContaining(invoiceLine2.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.Chapter98or99ShouldBeEnteredInProvTariff);

			tariff2.UE_AdditionalTariffNumberIndicator = true;
			invoiceLine2.JI_Tariff = "9808003000";
			AssertHasWarningContaining(invoiceLine2.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.Chapter98or99ShouldBeEnteredInProvTariff);
		}

		public void TestSupplementaryTariffForQuota()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "99130425";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_QuotaIndicator = true;

			USCTariff tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "0000000001";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.Today.AddDays(1);

			JobComInvoiceLine invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SupTariff = "99130425";
			invoiceLine.JI_Tariff = "0000000001";

			AssertNoWarningContaining(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.QuotaMayBeApplicable);
			AssertNoWarningContaining(invoiceLine.JI_TariffInfo, "There is a query record which indicates that the quota for this tariff was");

			USCQuota quota = Factory.New<USCQuota>();
			quota.UT_BeginDate = ZDateTime.BrettsBirthday;
			quota.UT_EndDate = ZDateTime.Today.AddDays(1);
			quota.UT_PeriodProcessDateIndicator = PeriodProcessingDateIndicatorList.Codes.PresentationDate;
			quota.UT_Code = "99130425";
			quota.UT_SecondTariffNo = "0000000001";

			tariff1.UE_QuotaIndicator = true;
			invoiceLine.JI_Tariff = "0000000001";
			AssertHasWarningContaining(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.QuotaMayBeApplicable);
			AssertHasWarningContaining(invoiceLine.JI_TariffInfo, "There is a query record which indicates that the quota for this tariff was");

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			MQEDIMessage message = (MQEDIMessage)invoiceLine.CusEntryLine.Header.Messages.AddNew(typeof(MQEDIMessage));
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryQuota;
			invoiceLine.JI_Tariff = "0000000001";

			AssertHasWarningContaining(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.QuotaMayBeApplicable);
			AssertHasWarningContaining(invoiceLine.JI_TariffInfo, "There is a query record which indicates that the quota for this tariff was");

			quota.UT_QuotaStatus = QuotaStatusList.Codes.Banned;
			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			invoiceLine.JI_Tariff = "0000000001";
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.BannedImportMessage);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.JI_Tariff = "0000000001";
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.BannedImportMessage);
		}

		public void TestInvalidTariffAdvice()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.BrettsBirthday.AddYears(1);

			invoiceLine.JI_Tariff = "0000000000";
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, TariffValidator.SendTariffRequestMessagesAdviceForImport);

			tariff.Delete();
			invoiceLine.JI_Tariff = "0000000000";
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, TariffValidator.SendTariffRequestMessagesAdviceForImport);
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "If the duty calculation date has changed due to a Release Date Update or any other date changes, you can reset the date used to validate tariffs by selecting from the menu, Brokerage > Reset Duty Calculation Date.");
		}

		public void TestCheckJI_InvoiceUQ()
		{
			var org = Factory.New<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "LK234";
			classification.CC_TariffNum = "2010304050";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART321";
			var orgRel = part.RelatedOrganisations.AddOwner(org);
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = orgRel.OU_OH;
			pivot.CI_CC = classification.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.JI_InvoiceUQ = ZString.Empty;
			declaration.ValidationModes = ValidationModes.FTZPTTValidationMode;
			invoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertNoMessageError(invoiceLine.JI_InvoiceUQInfo, JobComInvoiceLineValidation.InvoiceUQIsRequiredForWarehouse("Inventory Management"));
			declaration.ValidationModes = ValidationModes.EntrySummary;
			invoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertHasMessageError(invoiceLine.JI_InvoiceUQInfo, JobComInvoiceLineValidation.InvoiceUQIsRequiredForWarehouse("Inventory Management"));

			invoiceLine.JI_PartNo = ZString.Empty;
			AssertNoMessageError(invoiceLine.JI_InvoiceUQInfo, JobComInvoiceLineValidation.InvoiceUQIsRequiredForWarehouse("Inventory Management"));

			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.JI_InvoiceUQ = ZString.Empty;
			AssertHasMessageError(invoiceLine.JI_InvoiceUQInfo, JobComInvoiceLineValidation.InvoiceUQIsRequiredForWarehouse("Inventory Management"));

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_ParentID = invoiceLine2.PK;
			AssertNoMessageError(invoiceLine.JI_InvoiceUQInfo, JobComInvoiceLineValidation.InvoiceUQIsRequiredForWarehouse("Inventory Management"));
			invoiceLine.JI_ParentID = ZGuid.Empty;
			AssertHasMessageError(invoiceLine.JI_InvoiceUQInfo, JobComInvoiceLineValidation.InvoiceUQIsRequiredForWarehouse("Inventory Management"));
			invoiceLine.US_JI_ParentProduct = invoiceLine2.PK;
			AssertNoMessageError(invoiceLine.JI_InvoiceUQInfo, JobComInvoiceLineValidation.InvoiceUQIsRequiredForWarehouse("Inventory Management"));
			invoiceLine.US_JI_ParentProduct = ZGuid.Empty;
			AssertHasMessageError(invoiceLine.JI_InvoiceUQInfo, JobComInvoiceLineValidation.InvoiceUQIsRequiredForWarehouse("Inventory Management"));

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			invoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertNoMessageError(invoiceLine.JI_InvoiceUQInfo, JobComInvoiceLineValidation.InvoiceUQIsRequiredForWarehouse("Inventory Management"));

			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa;
			invoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertHasMessageError(invoiceLine.JI_InvoiceUQInfo, JobComInvoiceLineValidation.InvoiceUQIsRequiredForWarehouse("Inventory Management"));
		}

		public void TestCheckJI_InvoiceQuantity()
		{
			var org = Factory.New<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "LK234";
			classification.CC_TariffNum = "2010304050";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART321";
			var orgRel = part.RelatedOrganisations.AddOwner(org);
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = orgRel.OU_OH;
			pivot.CI_CC = classification.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			declaration.ValidationModes = ValidationModes.FTZPTTValidationMode;
			invoiceLine.Validation.ValidateJI_InvoiceQuantity();
			AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, JobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse("Inventory Management"));
			declaration.ValidationModes = ValidationModes.EntrySummary;
			invoiceLine.Validation.ValidateJI_InvoiceQuantity();
			AssertHasMessageError(invoiceLine.JI_InvoiceQuantityInfo, JobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse("Inventory Management"));

			invoiceLine.JI_PartNo = ZString.Empty;
			AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, JobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse("Inventory Management"));

			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertHasMessageError(invoiceLine.JI_InvoiceQuantityInfo, JobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse("Inventory Management"));

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_ParentID = invoiceLine2.PK;
			AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, JobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse("Inventory Management"));
			invoiceLine.JI_ParentID = ZGuid.Empty;
			AssertHasMessageError(invoiceLine.JI_InvoiceQuantityInfo, JobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse("Inventory Management"));
			invoiceLine.US_JI_ParentProduct = invoiceLine2.PK;
			AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, JobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse("Inventory Management"));
			invoiceLine.US_JI_ParentProduct = ZGuid.Empty;
			AssertHasMessageError(invoiceLine.JI_InvoiceQuantityInfo, JobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse("Inventory Management"));

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			invoiceLine.Validation.ValidateJI_InvoiceQuantity();
			AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, JobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse("Inventory Management"));

			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa;
			invoiceLine.Validation.ValidateJI_InvoiceQuantity();
			AssertHasMessageError(invoiceLine.JI_InvoiceQuantityInfo, JobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse("Inventory Management"));

			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			invoiceLine.Validation.ValidateJI_InvoiceQuantity();
			AssertHasMessageError(invoiceLine.JI_InvoiceQuantityInfo, JobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse("Inventory Management"));
			AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, ValidationConstants.InvoiceLine.InvoiceQtyShouldBeEqualToSumOfWHSPackedQty("Inventory Management"));

			var whsPack = declaration.WHSPacks.AddNew();
			whsPack.US_PackageQty = 1;
			var whsPackLine = declaration.WHSPackLines.AddNew(invoiceLine);
			whsPackLine.US_PackedQty = 100m;
			invoiceLine.Validation.ValidateJI_InvoiceQuantity();
			AssertHasMessageError(invoiceLine.JI_InvoiceQuantityInfo, JobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse("Inventory Management"));
			AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, ValidationConstants.InvoiceLine.InvoiceQtyShouldBeEqualToSumOfWHSPackedQty("Inventory Management"));

			invoiceLine.JI_InvoiceQuantity = 10m;
			AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, JobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse("Inventory Management"));
			AssertHasMessageError(invoiceLine.JI_InvoiceQuantityInfo, ValidationConstants.InvoiceLine.InvoiceQtyShouldBeEqualToSumOfWHSPackedQty("Inventory Management"));

			invoiceLine.JI_InvoiceQuantity = 0m;
			AssertHasMessageError(invoiceLine.JI_InvoiceQuantityInfo, JobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse("Inventory Management"));
			AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, ValidationConstants.InvoiceLine.InvoiceQtyShouldBeEqualToSumOfWHSPackedQty("Inventory Management"));

			invoiceLine.JI_InvoiceQuantity = 100m;
			AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, JobComInvoiceLineValidation.InvoiceQtyIsRequiredForWarehouse("Inventory Management"));
			AssertNoMessageError(invoiceLine.JI_InvoiceQuantityInfo, ValidationConstants.InvoiceLine.InvoiceQtyShouldBeEqualToSumOfWHSPackedQty("Inventory Management"));
		}

		[TestDate(2009, 1, 1)]
		public void TestValidateCustomsQuantities()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

			JobComInvoiceLine invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "0101100010";
			invoiceLine.JI_CustomsQuantity = -1m;
			AssertHasMessageError(invoiceLine.JI_CustomsQuantityInfo, ValidationConstants.NegativeAmountNotAllowed);
		}

		public void TestCheckJI_LinePrice()
		{
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine.JI_Tariff = "6402195061";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 44010m;
			invoiceLine.JI_CustomsQuantity = 6750m;
			invoiceLine.JI_Weight = 1000m;

			var connection = ((IDbConnected)Factory).Connection;
			connection.BeginTransaction();
			try
			{
				DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
				var numberRange = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
				var branchNumberFountain = Env.NumberFountains.USInBondNumberFountain(GlbBranch.CurrentBranch.PK.ToGuid());

				branchNumberFountain.SetNext(Factory, (long)(numberRange.LastNumber - 101));
				var dateTime = ZDateTime.Now.AddHours(-1).ToDateTime();
				Enterprise.Customs.US.DataRegistry.Business.USCustomsDataRegistry.Instance.NextInBondNumberLimitWarningReportRun.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, dateTime);

				invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				IDutyData cusEntryLine = invoiceLine.CusEntryLine;
				var unitPrice = ((ZDecimal)(invoiceLine.CusEntryLine.TotalCustomsValueIncludingSecondaryLines / cusEntryLine.Quantity1)).Round(4);
				AssertHasMessageErrorContaining(invoiceLine.JI_LinePriceInfo, string.Format("The unit price ${0} calculated with the customs value divided by the first quantity of entry lines is outside of the range the selected tariff allows", unitPrice));
				AssertHasMessageErrorContaining(invoiceLine.JI_LinePriceInfo, " > 3.0000 and <= 6.5000");

				invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
				invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				AssertNoMessageErrorContaining(invoiceLine.JI_LinePriceInfo, " > 3.0000 and <= 6.5000");
			}
			finally
			{
				connection.RollbackTransaction();
			}
		}

		public void TestCheckJI_LinePriceForSetXLines()
		{
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			var vLine = invoiceLine.AddSecondaryInvoiceLine();
			AssertEquals("PreCondition", SecondarySpecProgIndicatorList.Codes.V, vLine.US_SecondarySPI);

			invoiceLine.JI_LinePrice = 100m;
			AssertHasWarning(invoiceLine.JI_LinePriceInfo, ImportLinePriceValidator.LinePriceOfXWillBeIgnoredFor7501);

			invoiceLine.JI_LinePrice = 0m;
			AssertNoWarning(invoiceLine.JI_LinePriceInfo, ImportLinePriceValidator.LinePriceOfXWillBeIgnoredFor7501);

			vLine.JI_LinePrice = 100m;
			AssertNoWarning(vLine.JI_LinePriceInfo, ImportLinePriceValidator.LinePriceOfXWillBeIgnoredFor7501);

			invoiceLine.JI_LinePrice = 100m;
			AssertNoWarning(invoiceLine.JI_LinePriceInfo, ImportLinePriceValidator.LinePriceOfXWillBeIgnoredFor7501);
		}

		public void TestCheckJI_CustomsQuantity()
		{
			#region Setup Tariffs

			var helper = new UniversalReferenceTestDataHelper(Factory);

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			var meltConditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.MELT);
			var smeltConditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Rate, UniversalReferenceConstants.TariffConditionTypes.Codes.SMELT);
			Factory.Save();

			var tariff72 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7206100000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, meltConditionType.PK, tariff72.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var tariff76 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7601103000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, smeltConditionType.PK, tariff76.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			Factory.Save();

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000111122";
			tariff.UE_Unit1 = "KG";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Now;
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.CompoundSpecificAndAdValoremFirstQuantity;

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "0000111121";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.Now;

			#endregion

			declaration.US_IsInvoiceByRequest = true;
			declaration.US_EnableAII = true;
			invoice.US_IsLineGrouping = true;
			var range = invoiceLine.LineGroupingRanges.AddNew();
			var aiiLine1 = invoiceLine.AIILines.AddNew(range);
			aiiLine1.US_CustomsQty = 10m;
			var aiiLine2 = invoiceLine.AIILines.AddNew(range);
			aiiLine2.US_CustomsQty = 30m;

			invoice.US_IsLineGrouping = false;
			invoiceLine.JI_Tariff = "0000111122";
			invoiceLine.JI_CustomsQuantity = 0m;
			AssertHasMessageError(invoiceLine.JI_CustomsQuantityInfo, ValidationConstants.StatQTYRequired);

			var childInvoiceLine = invoice.InvoiceLines.AddNew();
			childInvoiceLine.JI_Tariff = "0000111122";
			childInvoiceLine.JI_ParentID = invoiceLine.PK;
			childInvoiceLine.JI_CustomsQuantity = 0m;
			AssertHasWarning(childInvoiceLine.JI_CustomsQuantityInfo, ValidationConstants.WarnIfStatQTYisZero);

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "7601203000";
			invoiceLine3.JI_CustomsQuantity = 0m;
			AssertHasMessageError(invoiceLine3.JI_CustomsQuantityInfo, ValidationConstants.StatQTYRequired);

			invoiceLine3.JI_CustomsQuantity = 10m;
			AssertNoMessageError(invoiceLine3.JI_CustomsQuantityInfo, ValidationConstants.StatQTYRequired);

			invoiceLine3.JI_Tariff = "7206100000";
			invoiceLine3.JI_CustomsQuantity = 0m;
			AssertNoMessageError(invoiceLine3.JI_CustomsQuantityInfo, ValidationConstants.StatQTYRequired);
			AssertHasWarning(invoiceLine3.JI_CustomsQuantityInfo, ValidationConstants.WarnIfStatQTYisZero);

			invoiceLine3.JI_CustomsQuantity = 10m;
			AssertNoMessageError(invoiceLine3.JI_CustomsQuantityInfo, ValidationConstants.StatQTYRequired);
			AssertNoWarning(invoiceLine3.JI_CustomsQuantityInfo, ValidationConstants.WarnIfStatQTYisZero);

			invoiceLine3.JI_Tariff = "7601103000";
			invoiceLine3.JI_CustomsQuantity = 0m;
			AssertNoMessageError(invoiceLine3.JI_CustomsQuantityInfo, ValidationConstants.StatQTYRequired);
			AssertHasWarning(invoiceLine3.JI_CustomsQuantityInfo, ValidationConstants.WarnIfStatQTYisZero);

			invoiceLine3.JI_CustomsQuantity = 10m;
			AssertNoMessageError(invoiceLine3.JI_CustomsQuantityInfo, ValidationConstants.StatQTYRequired);
			AssertNoWarning(invoiceLine3.JI_CustomsQuantityInfo, ValidationConstants.WarnIfStatQTYisZero);
		}

		public void TestCheckJI_DescriptionForPGA()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableINB = false;
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			var ttbLine = invoiceLine.TTBLines.AddNew();
			ttbLine.US_ProcessingCode = TTBProgramCodeList.Codes.Beverage;
			invoiceLine.JI_Description = "A";
			AssertNoMessageErrorContaining(invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_Description = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_CertifyCargoRelease = false;
			invoiceLine.Validation.ValidateJI_Description();
			AssertHasMessageErrorContaining(invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			invoiceLine.Validation.ValidateJI_Description();
			AssertHasMessageErrorContaining(invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_EnableCRL = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			declaration.US_EnableINB = false;
			invoiceLine.Validation.ValidateJI_Description();
			AssertNoMessageErrorContaining(invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_NOPInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine2.JI_Description = "A";
			AssertNoMessageErrorContaining(invoiceLine2.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			invoiceLine2.JI_Description = "";
			invoiceLine2.Validation.ValidateJI_Description();
			AssertHasMessageErrorContaining(invoiceLine2.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_Volume()
		{
			invoiceLine.JI_Volume = -10m;
			AssertHasErrors(invoiceLine.JI_VolumeInfo);

			invoiceLine.JI_Volume = 10m;
			AssertNoErrors(invoiceLine.JI_VolumeInfo);
		}

		public void TestWarnedWhenDutyFreeGoodsIsBonded()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "10000000";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_Column1RateSpecific = 0.5m;

			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "10001011";
			tariff2.UE_DutyComputationCode = ComputationCodeList.Codes.Free;
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff2.UE_Column1RateSpecific = ZDecimal.Zero;

			declaration.US_EnableINB = false;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("IsDutyFree", true, invoiceLine.IsDutyFree);
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasWarning(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.DutyFreeGoodsShouldNotBeBonded);

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_CustomsQuantity = 1000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("IsDutyFree", false, invoiceLine.IsDutyFree);
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoWarning(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.DutyFreeGoodsShouldNotBeBonded);

			declaration.US_EntryType = "";
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoWarning(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.DutyFreeGoodsShouldNotBeBonded);

			declaration.US_EntryType = EntryTypeList.Codes.ImmediateExportation;
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoWarning(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.DutyFreeGoodsShouldNotBeBonded);

			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoWarning(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.DutyFreeGoodsShouldNotBeBonded);

			const string tariffNumber = "2208201000";
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			tariff = new USCTariff.Loader(Factory).LoadBestMatch(tariffNumber, ZDateTime.Now);
			if (tariff == null)
			{
				tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = tariffNumber;
				tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
				tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
				tariff.UE_Unit1 = "PFL";
				tariff.UE_DutyComputationCode = "1";
				tariff.UE_Column2RateSpecific = 1.78m;
			}

			var taxRate = tariff.DutyRates.Count > 0 ? tariff.DutyRates[0] : null;
			if (taxRate == null)
			{
				taxRate = tariff.DutyRates.AddNew();
				taxRate.UD_DutyElement = "5";
				taxRate.UD_TaxFeeClassCode = "016";
				taxRate.UD_TaxFeeComputationCode = "1";
				taxRate.UD_TaxFeeFlag = "2";
				taxRate.UD_TaxFeeSpecificRate = 3.566322m;
			}

			invoiceLine.JI_Tariff = tariffNumber;
			invoiceLine.US_TaxApply = YesNoDefaultList.Codes.Yes;
			invoiceLine.JI_CustomsQuantity = 250.0m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("IsTaxOrFeePayable", true, invoiceLine.IsTaxOrFeePayable);
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoWarning(invoiceLine.JI_TariffInfo, FormalImportJobComInvoiceLineValidation.DutyFreeGoodsShouldNotBeBonded);
		}

		Bill bill;

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableINB = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;

			bill = declaration.Bills.AddNew();
			bill.US_AMSCarrierIndicator = YesNoDefaultList.Codes.No;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}
	}
}
