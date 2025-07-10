using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class PSTEditForm : ZChildForm
	{
		public PSTEditForm()
		{
		}

		public PSTEditForm(Pesticide header)
			: base(header)
		{
		}

		public void ChangeVisibilityOfControlsForProduct()
		{
			this.IsPSTLabelsSentCheckBox.Visible = false;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			UnitsGroupBox.AllowOutsideOfParent();
		}

		void OKButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
