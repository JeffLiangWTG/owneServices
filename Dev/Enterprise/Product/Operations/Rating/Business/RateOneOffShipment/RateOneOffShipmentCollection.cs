using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public class RateOneOffShipmentCollection : DependentBusinessObjectCollection<RateOneOffShipment, Quote>
	{
		public RateOneOffShipmentCollection(Quote parentQuote)
			: base(parentQuote)
		{
		}
	}
}

