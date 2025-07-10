using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.eTail.GUI
{
	public class USISFMenuGroup : BaseHVLVMenuGroup
	{
		public USISFMenuGroup(ForwardingShipment shipment)
			: base(ResString.GetMultilingualString("cf77198b-664e-46b3-bffc-7b7d79ba7eb6", "US Importer Security Filing"), shipment)
		{
		}

		protected override void BuildChildrenMenuItems()
		{
			createUSISFMenuItem = new CreateUSISFMenuItem(shipment);
			openUSISFMenuItem = new OpenUSISFMenuItem(shipment);
			syncUSISFMenuItem = new SyncUSISFMenuItem(shipment);

			MenuItems.Add(createUSISFMenuItem);
			MenuItems.Add(openUSISFMenuItem);
			MenuItems.Add(syncUSISFMenuItem);
		}

		public override void UpdateVisibilityAndCaption()
		{
			base.UpdateVisibilityAndCaption();
			Visible = createUSISFMenuItem.Visible || openUSISFMenuItem.Visible || syncUSISFMenuItem.Visible;
		}

		CreateUSISFMenuItem createUSISFMenuItem;
		OpenUSISFMenuItem openUSISFMenuItem;
		SyncUSISFMenuItem syncUSISFMenuItem;
	}
}
