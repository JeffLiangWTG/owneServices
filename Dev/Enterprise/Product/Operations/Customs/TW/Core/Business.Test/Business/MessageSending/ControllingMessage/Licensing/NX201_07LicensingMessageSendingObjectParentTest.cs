using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX201_07LicensingMessageSendingObjectParent))]
	sealed class NX201_07LicensingMessageSendingObjectParentTest : LicensingMessageSendingObjectParentAbstractTest
	{
		[ExpectNoExceptions]
		public void TestMessageSendingObjectType()
		{
			NUnit.Framework.Assert.That(LicensingMessageSendingObjectParentForTesting.SendingObjectsCollection.FirstOrDefault(), NUnit.Framework.Is.TypeOf<NX201_07MessageSendingObject>());
		}

		protected override bool ExpectedShowReasonDescription => true;

		[ExpectNoExceptions]
		public void TestGetLicensingMessageSendingObjectAdditionalMessageErrorCollector()
		{
			NUnit.Framework.Assert.That(((NX201_07LicensingMessageSendingObjectParentForTest)LicensingMessageSendingObjectParentForTesting).ExposedLicensingMessageSendingObjectAdditionalMessageErrorCollector(), NUnit.Framework.Is.TypeOf<NX201_07MessageSendingObjectAdditionalMessageErrorCollector>());
		}

		[ExpectNoExceptions]
		public void TestGetLicensingMessageSendingObjectParentNotificationCollectorCore()
		{
			NUnit.Framework.Assert.That(((NX201_07LicensingMessageSendingObjectParentForTest)LicensingMessageSendingObjectParentForTesting).ExposedLicensingMessageSendingObjectParentNotificationCollector(), NUnit.Framework.Is.TypeOf<NX201_07LicensingMessageSendingObjectParentNotificationCollector>());
		}

		protected override LicensingMessageSendingObjectParent CreateLicensingMessageSendingObjectParentForTesting()
		{
			var decl = Factory.NewWithValidTestData<JobDeclaration>();
			var messageHeader = decl.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX201_07;
			return new NX201_07LicensingMessageSendingObjectParentForTest(decl);
		}

		class NX201_07LicensingMessageSendingObjectParentForTest : NX201_07LicensingMessageSendingObjectParent
		{
			public NX201_07LicensingMessageSendingObjectParentForTest(JobDeclaration declaration) : base(declaration)
			{
			}

			public LicensingMessageSendingObjectAdditionalMessageErrorCollector ExposedLicensingMessageSendingObjectAdditionalMessageErrorCollector()
			{
				return GetLicensingMessageSendingObjectAdditionalMessageErrorCollectorCore();
			}

			public LicensingMessageSendingObjectParentNotificationCollector ExposedLicensingMessageSendingObjectParentNotificationCollector()
			{
				return GetLicensingMessageSendingObjectParentNotificationCollectorCore();
			}
		}
	}
}
