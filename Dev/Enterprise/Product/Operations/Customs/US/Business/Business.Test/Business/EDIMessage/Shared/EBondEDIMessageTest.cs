using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Business.XmlMessaging.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(XmlEDIMessage))]
	sealed class EBondEDIMessageTest : XmlEDIMessageTest
	{
		[UseSnapshotProtection]
		public void TestShouldPopulateUniqueMessageNumberUsingMessageNumberStrategy()
		{
			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();
			var message1 = factory1.New<EBondEDIMessage>();
			message1.MessageNumberStrategy = new MessageNumberStrategyForTest(factory1);
			message1.EM_MessageText = "~TEST~";
			var dbConnection = ((CargoWise.Data.IDbConnected)factory1).Connection;
			message1.OnSaving();
			var messageNum1 = message1.EM_MessageNum;
			dbConnection.RollbackTransaction();
			var message2 = factory2.New<EBondEDIMessage>();
			message2.MessageNumberStrategy = new MessageNumberStrategyForTest(factory2);
			message2.EM_MessageText = "~TEST~";
			factory2.Save();
			var messageNum2 = message2.EM_MessageNum;
			AssertEquals(false, messageNum1.IsEmpty);
			AssertEquals(false, messageNum2.IsEmpty);
			AssertEquals(messageNum1, messageNum2);
			dbConnection.BeginTransaction();
			AssertEquals(messageNum1, message1.EM_MessageNum);
			factory1.Save();
			AssertNotEquals(messageNum1, message1.EM_MessageNum);
		}

		public void TestHasStatusOrErrors()
		{
			var message = Factory.New<EBondEDIMessage>();
			Assert("Default to false.", !message.HasStatusOrErrors);
		}

		public void TestPopulateMessageNumber()
		{
			var message = Factory.NewWithValidTestData<EBondEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Assert("Should default to empty.", message.EM_MessageNum.IsEmpty);
			Factory.Save();
			Assert("Should populate a valid message number when it's saving.", !message.EM_MessageNum.IsEmpty);
			message = Factory.New<EBondEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Assert("Should default to empty.", message.EM_MessageNum.IsEmpty);
			Factory.Save();
			Assert("Should populate a valid message number when it's saving.", !message.EM_MessageNum.IsEmpty);
		}

		public void TestMessageNum()
		{
			var message = Factory.NewWithValidTestData<EBondEDIMessage>();
			Assert("Should default to empty.", message.EM_MessageNum.IsEmpty);
			Factory.Save();
			Assert("Should get a value from the number fountain.", !message.EM_MessageNum.IsEmpty);
		}

		public void TestMessageTypeDescription()
		{
			var message = Factory.New<EBondEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			AssertEquals("eBond Request", message.MessageTypeDescription);
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertEquals("eBond Response", message.MessageTypeDescription);
			message.EM_MessageType = EDIInterchangeTypeList.Codes.XDN;
			AssertEquals("XML Universal Shipment", message.MessageTypeDescription);
		}

		public void TestIsInterpretationInHtmlFormat()
		{
			var message = Factory.New<EBondEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Assert("Should be false when the message type is eBond and transmit.", !message.IsInterpretationInHtmlFormat);
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Assert("Should be true when the message type is eBond and receive.", message.IsInterpretationInHtmlFormat);
		}

		public void TestDefaultValues()
		{
			var message = Factory.New<EBondEDIMessage>();
			AssertEquals(ApplicationCodeList.Codes.USeBond, message.EM_ApplicationCode);
			AssertEquals(EDIInterchangeTypeList.Codes.XDC, message.EM_MessageType);
			AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
		}

		sealed class MessageNumberStrategyForTest : IMessageNumberStrategy
		{
			public MessageNumberStrategyForTest(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			string IMessageNumberStrategy.GetMessageReferenceNumber() => Env.NumberFountains.EDIFACTNumberFountain("M", "USI", "TST").GetNextFormatted(factory);

			readonly BusinessObjectFactory factory;
		}
	}

	[TestedType(typeof(EBondEDIMessage))]
	sealed class EBondEDIMessageBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
	}
}
