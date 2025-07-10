using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX201_01LicensingMessageSendingObjectParent))]
	sealed class NX201_01LicensingMessageSendingObjectParentTest : LicensingMessageSendingObjectParentAbstractTest
	{
		[ExpectNoExceptions]
		public void TestMessageSendingObjectType()
		{
			NUnit.Framework.Assert.That(LicensingMessageSendingObjectParentForTesting.SendingObjectsCollection.FirstOrDefault(), NUnit.Framework.Is.TypeOf<NX201_01MessageSendingObject>());
		}

		protected override bool ExpectedShowReasonDescription => true;

		[ExpectNoExceptions]
		public void TestGetNewMessageSendingValidation()
		{
			var parent = CreateLicensingMessageSendingObjectParentForTesting() as NX201_01LicensingMessageSendingObjectParent;
			parent.Declaration.JE_MessageType = "IMP";
			NUnit.Framework.Assert.That(parent.MessageSendingValidation, NUnit.Framework.Is.TypeOf<NX201_01ImportMessageSendingObjectParentValidation>());

			parent = CreateLicensingMessageSendingObjectParentForTesting() as NX201_01LicensingMessageSendingObjectParent;
			parent.Declaration.JE_MessageType = "EXP";
			NUnit.Framework.Assert.That(parent.MessageSendingValidation, NUnit.Framework.Is.TypeOf<NX201_01ExportMessageSendingObjectParentValidation>());
		}

		[ExpectNoExceptions]
		public void TestGetLicensingMessageSendingObjectParentNotificationCollectorCore()
		{
			NUnit.Framework.Assert.That(((NX201_01LicensingMessageSendingObjectParentForTest)LicensingMessageSendingObjectParentForTesting).ExposedLicensingMessageSendingObjectParentNotificationCollector(), NUnit.Framework.Is.TypeOf<NX201_01LicensingMessageSendingObjectParentNotificationCollector>());
		}

		protected override LicensingMessageSendingObjectParent CreateLicensingMessageSendingObjectParentForTesting()
		{
			var decl = Factory.NewWithValidTestData<JobDeclaration>();
			var messageHeader = decl.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX201_01;
			return new NX201_01LicensingMessageSendingObjectParentForTest(decl, "Send Application Message for I/E Permit (NX201_01)");
		}

		class NX201_01LicensingMessageSendingObjectParentForTest : NX201_01LicensingMessageSendingObjectParent
		{
			public NX201_01LicensingMessageSendingObjectParentForTest(JobDeclaration declaration, string menuCaption) : base(declaration, menuCaption)
			{
			}

			public LicensingMessageSendingObjectParentNotificationCollector ExposedLicensingMessageSendingObjectParentNotificationCollector()
			{
				return GetLicensingMessageSendingObjectParentNotificationCollectorCore();
			}
		}
	}
}
