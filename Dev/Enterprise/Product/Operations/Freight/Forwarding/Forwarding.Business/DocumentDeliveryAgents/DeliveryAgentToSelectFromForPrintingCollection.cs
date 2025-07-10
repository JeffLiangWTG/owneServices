using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business
{
	public class DeliveryAgentToSelectFromForPrintingCollection : BusinessObjectCollection<DeliveryAgentToSelectFromForPrinting>
	{
		public DeliveryAgentToSelectFromForPrintingCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(DeliveryAgentToSelectFromForPrinting);
		}
	}
}
