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
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.GUI.Tests
{
	public class DuplicatedOrganizationsUserControlTest : TestCaseWithFactory
	{
		public void TestCustomSQLFilter()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			using (TestingForm)
			{
				TestingForm.Show();
				var masterOrg = Factory.NewWithValidTestData<OrgHeader>();
				masterOrg.OH_Code = "TESTORG00";
				Factory.Save();
				var dedupOrg = Factory.Load<DeduplicationOrganisation>(masterOrg.PK);
				TestingControl.DeduplicationResultsForTest = new List<ScoringResult>();
				var filter = TestingControl.OrganisationFilterControl.FilterBusinessObject["Custom SQL Filter"] as ModuleSQLFilter;
				filter.IsActive = true;
				filter.Property1 = "1 = 1";
				TestingControl.OrganisationFilterControl.FirePerformSearch();
				AssertEquals(@"Found at least 1000 records.
Only showing the top 100 records.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!UnitTestUserNotification.Instance.LastMessage.WasError);
				filter.Property1 = "OH_PK IN (Select OH_PK From dbo.OrgHeader  Inner Join dbo.GlbStaff ON GS_Code = OH_SystemCreateUser  Inner Join dbo.GlbBranch ON GB_PK = GS_GB_HomeBranch Where GB_Code = 'ATL')";
				TestingControl.OrganisationFilterControl.FirePerformSearch();
				AssertEquals(@"Found no records.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!UnitTestUserNotification.Instance.LastMessage.WasError);
				filter.Property1 = "AAA";
				TestingControl.OrganisationFilterControl.FirePerformSearch();
				AssertEquals(@"There are errors. Please correct these before searching.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAutoDeduplicationForToBeProcessedRecords()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			using (TestingForm)
			{
				TestingForm.Show();
				var masterOrg = Factory.NewWithValidTestData<OrgHeader>();
				masterOrg.OH_Code = "TESTORG00";
				Factory.Save();
				var dedupOrg = Factory.Load<DeduplicationOrganisation>(masterOrg.PK);
				TestingControl.DeduplicationResultsForTest = new List<ScoringResult>();
				var codeFilter = TestingControl.OrganisationFilterControl.FilterBusinessObject["Code"] as ModuleTextFilter;
				codeFilter.IsActive = true;
				codeFilter.Property = "TESTORG00";
				AssertEquals("To Be Processed", dedupOrg.DOH_Status);
				TestingControl.OrganisationFilterControl.FirePerformSearch();
				dedupOrg.Reload();
				AssertEquals("Processed", dedupOrg.DOH_Status);
				var nduResult = Factory.LoadTop1<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, masterOrg.PK));
				AssertEquals("NDU", nduResult.PMT_Status);
				AssertEquals("NDU result doesn't go into the collection", 0, dedupOrg.MatchingResultCollection.Count);
			}
		}

		public void TestShouldNotCauseErrorWhenClickTheRecordWhichHasEmptyTarget()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TESTORG1";
			org2.OH_Code = "TESTORG2";
			org1.MainAddress.Address1 = "Address1";
			org2.MainAddress.Address1 = "Address1";
			Factory.Save();

			using (TestingForm)
			{
				TestingForm.Show();
				var filter = TestingControl.OrganisationFilterControl.FilterBusinessObject["Code"] as ModuleTextFilter;
				filter.IsActive = true;
				filter.Property = "TESTORG";
				filter.ComparisonOperator = "starts with";

				var thread = Task.Factory.StartNew(() =>
				{
					TestingControl.OrganisationFilterControl.FirePerformSearch();
				}, CancellationToken.None, TaskCreationOptions.DenyChildAttach, new SynchronousTaskSchedulerForTest());
				thread.Wait();
				AssertEquals(2, TestingControl.OrganisationFilterControl.GridCollection.Count);
				using (Db.DisposableActionForDbConnection())
				{
					var currentItem = TestingControl.OrganisationFilterControl.Grid.GetCurrent();
					var otherItemPk = currentItem.PK == org1.PK ? org2.PK : org1.PK;
					AssertNoExceptionThrown(() => TestingControl.OrganisationFilterControl.Grid.SelectSingleElementByPK(otherItemPk));
					AssertEquals("No exception, the record has been saved.", 1, Factory.Load<PatternMatchingResult>(new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, otherItemPk)).Length);
				}
			}
		}

		public void TestExcludeIncludeActionUpdatesStatus()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TESTORG1";
			org2.OH_Code = "TESTORG2";
			org1.MainAddress.Address1 = "Address1";
			org2.MainAddress.Address1 = "Address1";
			Factory.Save();

			using (TestingForm)
			{
				TestingForm.Show();
				var filter = TestingControl.OrganisationFilterControl.FilterBusinessObject["Code"] as ModuleTextFilter;
				filter.IsActive = true;
				filter.Property = "TESTORG";
				filter.ComparisonOperator = "starts with";
				TestingControl.DeduplicationResultsForTest = new List<ScoringResult>();

				var thread = Task.Factory.StartNew(() =>
				{
					TestingControl.OrganisationFilterControl.FirePerformSearch();
				}, CancellationToken.None, TaskCreationOptions.DenyChildAttach, new SynchronousTaskSchedulerForTest());
				thread.Wait();
				AssertEquals(2, TestingControl.OrganisationFilterControl.GridCollection.Count);
				using (Db.DisposableActionForDbConnection())
				{
					var currentItem = TestingControl.OrganisationFilterControl.Grid.GetCurrent();
					var dedupOrg = currentItem as DeduplicationOrganisation;
					AssertEquals("Expected status processed", "Processed", dedupOrg.DOH_Status);
					((IDeduplicatable)org1).IsExcludedFromDeduplication = true;
					TestingControl.PerformCurrentOrgDeduplicationActionOccurred(DeduplicationAction.MasterExclusionToggled);
					dedupOrg = TestingControl.OrganisationFilterControl.Grid.GetCurrent() as DeduplicationOrganisation;
					AssertEquals("Expected status excluded", "Excluded", dedupOrg.DOH_Status);
					((IDeduplicatable)org1).IsExcludedFromDeduplication = false;
					TestingControl.PerformCurrentOrgDeduplicationActionOccurred(DeduplicationAction.MasterExclusionToggled);
					dedupOrg = TestingControl.OrganisationFilterControl.Grid.GetCurrent() as DeduplicationOrganisation;
					AssertEquals("Expected status processed", "Processed", dedupOrg.DOH_Status);
				}
			}
		}

		public void TestPerformReloadRequiredActionOnProcessedOrgShouldSearchForDuplicates()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TESTDUP1";
			Factory.Save();

			using (TestingForm)
			{
				TestingForm.Show();
				var filter = TestingControl.OrganisationFilterControl.FilterBusinessObject["Code"] as ModuleTextFilter;
				filter.IsActive = true;
				filter.Property = "TESTDUP1";
				filter.ComparisonOperator = "starts with";
				var thread = Task.Factory.StartNew(() =>
				{
					TestingControl.OrganisationFilterControl.FirePerformSearch();
				}, CancellationToken.None, TaskCreationOptions.DenyChildAttach, new SynchronousTaskSchedulerForTest());
				thread.Wait();

				var currentlySelectedOrg = TestingControl.OrganisationFilterControl.Grid.GetCurrent() as DeduplicationOrganisation;
				AssertEquals("Precondition: selected org is TESTDUP1", "TESTDUP1", currentlySelectedOrg.MasterOrgHeader.OH_Code);

				TestingControl.PerformCurrentOrgDeduplicationActionOccurred(DeduplicationAction.ReloadRequired);

				AssertEquals("DuplicateSearchingFinished was invoked", true, TestingControl.IsDedupOrg_DuplicateSearchingFinished);
			}
		}

		public void TestOrganisationFilterControlShouldRerunSearchWhenClickOnRecordThatWasDeleted()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TESTORG1";
			org2.OH_Code = "TESTORG2";
			otherOrg.OH_Code = "OTHERORG";
			Factory.Save();

			using (TestingForm)
			{
				TestingForm.Show();
				var filter = TestingControl.OrganisationFilterControl.FilterBusinessObject["Code"] as ModuleTextFilter;
				filter.IsActive = true;
				filter.Property = "TESTORG";
				filter.ComparisonOperator = "starts with";
				TestingControl.OrganisationFilterControl.FirePerformSearch();
				AssertEquals("Precondition: OrganisationFilterControl should have 2 result", 2, TestingControl.OrganisationFilterControl.GridCollection.Count);

				org2.Delete();
				Factory.Save();
				TestingControl.OrganisationFilterControl.Grid.CurrentRowIndex = 1;

				AssertEquals("OrganisationFilterControl should refresh and now have 1 result as org2 was deleted", 1, TestingControl.OrganisationFilterControl.GridCollection.Count);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestingForm = new ZForm(new AdministrationPanelManager(Factory));
			TestingForm.Controls.Add(new DeduplicationOrganizationsUserControlForTest());
		}

		protected override void TearDown()
		{
			base.TearDown();
			TestingForm?.Dispose();
		}

		ZForm TestingForm;
		DeduplicationOrganizationsUserControlForTest TestingControl => TestingForm.Controls["DeduplicationOrganizationsUserControl"] as DeduplicationOrganizationsUserControlForTest;
		#endregion
	}

	public class DeduplicationOrganizationsUserControlForTest : DeduplicationOrganizationsUserControl
	{
		public ZFilterStripControl OrganisationFilterControl => Controls.Find("DeduplicationOrganisationFilterControl", true)[0] as ZFilterStripControl;

		public IEnumerable<ScoringResult> DeduplicationResultsForTest;

		public bool IsOverrideListManager_CurrentChanged;
		public bool IsDedupOrg_DuplicateSearchingFinished;

		public void RefreshRetainedRecordForTest() => RefreshRetainedRecord();

		public void PerformCurrentOrgDeduplicationActionOccurred(DeduplicationAction action)
		{
			CurrentOrgDeduplicationActionOccurred(null, new DuplicationEventArgs(null) { InvokedAction = action });
		}

		protected override void DedupOrg_DuplicateSearchingFinished(object sender, DuplicateSearchingFinishedEventArgs e)
		{
			var dedupOrg = sender as DeduplicationOrganisation;
			dedupOrg.SaveDeduplicationResults(DeduplicationResultsForTest);
			SetupDuplicationDetailView(dedupOrg);
			IsDedupOrg_DuplicateSearchingFinished = true;
		}

		protected override void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			if (IsOverrideListManager_CurrentChanged)
			{
				var dedupOrg = OrganisationFilterControl.Grid.ListManager.GetCurrent() as DeduplicationOrganisation;
				dedupOrg.DOH_Status = DeduplicationHelper.StatusConstants.Processed;
			}
			base.ListManager_CurrentChanged(sender, e);
		}
	}
}
