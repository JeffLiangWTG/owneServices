using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class MAWBPrintOptionsControl : ZUserControl
	{
		public MAWBPrintOptionsControl()
		{
			InitializeComponent();
		}

		public void HidePrintedDateDetails()
		{
			PrintedDateTitleLabel.Visible = false;
			PrintedDateContentLabel.Visible = false;
		}
	}
}
