using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;

	[CodeProperty("Code"), DescriptionProperty("Description")]
	public interface ILocation : ILocationReference
	{
		ZString Code { get; }
		ZString Description { get; }

		ZBool IsActive { get; }

		RefCityTown CityTown { get; }
		RefCountry Country { get; }
		RefCountryStates State { get; }
		RefUNLOCO UNLOCO { get; }
		IATACityCode IATACityCode { get; }
		RefZoneHeader[] Zones { get; }
	}

	public interface ILocationBiz : ILocation
	{
		ZPropertyInfo CodeInfo { get; }
		ZPropertyInfo DescriptionInfo { get; }
		ZPropertyInfo IsActiveInfo { get; }
		ZString StateDescription { get; }
		ZPropertyInfo StateDescriptionInfo { get; }
	}
}
