using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsLocationType))]
	class WhsLocationTypeTest : WhsEnvBusinessObjectTestCase
	{
		#region TestIsAutoLogged

		public void TestIsAutoLogged()
		{
			AssertEquals(true, Factory.New<WhsLocationType>().IsAutoAdminBusinessObjectLoggerEnabled);
		}

		#endregion

		#region TestWLT_LocationClass

		public void TestWLT_LocationClass()
		{
			AssertHasCustomAttribute<ListAttribute>(
				typeof(WhsLocationType), WhsLocationTypeSchema.Constants.WLT_LocationClass, false,
				la => la.ListDataSourceMember == "Lookups.LocationClasses");
		}

		#endregion

		#region TestWLT_RetainPalletIDsInFixedPickFaces

		public void TestWLT_RetainPalletIDsInFixedPickFaces_Readonly()
		{
			var locationType = Factory.New<WhsLocationType>();

			locationType.WLT_LocationClass = LocationClasses.Codes.TCL;
			AssertEquals(true, locationType.WLT_RetainPalletIDsInFixedPickFacesInfo.ReadOnly);

			locationType.WLT_LocationClass = LocationClasses.Codes.NOR;
			AssertEquals(true, locationType.WLT_RetainPalletIDsInFixedPickFacesInfo.ReadOnly);

			locationType.WLT_LocationClass = LocationClasses.Codes.DPF;
			AssertEquals(true, locationType.WLT_RetainPalletIDsInFixedPickFacesInfo.ReadOnly);

			locationType.WLT_LocationClass = LocationClasses.Codes.DDL;
			AssertEquals(true, locationType.WLT_RetainPalletIDsInFixedPickFacesInfo.ReadOnly);

			locationType.WLT_LocationClass = LocationClasses.Codes.HPL;
			AssertEquals(true, locationType.WLT_RetainPalletIDsInFixedPickFacesInfo.ReadOnly);

			locationType.WLT_LocationClass = LocationClasses.Codes.FIX;
			AssertEquals(false, locationType.WLT_RetainPalletIDsInFixedPickFacesInfo.ReadOnly);
		}

		#endregion

		#region TestWLT_DefaultCycleCountGranularity

		public void TestWLT_DefaultCycleCountGranularity()
		{
			AssertHasCustomAttribute<ListAttribute>(
				typeof(WhsLocationType), WhsLocationTypeSchema.Constants.WLT_DefaultCycleCountGranularity, false,
				la => la.ListDataSourceMember == "Lookups.CycleCountGranularities");
		}

		public void TestWLT_DefaultCycleCountGranularity_ReadOnly()
		{
			var locationType = Factory.New<WhsLocationType>();

			locationType.WLT_LocationClass = LocationClasses.Codes.TCL;
			AssertEquals(true, locationType.WLT_DefaultCycleCountGranularityInfo.ReadOnly);

			locationType.WLT_LocationClass = LocationClasses.Codes.NOR;
			AssertEquals(false, locationType.WLT_DefaultCycleCountGranularityInfo.ReadOnly);

			locationType.WLT_LocationClass = LocationClasses.Codes.DPF;
			AssertEquals(false, locationType.WLT_DefaultCycleCountGranularityInfo.ReadOnly);

			locationType.WLT_LocationClass = LocationClasses.Codes.DDL;
			AssertEquals(true, locationType.WLT_DefaultCycleCountGranularityInfo.ReadOnly);

			locationType.WLT_LocationClass = LocationClasses.Codes.FIX;
			AssertEquals(false, locationType.WLT_DefaultCycleCountGranularityInfo.ReadOnly);

			locationType.WLT_LocationClass = LocationClasses.Codes.HPL;
			AssertEquals(false, locationType.WLT_DefaultCycleCountGranularityInfo.ReadOnly);

			locationType.WLT_LocationClass = LocationClasses.Codes.PST;
			AssertEquals(true, locationType.WLT_DefaultCycleCountGranularityInfo.ReadOnly);

			locationType.WLT_LocationClass = LocationClasses.Codes.CON;
			AssertEquals(true, locationType.WLT_DefaultCycleCountGranularityInfo.ReadOnly);
		}

		#endregion

		#region TestProperties

		public void TestHumanReadableName()
		{
			var locationType = Factory.New<WhsLocationType>();
			AssertEquals("Location Type", locationType.HumanReadableName);

			locationType.WLT_Code = "132";
			AssertEquals("Location Type 132", locationType.HumanReadableName);
		}

		public void TestWLT_LocationClass_SetDefaults_NotFix()
		{
			var locationType1 = Helper.CreateLocationType("111", "Description", true, 500, LocationClasses.Codes.FIX);

			foreach (var locType in new LocationClasses().ToArray())
			{
				locationType1.WLT_MaximumNumberOfProducts = 500;
				locationType1.WLT_IsPalletIDNeutral = true;
				locationType1.WLT_RetainPalletIDsInFixedPickFaces = true;
				AssertEquals("Precondition", 500, locationType1.WLT_MaximumNumberOfProducts);
				AssertEquals("Precondition", true, locationType1.WLT_RetainPalletIDsInFixedPickFaces);

				locationType1.WLT_LocationClass = locType.Code;

				if (locType.Code != LocationClasses.Codes.FIX)
				{
					AssertEquals("Not FIX location class should have number of products set to 0.", 0, locationType1.WLT_MaximumNumberOfProducts);
					AssertEquals("Not FIX location class should have RetainPalletIDsInFixedPickFaces set to true.", false, locationType1.WLT_RetainPalletIDsInFixedPickFaces);
				}
				else
				{
					AssertEquals("Should not clear fields.", 500, locationType1.WLT_MaximumNumberOfProducts);
					AssertEquals("Should not clear fields.", true, locationType1.WLT_RetainPalletIDsInFixedPickFaces);
				}
			}
		}

		public void TestWLT_LocationClass_SetDefaults_DDL()
		{
			var locationType1 = Helper.CreateLocationType("111", "Description", true, 500, LocationClasses.Codes.FIX);

			foreach (var locType in new LocationClasses().ToArray())
			{
				locationType1.WLT_IsPalletIDNeutral = true;

				locationType1.WLT_LocationClass = locType.Code;

				if (locType.Code == LocationClasses.Codes.DDL)
				{
					AssertEquals("DDL location class should set Pallet ID neutral to false.", false, locationType1.WLT_IsPalletIDNeutral);
				}
				else
				{
					AssertEquals("Should not clear fields.", true, locationType1.WLT_IsPalletIDNeutral);
				}
			}
		}

		public void TestWLT_LocationClass_SetDefaults_NotTemperatureControlled()
		{
			var locationType1 = Helper.CreateLocationType("111", "Description", false, 0, LocationClasses.Codes.TCL);

			foreach (var locType in new LocationClasses().ToArray())
			{
				locationType1.WLT_MinimumTemperature = 1m;
				locationType1.WLT_MaximumTemperature = 2m;
				locationType1.WLT_TemperatureUnit = Enterprise.Core.Constants.Temperature.Fahrenheit;

				locationType1.WLT_LocationClass = locType.Code;
				if (locType.Code != LocationClasses.Codes.TCL)
				{
					AssertEquals("Should clear temperature fields.", 0m, locationType1.WLT_MinimumTemperature);
					AssertEquals("Should clear temperature fields.", 0m, locationType1.WLT_MaximumTemperature);
					AssertEquals("Should clear temperature fields.", string.Empty, locationType1.WLT_TemperatureUnit);
				}
				else
				{
					AssertEquals("Should *NOT* clear temperature fields.", 1m, locationType1.WLT_MinimumTemperature);
					AssertEquals("Should *NOT* clear temperature fields.", 2m, locationType1.WLT_MaximumTemperature);
					AssertEquals("Should *NOT* clear temperature fields.", Enterprise.Core.Constants.Temperature.Fahrenheit, locationType1.WLT_TemperatureUnit);
				}
			}
		}

		public void TestWLT_LocationClass_SetDefaults_TemperatureControlled_DefaultsTemperatureUnit()
		{
			var locationType = Helper.CreateLocationType("111", "Description", false, 0, LocationClasses.Codes.FIX);
			locationType.WLT_LocationClass = LocationClasses.Codes.TCL;
			AssertEquals("Should default temperature unit.", Enterprise.Core.Constants.Temperature.Centigrade, locationType.WLT_TemperatureUnit);
		}

		public void TestDefaultCycleCountGranularity_Default()
		{
			var locationType = Factory.New<WhsLocationType>();
			AssertEquals("Granularity default value has to be PWA", CycleCountGranularities.Codes.PWA, locationType.WLT_DefaultCycleCountGranularity);
		}

		public void TestWLT_DefaultCycleCountGranularities_DefaultToEmptyForUnsupportedTypes()
		{
			var locationType = Factory.New<WhsLocationType>();

			locationType.WLT_LocationClass = LocationClasses.Codes.DPF;
			AssertEquals("Granularity value should be PWA", CycleCountGranularities.Codes.PWA, locationType.WLT_DefaultCycleCountGranularity);

			locationType.WLT_LocationClass = LocationClasses.Codes.TCL;
			AssertEquals("Should clear the granularity automatically", "", locationType.WLT_DefaultCycleCountGranularity);

			locationType.WLT_LocationClass = LocationClasses.Codes.NOR;
			AssertEquals("Granularity value has to be changed back to PWA", CycleCountGranularities.Codes.PWA, locationType.WLT_DefaultCycleCountGranularity);

			locationType.WLT_LocationClass = LocationClasses.Codes.DDL;
			AssertEquals("Should clear the granularity automatically", "", locationType.WLT_DefaultCycleCountGranularity);

			locationType.WLT_LocationClass = LocationClasses.Codes.FIX;
			AssertEquals("Granularity value has to be changed back to PWA", CycleCountGranularities.Codes.PWA, locationType.WLT_DefaultCycleCountGranularity);

			locationType.WLT_LocationClass = LocationClasses.Codes.PST;
			AssertEquals("Should clear the granularity automatically", "", locationType.WLT_DefaultCycleCountGranularity);

			locationType.WLT_LocationClass = LocationClasses.Codes.HPL;
			AssertEquals("Granularity value has to be changed back to PWA", CycleCountGranularities.Codes.PWA, locationType.WLT_DefaultCycleCountGranularity);

			locationType.WLT_LocationClass = LocationClasses.Codes.CON;
			AssertEquals("Should clear the granularity automatically", "", locationType.WLT_DefaultCycleCountGranularity);
		}

		#endregion

		#region TestPropertyInfos

		#region TestWLT_IsPalletIDNeutralInfo

		public void TestWLT_IsPalletIDNeutralInfo()
		{
			TestIsDDLReadOnly(lt => lt.WLT_IsPalletIDNeutralInfo);
		}

		#endregion

		#region TestWLT_MaximumNumberOfProductsInfo

		public void TestWLT_MaximumNumberOfProductsInfo()
		{
			TestIsNotFIXTypeReadOnly(lt => lt.WLT_MaximumNumberOfProductsInfo);
		}

		#endregion

		#region TestWLT_MaximumTemperatureInfo

		public void TestWLT_MaximumTemperatureInfo()
		{
			TestIsNotTCLTypeReadOnly(lt => lt.WLT_MaximumTemperatureInfo);
		}

		#endregion

		#region TestWLT_MinimumTemperatureInfo

		public void TestWLT_MinimumTemperatureInfo()
		{
			TestIsNotTCLTypeReadOnly(lt => lt.WLT_MinimumTemperatureInfo);
		}

		#endregion

		#region TestWLT_TemperatureUnitInfo

		public void TestWLT_TemperatureUnitInfo_ReadOnly()
		{
			TestIsNotTCLTypeReadOnly(lt => lt.WLT_TemperatureUnitInfo);
		}

		public void TestWLT_TemperatureUnitInfo_List()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(WhsLocationType), nameof(WhsLocationType.WLT_TemperatureUnit), false, la => la.ListDataSourceMember == "Lookups.TemperatureUnits");
		}

		#endregion

		#region TestWLT_IsSystemInfo

		public void TestWLT_IsSystemInfo()
		{
			var locationType = Factory.New<WhsLocationType>();
			AssertEquals(true, locationType.WLT_IsSystemInfo.ReadOnly);
		}

		#endregion

		#region TestReadOnly

		#region TestIsDDLReadOnly

		void TestIsDDLReadOnly(Func<WhsLocationType, ZPropertyInfo> getPropertyInfo)
		{
			var locationType1 = Helper.CreateLocationType("333", LocationClasses.Codes.DDL);
			var info = getPropertyInfo(locationType1);
			AssertEquals($"When location class is DDL the {info.Name} should be readonly.", true, info.ReadOnly);

			locationType1.WLT_LocationClass = LocationClasses.Codes.NOR;
			AssertEquals($"When location class is NOT DDL the {info.Name} should not be readonly.", false, info.ReadOnly);
		}

		#endregion

		#region TestIsNotFIXTypeReadOnly

		void TestIsNotFIXTypeReadOnly(Func<WhsLocationType, ZPropertyInfo> getPropertyInfo)
		{
			var locationType1 = Helper.CreateLocationType("222", LocationClasses.Codes.NOR);
			var info = getPropertyInfo(locationType1);

			foreach (var locType in new LocationClasses().ToArray().Where(lc => lc.Code != LocationClasses.Codes.FIX))
			{
				locationType1.WLT_LocationClass = locType.Code;
				AssertEquals($"When location class is NOT FIX the {info.Name} should be readonly.", true, info.ReadOnly);
			}

			locationType1.WLT_LocationClass = LocationClasses.Codes.FIX;
			AssertEquals($"When location class is FIX the {info.Name} should not be readonly.", false, info.ReadOnly);
		}

		#endregion

		#region TestIsNotTCLTypeReadOnly

		void TestIsNotTCLTypeReadOnly(Func<WhsLocationType, ZPropertyInfo> getPropertyInfo)
		{
			var locationType1 = Helper.CreateLocationType("222", LocationClasses.Codes.NOR);
			var info = getPropertyInfo(locationType1);

			foreach (var locType in new LocationClasses().ToArray().Where(lc => lc.Code != LocationClasses.Codes.TCL))
			{
				locationType1.WLT_LocationClass = locType.Code;
				AssertEquals($"When location class is NOT TCL the {info.Name} should be readonly.", true, info.ReadOnly);
			}

			locationType1.WLT_LocationClass = LocationClasses.Codes.TCL;
			AssertEquals($"When location class is TCL the {info.Name} should not be readonly.", false, info.ReadOnly);
		}

		#endregion

		#endregion

		#endregion

		#region TestIsCycleCountingSupportedForLocationClass

		public void TestIsCycleCountingSupportedForLocationClass()
		{
			var unsupportedCycleCountLocationTypes
				= new[] { LocationClasses.Codes.TCL, LocationClasses.Codes.DDL, LocationClasses.Codes.PST, LocationClasses.Codes.CON };

			var locationClasses = new LocationClasses();

			foreach (var locationClass in locationClasses.GetAllCodes())
			{
				var expectSupport = !unsupportedCycleCountLocationTypes.Contains(locationClass);
				AssertEquals($"Expected value {expectSupport} for location class {locationClass}",
					expectSupport,
					WhsLocationType.IsCycleCountingSupportedForLocationClass(locationClass));
			}
		}

		#endregion

		#region TestIsLocationTypeSupportedByThisWarehouseType

		public void TestIsLocationTypeSupportedByThisWarehouseType()
		{
			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.DDL, WarehouseTypes.Codes.Product, true);
			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.DDL, WarehouseTypes.Codes.FreeTradeZone, true);
			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.DDL, WarehouseTypes.Codes.Transit, true);
			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.DDL, WarehouseTypes.Codes.ContainerYard, false);

			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.DPF, WarehouseTypes.Codes.Product, true);
			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.DPF, WarehouseTypes.Codes.FreeTradeZone, true);
			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.DPF, WarehouseTypes.Codes.Transit, true);
			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.DPF, WarehouseTypes.Codes.ContainerYard, true);

			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.FIX, WarehouseTypes.Codes.Product, true);
			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.FIX, WarehouseTypes.Codes.FreeTradeZone, true);
			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.FIX, WarehouseTypes.Codes.Transit, true);
			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.FIX, WarehouseTypes.Codes.ContainerYard, false);

			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.HPL, WarehouseTypes.Codes.Product, true);
			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.HPL, WarehouseTypes.Codes.FreeTradeZone, true);
			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.HPL, WarehouseTypes.Codes.Transit, true);
			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.HPL, WarehouseTypes.Codes.ContainerYard, false);

			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.NOR, WarehouseTypes.Codes.Product, true);
			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.NOR, WarehouseTypes.Codes.FreeTradeZone, true);
			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.NOR, WarehouseTypes.Codes.Transit, true);
			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.NOR, WarehouseTypes.Codes.ContainerYard, true);

			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.TCL, WarehouseTypes.Codes.Product, false);
			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.TCL, WarehouseTypes.Codes.FreeTradeZone, false);
			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.TCL, WarehouseTypes.Codes.Transit, true);
			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.TCL, WarehouseTypes.Codes.ContainerYard, true);

			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.PST, WarehouseTypes.Codes.Product, true);
			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.PST, WarehouseTypes.Codes.FreeTradeZone, true);
			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.PST, WarehouseTypes.Codes.Transit, false);
			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.PST, WarehouseTypes.Codes.ContainerYard, false);

			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.CON, WarehouseTypes.Codes.Product, true);
			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.CON, WarehouseTypes.Codes.FreeTradeZone, true);
			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.CON, WarehouseTypes.Codes.Transit, false);
			TestIsLocationTypeSupportedByThisWarehouseTypeCore(LocationClasses.Codes.CON, WarehouseTypes.Codes.ContainerYard, false);

			void TestIsLocationTypeSupportedByThisWarehouseTypeCore(string locationClass, string warehouseType, bool expectedSupported)
			{
				var locationType = Helper.CreateLocationType("111", "Description", false, 0, locationClass);
				AssertEquals(nameof(WhsLocationType.IsLocationTypeSupportedByThisWarehouseType), expectedSupported, locationType.IsLocationTypeSupportedByThisWarehouseType(warehouseType));
			}
		}

		#endregion

		#region TestDelete

		public void TestDelete()
		{
			var expectedMessageInUse = "Location types which are in use cannot be deleted.";

			var locationTypeNotInUse = Helper.CreateLocationType("DJ1");
			var locationTypeInUseByLocation = Helper.CreateLocationType("DJ2");
			var locationTypeInUseByWarehouse = Helper.CreateLocationType("DJ3");

			var warehouse = Helper.CreateWarehouse("WHS1");
			warehouse.WW_WLT_DefaultLocationType = locationTypeInUseByWarehouse.PK;

			Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2);
			Factory.Save();

			var location = warehouse.FindLocation("A-1-1");
			location.WLV_WLT_LocationType = locationTypeInUseByLocation.PK;

			AssertEquals("Can delete Location Type which is not in use", true, locationTypeNotInUse.CanDelete);
			AssertEquals("No Error Message", true, string.IsNullOrEmpty(locationTypeNotInUse.ReasonForNotAbleToDelete));

			AssertEquals("Cannot delete Location Type which is in use in Location", false, locationTypeInUseByLocation.CanDelete);
			AssertEquals("Error Message", expectedMessageInUse, locationTypeInUseByLocation.ReasonForNotAbleToDelete);

			AssertEquals("Cannot delete Location Type which is in use as default by Warehouse", false, locationTypeInUseByWarehouse.CanDelete);
			AssertEquals("Error Message", expectedMessageInUse, locationTypeInUseByWarehouse.ReasonForNotAbleToDelete);

			var expectedMessage = "System location types cannot be deleted.";
			var locationType = Helper.CreateLocationType("DJS");

			locationType.WLT_IsSystem = true;
			AssertEquals("Cannot delete System Location Type", false, locationType.CanDelete);
			AssertEquals("Error Message", expectedMessage, locationType.ReasonForNotAbleToDelete);

			locationType.WLT_IsSystem = false;
			AssertEquals("Can delete Non-System Location Type", true, locationType.CanDelete);
			AssertEquals("No Error Message", true, string.IsNullOrEmpty(locationType.ReasonForNotAbleToDelete));
		}

		#endregion
	}
}
