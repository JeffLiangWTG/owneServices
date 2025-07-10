using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ShipmentBasicRegistrationControl : ZUserControl
	{
		public ShipmentBasicRegistrationControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var shipment = (ForwardingShipment)CurrentDataItem;

			if (shipment != null)
			{
				JobHandler = new LocalClientJobHandler(shipment);
				JobHandler.Initialize();
				ModeAndParty.JobHandler = JobHandler;
			}
		}

		internal LocalClientJobHandler JobHandler;

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (JobHandler != null)
				{
					JobHandler.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}
	}
}
