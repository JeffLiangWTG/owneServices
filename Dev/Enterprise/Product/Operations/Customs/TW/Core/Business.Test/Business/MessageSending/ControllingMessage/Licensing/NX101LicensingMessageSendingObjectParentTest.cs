using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX101LicensingMessageSendingObjectParent))]
	sealed class NX101LicensingMessageSendingObjectParentTest : LicensingMessageSendingObjectParentAbstractTest
	{
		[ExpectNoExceptions]
		public void TestMessageSendingObjectType()
		{
			NUnit.Framework.Assert.That(LicensingMessageSendingObjectParentForTesting.SendingObjectsCollection.FirstOrDefault(), NUnit.Framework.Is.TypeOf<NX101MessageSendingObject>());
		}

		[ExpectNoExceptions]
		public void TestGetLicensingMessageSendingObjectAdditionalMessageErrorCollector()
		{
			NUnit.Framework.Assert.That(((NX101LicensingMessageSendingObjectParentForTest)LicensingMessageSendingObjectParentForTesting).ExposedLicensingMessageSendingObjectAdditionalMessageErrorCollector, NUnit.Framework.Is.TypeOf<NX101MessageSendingObjectAdditionalMessageErrorCollector>());
		}

		[ExpectNoExceptions]
		public void TestGetLicensingMessageSendingObjectParentNotificationCollectorCore()
		{
			NUnit.Framework.Assert.That(((NX101LicensingMessageSendingObjectParentForTest)LicensingMessageSendingObjectParentForTesting).ExposedLicensingMessageSendingObjectParentNotificationCollector(), NUnit.Framework.Is.TypeOf<NX101LicensingMessageSendingObjectParentNotificationCollector>());
		}

		protected override LicensingMessageSendingObjectParent CreateLicensingMessageSendingObjectParentForTesting()
		{
			var decl = Factory.NewWithValidTestData<JobDeclaration>();
			var messageHeader = decl.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			return new NX101LicensingMessageSendingObjectParentForTest(decl, "Send Application Message for Certificate of Origin (NX101)");
		}

		protected override bool ExpectedIsSupportingDocumentsNeededMessage => false;

		internal class NX101LicensingMessageSendingObjectParentForTest : NX101LicensingMessageSendingObjectParent
		{
			public NX101LicensingMessageSendingObjectParentForTest(JobDeclaration declaration, string menuCaption = "") : base(declaration, menuCaption)
			{
			}

			public LicensingMessageSendingObjectAdditionalMessageErrorCollector ExposedLicensingMessageSendingObjectAdditionalMessageErrorCollector => GetLicensingMessageSendingObjectAdditionalMessageErrorCollectorCore();

			public IEnumerable<INotification> ExposedMessageErrorCollector() => GetNewMessageErrorCollector();

			public LicensingMessageSendingObjectParentNotificationCollector ExposedLicensingMessageSendingObjectParentNotificationCollector()
			{
				return GetLicensingMessageSendingObjectParentNotificationCollectorCore();
			}
		}
	}
}
