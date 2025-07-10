using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterData.GUI.Test
{
	public class DuplicationOrganisationCandidatesUserControlTest : TestCaseWithFactory
	{
		public void TestInvisibleForEmptyResult()
		{
			var resultDetail = new DeduplicationOrganisationResultDetailForTest();
			resultDetail.SetIsEmptyOrNotForTesting(isEmpty: true);
			using (var form = new ZForm())
			{
				var control = new DuplicationOrganisationCandidatesUserControlForTest(isAdminPanel: false);
				form.Controls.Add(control);
				form.Show();

				control.SetupDataContext(resultDetail);

				AssertEquals(false, control.DuplicationCandidatesGrid.Visible);
			}
		}

		public void TestColumns()
		{
			var resultDetail = new DeduplicationOrganisationResultDetailForTest();
			resultDetail.SetIsEmptyOrNotForTesting(false);
			using (var form = new ZForm())
			{
				var control = new DuplicationOrganisationCandidatesUserControlForTest(isAdminPanel: false);
				form.Controls.Add(control);
				form.Show();

				control.SetupDataContext(resultDetail);

				var columns = control.DuplicationCandidatesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName);
				Assert(control.DuplicationCandidatesGrid.Visible);
				AssertContainsExactElementsInExactOrder(new[]
				{
					"ConfidenceDescription",
					"ConfidenceScore",
					"Code",
					"Name",
					"UNLOCO",
					"Type",
					"DebtorCompany",
					"CreditorCompany",
					"Active"
				}, columns);
			}
		}

		public void TestColumnsInAdminPanel()
		{
			var resultDetail = new DeduplicationOrganisationResultDetailForTest();
			resultDetail.SetIsEmptyOrNotForTesting(false);
			using (var form = new ZForm())
			{
				var control = new DuplicationOrganisationCandidatesUserControlForTest(isAdminPanel: true);
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
					"Code",
					"Name",
					"UNLOCO",
					"Type",
					"DebtorCompany",
					"CreditorCompany",
					"Active",
					"IgnoredForEveryoneStaff",
					"IgnoredByStaff",
					"AssociatedShipmentsCount",
					"AssociatedConsolsCount",
					"AssociatedDeclarationsCount"
				}, columns);
			}
		}

		public void TestEdiColumnsInAdminPanel()
		{
			var resultDetail = new DeduplicationOrganisationResultDetailForTest();
			resultDetail.SetIsEmptyOrNotForTesting(false);
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			using (var form = new ZForm())
			{
				var control = new DuplicationOrganisationCandidatesUserControlForTest(isAdminPanel: true);
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
					"Code",
					"Name",
					"UNLOCO",
					"Type",
					"DebtorCompany",
					"CreditorCompany",
					"Active",
					"IgnoredForEveryoneStaff",
					"IgnoredByStaff",
					"AssociatedShipmentsCount",
					"AssociatedConsolsCount",
					"AssociatedDeclarationsCount",
					"EnterpriseId",
					"EnterpriseCode",
					"CompanyCode",
					"ProductId"
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

			var resultDetail = new DeduplicationOrganisationResultDetailForTest();
			resultDetail.SetIsEmptyOrNotForTesting(false);
			resultDetail.DuplicationCandidatesForTest = new[] { candidate1, candidate2 };
			resultDetail.LoadCandidatesAndUpdateSelectedItem();

			using (var form = new ZForm())
			{
				var control = new DuplicationOrganisationCandidatesUserControlForTest(isAdminPanel: true);
				form.Controls.Add(control);
				form.Show();

				control.SetupDataContext(resultDetail);
				AssertEquals(candidate1PK, resultDetail.SelectedCandidatePK);
				control.DuplicationCandidatesGrid.SelectSingleElement(resultDetail.DuplicationCandidates[1]);
				AssertEquals(candidate2PK, resultDetail.SelectedCandidatePK);
				control.DuplicationCandidatesGrid.SelectSingleElement(resultDetail.DuplicationCandidates[0]);
				AssertEquals(candidate1PK, resultDetail.SelectedCandidatePK);
			}
		}

		DuplicationOrganisationCandidate CreateNewCandidate() => (DuplicationOrganisationCandidate)Activator.CreateInstance(typeof(DuplicationOrganisationCandidate), nonPublic: true);
	}

	class DuplicationOrganisationCandidatesUserControlForTest : DuplicationOrganisationCandidatesUserControl
	{
		public DuplicationOrganisationCandidatesUserControlForTest(bool isAdminPanel)
			: base(isAdminPanel)
		{
		}

		public new ZGrid DuplicationCandidatesGrid => base.DuplicationCandidatesGrid;
	}
}
