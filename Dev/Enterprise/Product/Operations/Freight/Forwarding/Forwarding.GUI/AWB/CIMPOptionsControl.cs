using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class CIMPOptionsControl : ZUserControl
	{
		public CIMPOptionsControl()
		{
			InitializeComponent();
		}

		public void HideSentDateDetails()
		{
			SentDateLabel.Visible = false;
			zLabel2.Visible = false;
		}
	}
}
