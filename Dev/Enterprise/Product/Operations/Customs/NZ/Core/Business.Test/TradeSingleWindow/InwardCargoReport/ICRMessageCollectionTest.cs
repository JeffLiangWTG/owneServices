using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	using Enterprise.Customs.NZ.Business.Declaration;
	using Enterprise.Customs.NZ.TradeSingleWindow;
	using Enterprise.Freight.Forwarding.Business;
	using NUnit.Framework;

	[TestedType(typeof(ICRMessageCollection))]
	public class ICRMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestElementsOnContruction()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Messages.AddNew();
			consol.Messages.AddNew();
			consol.Messages.AddNew();
			var message1 = consol.Messages.AddNew(typeof(ICRMessage));
			var message2 = consol.Messages.AddNew(typeof(NZCMessage));
			message2.EM_MessageType = MessageTypeList.Codes.TWR;

			var testCollection = new ICRMessageCollection(consol);
			testCollection.Load();
			AssertEquals("Collection is populated", 2, testCollection.Count);
			Assert("Collection is populated with Outward Report Message", testCollection.Contains(message1));
			Assert("Collection is populated with Response Message", testCollection.Contains(message2));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var consol = Factory.New<ForwardingConsol>();
			return new ICRMessageCollection(consol);
		}
	}
}
