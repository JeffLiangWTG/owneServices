using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX603LicensingMessageSendingObjectParent))]
	sealed class NX603LicensingMessageSendingObjectParentTest : LicensingMessageSendingObjectParentAbstractTest
	{
		[ExpectNoExceptions]
		public void TestMessageSendingObjectType()
		{
			NUnit.Framework.Assert.That(LicensingMessageSendingObjectParentForTesting.SendingObjectsCollection.FirstOrDefault(), NUnit.Framework.Is.TypeOf<NX603MessageSendingObject>());
		}

		[ExpectNoExceptions]
		public void TestGetLicensingMessageSendingObjectParentNotificationCollectorCore()
		{
			NUnit.Framework.Assert.That(((NX603LicensingMessageSendingObjectParentForTest)LicensingMessageSendingObjectParentForTesting).ExposedLicensingMessageSendingObjectParentNotificationCollector(), NUnit.Framework.Is.TypeOf<NX603LicensingMessageSendingObjectParentNotificationCollector>());
		}

		protected override LicensingMessageSendingObjectParent CreateLicensingMessageSendingObjectParentForTesting()
		{
			var decl = Factory.NewWithValidTestData<JobDeclaration>();
			var messageHeader = decl.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX603;
			return new NX603LicensingMessageSendingObjectParentForTest(decl);
		}

		class NX603LicensingMessageSendingObjectParentForTest : NX603LicensingMessageSendingObjectParent
		{
			public NX603LicensingMessageSendingObjectParentForTest(JobDeclaration declaration) : base(declaration)
			{
			}

			public LicensingMessageSendingObjectParentNotificationCollector ExposedLicensingMessageSendingObjectParentNotificationCollector() => GetLicensingMessageSendingObjectParentNotificationCollectorCore();
		}
	}
}
