using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.Business
{
	/// <summary>
	/// Plain collection of Delivery Agents to Print.
	/// </summary>
	public class DeliveryAgentToPrintCollection : BusinessObjectCollection<DeliveryAgentOrgHeader>
	{
		public DeliveryAgentToPrintCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public DeliveryAgentToPrintCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}
