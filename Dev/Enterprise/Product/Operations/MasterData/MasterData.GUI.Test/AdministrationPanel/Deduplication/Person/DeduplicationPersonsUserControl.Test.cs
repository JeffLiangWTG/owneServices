using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.GUI.Tests
{
	public class DeduplicationPersonsUserControlTest : TestCaseWithFactory
	{
		public void TestAutoDeduplicationForToBeProcessedRecords()
		{
			var scheduler = new SynchronousTaskSchedulerForTest();
			ObjectFactory.Substitute<TaskScheduler>(scheduler);

			using (TestingForm)
			{
				var registrySetting = SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value;
				try
				{
					SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					TestingForm.Show();
					var masterPerson = Factory.NewWithValidTestData<GlbPerson>();
					masterPerson.PER_FullName = "TESTPERSON00";
					Factory.Save();
					var dedupPerson = Factory.Load<DeduplicationPerson>(masterPerson.PK);
					TestingControl.DeduplicationResultsForTest = new List<ScoringResult>();
					var codeFilter = TestingControl.PersonFilterControl.FilterBusinessObject["Full Name"] as ModuleTextFilter;
					codeFilter.IsActive = true;
					codeFilter.Property = "TESTPERSON00";
					AssertEquals("To Be Processed", dedupPerson.DPE_Status);
					TestingControl.PersonFilterControl.FirePerformSearch();
					dedupPerson.Reload();
					AssertEquals("Processed", dedupPerson.DPE_Status);
					var nduResult = Factory.LoadTop1<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, masterPerson.PK));
					AssertEquals("NDU", nduResult.PMT_Status);
					AssertEquals("NDU result doesn't go into the collection", 0, dedupPerson.MatchingResultCollection.Count);
				}
				finally
				{
					SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
				}
			}
		}

		public void TestShouldNotCauseErrorWhenClickTheRecordWhichHasEmptyTarget()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_FullName = "TESTPERSON1";
			person2.PER_FullName = "TESTPERSON2";
			Factory.Save();

			using (TestingForm)
			{
				var registrySetting = SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value;
				try
				{
					SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					TestingForm.Show();
					var filter = TestingControl.PersonFilterControl.FilterBusinessObject["Full Name"] as ModuleTextFilter;
					filter.IsActive = true;
					filter.Property = "TESTPERSON";
					filter.ComparisonOperator = "starts with";

					var thread = Task.Factory.StartNew(() =>
					{
						TestingControl.PersonFilterControl.FirePerformSearch();
					}, CancellationToken.None, TaskCreationOptions.DenyChildAttach, new SynchronousTaskSchedulerForTest());
					thread.Wait();
					AssertEquals(2, TestingControl.PersonFilterControl.GridCollection.Count);
					using (Db.DisposableActionForDbConnection())
					{
						var currentItem = TestingControl.PersonFilterControl.Grid.GetCurrent();
						var otherItemPk = currentItem.PK == person1.PK ? person2.PK : person1.PK;
						AssertNoExceptionThrown(() => TestingControl.PersonFilterControl.Grid.SelectSingleElementByPK(otherItemPk));
						AssertEquals("No exception, the record has been saved.", 1, Factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, otherItemPk)).Length);
					}
				}
				finally
				{
					SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
				}
			}
		}

		public void TestExcludeIncludeActionUpdatesStatus()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_FullName = "TESTPERSON1";
			person2.PER_FullName = "TESTPERSON2";
			Factory.Save();

			using (TestingForm)
			{
				var registrySetting = SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value;
				try
				{
					SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					TestingForm.Show();
					var filter = TestingControl.PersonFilterControl.FilterBusinessObject["Full Name"] as ModuleTextFilter;
					filter.IsActive = true;
					filter.Property = "TESTPERSON";
					filter.ComparisonOperator = "starts with";
					TestingControl.DeduplicationResultsForTest = new List<ScoringResult>();

					var thread = Task.Factory.StartNew(() =>
					{
						TestingControl.PersonFilterControl.FirePerformSearch();
					}, CancellationToken.None, TaskCreationOptions.DenyChildAttach, new SynchronousTaskSchedulerForTest());
					thread.Wait();
					AssertEquals(2, TestingControl.PersonFilterControl.GridCollection.Count);
					using (Db.DisposableActionForDbConnection())
					{
						var currentItem = TestingControl.PersonFilterControl.Grid.GetCurrent();
						var dedupPerson = currentItem as DeduplicationPerson;
						AssertEquals("Expected status processed", "Processed", dedupPerson.DPE_Status);
						((IDeduplicatable)person1).IsExcludedFromDeduplication = true;
						TestingControl.PerformCurrentPersonDeduplicationActionOccurred(DeduplicationAction.MasterExclusionToggled);
						dedupPerson = TestingControl.PersonFilterControl.Grid.GetCurrent() as DeduplicationPerson;
						AssertEquals("Expected status excluded", "Excluded", dedupPerson.DPE_Status);
						((IDeduplicatable)person1).IsExcludedFromDeduplication = false;
						TestingControl.PerformCurrentPersonDeduplicationActionOccurred(DeduplicationAction.MasterExclusionToggled);
						dedupPerson = TestingControl.PersonFilterControl.Grid.GetCurrent() as DeduplicationPerson;
						AssertEquals("Expected status processed", "Processed", dedupPerson.DPE_Status);
					}
				}
				finally
				{
					SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
				}
			}
		}

		public void TestPersonFilterControlShouldRerunSearchWhenClickOnRecordThatWasDeleted()
		{
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			var person3 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_FullName = "TESTPERSON01";
			person2.PER_FullName = "TESTPERSON02";
			person3.PER_FullName = "TESTPERSON03";
			Factory.Save();

			using (TestingForm)
			{
				TestingForm.Show();
				var filter = TestingControl.PersonFilterControl.FilterBusinessObject["Full Name"] as ModuleTextFilter;
				filter.IsActive = true;
				filter.Property = "TESTPERSON";
				filter.ComparisonOperator = "starts with";
				TestingControl.PersonFilterControl.FirePerformSearch();
				AssertEquals("Precondition: PersonFilterControl should have 3 result", 3, TestingControl.PersonFilterControl.GridCollection.Count);
				person2.Delete();
				person3.Delete();
				Factory.Save();
				AssertEquals("Precondition: PersonFilterControl should still have 3 result", 3, TestingControl.PersonFilterControl.GridCollection.Count);
				AssertEquals("Precondition: the current selected row is the first row", 0, TestingControl.PersonFilterControl.Grid.CurrentRowIndex);

				TestingControl.PersonFilterControl.Grid.CurrentRowIndex = 1;

				AssertEquals("PersonFilterControl should refresh and now have 1 result as 2 records were deleted", 1, TestingControl.PersonFilterControl.GridCollection.Count);
			}
		}

		public void TestPersonFilterControlShouldNotCrashWhenSelectingDeletedRecordInFilterStrip()
		{
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			var person3 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_FullName = "TESTPERSON01";
			person2.PER_FullName = "TESTPERSON02";
			person3.PER_FullName = "TESTPERSON03";
			Factory.Save();

			using (TestingForm)
			{
				TestingForm.Show();
				var filter = TestingControl.PersonFilterControl.FilterBusinessObject["Full Name"] as ModuleTextFilter;
				filter.IsActive = true;
				filter.Property = "TESTPERSON";
				filter.ComparisonOperator = "starts with";
				TestingControl.PersonFilterControl.FirePerformSearch();

				person1.Delete();
				person2.Delete();
				person3.Delete();
				Factory.Save();

				AssertNoExceptionThrown("Precondition: No exception will be thrown when selecting deleted row TESTPERSON01", () => { TestingControl.PersonFilterControl.Grid.CurrentRowIndex = 0; });
				AssertNoExceptionThrown("Precondition: No exception will be thrown when selecting deleted row TESTPERSON02", () => { TestingControl.PersonFilterControl.Grid.CurrentRowIndex = 1; });
				AssertNoExceptionThrown("Precondition: No exception will be thrown when selecting deleted row TESTPERSON03", () => { TestingControl.PersonFilterControl.Grid.CurrentRowIndex = 2; });
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestingForm = new ZForm(new AdministrationPanelManager(Factory));
			TestingForm.Controls.Add(new DeduplicationPersonsUserControlForTest());
		}

		protected override void TearDown()
		{
			base.TearDown();
			TestingForm?.Dispose();
		}

		ZForm TestingForm;
		DeduplicationPersonsUserControlForTest TestingControl => TestingForm.Controls["DeduplicationPersonsUserControl"] as DeduplicationPersonsUserControlForTest;

		#endregion
	}

	public class DeduplicationPersonsUserControlForTest : DeduplicationPersonsUserControl
	{
		public ZFilterStripControl PersonFilterControl => Controls.Find("DeduplicationPersonFilterControl", true)[0] as ZFilterStripControl;

		public IEnumerable<ScoringResult> DeduplicationResultsForTest;

		public bool IsOverrideListManager_CurrentChanged;

		public void PerformCurrentPersonDeduplicationActionOccurred(DeduplicationAction action)
		{
			CurrentPersonDeduplicationActionOccurred(null, new DuplicationEventArgs(null) { InvokedAction = action });
		}

		protected override void DedupPerson_DuplicateSearchingFinished(object sender, DuplicateSearchingFinishedEventArgs e)
		{
			var dedupPerson = sender as DeduplicationPerson;
			dedupPerson.SaveDeduplicationResults(DeduplicationResultsForTest);
			SetupDuplicationDetailView(dedupPerson);
		}

		protected override void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			if (IsOverrideListManager_CurrentChanged)
			{
				var dedupPerson = PersonFilterControl.Grid.ListManager.GetCurrent() as DeduplicationPerson;
				dedupPerson.DPE_Status = DeduplicationHelper.StatusConstants.Processed;
			}
			base.ListManager_CurrentChanged(sender, e);
		}
	}
}
