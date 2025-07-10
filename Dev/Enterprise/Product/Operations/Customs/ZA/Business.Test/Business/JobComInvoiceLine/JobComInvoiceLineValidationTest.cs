using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class JobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJI_ValuationMarkup()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			CombineAssertions(() =>
			{
				invoiceLine.JI_ValuationMarkup = -10;
				AssertHasErrorContaining(invoiceLine.JI_ValuationMarkupInfo, "Valuation Markup % cannot be negative.");
			});
			CombineAssertions("Check Markup vs. Customs Value Override", () =>
			{
				invoiceLine.JI_ValuationMarkup = 10;
				AssertNoWarnings(invoiceLine.JI_ValuationMarkupInfo);
				invoiceLine.JI_CustomsValueOverride = 10;
				AssertHasWarningContaining(invoiceLine.JI_ValuationMarkupInfo, "The Valuation Markup % won't take effect since you have entered an Overridden Customs Value");
				invoiceLine.JI_ValuationMarkup = 0;
				AssertNoWarnings(invoiceLine.JI_ValuationMarkupInfo);
			});
			CombineAssertions("Check Markup vs. Header VDN", () =>
			{
				invoiceLine.JI_CustomsValueOverride = 0;
				invoiceLine.JI_ValuationMarkup = 10;
				AssertHasMessageErrorContaining(invoiceLine.JI_ValuationMarkupInfo, "Please make sure the parent Invoice Header has a valid VDN number");
				AssertNoWarnings(invoiceLine.JI_ValuationMarkupInfo);
			});
			CombineAssertions("Check Markup vs. Header Markup", () =>
			{
				invoice.JZ_VDN = "VDN";
				invoice.JZ_ValuationMarkup = 5;
				invoiceLine.JI_ValuationMarkup = 11;
				AssertNoMessageErrors("Check1", invoiceLine.JI_ValuationMarkupInfo);
				AssertNoWarnings("Check2", invoiceLine.JI_ValuationMarkupInfo);
				invoiceLine.JI_ValuationMarkup = 0;
				AssertNoMessageErrors(invoiceLine.JI_ValuationMarkupInfo);
				AssertHasWarningContaining(invoiceLine.JI_ValuationMarkupInfo, "You may need to provide a Valuation Markup % since the parent Invoice Header has captured a Valuation Markup % value");
				invoiceLine.JI_CustomsValueOverride = 123;
				AssertNoMessageErrors(invoiceLine.JI_ValuationMarkupInfo);
				AssertNoWarnings(invoiceLine.JI_ValuationMarkupInfo);
				AssertNoErrors(invoiceLine.JI_ValuationMarkupInfo);
			});
		}

		public void TestCheckJI_Procedure_IsExcise()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			testHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "45", "00", "", "", "EXW", group: UniversalReferenceConstants.RefCusProcedureGroup.Excise);
			var tariffType12A = testHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part2A);
			Factory.Save();
			var tariff = testHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12A.PK, "992002", new ZDateTime(2016, 08, 02), new ZDateTime(2079, 06, 06));
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = "45";
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = testInstruction1.PK;
			invLine.JI_Procedure = "4500";
			AssertHasMessageError(invLine.JI_ProcedureInfo, ValidationConstants.InvoiceLine.ThisEntryIsUsingAnExciseCPCButNoExciseDataDeclared);
			var lineTariff = invLine.CusLineTariffDetails.AddNew("12A", "992002");
			invLine.JI_Procedure = "4500";
			AssertNoMessageError(invLine.JI_ProcedureInfo, ValidationConstants.InvoiceLine.ThisEntryIsUsingAnExciseCPCButNoExciseDataDeclared);
		}

		public void TestCheckJI_PreviousEntryNumberSameOnAllLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._66;
			entryInstruction.CEI_PreviousMRN = "PREMRN";
			string previousMRNSForSameProcedureMustBeSame = ValidationConstants.InvoiceLine.PreviousMRNSForSameProcedureMustBeSame(entryInstruction.CEI_Style);
			JobComInvoiceHeader invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invLine1.JI_PreviousEntryNumber = "PREMRN1";
			invLine2.JI_PreviousEntryNumber = "PREMRN2";
			AssertHasMessageError(invLine2.JI_PreviousEntryNumberInfo, previousMRNSForSameProcedureMustBeSame);
			invLine2.JI_PreviousEntryNumber = "PREMRN1";
			AssertNoMessageError(invLine2.JI_PreviousEntryNumberInfo, previousMRNSForSameProcedureMustBeSame);
			invLine2.JI_PreviousEntryNumber = ZString.Empty;
			AssertHasMessageError(invLine2.JI_PreviousEntryNumberInfo, previousMRNSForSameProcedureMustBeSame);
			invLine1.JI_PreviousEntryNumber = ZString.Empty;
			AssertNoMessageError(invLine1.JI_PreviousEntryNumberInfo, previousMRNSForSameProcedureMustBeSame);
		}

		public void TestCheckJI_PreviousEntryNumber()
		{
			helper.CreateCustomsOfficeCusCodeEntry("DBN");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			CombineAssertions("Test MRN", () =>
			{
				invoiceLine.JI_PreviousEntryNumber = "ABC";
				AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, Common.ZA.ZAValidationConstants.InvalidMRNLength);
				invoiceLine.JI_PreviousEntryNumber = "ABC".PadRight(18, '0');
				AssertNoMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, Common.ZA.ZAValidationConstants.InvalidMRNLength);
				AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, ValidationConstants.Shared.InvalidMRNCustomsOffice);
				ZString sTomorrow = ZDateTime.Today.AddDays(1).ToString("yyyyMMdd");
				invoiceLine.JI_PreviousEntryNumber = ZString.Format("{0}{1}", "DBN", sTomorrow).PadRight(18, '0');
				AssertNoMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, ValidationConstants.Shared.InvalidMRNCustomsOffice);
				AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, ValidationConstants.Shared.InvalidMRNDate);
				invoiceLine.JI_PreviousEntryNumber = "DBN20160101@@".PadRight(18, '0');
				AssertNoMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, ValidationConstants.Shared.InvalidMRNDate);
				AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, ValidationConstants.Shared.InvalidMRNNumbersOnly);
				invoiceLine.JI_PreviousEntryNumber = "DBN20160101".PadRight(18, '0');
				AssertNoMessageErrors(invoiceLine.JI_PreviousEntryNumberInfo);
			});
			CombineAssertions("Test Mandatory MRN", () =>
			{
				invoiceLine.JI_PreviousEntryNumber = "";
				AssertNoMessageError(invoiceLine.JI_PreviousEntryNumberInfo, ValidationConstants.InvoiceLine.PreviousMRNRequiredForPPC);
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = ProcedureCodes._62;
				invoiceLine.JI_CEI = entryInstruction.PK;
				invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + ProcedureCodes._11;
				invoiceLine.JI_PreviousEntryNumber = "";
				AssertHasMessageError(invoiceLine.JI_PreviousEntryNumberInfo, ValidationConstants.InvoiceLine.PreviousMRNRequiredForPPC);
				entryInstruction.CEI_PreviousMRN = "@@";
				AssertNoMessageError(invoiceLine.JI_PreviousEntryNumberInfo, ValidationConstants.InvoiceLine.PreviousMRNRequiredForPPC);
				entryInstruction.CEI_PreviousMRN = "";
				invoiceLine.JI_PreviousEntryNumber = "@@";
				AssertNoMessageError(invoiceLine.JI_PreviousEntryNumberInfo, ValidationConstants.InvoiceLine.PreviousMRNRequiredForPPC);
				invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + ProcedureCodes._40;
				invoiceLine.JI_PreviousEntryNumber = "";
				AssertHasMessageError(invoiceLine.JI_PreviousEntryNumberInfo, ValidationConstants.InvoiceLine.PreviousMRNRequiredForPPC);
				invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + ProcedureCodes._00;
				invoiceLine.JI_PreviousEntryNumber = "";
				AssertNoMessageError(invoiceLine.JI_PreviousEntryNumberInfo, ValidationConstants.InvoiceLine.PreviousMRNRequiredForPPC);
			});
			CombineAssertions("Test for IMX", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
				invoiceLine.JI_PreviousEntryNumber = "ABC";
				AssertNoMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, Common.ZA.ZAValidationConstants.InvalidMRNLength);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				invoiceLine.JI_PreviousEntryNumber = "ABC";
				AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryNumberInfo, Common.ZA.ZAValidationConstants.InvalidMRNLength);
			});
		}

		public void TestCheckJI_PreviousEntryLineNumber()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryInstruction instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			instruction.CEI_Style = "11";
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.Validation.ValidateJI_PreviousEntryLineNumber();
			AssertEquals(false, invoiceLine.JI_PreviousEntryLineNumberInfo.HasNotifications());
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "00";
			invoiceLine.Validation.ValidateJI_PreviousEntryLineNumber();
			AssertEquals(false, invoiceLine.JI_PreviousEntryLineNumberInfo.HasNotifications());
			AssertNoMessageError(invoiceLine.JI_PreviousEntryLineNumberInfo, ValidationConstants.InvoiceLine.PreviousMRNLineNumberRequiredForPPC);
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "40";
			invoiceLine.Validation.ValidateJI_PreviousEntryLineNumber();
			AssertEquals(true, invoiceLine.JI_PreviousEntryLineNumberInfo.HasNotifications());
			AssertHasMessageError(invoiceLine.JI_PreviousEntryLineNumberInfo, ValidationConstants.InvoiceLine.PreviousMRNLineNumberRequiredForPPC);
			instruction.CEI_Style = "67";
			invoiceLine.Validation.ValidateJI_PreviousEntryLineNumber();
			AssertEquals(true, invoiceLine.JI_PreviousEntryLineNumberInfo.HasNotifications());
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "";
			invoiceLine.Validation.ValidateJI_PreviousEntryLineNumber();
			AssertEquals(false, invoiceLine.JI_PreviousEntryLineNumberInfo.HasNotifications());
		}

		public void TestCheckJI_PreviousEntryLineNumberMaxValue()
		{
			var mockInvoiceLine = Factory.NewMoq<JobComInvoiceLine>();
			mockInvoiceLine.Setup(m => m.JI_PreviousEntryLineNumber)
				.Returns(12345);
			var invoiceLine = mockInvoiceLine.Object;
			invoiceLine.Validation.ValidateJI_PreviousEntryLineNumber();
			AssertHasMessageError(invoiceLine.JI_PreviousEntryLineNumberInfo, ValidationConstants.InvoiceLine.PreviousMRNLineNumberMinMaxValue);
			mockInvoiceLine.Setup(m => m.JI_PreviousEntryLineNumber)
				.Returns(9999);
			invoiceLine.Validation.ValidateJI_PreviousEntryLineNumber();
			AssertNoMessageError(invoiceLine.JI_PreviousEntryLineNumberInfo, ValidationConstants.InvoiceLine.PreviousMRNLineNumberMinMaxValue);
		}

		public void TestCheckJI_PreviousEntryLineNumberMinValue()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_PreviousEntryLineNumber = -123;
			AssertHasMessageError(invoiceLine.JI_PreviousEntryLineNumberInfo, ValidationConstants.InvoiceLine.PreviousMRNLineNumberMinMaxValue);
			invoiceLine.JI_PreviousEntryLineNumber = 9999;
			AssertNoMessageError(invoiceLine.JI_PreviousEntryLineNumberInfo, ValidationConstants.InvoiceLine.PreviousMRNLineNumberMinMaxValue);
		}

		public void TestCheckJI_PreviousEntryLineNumberIMX()
		{
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				CusEntryInstruction instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				var testInvoice = declaration.Invoices.AddNew();
				testInvoice.JZ_InvoiceNumber = "INV3";
				testInvoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
				var testEntryHeader = declaration.CustomsEntryHeaders.AddNew();
				var testEntryLine = testEntryHeader.AllEntryLines.AddNew();
				JobComInvoiceLine invoiceLine = testInvoice.InvoiceLines.AddNew();
				instruction.CEI_Style = "11";
				invoiceLine.JI_CEI = instruction.PK;
				invoiceLine.Validation.ValidateJI_PreviousEntryLineNumber();
				invoiceLine.JI_PreviousEntryLineNumber = 10;
				AssertEquals(false, invoiceLine.JI_PreviousEntryLineNumberInfo.HasNotifications());
				invoiceLine.JI_PreviousEntryLineNumber = ZShort.Zero;
				invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "00";
				invoiceLine.Validation.ValidateJI_PreviousEntryLineNumber();
				AssertEquals(true, invoiceLine.JI_PreviousEntryLineNumberInfo.HasNotifications());
				AssertHasMessageErrorContaining(invoiceLine.JI_PreviousEntryLineNumberInfo, ValidationConstants.InvoiceLine.PreviousMRNLineNumberRequiredForIMX);
				invoiceLine.JI_CL = testEntryLine.PK;
				invoiceLine.JI_PreviousEntryLineNumber = 10;
				JobComInvoiceLine invoiceLine2 = testInvoice.InvoiceLines.AddNew();
				invoiceLine2.JI_CEI = instruction.PK;
				invoiceLine2.JI_Procedure = invoiceLine2.EntryInstruction.CEI_Style + "00";
				invoiceLine2.JI_PreviousEntryLineNumber = 11;
				invoiceLine2.Validation.ValidateJI_PreviousEntryLineNumber();
				AssertEquals(false, invoiceLine2.JI_PreviousEntryLineNumberInfo.HasNotifications());
				invoiceLine2.JI_PreviousEntryLineNumber = 10;
				invoiceLine2.Validation.ValidateJI_PreviousEntryLineNumber();
				AssertEquals(true, invoiceLine2.JI_PreviousEntryLineNumberInfo.HasNotifications());
				AssertHasMessageErrorContaining(invoiceLine2.JI_PreviousEntryLineNumberInfo, ValidationConstants.InvoiceLine.DuplicateMRNLineNumberExistPerEntryInstruction);
				invoiceLine2.JI_CL = testEntryLine.PK;
				AssertNoError(invoiceLine2.JI_PreviousEntryLineNumberInfo, JobComInvoiceLineValidation.WarehouseTransactionExistsNeedsCancel);
				invoiceLine2.EntryInstruction.EntryHeader.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
				Factory.Save();
				invoiceLine2.JI_PreviousEntryLineNumber = 11;
				AssertHasError(invoiceLine2.JI_PreviousEntryLineNumberInfo, JobComInvoiceLineValidation.WarehouseTransactionExistsNeedsCancel);
				invoiceLine2.EntryInstruction.EntryHeader.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCanceled;
				Factory.Save();
				invoiceLine2.JI_PreviousEntryLineNumber = 12;
				AssertNoError(invoiceLine2.JI_PreviousEntryLineNumberInfo, JobComInvoiceLineValidation.WarehouseTransactionExistsNeedsCancel);
				var testEntryHeader2 = declaration.CustomsEntryHeaders.AddNew();
				var testEntryLine2 = testEntryHeader2.AllEntryLines.AddNew();
				JobComInvoiceLine invoiceLine3 = testInvoice.InvoiceLines.AddNew();
				invoiceLine3.EntryInstruction.EntryHeader.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
				Factory.Save();
				invoiceLine3.JI_CEI = instruction.PK;
				invoiceLine3.JI_Procedure = invoiceLine2.EntryInstruction.CEI_Style + "00";
				invoiceLine3.JI_PreviousEntryLineNumber = 13;
				invoiceLine3.JI_CL = testEntryLine2.PK;
				invoiceLine3.Validation.ValidateJI_PreviousEntryLineNumber();
				AssertHasError(invoiceLine3.JI_PreviousEntryLineNumberInfo, JobComInvoiceLineValidation.WarehouseTransactionExistsNeedsCancel);
			}
		}

		public void TestCheckJI_BondedWhsQuantity()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ProcedureCodes._40;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_BondedWhsUnitQty = "KG";
			invoiceLine.JI_BondedWhsQuantity = 0;
			AssertHasMessageErrors(invoiceLine.JI_BondedWhsQuantityInfo);
			invoiceLine.JI_CEI = instruction.PK;
			AssertHasMessageErrorContaining(invoiceLine.JI_BondedWhsQuantityInfo, "Please enter a Countable Quantity.");
			invoiceLine.JI_BondedWhsQuantity = 10;
			AssertNoMessageErrorContaining(invoiceLine.JI_BondedWhsQuantityInfo, "Please enter a Countable Quantity.");
			instruction.CEI_Style = ProcedureCodes._10;
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_BondedWhsQuantity = 0;
			AssertNoMessageErrorContaining(invoiceLine.JI_BondedWhsQuantityInfo, "Please enter a Countable Quantity.");
		}

		public void TestCheckJI_BondedWhsUnitQty()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ProcedureCodes._40;
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_BondedWhsQuantity = 10;
			invoiceLine.JI_BondedWhsUnitQty = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_BondedWhsUnitQtyInfo, "You have not entered a unit of quantity.");
			invoiceLine.JI_BondedWhsUnitQty = "XX";
			AssertHasMessageErrorContaining(invoiceLine.JI_BondedWhsUnitQtyInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_CustomsUnitQty = "LI";
			invoiceLine.JI_BondedWhsUnitQty = "LI";
			AssertNoNotifications(invoiceLine.JI_BondedWhsUnitQtyInfo);
			invoiceLine.JI_CustomsThirdUnitQty = "KG";
			invoiceLine.JI_BondedWhsUnitQty = "KG";
			AssertNoNotifications(invoiceLine.JI_BondedWhsUnitQtyInfo);
			invoiceLine.JI_CustomsSecondUnitQty = "CU";
			invoiceLine.JI_BondedWhsUnitQty = "CU";
			AssertNoNotifications(invoiceLine.JI_BondedWhsUnitQtyInfo);
		}

		public void TestCheckJI_CEI()
		{
			var helper = new TestHelper(Factory);
			helper.SetExchangeRate(helper.USDCurrency, 0.5m, ZDateTime.Today);
			helper.SetExchangeRate(helper.USDCurrency, 0.5m, ZDateTime.Today.AddDays(-1));
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var instruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = ProcedureCodes._10;
			instruction1.CEI_Description = "qwe";
			var instruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction2.CEI_Style = ProcedureCodes._10;
			instruction2.CEI_Description = "asd";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV001";
			var line1 = invoice.InvoiceLines.AddNew();
			line1.JI_CEI = ZGuid.Empty;
			AssertHasMessageErrorContaining(line1.JI_CEIInfo, "You have not entered an Entry Instruction.");
			line1.JI_CEI = instruction1.PK;
			AssertNoMessageErrorContaining(line1.JI_CEIInfo, "You have not entered an Entry Instruction.");
			var standaloneInv = Factory.New<BaseJobComInvoiceHeader>();
			var fakeDec = new FakeDeclarationCreatorForInvoice(standaloneInv);
			Assert("Prereq: Fake declaration, i.e. Commercial Invoice", !standaloneInv.IsAttachedToPersistentDeclaration);
			var standaloneInvLine = standaloneInv.InvoiceLines.AddNew();
			standaloneInvLine.JI_CEI = ZGuid.Empty;
			AssertNoMessageErrors("JI_CEI should not be validated for commerical invoices", standaloneInvLine.JI_CEIInfo);
		}

		public void TestCheckJI_Description()
		{
			var invoice = Factory.New<JobComInvoiceLine>();
			invoice.JI_Description = "";
			AssertHasMessageErrorContaining(invoice.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			invoice.JI_Description = "XX";
			AssertNoMessageErrorContaining(invoice.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_Weight()
		{
			var invoice = Factory.New<JobComInvoiceLine>();
			invoice.JI_Weight = 0m;
			AssertHasMessageErrorContaining(invoice.JI_WeightInfo, MandatoryValidation.YouHaveNotEntered);
			invoice.JI_Weight = 55m;
			AssertNoMessageErrorContaining(invoice.JI_WeightInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestJI_CustomsQuantityNotZero()
		{
			var invoice = Factory.New<JobComInvoiceLine>();
			invoice.JI_CustomsQuantity = 0m;
			AssertHasMessageErrorContaining(invoice.JI_CustomsQuantityInfo, "Customs Qty should be greater than zero");
			invoice.JI_CustomsQuantity = 55;
			AssertNoMessageErrorContaining(invoice.JI_CustomsQuantityInfo, "Customs Qty should be greater than zero");
		}

		public void TestCheckJZ_RN_NKDefaultOrigin()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_CountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_CountryOfOrigin = "!@";
			AssertNoMessageErrorContaining(invoiceLine.JI_CountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(invoiceLine.JI_CountryOfOriginInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			AssertNoMessageErrorContaining(invoiceLine.JI_CountryOfOriginInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJZ_RN_NKDefaultOrigin_MustBeZA()
		{
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			foreach (var procedureCode in new ZString[] { "51", "52", "64", "68", "90" })
			{
				entryInstruction.CEI_Style = procedureCode;
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
				AssertNoMessageError($"{procedureCode}/{invoiceLine.JI_CountryOfOrigin}", invoiceLine.JI_CountryOfOriginInfo, ValidationConstants.InvoiceLine.CountryOfOriginMustBeZA);
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Namibia;
				AssertHasMessageError($"{procedureCode}/{invoiceLine.JI_CountryOfOrigin}", invoiceLine.JI_CountryOfOriginInfo, ValidationConstants.InvoiceLine.CountryOfOriginMustBeZA);
			}

			entryInstruction.CEI_Style = "75";
			AssertNoMessageError($"75/{invoiceLine.JI_CountryOfOrigin}", invoiceLine.JI_CountryOfOriginInfo, ValidationConstants.InvoiceLine.CountryOfOriginMustBeZA);
		}

		public void TestCheckJZ_RN_NKDefaultOrigin_CannotBeZA()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			const string cannotBeZANotification = "Goods Origin Cannot be South Africa (ZA) for selected Procedure Code.";

			foreach (var procedureCode in new ZString[] { "36", "38", "62", "65", "66", "83" })
			{
				entryInstruction.CEI_Style = procedureCode;
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
				AssertHasMessageError(invoiceLine.JI_CountryOfOriginInfo, cannotBeZANotification);
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Namibia;
				AssertNoMessageError(invoiceLine.JI_CountryOfOriginInfo, cannotBeZANotification);
			}

			entryInstruction.CEI_Style = "75";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			AssertNoMessageError($"75/{invoiceLine.JI_CountryOfOrigin}", invoiceLine.JI_CountryOfOriginInfo, cannotBeZANotification);
		}

		public void TestCheckJI_PrimaryPreference()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateAdditionalInformationCusCodeEntry("EUR");
			Factory.Save();
			var tariffType1P1 = testHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var preference = testHelper.CreatePreferenceForCountryAndGrouping("200", "EUTRADE", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			var rateType_ZA_DTY = testHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			var rateCode_ZA_DTY_D = testHelper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_DTY.PK);
			Factory.Save();
			var startDate = ZDateTime.BrettsBirthday;
			var endDate = ZDateTime.Today.AddDays(1);
			var tariff = testHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1010102030", startDate, endDate);
			var tariff1P1Rate = testHelper.CreateRate(tariff, rateCode_ZA_DTY_D.PK, startDate, endDate, "0.1 * VFD", preference.PK);
			Factory.Save();
			var testTradeGroup1 = testHelper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "EUTRADE", new ZDateTime(1980, 01, 01), new ZDateTime(2079, 06, 06));
			testHelper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.Germany, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			testHelper.CreateCusApplicability(tariff1P1Rate, testTradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			Factory.Save();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			CombineAssertions("Trade Agreement Exports", () =>
			{
				invoiceLine.JI_ROOCert = "123";
				invoiceLine.JI_PrimaryPreference = ZString.Empty;
				AssertHasMessageError(invoiceLine.JI_PrimaryPreferenceInfo, ValidationConstants.InvoiceLine.NoROOTypeEnteredForCert);
				invoiceLine.JI_PrimaryPreference = "!@";
				AssertHasMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
				invoiceLine.JI_PrimaryPreference = "EUR";
				AssertNoMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, ListValidation.InvalidCodeMessageError);
				invoiceLine.JI_ROOCert = string.Empty;
				invoiceLine.JI_PrimaryPreference = ZString.Empty;
				AssertNoMessageError(invoiceLine.JI_ROOCertInfo, ValidationConstants.InvoiceLine.NoROOCertEnteredForType);
				invoiceLine.JI_PrimaryPreference = "EUR";
				AssertHasMessageError(invoiceLine.JI_ROOCertInfo, ValidationConstants.InvoiceLine.NoROOCertEnteredForType);
			});
			CombineAssertions("Trade Agreement Imports", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				invoiceLine.JI_ROOCert = string.Empty;
				invoiceLine.JI_PrimaryPreference = ZString.Empty;
				invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Germany;
				var noROOCertEnteredForTradeAgreementWarning = "Rules of Origin Certificate may need to be provided";
				AssertHasMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoWarningContaining(invoiceLine.JI_ROOCertInfo, noROOCertEnteredForTradeAgreementWarning);
				invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Standard;
				AssertNoMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoWarningContaining(invoiceLine.JI_ROOCertInfo, noROOCertEnteredForTradeAgreementWarning);
				invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.PreferentialRate;
				AssertNoMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasWarningContaining(invoiceLine.JI_ROOCertInfo, noROOCertEnteredForTradeAgreementWarning);
			});
			CombineAssertions("Trade Agreement Exbond", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
				invoiceLine.JI_PrimaryPreference = ZString.Empty;
				AssertHasMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, MandatoryValidation.YouHaveNotEntered);
				invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Standard;
				AssertNoMessageErrorContaining(invoiceLine.JI_PrimaryPreferenceInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		[TestDate(2015, 7, 1)]
		public void TestCheckJI_Tariff()
		{
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType12A = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12A");
			var rateType_ZA_DTY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			var rateCode_ZA_DTY_D = helper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_DTY.PK);
			var preference100 = helper.CreatePreferenceForCountryAndGrouping("100", "None", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			var preference200 = helper.CreatePreferenceForCountryAndGrouping("200", "Preferential Rate", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			Factory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1020304050", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff1Rate1 = helper.CreateRate(tariff1, rateCode_ZA_DTY_D.PK, new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1), preferencePk: preference100.PK);
			var tariff1Rate2 = helper.CreateRate(tariff1, rateCode_ZA_DTY_D.PK, new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1), preferencePk: preference200.PK);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1020304060", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1020304070", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff3Rate = helper.CreateRate(tariff3, rateCode_ZA_DTY_D.PK, new ZDateTime(2015, 1, 1), new ZDateTime(2015, 5, 1));
			var tariff4 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12A.PK, "1020304080", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			Factory.Save();
			var tradeGroup1 = helper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "STANDARD", new ZDateTime(1980, 01, 01), new ZDateTime(2079, 06, 06));
			helper.AddCountry(tradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			helper.AddCountry(tradeGroup1, Enterprise.Core.Constants.CountryCodes.NewZealand, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			var tradeGroup2 = helper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "NZTRADE", new ZDateTime(1980, 01, 01), new ZDateTime(2079, 06, 06));
			helper.AddCountry(tradeGroup2, Enterprise.Core.Constants.CountryCodes.NewZealand, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			var testApplicability1 = helper.CreateCusApplicability(tariff1Rate1, tradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability2 = helper.CreateCusApplicability(tariff1Rate2, tradeGroup2, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			Factory.Save();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine.JI_Tariff = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff may not be empty");
			invoiceLine.JI_PartNo = "NEWPART";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasWarning("Tariff has warning", invoiceLine.JI_TariffInfo, JobComInvoiceLineValidation.MandatoryCCOrTariffForAutoCreateProduct);
			invoiceLine.JI_Tariff = "1020304090";
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff may not be empty");
			AssertNoWarning("Tariff has warning", invoiceLine.JI_TariffInfo, JobComInvoiceLineValidation.MandatoryCCOrTariffForAutoCreateProduct);
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "NEWCODE";
			classification.CC_IsActive = true;
			Factory.Save();
			invoiceLine.JI_CC = classification.PK;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoWarning("Tariff has warning", invoiceLine.JI_TariffInfo, JobComInvoiceLineValidation.MandatoryCCOrTariffForAutoCreateProduct);
			invoiceLine.JI_Tariff = "1020304090";
			var tariffMissingMessageError = ValidationConstants.InvoiceLine.Schedule1Part1TariffDoesNotExistsForDate("1020304090", new ZDateTime(2015, 7, 1));
			AssertHasMessageError(invoiceLine.JI_TariffInfo, tariffMissingMessageError);
			invoiceLine.JI_Tariff = "1020304080";
			tariffMissingMessageError = ValidationConstants.InvoiceLine.Schedule1Part1TariffDoesNotExistsForDate("1020304080", new ZDateTime(2015, 7, 1));
			AssertHasMessageError(invoiceLine.JI_TariffInfo, tariffMissingMessageError);
			invoiceLine.JI_Tariff = "1020304050";
			AssertNoMessageError(invoiceLine.JI_TariffInfo, tariffMissingMessageError);
			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			AssertNoMessageErrors("Base class won't do rate valiation if CountryOfOrigin is empty", invoiceLine.JI_TariffInfo);
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.NewZealand;
			invoiceLine.JI_PrimaryPreference = "400";
			string messageError = "There is no applicable Duty rate for the Tariff '1020304050' and Country Of Origin 'NZ' as at 01-Jul-15 00:00:00 in combination with other data entered on the form.";
			AssertHasMessageError(invoiceLine.JI_TariffInfo, messageError);
			AssertHasMessageError(invoiceLine.JI_TariffInfo, "Valid Duty rates exist where\r\n1: Preference = 100\r\n2: Preference = 200\r\n");
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			AssertEquals("DefaultPreference when setting JI_CountryOfOrigin", "", invoiceLine.JI_PrimaryPreference);
			AssertNoMessageError("Base class won't show message error if there is no applicable rates for the Tariff with current CountryOfOrigin and date and ratetype", invoiceLine.JI_TariffInfo, messageError);
			invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Standard;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.NewZealand;
			AssertEquals("DefaultPreference when setting JI_CountryOfOrigin", UniversalReferenceConstants.PrimaryPreference.PreferentialRate, invoiceLine.JI_PrimaryPreference);
			AssertNoMessageErrors("Have applicable rate for NZ and Preference=200", invoiceLine.JI_TariffInfo);
			invoiceLine.JI_PrimaryPreference = "400";
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageError("No rate validation message error for export declaration", invoiceLine.JI_TariffInfo, messageError);
		}

		[TestDate(2019, 04, 09)]
		public void TestCheckJI_TariffForREBPermit()
		{
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType3P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "3P1");
			var tariffType4P2 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "4P2");
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1234567890", new ZDateTime(2019, 1, 1), new ZDateTime(2019, 12, 1));
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1111111111", new ZDateTime(2019, 1, 1), new ZDateTime(2019, 12, 1));
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType3P1.PK, "1020304050", new ZDateTime(2019, 1, 1), new ZDateTime(2019, 12, 1));
			var tariff4 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType4P2.PK, "1020304051", new ZDateTime(2019, 1, 1), new ZDateTime(2019, 12, 1));
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var permit1 = Factory.NewWithValidTestData<CusPermitHeader>();
			permit1.CPH_OH_PermitHolder = org1.PK;
			permit1.CPH_Type = PermitTypeList.Codes.REB;
			permit1.CPH_StartDate = ZDate.Today;
			permit1.CPH_Number = "1020304051";
			var rule1 = permit1.CusPermitRules.AddNew();
			rule1.CPR_RuleCode = PermitRuleCodeList.Codes.TAR;
			rule1.CPR_ValueFrom = "1234567890";
			rule1.CPR_ValueTo = "1234567890";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org1.PK;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			var additionalTariff = invoiceLine.CusLineTariffDetails.AddNew();
			additionalTariff.BZ_Type = "3P1";
			additionalTariff.BZ_Tariff = tariff3.ZZ1_TariffCode;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff Code is not available for this Rebate Code in the REB Permit Rule.");
			var additionalTariff2 = invoiceLine.CusLineTariffDetails.AddNew();
			additionalTariff2.BZ_Type = "4P2";
			additionalTariff2.BZ_Tariff = tariff4.ZZ1_TariffCode;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff Code is not available for this Rebate Code in the REB Permit Rule.");
			invoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Tariff Code is not available for this Rebate Code in the REB Permit Rule.");
			permit1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee; // a trick to avoid long long mock
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining("It will not load guarantee hence no message error.", invoiceLine.JI_TariffInfo, "Tariff Code is not available for this Rebate Code in the REB Permit Rule.");
		}

		[TestDate(2015, 10, 10)]
		public void TestCheckJI_TariffWhenUOMsExceed()
		{
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType12A = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12A");
			var rateType_ZA_DTY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			var rateCode_ZA_DTY_D = helper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_DTY.PK);
			var preference = helper.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			var tariff0 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "2010101010", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12A.PK, "1020304050", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			helper.CreateTariffRelationship(tariff1.PK, tariff0.ZZ1_ZZI_TariffType, "201010");
			Factory.Save();
			var tariff1Rate1 = helper.CreateRate(tariff1, rateCode_ZA_DTY_D.PK, new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff1Rate2 = helper.CreateRate(tariff1, rateCode_ZA_DTY_D.PK, new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			helper.CreateTariffUOM(tariff0, "CU1", "KM");
			helper.CreateTariffUOM(tariff0, "CU2", "KG");
			helper.CreateTariffUOM(tariff0, "RU1", "MM");
			helper.CreateTariffUOM(tariff1, "CU1", "GJ");
			helper.CreateTariffUOM(tariff1, "CU2", "LI");
			helper.CreateTariffUOM(tariff1, "RU1", "CM");
			Factory.Save();
			var testTradeGroup1 = helper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "1TR", new ZDateTime(2010, 01, 01), new ZDateTime(2079, 06, 06));
			helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(2010, 01, 01), new ZDate(2079, 06, 06));
			helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.NewZealand, new ZDate(2010, 01, 01), new ZDate(2079, 06, 06));
			var applicability1 = helper.CreateCusApplicability(tariff1Rate1, testTradeGroup1, new ZDateTime(2011, 01, 01), new ZDateTime(2070, 06, 06));
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "1#";
			entryInstruction.CEI_DateForDuty = new ZDateTime(2015, 5, 30);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.NewZealand;
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "2$";
			invoiceLine.JI_Tariff = tariff0.ZZ1_TariffCode;
			AssertEquals("Already 2 Additional UOMs", 2, invoiceLine.DistinctAdditionalUOMsFromAllValidTariffs.Count());
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
			tariffDetail.BZ_Tariff = tariff1.ZZ1_TariffCode;
			AssertEquals("should have 3 UOMs", 3, tariffDetail.UniversalTariff.UnitsOfMeasure.Count);
			AssertEquals("BZ_UQ1 = GJ", "GJ", tariffDetail.BZ_UQ1);
			AssertEquals("Now 5 Additional UOMs", 5, invoiceLine.DistinctAdditionalUOMsFromAllValidTariffs.Count());
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageError(invoiceLine.JI_TariffInfo, ValidationConstants.CusLineTariffDetail.UOMsExceed);
		}

		public void TestCheckJI_Tariff_DutyFreeTariffUsedForIntoWarehouseEntry()
		{
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType12A = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12A");
			var rateType_ZA_DTY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			var rateCode_ZA_DTY_D = helper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_DTY.PK);
			var preference100 = helper.CreatePreferenceForCountryAndGrouping(UniversalReferenceConstants.PrimaryPreference.Standard, "STANDARD", Core.Constants.CountryCodes.SouthAfrica, Core.Constants.CountryCodes.SouthAfrica);
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1000010000", ZDateTime.Today, ZDateTime.Today.AddDays(1));
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12A.PK, "2000020000", ZDateTime.Today, ZDateTime.Today.AddDays(1));
			Factory.Save();
			var tariff1Rate = helper.CreateRate(tariff1, rateCode_ZA_DTY_D.PK, ZDateTime.Today, ZDateTime.Today.AddDays(1), "0", preference100.PK);
			var tariff2Rate = helper.CreateRate(tariff2, rateCode_ZA_DTY_D.PK, ZDateTime.Today, ZDateTime.Today.AddDays(1), "1", preference100.PK);
			var procedure11 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "11", "11", "11", "IMP", group: "IFD");
			var procedure41 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "41", "41", "41", "41", "IMP", group: "IFD");
			procedure41.ZZ6_IntoWarehouse = YesNoList.Codes.Yes;
			Factory.Save();
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "TradeGroup", new ZDateTime(1980, 01, 01), new ZDateTime(2079, 06, 06));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			helper.CreateCusApplicability(tariff1Rate, tradeGroup, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			helper.CreateCusApplicability(tariff2Rate, tradeGroup, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction11 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction11.CEI_Style = "11";
			var entryInstruction41 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction41.CEI_Style = "41";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction11.PK;
			invoiceLine.JI_PrimaryPreference = "200";
			invoiceLine.JI_Tariff = "2000020000";
			invoiceLine.JI_CountryOfOrigin = "ZA";
			using (ZACustomsRegistry.Instance.DutyFreeGoodsIntoBondedWarehouse.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				invoiceLine.RunPreSaveValidation();
				AssertNoWarning(invoiceLine.JI_TariffInfo, ValidationConstants.CusLineTariffDetail.DutyFreeTariffUsedForIntoWarehouseEntry);
				invoiceLine.JI_PrimaryPreference = "100";
				invoiceLine.RunPreSaveValidation();
				AssertNoWarning(invoiceLine.JI_TariffInfo, ValidationConstants.CusLineTariffDetail.DutyFreeTariffUsedForIntoWarehouseEntry);
				invoiceLine.JI_CEI = entryInstruction41.PK;
				invoiceLine.RunPreSaveValidation();
				AssertNoWarning(invoiceLine.JI_TariffInfo, ValidationConstants.CusLineTariffDetail.DutyFreeTariffUsedForIntoWarehouseEntry);
				invoiceLine.JI_Tariff = "1000010000";
				invoiceLine.RunPreSaveValidation();
				AssertHasWarning(invoiceLine.JI_TariffInfo, ValidationConstants.CusLineTariffDetail.DutyFreeTariffUsedForIntoWarehouseEntry);
			}

			using (ZACustomsRegistry.Instance.DutyFreeGoodsIntoBondedWarehouse.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				invoiceLine.RunPreSaveValidation();
				AssertHasError(invoiceLine.JI_TariffInfo, ValidationConstants.CusLineTariffDetail.DutyFreeTariffUsedForIntoWarehouseEntry);
			}
		}

		public void TestCheckJI_Tariff_DutyFreeTariffUsedForIntoWarehouseEntry_ShouldCheckStandardRateOnly()
		{
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var rateType_ZA_DTY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			var rateCode_ZA_DTY_D = helper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_DTY.PK);
			var preference100 = helper.CreatePreferenceForCountry(UniversalReferenceConstants.PrimaryPreference.Standard, "STANDARD", Core.Constants.CountryCodes.SouthAfrica);
			var preference200 = helper.CreatePreferenceForCountry(UniversalReferenceConstants.PrimaryPreference.PreferentialRate, "PreferentialRate", Core.Constants.CountryCodes.SouthAfrica);
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1000010000", ZDateTime.Today, ZDateTime.Today.AddDays(1));
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "2000020000", ZDateTime.Today, ZDateTime.Today.AddDays(1));
			Factory.Save();
			var tariff1Rate = helper.CreateRate(tariff1, rateCode_ZA_DTY_D.PK, ZDateTime.Today, ZDateTime.Today.AddDays(1), "0", preference100.PK);
			var tariff2Rate = helper.CreateRate(tariff2, rateCode_ZA_DTY_D.PK, ZDateTime.Today, ZDateTime.Today.AddDays(1), "0", preference200.PK);
			var tariff3Rate = helper.CreateRate(tariff2, rateCode_ZA_DTY_D.PK, ZDateTime.Today, ZDateTime.Today.AddDays(1), "1", preference100.PK);
			var procedure11 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "11", "11", "11", "IMP", group: "IFD");
			procedure11.ZZ6_IntoWarehouse = YesNoList.Codes.Yes;
			Factory.Save();
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "TradeGroup", new ZDateTime(1980, 01, 01), new ZDateTime(2079, 06, 06));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			helper.CreateCusApplicability(tariff1Rate, tradeGroup, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			helper.CreateCusApplicability(tariff2Rate, tradeGroup, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			helper.CreateCusApplicability(tariff3Rate, tradeGroup, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction11 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction11.CEI_Style = "11";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction11.PK;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			using (ZACustomsRegistry.Instance.DutyFreeGoodsIntoBondedWarehouse.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				invoiceLine.JI_Tariff = "1000010000";
				invoiceLine.JI_PrimaryPreference = "100";
				invoiceLine.RunPreSaveValidation();
				AssertHasWarning(invoiceLine.JI_TariffInfo, ValidationConstants.CusLineTariffDetail.DutyFreeTariffUsedForIntoWarehouseEntry);
				invoiceLine.JI_Tariff = "2000020000";
				invoiceLine.JI_PrimaryPreference = "100";
				invoiceLine.RunPreSaveValidation();
				AssertNoWarning("It should have no warning because the standard rate 100 (tariff3Rate) is not free.", invoiceLine.JI_TariffInfo, ValidationConstants.CusLineTariffDetail.DutyFreeTariffUsedForIntoWarehouseEntry);
			}
		}

		public void TestDiamondProcessingWarning()
		{
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			Factory.Save();
			//1P1 - Diamond Processing Required
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "991001", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			var tariffAttribute = helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.Diamond, "X", tariff1);
			//1P1 - Diamond Processing Not Required
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "991002", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			Factory.Save();
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invLine1 = dec.Invoices.AddNew().InvoiceLines.AddNew();
			AssertNoWarningContaining(invLine1.JI_TariffInfo, "This tariff indicates that Diamond details may be required. Please consider entering the requisite information on the Diamond Processing Tab.");
			invLine1.JI_Tariff = "991001";
			AssertHasWarningContaining(invLine1.JI_TariffInfo, "This tariff indicates that Diamond details may be required. Please consider entering the requisite information on the Diamond Processing Tab.");
			invLine1.JI_Tariff = "991002";
			AssertNoWarningContaining(invLine1.JI_TariffInfo, "This tariff indicates that Diamond details may be required. Please consider entering the requisite information on the Diamond Processing Tab.");
		}

		public void TestCountryOfOriginNotEmptyAndValidCode()
		{
			invoiceLine.JI_CountryOfOrigin = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_CountryOfOriginInfo, "You have not entered a Goods Origin.");
			invoiceLine.JI_CountryOfOrigin = "ZA";
			AssertNoMessageErrorContaining(invoiceLine.JI_CountryOfOriginInfo, "You have not entered a Goods Origin.");
		}

		public void TestValidateCustomsQtyAndUnit()
		{
			invoiceLine.JI_CustomsSecondUnitQty = "KG";
			invoiceLine.JI_CustomsThirdUnitQty = "KG";
			invoiceLine.JI_BondedWhsUnitQty = "KG";
			invoiceLine.RunPreSaveValidation();
			AssertEquals("Customs Qty is needed", true, invoiceLine.JI_CustomsSecondUnitQtyInfo.HasMessageErrors());
			AssertEquals("Customs Qty is needed", true, invoiceLine.JI_CustomsThirdUnitQtyInfo.HasMessageErrors());
			AssertEquals("Customs Qty is needed", false, invoiceLine.JI_BondedWhsQuantityInfo.HasMessageErrors());
			instruction.CEI_Style = ProcedureCodes._40;
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.Validation.ValidateJI_BondedWhsQuantity();
			AssertEquals("Customs Qty is needed", true, invoiceLine.JI_BondedWhsQuantityInfo.HasMessageErrors());
		}

		public void TestValidateDuplicateUnitCode()
		{
			invoiceLine.JI_CustomsSecondUnitQty = "KG";
			invoiceLine.JI_CustomsThirdUnitQty = "KG";
			invoiceLine.JI_BondedWhsUnitQty = "KG";
			invoiceLine.RunPreSaveValidation();
			AssertEquals("Customs Unit2 is a duplicate", true, invoiceLine.JI_CustomsSecondUnitQtyInfo.HasMessageErrors());
			AssertEquals("Customs unit3 is a duplicate", true, invoiceLine.JI_CustomsThirdUnitQtyInfo.HasMessageErrors());
			AssertEquals("Customs unit4 is a duplicate", false, invoiceLine.JI_BondedWhsUnitQtyInfo.HasMessageErrors());
			invoiceLine.JI_CustomsThirdUnitQty = "NO";
			invoiceLine.RunPreSaveValidation();
			AssertEquals("Customs Unit2 is a duplicate", false, invoiceLine.JI_CustomsSecondUnitQtyInfo.HasMessageErrors());
			AssertEquals("Customs unit3 is not a duplicate", false, invoiceLine.JI_CustomsThirdUnitQtyInfo.HasMessageErrors());
			AssertEquals("Customs unit4 is a duplicate", false, invoiceLine.JI_BondedWhsUnitQtyInfo.HasMessageErrors());
		}

		public void TestCheckJI_CustomsSecondQuantity()
		{
			invoiceLine.JI_CustomsSecondUnitQty = "KG";
			invoiceLine.RunPreSaveValidation();
			AssertEquals("Customs Qty is needed", true, invoiceLine.JI_CustomsSecondQuantityInfo.HasMessageErrors());
			invoiceLine.JI_CustomsSecondUnitQty = "";
			invoiceLine.JI_CustomsSecondQuantity = 5;
			AssertEquals("Unit of quantity required", true, invoiceLine.JI_CustomsSecondQuantityInfo.HasMessageErrors());
		}

		public void TestMessageErrorIfLinePriceIsZero()
		{
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 1;
			AssertNoMessageErrors(line.JI_LinePriceInfo);
			line.JI_LinePrice = 0;
			AssertHasMessageErrorContaining(line.JI_LinePriceInfo, "You cannot send a message without any export/import value.\r\nPlease specify Price here or provide Customs Value Override if you want to achieve Actual Price Zero");
			line.JI_CustomsValueOverride = 10m;
			AssertNoMessageErrors(line.JI_LinePriceInfo);
		}

		public void TestValidationType()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals(typeof(JobComInvoiceLineValidation), invoiceLine.Validation.GetType());
		}

		public void TestCheckJI_PartNo()
		{
			var helper = new ZAWhsDataTestHelper(Factory);
			helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;
			helper.Part2.RelatedOrganisations.RemoveAndDeleteAll();
			helper.Part2.RelatedOrganisations.AddSupplier(helper.Supplier);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = helper.Owner.PK;
			declaration.JE_OH_Supplier = helper.Supplier.PK;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var changeOfOwnershipCusProcedure = helper.ChangeOfOwnershipCusProcedure;
			entryInstruction.CEI_Style = changeOfOwnershipCusProcedure.ZZ6_ProcedureCode;
			entryInstruction.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
			entryInstruction.CEI_OA_Warehouse2 = entryInstruction.CEI_OA_Warehouse;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = helper.Supplier.PK;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Procedure = changeOfOwnershipCusProcedure.ZZ6_ProcedureCode + changeOfOwnershipCusProcedure.ZZ6_PreviousProcedureCode;
			invoiceLine.JI_PartNo = helper.Part2.OP_PartNum;
			var productMustBelongToImporterForChangeOfOwnership = ValidationConstants.InvoiceLine.ProductMustBelongToOrganisationForChangeOfOwnership(helper.Owner.OH_Code);
			AssertHasMessageError(invoiceLine.JI_PartNoInfo, productMustBelongToImporterForChangeOfOwnership);
			entry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertNoMessageError(invoiceLine.JI_PartNoInfo, productMustBelongToImporterForChangeOfOwnership);
			entry.CH_WarehouseTransactionStatus = ZString.Empty;
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertHasMessageError(invoiceLine.JI_PartNoInfo, productMustBelongToImporterForChangeOfOwnership);
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "$#";
			AssertNoMessageError(invoiceLine.JI_PartNoInfo, productMustBelongToImporterForChangeOfOwnership);
			invoiceLine.JI_Procedure = changeOfOwnershipCusProcedure.ZZ6_ProcedureCode + changeOfOwnershipCusProcedure.ZZ6_PreviousProcedureCode;
			AssertHasMessageError(invoiceLine.JI_PartNoInfo, productMustBelongToImporterForChangeOfOwnership);
			helper.Part2.RelatedOrganisations.AddOwner(helper.Owner);
			invoiceLine.Validation.ValidateJI_PartNo();
			AssertNoMessageError(invoiceLine.JI_PartNoInfo, productMustBelongToImporterForChangeOfOwnership);
		}

		public void TestCheckJI_ZZF_NKTaxType()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			var tariffType1P1 = testHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var testVINTariff = testHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "01010101", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			testHelper.CreateTaxOrFee("VAT", 0.14, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "VAT Normal");
			Factory.Save();
			invoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.JI_ZZF_NKTaxType = "";
			AssertNoNotifications(invoiceLine.JI_ZZF_NKTaxTypeInfo);
			invoiceLine.JI_ZZF_NKTaxType = "XXX";
			AssertHasMessageErrorContaining(invoiceLine.JI_ZZF_NKTaxTypeInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_ZZF_NKTaxType = "VAT";
			AssertNoNotifications(invoiceLine.JI_ZZF_NKTaxTypeInfo);
			invoiceLine.JI_Tariff = "01010101";
			invoiceLine.JI_ZZF_NKTaxType = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_ZZF_NKTaxTypeInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_ZZF_NKTaxType = "XXX";
			AssertHasMessageErrorContaining(invoiceLine.JI_ZZF_NKTaxTypeInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_ZZF_NKTaxType = "VAT";
			AssertNoNotifications(invoiceLine.JI_ZZF_NKTaxTypeInfo);
			invoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.JI_ZZF_NKTaxType = "";
			AssertNoMessageErrorContaining(invoiceLine.JI_ZZF_NKTaxTypeInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_Tariff = "01010101";
			invoiceLine.JI_ZZF_NKTaxType = "";
			AssertNoMessageErrorContaining(invoiceLine.JI_ZZF_NKTaxTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_ZZF_NKTaxTypeWhenVATNotEmpty()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "111", "One", "IMP", group: "IFD");
			procedure1.ZZ6_CalculateVAT = false;
			Factory.Save();
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = dec.Invoices.AddNew();
			var line = header.InvoiceLines.AddNew();
			line.JI_Procedure = "1111111";
			line.JI_ZZF_NKTaxType = UniversalReferenceConstants.TaxOrFeeTypeCode.VAT;
			AssertNoWarnings(line.JI_ZZF_NKTaxTypeInfo);
			procedure1.ZZ6_CalculateVAT = true;
			line.Validation.ValidateJI_ZZF_NKTaxType();
			AssertNoWarnings(line.JI_ZZF_NKTaxTypeInfo);
		}

		public void TestCheckJI_PreviousProcedure()
		{
			declaration.JE_MergeBy = Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			instruction.CEI_Style = ProcedureCodes._10;
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + ProcedureCodes._20;
			instruction.CEI_PreviousMRN = "BBR201603221234567";
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoErrors(invoiceLine.JI_ProcedureInfo);
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + ProcedureCodes._40;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError(invoiceLine.JI_ProcedureInfo, ValidationConstants.InvoiceLine.FromWarehouseRequiredForPPC);
			instruction.CEI_OA_Warehouse = warehouse.PK;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoErrors(invoiceLine.JI_ProcedureInfo);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			instruction.CEI_Style = ProcedureCodes._10;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + ProcedureCodes._48;
			instruction.CEI_OA_Warehouse = ZGuid.Empty;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError(invoiceLine.JI_ProcedureInfo, ValidationConstants.InvoiceLine.FromWarehouseRequiredForPPC);
			instruction.CEI_OA_Warehouse = warehouse.PK;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoErrors(invoiceLine.JI_ProcedureInfo);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + ProcedureCodes._40;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Procedure = invoiceLine2.EntryInstruction.CEI_Style + ProcedureCodes._20;
			invoiceLine2.Validation.ValidateJI_Procedure();
			AssertHasMessageErrorContaining(invoiceLine2.JI_ProcedureInfo, ValidationConstants.InvoiceLine.MixOfPreviousProceduresOnEntryInstruction);
			invoiceLine2.JI_Procedure = invoiceLine2.EntryInstruction.CEI_Style + ProcedureCodes._41;
			invoiceLine2.Validation.ValidateJI_Procedure();
			AssertNoMessageError("PPC are different but mixable", invoiceLine2.JI_ProcedureInfo, ValidationConstants.InvoiceLine.MixOfPreviousProceduresOnEntryInstruction);
			invoiceLine2.JI_Procedure = invoiceLine2.EntryInstruction.CEI_Style + ProcedureCodes._40;
			invoiceLine2.Validation.ValidateJI_Procedure();
			AssertNoMessageError("PPC are the same", invoiceLine2.JI_ProcedureInfo, ValidationConstants.InvoiceLine.MixOfPreviousProceduresOnEntryInstruction);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
			instruction.CEI_Style = ProcedureCodes._48;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + ProcedureCodes._42;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageError("Remover not required for CCP 48 with PPC 42", invoiceLine.JI_ProcedureInfo, ValidationConstants.InvoiceLine.RemoverRequiredForProcedureCode);
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + ProcedureCodes._48;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError("Remover required for CCP 48 with PPC 48", invoiceLine.JI_ProcedureInfo, ValidationConstants.InvoiceLine.RemoverRequiredForProcedureCode);
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + ProcedureCodes._49;
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError("Remover required for CCP 48 with PPC 49", invoiceLine.JI_ProcedureInfo, ValidationConstants.InvoiceLine.RemoverRequiredForProcedureCode);
			var removalTransportModes = new ZString[] { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Rail, Core.Constants.TransportModes.Mail, Core.Constants.TransportModes.FixedTransportInstallations, Core.Constants.TransportModes.Other, "" };
			foreach (var removalTransportMode in removalTransportModes)
			{
				declaration.JE_RemovalTransportCode = removalTransportMode;
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageError("Remover not required for " + removalTransportMode, invoiceLine.JI_ProcedureInfo, ValidationConstants.InvoiceLine.RemoverRequiredForProcedureCode);
			}

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			var instruction1 = declaration1.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = ProcedureCodes._41;
			var instruction2 = declaration1.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction2.CEI_Style = ProcedureCodes._47;
			var instruction3 = declaration1.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction2.CEI_Style = ProcedureCodes._48;
			var invoice1 = declaration1.Invoices.AddNew();
			var invoiceline1 = invoice1.InvoiceLines.AddNew();
			invoiceline1.JI_CEI = instruction1.PK;
			invoiceline1.JI_Procedure = invoiceline1.EntryInstruction.CEI_Style + ProcedureCodes._40;
			var invoiceline2 = invoice1.InvoiceLines.AddNew();
			invoiceline2.JI_CEI = instruction3.PK;
			invoiceline2.JI_Procedure = invoiceline2.EntryInstruction.CEI_Style + ProcedureCodes._44;
			var invoiceline3 = invoice1.InvoiceLines.AddNew();
			invoiceline3.JI_CEI = instruction2.PK;
			invoiceline3.JI_Procedure = invoiceline3.EntryInstruction.CEI_Style + ProcedureCodes._00;
			var invoiceline4 = invoice1.InvoiceLines.AddNew();
			invoiceline4.JI_CEI = instruction2.PK;
			invoiceline4.JI_Procedure = invoiceline4.EntryInstruction.CEI_Style + ProcedureCodes._46;
			invoiceline1.Validation.ValidateJI_Procedure();
			invoiceline2.Validation.ValidateJI_Procedure();
			invoiceline3.Validation.ValidateJI_Procedure();
			invoiceline4.Validation.ValidateJI_Procedure();
			AssertHasMessageError("PPC are the same", invoiceline4.JI_ProcedureInfo, ValidationConstants.InvoiceLine.MixOfPreviousProceduresOnEntryInstruction);
			AssertHasMessageError("PPC are different", invoiceline3.JI_ProcedureInfo, ValidationConstants.InvoiceLine.MixOfPreviousProceduresOnEntryInstruction);
			AssertNoMessageError("PPC are differnt but mixable", invoiceline2.JI_ProcedureInfo, ValidationConstants.InvoiceLine.MixOfPreviousProceduresOnEntryInstruction);
			AssertNoMessageError("PPC are differnt but mixable", invoiceline1.JI_ProcedureInfo, ValidationConstants.InvoiceLine.MixOfPreviousProceduresOnEntryInstruction);
		}

		public void TestCheckJI_PreviousProcedure_IncompatibleMergeByTypeAndPreviousProcedureCode()
		{
			AssertUnsupportedMergeByMessageError(assertion: false, mergeBy: "NON", previousProcedureCode: "00");
			AssertUnsupportedMergeByMessageError(assertion: false, mergeBy: "NOP", previousProcedureCode: "00");
			AssertUnsupportedMergeByMessageError(assertion: false, mergeBy: "TRF", previousProcedureCode: "00");
			AssertUnsupportedMergeByMessageError(assertion: false, mergeBy: "TRD", previousProcedureCode: "00");
			AssertUnsupportedMergeByMessageError(assertion: false, mergeBy: "CLS", previousProcedureCode: "00");
			AssertUnsupportedMergeByMessageError(assertion: false, mergeBy: "CLD", previousProcedureCode: "00");
			AssertUnsupportedMergeByMessageError(assertion: false, mergeBy: "PNO", previousProcedureCode: "00");
			AssertUnsupportedMergeByMessageError(assertion: false, mergeBy: "PNP", previousProcedureCode: "00");

			AssertUnsupportedMergeByMessageError(assertion: false, mergeBy: "NON", previousProcedureCode: "11");
			AssertUnsupportedMergeByMessageError(assertion: false, mergeBy: "NOP", previousProcedureCode: "11");
			AssertUnsupportedMergeByMessageError(assertion: true, mergeBy: "TRF", previousProcedureCode: "11");
			AssertUnsupportedMergeByMessageError(assertion: true, mergeBy: "TRD", previousProcedureCode: "11");
			AssertUnsupportedMergeByMessageError(assertion: true, mergeBy: "CLS", previousProcedureCode: "11");
			AssertUnsupportedMergeByMessageError(assertion: true, mergeBy: "CLD", previousProcedureCode: "11");
			AssertUnsupportedMergeByMessageError(assertion: true, mergeBy: "PNO", previousProcedureCode: "11");
			AssertUnsupportedMergeByMessageError(assertion: true, mergeBy: "PNP", previousProcedureCode: "11");
		}

		void AssertUnsupportedMergeByMessageError(bool assertion, string mergeBy, string previousProcedureCode)
		{
			declaration.JE_MergeBy = mergeBy;
			invoiceLine.JI_Procedure = "40" + previousProcedureCode;
			AssertEquals("Pre-req", previousProcedureCode, invoiceLine.JI_Calc_PreviousProcedure);
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertEquals(assertion, invoiceLine.JI_ProcedureInfo.HasMessageError(ValidationConstants.InvoiceLine.UnsupportedMergeByForNonZeroPreviousProcedureCode));
		}

		public void TestCheckJI_PreviousProcedure_Warehousing()
		{
			var cusProcedure1 = Factory.New<RefCusProcedure>();
			cusProcedure1.ZZ6_ProcedureCode = "AB";
			cusProcedure1.ZZ6_PreviousProcedureCode = "12";
			cusProcedure1.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			cusProcedure1.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			cusProcedure1.ZZ6_Description = "AB DESC";
			var cusProcedure2 = Factory.New<RefCusProcedure>();
			cusProcedure2.ZZ6_ProcedureCode = "AB";
			cusProcedure2.ZZ6_PreviousProcedureCode = "34";
			cusProcedure2.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			cusProcedure2.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			cusProcedure2.ZZ6_Description = "AB DESC";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MergeBy = Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "AB";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_Procedure = invoiceLine1.EntryInstruction.CEI_Style + "34";
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_Procedure = invoiceLine2.EntryInstruction.CEI_Style + "12";
			AssertHasError(invoiceLine2.JI_ProcedureInfo, ValidationConstants.InvoiceLine.PreviousProcedureCodeIsNotAnIntoWarehouse);
			invoiceLine2.JI_Procedure = "";
			AssertHasError(invoiceLine2.JI_ProcedureInfo, ValidationConstants.InvoiceLine.PreviousProcedureCodeIsNotAnIntoWarehouse);
			invoiceLine2.JI_Procedure = invoiceLine2.EntryInstruction.CEI_Style + "34";
			AssertNoError(invoiceLine2.JI_ProcedureInfo, ValidationConstants.InvoiceLine.PreviousProcedureCodeIsNotAnIntoWarehouse);
			invoiceLine1.JI_Procedure = invoiceLine1.EntryInstruction.CEI_Style + "12";
			invoiceLine2.JI_Procedure = "";
			AssertHasError(invoiceLine2.JI_ProcedureInfo, ValidationConstants.InvoiceLine.PreviousProcedureCodeIsNotAnOutOfWarehouse);
			invoiceLine2.JI_Procedure = invoiceLine2.EntryInstruction.CEI_Style + "34";
			AssertNoErrors(invoiceLine2.JI_ProcedureInfo);
			invoiceLine2.JI_Procedure = invoiceLine2.EntryInstruction.CEI_Style + "12";
			AssertNoErrors(invoiceLine2.JI_ProcedureInfo);
		}

		public void TestCheckJI_CustomsUnitQty()
		{
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariff0 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "2010101010", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			Factory.Save();
			helper.CreateTariffUOM(tariff0, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "KN");
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "1#";
			entryInstruction.CEI_DateForDuty = new ZDateTime(2015, 5, 30);
			var invLine1 = dec.Invoices.AddNew().InvoiceLines.AddNew();
			invLine1.JI_CustomsUnitQty = "XX";
			AssertHasMessageErrorContaining(invLine1.JI_CustomsUnitQtyInfo, JobComInvoiceLineValidation.UoMNotInknownUnitsOfMeasureList);
			invLine1.JI_CustomsUnitQty = "GJ";
			AssertNoNotifications(invLine1.JI_CustomsUnitQtyInfo);
			invLine1.JI_CustomsUnitQty = "YY";
			AssertHasMessageErrorContaining(invLine1.JI_CustomsUnitQtyInfo, JobComInvoiceLineValidation.UoMNotInknownUnitsOfMeasureList);
			invLine1.JI_CustomsUnitQty = "NO";
			AssertNoNotifications(invLine1.JI_CustomsUnitQtyInfo);
			invLine1.JI_CustomsUnitQty = "";
			AssertNoNotifications(invLine1.JI_CustomsUnitQtyInfo);
			invLine1.JI_CustomsUnitQty = "LI";
			invLine1.JI_CustomsSecondUnitQty = "LI";
			AssertHasMessageErrorContaining(invLine1.JI_CustomsSecondUnitQtyInfo, "You have entered a duplicate unit code, LI");
			invLine1.JI_CustomsThirdUnitQty = "KG";
			invLine1.JI_CustomsSecondUnitQty = "KG";
			AssertHasMessageErrorContaining(invLine1.JI_CustomsSecondUnitQtyInfo, "You have entered a duplicate unit code, KG");
			invLine1.JI_BondedWhsUnitQty = "KN";
			invLine1.JI_CustomsSecondUnitQty = "KN";
			AssertNoNotifications(invLine1.JI_CustomsUnitQtyInfo);

			invLine1.JI_Tariff = tariff0.ZZ1_TariffCode;
			invLine1.JI_CustomsUnitQty = "";
			AssertNoNotifications(invLine1.JI_CustomsUnitQtyInfo);
			invLine1.JI_CustomsUnitQty = "LI";
			AssertHasMessageErrorContaining(invLine1.JI_CustomsUnitQtyInfo, JobComInvoiceLineValidation.UoMIsNotRequired);
			helper.CreateTariffUOM(tariff0, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "KG");
			Factory.Save();
			invLine1.JI_CustomsUnitQty = "";
			AssertHasMessageErrorContaining(invLine1.JI_CustomsUnitQtyInfo, JobComInvoiceLineValidation.UoMIsRequired);
			invLine1.JI_CustomsUnitQty = "GJ";
			AssertHasMessageErrorContaining(invLine1.JI_CustomsUnitQtyInfo, JobComInvoiceLineValidation.UoMDoesNotMatchUoMRequired);
		}

		public void TestCheckJI_CustomsSecondUnitQty()
		{
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariff0 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "2010101010", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			Factory.Save();
			helper.CreateTariffUOM(tariff0, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "KG");
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "1#";
			entryInstruction.CEI_DateForDuty = new ZDateTime(2015, 5, 30);
			var invLine1 = dec.Invoices.AddNew().InvoiceLines.AddNew();
			invLine1.JI_CustomsSecondUnitQty = "XX";
			AssertHasMessageErrorContaining(invLine1.JI_CustomsSecondUnitQtyInfo, JobComInvoiceLineValidation.UoMNotInknownUnitsOfMeasureList);
			invLine1.JI_CustomsSecondUnitQty = "GJ";
			AssertNoNotifications(invLine1.JI_CustomsSecondUnitQtyInfo);
			invLine1.JI_CustomsSecondUnitQty = "YY";
			AssertHasMessageErrorContaining(invLine1.JI_CustomsSecondUnitQtyInfo, JobComInvoiceLineValidation.UoMNotInknownUnitsOfMeasureList);
			invLine1.JI_CustomsSecondUnitQty = "NO";
			AssertNoNotifications(invLine1.JI_CustomsSecondUnitQtyInfo);
			invLine1.JI_CustomsSecondUnitQty = "";
			AssertNoNotifications(invLine1.JI_CustomsSecondUnitQtyInfo);
			invLine1.JI_CustomsUnitQty = "LI";
			invLine1.JI_CustomsSecondUnitQty = "LI";
			AssertHasMessageErrorContaining(invLine1.JI_CustomsSecondUnitQtyInfo, "You have entered a duplicate unit code, LI");
			invLine1.JI_CustomsThirdUnitQty = "KG";
			invLine1.JI_CustomsSecondUnitQty = "KG";
			AssertHasMessageErrorContaining(invLine1.JI_CustomsSecondUnitQtyInfo, "You have entered a duplicate unit code, KG");
			invLine1.JI_BondedWhsUnitQty = "KN";
			invLine1.JI_CustomsSecondUnitQty = "KN";
			AssertNoNotifications(invLine1.JI_CustomsSecondUnitQtyInfo);

			invLine1.JI_Tariff = tariff0.ZZ1_TariffCode;
			invLine1.JI_CustomsSecondUnitQty = "LI";
			AssertHasMessageErrorContaining(invLine1.JI_CustomsSecondUnitQtyInfo, JobComInvoiceLineValidation.UoMIsNotRequired);
			invLine1.JI_CustomsSecondUnitQty = "";
			AssertNoNotifications(invLine1.JI_CustomsSecondUnitQtyInfo);
			helper.CreateTariffUOM(tariff0, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "KN");
			Factory.Save();
			invLine1.RunPreSaveValidation();
			AssertHasMessageErrorContaining(invLine1.JI_CustomsSecondUnitQtyInfo, JobComInvoiceLineValidation.UoMIsRequired);
			invLine1.JI_CustomsSecondUnitQty = "GJ";
			AssertHasMessageErrorContaining(invLine1.JI_CustomsSecondUnitQtyInfo, JobComInvoiceLineValidation.UoMDoesNotMatchUoMRequired);
		}

		public void TestCheckJI_CustomsThirdUnitQty()
		{
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariff0 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "2010101010", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			Factory.Save();
			helper.CreateTariffUOM(tariff0, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "KG");
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "1#";
			entryInstruction.CEI_DateForDuty = new ZDateTime(2015, 5, 30);
			var invLine1 = dec.Invoices.AddNew().InvoiceLines.AddNew();
			invLine1.JI_CustomsThirdUnitQty = "XX";
			AssertHasMessageErrorContaining(invLine1.JI_CustomsThirdUnitQtyInfo, JobComInvoiceLineValidation.UoMNotInknownUnitsOfMeasureList);
			invLine1.JI_CustomsThirdUnitQty = "GJ";
			AssertNoNotifications(invLine1.JI_CustomsThirdUnitQtyInfo);
			invLine1.JI_CustomsThirdUnitQty = "YY";
			AssertHasMessageErrorContaining(invLine1.JI_CustomsThirdUnitQtyInfo, JobComInvoiceLineValidation.UoMNotInknownUnitsOfMeasureList);
			invLine1.JI_CustomsThirdUnitQty = "NO";
			AssertNoNotifications(invLine1.JI_CustomsThirdUnitQtyInfo);
			invLine1.JI_CustomsThirdUnitQty = "";
			AssertNoNotifications(invLine1.JI_CustomsThirdUnitQtyInfo);
			invLine1.JI_CustomsUnitQty = "LI";
			invLine1.JI_CustomsThirdUnitQty = "LI";
			AssertHasMessageErrorContaining(invLine1.JI_CustomsThirdUnitQtyInfo, "You have entered a duplicate unit code, LI");
			invLine1.JI_CustomsSecondUnitQty = "KG";
			invLine1.JI_CustomsThirdUnitQty = "KG";
			AssertHasMessageErrorContaining(invLine1.JI_CustomsThirdUnitQtyInfo, "You have entered a duplicate unit code, KG");
			invLine1.JI_BondedWhsUnitQty = "KN";
			invLine1.JI_CustomsThirdUnitQty = "KN";
			AssertNoNotifications(invLine1.JI_CustomsThirdUnitQtyInfo);

			invLine1.JI_Tariff = tariff0.ZZ1_TariffCode;
			invLine1.JI_CustomsThirdUnitQty = "LI";
			AssertHasMessageErrorContaining(invLine1.JI_CustomsThirdUnitQtyInfo, JobComInvoiceLineValidation.UoMIsNotRequired);
			invLine1.JI_CustomsThirdUnitQty = "";
			AssertNoNotifications(invLine1.JI_CustomsThirdUnitQtyInfo);
			helper.CreateTariffUOM(tariff0, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "KN");
			helper.CreateTariffUOM(tariff0, UniversalReferenceConstants.RefCusTariffUOMTypes.ClassificationUOMType, "MM");
			Factory.Save();
			invLine1.RunPreSaveValidation();
			AssertHasMessageErrorContaining(invLine1.JI_CustomsThirdUnitQtyInfo, JobComInvoiceLineValidation.UoMIsRequired);
			invLine1.JI_CustomsThirdUnitQty = "GJ";
			AssertHasMessageErrorContaining(invLine1.JI_CustomsThirdUnitQtyInfo, JobComInvoiceLineValidation.UoMDoesNotMatchUoMRequired);
		}

		public void TestCheckJI_CustomsQuantity()
		{
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			Factory.Save();
			var testVINTariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "00867543", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			helper.CreateTariffUOM(testVINTariff, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "NO");
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.VIN, UniversalReferenceConstants.TariffAttributes.Values.Optional, testVINTariff);
			var testNONVINTariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "00867544", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			helper.CreateTariffUOM(testNONVINTariff, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "NO");
			Factory.Save();
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			CombineAssertions(() =>
			{
				invLine.JI_Tariff = testNONVINTariff.ZZ1_TariffCode;
				invLine.JI_CustomsQuantity = 0m;
				AssertHasMessageErrorContaining(invLine.JI_CustomsQuantityInfo, "Customs Qty should be greater than zero. Please enter a Customs Qty directly or enter an invoice UQ that can be convertible to Customs UQ 'NO'.");
				AssertNoMessageErrorContaining(invLine.JI_CustomsQuantityInfo, "Customs Qty can only be 1 for vehicles, only 1 vehicle allowed per line.");
				invLine.JI_CustomsQuantity = 1m;
				AssertNoMessageErrors("2", invLine.JI_CustomsQuantityInfo);
				invLine.JI_CustomsQuantity = 2m;
				AssertNoMessageErrors("3", invLine.JI_CustomsQuantityInfo);
				invLine.JI_Tariff = testVINTariff.ZZ1_TariffCode;
				invLine.JI_CustomsQuantity = 0m;
				AssertHasMessageErrorContaining(invLine.JI_CustomsQuantityInfo, "Customs Qty should be greater than zero. Please enter a Customs Qty directly or enter an invoice UQ that can be convertible to Customs UQ 'NO'.");
				AssertHasMessageErrorContaining(invLine.JI_CustomsQuantityInfo, "Customs Qty can only be 1 for vehicles, only 1 vehicle allowed per line.");
				invLine.JI_CustomsQuantity = 1m;
				AssertNoMessageErrors("5", invLine.JI_CustomsQuantityInfo);
				invLine.JI_CustomsQuantity = 2m;
				AssertHasMessageErrorContaining(invLine.JI_CustomsQuantityInfo, "Customs Qty can only be 1 for vehicles, only 1 vehicle allowed per line.");
				invLine.JI_CustomsQuantity = 1.1m;
				AssertHasMessageErrorContaining(invLine.JI_CustomsQuantityInfo, "Customs Qty can only be 1 for vehicles, only 1 vehicle allowed per line.");
				invLine.JI_CustomsQuantity = 0m;
				invLine.JI_Tariff = testVINTariff.ZZ1_TariffCode;
				AssertHasMessageErrorContaining(invLine.JI_CustomsQuantityInfo, "Customs Qty should be greater than zero. Please enter a Customs Qty directly or enter an invoice UQ that can be convertible to Customs UQ 'NO'.");
				AssertHasMessageErrorContaining(invLine.JI_CustomsQuantityInfo, "Customs Qty can only be 1 for vehicles, only 1 vehicle allowed per line.");
				invLine.JI_Tariff = testNONVINTariff.ZZ1_TariffCode;
				AssertHasMessageErrorContaining(invLine.JI_CustomsQuantityInfo, "Customs Qty should be greater than zero. Please enter a Customs Qty directly or enter an invoice UQ that can be convertible to Customs UQ 'NO'.");
				AssertNoMessageErrorContaining(invLine.JI_CustomsQuantityInfo, "Customs Qty can only be 1 for vehicles, only 1 vehicle allowed per line.");
			});
		}

		public void TestValidateJI_ContainerMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CusContainers.AddNew();
			var header = declaration.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
			line.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = false;
			line.JI_ContainerMode = Enterprise.Core.Constants.ContainerModes.Containerised;
			declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			line.Validation.ValidateJI_ContainerMode();
			Assert("Has Message Error", line.JI_ContainerModeInfo.HasMessageError("Invoice Line is in containerized mode but is not linked to a container, a container can be associated to all invoice lines from the context menu on the Containers grid on the container sub tab"));
			declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.RemoveAll();
			line.Validation.ValidateJI_ContainerMode();
			Assert("Has Warning", line.JI_ContainerModeInfo.HasWarning("Invoice Line is in containerized mode but is not linked to a container, a container can be associated to all invoice lines from the context menu on the Containers grid on the container sub tab"));
		}

		public void TestCheckJI_PartAttrib()
		{
			for (int i = 1; i <= 3; i++)
			{
				var owner = Factory.New<OrgHeader>();
				owner.OH_Code = @"OH{i}";
				owner.MiscServ[$"OM_IMPartAttrib{i}Type"] = PartAttributeTypeList.Codes.VIN;
				owner.MiscServ[$"OM_IMPartAttrib{i}Name"] = "VIN number attribute";
				var part = Factory.New<OrgSupplierPart>();
				part.OP_PartNum = "PARTNUM";
				var ownRelation = part.RelatedOrganisations.AddNew();
				ownRelation.OU_OH = owner.PK;
				ownRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_OH_Importer = ownRelation.OU_OH;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_PartNo = "PARTNUM";
				var partAttribName = $"JI_PartAttrib{i}";
				invoiceLine.JI_VIN = "VIN1234";
				invoiceLine[partAttribName] = "VIN3456";
				ownRelation[$"OU_UsePartAttrib{i}"] = true;
				invoiceLine.Validation.ValidateJI_PartAttrib1();
				invoiceLine.Validation.ValidateJI_PartAttrib2();
				invoiceLine.Validation.ValidateJI_PartAttrib3();
				AssertHasMessageError(invoiceLine.FindPropertyInfo(partAttribName), JobComInvoiceLineValidation.VINAndVINAttributeMustBeTheSame);
				invoiceLine.JI_VIN = "VIN3456";
				AssertNoMessageError(invoiceLine.FindPropertyInfo(partAttribName), JobComInvoiceLineValidation.VINAndVINAttributeMustBeTheSame);
			}
		}

		public void TestJI_CustomsQuantityIsAnInteger()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.FillWithValidTestData();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine5 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.FillWithValidTestData();
			invoiceLine2.FillWithValidTestData();
			invoiceLine3.FillWithValidTestData();
			invoiceLine4.FillWithValidTestData();
			invoiceLine5.FillWithValidTestData();
			invoiceLine.JI_CustomsUnitQty = QuantityCodeList.Codes.Number;
			invoiceLine.JI_CustomsQuantity = 1.00;
			invoiceLine2.JI_CustomsUnitQty = QuantityCodeList.Codes.Number;
			invoiceLine2.JI_CustomsQuantity = 1.10;
			invoiceLine3.JI_CustomsUnitQty = QuantityCodeList.Codes.Pairs;
			invoiceLine3.JI_CustomsQuantity = 1.00;
			invoiceLine4.JI_CustomsUnitQty = QuantityCodeList.Codes.Pairs;
			invoiceLine4.JI_CustomsQuantity = 1.10;
			invoiceLine5.JI_CustomsUnitQty = QuantityCodeList.Codes.Volume_L;
			invoiceLine5.JI_CustomsQuantity = 1.10;
			string messageError = "Only Integer values allowed for quantity";
			CombineAssertions(() =>
			{
				AssertNoMessageError(invoiceLine.JI_CustomsQuantityInfo, messageError);
				AssertHasMessageError(invoiceLine2.JI_CustomsQuantityInfo, messageError);
				AssertNoMessageError(invoiceLine3.JI_CustomsQuantityInfo, messageError);
				AssertHasMessageError(invoiceLine4.JI_CustomsQuantityInfo, messageError);
				AssertNoMessageError(invoiceLine5.JI_CustomsQuantityInfo, messageError);
			});
		}

		public void TestJI_CustomsSecondQuantityIsAnInteger()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.FillWithValidTestData();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine5 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine6 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.FillWithValidTestData();
			invoiceLine2.FillWithValidTestData();
			invoiceLine3.FillWithValidTestData();
			invoiceLine4.FillWithValidTestData();
			invoiceLine5.FillWithValidTestData();
			invoiceLine.JI_CustomsSecondUnitQty = QuantityCodeList.Codes.Number;
			invoiceLine.JI_CustomsSecondQuantity = 1.00;
			invoiceLine2.JI_CustomsSecondUnitQty = QuantityCodeList.Codes.Number;
			invoiceLine2.JI_CustomsSecondQuantity = 1.10;
			invoiceLine3.JI_CustomsSecondUnitQty = QuantityCodeList.Codes.Pairs;
			invoiceLine3.JI_CustomsSecondQuantity = 1.00;
			invoiceLine4.JI_CustomsSecondUnitQty = QuantityCodeList.Codes.Pairs;
			invoiceLine4.JI_CustomsSecondQuantity = 1.10;
			invoiceLine5.JI_CustomsSecondUnitQty = QuantityCodeList.Codes.Volume_L;
			invoiceLine5.JI_CustomsSecondQuantity = 1.10;
			string messageError = "Only Integer values allowed for quantity";
			CombineAssertions(() =>
			{
				AssertNoMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, messageError);
				AssertHasMessageError(invoiceLine2.JI_CustomsSecondQuantityInfo, messageError);
				AssertNoMessageError(invoiceLine3.JI_CustomsSecondQuantityInfo, messageError);
				AssertHasMessageError(invoiceLine4.JI_CustomsSecondQuantityInfo, messageError);
				AssertNoMessageError(invoiceLine5.JI_CustomsSecondQuantityInfo, messageError);
			});
		}

		public void TestJI_CustomsThirdQuantityIsAnInteger()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.FillWithValidTestData();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine5 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.FillWithValidTestData();
			invoiceLine2.FillWithValidTestData();
			invoiceLine3.FillWithValidTestData();
			invoiceLine4.FillWithValidTestData();
			invoiceLine5.FillWithValidTestData();
			invoiceLine.JI_CustomsThirdUnitQty = QuantityCodeList.Codes.Number;
			invoiceLine.JI_CustomsThirdQuantity = 1.00;
			invoiceLine2.JI_CustomsThirdUnitQty = QuantityCodeList.Codes.Number;
			invoiceLine2.JI_CustomsThirdQuantity = 1.10;
			invoiceLine3.JI_CustomsThirdUnitQty = QuantityCodeList.Codes.Pairs;
			invoiceLine3.JI_CustomsThirdQuantity = 1.00;
			invoiceLine4.JI_CustomsThirdUnitQty = QuantityCodeList.Codes.Pairs;
			invoiceLine4.JI_CustomsThirdQuantity = 1.10;
			invoiceLine5.JI_CustomsThirdUnitQty = QuantityCodeList.Codes.Volume_L;
			invoiceLine5.JI_CustomsThirdQuantity = 1.10;
			string messageError = "Only Integer values allowed for quantity";
			CombineAssertions(() =>
			{
				AssertNoMessageError(invoiceLine.JI_CustomsThirdQuantityInfo, messageError);
				AssertHasMessageError(invoiceLine2.JI_CustomsThirdQuantityInfo, messageError);
				AssertNoMessageError(invoiceLine3.JI_CustomsThirdQuantityInfo, messageError);
				AssertHasMessageError(invoiceLine4.JI_CustomsThirdQuantityInfo, messageError);
				AssertNoMessageError(invoiceLine5.JI_CustomsThirdQuantityInfo, messageError);
			});
		}

		public void TestJI_Description_BlankLines()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.FillWithValidTestData();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			const string expectedMessage = "Too many Line Feed and Carriage Return characters in this text field are known to cause EDI Gateway errors";
			CombineAssertions(() =>
			{
				invoiceLine.JI_Description = "\nStarting with a blank line\nshould result\nin a message error";
				AssertHasMessageError("With Blank Line", invoiceLine.JI_DescriptionInfo, expectedMessage);
				invoiceLine.JI_Description = "\rStarting with a carriage return";
				AssertHasMessageError("With CR", invoiceLine.JI_DescriptionInfo, expectedMessage);
				invoiceLine.JI_Description = "\r\nStarting with a carriage return and line feed";
				AssertHasMessageError("With CRLF", invoiceLine.JI_DescriptionInfo, expectedMessage);
				invoiceLine.JI_Description = "Starting without a blank line\nshould not result\nin a message error";
				AssertNoMessageError("Without Blank Line", invoiceLine.JI_DescriptionInfo, expectedMessage);
			});
		}

		public void TestValidateMaxNumberOfLines()
		{
			var expectedError = "Ex-bond Max Number of Job Invoice Lines has been Exceeded, Maximum allowed is 2";
			using (ZACustomsRegistry.Instance.ExbondMaxNumberJobInvoiceLines.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 2))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
				var invoice1 = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
				var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
				var invoiceLine3 = invoice1.JobComInvoiceLines.AddNew();

				AssertHasRowError(invoiceLine3, expectedError);
				AssertNoRowError(invoiceLine1, expectedError);
				AssertNoRowError(invoiceLine2, expectedError);

				declaration.InvoiceLines.Remove(invoiceLine3);
				AssertNoRowError(invoiceLine1, expectedError);
				AssertNoRowError(invoiceLine2, expectedError);

				var invoice2 = declaration.Invoices.AddNew();
				invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
				AssertHasRowError(invoiceLine3, expectedError);
				AssertNoRowError(invoiceLine1, expectedError);
				AssertNoRowError(invoiceLine2, expectedError);

				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				declaration.RunPreSaveValidation();
				AssertNoRowError(invoiceLine1, expectedError);
				AssertNoRowError(invoiceLine2, expectedError);
				AssertNoRowError(invoiceLine3, expectedError);
			}

			using (ZACustomsRegistry.Instance.ExbondMaxNumberJobInvoiceLines.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 2))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				var invoice1 = declaration.Invoices.AddNew();
				invoice1.JobComInvoiceLines.AddNew();
				invoice1.JobComInvoiceLines.AddNew();
				invoice1.JobComInvoiceLines.AddNew();
				AssertNoRowError(invoice1.JobComInvoiceLines[0], expectedError);
				AssertNoRowError(invoice1.JobComInvoiceLines[1], expectedError);
				AssertNoRowError(invoice1.JobComInvoiceLines[2], expectedError);
			}
		}

		public void TestCheckJI_VehicleFormat()
		{
			invoiceLine.JI_VehicleFormat = "X";
			AssertHasMessageErrorContaining(invoiceLine.JI_VehicleFormatInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_VehicleFormat = VehicleFormatList.Codes.CKD;
			AssertNoMessageErrorContaining(invoiceLine.JI_VehicleFormatInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJI_VehicleType()
		{
			invoiceLine.JI_VehicleType = "X";
			AssertHasMessageErrorContaining(invoiceLine.JI_VehicleTypeInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_VehicleType = VehicleTypeList.Codes.Hauler;
			AssertNoMessageErrorContaining(invoiceLine.JI_VehicleTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJI_YearOfManufacture()
		{
			var nextYear = ZDate.Today.Year + 1;
			invoiceLine.JI_YearOfManufacture = nextYear.ToString();
			AssertHasMessageErrorContaining(invoiceLine.JI_YearOfManufactureInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_YearOfManufacture = ZDateTime.Today.Year.ToString();
			AssertNoMessageErrorContaining(invoiceLine.JI_YearOfManufactureInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJI_NewUsed()
		{
			invoiceLine.JI_NewUsed = "";
			AssertHasMessageErrorContaining(invoiceLine.JI_NewUsedInfo, "You have not entered a");
			invoiceLine.JI_NewUsed = "X";
			AssertHasMessageErrorContaining(invoiceLine.JI_NewUsedInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_NewUsed = GoodsTypeList.Codes.U;
			AssertNoMessageErrorContaining(invoiceLine.JI_NewUsedInfo, ListValidation.InvalidCodeMessageError);
		}

		[TestDate(2016, 01, 01)]
		public void TestCheckVINAndEngineNumber()
		{
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			Factory.Save();
			CombineAssertions(() =>
			{
				string engineNumberRequiredIfVINCaptured = JobComInvoiceLineValidation.EngineNumberAndVINAreRequiredInPair;
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				invoiceLine.JI_VIN = "VINNUMBER123";
				invoiceLine.JI_EngineNumber = "";
				AssertHasMessageErrorContaining(invoiceLine.JI_EngineNumberInfo, engineNumberRequiredIfVINCaptured);
				AssertNoWarnings(invoiceLine.JI_EngineNumberInfo);
				invoiceLine.JI_EngineNumber = "ENGINE NO 123";
				invoiceLine.JI_VIN = "";
				AssertHasMessageErrorContaining(invoiceLine.JI_VINInfo, engineNumberRequiredIfVINCaptured);
				AssertNoWarnings(invoiceLine.JI_EngineNumberInfo);
				invoiceLine.JI_VIN = "";
				invoiceLine.JI_EngineNumber = "";
				AssertNoMessageErrorContaining(invoiceLine.JI_EngineNumberInfo, engineNumberRequiredIfVINCaptured);
				AssertNoWarnings(invoiceLine.JI_EngineNumberInfo);
				var testVINTariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "001122", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
				helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.VIN, UniversalReferenceConstants.TariffAttributes.Values.Optional, testVINTariff);
				Factory.Save();
				invoiceLine.JI_Tariff = "001122";
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertNoMessageErrors(invoiceLine.JI_VINInfo);
				AssertHasWarningContaining(invoiceLine.JI_VINInfo, "VIN Number may be required for the selected tariff:001122");
				invoiceLine.JI_VIN = "VINNUMBER123";
				invoiceLine.JI_EngineNumber = "ENGINE NO 123";
				AssertNoMessageErrors(invoiceLine.JI_EngineNumberInfo);
				AssertNoWarnings(invoiceLine.JI_EngineNumberInfo);
				AssertNoMessageErrors(invoiceLine.JI_VINInfo);
				AssertNoWarnings(invoiceLine.JI_VINInfo);
				invoiceLine.JI_EngineNumber = "ENGINE NO 123";
				invoiceLine.JI_VIN = "VINNUMBER123";
				AssertNoMessageErrors(invoiceLine.JI_VINInfo);
				AssertNoMessageErrors(invoiceLine.JI_EngineNumberInfo);
				AssertNoWarnings(invoiceLine.JI_EngineNumberInfo);
				AssertNoWarnings(invoiceLine.JI_VINInfo);
			});
		}

		public void TestCheckJI_VIN()
		{
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			Factory.Save();
			var testVINTariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "00876543", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.VIN, UniversalReferenceConstants.TariffAttributes.Values.Optional, testVINTariff);
			var testNONVINTariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "00876544", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			Factory.Save();
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invLine1 = dec.Invoices.AddNew().InvoiceLines.AddNew();
			var invLine2 = dec.Invoices.AddNew().InvoiceLines.AddNew();
			invLine1.JI_Tariff = testNONVINTariff.ZZ1_TariffCode;
			invLine1.JI_VIN = "VINNO1";
			AssertNoMessageErrors(invLine1.JI_VINInfo);
			AssertHasWarningContaining(invLine1.JI_VINInfo, "VIN Number may not be required for the selected tariff:00876544");
			invLine2.JI_Tariff = testVINTariff.ZZ1_TariffCode;
			invLine2.JI_VIN = "VINNO1";
			AssertHasMessageErrorContaining(invLine2.JI_VINInfo, "VIN Number should be unique, but \"VINNO1\" has been used on another Invoice Line, please double check the the correctness of this VIN Number.");
			AssertNoWarnings(invLine2.JI_VINInfo);
			invLine1.Validation.ValidateJI_VIN();
			AssertHasMessageErrorContaining(invLine1.JI_VINInfo, "VIN Number should be unique, but \"VINNO1\" has been used on another Invoice Line, please double check the the correctness of this VIN Number.");
			AssertHasWarningContaining(invLine1.JI_VINInfo, "VIN Number may not be required for the selected tariff:00876544");
			invLine2.JI_Tariff = testVINTariff.ZZ1_TariffCode;
			invLine2.JI_VIN = "VINNO2";
			AssertNoMessageErrors(invLine2.JI_VINInfo);
			AssertNoWarnings(invLine2.JI_VINInfo);
			invLine2.JI_VIN = "VINNO1";
			invLine2.JI_Tariff = testNONVINTariff.ZZ1_TariffCode;
			AssertHasMessageErrorContaining(invLine2.JI_VINInfo, "VIN Number should be unique, but \"VINNO1\" has been used on another Invoice Line, please double check the the correctness of this VIN Number.");
			AssertHasWarningContaining(invLine2.JI_VINInfo, "VIN Number may not be required for the selected tariff:00876544");
			invLine2.JI_VIN = "VINNO2";
			invLine2.JI_Tariff = testNONVINTariff.ZZ1_TariffCode;
			AssertNoMessageErrors(invLine2.JI_VINInfo);
			AssertHasWarningContaining(invLine2.JI_VINInfo, "VIN Number may not be required for the selected tariff:00876544");
			invLine1.Validation.ValidateJI_VIN();
			AssertNoMessageErrors(invLine1.JI_VINInfo);
			AssertHasWarningContaining(invLine1.JI_VINInfo, "VIN Number may not be required for the selected tariff:00876544");
		}

		public void TestCheckJI_ROOCert()
		{
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var preference = helper.CreatePreferenceForCountryAndGrouping("200", "EUTRADE", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			var rateType_ZA_DTY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			var rateCode_ZA_DTY_D = helper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_DTY.PK);
			Factory.Save();
			var startDate = ZDateTime.BrettsBirthday;
			var endDate = ZDateTime.Today.AddDays(1);
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1010102030", startDate, endDate);
			var tariff1P1Rate = helper.CreateRate(tariff, rateCode_ZA_DTY_D.PK, startDate, endDate, "0.1 * VFD", preference.PK);
			Factory.Save();
			var testTradeGroup1 = helper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "EUTRADE", new ZDateTime(1980, 01, 01), new ZDateTime(2079, 06, 06));
			helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.Germany, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			helper.CreateCusApplicability(tariff1P1Rate, testTradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			Factory.Save();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Germany;
			invoiceLine.JI_PrimaryPreference = "EUR";
			invoiceLine.JI_ROOCert = ZString.Empty;
			AssertHasMessageError(invoiceLine.JI_ROOCertInfo, ValidationConstants.InvoiceLine.NoROOCertEnteredForType);
			invoiceLine.JI_ROOCert = "111";
			AssertNoMessageErrorContaining(invoiceLine.JI_ROOCertInfo, ValidationConstants.InvoiceLine.NoROOCertEnteredForType);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			invoiceLine.JI_PrimaryPreference = PrimaryPreference.PreferentialRate;
			invoiceLine.JI_ROOCert = ZString.Empty;
			AssertHasWarning(invoiceLine.JI_ROOCertInfo, ValidationConstants.InvoiceLine.NoROOCertEnteredForTradeAgreement(TradeAgreement.EUTRADE));
			invoiceLine.JI_ROOCert = "111";
			AssertNoWarning(invoiceLine.JI_ROOCertInfo, ValidationConstants.InvoiceLine.NoROOCertEnteredForTradeAgreement(TradeAgreement.EUTRADE));
			invoiceLine.JI_PrimaryPreference = PrimaryPreference.Standard;
			invoiceLine.JI_ROOCert = ZString.Empty;
			AssertNoNotifications(invoiceLine.JI_ROOCertInfo);
		}

		public void TestCheckJI_TargetEntryLineNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = "NON";
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_Style = "11";
			testInst.CEI_DateForDuty = new ZDateTime(2019, 1, 1);
			var testInvoice = declaration.Invoices.AddNew();
			testInvoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var testInvoiceLine1 = testInvoice.InvoiceLines.AddNew();
			testInvoiceLine1.JI_CEI = testInst.PK;
			testInvoiceLine1.JI_TargetEntryLineNumber = 2;
			var testInvoiceLine2 = testInvoice.InvoiceLines.AddNew();
			testInvoiceLine2.JI_CEI = testInst.PK;
			testInvoiceLine2.JI_TargetEntryLineNumber = 2;
			CombineAssertions(() =>
			{
				declaration.DoMerge();
				AssertNotEquals(testInvoiceLine1.JI_CL, testInvoiceLine2.JI_CL);
				AssertEquals("Target on Line 1", new ZShort(2), testInvoiceLine1.JI_TargetEntryLineNumber);
				AssertEquals("Target on Line 2", new ZShort(2), testInvoiceLine2.JI_TargetEntryLineNumber);
				AssertEquals("Result on Line 1", new ZShort(2), testInvoiceLine1.CusEntryLine.CL_LineNumber);
				AssertEquals("Result on Line 2", new ZShort(3), testInvoiceLine2.CusEntryLine.CL_LineNumber);
				AssertNoMessageErrors(testInvoiceLine1.JI_TargetEntryLineNumberInfo);
				AssertHasMessageErrorContaining(testInvoiceLine2.JI_TargetEntryLineNumberInfo, "The linked entry line has a Line Number \"3\" while the Target Entry Line Number is \"2\".\r\nPlease make sure you put the right Target Entry Line Number.");
				testInvoiceLine2.JI_TargetEntryLineNumber = 0;
				AssertNoMessageErrors(testInvoiceLine1.JI_TargetEntryLineNumberInfo);
				testInvoiceLine2.JI_TargetEntryLineNumber = 4;
				AssertNoMessageErrors(testInvoiceLine1.JI_TargetEntryLineNumberInfo);
			});
		}

		public void TestCheckJI_TargetEntryLineNumberExceedsMaximum()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testInst1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst1.CEI_Style = "11";
			var testInst2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst2.CEI_Style = "21";
			testInst2.CEI_DateForDuty = new ZDateTime(2019, 1, 1);
			var testInvoice = declaration.Invoices.AddNew();
			var testInvoiceLine = testInvoice.InvoiceLines.AddNew();
			testInvoiceLine.JI_CEI = testInst1.PK;
			testInvoiceLine.JI_TargetEntryLineNumber = 10000;
			AssertNoMessageErrorContaining(testInvoiceLine.JI_TargetEntryLineNumberInfo, ValidationConstants.Shared.EntryLineNumberExceedMax);
			testInvoiceLine.JI_CEI = testInst2.PK;
			testInvoiceLine.JI_TargetEntryLineNumber = 9999;
			AssertNoMessageErrorContaining(testInvoiceLine.JI_TargetEntryLineNumberInfo, ValidationConstants.Shared.EntryLineNumberExceedMax);
			testInvoiceLine.JI_TargetEntryLineNumber = 10000;
			AssertHasMessageErrorContaining(testInvoiceLine.JI_TargetEntryLineNumberInfo, ValidationConstants.Shared.EntryLineNumberExceedMax);
		}

		public void TestCheckJI_TargetEntryLineNumberCanBeSpecifiedBeforeMerge()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testInst1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst1.CEI_Style = "11";
			var testInst2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst2.CEI_Style = "21";
			testInst2.CEI_DateForDuty = new ZDateTime(2019, 1, 1);
			var testInvoice = declaration.Invoices.AddNew();
			var testInvoiceLine = testInvoice.InvoiceLines.AddNew();

			testInvoiceLine.JI_CEI = testInst1.PK;
			testInvoiceLine.JI_TargetEntryLineNumber = 0;
			AssertNoMessageErrors(testInvoiceLine.JI_TargetEntryLineNumberInfo);
			testInvoiceLine.JI_TargetEntryLineNumber = 123;
			AssertNoMessageErrors(testInvoiceLine.JI_TargetEntryLineNumberInfo);

			testInvoiceLine.JI_CEI = testInst2.PK;
			testInvoiceLine.JI_TargetEntryLineNumber = 0;
			AssertNoMessageErrors(testInvoiceLine.JI_TargetEntryLineNumberInfo);
			testInvoiceLine.JI_TargetEntryLineNumber = 123;
			AssertNoMessageErrors(testInvoiceLine.JI_TargetEntryLineNumberInfo);
		}

		public void TestCheckJI_NewOwnerPartAttrib1()
		{
			var invoiceLine = SetupInvoiceLineWithOwnerPart();
			CheckPartAttributeValidation(invoiceLine.JI_NewOwnerPartAttrib1Info, invoiceLine.Validation.ValidateJI_NewOwnerPartAttrib1, invoiceLine.EntryInstruction.Owner, invoiceLine.NewOwnerProduct, 1);
		}

		public void TestCheckJI_NewOwnerPartAttrib2()
		{
			var invoiceLine = SetupInvoiceLineWithOwnerPart();
			CheckPartAttributeValidation(invoiceLine.JI_NewOwnerPartAttrib2Info, invoiceLine.Validation.ValidateJI_NewOwnerPartAttrib2, invoiceLine.EntryInstruction.Owner, invoiceLine.NewOwnerProduct, 2);
		}

		public void TestCheckJI_NewOwnerPartAttrib3()
		{
			var invoiceLine = SetupInvoiceLineWithOwnerPart();
			CheckPartAttributeValidation(invoiceLine.JI_NewOwnerPartAttrib3Info, invoiceLine.Validation.ValidateJI_NewOwnerPartAttrib3, invoiceLine.EntryInstruction.Owner, invoiceLine.NewOwnerProduct, 3);
		}

		public void TestCheckJI_NewOwnerPartNo()
		{
			var helper = new ZAWhsDataTestHelper(Factory);
			helper.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = false;
			helper.Owner.CompanyData.OB_IMUsedBondedWhs = true;
			helper.Part.RelatedOrganisations.AddSupplier(helper.Supplier);
			helper.Part2.RelatedOrganisations.AddSupplier(helper.Supplier);
			helper.OwnerPart.RelatedOrganisations.AddSupplier(helper.Supplier);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = helper.Importer.PK;
			declaration.JE_OH_Supplier = helper.Supplier.PK;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var changeOfOwnershipCusProcedure = helper.ChangeOfOwnershipCusProcedure;
			entryInstruction.CEI_Style = changeOfOwnershipCusProcedure.ZZ6_ProcedureCode;
			entryInstruction.CEI_OH_Owner = helper.Owner.PK;
			entryInstruction.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
			entryInstruction.CEI_OA_Warehouse2 = entryInstruction.CEI_OA_Warehouse;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = helper.Supplier.PK;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Procedure = changeOfOwnershipCusProcedure.ZZ6_ProcedureCode + changeOfOwnershipCusProcedure.ZZ6_PreviousProcedureCode;
			AssertHasMessageError(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.NewOwnerProductRequiresForChangeOfOwnership);
			helper.Owner.CompanyData.OB_IMUsedBondedWhs = false;
			invoiceLine.Validation.ValidateJI_NewOwnerPartNo();
			AssertNoMessageError(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.NewOwnerProductRequiresForChangeOfOwnership);
			helper.Importer.CompanyData.OB_IMUsedBondedWhs = false;
			helper.Owner.CompanyData.OB_IMUsedBondedWhs = true;
			invoiceLine.Validation.ValidateJI_NewOwnerPartNo();
			AssertHasMessageError(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.NewOwnerProductRequiresForChangeOfOwnership);
			helper.Owner.CompanyData.OB_IMUsedBondedWhs = false;
			helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;
			invoiceLine.Validation.ValidateJI_NewOwnerPartNo();
			AssertHasMessageError(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.NewOwnerProductRequiresForChangeOfOwnership);
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, JobComInvoiceLineValidation.WarningPartCodeNotFoundAtAll);
			invoiceLine.JI_NewOwnerPartNo = "SDS#@D";
			AssertHasMessageError(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.NewOwnerProductRequiresForChangeOfOwnership);
			AssertHasWarning(invoiceLine.JI_NewOwnerPartNoInfo, JobComInvoiceLineValidation.WarningPartCodeNotFoundAtAll);
			invoiceLine.JI_NewOwnerPartNo = helper.Part.OP_PartNum;
			AssertNoMessageError(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.NewOwnerProductRequiresForChangeOfOwnership);
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.WarningPartCodeFoundButNotRelatedToOwner);
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.WarningPartCodesFoundButNotRelatedToOwner);
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, JobComInvoiceLineValidation.WarningPartCodeNotFoundAtAll);
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, JobComInvoiceLineValidation.WarningMoreThanOneProductMatchFound);
			entryInstruction.CEI_OH_Owner = ZGuid.Empty;
			invoiceLine.Validation.ValidateJI_NewOwnerPartNo();
			AssertNoNotifications(invoiceLine.JI_NewOwnerPartNoInfo);
			AssertNoMessageError(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.NewOwnerProductRequiresForChangeOfOwnership);
			entryInstruction.CEI_OH_Owner = ZGuid.Invalid;
			invoiceLine.Validation.ValidateJI_NewOwnerPartNo();
			AssertNoNotifications(invoiceLine.JI_NewOwnerPartNoInfo);
			AssertNoMessageError(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.NewOwnerProductRequiresForChangeOfOwnership);
			var ownerPart2 = helper.CreateProduct(helper.Owner.PK, helper.OwnerPart.OP_PartNum);
			ownerPart2.RelatedOrganisations.AddSupplier(helper.Warehouse);
			entryInstruction.CEI_OH_Owner = helper.Owner.PK;
			invoiceLine.Validation.ValidateJI_NewOwnerPartNo();
			AssertNoMessageError(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.NewOwnerProductRequiresForChangeOfOwnership);
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.WarningPartCodeFoundButNotRelatedToOwner);
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.WarningPartCodesFoundButNotRelatedToOwner);
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, JobComInvoiceLineValidation.WarningPartCodeNotFoundAtAll);
			AssertHasWarning(invoiceLine.JI_NewOwnerPartNoInfo, JobComInvoiceLineValidation.WarningMoreThanOneProductMatchFound);
			invoiceLine.JI_NewOwnerPartNo = helper.Part2.OP_PartNum;
			AssertHasMessageError(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.NewOwnerProductRequiresForChangeOfOwnership);
			AssertHasWarning(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.WarningPartCodeFoundButNotRelatedToOwner);
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.WarningPartCodesFoundButNotRelatedToOwner);
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, JobComInvoiceLineValidation.WarningPartCodeNotFoundAtAll);
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, JobComInvoiceLineValidation.WarningMoreThanOneProductMatchFound);
			var part2 = helper.CreateProduct(helper.Importer.PK, helper.Part2.OP_PartNum);
			part2.RelatedOrganisations.AddSupplier(helper.Warehouse);
			invoiceLine.NewOwnerProductSyncManager.Refresh();
			invoiceLine.Validation.ValidateJI_NewOwnerPartNo();
			AssertHasMessageError(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.NewOwnerProductRequiresForChangeOfOwnership);
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.WarningPartCodeFoundButNotRelatedToOwner);
			AssertHasWarning(invoiceLine.JI_NewOwnerPartNoInfo, ValidationConstants.InvoiceLine.WarningPartCodesFoundButNotRelatedToOwner);
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, JobComInvoiceLineValidation.WarningPartCodeNotFoundAtAll);
			AssertNoWarning(invoiceLine.JI_NewOwnerPartNoInfo, JobComInvoiceLineValidation.WarningMoreThanOneProductMatchFound);
		}

		[TestDate(2015, 06, 01)]
		public void TestCheckJI_CustomsValueOverride()
		{
			var tariffType4P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "4P1");
			Factory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType4P1.PK, "1020304050", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			helper.CreateTariffRelationship(tariff1.PK, helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1").PK, "1");
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CostOfRepair, UniversalReferenceConstants.TariffAttributes.CostOfRepair, tariff1);
			Factory.Save();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1";
			invoiceLine.Validation.ValidateAll();
			AssertNoWarnings(invoiceLine.JI_CustomsValueOverrideInfo);
			var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail.BZ_Type = "4P1";
			tariffDetail.BZ_Tariff = "1020304050";
			invoiceLine.Validation.ValidateAll();
			AssertHasWarningContaining(invoiceLine.JI_CustomsValueOverrideInfo, "You may need to manually enter the Customs Value Override to present the Cost of Repair");
			tariffDetail.BZ_Tariff = "1111111";
			invoiceLine.Validation.ValidateAll();
			AssertNoWarnings(invoiceLine.JI_CustomsValueOverrideInfo);
		}

		public void TestCheckJI_RX_NKCustomsValueCurrencyOverride()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsValueOverride = 10;
			AssertNoMessageErrors(invoiceLine.JI_RX_NKCustomsValueCurrencyOverrideInfo);
			invoiceLine.JI_RX_NKCustomsValueCurrencyOverride = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_RX_NKCustomsValueCurrencyOverrideInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_CustomsValueOverride = 0;
			invoiceLine.JI_RX_NKCustomsValueCurrencyOverride = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.JI_RX_NKCustomsValueCurrencyOverrideInfo);
			invoiceLine.JI_RX_NKCustomsValueCurrencyOverride = "XXX";
			AssertHasMessageErrorContaining(invoiceLine.JI_RX_NKCustomsValueCurrencyOverrideInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_RX_NKCustomsValueCurrencyOverride = "USD";
			AssertNoNotifications(invoiceLine.JI_RX_NKCustomsValueCurrencyOverrideInfo);
			invoiceLine.JI_RX_NKCustomsValueCurrencyOverride = "AUD";
			AssertHasMessageErrorContaining(invoiceLine.JI_RX_NKCustomsValueCurrencyOverrideInfo, ListValidation.InvalidCodeMessageError);
			invoiceLine.JI_RX_NKCustomsValueCurrencyOverride = "ZAR";
			AssertNoNotifications(invoiceLine.JI_RX_NKCustomsValueCurrencyOverrideInfo);
		}

		[TestDate(2016, 09, 01)]
		public void TestCheckJI_PermitNumber()
		{
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1020304050", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var permit1 = Factory.NewWithValidTestData<CusPermitHeader>();
			permit1.CPH_OH_PermitHolder = org1.PK;
			permit1.CPH_Type = PermitTypeList.Codes.IMP;
			permit1.CPH_StartDate = ZDate.Today;
			permit1.CPH_Number = "PERMIT1";
			var rule1 = permit1.CusPermitRules.AddNew();
			rule1.CPR_RuleCode = PermitRuleCodeList.Codes.TAR;
			rule1.CPR_ValueFrom = "1020304050";
			rule1.CPR_ValueTo = "1020304050";
			var permit2 = Factory.NewWithValidTestData<CusPermitHeader>();
			permit2.CPH_OH_PermitHolder = org1.PK;
			permit2.CPH_Type = PermitTypeList.Codes.EXP;
			permit2.CPH_StartDate = ZDate.Today;
			permit2.CPH_Number = "PERMIT2";
			var rule2 = permit2.CusPermitRules.AddNew();
			rule2.CPR_RuleCode = PermitRuleCodeList.Codes.TAR;
			rule2.CPR_ValueFrom = "1020304050";
			rule2.CPR_ValueTo = "1020304050";
			var permit3 = Factory.NewWithValidTestData<CusPermitHeader>();
			permit3.CPH_Number = "PERMIT3";
			permit3.CPH_OH_PermitHolder = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertNotEquals("Pre-requisite: permit3.CPH_OH_PermitHolder must be different to org1.PK", org1.PK, permit3.CPH_OH_PermitHolder);
			var permit4 = Factory.NewWithValidTestData<CusPermitHeader>();
			permit4.CPH_OH_PermitHolder = org1.PK;
			permit4.CPH_Type = PermitTypeList.Codes.IMP;
			permit4.CPH_StartDate = ZDate.Today;
			permit4.CPH_Number = "PERMIT4";
			var rule4 = permit4.CusPermitRules.AddNew();
			rule4.CPR_RuleCode = PermitRuleCodeList.Codes.TAR;
			rule4.CPR_ValueFrom = "1020304049";
			rule4.CPR_ValueTo = "1020304049";
			var guarantee4 = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			guarantee4.CPH_OH_PermitHolder = org1.PK;
			guarantee4.CPH_Type = PermitTypeList.Codes.IMP;
			guarantee4.CPH_StartDate = ZDate.Today;
			guarantee4.CPH_Number = "GUARANTEE4";
			var guaranteeRule4 = guarantee4.CusGuaranteeRules.AddNew();
			guaranteeRule4.CPR_RuleCode = PermitRuleCodeList.Codes.TAR;
			guaranteeRule4.CPR_ValueFrom = "1020304050";
			guaranteeRule4.CPR_ValueTo = "1020304050";
			Factory.Save();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_OH_Importer = org1.PK;
			declaration.JE_OH_Supplier = org1.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_NewUsed = GoodsTypeList.Codes.N;
			invoiceLine.JI_Tariff = "1020304050";
			invoiceLine.RunPreSaveValidation();
			AssertNoNotifications(invoiceLine.JI_PermitNumberInfo);
			var testAttribute = Factory.New<TariffAttributeView>();
			testAttribute.ZZ3_ZZ1_ParentTariffOrNationalCode = tariff.PK;
			testAttribute.ZZ3_Name = UniversalReferenceConstants.TariffAttributes.ImportPermit;
			testAttribute.ZZ3_Value = UniversalReferenceConstants.TariffAttributes.Values.Mandatory;
			invoiceLine.RunPreSaveValidation();
			AssertNoMessageErrors(invoiceLine.JI_PermitNumberInfo);
			AssertHasWarningContaining(invoiceLine.JI_PermitNumberInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_PermitNumber = permit1.CPH_Number;
			AssertNoNotifications(invoiceLine.JI_PermitNumberInfo);
			permit1.CPH_UnitOfMeasure = "KG";
			invoiceLine.Validation.ValidateJI_PermitNumber();
			AssertHasMessageError(invoiceLine.JI_PermitNumberInfo, ValidationConstants.InvoiceLine.UnitOfMeasureDoesNotMatchPermit(invoiceLine.JI_PermitNumber, permit1.CPH_UnitOfMeasure));
			permit1.CPH_UnitOfMeasure = ZString.Empty;
			testAttribute.ZZ3_Value = UniversalReferenceConstants.TariffAttributes.Values.Optional;
			invoiceLine.JI_PermitNumber = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.JI_PermitNumberInfo);
			AssertHasWarning(invoiceLine.JI_PermitNumberInfo, ValidationConstants.InvoiceLine.PermitIsOptional);
			invoiceLine.JI_PermitNumber = permit1.CPH_Number;
			AssertNoNotifications(invoiceLine.JI_PermitNumberInfo);
			testAttribute.ZZ3_Name = UniversalReferenceConstants.TariffAttributes.ExportPermit;
			testAttribute.ZZ3_Value = UniversalReferenceConstants.TariffAttributes.Values.Mandatory;
			invoiceLine.JI_PermitNumber = ZString.Empty;
			AssertNoNotifications(invoiceLine.JI_PermitNumberInfo);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			invoiceLine.RunPreSaveValidation();
			AssertNoMessageErrors(invoiceLine.JI_PermitNumberInfo);
			AssertHasWarningContaining(invoiceLine.JI_PermitNumberInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_PermitNumber = permit2.CPH_Number;
			AssertNoNotifications(invoiceLine.JI_PermitNumberInfo);
			testAttribute.ZZ3_Value = UniversalReferenceConstants.TariffAttributes.Values.Optional;
			invoiceLine.JI_PermitNumber = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.JI_PermitNumberInfo);
			AssertNoWarningContaining(invoiceLine.JI_PermitNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarning(invoiceLine.JI_PermitNumberInfo, ValidationConstants.InvoiceLine.PermitIsOptional);
			invoiceLine.JI_PermitNumber = permit2.CPH_Number;
			AssertNoNotifications(invoiceLine.JI_PermitNumberInfo);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.JI_PermitNumber = ZString.Empty;
			AssertNoNotifications(invoiceLine.JI_PermitNumberInfo);
			invoiceLine.JI_NewUsed = GoodsTypeList.Codes.S;
			invoiceLine.RunPreSaveValidation();
			AssertNoMessageErrors(invoiceLine.JI_PermitNumberInfo);
			AssertNoWarningContaining(invoiceLine.JI_PermitNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarning(invoiceLine.JI_PermitNumberInfo, ValidationConstants.InvoiceLine.PermitForSecondHandGoods);
			invoiceLine.JI_PermitNumber = permit1.CPH_Number;
			AssertNoNotifications(invoiceLine.JI_PermitNumberInfo);
			invoiceLine.JI_NewUsed = GoodsTypeList.Codes.U;
			invoiceLine.JI_PermitNumber = ZString.Empty;
			AssertNoMessageErrors(invoiceLine.JI_PermitNumberInfo);
			AssertNoWarningContaining(invoiceLine.JI_PermitNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarning(invoiceLine.JI_PermitNumberInfo, ValidationConstants.InvoiceLine.PermitForSecondHandGoods);
			invoiceLine.JI_PermitNumber = permit1.CPH_Number;
			AssertNoNotifications(invoiceLine.JI_PermitNumberInfo);
			invoiceLine.JI_PermitNumber = permit2.CPH_Number;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			invoiceLine.JI_PermitNumber = ZString.Empty;
			AssertNoNotifications(invoiceLine.JI_PermitNumberInfo);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			invoiceLine.JI_PermitNumber = "A";
			AssertHasMessageError(invoiceLine.JI_PermitNumberInfo, ValidationConstants.InvoiceLine.PermitNotFound(invoiceLine.JI_PermitNumber));
			invoiceLine.JI_PermitNumber = permit3.CPH_Number;
			AssertHasMessageError(invoiceLine.JI_PermitNumberInfo, ValidationConstants.InvoiceLine.PermitHolderInvalid(invoiceLine.JI_PermitNumber, invoiceLine.Importer_Effective.OH_Code));
			invoiceLine.JI_PermitNumber = permit2.CPH_Number;
			AssertHasMessageError(invoiceLine.JI_PermitNumberInfo, ValidationConstants.InvoiceLine.PermitTypeInvalid(invoiceLine.JI_PermitNumber, PermitTypeList.Codes.IMP));
			invoiceLine.JI_Tariff = "1020304050";
			invoiceLine.JI_PermitNumber = permit4.CPH_Number;
			AssertHasMessageError(invoiceLine.JI_PermitNumberInfo, ValidationConstants.InvoiceLine.TariffNotInPermitRange(invoiceLine.JI_PermitNumber, invoiceLine.JI_Tariff));
			invoiceLine.JI_PermitNumber = guarantee4.CPH_Number;
			AssertHasMessageError("It will not load guarantee.", invoiceLine.JI_PermitNumberInfo, ValidationConstants.InvoiceLine.PermitNotFound(invoiceLine.JI_PermitNumber));
		}

		public void TestCheckJI_InvoiceUQ()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: true))
			{
				AssertEquals(expected: true, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today));

				_ = helper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, "INVUQ", "2I", "British thermal unit (international table) per hour", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
				Factory.Save();

				var lookups = new JobComInvoiceLineLookups(invoiceLine);
				var codeList = (ZZRefCusCodeListCombinedCollection)lookups.InvoiceUQUNE20CodeList;
				codeList.Load();
				AssertContainsExactElementsInAnyOrder("List", new[] { "2I" }, codeList.Select(x => x.ZZD_Code));

				invoiceLine.JI_InvoiceUQ = "2I";
				AssertNoMessageErrorContaining(invoiceLine.JI_InvoiceUQInfo, JobComInvoiceLineValidation.UoMNotFoundOnUNCodeList20);
				invoiceLine.JI_InvoiceUQ = "3I";
				AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceUQInfo, JobComInvoiceLineValidation.UoMNotFoundOnUNCodeList20);
				invoiceLine.Declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
				invoiceLine.Validation.ValidateJI_InvoiceUQ();
				AssertNoMessageErrorContaining(invoiceLine.JI_InvoiceUQInfo, JobComInvoiceLineValidation.UoMNotFoundOnUNCodeList20);
			}
		}

		public void TestCheckJI_InvoiceQuantity()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: true))
			{
				invoiceLine.JI_InvoiceQuantity = 0;
				AssertHasMessageErrorContaining(invoiceLine.JI_InvoiceQuantityInfo, JobComInvoiceLineValidation.InvoiceQuantityRequired);
				invoiceLine.JI_InvoiceQuantity = 1;
				AssertNoMessageErrorContaining(invoiceLine.JI_InvoiceQuantityInfo, JobComInvoiceLineValidation.InvoiceQuantityRequired);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: false))
			{
				invoiceLine.JI_InvoiceQuantity = 0;
				AssertNoMessageErrorContaining(invoiceLine.JI_InvoiceQuantityInfo, JobComInvoiceLineValidation.InvoiceQuantityRequired);
			}
		}

		[TestDate(2018, 1, 25)]
		public void TestCheckDuplicateJI_VIN()
		{
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			Factory.Save();
			var testVINTariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "001122", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.VIN, UniversalReferenceConstants.TariffAttributes.Values.Optional, testVINTariff);
			Factory.Save();
			CreateInvoiceLineWithVINNumber("IMP", "B000001", "INV001", 1, "001122", "VIN1234", ZDateTime.Now);
			CreateInvoiceLineWithVINNumber("IMP", "B000002", "INV002", 1, "001122", "VIN1234", ZDateTime.Now);
			CreateInvoiceLineWithVINNumber("EXP", "B000003", "INV003", 1, "001122", "VIN3456", ZDateTime.Now);
			CreateInvoiceLineWithVINNumber("EXP", "B000004", "INV004", 1, "001122", "VIN3456", ZDateTime.Now);
			CreateInvoiceLineWithVINNumber("IMP", "B000005", "INV005", 1, "001122", "VIN1234", ZDateTime.Now.AddMonths(-7));
			Factory.Save();
			var invoiceLine = CreateInvoiceLineWithVINNumber("IMP", "B000006", "INV006", 1, "001122", "VIN1234", ZDateTime.Now);
			AssertHasWarningContaining(invoiceLine.JI_VINInfo, "The VIN Number has been used on another declaration");
			AssertHasWarning(invoiceLine.JI_VINInfo, "The VIN Number has been used on another declaration:\r\nDeclaration: 'B000001', Invoice Number: 'INV001', Invoice Line Number: '1'\r\nDeclaration: 'B000002', Invoice Number: 'INV002', Invoice Line Number: '1'");
			invoiceLine.Declaration.JE_MessageType = "EXP";
			invoiceLine.JI_VIN = "VIN3456";
			AssertHasWarningContaining(invoiceLine.JI_VINInfo, "The VIN Number has been used on another declaration");
			AssertHasWarning(invoiceLine.JI_VINInfo, "The VIN Number has been used on another declaration:\r\nDeclaration: 'B000003', Invoice Number: 'INV003', Invoice Line Number: '1'\r\nDeclaration: 'B000004', Invoice Number: 'INV004', Invoice Line Number: '1'");
			invoiceLine.JI_VIN = "VIN5678";
			AssertNoWarningContaining(invoiceLine.JI_VINInfo, "The VIN Number has been used on another declaration");
		}

		public void TestCheckJI_ImportCustomsQty2()
		{
			var universalReferenceDataHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			Factory.Save();
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testLine = dec.InvoiceLines.AddNew();
			var testInstruction = dec.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			testLine.JI_CEI = testInstruction.PK;
			testLine.JI_Procedure = "XXYY";
			testLine.JI_ImportCustomsQty2UQ = "KG";
			testLine.JI_ImportCustomsQty2 = 0m;
			AssertHasMessageErrorContaining(testLine.JI_ImportCustomsQty2Info, MandatoryValidation.YouHaveNotEntered);
			testLine.JI_ImportCustomsQty2 = 1m;
			AssertNoNotifications(testLine.JI_ImportCustomsQty2Info);
		}

		public void TestCheckJI_ImportCustomsQty2UQ()
		{
			var universalReferenceDataHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			Factory.Save();
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testLine = dec.InvoiceLines.AddNew();
			var testInstruction = dec.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			testLine.JI_CEI = testInstruction.PK;
			testLine.JI_Procedure = "XXYY";
			testLine.JI_ImportCustomsQtyUQ = "AA";
			testLine.JI_ImportCustomsQty2 = 1m;
			testLine.JI_ImportCustomsQty2UQ = ZString.Empty;
			AssertHasMessageErrorContaining(testLine.JI_ImportCustomsQty2UQInfo, MandatoryValidation.YouHaveNotEntered);
			testLine.JI_ImportCustomsQty2UQ = "XX";
			AssertHasMessageErrorContaining(testLine.JI_ImportCustomsQty2UQInfo, ListValidation.InvalidCodeMessageError);
			testLine.JI_ImportCustomsQty2UQ = "AA";
			AssertHasMessageErrorContaining(testLine.JI_ImportCustomsQty2UQInfo, "You have entered a duplicate unit code");
			testLine.JI_ImportCustomsQty2UQ = "KG";
			AssertNoNotifications(testLine.JI_ImportCustomsQty2UQInfo);
		}

		public void TestCheckJI_ImportCustomsQty3()
		{
			var universalReferenceDataHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			Factory.Save();
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testLine = dec.InvoiceLines.AddNew();
			var testInstruction = dec.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			testLine.JI_CEI = testInstruction.PK;
			testLine.JI_Procedure = "XXYY";
			testLine.JI_ImportCustomsQty3UQ = "KG";
			testLine.JI_ImportCustomsQty3 = 0m;
			AssertHasMessageErrorContaining(testLine.JI_ImportCustomsQty3Info, MandatoryValidation.YouHaveNotEntered);
			testLine.JI_ImportCustomsQty3 = 1m;
			AssertNoNotifications(testLine.JI_ImportCustomsQty3Info);
		}

		public void TestCheckJI_ImportCustomsQty3UQ()
		{
			var universalReferenceDataHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			Factory.Save();
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testLine = dec.InvoiceLines.AddNew();
			var testInstruction = dec.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			testLine.JI_CEI = testInstruction.PK;
			testLine.JI_Procedure = "XXYY";
			testLine.JI_ImportCustomsQtyUQ = "AA";
			testLine.JI_ImportCustomsQty3 = 1m;
			testLine.JI_ImportCustomsQty3UQ = ZString.Empty;
			AssertHasMessageErrorContaining(testLine.JI_ImportCustomsQty3UQInfo, MandatoryValidation.YouHaveNotEntered);
			testLine.JI_ImportCustomsQty3UQ = "XX";
			AssertHasMessageErrorContaining(testLine.JI_ImportCustomsQty3UQInfo, ListValidation.InvalidCodeMessageError);
			testLine.JI_ImportCustomsQty3UQ = "AA";
			AssertHasMessageErrorContaining(testLine.JI_ImportCustomsQty3UQInfo, "You have entered a duplicate unit code");
			testLine.JI_ImportCustomsQty3UQ = "KG";
			AssertNoNotifications(testLine.JI_ImportCustomsQty3UQInfo);
		}

		public void TestCheckJI_ImportTariff()
		{
			PrepareOriginalEntry();
			var testDeclaration = Factory.New<JobDeclaration>();
			var testInstruction = testDeclaration.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			var testInvoice = testDeclaration.Invoices.AddNew();
			var testInvoiceLine = testInvoice.InvoiceLines.AddNew();
			testInvoiceLine.JI_CEI = testInstruction.PK;
			testInvoiceLine.JI_Procedure = testInvoiceLine.EntryInstruction.CEI_Style + "YY";
			testInvoiceLine.JI_PreviousEntryNumber = "TestMRN";
			CombineAssertions("OriginalNotFound", () =>
			{
				var targetInfo = testInvoiceLine.JI_ImportTariffInfo;
				testInvoiceLine.JI_PreviousEntryLineNumber = 1;
				testInvoiceLine.JI_ImportTariff = "";
				AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
				testInvoiceLine.JI_ImportTariff = "99991";
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportTariff = "99992";
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportTariff = "99993";
				AssertHasMessageErrorContaining(targetInfo, "No Schedule 1 Part 1 Tariff exists for '99993'.");
			});
			CombineAssertions("OriginalFound", () =>
			{
				var targetInfo = testInvoiceLine.JI_ImportTariffInfo;
				testInvoiceLine.JI_PreviousEntryLineNumber = 2;
				testInvoiceLine.JI_ImportTariff = "";
				AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
				testInvoiceLine.JI_ImportTariff = "99991";
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportTariff = "99992";
				AssertHasWarningContaining(targetInfo, "Tariff on DA63 line is different to tariff on import entry.");
				testInvoiceLine.JI_ImportTariff = "99993";
				AssertHasMessageErrorContaining(targetInfo, "No Schedule 1 Part 1 Tariff exists for '99993'.");
			});
		}

		public void TestCheckJI_ImportCustomsQty()
		{
			PrepareOriginalEntry();
			var testDeclaration = Factory.New<JobDeclaration>();
			var testInstruction = testDeclaration.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			var testInvoice = testDeclaration.Invoices.AddNew();
			var testInvoiceLine = testInvoice.InvoiceLines.AddNew();
			testInvoiceLine.JI_CEI = testInstruction.PK;
			testInvoiceLine.JI_Procedure = testInvoiceLine.EntryInstruction.CEI_Style + "YY";
			testInvoiceLine.JI_PreviousEntryNumber = "TestMRN";
			CombineAssertions("OriginalNotFound", () =>
			{
				var targetInfo = testInvoiceLine.JI_ImportCustomsQtyInfo;
				testInvoiceLine.JI_PreviousEntryLineNumber = 1;
				testInvoiceLine.JI_ImportCustomsQty = 0m;
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportCustomsQty = 100m;
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportCustomsQty = 200m;
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportCustomsQty = 300m;
				AssertNoNotifications(targetInfo);
			});
			CombineAssertions("OriginalFound", () =>
			{
				var targetInfo = testInvoiceLine.JI_ImportCustomsQtyInfo;
				testInvoiceLine.JI_PreviousEntryLineNumber = 2;
				testInvoiceLine.JI_ImportCustomsQty = 0m;
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportCustomsQty = 100m;
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportCustomsQty = 200m;
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportCustomsQty = 300m;
				AssertHasMessageErrorContaining(targetInfo, JobComInvoiceLineValidation.DA63ValueShouldntBeExceedingOriginalValue);
			});
		}

		public void TestCheckJI_ImportCustomsQtyUQ()
		{
			PrepareOriginalEntry();
			var testDeclaration = Factory.New<JobDeclaration>();
			var testInstruction = testDeclaration.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			var testInvoice = testDeclaration.Invoices.AddNew();
			var testInvoiceLine = testInvoice.InvoiceLines.AddNew();
			testInvoiceLine.JI_CEI = testInstruction.PK;
			testInvoiceLine.JI_Procedure = testInvoiceLine.EntryInstruction.CEI_Style + "YY";
			testInvoiceLine.JI_PreviousEntryNumber = "TestMRN";
			CombineAssertions("OriginalNotFound", () =>
			{
				var targetInfo = testInvoiceLine.JI_ImportCustomsQtyUQInfo;
				testInvoiceLine.JI_PreviousEntryLineNumber = 1;
				testInvoiceLine.JI_ImportCustomsQtyUQ = "";
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportCustomsQtyUQ = "KG";
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportCustomsQtyUQ = "LI";
				AssertNoNotifications(targetInfo);
			});
			CombineAssertions("OriginalFound", () =>
			{
				var targetInfo = testInvoiceLine.JI_ImportCustomsQtyUQInfo;
				testInvoiceLine.JI_PreviousEntryLineNumber = 2;
				testInvoiceLine.JI_ImportCustomsQtyUQ = "";
				AssertHasMessageErrorContaining(targetInfo, "Customs Quantity Unit on DA63 line should be the same as the Unit from original Import Entry.");
				testInvoiceLine.JI_ImportCustomsQtyUQ = "KG";
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportCustomsQtyUQ = "LI";
				AssertHasMessageErrorContaining(targetInfo, "Customs Quantity Unit on DA63 line should be the same as the Unit from original Import Entry.");
			});
		}

		public void TestCheckJI_ImportCustomsValue()
		{
			PrepareOriginalEntry();
			var testDeclaration = Factory.New<JobDeclaration>();
			var testInstruction = testDeclaration.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			var testInvoice = testDeclaration.Invoices.AddNew();
			var testInvoiceLine = testInvoice.InvoiceLines.AddNew();
			testInvoiceLine.JI_CEI = testInstruction.PK;
			testInvoiceLine.JI_Procedure = testInvoiceLine.EntryInstruction.CEI_Style + "YY";
			testInvoiceLine.JI_PreviousEntryNumber = "TestMRN";
			CombineAssertions("OriginalNotFound", () =>
			{
				var targetInfo = testInvoiceLine.JI_ImportCustomsValueInfo;
				testInvoiceLine.JI_PreviousEntryLineNumber = 1;
				testInvoiceLine.JI_ImportCustomsValue = 0m;
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportCustomsValue = 1000m;
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportCustomsValue = 2000m;
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportCustomsValue = 3000m;
				AssertNoNotifications(targetInfo);
			});
			CombineAssertions("OriginalFound", () =>
			{
				var targetInfo = testInvoiceLine.JI_ImportCustomsValueInfo;
				testInvoiceLine.JI_PreviousEntryLineNumber = 2;
				testInvoiceLine.JI_ImportCustomsValue = 0m;
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportCustomsValue = 1000m;
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportCustomsValue = 2000m;
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportCustomsValue = 3000m;
				AssertHasMessageErrorContaining(targetInfo, JobComInvoiceLineValidation.DA63ValueShouldntBeExceedingOriginalValue);
			});
		}

		public void TestCheckJI_ImportDutyPaid()
		{
			PrepareOriginalEntry();
			var testDeclaration = Factory.New<JobDeclaration>();
			var testInstruction = testDeclaration.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			var testInvoice = testDeclaration.Invoices.AddNew();
			var testInvoiceLine = testInvoice.InvoiceLines.AddNew();
			testInvoiceLine.JI_CEI = testInstruction.PK;
			testInvoiceLine.JI_Procedure = testInvoiceLine.EntryInstruction.CEI_Style + "YY";
			testInvoiceLine.JI_PreviousEntryNumber = "TestMRN";
			CombineAssertions("OriginalNotFound", () =>
			{
				var targetInfo = testInvoiceLine.JI_ImportDutyPaidInfo;
				testInvoiceLine.JI_PreviousEntryLineNumber = 1;
				testInvoiceLine.JI_ImportDutyPaid = 0m;
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportDutyPaid = 11m;
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportDutyPaid = 21m;
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportDutyPaid = 31m;
				AssertNoNotifications(targetInfo);
			});
			CombineAssertions("OriginalFound", () =>
			{
				var targetInfo = testInvoiceLine.JI_ImportDutyPaidInfo;
				testInvoiceLine.JI_PreviousEntryLineNumber = 2;
				testInvoiceLine.JI_ImportDutyPaid = 0m;
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportDutyPaid = 11m;
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportDutyPaid = 21m;
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportDutyPaid = 31m;
				AssertHasMessageErrorContaining(targetInfo, JobComInvoiceLineValidation.DA63ValueShouldntBeExceedingOriginalValue);
			});
		}

		public void TestCheckJI_ImportVATPaid()
		{
			PrepareOriginalEntry();
			var testDeclaration = Factory.New<JobDeclaration>();
			var testInstruction = testDeclaration.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "XX";
			var testInvoice = testDeclaration.Invoices.AddNew();
			var testInvoiceLine = testInvoice.InvoiceLines.AddNew();
			testInvoiceLine.JI_CEI = testInstruction.PK;
			testInvoiceLine.JI_Procedure = testInvoiceLine.EntryInstruction.CEI_Style + "YY";
			testInvoiceLine.JI_PreviousEntryNumber = "TestMRN";
			CombineAssertions("OriginalNotFound", () =>
			{
				var targetInfo = testInvoiceLine.JI_ImportVATPaidInfo;
				testInvoiceLine.JI_PreviousEntryLineNumber = 1;
				testInvoiceLine.JI_ImportVATPaid = 0m;
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportVATPaid = 13m;
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportVATPaid = 23m;
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportVATPaid = 33m;
				AssertNoNotifications(targetInfo);
			});
			CombineAssertions("OriginalFound", () =>
			{
				var targetInfo = testInvoiceLine.JI_ImportVATPaidInfo;
				testInvoiceLine.JI_PreviousEntryLineNumber = 2;
				testInvoiceLine.JI_ImportVATPaid = 0m;
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportVATPaid = 13m;
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportVATPaid = 23m;
				AssertNoNotifications(targetInfo);
				testInvoiceLine.JI_ImportVATPaid = 33m;
				AssertHasMessageErrorContaining(targetInfo, JobComInvoiceLineValidation.DA63ValueShouldntBeExceedingOriginalValue);
			});
		}

		void CheckPartAttributeValidation(ZPropertyInfo attributeInfo, Action validate, OrgHeader owner, OrgSupplierPart part, int attributeNo)
		{
			using (new PartAttributeValidationChecker.AttributeCallChecker(owner, part, attributeInfo, attributeNo))
			{
				validate();
			}
		}

		JobComInvoiceLine SetupInvoiceLineWithOwnerPart()
		{
			var helper = new ZAWhsDataTestHelper(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = helper.Importer.PK;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var changeOfOwnershipCusProcedure = helper.ChangeOfOwnershipCusProcedure;
			entryInstruction.CEI_Style = changeOfOwnershipCusProcedure.ZZ6_ProcedureCode;
			entryInstruction.CEI_OH_Owner = helper.Owner.PK;
			entryInstruction.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = changeOfOwnershipCusProcedure.ZZ6_ProcedureCode + changeOfOwnershipCusProcedure.ZZ6_PreviousProcedureCode;
			invoiceLine.JI_NewOwnerPartNo = helper.OwnerPart.OP_PartNum;
			return invoiceLine;
		}

		JobComInvoiceLine CreateInvoiceLineWithVINNumber(string messageType, string jobNumber, string invoiceNumber, short invoiceLineNo, string tariffNumber, string vinNumber, ZDateTime createDate)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = messageType;
			declaration.JE_DeclarationReference = jobNumber;
			declaration.JE_SystemCreateTimeUtc = createDate;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = invoiceNumber;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = invoiceLineNo;
			invoiceLine.JI_Tariff = tariffNumber;
			invoiceLine.JI_VIN = vinNumber;
			return invoiceLine;
		}

		void PrepareOriginalEntry()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "X", "YY", "00", "", "XXYY5", "IMP", true);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			var tariffType12B = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12B");
			var rateType1 = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty);
			var rateType2 = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty);
			var rateType3 = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.AdValoremExcise);
			var rateCode1 = helper.LoadOrCreateNewCusRateCode(Factory, "RT1", rateType1.PK);
			var rateCode2 = helper.LoadOrCreateNewCusRateCode(Factory, "RT2", rateType2.PK);
			var rateCode3 = helper.LoadOrCreateNewCusRateCode(Factory, "RT3", rateType3.PK);
			Factory.Save();
			var tariffView1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "99991", startDate, endDate);
			helper.CreateRate(tariffView1, rateCode1.PK, startDate, endDate);
			rateType1.ZZR_IsPayable = true;
			var tariffView2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "99992", startDate, endDate);
			helper.CreateRate(tariffView2, rateCode2.PK, startDate, endDate);
			var tariffView3 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12B.PK, "99990", startDate, endDate);
			helper.CreateRate(tariffView3, rateCode3.PK, startDate, endDate);
			rateType3.ZZR_IsPayable = true;
			helper.CreateTariffUOM(tariffView1, "CU1", "KG");
			helper.CreateTariffUOM(tariffView2, "CU1", "LI");
			Factory.Save();
			var testOrgDeclaration = Factory.New<JobDeclaration>();
			testOrgDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			testOrgDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testOrgInstruction = testOrgDeclaration.CustomsEntryInstructions.AddNew();
			testOrgInstruction.CEI_Style = "YY";
			var testOrgInvHeader = testOrgDeclaration.Invoices.AddNew();
			var testOrgInvLine = testOrgInvHeader.InvoiceLines.AddNew();
			testOrgInvLine.JI_CustomsQuantity = 150;
			testOrgInvLine.JI_CustomsUnitQty = "KG";
			testOrgInvLine.JI_ZZF_NKTaxType = "VAT";
			var testOrgInvLine2 = testOrgInvHeader.InvoiceLines.AddNew();
			testOrgInvLine2.JI_CustomsQuantity = 50;
			testOrgInvLine2.JI_CustomsUnitQty = "KG";
			testOrgInvLine2.JI_ZZF_NKTaxType = "VAT";
			var testOrgEntry = testOrgDeclaration.ActiveEntryHeaders.AddNew();
			testOrgEntry.MovementReferenceNumberSetter("TestMRN", ZDateTime.Today);
			var testEntryLine = testOrgEntry.MergedLines.AddNew();
			testOrgInvLine.JI_CL = testEntryLine.PK;
			testOrgInvLine2.JI_CL = testEntryLine.PK;
			testEntryLine.CL_AdValoremTariff = "99991";
			testEntryLine.CL_CustomsValue = 2000m;
			testEntryLine.CL_LineNumber = 2;
			testEntryLine.Fees.AddOrUpdate("1P1", 21);
			testEntryLine.Fees.AddOrUpdate("12B", 22);
			testEntryLine.Fees.AddOrUpdate("VAT", 23);
			testEntryLine.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PPA, 24);
			testEntryLine.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PEN, 25);
			Factory.Save();
		}

		protected override void SetUp()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface);
			base.SetUp();
			helper = new ZAUniversalReferenceTestDataHelper(Factory);
			declaration = Factory.New<JobDeclaration>();
			instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}

		ZAUniversalReferenceTestDataHelper helper;
		JobDeclaration declaration;
		CusEntryInstruction instruction;
		JobComInvoiceLine invoiceLine;
		JobComInvoiceHeader invoice;
	}
}
