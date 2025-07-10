using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class StatementHeaderAccountingIntegrationTest : TestCaseWithFactory
	{
		public void TestLogForPaymentStatusChanged_NormalDeclaration()
		{
			var testHelper = new Customs.Business.Testing.InvoicingTestHelper(Factory);

			var declaration = CreateDeclaration("1", testHelper.Importer);
			var statement = CreateStatement("123456", PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate, declaration.ImportEntryNumber, 20m, 5m);
			statement.B2_IsMonthlyStatement = false;

			AssertLogOfAuthorised("Precondition", statement, true, false);
			AssertLogOfAuthorised("Precondition", declaration, true, false);

			statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;

			AssertLogOfAuthorised("Should add an valid log with ATH on statement.", statement, false, false);
			AssertLogOfAuthorised("Should add an valid log with ATH on declaration.", declaration, false, false);

			statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationDeleted;

			AssertLogOfAuthorised("Should cancel all valid logs with ATH on statement.", statement, false, true);
			AssertLogOfAuthorised("Should cancel all valid logs with ATH on declaration.", declaration, false, true);
		}

		public void TestLogForPaymentStatusChanged_ReconDeclaration()
		{
			var testHelper = new Customs.Business.Testing.InvoicingTestHelper(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;

			var recon = new ReconDeclaration(declaration)
			{
				JE_OH_Importer = testHelper.Importer.PK,
				JE_ApplicationCode = JobApplicationCodeList.Codes.ACS,
				US_EntryFilerCode = "XJ5"
			};

			recon.ReconEntry.GetEntry().EntryNumber = "1234";
			Factory.Save();

			var statement = CreateStatement("123456", PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate, recon.ReconEntryNumber, 20m, 5m);
			statement.B2_IsMonthlyStatement = false;

			AssertLogOfAuthorised("Precondition", statement, true, false);
			AssertLogOfAuthorised("Precondition", declaration, true, false);

			statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;

			AssertLogOfAuthorised("Should add an valid log with ATH on statement.", statement, false, false);
			AssertLogOfAuthorised("Should add an valid log with ATH on the wrapped declaration of recon.", declaration, false, false);

			statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationDeleted;

			AssertLogOfAuthorised("Should cancel all valid logs with ATH on statement.", statement, false, true);
			AssertLogOfAuthorised("Should cancel all valid logs with ATH the wrapped declaration of recon.", declaration, false, true);
		}

		void AssertLogOfAuthorised(string message, IStmALogParent logParent, bool isNull, bool isCancelled)
		{
			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.AuthorisedCode);
			query.AddToFilter(StmALogSchema.SL_Reference, CusStatementHeader.PaymentAuthorizationAcceptedReference);

			var log = logParent.Logs.Find(query)
				.OrderByDescending(c => c.SL_EventTime)
				.FirstOrDefault();

			AssertEquals(message, isNull, log == null);

			if (log != null)
			{
				AssertEquals(message, isCancelled, log.IsCancelled);
			}
		}

		public void TestUserInvokedAccIntegrationWhenNothingToPay()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "S1";
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			statement.MakePayment = true;

			var line = statement.StatementLines.AddNew();
			line.B3_CustomsFeesTotal = 0;

			var result = statement.GetErrorNotificationsBeforePerformingAccIntegration();
			AssertContains(CusStatementHeader.NothingToPay, result);
		}

		public void TestIControllerIDProviderMembers()
		{
			var statement = Factory.New<CusStatementHeader>();
			IControllerIDProvider provider = statement;
			AssertEquals("ControllerID", ControllerIDs.Customs.CustomsStatement, provider.ControllerID);
			AssertEquals("BusinessObjectPK", statement.PK.ToGuid(), provider.BusinessObjectPK);
		}

		public void TestIsPMSType()
		{
			var statementheader = Factory.New<CusStatementHeader>();
			statementheader.B2_Status = StatementHeaderStatusList.Codes.Final;
			statementheader.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			AssertEquals("A payment type is 6,7 and 8", true, statementheader.IsPMSType);
		}

		public void TestLoadForRelevantCompany()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = "CA";
			company.GC_Code = "~CA";

			GlbCompany.CurrentCompany.SetCountry("CA");
			var statementCA = Factory.New<Integration.Customs.CA.ICusStatementHeader>();
			statementCA.B2_GC = company.PK;

			var statementLine = Factory.New<Integration.Customs.CA.ICusStatementLine>();
			statementLine.B3_B2 = statementCA.PK;
			statementLine.B3_EntryNum = "1";
			statementLine.B3_EntryFilerCode = "XJ5";

			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry("US");
			var statementUS = Factory.New<CusStatementHeader>();

			var statementLineUS = statementUS.StatementLines.AddNew();
			statementLineUS.B3_EntryFilerCode = "XJ5";
			statementLineUS.B3_EntryNum = "1";
			statementLineUS.B3_Status = StatementLineStatusList.Codes.Active;
			Factory.Save();

			AssertEquals(statementUS, new CusStatementHeader.Loader(Factory).Load("XJ5", "1", GlbCompany.CurrentCompany.PK));
		}

		public void TestGetWarningNotificationsBeforePerformingAccIntegration()
		{
			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			statement.MakePayment = true;
			AssertContains(CusStatementHeader.PrelimStatus, statement.GetWarningNotificationsBeforePerformingAccIntegration());

			statement.MakePayment = false;
			statement.PostARInvoices = true;
			AssertNotContains(CusStatementHeader.PrelimStatus, statement.GetWarningNotificationsBeforePerformingAccIntegration());

			AccountingIntegrationOptions options = new AccountingIntegrationOptions();
			options.EnableAccountingIntegration = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, options);

			USCustomsDataRegistry.Instance.MonthlyStatementPaymentPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, MonthlyStatementPaymentPostingOptionList.Codes._2);

			statement.B2_Status = StatementHeaderStatusList.Codes.Final;
			statement.B2_StatementNumber = "1";
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			AssertEquals(true, statement.IsPeriodicDailyStatement);
			statement.MakePayment = true;
			AssertContains(CusStatementHeader.OnePaymentPerMonthlyStatement, statement.GetWarningNotificationsBeforePerformingAccIntegration());

			USCustomsDataRegistry.Instance.MonthlyStatementPaymentPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, MonthlyStatementPaymentPostingOptionList.Codes._1);
			AssertNotContains(CusStatementHeader.OnePaymentPerMonthlyStatement, statement.GetWarningNotificationsBeforePerformingAccIntegration());
		}

		public void TestDoNotPopupPaidedErrorWhenEstimateDateEntered()
		{
			var testHelper = new Customs.Business.Testing.InvoicingTestHelper(Factory);
			var declaration = CreateDeclaration("1", testHelper.Importer);

			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;

			var statement = CreateStatement("123456", PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate, declaration.ImportEntryNumber, 20m, 5m);
			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			statement.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			statement.B2_AccountNo = "12345";
			declaration.US_PaymentDate = ZDateTime.Today;
			Factory.Save();

			statement.ResetPaymentStatus();

			AssertEquals(ZString.Empty, statement.B2_PaymentStatus);
			AssertEquals(ZString.Empty, statement.B2_PaymentParty);
			AssertEquals(ZString.Empty, statement.B2_AccountNo);
			AssertEquals(ZDateTime.Empty, statement.B2_PaymentAuthorizationDate);

			AssertEquals(false, statement.IsPaid);

			var task = statement.WorkflowItems.Triggers.AddNew();
			task.P9_ScheduledDateForBinding = DateTime.Today.AddDays(-1);
			task.P9_Description = "EnterEstimatedDate";
			task.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
			task.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			task.TriggerConditions.TriggerConditionValue = "Payment Authorization Accepted";
			task.P9_ScheduledDateForBinding = DateTime.Today.AddDays(1);

			var notification = task.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_EmailAddr = "dummy1@where.com";

			Factory.Save();

			AssertEquals(false, statement.IsPaid);
			var query = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, CusStatementHeader.PaymentAuthorizationAcceptedReference);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.Authorised.Code);
			var logs = statement.Logs.Find(query);

			foreach (var log in logs)
			{
				AssertEquals(log.SL_IsEstimate, true);
			}
		}

		[TestDate(2021, 06, 06)]
		public void TestPaymentDateCalculated()
		{
			AssertEquals("0 - Sunday", DayOfWeek.Sunday, ZDateTime.Today.DayOfWeek);
			var testHelper = new Customs.Business.Testing.InvoicingTestHelper(Factory);
			var declaration = CreateDeclaration("1", testHelper.Importer);

			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;

			var statement = CreateStatement("123456", PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, declaration.ImportEntryNumber, 20m, 5m);
			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
			statement.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			statement.B2_AccountNo = "12345";
			statement.B2_PaymentAuthorizationDate = ZDateTime.Today.AddDays(-4);
			declaration.US_PaymentDate = ZDateTime.Today;
			Factory.Save();

			AssertEquals(ZDateTime.Today.AddDays(-4), statement.B2_PaymentAuthorizationDate);
			AssertEquals("Friday", ZDateTime.Today.AddDays(-2), statement.PaymentDateCalculated);
		}

		public void TestResetPaymentStatus()
		{
			var testHelper = new Customs.Business.Testing.InvoicingTestHelper(Factory);
			var declaration = CreateDeclaration("1", testHelper.Importer);

			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;

			var statement = CreateStatement("123456", PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, declaration.ImportEntryNumber, 20m, 5m);
			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
			statement.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			statement.B2_AccountNo = "12345";
			statement.B2_PaymentAuthorizationDate = ZDateTime.Today;
			declaration.US_PaymentDate = ZDateTime.Today;
			Factory.Save();

			AssertEquals(true, statement.IsPaid);

			statement.ResetPaymentStatus();

			AssertEquals(ZString.Empty, statement.B2_PaymentStatus);
			AssertEquals(ZString.Empty, statement.B2_PaymentParty);
			AssertEquals(ZString.Empty, statement.B2_AccountNo);
			AssertEquals(ZDateTime.Empty, statement.B2_PaymentAuthorizationDate);
			AssertEquals(ZDateTime.Empty, declaration.US_PaymentDate);
			AssertEquals(false, statement.IsPaid);

			var logAdded = statement.Logs.LogsNotInDB[0];
			AssertEquals(Events.AuthorisationWithdrawnCode, logAdded.SL_SE_NKEvent);
			AssertEquals("Payment Status reset. Previous Payment Authorized on " + ZDateTime.Today.ToString("dd/MMM/yy"), logAdded.SL_Reference);

			var query = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, CusStatementHeader.PaymentAuthorizationAcceptedReference);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.Authorised.Code);
			var logs = statement.Logs.Find(query);
			foreach (var log in logs)
			{
				AssertEquals(log.IsCancelled, true);
			}
		}

		public void TestGetErrorNotificationsBeforePerformingAccIntegration()
		{
			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			statement.B2_Status = StatementHeaderStatusList.Codes.Deleted;
			AssertContains(CusStatementHeader.StatementIsDeleted, statement.GetErrorNotificationsBeforePerformingAccIntegration());

			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			AssertNotContains(CusStatementHeader.StatementIsDeleted, statement.GetErrorNotificationsBeforePerformingAccIntegration());
			AssertContains(CusStatementHeader.NoAccIntegrationOptionIsSelected, statement.GetErrorNotificationsBeforePerformingAccIntegration());

			statement.PostAPInvoices = true;
			AssertNotContains(CusStatementHeader.NoAccIntegrationOptionIsSelected, statement.GetErrorNotificationsBeforePerformingAccIntegration());

			statement.MakePayment = true;
			statement.B2_PaymentParty = PaymentPartyList.Codes.Importer;
			AssertContains(CusStatementHeader.NotPaidByBroker, statement.GetErrorNotificationsBeforePerformingAccIntegration());

			statement.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			AssertNotContains(CusStatementHeader.NotPaidByBroker, statement.GetErrorNotificationsBeforePerformingAccIntegration());
		}

		public void TestIntegrateWithAccountingAndGetResultEvenWhenRegistryIsOff()
		{
			var testHelper = SetRegistryItems(false, false, false);//not enabled

			JobDeclaration declaration = CreateDeclaration("1", testHelper.Importer);

			CusStatementHeader statement = CreateStatement("123456", PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, "1", 10m, 0m);
			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;

			//should attempt to autorate and make a payment even when preliminary.
			statement.PostAPInvoices = true;
			statement.MakePayment = true;
			AutoBillingResult result = statement.PerformAccIntegration();

			AssertEquals("Should not be successful", false, result.WasSuccessful);
			AssertContains("There is no bank account against which payment can be made for", result.Message);
		}

		public void TestIntegrateWithAccountingAndGetResultEndToEnd()
		{
			var testHelper = SetRegistryItems(true, false, false);

			JobDeclaration declaration = CreateDeclaration("1", testHelper.Importer);

			CusStatementHeader statement = CreateStatement("123456", PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, "1", 10m, 0m);
			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			statement.PostARInvoices = true;
			AutoBillingResult result = statement.PerformAccIntegration();
			AssertEquals(true, result.WasSuccessful);

			JobHeader job = Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, declaration.CompanyPK));
			AssertNotNull(job);

			JobCharge[] charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals("one charge should have been autorated", 1, charges.Length);
			AssertEquals("Not Posted for AP", false, charges[0].IsCostPosted);
			AssertEquals("Posted for AR", true, charges[0].IsRevenuePosted);
			AssertEquals("AAA is mapped to a right charge code", testHelper.DisbursementChargeCode.PK, charges[0].JR_AC);
			AssertEquals("Amount", 10m, charges[0].JR_LocalCostAmt);

			statement.StatementLines[0].Charges[0].B4_ChargeAmount = 20m;

			//post AP
			statement.PostAPInvoices = true;
			result = statement.PerformAccIntegration();
			AssertEquals(true, result.WasSuccessful);
			charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals("two charge should have been autorated", 2, charges.Length);

			var charge1 = charges[0];
			var charge2 = charges[1];
			AssertEquals("Posted for AP", true, charge1.IsCostPosted);
			AssertEquals("AP Amount", -10m, charge1.APLine.AL_LineAmount);

			AssertEquals("Posted for AP", true, charge2.IsCostPosted);
			AssertEquals("AP Amount", -10m, charge2.APLine.AL_LineAmount);
		}

		public void TestIntegrateWithAccountingAndGetResultEndToEndForPayment()
		{
			var testHelper = SetRegistryItems(true, false, false);

			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = Core.Constants.Groups.PostMastersGroupPK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);

			var brokerAccounts = new BrokersAccountCollection();
			var account = brokerAccounts.AddNew();
			account.PayerUnitNumber = "123456";
			account.BankAccount = bankAccount.PK;
			Enterprise.Customs.US.DataRegistry.Business.USCustomsDataRegistry.Instance.BrokersAccounts.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, brokerAccounts);

			var declaration = CreateDeclaration("1", testHelper.Importer);

			var statement = CreateStatement("123456", PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, "1", 10m, 0m);
			statement.B2_Status = StatementHeaderStatusList.Codes.Final;
			statement.B2_AccountNo = "123456";
			statement.PostAPInvoices = true;
			statement.MakePayment = true;
			var result = statement.PerformAccIntegration();
			Factory.Save();

			var job = Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, declaration.CompanyPK));
			AssertNotNull(job);

			var charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals("one charge should have been autorated", 1, charges.Length);
			AssertEquals("Posted for AP", true, charges[0].IsCostPosted);
			AssertEquals("AP Amount", -10m, charges[0].APLine.AL_LineAmount);

			var payment = Factory.LoadTop1<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, Enterprise.ZArchitecture.Core.TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, job.JH_GC));
			AssertNotNull(payment);
			AssertEquals(10m, payment.AH_OSTotal);
		}

		public void TestWhenFinalDailyStatementIsProcessedWhenAPAreNotPostedYet()
		{
			var testHelper = SetRegistryItems(true, false, false);

			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = Core.Constants.Groups.PostMastersGroupPK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);

			var brokerAccounts = new BrokersAccountCollection();
			var account = brokerAccounts.AddNew();
			account.PayerUnitNumber = "123456";
			account.BankAccount = bankAccount.PK;
			Enterprise.Customs.US.DataRegistry.Business.USCustomsDataRegistry.Instance.BrokersAccounts.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, brokerAccounts);

			var declaration = CreateDeclaration("1", testHelper.Importer);

			var statement = CreateStatement("123456", PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, "1", 10m, 0m);
			statement.B2_Status = StatementHeaderStatusList.Codes.Final;
			statement.B2_AccountNo = "123456";
			Factory.Save();

			var job = Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, declaration.CompanyPK));
			AssertNotNull(job);

			var charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals("one charge should have been autorated", 1, charges.Length);
			AssertEquals("Posted for AP", true, charges[0].IsCostPosted);
			AssertEquals("AP Amount", -10m, charges[0].APLine.AL_LineAmount);

			var payment = Factory.LoadTop1<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, Enterprise.ZArchitecture.Core.TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, job.JH_GC));
			AssertNotNull(payment);
			AssertEquals(10m, payment.AH_OSTotal);
		}

		public void TestIntegrationDoesNotHappenForImporterPaidStatement()
		{
			var testHelper = SetRegistryItems(true, false, false);

			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = Core.Constants.Groups.PostMastersGroupPK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);

			JobDeclaration declaration = CreateDeclaration("1", testHelper.Importer);

			CusStatementHeader statement = CreateStatement("123456", PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter, "1", 10m, 0m);
			statement.B2_Status = StatementHeaderStatusList.Codes.Final;
			Factory.Save();

			JobHeader job = Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, declaration.CompanyPK));
			AssertNull(job);
		}

		public void TestDailyStatementIsProcessed()
		{
			var testHelper = SetRegistryItems(true, false, false);

			JobDeclaration declaration = CreateDeclaration("1", testHelper.Importer);

			CusStatementHeader statement = CreateStatement("123456", PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, "1", 10m, 0m);
			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			Factory.Save();

			JobHeader job = Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, declaration.CompanyPK));
			AssertNotNull(job);

			JobCharge[] charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals("one charge should have been autorated", 1, charges.Length);
			AssertEquals("But not posted for AP", false, charges[0].IsCostPosted);
			AssertEquals("Not posted for AR", false, charges[0].IsRevenuePosted);
			AssertEquals("AAA is mapped to a right charge code", testHelper.DisbursementChargeCode.PK, charges[0].JR_AC);
			AssertEquals("Amount", 10m, charges[0].JR_LocalCostAmt);

			statement.StatementLines[0].Charges[0].B4_ChargeAmount = 20m;
			statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
			Factory.Save();

			var logAdded = statement.Logs.MostRecentLogByEventTime(Events.Authorised);
			AssertEquals("Payment Authorization Accepted", logAdded.SL_Reference);

			charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals("one charge should have been autorated", 1, charges.Length);
			AssertEquals("But not posted for AP", false, charges[0].IsCostPosted);
			AssertEquals("Amount is updated", 20m, charges[0].JR_LocalCostAmt);
			AssertEquals("AP Amount", 20m, charges[0].APLine.AL_LineAmount);
		}

		public void TestAutoBillingIsNotAttemptedInAWrongCompanyWhenFilerCodeIsShared()
		{
			var testHelper = SetRegistryItems(true, false, false);

			var declaration = CreateDeclaration("1", testHelper.Importer);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = "US";
			company.GC_Code = "~US";

			var branch = company.Branches.AddNew();
			branch.GB_Code = "~US";
			Factory.Save();

			using (new TemporaryUserContext() { BranchPK = branch.PK.ToGuid() }.Set())
			{
				var statement = CreateStatement("123456", PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, "1", 10m, 0m);
				statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
				Factory.Save();

				var jobs = new BusinessObjectFactory().Load<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, declaration.CompanyPK));
				AssertEquals("Should not have created another job in a wrong company", 0, jobs.Length);
			}
		}

		public void TestAutoBillingIsNotAttemptedInAWrongCompanyWhenFilerCodeIsShared_UserInvoked()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = "US";
			company.GC_Code = "~US";

			var branch = company.Branches.AddNew();
			branch.GB_Code = "~US";
			Factory.Save();

			var testHelper = SetRegistryItems(true, false, false);

			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = Core.Constants.Groups.PostMastersGroupPK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);

			var brokerAccounts = new BrokersAccountCollection();
			var account = brokerAccounts.AddNew();
			account.PayerUnitNumber = "123456";
			account.BankAccount = bankAccount.PK;
			Enterprise.Customs.US.DataRegistry.Business.USCustomsDataRegistry.Instance.BrokersAccounts.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, brokerAccounts);

			var declaration = CreateDeclaration("1", testHelper.Importer);

			var statement = CreateStatement("123456", PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, "1", 10m, 0m);
			statement.B2_Status = StatementHeaderStatusList.Codes.Final;
			statement.B2_AccountNo = "123456";
			statement.PostAPInvoices = true;
			statement.MakePayment = true;

			using (new TemporaryUserContext() { BranchPK = branch.PK.ToGuid() }.Set())
			{
				var result = statement.PerformAccIntegration();

				Assert("Not successful in a wrong company", !result.WasSuccessful);
				AssertContains("System will not proceed and will not perform auto-billing.", result.Message);
			}
		}

		public void TestOnlyActiveLinesAreAutoRated()
		{
			var testHelper = SetRegistryItems(true, false, false);

			JobDeclaration declaration = CreateDeclaration("1", testHelper.Importer);
			JobDeclaration declaration2 = CreateDeclaration("2", testHelper.Importer);

			CusStatementHeader statement = CreateStatement("123456", PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, "1", 10m, 0m);
			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;

			CusStatementLine line = statement.StatementLines.AddNew();
			line.B3_EntryNum = "2";
			line.B3_EntryFilerCode = "XJ5";
			line.B3_Status = StatementLineStatusList.Codes.Deleted;

			CusStatementLineCharge charge = line.Charges.AddNew();
			charge.B4_ChargeAmount = 10m;
			charge.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;

			Factory.Save();

			AssertNotNull(Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, declaration.CompanyPK)));
			AssertNull("Should not have autorated for declaration2 which links to a deleted statement line", Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration2.PK).AddToFilter(JobHeaderSchema.JH_GC, declaration2.CompanyPK)));
		}

		public void TestDailyStatementIsProcessedWhenARIsAlreadyPostedWhenEntryIsCleared()
		{
			var testHelper = SetRegistryItems(true, false, true);

			JobDeclaration declaration = CreateDeclaration("1", testHelper.Importer);
			Factory.Save();

			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.CustomsEntryHeaders[0].Charges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 10m);
			declaration.CustomsEntryHeaders[0].CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();//this should have raised AR

			JobHeader job = Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, declaration.CompanyPK));
			AssertNotNull(job);

			JobCharge[] charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals("one charge should have been autorated", 1, charges.Length);
			AssertEquals("But not posted for AP", false, charges[0].IsCostPosted);
			AssertEquals("But posted for AR", true, charges[0].IsRevenuePosted);
			AssertEquals("AAA is mapped to a right charge code", testHelper.DisbursementChargeCode.PK, charges[0].JR_AC);
			AssertEquals("Amount", 10m, charges[0].JR_LocalCostAmt);
			AssertEquals("AR Amount", 10m, charges[0].ARLine.AL_LineAmount);

			CusStatementHeader statement = CreateStatement("123456", PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, declaration.CustomsEntryHeaders[0].EntryNumber, 20m, 5m);
			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;

			Env.OutgoingMailManager.EmailsCreated.Clear();

			Factory.Save();

			charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));

			CombineAssertions(() =>
			{
				AssertEquals("two charges should have been autorated", 2, charges.Length);
				var charge1 = charges[0];
				var charge2 = charges[1];
				AssertEquals("But not posted for AP", false, charge1.IsCostPosted);
				AssertEquals("But posted for AR", true, charge1.IsRevenuePosted);
				AssertEquals("AAA is mapped to a right charge code", testHelper.DisbursementChargeCode.PK, charge1.JR_AC);
				AssertEquals("AR Amount should remain same when first raised", 10m, charge1.ARLine.AL_LineAmount);
				AssertEquals("Amount", 10m, charge1.JR_LocalCostAmt);

				AssertEquals("But not posted for AP", false, charge2.IsCostPosted);
				AssertEquals("But posted for AR", false, charge2.IsRevenuePosted);
				AssertEquals("AAA is mapped to a right charge code", testHelper.DisbursementChargeCode.PK, charge2.JR_AC);
				AssertEquals("AR Amount should remain same when first raised", -15m, charge2.ARLine.AL_LineAmount);
				AssertEquals("Amount", 15m, charge2.JR_LocalCostAmt);

				AssertEquals("one email should have been sent notifying the discrepancy between AR raised and Customs-advised fee", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			});
		}

		public void TestPeriodicDailyStatementIsProcessed()
		{
			var testHelper = SetRegistryItems(true, false, false);

			JobDeclaration declaration = CreateDeclaration("1", testHelper.Importer);

			CusStatementHeader statement = CreateStatement("123456", PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate, "1", 10m, 0m);
			AssertEquals(true, statement.IsPeriodicDailyStatement);
			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			Factory.Save();

			JobHeader job = Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, declaration.CompanyPK));
			AssertNotNull(job);

			JobCharge[] charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals("one charge should have been autorated", 1, charges.Length);
			AssertEquals("But not posted for AP", false, charges[0].IsCostPosted);
			AssertEquals("AAA is mapped to a right charge code", testHelper.DisbursementChargeCode.PK, charges[0].JR_AC);
			AssertEquals("Amount", 10m, charges[0].JR_LocalCostAmt);

			statement.B2_Status = StatementHeaderStatusList.Codes.Final;//not sure if this happens on a periodic daily statement, but it should not do anything
			Factory.Save();

			charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals("one charge should have been autorated", 1, charges.Length);
			AssertEquals("Final means payment accepted. When ACH authorised, it should have raised AP", true, charges[0].IsCostPosted);
			AssertEquals("But not posted for AR", false, charges[0].IsRevenuePosted);
			AssertEquals("AAA is mapped to a right charge code", testHelper.DisbursementChargeCode.PK, charges[0].JR_AC);
			AssertEquals("Amount", 10m, charges[0].JR_LocalCostAmt);
		}

		public void TestMonthlyStatementIsProcessed()
		{
			var testHelper = SetRegistryItems(true, false, false);

			JobDeclaration declaration = CreateDeclaration("1", testHelper.Importer);
			JobDeclaration declaration2 = CreateDeclaration("2", testHelper.Importer);

			CusStatementHeader statement = CreateStatement("123456", PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate, "1", 10m, 0m);
			AssertEquals(true, statement.IsPeriodicDailyStatement);
			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			Factory.Save();

			JobHeader job = Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, declaration.CompanyPK));
			AssertNotNull(job);

			JobCharge[] charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals("one charge should have been autorated", 1, charges.Length);
			AssertEquals("Not posted for AR", false, charges[0].IsRevenuePosted);
			AssertEquals("AP is not raised", false, charges[0].IsCostPosted);

			CusStatementHeader statement2 = CreateStatement("1234567", PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate, "2", 20m, 0m);
			AssertEquals(true, statement2.IsPeriodicDailyStatement);
			statement2.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			Factory.Save();

			JobHeader job2 = Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration2.PK).AddToFilter(JobHeaderSchema.JH_GC, declaration2.CompanyPK));
			AssertNotNull(job2);

			JobCharge[] charges2 = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job2.PK));
			AssertEquals("one charge should have been autorated", 1, charges2.Length);
			AssertEquals("AP is not raised", false, charges2[0].IsCostPosted);

			CusStatementHeader monthlyStatement = CreateStatement("1234P", PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate, "", 0m, 0m);
			AssertEquals(true, monthlyStatement.IsMonthlyStatement);

			statement.B2_B2_PeriodicStatement = monthlyStatement.PK;
			statement2.B2_B2_PeriodicStatement = monthlyStatement.PK;

			monthlyStatement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			Factory.Save();

			//same as before
			charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals("one charge should have been autorated", 1, charges.Length);
			AssertEquals("AP is not raised", false, charges[0].IsCostPosted);
			charges2 = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job2.PK));
			AssertEquals("one charge should have been autorated", 1, charges2.Length);
			AssertEquals("AR is not raised", false, charges2[0].IsRevenuePosted);
			AssertEquals("AP is not raised", false, charges2[0].IsCostPosted);

			Factory.Save();

			statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;//there should be one payment made for all the daily statements
			Factory.Save();
			charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals("one charge should have been autorated", 1, charges.Length);
			AssertEquals("AP is raised", false, charges[0].IsCostPosted);
		}

		public void TestCustomsDutyNotAccruedTwice()
		{
			var testHelper = SetRegistryItems(true, true, true);

			var declaration = CreateDeclaration("1", testHelper.Importer);
			declaration.ReleaseStatus = CRLReleaseStatusList.Codes.REL;

			var statement = CreateStatement("5714280Z86", PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate, "1", 190m, 6.25m);
			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			Factory.Save();

			var job = Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, declaration.CompanyPK));
			AssertNotNull(job);

			var charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals("one charge should have been autorated", 1, charges.Length);

			var charge = charges[0];
			AssertEquals("But not posted for AP", false, charge.IsCostPosted);
			AssertEquals("AAA is mapped to a right charge code", testHelper.DisbursementChargeCode.PK, charge.JR_AC);
			AssertEquals("Amount", 196.25m, charge.JR_LocalCostAmt);

			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();

			var brokerAccounts = new BrokersAccountCollection();
			var account = brokerAccounts.AddNew();
			account.PayerUnitNumber = "123456";
			account.BankAccount = bankAccount.PK;
			Enterprise.Customs.US.DataRegistry.Business.USCustomsDataRegistry.Instance.BrokersAccounts.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, brokerAccounts);

			var charge2 = Factory.New<JobCharge>();
			charge2.JR_JH = job.PK;
			charge2.JR_AC = charge.JR_AC;
			charge2.JR_GB = charge.JR_GB;
			charge2.JR_GE = charge.JR_GE;
			charge2.JR_EstimatedCost = -196.25m;
			charge2.JR_APInvoiceNum = "1-1";
			charge2.JR_APInvoiceDate = ZDateTime.Today;
			charge2.JR_LocalCostAmt = -196.25m;
			charge2.JR_OSCostAmt = -196.25m;
			charge2.JR_LocalSellAmt = -196.25m;
			charge2.JR_OSSellAmt = -196.25m;
			charge2.JR_OH_CostAccount = charge.JR_OH_CostAccount;
			charge2.JR_RX_NKCostCurrency = charge.JR_RX_NKCostCurrency;
			charge2.JR_RX_NKSellCurrency = charge.JR_RX_NKSellCurrency;
			charge2.JR_AT_CostGSTRate = charge.JR_AT_CostGSTRate;

			var charge3 = Factory.New<JobCharge>();
			charge3.JR_JH = job.PK;
			charge3.JR_AC = charge.JR_AC;
			charge3.JR_GB = charge.JR_GB;
			charge3.JR_GE = charge.JR_GE;
			charge3.JR_EstimatedCost = 196.25m;
			charge3.JR_LocalCostAmt = 196.25m;
			charge3.JR_OSCostAmt = 196.25m;
			charge3.JR_LocalSellAmt = 196.25m;
			charge3.JR_OSSellAmt = 196.25m;
			charge3.JR_OH_CostAccount = charge.JR_OH_CostAccount;
			charge3.JR_RX_NKCostCurrency = charge.JR_RX_NKCostCurrency;
			charge3.JR_RX_NKSellCurrency = charge.JR_RX_NKSellCurrency;
			charge3.JR_AT_CostGSTRate = charge.JR_AT_CostGSTRate;
			Factory.Save();

			statement.B2_AccountNo = "123456";
			statement.B2_Status = StatementHeaderStatusList.Codes.Final;
			Factory.Save();

			statement.Reload();
			AssertEquals(1, statement.StatementLinesForAccountingRecon.Count);
			AssertEquals(0m, statement.StatementLinesForAccountingRecon[0].DifferenceBetweenAPInvoiceAndCustomsAmount);
			AssertEquals(0m, statement.StatementLinesForAccountingRecon[0].DifferenceBetweenARInvoiceAndCustomsAmount);
		}

		JobDeclaration CreateDeclaration(ZString entryNumber, OrgHeader importer)
		{
			var result = Factory.New<JobDeclaration>();
			result.JE_OH_Importer = importer.PK;
			result.JE_MessageType = JobMessageTypeList.Codes.Import;
			result.JE_TransportMode = "SEA";
			result.US_EntryFilerCode = "XJ5";
			result.US_EnableENS = true;
			var entry = result.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = entryNumber;
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			Factory.Save();
			entry.EntryNumber = entryNumber;
			Factory.Save();

			return result;
		}

		CusStatementHeader CreateStatement(ZString statementNumber, ZString paymentType, ZString entryNumber, ZDecimal chargeAmountForAAA, ZDecimal chargeAmountForBBB)
		{
			return CreateStatement<CusStatementHeader>(statementNumber, paymentType, entryNumber, chargeAmountForAAA, chargeAmountForBBB);
		}

		T CreateStatement<T>(ZString statementNumber, ZString paymentType, ZString entryNumber, ZDecimal chargeAmountForAAA, ZDecimal chargeAmountForBBB) where T : CusStatementHeader
		{
			var result = Factory.New<T>();
			result.B2_PaymentType = paymentType;
			result.B2_StatementNumber = statementNumber;
			if (statementNumber.SubstringSafe(4, 1) == "P")
			{
				result.B2_IsMonthlyStatement = true;
			}

			var total = ZDecimal.Zero;

			if (!entryNumber.IsEmpty)
			{
				var line = result.StatementLines.AddNew();

				line.B3_EntryFilerCode = "XJ5";
				line.B3_Status = StatementLineStatusList.Codes.Active;

				if (chargeAmountForAAA > 0)
				{
					var charge = line.Charges.AddNew();
					charge.B4_ChargeAmount = chargeAmountForAAA;
					charge.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
					total += chargeAmountForAAA;
				}

				if (chargeAmountForBBB > 0)
				{
					var charge = line.Charges.AddNew();
					charge.B4_ChargeAmount = chargeAmountForBBB;
					charge.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.HMF;
					total += chargeAmountForBBB;
				}

				line.B3_CustomsFeesTotal = total;
				line.B3_EntryNum = entryNumber;
			}
			result.B2_StatementAmount = total;
			return result;
		}

		Customs.Business.Testing.InvoicingTestHelper SetRegistryItems(bool enableAccountingIntegration, bool postAP, bool postAR)
		{
			var option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = enableAccountingIntegration;
			option.APPostDSB = postAP;
			option.ARPostDSB = postAR;

			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, option);

			var postMaster = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = postMaster.PK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);

			var currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "test@cargowise.com";
			postMaster.Staff.Add(currentUserInCurrentFactory);

			Factory.Save();

			var testHelper = new Customs.Business.Testing.InvoicingTestHelper(Factory);
			testHelper.SetUpDisbursementCreditorAndChargeCode();
			return testHelper;
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
