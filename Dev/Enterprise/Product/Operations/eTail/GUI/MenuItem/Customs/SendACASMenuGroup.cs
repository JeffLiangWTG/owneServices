using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.eTail.GUI
{
	public class SendACASMenuGroup : BaseHVLVMenuGroup
	{
		public SendACASMenuGroup(ForwardingShipment shipment)
			: base(ResString.GetMultilingualString("9b8777f3-5ed9-4f40-bfaa-275486f67a1f", "ACAS"), shipment)
		{
		}

		protected override void BuildChildrenMenuItems()
		{
			reportMenuItem = new SendACASReportMenuItem(shipment);
			amendmentMenuItem = new SendACASAmendmentMenuItem(shipment);
			acknowledgementMenuItem = new SendACASAcknowledgementMenuItem(shipment);

			MenuItems.Add(reportMenuItem);
			MenuItems.Add(amendmentMenuItem);
			MenuItems.Add(acknowledgementMenuItem);
		}

		public override void UpdateVisibilityAndCaption()
		{
			base.UpdateVisibilityAndCaption();
			Visible = reportMenuItem.Visible || amendmentMenuItem.Visible || acknowledgementMenuItem.Visible;
		}

		SendACASReportMenuItem reportMenuItem;
		SendACASAmendmentMenuItem amendmentMenuItem;
		SendACASAcknowledgementMenuItem acknowledgementMenuItem;
	}
}
