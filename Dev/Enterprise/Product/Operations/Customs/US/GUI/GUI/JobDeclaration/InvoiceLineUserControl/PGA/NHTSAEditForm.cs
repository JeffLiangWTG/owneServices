using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class NHTSAEditForm : ZChildForm
	{
		public NHTSAEditForm(NHTSAHeader header)
			: base(header)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			ModifyControlsVisibilityForProduct();
		}

		void OKButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		void ModifyControlsVisibilityForProduct()
		{
			var header = BusinessEntity as NHTSAHeader;
			if (header != null && header.Parent != null && header.Parent.GetType() == typeof(CusClassPartPivot))
			{
				ElecImageSubmittedCheckBox.Visible = false;
				TravelDocumentGroupBox.Visible = false;
				LeftTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 58, true);

				PGAContantNameTextBox.Visible = false;
				PGAContactPhoneTextBox.Visible = false;
				PGAContactEmailTextBox.Visible = false;
				PGAContactGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 40, true);
			}
		}
	}
}
