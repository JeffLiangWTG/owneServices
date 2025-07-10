using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class TopLevelPacksImportProcessControl : ZUserControl
	{
		public TopLevelPacksImportProcessControl()
		{
			InitializeComponent();

			ContainerImportDOReleaseTextBox.CaptionResourceString = CommonContainer.ImportReleaseNumberStringData;
		}
	}
}
