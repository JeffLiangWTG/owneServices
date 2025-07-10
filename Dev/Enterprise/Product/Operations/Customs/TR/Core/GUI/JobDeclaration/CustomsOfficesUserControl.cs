using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public partial class CustomsOfficesUserControl : ZUserControl
	{
		public CustomsOfficesUserControl()
		{
			InitializeComponent();
			OfficesGroupBox.AllowOutsideOfParent();
		}
	}
}
