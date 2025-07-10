using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingBooking))]
	class TrackingBookingBizOChangesEmailNotificationTest : BizOChangesEmailNotificationTest<TrackingBooking>
	{
		protected override TrackingBooking GetNewBizOForNotification()
		{
			var trackingBooking = new TrackingBooking(Factory, new TestHelper(Factory).TestSiteUser);

			var packLine = trackingBooking.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 1;
			packLine.JL_F3_NKPackType = "FCL";
			packLine.JL_ActualWeight = 1;
			packLine.JL_ActualWeightUQ = "KG";
			packLine.JL_ActualVolume = 1;
			packLine.JL_ActualVolumeUQ = "M3";
			packLine.JL_Length = 1;
			packLine.JL_Width = 1;
			packLine.JL_Height = 1;
			packLine.JL_UnitOfDimension = "M";
			packLine.JL_MarksAndNumbers = "Marks";
			packLine.JL_HarmonisedCode = "HARMONY";
			packLine.JL_LinePrice = 1;
			packLine.JL_Description = "desc";

			var rc = Factory.LoadTop1<RefContainer>(new ZQuery());
			var container = trackingBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerNum = "ABC123";
			container.JC_RC = rc.PK;
			container.JC_ContainerCount = 2;

			trackingBooking.DocumentHelper.NewPublishedDocuments.AddNew();
			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Type = "MIL";
			var milestone = new TrackingMilestone(task);
			trackingBooking.Milestones.Add(milestone);
			milestone.EstimatedDate = ZDateTimeOffset.Today;
			milestone.ActualDate = ZDateTimeOffset.Today;

			return trackingBooking;
		}
	}
}
