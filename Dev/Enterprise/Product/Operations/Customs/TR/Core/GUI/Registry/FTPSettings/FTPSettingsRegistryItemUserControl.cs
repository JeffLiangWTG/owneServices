using System;
using Enterprise.Registry.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public partial class FTPSettingsRegistryItemUserControl : RegistryZUserControl
	{
		public FTPSettingsRegistryItemUserControl()
		{
			InitializeComponent();
			FTPPortCalcEdit.KeyPress += new System.Windows.Forms.KeyPressEventHandler(FTPPortCalcEdit_KeyPress);
			FTPPortCalcEdit.TextChanged += new EventHandler(ZCalc_EditValueChanged);
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			FTPAddressDropEdit.ReadOnly = readOnly;
			FTPPortCalcEdit.ReadOnly = readOnly;
			FTPInTextBox.ReadOnly = readOnly;
			FTPOutTextBox.ReadOnly = readOnly;
			ExportUnionUserCodeTextBox.ReadOnly = readOnly;
			ExportUnionUserPasswordTextBox.ReadOnly = readOnly;
			ExportUnionPaymentPasswordTextBox.ReadOnly = readOnly;
		}

		public void FTPPortCalcEdit_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
			{
				e.Handled = true;
			}
		}

		public void ZCalc_EditValueChanged(object sender, EventArgs e)
		{
			FTPPortCalcEdit.Text = FTPPortCalcEdit.Text.Replace(".", "").Replace(",", "");

			if (int.TryParse(FTPPortCalcEdit.Text, out int portValue))
			{
				if (portValue > 65535)
				{
					FTPPortCalcEdit.Text = "65535";
				}
				else if (portValue < 0)
				{
					FTPPortCalcEdit.Text = "0";
				}
			}
		}
	}
}
