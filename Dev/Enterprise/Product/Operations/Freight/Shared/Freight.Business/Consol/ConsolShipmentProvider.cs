using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class ConsolShipmentProvider
	{
		public ConsolShipmentProvider(BusinessObject potentialConsol)
		{
			if (potentialConsol is CommonConsol consol)
			{
				Consol = consol;
			}
		}

		public ConsolShipmentCollection Shipments
		{
			get
			{
				return Consol?.Shipments;
			}
		}

		CommonConsol Consol { get; }
	}
}
