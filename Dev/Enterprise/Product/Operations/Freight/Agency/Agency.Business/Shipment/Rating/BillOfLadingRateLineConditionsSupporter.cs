using System.Linq;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business.Shipment.Rating
{
	public class BillOfLadingRateLineConditionsSupporter : ShipmentRateLineConditionsSupporter
	{
		public BillOfLadingRateLineConditionsSupporter(BillOfLading objectToWrap) : base(objectToWrap) { }

		protected BillOfLading BillOfLading => ObjectToWrap as BillOfLading;

		protected override bool GetHasDangerousGoods() => BillOfLading.IsTopLevelPacksMode ? BillOfLading.TopLevelPacks.Cast<AgencyShipmentContainer>().Any(p => p.UNDGs.Any()) : BillOfLading.OuterPackLines.Cast<PackLine>().Any(p => p.UNDGs.Any());
	}
}
