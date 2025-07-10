using System;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.eTail.GUI
{
	public class SyncUSISFMenuItem : BaseHVLVMenuItem
	{
		public SyncUSISFMenuItem(ForwardingShipment shipment)
			: base(ResString.GetMultilingualString("eb426194-51d0-43d6-b0fa-01fb5f565734", "Sync US Importer Security Filing"), shipment)
		{
		}

		protected override Action MenuAction => () =>
		{
		};

		public override void UpdateVisibilityAndCaption()
		{
			Visible = false;
		}
	}
}
