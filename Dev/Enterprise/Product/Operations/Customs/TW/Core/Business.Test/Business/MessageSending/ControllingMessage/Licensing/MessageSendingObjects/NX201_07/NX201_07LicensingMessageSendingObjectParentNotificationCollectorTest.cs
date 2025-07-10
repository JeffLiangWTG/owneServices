using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX201_07LicensingMessageSendingObjectParentNotificationCollector))]
	sealed class NX201_07LicensingMessageSendingObjectParentNotificationCollectorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestExcludedAdditionalNotificationsFromInvoiceProperties()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			NUnit.Framework.Assert.That(!collector.ShouldIncludeNotificationsFromInfoForTest(invoice.JZ_InvoiceAmountInfo), NUnit.Framework.Is.True, "Exclude JobComInvoiceHeader.JZ_InvoiceAmount");
		}

		[ExpectNoExceptions]
		public void TestExcludedAdditionalNotificationsFromDeclarationProperties()
		{
			var decl = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!collector.ShouldIncludeNotificationsFromInfoForTest(decl.JE_DateAtOriginInfo), NUnit.Framework.Is.True, "Exclude JobDeclaration.JE_DateAtOrigin");
				NUnit.Framework.Assert.That(!collector.ShouldIncludeNotificationsFromInfoForTest(decl.JE_ExportDateInfo), NUnit.Framework.Is.True, "Exclude JobDeclaration.JE_ExportDate");
				NUnit.Framework.Assert.That(!collector.ShouldIncludeNotificationsFromInfoForTest(decl.JE_DateAtFinalDestinationInfo), NUnit.Framework.Is.True, "Exclude JobDeclaration.JE_DateAtFinalDestination");
				NUnit.Framework.Assert.That(!collector.ShouldIncludeNotificationsFromInfoForTest(decl.JE_TotalNoOfPacksInfo), NUnit.Framework.Is.True, "Exclude JobDeclaration.JE_TotalNoOfPacks");
				NUnit.Framework.Assert.That(!collector.ShouldIncludeNotificationsFromInfoForTest(decl.JE_TotalWeightInfo), NUnit.Framework.Is.True, "Exclude JobDeclaration.JE_TotalWeight");
				NUnit.Framework.Assert.That(!collector.ShouldIncludeNotificationsFromInfoForTest(decl.DeclarationNumberDisplayInfo), NUnit.Framework.Is.True, "Exclude JobDeclaration.DeclarationNumberDisplay");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			sendingObjectCollection = new LicensingMessageSendingObjectCollection(Factory);
			collector = new NX201_07LicensingMessageSendingObjectParentNotificationCollectorForTest(sendingObjectCollection, declaration, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
		}

		class NX201_07LicensingMessageSendingObjectParentNotificationCollectorForTest : NX201_07LicensingMessageSendingObjectParentNotificationCollector
		{
			public NX201_07LicensingMessageSendingObjectParentNotificationCollectorForTest(LicensingMessageSendingObjectCollection sendingObjects, IBusiness business, ZBool includeChildren, ZBool includeNotificationTypeInMessage, PropertyDescriptionType descriptionToInclude) : base(sendingObjects, business, includeChildren, includeNotificationTypeInMessage, descriptionToInclude)
			{
			}

			public bool ShouldIncludeNotificationsFromInfoForTest(ZPropertyInfo info)
			{
				return ShouldIncludeNotificationsFromInfo(info);
			}
		}

		JobDeclaration declaration;
		LicensingMessageSendingObjectCollection sendingObjectCollection;
		NX201_07LicensingMessageSendingObjectParentNotificationCollectorForTest collector;
	}
}
