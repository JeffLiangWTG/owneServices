using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX301MessageSendingObject))]
	sealed class NX301MessageSendingObjectTest : LicensingMessageSendingObjectTest<NX301MessageSendingObject>
	{
		protected override NX301MessageSendingObject GetMessageSendingObject(CusTWControllingMessageHeader header)
		{
			return new NX301MessageSendingObject(header);
		}

		[ExpectNoExceptions]
		public void TestCodeDescriptionPairList()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.TypeList, NUnit.Framework.Is.TypeOf<NX301TypeList>());
		}

		[ExpectNoExceptions]
		public void TestDefaultAction()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.Action, NUnit.Framework.Is.EqualTo(NXCommonActionCodeList.Codes._9).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		protected override void TestGoodsShipment()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.GoodsShipment, NUnit.Framework.Is.TypeOf<LicensingMessageGoodsShipment>());
		}

		protected override string ExpectedEM_MessageType => "301";
	}
}
