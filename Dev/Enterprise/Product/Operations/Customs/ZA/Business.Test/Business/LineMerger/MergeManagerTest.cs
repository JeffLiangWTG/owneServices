using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class MergeManagerTest : Customs.Business.Testing.MergeManagerTest
	{
		public override void TestRequiresMerge()
		{
			var declaration = (JobDeclaration)GetJobDeclaration();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			var mergeManager = new MergeManager(declaration);
			AssertEquals(false, mergeManager.RequiresMerge);
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			mergeManager = new MergeManager(declaration);
			AssertEquals(true, mergeManager.RequiresMerge);
		}

		public override void TestHumanReadableNameForMerge()
		{
			var declaration = GetJobDeclaration();
			var mergeManager = declaration.MergeManager;
			AssertEquals("HumanReadableNameForMerge", "frame", mergeManager.HumanReadableNameForMerge);
		}

		public override void TestSupportsAutoMerge()
		{
			var declaration = GetJobDeclaration();
			var mergeManager = declaration.MergeManager;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			AssertEquals(false, mergeManager.SupportsAutoMerge);
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			AssertEquals(true, mergeManager.SupportsAutoMerge);
		}

		public void TestValidationOfTargetEntryLineNumberUponMerged()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_DateForDuty = new ZDateTime(2019, 1, 1);
			testInst.CEI_Style = "11";
			var testInvoice = declaration.Invoices.AddNew();
			testInvoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var testInvoiceLine1 = testInvoice.InvoiceLines.AddNew();
			testInvoiceLine1.JI_CEI = testInst.PK;
			testInvoiceLine1.JI_Procedure = testInvoiceLine1.EntryInstruction.CEI_Style + "00";
			testInvoiceLine1.JI_Tariff = "101021";
			var testInvoiceLine2 = testInvoice.InvoiceLines.AddNew();
			testInvoiceLine2.JI_CEI = testInst.PK;
			testInvoiceLine2.JI_Procedure = testInvoiceLine2.EntryInstruction.CEI_Style + "00";
			testInvoiceLine2.JI_Tariff = "101024";
			testInvoiceLine1.JI_TargetEntryLineNumber = 1;
			testInvoiceLine2.JI_TargetEntryLineNumber = 1;
			declaration.DoMerge();
			CombineAssertions(() =>
			{
				var entryHeader = declaration.CustomsEntryHeaders[0];
				AssertEquals(2, entryHeader.MergedLines.Count);
				var mergedLine = entryHeader.MergedLines[0];
				AssertEquals(new ZShort(1), testInvoiceLine1.JI_TargetEntryLineNumber);
				AssertEquals(new ZShort(1), testInvoiceLine2.JI_TargetEntryLineNumber);
				AssertEquals(new ZShort(1), testInvoiceLine1.CusEntryLine.CL_LineNumber);
				AssertEquals(new ZShort(2), testInvoiceLine2.CusEntryLine.CL_LineNumber);
				AssertNoMessageErrors(testInvoiceLine1.JI_TargetEntryLineNumberInfo);
				AssertHasMessageErrorContaining(testInvoiceLine2.JI_TargetEntryLineNumberInfo, "The linked entry line has a Line Number \"2\" while the Target Entry Line Number is \"1\".\r\nPlease make sure you put the right Target Entry Line Number.");
			});
		}

		[ExpectNoExceptions]
		public void TestValidationOfRemovalDetailsUponMerged()
		{
			var entryInstructionMock = Factory.NewMoq<CusEntryInstruction>();
			var validationMock = new Mock<CusEntryInstructionValidation>(entryInstructionMock.Object);
			var validationProtectedMock = validationMock.Protected();
			validationProtectedMock.Setup("CheckCEI_OH_Carrier");
			validationProtectedMock.Setup("CheckCEI_OH_BondHolder");
			validationProtectedMock.Setup("CheckOH_SubContractor");
			entryInstructionMock.Protected().Setup<Customs.Business.CusEntryInstructionValidation>("GetNewValidation").Returns(validationMock.Object);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.CustomsEntryInstructions.Add(entryInstructionMock.Object);
			var testInvoice = declaration.Invoices.AddNew();
			testInvoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			_ = testInvoice.InvoiceLines.AddNew();
			declaration.DoMerge();

			validationProtectedMock.Verify("CheckCEI_OH_Carrier", Times.Once());
			validationProtectedMock.Verify("CheckCEI_OH_BondHolder", Times.Once());
			validationProtectedMock.Verify("CheckOH_SubContractor", Times.Once());
		}

		public void TestGetReasonCannotMerge()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, "ZAR"));
			currency.ExchangeRates.DeleteAll();
			RefExchangeRate rate = currency.ExchangeRates.AddNew();
			rate.RE_ExRateType = "CUS";
			rate.RE_StartDate = new ZDateTime(2016, 12, 1);
			rate.RE_ExpiryDate = new ZDateTime(2016, 12, 1);
			rate.RE_SellRate = 0.70m;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_MasterBillIssuedDate = new ZDateTime(2016, 12, 2);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var reasonCannotMergeInvoiceHadNoExchangeRate = ZA.Business.MergeManager.ReasonCannotMergeInvoiceHadNoExchangeRate;
			var notifier = new SendsMessagesToCustomsShutterUpperer(false);
			AssertEquals(reasonCannotMergeInvoiceHadNoExchangeRate, declaration.MergeManager.CheckAndGetPrerequisiteConditions(notifier));
			invoice.JZ_RX_NKInvoice_Currency = currency.RX_Code;
			AssertEquals(reasonCannotMergeInvoiceHadNoExchangeRate, declaration.MergeManager.CheckAndGetPrerequisiteConditions(notifier));
			declaration.JE_MasterBillIssuedDate = new ZDateTime(2016, 12, 1);
			AssertEquals(ZString.Empty, declaration.MergeManager.CheckAndGetPrerequisiteConditions(notifier));
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
			CusEntryInstruction instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			invoiceLine.JI_CEI = instruction.PK;
			AssertEquals(ZShort.Zero, invoiceLine.JI_PreviousEntryLineNumber);
			AssertEquals(ZA.Business.MergeManager.ReasonCannotMergeInvoiceLineHasNoWHSMRNLineNumber, declaration.MergeManager.CheckAndGetPrerequisiteConditions(notifier));
			invoiceLine.JI_PreviousEntryLineNumber = 3;
			AssertEquals(ZA.Business.MergeManager.ReasonCannotMergeEntryInstructionHasNoWHSMRNLineNumber, declaration.MergeManager.CheckAndGetPrerequisiteConditions(notifier));
			instruction.CEI_PreviousMRN = "JBH201505120123456";
			AssertEquals(ZString.Empty, declaration.MergeManager.CheckAndGetPrerequisiteConditions(notifier));
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_PreviousEntryLineNumber = 3;
			AssertEquals(ZA.Business.MergeManager.ReasonCannotMergeInvoiceLineHasDuplicateWHSMRNLineNumberPerEnteryInstruction(ZString.Empty, 3), declaration.MergeManager.CheckAndGetPrerequisiteConditions(notifier));
			invoiceLine2.JI_PreviousEntryLineNumber = 4;
			AssertEquals(ZString.Empty, declaration.MergeManager.CheckAndGetPrerequisiteConditions(notifier));
			CusEntryInstruction instruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction2.CEI_PreviousMRN = "JBH201505120123456";
			AssertEquals(ZA.Business.MergeManager.ReasonCannotMergeEntryInstructionHasDuplicateWHSMRNLineNumbers, declaration.MergeManager.CheckAndGetPrerequisiteConditions(notifier));
			instruction2.CEI_PreviousMRN = "JBH201505120123457";
			AssertEquals(ZString.Empty, declaration.MergeManager.CheckAndGetPrerequisiteConditions(notifier));
		}

		public void TestCheckAndGetPrerequisiteConditionsWhenSplittingByBondAmount()
		{
			var bondHolder1 = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Description = "CEI1";
			entryInstruction1.CEI_OH_BondHolder = bondHolder1.PK;
			var entryInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Description = "CEI2";

			using (ZACustomsRegistry.Instance.AllowAutomaticSplitEntriesByBondAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var shutterUpperer = new SendsMessagesToCustomsShutterUpperer(true);
				var mergeManager = new MergeManagerForTest(declaration);
				var result = mergeManager.CheckAndGetPrerequisiteConditions(shutterUpperer);

				CombineAssertions(() =>
				{
					AssertEquals("Should be two user notifications", 2, shutterUpperer.LastWarnings?.Count ?? 0);
					AssertEquals("CEI1", "Entry Instruction - CEI1. Bond Holder not found or does not have a Bond Guarantee Value configured. Entries will not be split.", shutterUpperer.LastWarnings?[0] ?? ZString.Empty);
					AssertEquals("CEI2", "Entry Instruction - CEI2. Bond Holder not found or does not have a Bond Guarantee Value configured. Entries will not be split.", shutterUpperer.LastWarnings?[1] ?? ZString.Empty);
					AssertEquals("CEI1 - BHValid - FALSE", false, entryInstruction1.BHValid);
					AssertEquals("CEI2 - BHValid - FALSE", false, entryInstruction2.BHValid);
				});

				bondHolder1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BondHolderCode, "1000", Core.Constants.CountryCodes.SouthAfrica);
				bondHolder1.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.BGV, "100000", "ZA");

				mergeManager = new MergeManagerForTest(declaration);
				shutterUpperer = new SendsMessagesToCustomsShutterUpperer(true);
				result = mergeManager.CheckAndGetPrerequisiteConditions(shutterUpperer);

				CombineAssertions(() =>
				{
					AssertEquals("Should be 1 user notification", 1, shutterUpperer.LastWarnings?.Count ?? 0);
					AssertEquals("CEI2", "Entry Instruction - CEI2. Bond Holder not found or does not have a Bond Guarantee Value configured. Entries will not be split.", shutterUpperer.LastWarnings?[0] ?? ZString.Empty);
					AssertEquals("CEI1 - BHValid - TRUE", true, entryInstruction1.BHValid);
					AssertEquals("CEI2 - BHValid - FALSE", false, entryInstruction2.BHValid);
				});

				var bondHolder2 = Factory.NewWithValidTestData<OrgHeader>();
				bondHolder2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BondHolderCode, "1000", Core.Constants.CountryCodes.SouthAfrica);
				bondHolder2.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.BGV, "100000", "ZA");
				entryInstruction2.CEI_OH_BondHolder = bondHolder2.PK;

				mergeManager = new MergeManagerForTest(declaration);
				shutterUpperer = new SendsMessagesToCustomsShutterUpperer(true);
				result = mergeManager.CheckAndGetPrerequisiteConditions(shutterUpperer);

				CombineAssertions(() =>
				{
					AssertEquals("Should be 0 user notification", 0, shutterUpperer.LastWarnings?.Count ?? 0);
					AssertEquals("CEI1 - BHValid - TRUE", true, entryInstruction1.BHValid);
					AssertEquals("CEI2 - BHValid - TRUE", true, entryInstruction2.BHValid);
				});
			}
		}

		protected override Type GetLineMergerType() => typeof(LineMerger);

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._11;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			return declaration;
		}
	}

	class MergeManagerForTest : MergeManager
	{
		public MergeManagerForTest(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override string GetReasonCannotMerge() => ZString.Empty;
	}
}
