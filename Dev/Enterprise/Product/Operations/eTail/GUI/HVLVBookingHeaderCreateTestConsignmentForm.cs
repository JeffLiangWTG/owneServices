using System;
using System.Windows.Forms;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI
{
	public partial class HVLVBookingHeaderCreateTestConsignmentForm : ZChildForm
	{
		public HVLVBookingHeaderCreateTestConsignmentForm(string bookingHeaderReference, bool hasConsignments)
		{
			InitializeComponent();
			textBoxBookingHeaderReference.Text = bookingHeaderReference;
			calcEditConsignmentCount.CalcValue = 20000;
			textBoxWaybillPrefix.Text = "TestConsignment";
			if (hasConsignments)
			{
				checkBoxDeleteExistingConsignmentsDescrition.ForeColor = System.Drawing.Color.Black;
			}
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (ModifierKeys == Keys.None && keyData == Keys.Escape)
			{
				Close();
				return true;
			}

			return base.ProcessDialogKey(keyData);
		}

		void BtnOK_Click(object sender, EventArgs e)
		{
			var bookingHeaderReference = textBoxBookingHeaderReference.Text;
			var consignmentCount = Convert.ToInt32(calcEditConsignmentCount.CalcValue);
			var waybillPrefix = textBoxWaybillPrefix.Text;

			if (!string.IsNullOrEmpty(bookingHeaderReference) && !string.IsNullOrEmpty(waybillPrefix) && consignmentCount > 0)
			{
				try
				{
					if (checkBoxDeleteExistingConsignments.Checked)
					{
						HVLVBookingHeaderTestConsignmentsCreator.DeleteExistingConsignments(bookingHeaderReference);
					}

					HVLVBookingHeaderTestConsignmentsCreator.CreateConsignments(bookingHeaderReference, consignmentCount, waybillPrefix);
					DialogResult = DialogResult.OK;
					Close();
				}
				catch
				{
					Globals.Message.ShowError(Res.GetString("e8b28866-2156-44d5-839e-3862ea43ab8d", "Failed to run SQL script"));
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("afacf397-47b6-4dbb-83cc-cf828dc4ceeb", "Please enter a valid value for each field"));
			}
		}
	}
}
