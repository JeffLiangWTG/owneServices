using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class ShipmentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookups()
		{
			var lookups = Factory.New<Trip>().Shipments.AddNew().Lookups;
			AssertEquals("ShipmentTypes", typeof(ShipmentTypes), lookups.ShipmentTypes.GetType());
			AssertEquals("ServiceTypes", typeof(ServiceTypes), lookups.ServiceTypes.GetType());
			AssertEquals("ScheduleKPortCodes", typeof(ZZRefCusCodeListCombinedCollection), lookups.ScheduleKPortCodes.GetType());
			AssertEquals("SCACCarrierCodes", typeof(USCarrierCombinedCollection), lookups.SCACCarrierCodes.GetType());
			var filters = (lookups.SCACCarrierCodes as USCarrierCombinedCollection).FilterBusinessObjectDefaults;
			var motFilter = filters["Mode Of Transportation:Property"];
			AssertEquals(Trip.Truck, motFilter.Value);
			AssertEquals("FIRMSCodes", typeof(ZZRefCusCodeListCombinedCollection), lookups.FIRMSCodes.GetType());
			Assert("WeightUnits", lookups.WeightUnits.ContainsCode(Core.Constants.Weight.Kilograms));
			Assert("VolumeUnits", lookups.VolumeUnits.ContainsCode(Core.Constants.Volume.CubicMetres));
			AssertEquals("QuantityUnits", typeof(PackageTypes), lookups.QuantityUnits.GetType());
			AssertEquals("ReleaseStatusList", typeof(ShipmentEntryStatusList), lookups.ReleaseStatusList.GetType());
			AssertEquals("Organizations", typeof(OrgHeaderCollection), lookups.Organizations.GetType());
			AssertEquals("ContainerMode", typeof(ContainerModeList), lookups.ContainerModeList.GetType());
			AssertEquals("Equipment", typeof(EquipmentCollection), lookups.Equipment.GetType());
			AssertEquals("UNDGSubs", typeof(UNDGSubstanceCollection), lookups.UNDGSubs.GetType());
			AssertEquals("Contacts", typeof(OrgContactCollection), lookups.Contacts.GetType());
		}

		public void TestScheduleKPortCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var foreignPort1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "port1", "port1", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			var foreignPort2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "port2", "port2", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort2.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.InBond);
			var foreignPort3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "port3", "port3", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort3.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.InBond);
			Factory.Save();
			var scheduleKCodeList = Factory.New<Trip>().Shipments.AddNew().Lookups.ScheduleKPortCodes;
			CombineAssertions(() =>
			{
				AssertNotNull(scheduleKCodeList);
				AssertSame("Cached", Factory.GetCachedValue("ScheduleKPortCodes", () => new ZZRefCusCodeListCombinedCollection(new CargoWise.EntityFramework.BusinessObjectFactory())), scheduleKCodeList);
				AssertEquals(3, scheduleKCodeList.Count);
				Assert(scheduleKCodeList.Contains(foreignPort1));
				Assert(scheduleKCodeList.Contains(foreignPort2));
				Assert(scheduleKCodeList.Contains(foreignPort3));
			});
		}
	}
}
