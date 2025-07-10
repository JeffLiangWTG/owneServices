using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(SimilarOrganizationSelectionForm))]
	sealed class SimilarOrganizationSelectionFormTest : ZFormBasherTest
	{
		#region Public/Internal Instance Methods
		[RequiresSTA]
		public void TestOrganizationSelected()
		{
			SimilarOrganizationSelectionForm testForm;
			using (testForm = (SimilarOrganizationSelectionForm)GetFormToBash())
			{
				testForm.Show();
				testForm.SaveNewOrgButton.PerformClick();
				AssertNull(testForm.FirstSelectedOrganization);
			}
			using (testForm = (SimilarOrganizationSelectionForm)GetFormToBash())
			{
				testForm.Show();
				testForm.CancelSaveButton.PerformClick();
				AssertNull(testForm.FirstSelectedOrganization);
			}
			using (testForm = (SimilarOrganizationSelectionForm)GetFormToBash())
			{
				testForm.Show();
				testForm.Close();
				AssertNull(testForm.FirstSelectedOrganization);
			}
		}

		[RequiresSTA]
		public void TestNewOrganizationToBeCreated()
		{
			using (SimilarOrganizationSelectionForm testForm = (SimilarOrganizationSelectionForm)GetFormToBash())
			{
				testForm.Show();
				testForm.newOrganizationButton.PerformClick();
				Assert(testForm.ToCreateNew);
			}
		}
		#endregion

		#region Private/Protected Members
		protected override Form GetFormToBashCore()
		{
			return new SimilarOrganizationSelectionForm(Factory.New<OrgHeader>());
		}
		#endregion
	}
}
