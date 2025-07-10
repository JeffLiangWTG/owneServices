using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AutoEntrySummaryQuerySenderTest : TestCaseWithFactory
	{
		[NUnit.Framework.TestDate(2013, 02, 11, 13, 30, 00)]
		public void TestSendIfEligible()
		{
			var now = ZDateTime.Now;
			var timespanNow = new TimeSpan(now.Hour, now.Minute, 0);

			DeclarationTestHelper.SetEntryFilerCode("XJ5");

			var declaration = GetDeclaration(ZDateTime.Empty, false, false, "");
			USCustomsDataRegistry.Instance.AutoQueryEntrySummaries.SetValue(declaration.RegistryCompanyPK, System.Guid.Empty, System.Guid.Empty, true);
			var entry = declaration.ActiveEntryHeaders[0];
			declaration.ImportEntryNumber = "12351231";
			var liquidation = Factory.New<CusLiquidation>();
			liquidation.B8_SystemCreateDate = new ZDateTime(2013, 01, 21);
			liquidation.B8_LiquidationDate = new ZDateTime(2013, 01, 20);
			liquidation.B8_EntryFilerCode = "XJ5";
			liquidation.B8_EntryNumber = "12351231";
			declaration.Liquidations.Add(liquidation);
			new AutoEntrySummaryQuerySender().SendIfEligible(entry);
			AssertEquals("ESQ should not be sent because job already liquidated", 0, entry.Messages.Count);

			var declaration2 = GetDeclaration(ZDateTime.Today.AddDays(-1), true, true, "!22");
			declaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var entry2 = declaration2.CustomsEntryHeaders[0];
			new AutoEntrySummaryQuerySender().SendIfEligible(entry2);
			AssertEquals("ESQ should be sent, because existing ESQ has been sent before Payment Due Date", 3, entry2.Messages.Count);

			var declaration3 = GetDeclaration(ZDateTime.Today.AddDays(-2), true, true, "!23");
			var entry3 = declaration3.ActiveEntryHeaders[0];
			var responseMessage = entry3.Messages.OfType<MQEDIMessage>().FirstOrDefault(x => x.EM_MessageType == ApplicationIdentifierCodeList.Codes.QueryEntrySummaryResponse);
			responseMessage.EM_SystemCreateTimeUtc = declaration3.US_PaymentDueDate.AddDays(AutoEntrySummaryQuerySender.HeldUntilDateDays).Add(timespanNow);
			new AutoEntrySummaryQuerySender().SendIfEligible(entry3);
			AssertEquals("ESQ should not be sent, because existing ESQ response msg date is equal Payment Due Date+3", 2, entry3.Messages.Count);

			var declaration4 = GetDeclaration(ZDateTime.Today.AddDays(-2), true, false, "!24");
			var entry4 = declaration4.ActiveEntryHeaders[0];
			var message = entry4.Messages[0];
			message.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-4);
			message.EM_HeldUntilDate = ZDateTime.Today.AddDays(-4).AddDays(AutoEntrySummaryQuerySender.HeldUntilDateDays);
			new AutoEntrySummaryQuerySender().SendIfEligible(entry4);
			AssertEquals(@"New ESQ should not be sent, but EM_HeldUntilDate should be updated for existing message, 
			because US_PaymentDueDate has changed", 1, entry4.Messages.Count);

			AssertEquals("EM_HeldUntilDate", declaration4.US_PaymentDueDate.AddDays(AutoEntrySummaryQuerySender.HeldUntilDateDays).Add(timespanNow), message.EM_HeldUntilDate);
		}

		[NUnit.Framework.TestDate(2024, 02, 11, 13, 30, 00)]
		public void TestDifferentPaymentTypeHeldUntilDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			USCustomsDataRegistry.Instance.AutoQueryEntrySummaries.SetValue(declaration.RegistryCompanyPK, System.Guid.Empty, System.Guid.Empty, true);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_PaymentDueDate = ZDateTime.Today.AddDays(-2);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			declaration.US_PeriodicStatementMM = "01";
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var autoEntrySummaryQuerySender = new AutoEntrySummaryQuerySender();
			autoEntrySummaryQuerySender.SendIfEligible(entry);
			var message = entry.Messages[0];
			AssertEquals("EM_HeldUntilDate", new ZDateTime(2025, 1, 28, 13, 30, 00), message.EM_HeldUntilDate);

			entry.Messages.RemoveAndDeleteAll();
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			declaration.US_PeriodicStatementMM = "01";
			declaration.US_PaymentDueDate = ZDateTime.Today.AddDays(-5);
			autoEntrySummaryQuerySender.SendIfEligible(entry);
			message = entry.Messages[0];
			AssertEquals("EM_HeldUntilDate", new ZDateTime(2024, 2, 9, 13, 30, 00), message.EM_HeldUntilDate);

			entry.Messages.RemoveAndDeleteAll();
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;
			declaration.US_PeriodicStatementMM = "02";
			declaration.US_PaymentDueDate = ZDateTime.Today.AddDays(-5);
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			autoEntrySummaryQuerySender.SendIfEligible(entry);
			message = entry.Messages[0];
			AssertEquals("Get the last day of month", new ZDateTime(2024, 2, 29, 13, 30, 00), message.EM_HeldUntilDate);
		}

		JobDeclaration GetDeclaration(ZDateTime paymentDueDate, bool addOutgoingESQ, bool addIncomingESQ, string msgNo)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_PaymentDueDate = paymentDueDate;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			if (addOutgoingESQ)
			{
				var message = Factory.New<MQEDIMessage>();
				message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryEntrySummary;
				message.EM_MessageNum = msgNo;
				message.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-2);
				entry.Messages.Add(message);
			}
			if (addIncomingESQ)
			{
				var responseMessage = Factory.New<MQEDIMessage>();
				responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
				responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				responseMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryEntrySummaryResponse;
				responseMessage.EM_MessageNum = msgNo;
				responseMessage.EM_MessageText = "B018888XJ5JR                                               58                   " +
						"J18888XJ5 0000011391-013199000010000000040000000000000                  001   B " +
						"J2XJ5 00000113      0000000000000000000000                               808001B" +
						"J3XJ5 00000113112607071126070112546821485211210789130010715000000002400B00001011" +
						"J5XJ5 00000113071120ESP                                                         " +
						"J98888XJ5 00000113BILLING DATA NOT ON FILE                                      " +
						"J98888XJ5 00000113COLLECTION DATA NOT ON FILE                                   " +
						"Y  8888XJ5JR00006";
				entry.Messages.Add(responseMessage);
			}
			return declaration;
		}
	}
}
