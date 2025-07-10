using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX301_AXLicensingMessageSendingObjectParent))]
	sealed class NX301_AXLicensingMessageSendingObjectParentTest : LicensingMessageSendingObjectParentAbstractTest
	{
		[ExpectNoExceptions]
		public void TestMessageSendingObjectType()
		{
			NUnit.Framework.Assert.That(LicensingMessageSendingObjectParentForTesting.SendingObjectsCollection.FirstOrDefault(), NUnit.Framework.Is.TypeOf<NX301_AXMessageSendingObject>());
		}

		[ExpectNoExceptions]
		public void TestGetLicensingMessageSendingObjectParentNotificationCollectorCore()
		{
			NUnit.Framework.Assert.That(((NX301_AXLicensingMessageSendingObjectParentForTest)LicensingMessageSendingObjectParentForTesting).ExposedLicensingMessageSendingObjectParentNotificationCollector(), NUnit.Framework.Is.TypeOf<NX301_AXMessageSendingObjectParentNotificationCollector>());
		}

		protected override LicensingMessageSendingObjectParent CreateLicensingMessageSendingObjectParentForTesting()
		{
			var decl = Factory.NewWithValidTestData<JobDeclaration>();
			var messageHeader = decl.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_AX;
			return new NX301_AXLicensingMessageSendingObjectParentForTest(decl);
		}

		class NX301_AXLicensingMessageSendingObjectParentForTest : NX301_AXLicensingMessageSendingObjectParent
		{
			public NX301_AXLicensingMessageSendingObjectParentForTest(JobDeclaration declaration) : base(declaration)
			{
			}

			public LicensingMessageSendingObjectParentNotificationCollector ExposedLicensingMessageSendingObjectParentNotificationCollector() => GetLicensingMessageSendingObjectParentNotificationCollectorCore();
		}
	}
}
