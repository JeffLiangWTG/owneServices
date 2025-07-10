using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CustomsResponseCollection))]
	sealed class CustomsResponseCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestEDIMessageCollection()
		{
			var message1 = Factory.New<CUSRESEDIMessage>();
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.SouthAfricanCustoms;
			message1.EM_MessageType = SARSEDIMessage.MessageTypes.CUSRES;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageText = ZA.Business.Testing.CUSRESMessageProcessorTest.TestMessageResNo;
			var message2 = Factory.New<CUSRESEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAACI;
			message2.EM_MessageType = SARSEDIMessage.MessageTypes.CUSRES;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageText = ZA.Business.Testing.CUSRESMessageProcessorTest.TestMessageResNo;
			var message3 = Factory.New<CUSRESEDIMessage>();
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.SouthAfricanCustoms;
			message3.EM_MessageType = SARSEDIMessage.MessageTypes.CUSDEC;
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message3.EM_MessageText = ZA.Business.Testing.CUSRESMessageProcessorTest.TestMessageResNo;
			var message4 = Factory.New<CUSRESEDIMessage>();
			message4.EM_ApplicationCode = EDIMessage.ApplicationCodes.SouthAfricanCustoms;
			message4.EM_MessageType = SARSEDIMessage.MessageTypes.CUSRES;
			message4.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message4.EM_MessageText = ZA.Business.Testing.CUSRESMessageProcessorTest.TestMessageResNo;
			Factory.Save();
			var collection = new CustomsResponseCollection(Factory);
			collection.Load();
			AssertCollectionContains(message1, collection);
			AssertCollectionNotContains(message2, collection);
			AssertCollectionNotContains(message3, collection);
			AssertCollectionNotContains(message4, collection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CUSRESEDIMessage>();

		protected override BusinessObjectCollection GetCollectionToTest() => new CustomsResponseCollection(Factory);
	}
}
