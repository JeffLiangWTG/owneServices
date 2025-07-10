using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class BillOfLadingContainersSubGridSection : ZUserControl
	{
		public BillOfLadingContainersSubGridSection()
		{
			InitializeComponent();

			releaseNumberTextBox.CaptionResourceString = CommonContainer.ImportReleaseNumberStringData;
		}
	}
}
