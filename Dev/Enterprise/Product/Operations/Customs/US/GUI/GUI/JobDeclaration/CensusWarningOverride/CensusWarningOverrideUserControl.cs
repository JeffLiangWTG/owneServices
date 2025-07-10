using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class CensusWarningOverrideUserControl : ZUserControl
	{
		public CensusWarningOverrideUserControl()
		{
			InitializeComponent();
		}

		public void SetAdditionalGridLayoutKey(string key)
		{
			CWOsGrid.LayoutKey += key;
		}
	}
}
