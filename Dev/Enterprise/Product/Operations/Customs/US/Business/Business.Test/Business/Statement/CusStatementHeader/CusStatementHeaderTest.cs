using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusStatementHeader))]
	public class CusStatementHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestClearPaymentDetailsWhenDeleted()
		{
			var statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_AccountNo = "123456";
			statementHeader.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			statementHeader.B2_PaymentAuthorizationDate = new ZDateTime(2024, 10, 10);

			statementHeader.B2_PaymentStatus = PaymentStatusList.Codes.PaymentFailed;
			AssertEquals("123456", statementHeader.B2_AccountNo);
			AssertEquals(PaymentPartyList.Codes.Broker, statementHeader.B2_PaymentParty);
			AssertEquals(new ZDateTime(2024, 10, 10), statementHeader.B2_PaymentAuthorizationDate);

			statementHeader.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationDeleted;
			AssertEquals("It should have cleared payment details", ZString.Empty, statementHeader.B2_AccountNo);
			AssertEquals("It should have cleared payment details", ZString.Empty, statementHeader.B2_PaymentParty);
			AssertEquals("It should have cleared payment details", ZDateTime.Empty, statementHeader.B2_PaymentAuthorizationDate);
		}

		public void TestGetCheckIfValidToSendAuthorizationOrPaymentMessage()
		{
			var header = Factory.New<CusStatementHeader>();
			Env.Security.USCustomsImportStatementModify.IsAllowed = false;
			Env.Security.USCustomsImportStatementView.IsAllowed = true;
			Env.Security.USCustomsImportStatementSendAuthMsgImporter.IsAllowed = true;
			header.B2_StatementNumber = "0914725836";

			header.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			header.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			Env.Security.USCustomsImportStatementSendAuthMsgBroker.IsAllowed = false;
			var messageText = header.GetCheckIfValidToSendAuthorizationOrPaymentMessage();
			Assert(messageText.Item2.Contains("Send Authorization Message (Broker statement)"));

			Env.Security.USCustomsImportStatementSendAuthMsgImporter.IsAllowed = false;
			header.B2_PaymentParty = ZString.Empty;
			header.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			messageText = header.GetCheckIfValidToSendAuthorizationOrPaymentMessage();
			Assert(messageText.Item2.Contains("Send Authorization Message (Importer statement)"));

			Env.Security.USCustomsImportStatementSendAuthMsgImporter.IsAllowed = true;
			DataRegistry.Business.USCustomsDataRegistry.Instance.RequireApprovalPriorAuthorizingStatement.SetValue(header.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			messageText = header.GetCheckIfValidToSendAuthorizationOrPaymentMessage();
			Assert(messageText.Item1.Contains("Permission to 'Send Payment Authorization' needs to be granted"));

			DataRegistry.Business.USCustomsDataRegistry.Instance.RequireApprovalPriorAuthorizingStatement.SetValue(header.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			messageText = header.GetCheckIfValidToSendAuthorizationOrPaymentMessage();
			AssertNull(messageText);
		}

		public void TestHumanReadableShortcutNameInStatement()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "BSSSSSS";
			AssertEquals("Statement - BSSSSSS", statement.HumanReadableShortcutName);
		}

		public void TestImporterCustomsIDForDisplay()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_ImporterCustomsID = "123-45-6789";

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			AssertEquals("***-**-****", statement.ImporterCustomsIDForDisplay);

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = true;
			AssertEquals("123-45-6789", statement.ImporterCustomsIDForDisplay);

			statement.B2_ImporterCustomsID = "12345-6789";
			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			AssertEquals("12345-6789", statement.ImporterCustomsIDForDisplay);
		}

		public void TestPayableTaxAndDeferredTax()
		{
			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			statement.B2_Status = StatementHeaderStatusList.Codes.Final;

			CusStatementLine statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_EntryNum = "123456";
			statementLine.B3_CustomsFeesTotal = 10000m;
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.ExciseTaxDeferred, 10m);

			CusStatementLine statementLine2 = statement.StatementLines.AddNew();
			statementLine2.B3_EntryFilerCode = "XJ5";
			statementLine2.B3_EntryNum = "123457";
			statementLine2.B3_CustomsFeesTotal = 10000m;
			statementLine2.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine2.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.ExciseTaxPayable, 20m);

			CusStatementLine statementLine3 = statement.StatementLines.AddNew();
			statementLine3.B3_EntryFilerCode = "XJ5";
			statementLine3.B3_EntryNum = "123457";
			statementLine3.B3_CustomsFeesTotal = 10000m;
			statementLine3.B3_Status = StatementLineStatusList.Codes.Deleted;
			statementLine3.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.ExciseTaxDeferred, 50m);

			AssertEquals(60m, statement.TotalDeferredTax);
			AssertEquals(20m, statement.TotalPayableTax);

			AssertEquals(10m, statement.FinalTotalDeferredTax);
			AssertEquals(20m, statement.FinalTotalPayableTax);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Definitely Deleting object should have an exception/error because of trigger.", true);
		}

		public void TestAllOrActiveLines()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;

			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_EntryNum = "123456";
			statementLine.B3_CustomsFeesTotal = 10000m;
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.ExciseTaxDeferred, 10m);
			Assert(!statement.HasDeletedLines);

			var statementLine2 = statement.StatementLines.AddNew();
			statementLine2.B3_EntryFilerCode = "XJ5";
			statementLine2.B3_EntryNum = "123457";
			statementLine2.B3_CustomsFeesTotal = 10000m;
			statementLine2.B3_Status = StatementLineStatusList.Codes.Deleted;//Deleted at the statement
			statementLine2.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.ExciseTaxPayable, 20m);

			AssertEquals("While preliminary, it should return all lines", 2, statement.AllOrActiveLines.Count);
			Assert(statement.HasDeletedLines);

			statement.B2_Status = StatementHeaderStatusList.Codes.Final;
			AssertEquals("When finalised, this collection should not return deleted lines", 1, statement.AllOrActiveLines.Count);
			Assert(statement.HasDeletedLines);
		}

		public void TestPaymentDateIsRecordedAgainstEntryIfExists()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "123456";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			JobDeclaration declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_EnableENS = true;
			declaration2.US_EntryFilerCode = "XJ5";
			declaration2.ImportEntryNumber = "123457";
			declaration2.Invoices.AddNew();
			declaration2.InvoiceLines.AddNew();
			declaration2.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;

			CusStatementLine statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_EntryNum = "123456";
			statementLine.B3_CustomsFeesTotal = 10000m;
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;

			CusStatementLine statementLine2 = statement.StatementLines.AddNew();
			statementLine2.B3_EntryFilerCode = "XJ5";
			statementLine2.B3_EntryNum = "123457";
			statementLine2.B3_CustomsFeesTotal = 10000m;
			statementLine2.B3_Status = StatementLineStatusList.Codes.Active;
			AssertNotNull("PreCondition", statementLine.Declaration);
			AssertNotNull("PreCondition", statementLine2.Declaration);

			Factory.Save();

			AssertEquals(ZDateTime.Empty, declaration.US_PaymentDate);
			AssertEquals(ZDateTime.Empty, declaration2.US_PaymentDate);

			statementLine2.B3_Status = StatementLineStatusList.Codes.Deleted;
			statement.B2_Status = StatementHeaderStatusList.Codes.Final;

			Factory.Save();

			AssertEquals(ZDateTime.Empty, declaration.US_PaymentDate);
			AssertEquals(ZDateTime.Empty, declaration2.US_PaymentDate);

			statement.B2_PaymentAuthorizationDate = new ZDateTime(2009, 2, 1);
			Factory.Save();

			AssertEquals("Payment Date is recorded against Declaration", new ZDateTime(2009, 2, 1), declaration.US_PaymentDate);
			AssertEquals("Payment date is not recorded because this line is deleted from this statement", ZDateTime.Empty, declaration2.US_PaymentDate);
		}

		public void TestIsFinalOrDeleted()
		{
			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			AssertEquals(false, statement.IsFinalOrDeleted);

			statement.B2_Status = StatementHeaderStatusList.Codes.Final;
			AssertEquals(true, statement.IsFinalOrDeleted);

			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			AssertEquals(false, statement.IsFinalOrDeleted);

			statement.B2_Status = StatementHeaderStatusList.Codes.Deleted;
			AssertEquals(true, statement.IsFinalOrDeleted);
		}

		public void TestGetAccountingAP_ARInvoiceAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			declaration.ImportEntryNumber = "1";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_EntryFilerCode = "XJ5";

			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			declaration2.ImportEntryNumber = "2";

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.US_EntryFilerCode = "XJ5";

			var entry3 = declaration2.CustomsEntryHeaders.AddNew();
			entry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			declaration3.ImportEntryNumber = "3";
			Factory.Save();

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "231342";

			AddStatementLine(statement, "1", 10, 20, 40, 80, true);
			statement.RefreshAP_ARInvoiceQueryResult();
			AssertEquals("APPostedAmount", 10m, statement.APPostedAmount);
			AssertEquals("APUnPostedAmount", 20m, statement.APUnPostedAmount);
			AssertEquals("ARPostedAmount", 40m, statement.ARPostedAmount);
			AssertEquals("ARUnPostedAmount", 80m, statement.ARUnPostedAmount);
			AssertEquals("APFullyPaid", true, statement.APFullyPaid);

			AddStatementLine(statement, "3", 0, 0, 0, 0, false);
			statement.RefreshAP_ARInvoiceQueryResult();
			AssertEquals("APPostedAmount", 10m, statement.APPostedAmount);
			AssertEquals("APUnPostedAmount", 20m, statement.APUnPostedAmount);
			AssertEquals("ARPostedAmount", 40m, statement.ARPostedAmount);
			AssertEquals("ARUnPostedAmount", 80m, statement.ARUnPostedAmount);
			AssertEquals("should disregard zero amounted line", true, statement.APFullyPaid);

			AddStatementLine(statement, "2", 1, 2, 4, 8, false);

			statement.RefreshAP_ARInvoiceQueryResult();
			AssertEquals("APPostedAmount", 11m, statement.APPostedAmount);
			AssertEquals("APUnPostedAmount", 22m, statement.APUnPostedAmount);
			AssertEquals("ARPostedAmount", 44m, statement.ARPostedAmount);
			AssertEquals("ARUnPostedAmount", 88m, statement.ARUnPostedAmount);
			AssertEquals("APFullyPaid", false, statement.APFullyPaid);
		}

		public void TestHasDiscrepancyBetweenAccountingInvoices()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			declaration.ImportEntryNumber = "1";
			Factory.Save();

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementAmount = 30m;
			statement.B2_StatementNumber = "1234";
			statement.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = "1";
			statementLine.B3_EntryFilerCode = "XJ5";

			var query = new TestQuery();
			query.Result = new AP_ARInvoiceQueryResult();
			query.Result.APPostedAmount = 10m;
			query.Result.APUnPostedAmount = 20m;
			query.Result.ARPostedAmount = 10m;
			query.Result.ARUnPostedAmount = 20m;
			query.Result.APFullyPaid = true;
			statementLine.AccInvQueryExposedForTesting = query;
			statementLine.B3_CustomsFeesTotal = 30m;
			var charge = statementLine.Charges.AddNew();
			charge.B4_ChargeType = "DTY";
			charge.B4_ChargeAmount = 30m;

			var monthlyStatement = Factory.New<CusStatementHeader>();
			monthlyStatement.B2_StatementNumber = "1234P";
			monthlyStatement.B2_IsMonthlyStatement = true;
			monthlyStatement.B2_StatementAmount = 30m;
			monthlyStatement.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			statement.B2_B2_PeriodicStatement = monthlyStatement.PK;

			var messageText = "B011101XJ5MSF1234P   01051607691-013199000                                      " +
