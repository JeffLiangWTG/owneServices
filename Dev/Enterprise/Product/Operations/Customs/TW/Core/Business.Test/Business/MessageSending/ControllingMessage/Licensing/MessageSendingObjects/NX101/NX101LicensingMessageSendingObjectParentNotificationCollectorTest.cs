using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing;

sealed class NX101LicensingMessageSendingObjectParentNotificationCollectorTest : TestCaseWithFactory
{
	[ExpectNoExceptions]
	public void TestExcludedAdditionalNotificationsFromDeclarationProperties()
	{
		var decl = Factory.New<JobDeclaration>();
		NUnit.Framework.Assert.That(!collector.ShouldIncludeNotificationsFromInfoForTest(decl.DeclarationNumberDisplayInfo), NUnit.Framework.Is.True, "Exclude JobDeclaration.DeclarationNumberDisplay");
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		sendingObjectCollection = new LicensingMessageSendingObjectCollection(Factory);
		collector = new NX101LicensingMessageSendingObjectParentNotificationCollectorForTest(sendingObjectCollection, declaration, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
	}

	class NX101LicensingMessageSendingObjectParentNotificationCollectorForTest : NX101LicensingMessageSendingObjectParentNotificationCollector
	{
		public NX101LicensingMessageSendingObjectParentNotificationCollectorForTest(LicensingMessageSendingObjectCollection sendingObjects, IBusiness business, ZBool includeChildren, ZBool includeNotificationTypeInMessage, PropertyDescriptionType descriptionToInclude) : base(sendingObjects, business, includeChildren, includeNotificationTypeInMessage, descriptionToInclude)
		{
		}

		public bool ShouldIncludeNotificationsFromInfoForTest(ZPropertyInfo info)
		{
			return ShouldIncludeNotificationsFromInfo(info);
		}
	}

	JobDeclaration declaration;
	LicensingMessageSendingObjectCollection sendingObjectCollection;
	NX101LicensingMessageSendingObjectParentNotificationCollectorForTest collector;
}
