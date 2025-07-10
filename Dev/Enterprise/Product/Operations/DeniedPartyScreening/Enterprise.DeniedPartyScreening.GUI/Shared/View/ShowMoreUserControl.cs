using System.Drawing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public partial class ShowMoreUserControl : ZUserControl
	{
		public ShowMoreUserControl()
		{
			InitializeComponent();

			var collapseDrawingGroup = Properties.Resources.collapseDrawingGroup;
			collapseDrawingGroup.RotateFlip(RotateFlipType.Rotate180FlipNone);
			SwitchBarPictureBox.Image = collapseDrawingGroup;
		}
	}
}
