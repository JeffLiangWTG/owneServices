using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class PackagesDetailControl : ZUserControl
	{
		public PackagesDetailControl()
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
		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			if (DataSource is CommonShipment shipment)
			{
				shipment.OuterPackLines.Cast<ForwardingPackLine>().ForEach(p => p.Validation.ValidateJL_PackLineId());
			}
		}
	}
}
