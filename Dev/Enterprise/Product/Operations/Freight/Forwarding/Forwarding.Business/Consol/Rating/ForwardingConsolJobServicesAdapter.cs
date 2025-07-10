using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingConsolJobServicesAdapter : ForwardingConsolRatingAdapter, IManualRateSelectionSupporter
	{
		public ForwardingConsolJobServicesAdapter(IRatingRoute<IRoutingSupport> ratingRoute) : base(ratingRoute) { }

		bool IManualRateSelectionSupporter.SupportsManualRateSelection => false;

		public override ZBool IsServicesOnly => true;

		public override bool StandardFreightCostEnabled => false;
	}
}
