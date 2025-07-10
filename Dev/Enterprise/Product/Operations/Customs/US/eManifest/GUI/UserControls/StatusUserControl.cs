using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.US.eManifest.GUI
{
	public partial class StatusUserControl : ZUserControl
	{
		public StatusUserControl()
		{
			InitializeComponent();
			MissingResourceStringChecker.ExcludeFromTest(this.StatusGroupBox);
		}
	}
}
