using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class AutoCreateCompletePalletPackagesForPickTest : WhsSecureServiceTestCase
	{
		#region TestAutoCreateCompletePalletPackagesForPick

		public void TestAutoCreateCompletePalletPackagesForPick_Enabled_Pallet()
		{
			TestAutoCreateCompletePalletPackagesForPickCore(packType: Constants.PkgUnit.Pallet, autoPackEnabled: true);
		}

		public void TestAutoCreateCompletePalletPackagesForPick_Disabled_Pallet()
		{
			TestAutoCreateCompletePalletPackagesForPickCore(packType: Constants.PkgUnit.Pallet, autoPackEnabled: false);
		}

		public void TestAutoCreateCompletePalletPackagesForPick_Enabled_CAS()
		{
			TestAutoCreateCompletePalletPackagesForPickCore(packType: Constants.PkgUnit.Case, autoPackEnabled: true);
		}

		public void TestAutoCreateCompletePalletPackagesForPick_Disabled_CAS()
		{
			TestAutoCreateCompletePalletPackagesForPickCore(packType: Constants.PkgUnit.Case, autoPackEnabled: false);
		}

		void TestAutoCreateCompletePalletPackagesForPickCore(string packType, bool autoPackEnabled)
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, packType, 3m);

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = autoPackEnabled;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 3m, data.Whs1.FindLocation("A-1"));
			receiveLine.WE_PalletID = "PLT1";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 3m);

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines[0];
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateCompletePalletPackagesForPick(pick.PK.ToGuid(), new[] { "PLT1" });
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			var packages = order.PackageJob.Packages;
			if (autoPackEnabled)
			{
			AssertEquals("Should be 1 added package for order.", 1, packages.Count);

			var package1 = packages.Single(p => p.KP_F3_NKPackType == packType && p.KP_PackageID == "PLT1" && p.IsClosed);
			var packageDivots1 = package1.PackedItemDivots;
			AssertEquals("Should be 1 added package divot", 1, packageDivots1.Count);
			AssertEquals("Should be correct divot KI_ParentID", order.Lines[0].PickLines[0].PK, packageDivots1[0].KI_ParentID);
		}
			else
			{
				AssertEquals("Should be 0 added package for order.", 0, packages.Count);
			}
		}

		public void TestAutoCreateCompletePalletPackagesForPick_NoConversions()
		{
			TestAutoCreateCompletePalletPackagesForPick_ConversionsCore((data) => { });
		}

		public void TestAutoCreateCompletePalletPackagesForPick_MultipleMatchingConversions()
		{
			TestAutoCreateCompletePalletPackagesForPick_ConversionsCore(
				(data) =>
				{
					Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Package, 3m);
					Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Box, 3m);
				});
		}

		void TestAutoCreateCompletePalletPackagesForPick_ConversionsCore(Action<TestDataSimpleEnvironment> createConversions)
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			createConversions(data);

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 3m, data.Whs1.FindLocation("A-1"));
			receiveLine.WE_PalletID = "PLT1";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 3m);

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines[0];
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateCompletePalletPackagesForPick(pick.PK.ToGuid(), new[] { "PLT1" });
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			var packages = order.PackageJob.Packages;
			AssertEquals("Should be 1 added package for order.", 1, packages.Count);

			var package1 = packages.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Package && p.KP_PackageID == "PLT1" && p.IsClosed);
			var packageDivots1 = package1.PackedItemDivots;
			AssertEquals("Should be 1 added package divot", 1, packageDivots1.Count);
			AssertEquals("Should be correct divot KI_ParentID", order.Lines[0].PickLines[0].PK, packageDivots1[0].KI_ParentID);
		}

		#endregion

		#region TestAutoCreateCompletePalletPackagesForPick_MultipleProducts

		public void TestAutoCreateCompletePalletPackagesForPick_MultipleProducts()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Package, 2m);

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine1.WE_PalletID = "RFT";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, location);
			receiveLine2.WE_PalletID = "RFT";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 1m);

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines[0];
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			var pickLine2 = orderLine2.PickLines[0];
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateCompletePalletPackagesForPick(pick.PK.ToGuid(), new[] { "RFT" });
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);
			AssertEquals("Should be no added packages for order.", 0, order.PackageJob.Packages.Count);
		}

		#endregion

		#region TestAutoCreateCompletePalletPackagesForPick_MultiOrderPick_PackagesAcrossOrders

		public void TestAutoCreateCompletePalletPackagesForPick_MultiOrderPick_PackagesAcrossOrders()
		{
			{
				var staff = Helper.CreateGlbStaff("GS1", "GS1");
				var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
				Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Package, 4m);

				var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
				pickParams.WPP_WW_Warehouse = data.Whs1.PK;
				pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

				var location = data.Whs1.FindLocation("A-1");
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
				var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 4m, location);
				receiveLine1.WE_PalletID = "RETPLT";
				receive.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(receive);
				Helper.Factory.Save();

				var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
				var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 2m);
				var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
				var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 2m);

				var pick = Helper.CreatePickNew(order1, order2);
				var pickLine1 = orderLine1.PickLines[0];
				pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
				var pickLine2 = orderLine2.PickLines[0];
				pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
				pick.RunPreSaveValidation();
				Helper.Factory.Save();

				AssertEquals("Precondition", true, pick.IsMultiOrderPick);

				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.AutoCreateCompletePalletPackagesForPick(pick.PK.ToGuid(), new[] { "RETPLT" });
				AssertEquals("No error should be on response", ErrorTypes.None, response.Error);
				AssertEquals("Should be no added packages for order1.", 0, order1.PackageJob.Packages.Count);
				AssertEquals("Should be no added packages for order2.", 0, order2.PackageJob.Packages.Count);
			}
		}

		#endregion

		#region TestAutoCreateCompletePalletPackagesForPick_MultiplePallets

		public void TestAutoCreateCompletePalletPackagesForPick_MultiplePallets()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.VIN);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Pallet, 4m);

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var location = data.Whs1.FindLocation("A-1");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 4m, location);
			receiveLine1.WE_PartAttrib1 = "PLT1";
			receiveLine1.WE_PalletID = "PLTPLT";
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);
			Helper.Factory.Save();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 2m, location);
			receiveLine2.WE_PartAttrib1 = "PKG1";
			receiveLine2.WE_PalletID = "PLTPKG";
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 4m);
			orderLine1.WE_PartAttrib1 = "PLT1";

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 2m);
			orderLine2.WE_PartAttrib1 = "PKG1";

			var pick = Helper.CreatePickNew(order1, order2);
			Helper.Factory.Save();

			orderLine1.PickLines.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Now);
			orderLine2.PickLines.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Now);
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var palletIDs = new[] { "PLTPLT", "PLTPKG" };
			var response = webService.AutoCreateCompletePalletPackagesForPick(pick.PK.ToGuid(), palletIDs);
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			var packages1 = order1.PackageJob.Packages;
			AssertEquals("Should be 1 added packages for order1.", 1, packages1.Count);
			var package1 = packages1.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Pallet && p.KP_PackageID == "PLTPLT" && p.IsClosed);
			var packageDivots1 = package1.PackedItemDivots;
			AssertContainsExactElementsInAnyOrder("Should be correct divot KI_ParentID", orderLine1.PickLines.Select(p => p.PK), packageDivots1.Select(d => d.KI_ParentID));

			var packages2 = order2.PackageJob.Packages;
			AssertEquals("Should be 1 added packages for order2.", 1, packages2.Count);
			var package2 = packages2.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Package && p.KP_PackageID == "PLTPKG" && p.IsClosed);
			var packageDivots2 = package2.PackedItemDivots;
			AssertContainsExactElementsInAnyOrder("Should be correct divot KI_ParentID", orderLine2.PickLines.Select(p => p.PK), packageDivots2.Select(d => d.KI_ParentID));
		}

		public void TestAutoCreateCompletePalletPackagesForPick_MultiplePallets_TwoClientsOnlyOneHasAutoPackEnabled()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.VIN);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var pickParams1 = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams1.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams1.WPP_EnableAutoPackageCreationOnPicking = false;

			var client2 = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);
			Helper.SetClientAttributeType(client2, AttributeNumber.One, PartAttributeTypeList.Codes.VIN);
			Helper.SetProductAttributeUse(client2, data.Part1, AttributeNumber.One, true);

			var pickParams2 = WhsClientPickingParams.GetClientPickingParams(client2).WarehousePickPackParams.AddNew();
			pickParams2.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams2.WPP_EnableAutoPackageCreationOnPicking = true;

			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Pallet, 4m);

			var location = data.Whs1.FindLocation("A-1");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 4m, location);
			receiveLine1.WE_PartAttrib1 = "PLT1";
			receiveLine1.WE_PalletID = "PLT1";
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);
			Helper.Factory.Save();

			var receive2 = Helper.CreateWhsReceive(client2, data.Whs1, "R2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 4m, location);
			receiveLine2.WE_PartAttrib1 = "PLT2";
			receiveLine2.WE_PalletID = "PLT2";
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 4m);
			orderLine1.WE_PartAttrib1 = "PLT1";

			var order2 = Helper.CreateWhsOrder(client2, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 4m);
			orderLine2.WE_PartAttrib1 = "PLT2";

			var pick = Helper.CreatePickNew(order1, order2);
			Helper.Factory.Save();

			orderLine1.PickLines.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Now);
			orderLine2.PickLines.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Now);
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var palletIDs = new[] { "PLT2", "PLT1" };
			var response = webService.AutoCreateCompletePalletPackagesForPick(pick.PK.ToGuid(), palletIDs);
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			AssertEquals("Should be 0 added packages for order1.", 0, order1.PackageJob.Packages.Count);

			var packages2 = order2.PackageJob.Packages;
			AssertEquals("Should be 1 added packages for order2.", 1, packages2.Count);
			var package2 = packages2.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Pallet && p.KP_PackageID == "PLT2" && p.IsClosed);
			var packageDivots2 = package2.PackedItemDivots;
			AssertContainsExactElementsInAnyOrder("Should be correct divot KI_ParentID", orderLine2.PickLines.Select(p => p.PK), packageDivots2.Select(d => d.KI_ParentID));
		}

		#endregion

		#region TestAutoCreateCompletePalletPackagesForPick_ErrorDuringPackageCreation

		public void TestAutoCreateCompletePalletPackagesForPick_ErrorDuringPackageCreation()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Package, 4m);

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var location = data.Whs1.FindLocation("A-1");
			var inv = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, location);
			inv.WI_PalletID = "RFT";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 4m);

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines[0];
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "Pkg1";
			package.Pack(orderLine.ReleaseLines[0], 4m);
			Helper.Factory.Save();

			AssertEquals("Precondition: orderline already packed", orderLine.PickLines[0].PK, package.PackedItemDivots[0].KI_ParentID);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateCompletePalletPackagesForPick(pick.PK.ToGuid(), new[] { "RFT" });

			AssertEquals("Should be error response", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Error message should be correct", "Auto-packing failed. Pick Line to Pack should be unpacked.", response.ErrorMessage);
			AssertEquals("Should be no additional packages for order.", 1, order.PackageJob.Packages.Count);
		}

		#endregion

		#region TestAutoCreateCompletePalletPackagesForPick_SaveFailure

		public void TestAutoCreateCompletePalletPackagesForPick_SaveFailure()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Pallet, 2m);

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var location = data.Whs1.FindLocation("A-1");
			var inv = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, location);
			inv.WI_PalletID = "RFT";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines[0];
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			webService.Factory.Saving += f => throw new ZCannotSaveException("Test Cannot Save failure", "Test", new Exception("Inner exception"));

			var response = webService.AutoCreateCompletePalletPackagesForPick(pick.PK.ToGuid(), new[] { "RFT" });

			AssertEquals("Should be error response", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Error message should be correct", "Auto-packing failed. A saving error occurred whilst attempting to auto-pack:\r\nTest Cannot Save failure\r\nInner exception", response.ErrorMessage);
			AssertEquals("Should be no added packages for order.", 0, order.PackageJob.Packages.Count);
		}

		#endregion

		#region TestAutoCreateCompletePalletPackagesForPick_FullPalletPicking_AttributeNeutralSerials

		public void TestAutoCreateCompletePalletPackagesForPick_FullPalletPicking_AttributeNeutralSerials()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Pallet, 3m);

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var location = data.Whs1.FindLocation("A-1");
			var inv1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			inv1.WI_SerialNumber = "SN1";
			inv1.WI_PalletID = "PalletIDHere";
			var inv2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			inv2.WI_SerialNumber = "SN2";
			inv2.WI_PalletID = "PalletIDHere";
			var inv3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			inv3.WI_SerialNumber = "SN3";
			inv3.WI_PalletID = "PalletIDHere";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 3m);

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine.PickLines[0];
			Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			var pickLine2 = orderLine.PickLines[1];
			Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			var pickLine3 = orderLine.PickLines[2];
			Helper.PickAndMakeInTransitTransfer(pickLine3, ZDateTimeOffset.Now);
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateCompletePalletPackagesForPick(pick.PK.ToGuid(), new[] { "PalletIDHere" });
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			var packages = order.PackageJob.Packages;
			AssertEquals("Should be 1 added packages for order.", 1, packages.Count);
			AssertEquals("Should be correct pack type.", Constants.PkgUnit.Pallet, packages[0].KP_F3_NKPackType);
			AssertEquals("Package should be closed.", true, packages[0].IsClosed);
			AssertEquals("Should be correct package ID.", "PalletIDHere", packages[0].KP_PackageID);

			var packageDivots = packages[0].PackedItemDivots;
			AssertEquals("Should be 3 added package divots", 3, packageDivots.Count);
			AssertContainsExactElementsInAnyOrder("Should be correct divot KI_ParentIDs", new[] { pickLine1.PK, pickLine2.PK, pickLine3.PK }, packageDivots.Select(p => p.KI_ParentID));
		}

		#endregion

		#region TestAutoCreateCompletePalletPackagesForPick_PickByBOM

		public void TestAutoCreateCompletePalletPackagesForPick_PickByBOM()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Pallet, 3m);

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;

			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, wheel, 10m, location);
			receiveLine.WI_PalletID = "PalletID";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);
			var pick = Helper.CreatePickNew(order);

			var wheelOrderLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var wheelPickLine = wheelOrderLine.PickLines.Single();
			wheelPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateCompletePalletPackagesForPick(pick.PK.ToGuid(), new[] { "PalletID" });
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			var packages = order.PackageJob.Packages;
			AssertEquals("Should be 0 added package for order.", 0, packages.Count);
		}

		#endregion

		#region TestAutoCreateCompletePalletPackagesForPick_ProductPackageSizes

		public void TestAutoCreateCompletePalletPackagesForPick_ProductPackageSizes()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			var partUnit = Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Box, 3m);
			partUnit.OF_Depth = 2;
			partUnit.OF_Height = 4;
			partUnit.OF_Width = 6;
			partUnit.OF_Weight = 50;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var location = data.Whs1.FindLocation("A-1");
			var inv = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, location);
			inv.WI_PalletID = "PalletIDHere";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 3m);

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines[0];
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateCompletePalletPackagesForPick(pick.PK.ToGuid(), new[] { "PalletIDHere" });
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			var packages = order.PackageJob.Packages;
			AssertEquals("Should be 1 added packages for order.", 1, packages.Count);
			AssertEquals("Should be correct pack type.", Constants.PkgUnit.Box, packages[0].KP_F3_NKPackType);
			AssertEquals("Should be correct package ID.", "PalletIDHere", packages[0].KP_PackageID);
			AssertEquals("Package should be closed.", true, packages[0].IsClosed);
			AssertEquals("Should be correct KP_Length.", 2m, packages[0].KP_Length);
			AssertEquals("Should be correct KP_Height.", 4m, packages[0].KP_Height);
			AssertEquals("Should be correct KP_Width.", 6m, packages[0].KP_Width);
			AssertEquals("Should be correct KP_Weight.", 56m, packages[0].KP_Weight);

			var packageDivots = packages[0].PackedItemDivots;
			AssertEquals("Should be 1 added package divot", 1, packageDivots.Count);
			AssertEquals("Should be correct divot KI_ParentID", orderLine.PickLines[0].PK, packageDivots[0].KI_ParentID);
		}

		#endregion

		#region TestAutoCreateCompletePalletPackagesForPick_DBHits

		public void TestAutoCreateCompletePalletPackagesForPick_DBHits()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			const int numberOfClients = 10;

			var clients = new List<OrgHeader>();
			for (var i = 0; i < numberOfClients; i++)
			{
				var clientX = Helper.CreateClient($"C2{i}");
				clients.Add(clientX);

				var pickParamsX = WhsClientPickingParams.GetClientPickingParams(clientX).WarehousePickPackParams.AddNew();
				pickParamsX.WPP_WW_Warehouse = data.Whs1.PK;
				pickParamsX.WPP_EnableAutoPackageCreationOnPicking = true;
			}

			var productAndQtys = new List<(OrgSupplierPart Part, int Qty)>();
			for (var i = 0; i < numberOfClients; i++)
			{
				var partX = Helper.CreateProduct($"PROD{i}", clients[i]);
				var qty = i + 3;
				productAndQtys.Add((partX, qty));

				Helper.CreateProductUnit(partX, partX.OP_StockKeepingUnit, Constants.PkgUnit.Pallet, qty);
			}
			Helper.Factory.Save();

			var location = data.Whs1.FindLocation("A-1");

			var palletIDs = new List<string>();
			for (var j = 0; j < numberOfClients; j++)
			{
				var palletID = $"O2{j}";
				var (part, qty) = productAndQtys[j];
				var receiveX = Helper.CreateWhsReceive(clients[j], data.Whs1, $"R1{j}");
				var receiveLine = Helper.CreateWhsReceiveLine(receiveX, part, qty, location);
				receiveLine.WE_PalletID = palletID;
				receiveX.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(receiveX);

				palletIDs.Add(palletID);
			}
			Helper.Factory.Save();

			var orders = new List<WhsOrder>();
			var orderLines = new List<WhsOrderLine>();
			for (var l = 0; l < numberOfClients; l++)
			{
				var (part, qty) = productAndQtys[l];
				var client = clients[l];
				var orderX = Helper.CreateWhsOrder(clients[l], data.Whs1, $"O2{l}");
				var orderLineX = Helper.CreateWhsOrderLine(orderX, part, qty);
				orders.Add(orderX);
				orderLines.Add(orderLineX);
			}

			var pick = Helper.CreatePickNew(orders.ToArray());
			foreach (var orderLine in orderLines)
			{
				var pickLine = orderLine.PickLines[0];
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			}
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var expectedDBHits = new Dictionary<string, int>()
				{
					{ JobDocAddressSchema.Constants.TableName, 1 },
					{ OrgAddressSchema.Constants.TableName, 3 },
					{ OrgHeaderSchema.Constants.TableName, 2 },
					{ PkgPackageSchema.Constants.TableName, 2 },
					{ PkgPackageJobSchema.Constants.TableName, 2 },
					{ WhsWarehouseSchema.Constants.TableName, 1 },
					{ OrgMiscServSchema.Constants.TableName, 1 },
					{ OrgSupplierPartSchema.Constants.TableName, 1 },
					{ PkgPackageItemDivotSchema.Constants.TableName, 1 },
					{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
					{ ProcessTasksSchema.Constants.TableName, 3 },
					{ ProcessTaskTemplateSchema.Constants.TableName, 2 },
					{ StmEventSchema.Constants.TableName, 2 },
					{ WhsDocketSchema.Constants.TableName, 4 },
					{ WhsDocketLineSchema.Constants.TableName, 5 },
					{ WhsPickSchema.Constants.TableName, 1 },
					{ RefPackTypeSchema.Constants.TableName, 1 },
					{ WhsPickLineSchema.Constants.TableName, 2 },
					{ OrgPartUnitSchema.Constants.TableName, 1 },
					{ WhsClientPickPackParamsByWhsSchema.Constants.TableName, 1 },
					{ StmALogSchema.Constants.TableName, 13 }, // These log hits are from a query with an Order By and cannot be fetch hinted.
				};

			var webService = GetNewWebService(data.Whs1, staff);
			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
			{
				var response = webService.AutoCreateCompletePalletPackagesForPick(pick.PK.ToGuid(), palletIDs.ToArray());
				AssertSuccessfulResponseWithNoErrors(response, webService);
			}

			CombineAssertions(() =>
			{
				foreach (var order in orders)
				{
					AssertEquals("Should be 1 added package for order.", 1, order.PackageJob.Packages.Count);
					AssertEquals("Has correct Pkg ID.", order.WD_ExternalReference, order.PackageJob.Packages[0].KP_PackageID);
				}
			});
		}

		#endregion

		#region TestAutoCreateCompletePalletPackagesForPick_Errors

		public void TestAutoCreateCompletePalletPackagesForPick_Errors_PickPKEmpty()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateCompletePalletPackagesForPick(Guid.Empty, new[] { "PLT1" });
			AssertEquals("Error should be on response", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Error should be on response", "Auto-packing failed. Pick PK was empty.", response.ErrorMessage);
		}

		public void TestAutoCreateCompletePalletPackagesForPick_Errors_PalletIDsNull()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateCompletePalletPackagesForPick(Guid.NewGuid(), null);
			AssertEquals("Error should be on response", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Error should be on response", "Auto-packing failed. Pallet IDs was null.", response.ErrorMessage);
		}

		public void TestAutoCreateCompletePalletPackagesForPick_Errors_PalletIDsEmpty()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateCompletePalletPackagesForPick(Guid.NewGuid(), Array.Empty<string>());
			AssertEquals("Error should be on response", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Error should be on response", "Auto-packing failed. Pallet IDs was empty.", response.ErrorMessage);
		}

		#endregion

		#region TestAutoCreateCompletePalletPackagesForPick_MultiplePackTypesForSameProduct

		public void TestAutoCreateCompletePalletPackagesForPick_MultiplePackTypesForSameProduct()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Pallet, 10m);
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Case, 3m);

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"));
			receiveLine1.WE_PalletID = "PLT1";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 3m, data.Whs1.FindLocation("A-1"));
			receiveLine2.WE_PalletID = "PLT2";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 3m);

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines[0];
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			var pickLine2 = orderLine2.PickLines[0];
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateCompletePalletPackagesForPick(pick.PK.ToGuid(), new[] { "PLT1", "PLT2" });
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			var packages = order.PackageJob.Packages;
			AssertEquals("Should be 2 added packages for order.", 2, packages.Count);

			var package1 = packages.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Pallet && p.KP_PackageID == "PLT1" && p.IsClosed);
			var packageDivots1 = package1.PackedItemDivots;
			AssertEquals("Should be 1 added package divot", 1, packageDivots1.Count);
			AssertEquals("Should be correct divot KI_ParentID", pickLine1.PK, packageDivots1[0].KI_ParentID);

			var package2 = packages.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Case && p.KP_PackageID == "PLT2" && p.IsClosed);
			var packageDivots2 = package2.PackedItemDivots;
			AssertEquals("Should be 1 added package divot", 1, packageDivots2.Count);
			AssertEquals("Should be correct divot KI_ParentID", pickLine2.PK, packageDivots2[0].KI_ParentID);
		}

		#endregion
	}
}
