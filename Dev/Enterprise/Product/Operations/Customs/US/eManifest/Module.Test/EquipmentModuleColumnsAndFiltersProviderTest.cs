using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.eManifest.Module.Testing
{
	sealed class EquipmentModuleColumnsAndFiltersProviderTest : TestCaseWithFactory
	{
		public void TestAddFiltersWhenLicenseValid()
		{
			var filter = (ModuleTextFilter)filters[EquipmentModuleColumnsAndFiltersProvider.Descriptions.eManifestStatusFilterId];
			AssertNotNull("e-Manifest Type filter", filter);
			AssertProperEquipmentLoaded(filter, EquipmentTypeFilterList.Codes.Conveyance, equipment1, equipment2);
			AssertProperEquipmentLoaded(filter, EquipmentTypeFilterList.Codes.Equipment, equipment3, equipment4);
			AssertProperEquipmentLoaded(filter, EquipmentTypeFilterList.Codes.All, equipment1, equipment2, equipment3, equipment4, equipment5);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var refContainer1 = Factory.NewWithValidTestData<RefContainer>();
			CreateUsContainerCodeMap(refContainer1, ConveyanceTypes.Codes.PickupTruck);
			equipment1 = Factory.NewWithValidTestData<RefEquipment>();
			equipment1.RQ_RC_RoadContainerType = refContainer1.PK;
			var refContainer2 = Factory.NewWithValidTestData<RefContainer>();
			CreateUsContainerCodeMap(refContainer2, ConveyanceTypes.Codes.SemiTractor);
			equipment2 = Factory.NewWithValidTestData<RefEquipment>();
			equipment2.RQ_RC_RoadContainerType = refContainer2.PK;
			var refContainer3 = Factory.NewWithValidTestData<RefContainer>();
			CreateUsContainerCodeMap(refContainer3, EquipmentTypes.Codes.Container20FtSeaClosedTop);
			equipment3 = Factory.NewWithValidTestData<RefEquipment>();
			equipment3.RQ_RC_RoadContainerType = refContainer3.PK;
			var refContainer4 = Factory.NewWithValidTestData<RefContainer>();
			CreateUsContainerCodeMap(refContainer4, EquipmentTypes.Codes.SemiTruckTrailer);
			equipment4 = Factory.NewWithValidTestData<RefEquipment>();
			equipment4.RQ_RC_RoadContainerType = refContainer4.PK;
			var refContainer5 = Factory.NewWithValidTestData<RefContainer>();
			equipment5 = Factory.NewWithValidTestData<RefEquipment>();
			equipment5.RQ_RC_RoadContainerType = refContainer5.PK;
			Factory.Save();
			filters = new ModuleFilterCollection();
			new EquipmentModuleColumnsAndFiltersProvider().AddFilters(filters);
		}

		RefContainerCodeMap CreateUsContainerCodeMap(RefContainer refContainer, string usCode)
		{
			RefContainerCodeMap result = null;
			if (refContainer.GetCountrySpecificContainerCode(Core.Constants.CountryCodes.UnitedStates).IsEmpty)
			{
				result = refContainer.CodeMapCollection.AddNew();
				result.RCM_RN_NKCountry = "US";
				result.RCM_RC_Container = refContainer.PK;
				result.RCM_Code = usCode;
			}

			return result;
		}

		void AssertProperEquipmentLoaded(ModuleTextBaseFilter filter, string property, params RefEquipment[] expectedEquipment)
		{
			filter.Property = property;
			var actualEquipment = Factory.Load<RefEquipment>(filter.Query);
			AssertEquals("Proper quantity of equipment loaded", expectedEquipment.Length, actualEquipment.Length);
			foreach (var equipment in expectedEquipment)
			{
				Assert("Equipment should be loaded", actualEquipment.Any(e => e.PK == equipment.PK));
			}
		}

		RefEquipment equipment1;
		RefEquipment equipment2;
		RefEquipment equipment3;
		RefEquipment equipment4;
		RefEquipment equipment5;
		ModuleFilterCollection filters;
	}
}
