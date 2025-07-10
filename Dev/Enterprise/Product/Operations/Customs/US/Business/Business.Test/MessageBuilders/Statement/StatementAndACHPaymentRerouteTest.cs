using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	[TestedType(typeof(StatementAndACHPaymentReroute))]
	sealed class StatementAndACHPaymentRerouteTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2015, 7, 31)]
		public void TestDefaultValues()
		{
			var reroute = new StatementAndACHPaymentReroute();
			AssertEquals(JobApplicationCodeList.Codes.ACE, reroute.Z9_MessageType);
			AssertEquals(ZDate.Today, reroute.Z9_TranmissionDate);
		}

		public void TestIsPeriodicMonthlyStatement()
		{
			var reroute = new StatementAndACHPaymentReroute();
			reroute.Z9_RerouteType = StatementTypeList.Codes.Daily;
			AssertEquals(false, reroute.IsPeriodicMonthlyStatement);
			reroute.Z9_RerouteType = StatementTypeList.Codes.PeriodicMonthly;
			AssertEquals(true, reroute.IsPeriodicMonthlyStatement);
		}

		public void TestHumanReadableName()
		{
			var reroute = new StatementAndACHPaymentReroute();
			AssertEquals("Statement and/or ACH Payment Reroute", reroute.HumanReadableName);
		}

		public void TestSendACSRequestAndSave()
		{
			var reroute = new StatementAndACHPaymentReroute();
			reroute.Z9_MessageType = JobApplicationCodeList.Codes.ACS;
			reroute.Z9_RerouteType = StatementTypeList.Codes.Daily;
			reroute.Z9_ImportOfRecordNumber = "IMP3723423";
			reroute.Z9_ClientBranch = "J3";
			reroute.Z9_StatementNumber = "ST323422";
			reroute.Z9_TranmissionDate = new ZDateTime(2007, 3, 20);
			reroute.Z9_PreliminaryStatementRequest = false;
			reroute.Z9_FinalStatementRequest = true;
			reroute.Z9_ACHPaymentRequest = false;
			reroute.Z9_PeriodicStatementPaymentAuthorizationRequest = true;

			var query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.UtcNow.Date);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			query.AddToFilter(EDIMessageSchema.EM_Status, MQEDIMessage.Status.Queued);
			query.AddToFilter(EDIMessageSchema.EM_MessageText, SQLComparisonOperator.Contains, "ST323422");
			query.AddToFilter(EDIMessageSchema.EM_MessageText, SQLComparisonOperator.Contains, "IMP3723423");

			var abiDailyStatementQuery = new ZQuery(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.ABIStatementACHPaymentReroute);
			abiDailyStatementQuery.AddToFilter(query);

			var abiDailyStatementCount = Factory.GetDatabaseCount(typeof(EDIMessage), abiDailyStatementQuery);
			var periodicStatementQuery = new ZQuery(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatementReroute);
			periodicStatementQuery.AddToFilter(query);

			int periodicStatementCount = Factory.GetDatabaseCount(typeof(EDIMessage), periodicStatementQuery);
			reroute.SendRequest();

			AssertEquals(abiDailyStatementCount + 1, Factory.GetDatabaseCount(typeof(EDIMessage), abiDailyStatementQuery));
			AssertEquals(periodicStatementCount, Factory.GetDatabaseCount(typeof(EDIMessage), periodicStatementQuery));

			reroute.Z9_RerouteType = StatementTypeList.Codes.PeriodicMonthly;
			reroute.SendRequest();

			AssertEquals(abiDailyStatementCount + 1, Factory.GetDatabaseCount(typeof(EDIMessage), abiDailyStatementQuery));
			AssertEquals(periodicStatementCount + 1, Factory.GetDatabaseCount(typeof(EDIMessage), periodicStatementQuery));
		}

		[TestDate(2015, 8, 4)]
		public void TestStatementRequestReroute()
		{
			var entryFiler = new EntryFiler();
			entryFiler.EntryFilerCode = "XJ5";
			USCustomsDataRegistry.Instance.EntryFiler.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, entryFiler);
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "1101");

			var reroute = new StatementAndACHPaymentReroute();
			reroute.Z9_MessageType = JobApplicationCodeList.Codes.ACS;
			reroute.Z9_RerouteType = StatementTypeList.Codes.Daily;
			reroute.Z9_TranmissionDate = ZDate.Today.AddDays(1);
			reroute.Z9_ImportOfRecordNumber = "IMP12345678";
			reroute.Z9_ClientBranch = "A1";
			reroute.Z9_StatementNumber = "ST123456";
			reroute.Z9_ACHPaymentRequest = true;
			reroute.SendRequest();

			var query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.ABIStatementACHPaymentReroute);
			query.AddToFilter(EDIMessageSchema.EM_MessageText, SQLComparisonOperator.Contains, "IMP12345678");
			query.AddToFilter(EDIMessageSchema.EM_MessageText, SQLComparisonOperator.Contains, "ST123456");

			var ediMessages = Factory.Load<EDIMessage>(query);
			AssertEquals(1, ediMessages.Length);
			AssertEquals(@"B011101XJ5QO                                               EDIEDIDAT_1          QR080515IMP12345678 A1ST123456   NNYN                                           Y  1101XJ5QO00001", ediMessages[0].EM_MessageText);

			reroute.Z9_ACHPaymentRequest = false;
			reroute.Z9_RerouteType = StatementTypeList.Codes.PeriodicMonthly;
			reroute.Z9_PreliminaryStatementRequest = true;
			reroute.SendRequest();

			query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatementReroute);
			query.AddToFilter(EDIMessageSchema.EM_MessageText, SQLComparisonOperator.Contains, "IMP12345678");
			query.AddToFilter(EDIMessageSchema.EM_MessageText, SQLComparisonOperator.Contains, "ST123456");

			ediMessages = Factory.Load<EDIMessage>(query);
			AssertEquals(1, ediMessages.Length);
			AssertEquals(@"B011101XJ5MO                                               EDIEDIDAT_2          QR080515IMP12345678 A1ST123456   YN                                             Y  1101XJ5MO00001", ediMessages[0].EM_MessageText);

			reroute.Z9_ImportOfRecordNumber = "IMP12345677";
			reroute.Z9_PreliminaryStatementRequest = false;
			reroute.Z9_MessageType = JobApplicationCodeList.Codes.ACE;
			reroute.Z9_RerouteType = StatementTypeList.Codes.Daily;
			reroute.Z9_ACHPaymentRequest = true;
			reroute.SendRequest();

			query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_MessageType, ACEApplicationIdentifierCodeList.Codes.StatementRequestReroute);
			query.AddToFilter(EDIMessageSchema.EM_MessageText, SQLComparisonOperator.Contains, "IMP12345677");
			query.AddToFilter(EDIMessageSchema.EM_MessageText, SQLComparisonOperator.Contains, "ST123456");

			ediMessages = Factory.Load<EDIMessage>(query);
			AssertEquals(1, ediMessages.Length);
			AssertEquals(@"B  1101XJ5MO                                               EDIEDIDAT_3          QR080515IMP12345677 A1ST123456   NNNN                                           Y  1101XJ5MO", ediMessages[0].EM_MessageText);

			reroute.Z9_ImportOfRecordNumber = "IMP12345676";
			reroute.Z9_ACHPaymentRequest = false;
			reroute.Z9_RerouteType = StatementTypeList.Codes.PeriodicMonthly;
			reroute.Z9_PreliminaryStatementRequest = true;
			reroute.SendRequest();

			query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_MessageType, ACEApplicationIdentifierCodeList.Codes.StatementRequestReroute);
			query.AddToFilter(EDIMessageSchema.EM_MessageText, SQLComparisonOperator.Contains, "IMP12345676");
			query.AddToFilter(EDIMessageSchema.EM_MessageText, SQLComparisonOperator.Contains, "ST123456");

			ediMessages = Factory.Load<EDIMessage>(query);
			AssertEquals(1, ediMessages.Length);
			AssertEquals(@"B  1101XJ5MO                                               EDIEDIDAT_4          QR080515IMP12345676 A1ST123456   NNYN                                           Y  1101XJ5MO", ediMessages[0].EM_MessageText);
		}

		protected override BusinessObject GetNewBusinessObject() => new StatementAndACHPaymentReroute();
	}
}
