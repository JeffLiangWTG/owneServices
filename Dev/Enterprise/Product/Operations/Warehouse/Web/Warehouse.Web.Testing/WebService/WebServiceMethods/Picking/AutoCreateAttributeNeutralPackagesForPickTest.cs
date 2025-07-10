using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class AutoCreateAttributeNeutralPackagesForPickTest : WhsSecureServiceTestCase
	{
		#region TestAutoCreateAttributeNeutralPackagesForPick_NoPackagesSent

		public void TestAutoCreateAttributeNeutralPackagesForPick_NoPackagesSent_EmptyCollection_AutoPackageEnabled()
		{
			TestAutoCreateAttributeNeutralPackagesForPick_NoPackagesSentCore(autoPackageEnabled: true, packageInfos: Array.Empty<AttributeNeutralPackageInfo>());
		}

		public void TestAutoCreateAttributeNeutralPackagesForPick_NoPackagesSent_EmptyCollection_AutoPackageDisabled()
		{
			TestAutoCreateAttributeNeutralPackagesForPick_NoPackagesSentCore(autoPackageEnabled: false, packageInfos: Array.Empty<AttributeNeutralPackageInfo>());
		}

		public void TestAutoCreateAttributeNeutralPackagesForPick_NoPackagesSent_NullCollection_AutoPackageEnabled()
		{
			TestAutoCreateAttributeNeutralPackagesForPick_NoPackagesSentCore(autoPackageEnabled: true, packageInfos: null);
		}

		public void TestAutoCreateAttributeNeutralPackagesForPick_NoPackagesSent_NullCollection_AutoPackageDisabled()
		{
			TestAutoCreateAttributeNeutralPackagesForPick_NoPackagesSentCore(autoPackageEnabled: false, packageInfos: null);
		}

		void TestAutoCreateAttributeNeutralPackagesForPick_NoPackagesSentCore(bool autoPackageEnabled, AttributeNeutralPackageInfo[] packageInfos)
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = autoPackageEnabled;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var location = data.Whs1.FindLocation("A-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 4m);
			Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(packageInfos);

			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);
			AssertEquals("Should be no added packages for order.", 0, order.PackageJob.Packages.Count);
		}

		#endregion

		#region TestAutoCreateAttributeNeutralPackagesForPick_SingleProduct_SinglePackage

		public void TestAutoCreateAttributeNeutralPackagesForPick_SingleProduct_SinglePackage_AutoPackageEnabled()
		{
			TestAutoCreateAttributeNeutralPackagesForPick_SingleProduct_SinglePackageCore(autoPackageEnabled: true);
		}

		public void TestAutoCreateAttributeNeutralPackagesForPick_SingleProduct_SinglePackage_AutoPackageDisabled()
		{
			TestAutoCreateAttributeNeutralPackagesForPick_SingleProduct_SinglePackageCore(autoPackageEnabled: false);
		}

		void TestAutoCreateAttributeNeutralPackagesForPick_SingleProduct_SinglePackageCore(bool autoPackageEnabled)
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = autoPackageEnabled;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var location = data.Whs1.FindLocation("A-1");
			var inv = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			inv.WI_SerialNumber = "RFT";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine.WE_SerialNumber = "RFT";

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines[0];
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(
				new AttributeNeutralPackageInfo[]
				{
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Box,
							PalletID = "PalletIDHere",
							ProductPK = data.Part1.PK.ToGuid(),
							ExpectedQuantityInPackage = 1,
							SerialNumbers = new[] { orderLine.WE_SerialNumber.ToString() },
						}
				}
			);
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			if (autoPackageEnabled)
			{
				var packages = order.PackageJob.Packages;
				AssertEquals("Should be 1 added packages for order.", 1, packages.Count);
				AssertEquals("Should be correct pack type.", Constants.PkgUnit.Box, packages[0].KP_F3_NKPackType);
				AssertEquals("Should be correct package ID.", string.Empty, packages[0].KP_PackageID);
				AssertEquals("Package should be closed.", true, packages[0].IsClosed);

				var packageDivots = packages[0].PackedItemDivots;
				AssertEquals("Should be 1 added package divot", 1, packageDivots.Count);
				AssertEquals("Should be correct divot KI_ParentID", orderLine.PickLines[0].PK, packageDivots[0].KI_ParentID);
				AssertEquals("Should be correct divot KI_ParentTableCode", WhsPickLineSchema.Constants.Prefix, packageDivots[0].KI_ParentTableCode);
				AssertEquals("Should be correct divot KI_PackedQty", 1m, packageDivots[0].KI_PackedQty);
			}
			else
			{
				AssertEquals("Should be no added packages for order.", 0, order.PackageJob.Packages.Count);
			}
		}

		#endregion

		#region TestAutoCreateAttributeNeutralPackagesForPick_SerialNumbersHasEmptyAndNullValues

		public void TestAutoCreateAttributeNeutralPackagesForPick_SerialNumbersHasEmptyAndNullValues()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var location = data.Whs1.FindLocation("A-1");
			var inv1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			inv1.WI_SerialNumber = "RFT";
			var inv2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			inv2.WI_SerialNumber = "SNO";
			var inv3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			inv3.WI_SerialNumber = "SNE";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 3m);

			var pick = Helper.CreatePickNew(order);
			orderLine.PickLines.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Now);
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(
				new AttributeNeutralPackageInfo[]
				{
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Box,
							PalletID = "PalletIDHere",
							ProductPK = data.Part1.PK.ToGuid(),
							ExpectedQuantityInPackage = 3,
							SerialNumbers = new[] { "RFT", null, string.Empty },
						}
				}
			);
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			var packages = order.PackageJob.Packages;
			AssertEquals("Should be 1 added packages for order.", 1, packages.Count);
			AssertEquals("Should be correct pack type.", Constants.PkgUnit.Box, packages[0].KP_F3_NKPackType);
			AssertEquals("Should be correct package ID.", string.Empty, packages[0].KP_PackageID);
			AssertEquals("Package should be closed.", true, packages[0].IsClosed);

			var packageDivots = packages[0].PackedItemDivots;
			AssertEquals("Should be 1 added package divot", 1, packageDivots.Count);
			var pickLine = orderLine.PickLines.Single(p => p.InventoryLine.WE_SerialNumber == "RFT");
			AssertEquals("Should be correct divot KI_ParentID", pickLine.PK, packageDivots[0].KI_ParentID);
			AssertEquals("Should be correct divot KI_ParentTableCode", WhsPickLineSchema.Constants.Prefix, packageDivots[0].KI_ParentTableCode);
		}

		#endregion

		#region TestAutoCreateAttributeNeutralPackagesForPick_MultiOrderPick

		public void TestAutoCreateAttributeNeutralPackagesForPick_MultiOrderPick_OrdersInSamePackage_AutoPackageEnabled()
		{
			TestAutoCreateAttributeNeutralPackagesForPick_MultiOrderPick_OrdersInSamePackageCore(autoPackageEnabled: true);
		}

		public void TestAutoCreateAttributeNeutralPackagesForPick_MultiOrderPick_OrdersInSamePackage_AutoPackageDisabled()
		{
			TestAutoCreateAttributeNeutralPackagesForPick_MultiOrderPick_OrdersInSamePackageCore(autoPackageEnabled: false);
		}

		void TestAutoCreateAttributeNeutralPackagesForPick_MultiOrderPick_OrdersInSamePackageCore(bool autoPackageEnabled)
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = autoPackageEnabled;

			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine1.WE_SerialNumber = "RFT";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine2.WE_SerialNumber = "DER";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			orderLine1.WE_SerialNumber = "RFT";
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			orderLine2.WE_SerialNumber = "DER";

			var pick = Helper.CreatePickNew(order1, order2);
			var pickLine1 = orderLine1.PickLines[0];
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			var pickLine2 = orderLine2.PickLines[0];
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			AssertEquals("Precondition", true, pick.IsMultiOrderPick);

			var packages =
				new AttributeNeutralPackageInfo[]
				{
					new AttributeNeutralPackageInfo()
					{
						PackType = Constants.PkgUnit.Package,
						ProductPK = data.Part1.PK.ToGuid(),
						PalletID = string.Empty,
						ExpectedQuantityInPackage = 2,
						SerialNumbers = new[]
						{
							orderLine1.WE_SerialNumber.ToString(),
							orderLine2.WE_SerialNumber.ToString()
						},
					}
				};

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(packages);
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);
			AssertEquals("Should be no added packages for order1.", 0, order1.PackageJob.Packages.Count);
			AssertEquals("Should be no added packages for order2.", 0, order2.PackageJob.Packages.Count);
		}

		public void TestAutoCreateAttributeNeutralPackagesForPick_MultiOrderPick_OrdersInDifferentPackages_AutoPackageEnabled()
		{
			TestAutoCreateAttributeNeutralPackagesForPick_MultiOrderPick_OrdersInDifferentPackagesCore(autoPackageEnabled: true);
		}

		public void TestAutoCreateAttributeNeutralPackagesForPick_MultiOrderPick_OrdersInDifferentPackages_AutoPackageDisabled()
		{
			TestAutoCreateAttributeNeutralPackagesForPick_MultiOrderPick_OrdersInDifferentPackagesCore(autoPackageEnabled: false);
		}

		void TestAutoCreateAttributeNeutralPackagesForPick_MultiOrderPick_OrdersInDifferentPackagesCore(bool autoPackageEnabled)
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = autoPackageEnabled;

			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine1.WE_SerialNumber = "RFT";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine2.WE_SerialNumber = "DER";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			orderLine1.WE_SerialNumber = "RFT";
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			orderLine2.WE_SerialNumber = "DER";

			var pick = Helper.CreatePickNew(order1, order2);
			var pickLine1 = orderLine1.PickLines[0];
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			var pickLine2 = orderLine2.PickLines[0];
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			AssertEquals("Precondition", true, pick.IsMultiOrderPick);

			var packages =
				new AttributeNeutralPackageInfo[]
				{
					new AttributeNeutralPackageInfo()
					{
						PackType = Constants.PkgUnit.Package,
						ProductPK = data.Part1.PK.ToGuid(),
						PalletID = string.Empty,
						ExpectedQuantityInPackage = 1,
						SerialNumbers = new[]
						{
							orderLine1.WE_SerialNumber.ToString()
						},
					},
					new AttributeNeutralPackageInfo()
					{
						PackType = Constants.PkgUnit.Box,
						ProductPK = data.Part1.PK.ToGuid(),
						PalletID = string.Empty,
						ExpectedQuantityInPackage = 1,
						SerialNumbers = new[]
						{
							orderLine2.WE_SerialNumber.ToString()
						},
					}
				};

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(packages);
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			if (autoPackageEnabled)
			{
				var packagesOnOrder1 = order1.PackageJob.Packages;
				AssertEquals("Should be 1 added packages for order1.", 1, packagesOnOrder1.Count);
				AssertEquals("Should be correct pack type.", Constants.PkgUnit.Package, packagesOnOrder1[0].KP_F3_NKPackType);
				AssertEquals("Should be correct package ID.", string.Empty, packagesOnOrder1[0].KP_PackageID);
				AssertEquals("Package should be closed.", true, packagesOnOrder1[0].IsClosed);

				var package1Divots = packagesOnOrder1[0].PackedItemDivots;
				AssertEquals("Should be 1 added package divot", 1, package1Divots.Count);
				AssertEquals("Should be correct divot KI_ParentID", orderLine1.PickLines[0].PK, package1Divots[0].KI_ParentID);
				AssertEquals("Should be correct divot KI_ParentTableCode", WhsPickLineSchema.Constants.Prefix, package1Divots[0].KI_ParentTableCode);
				AssertEquals("Should be correct divot KI_PackedQty", 1m, package1Divots[0].KI_PackedQty);

				var packagesOnOrder2 = order2.PackageJob.Packages;
				AssertEquals("Should be 1 added packages for order2.", 1, packagesOnOrder2.Count);
				AssertEquals("Should be correct pack type.", Constants.PkgUnit.Box, packagesOnOrder2[0].KP_F3_NKPackType);
				AssertEquals("Should be correct package ID.", string.Empty, packagesOnOrder2[0].KP_PackageID);
				AssertEquals("Package should be closed.", true, packagesOnOrder2[0].IsClosed);

				var package2Divots = packagesOnOrder2[0].PackedItemDivots;
				AssertEquals("Should be 1 added package divot", 1, package2Divots.Count);
				AssertEquals("Should be correct divot KI_ParentID", orderLine2.PickLines[0].PK, package2Divots[0].KI_ParentID);
				AssertEquals("Should be correct divot KI_ParentTableCode", WhsPickLineSchema.Constants.Prefix, package2Divots[0].KI_ParentTableCode);
				AssertEquals("Should be correct divot KI_PackedQty", 1m, package2Divots[0].KI_PackedQty);
			}
			else
			{
				AssertEquals("Should be no added packages for order1.", 0, order1.PackageJob.Packages.Count);
				AssertEquals("Should be no added packages for order2.", 0, order2.PackageJob.Packages.Count);
			}
		}

		public void TestAutoCreateAttributeNeutralPackagesForPick_MultiOrderPick_OnlyOneOrderHasAutoPackageSet()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);

			var partRelation1 = data.Part1.RelatedOrganisations[0];
			partRelation1.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation1.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams1 = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams1.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams1.WPP_EnableAutoPackageCreationOnPicking = false;

			var client2 = Helper.CreateClient("C2");
			var partRelation2 = Helper.CreateProductClientRelationShip(client2, data.Part1);
			Helper.SetClientAttributeType(client2, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(client2, data.Part1, AttributeNumber.Serial, use: true);

			partRelation2.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation2.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams2 = WhsClientPickingParams.GetClientPickingParams(client2).WarehousePickPackParams.AddNew();
			pickParams2.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams2.WPP_EnableAutoPackageCreationOnPicking = true;

			var location = data.Whs1.FindLocation("A-1");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 1m, location);
			receiveLine1.WE_SerialNumber = "RFT";
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(client2, data.Whs1, "R2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 1m, location);
			receiveLine2.WE_SerialNumber = "DER";
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			orderLine1.WE_SerialNumber = "RFT";
			var order2 = Helper.CreateWhsOrder(client2, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			orderLine2.WE_SerialNumber = "DER";

			var pick = Helper.CreatePickNew(order1, order2);
			var pickLine1 = orderLine1.PickLines[0];
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			var pickLine2 = orderLine2.PickLines[0];
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			AssertEquals("Precondition", true, pick.IsMultiOrderPick);

			var packages =
				new AttributeNeutralPackageInfo[]
				{
					new AttributeNeutralPackageInfo()
					{
						PackType = Constants.PkgUnit.Package,
						ProductPK = data.Part1.PK.ToGuid(),
						PalletID = string.Empty,
						ExpectedQuantityInPackage = 1,
						SerialNumbers = new[]
						{
							orderLine1.WE_SerialNumber.ToString()
						},
					},
					new AttributeNeutralPackageInfo()
					{
						PackType = Constants.PkgUnit.Box,
						ProductPK = data.Part1.PK.ToGuid(),
						PalletID = string.Empty,
						ExpectedQuantityInPackage = 1,
						SerialNumbers = new[]
						{
							orderLine2.WE_SerialNumber.ToString()
						},
					}
				};

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(packages);
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			AssertEquals("Should be no added packages for order1.", 0, order1.PackageJob.Packages.Count);

			var packagesOnOrder2 = order2.PackageJob.Packages;
			AssertEquals("Should be 1 added packages for order2.", 1, packagesOnOrder2.Count);
			AssertEquals("Should be correct pack type.", Constants.PkgUnit.Box, packagesOnOrder2[0].KP_F3_NKPackType);
			AssertEquals("Should be correct package ID.", string.Empty, packagesOnOrder2[0].KP_PackageID);
			AssertEquals("Package should be closed.", true, packagesOnOrder2[0].IsClosed);

			var package2Divots = packagesOnOrder2[0].PackedItemDivots;
			AssertEquals("Should be 1 added package divot", 1, package2Divots.Count);
			AssertEquals("Should be correct divot KI_ParentID", orderLine2.PickLines[0].PK, package2Divots[0].KI_ParentID);
			AssertEquals("Should be correct divot KI_ParentTableCode", WhsPickLineSchema.Constants.Prefix, package2Divots[0].KI_ParentTableCode);
			AssertEquals("Should be correct divot KI_PackedQty", 1m, package2Divots[0].KI_PackedQty);
		}

		public void TestAutoCreateAttributeNeutralPackagesForPick_MultiOrderPick_DbHits()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			const int numberOfClients = 10;

			var clients = new List<OrgHeader>();
			for (var i = 0; i < numberOfClients; i++)
			{
				var clientX = Helper.CreateClient($"C2{i}");
				clients.Add(clientX);

				var partRelationX = Helper.CreateProductClientRelationShip(clientX, data.Part1);
				Helper.SetClientAttributeType(clientX, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(clientX, data.Part1, AttributeNumber.Serial, use: true);

				partRelationX.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
				partRelationX.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

				var pickParamsX = WhsClientPickingParams.GetClientPickingParams(clientX).WarehousePickPackParams.AddNew();
				pickParamsX.WPP_WW_Warehouse = data.Whs1.PK;
				pickParamsX.WPP_EnableAutoPackageCreationOnPicking = true;
			}

			var location = data.Whs1.FindLocation("A-1");

			for (var j = 0; j < numberOfClients; j++)
			{
				var receive1X = Helper.CreateWhsReceive(data.Org1, data.Whs1, $"R1{j}");
				var receiveLine1 = Helper.CreateWhsReceiveLine(receive1X, data.Part1, 1m, location);
				receiveLine1.WE_SerialNumber = $"RFT{j}";
				receive1X.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(receive1X);

				var client = clients[j];
				var receive2X = Helper.CreateWhsReceive(client, data.Whs1, $"R2{j}");
				var receiveLine2 = Helper.CreateWhsReceiveLine(receive2X, data.Part1, 1m, location);
				receiveLine2.WE_SerialNumber = $"DER{j}";
				receive2X.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(receive2X);
			}
			Helper.Factory.Save();

			var orders = new List<WhsOrder>();
			var orderLines = new List<WhsOrderLine>();
			for (var l = 0; l < numberOfClients; l++)
			{
				var order1X = Helper.CreateWhsOrder(data.Org1, data.Whs1, $"O1{l}");
				var orderLine1X = Helper.CreateWhsOrderLine(order1X, data.Part1, 1m);
				orderLine1X.WE_SerialNumber = $"RFT{l}";
				orders.Add(order1X);
				orderLines.Add(orderLine1X);

				var client = clients[l];
				var order2X = Helper.CreateWhsOrder(client, data.Whs1, $"O2{l}");
				var orderLine2X = Helper.CreateWhsOrderLine(order2X, data.Part1, 1m);
				orderLine2X.WE_SerialNumber = $"DER{l}";
				orders.Add(order2X);
				orderLines.Add(orderLine2X);
			}

			var pick = Helper.CreatePickNew(orders.ToArray());
			foreach (var orderLine in orderLines)
			{
				var pickLine = orderLine.PickLines[0];
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			}
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			AssertEquals("Precondition", true, pick.IsMultiOrderPick);

			var packageInfos = new List<AttributeNeutralPackageInfo>();
			for (var p = 0; p < numberOfClients; p++)
			{
				var info1X =
					new AttributeNeutralPackageInfo()
					{
						PackType = Constants.PkgUnit.Package,
						ProductPK = data.Part1.PK.ToGuid(),
						PalletID = string.Empty,
						ExpectedQuantityInPackage = 1,
						SerialNumbers = new[]
						{
							$"RFT{p}"
						},
					};
				packageInfos.Add(info1X);

				var info2X =
					new AttributeNeutralPackageInfo()
					{
						PackType = Constants.PkgUnit.Package,
						ProductPK = data.Part1.PK.ToGuid(),
						PalletID = string.Empty,
						ExpectedQuantityInPackage = 1,
						SerialNumbers = new[]
						{
							$"DER{p}"
						},
					};
				packageInfos.Add(info2X);
			}

			var webService = GetNewWebService(data.Whs1);
			var expectedDBHits = new Dictionary<string, int>()
				{
					{ JobDocAddressSchema.Constants.TableName, 1 },
					{ OrgAddressSchema.Constants.TableName, 3 },
					{ OrgHeaderSchema.Constants.TableName, 2 },
					{ PkgPackageSchema.Constants.TableName, 2 },
					{ PkgPackageJobSchema.Constants.TableName, 2 },
					{ WhsWarehouseSchema.Constants.TableName, 1 },
					{ OrgMiscServSchema.Constants.TableName, 1 },
					{ OrgPartRelationSchema.Constants.TableName, 1 },
					{ OrgSupplierPartSchema.Constants.TableName, 1 },
					{ PkgPackageItemDivotSchema.Constants.TableName, 1 },
					{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
					{ ProcessTasksSchema.Constants.TableName, 3 },
					{ ProcessTaskTemplateSchema.Constants.TableName, 2 },
					{ RefPackTypeSchema.Constants.TableName, 1 },
					{ StmEventSchema.Constants.TableName, 2 },
					{ WhsClientPickPackParamsByWhsSchema.Constants.TableName, 1 },
					{ WhsDocketSchema.Constants.TableName, 4 },
					{ WhsDocketLineSchema.Constants.TableName, 5 },
					{ WhsPickSchema.Constants.TableName, 1 },
					{ WhsPickLineSchema.Constants.TableName, 2 },
					{ OrgPartUnitSchema.Constants.TableName, 1 },
					{ StmALogSchema.Constants.TableName, 22 }, // These log hits are from a query with an Order By and cannot be fetch hinted.
				};

			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
			{
				var response = webService.AutoCreateAttributeNeutralPackagesForPick(packageInfos.ToArray());
				AssertSuccessfulResponseWithNoErrors(response, webService);
			}

			CombineAssertions(() =>
			{
				foreach (var order in orders)
				{
					AssertEquals("Should be 1 added package for order.", 1, order.PackageJob.Packages.Count);
				}
			});
		}

		#endregion

		#region TestAutoCreateAttributeNeutralPackagesForPick_SingleProduct_SinglePackage_MultipleSerials

		public void TestAutoCreateAttributeNeutralPackagesForPick_SingleProduct_SinglePackage_MultipleSerials_AutoPackageEnabled()
		{
			TestAutoCreateAttributeNeutralPackagesForPick_SingleProduct_SinglePackage_MultipleSerialsCore(autoPackageEnabled: true);
		}

		public void TestAutoCreateAttributeNeutralPackagesForPick_SingleProduct_SinglePackage_MultipleSerials_AutoPackageDisabled()
		{
			TestAutoCreateAttributeNeutralPackagesForPick_SingleProduct_SinglePackage_MultipleSerialsCore(autoPackageEnabled: false);
		}

		void TestAutoCreateAttributeNeutralPackagesForPick_SingleProduct_SinglePackage_MultipleSerialsCore(bool autoPackageEnabled)
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = autoPackageEnabled;

			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine1.WE_SerialNumber = "RFT";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine2.WE_SerialNumber = "DER";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine1.WE_SerialNumber = "RFT";
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine2.WE_SerialNumber = "DER";

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines[0];
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			var pickLine2 = orderLine2.PickLines[0];
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var packagesToCreate =
				new AttributeNeutralPackageInfo[]
				{
					new AttributeNeutralPackageInfo()
					{
						PackType = Constants.PkgUnit.Box,
						ProductPK = data.Part1.PK.ToGuid(),
						PalletID = string.Empty,
						ExpectedQuantityInPackage = 2,
						SerialNumbers = new[]
						{
							orderLine1.WE_SerialNumber.ToString(),
							orderLine2.WE_SerialNumber.ToString()
						},
					}
				};

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(packagesToCreate);
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			if (autoPackageEnabled)
			{
				var packages = order.PackageJob.Packages;
				AssertEquals("Should be 1 added packages for order.", 1, packages.Count);
				AssertEquals("Should be correct pack type.", Constants.PkgUnit.Box, packages[0].KP_F3_NKPackType);
				AssertEquals("Package should be closed.", true, packages[0].IsClosed);

				var packageDivots = packages[0].PackedItemDivots;
				AssertEquals("Should be 2 added package divots", 2, packageDivots.Count);
				AssertContainsExactElementsInAnyOrder("Should be correct divot KI_ParentIDs", new[] { orderLine1.PickLines[0].PK, orderLine2.PickLines[0].PK }, packageDivots.Select(p => p.KI_ParentID));
			}
			else
			{
				AssertEquals("Should be no added packages for order.", 0, order.PackageJob.Packages.Count);
			}
		}

		#endregion

		#region TestAutoCreateAttributeNeutralPackagesForPick_SingleProduct_SameOrder_MultiplePackages

		public void TestAutoCreateAttributeNeutralPackagesForPick_SingleProduct_SameOrder_MultiplePackages_AutoPackageEnabled()
		{
			TestAutoCreateAttributeNeutralPackagesForPick_SingleProduct_SameOrder_MultiplePackagesCore(autoPackageEnabled: true);
		}

		public void TestAutoCreateAttributeNeutralPackagesForPick_SingleProduct_SameOrder_MultiplePackages_AutoPackageDisabled()
		{
			TestAutoCreateAttributeNeutralPackagesForPick_SingleProduct_SameOrder_MultiplePackagesCore(autoPackageEnabled: false);
		}

		void TestAutoCreateAttributeNeutralPackagesForPick_SingleProduct_SameOrder_MultiplePackagesCore(bool autoPackageEnabled)
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = autoPackageEnabled;

			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine1.WE_SerialNumber = "DER";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine2.WE_SerialNumber = "RFT";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine.PickLines[0];
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			var pickLine2 = orderLine.PickLines[1];
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var packagesToCreate =
				new AttributeNeutralPackageInfo[]
				{
					new AttributeNeutralPackageInfo()
					{
						PackType = Constants.PkgUnit.Box,
						ProductPK = data.Part1.PK.ToGuid(),
						PalletID = string.Empty,
						ExpectedQuantityInPackage = 1,
						SerialNumbers = new[] { "DER" },
					},
					new AttributeNeutralPackageInfo()
					{
						PackType = Constants.PkgUnit.Box,
						ProductPK = data.Part1.PK.ToGuid(),
						PalletID = string.Empty,
						ExpectedQuantityInPackage = 1,
						SerialNumbers = new[] { "RFT" },
					}
				};

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(packagesToCreate);
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			if (autoPackageEnabled)
			{
				var packages = order.PackageJob.Packages;
				AssertEquals("Should be 2 added packages for order.", 2, packages.Count);

				// Pkg 1
				AssertEquals("Should be correct pack type.", Constants.PkgUnit.Box, packages[0].KP_F3_NKPackType);
				AssertEquals("Package should be closed.", true, packages[0].IsClosed);

				var package1Divots = packages[0].PackedItemDivots;
				AssertEquals("Should be 1 added package divots", 1, package1Divots.Count);

				var orderLine1Divot = package1Divots.Single(d => d.KI_ParentID == orderLine.PickLines[0].PK);
				AssertEquals("Should be correct orderLine1Divot KI_PackedQty", 1m, orderLine1Divot.KI_PackedQty);

				// Pkg 2
				AssertEquals("Should be correct pack type.", Constants.PkgUnit.Box, packages[1].KP_F3_NKPackType);
				AssertEquals("Package should be closed.", true, packages[1].IsClosed);

				var package2Divots = packages[1].PackedItemDivots;
				AssertEquals("Should be 1 added package divots", 1, package2Divots.Count);

				var orderLine2Divot = package2Divots.Single(d => d.KI_ParentID == orderLine.PickLines[1].PK);
				AssertEquals("Should be correct orderLine2Divot KI_PackedQty", 1m, orderLine2Divot.KI_PackedQty);
			}
			else
			{
				AssertEquals("Should be no added packages for order.", 0, order.PackageJob.Packages.Count);
			}
		}

		#endregion

		#region TestAutoCreateAttributeNeutralPackagesForPick_TwoProducts_SameOrder_SameSerialNumber

		public void TestAutoCreateAttributeNeutralPackagesForPick_TwoProducts_SameOrder_SameSerialNumber()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);
			var part1Relation = data.Part1.RelatedOrganisations[0];
			part1Relation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			part1Relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, use: true);
			var part2Relation = data.Part2.RelatedOrganisations[0];
			part2Relation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			part2Relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine1.WE_SerialNumber = "RFT";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, location);
			receiveLine2.WE_SerialNumber = "RFT";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine1.WE_SerialNumber = "RFT";
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 1m);
			orderLine2.WE_SerialNumber = "RFT";

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines[0];
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			var pickLine2 = orderLine2.PickLines[0];
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var packagesToCreate =
				new AttributeNeutralPackageInfo[]
				{
					new AttributeNeutralPackageInfo()
					{
						PackType = Constants.PkgUnit.Box,
						ProductPK = data.Part2.PK.ToGuid(),
						PalletID = string.Empty,
						ExpectedQuantityInPackage = 1,
						SerialNumbers = new[] { "RFT" },
					}
				};

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(packagesToCreate);
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			var packages = order.PackageJob.Packages;
			AssertEquals("Should be 1 added packages for order.", 1, packages.Count);
			AssertEquals("Should be correct pack type.", Constants.PkgUnit.Box, packages[0].KP_F3_NKPackType);
			AssertEquals("Package should be closed.", true, packages[0].IsClosed);

			var packageDivots = packages[0].PackedItemDivots;
			AssertEquals("Should be 1 added package divots", 1, packageDivots.Count);

			var orderLineDivot = packageDivots.Single(d => d.KI_ParentID == orderLine2.PickLines[0].PK);
			AssertEquals("Should be correct orderLine1Divot KI_PackedQty", 1m, orderLineDivot.KI_PackedQty);
		}

		#endregion

		#region TestAutoCreateAttributeNeutralPackagesForPick_TwoPicks_TwoOrders_TwoProducts_TwoPackages_SameSerialNumbers

		public void TestAutoCreateAttributeNeutralPackagesForPick_TwoPicks_TwoOrders_TwoProducts_TwoPackages_SameSerialNumbers()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);
			var part1Relation = data.Part1.RelatedOrganisations[0];
			part1Relation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			part1Relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, use: true);
			var part2Relation = data.Part2.RelatedOrganisations[0];
			part2Relation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			part2Relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine1.WE_SerialNumber = "RFT";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine2.WE_SerialNumber = "DAF";
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, location);
			receiveLine3.WE_SerialNumber = "RFT";
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, location);
			receiveLine4.WE_SerialNumber = "DAF";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine1.WE_SerialNumber = "RFT";
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part2, 1m);
			orderLine3.WE_SerialNumber = "DAF";

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			orderLine2.WE_SerialNumber = "DAF";

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines[0];
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			var pickLine3 = orderLine3.PickLines[0];
			pickLine3.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();

			var pick2 = Helper.CreatePickNew(order2);
			var pickLine2 = orderLine2.PickLines[0];
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var packagesToCreate =
				new AttributeNeutralPackageInfo[]
				{
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Box,
							ProductPK = data.Part1.PK.ToGuid(),
							PalletID = string.Empty,
							ExpectedQuantityInPackage = 1,
							SerialNumbers = new[] { "RFT" },
						},
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Box,
							ProductPK = data.Part2.PK.ToGuid(),
							PalletID = string.Empty,
							ExpectedQuantityInPackage = 1,
							SerialNumbers = new[] { "DAF" },
						}
				};

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(packagesToCreate);
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			var packages = order.PackageJob.Packages;
			AssertEquals("Should be 2 added packages for order.", 2, packages.Count);
			AssertEquals("Should be correct pack type.", Constants.PkgUnit.Box, packages[0].KP_F3_NKPackType);
			AssertEquals("Package should be closed.", true, packages[0].IsClosed);
			AssertEquals("Should be correct pack type.", Constants.PkgUnit.Box, packages[1].KP_F3_NKPackType);
			AssertEquals("Package should be closed.", true, packages[1].IsClosed);

			var package1Divots = packages[0].PackedItemDivots;
			AssertEquals("Should be 1 added package divots", 1, package1Divots.Count);

			var orderLine1Divot = package1Divots.Single(d => d.KI_ParentID == orderLine1.PickLines[0].PK);
			AssertEquals("Should be correct orderLine1Divot KI_PackedQty", 1m, orderLine1Divot.KI_PackedQty);

			var package2Divots = packages[1].PackedItemDivots;
			AssertEquals("Should be 1 added package divots", 1, package2Divots.Count);

			var orderLine3Divot = package2Divots.Single(d => d.KI_ParentID == orderLine3.PickLines[0].PK);
			AssertEquals("Should be correct orderLine3Divot KI_PackedQty", 1m, orderLine3Divot.KI_PackedQty);
		}

		#endregion

		#region TestAutoCreateAttributeNeutralPackagesForPick_MultipleProduct_MultiplePackages

		public void TestAutoCreateAttributeNeutralPackagesForPick_MultipleProduct_MultiplePackages_AutoPackageEnabled()
		{
			TestAutoCreateAttributeNeutralPackagesForPick_MultipleProduct_MultiplePackagesCore(autoPackageEnabled: true);
		}

		public void TestAutoCreateAttributeNeutralPackagesForPick_MultipleProduct_MultiplePackages_AutoPackageDisabled()
		{
			TestAutoCreateAttributeNeutralPackagesForPick_MultipleProduct_MultiplePackagesCore(autoPackageEnabled: false);
		}

		void TestAutoCreateAttributeNeutralPackagesForPick_MultipleProduct_MultiplePackagesCore(bool autoPackageEnabled)
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);
			var partRelation1 = data.Part1.RelatedOrganisations[0];
			partRelation1.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation1.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, use: true);
			var partRelation2 = data.Part1.RelatedOrganisations[0];
			partRelation2.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation2.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = autoPackageEnabled;

			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine1.WE_SerialNumber = "RFT";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, location);
			receiveLine2.WE_SerialNumber = "DER";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine1.WE_SerialNumber = "RFT";
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 1m);
			orderLine2.WE_SerialNumber = "DER";

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines[0];
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			var pickLine2 = orderLine2.PickLines[0];
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var packagesToCreate =
				new AttributeNeutralPackageInfo[]
				{
					new AttributeNeutralPackageInfo()
					{
						PackType = Constants.PkgUnit.Box,
						ProductPK = data.Part1.PK.ToGuid(),
						PalletID = string.Empty,
						ExpectedQuantityInPackage = 1,
						SerialNumbers = new[] { orderLine1.WE_SerialNumber.ToString() },
					},
					new AttributeNeutralPackageInfo()
					{
						PackType = Constants.PkgUnit.Package,
						ProductPK = data.Part2.PK.ToGuid(),
						PalletID = string.Empty,
						ExpectedQuantityInPackage = 2,
						SerialNumbers = new[] { orderLine2.WE_SerialNumber.ToString() },
					}
				};

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(packagesToCreate);
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			if (autoPackageEnabled)
			{
				var packages = order.PackageJob.Packages;
				AssertEquals("Should be 2 added packages for order.", 2, packages.Count);

				var package1 = packages.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Box && p.IsClosed);
				var packageDivots1 = package1.PackedItemDivots;
				AssertEquals("Should be 1 added package divot", 1, packageDivots1.Count);
				AssertEquals("Should be correct divot KI_ParentID", order.Lines[0].PickLines[0].PK, packageDivots1[0].KI_ParentID);

				var package2 = packages.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Package && p.IsClosed);
				var packageDivots2 = package2.PackedItemDivots;
				AssertEquals("Should be 1 added package divot", 1, packageDivots2.Count);
				AssertEquals("Should be correct divot KI_ParentID", order.Lines[1].PickLines[0].PK, packageDivots2[0].KI_ParentID);
			}
			else
			{
				AssertEquals("Should be no added packages for order1.", 0, order.PackageJob.Packages.Count);
			}
		}

		#endregion

		#region TestAutoCreateAttributeNeutralPackagesForPick_ErrorDuringPackageCreation

		public void TestAutoCreateAttributeNeutralPackagesForPick_ErrorDuringPackageCreation_AutoPackageEnabled()
		{
			TestAutoCreateAttributeNeutralPackagesForPick_ErrorDuringPackageCreationCore(autoPackageEnabled: true);
		}

		public void TestAutoCreateAttributeNeutralPackagesForPick_ErrorDuringPackageCreation_AutoPackageDisabled()
		{
			TestAutoCreateAttributeNeutralPackagesForPick_ErrorDuringPackageCreationCore(autoPackageEnabled: false);
		}

		void TestAutoCreateAttributeNeutralPackagesForPick_ErrorDuringPackageCreationCore(bool autoPackageEnabled)
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = autoPackageEnabled;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var location = data.Whs1.FindLocation("A-1");
			var inv = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			inv.WI_SerialNumber = "RFT";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine.WE_SerialNumber = "RFT";

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines[0];
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "Pkg1";
			package.Pack(orderLine.ReleaseLines[0], 1m);
			Helper.Factory.Save();

			AssertEquals("Precondtion: orderline already packed", orderLine.PickLines[0].PK, package.PackedItemDivots[0].KI_ParentID);

			var packagesToCreate =
				new AttributeNeutralPackageInfo[]
				{
					new AttributeNeutralPackageInfo()
					{
						PackType = Constants.PkgUnit.Box,
						ProductPK = data.Part1.PK.ToGuid(),
						PalletID = string.Empty,
						ExpectedQuantityInPackage = 1,
						SerialNumbers = new[] { orderLine.WE_SerialNumber.ToString() },
					}
				};

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(packagesToCreate);

			if (autoPackageEnabled)
			{
				AssertEquals("Should be error response", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Error message should be correct", "Auto-packing failed. 'Pick Line to Pack for Attribute Neutral should be unpacked.'", response.ErrorMessage);
			}
			else
			{
				AssertEquals("Should be no error response", ErrorTypes.None, response.Error);
			}
			AssertEquals("Should be no additional packages for order.", 1, order.PackageJob.Packages.Count);
		}

		#endregion

		#region TestAutoCreateAttributeNeutralPackagesForPick_SaveFailure

		public void TestAutoCreateAttributeNeutralPackagesForPick_SaveFailure_AutoPackageEnabled()
		{
			TestAutoCreateAttributeNeutralPackagesForPick_SaveFailureCore(autoPackageEnabled: true);
		}

		public void TestAutoCreateAttributeNeutralPackagesForPick_SaveFailure_AutoPackageDisabled()
		{
			TestAutoCreateAttributeNeutralPackagesForPick_SaveFailureCore(autoPackageEnabled: false);
		}

		void TestAutoCreateAttributeNeutralPackagesForPick_SaveFailureCore(bool autoPackageEnabled)
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = autoPackageEnabled;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var location = data.Whs1.FindLocation("A-1");
			var inv = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			inv.WI_SerialNumber = "RFT";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine.WE_SerialNumber = "RFT";

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines[0];
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			webService.Factory.Saving += f => throw new ZCannotSaveException("Test Cannot Save failure", "Test", new Exception("Inner exception"));

			var packagesToCreate =
				new AttributeNeutralPackageInfo[]
				{
					new AttributeNeutralPackageInfo()
					{
						PackType = Constants.PkgUnit.Box,
						ProductPK = data.Part1.PK.ToGuid(),
						PalletID = string.Empty,
						ExpectedQuantityInPackage = 1,
						SerialNumbers = new[] { orderLine.WE_SerialNumber.ToString() },
					}
				};

			var response = webService.AutoCreateAttributeNeutralPackagesForPick(packagesToCreate);

			if (autoPackageEnabled)
			{
				AssertEquals("Should be error response", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Error message should be correct", "Auto-packing failed. A saving error occurred whilst attempting to auto-pack:\r\nTest Cannot Save failure\r\nInner exception", response.ErrorMessage);
				AssertEquals("Error expected", "Auto-packing failed. A saving error occurred whilst attempting to auto-pack:\r\nTest Cannot Save failure\r\nInner exception", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
			else
			{
				AssertEquals("Should be no error response", ErrorTypes.None, response.Error);
				AssertEquals("No error expected", string.Empty, ErrorReporter.LastMessageReported);
			}
			AssertEquals("Should be no added packages for order.", 0, order.PackageJob.Packages.Count);
		}

		#endregion

		#region TestAutoCreateAttributeNeutralPackagesForPick_FullPalletPicking

		public void TestAutoCreateAttributeNeutralPackagesForPick_FullPalletPicking_AutoPackageEnabled_CompleteNotSelected()
		{
			TestAutoCreateAttributeNeutralPackagesForPick_FullPalletPickingCore(autoPackageEnabled: true);
		}

		public void TestAutoCreateAttributeNeutralPackagesForPick_FullPalletPicking_AutoPackageDisabled_CompleteNotSelected()
		{
			TestAutoCreateAttributeNeutralPackagesForPick_FullPalletPickingCore(autoPackageEnabled: false);
		}

		void TestAutoCreateAttributeNeutralPackagesForPick_FullPalletPickingCore(bool autoPackageEnabled)
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = autoPackageEnabled;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var location = data.Whs1.FindLocation("A-1");
			var inv1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			inv1.WI_SerialNumber = "RFT1";
			inv1.WI_PalletID = "RFTPalletIDHere";
			var inv2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			inv2.WI_SerialNumber = "RFT2";
			inv2.WI_PalletID = "RFTPalletIDHere";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine.PickLines[0];
			Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			var pickLine2 = orderLine.PickLines[1];
			Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var packagesToCreate =
				new AttributeNeutralPackageInfo[]
				{
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Case,
							ProductPK = data.Part1.PK.ToGuid(),
							PalletID = "RFTPalletIDHere",
							ExpectedQuantityInPackage = 2,
							SerialNumbers = new[] { "RFT1", "RFT2" },
						}
				};

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(packagesToCreate);
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			if (autoPackageEnabled)
			{
				var packages = order.PackageJob.Packages;
				AssertEquals("Should be 1 added packages for order.", 1, packages.Count);
				AssertEquals("Should be correct pack type.", Constants.PkgUnit.Case, packages[0].KP_F3_NKPackType);
				AssertEquals("Package should be closed.", true, packages[0].IsClosed);
				AssertEquals(
					"Should be correct package ID.", "RFTPALLETIDHERE", packages[0].KP_PackageID);

				var packageDivots = packages[0].PackedItemDivots;
				AssertEquals("Should be 1 added package divot", 2, packageDivots.Count);
				AssertContainsExactElementsInAnyOrder(
					"Should be correct divot KI_ParentIDs",
					new[] { orderLine.PickLines[0].PK, orderLine.PickLines[1].PK },
					packageDivots.Select(d => d.KI_ParentID));
			}
			else
			{
				AssertEquals("Should be no added packages for order.", 0, order.PackageJob.Packages.Count);
			}
		}

		public void TestAutoCreateAttributeNeutralPackagesForPick_FullPalletPicking()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var location = data.Whs1.FindLocation("A-1");
			var inv = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			inv.WI_SerialNumber = "RFT";
			inv.WI_PalletID = "RFTPalletIDHere";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine.WE_SerialNumber = "RFT";

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines[0];
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var packagesToCreate =
				new AttributeNeutralPackageInfo[]
				{
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Case,
							ProductPK = data.Part1.PK.ToGuid(),
							PalletID = "RFTPalletIDHere",
							ExpectedQuantityInPackage = 1,
							SerialNumbers = new[] { orderLine.WE_SerialNumber.ToString() },
						}
				};

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(packagesToCreate);
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			var packages = order.PackageJob.Packages;
			AssertEquals("Should be 1 added packages for order.", 1, packages.Count);
			AssertEquals("Should be correct pack type.", Constants.PkgUnit.Case, packages[0].KP_F3_NKPackType);
			AssertEquals("Package should be closed.", true, packages[0].IsClosed);
			AssertEquals("Should be correct package ID.", "RFTPALLETIDHERE", packages[0].KP_PackageID);

			var packageDivots = packages[0].PackedItemDivots;
			AssertEquals("Should be 1 added package divot", 1, packageDivots.Count);
			AssertEquals("Should be correct divot KI_ParentID", orderLine.PickLines[0].PK, packageDivots[0].KI_ParentID);
		}

		#endregion

		#region TestAutoCreateAttributeNeutralPackagesForPick_FullPalletPicking_MultipleSerials

		public void TestAutoCreateAttributeNeutralPackagesForPick_FullPalletPicking_MultipleSerials_AutoPackageEnabled()
		{
			TestAutoCreateAttributeNeutralPackagesForPick_FullPalletPicking_MultipleSerialsCore(autoPackageEnabled: true);
		}

		public void TestAutoCreateAttributeNeutralPackagesForPick_FullPalletPicking_MultipleSerials_AutoPackageDisabled()
		{
			TestAutoCreateAttributeNeutralPackagesForPick_FullPalletPicking_MultipleSerialsCore(autoPackageEnabled: false);
		}

		void TestAutoCreateAttributeNeutralPackagesForPick_FullPalletPicking_MultipleSerialsCore(bool autoPackageEnabled)
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
			pickParams.WPP_EnableAutoPackageCreationOnPicking = autoPackageEnabled;

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
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			var pickLine2 = orderLine.PickLines[1];
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			var pickLine3 = orderLine.PickLines[2];
			pickLine3.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var packagesToCreate =
				new AttributeNeutralPackageInfo[]
				{
					new AttributeNeutralPackageInfo()
					{
						PackType = Constants.PkgUnit.Pallet,
						ProductPK = data.Part1.PK.ToGuid(),
						PalletID = "PalletIDHere",
						ExpectedQuantityInPackage = 3,
						SerialNumbers = new[] { "SN1", "SN2", "SN3" },
					}
				};

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(packagesToCreate);
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			var packages = order.PackageJob.Packages;

			if (autoPackageEnabled)
			{
				AssertEquals("Should be 1 added packages for order.", 1, packages.Count);
				AssertEquals("Should be correct pack type.", Constants.PkgUnit.Pallet, packages[0].KP_F3_NKPackType);
				AssertEquals("Package should be closed.", true, packages[0].IsClosed);
				AssertEquals("Should be correct package ID.", "PALLETIDHERE", packages[0].KP_PackageID);

				var packageDivots = packages[0].PackedItemDivots;
				AssertEquals("Should be 3 added package divots", 3, packageDivots.Count);
				AssertContainsExactElementsInAnyOrder("Should be correct divot KI_ParentIDs", orderLine.PickLines.Select(l => l.PK), packageDivots.Select(p => p.KI_ParentID));
			}
			else
			{
				AssertEquals("Should be 0 added packages for order.", 0, packages.Count);
			}
		}

		#endregion

		#region TestAutoCreateAttributeNeutralPackagesForPick_PickLinesAndSerialNumberCountsDoNotMatch

		public void TestAutoCreateAttributeNeutralPackagesForPick_PickLinesAndSerialNumberCountsDoNotMatch_SerialNull()
		{
			TestAutoCreateAttributeNeutralPackagesForPick_PickLinesAndSerialNumberCountsDoNotMatch_SerialCore(serial: null);
		}

		public void TestAutoCreateAttributeNeutralPackagesForPick_PickLinesAndSerialNumberCountsDoNotMatch_SerialEmpty()
		{
			TestAutoCreateAttributeNeutralPackagesForPick_PickLinesAndSerialNumberCountsDoNotMatch_SerialCore(serial: string.Empty);
		}

		void TestAutoCreateAttributeNeutralPackagesForPick_PickLinesAndSerialNumberCountsDoNotMatch_SerialCore(string serial)
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var location = data.Whs1.FindLocation("A-1");
			var inv = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			inv.WI_SerialNumber = "RFT";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine.WE_SerialNumber = "RFT";
			Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var packagesToCreate =
				new AttributeNeutralPackageInfo[]
				{
					new AttributeNeutralPackageInfo()
					{
						PackType = Constants.PkgUnit.Box,
						ProductPK = data.Part1.PK.ToGuid(),
						PalletID = "PalletIDHere",
						ExpectedQuantityInPackage = 1,
						SerialNumbers = new[] { serial },
					}
				};

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(packagesToCreate);

			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);
			AssertEquals("Should be no added packages for order1.", 0, order.PackageJob.Packages.Count);
		}

		public void TestAutoCreateAttributeNeutralPackagesForPick_PickLinesAndSerialNumberCountsDoNotMatch_NoPickLinePK()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var location = data.Whs1.FindLocation("A-1");
			var inv = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			inv.WI_SerialNumber = "RFT";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine.WE_SerialNumber = "RFT";
			Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var packagesToCreate =
				new AttributeNeutralPackageInfo[]
				{
					new AttributeNeutralPackageInfo()
					{
						PackType = Constants.PkgUnit.Box,
						ProductPK = data.Part1.PK.ToGuid(),
						ExpectedQuantityInPackage = 1,
						SerialNumbers = new[] { "serial" },
					}
				};

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(packagesToCreate);

			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);
			AssertEquals("Should be no added packages for order1.", 0, order.PackageJob.Packages.Count);
		}

		#endregion

		#region TestAutoCreateAttributeNeutralPackagesForPick_ExtraSerialProvided

		public void TestAutoCreateAttributeNeutralPackagesForPick_ExtraSerialProvided()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var location = data.Whs1.FindLocation("A-1");
			var inv = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			inv.WI_SerialNumber = "RFT";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine.WE_SerialNumber = "RFT";

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines[0];
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(
				new AttributeNeutralPackageInfo[]
				{
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Box,
							PalletID = "PalletIDHere",
							ProductPK = data.Part1.PK.ToGuid(),
							ExpectedQuantityInPackage = 2,
							SerialNumbers = new[] { "RFT", "NOTSERIAL" },
						}
				}
			);
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			var packages = order.PackageJob.Packages;
			AssertEquals("Should be 1 added packages for order from first correct serial number.", 1, packages.Count);
			AssertEquals("Error expected", $"Attempting to pack serial number 'NOTSERIAL' and productPK '{data.Part1.PK}' but orderLine dictionary does not contain values.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#endregion

		#region TestAutoCreateAttributeNeutralPackagesForPick_PkgsAcrossOrders

		public void TestAutoCreateAttributeNeutralPackagesForPick_PkgsAcrossOrders()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine1.WE_SerialNumber = "RFT";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine2.WE_SerialNumber = "DER";
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine3.WE_SerialNumber = "EWD";
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine4.WE_SerialNumber = "FRT";
			var receiveLine5 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine5.WE_SerialNumber = "GBE";
			var receiveLine6 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine6.WE_SerialNumber = "XSD";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var orderLine3 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine4 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			var orderLine5 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			var orderLine6 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);

			var pick = Helper.CreatePickNew(order1, order2);
			pick.GetAllPickLines().ForEach(line => line.WZ_PickedDateTime = ZDateTimeOffset.Now);
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			AssertEquals("Precondition", true, pick.IsMultiOrderPick);

			var pkg1Serials = new[]
			{
					orderLine1.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
					orderLine3.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
					orderLine5.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
				};

			var pkg2Serials = new[]
			{
					orderLine2.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
					orderLine4.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
					orderLine6.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
				};
			var packages =
				new AttributeNeutralPackageInfo[]
				{
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Package,
							ProductPK = data.Part1.PK.ToGuid(),
							PalletID = string.Empty,
							ExpectedQuantityInPackage = 3,
							SerialNumbers = pkg1Serials,
						},
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Package,
							ProductPK = data.Part1.PK.ToGuid(),
							PalletID = string.Empty,
							ExpectedQuantityInPackage = 3,
							SerialNumbers = pkg2Serials,
						},
				};

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(packages);
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var order1R = webService.Factory.Load<WhsOrder>(order1.PK);
			var order2R = webService.Factory.Load<WhsOrder>(order2.PK);

			// Order 1 Pkg
			var packages1 = order1R.PackageJob.Packages;

			AssertEquals("Should be correct pack type.", Constants.PkgUnit.Package, packages1[0].KP_F3_NKPackType);
			AssertEquals("Package should be closed.", true, packages1[0].IsClosed);
			AssertEquals("Should be correct package ID.", string.Empty, packages1[0].KP_PackageID);

			var packageDivots1 = packages1[0].PackedItemDivots;
			AssertEquals("Should be 3 added package divot", 3, packageDivots1.Count);
			AssertContainsExactElementsInAnyOrder(
				"Should be correct divot KI_ParentIDs",
				new[] { orderLine1.PickLines[0].PK, orderLine2.PickLines[0].PK, orderLine3.PickLines[0].PK },
				packageDivots1.Select(d => d.KI_ParentID));
			AssertEquals("Should be correct KI_ParentTableCode for all divots", true, packageDivots1.All(d => d.KI_ParentTableCode == WhsPickLineSchema.Constants.Prefix));

			// Order 2 Pkg
			var packages2 = order2R.PackageJob.Packages;
			AssertEquals("Should be 1 added packages for order.", 1, packages2.Count);
			AssertEquals("Should be correct pack type.", Constants.PkgUnit.Package, packages2[0].KP_F3_NKPackType);
			AssertEquals("Package should be closed.", true, packages2[0].IsClosed);
			AssertEquals("Should be correct package ID.", string.Empty, packages2[0].KP_PackageID);

			var packageDivots2 = packages2[0].PackedItemDivots;
			AssertEquals("Should be 3 added package divot", 3, packageDivots2.Count);
			AssertContainsExactElementsInAnyOrder(
				"Should be correct divot KI_ParentIDs",
				new[] { orderLine4.PickLines[0].PK, orderLine5.PickLines[0].PK, orderLine6.PickLines[0].PK },
				packageDivots2.Select(d => d.KI_ParentID));
			AssertEquals("Should be correct KI_ParentTableCode for all divots", true, packageDivots2.All(d => d.KI_ParentTableCode == WhsPickLineSchema.Constants.Prefix));

			var order1Serials = order1R.Lines.Select(l => l.PickLines[0].InventoryLine.WE_SerialNumber.ToString()).ToArray();
			var order2Serials = order2R.Lines.Select(l => l.PickLines[0].InventoryLine.WE_SerialNumber.ToString()).ToArray();
			if (pkg1Serials.ToHashSet().Contains(orderLine1.PickLines[0].InventoryLine.WE_SerialNumber.ToString()))
			{
				AssertContainsExactElementsInAnyOrder(pkg1Serials, order1Serials);
				AssertContainsExactElementsInAnyOrder(pkg2Serials, order2Serials);
			}
			else
			{
				AssertContainsExactElementsInAnyOrder(pkg1Serials, order2Serials);
				AssertContainsExactElementsInAnyOrder(pkg2Serials, order1Serials);
			}
		}

		public void TestAutoCreateAttributeNeutralPackagesForPick_PkgsAcrossOrders_MultipleProducts()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);
			var partRelation1 = data.Part1.RelatedOrganisations[0];
			partRelation1.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation1.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, use: true);
			var partRelation2 = data.Part2.RelatedOrganisations[0];
			partRelation2.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation2.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, location);
			receiveLine1.WE_SerialNumber = "RFT";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, location);
			receiveLine2.WE_SerialNumber = "DER";
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, location);
			receiveLine3.WE_SerialNumber = "EWD";
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine4.WE_SerialNumber = "FRT";
			var receiveLine5 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine5.WE_SerialNumber = "GBE";
			var receiveLine6 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine6.WE_SerialNumber = "XSD";
			var receiveLine7 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, location);
			receiveLine7.WE_SerialNumber = "TYT";
			var receiveLine8 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine8.WE_SerialNumber = "POL";
			var receiveLine9 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine9.WE_SerialNumber = "DSF";
			var receiveLine10 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, location);
			receiveLine10.WE_SerialNumber = "RFY";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var orderLine3 = Helper.CreateWhsOrderLine(order1, data.Part2, 1m);
			var orderLine4 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var orderLine5 = Helper.CreateWhsOrderLine(order1, data.Part2, 1m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine6 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			var orderLine7 = Helper.CreateWhsOrderLine(order2, data.Part2, 1m);
			var orderLine8 = Helper.CreateWhsOrderLine(order2, data.Part2, 1m);
			var orderLine9 = Helper.CreateWhsOrderLine(order2, data.Part2, 1m);
			var orderLine10 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);

			var pick = Helper.CreatePickNew(order1, order2);
			pick.GetAllPickLines().ForEach(line => line.WZ_PickedDateTime = ZDateTimeOffset.Now);
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			AssertEquals("Precondition", true, pick.IsMultiOrderPick);

			var serialPickLineDictionary = new Dictionary<ZGuid, string>();
			serialPickLineDictionary[orderLine1.PickLines[0].PK] = orderLine1.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLineDictionary[orderLine2.PickLines[0].PK] = orderLine2.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLineDictionary[orderLine3.PickLines[0].PK] = orderLine3.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLineDictionary[orderLine4.PickLines[0].PK] = orderLine4.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLineDictionary[orderLine5.PickLines[0].PK] = orderLine5.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLineDictionary[orderLine6.PickLines[0].PK] = orderLine6.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLineDictionary[orderLine7.PickLines[0].PK] = orderLine7.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLineDictionary[orderLine8.PickLines[0].PK] = orderLine8.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLineDictionary[orderLine9.PickLines[0].PK] = orderLine9.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLineDictionary[orderLine10.PickLines[0].PK] = orderLine10.PickLines[0].InventoryLine.WE_SerialNumber.ToString();

			var pkg1Serials = new[] // p2
			{
					orderLine8.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
					orderLine7.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
					orderLine3.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
				};

			var pkg2Serials = new[] // p1
			{
					orderLine4.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
					orderLine6.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
				};

			var pkg3Serials = new[] // p2
			{
					orderLine5.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
					orderLine9.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
				};

			var pkg4Serials = new[] // p1
			{
					orderLine1.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
					orderLine2.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
					orderLine10.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
				};

			var packages =
				new AttributeNeutralPackageInfo[]
				{
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Bag,
							ProductPK = data.Part2.PK.ToGuid(),
							PalletID = string.Empty,
							ExpectedQuantityInPackage = 3,
							SerialNumbers = pkg1Serials,
						},
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Package,
							ProductPK = data.Part1.PK.ToGuid(),
							PalletID = string.Empty,
							ExpectedQuantityInPackage = 2,
							SerialNumbers = pkg2Serials,
						},
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Package,
							ProductPK = data.Part2.PK.ToGuid(),
							PalletID = string.Empty,
							ExpectedQuantityInPackage = 2,
							SerialNumbers = pkg3Serials,
						},
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Bag,
							ProductPK = data.Part1.PK.ToGuid(),
							PalletID = string.Empty,
							ExpectedQuantityInPackage = 3,
							SerialNumbers = pkg4Serials,
						},
				};

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(packages);
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			// Order 1 Pkg
			var packagesOnO1 = order1.PackageJob.Packages;
			AssertEquals("Should be 2 added packages for order.", 2, packagesOnO1.Count);
			AssertEquals("Should be correct package ID.", true, packagesOnO1.All(p => p.KP_PackageID == string.Empty));

			var packageDivots1 = packagesOnO1.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Bag && p.IsClosed).PackedItemDivots;
			AssertEquals("Should be 3 added package divot", 3, packageDivots1.Count);
			AssertEquals("Should be correct KI_PackedQty for all divots", true, packageDivots1.All(d => d.KI_PackedQty == 1m));
			AssertContainsExactElementsInAnyOrder(
				"Should be correct divot KI_ParentIDs",
				pkg4Serials,
				packageDivots1.Select(d => serialPickLineDictionary[d.KI_ParentID]));

			var packageDivots2 = packagesOnO1.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Package && p.IsClosed).PackedItemDivots;
			AssertEquals("Should be 2 added package divot", 2, packageDivots2.Count);
			AssertEquals("Should be correct KI_PackedQty for all divots", true, packageDivots2.All(d => d.KI_PackedQty == 1m));
			AssertContainsExactElementsInAnyOrder(
				"Should be correct divot KI_ParentIDs",
				pkg3Serials,
				packageDivots2.Select(d => serialPickLineDictionary[d.KI_ParentID]));

			// Order 2 Pkg
			var packagesOnO2 = order2.PackageJob.Packages;
			AssertEquals("Should be 2 added packages for order.", 2, packagesOnO2.Count);
			AssertEquals("Should be correct package ID.", true, packagesOnO2.All(p => p.KP_PackageID == string.Empty));

			var packageDivots3 = packagesOnO2.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Bag && p.IsClosed).PackedItemDivots;
			AssertEquals("Should be 3 added package divot", 3, packageDivots3.Count);
			AssertEquals("Should be correct KI_PackedQty for all divots", true, packageDivots3.All(d => d.KI_PackedQty == 1m));
			AssertContainsExactElementsInAnyOrder(
				"Should be correct divot KI_ParentIDs",
				pkg1Serials,
				packageDivots3.Select(d => serialPickLineDictionary[d.KI_ParentID]));

			var packageDivots4 = packagesOnO2.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Package && p.IsClosed).PackedItemDivots;
			AssertEquals("Should be 2 added package divot", 2, packageDivots4.Count);
			AssertEquals("Should be correct KI_PackedQty for all divots", true, packageDivots4.All(d => d.KI_PackedQty == 1m));
			AssertContainsExactElementsInAnyOrder(
				"Should be correct divot KI_ParentIDs",
				pkg2Serials,
				packageDivots4.Select(d => serialPickLineDictionary[d.KI_ParentID]));
		}

		public void TestAutoCreateAttributeNeutralPackagesForPick_PkgsAcrossOrders_MultipleLocations()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location1);
			receiveLine1.WE_SerialNumber = "SN1";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location1);
			receiveLine2.WE_SerialNumber = "SN2";
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location2);
			receiveLine3.WE_SerialNumber = "SN3";
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location2);
			receiveLine4.WE_SerialNumber = "SN4";
			var receiveLine5 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location1);
			receiveLine5.WE_SerialNumber = "SN5";
			var receiveLine6 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location1);
			receiveLine6.WE_SerialNumber = "SN6";
			var receiveLine7 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location2);
			receiveLine7.WE_SerialNumber = "SN7";
			var receiveLine8 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location2);
			receiveLine8.WE_SerialNumber = "SN8";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var orderLine3 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var orderLine4 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine5 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			var orderLine6 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			var orderLine7 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			var orderLine8 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);

			var pick = Helper.CreatePickNew(order1, order2);
			pick.GetAllPickLines().ForEach(line => line.WZ_PickedDateTime = ZDateTimeOffset.Now);
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			AssertEquals("Precondition", true, pick.IsMultiOrderPick);

			var serialPickLineDictionary = new Dictionary<ZGuid, string>();
			serialPickLineDictionary[orderLine1.PickLines[0].PK] = orderLine1.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLineDictionary[orderLine2.PickLines[0].PK] = orderLine2.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLineDictionary[orderLine3.PickLines[0].PK] = orderLine3.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLineDictionary[orderLine4.PickLines[0].PK] = orderLine4.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLineDictionary[orderLine5.PickLines[0].PK] = orderLine5.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLineDictionary[orderLine6.PickLines[0].PK] = orderLine6.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLineDictionary[orderLine7.PickLines[0].PK] = orderLine7.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLineDictionary[orderLine8.PickLines[0].PK] = orderLine8.PickLines[0].InventoryLine.WE_SerialNumber.ToString();

			var pkg1Serials = new[]
			{
					orderLine1.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
					orderLine2.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
				};

			var pkg2Serials = new[]
			{
					orderLine3.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
					orderLine4.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
				};

			var pkg3Serials = new[]
			{
					orderLine5.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
					orderLine6.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
				};

			var pkg4Serials = new[]
			{
					orderLine7.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
					orderLine8.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
				};

			var packages =
				new AttributeNeutralPackageInfo[]
				{
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Package,
							ProductPK = data.Part1.PK.ToGuid(),
							PalletID = string.Empty,
							ExpectedQuantityInPackage = 2,
							SerialNumbers = pkg1Serials,
						},
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Package,
							ProductPK = data.Part1.PK.ToGuid(),
							PalletID = string.Empty,
							ExpectedQuantityInPackage = 2,
							SerialNumbers = pkg2Serials,
						},
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Package,
							ProductPK = data.Part1.PK.ToGuid(),
							PalletID = string.Empty,
							ExpectedQuantityInPackage = 2,
							SerialNumbers = pkg3Serials,
						},
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Package,
							ProductPK = data.Part1.PK.ToGuid(),
							PalletID = string.Empty,
							ExpectedQuantityInPackage = 2,
							SerialNumbers = pkg4Serials,
						},
				};

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(packages);
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			// Order 1 Pkg
			var packages1 = order1.PackageJob.Packages;
			AssertEquals("Should be 1 added packages for order.", 2, packages1.Count);
			AssertEquals("Should be correct pack type.", true, packages1.All(p => p.KP_F3_NKPackType == Constants.PkgUnit.Package));
			AssertEquals("Packages should be closed.", true, packages1.All(p => p.IsClosed));
			AssertEquals("Should be correct package ID.", true, packages1.All(p => p.KP_PackageID == string.Empty));

			var packageDivots1 = packages1[0].PackedItemDivots;
			AssertEquals("Should be 2 added package divot", 2, packageDivots1.Count);
			AssertEquals("Should be correct KI_PackedQty for all divots", true, packageDivots1.All(d => d.KI_PackedQty == 1m));
			AssertContainsExactElementsInAnyOrder(
				"Should be correct divot KI_ParentIDs",
				pkg1Serials,
				packageDivots1.Select(d => serialPickLineDictionary[d.KI_ParentID]));

			var packageDivots2 = packages1[1].PackedItemDivots;
			AssertEquals("Should be 2 added package divot", 2, packageDivots2.Count);
			AssertEquals("Should be correct KI_PackedQty for all divots", true, packageDivots2.All(d => d.KI_PackedQty == 1m));
			AssertContainsExactElementsInAnyOrder(
				"Should be correct divot KI_ParentIDs",
				pkg2Serials,
				packageDivots2.Select(d => serialPickLineDictionary[d.KI_ParentID]));

			// Order 2 Pkg
			var packages2 = order2.PackageJob.Packages;
			AssertEquals("Should be 2 added packages for order.", 2, packages2.Count);
			AssertEquals("Should be correct pack type.", true, packages2.All(p => p.KP_F3_NKPackType == Constants.PkgUnit.Package));
			AssertEquals("Packages should be closed.", true, packages2.All(p => p.IsClosed));
			AssertEquals("Should be correct package ID.", true, packages2.All(p => p.KP_PackageID == string.Empty));

			var packageDivots3 = packages2[0].PackedItemDivots;
			AssertEquals("Should be 2 added package divot", 2, packageDivots3.Count);
			AssertEquals("Should be correct KI_PackedQty for all divots", true, packageDivots3.All(d => d.KI_PackedQty == 1m));
			AssertContainsExactElementsInAnyOrder(
				"Should be correct divot KI_ParentIDs",
				pkg3Serials,
				packageDivots3.Select(d => serialPickLineDictionary[d.KI_ParentID]));

			var packageDivots4 = packages2[1].PackedItemDivots;
			AssertEquals("Should be 2 added package divot", 2, packageDivots4.Count);
			AssertEquals("Should be correct KI_PackedQty for all divots", true, packageDivots4.All(d => d.KI_PackedQty == 1m));
			AssertContainsExactElementsInAnyOrder(
				"Should be correct divot KI_ParentIDs",
				pkg4Serials,
				packageDivots4.Select(d => serialPickLineDictionary[d.KI_ParentID]));
		}

		public void TestAutoCreateAttributeNeutralPackagesForPick_PkgsAcrossOrders_NoEnoughStock()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine1.WE_SerialNumber = "sn1";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine2.WE_SerialNumber = "sn2";
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine3.WE_SerialNumber = "sn3";
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine4.WE_SerialNumber = "sn4";
			var receiveLine5 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine5.WE_SerialNumber = "sn5";
			var receiveLine6 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine6.WE_SerialNumber = "sn6";
			var receiveLine7 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine7.WE_SerialNumber = "sn7";
			var receiveLine8 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine8.WE_SerialNumber = "SN8";
			var receiveLine9 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine9.WE_SerialNumber = "SN9";
			var receiveLine10 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine10.WE_SerialNumber = "SN10";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var orderLine3 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var orderLine4 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var orderLine5 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var orderLine6 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine7 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			var orderLine8 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			var orderLine9 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			var orderLine10 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);

			var pick = Helper.CreatePickNew(order1, order2);
			pick.GetAllPickLines().ForEach(line => line.WZ_PickedDateTime = ZDateTimeOffset.Now);
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			AssertEquals("Precondition", true, pick.IsMultiOrderPick);

			var serialPickLinePKs = new Dictionary<ZGuid, string>();
			serialPickLinePKs[orderLine1.PickLines[0].PK] = orderLine1.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLinePKs[orderLine2.PickLines[0].PK] = orderLine2.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLinePKs[orderLine3.PickLines[0].PK] = orderLine3.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLinePKs[orderLine4.PickLines[0].PK] = orderLine4.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLinePKs[orderLine5.PickLines[0].PK] = orderLine5.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLinePKs[orderLine6.PickLines[0].PK] = orderLine6.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLinePKs[orderLine7.PickLines[0].PK] = orderLine7.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLinePKs[orderLine8.PickLines[0].PK] = orderLine8.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLinePKs[orderLine9.PickLines[0].PK] = orderLine9.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLinePKs[orderLine10.PickLines[0].PK] = orderLine10.PickLines[0].InventoryLine.WE_SerialNumber.ToString();

			var expectedUnpackedPKs = new[] { orderLine2.PickLines[0].PK, orderLine3.PickLines[0].PK };

			var pkg1Serials = new[]
			{
					orderLine1.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
					orderLine4.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
					orderLine9.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
				};

			var pkg2Serials = new[]
			{
					orderLine2.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
					orderLine3.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
				};

			var pkg3Serials = new[]
			{
					orderLine5.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
					orderLine6.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
					orderLine8.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
					orderLine7.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
					orderLine10.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
				};

			var packages =
				new AttributeNeutralPackageInfo[]
				{
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Case,
							ProductPK = data.Part1.PK.ToGuid(),
							PalletID = string.Empty,
							ExpectedQuantityInPackage = 3,
							SerialNumbers = pkg1Serials,
						},
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Bag,
							ProductPK = data.Part1.PK.ToGuid(),
							PalletID = string.Empty,
							ExpectedQuantityInPackage = 2,
							SerialNumbers = pkg2Serials,
						},
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Box,
							ProductPK = data.Part1.PK.ToGuid(),
							PalletID = string.Empty,
							ExpectedQuantityInPackage = 5,
							SerialNumbers = pkg3Serials,
						},
				};

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(packages);
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			// Order 1 Pkg
			var packages1 = order1.PackageJob.Packages;
			AssertEquals("Should be 1 added packages for order.", 1, packages1.Count);
			AssertEquals("Should be correct pack type.", Constants.PkgUnit.Box, packages1[0].KP_F3_NKPackType);
			AssertEquals("Package should be closed.", true, packages1[0].IsClosed);

			var packageDivots1 = packages1[0].PackedItemDivots;
			AssertEquals("Should be 5 added package divot", 5, packageDivots1.Count);
			AssertContainsExactElementsInAnyOrder(
				"Should be correct divot KI_ParentIDs",
				pkg3Serials,
				packageDivots1.Select(d => serialPickLinePKs[d.KI_ParentID]));

			// Order 2 Pkg
			var packages2 = order2.PackageJob.Packages;
			AssertEquals("Should be 1 added packages for order.", 1, packages2.Count);
			AssertEquals("Should be correct pack type.", Constants.PkgUnit.Case, packages2[0].KP_F3_NKPackType);
			AssertEquals("Package should be closed.", true, packages2[0].IsClosed);

			var packageDivots2 = packages2[0].PackedItemDivots;
			AssertEquals("Should be 4 added package divot", 3, packageDivots2.Count);
			AssertContainsExactElementsInAnyOrder(
				"Should be correct divot KI_ParentIDs",
				pkg1Serials,
				packageDivots2.Select(d => serialPickLinePKs[d.KI_ParentID]));

			var otherPickLineDivots
				= webService.Factory.Load<PkgPackageItemDivot>(
						new ZQuery(
							PkgPackageItemDivotSchema.KI_ParentID,
							expectedUnpackedPKs));
			AssertEquals("Other picklines are unpacked", 0, otherPickLineDivots?.Length ?? 0);
		}

		public void TestAutoCreateAttributeNeutralPackagesForPick_PkgsAcrossOrders_TooMuchStock()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine1.WE_SerialNumber = "SN100";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine2.WE_SerialNumber = "SN2";
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine3.WE_SerialNumber = "SN3";
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine4.WE_SerialNumber = "SN4";
			var receiveLine5 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine5.WE_SerialNumber = "SN5";
			var receiveLine6 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine6.WE_SerialNumber = "SN6";
			var receiveLine7 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine7.WE_SerialNumber = "SN7";
			var receiveLine8 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location);
			receiveLine8.WE_SerialNumber = "SN8";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var orderLine3 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var orderLine4 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var orderLine5 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine6 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			var orderLine7 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			var orderLine8 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);

			var pick = Helper.CreatePickNew(order1, order2);
			pick.GetAllPickLines().ForEach(line => line.WZ_PickedDateTime = ZDateTimeOffset.Now);
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			AssertEquals("Precondition", true, pick.IsMultiOrderPick);

			var serialPickLineDictionary = new Dictionary<ZGuid, string>();
			serialPickLineDictionary[orderLine1.PickLines[0].PK] = orderLine1.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLineDictionary[orderLine2.PickLines[0].PK] = orderLine2.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLineDictionary[orderLine3.PickLines[0].PK] = orderLine3.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLineDictionary[orderLine4.PickLines[0].PK] = orderLine4.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLineDictionary[orderLine5.PickLines[0].PK] = orderLine5.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLineDictionary[orderLine6.PickLines[0].PK] = orderLine6.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLineDictionary[orderLine7.PickLines[0].PK] = orderLine7.PickLines[0].InventoryLine.WE_SerialNumber.ToString();
			serialPickLineDictionary[orderLine8.PickLines[0].PK] = orderLine8.PickLines[0].InventoryLine.WE_SerialNumber.ToString();

			var pkg1Serials = new[]
{
					orderLine1.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
					orderLine4.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
					orderLine7.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
					orderLine8.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
				};

			var pkg2Serials = new[]
			{
					orderLine2.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
					orderLine3.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
					orderLine6.PickLines[0].InventoryLine.WE_SerialNumber.ToString(),
				};

			var packages =
				new AttributeNeutralPackageInfo[]
				{
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Case,
							ProductPK = data.Part1.PK.ToGuid(),
							PalletID = string.Empty,
							ExpectedQuantityInPackage = 4,
							SerialNumbers = pkg1Serials,
						},
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Bag,
							ProductPK = data.Part1.PK.ToGuid(),
							PalletID = string.Empty,
							ExpectedQuantityInPackage = 3,
							SerialNumbers = pkg2Serials,
						},
				};

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(packages);
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			// Order 1 Pkg
			var packages1 = order1.PackageJob.Packages;
			AssertEquals("Should be 1 added packages for order.", 1, packages1.Count);
			AssertEquals("Should be correct pack type.", Constants.PkgUnit.Case, packages1[0].KP_F3_NKPackType);
			AssertEquals("Package should be closed.", true, packages1[0].IsClosed);

			var packageDivots1 = packages1[0].PackedItemDivots;
			AssertEquals("Should be 4 added package divot", 4, packageDivots1.Count);
			AssertEquals("Should be correct KI_PackedQty for all divots", true, packageDivots1.All(d => d.KI_PackedQty == 1m));
			AssertContainsExactElementsInAnyOrder(
				"Should be correct divot KI_ParentIDs",
				pkg1Serials,
				packageDivots1.Select(d => serialPickLineDictionary[d.KI_ParentID]));

			// Order 2 Pkg
			var packages2 = order2.PackageJob.Packages;
			AssertEquals("Should be 1 added packages for order.", 1, packages2.Count);
			AssertEquals("Should be correct pack type.", Constants.PkgUnit.Bag, packages2[0].KP_F3_NKPackType);
			AssertEquals("Package should be closed.", true, packages2[0].IsClosed);

			var packageDivots2 = packages2[0].PackedItemDivots;
			AssertEquals("Should be 3 added package divot", 3, packageDivots2.Count);
			AssertEquals("Should be correct KI_PackedQty for all divots", true, packageDivots2.All(d => d.KI_PackedQty == 1m));
			AssertContainsExactElementsInAnyOrder(
				"Should be correct divot KI_ParentIDs",
				pkg2Serials,
				packageDivots2.Select(d => serialPickLineDictionary[d.KI_ParentID]));
		}

		#endregion

		#region TestAutoCreateAttributeNeutralPackagesForPick_ProductPackageSizes

		public void TestAutoCreateAttributeNeutralPackagesForPick_ProductPackageSizes()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			var partUnit = Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Box, 1m);
			partUnit.OF_Depth = 2;
			partUnit.OF_Height = 4;
			partUnit.OF_Width = 6;
			partUnit.OF_Weight = 50;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var location = data.Whs1.FindLocation("A-1");
			var inv = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			inv.WI_SerialNumber = "RFT";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine.WE_SerialNumber = "RFT";

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines[0];
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(
				new AttributeNeutralPackageInfo[]
				{
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Box,
							ProductPK = data.Part1.PK.ToGuid(),
							ExpectedQuantityInPackage = 1,
							SerialNumbers = new[] { orderLine.WE_SerialNumber.ToString() },
						}
				}
			);
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			var packages = order.PackageJob.Packages;
			AssertEquals("Should be 1 added packages for order.", 1, packages.Count);
			AssertEquals("Should be correct pack type.", Constants.PkgUnit.Box, packages[0].KP_F3_NKPackType);
			AssertEquals("Package should be closed.", true, packages[0].IsClosed);
			AssertEquals("Should be correct KP_Length.", 2m, packages[0].KP_Length);
			AssertEquals("Should be correct KP_Height.", 4m, packages[0].KP_Height);
			AssertEquals("Should be correct KP_Width.", 6m, packages[0].KP_Width);
			AssertEquals("Should be correct KP_Weight.", 52m, packages[0].KP_Weight);

			var packageDivots = packages[0].PackedItemDivots;
			AssertEquals("Should be 1 added package divot", 1, packageDivots.Count);
			AssertEquals("Should be correct divot KI_ParentID", orderLine.PickLines[0].PK, packageDivots[0].KI_ParentID);
		}

		#endregion

		#region TestAutoCreateAttributeNeutralPackagesForPick_PartOfPallet

		public void TestAutoCreateAttributeNeutralPackagesForPick_PartOfPallet()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var location = data.Whs1.FindLocation("A-1");
			var inv1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			inv1.WI_SerialNumber = "RFT1";
			inv1.WI_PalletID = "RFTPalletIDHere";
			var inv2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			inv2.WI_SerialNumber = "RFT2";
			inv2.WI_PalletID = "RFTPalletIDHere";
			var inv3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			inv3.WI_SerialNumber = "RFT3";
			inv3.WI_PalletID = "RFTPalletIDHere";
			var inv4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			inv4.WI_SerialNumber = "RFT4";
			inv4.WI_PalletID = "RFTPalletIDHere";
			var inv5 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			inv5.WI_SerialNumber = "RFT5";
			inv5.WI_PalletID = "RFTPalletIDHere";
			var inv6 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			inv6.WI_SerialNumber = "RFT6";
			inv6.WI_PalletID = "RFTPalletIDHere";
			var inv7 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			inv7.WI_SerialNumber = "RFT7";
			inv7.WI_PalletID = "RFTPalletIDHere";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines[0];
			orderLine.PickLines.ForEach(p => p.WZ_PickedDateTime = ZDateTimeOffset.Now);
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var packagesToCreate =
				new AttributeNeutralPackageInfo[]
				{
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Case,
							ProductPK = data.Part1.PK.ToGuid(),
							PalletID = "RFTPalletIDHere",
							ExpectedQuantityInPackage = 5,
							SerialNumbers = new[] {  "RFT1", "RFT2", "RFT3", "RFT4", "RFT5" },
						}
				};

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(packagesToCreate);
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			var packages = order.PackageJob.Packages;
			AssertEquals("Should be 1 added packages for order.", 1, packages.Count);
			AssertEquals("Should be correct pack type.", Constants.PkgUnit.Case, packages[0].KP_F3_NKPackType);
			AssertEquals("Package should be closed.", true, packages[0].IsClosed);
			AssertEquals("Should be correct package ID.", "", packages[0].KP_PackageID);

			var packageDivots = packages[0].PackedItemDivots;
			AssertEquals("Should be 5 added package divots", 5, packageDivots.Count);
			AssertContainsExactElementsInAnyOrder(
					"Should be correct divot KI_ParentIDs",
					orderLine.PickLines.Select(p => p.PK),
					packageDivots.Select(d => d.KI_ParentID));
		}

		#endregion

		#region TestAutoCreateAttributeNeutralPackagesForPick_PickByBOM

		public void TestAutoCreateAttributeNeutralPackagesForPick_PickByBOM()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;

			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, wheel, AttributeNumber.Serial, use: true);
			var partRelation = wheel.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inv1 = Helper.CreateWhsReceiveInventoryLine(receive, wheel, 1m, location);
			inv1.WI_SerialNumber = "SER1";
			var inv2 = Helper.CreateWhsReceiveInventoryLine(receive, wheel, 1m, location);
			inv2.WI_SerialNumber = "SER2";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 1m);
			var pick = Helper.CreatePickNew(order);

			var wheelOrderLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			wheelOrderLine.PickLines.ForEach(l => l.WZ_PickedDateTime = ZDateTimeOffset.Now);
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var packagesToCreate =
				new AttributeNeutralPackageInfo[]
				{
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Case,
							ProductPK = wheel.PK.ToGuid(),
							PalletID = "",
							ExpectedQuantityInPackage = 2,
							SerialNumbers = new[] { "SER1", "SER2" },
						}
				};

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(packagesToCreate);
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			var packages = order.PackageJob.Packages;
			AssertEquals("Should be 0 added packages for order.", 0, packages.Count);
		}

		#endregion

		#region TestAutoCreateAttributeNeutralPackagesForPick_FullPallet_TwoPalletsOrdersLocations

		public void TestAutoCreateAttributeNeutralPackagesForPick_FullPallet_TwoPalletsOrdersLocations()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var inv1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location1);
			inv1.WI_SerialNumber = "RFT1";
			inv1.WI_PalletID = "PLT1";
			var inv2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location1);
			inv2.WI_SerialNumber = "RFT2";
			inv2.WI_PalletID = "PLT1";
			var inv3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location1);
			inv3.WI_SerialNumber = "RFT3";
			inv3.WI_PalletID = "PLT1";
			var inv4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location2);
			inv4.WI_SerialNumber = "RFT4";
			inv4.WI_PalletID = "PLT2";
			var inv5 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location2);
			inv5.WI_SerialNumber = "RFT5";
			inv5.WI_PalletID = "PLT2";
			var inv6 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location2);
			inv6.WI_SerialNumber = "RFT6";
			inv6.WI_PalletID = "PLT2";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 3m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 3m);

			var pick = Helper.CreatePickNew(order1, order2);
			orderLine1.PickLines.ForEach(p => p.WZ_PickedDateTime = ZDateTimeOffset.Now);
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			orderLine2.PickLines.ForEach(p => p.WZ_PickedDateTime = ZDateTimeOffset.Now);
			pick.RunPreSaveValidation();
			Helper.Factory.Save();

			var packagesToCreate =
				new AttributeNeutralPackageInfo[]
				{
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Case,
							ProductPK = data.Part1.PK.ToGuid(),
							PalletID = "PLT1",
							ExpectedQuantityInPackage = 3,
							SerialNumbers = new[] {  "RFT1", "RFT2", "RFT3" },
						},
						new AttributeNeutralPackageInfo()
						{
							PackType = Constants.PkgUnit.Case,
							ProductPK = data.Part1.PK.ToGuid(),
							PalletID = "PLT2",
							ExpectedQuantityInPackage = 3,
							SerialNumbers = new[] {  "RFT4", "RFT5", "RFT6" },
						}
				};

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AutoCreateAttributeNeutralPackagesForPick(packagesToCreate);
			AssertEquals("No error should be on response", ErrorTypes.None, response.Error);

			var packages1 = order1.PackageJob.Packages;
			AssertEquals("Should be 1 added packages for order.", 1, packages1.Count);
			AssertEquals("Should be correct pack type.", Constants.PkgUnit.Case, packages1[0].KP_F3_NKPackType);
			AssertEquals("Package should be closed.", true, packages1[0].IsClosed);
			AssertEquals("Should be correct package ID.", "PLT1", packages1[0].KP_PackageID);

			var packageDivots1 = packages1[0].PackedItemDivots;
			AssertEquals("Should be 3 added package divots", 3, packageDivots1.Count);
			AssertContainsExactElementsInAnyOrder(
					"Should be correct divot KI_ParentIDs",
					orderLine1.PickLines.Select(p => p.PK),
					packageDivots1.Select(d => d.KI_ParentID));
			AssertContainsExactElementsInAnyOrder(
					"Should be correct Serial Numbers",
					orderLine1.PickLines.Select(p => p.InventoryLine.WE_SerialNumber),
					new[] { "RFT1", "RFT2", "RFT3" });

			var packages2 = order2.PackageJob.Packages;
			AssertEquals("Should be 1 added packages for order.", 1, packages2.Count);
			AssertEquals("Should be correct pack type.", Constants.PkgUnit.Case, packages2[0].KP_F3_NKPackType);
			AssertEquals("Package should be closed.", true, packages2[0].IsClosed);
			AssertEquals("Should be correct package ID.", "PLT2", packages2[0].KP_PackageID);

			var packageDivots2 = packages2[0].PackedItemDivots;
			AssertEquals("Should be 3 added package divots", 3, packageDivots2.Count);
			AssertContainsExactElementsInAnyOrder(
					"Should be correct divot KI_ParentIDs",
					orderLine2.PickLines.Select(p => p.PK),
					packageDivots2.Select(d => d.KI_ParentID));
			AssertContainsExactElementsInAnyOrder(
					"Should be correct Serial Numbers",
					orderLine2.PickLines.Select(p => p.InventoryLine.WE_SerialNumber),
					new[] { "RFT4", "RFT5", "RFT6" });
		}

		#endregion
	}
}
