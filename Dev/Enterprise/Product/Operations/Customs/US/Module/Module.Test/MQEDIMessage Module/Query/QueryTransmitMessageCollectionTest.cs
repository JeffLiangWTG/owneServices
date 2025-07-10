using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(QueryTransmitMessageCollection))]
	sealed class QueryTransmitMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCreateAdditionalQuery()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryQuota;
			message.EM_ApplicationCode = Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			message.EM_Status = "!";
			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryQuotaResponse;
			message2.EM_ApplicationCode = Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.USCustomsImport;
			message2.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			message2.EM_Status = "!";
			var message3 = Factory.New<MQEDIMessage>();
			message3.EM_MessageType = ApplicationIdentifierCodeList.Codes.StatementDeleteTransaction;
			message3.EM_ApplicationCode = Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.USCustomsImport;
			message3.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			message3.EM_Status = "!";
			var message4 = Factory.New<MQEDIMessage>();
			message4.EM_MessageType = ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifier;
			message4.EM_ApplicationCode = Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.USCustomsImport;
			message4.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			message4.EM_Status = "!";
			var message5 = Factory.New<MQEDIMessage>();
			message5.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryEntrySummary;
			message5.EM_ApplicationCode = Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.USCustomsImport;
			message5.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			message5.EM_Status = "!";
			var message6 = Factory.New<MQEDIMessage>();
			message6.EM_MessageType = ApplicationIdentifierCodeList.Codes.UserStatistics;
			message6.EM_ApplicationCode = Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.USCustomsImport;
			message6.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			message6.EM_Status = "!";
			var coll = new QueryTransmitMessageCollection(Factory);
			coll.LoadWithMoreFiltering(new ZQuery(EDIMessageSchema.EM_Status, "!"));
			AssertEquals(4, coll.Count);
			AssertEquals(true, coll.Contains(message));
			AssertEquals(true, coll.Contains(message4));
			AssertEquals(true, coll.Contains(message5));
			AssertEquals(true, coll.Contains(message6));
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new QueryTransmitMessageCollection(Factory);
	}
}
