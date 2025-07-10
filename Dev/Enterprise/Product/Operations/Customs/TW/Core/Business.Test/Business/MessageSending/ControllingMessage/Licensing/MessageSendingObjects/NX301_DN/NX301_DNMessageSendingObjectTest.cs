using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX301_DNMessageSendingObject))]
	sealed class NX301_DNMessageSendingObjectTest : LicensingMessageSendingObjectTest<NX301_DNMessageSendingObject>
	{
		protected override NX301_DNMessageSendingObject GetMessageSendingObject(CusTWControllingMessageHeader header) => new NX301_DNMessageSendingObjectForTesting(header);

		[ExpectNoExceptions]
		protected override void TestPackaging()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.Packaging, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IPackaging)), "Packaging - should be [null]");
		}

		[ExpectNoExceptions]
		protected override void TestAcceptanceDateTime()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.AcceptanceDateTime, NUnit.Framework.Is.EqualTo(ZDate.Empty), "AcceptanceDateTime");
		}

		[ExpectNoExceptions]
		public void TestCodeDescriptionPairList()
		{
			(var messageSendingObject, _, _) = SetupData();
			var testObject = messageSendingObject as NX301_DNMessageSendingObjectForTesting;
			CombineAssertions(() =>
			{
				testObject.HeaderExposed.TW1_BusinessType = CPT_111_BusinessTypeList.Codes.Inspection;
				NUnit.Framework.Assert.That(messageSendingObject.TypeList, NUnit.Framework.Is.TypeOf<NX301_DN_ATypeList>());

				testObject.HeaderExposed.TW1_BusinessType = CPT_111_BusinessTypeList.Codes.ExemptionFromInspection;
				NUnit.Framework.Assert.That(messageSendingObject.TypeList, NUnit.Framework.Is.TypeOf<NX301_DN_CTypeList>());

				testObject.HeaderExposed.TW1_BusinessType = CPT_111_BusinessTypeList.Codes.Recheck;
				NUnit.Framework.Assert.That(messageSendingObject.TypeList, NUnit.Framework.Is.TypeOf<CodeDescriptionPairList>());
			});
		}

		[ExpectNoExceptions]
		public void TestDefaultAction()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.Action, NUnit.Framework.Is.EqualTo(NX301_DNActionCodeList.Codes._9).Using(CustomComparers.TypeComparison));
		}

		protected override string ExpectedEM_MessageType => "31D";

		class NX301_DNMessageSendingObjectForTesting : NX301_DNMessageSendingObject
		{
			public NX301_DNMessageSendingObjectForTesting(CusTWControllingMessageHeader header) : base(header)
			{
				HeaderExposed = header;
			}

			public CusTWControllingMessageHeader HeaderExposed { get; set; }
		}
	}
}
