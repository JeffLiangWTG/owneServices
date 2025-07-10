using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.QuotedBookings.Business;
using Moq;

namespace Enterprise.Tracking.Business.Testing
{
	public class TrackingScheduleChooserCreatorTest : TestCaseWithFactory
	{
		public void TestCreate()
		{
			var sailingChooserParent = new Mock<ISailingChooserParent>().Object;
			var scheduleChoose = TestScheduleChooserCreator.Create(sailingChooserParent);
			AssertNotNull(scheduleChoose);
			AssertEquals(sailingChooserParent, scheduleChoose.Parent);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var testSiteUser = new TrackingSiteUser();
			TestScheduleChooserCreator = new TrackingScheduleChooserCreator(testSiteUser);
		}

		TrackingScheduleChooserCreator TestScheduleChooserCreator;
	}
}
