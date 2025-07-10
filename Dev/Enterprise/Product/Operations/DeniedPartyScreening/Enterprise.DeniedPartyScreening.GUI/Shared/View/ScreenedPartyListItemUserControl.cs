using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public partial class ScreenedPartyListItemUserControl : ZUserControl
	{
		public ScreenedPartyListItemUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource is ScreenedPartyWinModel screenedPartyWinModel)
			{
				base.SetDataBinding(dataSource, dataMember);
				EntityIconPicture.Image = screenedPartyWinModel.EntityTypeIcon.ToBitmap();
			}
		}
	}
}
