using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Not a valid physical location but a representation for Accounting Taxation purposes.
	/// Should not be mapped to any physical location i.e. Country, State etc.
	/// </summary>
	public class LocationRule : ILocation
	{
		public LocationRule(GlbCompany company, string placeOfSupplyRule)
		{
			Argument.NotNull(company, nameof(company));
			Argument.NotNullOrEmpty(placeOfSupplyRule, nameof(placeOfSupplyRule));

			PlaceOfSupplyRule = placeOfSupplyRule;
			Company = company;
		}

		string PlaceOfSupplyRule { get; }

		GlbCompany Company { get; }

		ZString ILocation.Code => PlaceOfSupplyRule;

		ZString ILocation.Description => PlaceOfSupplyListProvider.GetPlaceOfSupplyList(Company)?.GetDescriptionFromCode(PlaceOfSupplyRule) ?? string.Empty;

		ZBool ILocation.IsActive => true;

		RefCityTown ILocation.CityTown => null;

		RefCountry ILocation.Country => null;

		RefCountryStates ILocation.State => null;

		RefUNLOCO ILocation.UNLOCO => null;

		IATACityCode ILocation.IATACityCode => null;

		RefZoneHeader[] ILocation.Zones => Array.Empty<RefZoneHeader>();

		bool ILocationReference.IsLocalInRelationTo(ZString code) => false;
	}
}
