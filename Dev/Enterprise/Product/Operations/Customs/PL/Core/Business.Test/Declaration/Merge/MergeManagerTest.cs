using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class MergeManagerTest : EU.Business.Declaration.Testing.MergeManagerTest
{
	public void TestCheckAndGetPrerequisiteConditions_Messages()
	{
		var declaration = Factory.New<JobDeclaration>();
		var errorCondition = declaration.MergeManager.CheckAndGetPrerequisiteConditions(new SendsMessagesToCustomsReturningResultsAsProperties(true));
		AssertContains("There is no Invoice yet", Core.Constants.Customs.MergeErrors.ReasonCannotMergeNoInvoiceHeaders, errorCondition);

		declaration.Invoices.AddNew();
		errorCondition = declaration.MergeManager.CheckAndGetPrerequisiteConditions(new SendsMessagesToCustomsReturningResultsAsProperties(true));
		AssertContains("There is no Invoice line yet", Core.Constants.Customs.MergeErrors.ReasonCannotMergeInvoiceHadNoInvoiceLines, errorCondition);

		declaration.InvoiceLines.AddNew();
		errorCondition = declaration.MergeManager.CheckAndGetPrerequisiteConditions(new SendsMessagesToCustomsReturningResultsAsProperties(true));
		AssertEquals("There is no Entry Instruction line yet", "You can't merge this entry because there are no Entry Instructions.", errorCondition);

		var entry = declaration.CustomsEntryInstructions.AddNew();
		errorCondition = declaration.MergeManager.CheckAndGetPrerequisiteConditions(new SendsMessagesToCustomsReturningResultsAsProperties(true));
		AssertEquals("There is entry instruction, invoice and invoice line", ZString.Empty, errorCondition);

		entry.CEI_DateForDuty = ZDateTime.UtcToday.AddDays(2);
		errorCondition = declaration.MergeManager.CheckAndGetPrerequisiteConditions(new SendsMessagesToCustomsReturningResultsAsProperties(true));
		AssertEquals("Entry instruction Decl. Date is in the future.", ZString.Empty, errorCondition);

		entry.CEI_DateForDuty = ZDateTime.UtcToday.AddDays(-2);
		errorCondition = declaration.MergeManager.CheckAndGetPrerequisiteConditions(new SendsMessagesToCustomsReturningResultsAsProperties(true));
		AssertEquals("Entry instruction Decl. Date is in the past.", "Entry Instruction Declaration Date is obsolete user canceled merge.", errorCondition);

		entry.CEI_DateForDuty = ZDateTime.UtcToday.AddDays(2);
		errorCondition = declaration.MergeManager.CheckAndGetPrerequisiteConditions(new SendsMessagesToCustomsReturningResultsAsProperties(true));
		AssertEquals("Entry instruction Decl. Date is in the future.", ZString.Empty, errorCondition);

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entry.PK;
		AssertNotNull(entry.EntryHeader);

		entryHeader.EntryNumber = "Test";
		entry.CEI_DateForDuty = ZDateTime.UtcToday.AddDays(-2);
		errorCondition = declaration.MergeManager.CheckAndGetPrerequisiteConditions(new SendsMessagesToCustomsReturningResultsAsProperties(true));
		AssertEquals("Entry instruction Decl. Date is in the past.", ZString.Empty, errorCondition);

		entryHeader.EntryNumber = ZString.Empty;
		entry.CEI_DateForDuty = ZDateTime.UtcToday.AddDays(-2);
		errorCondition = declaration.MergeManager.CheckAndGetPrerequisiteConditions(new SendsMessagesToCustomsReturningResultsAsProperties(true));
		AssertEquals("Entry instruction Decl. Date is in the past.", "Entry Instruction Declaration Date is obsolete user canceled merge.", errorCondition);
	}

	public void TestCheckAndGetPrerequisiteConditions_UpdateObsoleteDateForDuty()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.Invoices.AddNew();
		var invoiceLine = declaration.InvoiceLines.AddNew();
		var entry1 = declaration.CustomsEntryInstructions.AddNew();
		entry1.CEI_DateForDuty = ZDateTime.Empty;
		invoiceLine.JI_CEI = entry1.PK;

		var entry2 = declaration.CustomsEntryInstructions.AddNew();
		var entry2DateForDuty = ZDateTime.Today.AddDays(-2);
		entry2.CEI_DateForDuty = entry2DateForDuty;

		var entry3 = declaration.CustomsEntryInstructions.AddNew();
		var entry3DateForDuty = ZDateTime.Today.AddDays(2);
		entry3.CEI_DateForDuty = entry3DateForDuty;

		var errorCondition = declaration.MergeManager.CheckAndGetPrerequisiteConditions(new SendsMessagesToCustomsReturningResultsAsProperties(YesNoCancel.Cancel));
		AssertEquals("Entry Instruction Declaration Date is obsolete user canceled merge.", errorCondition);
		AssertEquals(entry3DateForDuty, entry3.CEI_DateForDuty);
		AssertEquals(entry2DateForDuty, entry2.CEI_DateForDuty);
		AssertEquals(ZDateTime.Empty, entry1.CEI_DateForDuty);

		errorCondition = declaration.MergeManager.CheckAndGetPrerequisiteConditions(new SendsMessagesToCustomsReturningResultsAsProperties(YesNoCancel.No));
		AssertEquals(ZString.Empty, errorCondition);
		AssertEquals(entry3DateForDuty, entry3.CEI_DateForDuty);
		AssertEquals(entry2DateForDuty, entry2.CEI_DateForDuty);
		AssertEquals(ZDateTime.Empty, entry1.CEI_DateForDuty);

		errorCondition = declaration.MergeManager.CheckAndGetPrerequisiteConditions(new SendsMessagesToCustomsReturningResultsAsProperties(YesNoCancel.Yes));
		AssertEquals(ZString.Empty, errorCondition);
		AssertEquals(entry3DateForDuty, entry3.CEI_DateForDuty);
		Assert(entry2.CEI_DateForDuty > ZDateTime.Today.AddMinutes(-2));
		Assert(entry1.CEI_DateForDuty > ZDateTime.Today.AddMinutes(-2));
	}

	public void TestFeeShouldHasNotificationsAfterMerge()
	{
		var tariffCode = "0101100000";
		var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
		var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
		var tariffType = referenceDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, Universal.Constants.TariffTypes.Export);
		var tariff = referenceDataHelper.LoadOrCreateNewTariff(dataGrouping, tariffType.PK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Tariff");
		var rateType = referenceDataHelper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.Duty);
		var rateCode = referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, rateType.PK);
		var rateForTariff = referenceDataHelper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.8", dataGrouping: dataGrouping);
		var tradeGroup = referenceDataHelper.LoadOrCreateTradeGroup(dataGrouping, Core.Constants.CountryCodes.Poland, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		referenceDataHelper.CreateCusApplicability(rateForTariff, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

		Factory.Save();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
		var invoiceLineForFee = invoice.JobComInvoiceLines.AddNew();
		invoiceLineForFee.JI_Tariff = tariffCode;
		invoiceLineForFee.JI_LinePrice = 10;
		invoiceLineForFee.JI_CEI = instruction.PK;

		var entry = declaration.ActiveEntryHeaders.AddNew();
		entry.CH_BGMReference = tariffCode;
		var entryLine = entry.MergedLines.AddNew();
		entryLine.CL_CustomsValue = 10;
		entryLine.CL_AdValoremTariff = tariffCode;
		invoiceLineForFee.JI_CL = entryLine.PK;

		declaration.DoMerge();

		var fee = entryLine.Fees[0];
		var notifications = fee.Notifications.Count();
		AssertGreaterThan(notifications, 0);
	}

	public void TestYesNoMessage()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.Invoices.AddNew();
		var invoiceLine = declaration.InvoiceLines.AddNew();
		var entry1 = declaration.CustomsEntryInstructions.AddNew();
		entry1.CEI_SubStyle = "T";
		entry1.CEI_Description = "D";
		invoiceLine.JI_CEI = ZGuid.Empty;

		const string yesNoQuestionMessage = "You can’t merge this entry because there are Invoice Lines without Entry Instruction.\r\n Auto assign Entry Instruction < T - D > \r\n for all Invoice Lines?";
		const string yesNoQuestionCaption = "There are Inv. Lines without Entry Instruction.";
		const string yesNoWarningMessage = "You can’t merge this entry because there are Inv. Lines without Entry Instruction.";
		const string yesNoWarningCaption = "There are multiple Entry Instructions that prevent automatic assignment.";

		CombineAssertions("Message and Caption", () =>
		{
			var yesNoQueryMessage = string.Empty;
			var yesNoQueryCaption = string.Empty;
			var warnUserAboutSomethingMessage = string.Empty;
			var warnUserAboutSomethingCaption = string.Empty;
			var mock = new Mock<ISendsMessagesToCustoms>();
			mock.Setup(x => x.YesNoQuery(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageStyle>()))
				.Callback(new Action<string, string, MessageStyle>((m, c, s) =>
				{
					yesNoQueryMessage = m;
					yesNoQueryCaption = c;
				})).Returns(false);
			mock.Setup(x => x.WarnUserAboutSomething(It.IsAny<string>(), It.IsAny<string>()))
				.Callback(new Action<string, string>((m, c) =>
				{
					warnUserAboutSomethingMessage = m;
					warnUserAboutSomethingCaption = c;
				}));

			var notifier = mock.Object;

			declaration.MergeManager.CheckAndGetPrerequisiteConditions(notifier);

			AssertEquals("YesNoQuery Message", yesNoQuestionMessage, yesNoQueryMessage);
			AssertEquals("YesNoQuery Caption", yesNoQuestionCaption, yesNoQueryCaption);

			var entry2 = declaration.CustomsEntryInstructions.AddNew();
			declaration.MergeManager.CheckAndGetPrerequisiteConditions(notifier);

			AssertEquals("WarnUser Message", yesNoWarningMessage, warnUserAboutSomethingMessage);
			AssertEquals("WarnUser Caption", yesNoWarningCaption, warnUserAboutSomethingCaption);
		});
	}

	public void TestCheckAndGetPrerequisiteConditions_UpdateEntryInstructionInInvoiceLineWhenAreMultipleEntryInstructionsAllowedIsTrue()
	{
		var declaration = Factory.New<DummyJobDeclaration>();
		declaration.AreMultipleEntryInstructionsAllowedReturns = true;
		declaration.Invoices.AddNew();
		var invoiceLine = declaration.InvoiceLines.AddNew();
		var entry1 = declaration.CustomsEntryInstructions.AddNew();
		entry1.CEI_DateForDuty = ZDateTime.Today.AddDays(2);
		invoiceLine.JI_CEI = ZGuid.Empty;

		CombineAssertions("When AreMultipleEntryInstructionsAllowed is true", () =>
		{
			AssertEquals("AreMultipleEntryInstructionsAllowed should be true", true, declaration.AreMultipleEntryInstructionsAllowed);

			var errorCondition = declaration.MergeManager.CheckAndGetPrerequisiteConditions(new SendsMessagesToCustomsReturningResultsAsProperties(false));
			AssertEquals("There should be an error", "You can’t merge this entry because there are Inv. Lines without Entry Instruction.", errorCondition);
			AssertEquals("JI_CEI should be empty", ZGuid.Empty, invoiceLine.JI_CEI);

			errorCondition = declaration.MergeManager.CheckAndGetPrerequisiteConditions(new SendsMessagesToCustomsReturningResultsAsProperties(true));
			AssertEquals("There should be no error", ZString.Empty, errorCondition);
			AssertEquals("JI_CEI should not be empty", entry1.PK, invoiceLine.JI_CEI);

			var entry2 = declaration.CustomsEntryInstructions.AddNew();
			entry2.CEI_DateForDuty = ZDateTime.Today.AddDays(2);
			invoiceLine.JI_CEI = ZGuid.Empty;

			errorCondition = declaration.MergeManager.CheckAndGetPrerequisiteConditions(new SendsMessagesToCustomsReturningResultsAsProperties(false));
			AssertEquals("There should be an error", "You can’t merge this entry because there are Inv. Lines without Entry Instruction.", errorCondition);
			AssertEquals("JI_CEI should be empty", ZGuid.Empty, invoiceLine.JI_CEI);

			errorCondition = declaration.MergeManager.CheckAndGetPrerequisiteConditions(new SendsMessagesToCustomsReturningResultsAsProperties(true));
			AssertEquals("There should be an error", "You can’t merge this entry because there are Inv. Lines without Entry Instruction.", errorCondition);
			AssertEquals("JI_CEI should be empty", ZGuid.Empty, invoiceLine.JI_CEI);
		});
	}

	public void TestCheckAndGetPrerequisiteConditions_UpdateEntryInstructionInInvoiceLineWhenAreMultipleEntryInstructionsAllowedIsFalse()
	{
		var declaration = Factory.New<DummyJobDeclaration>();
		declaration.AreMultipleEntryInstructionsAllowedReturns = false;
		declaration.Invoices.AddNew();
		var invoiceLine = declaration.InvoiceLines.AddNew();
		var entry1 = declaration.CustomsEntryInstructions.AddNew();
		entry1.CEI_DateForDuty = ZDateTime.Today.AddDays(2);
		invoiceLine.JI_CEI = ZGuid.Empty;

		CombineAssertions("When AreMultipleEntryInstructionsAllowed is false", () =>
		{
			AssertEquals("AreMultipleEntryInstructionsAllowed should be false", false, declaration.AreMultipleEntryInstructionsAllowed);

			var errorCondition = declaration.MergeManager.CheckAndGetPrerequisiteConditions(new SendsMessagesToCustomsReturningResultsAsProperties(false));
			AssertEquals("There should be no error", ZString.Empty, errorCondition);
			AssertEquals("JI_CEI should not be empty", entry1.PK, invoiceLine.JI_CEI);

			invoiceLine.JI_CEI = ZGuid.Empty;
			errorCondition = declaration.MergeManager.CheckAndGetPrerequisiteConditions(new SendsMessagesToCustomsReturningResultsAsProperties(true));
			AssertEquals("There should be no error", ZString.Empty, errorCondition);
			AssertEquals("JI_CEI should not be empty", entry1.PK, invoiceLine.JI_CEI);

			var entry2 = declaration.CustomsEntryInstructions.AddNew();
			entry2.CEI_DateForDuty = ZDateTime.Today.AddDays(2);
			invoiceLine.JI_CEI = ZGuid.Empty;

			errorCondition = declaration.MergeManager.CheckAndGetPrerequisiteConditions(new SendsMessagesToCustomsReturningResultsAsProperties(false));
			AssertEquals("There should be an error", "You can’t merge this entry because there are Inv. Lines without Entry Instruction.", errorCondition);
			AssertEquals("JI_CEI should be empty", ZGuid.Empty, invoiceLine.JI_CEI);

			errorCondition = declaration.MergeManager.CheckAndGetPrerequisiteConditions(new SendsMessagesToCustomsReturningResultsAsProperties(true));
			AssertEquals("There should be an error", "You can’t merge this entry because there are Inv. Lines without Entry Instruction.", errorCondition);
			AssertEquals("JI_CEI should be empty", ZGuid.Empty, invoiceLine.JI_CEI);
		});
	}

	protected override BaseJobDeclaration GetJobDeclaration()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entry = dec.CustomsEntryInstructions.AddNew();
		entry.CEI_DateForDuty = ZDateTime.UtcToday.AddDays(2);
		return dec;
	}

	protected override BaseJobDeclaration ImportJobDeclaration
	{
		get
		{
			var result = (JobDeclaration)GetJobDeclaration();
			result.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			return result;
		}
	}

	protected override Type GetLineMergerType() => typeof(LineMerger);
}

sealed class DummyJobDeclaration : JobDeclaration
{
	public DummyJobDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public ZBool AreMultipleEntryInstructionsAllowedReturns { get; set; } = false;

	public override ZBool AreMultipleEntryInstructionsAllowed => AreMultipleEntryInstructionsAllowedReturns;
}