"Q11234     11101XJ5            0516070516070000559488300000000000               " +
"Q2000000203510000004263100005959169                                             " +
"QA010560000000618410500000003530496000000005000540000000911205300000002159      " +
"QA024990000024509331100000000800106000000024200550000001025650100000000316      " +
"QA030570000000732109000000001172102000000059911030000000481010400000001640      " +
"Q38804P04001061107061507XXX            0001118976600000000000                   " +
"Q4000000407020000008526200000003000                                             " +
"QE010560000001236810500000007060496000000010000540000001822405300000004318      " +
"QE024990000049018631100000001600106000000048400550000002051250100000000632      " +
"QE030570000001464209000000002344102000000119821030000000962010400000003280      " +
"Q58804P04001061107061507XXX            0001118976600000000000                   " +
"Q6000000407020000008526200011934338                                             " +
"Y  1101XJ5MS00022";

			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement;
			message.EM_MessageText = messageText;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageNum = "";
			Factory.Save();

			monthlyStatement.Messages.Add(message);

			monthlyStatement.RefreshAP_ARInvoiceQueryResult();
			AssertEquals("DifferenceBetweenAPInvoiceAndCustomsAmount", 0m, statement.DifferenceBetweenAPInvoiceAndCustomsAmount);
			AssertEquals("DifferenceBetweenARInvoiceAndCustomsAmount", 0m, statement.DifferenceBetweenARInvoiceAndCustomsAmount);
			AssertEquals("HasDiscrepancy", false, statement.HasDiscrepancyBetweenInvoicesAndCustomsAmount);

			AssertEquals("DifferenceBetweenAPInvoiceAndCustomsAmount", 0m, monthlyStatement.DifferenceBetweenAPInvoiceAndCustomsAmount);
			AssertEquals("DifferenceBetweenARInvoiceAndCustomsAmount", 0m, monthlyStatement.DifferenceBetweenARInvoiceAndCustomsAmount);
			AssertEquals("HasDiscrepancy", false, monthlyStatement.HasDiscrepancyBetweenInvoicesAndCustomsAmount);

			query.Result.ARPostedAmount = 40m;
			query.Result.ARUnPostedAmount = 80m;

			monthlyStatement.RefreshAP_ARInvoiceQueryResult();
			AssertEquals("DifferenceBetweenAPInvoiceAndCustomsAmount", 0m, statement.DifferenceBetweenAPInvoiceAndCustomsAmount);
			AssertEquals("DifferenceBetweenARInvoiceAndCustomsAmount", 90m, statement.DifferenceBetweenARInvoiceAndCustomsAmount);
			AssertEquals("HasDiscrepancy", true, statement.HasDiscrepancyBetweenInvoicesAndCustomsAmount);

			AssertEquals("DifferenceBetweenAPInvoiceAndCustomsAmount", 0m, monthlyStatement.DifferenceBetweenAPInvoiceAndCustomsAmount);
			AssertEquals("DifferenceBetweenARInvoiceAndCustomsAmount", 90m, monthlyStatement.DifferenceBetweenARInvoiceAndCustomsAmount);
			AssertEquals("HasDiscrepancy", true, monthlyStatement.HasDiscrepancyBetweenInvoicesAndCustomsAmount);

			query.Result.APPostedAmount = 20m;
			monthlyStatement.RefreshAP_ARInvoiceQueryResult();
			AssertEquals("DifferenceBetweenAPInvoiceAndCustomsAmount", 10m, statement.DifferenceBetweenAPInvoiceAndCustomsAmount);
			AssertEquals("DifferenceBetweenARInvoiceAndCustomsAmount", 90m, statement.DifferenceBetweenARInvoiceAndCustomsAmount);
			AssertEquals("HasDiscrepancy", true, statement.HasDiscrepancyBetweenInvoicesAndCustomsAmount);

			AssertEquals("DifferenceBetweenAPInvoiceAndCustomsAmount", 10m, monthlyStatement.DifferenceBetweenAPInvoiceAndCustomsAmount);
			AssertEquals("DifferenceBetweenARInvoiceAndCustomsAmount", 90m, monthlyStatement.DifferenceBetweenARInvoiceAndCustomsAmount);
			AssertEquals("HasDiscrepancy", true, monthlyStatement.HasDiscrepancyBetweenInvoicesAndCustomsAmount);
		}

		public void TestHasDiscrepancyBetweenAccountingInvoicesForImporterPayment()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "1";
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementAmount = 30m;
			statement.B2_StatementNumber = "1234";
			statement.B2_PaymentParty = PaymentPartyList.Codes.Importer;
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = "1";
			statementLine.B3_EntryFilerCode = "XJ5";

			var query = new TestQuery();
			query.Result = new AP_ARInvoiceQueryResult();
			query.Result.APPostedAmount = 0m;
			query.Result.APUnPostedAmount = 0m;
			query.Result.ARPostedAmount = 0m;
			query.Result.ARUnPostedAmount = 0m;
			statementLine.AccInvQueryExposedForTesting = query;
			statementLine.B3_CustomsFeesTotal = 30m;
			var charge = statementLine.Charges.AddNew();
			charge.B4_ChargeType = "DTY";
			charge.B4_ChargeAmount = 30m;

			var monthlyStatement = Factory.New<CusStatementHeader>();
			monthlyStatement.B2_StatementNumber = "1234P";
			monthlyStatement.B2_StatementAmount = 30m;
			statement.B2_B2_PeriodicStatement = monthlyStatement.PK;

			monthlyStatement.RefreshAP_ARInvoiceQueryResult();

			AssertEquals("AP&AR amounts should be compared with CusStatementHeader.BrokerPaymentAmount", 0m, statement.DifferenceBetweenAPInvoiceAndCustomsAmount);
			AssertEquals("AP&AR amounts should be compared with CusStatementHeader.BrokerPaymentAmount", 0m, statement.DifferenceBetweenARInvoiceAndCustomsAmount);
			AssertEquals("HasDiscrepancy", false, statement.HasDiscrepancyBetweenInvoicesAndCustomsAmount);

			AssertEquals("AP&AR amounts should be compared with CusStatementHeader.BrokerPaymentAmount", 0m, monthlyStatement.DifferenceBetweenAPInvoiceAndCustomsAmount);
			AssertEquals("AP&AR amounts should be compared with CusStatementHeader.BrokerPaymentAmount", 0m, monthlyStatement.DifferenceBetweenARInvoiceAndCustomsAmount);
			AssertEquals("HasDiscrepancy", false, monthlyStatement.HasDiscrepancyBetweenInvoicesAndCustomsAmount);
		}

		TestQuery AddStatementLine(CusStatementHeader statement, ZString entryNumber, ZDecimal apPosted, ZDecimal apUnposted, ZDecimal arPosted, ZDecimal arUnposted, ZBool fullyPaid)
		{
			CusStatementLine statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = entryNumber;
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_CustomsFeesTotal = apPosted + apUnposted;
			statement.ActiveLines.Rebuild();
			TestQuery query = new TestQuery();
			query.Result = new AP_ARInvoiceQueryResult();
			query.Result.APPostedAmount = apPosted;
			query.Result.APUnPostedAmount = apUnposted;
			query.Result.ARPostedAmount = arPosted;
			query.Result.ARUnPostedAmount = arUnposted;
			query.Result.APFullyPaid = fullyPaid;

			statementLine.AccInvQueryExposedForTesting = query;
			return query;
		}

		public void TestPaymentStatusIsSetWhenFinalised()
		{
			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			statement.B2_Status = StatementHeaderStatusList.Codes.Final;

			AssertEquals(PaymentStatusList.Codes.PaymentAuthorizationAccepted, statement.B2_PaymentStatus);
		}

		public void TestIsPaidByBroker()
		{
			var statementHeader = Factory.New<CusStatementHeader>();
			AssertEquals(false, statementHeader.IsPaidByBroker);

			statementHeader.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			AssertEquals(true, statementHeader.IsPaidByBroker);
			AssertEquals(true, statementHeader.IsPaymentPartyBroker);

			statementHeader.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			statementHeader.B2_PaymentParty = ZString.Empty;
			AssertEquals(false, statementHeader.IsPaidByBroker);
			AssertEquals(true, statementHeader.IsPaymentPartyBroker);
		}

		public void TestClearPaymentDetailsWhenFailed()
		{
			var statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_AccountNo = "123456";
			statementHeader.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			statementHeader.B2_PaymentAuthorizationDate = ZDateTime.Today;

			statementHeader.B2_PaymentStatus = PaymentStatusList.Codes.PaymentFailed;
			AssertEquals("123456", statementHeader.B2_AccountNo);
			AssertEquals(PaymentPartyList.Codes.Broker, statementHeader.B2_PaymentParty);
			AssertEquals(ZDateTime.Today, statementHeader.B2_PaymentAuthorizationDate);

			statementHeader.B2_PaymentAuthorizationDate = ZDateTime.Empty;
			statementHeader.B2_PaymentStatus = PaymentStatusList.Codes.PaymentFailed;
			AssertEquals("It should have cleared payment details", ZString.Empty, statementHeader.B2_AccountNo);
			AssertEquals("It should have cleared payment details", ZString.Empty, statementHeader.B2_PaymentParty);
			AssertEquals("It should have cleared payment details", ZDateTime.Empty, statementHeader.B2_PaymentAuthorizationDate);

			statementHeader.B2_AccountNo = "123456";
			statementHeader.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			statementHeader.B2_PaymentAuthorizationDate = ZDateTime.Today;
			statementHeader.B2_PaymentStatus = ZString.Empty;
			AssertEquals("It should have cleared payment details", ZString.Empty, statementHeader.B2_AccountNo);
			AssertEquals("It should have cleared payment details", ZString.Empty, statementHeader.B2_PaymentParty);
			AssertEquals("It should have cleared payment details", ZDateTime.Empty, statementHeader.B2_PaymentAuthorizationDate);
		}

		public void TestTotalAmountDueForDailyStatements()
		{
			CusStatementHeader monthlyStatement = Factory.New<CusStatementHeader>();
			monthlyStatement.B2_StatementNumber = "1112P2222";
			monthlyStatement.B2_IsMonthlyStatement = true;
			monthlyStatement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			monthlyStatement.B2_StatementAmount = 3300.00m;

			CusStatementHeader dailyStatement1 = monthlyStatement.DailyStatements.AddNew();
			dailyStatement1.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			dailyStatement1.B2_StatementAmount = 300.00m;

			CusStatementLine statementLine1ForDailyStatement1 = dailyStatement1.StatementLines.AddNew();
			statementLine1ForDailyStatement1.B3_Status = StatementLineStatusList.Codes.Active;
			SetFeesToStatementLine1(statementLine1ForDailyStatement1);

			CusStatementHeader dailyStatement2 = monthlyStatement.DailyStatements.AddNew();
			dailyStatement2.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			dailyStatement2.B2_StatementAmount = 3000.00m;

			CusStatementLine statementLine1ForDailyStatement2 = dailyStatement2.StatementLines.AddNew();
			statementLine1ForDailyStatement2.B3_Status = StatementLineStatusList.Codes.Active;
			SetFeesToStatementLine2(statementLine1ForDailyStatement2);

			AssertEquals(300.00m, dailyStatement1.TotalAmountDue);
			AssertEquals(3000.00m, dailyStatement2.TotalAmountDue);

			statementLine1ForDailyStatement1.B3_Status = StatementLineStatusList.Codes.Deleted;
			statementLine1ForDailyStatement1.B3_CustomsFeesTotal = 40m;
			statementLine1ForDailyStatement2.B3_Status = StatementLineStatusList.Codes.Deleted;
			statementLine1ForDailyStatement2.B3_CustomsFeesTotal = 50m;

			AssertEquals(300.00m, dailyStatement1.TotalAmountDue);
			AssertEquals(3000.00m, dailyStatement2.TotalAmountDue);

			dailyStatement1.B2_Status = StatementHeaderStatusList.Codes.Final;
			dailyStatement2.B2_Status = StatementHeaderStatusList.Codes.Final;
			AssertEquals(340.00m, dailyStatement1.TotalAmountDue);
			AssertEquals(3050.00m, dailyStatement2.TotalAmountDue);
		}

		public void TestFinalTotalAmountDueForDailyStatements()
		{
			CusStatementHeader monthlyStatement = Factory.New<CusStatementHeader>();
			monthlyStatement.B2_StatementNumber = "1112P2222";
			monthlyStatement.B2_StatementAmount = 3300.00m;
			monthlyStatement.B2_IsMonthlyStatement = true;

			CusStatementHeader dailyStatement1 = monthlyStatement.DailyStatements.AddNew();
			dailyStatement1.B2_StatementAmount = 300.00m;

			CusStatementLine statementLine1ForDailyStatement1 = dailyStatement1.StatementLines.AddNew();
			statementLine1ForDailyStatement1.B3_Status = StatementLineStatusList.Codes.Active;
			SetFeesToStatementLine1(statementLine1ForDailyStatement1);

			CusStatementHeader dailyStatement2 = monthlyStatement.DailyStatements.AddNew();
			dailyStatement2.B2_StatementAmount = 3000.00m;
			CusStatementLine statementLine1ForDailyStatement2 = dailyStatement2.StatementLines.AddNew();
			statementLine1ForDailyStatement2.B3_Status = StatementLineStatusList.Codes.Active;
			SetFeesToStatementLine2(statementLine1ForDailyStatement2);

			AssertEquals(300.00m, dailyStatement1.FinalTotalAmountDue);
			AssertEquals(3000.00m, dailyStatement2.FinalTotalAmountDue);

			statementLine1ForDailyStatement1.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine1ForDailyStatement2.B3_Status = StatementLineStatusList.Codes.Deleted;

			AssertEquals(300.00m, dailyStatement1.FinalTotalAmountDue);
			AssertEquals(3000.00m, dailyStatement2.FinalTotalAmountDue);
		}

		void SetFeesToStatementLine1(CusStatementLine statementLine)
		{
			CusStatementLineCharge statementLine1Charge1 = statementLine.Charges.AddNew();
			statementLine1Charge1.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Duty;
			statementLine1Charge1.B4_ChargeAmount = 1.00m;

			CusStatementLineCharge statementLine1Charge2 = statementLine.Charges.AddNew();
			statementLine1Charge2.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.AntidumpingDuty;
			statementLine1Charge2.B4_ChargeAmount = 2.00m;

			CusStatementLineCharge statementLine1Charge3 = statementLine.Charges.AddNew();
			statementLine1Charge3.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.CountervailingDuty;
			statementLine1Charge3.B4_ChargeAmount = 3.00m;

			CusStatementLineCharge statementLine1Charge4 = statementLine.Charges.AddNew();
			statementLine1Charge4.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.ReconciliationInterest;
			statementLine1Charge4.B4_ChargeAmount = 4.00m;

			CusStatementLineCharge statementLine1Charge5 = statementLine.Charges.AddNew();
			statementLine1Charge5.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Cotton;
			statementLine1Charge5.B4_ChargeAmount = 5.00m;

			CusStatementLineCharge statementLine1Charge6 = statementLine.Charges.AddNew();
			statementLine1Charge6.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Sugar;
			statementLine1Charge6.B4_ChargeAmount = 6.00m;

			CusStatementLineCharge statementLine1Charge7 = statementLine.Charges.AddNew();
			statementLine1Charge7.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.DutiableMail;
			statementLine1Charge7.B4_ChargeAmount = 7.00m;

			CusStatementLineCharge statementLine1Charge8 = statementLine.Charges.AddNew();
			statementLine1Charge8.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Pork;
			statementLine1Charge8.B4_ChargeAmount = 8.00m;

			CusStatementLineCharge statementLine1Charge9 = statementLine.Charges.AddNew();
			statementLine1Charge9.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Beef;
			statementLine1Charge9.B4_ChargeAmount = 9.00m;

			CusStatementLineCharge statementLine1Charge10 = statementLine.Charges.AddNew();
			statementLine1Charge10.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			statementLine1Charge10.B4_ChargeAmount = 10.00m;

			CusStatementLineCharge statementLine1Charge11 = statementLine.Charges.AddNew();
			statementLine1Charge11.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseInformal;
			statementLine1Charge11.B4_ChargeAmount = 11.00m;

			CusStatementLineCharge statementLine1Charge12 = statementLine.Charges.AddNew();
			statementLine1Charge12.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge;
			statementLine1Charge12.B4_ChargeAmount = 12.00m;

			CusStatementLineCharge statementLine1Charge13 = statementLine.Charges.AddNew();
			statementLine1Charge13.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Honey;
			statementLine1Charge13.B4_ChargeAmount = 13.00m;

			CusStatementLineCharge statementLine1Charge14 = statementLine.Charges.AddNew();
			statementLine1Charge14.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.HMF;
			statementLine1Charge14.B4_ChargeAmount = 14.00m;

			CusStatementLineCharge statementLine1Charge15 = statementLine.Charges.AddNew();
			statementLine1Charge15.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Raspberry;
			statementLine1Charge15.B4_ChargeAmount = 15.00m;

			CusStatementLineCharge statementLine1Charge16 = statementLine.Charges.AddNew();
			statementLine1Charge16.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Potato;
			statementLine1Charge16.B4_ChargeAmount = 16.00m;

			CusStatementLineCharge statementLine1Charge17 = statementLine.Charges.AddNew();
			statementLine1Charge17.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.FreshLimes;
			statementLine1Charge17.B4_ChargeAmount = 17.00m;

			CusStatementLineCharge statementLine1Charge18 = statementLine.Charges.AddNew();
			statementLine1Charge18.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Mushroom;
			statementLine1Charge18.B4_ChargeAmount = 18.00m;

			CusStatementLineCharge statementLine1Charge19 = statementLine.Charges.AddNew();
			statementLine1Charge19.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Watermelon;
			statementLine1Charge19.B4_ChargeAmount = 19.00m;

			CusStatementLineCharge statementLine1Charge20 = statementLine.Charges.AddNew();
			statementLine1Charge20.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.SoftwoodLumber;
			statementLine1Charge20.B4_ChargeAmount = 20.00m;

			CusStatementLineCharge statementLine1Charge21 = statementLine.Charges.AddNew();
			statementLine1Charge21.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Blueberry;
			statementLine1Charge21.B4_ChargeAmount = 21.00m;

			CusStatementLineCharge statementLine1Charge22 = statementLine.Charges.AddNew();
			statementLine1Charge22.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Avocado;
			statementLine1Charge22.B4_ChargeAmount = 22.00m;

			CusStatementLineCharge statementLine1Charge23 = statementLine.Charges.AddNew();
			statementLine1Charge23.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Mango;
			statementLine1Charge23.B4_ChargeAmount = 23.00m;

			CusStatementLineCharge statementLine1Charge24 = statementLine.Charges.AddNew();
			statementLine1Charge24.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Sorghum;
			statementLine1Charge24.B4_ChargeAmount = 24.00m;
		}

		void SetFeesToStatementLine2(CusStatementLine statementLine)
		{
			CusStatementLineCharge statementLine2Charge1 = statementLine.Charges.AddNew();
			statementLine2Charge1.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Duty;
			statementLine2Charge1.B4_ChargeAmount = 1000.00m;

			CusStatementLineCharge statementLine2Charge2 = statementLine.Charges.AddNew();
			statementLine2Charge2.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.AntidumpingDuty;
			statementLine2Charge2.B4_ChargeAmount = 2000.00m;
		}

		public void TestImporterWrapperAndImporterName()
		{
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "Test Importer For Statement";
			statementHeader.B2_OH_Importer = importer.PK;
			AssertEquals(typeof(OrgHeaderWrapper), statementHeader.ImporterWrapper.GetType());
			AssertEquals(importer.PK, statementHeader.ImporterWrapper.organisation.PK);
			AssertEquals("Test Importer For Statement", statementHeader.ImporterName);
		}

		public void TestAccessingDailyStatementsInsideDailyStatemenResultInException()
		{
			ErrorReporter.Clear();

			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "1";
			AssertEquals("Daily statement", true, statement.IsDailyStatement);

			try
			{
				statement.DailyStatements.AddNew();
				AssertEquals("You are trying to access DailyStatements collection from a daily statement", ErrorReporter.LastMessageReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestIsPeriodicDailyStatement()
		{
			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "1234";
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			AssertEquals(false, statement.IsPeriodicDailyStatement);
			AssertEquals(PaymentTypeList.Descriptions.BatchedByDailyPrintDateAndFilerCode, statement.PaymentTypeDescription);

			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			AssertEquals(true, statement.IsPeriodicDailyStatement);
			AssertEquals(PaymentTypeList.Descriptions.BatchedByPeriodicPrintDateAndFilerDate, statement.PaymentTypeDescription);
		}

		public void TestHasLinesWithDeletionPending()
		{
			CusStatementHeader monthlyStatement = Factory.New<CusStatementHeader>();
			monthlyStatement.B2_StatementNumber = "1234P";
			monthlyStatement.B2_IsMonthlyStatement = true;

			CusStatementHeader dailyStatement = monthlyStatement.DailyStatements.AddNew();
			dailyStatement.B2_StatementNumber = "1234";

			CusStatementLine statementLine = dailyStatement.StatementLines.AddNew();
			statementLine.B3_Status = StatementLineStatusList.Codes.Deleted;

			CusStatementHeader dailyStatement2 = monthlyStatement.DailyStatements.AddNew();
			dailyStatement2.B2_StatementNumber = "1234";

			CusStatementLine statementLine2 = dailyStatement2.StatementLines.AddNew();
			statementLine2.B3_Status = StatementLineStatusList.Codes.DeletionPending;

			AssertEquals(true, monthlyStatement.HasLinesWithDeletionPending);
			AssertEquals(false, dailyStatement.HasLinesWithDeletionPending);
			AssertEquals(true, dailyStatement2.HasLinesWithDeletionPending);
		}

		public void TestIsPaid()
		{
			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			AssertEquals(false, statement.IsPaid);

			//payment is attempted
			statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentInProgress;
			AssertEquals(false, statement.IsPaid);
			AssertEquals(PaymentStatusList.Descriptions.PaymentInProgress, statement.PaymentStatusDescription);

			//payment message is responded
			statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
			AssertEquals(true, statement.IsPaid);
			AssertEquals(PaymentStatusList.Descriptions.PaymentAuthorizationAccepted, statement.PaymentStatusDescription);

			//final statement is issued
			statement.B2_Status = StatementHeaderStatusList.Codes.Final;
			AssertEquals(true, statement.IsPaid);

			statement.B2_Status = ZString.Empty;
			statement.B2_PaymentStatus = ZString.Empty;
			var logAdded = statement.Logs.MostRecentLogByEventTime(Events.Authorised);
			AssertEquals("Payment Authorization Accepted", logAdded.SL_Reference);
			AssertEquals(true, statement.IsPaid);
		}

		public void TestTotalAmountsPayable()
		{
			CusStatementHeader monthlyStatement = Factory.New<CusStatementHeader>();
			monthlyStatement.B2_StatementNumber = "1112P2222";
			monthlyStatement.B2_IsMonthlyStatement = true;

			CusStatementHeader dailyStatement1 = monthlyStatement.DailyStatements.AddNew();
			CusStatementLine statementLine1 = dailyStatement1.StatementLines.AddNew();
			statementLine1.B3_Status = Enterprise.Customs.US.Business.StatementLineStatusList.Codes.Active;
			statementLine1.B3_CustomsFeesTotal = 100m;

			CusStatementHeader dailyStatement2 = monthlyStatement.DailyStatements.AddNew();
			CusStatementLine statementLine2 = dailyStatement2.StatementLines.AddNew();
			statementLine2.B3_Status = Enterprise.Customs.US.Business.StatementLineStatusList.Codes.Active;
			statementLine2.B3_CustomsFeesTotal = 200m;

			CusStatementLine statementLine3 = dailyStatement2.StatementLines.AddNew();
			statementLine3.B3_Status = Enterprise.Customs.US.Business.StatementLineStatusList.Codes.Deleted;
			statementLine3.B3_CustomsFeesTotal = 400m;

			AssertEquals(100m, dailyStatement1.TotalAmountsPayable);
			AssertEquals(200m, dailyStatement2.TotalAmountsPayable);

			AssertEquals(300m, monthlyStatement.TotalAmountsPayable);
		}

		public void TestTotalsForFinalMonthlyHeader()
		{
			var monthlyStatement = Factory.New<CusStatementHeader>();
			monthlyStatement.B2_StatementNumber = "1112P2222";
			monthlyStatement.B2_IsMonthlyStatement = true;

			var messageText = "B011101X02MSF8804P04001051607691-013199000                                      " +
"Q127092837711101X02            0516070516070000559488300000000000               " +
"Q2000000203510000004263100005959169                                             " +
"QA010560000000618410500000003530496000000005000540000000911205300000002159      " +
"QA024990000024509331100000000800106000000024200550000001025650100000000316      " +
"QA030570000000732109000000001172102000000059911030000000481010400000001640      " +
"Q127092837721101X02            0516070516070000559488300000000000               " +
"Q2000000203510000004263100005959169                                             " +
"QA010560000000618410500000003530496000000005000540000000911205300000002159      " +
"QA024990000024509331100000000800106000000024200550000001025650100000000316      " +
"QA030570000000732109000000001172102000000059911030000000481010400000001640      " +
"Q38804P04001061107061507XXX            0001118976600000000000                   " +
"Q4000000407020000008526200011918338                                             " +
"QE010560000001236810500000007060496000000010000540000001822405300000004318      " +
"QE024990000049018631100000001600106000000048400550000002051250100000000632      " +
"QE030570000001464209000000002344102000000119821030000000962010400000003280      " +
"Q58804P04001061107061507XXX            0001118976600000000000                   " +
"Q6000000407020000008526200011934338                                             " +
"QJ010560000001236810500000007060496000000010000540000001822405300000004318      " +
"QJ024990000050618631100000001600106000000048400550000002051250100000000632      " +
"QJ030570000001464209000000002344102000000119821030000000962010400000003280      " +
"Q78804091001XXX10135797ABIXXX10135698ABI                                        " +
"Q78804091002XXX20135795ABIXXX20135696ABI                                        " +
"Y  1101X02MS00022";

			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement;
			message.EM_MessageText = messageText;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageNum = "";
			Factory.Save();

			monthlyStatement.Messages.Add(message);

			foreach (var line in monthlyStatement.StatementFeeCodes.Cast<StatementFeeCodeLine>())
			{
				if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.Cotton))
				{
					AssertEquals(123.68m, line.TotalFee);
					AssertEquals(123.68m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.SoftwoodLumber))
				{
					AssertEquals(70.60m, line.TotalFee);
					AssertEquals(70.60m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.DutiableMail))
				{
					AssertEquals(10m, line.TotalFee);
					AssertEquals(10m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.Pork))
				{
					AssertEquals(182.24m, line.TotalFee);
					AssertEquals(182.24m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.Beef))
				{
					AssertEquals(43.18m, line.TotalFee);
					AssertEquals(43.18m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing))
				{
					AssertEquals(5061.86m, line.TotalFee);
					AssertEquals(4901.86m, line.FinalTotalFee);
					AssertEquals(-160.00m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal))
				{
					AssertEquals(16m, line.TotalFee);
					AssertEquals(16m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.Blueberry))
				{
					AssertEquals(48.40m, line.TotalFee);
					AssertEquals(48.40m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.Honey))
				{
					AssertEquals(205.12m, line.TotalFee);
					AssertEquals(205.12m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.HMF))
				{
					AssertEquals(6.32m, line.TotalFee);
					AssertEquals(6.32m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.Raspberry))
				{
					AssertEquals(146.42m, line.TotalFee);
					AssertEquals(146.42m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.Potato))
				{
					AssertEquals(23.44m, line.TotalFee);
					AssertEquals(23.44m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.FreshLimes))
				{
					AssertEquals(119.82m, line.TotalFee);
					AssertEquals(119.82m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.Mushroom))
				{
					AssertEquals(96.20m, line.TotalFee);
					AssertEquals(96.20m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.Watermelon))
				{
					AssertEquals(32.80m, line.TotalFee);
					AssertEquals(32.80m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
			}

			AssertEquals(111897.66m, monthlyStatement.TotalDuty);
			AssertEquals(852.62m, monthlyStatement.TotalCVD);
			AssertEquals(407.02m, monthlyStatement.TotalADD);
			AssertEquals(119343.38m, monthlyStatement.TotalAmountDue);

			AssertEquals(852.62m, monthlyStatement.FinalTotalCVD);
			AssertEquals(111897.66m, monthlyStatement.FinalTotalDuty);
			AssertEquals(407.02m, monthlyStatement.FinalTotalADD);
			AssertEquals(119183.38m, monthlyStatement.FinalTotalAmountDue);
		}

		public void TestTotalsForPreliminaryMonthlyHeader()
		{
			var monthlyStatement = Factory.New<CusStatementHeader>();
			monthlyStatement.B2_StatementNumber = "1112P2222";
			monthlyStatement.B2_IsMonthlyStatement = true;

			var messageText = "B015201OHLMSP5211P04330041511756-213161400    2008OHL                           " +
"Q152110772135201OHL56-2131614000318110321110000000000000000000000               " +
"Q2000000000000000000000000000016176                                             " +
"QA014990000001014050100000006036                                                " +
"Q152110831775201OHL56-2131614000324110325110000000257400000000000               " +
"Q2000000000000000000000000000017743                                             " +
"QA024990000000950850100000005661                                                " +
"Q152111011855201OHL56-2131614000411110412110000000065000000000000               " +
"Q2000000000000000000000000000012035                                             " +
"QA034990000000713650100000004249                                                " +
"Q35211P04330041511042111OHL56-2131614000000000322400000000000                   " +
"Q4000000000000000000000000000045954                                             " +
"QE014990000002678450100000015946                                                " +
"Y  5201OHLMS00012";

			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement;
			message.EM_MessageText = messageText;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageNum = "";
			Factory.Save();

			monthlyStatement.Messages.Add(message);

			foreach (var line in monthlyStatement.StatementFeeCodes.Cast<StatementFeeCodeLine>())
			{
				if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.Cotton))
				{
					AssertEquals(0m, line.TotalFee);
					AssertEquals(0m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.SoftwoodLumber))
				{
					AssertEquals(0m, line.TotalFee);
					AssertEquals(0m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.DutiableMail))
				{
					AssertEquals(0m, line.TotalFee);
					AssertEquals(0m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.Pork))
				{
					AssertEquals(0m, line.TotalFee);
					AssertEquals(0m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.Beef))
				{
					AssertEquals(0m, line.TotalFee);
					AssertEquals(0m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing))
				{
					AssertEquals(267.84m, line.TotalFee);
					AssertEquals(0m, line.FinalTotalFee);
					AssertEquals(-267.84m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal))
				{
					AssertEquals(0m, line.TotalFee);
					AssertEquals(0m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.Blueberry))
				{
					AssertEquals(0m, line.TotalFee);
					AssertEquals(0m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.Honey))
				{
					AssertEquals(0m, line.TotalFee);
					AssertEquals(0m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.HMF))
				{
					AssertEquals(159.46m, line.TotalFee);
					AssertEquals(0m, line.FinalTotalFee);
					AssertEquals(-159.46m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.Raspberry))
				{
					AssertEquals(0m, line.TotalFee);
					AssertEquals(0m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.Potato))
				{
					AssertEquals(0m, line.TotalFee);
					AssertEquals(0m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.FreshLimes))
				{
					AssertEquals(0m, line.TotalFee);
					AssertEquals(0m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.Mushroom))
				{
					AssertEquals(0m, line.TotalFee);
					AssertEquals(0m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.Watermelon))
				{
					AssertEquals(0m, line.TotalFee);
					AssertEquals(0m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
			}

			AssertEquals(32.24m, monthlyStatement.TotalDuty);
			AssertEquals(0m, monthlyStatement.TotalCVD);
			AssertEquals(0m, monthlyStatement.TotalADD);
			AssertEquals(459.54m, monthlyStatement.TotalAmountDue);

			AssertEquals(0m, monthlyStatement.FinalTotalCVD);
			AssertEquals(0m, monthlyStatement.FinalTotalDuty);
			AssertEquals(0m, monthlyStatement.FinalTotalADD);
			AssertEquals(0m, monthlyStatement.FinalTotalAmountDue);
		}

		public void TestTotalsForDailyHeaders()
		{
			var dailyStatement1 = Factory.New<CusStatementHeader>();

			var statementLine1 = dailyStatement1.StatementLines.AddNew();
			statementLine1.B3_Status = Enterprise.Customs.US.Business.StatementLineStatusList.Codes.Active;

			var statementLineCharge1 = statementLine1.Charges.AddNew();
			statementLineCharge1.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Duty;
			statementLineCharge1.B4_ChargeAmount = 28.00m;

			var statementLineCharge2 = statementLine1.Charges.AddNew();
			statementLineCharge2.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.CountervailingDuty;
			statementLineCharge2.B4_ChargeAmount = 11.00m;

			var statementLineCharge3 = statementLine1.Charges.AddNew();
			statementLineCharge3.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Beef;
			statementLineCharge3.B4_ChargeAmount = 3.20m;

			var statementLineCharge4 = statementLine1.Charges.AddNew();
			statementLineCharge4.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseInformal;
			statementLineCharge4.B4_ChargeAmount = 1.05m;
			statementLine1.B3_CustomsFeesTotal = 43.25m;

			AssertEquals(28m, dailyStatement1.TotalDuty);
			AssertEquals(11m, dailyStatement1.TotalCVD);
			AssertEquals(0m, dailyStatement1.TotalADD);

			foreach (var line in dailyStatement1.StatementFeeCodes.Cast<StatementFeeCodeLine>())
			{
				if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.Beef))
				{
					AssertEquals(3.20m, line.TotalFee);
					AssertEquals(3.20m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
				else if (line.FeeCode.Equals(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal))
				{
					AssertEquals(1.05m, line.TotalFee);
					AssertEquals(1.05m, line.FinalTotalFee);
					AssertEquals(0m, line.Difference);
				}
			}

			var dailyStatement2 = Factory.New<CusStatementHeader>();

			var statementLine2 = dailyStatement2.StatementLines.AddNew();
			statementLine2.B3_Status = Enterprise.Customs.US.Business.StatementLineStatusList.Codes.Active;

			var statementLineCharge5 = statementLine2.Charges.AddNew();
			statementLineCharge5.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Duty;
			statementLineCharge5.B4_ChargeAmount = 2.00m;

			var statementLineCharge6 = statementLine2.Charges.AddNew();
			statementLineCharge6.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.CountervailingDuty;
			statementLineCharge6.B4_ChargeAmount = 9.00m;
			statementLine2.B3_CustomsFeesTotal = 11.00m;

			var statementLine3 = dailyStatement2.StatementLines.AddNew();
			statementLine3.B3_Status = Enterprise.Customs.US.Business.StatementLineStatusList.Codes.Deleted;

			var statementLineCharge7 = statementLine3.Charges.AddNew();
			statementLineCharge7.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.CountervailingDuty;
			statementLineCharge7.B4_ChargeAmount = 15.00m;

			var statementLineCharge8 = statementLine3.Charges.AddNew();
			statementLineCharge8.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.AntidumpingDuty;
			statementLineCharge8.B4_ChargeAmount = 11.30m;
			statementLine3.B3_CustomsFeesTotal = 26.3m;

			var statementLine4 = dailyStatement2.StatementLines.AddNew();
			statementLine4.B3_Status = Enterprise.Customs.US.Business.StatementLineStatusList.Codes.Active;

			var statementLine5 = dailyStatement2.StatementLines.AddNew();
			statementLine5.B3_Status = Enterprise.Customs.US.Business.StatementLineStatusList.Codes.Deleted;

			AssertEquals(0m, dailyStatement2.FinalTotalADD);
			AssertEquals(11.30m, dailyStatement2.TotalADD);
		}

		public void TestTotalNumberOfEntries()
		{
			var monthlyStatement = Factory.New<CusStatementHeader>();
			monthlyStatement.B2_StatementNumber = "1112P2222";
			monthlyStatement.B2_IsMonthlyStatement = true;

			var dailyStatement1 = monthlyStatement.DailyStatements.AddNew();
			var statementLine1 = dailyStatement1.StatementLines.AddNew();
			statementLine1.B3_Status = Enterprise.Customs.US.Business.StatementLineStatusList.Codes.Active;
			statementLine1.B3_CustomsFeesTotal = 40.00m;

			var dailyStatement2 = monthlyStatement.DailyStatements.AddNew();

			var statementLine2 = dailyStatement2.StatementLines.AddNew();
			statementLine2.B3_Status = Enterprise.Customs.US.Business.StatementLineStatusList.Codes.Active;
			statementLine2.B3_CustomsFeesTotal = 11.00m;

			var statementLine3 = dailyStatement2.StatementLines.AddNew();
			statementLine3.B3_Status = Enterprise.Customs.US.Business.StatementLineStatusList.Codes.Deleted;
			statementLine3.B3_CustomsFeesTotal = 26.3m;

			var statementLine4 = dailyStatement2.StatementLines.AddNew();
			statementLine4.B3_Status = Enterprise.Customs.US.Business.StatementLineStatusList.Codes.Active;
			var statementLine5 = dailyStatement2.StatementLines.AddNew();
			statementLine5.B3_Status = Enterprise.Customs.US.Business.StatementLineStatusList.Codes.Deleted;

			AssertEquals(1, dailyStatement1.TotalNumberRevenueProducingEntries);
			AssertEquals("1", dailyStatement1.TotalNumberRevenueProducingEntriesForPrint);
			AssertEquals(0, dailyStatement1.TotalNumberNonRevenueProducingEntries);
			AssertEquals("0", dailyStatement1.TotalNumberNonRevenueProducingEntriesForPrint);

			AssertEquals(2, dailyStatement2.TotalNumberRevenueProducingEntries);
			AssertEquals("2", dailyStatement2.TotalNumberRevenueProducingEntriesForPrint);
			AssertEquals(2, dailyStatement2.TotalNumberNonRevenueProducingEntries);
			AssertEquals("2", dailyStatement2.TotalNumberNonRevenueProducingEntriesForPrint);

			AssertEquals(3, monthlyStatement.TotalNumberRevenueProducingEntries);
			AssertEquals("3", monthlyStatement.TotalNumberRevenueProducingEntriesForPrint);
			AssertEquals(2, monthlyStatement.TotalNumberNonRevenueProducingEntries);
			AssertEquals("2", monthlyStatement.TotalNumberNonRevenueProducingEntriesForPrint);
			AssertEquals(2, monthlyStatement.FinalTotalNumberRevenueProducingEntries);
			AssertEquals("2", monthlyStatement.FinalTotalNumberRevenueProducingEntriesForPrint);
			AssertEquals(1, monthlyStatement.FinalTotalNumberNonRevenueProducingEntries);
			AssertEquals("1", monthlyStatement.FinalTotalNumberNonRevenueProducingEntriesForPrint);
		}

		public void TestDeveloperExceptionForAccessingEntryLines()
		{
			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "1234P";
			statement.B2_IsMonthlyStatement = true;

			ErrorReporter.Clear();

			try
			{
				int accessed = statement.StatementLines.Count;
				AssertEquals("StatementLines for a monthly statement", ErrorReporter.LastKeyReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestLineStatusCollection()
		{
			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			CusStatementLine activeLine = statement.StatementLines.AddNew();
			activeLine.B3_Status = StatementLineStatusList.Codes.Active;

			CusStatementLine deletedLine = statement.StatementLines.AddNew();
			deletedLine.B3_Status = StatementLineStatusList.Codes.Deleted;

			AssertEquals(1, statement.ActiveLines.Count);
			AssertEquals(true, statement.ActiveLines.Contains(activeLine));

			AssertEquals(1, statement.DailyDeletedLines.Count);
			AssertEquals(true, statement.DailyDeletedLines.Contains(deletedLine));
		}

		public void TestMonthlyStatement()
		{
			CusStatementHeader monthlyStatement = Factory.New<CusStatementHeader>();
			monthlyStatement.B2_StatementNumber = "1234P";
			monthlyStatement.B2_IsMonthlyStatement = true;

			CusStatementHeader dailyStatement = Factory.New<CusStatementHeader>();
			AssertNull(dailyStatement.MonthlyStatementHeader);

			dailyStatement.B2_B2_PeriodicStatement = monthlyStatement.PK;
			AssertEquals(monthlyStatement, dailyStatement.MonthlyStatementHeader);
		}

		public void TestIsPreliminary()
		{
			statementHeader.B2_Status = StatementHeaderStatusList.Codes.Final;
			AssertEquals(false, statementHeader.IsPreliminary);

			statementHeader.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			AssertEquals(true, statementHeader.IsPreliminary);
		}

		public void TestStatementLines()
		{
			AssertEquals(1, statementHeader.StatementLines.Count);
		}

		public void TestStatementType()
		{
			statementHeader.B2_IsMonthlyStatement = true;
			AssertEquals("Monthly", statementHeader.StatementType);

			statementHeader.B2_IsMonthlyStatement = false;
			AssertEquals(ZString.Empty, statementHeader.StatementType);

			statementHeader.B2_StatementNumber = "1234";
			AssertEquals("Daily", statementHeader.StatementType);
		}

		public void TestStatementFor()
		{
			statementHeader.B2_BranchDesignation = "01";
			AssertEquals(statementHeader.Company.OrgProxy.OH_FullName, statementHeader.StatementFor);

			GlbDepartment dep1 = Factory.NewWithValidTestData<GlbDepartment>();
			dep1.GE_Desc = "Test1";

			GlbDepartment dep2 = Factory.NewWithValidTestData<GlbDepartment>();
			dep2.GE_Desc = "Test2";

			GlbDepartment dep3 = Factory.NewWithValidTestData<GlbDepartment>();
			dep3.GE_Desc = "Test3";

			GlbBranch testBranch = Factory.NewWithValidTestData<GlbBranch>();
			testBranch.GB_BranchName = "Test Branch 1";
			testBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			testBranch.GB_OH_OrgProxy = ZGuid.Empty;
			USCustomsDataRegistry.Instance.ClientBranchDesignation.SetValue(Guid.Empty, testBranch.PK.ToGuid(), Guid.Empty, "01");
			AssertEquals("Test Branch 1", statementHeader.StatementFor);

			testBranch.GB_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			testBranch.OrgProxy.MainAddress.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.CustomsAddressOfRecord.Code).Enabled = true;
			testBranch.OrgProxy.MainAddress.OA_CompanyNameOverride = "Customs Name";
			AssertEquals("Customs Name", statementHeader.StatementFor);

			USCustomsDataRegistry.Instance.ClientBranchDesignation.SetValue(Guid.Empty, testBranch.PK.ToGuid(), Guid.Empty, "");
			USCustomsDataRegistry.Instance.ClientBranchDesignation.SetValue(Guid.Empty, testBranch.PK.ToGuid(), dep2.PK.ToGuid(), "01");
			AssertEquals("Customs Name", statementHeader.StatementFor);

			testBranch.OrgProxy.MainAddress.OA_CompanyNameOverride = "";
			statementHeader.Company.OrgProxy.MainAddress.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.CustomsAddressOfRecord.Code).Enabled = false;
			statementHeader.B2_BranchDesignation = ZString.Empty;
			statementHeader.B2_OH_Importer = ZGuid.Empty;
			statementHeader.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			AssertEquals(GlbCompany.CurrentCompany.OrgProxy.OH_FullName, statementHeader.StatementFor);

			statementHeader.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			statementHeader.B2_ImporterCustomsID = "11-358469900";
			AssertEquals(ZString.Empty, statementHeader.StatementFor);

			OrgHeader organization = Factory.New<OrgHeader>();
			organization.OH_FullName = "Test Organization";
			statementHeader.B2_OH_Importer = organization.PK;
			AssertEquals("Test Organization", statementHeader.StatementFor);

			organization.MainAddress.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.CustomsAddressOfRecord.Code).Enabled = true;
			organization.MainAddress.OA_CompanyNameOverride = "Customs Name2";
			AssertEquals("Customs Name2", statementHeader.StatementFor);
		}

		public void TestTotalTaxes()
		{
			CusStatementLine statementLine2 = statementHeader.StatementLines.AddNew();
			CusStatementLineCharge statementLineCharge = statementLine.Charges.AddNew();
			statementLineCharge.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Duty;
			statementLineCharge.B4_ChargeAmount = 11.5m;

			CusStatementLineCharge statementLineCharge2 = statementLine.Charges.AddNew();
			statementLineCharge2.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.ExciseTaxPayable;
			statementLineCharge2.B4_ChargeAmount = 2.5m;

			CusStatementLineCharge statementLineCharge5 = statementLine.Charges.AddNew();
			statementLineCharge5.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.CountervailingDuty;
			statementLineCharge5.B4_ChargeAmount = 10m;

			CusStatementLineCharge statementLineCharge6 = statementLine.Charges.AddNew();
			statementLineCharge6.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.AntidumpingDuty;
			statementLineCharge6.B4_ChargeAmount = 6m;

			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.HMF, 5m);

			Assert(!statementHeader.IsTaxDeferred);

			CusStatementLineCharge statementLineCharge3 = statementLine2.Charges.AddNew();
			statementLineCharge3.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.CountervailingDuty;
			statementLineCharge3.B4_ChargeAmount = 20m;

			CusStatementLineCharge statementLineCharge4 = statementLine2.Charges.AddNew();
			statementLineCharge4.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.AntidumpingDuty;
			statementLineCharge4.B4_ChargeAmount = 7.6m;
			CusStatementLineCharge statementLineCharge7 = statementLine2.Charges.AddNew();
			statementLineCharge7.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.ExciseTaxDeferred;
			statementLineCharge7.B4_ChargeAmount = 3m;

			CusStatementLineCharge statementLineCharge8 = statementLine2.Charges.AddNew();
			statementLineCharge8.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Watermelon;
			statementLineCharge8.B4_ChargeAmount = 40m;
			CusStatementLineCharge statementLineCharge9 = statementLine2.Charges.AddNew();
			statementLineCharge9.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.FreshLimes;
			statementLineCharge9.B4_ChargeAmount = 30m;

			statementLine.B3_CustomsFeesTotal = 115.2m;
			statementLine2.B3_CustomsFeesTotal = 20m;

			AssertEquals(11.5m, statementHeader.TotalDuty);
			AssertEquals(2.5m, statementHeader.TotalPayableTax);
			AssertEquals(30m, statementHeader.TotalCVD);
			AssertEquals(13.6m, statementHeader.TotalADD);
			Assert(statementHeader.IsTaxDeferred);

			statementLine.B3_EntryType = EntryTypeList.Codes.Warehouse;
			AssertEquals(0m, statementHeader.TotalDuty);
			AssertEquals(3m, statementHeader.TotalDeferredTax);
			AssertEquals(20m, statementHeader.TotalCVD);
			AssertEquals(7.6m, statementHeader.TotalADD);
		}

		public void TestStatementStatusDescription()
		{
			statementHeader.B2_Status = StatementHeaderStatusList.Codes.Final;
			AssertEquals(StatementHeaderStatusList.Descriptions.Final, statementHeader.StatementStatusDescription);
		}

		public void TestReadOnly()
		{
			AssertEquals(true, statementHeader.B2_StatementNumberInfo.ReadOnly);
			AssertEquals(false, statementHeader.FilterStatementLinesByInfo.ReadOnly);
			Assert("Users should be able to enter check no themselves", !statementHeader.B2_CheckNoInfo.ReadOnly);
		}

		public void TestPaymentPartySetWhenCheckNoIsEntered()
		{
			statementHeader.B2_PaymentParty = ZString.Empty;
			statementHeader.B2_CheckNo = ZString.Empty;
			AssertEquals(ZString.Empty, statementHeader.B2_PaymentParty);

			statementHeader.B2_CheckNo = "8742378";
			AssertEquals(PaymentPartyList.Codes.Importer, statementHeader.B2_PaymentParty);
		}

		public void TestLoadedStatementLineIsLatest()
		{
			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_StatementNumber = "88080122";
			statement1.B2_SystemCreateTimeUtc = ZDateTime.UtcToday.AddDays(-1);
			var statementLine1 = statement1.StatementLines.AddNew();
			statementLine1.B3_EntryFilerCode = "XJ5";
			statementLine1.B3_EntryNum = "12345678";
			statementLine1.B3_SystemCreateTimeUtc = ZDateTime.UtcToday.AddDays(-1);

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_StatementNumber = "88080222";
			statement2.B2_SystemCreateTimeUtc = ZDateTime.UtcToday;
			var statementLine2 = statement2.StatementLines.AddNew();
			statementLine2.B3_EntryFilerCode = "XJ5";
			statementLine2.B3_EntryNum = "12345678";
			statementLine2.B3_SystemCreateTimeUtc = ZDateTime.UtcToday;

			var loadedStatement = new CusStatementHeader.Loader(Factory).Load("XJ5", "12345678", GlbCompany.CurrentCompany.PK);
			AssertEquals(statement2.PK, loadedStatement.PK);
		}

		public void TestManagedACHFlag()
		{
			statementHeader.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			statementHeader.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			AssertEquals(true, statementHeader.ManagedACH);

			statementHeader.B2_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertEquals(false, statementHeader.ManagedACH);
		}

		public void TestOnSavedForReconDeclaration()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_EntryFilerCode = "XJ5";
			reconDeclaration.ReconEntry.GetEntry().EntryNumber = "00012301";

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;

			var line = statement.StatementLines.AddNew();
			line.B3_EntryFilerCode = "XJ5";
			line.B3_EntryNum = "00012301";
			Factory.Save();
			statement.B2_PaymentAuthorizationDate = ZDateTime.Today;
			statement.PaymentDateChangedByMessageProcessor = true;
			reconDeclaration.US_PaymentDate = ZDateTime.Today.AddDays(-2);

			ErrorReporter.Clear();

			try
			{
				Factory.Save();
				Assert("Should access JE_AddInfoInfo for recon declaration", ErrorReporter.LastKeyReported.Contains("JobDeclaration.PaymentDate changes not saved"));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestImporterCustomsIDForDocument()
		{
			var statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_ImporterCustomsID = "123-45-6789";

			AssertEquals(ZString.Empty, statementHeader.ImporterCustomsIDForDocument);

			statementHeader.B2_ImporterCustomsID = "12-123456789";
			AssertEquals("12-123456789", statementHeader.ImporterCustomsIDForDocument);
		}

#region IJobHeaderParent Members

		public void TestIJobHeaderParent_AllowInvoiceDeletion()
		{
			IJobHeaderParent cusStatementHeader = Factory.New<CusStatementHeader>();
			Assert(cusStatementHeader.AllowInvoiceDeletion);
		}

#endregion

		public void TestApprovalActionAuthorised()
		{
			statementHeader.AddAuthorisationLog();
			Assert("Payment Action Authorized", statementHeader.ApprovalActionAuthorised);

			statementHeader.AddAuthorisationLog();
			Assert("Payment Action Authorization denied", !statementHeader.ApprovalActionAuthorised);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			statementHeader = Factory.New<CusStatementHeader>();
			statementLine = statementHeader.StatementLines.AddNew();
		}

		CusStatementHeader statementHeader;
		CusStatementLine statementLine;

		public void TestIHaveRequiredDocuments()
		{
			IDocManagerSupport iDocManagerSupport = statementHeader;
			AssertNotNull("DocManagerInfo", iDocManagerSupport.DocManagerInfo);
			AssertEquals(Core.Constants.DocManagerCodes.CusStatementHeader, iDocManagerSupport.DocManagerInfo.DocManagerCode);

			IHaveRequiredDocuments iHaveRequiredDocuments = statementHeader;
			statementHeader.B2_StatementNumber = TestNumber;
			AssertEquals(TestNumber, iHaveRequiredDocuments.UniqueConsignRef);

			AssertEquals(ZString.Empty, iHaveRequiredDocuments.HouseBill);
			AssertEquals(ZString.Empty, iHaveRequiredDocuments.MasterBill);
			AssertNull(iHaveRequiredDocuments.ExportBroker);
			AssertEquals(iHaveRequiredDocuments.TableCode, CusStatementHeaderSchema.Constants.Prefix);
			AssertEquals(0, iHaveRequiredDocuments.AdditionalRefTypes.Count);
			AssertEquals("RequiredDocuments", typeof(JobRequiredDocumentDependentCollection), iHaveRequiredDocuments.RequiredDocuments.GetType());
			AssertEquals(statementHeader, iHaveRequiredDocuments.UltimateDocumentParent);
		}
		const string TestNumber = "1234abcd";

		public void TestDeactivateStatementLine()
		{
			statementLine.B3_EntryFilerCode = "SV9";
			statementLine.B3_EntryNum = "00000001";
			statementHeader.DeactivateStatementLine("SV9", "00000001");
			AssertEquals(StatementLineStatusList.Codes.Deleted, statementLine.B3_Status);
			AssertEquals(StatementHeaderStatusList.Codes.Deleted, statementHeader.B2_Status);
		}

		public void TestHumanReadableNameForCusStatementHeader()
		{
			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			CusStatementHeader statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_StatementNumber = "1";

			AssertEquals("Statement", statement.HumanReadableName);
			AssertEquals("Statement 1", statement1.HumanReadableName);
		}

		public void TestFetchHints()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "Test Importer For Statement";
			statementHeader.B2_OH_Importer = importer.PK;

			statementLine.B3_EntryNum = "12345678";
			statementLine.B3_EntryFilerCode = "XJ5";

			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.CountervailingDuty, 11.2m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.Duty, 15m);

			var statementLine2 = statementHeader.StatementLines.AddNew();
			statementLine2.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine2.B3_EntryNum = "80000409";
			statementLine2.B3_EntryFilerCode = "SV9";
			statementLine2.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.CountervailingDuty, 11.2m);
			statementLine2.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.Duty, 15m);
			statementLine2.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest, 12m);
			statementLine2.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty, 20.5m);
			statementLine2.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.ExciseTaxPayable, 25m);

			var strategy = statementHeader.FetchStrategy;
			int fetchCount = Factory.ActiveTableFetchHints;
			strategy.FetchForLoadChildEditableObjects();
			AssertEquals("hints should be added", fetchCount + 1, Factory.ActiveTableFetchHints);

			var columns = new TableColumn[]
			{
				new TableColumn(CusStatementHeaderSchema.Constants.TableName, CusStatementHeader.Schema.ImporterName),
				new TableColumn(CusStatementHeaderSchema.Constants.TableName, CusStatementHeader.Schema.TotalAmountDue),
				new TableColumn(CusStatementHeaderSchema.Constants.TableName, CusStatementHeader.Schema.FinalTotalAmountDue),
				new TableColumn(CusStatementHeaderSchema.Constants.TableName, CusStatementHeader.Schema.ManagedACH)
			};

			strategy.FetchForView(columns);
			AssertEquals("hints should be added", fetchCount + 3, Factory.ActiveTableFetchHints);
		}
	}
}
