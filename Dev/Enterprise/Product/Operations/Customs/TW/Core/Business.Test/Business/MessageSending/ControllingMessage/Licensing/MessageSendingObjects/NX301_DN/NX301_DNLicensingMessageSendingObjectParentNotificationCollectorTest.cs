using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX301_DNLicensingMessageSendingObjectParentNotificationCollector))]
	sealed class NX301_DNLicensingMessageSendingObjectParentNotificationCollectorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestExcludedAdditionalNotificationsFromDeclarationProperties()
		{
			var decl = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(!collector.ShouldIncludeNotificationsFromInfoForTest(decl.JE_MasterBillInfo), NUnit.Framework.Is.True, "Exclude JobDeclaration.JE_MasterBill");
				NUnit.Framework.Assert.That(!collector.ShouldIncludeNotificationsFromInfoForTest(decl.JE_VesselNameInfo), NUnit.Framework.Is.True, "Exclude JobDeclaration.JE_VesselName");
				NUnit.Framework.Assert.That(!collector.ShouldIncludeNotificationsFromInfoForTest(decl.JE_VoyageFlightNoInfo), NUnit.Framework.Is.True, "Exclude JobDeclaration.JE_VoyageFlightNo");
				NUnit.Framework.Assert.That(!collector.ShouldIncludeNotificationsFromInfoForTest(decl.JE_TotalWeightInfo), NUnit.Framework.Is.True, "Exclude JobDeclaration.JE_TotalWeight");
				NUnit.Framework.Assert.That(!collector.ShouldIncludeNotificationsFromInfoForTest(decl.JE_TotalNoOfPacksInfo), NUnit.Framework.Is.True, "Exclude JobDeclaration.JE_TotalNoOfPacks");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			sendingObjectCollection = new LicensingMessageSendingObjectCollection(Factory);
			collector = new NX301_DNLicensingMessageSendingObjectParentNotificationCollectorForTest(sendingObjectCollection, declaration, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
		}

		class NX301_DNLicensingMessageSendingObjectParentNotificationCollectorForTest : NX301_DNLicensingMessageSendingObjectParentNotificationCollector
		{
			public NX301_DNLicensingMessageSendingObjectParentNotificationCollectorForTest(LicensingMessageSendingObjectCollection sendingObjects, IBusiness business, ZBool includeChildren, ZBool includeNotificationTypeInMessage, PropertyDescriptionType descriptionToInclude) : base(sendingObjects, business, includeChildren, includeNotificationTypeInMessage, descriptionToInclude)
			{
			}

			public bool ShouldIncludeNotificationsFromInfoForTest(ZPropertyInfo info)
			{
				return ShouldIncludeNotificationsFromInfo(info);
			}
		}

		JobDeclaration declaration;
		LicensingMessageSendingObjectCollection sendingObjectCollection;
		NX301_DNLicensingMessageSendingObjectParentNotificationCollectorForTest collector;
	}
}
