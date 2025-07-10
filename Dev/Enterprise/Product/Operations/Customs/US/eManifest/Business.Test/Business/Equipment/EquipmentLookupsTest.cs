using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class EquipmentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookups()
		{
			var trip = Factory.New<Trip>();
			var lookups = trip.Equipment.AddNew().Lookups;
			AssertEquals("Equipment", typeof(RefEquipmentCollection), lookups.Equipment.GetType());
			AssertEquals("Currencies", typeof(RefCurrencyCollection), lookups.Currencies.GetType());
			AssertEquals("ConveyanceTypes", typeof(ConveyanceTypes), lookups.ConveyanceTypes.GetType());
			AssertEquals("EquipmentTypes", typeof(EquipmentTypes), lookups.EquipmentTypes.GetType());
			AssertEquals("RoadContainerTypes", typeof(RefContainerCollection), lookups.RoadContainerTypes.GetType());
			AssertEquals("RegistrationStates", typeof(RefCountryStatesCollection), lookups.RegistrationStates.GetType());
		}

		public void TestEquipmentFilterDefaultsForEquipment()
		{
			var equipment = (RefEquipmentCollection)Factory.New<Trip>().Equipment.AddNew().Lookups.Equipment;
			var filterDefault = equipment.FilterBusinessObjectDefaults["Vehicle Status:Property"];
			AssertNotNull("Vehicle Status default", filterDefault);
			AssertEquals("Vehicle Status default value", "Is not a Vehicle", filterDefault.Value);
			filterDefault = equipment.FilterBusinessObjectDefaults["e-Manifest Type:Property"];
			AssertNotNull("e-Manifest Type default", filterDefault);
			AssertEquals("e-Manifest Type default value", EquipmentTypeFilterList.Codes.Equipment, filterDefault.Value);
		}

		public void TestEquipmentFilterDefaultsForConveyance()
		{
			var equipment = (RefEquipmentCollection)Factory.New<Trip>().Conveyance.Lookups.Equipment;
			var filterDefault = equipment.FilterBusinessObjectDefaults["Vehicle Status:Property"];
			AssertNotNull("Vehicle Status default", filterDefault);
			AssertEquals("Vehicle Status default value", "Is a Vehicle", filterDefault.Value);
			filterDefault = equipment.FilterBusinessObjectDefaults["e-Manifest Type:Property"];
			AssertNotNull("e-Manifest Type default", filterDefault);
			AssertEquals("e-Manifest Type default value", EquipmentTypeFilterList.Codes.Conveyance, filterDefault.Value);
		}

		public void TestEquipmentFilterDefaultsForRoadContainerTypes()
		{
			var equipment = Factory.New<Trip>().Equipment.AddNew();
			var filterDefault = equipment.Lookups.RoadContainerTypes.FilterBusinessObjectDefaults["Transport Mode" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"];
			AssertNotNull("RoadContainerTypes Transport Mode default", filterDefault);
			AssertEquals("RoadContainerTypes Transport Mode is Road", RefContainerLookups.ShippingModes.Road, filterDefault.Value);
		}

		public void TestEquipmentFilterDefaultsForRegistrationStates()
		{
			var equipment = Factory.New<Trip>().Equipment.AddNew();
			equipment.BJ_RN_NKRegistrationCountry = Core.Constants.CountryCodes.Australia;
			var additionalFilter = equipment.Lookups.RegistrationStates.AdditionalFilter;
			AssertNotNull("Has Additional Filter", additionalFilter);
			Assert(additionalFilter.HasParameters);
			AssertEquals("AdditionalFilter is Country", RefCountryStatesSchema.RW_RN_NKCountryCode, additionalFilter.Params[0].SchemaColumn);
			equipment.BJ_RN_NKRegistrationCountry = ZString.Empty;
			var additionalFilterNoResults = equipment.Lookups.RegistrationStates.AdditionalFilter;
			AssertNotNull("Has Additional Filter", additionalFilterNoResults);
			AssertEquals("Additional Filter is No Result Query", ZQuery.NoResultQuery, additionalFilterNoResults);
		}
	}
}
