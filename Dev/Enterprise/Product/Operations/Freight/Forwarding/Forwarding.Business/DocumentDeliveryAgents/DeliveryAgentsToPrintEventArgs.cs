using System;

namespace Enterprise.Freight.Forwarding.Business
{
	public class DeliveryAgentsToPrintEventArgs : EventArgs
	{
		public DeliveryAgentsToPrintEventArgs(DeliveryAgentToSelectFromForPrintingCollection deliveryAgentsToSelectFrom)
		{
			if (deliveryAgentsToSelectFrom == null)
			{
				throw new ArgumentException("No Delivery Agents To Select From");
			}

			this.DeliveryAgentsToSelectFrom = deliveryAgentsToSelectFrom;
			CancelToPrint = false;
		}

		public DeliveryAgentToPrintCollection DeliveryAgentsToPrint
		{
			get
			{
				if (fDeliveryAgentsToPrint == null)
				{
					fDeliveryAgentsToPrint = new DeliveryAgentToPrintCollection(DeliveryAgentsToSelectFrom.Factory);
				}
				return fDeliveryAgentsToPrint;
			}
		}
		public readonly DeliveryAgentToSelectFromForPrintingCollection DeliveryAgentsToSelectFrom;
		protected DeliveryAgentToPrintCollection fDeliveryAgentsToPrint;
		public bool CancelToPrint;
	}
}
