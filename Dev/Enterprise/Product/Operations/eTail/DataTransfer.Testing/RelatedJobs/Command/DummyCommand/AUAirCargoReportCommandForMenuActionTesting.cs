using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class AUAirCargoReportCommandForMenuActionTesting : AUAirCargoReportCommand
	{
		public AUAirCargoReportCommandForMenuActionTesting(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		public override MultilingualString RelatedJobName => (NoResString)(base.RelatedJobName + "(Testing)");
	}
}
