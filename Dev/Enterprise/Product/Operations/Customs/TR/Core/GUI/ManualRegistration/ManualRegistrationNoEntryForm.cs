using System;
using System.Windows.Forms;
using Enterprise.Customs.TR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public partial class ManualRegistrationNoEntryForm : ZChildForm
	{
		public ManualRegistrationNoEntryForm(ManualRegistrationNoEntry manualRegistrationNoEntry)
			: base(manualRegistrationNoEntry)
		{
			InitializeComponent();
			this.manualRegistrationNoEntry = manualRegistrationNoEntry;
		}

		readonly ManualRegistrationNoEntry manualRegistrationNoEntry;

		void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Baseline")]
		void btnOk_Click(object sender, EventArgs e)
		{
			if (manualRegistrationNoEntry != null)
			{
				var regDate = manualRegistrationNoEntry.RegistrationDate;
				if (!regDate.IsEmpty && !regDate.IsValid)
				{
					MessageBox.Show(
						Res.GetString("571EADF0-5FC7-4879-8C59-34788609FFEA", "Registration Date is invalid, please correct it."),
						Res.GetString("E36F9FF9-EEAC-4363-B1B1-6176CD1F800D", "Manual Registration No Entry"),
						MessageBoxButtons.OK,
						MessageBoxIcon.Error
						);
					return;
				}
			}
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
