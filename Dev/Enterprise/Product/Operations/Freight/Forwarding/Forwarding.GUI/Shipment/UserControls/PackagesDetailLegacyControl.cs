using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class PackagesDetailLegacyControl : ZUserControl
	{
		public PackagesDetailLegacyControl()
		{
			InitializeComponent();
		}

		public EventHandler<EventArgs> OnUpdatePackLineButtonClickHandler;

		void OnUpdatePackLineButtonClick(object sender, EventArgs e)
		{
			if (OnUpdatePackLineButtonClickHandler != null)
			{
				this.OnUpdatePackLineButtonClickHandler(sender, e);
			}
		}
	}
}
