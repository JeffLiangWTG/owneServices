using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Web;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingMilestoneCollection))]
	sealed class TrackingMilestoneCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TrackingMilestoneCollection>
	{
		protected override TrackingMilestoneCollection GetCollectionToTest()
		{
			return new TrackingMilestoneCollection(new TrackingBooking(QuotedBooking.CreateNewBooking(Factory).PK, Factory, null));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var task = Factory.New<ProcessTask>();
			using (task.SuspendSettingHasChanges())
			{
				task.P9_Type = "MIL";
			}
			return new TrackingMilestone(task);
		}

		public void TestNextMilestone()
		{
			using (WebDataRegistry.Instance.MilestoneStatusVisibility.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneStatusVisibilityList.Codes.None))
			{
				var testBooking = new TrackingBooking(QuotedBooking.CreateNewBooking(Factory).PK, Factory, null);

				var testDate = ZDateTimeOffset.Now;

				var testMilestone = testBooking.QuotedBooking.WorkflowItems.Milestones.AddNew();
				testMilestone.P9_Description = "completed";
				testMilestone.P9_IsPublished = true;
				testMilestone.SetMilestoneActualDateForTest(testDate);
				testMilestone.SetMilestoneScheduledDateForTest(testDate.AddHours(-1).AddMinutes(1));

				testMilestone = testBooking.QuotedBooking.WorkflowItems.Milestones.AddNew();
				testMilestone.P9_Description = "completed";
				testMilestone.P9_IsPublished = true;
				testMilestone.SetMilestoneActualDateForTest(testDate);
				testMilestone.SetMilestoneScheduledDateForTest(testDate.AddHours(-1).AddMinutes(1));

				var milestones = new TrackingMilestoneCollection(testBooking.QuotedBooking);
				AssertNull(milestones.NextMilestone);

				testMilestone = testBooking.QuotedBooking.WorkflowItems.Milestones.AddNew();
				testMilestone.P9_Description = "pending";
				testMilestone.P9_IsPublished = true;
				testMilestone.SetMilestoneScheduledDateForTest(testDate);
				AssertEquals(ProcessTask.Pending, testMilestone.Status);
				milestones = new TrackingMilestoneCollection(testBooking.QuotedBooking);
				AssertEquals("pending", milestones.NextMilestone.Description);

				testMilestone.P9_Description = "overdue";
				testMilestone.SetMilestoneScheduledDateForTest(testDate.AddDays(-1));
				AssertEquals(ProcessTask.Overdue, testMilestone.Status);
				milestones = new TrackingMilestoneCollection(testBooking.QuotedBooking);
				AssertEquals("overdue", milestones.NextMilestone.Description);
			}
		}

		public void TestLastMilestone()
		{
			using (WebDataRegistry.Instance.MilestoneStatusVisibility.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneStatusVisibilityList.Codes.None))
			{
				string cachedRegistrySortOrderValue = WebDataRegistry.Instance.MilestoneSortOrder.Value;
				WebDataRegistry.Instance.MilestoneSortOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneSortOrderList.Codes.Chronological);

				TrackingBooking testBooking = new TrackingBooking(QuotedBooking.CreateNewBooking(Factory).PK, Factory, null);

				var testDate = ZDateTimeOffset.Now;

				ProcessTask testLastCompletedMilestone1 = testBooking.QuotedBooking.WorkflowItems.Milestones.AddNew();
				testLastCompletedMilestone1.P9_Description = "lastCompleted1";
				testLastCompletedMilestone1.P9_IsPublished = true;
				testLastCompletedMilestone1.SetMilestoneActualDateForTest(testDate);
				testLastCompletedMilestone1.SetMilestoneScheduledDateForTest(testDate.AddHours(-1).AddMinutes(1));
				testLastCompletedMilestone1.P9_Sequence = 1;

				ProcessTask testPendingMilestone = testBooking.QuotedBooking.WorkflowItems.Milestones.AddNew();
				testPendingMilestone.P9_Description = "pending";
				testPendingMilestone.P9_IsPublished = true;
				testPendingMilestone.SetMilestoneScheduledDateForTest(testDate);
				testPendingMilestone.P9_Sequence = 2;

				ProcessTask testCompletedMilestone = testBooking.QuotedBooking.WorkflowItems.Milestones.AddNew();
				testCompletedMilestone.P9_Description = "completed";
				testCompletedMilestone.P9_IsPublished = true;
				testCompletedMilestone.SetMilestoneActualDateForTest(testDate.AddHours(-1));
				testCompletedMilestone.SetMilestoneScheduledDateForTest(testDate);
				testCompletedMilestone.P9_Sequence = 3;

				ProcessTask testLastCompletedMilestone2 = testBooking.QuotedBooking.WorkflowItems.Milestones.AddNew();
				testLastCompletedMilestone2.P9_Description = "lastCompleted2";
				testLastCompletedMilestone2.P9_IsPublished = true;
				testLastCompletedMilestone2.SetMilestoneActualDateForTest(testDate);
				testLastCompletedMilestone2.SetMilestoneScheduledDateForTest(testDate.AddHours(-1).AddMinutes(1));
				testLastCompletedMilestone2.P9_Sequence = 4;

				TrackingMilestoneCollection testMilestones = new TrackingMilestoneCollection(testBooking.QuotedBooking);

				AssertEquals("lastCompleted2", testMilestones.LastMilestone.Description);

				WebDataRegistry.Instance.MilestoneSortOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneSortOrderList.Codes.ReversedChronological);
				testMilestones = new TrackingMilestoneCollection(testBooking.QuotedBooking);

				AssertEquals("lastCompleted2", testMilestones.LastMilestone.Description);

				WebDataRegistry.Instance.MilestoneSortOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneSortOrderList.Codes.Sequence);
				testMilestones = new TrackingMilestoneCollection(testBooking.QuotedBooking);

				AssertEquals("lastCompleted2", testMilestones.LastMilestone.Description);

				WebDataRegistry.Instance.MilestoneSortOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedRegistrySortOrderValue);
			}
		}

		public void TestEditMode()
		{
			AssertEquals("Precondition: default registry value", MilestoneVisibilityList.Codes.All, WebDataRegistry.Instance.MilestoneVisibility.Value);

			TrackingBooking booking = new TrackingBooking(QuotedBooking.CreateNewBooking(Factory).PK, Factory, null);

			var now = ZDateTimeOffset.Now;

			ProcessTask unpublished = booking.QuotedBooking.WorkflowItems.Milestones.AddNew();
			unpublished.P9_Description = "Unpublished";
			unpublished.TriggerConditions.TriggerEventCode = "AAA";
			unpublished.P9_IsPublished = false;
			AssertEquals(unpublished.Status, "");

			ProcessTask completed = booking.QuotedBooking.WorkflowItems.Milestones.AddNew();
			completed.P9_Description = "Completed";
			completed.TriggerConditions.TriggerEventCode = "BBB";
			completed.P9_IsPublished = true;
			completed.SetMilestoneActualDateForTest(now.AddHours(-1));
			completed.SetMilestoneScheduledDateForTest(now);

			AssertEquals(completed.Status, "Completed");

			ProcessTask lastCompleted = booking.QuotedBooking.WorkflowItems.Milestones.AddNew();
			lastCompleted.P9_Description = "Completed";
			lastCompleted.TriggerConditions.TriggerEventCode = "CCC";
			lastCompleted.P9_IsPublished = true;
			lastCompleted.SetMilestoneActualDateForTest(now.AddHours(-1));
			lastCompleted.SetMilestoneScheduledDateForTest(now);
			AssertEquals(lastCompleted.Status, "Completed");

			ProcessTask pending = booking.QuotedBooking.WorkflowItems.Milestones.AddNew();
			pending.P9_Description = "Pending";
			pending.TriggerConditions.TriggerEventCode = "DDD";
			pending.P9_IsPublished = true;
			pending.SetMilestoneScheduledDateForTest(now);
			AssertEquals(pending.Status, "Pending");

			TrackingMilestoneCollection collection = new TrackingMilestoneCollection(booking.QuotedBooking);
			AssertEquals(3, collection.Count);

			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.CompletedMilestonesOnly);
			collection = new TrackingMilestoneCollection(booking.QuotedBooking);
			AssertEquals(2, collection.Count);

			IUpdatableMilestoneEventsProvider updMilestoneEventsProvider = booking;

			updMilestoneEventsProvider.UpdatableMilestoneEventCodes.Add("AAA");
			updMilestoneEventsProvider.UpdatableMilestoneEventCodes.Add("CCC");
			updMilestoneEventsProvider.UpdatableMilestoneEventCodes.Add("DDD");

			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.CompletedMilestonesOnly);
			TrackingMilestoneCollection editableMilestonesCollection = new TrackingMilestoneCollection(booking.QuotedBooking, updMilestoneEventsProvider, true);//Edit Mode
			AssertEquals(2, editableMilestonesCollection.Count);
			AssertEquals(editableMilestonesCollection[0].EventCode, "CCC");
			AssertEquals(editableMilestonesCollection[1].EventCode, "DDD");
		}

		public void TestMilestoneSortOrder()
		{
			string originalRegistrySortOrderValue = WebDataRegistry.Instance.MilestoneSortOrder.Value;
			WebDataRegistry.Instance.MilestoneSortOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneSortOrderList.Codes.Chronological);

			TrackingBooking booking = new TrackingBooking(QuotedBooking.CreateNewBooking(Factory).PK, Factory, null);

			var now = new ZDateTimeOffset(ZDateTime.SmallDateTimeNow);
			var milestones = booking.QuotedBooking.WorkflowItems.Milestones;

			ProcessTask completed = milestones.AddNew();
			completed.P9_Description = "completed";
			completed.P9_IsPublished = true;
			completed.SetMilestoneActualDateForTest(now.AddHours(-1));
			completed.SetMilestoneScheduledDateForTest(now);
			completed.P9_Sequence = 1;

			ProcessTask lastCompleted = milestones.AddNew();
			lastCompleted.P9_Description = "lastCompleted";
			lastCompleted.P9_IsPublished = true;
			lastCompleted.SetMilestoneActualDateForTest(now);
			lastCompleted.SetMilestoneScheduledDateForTest(now.AddHours(-1).AddMinutes(1));
			lastCompleted.P9_Sequence = 3;

			ProcessTask pending = milestones.AddNew();
			pending.P9_Description = "pending";
			pending.P9_IsPublished = true;
			pending.SetMilestoneScheduledDateForTest(now);
			pending.P9_Sequence = 2;

			milestones.Sort(milestones.GetDefaultOrderComparer());

			TrackingMilestoneCollection collection = new TrackingMilestoneCollection(booking.QuotedBooking);

			AssertEquals("completed", collection[0].Description);
			AssertEquals("lastCompleted", collection[1].Description);
			AssertEquals("pending", collection[2].Description);

			WebDataRegistry.Instance.MilestoneSortOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneSortOrderList.Codes.ReversedChronological);
			collection = new TrackingMilestoneCollection(booking.QuotedBooking);

			AssertEquals("lastCompleted", collection[0].Description);
			AssertEquals("pending", collection[1].Description);
			AssertEquals("completed", collection[2].Description);

			WebDataRegistry.Instance.MilestoneSortOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneSortOrderList.Codes.Sequence);
			collection = new TrackingMilestoneCollection(booking.QuotedBooking);

			AssertEquals("completed", collection[0].Description);
			AssertEquals("pending", collection[1].Description);
			AssertEquals("lastCompleted", collection[2].Description);

			WebDataRegistry.Instance.MilestoneSortOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneSortOrderList.Codes.NoSort);
			collection = new TrackingMilestoneCollection(booking.QuotedBooking);

			AssertEquals("completed", collection[0].Description);
			AssertEquals("lastCompleted", collection[1].Description);
			AssertEquals("pending", collection[2].Description);

			WebDataRegistry.Instance.MilestoneSortOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalRegistrySortOrderValue);
		}

		public void TestCreateByIWorkflowProvider()
		{
			AssertEquals("Precondition: default registry value", MilestoneVisibilityList.Codes.All, WebDataRegistry.Instance.MilestoneVisibility.Value);

			TrackingBooking booking = new TrackingBooking(QuotedBooking.CreateNewBooking(Factory).PK, Factory, null);

			var now = new ZDateTimeOffset(ZDateTime.SmallDateTimeNow);

			ProcessTask unpublished = booking.QuotedBooking.WorkflowItems.Milestones.AddNew();
			unpublished.P9_Description = "unpublished";
			unpublished.P9_IsPublished = false;
			unpublished.SetMilestoneActualDateForTest(now);

			ProcessTask completed = booking.QuotedBooking.WorkflowItems.Milestones.AddNew();
			completed.P9_Description = "completed";
			completed.P9_IsPublished = true;
			completed.SetMilestoneActualDateForTest(now.AddHours(-1));
			completed.SetMilestoneScheduledDateForTest(now);

			ProcessTask lastCompleted = booking.QuotedBooking.WorkflowItems.Milestones.AddNew();
			lastCompleted.P9_Description = "lastCompleted";
			lastCompleted.P9_IsPublished = true;
			lastCompleted.SetMilestoneActualDateForTest(now);
			lastCompleted.SetMilestoneScheduledDateForTest(now.AddHours(-1));

			ProcessTask pending = booking.QuotedBooking.WorkflowItems.Milestones.AddNew();
			pending.P9_Description = "pending";
			pending.P9_IsPublished = true;
			pending.SetMilestoneScheduledDateForTest(now);

			TrackingMilestoneCollection collection = new TrackingMilestoneCollection(booking.QuotedBooking);

			AssertEquals(3, collection.Count);
			AssertEquals("completed", collection[0].Description);   //milestones should be sorted by DisplayDate (instead of ScheduledDate) and then by sequence
			AssertEquals("lastCompleted", collection[1].Description);
			AssertEquals("pending", collection[2].Description);

			AssertEquals(collection[1].Description, collection.LastMilestone.Description);
			AssertEquals(collection[2].Description, collection.NextMilestone.Description);

			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.CompletedMilestonesOnly);
			collection = new TrackingMilestoneCollection(booking.QuotedBooking);

			AssertEquals(2, collection.Count);
			AssertEquals("completed", collection[0].Description);
			AssertEquals("lastCompleted", collection[1].Description);

			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.LastCompletedMilestoneOnly);
			collection = new TrackingMilestoneCollection(booking.QuotedBooking);

			AssertEquals(1, collection.Count);
			AssertEquals("lastCompleted", collection[0].Description);

			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.None);
			collection = new TrackingMilestoneCollection(booking.QuotedBooking);

			AssertEquals(0, collection.Count);
		}

		public void TestAddForEmailReporting_SkipsDeleted()
		{
			var booking = new TrackingBooking(QuotedBooking.CreateNewBooking(Factory).PK, Factory, null);
			var notDeleted = booking.QuotedBooking.WorkflowItems.Milestones.AddNew();
			notDeleted.P9_Description = "NotDeleted";
			notDeleted.P9_IsPublished = true;
			var deleted = booking.QuotedBooking.WorkflowItems.Milestones.AddNew();
			deleted.P9_Description = "Deleted";
			deleted.P9_IsPublished = true;

			var noTaskThisConstructorShouldBeRemoved = new TrackingMilestone("NoTask", ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, 0);
			var milestoneCollection = new TrackingMilestoneCollection(booking.QuotedBooking)
				{
					noTaskThisConstructorShouldBeRemoved
				};

			deleted.Delete();

			var propertiesForEmailReporting = new PropertyChangeInfoCollection();
			milestoneCollection.AddForEmailReporting(DataState.Original, propertiesForEmailReporting);
			var properties = propertiesForEmailReporting.GetValuesAsArray();

			AssertEquals(2, properties.Length);
			var humanReadableNames = properties.Select(x => x.HumanReadableName.ToString());
			AssertContainsExactElementsInAnyOrder(new[] { "Milestone: NotDeleted", "Milestone: NoTask" }, humanReadableNames);
		}
	}
}
