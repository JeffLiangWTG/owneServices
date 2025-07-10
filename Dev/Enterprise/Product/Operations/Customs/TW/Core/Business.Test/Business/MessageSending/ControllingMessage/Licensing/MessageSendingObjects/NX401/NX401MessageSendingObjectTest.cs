using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestsSubclassesOf(typeof(NX401MessageSendingObject))]
	abstract class NX401MessageSendingObjectTest<T> : LicensingMessageSendingObjectTest<NX401MessageSendingObject>
		where T : NX401MessageSendingObject
	{
		[ExpectNoExceptions]
		protected override void TestConsignment()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.Consignment, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IConsignment)), "Consignment - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestDefaultAction()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.Action, NUnit.Framework.Is.EqualTo(NXCommonActionCodeList.Codes._9).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestAdditionalInformation()
		{
			(var messageSendingObject, var header, _) = SetupData();
			header.TW1_PrePermitNumber = "X1";
			NUnit.Framework.Assert.That(((INX401Declaration)messageSendingObject).AdditionalInformation.StatementDescription, NUnit.Framework.Is.EqualTo("X1").Using(CustomComparers.TypeComparison));
		}

		protected override string ExpectedEM_MessageType => "401";
	}

	[TestedType(typeof(NX401MessageSendingObject))]
	sealed class NX401MessageSendingObjectBaseOnlyTest : NX401MessageSendingObjectTest<NX401MessageSendingObject>
	{
		protected override NX401MessageSendingObject GetMessageSendingObject(CusTWControllingMessageHeader header)
		{
			return new NX401MessageSendingObjectForTesting(header);
		}

		class NX401MessageSendingObjectForTesting : NX401MessageSendingObject
		{
			public NX401MessageSendingObjectForTesting(CusTWControllingMessageHeader header) : base(header)
			{
			}
		}
	}
}
