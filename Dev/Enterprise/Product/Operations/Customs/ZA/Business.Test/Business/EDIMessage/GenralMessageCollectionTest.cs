using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(GenralMessageCollection))]
	sealed class GenralMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestEDIMessageCollection()
		{
			var message1 = Factory.New<GENRALMessage>();
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.SouthAfricanCustoms;
			message1.EM_MessageType = CustomsResponseMessageTypeList.Codes.GEN;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var message2 = Factory.New<GENRALMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.SouthAfricanCustoms;
			message2.EM_MessageType = CustomsResponseMessageTypeList.Codes.GEN;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			var message3 = Factory.New<GENRALMessage>();
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.SouthAfricanCustoms;
			message3.EM_MessageType = CustomsResponseMessageTypeList.Codes.RES;
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var message4 = Factory.New<GENRALMessage>();
			message4.EM_ApplicationCode = EDIMessage.ApplicationCodes.SouthAfricanCustoms;
			message4.EM_MessageType = CustomsResponseMessageTypeList.Codes.RES;
			message4.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Factory.Save();

			var collection = new GenralMessageCollection(Factory);
			collection.Load();

			AssertCollectionContains(message1, collection);
			AssertCollectionNotContains(message2, collection);
			AssertCollectionNotContains(message3, collection);
			AssertCollectionNotContains(message4, collection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<GENRALMessage>();

		protected override BusinessObjectCollection GetCollectionToTest() => new GenralMessageCollection(Factory);
	}
}
