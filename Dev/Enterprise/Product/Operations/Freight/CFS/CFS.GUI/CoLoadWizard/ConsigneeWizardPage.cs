using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class ConsigneeWizardPage : WizardPage
	{
		public ConsigneeWizardPage()
		{
			InitializeComponent();
		}

		public override CoLoadWizardSteps WizardPageStep
		{
			get
			{
				return Enterprise.Freight.CFS.Business.CoLoadWizardSteps.ConsigneeDetails;
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

		void SimilarOrgMatchesBoundGrid_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				var hitTestInfo = SimilarOrgMatchesBoundGrid.HitTest(SimilarOrgMatchesBoundGrid.PointToClient(Cursor.Position));
				if (SimilarOrgMatchesBoundGrid.CurrentRowIndex >= 0 && hitTestInfo.Type == DataGrid.HitTestType.Cell)
				{
					var foundMatch =
						((CoLoadWizardShipment)SimilarOrgMatchesBoundGrid.DataSource).SimilarOrganisations[
							SimilarOrgMatchesBoundGrid.CurrentRowIndex];
					var parentZForm = (CoLoadWizardForm)ParentForm;
					var parentBusinessObject = (CoLoadWizardShipment)parentZForm.BusinessEntity;
					parentBusinessObject.CW_OH_Consignee = foundMatch.OS_OH;
					parentZForm.CurrentWizardPageIndex++;
				}
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}
			}
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

