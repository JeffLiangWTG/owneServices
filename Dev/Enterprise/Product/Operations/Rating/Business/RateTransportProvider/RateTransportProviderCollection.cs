using CargoWise.EntityFramework;
using Enterprise.Rating.Integration;

namespace Enterprise.Rating.Business
{
	public class RateTransportProviderCollection : BusinessObjectCollection<RateTransportProvider>, IRateTransportProviderCollection
	{
		public RateTransportProviderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new RateTransportProviderFindBoxListProvider(this); }
		}
	}
}

