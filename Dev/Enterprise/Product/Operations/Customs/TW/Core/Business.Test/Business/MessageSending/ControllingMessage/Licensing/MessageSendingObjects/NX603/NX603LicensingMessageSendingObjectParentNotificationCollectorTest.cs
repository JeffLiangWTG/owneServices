using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX603LicensingMessageSendingObjectParentNotificationCollector))]
	sealed class NX603LicensingMessageSendingObjectParentNotificationCollectorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestExcludedAdditionalNotificationsFromInvoiceProperties()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			NUnit.Framework.Assert.That(!collector.ShouldIncludeNotificationsFromInfoForTest(invoice.JZ_NetWeightInfo), NUnit.Framework.Is.True, "Exclude JobComInvoiceHeader.JZ_NetWeight");
		}

		[ExpectNoExceptions]
		public void TestExcludedAdditionalNotificationsFromDeclarationProperties()
		{
			var decl = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!collector.ShouldIncludeNotificationsFromInfoForTest(decl.JE_TotalNoOfPacksInfo), NUnit.Framework.Is.True, "Exclude JobDeclaration.JE_TotalNoOfPacks");
				NUnit.Framework.Assert.That(!collector.ShouldIncludeNotificationsFromInfoForTest(decl.JE_TotalWeightInfo), NUnit.Framework.Is.True, "Exclude JobDeclaration.JE_TotalWeight");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var sendingObjectCollection = new LicensingMessageSendingObjectCollection(Factory);
			collector = new NX603LicensingMessageSendingObjectParentNotificationCollectorForTest(sendingObjectCollection, declaration, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
		}

		class NX603LicensingMessageSendingObjectParentNotificationCollectorForTest : NX603LicensingMessageSendingObjectParentNotificationCollector
		{
			public NX603LicensingMessageSendingObjectParentNotificationCollectorForTest(LicensingMessageSendingObjectCollection sendingObjects, IBusiness business, ZBool includeChildren, ZBool includeNotificationTypeInMessage, PropertyDescriptionType descriptionToInclude) : base(sendingObjects, business, includeChildren, includeNotificationTypeInMessage, descriptionToInclude)
			{
			}

			public bool ShouldIncludeNotificationsFromInfoForTest(ZPropertyInfo info) => ShouldIncludeNotificationsFromInfo(info);
		}

		NX603LicensingMessageSendingObjectParentNotificationCollectorForTest collector;
	}
}
