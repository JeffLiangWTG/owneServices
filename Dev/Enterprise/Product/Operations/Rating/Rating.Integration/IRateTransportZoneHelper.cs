using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Rating.Integration
{
	public interface IRateTransportZoneHelper
	{
		string GetZoneName(BusinessObjectFactory factory, IOrgHeader owner, object location, string countryCode, string postCode, string citySuburb);
		bool IsBeyond(BusinessObjectFactory factory, IOrgHeader owner, object location, string countryCode, string postCode, string citySuburb);
	}
}

#region Test
#if DEBUG

namespace Enterprise.Rating.Integration.Testing
{
	public interface IRateTransportZoneTestHelper
	{
		void AddTestZone(string zoneName, string postcodeFrom, string postcodeTo, string postcodeCitySuburb);
		void CreateRateTransportZones(object factory, IOrgHeader transportProvider);
	}
}

#endif
#endregion
