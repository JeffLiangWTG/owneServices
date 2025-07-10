using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX401LicensingMessageSendingObjectParent))]
	sealed class NX401LicensingMessageSendingObjectParentTest : LicensingMessageSendingObjectParentAbstractTest
	{
		[ExpectNoExceptions]
		public void TestMessageSendingObjectType()
		{
			Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var testParent = CreateLicensingMessageSendingObjectParentForTesting();
			NUnit.Framework.Assert.That(testParent.SendingObjectsCollection.FirstOrDefault(), NUnit.Framework.Is.TypeOf<NX401ImportMessageSendingObject>());

			Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			testParent = CreateLicensingMessageSendingObjectParentForTesting();
			NUnit.Framework.Assert.That(testParent.SendingObjectsCollection.FirstOrDefault(), NUnit.Framework.Is.TypeOf<NX401ExportMessageSendingObject>());
		}

		[ExpectNoExceptions]
		public void TestGetLicensingMessageSendingObjectAdditionalMessageErrorCollector()
		{
			NUnit.Framework.Assert.That(((NX401LicensingMessageSendingObjectParentForTest)LicensingMessageSendingObjectParentForTesting).ExposedLicensingMessageSendingObjectAdditionalMessageErrorCollector, NUnit.Framework.Is.TypeOf<NX401MessageSendingObjectAdditionalMessageErrorCollector>());
		}

		[ExpectNoExceptions]
		public void TestGetLicensingMessageSendingObjectParentNotificationCollectorCore()
		{
			NUnit.Framework.Assert.That(((NX401LicensingMessageSendingObjectParentForTest)LicensingMessageSendingObjectParentForTesting).ExposedLicensingMessageSendingObjectParentNotificationCollector(), NUnit.Framework.Is.TypeOf<NX401LicensingMessageSendingObjectParentNotificationCollector>());
		}

		protected override LicensingMessageSendingObjectParent CreateLicensingMessageSendingObjectParentForTesting()
		{
			var messageHeader = Declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			return new NX401LicensingMessageSendingObjectParentForTest(Declaration);
		}

		JobDeclaration Declaration => decl ??= Factory.NewWithValidTestData<JobDeclaration>();
		JobDeclaration decl;

		class NX401LicensingMessageSendingObjectParentForTest : NX401LicensingMessageSendingObjectParent
		{
			public NX401LicensingMessageSendingObjectParentForTest(JobDeclaration declaration) : base(declaration)
			{
			}

			public LicensingMessageSendingObjectAdditionalMessageErrorCollector ExposedLicensingMessageSendingObjectAdditionalMessageErrorCollector => GetLicensingMessageSendingObjectAdditionalMessageErrorCollectorCore();

			public LicensingMessageSendingObjectParentNotificationCollector ExposedLicensingMessageSendingObjectParentNotificationCollector() => GetLicensingMessageSendingObjectParentNotificationCollectorCore();
		}
	}
}
