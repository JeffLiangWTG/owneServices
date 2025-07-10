using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business
{
	public class DocumentDeliveryAgents : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DocumentDeliveryAgents(DeliveryAgentToSelectFromForPrintingCollection deliveryAgentsToSelectFromForPrinting) : base(deliveryAgentsToSelectFromForPrinting.Factory)
		{
			fDeliveryAgentsToSelectFrom = deliveryAgentsToSelectFromForPrinting;
		}

		public DeliveryAgentToSelectFromForPrintingCollection DeliveryAgentsToSelectFrom
		{
			get { return fDeliveryAgentsToSelectFrom; }
		}

		public DeliveryAgentToPrintCollection DeliveryAgentsToPrint
		{
			get
			{
				DeliveryAgentToPrintCollection result = new DeliveryAgentToPrintCollection(Factory);
				foreach (DeliveryAgentToSelectFromForPrinting currentDeliveryAgent in DeliveryAgentsToSelectFrom)
				{
					if (currentDeliveryAgent.OH_Calc_PrintDocumentForDeliveryAgent == ZBool.True)
					{
						var deliveryAgentToPrint = Factory.Load<DeliveryAgentOrgHeader>(currentDeliveryAgent.PK);
						result.Add(deliveryAgentToPrint);
					}
				}
				return result;
			}
		}
		protected DeliveryAgentToSelectFromForPrintingCollection fDeliveryAgentsToSelectFrom;
	}
}
