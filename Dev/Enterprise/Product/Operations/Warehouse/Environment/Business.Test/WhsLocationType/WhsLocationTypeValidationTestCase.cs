using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class WhsLocationTypeValidationTestCase : WhsBusinessObjectValidationTestCase
	{
		#region TestCheckWLT_Code

		public void TestCheckWLT_Code()
		{
			var expectedEnterACodeErrorMessage = "Please enter a Code.";
			var expectedUniqueCodeErrorMessage = "Code must be unique.";

			var locationType1 = Factory.New<WhsLocationType>();
			locationType1.WLT_Code = "";
			AssertHasError(locationType1.WLT_CodeInfo, expectedEnterACodeErrorMessage);

			locationType1.WLT_Code = "TSU";
			AssertNoError(locationType1.WLT_CodeInfo, expectedEnterACodeErrorMessage);

			var locationType2 = Helper.CreateLocationType("TSU");
			AssertHasError(locationType2.WLT_CodeInfo, expectedUniqueCodeErrorMessage);

			locationType2.WLT_Code = "TSY";
			AssertNoError(locationType2.WLT_CodeInfo, expectedUniqueCodeErrorMessage);
		}

		#endregion

		#region TestCheckWLT_Description

		public void TestCheckWLT_Description()
		{
			var expectedMessage = "Please enter a Description.";
			var locationType1 = Factory.New<WhsLocationType>();
			locationType1.WLT_Description = "";
			AssertHasError(locationType1.WLT_DescriptionInfo, expectedMessage);

			locationType1.WLT_Description = "New Description";
			AssertNoError(locationType1.WLT_DescriptionInfo, expectedMessage);
		}

		#endregion

		#region  TestCheckWLT_LocationClass

		public void TestCheckWLT_LocationClass()
		{
			var expectedMessage = "Enter a valid Location Class.";
			var locationType1 = Factory.New<WhsLocationType>();
			locationType1.WLT_LocationClass = "ANC";
			AssertHasError(locationType1.WLT_LocationClassInfo, expectedMessage);

			locationType1.WLT_LocationClass = LocationClasses.Codes.DDL;
			AssertNoError(locationType1.WLT_LocationClassInfo, expectedMessage);
		}

		public void TestCheckWLT_LocationClass_MustNotBeUsedOnLocationWithPendingAvailableStock()
		{
			var expectedMessage = "Location Class cannot be changed if it is used on location with pending/available stock.";

			var warehouse = Helper.CreateWarehouse("WH1", "B", 2, 2);
			Factory.Save();

			var location1 = warehouse.FindLocation("B-1-2");
			var location2 = warehouse.FindLocation("B-2-1");
			var locationType1 = Helper.CreateLocationType("TB1", "TB1 Test", false, 0, LocationClasses.Codes.NOR);
			var locationType2 = Helper.CreateLocationType("TB2", "TB2 Test", false, 0, LocationClasses.Codes.NOR);
			location1.WLV_WLT_LocationType = locationType1.PK;
			location2.WLV_WLT_LocationType = locationType2.PK;

			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var orgPK = iHelper.CreateClient("WHB1TST");
			var part = iHelper.CreateProduct(orgPK, "PB");
			var receivePK = iHelper.CreateWhsReceive(orgPK, warehouse.PK, "1", Notify);
			var receiveLinePk = iHelper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, location1.PK);
			iHelper.FinaliseDocket(receivePK);
			Factory.Save(); // needed for DBOnly Query

			locationType1.WLT_LocationClass = LocationClasses.Codes.DDL;
			locationType2.WLT_LocationClass = LocationClasses.Codes.DDL;
			AssertHasError(locationType1.WLT_LocationClassInfo, expectedMessage);
			AssertNoError(locationType2.WLT_LocationClassInfo, expectedMessage);

			var inventory = Factory.Load<IWhsInventoryView>(receiveLinePk);
			AssertEquals("Precondition", 10m, inventory.WI_TotalUnits);

			var orderPk = iHelper.CreateWhsOrder(orgPK, warehouse.PK, "1", Notify);
			var orderLine = iHelper.CreateWhsOrderLine(orderPk, part.PK, 10m);
			var pickPk = iHelper.CreateWhsPick(new[] { orderPk });
			iHelper.FinaliseDocket(orderPk);
			iHelper.FinalisePick(pickPk);
			AssertEquals("Precondition", 0m, inventory.WI_TotalUnits);

			locationType1.WLT_LocationClass = LocationClasses.Codes.DDL;
			AssertNoError(locationType1.WLT_LocationClassInfo, expectedMessage);
		}

		public void TestCheckWLT_LocationClass_CannotChangeIfLocationIsUsedAsWarehouseDefaultDDL()
		{
			var expectedMessage = "Location Class cannot be changed if it is used on warehouse default outbound dock door location.";
			var warehouse = Helper.CreateWarehouse("WH1", "A");
			Factory.Save(); // to generate DDL

			var defaultDDLType = warehouse.DefaultOutboundDockDoorLocation.LocationType;
			defaultDDLType.WLT_LocationClass = LocationClasses.Codes.NOR;
			AssertHasError("When location type is used by a location that is default outbound DDL of warehouse, do not allow to change class.", defaultDDLType.WLT_LocationClassInfo, expectedMessage);

			defaultDDLType.WLT_LocationClass = LocationClasses.Codes.DDL; // reset location class
			var ddlLocationType = Helper.CreateLocationType("123", LocationClasses.Codes.DDL);
			var location = warehouse.FindLocation("A");
			location.WLV_WLT_LocationType = ddlLocationType.PK;
			Factory.Save();

			// link another DDL as Whs default DDL
			warehouse.WW_DefaultOutboundDockDoor = location.PK;
			Factory.Save(); // validation is using DBOnlyQuery, need to save whs with new outbound
			defaultDDLType.WLT_LocationClass = LocationClasses.Codes.DPF;
			AssertNoError("Changing class of location type that is no longer used by default outbound DDL on warehouse is permitted.", defaultDDLType.WLT_LocationClassInfo, expectedMessage);
		}

		public void TestCheckWLT_LocationClass_CannotChangeIfLocationIsUsedAsPicksDDL()
		{
			var expectedMessage = "Location Class cannot be changed if it is used on pick as dock door location.";
			var warehouse = Helper.CreateWarehouse("WH1", "A");
			Factory.Save();

			var ddlLocationType = Helper.CreateLocationType("123", LocationClasses.Codes.DDL);
			var location = warehouse.FindLocation("A");
			location.WLV_WLT_LocationType = ddlLocationType.PK;

			var pick = (BusinessObject)Factory.New<IWhsPick>();
			pick[WhsPickSchema.WP_WL_DockDoor] = location.PK;
			Factory.Save();

			ddlLocationType.WLT_LocationClass = LocationClasses.Codes.NOR;
			AssertHasError("When location type is used by a location that is DDL of pick, do not allow to change class.", ddlLocationType.WLT_LocationClassInfo, expectedMessage);

			// remove DDL from pick
			pick[WhsPickSchema.WP_WL_DockDoor] = ZGuid.Empty;
			Factory.Save(); // validation is using DBOnlyQuery, need to save whs with new outbound
			ddlLocationType.WLT_LocationClass = LocationClasses.Codes.DPF;
			AssertNoError("Changing class of location type that is no longer used as DDL on pick is permitted.", ddlLocationType.WLT_LocationClassInfo, expectedMessage);
		}

		public void TestCheckWLT_LocationClass_CannotChangeIfLocationIsUsedAsPicksPackingStation()
		{
			var expectedMessage = "Location Class cannot be changed if it is used on pick as packing station location.";
			var warehouse = Helper.CreateWarehouse("WH1", "A");
			Factory.Save();

			var pstLocationType = Helper.CreateLocationType("123", LocationClasses.Codes.PST);
			var location = warehouse.FindLocation("A");
			location.WLV_WLT_LocationType = pstLocationType.PK;

			var client = Helper.CreateClient("WHS1TST");
			var part = Helper.CreateProduct(client, "P1");
			Factory.Save();

			var transactionHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var orderPK = transactionHelper.CreateWhsOrder(client.PK, warehouse.PK, "O1", Notify);
			transactionHelper.CreateWhsOrderLine(orderPK, part.PK, 100m);

			var pick = transactionHelper.CreatePickNew(finaliseOrders: false, finalisePick: false, orderPK);
			pick[WhsPickSchema.WP_WL_PackingStation] = location.PK;
			Factory.Save();

			pstLocationType.WLT_LocationClass = LocationClasses.Codes.NOR;
			AssertHasError("When location type is used by a location that is PST of pick, do not allow to change class.", pstLocationType.WLT_LocationClassInfo, expectedMessage);

			pstLocationType.WLT_LocationClass = LocationClasses.Codes.PST;
			AssertNoError(pstLocationType.WLT_LocationClassInfo, expectedMessage);
		}

		public void TestCheckWLT_LocationClass_CannotChangeFromFIXWhenUsedInPickFace()
		{
			var expectedMessage = "For Location used in a Pick face the location type class must be FIX";

			var warehouse = Helper.CreateWarehouse("WHT");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "PF", 2, 1);

			var fixLocationUsed = row.Locations[0];
			var fixLocationNotUsed = row.Locations[1];
			var locationTypePF1 = Helper.CreateLocationType("PF1", "Pick Face One", false, 1, LocationClasses.Codes.FIX);
			var locationTypePF2 = Helper.CreateLocationType("PF2", "Pick Face Two", false, 1, LocationClasses.Codes.FIX);
			fixLocationUsed.WLV_WLT_LocationType = locationTypePF1.PK;
			fixLocationNotUsed.WLV_WLT_LocationType = locationTypePF2.PK;

			var org = Helper.CreateClient("CLT001");
			var part = Helper.CreateProduct(org, "PDT001");
			var pickFace = Helper.CreateProductPickFace(part, org, fixLocationUsed);

			Factory.Save();

			locationTypePF2.WLT_LocationClass = LocationClasses.Codes.NOR;
			locationTypePF2.WLT_MaximumNumberOfProducts = 0;
			AssertNoError("Changing a Location class to another code is permitted when the location type is not part of a Pick Face", locationTypePF2.WLT_LocationClassInfo, expectedMessage);

			locationTypePF1.WLT_LocationClass = LocationClasses.Codes.NOR;
			locationTypePF1.WLT_MaximumNumberOfProducts = 0;
			AssertHasError("When a location type is used by a location that is used by a Pick face the class must stay as FIX.", locationTypePF1.WLT_LocationClassInfo, expectedMessage);

			locationTypePF1.WLT_LocationClass = LocationClasses.Codes.FIX;
			locationTypePF1.WLT_MaximumNumberOfProducts = 1;
			pickFace.Delete();
			Factory.Save();

			locationTypePF1.WLT_LocationClass = LocationClasses.Codes.NOR;
			AssertNoError("Changing a Location class to another code is permitted when the location type is not part of a Pick Face", locationTypePF1.WLT_LocationClassInfo, expectedMessage);
		}

		#endregion

		#region TestCheckWLT_MaximumNumberOfProducts

		public void TestCheckWLT_MaximumNumberOfProducts()
		{
			var expectedMessage = "Maximum Number Of Products can only be 0 for this location class.";
			var locationType = Helper.CreateLocationType("TSL", LocationClasses.Codes.NOR);
			AssertNoError(locationType.WLT_MaximumNumberOfProductsInfo, expectedMessage);

			locationType.WLT_MaximumNumberOfProducts = -1;
			AssertHasError(locationType.WLT_MaximumNumberOfProductsInfo, expectedMessage);

			locationType.WLT_MaximumNumberOfProducts = 20;
			AssertHasError(locationType.WLT_MaximumNumberOfProductsInfo, expectedMessage);

			locationType.WLT_LocationClass = LocationClasses.Codes.FIX;
			locationType.WLT_MaximumNumberOfProducts = 15;
			AssertNoError(locationType.WLT_MaximumNumberOfProductsInfo, expectedMessage);

			TestMinInt(locationType.WLT_MaximumNumberOfProductsInfo, ErrorCheckType.HasErrors, 1);
		}

		#endregion

		#region TestCheckWLT_MinimumTemperature

		public void TestCheckWLT_MinimumTemperature()
		{
			var expectedMessage = "Minimum Temperature can only be 0 for this location class.";
			var locationType = Helper.CreateLocationType("TSL", LocationClasses.Codes.TCL);
			AssertNoError(locationType.WLT_MinimumTemperatureInfo, expectedMessage);

			locationType.WLT_MinimumTemperature = -1;
			AssertNoError(locationType.WLT_MinimumTemperatureInfo, expectedMessage);

			locationType.WLT_MinimumTemperature = 1;
			AssertNoError(locationType.WLT_MinimumTemperatureInfo, expectedMessage);

			foreach (var locType in new LocationClasses().ToArray().Where(lc => lc.Code != LocationClasses.Codes.TCL))
			{
				locationType.WLT_LocationClass = locType.Code;

				locationType.WLT_MinimumTemperature = -1;
				AssertHasError(locationType.WLT_MinimumTemperatureInfo, expectedMessage);

				locationType.WLT_MinimumTemperature = 1;
				AssertHasError(locationType.WLT_MinimumTemperatureInfo, expectedMessage);
			}
		}

		#endregion

		#region TestCheckWLT_MinimumTemperature_MustBeLessThanOrEqualToTheMaximum

		public void TestCheckWLT_MinimumTemperature_MustBeLessThanOrEqualToTheMaximum()
		{
			var expectedMessage = "The Minimum Temperature must be less than or equal to the Maximum Temperature.";
			var locationType = Helper.CreateLocationType("TSL", LocationClasses.Codes.TCL);
			AssertNoError(locationType.WLT_MinimumTemperatureInfo, expectedMessage);

			locationType.WLT_MinimumTemperature = 1;
			locationType.WLT_MaximumTemperature = 1;
			AssertNoError(locationType.WLT_MinimumTemperatureInfo, expectedMessage);

			locationType.WLT_MinimumTemperature = 2;
			AssertHasError(locationType.WLT_MinimumTemperatureInfo, expectedMessage);

			locationType.WLT_MinimumTemperature = 0;
			AssertNoError(locationType.WLT_MinimumTemperatureInfo, expectedMessage);

			locationType.WLT_MaximumTemperature = -1;
			AssertHasError(locationType.WLT_MinimumTemperatureInfo, expectedMessage);
		}

		#endregion

		#region TestCheckWLT_MaximumTemperature

		public void TestCheckWLT_MaximumTemperature()
		{
			var expectedMessage = "Maximum Temperature can only be 0 for this location class.";
			var locationType = Helper.CreateLocationType("TSL", LocationClasses.Codes.TCL);
			AssertNoError(locationType.WLT_MaximumTemperatureInfo, expectedMessage);

			locationType.WLT_MaximumTemperature = -1;
			AssertNoError(locationType.WLT_MaximumTemperatureInfo, expectedMessage);

			locationType.WLT_MaximumTemperature = 1;
			AssertNoError(locationType.WLT_MaximumTemperatureInfo, expectedMessage);

			foreach (var locType in new LocationClasses().ToArray().Where(lc => lc.Code != LocationClasses.Codes.TCL))
			{
				locationType.WLT_LocationClass = locType.Code;

				locationType.WLT_MaximumTemperature = -1;
				AssertHasError(locationType.WLT_MaximumTemperatureInfo, expectedMessage);

				locationType.WLT_MaximumTemperature = 1;
				AssertHasError(locationType.WLT_MaximumTemperatureInfo, expectedMessage);
			}
		}

		#endregion

		#region TestCheckWLT_MaximumTemperature_MustBeGreaterThanOrEqualToTheMinimum

		public void TestCheckWLT_MaximumTemperature_MustBeGreaterThanOrEqualToTheMinimum()
		{
			var expectedMessage = "The Maximum Temperature must be greater than or equal to the Minimum Temperature.";
			var locationType = Helper.CreateLocationType("TSL", LocationClasses.Codes.TCL);
			AssertNoError(locationType.WLT_MaximumTemperatureInfo, expectedMessage);

			locationType.WLT_MinimumTemperature = 1;
			locationType.WLT_MaximumTemperature = 1;
			AssertNoError(locationType.WLT_MaximumTemperatureInfo, expectedMessage);

			locationType.WLT_MaximumTemperature = 0;
			AssertHasError(locationType.WLT_MaximumTemperatureInfo, expectedMessage);

			locationType.WLT_MaximumTemperature = 2;
			AssertNoError(locationType.WLT_MaximumTemperatureInfo, expectedMessage);

			locationType.WLT_MinimumTemperature = 3;
			AssertHasError(locationType.WLT_MaximumTemperatureInfo, expectedMessage);
		}

		#endregion

		#region TestCheckWLT_TemperatureUnit_NotTCL

		public void TestCheckWLT_TemperatureUnit_NotTCL()
		{
			var expectedMessage = "Temperature Unit should not be set for this location class.";

			var locationType = Helper.CreateLocationType("TSL", LocationClasses.Codes.TCL);
			locationType.WLT_TemperatureUnit = Enterprise.Core.Constants.Temperature.Centigrade;
			AssertNoError(locationType.WLT_TemperatureUnitInfo, expectedMessage);

			foreach (var locType in new LocationClasses().ToArray().Where(lc => lc.Code != LocationClasses.Codes.TCL))
			{
				locationType.WLT_LocationClass = locType.Code;
				locationType.WLT_TemperatureUnit = Enterprise.Core.Constants.Temperature.Centigrade; // Readonly and reset on change, no point manually running validation in production code
				AssertHasError(locationType.WLT_TemperatureUnitInfo, expectedMessage);
			}
		}

		#endregion

		#region TestCheckWLT_TemperatureUnit_TCL

		public void TestCheckWLT_TemperatureUnit_TCL()
		{
			var expectedMessageShouldBeSet = "Temperature Unit should be set for this location class.";
			var expectedMessageInvalid = "Enter a valid Temperature Unit.";

			var locationType = Helper.CreateLocationType("TSL", LocationClasses.Codes.TCL);
			locationType.WLT_TemperatureUnit = string.Empty;
			AssertHasError(locationType.WLT_TemperatureUnitInfo, expectedMessageShouldBeSet);

			locationType.WLT_TemperatureUnit = Enterprise.Core.Constants.Temperature.Centigrade;
			AssertNoError(locationType.WLT_TemperatureUnitInfo, expectedMessageShouldBeSet);
			AssertNoError(locationType.WLT_TemperatureUnitInfo, expectedMessageInvalid);

			locationType.WLT_TemperatureUnit = "Z";
			AssertHasError(locationType.WLT_TemperatureUnitInfo, expectedMessageInvalid);
		}

		#endregion

		#region TestCheckWLT_IsPalletNeutral

		public void TestCheckWLT_IsPalletNeutral_DDL() => TestCheckWLT_IsPalletNeutral(LocationClasses.Codes.DDL);
		public void TestCheckWLT_IsPalletNeutral_PST() => TestCheckWLT_IsPalletNeutral(LocationClasses.Codes.PST);
		public void TestCheckWLT_IsPalletNeutral_CON() => TestCheckWLT_IsPalletNeutral(LocationClasses.Codes.CON);

		void TestCheckWLT_IsPalletNeutral(string locationClass)
		{
			var expectedMessage = $"Pallet ID Neutral cannot be checked for Location Class: {locationClass}.";
			var locationType = Factory.New<WhsLocationType>();
			locationType.WLT_LocationClass = locationClass;

			locationType.WLT_IsPalletIDNeutral = true;
			AssertHasError(locationType.WLT_IsPalletIDNeutralInfo, expectedMessage);

			locationType.WLT_IsPalletIDNeutral = false;
			AssertNoError(locationType.WLT_IsPalletIDNeutralInfo, expectedMessage);
		}

		public void TestCheckWLT_IsPalletIDNeutral_FIX()
		{
			var expectedMessage = "Pallet ID Neutral cannot be checked for Location Class FIX when Retain Pallet IDs In Fixed Pick Faces is not checked.";
			var locationType = Factory.New<WhsLocationType>();
			locationType.WLT_LocationClass = LocationClasses.Codes.FIX;

			locationType.WLT_RetainPalletIDsInFixedPickFaces = true;
			locationType.WLT_IsPalletIDNeutral = false;
			AssertNoError(locationType.WLT_IsPalletIDNeutralInfo, expectedMessage);

			locationType.WLT_RetainPalletIDsInFixedPickFaces = false;
			locationType.WLT_IsPalletIDNeutral = false;
			AssertNoError(locationType.WLT_IsPalletIDNeutralInfo, expectedMessage);

			locationType.WLT_RetainPalletIDsInFixedPickFaces = false;
			locationType.WLT_IsPalletIDNeutral = true;
			AssertHasError(locationType.WLT_IsPalletIDNeutralInfo, expectedMessage);

			locationType.WLT_RetainPalletIDsInFixedPickFaces = true;
			locationType.WLT_IsPalletIDNeutral = true;
			AssertNoError(locationType.WLT_IsPalletIDNeutralInfo, expectedMessage);
		}

		#endregion

		#region TestCheckWLT_RetainPalletIDsInFixedPickFaces

		public void TestCheckWLT_RetainPalletIDsInFixedPickFaces_CON() => TestCheckWLT_RetainPalletIDsInFixedPickFaces(LocationClasses.Codes.CON);
		public void TestCheckWLT_RetainPalletIDsInFixedPickFaces_DDL() => TestCheckWLT_RetainPalletIDsInFixedPickFaces(LocationClasses.Codes.DDL);
		public void TestCheckWLT_RetainPalletIDsInFixedPickFaces_DPF() => TestCheckWLT_RetainPalletIDsInFixedPickFaces(LocationClasses.Codes.DPF);
		public void TestCheckWLT_RetainPalletIDsInFixedPickFaces_HPL() => TestCheckWLT_RetainPalletIDsInFixedPickFaces(LocationClasses.Codes.HPL);
		public void TestCheckWLT_RetainPalletIDsInFixedPickFaces_NOR() => TestCheckWLT_RetainPalletIDsInFixedPickFaces(LocationClasses.Codes.NOR);
		public void TestCheckWLT_RetainPalletIDsInFixedPickFaces_PST() => TestCheckWLT_RetainPalletIDsInFixedPickFaces(LocationClasses.Codes.PST);
		public void TestCheckWLT_RetainPalletIDsInFixedPickFaces_TCL() => TestCheckWLT_RetainPalletIDsInFixedPickFaces(LocationClasses.Codes.TCL);

		void TestCheckWLT_RetainPalletIDsInFixedPickFaces(string locationClass)
		{
			var expectedMessage = $"Retain Pallet IDs In Fixed Pick Faces cannot be checked for Location Class: {locationClass}.";
			var locationType = Factory.New<WhsLocationType>();
			locationType.WLT_LocationClass = locationClass;

			locationType.WLT_RetainPalletIDsInFixedPickFaces = true;
			AssertHasError(locationType.WLT_RetainPalletIDsInFixedPickFacesInfo, expectedMessage);

			locationType.WLT_RetainPalletIDsInFixedPickFaces = false;
			AssertNoError(locationType.WLT_RetainPalletIDsInFixedPickFacesInfo, expectedMessage);
		}

		public void TestCheckWLT_RetainPalletIDsInFixedPickFaces_FIX()
		{
			var locationType = Factory.New<WhsLocationType>();
			locationType.WLT_LocationClass = LocationClasses.Codes.FIX;

			locationType.WLT_RetainPalletIDsInFixedPickFaces = true;
			AssertNoErrors(locationType.WLT_RetainPalletIDsInFixedPickFacesInfo);

			locationType.WLT_RetainPalletIDsInFixedPickFaces = false;
			AssertNoErrors(locationType.WLT_RetainPalletIDsInFixedPickFacesInfo);
		}

		#endregion

		#region TestCheckWLT_DefaultCycleCountGranularity

		string[] UnsupportedCycleCountLocations
			=> new[] { LocationClasses.Codes.TCL, LocationClasses.Codes.DDL, LocationClasses.Codes.PST, LocationClasses.Codes.CON };

		public void TestCheckWLT_DefaultCycleCountGranularity_DoesNotSupportGranularities()
		{
			var locationType = Factory.New<WhsLocationType>();

			foreach (var locationClass in UnsupportedCycleCountLocations)
			{
				CheckCycleCountGranularityErrorForLocationClass(locationType, locationClass, string.Empty, false);
				var granularities = new CycleCountGranularities().ToArray();
				granularities.ForEach(g => CheckCycleCountGranularityErrorForLocationClass(locationType, locationClass, g.Code, shouldThrowError: true));
			}
		}

		public void TestCheckWLT_DefaultCycleCountGranularity_SupportsGranularities()
		{
			var locationType = Factory.New<WhsLocationType>();
			var locationClasses = new LocationClasses();

			foreach (var locationClass in locationClasses.GetAllCodes().Except(UnsupportedCycleCountLocations))
			{
				CheckCycleCountGranularityErrorForLocationClass(locationType, locationClass, string.Empty, true);
				var granularities = new CycleCountGranularities().ToArray();
				granularities.ForEach(g => CheckCycleCountGranularityErrorForLocationClass(locationType, locationClass, g.Code, shouldThrowError: false));
			}
		}

		void CheckCycleCountGranularityErrorForLocationClass(WhsLocationType locationType, string locationClass, string cycleCountGranularity, bool shouldThrowError)
		{
			locationType.WLT_LocationClass = locationClass;
			locationType.WLT_DefaultCycleCountGranularity = cycleCountGranularity;

			if (shouldThrowError)
			{
				var locationClassDescription = new LocationClasses().GetDescriptionFromCode(locationClass);
				var expectedMessage = $"{locationClassDescription} types cannot have a Default Cycle Count Granularity specified.";
				AssertHasErrors($"WLT_LocationClass {locationClass} and WLT_DefaultCycleCountGranularity {cycleCountGranularity} should have the error '{expectedMessage}'.",
					locationType.WLT_DefaultCycleCountGranularityInfo);
			}
			else
			{
				AssertNoErrors($"WLT_LocationClass {locationClass} and WLT_DefaultCycleCountGranularity {cycleCountGranularity} should not have any errors.",
					locationType.WLT_DefaultCycleCountGranularityInfo);
			}
		}

		#endregion
	}
}
