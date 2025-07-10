using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.US.GUI
{
	public partial class ExportClassificationUserControl : Customs.GUI.BaseClassificationUserControl
	{
		public ExportClassificationUserControl()
		{
			InitializeComponent();
			MissingResourceStringChecker.ExcludeFromTest(cC_ScheduleBBoundFindBox);
		}
	}
}
