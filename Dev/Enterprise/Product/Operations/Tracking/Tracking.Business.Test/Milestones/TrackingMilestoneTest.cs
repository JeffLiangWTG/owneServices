using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Web;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Shared;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingMilestone))]
	public class TrackingMilestoneTest : NonPersistentBusinessObjectTestCase
	{
		protected override void TearDown()
		{
			SuppressionForTest.CacheObjectClear();
			Globals.IsWeb = false;
			base.TearDown();
		}

		public void TestTask()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Type = "MIL";
			var testMilestone = new TrackingMilestone(task);

			AssertNotNull(testMilestone.Task);
			AssertEquals(task, testMilestone.Task);

			testMilestone = new TrackingMilestone(ZString.Empty, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, ZInt.Zero);
			AssertNull(testMilestone.Task);
		}

		public void TestHasChanges()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Type = "MIL";

			var testMilestone = new TrackingMilestone(task);
			Assert(testMilestone.HasChanges);

			testMilestone = new TrackingMilestone(ZString.Empty, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, ZInt.Zero);
			Assert(!testMilestone.HasChanges);
		}

		public void TestEstimatedDate()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Type = "MIL";
			var testDate = new ZDateTimeOffset(ZDateTime.SmallDateTimeNow.AddDays(10));
			task.SetMilestoneScheduledDateForTest(testDate);
			var testMilestone = new TrackingMilestone(task);
			AssertEquals(testDate, testMilestone.EstimatedDate);

			var testDate2 = new ZDateTimeOffset(ZDateTime.SmallDateTimeNow.AddDays(20));
			testMilestone.EstimatedDate = testDate2;
			AssertEquals(testDate2, testMilestone.EstimatedDate);
			AssertEquals(testDate2.ToZDateTime(), task.P9_ScheduledDate.ToZDateTime());

			testMilestone = new TrackingMilestone(ZString.Empty, ZDateTimeOffset.Now, testDate, ZInt.Zero);
			AssertEquals(testDate, testMilestone.EstimatedDate);
			testMilestone.EstimatedDate = testDate2;
			AssertEquals(testDate, testMilestone.EstimatedDate);
		}

		public void TestActualDate()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Type = "MIL";
			var testDate = ZDateTimeOffset.Now.AddDays(10);
			task.SetMilestoneActualDateForTest(testDate);
			var testMilestone = new TrackingMilestone(task);
			AssertEquals(testDate, testMilestone.ActualDate);

			var testDate2 = ZDateTimeOffset.Now.AddDays(20);
			testMilestone.ActualDate = testDate2;
			AssertEquals(testDate2, testMilestone.ActualDate);
			AssertEquals(testDate2.ToZDateTime(), task.P9_ActualDate.ToZDateTime());

			testMilestone = new TrackingMilestone(ZString.Empty, testDate, ZDateTimeOffset.Now, ZInt.Zero);
			AssertEquals(testDate, testMilestone.ActualDate);
			testMilestone.ActualDate = testDate2;
			AssertEquals(testDate, testMilestone.ActualDate);
		}

		public void TestGenerateDetailsForEmailReporting()
		{
			var estimated = ZDateTimeOffset.Now.AddDays(10);
			var actual = ZDateTimeOffset.Now.AddDays(20);
			var testMilestone = new TrackingMilestone("Test Description", actual, estimated, 0);

			ZString expected = string.Format("Description: {1}{0}Estimated Date: {2}{0}Actual Date: {3}{0}Status: {4}", System.Environment.NewLine, "Test Description", WebDateTimeFormatter.GetFormattedDate(estimated, ZDateTimePickerFormat.Long), WebDateTimeFormatter.GetFormattedDate(actual, ZDateTimePickerFormat.Long), testMilestone.Status);
			AssertEquals(expected, testMilestone.GenerateDetailsForEmailReporting());
		}

		public void TestEventCode()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Type = "MIL";
			task.TriggerConditions.TriggerEventCode = ZString.Empty;
			var testMilestone = new TrackingMilestone(task);
			AssertEquals(ZString.Empty, testMilestone.EventCode);

			task.TriggerConditions.TriggerEventCode = "TST";
			testMilestone = new TrackingMilestone(task);
			AssertEquals("TST", testMilestone.EventCode);

			task.TriggerConditions.TriggerEventCode = "TS2";
			AssertNull(task.MilestoneEvent);
			AssertEquals("TST", testMilestone.EventCode);

			task.TriggerConditions.TriggerEventCode = "ADD";
			AssertNotNull(task.MilestoneEvent);
			AssertEquals("ADD", testMilestone.EventCode);
		}

		public void TestDisplayDateAndStatus_MilesoneStatusVisibilityAll()
		{
			using (WebDataRegistry.Instance.MilestoneStatusVisibility.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneStatusVisibilityList.Codes.All))
			{
				AssertEquals("Precondition: default registry value", MilestoneStatusVisibilityList.Codes.All, WebDataRegistry.Instance.MilestoneStatusVisibility.Value);

				var task = Factory.New<ProcessTask>();
				task.P9_Type = "MIL";
				task.SetMilestoneActualDateForTest(new ZDateTimeOffset(new ZDateTime(2009, 01, 10)));
				AssertEquals(new ZDateTime(2009, 01, 10), new TrackingMilestone(task).DisplayDate.ToZDateTime());
				AssertEquals("Completed", new TrackingMilestone(task).Status);
				AssertEquals("Completed", new TrackingMilestone(task).ActualStatus);

				task.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2009, 01, 09)));
				AssertEquals(new ZDateTime(2009, 01, 10), new TrackingMilestone(task).DisplayDate.ToZDateTime());
				AssertEquals("Completed Late", new TrackingMilestone(task).Status);
				AssertEquals("Completed Late", new TrackingMilestone(task).ActualStatus);

				task.SetMilestoneActualDateForTest(ZDateTimeOffset.Empty);
				AssertEquals(new ZDateTime(2009, 01, 09), new TrackingMilestone(task).DisplayDate.ToZDateTime());
				AssertEquals("Overdue", new TrackingMilestone(task).Status);
				AssertEquals("Overdue", new TrackingMilestone(task).ActualStatus);

				task.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(ZDateTime.Now.AddDays(1)));
				AssertEquals("Pending", new TrackingMilestone(task).Status);
				AssertEquals("Pending", new TrackingMilestone(task).ActualStatus);
			}
		}

		public void TestDisplayDateAndStatus_MilesoneStatusVisibilityNone()
		{
			using (WebDataRegistry.Instance.MilestoneStatusVisibility.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneStatusVisibilityList.Codes.None))
			{
				AssertEquals("Precondition: default registry value", MilestoneStatusVisibilityList.Codes.None, WebDataRegistry.Instance.MilestoneStatusVisibility.Value);

				var task = Factory.New<ProcessTask>();
				task.P9_Type = "MIL";
				task.SetMilestoneActualDateForTest(new ZDateTimeOffset(new ZDateTime(2009, 01, 10)));
				AssertEquals(new ZDateTime(2009, 01, 10), new TrackingMilestone(task).DisplayDate.ToZDateTime());
				AssertEquals(ZString.Empty, new TrackingMilestone(task).Status);
				AssertEquals("Completed", new TrackingMilestone(task).ActualStatus);

				task.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2009, 01, 09)));
				AssertEquals(new ZDateTime(2009, 01, 10), new TrackingMilestone(task).DisplayDate.ToZDateTime());
				AssertEquals(ZString.Empty, new TrackingMilestone(task).Status);
				AssertEquals("Completed Late", new TrackingMilestone(task).ActualStatus);

				task.SetMilestoneActualDateForTest(ZDateTimeOffset.Empty);
				AssertEquals(new ZDateTime(2009, 01, 09), new TrackingMilestone(task).DisplayDate.ToZDateTime());
				AssertEquals(ZString.Empty, new TrackingMilestone(task).Status);
				AssertEquals("Overdue", new TrackingMilestone(task).ActualStatus);

				task.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(ZDateTime.Now.AddDays(1)));
				AssertEquals(ZString.Empty, new TrackingMilestone(task).Status);
				AssertEquals("Pending", new TrackingMilestone(task).ActualStatus);
			}
		}

		public void TestDatesWithSuppression()
		{
			MilestoneSuppressionDetailsDummy details = new MilestoneSuppressionDetailsDummy();
			ProcessTask task = Factory.New<ProcessTask>();
			task.P9_Type = "MIL";

			Globals.IsWeb = true;

			CodeDescriptionBoolCollection newRegistryItem = new CodeDescriptionBoolCollection();

			task.SetMilestoneActualDateForTest(new ZDateTimeOffset(new ZDateTime(2009, 01, 10)));
			task.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2009, 02, 10)));
			details.IsPassengerFlight = true;
			details.HasFinalRoutingLegATDPassed = false;
			details.IsAir = true;
			details.JobDirection = Directions.Domestic;
			task.TriggerConditions.TriggerEventCode = AutoEvents.ArrivalDetailsChangedCode;
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2009, 01, 10)), new TrackingMilestone(task, details).ActualDate);
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2009, 01, 10)), new TrackingMilestone(task, details).ActualDateWithSuppression);
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2009, 02, 10)), new TrackingMilestone(task, details).EstimatedDate);
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2009, 02, 10)), new TrackingMilestone(task, details).EstimatedDateWithSuppression);
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2009, 01, 10)), new TrackingMilestone(task, details).DisplayDate);

			task.TriggerConditions.TriggerEventCode = AutoEvents.ArrivalCode;
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2009, 01, 10)), new TrackingMilestone(task, details).ActualDate);
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2009, 01, 10)), new TrackingMilestone(task, details).ActualDateWithSuppression);
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2009, 02, 10)), new TrackingMilestone(task, details).EstimatedDate);
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2009, 02, 10)), new TrackingMilestone(task, details).EstimatedDateWithSuppression);
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2009, 01, 10)), new TrackingMilestone(task, details).DisplayDate);

			task.TriggerConditions.TriggerEventCode = AutoEvents.DeliveredCode;
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2009, 01, 10)), new TrackingMilestone(task, details).ActualDate);
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2009, 01, 10)), new TrackingMilestone(task, details).ActualDateWithSuppression);
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2009, 02, 10)), new TrackingMilestone(task, details).EstimatedDate);
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2009, 02, 10)), new TrackingMilestone(task, details).EstimatedDateWithSuppression);
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2009, 01, 10)), new TrackingMilestone(task, details).DisplayDate);

			task.TriggerConditions.TriggerEventCode = AutoEvents.DepartureCode;
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2009, 01, 10)), new TrackingMilestone(task, details).ActualDate);
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2009, 01, 10)), new TrackingMilestone(task, details).ActualDateWithSuppression);
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2009, 02, 10)), new TrackingMilestone(task, details).EstimatedDate);
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2009, 02, 10)), new TrackingMilestone(task, details).EstimatedDateWithSuppression);
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2009, 01, 10)), new TrackingMilestone(task, details).DisplayDate);

			SuppressionForTest.CacheObjectClear();
			SuppressionTest.SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForDomestic, true);

			task.TriggerConditions.TriggerEventCode = AutoEvents.DepartureCode;
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2009, 01, 10)), new TrackingMilestone(task, details).ActualDate);
			AssertEquals(new ZDateTimeOffset(Suppression.SuppressedDate), new TrackingMilestone(task, details).ActualDateWithSuppression);
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2009, 02, 10)), new TrackingMilestone(task, details).EstimatedDate);
			AssertEquals(new ZDateTimeOffset(Suppression.SuppressedDate), new TrackingMilestone(task, details).EstimatedDateWithSuppression);
			AssertEquals(new ZDateTimeOffset(Suppression.SuppressedDate), new TrackingMilestone(task, details).DisplayDate);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var milestone = Factory.New<ProcessTask>();
			using (milestone.SuspendSettingHasChanges())
			{
				milestone.P9_Type = "MIL";
			}
			return new TrackingMilestone(milestone);
		}

		#region MilestoneSuppressionDetailsDummy

		protected class MilestoneSuppressionDetailsDummy : IFlightDetailsSuppression
		{
			public ZBool HasActualRCVPassed
			{
				get { return hasActualRCVPassed; }
				set { hasActualRCVPassed = value; }
			}
			ZBool hasActualRCVPassed;

			public ZBool HasETDPassed
			{
				get { return hasETDPassed; }
				set { hasETDPassed = value; }
			}
			ZBool hasETDPassed;

			public ZBool HasFinalRoutingLegATDPassed
			{
				get { return hasFinalRoutingLegATDPassed; }
				set { hasFinalRoutingLegATDPassed = value; }
			}
			ZBool hasFinalRoutingLegATDPassed;

			public bool IsAir
			{
				get { return isAir; }
				set { isAir = value; }
			}
			bool isAir;

			public ZBool IsPassengerFlight
			{
				get { return isPassengerFlight; }
				set { isPassengerFlight = value; }
			}
			ZBool isPassengerFlight;

			public Directions JobDirection
			{
				get { return jobDirection; }
				set { jobDirection = value; }
			}
			Directions jobDirection = Directions.Unknown;
		}

		#endregion
	}
}
