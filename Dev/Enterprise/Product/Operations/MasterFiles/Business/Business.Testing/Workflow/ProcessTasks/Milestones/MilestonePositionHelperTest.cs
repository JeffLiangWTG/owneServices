using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class MilestonePositionHelperTest : TestCaseWithFactory
	{
		public void TestLastMilestone()
		{
			var milestone = Collection.AddNew();
			milestone.IsMilestone = true;
			milestone.SetMilestoneActualDateForTest(ZDateTime.Empty);
			milestone.P9_ScheduledDateForBinding = ZDateTimeOffset.Today;

			var helper = new MilestonePositionHelper((ProcessTask[])Collection.ToArray(typeof(ProcessTask)));
			AssertNull(helper.LastMilestone);

			milestone = Collection.AddNew();
			milestone.IsMilestone = true;
			milestone.SetMilestoneActualDateForTest(ZDateTime.Today);
			milestone.P9_ScheduledDateForBinding = ZDateTimeOffset.Empty;

			helper = new MilestonePositionHelper((ProcessTask[])Collection.ToArray(typeof(ProcessTask)));
			var lastMilestone = helper.LastMilestone;
			AssertNotNull(lastMilestone);
			AssertDateTimeWithinOneSecond("", ZDateTime.Today.ToDateTime(), lastMilestone.P9_ActualDate.ToDateTime());
			AssertEquals(milestone, lastMilestone);

			milestone = Collection.AddNew();
			milestone.IsMilestone = true;
			milestone.P9_ScheduledDateForBinding = ZDateTimeOffset.Today;
			milestone.SetMilestoneActualDateForTest(ZDateTime.Today.AddDays(1));

			helper = new MilestonePositionHelper((ProcessTask[])Collection.ToArray(typeof(ProcessTask)));
			lastMilestone = helper.LastMilestone;
			AssertNotNull(lastMilestone);
			AssertDateTimeWithinOneSecond("", ZDateTime.Today.AddDays(1).ToDateTime(), lastMilestone.P9_ActualDate.ToDateTime());
			AssertEquals(milestone, lastMilestone);
		}

		public void TestCurrentCompanyLastMilestone()
		{
			var newCompany = Factory.New<GlbCompany>();

			var milestone = Collection.AddNew();
			milestone.IsMilestone = true;
			milestone.SetMilestoneActualDateForTest(new ZDateTime(2016, 4, 21, 12, 12, 12));
			milestone.P9_ScheduledDateForBinding = ZDateTimeOffset.Empty;

			var milestone2 = Collection.AddNew();
			milestone2.IsMilestone = true;
			milestone2.SetMilestoneActualDateForTest(new ZDateTime(2016, 4, 22, 10, 0, 0));

			var anotherCompanyTask = Collection.AddNew();
			anotherCompanyTask.IsMilestone = true;
			anotherCompanyTask.SetMilestoneActualDateForTest(new ZDateTime(2016, 4, 25, 10, 0, 0));
			anotherCompanyTask.P9_ScheduledDateForBinding = ZDateTimeOffset.Empty;
			anotherCompanyTask.P9_GC = newCompany.PK;

			var helper = new MilestonePositionHelper(Collection.Cast<ProcessTask>());
			AssertNotNull(helper.CurrentCompanyLastMilestone);
			AssertEquals(GlbCompany.CurrentCompany.PK, helper.CurrentCompanyLastMilestone.P9_GC);
			AssertEquals(milestone2.PK, helper.CurrentCompanyLastMilestone.PK);
		}

		[TestDate(2016, 4, 26)]
		public void TestNextMilestone()
		{
			var milestone = Collection.AddNew();
			milestone.IsMilestone = true;
			milestone.SetMilestoneActualDateForTest(ZDateTime.Today);
			milestone.P9_ScheduledDateForBinding = ZDateTimeOffset.Empty;

			var helper = new MilestonePositionHelper((ProcessTask[])Collection.ToArray(typeof(ProcessTask)));
			AssertNull(helper.NextMilestone);

			milestone = Collection.AddNew();
			milestone.IsMilestone = true;
			milestone.SetMilestoneActualDateForTest(ZDateTimeOffset.Empty);

			helper = new MilestonePositionHelper((ProcessTask[])Collection.ToArray(typeof(ProcessTask)));
			var nextMilestone = helper.NextMilestone;
			AssertNotNull(nextMilestone);
			AssertEquals(ZDateTimeOffset.Empty, nextMilestone.P9_ActualDateForBinding);
			AssertEquals(milestone, nextMilestone);

			milestone.P9_ScheduledDateForBinding = ZDateTimeOffset.Today;

			helper = new MilestonePositionHelper((ProcessTask[])Collection.ToArray(typeof(ProcessTask)));
			nextMilestone = helper.NextMilestone;
			AssertNotNull(nextMilestone);
			AssertEquals(ZDateTimeOffset.Today, nextMilestone.P9_ScheduledDateForBinding);
			AssertEquals(ZDateTimeOffset.Empty, nextMilestone.P9_ActualDateForBinding);
			AssertEquals(milestone, nextMilestone);

			var milestone2 = Collection.AddNew();
			milestone2.IsMilestone = true;
			milestone2.SetMilestoneActualDateForTest(ZDateTimeOffset.Empty);
			milestone2.P9_ScheduledDateForBinding = ZDateTimeOffset.Today.AddDays(1);

			helper = new MilestonePositionHelper((ProcessTask[])Collection.ToArray(typeof(ProcessTask)));
			nextMilestone = helper.NextMilestone;
			AssertNotNull(nextMilestone);
			AssertEquals(ZDateTimeOffset.Today, nextMilestone.P9_ScheduledDateForBinding);
			AssertEquals(ZDateTimeOffset.Empty, nextMilestone.P9_ActualDateForBinding);
			AssertEquals(milestone, nextMilestone);
		}

		[TestDate(2016, 4, 26)]
		public void TestCurrentCompanyNextMilestone()
		{
			var newCompany = Factory.New<GlbCompany>();

			var anotherCompanyMilestone = Collection.AddNew();
			anotherCompanyMilestone.IsMilestone = true;
			anotherCompanyMilestone.SetMilestoneActualDateForTest(ZDateTime.Empty);
			anotherCompanyMilestone.P9_ScheduledDateForBinding = ZDateTimeOffset.Empty;
			anotherCompanyMilestone.P9_GC = newCompany.PK;

			var task = Collection.AddNew();
			task.IsMilestone = true;
			task.SetMilestoneActualDateForTest(ZDateTime.Empty);
			task.P9_ScheduledDateForBinding = ZDateTimeOffset.Empty;

			var helper = new MilestonePositionHelper((ProcessTask[])Collection.ToArray(typeof(ProcessTask)));
			var processTask = helper.CurrentCompanyNextMilestone;
			AssertNotNull(processTask);
			AssertEquals(ZDateTimeOffset.Empty, processTask.P9_ActualDateForBinding);
			AssertEquals(GlbCompany.CurrentCompany.PK, processTask.P9_GC);

			task.P9_ScheduledDateForBinding = ZDateTimeOffset.Today;
			anotherCompanyMilestone.P9_ScheduledDateForBinding = ZDateTimeOffset.Today;

			helper = new MilestonePositionHelper((ProcessTask[])Collection.ToArray(typeof(ProcessTask)));
			processTask = helper.CurrentCompanyNextMilestone;
			AssertNotNull(processTask);
			AssertEquals(ZDateTimeOffset.Today, processTask.P9_ScheduledDateForBinding);
			AssertEquals(ZDateTimeOffset.Empty, processTask.P9_ActualDateForBinding);
			AssertEquals(GlbCompany.CurrentCompany.PK, processTask.P9_GC);

			var task2 = Collection.AddNew();
			task2.IsMilestone = true;
			task2.SetMilestoneActualDateForTest(ZDateTime.Empty);
			task2.P9_ScheduledDateForBinding = ZDateTimeOffset.Today.AddDays(1);

			var anotherCompanyTask2 = Collection.AddNew();
			anotherCompanyTask2.IsMilestone = true;
			anotherCompanyTask2.SetMilestoneActualDateForTest(ZDateTimeOffset.Empty);
			anotherCompanyTask2.P9_ScheduledDateForBinding = ZDateTimeOffset.Today.AddDays(1);
			anotherCompanyTask2.P9_GC = newCompany.PK;

			helper = new MilestonePositionHelper((ProcessTask[])Collection.ToArray(typeof(ProcessTask)));
			processTask = helper.CurrentCompanyNextMilestone;
			AssertNotNull(processTask);
			AssertEquals(ZDateTimeOffset.Today, processTask.P9_ScheduledDateForBinding);
			AssertEquals(ZDateTimeOffset.Empty, processTask.P9_ActualDateForBinding);
			AssertEquals(GlbCompany.CurrentCompany.PK, processTask.P9_GC);
		}

		#region Implementation

		DummyWithWorkflow Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyWithWorkflow>()); }
		}
		DummyWithWorkflow dummy;

		public MilestoneCollectionView Collection
		{
			get { return fCollection ?? (fCollection = new MilestoneCollectionView(Dummy.WorkflowItems)); }
		}
		MilestoneCollectionView fCollection;

		#endregion
	}
}
