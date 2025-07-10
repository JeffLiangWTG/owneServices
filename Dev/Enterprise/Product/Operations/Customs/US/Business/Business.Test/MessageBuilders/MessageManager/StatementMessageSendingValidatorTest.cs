using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class StatementMessageSendingValidatorTest : TestCaseWithFactory
	{
		public void TestGetNotificationsForPayment()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeaderWrapper.New(importer).ZO_PayMethod = ACHPaymentTypeList.Codes.ImporterCheck;
			Factory.Save();

			CusStatementHeader header = Factory.New<CusStatementHeader>();
			header.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
			header.B2_OH_Importer = importer.PK;

			MessageSendingNotificationCollection notifications = Validator.GetNotificationsForPayment(header);
			Assert(notifications.ContainsError(StatementMessageSendingValidator.ThisMessageWillFail + StatementMessageSendingValidator.AlreadyPaid));

			header.B2_Status = StatementHeaderStatusList.Codes.Final;
			notifications = Validator.GetNotificationsForPayment(header);
			Assert(notifications.ContainsError(StatementMessageSendingValidator.ThisMessageWillFail + StatementMessageSendingValidator.AlreadyPaid));

			header.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationDeleted;
			header.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			header.B2_PaymentStatus = PaymentStatusList.Codes.PaymentInProgress;
			notifications = Validator.GetNotificationsForPayment(header);
			Assert(notifications.ContainsWarning(StatementMessageSendingValidator.ThisMessageWillFail + StatementMessageSendingValidator.PaymentInProgress));

			header.B2_Status = StatementHeaderStatusList.Codes.Deleted;
			notifications = Validator.GetNotificationsForPayment(header);
			Assert(notifications.ContainsError(StatementMessageSendingValidator.ThisMessageWillFail + StatementMessageSendingValidator.Deleted));

			header.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			CusStatementLine line = header.StatementLines.AddNew();
			line.B3_Status = StatementLineStatusList.Codes.DeletionPending;
			AssertEquals(true, header.StatementLines.HasLinesWithDeletionPending);
			notifications = Validator.GetNotificationsForPayment(header);
			Assert(notifications.ContainsWarning(StatementMessageSendingValidator.PaymentWarning));

			header.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;
			notifications = Validator.GetNotificationsForPayment(header);
			Assert(notifications.ContainsWarning(StatementMessageSendingValidator.ImporterCheckIsADefaultOption));

			header.B2_CheckNo = "31123";
			notifications = Validator.GetNotificationsForPayment(header);
			Assert(notifications.ContainsError(StatementMessageSendingValidator.CheckNoEntered));
		}

		public void TestGetNotificationsForResettingPaymentStatus()
		{
			var header = Factory.New<CusStatementHeader>();
			header.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;

			var notifications = Validator.GetNotificationsForResettingPaymentStatus(header);
			Assert(notifications.ContainsWarning(StatementMessageSendingValidator.AlreadyPaid.Substring(0, 1).ToUpper() + StatementMessageSendingValidator.AlreadyPaid.Substring(1)));

			header.B2_Status = StatementHeaderStatusList.Codes.Final;
			notifications = Validator.GetNotificationsForResettingPaymentStatus(header);
			Assert(notifications.ContainsWarning(StatementMessageSendingValidator.Finalised.Substring(0, 1).ToUpper() + StatementMessageSendingValidator.Finalised.Substring(1)));

			header.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationDeleted;
			header.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			header.B2_PaymentStatus = PaymentStatusList.Codes.PaymentInProgress;
			notifications = Validator.GetNotificationsForResettingPaymentStatus(header);
			Assert(notifications.ContainsWarning(StatementMessageSendingValidator.PaymentInProgress.Substring(0, 1).ToUpper() + StatementMessageSendingValidator.PaymentInProgress.Substring(1)));

			header.B2_PaymentStatus = "";
			AssertNoExceptionThrown(() => _ = Validator.GetNotificationsForResettingPaymentStatus(header));
		}

		public void TestGetNotificationsForStatementDeleteAddForStatement()
		{
			CusStatementHeader header = Factory.New<CusStatementHeader>();
			header.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;

			MessageSendingNotificationCollection notifications = Validator.GetNotificationsForStatementDeleteAdd(header);
			Assert(notifications.ContainsError(StatementMessageSendingValidator.ThisMessageWillFail + StatementMessageSendingValidator.AlreadyPaid));

			header.B2_Status = StatementHeaderStatusList.Codes.Final;
			notifications = Validator.GetNotificationsForStatementDeleteAdd(header);
			Assert(notifications.ContainsError(StatementMessageSendingValidator.ThisMessageWillFail + StatementMessageSendingValidator.AlreadyPaid));

			header.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationDeleted;
			header.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			header.B2_PaymentStatus = PaymentStatusList.Codes.PaymentInProgress;
			notifications = Validator.GetNotificationsForStatementDeleteAdd(header);
			Assert(notifications.ContainsWarning(StatementMessageSendingValidator.ThisMessageWillFail + StatementMessageSendingValidator.PaymentInProgress));

			header.B2_Status = StatementHeaderStatusList.Codes.Deleted;
			notifications = Validator.GetNotificationsForStatementDeleteAdd(header);
			Assert(notifications.ContainsError(StatementMessageSendingValidator.ThisMessageWillFail + StatementMessageSendingValidator.Deleted));
		}

		public void TestPerformUpdateStatementMessageWithErrors()
		{
			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "2710288B73";
			statement.B2_StatementAmount = 200m;
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;
			statement.B2_EntryFilerCode = "XJ5";
			statement.B2_Status = StatementHeaderStatusList.Codes.Final;
			statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;

			CusStatementLine line = statement.StatementLines.AddNew();
			line.B3_Status = StatementLineStatusList.Codes.Active;

			Env.Security.USCustomsImportStatementSendUpdateMsgWithErrors.IsAllowed = true;
			MessageSendingNotificationCollection notifications = Validator.GetNotificationsForStatementDeleteAdd(statement);
			Assert(notifications.ContainsWarning(StatementMessageSendingValidator.AlreadyPaidAndFinalised));

			Env.Security.USCustomsImportStatementSendUpdateMsgWithErrors.IsAllowed = false;
			notifications = Validator.GetNotificationsForStatementDeleteAdd(statement);
			Assert(notifications.ContainsError(StatementMessageSendingValidator.ThisMessageWillFail + StatementMessageSendingValidator.AlreadyPaid));
		}

		public void TestPerformUpdateStatementMessageFromDeclaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.AllocateEntryNumber("TEST");

			Factory.Save();

			var statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_PaymentParty = PaymentPartyList.Codes.Importer;
			statementHeader.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;
			statementHeader.B2_StatementNumber = "2709283772";
			statementHeader.B2_Status = StatementHeaderStatusList.Codes.Final;
			statementHeader.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;

			var statementLine = statementHeader.StatementLines.AddNew();
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_EntryNum = "TEST";
			Factory.Save();

			AssertNotNull(declaration.RelatedStatement);
			AssertEquals(statementHeader.PK, declaration.RelatedStatement.PK);

			Env.Security.USCustomsImportStatementSendUpdateMsgWithErrors.IsAllowed = true;
			var notifications = Validator.GetNotificationsForStatementDeleteAdd(declaration);
			Assert(notifications.ContainsWarning(StatementMessageSendingValidator.AlreadyPaidAndFinalised));

			Env.Security.USCustomsImportStatementSendUpdateMsgWithErrors.IsAllowed = false;
			notifications = Validator.GetNotificationsForStatementDeleteAdd(declaration);
			Assert(notifications.ContainsError(StatementMessageSendingValidator.ThisMessageWillFail + StatementMessageSendingValidator.AlreadyPaid));
		}

		public void TestGetNotificationsForStatementDeleteAddForDeclaration()
		{
			var notifications = Validator.GetNotificationsForStatementDeleteAdd(Declaration);
			var errorText = string.Format(StatementMessageSendingValidator.Entry7501NotLodged, "7501");
			Assert(!notifications.ContainsWarning(errorText));

			Declaration.DecEntryNumber = "123456";
			notifications = Validator.GetNotificationsForStatementDeleteAdd(Declaration);
			Assert(notifications.ContainsWarning(errorText));

			var ensEntry = Declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			notifications = Validator.GetNotificationsForStatementDeleteAdd(Declaration);
			Assert(notifications.ContainsWarning(errorText));

			ensEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			notifications = Validator.GetNotificationsForStatementDeleteAdd(Declaration);
			Assert(!notifications.ContainsWarning(errorText));

			Declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			notifications = Validator.GetNotificationsForStatementDeleteAdd(Declaration);
			Assert(notifications.ContainsError(StatementMessageSendingValidator.EntriesNotMeetConditions));

			Declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			notifications = Validator.GetNotificationsForStatementDeleteAdd(Declaration);
			Assert(!notifications.ContainsError(StatementMessageSendingValidator.EntriesNotMeetConditions));

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.DecEntryNumber = ZString.Empty;
			notifications = Validator.GetNotificationsForStatementDeleteAdd(declaration2);
			Assert(notifications.ContainsError(StatementMessageSendingValidator.DecEntryNumberIsRequired));

			declaration2.DecEntryNumber = "123456";
			notifications = Validator.GetNotificationsForStatementDeleteAdd(declaration2);
			Assert(!notifications.ContainsError(StatementMessageSendingValidator.DecEntryNumberIsRequired));

			var ensEntry2 = declaration2.CustomsEntryHeaders.AddNew();
			ensEntry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ensEntry2.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			declaration2.DecEntryNumber = ZString.Empty;
			notifications = Validator.GetNotificationsForStatementDeleteAdd(declaration2);
			Assert(!notifications.ContainsError(StatementMessageSendingValidator.DecEntryNumberIsRequired));
		}

		public void TestGetNotificationsForStatementDeleteAddForRecon()
		{
			var notifications = Validator.GetNotificationsForStatementDeleteAdd(ReconDeclaration);
			var errorText = string.Format(StatementMessageSendingValidator.Entry7501NotLodged, "Reconciliation");
			Assert(notifications.ContainsError(errorText));

			ReconDeclaration.ReconEntry.CH_Status = ReconMessageStatusList.Codes.ClearReconOriginal;
			notifications = Validator.GetNotificationsForStatementDeleteAdd(ReconDeclaration);
			Assert(!notifications.ContainsError(errorText));

			ReconDeclaration.ReconEntry.CH_Status = ReconMessageStatusList.Codes.ClearReconDelete;
			notifications = Validator.GetNotificationsForStatementDeleteAdd(ReconDeclaration);
			Assert(notifications.ContainsError(errorText));

			ReconDeclaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			notifications = Validator.GetNotificationsForStatementDeleteAdd(ReconDeclaration);
			Assert(notifications.ContainsError(StatementMessageSendingValidator.EntriesNotMeetConditions));

			ReconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			notifications = Validator.GetNotificationsForStatementDeleteAdd(ReconDeclaration);
			Assert(!notifications.ContainsError(StatementMessageSendingValidator.EntriesNotMeetConditions));
		}

		public void TestGetNotificationsForDeletePaymentAuthorization()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var header = Factory.New<CusStatementHeader>();
			header.B2_OH_Importer = importer.PK;

			var notifications = Validator.GetNotificationsForDeletePaymentAuthorization(header);
			Assert(notifications.ContainsWarning(StatementMessageSendingValidator.NotPaidYet));

			header.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
			header.B2_PaymentAuthorizationDate = ZDateTime.Empty;
			notifications = Validator.GetNotificationsForDeletePaymentAuthorization(header);
			Assert(!notifications.ContainsWarning(StatementMessageSendingValidator.NotPaidYet));
			Assert(notifications.ContainsWarning(StatementMessageSendingValidator.NegationDate));

			header.B2_PaymentAuthorizationDate = ZDateTime.Today;
			notifications = Validator.GetNotificationsForDeletePaymentAuthorization(header);
			Assert(!notifications.ContainsWarning(StatementMessageSendingValidator.NegationDate));
		}

		StatementMessageSendingValidator validator;
		StatementMessageSendingValidator Validator => validator ?? (validator = new StatementMessageSendingValidator());

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				}
				return declaration;
			}
		}

		ReconDeclaration reconDeclaration;
		ReconDeclaration ReconDeclaration
		{
			get
			{
				if (reconDeclaration == null)
				{
					Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetEntryFilerCode("XJ5");
					var declaration = Factory.New<JobDeclaration>();
					reconDeclaration = new ReconDeclaration(declaration);
					reconDeclaration.OriginalEntries.AddNew();
					JobComInvoiceHeader invoice = reconDeclaration.Invoices.AddNew();
					invoice.US_CH_ReconEntry = reconDeclaration.OriginalEntries[0].CH_PK;
					reconDeclaration.InvoiceLines.AddNew();

					Factory.Save();
				}
				return reconDeclaration;
			}
		}
	}
}
