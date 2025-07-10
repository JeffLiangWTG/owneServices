using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX401ExportMessageSendingObject))]
	sealed class NX401ExportMessageSendingObjectTest : NX401MessageSendingObjectTest<NX401ExportMessageSendingObject>
	{
		protected override NX401MessageSendingObject GetMessageSendingObject(CusTWControllingMessageHeader header) => new NX401ExportMessageSendingObject(header);

		[ExpectNoExceptions]
		protected override void TestGoodsShipment()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.GoodsShipment, NUnit.Framework.Is.TypeOf(typeof(NX401ExportLicensingMessageGoodsShipment)));
		}

		[ExpectNoExceptions]
		protected override void TestImporter()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.Importer, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IPartyDetails)), "Importer should be null when EXP - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestBorderTransportMeansType()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.BorderTransportMeans, NUnit.Framework.Is.TypeOf<NX401ExportLicensingMessageTransportMeans>());
		}
	}
}
