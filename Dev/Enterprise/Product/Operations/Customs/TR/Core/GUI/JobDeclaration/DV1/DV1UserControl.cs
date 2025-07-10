using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public partial class DV1UserControl : EU.GUI.DV1UserControl
	{
		public DV1UserControl()
		{
			InitializeComponent();
		}

		protected override Type GetGridUserControl() => typeof(DV1GridUserControl);

		public override IPanelLayoutProvider GetDv1DetailsLayout() => new DV1DetailsLayout();
	}
}
