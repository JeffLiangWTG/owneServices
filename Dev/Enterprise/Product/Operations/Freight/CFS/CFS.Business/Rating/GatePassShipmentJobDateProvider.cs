using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class GatePassShipmentJobDateProvider : JobDatesProvider<GatePassShipment>
	{
		public GatePassShipmentJobDateProvider(GatePassShipment gatePassShipment)
			: base(gatePassShipment) { }

		protected override ZDateTime GetArrivalDateCore()
		{
			return GetActualJobDate();
		}

		protected override ZDateTime GetDepartureDateCore()
		{
			return GetActualJobDate();
		}

		ZDateTime GetActualJobDate()
		{
			var deliveredDate = ZDateTime.Today;

			foreach (var leg in Parent.DestinationCFSDepartures)
			{
				if (!leg.EU_PickupDeliveryTime.IsEmpty && leg.EU_PickupDeliveryTime < deliveredDate)
				{
					deliveredDate = leg.EU_PickupDeliveryTime;
				}
			}

			return deliveredDate;
		}

		protected override ZDateTime GetFirstContainerGateInDateCore()
		{
			return GetEarliestValidDate(Parent.Containers.Cast<CommonContainer>().Select(c => c.JC_FCLWharfGateIn));
		}

		protected override ZDateTime GetLastContainerGateInDateCore()
		{
			return GetLatestValidDate(Parent.Containers.Cast<CommonContainer>().Select(c => c.JC_FCLWharfGateIn));
		}
	}
}
