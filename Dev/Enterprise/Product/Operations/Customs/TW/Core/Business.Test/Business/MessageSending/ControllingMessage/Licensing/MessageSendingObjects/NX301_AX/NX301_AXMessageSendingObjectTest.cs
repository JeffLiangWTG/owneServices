using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX301_AXMessageSendingObject))]
	sealed class NX301_AXMessageSendingObjectTest : LicensingMessageSendingObjectTest<NX301_AXMessageSendingObject>
	{
		protected override NX301_AXMessageSendingObject GetMessageSendingObject(CusTWControllingMessageHeader header) => new NX301_AXMessageSendingObject(header);

		[ExpectNoExceptions]
		protected override void TestApplication()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.Application, NUnit.Framework.Is.TypeOf(typeof(NX301_AXLicensingMessageApplication)));
		}

		[ExpectNoExceptions]
		protected override void TestAcceptanceDateTime()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.AcceptanceDateTime, NUnit.Framework.Is.EqualTo(ZDate.Empty), "AcceptanceDateTime");
		}

		[ExpectNoExceptions]
		public void TestTypeList()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.TypeList, NUnit.Framework.Is.TypeOf<NX301_AXTypeList>());
		}

		[ExpectNoExceptions]
		public void TestDefaultAction()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.Action, NUnit.Framework.Is.EqualTo(NXCommonActionCodeList.Codes._9).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestValidation()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.Validation, NUnit.Framework.Is.TypeOf<NX301_AXMessageSendingObjectValidation>());
		}

		protected override string ExpectedEM_MessageType => "31A";
	}
}
