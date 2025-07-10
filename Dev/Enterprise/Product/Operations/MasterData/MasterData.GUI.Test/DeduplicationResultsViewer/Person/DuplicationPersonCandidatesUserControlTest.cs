using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI.Test
{
	public class DuplicationPersonCandidatesUserControlTest : TestCaseWithFactory
	{
		public void TestInvisibleForEmptyResult()
		{
			var resultDetail = new DeduplicationPersonResultDetailForTest();
			resultDetail.SetIsEmptyOrNotForTesting(isEmpty: true);
			using (var form = new ZForm())
			{
				var control = new DuplicationPersonCandidatesUserControlForTest(isAdminPanel: false);
				form.Controls.Add(control);
				form.Show();

				control.SetupDataContext(resultDetail);

				AssertEquals(false, control.DuplicationCandidatesGrid.Visible);
			}
		}

		public void TestColumns()
		{
			var resultDetail = new DeduplicationPersonResultDetailForTest();
			resultDetail.SetIsEmptyOrNotForTesting(isEmpty: false);
			using (var form = new ZForm())
			{
				var control = new DuplicationPersonCandidatesUserControlForTest(isAdminPanel: false);
				form.Controls.Add(control);
				form.Show();

				control.SetupDataContext(resultDetail);

				var columns = control.DuplicationCandidatesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName);
				Assert(control.DuplicationCandidatesGrid.Visible);
				AssertContainsExactElementsInExactOrder(new[]
				{
					"ConfidenceDescription",
					"ConfidenceScore",
					"Name",
					"Type",
					"Phone",
					"Title",
					"Active",
					"RelatedTo",
					"IsDissolved"
				}, columns);
			}
		}

		public void TestColumnsInAdminPanel()
		{
			var resultDetail = new DeduplicationPersonResultDetailForTest();
			resultDetail.SetIsEmptyOrNotForTesting(isEmpty: false);
			using (var form = new ZForm())
			{
				var control = new DuplicationPersonCandidatesUserControlForTest(isAdminPanel: true);
				form.Controls.Add(control);
				form.Show();

				control.SetupDataContext(resultDetail);

				var columns = control.DuplicationCandidatesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName);
				Assert(control.DuplicationCandidatesGrid.Visible);
				AssertContainsExactElementsInExactOrder(new[]
				{
					"ConfidenceDescription",
					"ConfidenceScore",
					"Status",
					"Name",
					"Type",
					"Phone",
					"Title",
					"Active",
					"RelatedTo",
					"IsDissolved",
					"IgnoredForEveryoneStaff",
					"IgnoredByStaff"
				}, columns);
			}
		}

		public void TestUpdateSelectedCandidate()
		{
			var candidate1PK = Guid.NewGuid();
			var candidate2PK = Guid.NewGuid();
			var candidate1 = CreateNewCandidate();
			candidate1.TargetPK = candidate1PK;
			var candidate2 = CreateNewCandidate();
			candidate2.TargetPK = candidate2PK;

			var resultDetail = new DeduplicationPersonResultDetailForTest();
			resultDetail.SetIsEmptyOrNotForTesting(isEmpty: false);
			resultDetail.DuplicationCandidatesForTest = new[] { candidate1, candidate2 };
			resultDetail.LoadCandidatesAndUpdateSelectedItem();

			using (var form = new ZForm())
			{
				var control = new DuplicationPersonCandidatesUserControlForTest(isAdminPanel: true);
				form.Controls.Add(control);
				form.Show();

				control.SetupDataContext(resultDetail);
				AssertEquals(candidate1PK, resultDetail.SelectedCandidatePK);
				control.DuplicationCandidatesGrid.CurrentRowIndex = 1;
				AssertEquals(candidate2PK, resultDetail.SelectedCandidatePK);
				control.DuplicationCandidatesGrid.CurrentRowIndex = 0;
				AssertEquals(candidate1PK, resultDetail.SelectedCandidatePK);
			}
		}

		DuplicationPersonCandidate CreateNewCandidate() => (DuplicationPersonCandidate)Activator.CreateInstance(typeof(DuplicationPersonCandidate), nonPublic: true);
	}

	class DuplicationPersonCandidatesUserControlForTest : DuplicationPersonCandidatesUserControl
	{
		public DuplicationPersonCandidatesUserControlForTest(bool isAdminPanel)
			: base(isAdminPanel)
		{
		}

		public new ZGrid DuplicationCandidatesGrid => base.DuplicationCandidatesGrid;
	}
}
