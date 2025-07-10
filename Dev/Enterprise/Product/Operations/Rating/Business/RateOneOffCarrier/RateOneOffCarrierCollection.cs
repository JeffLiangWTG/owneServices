using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public class RateOneOffCarrierCollection : DependentBusinessObjectCollection<RateOneOffCarrier, RateOneOffShipment>
	{
		public RateOneOffCarrierCollection(RateOneOffShipment master)
			: base(master)
		{
		}
	}
}
