using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LicensingMessageSendingObjectParentNotificationCollector))]
	sealed class LicensingMessageSendingObjectParentNotificationCollectorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestShouldIncludeNotificationsFromObject()
		{
			var messageHeader1 = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var messageHeader2 = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var messageHeader3 = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var sendingObject1 = new NX401ExportMessageSendingObject(messageHeader1);
			sendingObject1.ShouldSend = true;
			var sendingObject2 = new NX401ExportMessageSendingObject(messageHeader2);
			sendingObject2.ShouldSend = false;
			var sendingObject3 = new NX401ExportMessageSendingObject(messageHeader3);

			sendingObjectCollection.Add(sendingObject1);
			sendingObjectCollection.Add(sendingObject2);
			collector = new LicensingMessageSendingObjectParentNotificationCollectorForTest(sendingObjectCollection, declaration, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);

			var invoiceLine = Factory.New<JobComInvoiceLine>();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!collector.ShouldIncludeNotificationsFromObjectForTest(invoiceLine), NUnit.Framework.Is.True, "Not include the AddInfoJobComInvoiceLine");
				NUnit.Framework.Assert.That(collector.ShouldIncludeNotificationsFromObjectForTest(messageHeader1), NUnit.Framework.Is.True, "Should include the CusTWControllingMessageHeader which Should Send");
				NUnit.Framework.Assert.That(!collector.ShouldIncludeNotificationsFromObjectForTest(messageHeader2), NUnit.Framework.Is.True, "Not include the CusTWControllingMessageHeader which Should Send is false");
				NUnit.Framework.Assert.That(!collector.ShouldIncludeNotificationsFromObjectForTest(messageHeader3), NUnit.Framework.Is.True, "Not include the CusTWControllingMessageHeader which not be sent");
			});
		}

		[ExpectNoExceptions]
		public void TestShouldIncludeNotificationsFromInfo()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!collector.ShouldIncludeNotificationsFromInfoForTest(invoiceLine.JI_TextileWidthInfo), NUnit.Framework.Is.True, "Not include the JobComInvoiceLine.JI_TextileWidthInfo");
				NUnit.Framework.Assert.That(!collector.ShouldIncludeNotificationsFromInfoForTest(invoiceLine.JI_TextileWidthUQInfo), NUnit.Framework.Is.True, "Not include the JobComInvoiceLine.JI_TextileWidthUQInfo");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			sendingObjectCollection = new LicensingMessageSendingObjectCollection(Factory);
			collector = new LicensingMessageSendingObjectParentNotificationCollectorForTest(sendingObjectCollection, declaration, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
		}

		class LicensingMessageSendingObjectParentNotificationCollectorForTest : LicensingMessageSendingObjectParentNotificationCollector
		{
			public LicensingMessageSendingObjectParentNotificationCollectorForTest(LicensingMessageSendingObjectCollection sendingObjects, IBusiness business, ZBool includeChildren, ZBool includeNotificationTypeInMessage, PropertyDescriptionType descriptionToInclude) : base(sendingObjects, business, includeChildren, includeNotificationTypeInMessage, descriptionToInclude)
			{
			}

			public bool ShouldIncludeNotificationsFromObjectForTest(BusinessObject businessObject)
			{
				return ShouldIncludeNotificationsFromObject(businessObject);
			}

			public bool ShouldIncludeNotificationsFromInfoForTest(ZPropertyInfo info)
			{
				return ShouldIncludeNotificationsFromInfo(info);
			}
		}

		JobDeclaration declaration;
		LicensingMessageSendingObjectCollection sendingObjectCollection;
		LicensingMessageSendingObjectParentNotificationCollectorForTest collector;
	}
}
