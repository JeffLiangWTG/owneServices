using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Freight.CFS.GUI
{
	public partial class ConsignorWizardPage : WizardPage
	{
		public ConsignorWizardPage()
		{
			InitializeComponent();
		}

		public override CoLoadWizardSteps WizardPageStep
		{
			get
			{
				return Enterprise.Freight.CFS.Business.CoLoadWizardSteps.ConsignorDetails;
			}
		}

		public override Control DefaultFocusedControl
		{
			get
			{
				return CW_TempOrgNameTextBox1;
			}
		}

		#region Implementation

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		private void SimilarOrgMatchesBoundGrid_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				DataGrid.HitTestInfo hitTestInfo = SimilarOrgMatchesBoundGrid.HitTest(SimilarOrgMatchesBoundGrid.PointToClient(Cursor.Position));
				if (SimilarOrgMatchesBoundGrid.CurrentRowIndex >= 0 && hitTestInfo.Type == DataGrid.HitTestType.Cell)
				{
					OrgPatternMatch foundMatch = ((CoLoadWizardShipment)SimilarOrgMatchesBoundGrid.DataSource).SimilarOrganisations[SimilarOrgMatchesBoundGrid.CurrentRowIndex];
					CoLoadWizardForm parentZForm = (CoLoadWizardForm)ParentForm;
					CoLoadWizardShipment parentBusinessObject = (CoLoadWizardShipment)parentZForm.BusinessEntity;
					parentBusinessObject.CW_OH_Consignor = foundMatch.OS_OH;
					parentZForm.CurrentWizardPageIndex++;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException()) { }
		}

		#endregion

		void ClearButton_Click(object sender, EventArgs e)
		{
			CW_TempOrgNameTextBox1.Text = "";
			CW_TempOrgAdress1TextBox2.Text = "";
			CW_TempOrgAdress2TextBox3.Text = "";
			CW_TempOrgPostCodeTextBox.Text = "";
			CW_TempOrgCityTextBox.Text = "";
		}
	}
}
