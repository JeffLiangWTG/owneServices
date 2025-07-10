using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX401ImportMessageSendingObject))]
	sealed class NX401ImportMessageSendingObjectTest : NX401MessageSendingObjectTest<NX401ImportMessageSendingObject>
	{
		protected override NX401MessageSendingObject GetMessageSendingObject(CusTWControllingMessageHeader header) => new NX401ImportMessageSendingObject(header);

		[ExpectNoExceptions]
		protected override void TestGoodsShipment()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.GoodsShipment, NUnit.Framework.Is.TypeOf(typeof(NX401ImportLicensingMessageGoodsShipment)));
		}

		[ExpectNoExceptions]
		protected override void TestImporter()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.Importer, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Messaging.IPartyDetails)), "Importer should not be null when IMP - should not be [null]");
		}
	}
}
