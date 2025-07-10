using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingContainer))]
	class TrackingContainerBizOChangesEmailNotificationTest : BizOChangesEmailNotificationTest<TrackingContainer>
	{
		protected override TrackingContainer GetNewBizOForNotification()
		{
			var trackingContainer = Factory.NewWithValidTestData<TrackingContainer>();
			trackingContainer.HasChanges = true;
			return trackingContainer;
		}
	}
}
