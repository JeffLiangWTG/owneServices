using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX601MessageSendingObject))]
	sealed class NX601MessageSendingObjectTest : LicensingMessageSendingObjectTest<NX601MessageSendingObject>
	{
		protected override NX601MessageSendingObject GetMessageSendingObject(CusTWControllingMessageHeader header) => new NX601MessageSendingObject(header);

		[ExpectNoExceptions]
		protected override void TestApplication()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.Application, NUnit.Framework.Is.TypeOf(typeof(NX601LicensingMessageApplication)));
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
		protected override string ExpectedEM_MessageType => "601";
	}
}
