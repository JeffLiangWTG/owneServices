using System;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class EPAEditForm : ZChildForm
	{
		public EPAEditForm()
		{
		}

		public EPAEditForm(Vehicle vehicle)
			: base(vehicle)
		{
		}

		public void ChangeVisibilityOfControlsForProduct()
		{
			ElecImageSubmittedCheckBox.Visible = false;
			US_ContactNameTextBox.Visible = false;
			US_ContactEmailTextBox.Visible = false;
			US_ContactPhoneNoTextBox.Visible = false;
			this.VehicleGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(588, 585, true);
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(588, 644, true);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void BtnOK_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
