using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX601LicensingMessageSendingObjectParent))]
	sealed class NX601LicensingMessageSendingObjectParentTest : LicensingMessageSendingObjectParentAbstractTest
	{
		[ExpectNoExceptions]
		public void TestMessageSendingObjectType()
		{
			NUnit.Framework.Assert.That(LicensingMessageSendingObjectParentForTesting.SendingObjectsCollection.FirstOrDefault(), NUnit.Framework.Is.TypeOf<NX601MessageSendingObject>());
		}

		[ExpectNoExceptions]
		public void TestGetLicensingMessageSendingObjectParentNotificationCollectorCore()
		{
			NUnit.Framework.Assert.That(((NX601LicensingMessageSendingObjectParentForTest)LicensingMessageSendingObjectParentForTesting).ExposedLicensingMessageSendingObjectParentNotificationCollector(), NUnit.Framework.Is.TypeOf<NX601MessageSendingObjectParentNotificationCollector>());
		}

		[ExpectNoExceptions]
		public void TestGetLicensingMessageSendingObjectAdditionalMessageErrorCollectorCore()
		{
			NUnit.Framework.Assert.That(((NX601LicensingMessageSendingObjectParentForTest)LicensingMessageSendingObjectParentForTesting).ExposedLicensingMessageSendingObjectAdditionalMessageErrorCollector(), NUnit.Framework.Is.TypeOf<NX601MessageSendingObjectAdditionalMessageErrorCollector>());
		}

		protected override LicensingMessageSendingObjectParent CreateLicensingMessageSendingObjectParentForTesting()
		{
			var decl = Factory.NewWithValidTestData<JobDeclaration>();
			var messageHeader = decl.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			return new NX601LicensingMessageSendingObjectParentForTest(decl);
		}

		class NX601LicensingMessageSendingObjectParentForTest : NX601LicensingMessageSendingObjectParent
		{
			public NX601LicensingMessageSendingObjectParentForTest(JobDeclaration declaration) : base(declaration)
			{
			}

			public LicensingMessageSendingObjectParentNotificationCollector ExposedLicensingMessageSendingObjectParentNotificationCollector() => GetLicensingMessageSendingObjectParentNotificationCollectorCore();

			public LicensingMessageSendingObjectAdditionalMessageErrorCollector ExposedLicensingMessageSendingObjectAdditionalMessageErrorCollector() => GetLicensingMessageSendingObjectAdditionalMessageErrorCollectorCore();
		}
	}
}
