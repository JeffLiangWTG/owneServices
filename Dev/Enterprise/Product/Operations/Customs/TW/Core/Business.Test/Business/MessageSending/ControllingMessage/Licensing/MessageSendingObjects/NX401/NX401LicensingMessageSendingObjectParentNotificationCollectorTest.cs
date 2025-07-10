using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX401LicensingMessageSendingObjectParentNotificationCollector))]
	sealed class NX401LicensingMessageSendingObjectParentNotificationCollectorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestExcludedAdditionalNotificationsFromDeclarationProperties()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!collector.ShouldIncludeNotificationsFromInfoForTest(declaration.JE_TotalNoOfPacksInfo), NUnit.Framework.Is.True, "Exclude JobDeclaration.JE_TotalNoOfPacks");
				NUnit.Framework.Assert.That(!collector.ShouldIncludeNotificationsFromInfoForTest(declaration.JE_TotalWeightInfo), NUnit.Framework.Is.True, "Exclude JobDeclaration.JE_TotalWeight");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			sendingObjectCollection = new LicensingMessageSendingObjectCollection(Factory);
			collector = new NX401LicensingMessageSendingObjectParentNotificationCollectorForTest(sendingObjectCollection, declaration, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
		}

		JobDeclaration declaration;
		LicensingMessageSendingObjectCollection sendingObjectCollection;
		NX401LicensingMessageSendingObjectParentNotificationCollectorForTest collector;

		class NX401LicensingMessageSendingObjectParentNotificationCollectorForTest : NX401LicensingMessageSendingObjectParentNotificationCollector
		{
			public NX401LicensingMessageSendingObjectParentNotificationCollectorForTest(LicensingMessageSendingObjectCollection sendingObjects, IBusiness business, ZBool includeChildren, ZBool includeNotificationTypeInMessage, PropertyDescriptionType descriptionToInclude) : base(sendingObjects, business, includeChildren, includeNotificationTypeInMessage, descriptionToInclude)
			{
			}

			public bool ShouldIncludeNotificationsFromInfoForTest(ZPropertyInfo info)
			{
				return ShouldIncludeNotificationsFromInfo(info);
			}
		}
	}
}
