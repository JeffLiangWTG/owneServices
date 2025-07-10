using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsDocketInfo))]
	public class WhsDocketInfoTestCase : DataObjectInfoTestCase<WhsDocketInfo>
	{
		#region Constructors

		public void TestAdditionalConstructors_Receive()
		{
			TestAdditionalConstructors_ReceiveCore(hasTaskManagement: false);
		}

		public void TestAdditionalConstructors_Receive_TaskManagement()
		{
			TestAdditionalConstructors_ReceiveCore(hasTaskManagement: true);
		}

		void TestAdditionalConstructors_ReceiveCore(bool hasTaskManagement)
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var client = helper.CreateClient("CLIENT");
			var part = helper.CreateProduct(client, "Part1");
			var warehouse = helper.CreateWarehouse("WHS");
			var staff1 = helper.CreateGlbStaff("S3", "S3");
			var staff2 = helper.CreateGlbStaff("S2", "S2");
			factory.Save();

			if (hasTaskManagement)
			{
				var releaseGroup = helper.CreateReleaseGroup("RG1", "RG1");
				warehouse.WW_GG_ReleaseGroup = releaseGroup.PK;
			}

			var receive = helper.CreateWhsReceive(client, warehouse);
			var inventory1 = helper.CreateWhsReceiveInventoryLine(receive, part, 10);
			var inventory2 = helper.CreateWhsReceiveInventoryLine(receive, part, 5);
			var supplier = helper.CreateClient("Supplier123");
			var transport = helper.CreateClient("Transport123");
			receive.SupplierDocAddress.OrganisationPK = supplier.PK;
			receive.TransportCoDocAddress.OrganisationPK = transport.PK;

			var unloadTask = helper.CreateProcessTaskForReceive(receive, staff1);
			helper.CreateProcessTaskForReceive(receive, staff2);
			factory.Save();

			receive.Client.MiscServ.OM_IMPartAttrib1Name = "Test1";
			receive.Client.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			receive.Client.MiscServ.OM_IMPartAttrib2Name = "Test2";
			receive.Client.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.NonMandatory;
			receive.Client.MiscServ.OM_IMPartAttrib3Name = "";
			receive.WD_DocketID = "DocketID1";
			receive.WD_ExternalReference = "ExternalReference1";

			var receiveInfo = new WhsDocketInfo(receive, staff: staff1);
			AssertNotNull(receiveInfo);
			AssertEquals(receive.PK.ToGuid(), receiveInfo.PK);
			AssertEquals("DocketID1", receiveInfo.DocketID);
			AssertEquals("ExternalReference1", receiveInfo.ExternalReference);
			AssertEquals(new ZByte(0), receiveInfo.ExternalReferenceSplit);
			AssertEquals("CLIENT", receiveInfo.ClientCode);
			AssertEquals("Supplier123", receiveInfo.SupplierCode);
			AssertEquals("Transport123", receiveInfo.TransportCompanyCode);
			AssertNotNull(receiveInfo.PartAttributes);
			AssertEquals("Test1", receiveInfo.PartAttributes.Attribute1Caption);
			AssertEquals(true, receiveInfo.PartAttributes.Attribute1IsMandatory);
			AssertEquals("Test2", receiveInfo.PartAttributes.Attribute2Caption);
			AssertEquals(false, receiveInfo.PartAttributes.Attribute2IsMandatory);
			AssertEquals("", receiveInfo.PartAttributes.Attribute3Caption);
			AssertEquals(false, receiveInfo.PartAttributes.Attribute3IsMandatory);
			AssertNotNull(receiveInfo.Lines);
			AssertEquals(2, receiveInfo.Lines.Count);

			AssertEquals((byte)0, receiveInfo.PickPriority);
			AssertEquals(null, receiveInfo.AssignedPacker);
			AssertEquals(DateTime.MinValue, receiveInfo.RequiredDate);
			AssertEquals(DateTime.MinValue, receiveInfo.CreateTime);
			AssertEquals(null, receiveInfo.RecentlyUsedDockDoorLocation);
			AssertEquals(null, receiveInfo.RecentlyUsedDockDoorLocation_UserFriendly);
			AssertEquals(Guid.Empty, receiveInfo.RecentlyUsedDockDoorLocationPK);
			AssertEquals(nameof(receiveInfo.TaskPK), hasTaskManagement ? unloadTask.PK : Guid.Empty, receiveInfo.TaskPK);

			receive.WD_OH_Client = ZGuid.Empty;
			receive.SupplierDocAddress.OrganisationPK = ZGuid.Empty;
			receive.TransportCoDocAddress.OrganisationPK = ZGuid.Empty;
			receive.WD_DocketID = "DocketID2";
			receive.WD_ExternalReference = "ExternalReference2";
			receive.Lines.DeleteAll();
			receive.WD_ExternalReferenceSplit = 1;

			receiveInfo = new WhsDocketInfo(receive);
			AssertNotNull(receiveInfo);
			AssertEquals(receive.PK.ToGuid(), receiveInfo.PK);
			AssertEquals("DocketID2", receiveInfo.DocketID);
			AssertEquals("ExternalReference2", receiveInfo.ExternalReference);
			AssertEquals(ReceiveType.Codes.Receipt, receiveInfo.DocketSubType);
			AssertEquals(new ZByte(1), receiveInfo.ExternalReferenceSplit);
			AssertEquals("", receiveInfo.ClientCode);
			AssertEquals("", receiveInfo.SupplierCode);
			AssertEquals("", receiveInfo.TransportCompanyCode);
			AssertNotNull(receiveInfo.PartAttributes);
			AssertEquals("", receiveInfo.PartAttributes.Attribute1Caption);
			AssertEquals(false, receiveInfo.PartAttributes.Attribute1IsMandatory);
			AssertEquals("", receiveInfo.PartAttributes.Attribute2Caption);
			AssertEquals(false, receiveInfo.PartAttributes.Attribute2IsMandatory);
			AssertEquals("", receiveInfo.PartAttributes.Attribute3Caption);
			AssertEquals(false, receiveInfo.PartAttributes.Attribute3IsMandatory);
			AssertNotNull(receiveInfo.Lines);
			AssertEquals(0, receiveInfo.Lines.Count);

			AssertEquals((byte)0, receiveInfo.PickPriority);
			AssertEquals(null, receiveInfo.AssignedPacker);
			AssertEquals(DateTime.MinValue, receiveInfo.RequiredDate);
			AssertEquals(DateTime.MinValue, receiveInfo.CreateTime);
		}

		public void TestAdditionalConstructors_WhsOrder()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var client = helper.CreateClient("CLIENT");
			var part = helper.CreateProduct(client, "Part1");
			var warehouse = helper.CreateWarehouse("WHS");

			var receive = helper.CreateWhsReceive(client, warehouse);
			helper.CreateWhsReceiveInventoryLine(receive, part, 10);
			factory.Save();

			var order = helper.CreateWhsOrder(client, warehouse);
			order.WD_UseDirectedPackingConsolidation = true;
			helper.CreateWhsOrderLine(order, part, 10m);
			var pick = helper.CreatePickNew(order);
			pick.WP_WL_DockDoor = warehouse.DefaultOutboundDockDoorLocation.PK;

			order.WD_GS_NKAssignedPacker = "ABC";
			order.WD_PickPriority = (ZByte)2;
			order.WD_RequiredDate = DateTime.Now;
			factory.Save();

			var orderInfo = new WhsDocketInfo(order);
			AssertEquals((byte)2, orderInfo.PickPriority);
			AssertEquals("ABC", orderInfo.AssignedPacker);
			AssertEquals(order.WD_RequiredDate.ToDateTime(), orderInfo.RequiredDate);
			AssertEquals(order.WD_SystemCreateTimeUtc.ToDateTime(), orderInfo.CreateTime);
		}

		public void TestAdditionalConstructors_WhsOrder_NoPick()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var client = helper.CreateClient("CLIENT");
			var part = helper.CreateProduct(client, "Part1");
			var warehouse = helper.CreateWarehouse("WHS");

			var receive = helper.CreateWhsReceive(client, warehouse);
			helper.CreateWhsReceiveInventoryLine(receive, part, 10);
			factory.Save();

			var order = helper.CreateWhsOrder(client, warehouse);
			helper.CreateWhsOrderLine(order, part, 10m);
			factory.Save();

			var orderInfo = new WhsDocketInfo(order);
			AssertEquals(null, orderInfo.RecentlyUsedDockDoorLocation);
			AssertEquals(Guid.Empty, orderInfo.RecentlyUsedDockDoorLocationPK);
			AssertEquals(null, orderInfo.RecentlyUsedDockDoorLocation_UserFriendly);
		}

		public void TestAdditionalConstructor_DocketHoldCodes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine1.WE_CurrentInventoryStatus = "HEL";
			receiveLine1.WE_WHC_NKCurrentInventoryHeldCode = "DAM";

			Helper.CreateWhsReceiveLine(receive, data.Part1, 8m);

			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 5m);
			receiveLine3.WE_CurrentInventoryStatus = "AVL";

			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part2, 2m);
			receiveLine4.WE_CurrentInventoryStatus = "HEL";
			receiveLine4.WE_WHC_NKCurrentInventoryHeldCode = "LCC";

			var whsDocketInfo = new WhsDocketInfo(receive);
			AssertNotNull(whsDocketInfo);
			var docketHoldCodes = whsDocketInfo.DocketHoldCodes;
			AssertEquals("Collection has 2 records", 2, docketHoldCodes.Count);

			AssertEquals("1st record ProductCode correct", data.Part1.OP_PartNum, docketHoldCodes[0].ProductCode);
			AssertEquals("1st record HoldCodes correct", "DAM,", string.Join(",", docketHoldCodes[0].HoldCodes));

			AssertEquals("2nd record ProductCode correct", data.Part2.OP_PartNum, docketHoldCodes[1].ProductCode);
			AssertEquals("2nd record HoldCodes correct", ",LCC", string.Join(",", docketHoldCodes[1].HoldCodes));
		}

		#endregion

		#region Properties

		#region TestPK

		public void TestPK()
		{
			AssertEquals(Guid.Empty, Parent.PK);

			var pk1 = Guid.NewGuid();
			Parent.PK = pk1;
			AssertEquals(pk1, Parent.PK);
		}

		#endregion

		#region TestClientCode

		public void TestClientCode()
		{
			AssertEquals("", Parent.ClientCode);

			Parent.ClientCode = "1234";
			AssertEquals("1234", Parent.ClientCode);

			Parent.ClientCode = "4321";
			AssertEquals("4321", Parent.ClientCode);
		}

		#endregion

		#region TestPalletsToTransferCompletely

		public void TestPalletsToTransferCompletely()
		{
			AssertEquals("Pallet id collection should be intialized but should not have any elements",
				0, Parent.PalletsToTransferCompletely.Count);

			Parent.PalletsToTransferCompletely.AddRange(new string[] { "ABCD", "EFGH" });
			AssertContainsExactElementsInAnyOrder(new string[] { "ABCD", "EFGH" }, Parent.PalletsToTransferCompletely);

			Parent.PalletsToTransferCompletely = new List<string>(new string[] { "1234", "5678" });
			AssertContainsExactElementsInAnyOrder(new string[] { "1234", "5678" }, Parent.PalletsToTransferCompletely);

			Parent.PalletsToTransferCompletely.Add("KLMN");
			AssertContainsExactElementsInAnyOrder(new string[] { "1234", "5678", "KLMN" }, Parent.PalletsToTransferCompletely);
		}

		#endregion

		#region TestDocketID

		public void TestDocketID()
		{
			AssertEquals("", Parent.DocketID);

			Parent.DocketID = "1234";
			AssertEquals("1234", Parent.DocketID);

			Parent.DocketID = "4321";
			AssertEquals("4321", Parent.DocketID);
		}

		#endregion

		#region TestExternalReference

		public void TestExternalReference()
		{
			AssertEquals("", Parent.ExternalReference);

			Parent.ExternalReference = "1234";
			AssertEquals("1234", Parent.ExternalReference);

			Parent.ExternalReference = "4321";
			AssertEquals("4321", Parent.ExternalReference);
		}

		#endregion

		#region TestDocketSubType

		public void TestDocketSubType()
		{
			AssertEquals("", Parent.DocketSubType);

			Parent.DocketSubType = ReceiveType.Codes.Receipt;
			AssertEquals(ReceiveType.Codes.Receipt, Parent.DocketSubType);

			Parent.DocketSubType = ReceiveType.Codes.Customs;
			AssertEquals(ReceiveType.Codes.Customs, Parent.DocketSubType);
		}

		#endregion

		#region TestExternalReferenceSplit

		public void TestExternalReferenceSplit()
		{
			var docketInfo = new WhsDocketInfo();
			AssertEquals(ZByte.Zero, docketInfo.ExternalReferenceSplit);

			docketInfo.ExternalReferenceSplit = 2;
			AssertEquals(new ZByte(2), docketInfo.ExternalReferenceSplit);

			docketInfo.ExternalReferenceSplit = 4;
			AssertEquals(new ZByte(4), docketInfo.ExternalReferenceSplit);
		}

		#endregion

		#region TestSupplierCode

		public void TestSupplierCode()
		{
			AssertEquals("", Parent.SupplierCode);

			Parent.SupplierCode = "1234";
			AssertEquals("1234", Parent.SupplierCode);

			Parent.SupplierCode = "4321";
			AssertEquals("4321", Parent.SupplierCode);
		}

		#endregion

		#region TestTransportCompanyCode

		public void TestTransportCompanyCode()
		{
			AssertEquals("", Parent.TransportCompanyCode);

			Parent.TransportCompanyCode = "1234";
			AssertEquals("1234", Parent.TransportCompanyCode);

			Parent.TransportCompanyCode = "4321";
			AssertEquals("4321", Parent.TransportCompanyCode);
		}

		#endregion

		#region TestCanFullPalletUnload

		public void TestCanFullPalletUnload()
		{
			AssertEquals(ASNState.NoPallets, Parent.ASNUnloadState);

			Parent.ASNUnloadState = ASNState.HasASNPalletsToUnload;
			AssertEquals(ASNState.HasASNPalletsToUnload, Parent.ASNUnloadState);

			Parent.ASNUnloadState = ASNState.AllASNPalletsUnloaded;
			AssertEquals(ASNState.AllASNPalletsUnloaded, Parent.ASNUnloadState);
		}

		#endregion

		#region TestIsEmptyAsnPalletMatchingEnabled

		public void TestIsEmptyAsnPalletMatchingEnabled()
		{
			AssertEquals(false, Parent.IsEmptyAsnPalletMatchingEnabledForUnload);

			Parent.IsEmptyAsnPalletMatchingEnabledForUnload = true;
			AssertEquals(true, Parent.IsEmptyAsnPalletMatchingEnabledForUnload);
		}

		#endregion

		#region TestPartAttributes

		public void TestPartAttributes()
		{
			AssertNotNull(Parent.PartAttributes);
			AssertEquals("", Parent.PartAttributes.Attribute1Caption);
			AssertEquals(false, Parent.PartAttributes.Attribute1IsMandatory);
			AssertEquals("", Parent.PartAttributes.Attribute2Caption);
			AssertEquals(false, Parent.PartAttributes.Attribute2IsMandatory);
			AssertEquals("", Parent.PartAttributes.Attribute3Caption);
			AssertEquals(false, Parent.PartAttributes.Attribute3IsMandatory);

			Parent.PartAttributes.Attribute1Caption = "Test1";
			Parent.PartAttributes.Attribute1IsMandatory = true;
			Parent.PartAttributes.Attribute2Caption = "Test2";
			Parent.PartAttributes.Attribute2IsMandatory = true;
			Parent.PartAttributes.Attribute3Caption = "Test3";
			Parent.PartAttributes.Attribute3IsMandatory = true;

			AssertEquals("Test1", Parent.PartAttributes.Attribute1Caption);
			AssertEquals(true, Parent.PartAttributes.Attribute1IsMandatory);
			AssertEquals("Test2", Parent.PartAttributes.Attribute2Caption);
			AssertEquals(true, Parent.PartAttributes.Attribute2IsMandatory);
			AssertEquals("Test3", Parent.PartAttributes.Attribute3Caption);
			AssertEquals(true, Parent.PartAttributes.Attribute3IsMandatory);
		}

		#endregion

		#region TestLines

		public void TestLines()
		{
			AssertNotNull(Parent.Lines);
			AssertEquals(0, Parent.Lines.Count);
			WhsDocketLineInfoCollection lines = new WhsDocketLineInfoCollection();
			Parent.Lines = lines;
			AssertEquals(lines, Parent.Lines);
		}

		#endregion

		#region TestEnforcePalletIDs

		public void TestEnforcePalletIDs_Enabled_HasParamForWarehouse()
		{
			TestEnforcePalletIDs_Core(true, true);
		}

		public void TestEnforcePalletIDs_Enabled_HasNoParamForWarehouse()
		{
			TestEnforcePalletIDs_Core(true, false);
		}

		public void TestEnforcePalletIDs_Disabled_HasParamForWarehouse()
		{
			TestEnforcePalletIDs_Core(false, true);
		}

		public void TestEnforcePalletIDs_Disabled_HasNoParamForWarehouse()
		{
			TestEnforcePalletIDs_Core(false, false);
		}

		public void TestEnforcePalletIDs_Core(bool enforcePalletIDs, bool hasParamForWarehouse)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var whsClientParams = Factory.New<WhsClientParameterByWarehouse>();
			whsClientParams.WY_OH_Client = data.Org1.PK;
			whsClientParams.WY_WW_Whs = ZGuid.Empty;

			if (hasParamForWarehouse)
			{
				whsClientParams.WY_WW_Whs = data.Whs1.PK;
			}
			whsClientParams.WY_EnforcePalletIDEntry = enforcePalletIDs;

			var testDataInfo = new WhsDocketInfo(receive);

			AssertEquals(enforcePalletIDs, testDataInfo.EnforcePalletIDs);
		}

		#endregion

		#region TestProductsWhichMayFulfillAsnLinesWithStockUnit

		public void TestProductsWhichMayFulfillAsnLinesWithStockUnit()
		{
			AssertNotNull(Parent.ProductsWhichMayFulfillAsnLinesWithStockUnit);
			AssertEquals(0, Parent.ProductsWhichMayFulfillAsnLinesWithStockUnit.Length);

			var pk = Guid.NewGuid();
			Parent.ProductsWhichMayFulfillAsnLinesWithStockUnit = new[] { pk };
			AssertContainsExactElementsInAnyOrder(new[] { pk }, Parent.ProductsWhichMayFulfillAsnLinesWithStockUnit);
		}

		#endregion

		#region TestDocketHoldCodes

		public void TestDocketHoldCodes()
		{
			AssertNotNull(Parent.DocketHoldCodes);
			AssertEquals(0, Parent.DocketHoldCodes.Count);

			var docketHoldCodes = new WhsDocketProductHoldCodeInfoCollection();
			Parent.DocketHoldCodes = docketHoldCodes;
			AssertEquals(docketHoldCodes, Parent.DocketHoldCodes);
		}

		#endregion

		#region TestValidatePalletIDAsSSCCOnUnload

		public void TestValidatePalletIDAsSSCCOnUnload_Enabled_HasParamForWarehouse()
		{
			TestEnforcePalletIDs_Core(true, true);
		}

		public void TestValidatePalletIDAsSSCCOnUnload_Enabled_HasNoParamForWarehouse()
		{
			TestEnforcePalletIDs_Core(true, false);
		}

		public void TestValidatePalletIDAsSSCCOnUnload_Disabled_HasParamForWarehouse()
		{
			TestEnforcePalletIDs_Core(false, true);
		}

		public void TestValidatePalletIDAsSSCCOnUnload_Disabled_HasNoParamForWarehouse()
		{
			TestEnforcePalletIDs_Core(false, false);
		}

		public void TestValidatePalletIDAsSSCCOnUnloadCore(bool validatePalletIDAsSSCCOnUnload, bool hasParamForWarehouse)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var whsClientParams = Factory.New<WhsClientParameterByWarehouse>();
			whsClientParams.WY_OH_Client = data.Org1.PK;
			whsClientParams.WY_WW_Whs = ZGuid.Empty;

			if (hasParamForWarehouse)
			{
				whsClientParams.WY_WW_Whs = data.Whs1.PK;
			}
			whsClientParams.WY_ValidatePalletIDAsSSCCOnUnload = validatePalletIDAsSSCCOnUnload;

			var testDataInfo = new WhsDocketInfo(receive);

			AssertEquals(validatePalletIDAsSSCCOnUnload, testDataInfo.ValidatePalletID);
		}

		#endregion

		#region TestGS1Prefix

		public void TestGS1Prefix_WhsReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			data.Org1.MiscServ.OM_WhsGenerateSSCCOnInbound = false;
			data.Org1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GS1, "1234567");

			var testDataInfo = new WhsDocketInfo(receive);
			AssertNullOrEmpty(testDataInfo.GS1Prefix);

			data.Org1.MiscServ.OM_WhsGenerateSSCCOnInbound = true;

			var testDataInfo2 = new WhsDocketInfo(receive);
			AssertEquals("1234567", testDataInfo2.GS1Prefix);
		}

		#endregion

		#region TestDockDoorLocation

		public void TestDockDoorLocation()
		{
			AssertNull(Parent.RecentlyUsedDockDoorLocation);
			AssertNull(Parent.RecentlyUsedDockDoorLocation_UserFriendly);
			AssertEquals(Guid.Empty, Parent.RecentlyUsedDockDoorLocationPK);

			var dockDoorLocationPK = Guid.NewGuid();
			Parent.RecentlyUsedDockDoorLocation = "A-1";
			Parent.RecentlyUsedDockDoorLocation_UserFriendly = "A-2";
			Parent.RecentlyUsedDockDoorLocationPK = dockDoorLocationPK;
			AssertEquals("A-1", Parent.RecentlyUsedDockDoorLocation);
			AssertEquals("A-2", Parent.RecentlyUsedDockDoorLocation_UserFriendly);
			AssertEquals(dockDoorLocationPK, Parent.RecentlyUsedDockDoorLocationPK);
		}

		#endregion

		#endregion

		#region Implementation

		protected new WhsDocketInfo Parent
		{
			get
			{
				return (WhsDocketInfo)base.Parent;
			}
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new WhsDocketInfo();
		}

		#endregion
	}
}
