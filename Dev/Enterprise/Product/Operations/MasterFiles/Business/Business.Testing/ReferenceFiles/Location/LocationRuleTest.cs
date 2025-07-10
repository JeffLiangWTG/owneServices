using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class LocationRuleTest : TestCaseWithFactory
	{
		public void TestAsILocation()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var location = new LocationRule(GlbCompany.CurrentCompany, PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry) as ILocation;
				AssertNotNull("Should not be null", location);
				AssertNull("CityTown should be null", location.CityTown);
				AssertEquals("Code", "ALX", location.Code);
				AssertNull("Country", location.Country);
				AssertEquals("Description", "Outside the Login Country/Region", location.Description);
				AssertNull("IATACityCode", location.IATACityCode);
				AssertEquals("IsActive", true, location.IsActive);
				AssertNull("State", location.State);
				AssertNull("UNLOCO", location.UNLOCO);
				AssertEquals("Zone", 0, location.Zones.Length);
			}
		}

		public void TestDescriptionSetByCode()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.PredefinedRule.Code))
			{
				var location = new LocationRule(GlbCompany.CurrentCompany, PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry) as ILocation;
				AssertEquals(PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry, location.Code);
				AssertEquals(PlaceOfSupplyListProvider.Descriptions.OutsideTheLoginCountry, location.Description);

				location = new LocationRule(GlbCompany.CurrentCompany, PlaceOfSupplyListProvider.Codes.OtherTerritories);
				AssertEquals(PlaceOfSupplyListProvider.Codes.OtherTerritories, location.Code);
				AssertEquals(PlaceOfSupplyListProvider.Descriptions.OtherTerritories, location.Description);
			}

			var locationWithoutRegistry = new LocationRule(GlbCompany.CurrentCompany, PlaceOfSupplyListProvider.Codes.OtherTerritories) as ILocation;
			AssertEquals(PlaceOfSupplyListProvider.Codes.OtherTerritories, locationWithoutRegistry.Code);
			AssertEquals("Not valid unless registry is enabled", ZString.Empty, locationWithoutRegistry.Description);
		}
	}
}
