using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingCartage))]
	sealed class TrackingCartageBizOChangesEmailNotificationTest : BizOChangesEmailNotificationTest<TrackingCartage>
	{
		protected override TrackingCartage GetNewBizOForNotification()
		{
			var trackingCartage = Factory.NewWithValidTestData<TrackingCartage>();
			return trackingCartage;
		}
	}
}
