using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingDeclaration))]
	sealed class TrackingDeclarationBizOChangesEmailNotificationTest : BizOChangesEmailNotificationTest<TrackingDeclaration>
	{
		protected override TrackingDeclaration GetNewBizOForNotification()
		{
			return new TrackingDeclaration(Factory.NewWithValidTestData<BaseJobDeclaration>()) { HasChanges = true };
		}
	}
}
