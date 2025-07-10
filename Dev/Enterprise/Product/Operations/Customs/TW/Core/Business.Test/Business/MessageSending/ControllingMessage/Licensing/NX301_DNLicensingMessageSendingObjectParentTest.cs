using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX301_DNLicensingMessageSendingObjectParent))]
	sealed class NX301_DNLicensingMessageSendingObjectParentTest : LicensingMessageSendingObjectParentAbstractTest
	{
		[ExpectNoExceptions]
		public void TestMessageSendingObjectType()
		{
			NUnit.Framework.Assert.That(LicensingMessageSendingObjectParentForTesting.SendingObjectsCollection.FirstOrDefault(), NUnit.Framework.Is.TypeOf<NX301_DNMessageSendingObject>());
		}

		[ExpectNoExceptions]
		public void TestGetLicensingMessageSendingObjectAdditionalMessageErrorCollector()
		{
			NUnit.Framework.Assert.That(((NX301_DNLicensingMessageSendingObjectParentForTest)LicensingMessageSendingObjectParentForTesting).ExposedLicensingMessageSendingObjectAdditionalMessageErrorCollector, NUnit.Framework.Is.TypeOf<NX301_DNMessageSendingObjectAdditionalMessageErrorCollector>());
		}

		[ExpectNoExceptions]
		public void TestGetLicensingMessageSendingObjectParentNotificationCollectorCore()
		{
			NUnit.Framework.Assert.That(((NX301_DNLicensingMessageSendingObjectParentForTest)LicensingMessageSendingObjectParentForTesting).ExposedLicensingMessageSendingObjectParentNotificationCollector(), NUnit.Framework.Is.TypeOf<NX301_DNLicensingMessageSendingObjectParentNotificationCollector>());
		}

		protected override LicensingMessageSendingObjectParent CreateLicensingMessageSendingObjectParentForTesting()
		{
			var decl = Factory.NewWithValidTestData<JobDeclaration>();
			var messageHeader = decl.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
			return new NX301_DNLicensingMessageSendingObjectParentForTest(decl);
		}

		class NX301_DNLicensingMessageSendingObjectParentForTest : NX301_DNLicensingMessageSendingObjectParent
		{
			public NX301_DNLicensingMessageSendingObjectParentForTest(JobDeclaration declaration) : base(declaration)
			{
			}

			public LicensingMessageSendingObjectAdditionalMessageErrorCollector ExposedLicensingMessageSendingObjectAdditionalMessageErrorCollector => GetLicensingMessageSendingObjectAdditionalMessageErrorCollectorCore();

			public LicensingMessageSendingObjectParentNotificationCollector ExposedLicensingMessageSendingObjectParentNotificationCollector() => GetLicensingMessageSendingObjectParentNotificationCollectorCore();
		}
	}
}
