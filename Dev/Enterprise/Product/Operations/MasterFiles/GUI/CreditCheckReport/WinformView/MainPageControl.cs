using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class MainPageControl : ZUserControl
	{
		public MainPageControl()
		{
			InitializeComponent();
			eventsBannerControl.AllowOutsideOfParent();
		}
	}
}
