using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(StatementDeleteAndSendingAction))]
	sealed class StatementDeleteAndSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPaymentType()
		{
			StatementDeleteAndSendingAction action = (StatementDeleteAndSendingAction)GetNewBusinessObject();
			action.US_SendMessage = true;
			action.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			Assert(action.PaymentTypeList.ContainsCode(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate));
			action.US_PreliminaryStatementPrintDate = new ZDateTime(2009, 1, 1);
			action.US_PeriodicStatementMonth = "01";

			action.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			AssertEquals("", action.US_PeriodicStatementMonth);
			AssertEquals(new ZDateTime(2009, 1, 1), action.US_PreliminaryStatementPrintDate);

			action.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertEquals(ZDateTime.Empty, action.US_PreliminaryStatementPrintDate);
		}

		[TestDate(2012, 12, 1)]
		public void TestStatementMonth()
		{
			var action = (StatementDeleteAndSendingAction)GetNewBusinessObject();
			action.US_SendMessage = true;
			action.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			Assert(action.PaymentTypeList.ContainsCode(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate));
			action.US_PreliminaryStatementPrintDate = new ZDateTime(2012, 3, 1);
			action.US_PeriodicStatementMonth = "04";

			AssertHasMessageError(action.US_PeriodicStatementMonthInfo, PeriodicStatementMMValidator.CurrentMonthOrNextTwo);
		}

		public void TestPaymentType1ForACE()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.ImportEntryNumber = "1";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			var actions = new StatementDeleteAndSendingActionCollection(declaration);
			AssertEquals(1, actions.Count);
			actions[0].US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertHasMessageError(actions[0].US_PaymentTypeInfo, ACEImportAddInfoJobDeclarationValidation.IndividualPaymentTypeNotAllowedUntilStatementIsIssued);

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_EntryFilerCode = "XJ5";

			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_EntryNum = "1";
			Factory.Save();

			AssertNotNull(statementLine.Declaration);
			actions[0].US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertNoMessageError(actions[0].US_PaymentTypeInfo, ACEImportAddInfoJobDeclarationValidation.IndividualPaymentTypeNotAllowedUntilStatementIsIssued);

			statementLine.B3_Status = StatementLineStatusList.Codes.Deleted;
			Factory.Save();
			actions[0].US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertNoMessageError(actions[0].US_PaymentTypeInfo, ACEImportAddInfoJobDeclarationValidation.IndividualPaymentTypeNotAllowedUntilStatementIsIssued);
		}

		public void TestPaymentTypeList()
		{
			ReconDeclaration reconDecl = new ReconDeclaration(Factory.New<JobDeclaration>());
			StatementDeleteAndSendingAction action1 = new StatementDeleteAndSendingAction(reconDecl);
			AssertEquals(false, action1.PaymentTypeList.ContainsCode(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate));
			AssertEquals(false, action1.PaymentTypeList.ContainsCode(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter));
			AssertEquals(false, action1.PaymentTypeList.ContainsCode(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporterWithSuffixes));
			AssertEquals(true, action1.PaymentTypeList.ContainsCode(PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode));
		}

		public void TestValidateUS_PaymentType()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_IsInvoiceByRequest = true;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "123008";

			StatementDeleteAndSendingAction action = (StatementDeleteAndSendingAction)GetNewBusinessObject();
			action.US_SendMessage = true;
			action.US_PaymentType = "";
			AssertHasMessageErrorContaining(action.US_PaymentTypeInfo, MandatoryValidation.YouHaveNotEntered);

			action.US_PaymentType = "~";
			AssertNoMessageErrorContaining(action.US_PaymentTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(action.US_PaymentTypeInfo, ListValidation.InvalidCodeMessageError);

			action.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			AssertNoMessageErrorContaining(action.US_PaymentTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestPortOfEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_IsInvoiceByRequest = true;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var action = new StatementDeleteAndSendingAction(declaration);
			AssertHasMessageErrorContaining(action.PortOfEntryInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_SchDEntry = "1101";
			action = new StatementDeleteAndSendingAction(declaration);
			AssertNoMessageErrorContaining(action.PortOfEntryInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestIsSinglePaymentOrIsStatementPayment()
		{
			StatementDeleteAndSendingAction action = (StatementDeleteAndSendingAction)GetNewBusinessObject();
			AssertEquals(false, action.IsSinglePayment);

			action.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertEquals(true, action.IsSinglePayment);

			action.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			AssertEquals(false, action.IsSinglePayment);
		}

		[TestDate(2013, 03, 09)]
		public void TestValidateUS_PeriodicStatementMonth()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_IsInvoiceByRequest = true;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2013, 03, 01);

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "123008";

			var action = new StatementDeleteAndSendingAction(declaration);
			action.US_SendMessage = true;
			action.US_PeriodicStatementMonth = "03";
			AssertHasWarning(action.US_PeriodicStatementMonthInfo, PeriodicStatementMMValidator.MonthTheSameAsReleaseDateMonth);

			action.US_PeriodicStatementMonth = "04";
			AssertNoWarning(action.US_PeriodicStatementMonthInfo, PeriodicStatementMMValidator.MonthTheSameAsReleaseDateMonth);

			var reconDecl = new ReconDeclaration(Factory.New<JobDeclaration>());
			var action1 = new StatementDeleteAndSendingAction(reconDecl);
			action1.US_SendMessage = true;
			action1.US_PeriodicStatementMonth = "03";
			AssertNoWarning("Not relevant for reconciliation", action1.US_PeriodicStatementMonthInfo, PeriodicStatementMMValidator.MonthTheSameAsReleaseDateMonth);
		}

		[TestDate(2009, 05, 19)]
		public void TestValidateUS_PreliminaryStatementPrintDate()
		{
			StatementDeleteAndSendingAction action = (StatementDeleteAndSendingAction)GetNewBusinessObject();
			action.US_SendMessage = true;
			action.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			action.US_PreliminaryStatementPrintDate = ZDateTime.BrettsBirthday;
			AssertHasMessageError(action.US_PreliminaryStatementPrintDateInfo, ValidationConstants.Statement.PSDNotAllowedForPayType1);

			action.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
			AssertNoMessageError(action.US_PreliminaryStatementPrintDateInfo, ValidationConstants.Statement.PSDNotAllowedForPayType1);

			action.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			AssertHasMessageError(action.US_PreliminaryStatementPrintDateInfo, ValidationConstants.Statement.PSDRequiredForNonPayType1);

			action.US_PreliminaryStatementPrintDate = ZDateTime.Invalid;
			AssertHasMessageError(action.US_PreliminaryStatementPrintDateInfo, ValidationConstants.Statement.PSDIsInvalid);

			action.US_PreliminaryStatementPrintDate = ZDateTime.BrettsBirthday;
			AssertNoMessageError(action.US_PreliminaryStatementPrintDateInfo, ValidationConstants.Statement.PSDNotAllowedForPayType1);
			AssertNoMessageError(action.US_PreliminaryStatementPrintDateInfo, ValidationConstants.Statement.PSDIsInvalid);

			action.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			action.US_PreliminaryStatementPrintDate = new ZDateTime(2006, 1, 8); // Saturday
			AssertHasMessageError(action.US_PreliminaryStatementPrintDateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHolidayOrWeekend);

			//holidays (2012, 01, 16));
			action.US_PreliminaryStatementPrintDate = new ZDateTime(2012, 01, 16);
			AssertHasMessageError(action.US_PreliminaryStatementPrintDateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHoliday);

			action.US_PreliminaryStatementPrintDate = new ZDateTime(2012, 01, 17);
			AssertNoMessageError(action.US_PreliminaryStatementPrintDateInfo, WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHoliday);

			action.US_PreliminaryStatementPrintDate = new ZDateTime(2009, 1, 1);
			AssertHasMessageError(action.US_PreliminaryStatementPrintDateInfo, ValidationConstants.Statement.PSDMustBeFutureDate);

			action.US_PreliminaryStatementPrintDate = new ZDateTime(2009, 06, 01);
			AssertNoMessageError(action.US_PreliminaryStatementPrintDateInfo, ValidationConstants.Statement.PSDMustBeFutureDate);

			action.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			action.US_PeriodicStatementMonth = "05";
			action.US_PreliminaryStatementPrintDate = new ZDateTime(2009, 05, 10);
			AssertNoMessageError(action.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PSDMustBeLessThanOrEqual11BusinessDay);

			action.US_PeriodicStatementMonth = "01";
			action.US_PreliminaryStatementPrintDate = new ZDateTime(2009, 12, 20);
			AssertNoMessageError(action.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PSDMustBeLessThanOrEqual11BusinessDay);

			action.US_PeriodicStatementMonth = "05";
			action.US_PreliminaryStatementPrintDate = ZDateTime.Today;
			AssertHasMessageError(action.US_PreliminaryStatementPrintDateInfo, PrelimStatementPrintDateValidator.PSDMustBeLessThanOrEqual11BusinessDay);
		}

		public void TestValidateUS_PeriodicStatementMonthMandatoryValidation()
		{
			var action = (StatementDeleteAndSendingAction)GetNewBusinessObject();
			action.US_SendMessage = true;
			action.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			action.US_PeriodicStatementMonth = "01";
			AssertHasMessageError(action.US_PeriodicStatementMonthInfo, PeriodicStatementMMValidator.MonthNotRequiredForSelectedPaymentType);

			action.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporterWithSuffixes;
			AssertNoMessageError(action.US_PeriodicStatementMonthInfo, PeriodicStatementMMValidator.MonthNotRequiredForSelectedPaymentType);

			action.US_PeriodicStatementMonth = "";
			AssertHasMessageError(action.US_PeriodicStatementMonthInfo, PeriodicStatementMMValidator.MonthRequiredForPeriodicPaymentType);

			action.US_PeriodicStatementMonth = "02";
			AssertNoMessageError(action.US_PeriodicStatementMonthInfo, PeriodicStatementMMValidator.MonthRequiredForPeriodicPaymentType);
		}

		public void TestProperties()
		{
			var statementLine = (CusStatementLine)Line;
			statementLine.StatementHeader.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			declaration.US_PeriodicStatementMM = "10";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "123008";
			Factory.Save();

			AssertEquals(entry.Declaration, statementLine.Declaration);

			var action = new StatementDeleteAndSendingAction(Line);

			AssertEquals("Month should have been defaulted from declaration", "10", action.US_PeriodicStatementMonth);

			AssertEquals("US_SendMessage_getter", typeof(ZBool), action.US_SendMessage.GetType());
			AssertEquals("US_PaymentType_getter", typeof(ZString), action.US_PaymentType.GetType());
			AssertEquals("IsSinglePayment_getter", typeof(bool), action.IsSinglePayment.GetType());
			AssertEquals("PaymentTypeList_getter", typeof(PaymentTypeList), action.PaymentTypeList.GetType());
			AssertEquals("US_EntryNumber_getter", typeof(ZString), action.US_EntryNumber.GetType());
			AssertEquals("US_PreliminaryStatementPrintDate_getter", typeof(ZDateTime), action.US_PreliminaryStatementPrintDate.GetType());
			AssertEquals("US_EntryFilerCode_getter", typeof(ZString), action.US_EntryFilerCode.GetType());
			AssertEquals("US_ClientBranchDesignation_getter", typeof(ZString), action.US_ClientBranchDesignation.GetType());
			AssertEquals("US_PeriodicStatementMonth_getter", typeof(ZString), action.US_PeriodicStatementMonth.GetType());
			AssertEquals("US_StatementProcessingPort_getter", typeof(ZString), action.PortOfEntry.GetType());
			AssertEquals("MonthList_getter", typeof(MonthList), action.MonthList.GetType());

			action.US_SendMessage = ZBool.True;
			action.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;
			action.US_PreliminaryStatementPrintDate = ZDateTime.Today;
			action.US_ClientBranchDesignation = "XX";
			action.US_PeriodicStatementMonth = MonthList.Codes._09;
			AssertEquals("US_SendMessage_setter", ZBool.True, action.US_SendMessage);
			AssertEquals("US_PaymentType_setter", PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter, action.US_PaymentType);
			AssertEquals("US_PreliminaryStatementPrintDate_setter", ZDateTime.Today, action.US_PreliminaryStatementPrintDate);
			AssertEquals("US_ClientBranchDesignation_setter", "XX", action.US_ClientBranchDesignation);
			AssertEquals("US_PeriodicStatementMonth_setter", MonthList.Codes._09, action.US_PeriodicStatementMonth);
		}

		public void TestValidateUS_SendMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.ImportEntryNumber = "1";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			var actions = new StatementDeleteAndSendingActionCollection(declaration);
			AssertEquals(1, actions.Count);
			actions[0].US_SendMessage = true;
			AssertNoMessageError(actions[0].US_SendMessageInfo, ValidationConstants.Statement.ShouldNotSendWhenWaitingForSUResponse);

			CreateSTUMessage(declaration, true);
			actions = new StatementDeleteAndSendingActionCollection(declaration);
			AssertEquals(1, actions.Count);
			actions[0].US_SendMessage = true;
			AssertHasMessageError(actions[0].US_SendMessageInfo, ValidationConstants.Statement.ShouldNotSendWhenWaitingForSUResponse);

			CreateSTUMessage(declaration, false);
			actions = new StatementDeleteAndSendingActionCollection(declaration);
			AssertEquals(1, actions.Count);
			actions[0].US_SendMessage = true;
			AssertNoMessageError(actions[0].US_SendMessageInfo, ValidationConstants.Statement.ShouldNotSendWhenWaitingForSUResponse);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			_ = new StatementDeleteAndSendingActionCollection(Header);
			return new StatementDeleteAndSendingAction(Line);
		}

		void CreateSTUMessage(JobDeclaration declaration, bool isTransmit)
		{
			var message = Factory.NewWithValidTestData<MQEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.USCustomsImport;
			message.EM_MessageType = isTransmit ? ACEApplicationIdentifierCodeList.Codes.StatementUpdate : ACEApplicationIdentifierCodeList.Codes.StatementUpdateResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			message.EM_ReceiveTransmit = isTransmit ? EDIMessage.Direction.Transmit : EDIMessage.Direction.Receive;
			message.EM_Status = isTransmit ? EDIMessage.Status.Sent : EDIMessage.Status.Received;
			message.EM_MessageNum = "HYEDUSCMT_000001";
			declaration.Messages.Add(message);
		}

		CusStatementHeader header;
		CusStatementHeader Header => header ?? (header = Factory.New<CusStatementHeader>());

		CusStatementLine line;
		IStatementDeleteTransaction Line
		{
			get
			{
				if (line == null)
				{
					if (header == null)
					{
						header = Factory.New<CusStatementHeader>();
					}
					line = Header.StatementLines.AddNew();

					line.B3_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
					line.B3_EntryFilerCode = "XJ5";
					line.B3_EntryNum = "123008";
				}
				return line;
			}
		}
	}
}
