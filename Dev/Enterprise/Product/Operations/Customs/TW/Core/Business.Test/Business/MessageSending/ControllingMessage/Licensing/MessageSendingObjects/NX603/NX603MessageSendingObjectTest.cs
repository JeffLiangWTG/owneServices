using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX603MessageSendingObject))]
	sealed class NX603MessageSendingObjectTest : LicensingMessageSendingObjectTest<NX603MessageSendingObject>
	{
		protected override NX603MessageSendingObject GetMessageSendingObject(CusTWControllingMessageHeader header)
		{
			return new NX603MessageSendingObject(header);
		}

		[ExpectNoExceptions]
		public void TestCodeDescriptionPairList()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.TypeList, NUnit.Framework.Is.TypeOf<NX601_NX603TypeList>());
		}

		[ExpectNoExceptions]
		public void TestDefaultAction()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.Action, NUnit.Framework.Is.EqualTo(NXCommonActionCodeList.Codes._9).Using(CustomComparers.TypeComparison));
		}
		protected override string ExpectedEM_MessageType => "603";
	}
}
