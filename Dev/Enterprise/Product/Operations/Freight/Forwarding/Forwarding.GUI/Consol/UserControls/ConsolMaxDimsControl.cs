using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ConsolMaxDimsControl : ZUserControl
	{
		public ConsolMaxDimsControl()
		{
			InitializeComponent();

			MaxPackageHeightCalcEdit.AllowOverlap(HeightLabel);
			MaxPackageWidthCalcEdit.AllowOverlap(WidthLabel);
		}
	}
}
