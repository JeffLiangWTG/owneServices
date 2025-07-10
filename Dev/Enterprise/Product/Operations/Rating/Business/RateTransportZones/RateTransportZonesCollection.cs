using CargoWise.EntityFramework;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Business
{
	[ModuleID(ModuleId.RateTransportZone)]
	public class RateTransportZonesCollection : ActiveBusinessObjectCollection<RateTransportZone>, IRateTransportZonesCollection
	{
		public RateTransportZonesCollection(RateTransportProvider provider)
			: base(provider)
		{
		}

		public RateTransportZonesCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}

