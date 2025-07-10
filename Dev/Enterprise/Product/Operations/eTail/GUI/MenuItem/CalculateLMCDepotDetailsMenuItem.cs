using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.eTail.GUI
{
	public class CalculateLMCDepotDetailsMenuItem : BaseHVLVMenuItem
	{
		public CalculateLMCDepotDetailsMenuItem(ForwardingShipment shipment)
			: base(ResString.GetMultilingualString("56d9710a-5aae-416a-8749-f5272825c2d8", "Calculate Depot and Last Mile Carrier Details"), shipment)
		{
		}

		protected override Action MenuAction => () =>
		{
			LMCDepotDetailsCalculator.UpdateConsignmentsDestinationDetails(Header.ConsignmentsForBinding.Select(n => n.PK));
			Header.ConsignmentsForBinding.ForEach(consignment =>
			{
				consignment.Reload();
				consignment.RefreshBindingForLMCProperties();
			});
		};

		HVLVConsignmentLastMileCarrierAndDepotDetailsCalculator LMCDepotDetailsCalculator => lmcDepotDetailsCalculator ?? (lmcDepotDetailsCalculator = new HVLVConsignmentLastMileCarrierAndDepotDetailsCalculator());
		HVLVConsignmentLastMileCarrierAndDepotDetailsCalculator lmcDepotDetailsCalculator;
	}
}
