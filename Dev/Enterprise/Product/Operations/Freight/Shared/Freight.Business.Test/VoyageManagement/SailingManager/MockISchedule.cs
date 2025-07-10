using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business.Testing
{
	sealed class MockISchedule : MockISailing
	{
		public MockISchedule(BusinessObjectFactory factory)
			: base(factory)
		{
			TransportMode = Core.Constants.TransportModes.Air;
		}

		public ZString AircraftRego
		{
			get { return Vessel; }
			set { Vessel = value; }
		}

		public ZString Flight
		{
			get { return Voyage; }
			set { Voyage = value; }
		}
	}
}
