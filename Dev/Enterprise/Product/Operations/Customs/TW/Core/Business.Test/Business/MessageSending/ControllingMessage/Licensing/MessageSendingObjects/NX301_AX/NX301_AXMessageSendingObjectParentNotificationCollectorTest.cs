using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX301_AXMessageSendingObjectParentNotificationCollector))]
	sealed class NX301_AXMessageSendingObjectParentNotificationCollectorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestExcludedAdditionalNotificationsFromCusEntryHeaderProperties()
		{
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			NUnit.Framework.Assert.That(!collector.ShouldIncludeNotificationsFromInfoForTest(cusEntryHeader.CH_TotalNetWeightInKilogramsInfo), NUnit.Framework.Is.True, "Exclude CusEntryHeader.CH_TotalNetWeightInKilograms");
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
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			sendingObjectCollection = new LicensingMessageSendingObjectCollection(Factory);
			collector = new NX301_AXMessageSendingObjectParentNotificationCollectorForTest(sendingObjectCollection, declaration, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
		}

		class NX301_AXMessageSendingObjectParentNotificationCollectorForTest : NX301_AXMessageSendingObjectParentNotificationCollector
		{
			public NX301_AXMessageSendingObjectParentNotificationCollectorForTest(LicensingMessageSendingObjectCollection sendingObjects, IBusiness business, ZBool includeChildren, ZBool includeNotificationTypeInMessage, PropertyDescriptionType descriptionToInclude) : base(sendingObjects, business, includeChildren, includeNotificationTypeInMessage, descriptionToInclude)
			{
			}

			public bool ShouldIncludeNotificationsFromInfoForTest(ZPropertyInfo info)
			{
				return ShouldIncludeNotificationsFromInfo(info);
			}
		}

		JobDeclaration declaration;
		LicensingMessageSendingObjectCollection sendingObjectCollection;
		NX301_AXMessageSendingObjectParentNotificationCollectorForTest collector;
	}
}
