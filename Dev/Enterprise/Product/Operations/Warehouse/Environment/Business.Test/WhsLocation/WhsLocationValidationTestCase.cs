using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.NUnit;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class WhsLocationValidationTestCase : WhsBusinessObjectValidationTestCase
	{
		#region TestValidateWLV_PickPathSequence

		public void TestValidateWLV_PickPathSequence()
		{
			var location = Factory.New<WhsLocation>();
			AssertNoErrors("Precondition:", location.WLV_PickPathSequenceInfo);

			location.WLV_PickPathSequence = -1;
			AssertHasError(location.WLV_PickPathSequenceInfo, "Pick Path Sequence cannot be negative.");

			location.WLV_PickPathSequence = 1;
			AssertNoErrors(location.WLV_PickPathSequenceInfo);
		}

		#endregion

		#region TestValidateWLV_WLT_LocationType

		public void TestValidateWLV_WLT_LocationType()
		{
			var row = Factory.New<WhsRow>();
			var locn = row.Locations.AddNew();
			locn.Validation.ValidateWLV_WLT_LocationType();
			AssertHasErrors(locn.WLV_WLT_LocationTypeInfo);

			locn.WLV_WLT_LocationType = ZGuid.Invalid;
			AssertHasErrors(locn.WLV_WLT_LocationTypeInfo);

			var validLocationType = Helper.CreateLocationType("TT1", "Test", false, 0, LocationClasses.Codes.NOR);
			locn.WLV_WLT_LocationType = validLocationType.PK;
			AssertNoErrors(locn.WLV_WLT_LocationTypeInfo);
		}

		public void TestValidateWL_WLT_LocationType_CanNotChangeIfStockExists()
		{
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var orgPK = iHelper.CreateClient("WHS1TST");
			var part = iHelper.CreateProduct(orgPK, "P1");

			var whs = Helper.CreateWarehouse("WHS", "A", 2, 1);
			Factory.Save();

			var location1 = whs.FindLocation("A-1");
			var location2 = whs.FindLocation("A-2");
			Factory.Save();

			var receivePK = iHelper.CreateWhsReceive(orgPK, whs.PK, "1", Notify);
			iHelper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A-1");
			iHelper.FinaliseDocket(receivePK);
			Factory.Save();

			var order = iHelper.CreateWhsOrder(orgPK, whs.PK, orgPK, "O1");
			iHelper.CreateWhsOrderLine(order.PK, part.PK, 10m);

			AssertEquals("Precondition", "RNO", location1.LocationType.WLT_Code);
			AssertEquals("Precondition", LocationClasses.Codes.NOR, location1.LocationType.WLT_LocationClass);
			AssertEquals("Precondition", "RNO", location2.LocationType.WLT_Code);

			var locationTypeNorPk = location1.LocationType.PK;
			var locationTypeFIX = Helper.CreateLocationType("AAA", "AAA Test", false, 1, LocationClasses.Codes.FIX);

			location1.WLV_WLT_LocationType = locationTypeFIX.PK;
			location2.WLV_WLT_LocationType = locationTypeFIX.PK;

			AssertHasError("Should have an error as there is stock in this location.", location1.WLV_WLT_LocationTypeInfo, "Cannot change Location Type as there is Existing or Pending Inventory.");
			AssertNoErrors("Should have no errors as there is *no* stock in this location.", location2.WLV_WLT_LocationTypeInfo);

			location1.WLV_WLT_LocationType = locationTypeNorPk;
			AssertNoErrors(location1.WLV_WLT_LocationTypeInfo);

			var pick = iHelper.CreateWhsPick(new[] { order.PK });
			iHelper.FinaliseDocket(order.PK);
			iHelper.FinalisePick(pick);
			Factory.Save();

			location1.WLV_WLT_LocationType = locationTypeFIX.PK;
			AssertNoErrors("Should have no errors as there is stock in this location.", location1.WLV_WLT_LocationTypeInfo);
			AssertNoExceptionThrown("No triggers should prevent saving changes to location.", Factory.Save);
		}

		public void TestValidateWL_WLT_LocationType_WithStock_AllowChange_NORtoHPL()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var location = data.Whs1.FindLocation("A-1");
			var locationType_NOR = Helper.CreateLocationType("NOR", "NOR");
			var locationType_HPL = Helper.CreateLocationType("HPL", "HPL");
			location.WLV_WLT_LocationType = locationType_NOR.PK;
			Factory.Save();

			var receive = iHelper.CreateWhsReceiveWithInventory(data.Org1.PK, data.Whs1.PK, "R1", data.Part1.PK, 10m);
			iHelper.FinaliseDocket(receive.PK);
			Factory.Save();

			AssertEquals("Precondition: Expected location to be of type 'NOR'.", locationType_NOR, location.LocationType);
			location.WLV_WLT_LocationType = locationType_HPL.PK;
			AssertNoErrors("Should have no errors as changing types from NOR to HPL is allowed.", location.WLV_WLT_LocationTypeInfo);
			Factory.Save();

			AssertEquals("Expected location to be of type 'HPL'.", locationType_HPL, location.LocationType);
		}

		public void TestValidateWL_WLT_LocationType_WithStock_AllowChange_HPLtoNOR()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var location = data.Whs1.FindLocation("A-1");
			var locationType_NOR = Helper.CreateLocationType("NOR", "NOR");
			var locationType_HPL = Helper.CreateLocationType("HPL", "HPL");
			location.WLV_WLT_LocationType = locationType_HPL.PK;
			Factory.Save();

			var receive = iHelper.CreateWhsReceiveWithInventory(data.Org1.PK, data.Whs1.PK, "R1", data.Part1.PK, 10m);
			iHelper.FinaliseDocket(receive.PK);
			Factory.Save();

			AssertEquals("Precondition: Expected location to be of type 'HPL'.", locationType_HPL, location.LocationType);
			location.WLV_WLT_LocationType = locationType_NOR.PK;
			AssertNoErrors("Should have no errors as changing types from HPL to NOR is allowed.", location.WLV_WLT_LocationTypeInfo);
			Factory.Save();

			AssertEquals("Expected location to be of type 'NOR'.", locationType_NOR, location.LocationType);
		}

		public void TestValidateWL_WLT_LocationType_CannotChangeFromDDLIfUsedAsDefaultInboundDockDoorByWarehouse()
		{
			var whs = Helper.CreateWarehouse("W1", "A", 2, 1);
			Factory.Save();

			var locationA1 = whs.FindLocation("A-1");
			var locationA2 = whs.FindLocation("A-2");

			var ddlLocationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_LocationClass, "DDL"));
			locationA1.WLV_WLT_LocationType = ddlLocationType.PK;
			locationA2.WLV_WLT_LocationType = ddlLocationType.PK;
			Assert("Precondition:", locationA1.IsDockDoorLocation);
			Assert("Precondition:", locationA2.IsDockDoorLocation);

			whs.WW_DefaultInboundDockDoor = locationA1.PK;
			AssertNoErrors(locationA1);

			var newLocationType = Helper.CreateLocationType("AAA", "AAA Test", false, 0, LocationClasses.Codes.NOR);
			locationA1.WLV_WLT_LocationType = newLocationType.PK;
			AssertHasError(locationA1.WLV_WLT_LocationTypeInfo, WhsLocationViewValidation.ErrorChangingTypeFromDockDoorIfUsedByWarehouseAsDefaultDockDoor(WhsLocationViewValidation.Inbound));
			AssertNoErrors(locationA2);

			locationA2.WLV_WLT_LocationType = newLocationType.PK;
			AssertNoErrors("It is ok to change type of locationA2 as it is not pointed by warehouse.", locationA2);
		}

		public void TestValidateWL_WLT_LocationType_CannotChangeFromDDLIfUsedAsDefaultOutboundDockDoorByWarehouse()
		{
			var whs = Helper.CreateWarehouse("W1", "A", 2, 1);
			Factory.Save();

			var locationA1 = whs.FindLocation("A-1");
			var locationA2 = whs.FindLocation("A-2");

			var ddlLocationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_LocationClass, "DDL"));
			locationA1.WLV_WLT_LocationType = ddlLocationType.PK;
			locationA2.WLV_WLT_LocationType = ddlLocationType.PK;
			Assert("Precondition:", locationA1.IsDockDoorLocation);
			Assert("Precondition:", locationA2.IsDockDoorLocation);

			whs.WW_DefaultOutboundDockDoor = locationA1.PK;
			AssertNoErrors(locationA1);

			var newLocationType = Helper.CreateLocationType("AAA", "AAA Test", false, 0, LocationClasses.Codes.NOR);
			locationA1.WLV_WLT_LocationType = newLocationType.PK;
			AssertHasError(locationA1.WLV_WLT_LocationTypeInfo, WhsLocationViewValidation.ErrorChangingTypeFromDockDoorIfUsedByWarehouseAsDefaultDockDoor(WhsLocationViewValidation.Outbound));
			AssertNoErrors(locationA2);

			locationA2.WLV_WLT_LocationType = newLocationType.PK;
			AssertNoErrors("It is ok to change type of locationA2 as it is not pointed by warehouse.", locationA2);
		}

		public void TestValidateWL_WLT_LocationType_CanNotChangeIfPartOfPickFace()
		{
			var expectedMessage = "A Location used in a Pick Face cannot change its Location Type, unless the new Location Type is also of class FIX";

			var warehouse = Helper.CreateWarehouse("WHT");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "PF", 2, 1);

			var fixLocationUsed = row.Locations[0];
			var fixLocationNotUsed = row.Locations[1];
			var locationTypePF1 = Helper.CreateLocationType("PF1", "Pick Face One", false, 1, LocationClasses.Codes.FIX);
			var locationTypePF2 = Helper.CreateLocationType("PF2", "Pick Face Two", false, 1, LocationClasses.Codes.FIX);
			var locationTypeTB1 = Helper.CreateLocationType("TB1", "TB1 Test", false, 0, LocationClasses.Codes.NOR);
			fixLocationUsed.WLV_WLT_LocationType = locationTypePF1.PK;
			fixLocationNotUsed.WLV_WLT_LocationType = locationTypePF2.PK;

			var org = Helper.CreateClient("CLT001");
			var part = Helper.CreateProduct(org, "PDT001");
			var pickFace = Helper.CreateProductPickFace(part, org, fixLocationUsed);

			Factory.Save();

			fixLocationNotUsed.WLV_WLT_LocationType = locationTypeTB1.PK;
			AssertNoErrors("It is ok to change type of a location that is not used in a Pick Face.", fixLocationNotUsed);

			fixLocationNotUsed.WLV_WLT_LocationType = locationTypePF1.PK;
			AssertNoErrors("It is ok to change type of a location that is not used in a Pick Face.", fixLocationNotUsed);

			fixLocationUsed.WLV_WLT_LocationType = locationTypeTB1.PK;
			AssertHasError("Should have an error as the new location type does not have a class of FIX and the location is part of a Pick Face.", fixLocationUsed.WLV_WLT_LocationTypeInfo, expectedMessage);

			fixLocationUsed.WLV_WLT_LocationType = locationTypePF2.PK;
			AssertNoErrors("It is ok to change type of a location that is in a Pick Face if the new type has a class of FIX.", fixLocationUsed);
		}

		public void TestValidateWL_WLT_LocationType_WithStock_NoAssignedPickfaces_AllowChange_FIXtoNOR()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var location = data.Whs1.FindLocation("A-1");
			var locationType_FIX = Helper.CreateLocationType("FIX", "FIX Test", false, 1, LocationClasses.Codes.FIX);
			var locationType_NOR = Helper.CreateLocationType("NOR", "NOR");
			location.WLV_WLT_LocationType = locationType_FIX.PK;

			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, location);
			var receive = iHelper.CreateWhsReceiveWithInventory(data.Org1.PK, data.Whs1.PK, "R1", data.Part1.PK, 10m);
			iHelper.FinaliseDocket(receive.PK);
			Factory.Save();

			// Unassign pick face
			pickFace.Delete();
			Factory.Save();

			AssertEquals("Precondition: Expected location to be of type 'FIX'.", locationType_FIX, location.LocationType);
			location.WLV_WLT_LocationType = locationType_NOR.PK;
			AssertNoErrors("Changing types from FIX to NOR should be allowed since no pick faces are assigned to location.", location.WLV_WLT_LocationTypeInfo);
			Factory.Save();

			AssertEquals("Expected location to be of type 'NOR'.", locationType_NOR, location.LocationType);
		}

		public void TestValidateWL_WLT_LocationType_WithStock_NoAssignedPickfaces_AllowChange_FIXtoHPL()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var location = data.Whs1.FindLocation("A-1");
			var locationType_FIX = Helper.CreateLocationType("FIX", "FIX Test", false, 1, LocationClasses.Codes.FIX);
			var locationType_HPL = Helper.CreateLocationType("HPL", "HPL");
			location.WLV_WLT_LocationType = locationType_FIX.PK;

			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, location);
			var receive = iHelper.CreateWhsReceiveWithInventory(data.Org1.PK, data.Whs1.PK, "R1", data.Part1.PK, 10m);
			iHelper.FinaliseDocket(receive.PK);
			Factory.Save();

			// Unassign pick face
			pickFace.Delete();
			Factory.Save();

			AssertEquals("Precondition: Expected location to be of type 'FIX'.", locationType_FIX, location.LocationType);
			location.WLV_WLT_LocationType = locationType_HPL.PK;
			AssertNoErrors("Changing types from FIX to HPL should be allowed since no pick faces are assigned to location.", location.WLV_WLT_LocationTypeInfo);
			Factory.Save();

			AssertEquals("Expected location to be of type 'HPL'.", locationType_HPL, location.LocationType);
		}

		public void TestValidateWL_WLT_LocationType_WithStock_NoAssignedPickfaces_PreventChange_FIXtoDDL()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var location = data.Whs1.FindLocation("A-1");
			var locationType_FIX = Helper.CreateLocationType("FIX", "FIX Test", false, 1, LocationClasses.Codes.FIX);
			var locationType_DDL = Helper.CreateLocationType("DDL", "DDL");
			location.WLV_WLT_LocationType = locationType_FIX.PK;

			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, location);
			var receive = iHelper.CreateWhsReceiveWithInventory(data.Org1.PK, data.Whs1.PK, "R1", data.Part1.PK, 10m);
			iHelper.FinaliseDocket(receive.PK);
			Factory.Save();

			// Unassign pick face
			pickFace.Delete();
			Factory.Save();

			AssertEquals("Precondition: Expected location to be of type 'FIX'.", locationType_FIX, location.LocationType);
			location.WLV_WLT_LocationType = locationType_DDL.PK;
			AssertHasError("Should have an error as there is stock in this location.", location.WLV_WLT_LocationTypeInfo, "Cannot change Location Type as there is Existing or Pending Inventory.");
		}

		public void TestValidateWL_WLT_LocationType_WithStock_AssignedPickfaces_PreventChange_FIXtoNOR()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var location = data.Whs1.FindLocation("A-1");
			var locationType_FIX = Helper.CreateLocationType("FIX", "FIX Test", false, 1, LocationClasses.Codes.FIX);
			var locationType_NOR = Helper.CreateLocationType("NOR", "NOR");
			location.WLV_WLT_LocationType = locationType_FIX.PK;

			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, location);
			var receive = iHelper.CreateWhsReceiveWithInventory(data.Org1.PK, data.Whs1.PK, "R1", data.Part1.PK, 10m);
			iHelper.FinaliseDocket(receive.PK);
			Factory.Save();

			AssertEquals("Precondition: Expected PickFace to be assigned to location.", pickFace.Location, location);
			AssertEquals("Precondition: Expected location to be of type 'FIX'.", locationType_FIX, location.LocationType);
			location.WLV_WLT_LocationType = locationType_NOR.PK;
			AssertHasError("Should have an error as this location is assigned to a pick face.", location.WLV_WLT_LocationTypeInfo, "A Location used in a Pick Face cannot change its Location Type, unless the new Location Type is also of class FIX");
		}

		public void TestValidateWL_WLT_LocationType_WithStock_AssignedPickfaces_PreventChange_FIXtoHPL()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var location = data.Whs1.FindLocation("A-1");
			var locationType_FIX = Helper.CreateLocationType("FIX", "FIX Test", false, 1, LocationClasses.Codes.FIX);
			var locationType_HPL = Helper.CreateLocationType("HPL", "HPL");
			location.WLV_WLT_LocationType = locationType_FIX.PK;

			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, location);
			var receive = iHelper.CreateWhsReceiveWithInventory(data.Org1.PK, data.Whs1.PK, "R1", data.Part1.PK, 10m);
			iHelper.FinaliseDocket(receive.PK);
			Factory.Save();

			AssertEquals("Precondition: Expected PickFace to be assigned to location.", pickFace.Location, location);
			AssertEquals("Precondition: Expected location to be of type 'FIX'.", locationType_FIX, location.LocationType);
			location.WLV_WLT_LocationType = locationType_HPL.PK;
			AssertHasError("Should have an error as this location is assigned to a pick face.", location.WLV_WLT_LocationTypeInfo, "A Location used in a Pick Face cannot change its Location Type, unless the new Location Type is also of class FIX");
		}

		public void TestValidateWL_WLT_LocationType_WithStock_AllowChange_DDLtoDDL()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var location = data.Whs1.FindLocation("A-1");
			var locationType_DDL = Helper.CreateLocationType("DDL", "DDL");
			location.WLV_WLT_LocationType = locationType_DDL.PK;

			var receive = iHelper.CreateWhsReceiveWithInventory(data.Org1.PK, data.Whs1.PK, "R1", data.Part1.PK, 10m);
			iHelper.FinaliseDocket(receive.PK);
			Factory.Save();

			AssertEquals("Precondition: Expected location to be of type 'DDL'.", locationType_DDL, location.LocationType);
			location.WLV_WLT_LocationType = locationType_DDL.PK;
			Factory.Save();
			AssertEquals("Expected location to be of type 'DDL'.", locationType_DDL, location.LocationType);

			var locationType_DDL2 = Helper.CreateLocationType("DL2", "DDL");
			location.WLV_WLT_LocationType = locationType_DDL2.PK;
			Factory.Save();
			AssertEquals("Expected location to be of type 'DDL' (the Type is different but the class is the same, this is allowed atm).", locationType_DDL2, location.LocationType);
		}

		public void TestValidateWL_WLT_LocationType_WithStock_PreventChange_FIXtoFIX()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var location = data.Whs1.FindLocation("A-1");
			var locationType_FIX1 = Helper.CreateLocationType("FX1", "FIX1 Test", false, 1, LocationClasses.Codes.FIX);
			var locationType_FIX2 = Helper.CreateLocationType("FX2", "FIX2 Test", false, 1, LocationClasses.Codes.FIX);
			location.WLV_WLT_LocationType = locationType_FIX1.PK;

			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, location);
			var receive = iHelper.CreateWhsReceiveWithInventory(data.Org1.PK, data.Whs1.PK, "R1", data.Part1.PK, 10m);
			Factory.Save();

			pickFace.Delete();
			Factory.Save();

			AssertEquals("Precondition: Expected location with a class of 'FIX'.", LocationClasses.Codes.FIX, location.LocationType.WLT_LocationClass);
			location.WLV_WLT_LocationType = locationType_FIX2.PK;
			AssertHasError("Should have an error as changing types from FIX to FIX is not allowed.", location.WLV_WLT_LocationTypeInfo, "Cannot change Location Type as there is Existing or Pending Inventory.");
		}

		public void TestValidateWL_WLT_LocationType_NoStock_AllowChange_FIXtoDDL()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var location = data.Whs1.FindLocation("A-1");
			var locationType_FIX = Helper.CreateLocationType("FIX", "FIX Test", false, 1, LocationClasses.Codes.FIX);
			var locationType_DDL = Helper.CreateLocationType("DDL", "DDL");
			location.WLV_WLT_LocationType = locationType_FIX.PK;

			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, location);
			var receive = iHelper.CreateWhsReceiveWithInventory(data.Org1.PK, data.Whs1.PK, "R1", data.Part1.PK, 10m);
			iHelper.FinaliseDocket(receive.PK);
			Factory.Save();

			pickFace.Delete();

			var orderPK = iHelper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, "O1", Notify);
			var orderLinePK = iHelper.CreateWhsOrderLine(orderPK, data.Part1.PK, 10m);
			var pickPK = iHelper.CreateWhsPick(new[] { orderPK });
			iHelper.FinaliseDocket(orderPK);
			iHelper.FinalisePick(pickPK);
			Factory.Save();

			AssertEquals("Precondition: Expected location to be of type 'FIX'.", locationType_FIX, location.LocationType);
			location.WLV_WLT_LocationType = locationType_DDL.PK;
			Factory.Save();

			AssertEquals("Expected location to be of type 'DDL'.", locationType_DDL, location.LocationType);
		}

		public void TestValidateWL_WLT_LocationType_PreventConcurrentPickFaceUpdate()
		{
			var factory1 = Factory;
			var data = new EnvTestDataSimpleEnvironment(factory1, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var locationType_FIX = Helper.CreateLocationType("FIX", "FIX Test", false, 1, LocationClasses.Codes.FIX);
			var locationType_NOR = Helper.CreateLocationType("NOR", "NOR");
			location.WLV_WLT_LocationType = locationType_FIX.PK;
			factory1.Save();

			location.WLV_WLT_LocationType = locationType_NOR.PK;
			AssertNoErrors("Precondition.", location.WLV_WLT_LocationTypeInfo);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var iHelper_factory2 = WhsTransactionTestHelperCreator.GetNewHelper(factory2);
			iHelper_factory2.CreatePickface(data.Org1.PK, data.Whs1.PK, data.Part1.PK, location.PK);
			factory2.Save();

			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "A location used in a Pick Face must have a location type that has a class of FIX", true), "Expected to throw an exception since a pick face was assigned to the location in a different instance.");
		}

		public void TestValidateWL_WLT_LocationType_DynamicLocationsOnlySetInDynamicAreas()
		{
			var warehouse = Helper.CreateWarehouse("TST");

			var dpfArea = Factory.New<WhsArea>();
			dpfArea.WA_WW_Whs = warehouse.PK;
			dpfArea.WA_AreaType = AreaTypes.Codes.DynamicPickFace;
			dpfArea.WA_IsPickingArea = true;

			var bonArea = Factory.New<WhsArea>();
			bonArea.WA_WW_Whs = warehouse.PK;
			bonArea.WA_AreaType = AreaTypes.Codes.Bonded;
			bonArea.WA_IsPickingArea = true;

			var ddaArea = Factory.New<WhsArea>();
			ddaArea.WA_WW_Whs = warehouse.PK;
			ddaArea.WA_AreaType = AreaTypes.Codes.DockDoor;
			ddaArea.WA_IsPickingArea = true;

			var execArea = Factory.New<WhsArea>();
			execArea.WA_WW_Whs = warehouse.PK;
			execArea.WA_AreaType = AreaTypes.Codes.Excise;
			execArea.WA_IsPickingArea = true;

			var freeArea = Factory.New<WhsArea>();
			freeArea.WA_WW_Whs = warehouse.PK;
			freeArea.WA_AreaType = AreaTypes.Codes.FreeStore;
			freeArea.WA_IsPickingArea = true;

			var iprArea = Factory.New<WhsArea>();
			iprArea.WA_WW_Whs = warehouse.PK;
			iprArea.WA_AreaType = AreaTypes.Codes.InwardProcessing;
			iprArea.WA_IsPickingArea = true;
			iprArea.WA_IsPutawayArea = true;

			var dynamicPickFaceLocationType = Factory.New<WhsLocationType>();
			dynamicPickFaceLocationType.WLT_Code = LocationClasses.Codes.DPF;
			dynamicPickFaceLocationType.WLT_IsActive = true;
			dynamicPickFaceLocationType.WLT_Description = LocationClasses.Descriptions.DPF;
			dynamicPickFaceLocationType.WLT_IsSystem = true;
			dynamicPickFaceLocationType.WLT_MaximumNumberOfProducts = 1;
			dynamicPickFaceLocationType.WLT_LocationClass = LocationClasses.Codes.DPF;

			var location = Factory.New<WhsLocation>();
			location.WLV_WW_Whs = warehouse.PK;
			location.WLV_WLT_LocationType = dynamicPickFaceLocationType.PK;

			location.WLV_WA_PickingArea = dpfArea.PK;
			AssertNoErrorContaining("Dynamic Pick Face Locations can be set in Dynamic Pick Face Areas.", location.WLV_WLT_LocationTypeInfo, "Dynamic Pick Face Locations can only be set in Dynamic Pick Face Areas.");

			location.WLV_WA_PickingArea = bonArea.PK;
			AssertHasError("Dynamic Pick Face Locations can NOT be set outside Dynamic Pick Face Areas (Bonded).", location.WLV_WLT_LocationTypeInfo, "Dynamic Pick Face Locations can only be set in Dynamic Pick Face Areas.");

			location.WLV_WA_PickingArea = ddaArea.PK;
			AssertHasError("Dynamic Pick Face Locations can NOT be set outside Dynamic Pick Face Areas (DockDoor).", location.WLV_WLT_LocationTypeInfo, "Dynamic Pick Face Locations can only be set in Dynamic Pick Face Areas.");

			location.WLV_WA_PickingArea = execArea.PK;
			AssertHasError("Dynamic Pick Face Locations can NOT be set outside Dynamic Pick Face Areas (Excise).", location.WLV_WLT_LocationTypeInfo, "Dynamic Pick Face Locations can only be set in Dynamic Pick Face Areas.");

			location.WLV_WA_PickingArea = freeArea.PK;
			AssertHasError("Dynamic Pick Face Locations can NOT be set outside Dynamic Pick Face Areas (FreeStore).", location.WLV_WLT_LocationTypeInfo, "Dynamic Pick Face Locations can only be set in Dynamic Pick Face Areas.");

			location.WLV_WA_PickingArea = iprArea.PK;
			location.WLV_WA_PutawayArea = iprArea.PK;
			AssertHasError("Dynamic Pick Face Locations can NOT be set outside Dynamic Pick Face Areas (InwardProcessing).", location.WLV_WLT_LocationTypeInfo, "Dynamic Pick Face Locations can only be set in Dynamic Pick Face Areas.");
		}

		public void TestValidateWL_WLT_LocationType_DynamicContainsOnlyDynamicLocations()
		{
			var warehouse = Helper.CreateWarehouse("WHS9");

			var area = Factory.New<WhsArea>();
			area.WA_AreaType = AreaTypes.Codes.DynamicPickFace;
			area.WA_IsPickingArea = true;
			area.WA_IsPutawayArea = false;

			var dynamicPickFaceLocationType = Factory.New<WhsLocationType>();
			dynamicPickFaceLocationType.WLT_Code = LocationClasses.Codes.DPF;
			dynamicPickFaceLocationType.WLT_IsActive = true;
			dynamicPickFaceLocationType.WLT_Description = LocationClasses.Descriptions.DPF;
			dynamicPickFaceLocationType.WLT_IsSystem = true;
			dynamicPickFaceLocationType.WLT_MaximumNumberOfProducts = 1;
			dynamicPickFaceLocationType.WLT_LocationClass = LocationClasses.Codes.DPF;

			var fixLocationType = Factory.New<WhsLocationType>();
			fixLocationType.WLT_Code = LocationClasses.Codes.FIX;
			fixLocationType.WLT_IsActive = true;
			fixLocationType.WLT_Description = LocationClasses.Descriptions.FIX;
			fixLocationType.WLT_IsSystem = true;
			fixLocationType.WLT_MaximumNumberOfProducts = 1;
			fixLocationType.WLT_LocationClass = LocationClasses.Codes.FIX;

			var location = Factory.New<WhsLocation>();
			location.WLV_WLT_LocationType = dynamicPickFaceLocationType.PK;
			location.WLV_WA_PickingArea = area.PK;

			AssertNoErrors("Precondition: Dynamic loc has No Errors", location.WLV_WLT_LocationTypeInfo);
			AssertNoErrorContaining(location.WLV_WA_PickingAreaInfo, "Dynamic Pick Face Areas must only have Dynamic Pick Face Locations.");

			location.WLV_WLT_LocationType = fixLocationType.PK;

			AssertHasError(location.WLV_WLT_LocationTypeInfo, "Dynamic Pick Face Areas must have Dynamic Pick Face Locations only.");
			AssertHasError(location.WLV_WA_PickingAreaInfo, "Dynamic Pick Face Areas must only have Dynamic Pick Face Locations.");
		}

		public void TestValidateWL_WLT_LocationType_CannotUnassignDynamicAreaIfNoOtherLocations()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC");
			dynamicArea.WA_AreaType = AreaTypes.Codes.DynamicPickFace;
			dynamicArea.WA_IsPickingArea = true;
			dynamicArea.WA_IsPutawayArea = false;

			var fixedArea = Helper.CreateArea(data.Whs1, "FIXED");
			fixedArea.WA_AreaType = AreaTypes.Codes.FreeStore;
			fixedArea.WA_IsPickingArea = true;
			fixedArea.WA_IsPutawayArea = false;

			var dynamicPickFaceLocationType = Helper.CreateLocationType("LC1", LocationClasses.Codes.DPF);
			var fixLocationType = Helper.CreateLocationType("LC2", "LC2 Test", false, 1, LocationClasses.Codes.FIX);

			var dynamicLocation = data.Whs1.FindLocation("A-1");
			dynamicLocation.WLV_WLT_LocationType = dynamicPickFaceLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			AssertNoErrors("Precondition: Dynamic location has no errors", dynamicLocation.WLV_WLT_LocationTypeInfo);

			Factory.Save();

			var transactionHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var paramsByWhsAndClient = transactionHelper.CreateProductParamsByWhsAndClient(data.Part1.PK, data.Org1.PK, data.Whs1.PK, 2m, 10m, 3m, "CNT", 0);
			paramsByWhsAndClient[WhsProductParamsByWhsAndClientSchema.W3_WA_DynamicPickFaceArea] = dynamicArea.PK;

			Factory.Save();

			dynamicLocation.WLV_WA_PickingArea = fixedArea.PK;
			AssertHasError(dynamicLocation.WLV_WA_PickingAreaInfo, string.Format("Cannot change pick area from {0} to {1}, as previous dynamic pick area {0} would contain no locations.", dynamicArea.WA_NameMultilingual, fixedArea.WA_NameMultilingual));

			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			AssertNoErrors("Error should be removed", dynamicLocation.WLV_WLT_LocationTypeInfo);
		}

		public void TestValidateWL_WLT_LocationType_CanUnassignDynamicAreaIfOtherLocationsExist()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 2);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC");
			dynamicArea.WA_AreaType = AreaTypes.Codes.DynamicPickFace;
			dynamicArea.WA_IsPickingArea = true;
			dynamicArea.WA_IsPutawayArea = false;

			var dynamicArea2 = Helper.CreateArea(data.Whs1, "DYNAMIC2");
			dynamicArea2.WA_AreaType = AreaTypes.Codes.DynamicPickFace;
			dynamicArea2.WA_IsPickingArea = true;
			dynamicArea2.WA_IsPutawayArea = false;

			var dynamicPickFaceLocationType = Helper.CreateLocationType("LC1", LocationClasses.Codes.DPF);
			var fixLocationType = Helper.CreateLocationType("LC2", "LC2 Test", false, 1, LocationClasses.Codes.FIX);

			var dynamicLocation1 = data.Whs1.FindLocation("A-1");
			dynamicLocation1.WLV_WLT_LocationType = dynamicPickFaceLocationType.PK;
			dynamicLocation1.WLV_WA_PickingArea = dynamicArea.PK;
			var dynamicLocation2 = data.Whs1.FindLocation("A-2");
			dynamicLocation2.WLV_WLT_LocationType = dynamicPickFaceLocationType.PK;
			dynamicLocation2.WLV_WA_PickingArea = dynamicArea.PK;

			AssertNoErrors("Precondition: Dynamic location 1 has no errors", dynamicLocation1.WLV_WLT_LocationTypeInfo);
			AssertNoErrors("Precondition: Dynamic location 2 has no errors", dynamicLocation2.WLV_WLT_LocationTypeInfo);

			Factory.Save();

			var transactionHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var paramsByWhsAndClient = transactionHelper.CreateProductParamsByWhsAndClient(data.Part1.PK, data.Org1.PK, data.Whs1.PK, 2m, 10m, 3m, "CNT", 0);
			paramsByWhsAndClient[WhsProductParamsByWhsAndClientSchema.W3_WA_DynamicPickFaceArea] = dynamicArea.PK;

			Factory.Save();

			dynamicLocation1.WLV_WA_PickingArea = dynamicArea2.PK;
			AssertNoErrors("Location 1 can be unassigned from dynamic area as other locations are still assigned", dynamicLocation1.WLV_WLT_LocationTypeInfo);

			dynamicLocation2.WLV_WA_PickingArea = dynamicArea2.PK;
			AssertHasError(dynamicLocation2.WLV_WA_PickingAreaInfo, string.Format("Cannot change pick area from {0} to {1}, as previous dynamic pick area {0} would contain no locations.", dynamicArea.WA_NameMultilingual, dynamicArea2.WA_NameMultilingual));
		}

		public void TestValidateWL_WLT_LocationType_CanUnassignDynamicAreaIfNoOtherLocations_IfAreaAssignedNoProducts()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC");
			dynamicArea.WA_AreaType = AreaTypes.Codes.DynamicPickFace;
			dynamicArea.WA_IsPickingArea = true;
			dynamicArea.WA_IsPutawayArea = false;

			var dynamicArea2 = Helper.CreateArea(data.Whs1, "DYNAMIC2");
			dynamicArea2.WA_AreaType = AreaTypes.Codes.DynamicPickFace;
			dynamicArea2.WA_IsPickingArea = true;
			dynamicArea2.WA_IsPutawayArea = false;

			var dynamicPickFaceLocationType = Helper.CreateLocationType("LC1", LocationClasses.Codes.DPF);
			var fixLocationType = Helper.CreateLocationType("LC2", "LC2 Test", true, 1, LocationClasses.Codes.FIX);

			var dynamicLocation = data.Whs1.FindLocation("A-1");
			dynamicLocation.WLV_WLT_LocationType = dynamicPickFaceLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			AssertNoErrors("Precondition: Dynamic location has no errors", dynamicLocation.WLV_WLT_LocationTypeInfo);

			dynamicLocation.WLV_WA_PickingArea = dynamicArea2.PK;

			AssertNoErrors("Location can be unassigned from dynamic area is area is not used for any products", dynamicLocation.WLV_WLT_LocationTypeInfo);
		}

		public void TestValidateWL_WLT_LocationType_MaxCapacityGreaterThanZero_WLV_MaxWeight()
		{
			TestValidateWL_WLT_LocationType_MaxCapacityGreaterThanZeroCore(location => location.WLV_MaxWeightInfo);
		}

		public void TestValidateWL_WLT_LocationType_MaxCapacityGreaterThanZero_WLV_MaxCubic()
		{
			TestValidateWL_WLT_LocationType_MaxCapacityGreaterThanZeroCore(location => location.WLV_MaxWeightInfo);
		}

		public void TestValidateWL_WLT_LocationType_MaxCapacityGreaterThanZero_WLV_MaxQuantity()
		{
			TestValidateWL_WLT_LocationType_MaxCapacityGreaterThanZeroCore(location => location.WLV_MaxWeightInfo);
		}

		void TestValidateWL_WLT_LocationType_MaxCapacityGreaterThanZeroCore(Func<WhsLocation, ZPropertyInfo> getPropertyInfo)
		{
			var expectedMessage = "The max capacity of Fixed or Dynamic Pick Face Location must be 0.";
			var normalLocationType = Helper.CreateLocationType("AAA", LocationClasses.Codes.NOR);
			var fixedLocationType = Helper.CreateLocationType("AAA", "AAA Test", true, 1, LocationClasses.Codes.FIX);
			var dynamicPickFaceLocationType = Helper.CreateLocationType("AAA", LocationClasses.Codes.DPF);
			var row = Factory.New<WhsRow>();
			var location = row.Locations.AddNew();

			AssertNoError("Precondition:", location.WLV_WLT_LocationTypeInfo, expectedMessage);

			var propertyInfo = getPropertyInfo(location);
			location.SetPropertyValue(propertyInfo.Name, (ZDecimal)5m);
			location.WLV_WLT_LocationType = normalLocationType.PK;
			AssertNoErrors("Should not have error when set Normal Location Type ", location.WLV_WLT_LocationTypeInfo);

			location.WLV_WLT_LocationType = fixedLocationType.PK;
			AssertHasError("Should have error when set Fixed Location Type ", location.WLV_WLT_LocationTypeInfo, expectedMessage);

			location.WLV_WLT_LocationType = normalLocationType.PK;
			AssertNoErrors("Should not have error when set Normal Location Type ", location.WLV_WLT_LocationTypeInfo);

			location.WLV_WLT_LocationType = dynamicPickFaceLocationType.PK;
			AssertHasError("Should have error when set Dynamic PickFace Location Type ", location.WLV_WLT_LocationTypeInfo, expectedMessage);
		}

		public void TestValidateWL_WLT_LocationType_PickFacesCached()
		{
			TestValidateWL_WLT_LocationTypeCacheCore(WhsPickFaceSchema.Constants.TableName, "HasPickFace is cached in the factory.");
		}

		public void TestValidateWL_WLT_LocationType_PickDockDoorLocationsCached()
		{
			TestValidateWL_WLT_LocationTypeCacheCore(WhsPickSchema.Constants.TableName, "HasPick is cached in the factory.");
		}

		public void TestValidateWL_WLT_LocationTypeCacheCore(string tableName, string message)
		{
			var warehouse = Helper.CreateWarehouse("WHT");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "PF", 2, 1);

			var location = row.Locations[0];
			var locationType = Helper.CreateLocationType("PF1", "Pick Face One", false, 1, LocationClasses.Codes.FIX);
			location.WLV_WLT_LocationType = locationType.PK;

			AssertEquals(message, 0, Factory.TableSelects.Count(t => t.TableName == tableName));
		}

		public void TestCheckWL_LocationClass_CannotUseCYOnlyLocationTypeIfWarehouseIsNotCYD()
		{
			var warehouse = Helper.CreateWarehouse("WH1", "A");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "PF", 1, 1);
			row.Locations[0].WLV_WLT_LocationType = Helper.CreateLocationType("123", LocationClasses.Codes.DDL).PK;
			Factory.Save();

			TestCase(LocationClasses.Codes.WSA);
			TestCase(LocationClasses.Codes.WCL);
			TestCase(LocationClasses.Codes.RPR);
			TestCase(LocationClasses.Codes.INS);
			TestCase(LocationClasses.Codes.SUR);

			void TestCase(string locationTypeCode)
			{
				var expectedMessage = $"{locationTypeCode} Location Class cannot be used if the warehouse type is not CYD.";
				var locationType = Helper.CreateLocationType(locationTypeCode, locationTypeCode);

				row.Locations[0].WLV_WLT_LocationType = locationType.PK;
				AssertHasError($"Do not allow setting {locationTypeCode} location class if warehouse type is not CYD", row.Locations[0].WLV_WLT_LocationTypeInfo, expectedMessage);
			}
		}

		public void TestChcekcWL_LocationClass_CanUseTCLIfWarehouseIsCYD()
		{
			var warehouse = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "PF", 1, 1);
			row.Locations[0].WLV_WLT_LocationType = Helper.CreateLocationType("123", LocationClasses.Codes.DDL).PK;
			Factory.Save();

			var locationType = Helper.CreateLocationType("111", LocationClasses.Codes.TCL);

			row.Locations[0].WLV_WLT_LocationType = locationType.PK;
			AssertNoErrors(row.Locations[0].WLV_WLT_LocationTypeInfo);
		}

		public void TestCheckWL_LocationClass_CanUseCYOnlyLocationTypeIfWarehouseIsCYD()
		{
			var warehouse = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "PF", 1, 1);
			row.Locations[0].WLV_WLT_LocationType = Helper.CreateLocationType("123", LocationClasses.Codes.DDL).PK;
			Factory.Save();

			TestCase(LocationClasses.Codes.WSA);
			TestCase(LocationClasses.Codes.WCL);
			TestCase(LocationClasses.Codes.RPR);
			TestCase(LocationClasses.Codes.INS);
			TestCase(LocationClasses.Codes.SUR);

			void TestCase(string locationTypeCode)
			{
				var locationType = Helper.CreateLocationType("111", locationTypeCode);

				row.Locations[0].WLV_WLT_LocationType = locationType.PK;
				AssertNoErrors(row.Locations[0].WLV_WLT_LocationTypeInfo);
			}
		}

		public void TestCheckWL_LocationClass_CannotUseLocationTypesInvalidForCYDIfWarehouseIsCYD()
		{
			var warehouse = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "PF", 1, 1);
			Factory.Save();

			TestCase(LocationClasses.Codes.PST);
			TestCase(LocationClasses.Codes.CON);
			TestCase(LocationClasses.Codes.DDL);
			TestCase(LocationClasses.Codes.FIX);
			TestCase(LocationClasses.Codes.HPL);

			void TestCase(string locationTypeCode)
			{
				var expectedMessage = "This Location Type is not supported for this type of Warehouse.";
				var locationType = Helper.CreateLocationType(locationTypeCode, locationTypeCode);

				row.Locations[0].WLV_WLT_LocationType = locationType.PK;
				AssertHasError($"Do not allow setting {locationTypeCode} location class if warehouse type is CYD", row.Locations[0].WLV_WLT_LocationTypeInfo, expectedMessage);
			}
		}

		public void TestCheckWL_LocationClass_CannotChangeCYDWaitingBayLocationTypeWhileInUse()
		{
			var expectedMessage = "The selected location is currently being used as a waiting bay for Transportation Units (A0001, B0002). Please select another location to continue.";
			var warehouse = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "RW1", 1, 1);
			var wsaLocation = Helper.CreateLocationType("WSA", LocationClasses.Codes.WSA);
			var norLocation = Helper.CreateLocationType("NOR", LocationClasses.Codes.NOR);
			row.Locations[0].WLV_WLT_LocationType = wsaLocation.PK;

			Factory.Save();

			var command = @$"insert into CYDTransportationUnit (YTU_PK, YTU_WW_Yard, YTU_TransportationUnitID, YTU_TransportationReference, YTU_WL_WaitingBayLocation, YTU_GateInTime, YTU_SystemCreateTimeUtc, YTU_SystemLastEditTimeUtc, YTU_SystemCreateUser, YTU_SystemLastEditUser)
                values ('65964610-0494-4740-A927-948CC71D1B68', '{warehouse.PK}', 'YTU0001', 'A0001', '{row.Locations[0].PK}', '2024-09-25', '2024-09-25', '2024-09-25', 'TST', 'TST')";
			Db.Connection.ExecuteNonQuery(command);

			command = @$"insert into CYDTransportationUnit (YTU_PK, YTU_WW_Yard, YTU_TransportationUnitID, YTU_TransportationReference, YTU_WL_WaitingBayLocation, YTU_GateInTime, YTU_SystemCreateTimeUtc, YTU_SystemLastEditTimeUtc, YTU_SystemCreateUser, YTU_SystemLastEditUser)
                values ('905CB4C6-3B1C-4870-87EF-21790654A4BA', '{warehouse.PK}', 'YTU0002', 'B0002', '{row.Locations[0].PK}', '2024-09-25', '2024-09-25', '2024-09-25', 'TST', 'TST')";
			Db.Connection.ExecuteNonQuery(command);

			row.Locations[0].WLV_WLT_LocationType = norLocation.PK;
			AssertHasError("Do not allow change location type if location class is WSA and location is in use.", row.Locations[0].WLV_WLT_LocationTypeInfo, expectedMessage);
		}

		public void TestCheckWL_LocationClass_CanChangeCYDWaitingBayLocationTypeWhileNotInUse()
		{
			var warehouse = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "RW1", 1, 1);
			var wsaLocation = Helper.CreateLocationType("WSA", LocationClasses.Codes.WSA);
			var norLocation = Helper.CreateLocationType("NOR", LocationClasses.Codes.NOR);
			row.Locations[0].WLV_WLT_LocationType = wsaLocation.PK;

			Factory.Save();

			row.Locations[0].WLV_WLT_LocationType = norLocation.PK;
			AssertNoErrors("Allow change location type if location class is WSA and location is not in use.", row.Locations[0].WLV_WLT_LocationTypeInfo);
		}

		public void TestCheckWL_LocationClass_CanChangeCYDWaitingBayLocationTypeFromOtherClassToWSA()
		{
			var warehouse = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "RW1", 1, 1);
			var wsaLocation = Helper.CreateLocationType("WSA", LocationClasses.Codes.WSA);
			var norLocation = Helper.CreateLocationType("NOR", LocationClasses.Codes.NOR);
			row.Locations[0].WLV_WLT_LocationType = norLocation.PK;

			Factory.Save();

			var command = @$"insert into CYDTransportationUnit (YTU_PK, YTU_WW_Yard, YTU_TransportationUnitID, YTU_WL_WaitingBayLocation, YTU_GateInTime, YTU_SystemCreateTimeUtc, YTU_SystemLastEditTimeUtc, YTU_SystemCreateUser, YTU_SystemLastEditUser)
                values ('65964610-0494-4740-A927-948CC71D1B68', '{warehouse.PK}', 'YTU0001', '{row.Locations[0].PK}', '2024-09-25', '2024-09-25', '2024-09-25', 'TST', 'TST')";
			Db.Connection.ExecuteNonQuery(command);

			row.Locations[0].WLV_WLT_LocationType = wsaLocation.PK;
			AssertNoErrors("Allow change location type from other class to WSA at any time.", row.Locations[0].WLV_WLT_LocationTypeInfo);
		}

		#endregion

		#region TestValidateWL_WLT_LocationType_TemperatureControlled

		public void TestValidateWL_WLT_LocationType_TemperatureControlled()
		{
			var expectedMessage = "This Location Type is not supported for this type of Warehouse.";

			var productWarehouse = Helper.CreateWarehouse("WHS", "A");
			var transitWarehouse = Helper.CreateTRWWarehouse("TRW", "A");
			var ftzWarehouse = Helper.CreateFTZWarehouse("FTZ", "A");
			var containerYardWarehouse = Helper.CreateWarehouse("CYD", "A");
			containerYardWarehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;

			var tclLocationType = Helper.CreateLocationType("TCL", LocationClasses.Codes.TCL);
			var fixLocationType = Helper.CreateLocationType("LC2", "LC2 Test", false, 1, LocationClasses.Codes.FIX);

			var productWhsLocation = productWarehouse.DefaultLocation;
			var transitWhsLocation = transitWarehouse.DefaultLocation;
			var ftzWhsLocation = ftzWarehouse.DefaultLocation;
			var containerYardWhsLocation = containerYardWarehouse.DefaultLocation;

			AssertNoErrors("Precondition: location has no errors", productWhsLocation.WLV_WLT_LocationTypeInfo);
			AssertNoErrors("Precondition: location has no errors", transitWhsLocation.WLV_WLT_LocationTypeInfo);
			AssertNoErrors("Precondition: location has no errors", ftzWhsLocation.WLV_WLT_LocationTypeInfo);
			AssertNoErrors("Precondition: location has no errors", containerYardWhsLocation.WLV_WLT_LocationTypeInfo);
			Factory.Save();

			productWhsLocation.WLV_WLT_LocationType = tclLocationType.PK;
			AssertHasError(productWhsLocation.WLV_WLT_LocationTypeInfo, expectedMessage);

			ftzWhsLocation.WLV_WLT_LocationType = tclLocationType.PK;
			AssertHasError(ftzWhsLocation.WLV_WLT_LocationTypeInfo, expectedMessage);

			containerYardWhsLocation.WLV_WLT_LocationType = tclLocationType.PK;
			AssertNoErrors(containerYardWhsLocation.WLV_WLT_LocationTypeInfo);

			transitWhsLocation.WLV_WLT_LocationType = tclLocationType.PK;
			AssertNoError(transitWhsLocation.WLV_WLT_LocationTypeInfo, expectedMessage);

			productWhsLocation.WLV_WLT_LocationType = fixLocationType.PK;
			ftzWhsLocation.WLV_WLT_LocationType = fixLocationType.PK;
			AssertNoError(productWhsLocation.WLV_WLT_LocationTypeInfo, expectedMessage);
			AssertNoError(ftzWhsLocation.WLV_WLT_LocationTypeInfo, expectedMessage);
		}

		#endregion

		#region TestValidateWL_WLT_LocationType_PackingLocation

		public void TestValidateWL_WLT_LocationType_PackingStation()
		{
			TestValidateWL_WLT_LocationType_PackingLocationCore(LocationClasses.Codes.PST);
		}

		public void TestValidateWL_WLT_LocationType_PackingConsolidation()
		{
			TestValidateWL_WLT_LocationType_PackingLocationCore(LocationClasses.Codes.CON);
		}

		void TestValidateWL_WLT_LocationType_PackingLocationCore(string locationClass)
		{
			var expectedMessage = "This Location Type is not supported for this type of Warehouse.";

			var productWarehouse = Helper.CreateWarehouse("WHS", "A");
			var transitWarehouse = Helper.CreateTRWWarehouse("TRW", "A");
			var ftzWarehouse = Helper.CreateFTZWarehouse("FTZ", "A");
			var containerYardWarehouse = Helper.CreateWarehouse("CYD", "A");
			containerYardWarehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;

			var packingLocationType = Helper.CreateLocationType(locationClass, locationClass, false, 0, locationClass);

			var productWhsLocation = productWarehouse.DefaultLocation;
			var transitWhsLocation = transitWarehouse.DefaultLocation;
			var ftzWhsLocation = ftzWarehouse.DefaultLocation;
			var containerYardWhsLocation = containerYardWarehouse.DefaultLocation;

			AssertNoErrors("Precondition: location has no errors", productWhsLocation.WLV_WLT_LocationTypeInfo);
			AssertNoErrors("Precondition: location has no errors", transitWhsLocation.WLV_WLT_LocationTypeInfo);
			AssertNoErrors("Precondition: location has no errors", ftzWhsLocation.WLV_WLT_LocationTypeInfo);
			AssertNoErrors("Precondition: location has no errors", containerYardWhsLocation.WLV_WLT_LocationTypeInfo);
			Factory.Save();

			productWhsLocation.WLV_WLT_LocationType = packingLocationType.PK;
			AssertNoError(productWhsLocation.WLV_WLT_LocationTypeInfo, expectedMessage);

			ftzWhsLocation.WLV_WLT_LocationType = packingLocationType.PK;
			AssertNoError(ftzWhsLocation.WLV_WLT_LocationTypeInfo, expectedMessage);

			containerYardWhsLocation.WLV_WLT_LocationType = packingLocationType.PK;
			AssertHasError(containerYardWhsLocation.WLV_WLT_LocationTypeInfo, expectedMessage);

			transitWhsLocation.WLV_WLT_LocationType = packingLocationType.PK;
			AssertHasError(transitWhsLocation.WLV_WLT_LocationTypeInfo, expectedMessage);
		}

		#endregion

		#region TestCheckWLV_RS_NKTransitServiceLevel

		public void TestCheckWLV_RS_NKTransitServiceLevel()
		{
			var locationView = Factory.New<WhsLocation>();
			locationView.WLV_TransitDischargeLRC = "";
			locationView.WLV_RS_NKTransitServiceLevel = "";
			AssertNoErrors("Since DischargeLRC is empty, TransitServiceLevel must not have any errors.", locationView.WLV_RS_NKTransitServiceLevelInfo);

			locationView.WLV_RS_NKTransitServiceLevel = "XXX";
			AssertHasError(locationView.WLV_RS_NKTransitServiceLevelInfo, "Enter a valid Service Level.");

			locationView.WLV_RS_NKTransitServiceLevel = "STD";
			AssertHasError(locationView.WLV_RS_NKTransitServiceLevelInfo, "Discharge and Service Level should be both set or both empty.");

			locationView.WLV_TransitDischargeLRC = "AU";
			locationView.WLV_RS_NKTransitServiceLevel = "";
			AssertHasError(locationView.WLV_RS_NKTransitServiceLevelInfo, "Discharge and Service Level should be both set or both empty.");

			locationView.WLV_RS_NKTransitServiceLevel = "XXX";
			AssertHasError(locationView.WLV_RS_NKTransitServiceLevelInfo, "Enter a valid Service Level.");

			locationView.WLV_RS_NKTransitServiceLevel = "STD";
			AssertNoErrors("Since both service level and discharge has a value, there must no errors.", locationView.WLV_TransitDischargeLRCInfo);
			AssertNoErrors("Since both service level and discharge has a value, there must no errors.", locationView.WLV_RS_NKTransitServiceLevelInfo);
		}

		#endregion

		#region TestCheckWLV_TransitDischargeLRC

		public void TestCheckWLV_TransitDischargeLRC()
		{
			var locationView = Factory.New<WhsLocation>();

			locationView.WLV_RS_NKTransitServiceLevel = "";
			locationView.WLV_TransitDischargeLRC = "";
			AssertNoErrors("Since WLV_RS_NKTransitServiceLevel is empty, WLV_TransitDischargeLRC must not have any errors.", locationView.WLV_TransitDischargeLRCInfo);

			locationView.WLV_TransitDischargeLRC = "Q";
			AssertHasError(locationView.WLV_TransitDischargeLRCInfo, "Enter a valid Discharge.");

			locationView.WLV_TransitDischargeLRC = "AU";
			AssertHasError(locationView.WLV_TransitDischargeLRCInfo, "Discharge and Service Level should be both set or both empty.");

			locationView.WLV_RS_NKTransitServiceLevel = "STD";
			locationView.WLV_TransitDischargeLRC = "";
			AssertHasError(locationView.WLV_TransitDischargeLRCInfo, "Discharge and Service Level should be both set or both empty.");

			locationView.WLV_TransitDischargeLRC = "Q";
			AssertHasError(locationView.WLV_TransitDischargeLRCInfo, "Enter a valid Discharge.");

			locationView.WLV_TransitDischargeLRC = "QQ";
			AssertHasError(locationView.WLV_TransitDischargeLRCInfo, "Enter a valid Discharge.");

			locationView.WLV_TransitDischargeLRC = "QQQ";
			AssertHasError(locationView.WLV_TransitDischargeLRCInfo, "Enter a valid Discharge.");

			locationView.WLV_TransitDischargeLRC = "AU";
			AssertNoErrors(locationView.WLV_TransitDischargeLRCInfo);

			locationView.WLV_TransitDischargeLRC = "CNSHA";
			AssertNoErrors(locationView.WLV_TransitDischargeLRCInfo);

			locationView.WLV_TransitDischargeLRC = "AUSYD";
			AssertNoErrors("Since both service level and discharge has a value, there must no errors.", locationView.WLV_TransitDischargeLRCInfo);
			AssertNoErrors("Since both service level and discharge has a value, there must no errors.", locationView.WLV_RS_NKTransitServiceLevelInfo);
		}

		#endregion

		#region TestNoDuplicationOfServiceLevelAndDischargeLRC

		public void TestNoDuplicationOfServiceLevelAndDischargeLRC()
		{
			var warehouse1 = Helper.CreateWarehouse("Whs1", "A", 2, 1);
			Factory.Save();

			var location1 = warehouse1.FindLocation("A-1");
			location1.WLV_RS_NKTransitServiceLevel = "";
			location1.WLV_TransitDischargeLRC = "";
			AssertNoErrors("Since WLV_RS_NKTransitServiceLevel is empty, WLV_TransitDischargeLRC mustn't have any errors.", location1.WLV_TransitDischargeLRCInfo);
			AssertNoErrors("Since WLV_RS_NKTransitServiceLevel is empty, WLV_TransitDischargeLRC mustn't have any errors.", location1.WLV_RS_NKTransitServiceLevelInfo);

			location1.WLV_RS_NKTransitServiceLevel = "STD";
			location1.WLV_TransitDischargeLRC = "AU";
			AssertNoErrors("Since both WLV_RS_NKTransitServiceLevel and WLV_TransitDischargeLRC are not empty, mustn't have any errors.", location1.WLV_TransitDischargeLRCInfo);
			AssertNoErrors("Since WLV_RS_NKTransitServiceLevel is empty, WLV_TransitDischargeLRC mustn't have any errors.", location1.WLV_RS_NKTransitServiceLevelInfo);

			var location2 = warehouse1.FindLocation("A-2");
			location2.WLV_RS_NKTransitServiceLevel = "";
			location2.WLV_TransitDischargeLRC = "";
			AssertNoErrors("Since WLV_RS_NKTransitServiceLevel is empty, WLV_TransitDischargeLRC mustn't have any errors.", location2.WLV_TransitDischargeLRCInfo);
			AssertNoErrors("Since WLV_RS_NKTransitServiceLevel is empty, WLV_TransitDischargeLRC mustn't have any errors.", location2.WLV_RS_NKTransitServiceLevelInfo);

			location2.WLV_RS_NKTransitServiceLevel = "STD";
			location2.WLV_TransitDischargeLRC = "AU";
			AssertHasError(location2.WLV_TransitDischargeLRCInfo, "There is already a location for Discharge 'AU' and Service Level 'STD'. Transit automation will therefore not know which location to use when cross-docking packages.");
			AssertHasError(location2.WLV_RS_NKTransitServiceLevelInfo, "There is already a location for Discharge 'AU' and Service Level 'STD'. Transit automation will therefore not know which location to use when cross-docking packages.");

			location2.WLV_TransitDischargeLRC = "CN";
			AssertNoErrors("Different WLV_TransitDischargeLRC must not have any errors.", location2.WLV_TransitDischargeLRCInfo);
			AssertNoErrors("Different WLV_TransitDischargeLRC must not have any errors.", location2.WLV_RS_NKTransitServiceLevelInfo);

			location2.WLV_TransitDischargeLRC = "AU";
			AssertHasError(location2.WLV_TransitDischargeLRCInfo, "There is already a location for Discharge 'AU' and Service Level 'STD'. Transit automation will therefore not know which location to use when cross-docking packages.");
			AssertHasError(location2.WLV_RS_NKTransitServiceLevelInfo, "There is already a location for Discharge 'AU' and Service Level 'STD'. Transit automation will therefore not know which location to use when cross-docking packages.");

			location2.WLV_RS_NKTransitServiceLevel = "D2D";
			AssertNoErrors("Different WLV_RS_NKTransitServiceLevel mustn't have any errors.", location2.WLV_TransitDischargeLRCInfo);
			AssertNoErrors("Different WLV_TransitDischargeLRC must not have any errors.", location2.WLV_RS_NKTransitServiceLevelInfo);

			var warehouse2 = Helper.CreateWarehouse("Whs2", "A", 2, 1);
			Factory.Save();

			var location21 = warehouse2.FindLocation("A-1");
			location21.WLV_TransitDischargeLRC = "AU";
			location21.WLV_RS_NKTransitServiceLevel = "STD";
			AssertNoErrors("Although same WLV_RS_NKTransitServiceLevel and WLV_TransitDischargeLRC, but different Warehouse mustn't have any errors.", location21.WLV_TransitDischargeLRCInfo);
			AssertNoErrors("Although same WLV_RS_NKTransitServiceLevel and WLV_TransitDischargeLRC, but different Warehouse mustn't have any errors.", location21.WLV_RS_NKTransitServiceLevelInfo);
		}

		#endregion

		#region TestWarningForDischargeLRCOverlapping_ZoneOverlapsCountry

		public void TestWarningForDischargeLRCOverlapping_ZoneOverlapsCountry()
		{
			var warehouse = Helper.CreateWarehouse("Whs1", "A", 2, 1);
			Helper.CreateUNLOCO("ZZ111", "CN");
			Factory.Save();

			var location1 = warehouse.FindLocation("A-1");
			location1.WLV_RS_NKTransitServiceLevel = "";
			location1.WLV_TransitDischargeLRC = "";
			AssertNoWarnings("Both WLV_RS_NKTransitServiceLevel and WLV_TransitDischargeLRC are empty, no warnings.", location1.WLV_TransitDischargeLRCInfo);

			location1.WLV_RS_NKTransitServiceLevel = "STD";
			location1.WLV_TransitDischargeLRC = "AU";
			AssertNoWarnings("Both WLV_RS_NKTransitServiceLevel and WLV_TransitDischargeLRC are not empty, no warnings.", location1.WLV_TransitDischargeLRCInfo);

			var countryAU = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));
			var zoneSyd = Helper.CreateTransitWarehouseZone("SYDZ", "Sydney Zone");
			zoneSyd.Countries.Add(countryAU);
			AssertEquals(1, zoneSyd.Countries.Count);
			Factory.Save();

			var location2 = warehouse.FindLocation("A-2");
			location2.WLV_RS_NKTransitServiceLevel = "STD";
			location2.WLV_TransitDischargeLRC = "SYDZ";
			AssertNoWarnings("Both WLV_RS_NKTransitServiceLevel and WLV_TransitDischargeLRC are not empty, no warnings.", location2.WLV_TransitDischargeLRCInfo);

			location1.Validation.ValidateWLV_TransitDischargeLRC();
			AssertHasWarning(location1.WLV_TransitDischargeLRCInfo, "This location will never be matched because the following locations [A-2] have a Discharge Zone that contains AU and would be matched instead.");

			location2.WLV_RS_NKTransitServiceLevel = "STD";
			location2.WLV_TransitDischargeLRC = "ZZ111";
			AssertNoWarnings("Both WLV_RS_NKTransitServiceLevel and WLV_TransitDischargeLRC are not empty, no warnings.", location2.WLV_TransitDischargeLRCInfo);

			location1.Validation.ValidateWLV_TransitDischargeLRC();
			AssertNoWarnings("No Overlapping should be found, so no warning message for location1.", location1.WLV_TransitDischargeLRCInfo);
		}

		#endregion

		#region TestValidateWLV_LocationStatus

		public void TestValidateWLV_LocationStatus()
		{
			var row = Factory.New<WhsRow>();
			var locn = row.Locations.AddNew();
			TestCodePairList(locn.WLV_LocationStatusInfo, ErrorCheckType.HasErrors, false, new LocationStatus());
		}

		public void TestValidateWLV_LocationStatus_WhenLocationUsedAsInboundDefaultByWarehouse()
		{
			var whs = Helper.CreateWarehouse("W1", "A", 2, 1);
			Factory.Save();

			var locationUsedByWarehouse = whs.FindLocation("A-1");
			var locationNotUsed = whs.FindLocation("A-2");

			locationUsedByWarehouse.WLV_WLT_LocationType = whs.DefaultInboundDockDoorLocation.WLV_WLT_LocationType;
			locationNotUsed.WLV_WLT_LocationType = whs.DefaultInboundDockDoorLocation.WLV_WLT_LocationType;
			Assert("Precondition:", locationUsedByWarehouse.IsDockDoorLocation);
			Assert("Precondition:", locationNotUsed.IsDockDoorLocation);

			whs.WW_DefaultInboundDockDoor = locationUsedByWarehouse.PK;
			AssertNoErrors(locationUsedByWarehouse.WLV_LocationStatusInfo);

			foreach (var locationStatus in new LocationStatus().ToArray().Select(l => l.Code).Where(c => c != LocationStatus.Codes.Normal))
			{
				locationUsedByWarehouse.WLV_LocationStatus = locationStatus;
				AssertHasError(locationUsedByWarehouse.WLV_LocationStatusInfo, "Cannot change Location Status from NOR as this location is the default inbound dock door location for this warehouse.");

				locationUsedByWarehouse.WLV_LocationStatus = LocationStatus.Codes.Normal;
				AssertNoErrors(locationUsedByWarehouse.WLV_LocationStatusInfo);
			}

			foreach (var locationStatus in new LocationStatus().ToArray().Select(l => l.Code))
			{
				locationNotUsed.WLV_LocationStatus = locationStatus;
				AssertNoErrors(locationNotUsed.WLV_LocationStatusInfo);
			}
		}

		public void TestValidateWLV_LocationStatus_WhenLocationUsedAsOutboundDefaultByWarehouse()
		{
			var whs = Helper.CreateWarehouse("W1", "A", 2, 1);
			Factory.Save();

			var locationUsedByWarehouse = whs.FindLocation("A-1");
			var locationNotUsed = whs.FindLocation("A-2");

			locationUsedByWarehouse.WLV_WLT_LocationType = whs.DefaultOutboundDockDoorLocation.WLV_WLT_LocationType;
			locationNotUsed.WLV_WLT_LocationType = whs.DefaultOutboundDockDoorLocation.WLV_WLT_LocationType;
			Assert("Precondition:", locationUsedByWarehouse.IsDockDoorLocation);
			Assert("Precondition:", locationNotUsed.IsDockDoorLocation);

			whs.WW_DefaultOutboundDockDoor = locationUsedByWarehouse.PK;
			AssertNoErrors(locationUsedByWarehouse.WLV_LocationStatusInfo);

			foreach (var locationStatus in new LocationStatus().ToArray().Select(l => l.Code).Where(c => c != LocationStatus.Codes.Normal))
			{
				locationUsedByWarehouse.WLV_LocationStatus = locationStatus;
				AssertHasError(locationUsedByWarehouse.WLV_LocationStatusInfo, "Cannot change Location Status from NOR as this location is the default outbound dock door location for this warehouse.");

				locationUsedByWarehouse.WLV_LocationStatus = LocationStatus.Codes.Normal;
				AssertNoErrors(locationUsedByWarehouse.WLV_LocationStatusInfo);
			}

			foreach (var locationStatus in new LocationStatus().ToArray().Select(l => l.Code))
			{
				locationNotUsed.WLV_LocationStatus = locationStatus;
				AssertNoErrors(locationNotUsed.WLV_LocationStatusInfo);
			}
		}

		public void TestValidateWLV_LocationStatus_WhenLocationUsedAsInboundAndOutboundDefaultByWarehouse()
		{
			var whs = Helper.CreateWarehouse("W1", "A", 2, 1);
			Factory.Save();

			whs.DefaultOutboundDockDoorLocation.WLV_LocationStatus = LocationStatus.Codes.Void;
			AssertHasError(whs.DefaultOutboundDockDoorLocation.WLV_LocationStatusInfo, "Cannot change Location Status from NOR as this location is the default inbound dock door location for this warehouse.");
			AssertHasError(whs.DefaultOutboundDockDoorLocation.WLV_LocationStatusInfo, "Cannot change Location Status from NOR as this location is the default outbound dock door location for this warehouse.");
		}

		public void TestValidateWLV_LocationStatus_WhenUnfinalisedPickReferencesDockDoor()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			var stockHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			stockHelper.CreateStock(data.Whs1.PK, data.Org1.PK, data.Part1.PK, 10m);
			Factory.Save();

			var dockRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "DOCK");
			var dockLocation = dockRow.Locations.Single();
			dockLocation.WLV_WLT_LocationType = data.Whs1.DefaultOutboundDockDoorLocation.WLV_WLT_LocationType;

			var orderOnUnfinalisedPick = stockHelper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, data.Org1.PK, "O1");
			stockHelper.CreateWhsOrderLine(orderOnUnfinalisedPick.PK, data.Part1.PK, 10m);
			var unfinalisedPickPK = stockHelper.CreateWhsPick(new[] { orderOnUnfinalisedPick.PK });
			var unfinalisedPick = (IWhsPick)Factory.Load(ObjectFactory.GetType<IWhsPick>(), unfinalisedPickPK);
			unfinalisedPick.WP_WL_DockDoor = dockLocation.PK;
			Factory.Save();

			foreach (var locationStatus in new LocationStatus().ToArray().Select(l => l.Code).Where(c => c != LocationStatus.Codes.Normal))
			{
				dockLocation.WLV_LocationStatus = locationStatus;
				AssertHasError(dockLocation.WLV_LocationStatusInfo, "Cannot change Location Status from NOR as an un-finalized Pick exists that references this Location.");

				dockLocation.WLV_LocationStatus = LocationStatus.Codes.Normal;
				AssertNoErrors(dockLocation.WLV_LocationStatusInfo);
			}
		}

		public void TestValidateWLV_LocationStatus_WhenFinalisedPickReferencesDockDoor()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			var stockHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			stockHelper.CreateStock(data.Whs1.PK, data.Org1.PK, data.Part1.PK, 10m);
			Factory.Save();

			var dockRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "DOCK");
			var dockLocation = dockRow.Locations.Single();
			dockLocation.WLV_WLT_LocationType = data.Whs1.DefaultOutboundDockDoorLocation.WLV_WLT_LocationType;

			var orderOnFinalisedPick = stockHelper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, data.Org1.PK, "O1");
			stockHelper.CreateWhsOrderLine(orderOnFinalisedPick.PK, data.Part1.PK, 10m);
			var finalisedPickPK = stockHelper.CreateWhsPick(new[] { orderOnFinalisedPick.PK });

			var finalisedPick = (IWhsPick)Factory.Load(ObjectFactory.GetType<IWhsPick>(), finalisedPickPK);
			finalisedPick.WP_WL_DockDoor = dockLocation.PK;
			stockHelper.FinaliseDocketWithoutUserConfirmation(orderOnFinalisedPick.PK);
			stockHelper.FinalisePick(finalisedPickPK);
			Factory.Save();

			foreach (var locationStatus in new LocationStatus().ToArray().Select(l => l.Code))
			{
				dockLocation.WLV_LocationStatus = locationStatus;
				AssertNoErrors(dockLocation.WLV_LocationStatusInfo);
			}
		}

		public void TestValidateWLV_LocationStatus_WhenCancelledPickReferencesDockDoor()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			Factory.Save();

			var dockRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "DOCK");
			var dockLocation = dockRow.Locations.Single();
			dockLocation.WLV_WLT_LocationType = data.Whs1.DefaultOutboundDockDoorLocation.WLV_WLT_LocationType;

			var cancelledPick = Factory.New<IWhsPick>();
			cancelledPick.WP_WW_Whs = data.Whs1.PK;
			cancelledPick.WP_WL_DockDoor = dockLocation.PK;
			cancelledPick.WP_PickStatus = "CAN";
			Factory.Save();

			foreach (var locationStatus in new LocationStatus().ToArray().Select(l => l.Code))
			{
				dockLocation.WLV_LocationStatus = locationStatus;
				AssertNoErrors(dockLocation.WLV_LocationStatusInfo);
			}
		}

		public void TestValidateWLV_LocationStatus_WhenUnfinalisedPickReferencesPackingStation()
		{
			var warehouse = Helper.CreateWarehouse("W1", "A", 2, 2);
			var area = warehouse.Areas[0];
			Factory.Save();
			var location = warehouse.FindLocation("A-1-1");
			Factory.Save();

			var locationTypePST = Helper.CreateLocationType("PST", LocationClasses.Codes.PST);
			location.WLV_WLT_LocationType = locationTypePST.PK;

			var data = new PickFaceViewTestData(Factory, clients: 1, locationsPerWarehouse: 1);
			var transactionHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var order1 = transactionHelper.CreateWhsOrder(data.Clients[0].PK, warehouse.PK, data.Clients[0].PK, "order1");
			transactionHelper.CreateWhsOrderLine(order1.PK, data.Parts[0].PK, 100m);
			var unfinalisedPick = transactionHelper.CreatePickNew(false, false, order1.PK);
			unfinalisedPick[WhsPickSchema.WP_WW_Whs] = warehouse.PK;
			unfinalisedPick[WhsPickSchema.WP_WL_PackingStation] = location.PK;

			Factory.Save();

			foreach (var locationStatus in new LocationStatus().ToArray().Select(l => l.Code).Where(c => c != LocationStatus.Codes.Normal))
			{
				location.WLV_LocationStatus = locationStatus;
				AssertHasError(location.WLV_LocationStatusInfo, "Cannot change Location Status from NOR as an un-finalized Pick exists that references this Location.");

				location.WLV_LocationStatus = LocationStatus.Codes.Normal;
				AssertNoErrors(location.WLV_LocationStatusInfo);
			}
		}

		public void TestValidateWLV_LocationStatus_WhenFinalisedPickReferencesPackingStation()
		{
			var warehouse = Helper.CreateWarehouse("W1", "A", 2, 2);
			var area = warehouse.Areas[0];
			Factory.Save();
			var location = warehouse.FindLocation("A-1-1");
			Factory.Save();

			var locationTypePST = Helper.CreateLocationType("PST", LocationClasses.Codes.PST);
			location.WLV_WLT_LocationType = locationTypePST.PK;

			var data = new PickFaceViewTestData(Factory, clients: 1, locationsPerWarehouse: 1);
			var transactionHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var order1 = transactionHelper.CreateWhsOrder(data.Clients[0].PK, warehouse.PK, data.Clients[0].PK, "order1");
			transactionHelper.CreateWhsOrderLine(order1.PK, data.Parts[0].PK, 100m);
			var finalisedPick = transactionHelper.CreatePickNew(true, true, order1.PK);
			finalisedPick[WhsPickSchema.WP_WW_Whs] = warehouse.PK;
			finalisedPick[WhsPickSchema.WP_WL_PackingStation] = location.PK;
			Factory.Save();

			foreach (var locationStatus in new LocationStatus().ToArray().Select(l => l.Code))
			{
				location.WLV_LocationStatus = locationStatus;
				AssertNoErrors(location.WLV_LocationStatusInfo);
			}
		}

		public void TestValidateWLV_LocationStatus_WhenCancelledPickReferencesPackingStation()
		{
			var warehouse = Helper.CreateWarehouse("W1", "A", 2, 2);
			var area = warehouse.Areas[0];
			Factory.Save();
			var location = warehouse.FindLocation("A-1-1");
			Factory.Save();

			var locationTypePST = Helper.CreateLocationType("PST", LocationClasses.Codes.PST);
			location.WLV_WLT_LocationType = locationTypePST.PK;

			var data = new PickFaceViewTestData(Factory, clients: 1, locationsPerWarehouse: 1);
			var transactionHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var order1 = transactionHelper.CreateWhsOrder(data.Clients[0].PK, warehouse.PK, data.Clients[0].PK, "order1");
			transactionHelper.CreateWhsOrderLine(order1.PK, data.Parts[0].PK, 100m);
			var cancelledPick = transactionHelper.CreatePickNew(false, false, order1.PK);
			cancelledPick[WhsPickSchema.WP_WW_Whs] = warehouse.PK;
			cancelledPick[WhsPickSchema.WP_WL_PackingStation] = location.PK;
			Factory.Save();
			cancelledPick[WhsPickSchema.WP_PickStatus] = "CAN";
			Factory.Save();

			foreach (var locationStatus in new LocationStatus().ToArray().Select(l => l.Code))
			{
				location.WLV_LocationStatus = locationStatus;
				AssertNoErrors(location.WLV_LocationStatusInfo);
			}
		}

		public void TestValidateWLV_LocationStatus_VoidStatus()
		{
			var warehouse = Helper.CreateWarehouse("WHS", "A");
			var location = warehouse.DefaultLocation;

			const bool SAVE = true;

			AssertLocationStatus(location, LocationStatus.Codes.Damaged, !SAVE);
			AssertLocationStatus(location, LocationStatus.Codes.Normal, SAVE);
			AssertLocationStatus(location, LocationStatus.Codes.Void, SAVE);
			AssertLocationStatus(location, LocationStatus.Codes.Normal, SAVE);

			var client = Helper.CreateClient();
			var product = Helper.CreateProduct("Fedora", client);
			var stockHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			stockHelper.CreateStock(warehouse.PK, client.PK, product.PK, 10m);
			Factory.Save();

			AssertLocationStatus(location, LocationStatus.Codes.Held, !SAVE);
			AssertLocationStatus(location, LocationStatus.Codes.Held, !SAVE);
			AssertLocationStatus(location, LocationStatus.Codes.Void, !SAVE, "Cannot change Location Status to VOI as stock exists in this Location.");

			// Disable Trigger to allow saving a Location with a Void Status even though there is stock on hand
			TestConnection.ExecuteNonQuery("DISABLE TRIGGER TG_WhsLocation_PreventChangingLocationStatus ON WhsLocation");
			Factory.Save();
			location.Validation.ValidateWLV_LocationStatus();
			AssertNoErrors(location.WLV_LocationStatusInfo);

			// should be able to change the location status back to normal even if stock exists in the location
			AssertLocationStatus(location, LocationStatus.Codes.Normal, !SAVE);
			AssertLocationStatus(location, LocationStatus.Codes.Void, !SAVE);

			Factory.Save();
			location.Validation.ValidateWLV_LocationStatus();
			AssertNoErrors(location.WLV_LocationStatusInfo);

			// should be able to change the location status back to normal even if stock exists in the location
			AssertLocationStatus(location, LocationStatus.Codes.Normal, !SAVE);
		}

		public void TestValidateTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfHasOpenRTU()
		{
			const bool SAVE = true;

			var warehouse = Helper.CreateTRWWarehouse();

			var row = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			warehouse.Rows.Add(row);
			var location = row.Locations[0];
			location.WLV_LocationStatus = "NOR";

			var dateTimeNow = DateTimeOffset.Now;
			var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);
			var rtu = (BusinessObject)Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_WL_StagingLocation] = location.PK;
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_GateInTime] = dateTimeOffset;
			Factory.Save();

			AssertLocationStatus(location, LocationStatus.Codes.Void, !SAVE, "Cannot change Location Status to Void as there are Receive Transportation Units planned for this location or currently being unloaded.");
		}

		public void TestValidateTransitWarehouse_WLV_LocationStatus_CanChangeToVoidIfHasCompletedRTU()
		{
			const bool SAVE = true;

			var warehouse = Helper.CreateTRWWarehouse();

			var row = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			warehouse.Rows.Add(row);
			var location = row.Locations[0];
			location.WLV_LocationStatus = "NOR";

			var dateTimeNow = DateTimeOffset.Now;
			var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);
			var rtu = (BusinessObject)Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_WL_StagingLocation] = location.PK;
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_GateInTime] = dateTimeOffset;
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_UnloadCompleteTime] = dateTimeOffset.AddHours(2);
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_UnloadCompleteNotYetProcessedTime] = dateTimeOffset.AddHours(2);
			Factory.Save();

			AssertLocationStatus(location, LocationStatus.Codes.Void, !SAVE);
		}

		public void TestValidateTransitWarehouse_WLV_LocationStatus_CanChangeToVoidIfNoOpenRTU()
		{
			const bool SAVE = true;

			var warehouse = Helper.CreateTRWWarehouse();

			var row = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			warehouse.Rows.Add(row);
			var location = row.Locations[0];
			location.WLV_LocationStatus = "NOR";

			var dateTimeNow = DateTimeOffset.Now;
			var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);
			var rtu = (BusinessObject)Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_WL_StagingLocation] = location.PK;
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_GateInTime] = dateTimeOffset;
			Factory.Save();

			AssertLocationStatus(location, LocationStatus.Codes.Void, !SAVE, "Cannot change Location Status to Void as there are Receive Transportation Units planned for this location or currently being unloaded.");

			var row2 = Helper.CreateRowAndGenerateLocations(warehouse, "DifferntDOCK");
			warehouse.Rows.Add(row2);
			var location2 = row2.Locations[0];
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_WL_StagingLocation] = location2.PK;
			Factory.Save();

			AssertLocationStatus(location, LocationStatus.Codes.Void, !SAVE);
		}

		public void TestValidateTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfHasPackage()
		{
			const bool SAVE = true;

			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			warehouse.Rows.Add(row);
			var location = row.Locations[0];
			location.WLV_LocationStatus = "NOR";

			var dateTimeNow = DateTimeOffset.Now;
			var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);
			var rtu = (BusinessObject)Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_WL_StagingLocation] = location.PK;
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_GateInTime] = dateTimeOffset;
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_UnloadCompleteTime] = dateTimeOffset.AddHours(2);
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_UnloadCompleteNotYetProcessedTime] = dateTimeOffset.AddHours(2);

			var packageJob = Helper.CreatePackageJob(rtu);
			var package = (BusinessObject)Helper.CreatePackage(packageJob);
			var packageState = CreateWhsItemPackageState("ARV", location, rtu, package);
			Factory.Save();

			AssertLocationStatus(location, LocationStatus.Codes.Void, !SAVE, "Cannot change Location Status to Void as there are Packages in this location.");
		}

		public void TestValidateTransitWarehouse_WLV_LocationStatus_CanChangeToVoidIfNoPackage()
		{
			const bool SAVE = true;

			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			warehouse.Rows.Add(row);
			var location = row.Locations[0];
			location.WLV_LocationStatus = "NOR";

			var dateTimeNow = DateTimeOffset.Now;
			var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);
			var rtu = (BusinessObject)Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_WL_StagingLocation] = location.PK;
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_GateInTime] = dateTimeOffset;
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_UnloadCompleteTime] = dateTimeOffset.AddHours(2);
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_UnloadCompleteNotYetProcessedTime] = dateTimeOffset.AddHours(2);

			var packageJob = Helper.CreatePackageJob(rtu);
			var package = (BusinessObject)Helper.CreatePackage(packageJob);
			var packageState = CreateWhsItemPackageState("ARV", location, rtu, package);
			Factory.Save();

			AssertLocationStatus(location, LocationStatus.Codes.Void, !SAVE, "Cannot change Location Status to Void as there are Packages in this location.");

			var row2 = Helper.CreateRowAndGenerateLocations(warehouse, "DifferntDOCK");
			warehouse.Rows.Add(row2);
			var location2 = row2.Locations[0];
			packageState[WhsItemPackageStateSchema.WPS_WL_LastLocation] = location2.PK;
			Factory.Save();

			AssertLocationStatus(location, LocationStatus.Codes.Void, !SAVE);
		}

		public void TestValidateTransitWarehouse_WLV_LocationStatus_CanChangeToVoidIfPackageAdjustedOut()
		{
			const bool SAVE = true;
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			warehouse.Rows.Add(row);
			var location = row.Locations[0];
			location.WLV_LocationStatus = "NOR";

			var dateTimeNow = DateTimeOffset.Now;
			var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);
			var rtu = (BusinessObject)Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_WL_StagingLocation] = location.PK;
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_GateInTime] = dateTimeOffset;
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_UnloadCompleteTime] = dateTimeOffset.AddHours(2);
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_UnloadCompleteNotYetProcessedTime] = dateTimeOffset.AddHours(2);

			var packageJob = Helper.CreatePackageJob(rtu);
			var package = (BusinessObject)Helper.CreatePackage(packageJob);
			var packageState = CreateWhsItemPackageState("ADJ", location, rtu, package);
			packageState[WhsItemPackageStateSchema.WPS_AdjustedOut] = "lol";

			Factory.Save();

			AssertLocationStatus(location, LocationStatus.Codes.Void, !SAVE);
		}

		public void TestValidateTransitWarehouse_WLV_LocationStatus_CanChangeToVoidIfPackageDeparted()
		{
			const bool SAVE = true;
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			warehouse.Rows.Add(row);
			var location = row.Locations[0];
			location.WLV_LocationStatus = "NOR";

			var dateTimeNow = DateTimeOffset.Now;
			var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);
			var rtu = (BusinessObject)Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_WL_StagingLocation] = location.PK;
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_GateInTime] = dateTimeOffset;
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_UnloadCompleteTime] = dateTimeOffset.AddHours(2);
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_UnloadCompleteNotYetProcessedTime] = dateTimeOffset.AddHours(2);

			var dtu = (BusinessObject)Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			dtu[WhsItemDispatchTransportationUnitSchema.WDH_GateInTime] = dateTimeOffset.AddHours(4);
			dtu[WhsItemDispatchTransportationUnitSchema.WDH_LoadCompleteTime] = dateTimeOffset.AddHours(5);
			dtu[WhsItemDispatchTransportationUnitSchema.WDH_VehicleReference] = "dtu go brrr";

			var loadList = (BusinessObject)Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			loadList[WhsItemDispatchLoadListSchema.WDL_WL_StagingLocation] = location.PK;
			loadList[WhsItemDispatchLoadListSchema.WDL_CompleteTime] = dateTimeOffset.AddHours(5);

			var dcn = (BusinessObject)Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var rcn = (BusinessObject)Helper.CreateReceiveConsignment("RCN1", warehouse.PK);

			var packageJob = Helper.CreatePackageJob(rtu);
			var package = (BusinessObject)Helper.CreatePackage(packageJob);
			var packageState = CreateWhsItemPackageState("DEP", location, rtu, package);
			packageState[WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment] = rcn.PK;
			packageState[WhsItemPackageStateSchema.WPS_WDL_LoadList] = loadList.PK;
			packageState[WhsItemPackageStateSchema.WPS_WDH_TransitDispatchHeader] = dtu.PK;
			packageState[WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment] = dcn.PK;
			packageState[WhsItemPackageStateSchema.WPS_IsSecure] = 1;

			Factory.Save();

			AssertLocationStatus(location, LocationStatus.Codes.Void, !SAVE);
		}

		public void TestValidateTransitWarehouse_WLV_LocationStatus_CanChangeToVoidIfPackageFinalised()
		{
			const bool SAVE = true;
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			warehouse.Rows.Add(row);
			var location = row.Locations[0];
			location.WLV_LocationStatus = "NOR";

			var dateTimeNow = DateTimeOffset.Now;
			var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);
			var rtu = (BusinessObject)Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_WL_StagingLocation] = location.PK;
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_GateInTime] = dateTimeOffset;
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_UnloadCompleteTime] = dateTimeOffset.AddHours(2);
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_UnloadCompleteNotYetProcessedTime] = dateTimeOffset.AddHours(2);

			var dtu = (BusinessObject)Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			dtu[WhsItemDispatchTransportationUnitSchema.WDH_GateInTime] = dateTimeOffset.AddHours(4);
			dtu[WhsItemDispatchTransportationUnitSchema.WDH_LoadCompleteTime] = dateTimeOffset.AddHours(5);
			dtu[WhsItemDispatchTransportationUnitSchema.WDH_VehicleReference] = "dtu go brrr";

			var loadList = (BusinessObject)Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			loadList[WhsItemDispatchLoadListSchema.WDL_WL_StagingLocation] = location.PK;
			loadList[WhsItemDispatchLoadListSchema.WDL_CompleteTime] = dateTimeOffset.AddHours(5);

			var dcn = (BusinessObject)Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var rcn = (BusinessObject)Helper.CreateReceiveConsignment("RCN1", warehouse.PK);

			var packageJob = Helper.CreatePackageJob(rtu);
			var package = (BusinessObject)Helper.CreatePackage(packageJob);
			var packageState = CreateWhsItemPackageState("FIN", location, rtu, package);
			packageState[WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment] = rcn.PK;
			packageState[WhsItemPackageStateSchema.WPS_WDL_LoadList] = loadList.PK;
			packageState[WhsItemPackageStateSchema.WPS_WDH_TransitDispatchHeader] = dtu.PK;
			packageState[WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment] = dcn.PK;
			packageState[WhsItemPackageStateSchema.WPS_IsSecure] = 1;

			Factory.Save();

			AssertLocationStatus(location, LocationStatus.Codes.Void, !SAVE);
		}

		public void TestValidateTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfCycleCountIsNotCompleted_NotStart() => TestValidateTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfCycleCountIsNotCompletedCore("NST");
		public void TestValidateTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfCycleCountIsNotCompleted_InProcessing() => TestValidateTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfCycleCountIsNotCompletedCore("INP");
		public void TestValidateTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfCycleCountIsNotCompleted_ProcessingVariance() => TestValidateTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfCycleCountIsNotCompletedCore("PCV");
		public void TestValidateTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfCycleCountIsNotCompleted_Error() => TestValidateTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfCycleCountIsNotCompletedCore("ERR");
		void TestValidateTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfCycleCountIsNotCompletedCore(string status)
		{
			const bool SAVE = true;
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			warehouse.Rows.Add(row);
			var location = row.Locations[0];
			location.WLV_LocationStatus = "NOR";

			var cycleCount = (BusinessObject)Factory.New<IWhsItemCycleCountLocation>();
			cycleCount[WhsItemCycleCountLocationSchema.WIC_JobID] = "C42";
			cycleCount[WhsItemCycleCountLocationSchema.WIC_WL_Location] = location.PK;
			cycleCount[WhsItemCycleCountLocationSchema.WIC_Status] = status;
			switch (status)
			{
				case "INP":
					cycleCount[WhsItemCycleCountLocationSchema.WIC_StartTime] = DateTimeOffset.Now;
					break;
				case "PCV":
					cycleCount[WhsItemCycleCountLocationSchema.WIC_StartTime] = DateTimeOffset.Now;
					cycleCount[WhsItemCycleCountLocationSchema.WIC_ProcessingTime] = DateTimeOffset.Now;
					break;
				case "CMP":
				case "PAV":
				case "PRV":
					cycleCount[WhsItemCycleCountLocationSchema.WIC_StartTime] = DateTimeOffset.Now;
					cycleCount[WhsItemCycleCountLocationSchema.WIC_ProcessingTime] = DateTimeOffset.Now;
					cycleCount[WhsItemCycleCountLocationSchema.WIC_EndTime] = DateTimeOffset.Now;
					break;
				default:
					break;
			}

			cycleCount[WhsItemCycleCountLocationSchema.WIC_GS_NKAssignedTo] = Env.CurrentUser.Initials;

			Factory.Save();

			AssertLocationStatus(location, LocationStatus.Codes.Void, !SAVE, "Cannot change Location Status to Void as there are pending cycle counting jobs for this location.");
		}

		public void TestValidateTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfCycleCountHasPendingVariance_ExpectedStockLocation() =>
			TestValidateTransitWarehouse_WLV_LocationStatus_WithCycleCountVarianceCore(true, "OPN", "Cannot change Location Status to Void as there are open cycle counting variance for this location.");

		public void TestValidateTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfCycleCountHasPendingVariance_CycleCountLocation() =>
			TestValidateTransitWarehouse_WLV_LocationStatus_WithCycleCountVarianceCore(false, "OPN", "Cannot change Location Status to Void as there are open cycle counting variance for this location.");

		public void TestValidateTransitWarehouse_WLV_LocationStatus_CanChangeToVoidIfCycleCountHasRjectedVariance_CycleCountLocation() => TestValidateTransitWarehouse_WLV_LocationStatus_WithCycleCountVarianceCore(false, "REJ");

		public void TestValidateTransitWarehouse_WLV_LocationStatus_CanChangeToVoidIfCycleCountHasRjectedVariance_ExpectedStockLocation() => TestValidateTransitWarehouse_WLV_LocationStatus_WithCycleCountVarianceCore(true, "REJ");

		public void TestValidateTransitWarehouse_WLV_LocationStatus_CanChangeToVoidIfCycleCountHasApprovedVariance_CycleCountLocation() =>
			TestValidateTransitWarehouse_WLV_LocationStatus_WithCycleCountVarianceCore(false, "APP");

		public void TestValidateTransitWarehouse_WLV_LocationStatus_CanChangeToVoidIfCycleCountHasApprovedVariance_ExpectedStockLocation() =>
			TestValidateTransitWarehouse_WLV_LocationStatus_WithCycleCountVarianceCore(true, "APP");

		void TestValidateTransitWarehouse_WLV_LocationStatus_WithCycleCountVarianceCore(bool locationIsExpectedStockLocation, string varianceStatus, string errorMessage = null)
		{
			const bool SAVE = true;
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK", 2);
			warehouse.Rows.Add(row);
			var location = row.Locations[0];
			location.WLV_LocationStatus = "NOR";
			var location1 = row.Locations[1];
			location1.WLV_LocationStatus = "NOR";

			var now = DateTimeOffset.Now;
			var cycleCount = (BusinessObject)Factory.New<IWhsItemCycleCountLocation>();
			cycleCount[WhsItemCycleCountLocationSchema.WIC_JobID] = "C42";
			cycleCount[WhsItemCycleCountLocationSchema.WIC_WL_Location] = location.PK;
			cycleCount[WhsItemCycleCountLocationSchema.WIC_Status] = "APP";
			cycleCount[WhsItemCycleCountLocationSchema.WIC_StartTime] = now;
			cycleCount[WhsItemCycleCountLocationSchema.WIC_ProcessingTime] = now.AddHours(1);
			cycleCount[WhsItemCycleCountLocationSchema.WIC_EndTime] = now.AddHours(1);
			cycleCount[WhsItemCycleCountLocationSchema.WIC_GS_NKAssignedTo] = Env.CurrentUser.Initials;

			var cycleCountVariance = (BusinessObject)Factory.New<IWhsItemCycleCountLocationVariance>();
			cycleCountVariance[WhsItemCycleCountLocationVarianceSchema.WIV_Status] = varianceStatus;
			if (locationIsExpectedStockLocation)
			{
				var rtu = (BusinessObject)Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location1.PK);
				rtu[WhsItemReceiveTransportationUnitSchema.WRH_WL_StagingLocation] = location1.PK;
				var dateTimeNow = DateTimeOffset.Now;
				var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);
				rtu[WhsItemReceiveTransportationUnitSchema.WRH_GateInTime] = dateTimeOffset;
				rtu[WhsItemReceiveTransportationUnitSchema.WRH_UnloadCompleteTime] = dateTimeOffset.AddHours(2);
				rtu[WhsItemReceiveTransportationUnitSchema.WRH_UnloadCompleteNotYetProcessedTime] = dateTimeOffset.AddHours(2);

				var packageJob = Helper.CreatePackageJob(rtu);
				var package = (BusinessObject)Helper.CreatePackage(packageJob);
				var packageState = (BusinessObject)Factory.New<IWhsItemPackageState>();
				packageState[WhsItemPackageStateSchema.WPS_WL_LastLocation] = location.PK;
				packageState[WhsItemPackageStateSchema.WPS_Status] = "ARV";
				packageState[WhsItemPackageStateSchema.WPS_WRH_TransitReceiveHeader] = rtu.PK;
				packageState[WhsItemPackageStateSchema.WPS_KP_Package] = package.PK;
				packageState[WhsItemPackageStateSchema.WPS_WW_Warehouse] = warehouse.PK;
				packageState[WhsItemPackageStateSchema.WPS_UnloadedTime] = ZDateTimeOffset.Now;

				var transferLine = (BusinessObject)Helper.CreateTransferLine(Helper.CreateTransferHeader("TRF1", warehouse, false), location1, location, (IWhsItemPackageState)packageState);

				cycleCountVariance[WhsItemCycleCountLocationVarianceSchema.WIV_VarianceQty] = (short)1;
				cycleCountVariance[WhsItemCycleCountLocationVarianceSchema.WIV_WL_ExpectedStockLocation] = location1.PK;
				if (varianceStatus == "APP")
				{
					cycleCountVariance[WhsItemCycleCountLocationVarianceSchema.WIV_WTF_TransferLine] = transferLine.PK;
				}
				cycleCountVariance[WhsItemCycleCountLocationVarianceSchema.WIV_WPS_PackageState] = packageState.PK;
			}
			else
			{
				cycleCountVariance[WhsItemCycleCountLocationVarianceSchema.WIV_VarianceQty] = (short)0;
			}
			cycleCountVariance[WhsItemCycleCountLocationVarianceSchema.WIV_WIC_CycleCountLocation] = cycleCount.PK;

			Factory.Save();

			AssertLocationStatus(locationIsExpectedStockLocation ? location1 : location, LocationStatus.Codes.Void, !SAVE, errorMessage);
		}

		public void TestValidateTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfHasUnfinalisedTransferToThisLocation()
		{
			const bool SAVE = true;

			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "PUTAWAY1");
			warehouse.Rows.Add(row);
			var fromLocation = row.Locations[0];

			var row2 = Helper.CreateRowAndGenerateLocations(warehouse, "PUTAWAY2");
			warehouse.Rows.Add(row2);
			var toLocation = row2.Locations[0];
			toLocation.WLV_LocationStatus = "NOR";

			var rtu = (BusinessObject)Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, fromLocation.PK);
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_WL_StagingLocation] = fromLocation.PK;

			var packageJob = Helper.CreatePackageJob(rtu);
			var package = (BusinessObject)Helper.CreatePackage(packageJob);
			var packageState = CreateWhsItemPackageState("ARV", fromLocation, rtu, package);

			var transfer = Helper.CreateTransferHeader("TRF1", warehouse, false);
			var transferLine = Helper.CreateTransferLine(transfer, fromLocation, toLocation, (IWhsItemPackageState)packageState);
			Factory.Save();

			AssertLocationStatus(toLocation, LocationStatus.Codes.Void, !SAVE, "Cannot change Location Status to Void as there are un-finalized Transfers to this location.");
		}

		public void TestValidateTransitWarehouse_WLV_LocationStatus_CanChangeToVoidIfNoUnfinalisedTransferToThisLocation()
		{
			const bool SAVE = true;

			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "PUTAWAY1");
			warehouse.Rows.Add(row);
			var fromLocation = row.Locations[0];

			var row2 = Helper.CreateRowAndGenerateLocations(warehouse, "PUTAWAY2");
			warehouse.Rows.Add(row2);
			var toLocation = row2.Locations[0];
			toLocation.WLV_LocationStatus = "NOR";

			var rtu = (BusinessObject)Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, fromLocation.PK);
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_WL_StagingLocation] = fromLocation.PK;

			var packageJob = Helper.CreatePackageJob(rtu);
			var package = (BusinessObject)Helper.CreatePackage(packageJob);
			var packageState = CreateWhsItemPackageState("ARV", fromLocation, rtu, package);

			var transfer = Helper.CreateTransferHeader("TRF1", warehouse, false);
			var transferLine = (BusinessObject)Helper.CreateTransferLine(transfer, fromLocation, toLocation, (IWhsItemPackageState)packageState);
			Factory.Save();

			AssertLocationStatus(toLocation, LocationStatus.Codes.Void, !SAVE, "Cannot change Location Status to Void as there are un-finalized Transfers to this location.");

			var row3 = Helper.CreateRowAndGenerateLocations(warehouse, "PUTAWAY3");
			warehouse.Rows.Add(row3);
			var differentLocation = row3.Locations[0];
			transferLine[WhsItemTransferLineSchema.WTF_WL_To] = differentLocation.PK;
			Factory.Save();

			AssertLocationStatus(toLocation, LocationStatus.Codes.Void, !SAVE);
		}

		public void TestValidateTransitWarehouse_WLV_LocationStatus_CanChangeToVoidIfHasFinalisedTransferToThisLocation()
		{
			const bool SAVE = true;

			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "PUTAWAY1");
			warehouse.Rows.Add(row);
			var fromLocation = row.Locations[0];

			var row2 = Helper.CreateRowAndGenerateLocations(warehouse, "PUTAWAY2");
			warehouse.Rows.Add(row2);
			var toLocation = row2.Locations[0];
			toLocation.WLV_LocationStatus = "NOR";

			var rtu = (BusinessObject)Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, fromLocation.PK);
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_WL_StagingLocation] = fromLocation.PK;

			var packageJob = Helper.CreatePackageJob(rtu);
			var package = (BusinessObject)Helper.CreatePackage(packageJob);
			var packageState = CreateWhsItemPackageState("ARV", fromLocation, rtu, package);

			var transfer = Helper.CreateTransferHeader("TRF1", warehouse, true);
			var transferLine = Helper.CreateTransferLine(transfer, fromLocation, toLocation, (IWhsItemPackageState)packageState);
			Factory.Save();

			AssertLocationStatus(toLocation, LocationStatus.Codes.Void, !SAVE);
		}

		public void TestValidateTransitWarehouse_WLV_LocationStatus_CannotChangeToVoidIfHasLoadListForThisLocation()
		{
			const bool SAVE = true;

			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			warehouse.Rows.Add(row);
			var location = row.Locations[0];
			location.WLV_LocationStatus = "NOR";

			var loadList = (BusinessObject)Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			loadList[WhsItemDispatchLoadListSchema.WDL_WL_StagingLocation] = location.PK;
			Factory.Save();

			AssertLocationStatus(location, LocationStatus.Codes.Void, !SAVE, "Cannot change Location Status to Void as there are Loads planned for this location.");
		}

		public void TestValidateTransitWarehouse_WLV_LocationStatus_CanChangeToVoidIfNoLoadListForThisLocation()
		{
			const bool SAVE = true;

			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			warehouse.Rows.Add(row);
			var location = row.Locations[0];
			location.WLV_LocationStatus = "NOR";

			var loadList = (BusinessObject)Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			loadList[WhsItemDispatchLoadListSchema.WDL_WL_StagingLocation] = location.PK;
			Factory.Save();

			AssertLocationStatus(location, LocationStatus.Codes.Void, !SAVE, "Cannot change Location Status to Void as there are Loads planned for this location.");

			var row2 = Helper.CreateRowAndGenerateLocations(warehouse, "PUTAWAY2");
			warehouse.Rows.Add(row2);
			var differentLocation = row2.Locations[0];

			loadList[WhsItemDispatchLoadListSchema.WDL_WL_StagingLocation] = differentLocation.PK;
			Factory.Save();

			AssertLocationStatus(location, LocationStatus.Codes.Void, !SAVE);
		}

		public void TestValidateTransitWarehouse_WLV_LocationStatus_CanChangeToVoidIfCompletedLoadListForThisLocation()
		{
			const bool SAVE = true;

			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			warehouse.Rows.Add(row);
			var location = row.Locations[0];
			location.WLV_LocationStatus = "NOR";

			var loadList = (BusinessObject)Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			loadList[WhsItemDispatchLoadListSchema.WDL_WL_StagingLocation] = location.PK;
			Factory.Save();

			AssertLocationStatus(location, LocationStatus.Codes.Void, !SAVE, "Cannot change Location Status to Void as there are Loads planned for this location.");

			loadList[WhsItemDispatchLoadListSchema.WDL_CompleteTime] = DateTimeOffset.Now;
			Factory.Save();

			AssertLocationStatus(location, LocationStatus.Codes.Void, !SAVE);
		}

		void AssertLocationStatus(WhsLocation location, string locationStatus, bool saveInFactory, string errorMessage = "")
		{
			if (saveInFactory)
			{
				Factory.Save();
			}

			location.WLV_LocationStatus = locationStatus;
			if (!string.IsNullOrEmpty(errorMessage))
			{
				AssertHasError(location.WLV_LocationStatusInfo, errorMessage);
			}
			else
			{
				AssertNoErrors(location.WLV_LocationStatusInfo);
			}
		}

		#endregion

		#region TestValidateWLV_PickMethod

		public void TestValidateWLV_PickMethod()
		{
			var row = Factory.New<WhsRow>();
			var locn = row.Locations.AddNew();
			TestCodePairList(locn.WLV_PickMethodInfo, ErrorCheckType.HasErrors, false, WarehouseDataRegistry.Instance.PickMethod.Value);
		}

		#endregion

		#region TestAreaValidation

		#region TestValidateWLV_WA_PickingArea

		public void TestValidateWLV_WA_PickingArea()
		{
			const string errorMessage = "Enter a valid Pick Area.";
			var warehouse = Helper.CreateWarehouse("W1");
			var differentWarehouse = Helper.CreateWarehouse("W2");
			var pickingArea = Helper.CreateArea(warehouse, "A1", AreaTypes.Codes.FreeStore, true, false);
			var putawayArea = Helper.CreateArea(warehouse, "A2", AreaTypes.Codes.FreeStore, false, true);
			var bothArea = Helper.CreateArea(warehouse, "A3", AreaTypes.Codes.FreeStore, true, true);
			var pickingAreaInDifferentWarehouse = Helper.CreateArea(differentWarehouse, "A4", AreaTypes.Codes.FreeStore, true, false);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "R1", 1, 1);
			var location = row.Locations.Single();
			location.WLV_WA_PickingArea = pickingArea.PK;
			AssertNoErrors("Area set is a picking area therefore must not have any errors.", location.WLV_WA_PickingAreaInfo);

			location.WLV_WA_PickingArea = putawayArea.PK;
			AssertHasError("Area set is putaway area therefore must have an error.", location.WLV_WA_PickingAreaInfo, errorMessage);

			location.WLV_WA_PickingArea = bothArea.PK;
			AssertNoErrors("Setting an area with Both area type must not have any errors.", location.WLV_WA_PickingAreaInfo);

			location.WLV_WA_PickingArea = ZGuid.NewZGuid();
			AssertHasError("Area PK does not exists therefore must have errors.", location.WLV_WA_PickingAreaInfo, errorMessage);

			location.WLV_WA_PickingArea = ZGuid.Empty;
			AssertHasError("Area PK is empty therefore must have errors.", location.WLV_WA_PickingAreaInfo, "Please enter a Pick Area.");

			location.WLV_WA_PickingArea = pickingAreaInDifferentWarehouse.PK;
			AssertHasError("Picking area in a different warehouse must not be valid.", location.WLV_WA_PickingAreaInfo, errorMessage);
		}

		public void TestValidateWLV_WA_PickingArea_DynamicContainsOnlyDynamicLocations()
		{
			var warehouse = Helper.CreateWarehouse("WHS9");

			var dynamicArea = Factory.New<WhsArea>();
			dynamicArea.WA_AreaType = AreaTypes.Codes.DynamicPickFace;
			dynamicArea.WA_IsPickingArea = true;
			dynamicArea.WA_IsPutawayArea = false;

			var freeStoreArea = Factory.New<WhsArea>();
			freeStoreArea.WA_AreaType = AreaTypes.Codes.DockDoor;
			freeStoreArea.WA_IsPickingArea = true;
			freeStoreArea.WA_IsPutawayArea = false;

			var fixLocationType = Factory.New<WhsLocationType>();
			fixLocationType.WLT_Code = LocationClasses.Codes.FIX;
			fixLocationType.WLT_IsActive = true;
			fixLocationType.WLT_Description = LocationClasses.Descriptions.FIX;
			fixLocationType.WLT_IsSystem = true;
			fixLocationType.WLT_MaximumNumberOfProducts = 1;
			fixLocationType.WLT_LocationClass = LocationClasses.Codes.FIX;

			var dynamicPickFaceLocType = Factory.New<WhsLocationType>();
			dynamicPickFaceLocType.WLT_Code = LocationClasses.Codes.DPF;
			dynamicPickFaceLocType.WLT_IsActive = true;
			dynamicPickFaceLocType.WLT_Description = LocationClasses.Descriptions.DPF;
			dynamicPickFaceLocType.WLT_IsSystem = true;
			dynamicPickFaceLocType.WLT_MaximumNumberOfProducts = 1;
			dynamicPickFaceLocType.WLT_LocationClass = LocationClasses.Codes.DPF;

			var location = Factory.New<WhsLocation>();
			location.WLV_WLT_LocationType = fixLocationType.PK;
			location.WLV_WA_PickingArea = freeStoreArea.PK;

			AssertNoErrorContaining(location.WLV_WA_PickingAreaInfo, "Dynamic Pick Face Areas must only have Dynamic Pick Face Locations.");
			AssertNoErrorContaining(location.WLV_WLT_LocationTypeInfo, "Dynamic Pick Face Areas must have Dynamic Pick Face Locations only.");

			location.WLV_WA_PickingArea = dynamicArea.PK;

			AssertHasError("Should not allow non-dynamic locs in dynamic areas", location.WLV_WA_PickingAreaInfo, "Dynamic Pick Face Areas must only have Dynamic Pick Face Locations.");
			AssertHasError("Should not allow non-dynamic locs in dynamic areas", location.WLV_WLT_LocationTypeInfo, "Dynamic Pick Face Areas must have Dynamic Pick Face Locations only.");

			location.WLV_WLT_LocationType = dynamicPickFaceLocType.PK;

			AssertNoErrorContaining(location.WLV_WA_PickingAreaInfo, "Dynamic Pick Face Areas must only have Dynamic Pick Face Locations.");
			AssertNoErrorContaining(location.WLV_WLT_LocationTypeInfo, "Dynamic Pick Face Areas must have Dynamic Pick Face Locations only.");
		}

		public void TestValidateWLV_WA_PickingArea_PreventChangeFromBondedToNonBonded_IfLocationHasSOH()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var bondedArea = Helper.CreateArea(data.Whs1, "Bonded", AreaTypes.Codes.Bonded);
			var freeStoreArea = Helper.CreateArea(data.Whs1, "FreeStore", AreaTypes.Codes.FreeStore);

			var location = data.Whs1.FindLocation("A-1");
			location.WLV_WA_PickingArea = bondedArea.PK;
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var receivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", "CUS", Notify);
			transactionTestHelper.CreateWhsReceiveInventoryLine(receivePK, data.Part1.PK, 10m, ZDate.Empty, ZDate.Empty, "", "", "", "EntryKey", "A-1");
			transactionTestHelper.FinaliseDocket(receivePK);
			Factory.Save();

			var bondedSOHError = "Locations containing Customs inventory in a Bonded Picking Area cannot change Picking Area.";
			AssertNoErrorContaining(location.WLV_WA_PickingAreaInfo, bondedSOHError);

			location.WLV_WA_PickingArea = freeStoreArea.PK;
			AssertHasError("Should not allow changing bonded locations with stock to non-bonded", location.WLV_WA_PickingAreaInfo, bondedSOHError);

			location.WLV_WA_PickingArea = bondedArea.PK;
			AssertNoErrorContaining(location.WLV_WA_PickingAreaInfo, bondedSOHError);
		}

		public void TestValidateWLV_WA_PickingArea_PreventChangeFromBondedToNonBonded_IfLocationHasTransitPackages()
		{
			var warehouse = Helper.CreateTRWWarehouse("WHS", "A", 5, 2);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			warehouse.Rows.Add(row);

			var bondedArea = Helper.CreateArea(warehouse, "Bonded", AreaTypes.Codes.Bonded);
			var freeStoreArea = Helper.CreateArea(warehouse, "FreeStore", AreaTypes.Codes.FreeStore);
			Factory.Save();

			var dockDoor = warehouse.FindLocation("DOCK");
			var location = warehouse.FindLocation("A-1");
			location.WLV_WA_PickingArea = bondedArea.PK;
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var rtu = (BusinessObject)Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, dockDoor.PK);
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_WL_StagingLocation] = dockDoor.PK;

			var packageJob = Helper.CreatePackageJob(rtu);
			var package1 = (BusinessObject)Helper.CreatePackage(packageJob);
			var packageState = CreateWhsItemPackageState("ARV", location, rtu, package1);
			Factory.Save();

			var bondedError = "Locations containing packages in a Bonded Picking Area cannot change Picking Area.";
			AssertNoErrorContaining(location.WLV_WA_PickingAreaInfo, bondedError);

			location.WLV_WA_PickingArea = freeStoreArea.PK;
			AssertHasError("Should not allow changing bonded locations with stock to non-bonded", location.WLV_WA_PickingAreaInfo, bondedError);

			location.WLV_WA_PickingArea = bondedArea.PK;
			AssertNoErrorContaining(location.WLV_WA_PickingAreaInfo, bondedError);
		}

		public void TestValidateWLV_WA_PickingArea_InwardProcessing_IfPutawayAreaIsInwardProcessing()
		{
			var warehouse = Helper.CreateWarehouse("WHS9");
			warehouse.WW_IsVirtualWarehouse = true;
			var inwardProcessingArea1 = Helper.CreateArea(warehouse, "IP1", AreaTypes.Codes.InwardProcessing);
			var inwardProcessingArea2 = Helper.CreateArea(warehouse, "IP2", AreaTypes.Codes.InwardProcessing);

			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A");
			var location = row.Locations[0];

			const string errorMessage = "When using Inward Processing Areas both the Picking Area and Putaway Area must be marked as Inward Processing Areas.";
			location.WLV_WA_PickingArea = inwardProcessingArea1.PK;
			AssertHasError(location.WLV_WA_PickingAreaInfo, errorMessage);

			location.WLV_WA_PutawayArea = inwardProcessingArea2.PK;
			AssertNoErrorContaining(location.WLV_WA_PickingAreaInfo, errorMessage);
			AssertNoErrorContaining(location.WLV_WA_PutawayAreaInfo, errorMessage);
		}

		public void TestValidateWLV_WA_PickingArea_InwardProcessing_CannotConvertFromWithStockOnHand()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var freeStoreArea = Helper.CreateArea(data.Whs1, "FRE", AreaTypes.Codes.FreeStore);
			var inwardProcessingArea1 = Helper.CreateArea(data.Whs1, "IP1", AreaTypes.Codes.InwardProcessing);
			var inwardProcessingArea2 = Helper.CreateArea(data.Whs1, "IP2", AreaTypes.Codes.InwardProcessing);
			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var receivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", Notify);
			transactionTestHelper.CreateWhsReceiveInventoryLine(receivePK, data.Part1.PK, 10m, ZDate.Empty, ZDate.Empty, "", "", "", "EntryKey-1", "A-1");
			transactionTestHelper.FinaliseDocket(receivePK);
			Factory.Save();

			// Hack, no other way to get inventory into an IPR area yet.
			location1.WLV_WA_PutawayArea = inwardProcessingArea1.PK;
			location1.WLV_WA_PickingArea = inwardProcessingArea1.PK;

			location2.WLV_WA_PutawayArea = inwardProcessingArea1.PK;
			location2.WLV_WA_PickingArea = inwardProcessingArea1.PK;
			Factory.Save();

			const string errorMessage = "Locations containing inventory cannot have their Area changed to or from an Inward Processing Area.";
			location1.WLV_WA_PickingArea = freeStoreArea.PK;
			location1.WLV_WA_PutawayArea = freeStoreArea.PK;
			AssertHasError("Should not be able to change the area on IPR areas with inventory.", location1.WLV_WA_PickingAreaInfo, errorMessage);

			location1.WLV_WA_PickingArea = inwardProcessingArea2.PK;
			location1.WLV_WA_PutawayArea = inwardProcessingArea2.PK;
			AssertNoErrorContaining(location1.WLV_WA_PickingAreaInfo, errorMessage);

			location1.WLV_WA_PickingArea = inwardProcessingArea1.PK;
			location1.WLV_WA_PutawayArea = inwardProcessingArea1.PK;
			AssertNoErrorContaining(location1.WLV_WA_PickingAreaInfo, errorMessage);

			location2.WLV_WA_PickingArea = freeStoreArea.PK;
			location2.WLV_WA_PutawayArea = freeStoreArea.PK;
			AssertNoErrorContaining("Should be able to change the area on IPR areas with no inventory.", location1.WLV_WA_PickingAreaInfo, errorMessage);
		}

		public void TestValidateWLV_WA_PickingArea_InwardProcessing_CannotConvertToWithStockOnHand()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var otherFreeStoreArea = Helper.CreateArea(data.Whs1, "FRE", AreaTypes.Codes.FreeStore);
			var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IP1", AreaTypes.Codes.InwardProcessing);
			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");

			var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var receivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", Notify);
			transactionTestHelper.CreateWhsReceiveInventoryLine(receivePK, data.Part1.PK, 10m, ZDate.Empty, ZDate.Empty, "", "", "", "EntryKey-1", "A-1");
			transactionTestHelper.FinaliseDocket(receivePK);
			Factory.Save();

			const string errorMessage = "Locations containing inventory cannot have their Area changed to or from an Inward Processing Area.";
			location1.WLV_WA_PickingArea = inwardProcessingArea.PK;
			location1.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			AssertHasError("Should not be able to change the area on IPR areas with inventory.", location1.WLV_WA_PickingAreaInfo, errorMessage);

			location1.WLV_WA_PickingArea = otherFreeStoreArea.PK;
			location1.WLV_WA_PutawayArea = otherFreeStoreArea.PK;
			AssertNoErrorContaining(location1.WLV_WA_PickingAreaInfo, errorMessage);
		}

		#endregion

		#region TestValidateWLV_WA_PutawayArea

		public void TestValidateWLV_WA_PutawayArea()
		{
			const string errorMessage = "Enter a valid Putaway Area.";
			var warehouse = Helper.CreateWarehouse("W1");
			var differentWarehouse = Helper.CreateWarehouse("W2");
			var pickingArea = Helper.CreateArea(warehouse, "A1", AreaTypes.Codes.FreeStore, true, false);
			var putawayArea = Helper.CreateArea(warehouse, "A2", AreaTypes.Codes.FreeStore, false, true);
			var bothArea = Helper.CreateArea(warehouse, "A3", AreaTypes.Codes.FreeStore, true, true);
			var putawayAreaInDifferentWarehouse = Helper.CreateArea(differentWarehouse, "A4", AreaTypes.Codes.FreeStore, false, true);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "R1", 1, 1);
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = pickingArea.PK;
			AssertHasError("Area set is picking area therefore must have errors", location.WLV_WA_PutawayAreaInfo, errorMessage);

			location.WLV_WA_PutawayArea = putawayArea.PK;
			AssertNoErrors("Area set is putaway area therefore must have errors.", location.WLV_WA_PutawayAreaInfo);

			location.WLV_WA_PutawayArea = bothArea.PK;
			AssertNoErrors("Setting an area with Both area type must not have any errors.", location.WLV_WA_PutawayAreaInfo);

			location.WLV_WA_PutawayArea = ZGuid.NewZGuid();
			AssertHasError("Area PK does not exists therefore must have errors.", location.WLV_WA_PutawayAreaInfo, errorMessage);

			location.WLV_WA_PutawayArea = ZGuid.Empty;
			AssertHasError("Area PK is empty therefore must have errors.", location.WLV_WA_PutawayAreaInfo, "Please enter a Putaway Area.");

			location.WLV_WA_PutawayArea = putawayAreaInDifferentWarehouse.PK;
			AssertHasError("Putaway area in a different warehouse must not be valid.", location.WLV_WA_PutawayAreaInfo, errorMessage);
		}

		public void TestValidateWLV_WA_PutawayArea_PreventChangeFromBondedToNonBonded_IfLocationHasSOH()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var bondedArea = Helper.CreateArea(data.Whs1, "Bonded", AreaTypes.Codes.Bonded);
			var freeStoreArea = Helper.CreateArea(data.Whs1, "FreeStore", AreaTypes.Codes.FreeStore);

			var location = data.Whs1.FindLocation("A-1");
			location.WLV_WA_PickingArea = bondedArea.PK;
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var receivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", "CUS", Notify);
			transactionTestHelper.CreateWhsReceiveInventoryLine(receivePK, data.Part1.PK, 10m, ZDate.Empty, ZDate.Empty, "", "", "", "EntryKey", "A-1");
			transactionTestHelper.FinaliseDocket(receivePK);
			Factory.Save();

			var bondedSOHError = "Locations containing Customs inventory in a Bonded Putaway Area cannot change Putaway Area.";
			AssertNoErrorContaining(location.WLV_WA_PutawayAreaInfo, bondedSOHError);

			location.WLV_WA_PutawayArea = freeStoreArea.PK;
			AssertHasError("Should not allow changing bonded locations with stock to non-bonded", location.WLV_WA_PutawayAreaInfo, bondedSOHError);

			location.WLV_WA_PutawayArea = bondedArea.PK;
			AssertNoErrorContaining(location.WLV_WA_PutawayAreaInfo, bondedSOHError);
		}

		public void TestValidateWLV_WA_PutawayArea_PreventChangeFromBondedToNonBonded_IfLocationHasTransitPackages()
		{
			var warehouse = Helper.CreateTRWWarehouse("WHS", "A", 5, 2);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			warehouse.Rows.Add(row);

			var bondedArea = Helper.CreateArea(warehouse, "Bonded", AreaTypes.Codes.Bonded);
			var freeStoreArea = Helper.CreateArea(warehouse, "FreeStore", AreaTypes.Codes.FreeStore);
			Factory.Save();

			var dockDoor = warehouse.FindLocation("DOCK");
			var location = warehouse.FindLocation("A-1");
			location.WLV_WA_PickingArea = bondedArea.PK;
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var rtu = (BusinessObject)Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, dockDoor.PK);
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_WL_StagingLocation] = dockDoor.PK;

			var packageJob = Helper.CreatePackageJob(rtu);
			var package1 = (BusinessObject)Helper.CreatePackage(packageJob);
			var packageState = CreateWhsItemPackageState("ARV", location, rtu, package1);
			Factory.Save();

			var bondedError = "Locations containing packages in a Bonded Putaway Area cannot change Putaway Area.";
			AssertNoErrorContaining(location.WLV_WA_PutawayAreaInfo, bondedError);

			location.WLV_WA_PutawayArea = freeStoreArea.PK;
			AssertHasError("Should not allow changing bonded locations with stock to non-bonded", location.WLV_WA_PutawayAreaInfo, bondedError);

			location.WLV_WA_PutawayArea = bondedArea.PK;
			AssertNoErrorContaining(location.WLV_WA_PutawayAreaInfo, bondedError);
		}

		public void TestValidateWLV_WA_PutawayArea_InwardProcessing_IfPickingAreaIsInwardProcessing()
		{
			var warehouse = Helper.CreateWarehouse("WHS9");
			warehouse.WW_IsVirtualWarehouse = true;
			var inwardProcessingArea1 = Helper.CreateArea(warehouse, "IP1", AreaTypes.Codes.InwardProcessing);
			var inwardProcessingArea2 = Helper.CreateArea(warehouse, "IP2", AreaTypes.Codes.InwardProcessing);

			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A");
			var location = row.Locations[0];

			const string errorMessage = "When using Inward Processing Areas both the Picking Area and Putaway Area must be marked as Inward Processing Areas.";
			location.WLV_WA_PutawayArea = inwardProcessingArea1.PK;
			AssertHasError(location.WLV_WA_PutawayAreaInfo, errorMessage);

			location.WLV_WA_PickingArea = inwardProcessingArea2.PK;
			AssertNoErrorContaining(location.WLV_WA_PutawayAreaInfo, errorMessage);
			AssertNoErrorContaining(location.WLV_WA_PickingAreaInfo, errorMessage);
		}

		public void TestValidateWLV_WA_PutawayArea_InwardProcessing_CannotConvertFromWithStockOnHand()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var freeStoreArea = Helper.CreateArea(data.Whs1, "FRE", AreaTypes.Codes.FreeStore);
			var inwardProcessingArea1 = Helper.CreateArea(data.Whs1, "IP1", AreaTypes.Codes.InwardProcessing);
			var inwardProcessingArea2 = Helper.CreateArea(data.Whs1, "IP2", AreaTypes.Codes.InwardProcessing);
			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var receivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", Notify);
			transactionTestHelper.CreateWhsReceiveInventoryLine(receivePK, data.Part1.PK, 10m, ZDate.Empty, ZDate.Empty, "", "", "", "EntryKey-1", "A-1");
			transactionTestHelper.FinaliseDocket(receivePK);
			Factory.Save();

			// Hack, no other way to get inventory into an IPR area yet.
			location1.WLV_WA_PutawayArea = inwardProcessingArea1.PK;
			location1.WLV_WA_PickingArea = inwardProcessingArea1.PK;

			location2.WLV_WA_PutawayArea = inwardProcessingArea1.PK;
			location2.WLV_WA_PickingArea = inwardProcessingArea1.PK;
			Factory.Save();

			const string errorMessage = "Locations containing inventory cannot have their Area changed to or from an Inward Processing Area.";
			location1.WLV_WA_PutawayArea = freeStoreArea.PK;
			location1.WLV_WA_PickingArea = freeStoreArea.PK;
			AssertHasError("Should not be able to change the area on IPR areas with inventory.", location1.WLV_WA_PutawayAreaInfo, errorMessage);

			location1.WLV_WA_PutawayArea = inwardProcessingArea2.PK;
			location1.WLV_WA_PickingArea = inwardProcessingArea2.PK;
			AssertNoErrorContaining(location1.WLV_WA_PutawayAreaInfo, errorMessage);

			location1.WLV_WA_PutawayArea = inwardProcessingArea1.PK;
			location1.WLV_WA_PickingArea = inwardProcessingArea1.PK;
			AssertNoErrorContaining(location1.WLV_WA_PutawayAreaInfo, errorMessage);

			location2.WLV_WA_PutawayArea = freeStoreArea.PK;
			location2.WLV_WA_PickingArea = freeStoreArea.PK;
			AssertNoErrorContaining("Should be able to change the area on IPR areas with no inventory.", location1.WLV_WA_PutawayAreaInfo, errorMessage);
		}

		public void TestValidateWLV_WA_PutawayArea_InwardProcessing_CannotConvertToWithStockOnHand()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var otherFreeStoreArea = Helper.CreateArea(data.Whs1, "FRE", AreaTypes.Codes.FreeStore);
			var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IP1", AreaTypes.Codes.InwardProcessing);
			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");

			var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var receivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", Notify);
			transactionTestHelper.CreateWhsReceiveInventoryLine(receivePK, data.Part1.PK, 10m, ZDate.Empty, ZDate.Empty, "", "", "", "EntryKey-1", "A-1");
			transactionTestHelper.FinaliseDocket(receivePK);
			Factory.Save();

			const string errorMessage = "Locations containing inventory cannot have their Area changed to or from an Inward Processing Area.";
			location1.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			location1.WLV_WA_PickingArea = inwardProcessingArea.PK;
			AssertHasError("Should not be able to change the area on IPR areas with inventory.", location1.WLV_WA_PutawayAreaInfo, errorMessage);

			location1.WLV_WA_PutawayArea = otherFreeStoreArea.PK;
			location1.WLV_WA_PickingArea = otherFreeStoreArea.PK;
			AssertNoErrorContaining(location1.WLV_WA_PutawayAreaInfo, errorMessage);
		}

		#endregion

		#endregion

		#region TestValidateWLV_MaxWeight

		public void TestValidateWLV_MaxWeight()
		{
			var row = Factory.New<WhsRow>();
			var locn = row.Locations.AddNew();
			TestMinDecimal(locn.WLV_MaxWeightInfo, ErrorCheckType.HasErrors, 0);
		}

		public void TestValidateWLV_MaxWeight_PickFace()
		{
			TestValidateMaxCapacityCore(LocationClasses.Codes.DPF, true, location => location.WLV_MaxWeightInfo);
		}

		public void TestValidateWLV_MaxWeight_FixedLocation()
		{
			TestValidateMaxCapacityCore(LocationClasses.Codes.FIX, true, location => location.WLV_MaxWeightInfo);
		}

		public void TestValidateWLV_MaxWeight_NonPickFaceLocation()
		{
			TestValidateMaxCapacityCore(LocationClasses.Codes.NOR, false, location => location.WLV_MaxWeightInfo);
		}

		void TestValidateMaxCapacityCore(string locationClass, bool expectedHasError, Func<WhsLocation, ZPropertyInfo> propertyFunc)
		{
			var expectedMessage = "The max capacity of Fixed or Dynamic Pick Face Location must be 0.";
			var locationType = locationClass == "FIX" ? Helper.CreateLocationType("AAA", "AAA Test", false, 1, locationClass) : Helper.CreateLocationType("AAA", locationClass);
			var warehouse = Helper.CreateWarehouse("WHS1");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 1);
			var location = row.Locations[0];
			location.WLV_WLT_LocationType = locationType.PK;
			Factory.Save();

			var propertyInfo = propertyFunc(location);
			AssertEquals("Precondition:", 0m, (ZDecimal)propertyInfo.Value);
			AssertNoErrors("When Value is 0m should not have error", propertyInfo);

			location.SetPropertyValue(propertyInfo.Name, (ZDecimal)5m);
			if (expectedHasError)
			{
				AssertHasError("When Location Type is FIX or DPF and Value > 0 should have error", propertyInfo, expectedMessage);
			}
			else
			{
				AssertNoErrors("When Location Type is not FIX or DPF and Value > 0 should have error", propertyInfo);
			}

			location.SetPropertyValue(propertyInfo.Name, (ZDecimal)0m);
			AssertNoErrors("When Value is 0m should not have error", propertyInfo);
		}

		#endregion

		#region TestValidateWLV_MaxCubic

		public void TestValidateWLV_MaxCubic()
		{
			var row = Factory.New<WhsRow>();
			var locn = row.Locations.AddNew();
			TestMinDecimal(locn.WLV_MaxCubicInfo, ErrorCheckType.HasErrors, 0);
		}

		public void TestValidateWLV_MaxCubic_PickFace()
		{
			TestValidateMaxCapacityCore(LocationClasses.Codes.DPF, true, location => location.WLV_MaxWeightInfo);
		}

		public void TestValidateWLV_MaxCubic_FixedLocation()
		{
			TestValidateMaxCapacityCore(LocationClasses.Codes.FIX, true, location => location.WLV_MaxCubicInfo);
		}

		public void TestValidateWLV_MaxCubic_NonPickFaceLocation()
		{
			TestValidateMaxCapacityCore(LocationClasses.Codes.NOR, false, location => location.WLV_MaxCubicInfo);
		}

		#endregion

		#region TestValidateWLV_MaxHeight

		public void TestValidateWLV_MaxHeight()
		{
			var row = Factory.New<WhsRow>();
			var locn = row.Locations.AddNew();
			AssertNoError(locn.WLV_MaxHeightInfo, "Please enter a 'Max Height' greater than or equal to 0.");

			locn.WLV_MaxHeight = -1m;
			AssertHasError(locn.WLV_MaxHeightInfo, "Please enter a 'Max Height' greater than or equal to 0.");

			locn.WLV_MaxHeight = 1m;
			AssertNoError(locn.WLV_MaxHeightInfo, "Please enter a 'Max Height' greater than or equal to 0.");
		}

		public void TestValidateWLV_MaxHeight_MaxDimensionsAllOrNothing() => TestValidateMaxDimensionsAllOrNothingCore((l, dim) => l.WLV_MaxHeight = dim);

		void TestValidateMaxDimensionsAllOrNothingCore(Action<WhsLocation, decimal> setMaxDimension)
		{
			var row = Factory.New<WhsRow>();
			var locn = row.Locations.AddNew();
			locn.WLV_MaxDimensionUnit = "M";
			AssertNoError(locn.WLV_MaxHeightInfo, "Either all or no Max Dimensions must be entered.");
			AssertNoError(locn.WLV_MaxDepthInfo, "Either all or no Max Dimensions must be entered.");
			AssertNoError(locn.WLV_MaxWidthInfo, "Either all or no Max Dimensions must be entered.");

			setMaxDimension(locn, 1m);
			AssertHasError(locn.WLV_MaxHeightInfo, "Either all or no Max Dimensions must be entered.");
			AssertHasError(locn.WLV_MaxDepthInfo, "Either all or no Max Dimensions must be entered.");
			AssertHasError(locn.WLV_MaxWidthInfo, "Either all or no Max Dimensions must be entered.");

			setMaxDimension(locn, 0m);
			AssertNoError(locn.WLV_MaxHeightInfo, "Either all or no Max Dimensions must be entered.");
			AssertNoError(locn.WLV_MaxDepthInfo, "Either all or no Max Dimensions must be entered.");
			AssertNoError(locn.WLV_MaxWidthInfo, "Either all or no Max Dimensions must be entered.");

			locn.WLV_MaxDepth = 1m;
			locn.WLV_MaxWidth = 1m;
			locn.WLV_MaxHeight = 1m;
			AssertNoError(locn.WLV_MaxHeightInfo, "Either all or no Max Dimensions must be entered.");
			AssertNoError(locn.WLV_MaxDepthInfo, "Either all or no Max Dimensions must be entered.");
			AssertNoError(locn.WLV_MaxWidthInfo, "Either all or no Max Dimensions must be entered.");

			setMaxDimension(locn, 0m);
			AssertHasError(locn.WLV_MaxHeightInfo, "Either all or no Max Dimensions must be entered.");
			AssertHasError(locn.WLV_MaxDepthInfo, "Either all or no Max Dimensions must be entered.");
			AssertHasError(locn.WLV_MaxWidthInfo, "Either all or no Max Dimensions must be entered.");
		}

		#endregion

		#region TestValidateWLV_MaxDepth

		public void TestValidateWLV_MaxDepth()
		{
			var row = Factory.New<WhsRow>();
			var locn = row.Locations.AddNew();
			AssertNoError(locn.WLV_MaxDepthInfo, "Please enter a 'Max Depth' greater than or equal to 0.");

			locn.WLV_MaxDepth = -1m;
			AssertHasError(locn.WLV_MaxDepthInfo, "Please enter a 'Max Depth' greater than or equal to 0.");

			locn.WLV_MaxDepth = 1m;
			AssertNoError(locn.WLV_MaxDepthInfo, "Please enter a 'Max Depth' greater than or equal to 0.");
		}

		public void TestValidateWLV_MaxDepth_MaxDimensionsAllOrNothing() => TestValidateMaxDimensionsAllOrNothingCore((l, dim) => l.WLV_MaxDepth = dim);

		#endregion

		#region TestValidateWLV_MaxWidth

		public void TestValidateWLV_MaxWidth()
		{
			var row = Factory.New<WhsRow>();
			var locn = row.Locations.AddNew();
			AssertNoError(locn.WLV_MaxWidthInfo, "Please enter a 'Max Width' greater than or equal to 0.");

			locn.WLV_MaxWidth = -1m;
			AssertHasError(locn.WLV_MaxWidthInfo, "Please enter a 'Max Width' greater than or equal to 0.");

			locn.WLV_MaxWidth = 1m;
			AssertNoError(locn.WLV_MaxWidthInfo, "Please enter a 'Max Width' greater than or equal to 0.");
		}

		public void TestValidateWLV_MaxWidth_MaxDimensionsAllOrNothing() => TestValidateMaxDimensionsAllOrNothingCore((l, dim) => l.WLV_MaxWidth = dim);

		#endregion

		#region TestValidateWLV_MaxQuantity

		public void TestValidateWLV_MaxQuantity()
		{
			var row = Factory.New<WhsRow>();
			var locn = row.Locations.AddNew();
			TestMinDecimal(locn.WLV_MaxQuantityInfo, ErrorCheckType.HasErrors, 0);
		}

		public void TestValidateWLV_MaxQuantity_PickFace()
		{
			TestValidateMaxCapacityCore(LocationClasses.Codes.DPF, true, location => location.WLV_MaxQuantityInfo);
		}

		public void TestValidateWLV_MaxQuantity_FixedLocation()
		{
			TestValidateMaxCapacityCore(LocationClasses.Codes.FIX, true, location => location.WLV_MaxQuantityInfo);
		}

		public void TestValidateWLV_MaxQuantity_NonPickFaceLocation()
		{
			TestValidateMaxCapacityCore(LocationClasses.Codes.NOR, false, location => location.WLV_MaxQuantityInfo);
		}

		public void TestValidateWLV_MaxQuantity_CapacityCheck()
		{
			var factory = new BusinessObjectFactory();
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(factory);
			var whs = helper.CreateWarehouse("WHS-1", "A", 2, 1) as WhsWarehouse;
			var clientPK = helper.CreateClient("CL1");
			var product = helper.CreateProduct(clientPK, "P1");
			factory.Save();

			var locationA1 = whs.FindLocation("A-1");
			var locationA2 = whs.FindLocation("A-2");
			var dockDoorLocationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_LocationClass, LocationClasses.Codes.DDL));
			locationA1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			factory.Save();

			var receivePK = helper.CreateWhsReceive(clientPK, whs.PK, "R1", Notify);
			helper.CreateWhsReceiveInventoryLine(receivePK, product.PK, 10m, locationA1.PK);

			var receive2PK = helper.CreateWhsReceive(clientPK, whs.PK, "R2", Notify);
			var receive2LinePK = helper.CreateWhsReceiveInventoryLine(receivePK, product.PK, 5m, locationA1.PK);
			var receive2Line = factory.Load<IWhsReceiveLine>(receive2LinePK);
			receive2Line.WE_PalletID = "PLT-1";
			factory.Save();

			var transferPK = helper.CreateWhsTransfer(clientPK, whs.PK, "TR1", Notify);
			var transfer = (BusinessObject)factory.Load<IWhsDocket>(transferPK);
			transfer[WhsDocketSchema.WD_IsPutawayTransfer] = true;
			var transferLinePK = helper.CreateWhsTransferLine(transferPK, product.PK, 5m, locationA1.PK, locationA2.PK);
			var transferLine = (BusinessObject)factory.Load<IWhsDocketLine>(transferLinePK);
			transferLine[WhsDocketLineSchema.WE_TransferFromPalletId] = "PLT-1";
			transferLine[WhsDocketLineSchema.WE_PalletID] = "PLT-1";
			transferLine[WhsDocketLineSchema.WE_OriginalInventoryStatus] = "REC";
			helper.FinaliseDocket(transferPK);
			AssertEquals("Precondition", 0m, receive2Line.WE_StockOnHand);
			AssertEquals("Precondition", "PFU", receive2Line.WE_DocketLineStatus);

			factory.Save();

			locationA1.WLV_MaxQuantity = 15;
			AssertNoErrors(locationA1.WLV_MaxQuantityInfo);
			factory.Save();

			locationA1.WLV_MaxQuantity = 6;
			AssertHasError("Should have an error.", locationA1.WLV_MaxQuantityInfo, "There are currently 10 units consuming capacity for this location, you cannot reduce the Max Capacity below this amount.");

			locationA1.WLV_MaxQuantity = 9;
			AssertHasError("Should have an error.", locationA1.WLV_MaxQuantityInfo, "There are currently 10 units consuming capacity for this location, you cannot reduce the Max Capacity below this amount.");

			locationA1.WLV_MaxQuantity = 10;
			AssertNoErrors(locationA1.WLV_MaxQuantityInfo);

			locationA1.WLV_MaxQuantity = 0;
			AssertNoErrors(locationA1.WLV_MaxQuantityInfo);
			factory.Save();

			locationA1.WLV_MaxQuantity = 6;
			AssertHasError("Changing WLV_MaxQuantity from 0 to 6 is a reduction of capacity.", locationA1.WLV_MaxQuantityInfo, "There are currently 10 units consuming capacity for this location, you cannot reduce the Max Capacity below this amount.");
		}

		public void TestValidateWLV_ChangeMaxQuantityWSA_With_AllocatedQuantity()
		{
			var factory = new BusinessObjectFactory();
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(factory);
			var whs = helper.CreateWarehouse("WHS-1", "A", 2, 1) as WhsWarehouse;
			whs.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			factory.Save();

			var locationA1 = whs.FindLocation("A-1");
			var locationType = factory.New<WhsLocationType>();
			locationType.WLT_Code = LocationClasses.Codes.WSA;
			locationType.WLT_Description = LocationClasses.Descriptions.WSA;
			factory.Save();

			locationA1.WLV_WLT_LocationType = locationType.PK;
			factory.Save();

			string command1 = $"INSERT INTO CYDTransportationUnit (YTU_PK, YTU_SystemCreateTimeUtc, YTU_SystemLastEditTimeUtc, YTU_WW_Yard, YTU_TransportationUnitID, YTU_WL_WaitingBayLocation, YTU_GateInTime, YTU_SystemCreateUser, YTU_SystemLastEditUser) " +
				$"VALUES ('BB89B793-5DC6-4988-B060-246DD2B07C66', '2024-09-25', '2024-09-25', '{whs.PK}', 'YTU0001', '{locationA1.PK}', '2024-09-25', 'TST', 'TST')";

			string command2 = $"INSERT INTO CYDTransportationUnit (YTU_PK, YTU_SystemCreateTimeUtc, YTU_SystemLastEditTimeUtc, YTU_WW_Yard, YTU_TransportationUnitID, YTU_WL_WaitingBayLocation, YTU_GateInTime, YTU_SystemCreateUser, YTU_SystemLastEditUser) " +
				$"VALUES ('CC89B793-5DC6-4988-B060-246DD2B07C66', '2024-09-25', '2024-09-25', '{whs.PK}', 'YTU0002', '{locationA1.PK}', '2024-09-25', 'TST', 'TST')";

			string command3 = $"INSERT INTO CYDTransportationUnit (YTU_PK, YTU_SystemCreateTimeUtc, YTU_SystemLastEditTimeUtc, YTU_WW_Yard, YTU_TransportationUnitID, YTU_WL_WaitingBayLocation, YTU_GateInTime, YTU_SystemCreateUser, YTU_SystemLastEditUser) " +
				$"VALUES ('DD89B793-5DC6-4988-B060-246DD2B07C66', '2024-09-25', '2024-09-25', '{whs.PK}', 'YTU0003', '{locationA1.PK}', '2024-09-25', 'TST', 'TST')";
			Db.Connection.ExecuteNonQuery(command1);
			Db.Connection.ExecuteNonQuery(command2);
			Db.Connection.ExecuteNonQuery(command3);
			factory.Save();

			locationA1.WLV_MaxQuantity = 5;
			AssertNoErrors(locationA1.WLV_MaxQuantityInfo);

			locationA1.WLV_MaxQuantity = 3;
			AssertNoErrors(locationA1.WLV_MaxQuantityInfo);

			locationA1.WLV_MaxQuantity = 2;
			AssertHasError("Should have an error.", locationA1.WLV_MaxQuantityInfo, "There are currently 3 units consuming capacity for this location, you cannot reduce the Max Capacity below this amount.");
		}

		public void TestValidateWLV_MaxQuantity_StagedInventory()
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var whs = helper.CreateWarehouse("WHS-1", "A") as WhsWarehouse;
			var clientPK = helper.CreateClient("CL1");
			var product = helper.CreateProduct(clientPK, "P1");
			var receivePK = helper.CreateWhsReceive(clientPK, whs.PK, "R1", Notify);
			helper.CreateWhsReceiveInventoryLine(receivePK, product.PK, 10m, whs.DefaultLocation.PK);
			helper.FinaliseDocket(receivePK);
			Factory.Save();

			var orderPk = helper.CreateWhsOrder(clientPK, whs.PK, "O1", Notify);
			helper.CreateWhsOrderLine(orderPk, product.PK, 10m);
			var pickPk = helper.CreateWhsPick(new[] { orderPk });
			var pickLine = helper.GetPickLines(pickPk).Single();
			var transferLine = helper.PickAndMakeInTransitTransfer(pickLine, ZDateTime.Today);
			helper.FinaliseDocketLine(transferLine.PK);
			Factory.Save();

			AssertEquals("Precondition.", "STA", transferLine.WE_CurrentInventoryStatus);

			var dockDoorLocation = whs.DefaultOutboundDockDoorLocation;
			dockDoorLocation.WLV_MaxQuantity = 15m;
			AssertNoErrors(dockDoorLocation.WLV_MaxQuantityInfo);
			Factory.Save();

			dockDoorLocation.WLV_MaxQuantity = 9m;
			AssertHasError("Should have an error.", dockDoorLocation.WLV_MaxQuantityInfo, "There are currently 10 units consuming capacity for this location, you cannot reduce the Max Capacity below this amount.");

			dockDoorLocation.WLV_MaxQuantity = 10m;
			AssertNoErrors(dockDoorLocation.WLV_MaxQuantityInfo);
		}

		public void TestValidateWLV_MaxQuantity_WithCancelledReceive()
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var whs = helper.CreateWarehouse("WHS-1", "A") as WhsWarehouse;
			var clientPK = helper.CreateClient("CL1");
			var product = helper.CreateProduct(clientPK, "P1");

			var dockDoorLocation = whs.DefaultOutboundDockDoorLocation;
			var receive1PK = helper.CreateWhsReceive(clientPK, whs.PK, "R1", Notify);
			helper.CreateWhsReceiveInventoryLine(receive1PK, product.PK, 10m, dockDoorLocation.PK);

			var cancelledReceivePK = helper.CreateWhsReceive(clientPK, whs.PK, "R2", Notify);
			var cancelledReceiveLinePK = helper.CreateWhsReceiveInventoryLine(cancelledReceivePK, product.PK, 5m, dockDoorLocation.PK);

			var cancelledReceive = Factory.Load<IWhsReceive>(cancelledReceivePK);
			cancelledReceive.WD_DocketStatus = "CAN";

			var cancelledReceiveLine = Factory.Load<IWhsReceiveLine>(cancelledReceiveLinePK);
			cancelledReceiveLine.WE_StockOnHand = 0m;
			cancelledReceiveLine.WE_DocketLineStatus = "CAN";
			Factory.Save();

			dockDoorLocation.WLV_MaxQuantity = 15m;
			AssertNoErrors("Should have no error.", dockDoorLocation.WLV_MaxQuantityInfo);
			Factory.Save();

			dockDoorLocation.WLV_MaxQuantity = 10m;
			AssertNoErrors("Should have no error.", dockDoorLocation.WLV_MaxQuantityInfo);

			dockDoorLocation.WLV_MaxQuantity = 9m;
			AssertHasError("Should have an error.", dockDoorLocation.WLV_MaxQuantityInfo, "There are currently 10 units consuming capacity for this location, you cannot reduce the Max Capacity below this amount.");
		}

		public void TestValidateWLV_Default_MaxQuantity_CYD()
		{
			TestValidateWLV_Default_MaxQuantityCore(WarehouseTypes.Codes.ContainerYard, 1.0m);
		}

		public void TestValidateWLV_Default_MaxQuantity_FTZ()
		{
			TestValidateWLV_Default_MaxQuantityCore(WarehouseTypes.Codes.FreeTradeZone, 0m);
		}

		public void TestValidateWLV_Default_MaxQuantity_PRW()
		{
			TestValidateWLV_Default_MaxQuantityCore(WarehouseTypes.Codes.Product, 0m);
		}

		public void TestValidateWLV_Default_MaxQuantity_TRW()
		{
			TestValidateWLV_Default_MaxQuantityCore(WarehouseTypes.Codes.Transit, 0m);
		}

		public void TestValidateWLV_Default_MaxQuantityCore(ZString warehouseType, ZDecimal defaultMaxQuantity)
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var whs = helper.CreateWarehouse("WHS-1", "A") as WhsWarehouse;
			whs.WW_WarehouseType = warehouseType;
			Factory.Save();

			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 1);
			var location = row.Locations[0];
			AssertEquals("Should have an error.", location.WLV_MaxQuantity, defaultMaxQuantity);
		}

		public void TestValidateWLV_MaxQuantity_Zero_CYD()
		{
			TestValidateWLV_MaxQuantity_Zero_Core(WarehouseTypes.Codes.ContainerYard, "Please enter a number greater than or equal to 1 for max quantity.");
		}

		public void TestValidateWLV_MaxQuantity_Zero_FTZ()
		{
			TestValidateWLV_MaxQuantity_Zero_Core(WarehouseTypes.Codes.FreeTradeZone, "");
		}

		public void TestValidateWLV_MaxQuantity_Zero_PRW()
		{
			TestValidateWLV_MaxQuantity_Zero_Core(WarehouseTypes.Codes.Product, "");
		}

		public void TestValidateWLV_MaxQuantity_Zero_TRW()
		{
			TestValidateWLV_MaxQuantity_Zero_Core(WarehouseTypes.Codes.Transit, "");
		}

		public void TestValidateWLV_MaxQuantity_Zero_Core(ZString warehouseType, string errorMessage)
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var whs = helper.CreateWarehouse("WHS-1", "A") as WhsWarehouse;
			whs.WW_WarehouseType = warehouseType;
			Factory.Save();

			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 1);
			var location = row.Locations[0];

			location.WLV_MaxQuantity = 0m;
			if (!string.IsNullOrEmpty(errorMessage))
			{
				AssertHasError("Should have an error.", location.WLV_MaxQuantityInfo, errorMessage);
			}
			else
			{
				AssertNoErrors("Should have no error.", location.WLV_MaxQuantityInfo);
			}
		}

		public void TestValidateWLV_MaxQuantity_NonZero_CYD()
		{
			TestValidateWLV_MaxQuantity_NonZero_Core(WarehouseTypes.Codes.ContainerYard);
		}

		public void TestValidateWLV_MaxQuantity_NonZero_FTZ()
		{
			TestValidateWLV_MaxQuantity_NonZero_Core(WarehouseTypes.Codes.FreeTradeZone);
		}

		public void TestValidateWLV_MaxQuantity_NonZero_PRW()
		{
			TestValidateWLV_MaxQuantity_NonZero_Core(WarehouseTypes.Codes.Product);
		}

		public void TestValidateWLV_MaxQuantity_NonZero_TRW()
		{
			TestValidateWLV_MaxQuantity_NonZero_Core(WarehouseTypes.Codes.Transit);
		}

		public void TestValidateWLV_MaxQuantity_NonZero_Core(ZString warehouseType)
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var whs = helper.CreateWarehouse("WHS-1", "A") as WhsWarehouse;
			whs.WW_WarehouseType = warehouseType;
			Factory.Save();

			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 1);
			var location = row.Locations[0];

			location.WLV_MaxQuantity = 3m;
			AssertNoErrors("Should have no error.", location.WLV_MaxQuantityInfo);
		}

		#endregion

		#region TestValidateWLV_MaxWeightUnit

		public void TestValidateWLV_MaxWeightUnit()
		{
			var row = Factory.New<WhsRow>();
			var locn = row.Locations.AddNew();
			TestCodePairList(locn.WLV_MaxWeightUnitInfo, ErrorCheckType.HasErrors, true, new CodeDescriptionPairList(OLookUpEditType.Weight));

			var location = Factory.New<WhsLocation>();
			AssertNoErrors(location.WLV_MaxWeightUnitInfo);

			location.WLV_MaxWeight = 10m;
			AssertHasError(location.WLV_MaxWeightUnitInfo, "Please enter a Max Weight UQ.");

			location.WLV_MaxWeightUnit = "KG";
			AssertNoErrors(location.WLV_MaxWeightUnitInfo);

			location.WLV_MaxWeightUnit = "";
			AssertHasError("Precondition", location.WLV_MaxWeightUnitInfo, "Please enter a Max Weight UQ.");
			location.WLV_MaxWeight = 0m;
			AssertNoErrors(location.WLV_MaxWeightUnitInfo);
		}

		#endregion

		#region TestValidateWLV_MaxCubicUnit

		public void TestValidateWLV_MaxCubicUnit()
		{
			var row = Factory.New<WhsRow>();
			var locn = row.Locations.AddNew();
			TestCodePairList(locn.WLV_MaxCubicUnitInfo, ErrorCheckType.HasErrors, true, new CodeDescriptionPairList(OLookUpEditType.Volume));

			var location = Factory.New<WhsLocation>();
			AssertNoErrors(location.WLV_MaxCubicUnitInfo);

			location.WLV_MaxCubic = 10m;
			AssertHasError(location.WLV_MaxCubicUnitInfo, "Please enter a Max Cubic UQ.");

			location.WLV_MaxCubicUnit = "M3";
			AssertNoErrors(location.WLV_MaxCubicUnitInfo);

			location.WLV_MaxCubicUnit = "";
			AssertHasError("Precondition", location.WLV_MaxCubicUnitInfo, "Please enter a Max Cubic UQ.");
			location.WLV_MaxCubic = 0m;
			AssertNoErrors(location.WLV_MaxCubicUnitInfo);
		}

		#endregion

		#region TestValidateWLV_MaxDimensionUnit

		public void TestValidateWLV_MaxDimensionUnit()
		{
			var row = Factory.New<WhsRow>();
			var locn = row.Locations.AddNew();
			TestCodePairList(locn.WLV_MaxDimensionUnitInfo, ErrorCheckType.HasErrors, true, new CodeDescriptionPairList(OLookUpEditType.Length));
		}

		public void TestValidateWLV_MaxDimensionUnit_Height()
		{
			var location = Factory.New<WhsLocation>();
			AssertNoErrors(location.WLV_MaxDimensionUnitInfo);

			location.WLV_MaxHeight = 10m;
			AssertHasError(location.WLV_MaxDimensionUnitInfo, "Please enter a Max Dimension UQ.");

			location.WLV_MaxDimensionUnit = "M";
			AssertNoErrors(location.WLV_MaxDimensionUnitInfo);

			location.WLV_MaxDimensionUnit = "";
			AssertHasError("Precondition", location.WLV_MaxDimensionUnitInfo, "Please enter a Max Dimension UQ.");

			location.WLV_MaxHeight = 0m;
			AssertNoErrors(location.WLV_MaxDimensionUnitInfo);
		}

		public void TestValidateWLV_MaxDimensionUnit_Depth()
		{
			var location = Factory.New<WhsLocation>();
			AssertNoErrors(location.WLV_MaxDimensionUnitInfo);

			location.WLV_MaxDepth = 10m;
			AssertHasError(location.WLV_MaxDimensionUnitInfo, "Please enter a Max Dimension UQ.");

			location.WLV_MaxDimensionUnit = "M";
			AssertNoErrors(location.WLV_MaxDimensionUnitInfo);

			location.WLV_MaxDimensionUnit = "";
			AssertHasError("Precondition", location.WLV_MaxDimensionUnitInfo, "Please enter a Max Dimension UQ.");

			location.WLV_MaxDepth = 0m;
			AssertNoErrors(location.WLV_MaxDimensionUnitInfo);
		}

		public void TestValidateWLV_MaxDimensionUnit_Width()
		{
			var location = Factory.New<WhsLocation>();
			AssertNoErrors(location.WLV_MaxDimensionUnitInfo);

			location.WLV_MaxWidth = 10m;
			AssertHasError(location.WLV_MaxDimensionUnitInfo, "Please enter a Max Dimension UQ.");

			location.WLV_MaxDimensionUnit = "M";
			AssertNoErrors(location.WLV_MaxDimensionUnitInfo);

			location.WLV_MaxDimensionUnit = "";
			AssertHasError("Precondition", location.WLV_MaxDimensionUnitInfo, "Please enter a Max Dimension UQ.");

			location.WLV_MaxWidth = 0m;
			AssertNoErrors(location.WLV_MaxDimensionUnitInfo);
		}

		#endregion

		#region TestValidateWLV_PalletFloorSpaces

		public void TestValidateWLV_PalletFloorSpaces_IsZeroWhenPalletStackHeightIsSpecified()
		{
			var location = Factory.New<WhsLocation>();
			AssertNoErrors(location.WLV_PalletFloorSpacesInfo);

			location.WLV_PalletStackHeight = new ZByte(1);
			AssertHasError(location.WLV_PalletFloorSpacesInfo, "Pallet Floor Spaces must be set when a Pallet Stack Height is specified.");

			location.WLV_PalletStackHeight = new ZByte(0);
			AssertNoErrors(location.WLV_PalletFloorSpacesInfo);

			location.WLV_PalletStackHeight = new ZByte(1);
			AssertHasError(location.WLV_PalletFloorSpacesInfo, "Pallet Floor Spaces must be set when a Pallet Stack Height is specified.");

			location.WLV_PalletFloorSpaces = new ZByte(1);
			AssertNoErrors(location.WLV_PalletFloorSpacesInfo);
		}

		public void TestValidateWLV_PalletFloorSpacesAndStackHeight_NoErrorsWhenBothAreZero_WithExistingNonPalletStocks()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_PalletFloorSpaces = 0;
			location1.WLV_PalletStackHeight = 0;
			Factory.Save();

			var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var goodsReceivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", Notify);
			transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, "A-1", heldCode: "", palletID: "");
			transactionTestHelper.FinaliseDocket(goodsReceivePK);
			Factory.Save();

			location1.Validation.ValidateWLV_PalletFloorSpaces();
			location1.Validation.ValidateWLV_PalletStackHeight();
			AssertNoErrors(location1.WLV_PalletFloorSpacesInfo);
			AssertNoErrors(location1.WLV_PalletStackHeightInfo);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenReducing_CurrentPalletCountWillExceed_ProductWarehouse()
		{
			TestValidateWLV_PalletFloorSpaces_WhenReducing_CurrentPalletCountWillExceed_Core(WarehouseTypes.Codes.Product, shouldShowError: true);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenReducing_CurrentPalletCountWillExceed_FreeTradeZone()
		{
			TestValidateWLV_PalletFloorSpaces_WhenReducing_CurrentPalletCountWillExceed_Core(WarehouseTypes.Codes.FreeTradeZone, shouldShowError: true);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenReducing_CurrentPalletCountWillExceed_TransitWarehouse()
		{
			TestValidateWLV_PalletFloorSpaces_WhenReducing_CurrentPalletCountWillExceed_Core(WarehouseTypes.Codes.Transit, shouldShowError: false);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenReducing_CurrentPalletCountWillExceed_ContainerYard()
		{
			TestValidateWLV_PalletFloorSpaces_WhenReducing_CurrentPalletCountWillExceed_Core(WarehouseTypes.Codes.ContainerYard, shouldShowError: false);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenReducing_CurrentPalletCountWillExceed_Core(string warehouseType, bool shouldShowError)
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_WarehouseType = warehouseType;
			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_PalletFloorSpaces = 2;
			location1.WLV_PalletStackHeight = 1;
			Factory.Save();

			if (warehouseType == WarehouseTypes.Codes.Product || warehouseType == WarehouseTypes.Codes.FreeTradeZone)
			{
				var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
				var goodsReceivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", Notify);
				transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, "A-1", heldCode: "", palletID: "PLT01");
				transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, "A-1", heldCode: "", palletID: "PLT02");
				transactionTestHelper.FinaliseDocket(goodsReceivePK);
				Factory.Save();
			}

			location1.WLV_PalletFloorSpaces = 1;
			if (shouldShowError)
			{
				AssertHasError(location1.WLV_PalletFloorSpacesInfo, "Pallet Spaces (1) cannot be lower than existing Pallet Count (2) for this location.");
			}
			else
			{
				AssertNoErrors(location1.WLV_PalletFloorSpacesInfo);
			}
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenReducing_CurrentPalletCountWillNotExceed_ProductWarehouse()
		{
			TestValidateWLV_PalletFloorSpaces_WhenReducing_CurrentPalletCountWillNotExceed_Core(WarehouseTypes.Codes.Product);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenReducing_CurrentPalletCountWillNotExceed_FreeTradeZone()
		{
			TestValidateWLV_PalletFloorSpaces_WhenReducing_CurrentPalletCountWillNotExceed_Core(WarehouseTypes.Codes.FreeTradeZone);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenReducing_CurrentPalletCountWillNotExceed_TransitWarehouse()
		{
			TestValidateWLV_PalletFloorSpaces_WhenReducing_CurrentPalletCountWillNotExceed_Core(WarehouseTypes.Codes.Transit);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenReducing_CurrentPalletCountWillNotExceed_ContainerYard()
		{
			TestValidateWLV_PalletFloorSpaces_WhenReducing_CurrentPalletCountWillNotExceed_Core(WarehouseTypes.Codes.ContainerYard);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenReducing_CurrentPalletCountWillNotExceed_Core(string warehouseType)
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_WarehouseType = warehouseType;
			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_PalletFloorSpaces = 3;
			location1.WLV_PalletStackHeight = 1;
			Factory.Save();

			if (warehouseType == WarehouseTypes.Codes.Product || warehouseType == WarehouseTypes.Codes.FreeTradeZone)
			{
				var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
				var goodsReceivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", Notify);
				transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, "A-1", heldCode: "", palletID: "PLT01");
				transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, "A-1", heldCode: "", palletID: "PLT02");
				transactionTestHelper.FinaliseDocket(goodsReceivePK);
				Factory.Save();
			}

			location1.WLV_PalletFloorSpaces = 2;
			AssertNoErrors(location1.WLV_PalletFloorSpacesInfo);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsEmpty_ProductWarehouse()
		{
			TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsEmpty_Core(WarehouseTypes.Codes.Product);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsEmpty_FreeTradeZone()
		{
			TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsEmpty_Core(WarehouseTypes.Codes.FreeTradeZone);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsEmpty_TransitWarehouse()
		{
			TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsEmpty_Core(WarehouseTypes.Codes.Transit);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsEmpty_ContainerYard()
		{
			TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsEmpty_Core(WarehouseTypes.Codes.ContainerYard);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsEmpty_Core(string warehouseType)
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_WarehouseType = warehouseType;
			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_PalletFloorSpaces = 0;
			location1.WLV_PalletStackHeight = 0;
			Factory.Save();

			location1.WLV_PalletFloorSpaces = 1;
			AssertNoErrors(location1.WLV_PalletFloorSpacesInfo);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsNotEmptyAndAreAllPallets_ProductWarehouse()
		{
			TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsNotEmptyAndAreAllPallets_Core(WarehouseTypes.Codes.Product);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsNotEmptyAndAreAllPallets_FreeTradeZone()
		{
			TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsNotEmptyAndAreAllPallets_Core(WarehouseTypes.Codes.FreeTradeZone);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsNotEmptyAndAreAllPallets_TransitWarehouse()
		{
			TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsNotEmptyAndAreAllPallets_Core(WarehouseTypes.Codes.Transit);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsNotEmptyAndAreAllPallets_ContainerYard()
		{
			TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsNotEmptyAndAreAllPallets_Core(WarehouseTypes.Codes.ContainerYard);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsNotEmptyAndAreAllPallets_Core(string warehouseType)
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_WarehouseType = warehouseType;
			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_PalletFloorSpaces = 0;
			location1.WLV_PalletStackHeight = 0;
			Factory.Save();

			if (warehouseType == WarehouseTypes.Codes.Product || warehouseType == WarehouseTypes.Codes.FreeTradeZone)
			{
				var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
				var goodsReceivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", Notify);
				transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, "A-1", heldCode: "", palletID: "PLT01");
				transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, "A-1", heldCode: "", palletID: "PLT02");
				transactionTestHelper.FinaliseDocket(goodsReceivePK);
				Factory.Save();
			}

			location1.WLV_PalletStackHeight = 1;
			location1.WLV_PalletFloorSpaces = 2;
			AssertNoErrors(location1.WLV_PalletFloorSpacesInfo);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsNotEmptyAndNotAllPallets_ProductWarehouse()
		{
			TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsNotEmptyAndNotAllPallets_Core(WarehouseTypes.Codes.Product, shouldShowError: true);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsNotEmptyAndNotAllPallets_FreeTradeZone()
		{
			TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsNotEmptyAndNotAllPallets_Core(WarehouseTypes.Codes.FreeTradeZone, shouldShowError: true);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsNotEmptyAndNotAllPallets_TransitWarehouse()
		{
			TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsNotEmptyAndNotAllPallets_Core(WarehouseTypes.Codes.Transit, shouldShowError: false);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsNotEmptyAndNotAllPallets_ContainerYard()
		{
			TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsNotEmptyAndNotAllPallets_Core(WarehouseTypes.Codes.ContainerYard, shouldShowError: false);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationIsNotEmptyAndNotAllPallets_Core(string warehouseType, bool shouldShowError)
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_WarehouseType = warehouseType;
			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_PalletFloorSpaces = 0;
			location1.WLV_PalletStackHeight = 0;
			Factory.Save();

			if (warehouseType == WarehouseTypes.Codes.Product || warehouseType == WarehouseTypes.Codes.FreeTradeZone)
			{
				var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
				var goodsReceivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", Notify);
				transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, "A-1", heldCode: "", palletID: "PLT01");
				transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, "A-1", heldCode: "", palletID: "");
				transactionTestHelper.FinaliseDocket(goodsReceivePK);
				Factory.Save();
			}

			location1.WLV_PalletStackHeight = 1;
			location1.WLV_PalletFloorSpaces = 2;
			if (shouldShowError)
			{
				AssertHasError(location1.WLV_PalletFloorSpacesInfo, "Cannot Setup Pallet Spaces for a Location currently containing non-palletized stock.");
			}
			else
			{
				AssertNoErrors(location1.WLV_PalletFloorSpacesInfo);
			}
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationClassDDL_ProductWarehouse()
		{
			TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationClass_Core(WarehouseTypes.Codes.Product, LocationClasses.Codes.DDL, shouldShowError: true);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationClassDDL_FreeTradeZone()
		{
			TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationClass_Core(WarehouseTypes.Codes.FreeTradeZone, LocationClasses.Codes.DDL, shouldShowError: true);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationClassPST_ProductWarehouse()
		{
			TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationClass_Core(WarehouseTypes.Codes.Product, LocationClasses.Codes.PST, shouldShowError: true);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationClassPST_FreeTradeZone()
		{
			TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationClass_Core(WarehouseTypes.Codes.FreeTradeZone, LocationClasses.Codes.PST, shouldShowError: true);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationClassCON_ProductWarehouse()
		{
			TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationClass_Core(WarehouseTypes.Codes.Product, LocationClasses.Codes.CON, shouldShowError: true);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationClassCON_FreeTradeZone()
		{
			TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationClass_Core(WarehouseTypes.Codes.FreeTradeZone, LocationClasses.Codes.CON, shouldShowError: true);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationClassNOR_ProductWarehouse()
		{
			TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationClass_Core(WarehouseTypes.Codes.Product, LocationClasses.Codes.NOR, shouldShowError: false);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationClassNOR_FreeTradeZone()
		{
			TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationClass_Core(WarehouseTypes.Codes.FreeTradeZone, LocationClasses.Codes.NOR, shouldShowError: false);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationClassNOR_TransitWarehouse()
		{
			TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationClass_Core(WarehouseTypes.Codes.Transit, LocationClasses.Codes.NOR, shouldShowError: false);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationClassNOR_ContainerYard()
		{
			TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationClass_Core(WarehouseTypes.Codes.ContainerYard, LocationClasses.Codes.NOR, shouldShowError: false);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenEnabling_CurrentLocationClass_Core(string warehouseType, string locationClass, bool shouldShowError)
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_WarehouseType = warehouseType;
			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_PalletFloorSpaces = 0;
			location1.WLV_PalletStackHeight = 0;
			var locationTypeObj = Factory.NewWithValidTestData<WhsLocationType>();
			locationTypeObj.WLT_LocationClass = locationClass;
			location1.WLV_WLT_LocationType = locationTypeObj.PK;
			Factory.Save();

			location1.WLV_PalletFloorSpaces = 1;
			if (shouldShowError)
			{
				AssertHasError(location1.WLV_PalletFloorSpacesInfo, $"Cannot Setup Pallet Spaces for a Location with Location Class {locationClass}.");
			}
			else
			{
				AssertNoErrors(location1.WLV_PalletFloorSpacesInfo);
			}
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenIncreasingFromNonZero_ProductWarehouse()
		{
			TestValidateWLV_PalletFloorSpaces_WhenIncreasingFromNonZero_Core(WarehouseTypes.Codes.Product);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenIncreasingFromNonZero_FreeTradeZone()
		{
			TestValidateWLV_PalletFloorSpaces_WhenIncreasingFromNonZero_Core(WarehouseTypes.Codes.FreeTradeZone);
		}

		public void TestValidateWLV_PalletFloorSpaces_WhenIncreasingFromNonZero_Core(string warehouseType)
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_WarehouseType = warehouseType;
			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_PalletFloorSpaces = 1;
			location1.WLV_PalletStackHeight = 1;
			Factory.Save();

			var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var goodsReceivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", Notify);
			transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, "A-1", heldCode: "", palletID: "PLT01");
			transactionTestHelper.FinaliseDocket(goodsReceivePK);
			Factory.Save();

			location1.WLV_PalletFloorSpaces = 2;
			AssertNoErrors(location1.WLV_PalletFloorSpacesInfo);
		}

		#endregion

		#region TestValidateWLV_PalletStackHeight

		public void TestValidateWLV_PalletStackHeight_IsZeroWhenPalletSpacesIsSpecified()
		{
			var location = Factory.New<WhsLocation>();
			AssertNoErrors(location.WLV_PalletStackHeightInfo);

			location.WLV_PalletFloorSpaces = new ZByte(1);
			AssertHasError(location.WLV_PalletStackHeightInfo, "Pallet Stack Height must be set when Pallet Floor Spaces are specified.");

			location.WLV_PalletFloorSpaces = new ZByte(0);
			AssertNoErrors(location.WLV_PalletStackHeightInfo);

			location.WLV_PalletFloorSpaces = new ZByte(1);
			AssertHasError(location.WLV_PalletStackHeightInfo, "Pallet Stack Height must be set when Pallet Floor Spaces are specified.");

			location.WLV_PalletFloorSpaces = new ZByte(0);
			AssertNoErrors(location.WLV_PalletStackHeightInfo);
		}

		public void TestValidateWLV_PalletStackHeight_WhenReducing_CurrentPalletCountWillExceed_ProductWarehouse()
		{
			TestValidateWLV_PalletStackHeight_WhenReducing_CurrentPalletCountWillExceed_Core(WarehouseTypes.Codes.Product, shouldShowError: true);
		}

		public void TestValidateWLV_PalletStackHeight_WhenReducing_CurrentPalletCountWillExceed_FreeTradeZone()
		{
			TestValidateWLV_PalletStackHeight_WhenReducing_CurrentPalletCountWillExceed_Core(WarehouseTypes.Codes.FreeTradeZone, shouldShowError: true);
		}

		public void TestValidateWLV_PalletStackHeight_WhenReducing_CurrentPalletCountWillExceed_TransitWarehouse()
		{
			TestValidateWLV_PalletStackHeight_WhenReducing_CurrentPalletCountWillExceed_Core(WarehouseTypes.Codes.Transit, shouldShowError: false);
		}

		public void TestValidateWLV_PalletStackHeight_WhenReducing_CurrentPalletCountWillExceed_ContainerYard()
		{
			TestValidateWLV_PalletStackHeight_WhenReducing_CurrentPalletCountWillExceed_Core(WarehouseTypes.Codes.ContainerYard, shouldShowError: false);
		}

		public void TestValidateWLV_PalletStackHeight_WhenReducing_CurrentPalletCountWillExceed_Core(string warehouseType, bool shouldShowError)
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_WarehouseType = warehouseType;
			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_PalletFloorSpaces = 1;
			location1.WLV_PalletStackHeight = 2;
			Factory.Save();

			if (warehouseType == WarehouseTypes.Codes.Product || warehouseType == WarehouseTypes.Codes.FreeTradeZone)
			{
				var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
				var goodsReceivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", Notify);
				transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, "A-1", heldCode: "", palletID: "PLT01");
				transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, "A-1", heldCode: "", palletID: "PLT02");
				transactionTestHelper.FinaliseDocket(goodsReceivePK);
				Factory.Save();
			}

			location1.WLV_PalletStackHeight = 1;
			if (shouldShowError)
			{
				AssertHasError(location1.WLV_PalletStackHeightInfo, "Pallet Spaces (1) cannot be lower than existing Pallet Count (2) for this location.");
			}
			else
			{
				AssertNoErrors(location1.WLV_PalletStackHeightInfo);
			}
		}

		public void TestValidateWLV_PalletStackHeight_WhenReducing_CurrentPalletCountWillNotExceed_ProductWarehouse()
		{
			TestValidateWLV_PalletStackHeight_WhenReducing_CurrentPalletCountWillNotExceed_Core(WarehouseTypes.Codes.Product);
		}

		public void TestValidateWLV_PalletStackHeight_WhenReducing_CurrentPalletCountWillNotExceed_FreeTradeZone()
		{
			TestValidateWLV_PalletStackHeight_WhenReducing_CurrentPalletCountWillNotExceed_Core(WarehouseTypes.Codes.FreeTradeZone);
		}

		public void TestValidateWLV_PalletStackHeight_WhenReducing_CurrentPalletCountWillNotExceed_TransitWarehouse()
		{
			TestValidateWLV_PalletStackHeight_WhenReducing_CurrentPalletCountWillNotExceed_Core(WarehouseTypes.Codes.Transit);
		}

		public void TestValidateWLV_PalletStackHeight_WhenReducing_CurrentPalletCountWillNotExceed_ContainerYard()
		{
			TestValidateWLV_PalletStackHeight_WhenReducing_CurrentPalletCountWillNotExceed_Core(WarehouseTypes.Codes.ContainerYard);
		}

		public void TestValidateWLV_PalletStackHeight_WhenReducing_CurrentPalletCountWillNotExceed_Core(string warehouseType)
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_WarehouseType = warehouseType;
			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_PalletFloorSpaces = 3;
			location1.WLV_PalletStackHeight = 1;
			Factory.Save();

			if (warehouseType == WarehouseTypes.Codes.Product || warehouseType == WarehouseTypes.Codes.FreeTradeZone)
			{
				var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
				var goodsReceivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", Notify);
				transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, "A-1", heldCode: "", palletID: "PLT01");
				transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, "A-1", heldCode: "", palletID: "PLT02");
				transactionTestHelper.FinaliseDocket(goodsReceivePK);
				Factory.Save();
			}

			location1.WLV_PalletStackHeight = 1;
			AssertNoErrors(location1.WLV_PalletStackHeightInfo);
		}

		public void TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsEmpty_ProductWarehouse()
		{
			TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsEmpty_Core(WarehouseTypes.Codes.Product);
		}

		public void TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsEmpty_FreeTradeZone()
		{
			TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsEmpty_Core(WarehouseTypes.Codes.FreeTradeZone);
		}

		public void TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsEmpty_TransitWarehouse()
		{
			TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsEmpty_Core(WarehouseTypes.Codes.Transit);
		}

		public void TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsEmpty_ContainerYard()
		{
			TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsEmpty_Core(WarehouseTypes.Codes.ContainerYard);
		}

		public void TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsEmpty_Core(string warehouseType)
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_WarehouseType = warehouseType;
			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_PalletFloorSpaces = 2;
			location1.WLV_PalletStackHeight = 1;
			Factory.Save();

			location1.WLV_PalletStackHeight = 1;
			AssertNoErrors(location1.WLV_PalletStackHeightInfo);
		}

		public void TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsNotEmptyAndAreAllPallets_ProductWarehouse()
		{
			TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsNotEmptyAndAreAllPallets_Core(WarehouseTypes.Codes.Product);
		}

		public void TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsNotEmptyAndAreAllPallets_FreeTradeZone()
		{
			TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsNotEmptyAndAreAllPallets_Core(WarehouseTypes.Codes.FreeTradeZone);
		}

		public void TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsNotEmptyAndAreAllPallets_TransitWarehouse()
		{
			TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsNotEmptyAndAreAllPallets_Core(WarehouseTypes.Codes.Transit);
		}

		public void TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsNotEmptyAndAreAllPallets_ContainerYard()
		{
			TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsNotEmptyAndAreAllPallets_Core(WarehouseTypes.Codes.ContainerYard);
		}

		public void TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsNotEmptyAndAreAllPallets_Core(string warehouseType)
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_WarehouseType = warehouseType;
			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_PalletFloorSpaces = 0;
			location1.WLV_PalletStackHeight = 0;
			Factory.Save();

			if (warehouseType == WarehouseTypes.Codes.Product || warehouseType == WarehouseTypes.Codes.FreeTradeZone)
			{
				var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
				var goodsReceivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", Notify);
				transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, "A-1", heldCode: "", palletID: "PLT01");
				transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, "A-1", heldCode: "", palletID: "PLT02");
				transactionTestHelper.FinaliseDocket(goodsReceivePK);
				Factory.Save();
			}

			location1.WLV_PalletFloorSpaces = 1;
			location1.WLV_PalletStackHeight = 2;
			AssertNoErrors(location1.WLV_PalletStackHeightInfo);
		}

		public void TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsNotEmptyAndNotAllPallets_ProductWarehouse()
		{
			TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsNotEmptyAndNotAllPallets_Core(WarehouseTypes.Codes.Product, shouldShowError: true);
		}

		public void TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsNotEmptyAndNotAllPallets_FreeTradeZone()
		{
			TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsNotEmptyAndNotAllPallets_Core(WarehouseTypes.Codes.FreeTradeZone, shouldShowError: true);
		}

		public void TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsNotEmptyAndNotAllPallets_TransitWarehouse()
		{
			TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsNotEmptyAndNotAllPallets_Core(WarehouseTypes.Codes.Transit, shouldShowError: false);
		}

		public void TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsNotEmptyAndNotAllPallets_ContainerYard()
		{
			TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsNotEmptyAndNotAllPallets_Core(WarehouseTypes.Codes.ContainerYard, shouldShowError: false);
		}

		public void TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationIsNotEmptyAndNotAllPallets_Core(string warehouseType, bool shouldShowError)
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_WarehouseType = warehouseType;
			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_PalletFloorSpaces = 0;
			location1.WLV_PalletStackHeight = 0;
			Factory.Save();

			if (warehouseType == WarehouseTypes.Codes.Product || warehouseType == WarehouseTypes.Codes.FreeTradeZone)
			{
				var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
				var goodsReceivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", Notify);
				transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, "A-1", heldCode: "", palletID: "PLT01");
				transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, "A-1", heldCode: "", palletID: "");
				transactionTestHelper.FinaliseDocket(goodsReceivePK);
				Factory.Save();
			}

			location1.WLV_PalletFloorSpaces = 1;
			location1.WLV_PalletStackHeight = 2;
			if (shouldShowError)
			{
				AssertHasError(location1.WLV_PalletStackHeightInfo, "Cannot Setup Pallet Spaces for a Location currently containing non-palletized stock.");
			}
			else
			{
				AssertNoErrors(location1.WLV_PalletStackHeightInfo);
			}
		}

		public void TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationClassDDL_ProductWarehouse()
		{
			TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationClass_Core(WarehouseTypes.Codes.Product, LocationClasses.Codes.DDL, shouldShowError: true);
		}

		public void TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationClassDDL_FreeTradeZone()
		{
			TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationClass_Core(WarehouseTypes.Codes.FreeTradeZone, LocationClasses.Codes.DDL, shouldShowError: true);
		}

		public void TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationClassPST_ProductWarehouse()
		{
			TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationClass_Core(WarehouseTypes.Codes.Product, LocationClasses.Codes.PST, shouldShowError: true);
		}

		public void TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationClassPST_FreeTradeZone()
		{
			TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationClass_Core(WarehouseTypes.Codes.FreeTradeZone, LocationClasses.Codes.PST, shouldShowError: true);
		}

		public void TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationClassCON_ProductWarehouse()
		{
			TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationClass_Core(WarehouseTypes.Codes.Product, LocationClasses.Codes.CON, shouldShowError: true);
		}

		public void TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationClassCON_FreeTradeZone()
		{
			TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationClass_Core(WarehouseTypes.Codes.FreeTradeZone, LocationClasses.Codes.CON, shouldShowError: true);
		}

		public void TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationClassNOR_ProductWarehouse()
		{
			TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationClass_Core(WarehouseTypes.Codes.Product, LocationClasses.Codes.NOR, shouldShowError: false);
		}

		public void TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationClassNOR_FreeTradeZone()
		{
			TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationClass_Core(WarehouseTypes.Codes.FreeTradeZone, LocationClasses.Codes.NOR, shouldShowError: false);
		}

		public void TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationClassNOR_TransitWarehouse()
		{
			TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationClass_Core(WarehouseTypes.Codes.Transit, LocationClasses.Codes.NOR, shouldShowError: false);
		}

		public void TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationClassNOR_ContainerYard()
		{
			TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationClass_Core(WarehouseTypes.Codes.ContainerYard, LocationClasses.Codes.NOR, shouldShowError: false);
		}

		public void TestValidateWLV_PalletStackHeight_WhenEnabling_CurrentLocationClass_Core(string warehouseType, string locationClass, bool shouldShowError)
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_WarehouseType = warehouseType;
			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_PalletFloorSpaces = 0;
			location1.WLV_PalletStackHeight = 0;
			var locationTypeObj = Factory.NewWithValidTestData<WhsLocationType>();
			locationTypeObj.WLT_LocationClass = locationClass;
			location1.WLV_WLT_LocationType = locationTypeObj.PK;
			Factory.Save();

			location1.WLV_PalletStackHeight = 1;
			if (shouldShowError)
			{
				AssertHasError(location1.WLV_PalletStackHeightInfo, $"Cannot Setup Pallet Spaces for a Location with Location Class {locationClass}.");
			}
			else
			{
				AssertNoErrors(location1.WLV_PalletStackHeightInfo);
			}
		}

		public void TestValidateWLV_PalletStackHeight_WhenIncreasingFromNonZero_ProductWarehouse()
		{
			TestValidateWLV_PalletStackHeight_WhenIncreasingFromNonZero_Core(WarehouseTypes.Codes.Product);
		}

		public void TestValidateWLV_PalletStackHeight_WhenIncreasingFromNonZero_FreeTradeZone()
		{
			TestValidateWLV_PalletStackHeight_WhenIncreasingFromNonZero_Core(WarehouseTypes.Codes.FreeTradeZone);
		}

		public void TestValidateWLV_PalletStackHeight_WhenIncreasingFromNonZero_Core(string warehouseType)
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_WarehouseType = warehouseType;
			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_PalletFloorSpaces = 1;
			location1.WLV_PalletStackHeight = 1;
			Factory.Save();

			var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var goodsReceivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", Notify);
			transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, "A-1", heldCode: "", palletID: "PLT01");
			transactionTestHelper.FinaliseDocket(goodsReceivePK);
			Factory.Save();

			location1.WLV_PalletStackHeight = 2;
			AssertNoErrors(location1.WLV_PalletStackHeightInfo);
		}

		public void TestValidatePalletSpaces_WithMixedProducts()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 3, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-3");
			Factory.Save();

			var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var goodsReceivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", Notify);
			transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, "A-2", heldCode: "", palletID: "PLT01");
			transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, "A-3", heldCode: "", palletID: "PLT02");
			transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part2.PK, 10m, "A-3", heldCode: "", palletID: "PLT02");
			transactionTestHelper.FinaliseDocket(goodsReceivePK);
			Factory.Save();

			location1.WLV_PalletStackHeight = 1;
			location1.WLV_PalletFloorSpaces = 1;
			AssertNoErrors(location1.WLV_PalletStackHeightInfo);
			AssertNoErrors(location1.WLV_PalletFloorSpacesInfo);

			location2.WLV_PalletStackHeight = 1;
			location2.WLV_PalletFloorSpaces = 1;
			AssertNoErrors(location2.WLV_PalletStackHeightInfo);
			AssertNoErrors(location2.WLV_PalletFloorSpacesInfo);

			location3.WLV_PalletStackHeight = 1;
			location3.WLV_PalletFloorSpaces = 1;
			AssertHasError(location3.WLV_PalletStackHeightInfo, "Cannot Setup Pallet Spaces for a Location containing multiple products.");
			AssertHasError(location3.WLV_PalletFloorSpacesInfo, "Cannot Setup Pallet Spaces for a Location containing multiple products.");
		}

		#endregion

		#region TestValidateWLV_MaximumPickCountBeforeAutomatedStocktake

		public void TestValidateWLV_MaximumPickCountBeforeAutomatedStocktake()
		{
			var location = Factory.New<WhsLocation>();
			location.WLV_FinalisedPickCount = 2;
			location.WLV_MaximumPickCountBeforeAutomatedStocktake = 4;
			AssertNoWarnings(location.WLV_MaximumPickCountBeforeAutomatedStocktakeInfo);

			location.WLV_MaximumPickCountBeforeAutomatedStocktake = 1;
			AssertHasWarning(location.WLV_MaximumPickCountBeforeAutomatedStocktakeInfo, "Since the Current Touch Count is greater than the Maximum Touch Count," +
														 " once a pick is finalized for this location, Stocktake will be generated and the Current Touch Count will be set back to zero.");
			location.WLV_MaximumPickCountBeforeAutomatedStocktake = 5;
			AssertNoWarnings(location.WLV_MaximumPickCountBeforeAutomatedStocktakeInfo);

			location.WLV_MaximumPickCountBeforeAutomatedStocktake = -1;
			AssertHasError("Maximum touch count should be positive.", location.WLV_MaximumPickCountBeforeAutomatedStocktakeInfo, "Please enter a 'Maximum Pick Count Before Automated Stocktake' greater than or equal to 0.");
		}

		#endregion

		#region TestCheckWLV_MaxCubicIsNotEmpty

		public void TestCheckWLV_MaxCubicIsNotEmpty()
		{
			var location = Factory.New<WhsLocation>();
			location.WLV_MaxCubic = 0;
			AssertNoErrors(location.WLV_MaxCubicInfo);

			location.WLV_MaxCubic = 1;
			AssertNoErrors(location.WLV_MaxCubicInfo);
		}

		#endregion

		#region TestCheckWLV_FinalisedPickCountIsNotEmpty

		public void TestCheckWLV_FinalisedPickCountIsNotEmpty()
		{
			var location = Factory.New<WhsLocation>();
			location.WLV_FinalisedPickCount = 0;
			AssertNoErrors(location.WLV_FinalisedPickCountInfo);

			location.WLV_FinalisedPickCount = 1;
			AssertNoErrors(location.WLV_FinalisedPickCountInfo);
		}

		#endregion

		#region TestCheckWLV_MaxCubicUnitIsNotEmpty

		public void TestCheckWLV_MaxCubicUnitIsNotEmpty()
		{
			var location = Factory.New<WhsLocation>();
			location.WLV_MaxCubicUnit = "";
			AssertNoErrors(location.WLV_MaxCubicUnitInfo);

			location.WLV_MaxCubicUnit = "M3";
			AssertNoErrors(location.WLV_MaxCubicUnitInfo);
		}

		#endregion

		#region TestCheckWLV_MaximumPickCountBeforeAutomatedStocktakeIsNotEmpty

		public void TestCheckWLV_MaximumPickCountBeforeAutomatedStocktakeIsNotEmpty()
		{
			var location = Factory.New<WhsLocation>();
			location.WLV_MaximumPickCountBeforeAutomatedStocktake = 0;
			AssertNoErrors(location.WLV_MaximumPickCountBeforeAutomatedStocktakeInfo);

			location.WLV_MaximumPickCountBeforeAutomatedStocktake = 1;
			AssertNoErrors(location.WLV_MaximumPickCountBeforeAutomatedStocktakeInfo);
		}

		#endregion

		#region TestCheckWLV_MaxWeightIsNotEmpty

		public void TestCheckWLV_MaxWeightIsNotEmpty()
		{
			var location = Factory.New<WhsLocation>();
			location.WLV_MaxWeight = 0;
			AssertNoErrors(location.WLV_MaxWeightInfo);

			location.WLV_FinalisedPickCount = 1;
			AssertNoErrors(location.WLV_MaxWeightInfo);
		}

		#endregion

		#region TestCheckWLV_MaxWeightUnitIsNotEmpty

		public void TestCheckWLV_MaxWeightUnitIsNotEmpty()
		{
			var location = Factory.New<WhsLocation>();
			location.WLV_MaxWeightUnit = "";
			AssertNoErrors(location.WLV_MaxWeightUnitInfo);

			location.WLV_MaxWeightUnit = "KG";
			AssertNoErrors(location.WLV_MaxWeightUnitInfo);
		}

		#endregion

		#region TestCheckWLV_MaxQuantityUnitIsNotEmpty

		public void TestCheckWLV_MaxQuantityUnitIsNotEmpty()
		{
			var location = Factory.New<WhsLocation>();
			location.WLV_MaxQuantityUnit = "";
			AssertNoErrors(location.WLV_MaxQuantityUnitInfo);

			location.WLV_MaxQuantityUnit = "UNT";
			AssertNoErrors(location.WLV_MaxQuantityUnitInfo);
		}

		#endregion

		#region TestCheckWLV_MaxHeightIsNotEmpty

		public void TestCheckWLV_MaxHeightIsNotEmpty()
		{
			var location = Factory.New<WhsLocation>();
			location.WLV_MaxHeight = 0;
			AssertNoErrors(location.WLV_MaxHeightInfo);
		}

		#endregion

		#region TestCheckWLV_MaxDepthIsNotEmpty

		public void TestCheckWLV_MaxDepthIsNotEmpty()
		{
			var location = Factory.New<WhsLocation>();
			location.WLV_MaxDepth = 0;
			AssertNoErrors(location.WLV_MaxDepthInfo);
		}

		#endregion

		#region TestCheckWLV_MaxWidthIsNotEmpty

		public void TestCheckWLV_MaxWidthIsNotEmpty()
		{
			var location = Factory.New<WhsLocation>();
			location.WLV_MaxWidth = 0;
			AssertNoErrors(location.WLV_MaxWidthInfo);
		}

		#endregion

		#region TestCheckWLV_MaxDimensionUnitIsNotEmpty

		public void TestCheckWLV_MaxDimensionUnitIsNotEmpty()
		{
			var location = Factory.New<WhsLocation>();
			location.WLV_MaxDimensionUnit = "";
			AssertNoErrors(location.WLV_MaxDimensionUnitInfo);

			location.WLV_MaxDimensionUnit = "M";
			AssertNoErrors(location.WLV_MaxDimensionUnitInfo);
		}

		#endregion

		#region TestCheckWLV_PalletFloorSpaces

		public void TestCheckWLV_PalletFloorSpaces()
		{
			var location = Factory.New<WhsLocation>();
			location.WLV_PalletFloorSpaces = 0;
			AssertNoErrors(location.WLV_PalletFloorSpacesInfo);

			location.WLV_PalletFloorSpaces = 1;
			AssertNoErrors(location.WLV_PalletFloorSpacesInfo);
		}

		#endregion

		#region TestWLV_PalletStackHeight

		public void TestWLV_PalletStackHeight()
		{
			var location = Factory.New<WhsLocation>();
			location.WLV_MaxHeight = 0;
			AssertNoErrors(location.WLV_PalletStackHeightInfo);

			location.WLV_MaxHeight = 1;
			AssertNoErrors(location.WLV_PalletStackHeightInfo);
		}

		#endregion

		#region TestCheckWLV_CheckDigit

		public void TestCheckWLV_CheckDigit()
		{
			var location = Factory.New<WhsLocation>();
			location.WLV_CheckDigit = 0;
			AssertNoErrors(location.WLV_CheckDigitInfo);

			location.WLV_CheckDigit = 1;
			AssertNoErrors(location.WLV_CheckDigitInfo);
		}

		public void TestCheckWLV_CheckDigit_NoWarnOnMultipleUnique()
		{
			var warehouse1 = Helper.CreateWarehouse("Whs1", "A", 2, 1);
			Factory.Save();

			var location1 = warehouse1.FindLocation("A-1");
			location1.WLV_CheckDigit = 2;
			AssertNoWarnings("WLV_CheckDigit added is unique, mustn't have any warnings.", location1.WLV_CheckDigitInfo);

			var location2 = warehouse1.FindLocation("A-2");
			location2.WLV_CheckDigit = 3;
			AssertNoWarnings("WLV_CheckDigit added is unique, mustn't have any warnings.", location2.WLV_CheckDigitInfo);
		}

		public void TestCheckWLV_CheckDigit_WarnOnAddNonUnique()
		{
			var warehouse = Helper.CreateWarehouse("Whs1", "A", 2, 1);
			Factory.Save();

			var location1 = warehouse.FindLocation("A-1");
			location1.WLV_CheckDigit = 2;
			AssertNoWarnings("WLV_CheckDigit added is unique, mustn't have any warnings.", location1.WLV_CheckDigitInfo);

			var location2 = warehouse.FindLocation("A-2");
			location2.WLV_CheckDigit = 2;
			AssertHasWarnings("WLV_CheckDigit added is not unique, warning should have been raised but wasn't.", location2.WLV_CheckDigitInfo);
		}

		public void TestCheckWLV_CheckDigit_NoWarnOnAddNonUniqueDifferentRows()
		{
			var warehouse = Helper.CreateWarehouse("WHT");
			Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 1);
			Helper.CreateRowAndGenerateLocations(warehouse, "B", 2, 1);
			Factory.Save();

			var location1 = warehouse.FindLocation("A-1");
			location1.WLV_CheckDigit = 2;
			AssertNoWarnings("WLV_CheckDigit added is unique for this row, mustn't have any warnings.", location1.WLV_CheckDigitInfo);

			var location2 = warehouse.FindLocation("B-1");
			location2.WLV_CheckDigit = 2;
			AssertNoWarnings("WLV_CheckDigit added is unique for this row, mustn't have any warnings.", location2.WLV_CheckDigitInfo);
		}

		public void TestCheckWLV_CheckDigit_NoWarnOnAddEmpty()
		{
			var warehouse = Helper.CreateWarehouse("Whs1", "A", 2, 1);
			Factory.Save();

			var location1 = warehouse.FindLocation("A-1");
			location1.WLV_CheckDigit = WhsLocation.EmptyCheckDigit;
			AssertNoWarnings("WLV_CheckDigit added is empty, mustn't have any warnings.", location1.WLV_CheckDigitInfo);

			var location2 = warehouse.FindLocation("A-2");
			location2.WLV_CheckDigit = WhsLocation.EmptyCheckDigit;
			AssertNoWarnings("WLV_CheckDigit added is empty, mustn't have any warnings.", location2.WLV_CheckDigitInfo);
		}

		public void TestCheckWLV_CheckDigit_ErrorOnDockDoorLocation()
		{
			var whs = Helper.CreateWarehouse("W1", "A", 2, 1);
			Factory.Save();

			var location = whs.FindLocation("A-1");

			var ddlLocationType = Helper.CreateLocationType("DDL", "DDL");
			location.WLV_WLT_LocationType = ddlLocationType.PK;
			Assert("Precondition:", location.IsDockDoorLocation);

			location.WLV_CheckDigit = 1;
			AssertHasErrors("CheckDigit being added to a dock door location should result in an error", location.WLV_CheckDigitInfo);
		}

		public void TestCheckWLV_CheckDigit_ErrorOnConsolidationLocation()
		{
			var whs = Helper.CreateWarehouse("W1", "A", 2, 1);
			Factory.Save();

			var location = whs.FindLocation("A-1");

			var conLocationType = Helper.CreateLocationType("CON", "CON");
			location.WLV_WLT_LocationType = conLocationType.PK;
			Assert("Precondition:", location.IsPackingConsolidationLocation);

			location.WLV_CheckDigit = 1;
			AssertHasErrors("CheckDigit being added to a consolidation location should result in an error", location.WLV_CheckDigitInfo);
		}

		public void TestCheckWLV_CheckDigit_ErrorOnPackingStationLocation()
		{
			var whs = Helper.CreateWarehouse("W1", "A", 2, 1);
			Factory.Save();

			var location = whs.FindLocation("A-1");

			var pstLocationType = Helper.CreateLocationType("PST", "PST");
			location.WLV_WLT_LocationType = pstLocationType.PK;
			Assert("Precondition:", location.IsPackingStationLocation);

			location.WLV_CheckDigit = 1;
			AssertHasErrors("CheckDigit being added to a packing station location should result in an error", location.WLV_CheckDigitInfo);
		}

		#endregion

		#region TestCheckWLV_CycleCountPathSequence

		public void TestCheckWLV_CycleCountPathSequence()
		{
			var location = Factory.New<WhsLocation>();
			location.WLV_CycleCountPathSequence = 0;
			AssertNoErrors(location.WLV_CycleCountPathSequenceInfo);

			location.WLV_CycleCountPathSequence = -1;
			AssertHasError(location.WLV_CycleCountPathSequenceInfo, "Please do not enter a negative cycle count path sequence.");

			location.WLV_CycleCountPathSequence = 1;
			AssertNoErrors(location.WLV_CycleCountPathSequenceInfo);
		}

		#endregion

		#region TestCheckWLV_PutawayPathSequence

		public void TestCheckWLV_PutawayPathSequence()
		{
			var location = Factory.New<WhsLocation>();
			location.WLV_PutawayPathSequence = 0;
			AssertHasError(location.WLV_PutawayPathSequenceInfo, "Putaway Path Sequence cannot be zero.");

			location.WLV_PutawayPathSequence = -1;
			AssertHasError(location.WLV_PutawayPathSequenceInfo, "Putaway Path Sequence cannot be negative.");

			location.WLV_PutawayPathSequence = 1;
			AssertNoErrors(location.WLV_PutawayPathSequenceInfo);
		}

		#endregion

		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var location = Factory.New<WhsLocation>();
			var validation = new TestWhsLocationViewValidation(location);

			var list = new string[]
			{
				WhsLocationViewSchema.Constants.WLV_WA_PickingArea,
				WhsLocationViewSchema.Constants.WLV_WA_PutawayArea,
				WhsLocationViewSchema.Constants.WLV_WR,
				WhsLocationViewSchema.Constants.WLV_SQ_DefaultPrintQueue
			};

			foreach (var propertyInfo in location.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
			{
				if (list.Contains(propertyInfo.Name))
				{
					AssertEquals("NK/FK which cannot be cancelled.", false, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
				else
				{
					AssertEquals("All other properties should just return base condition of true.", true, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
			}
		}

		#endregion

		#region TestWhsLocationViewValidation

		class TestWhsLocationViewValidation : WhsLocationViewValidation
		{
			public TestWhsLocationViewValidation(WhsLocation parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion

		#region CreateWhsItemPackageState

		BusinessObject CreateWhsItemPackageState(string status, WhsLocation location, BusinessObject rtu, BusinessObject package)
		{
			var packageState = (BusinessObject)Factory.New<IWhsItemPackageState>();
			packageState[WhsItemPackageStateSchema.WPS_Status] = status;
			packageState[WhsItemPackageStateSchema.WPS_WL_LastLocation] = location.PK;
			packageState[WhsItemPackageStateSchema.WPS_WRH_TransitReceiveHeader] = rtu.PK;
			packageState[WhsItemPackageStateSchema.WPS_KP_Package] = package.PK;
			packageState[WhsItemPackageStateSchema.WPS_WW_Warehouse] = location.WLV_WW_Whs;
			packageState[WhsItemPackageStateSchema.WPS_UnloadedTime] = ZDateTimeOffset.Now;
			packageState[WhsItemPackageStateSchema.WPS_SecurityStatus] = "SEC";
			return packageState;
		}

		#endregion
	}
}
