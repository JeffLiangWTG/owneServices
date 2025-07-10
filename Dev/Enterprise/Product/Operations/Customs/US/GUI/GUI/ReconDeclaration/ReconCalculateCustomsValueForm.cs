using System;
using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class ReconCalculateCustomsValueForm : ZChildForm
	{
		public ReconCalculateCustomsValueForm(ReconCustomsValueCalculationManager calculator)
			: base(calculator)
		{
			this.calculator = calculator;
			InitializeComponent();
		}

		readonly ReconCustomsValueCalculationManager calculator;

		void OKButton_Click(object sender, EventArgs e)
		{
			calculator.ValidateAll();

			if (calculator.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				var confirmMessage = "All Reconciled Values are about to be changed.\r\n" + calculator.RecalculationDesc + " " + calculator.RecalculationPercentage + "% from Original Customs Values. Are you sure?";
				if (Globals.Message.Show(confirmMessage, "Send messages", MessageBoxButtons.OKCancel, DialogResult.Cancel) == DialogResult.OK)
				{
					this.DialogResult = DialogResult.OK;
					Dispose();
					calculator.CalculateCustomsValuesForAllEntryLines();

					var resultMessage = "Calculation finished. Please check 'Rec Customs Value' for each original entry line.";
					Globals.Message.Show(resultMessage, "Customs Values Calculation", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
