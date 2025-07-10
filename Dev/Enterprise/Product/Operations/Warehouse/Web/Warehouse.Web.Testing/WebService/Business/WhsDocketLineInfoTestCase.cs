using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsDocketLineInfo))]
	public class WhsDocketLineInfoTestCase : DataObjectInfoTestCase<WhsDocketLineInfo>
	{
		#region TestConstructor

		#region TestConstructor

		public void TestConstructor()
		{
			var docketLine = new WhsDocketLineInfo();
			AssertConstructor(docketLine);
		}

		protected virtual void AssertConstructor(WhsDocketLineInfo docketLine)
		{
			AssertNotNull(docketLine);
			AssertNotNull(docketLine.Product);
			AssertNotNull(docketLine.PartAttributes);
			AssertEquals("", docketLine.Attribute1);
			AssertEquals("", docketLine.Attribute2);
			AssertEquals("", docketLine.Attribute3);
			AssertEquals("", docketLine.SerialNumber);
			AssertEquals(new DateTime(), docketLine.ExpiryDate);
			AssertEquals(new DateTime(), docketLine.PackingDate);
			AssertEquals(0m, docketLine.Packs);
			AssertEquals("", docketLine.PackUQ);
			AssertEquals(0m, docketLine.Qty);
			AssertEquals("", docketLine.QtyUQ);
			AssertEquals("", docketLine.PalletID);
			AssertEquals("", docketLine.InventoryStatus);
			AssertEquals("", docketLine.InventoryHeldCode);
			AssertEquals(Guid.Empty, docketLine.PK);
			AssertEquals("", docketLine.ClientCode);
			AssertEquals((short)0, docketLine.ProductShelfLife);
			AssertEquals("", docketLine.LocationFormattedCheckDigit);
			AssertEquals("", docketLine.DestLocationFormattedCheckDigit);
		}

		#endregion

		#region TestConstructor_ClientPartBarcode

		public void TestConstructor_ClientPartBarcode()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var client1 = helper.CreateClient("CLIENT1");
			var part = helper.CreateProduct(client1, "PART");
			var docketPK = Guid.NewGuid();
			var whs = helper.CreateWarehouse("WHS1");

			SetupDataForClientPart(client1, part, "12345", "DAM");
			var docketLine = new WhsDocketLineInfo(client1, part, "12345", docketPK, whs);
			AssertClientPartBarcodeConstructor(docketLine, "LB1", "KG1", "DAM");

			SetupDataForClientPart(client1, part, "");
			docketLine = new WhsDocketLineInfo(client1, part, "", docketPK, whs);
			AssertClientPartBarcodeConstructor(docketLine, "LB1", "LB1", "DAM");
		}

		protected virtual void AssertClientPartBarcodeConstructor(WhsDocketLineInfo docketLine, string expectedQtyUQ, string expectedPackUQ, string inventoryHoldCode)
		{
			AssertNotNull(docketLine);
			AssertNotNull(docketLine.Product);
			AssertNotNull(docketLine.PartAttributes);
			AssertEquals("", docketLine.Attribute1);
			AssertEquals("", docketLine.Attribute2);
			AssertEquals("", docketLine.Attribute3);
			AssertEquals(new DateTime(), docketLine.ExpiryDate);
			AssertEquals(new DateTime(), docketLine.PackingDate);
			AssertEquals(0m, docketLine.Packs);
			AssertEquals(0m, docketLine.Qty);
			AssertEquals(expectedQtyUQ, docketLine.QtyUQ);

			AssertEquals("PART", docketLine.Product.Code);
			AssertEquals(expectedPackUQ, docketLine.PackUQ);

			AssertEquals(inventoryHoldCode, docketLine.InventoryHeldCode);

			AssertPartAttributes(docketLine);
		}

		public void AssertPartAttributes(WhsDocketLineInfo docketLine)
		{
			AssertEquals("Attr1 Name", docketLine.PartAttributes.Attribute1Caption);
			AssertEquals(true, docketLine.PartAttributes.Attribute1IsMandatory);
			AssertEquals(true, docketLine.PartAttributes.Attribute1IsUsed);
			AssertEquals("Attr2 Name", docketLine.PartAttributes.Attribute2Caption);
			AssertEquals(false, docketLine.PartAttributes.Attribute2IsMandatory);
			AssertEquals(true, docketLine.PartAttributes.Attribute2IsUsed);
			AssertEquals("Attr3 Name", docketLine.PartAttributes.Attribute3Caption);
			AssertEquals(false, docketLine.PartAttributes.Attribute3IsMandatory);
			AssertEquals(false, docketLine.PartAttributes.Attribute3IsUsed);
		}

		#endregion

		#region TestConstructor_UsesWarehouseCountryFormatString

		public void TestConstructor_UsesWarehouseCountryFormatString()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var client1 = helper.CreateClient("CLIENT1");
			var part1 = helper.CreateProduct(client1, "PART1");
			var part2 = helper.CreateProduct(client1, "PART2");
			var whs = helper.CreateWarehouse("WHS1");

			Helper.SetClientAllAttributeType(client1, true);
			Helper.SetProductAllAttributeUse(client1, part1, true);
			Helper.SetProductAllAttributeUse(client1, part2, true);

			var docketLine1 = new WhsDocketLineInfo(client1, part1, "12345", Guid.NewGuid(), whs);
			AssertEquals("ddMMyy", docketLine1.PartAttributes.ExpiryDateFormatString);
			AssertEquals("ddMMyy", docketLine1.PartAttributes.PackingDateFormatString);

			whs.WarehouseAddress.OA_RN_NKCountryCode = "CN";

			var docketLine2 = new WhsDocketLineInfo(client1, part2, "67890", Guid.NewGuid(), whs);
			AssertEquals("yyMMdd", docketLine2.PartAttributes.ExpiryDateFormatString);
			AssertEquals("yyMMdd", docketLine2.PartAttributes.PackingDateFormatString);
		}

		#endregion

		#region TestConstructor_ClientPartBarcode_MissingClient

		public void TestConstructor_ClientPartBarcode_MissingClient()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var client = helper.CreateClient("CLIENT");
			var part = helper.CreateProduct(client, "PART");
			var docketPK = Guid.NewGuid();
			var whs = helper.CreateWarehouse("WHS1");

			try
			{
				new WhsDocketLineInfo(null, part, "12345", docketPK, whs);
			}
			catch (ArgumentNullException ex)
			{
				AssertEquals("client", ex.ParamName);
			}
		}

		#endregion

		#region TestConstructor_ClientPartBarcode_MissingProduct

		public void TestConstructor_ClientPartBarcode_MissingProduct()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var client = helper.CreateClient("CLIENT");
			var docketPK = Guid.NewGuid();
			var whs = helper.CreateWarehouse("WHS1");

			try
			{
				new WhsDocketLineInfo(client, null, "12345", docketPK, whs);
			}
			catch (ArgumentNullException ex)
			{
				AssertEquals("part", ex.ParamName);
			}
		}

		#endregion

		#region TestConstructor_MissingDocketLine

		public void TestConstructor_MissingDocketLine()
		{
			try
			{
				new WhsDocketLineInfo(null);
			}
			catch (ArgumentNullException ex)
			{
				AssertEquals("docketLine", ex.ParamName);
			}
		}

		#endregion

		#region TestConstructor_WithWhsDocketLine

		public void TestConstructor_WithWhsDocketLine()
		{
			var client = Helper.CreateClient("CLIENT");
			var whs = Helper.CreateWarehouse("SYDNEY");
			var part = Helper.CreateProduct(client, "PART1");
			SetupDataForClientPart(client, part, "");
			var receive = Helper.CreateWhsReceive(client, whs);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, part, 10, new ZDate(2009, 01, 08), new ZDate(2010, 02, 04), "Attribute1", "Attribute2", "Attribute3", "");
			inventory.WI_SerialNumber = "SN1";

			Factory.Save();

			var receiveLine = inventory.InDocketLine;
			var docketLineInfo = new WhsDocketLineInfo(receiveLine);
			AssertDocketLineConstructor(docketLineInfo, receiveLine);
			AssertDocketLinePutawayTime(docketLineInfo, receiveLine, null);
		}

		protected virtual void SetupDataForClientPart(OrgHeader client, OrgSupplierPart part, string barcode, string inventoryHoldCode = "")
		{
			client.MiscServ.OM_IMPartAttrib1Name = "Attr1 Name";
			client.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			client.MiscServ.OM_IMPartAttrib2Name = "Attr2 Name";
			client.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.NonMandatory;
			client.MiscServ.OM_IMPartAttrib3Name = "Attr3 Name";
			client.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.NonMandatory;

			part.OP_StockKeepingUnit = "LB1";

			var relation = part.RelatedOrganisations.FindFirstByOrganisationPK(client.PK);
			relation.OU_UsePartAttrib1 = true;
			relation.OU_UsePartAttrib2 = true;
			relation.OU_UsePartAttrib3 = false;

			if (!string.IsNullOrEmpty(inventoryHoldCode))
			{
				var whsInventoryHoldCode = Factory.Load<WhsInventoryHeldCode>(new ZQuery(WhsInventoryHeldCodeSchema.WHC_Code, inventoryHoldCode)).Single();
				relation.OU_WHC_DefaultInventoryHoldCode = whsInventoryHoldCode.PK;
			}

			if (!string.IsNullOrEmpty(barcode))
			{
				part.PartBarcodes.AddNew();
				part.PartBarcodes[0].PH_Barcode = barcode;
				part.PartBarcodes[0].PH_F3_NKPackType = "KG1";
			}
			part.PartUnits.AddNew();
			part.PartUnits[0].OF_PackType = "LB1";
			part.PartUnits[0].OF_ParentPackType = "KG1";
			part.PartUnits[0].OF_QuantityInParent = 10;
		}

		#endregion

		#region TestConstructor_WithWhsTransferLine

		[TestDate(2015, 02, 06)]
		public void TestConstructor_WithWhsTransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-1", "", InventoryHoldCodes.Codes.Damaged);
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			var today = ZDateTimeOffset.Today;
			var staff = Helper.CreateGlbStaff("A.A", "AAA");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "A-2", InventoryHoldCodes.Codes.Damaged);
			transferLine.WE_GS_NKPutawayBy = staff.GS_Code;
			transferLine.WE_TransferFromPalletId = "PLT-1";
			transferLine.WE_PalletID = "PLT-2";
			transferLine.GS_NKPickedBy = staff.GS_Code;
			transferLine.PickedTime = today.AddDays(-5);
			AssertEquals("Precondition: Transfer Line is Picked.", true, transferLine.PickedTime.IsValid);

			Factory.Save();

			var transferLineInfo = new WhsDocketLineInfo(transferLine);
			AssertDocketLineConstructor(transferLineInfo, transferLine);
			AssertDocketLinePutawayTime(transferLineInfo, transferLine, null);
		}

		[TestDate(2015, 02, 06)]
		public void TestConstructor_WithFinalisedWhsTransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var timeZone = Factory.New<RefTimeZone>();
			timeZone.R2_CivilianTimeZoneCode = "DDT";
			timeZone.R2_OffsetMinutesFromUTC = 120;

			var timeZoneSet = Factory.New<RefTimeZoneSet>();
			timeZoneSet.R3_R2_StandardZone = timeZone.PK;
			timeZoneSet.R3_R2_DaylightSavingZone = Factory.NewWithValidTestData<DaylightSavingTimeZone>().PK;
			timeZoneSet.R3_TimeZoneSetName = "DD";

			var port = Factory.New<RefUNLOCO>();
			port.RL_Code = "DDVVV";
			port.RL_R3 = timeZoneSet.PK;
			port.RL_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			data.Whs1.RelatedCompanyBranch.GB_RL_NKHomePort = "DDVVV";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-1", "", InventoryHoldCodes.Codes.Damaged);
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			var today = ZDateTimeOffset.Today;
			var staff = Helper.CreateGlbStaff("A.A", "AAA");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "A-2", InventoryHoldCodes.Codes.Damaged);
			transferLine.WE_GS_NKPutawayBy = staff.GS_Code;
			transferLine.WE_TransferFromPalletId = "PLT-1";
			transferLine.WE_PalletID = "PLT-2";
			transferLine.GS_NKPickedBy = staff.GS_Code;
			transferLine.PickedTime = today.AddDays(-5);
			AssertEquals("Precondition: Transfer Line is Picked.", true, transferLine.PickedTime.IsValid);

			Factory.Save();

			transfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertEquals("Precondition: Transfer Line is Putaway.", true, transferLine.WE_PutawayTime.IsValid);

			var transferLineInfo = new WhsDocketLineInfo(transferLine);
			AssertDocketLineConstructor(transferLineInfo, transferLine);
			AssertDocketLinePutawayTime(transferLineInfo, transferLine, timeZoneSet);
		}

		public void TestConstructor_WithWhsTransferLine_PalletIDNeutral()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var palletIdNeutralType = Helper.CreateLocationType("PLT", "Neutral", isPalletIDNeutral: true, 0, "NOR");
			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_WLT_LocationType = palletIdNeutralType.PK;

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLineWithNoPallet = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "A-2");
			var transferLineWithPallet = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "PLT-1", "A-2", "");
			var transferLineWithPalletInNonNeutralLocation = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-2", "PLT-2", "A-1", "");
			AssertEquals("No Pallet Id, not considered PalletID Neutral.", false, new WhsDocketLineInfo(transferLineWithNoPallet).PalletIDNeutral);
			AssertEquals("Non Neutral Location, not considered PalletID Neutral.", false, new WhsDocketLineInfo(transferLineWithPalletInNonNeutralLocation).PalletIDNeutral);
			AssertEquals("Pallet ID in Neutral Location, is considered PalletID Neutral.", true, new WhsDocketLineInfo(transferLineWithPallet).PalletIDNeutral);
		}

		#endregion

		#region TestConstructor_ProductShelfLife

		public void TestConstructor_ProductShelfLife()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var client = helper.CreateClient("CLIENT1");
			var docketPK = Guid.NewGuid();
			var whs = helper.CreateWarehouse("WHS1");

			client.MiscServ.OM_IMUseExpiryDate = true;
			client.MiscServ.OM_IMUsePackingDate = true;

			var part = helper.CreateProduct(client, "Part");
			var relation = helper.CreateProductClientRelationShip(client, part);
			relation.OU_UseExpiryDate = true;
			relation.OU_UsePackingDate = true;

			var productParam = helper.CreateProductParamsByWhsAndClient(part, client, whs);
			productParam.W3_MaximumShelfLife = 10;
			helper.Factory.Save();

			var docketLine = new WhsDocketLineInfo(client, part, "12345", docketPK, whs);
			AssertEquals("Returned product shelf life is correct.", (short)10, docketLine.ProductShelfLife);
		}

		public void TestConstructor_ProductShelfLife_NoProductParam()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var client = helper.CreateClient("CLIENT1");
			var docketPK = Guid.NewGuid();
			var whs = helper.CreateWarehouse("WHS1");

			client.MiscServ.OM_IMUseExpiryDate = true;
			client.MiscServ.OM_IMUsePackingDate = true;

			var part = helper.CreateProduct(client, "Part");
			var relation = helper.CreateProductClientRelationShip(client, part);
			relation.OU_UseExpiryDate = true;
			relation.OU_UsePackingDate = true;
			helper.Factory.Save();

			var docketLine = new WhsDocketLineInfo(client, part, "12345", docketPK, whs);
			AssertEquals("No product shelf life is returned.", (short)0, docketLine.ProductShelfLife);
		}

		public void TestConstructor_ProductShelfLife_ProductNotUsingExpiryDate()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var client = helper.CreateClient("CLIENT1");
			var docketPK = Guid.NewGuid();
			var whs = helper.CreateWarehouse("WHS1");

			client.MiscServ.OM_IMUseExpiryDate = true;
			client.MiscServ.OM_IMUsePackingDate = true;

			var part = helper.CreateProduct(client, "Part");
			var relation = helper.CreateProductClientRelationShip(client, part);
			relation.OU_UsePackingDate = true;

			var productParam = helper.CreateProductParamsByWhsAndClient(part, client, whs);
			productParam.W3_MaximumShelfLife = 10;
			helper.Factory.Save();

			var docketLine = new WhsDocketLineInfo(client, part, "12345", docketPK, whs);
			AssertEquals("No product shelf life is returned.", (short)0, docketLine.ProductShelfLife);
		}

		public void TestConstructor_ProductShelfLife_ProductNotUsingPackingDate()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var client = helper.CreateClient("CLIENT1");
			var docketPK = Guid.NewGuid();
			var whs = helper.CreateWarehouse("WHS1");

			client.MiscServ.OM_IMUseExpiryDate = true;
			client.MiscServ.OM_IMUsePackingDate = true;

			var part = helper.CreateProduct(client, "Part");
			var relation = helper.CreateProductClientRelationShip(client, part);
			relation.OU_UseExpiryDate = true;

			var productParam = helper.CreateProductParamsByWhsAndClient(part, client, whs);
			productParam.W3_MaximumShelfLife = 10;
			helper.Factory.Save();

			var docketLine = new WhsDocketLineInfo(client, part, "12345", docketPK, whs);
			AssertEquals("No product shelf life is returned.", (short)0, docketLine.ProductShelfLife);
		}

		public void TestConstructor_ProductShelfLife_MissingWarehouse()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var client = helper.CreateClient("CLIENT");
			var part = helper.CreateProduct(client, "PART");
			var docketPK = Guid.NewGuid();

			try
			{
				new WhsDocketLineInfo(client, part, "12345", docketPK, null);
			}
			catch (ArgumentNullException ex)
			{
				AssertEquals("warehouse", ex.ParamName);
			}
		}

		#endregion

		#region TestConstructor_LocationFormattedCheckDigit

		public void TestConstructor_LocationFormattedCheckDigit_ReceiveLine()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 100, 1);
			var location = data.Whs1.DefaultLocation;
			location.FormattedCheckDigit = "11";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10, location);
			Factory.Save();

			var docketLineInfo = new WhsDocketLineInfo(receiveLine);
			AssertEquals("", docketLineInfo.LocationFormattedCheckDigit);
			AssertEquals("11", docketLineInfo.DestLocationFormattedCheckDigit);
		}

		public void TestConstructor_LocationFormattedCheckDigit_TransferLine()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 100, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			location1.FormattedCheckDigit = "11";
			location2.FormattedCheckDigit = "22";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1);
			receive.FinaliseDocketWithoutUserConfirmation();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location1, location2);
			transfer.RunPreSaveValidation();
			Factory.Save();

			var docketLineInfo = new WhsDocketLineInfo(transferLine);
			AssertEquals("11", docketLineInfo.LocationFormattedCheckDigit);
			AssertEquals("22", docketLineInfo.DestLocationFormattedCheckDigit);
		}

		public void TestConstructor_LocationFormattedCheckDigit_AdjustmentLine()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 100, 1);
			var location = data.Whs1.DefaultLocation;
			location.FormattedCheckDigit = "11";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location);
			receive.FinaliseDocketWithoutUserConfirmation();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 1m, location);
			Factory.Save();

			var docketLineInfo = new WhsDocketLineInfo(adjustmentLine);
			AssertEquals("", docketLineInfo.LocationFormattedCheckDigit);
			AssertEquals("11", docketLineInfo.DestLocationFormattedCheckDigit);
		}

		public void TestConstructor_LocationFormattedCheckDigit_OrderLine()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 100, 1);
			var location = data.Whs1.DefaultLocation;
			location.FormattedCheckDigit = "11";

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var oderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10);
			Factory.Save();

			var docketLineInfo = new WhsDocketLineInfo(oderLine);
			AssertEquals("", docketLineInfo.LocationFormattedCheckDigit);
			AssertEquals("", docketLineInfo.DestLocationFormattedCheckDigit);
		}

		public void TestConstructor_LocationFormattedCheckDigit_WorkOrderLine()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 100, 1);
			var location = data.Whs1.DefaultLocation;
			location.FormattedCheckDigit = "11";

			var order = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var oderLine = Helper.CreateWhsWorkOrderLine(order, data.Part1, 10);
			Factory.Save();

			var docketLineInfo = new WhsDocketLineInfo(oderLine);
			AssertEquals("", docketLineInfo.LocationFormattedCheckDigit);
			AssertEquals("", docketLineInfo.DestLocationFormattedCheckDigit);
		}

		#endregion

		#region Asserts

		void AssertDocketLinePutawayTime(WhsDocketLineInfo docketLineInfo, WhsDocketLine docketLine, RefTimeZoneSet timeZoneSet)
		{
			if (timeZoneSet != null)
			{
				var utcPutawayTime = docketLine.WE_PutawayTime.ToUtcDateTime();
				var localisedTime = timeZoneSet.GetCalculationTimeZone().ToLocalTime(utcPutawayTime);
				AssertEquals(docketLineInfo.PutawayByTime, localisedTime);
			}
			else
			{
				AssertEquals(docketLineInfo.PutawayByTime, DateTime.MinValue);
			}
		}

		void AssertDocketLineConstructor(WhsDocketLineInfo docketLineInfo, WhsDocketLine docketLine)
		{
			var docket = docketLine.Docket;
			AssertNotNull(docketLineInfo);
			AssertNotNull(docketLineInfo.Product);
			AssertEquals(docketLineInfo.Product.Code, docketLine.SupplierPart.OP_PartNum);
			AssertNotNull(docketLineInfo.PartAttributes);
			AssertPartAttributes(docketLineInfo, docketLine);
			AssertEquals(docketLineInfo.Attribute1, docketLine.WE_PartAttrib1);
			AssertEquals(docketLineInfo.Attribute2, docketLine.WE_PartAttrib2);
			AssertEquals(docketLineInfo.Attribute3, docketLine.WE_PartAttrib3);
			AssertEquals(docketLineInfo.SerialNumber, docketLine.WE_SerialNumber);
			AssertEquals(docketLineInfo.ExpiryDate, docketLine.WE_ExpiryDate);
			AssertEquals(docketLineInfo.PackingDate, docketLine.WE_PackingDate);
			AssertEquals(docketLineInfo.Packs, docketLine.WE_PackQuantity);
			AssertEquals(docketLineInfo.PackUQ, docketLine.WE_F3_NKPackType);
			AssertEquals(docketLineInfo.Qty, docketLine.WE_TransactionQuantity);
			AssertEquals(docketLineInfo.QtyUQ, docketLine.ProductUQ);
			AssertEquals(docketLineInfo.PalletID, docketLine.WE_TransferFromPalletId);
			AssertEquals(docketLineInfo.PK, docketLine.PK.ToGuid());
			AssertEquals(docketLineInfo.DocketPK, docket.PK.ToGuid());
			AssertEquals(docketLineInfo.ClientCode, docket.Client.OH_Code);
			AssertEquals(docketLineInfo.InventoryStatus, docketLine.WE_OriginalInventoryStatus);
			AssertEquals(docketLineInfo.InventoryHeldCode, docketLine.WE_WHC_NKOriginalInventoryHeldCode);

			var transferLine = docketLine as WhsTransferLine;
			if (transferLine != null)
			{
				AssertEquals(docketLineInfo.Location, transferLine.TransferFromLocationString);

				if (transferLine.PickedBy != null)
				{
					AssertEquals(docketLineInfo.PickedBy, transferLine.PickedBy.GS_LoginName);
				}

				AssertEquals(transferLine.PickedTime.ToDateTime(), docketLineInfo.PickedByTime);
			}

			AssertEquals(docketLineInfo.DestPalletID, docketLine.WE_PalletID);
			AssertEquals(docketLineInfo.DestLocation, docketLine.LocationString);
			var putawayBy = docketLine.PutawayBy;
			if (putawayBy != null)
			{
				AssertEquals(docketLineInfo.PutawayBy, putawayBy.GS_LoginName);
			}
		}

		void AssertPartAttributes(WhsDocketLineInfo docketLineInfo, WhsDocketLine docketLine)
		{
			var client = docketLine.Docket.Client;
			var relation = docketLine.SupplierPart.RelatedOrganisations.FindFirstByOrganisationPK(client.PK);

			AssertEquals(client.MiscServ.OM_IMPartAttrib1Name, docketLineInfo.PartAttributes.Attribute1Caption);
			AssertEquals(client.PartAttributeManager.IsPartAttributeMandatory(1), docketLineInfo.PartAttributes.Attribute1IsMandatory);
			AssertEquals(relation.OU_UsePartAttrib1, docketLineInfo.PartAttributes.Attribute1IsUsed);
			AssertEquals(client.MiscServ.OM_IMPartAttrib2Name, docketLineInfo.PartAttributes.Attribute2Caption);
			AssertEquals(client.PartAttributeManager.IsPartAttributeMandatory(2), docketLineInfo.PartAttributes.Attribute2IsMandatory);
			AssertEquals(relation.OU_UsePartAttrib2, docketLineInfo.PartAttributes.Attribute2IsUsed);
			AssertEquals(client.MiscServ.OM_IMPartAttrib3Name, docketLineInfo.PartAttributes.Attribute3Caption);
			AssertEquals(client.PartAttributeManager.IsPartAttributeMandatory(3), docketLineInfo.PartAttributes.Attribute3IsMandatory);
			AssertEquals(relation.OU_UsePartAttrib3, docketLineInfo.PartAttributes.Attribute3IsUsed);
		}

		#endregion

		#endregion

		#region Related Property Infos

		public void TestProduct()
		{
			AssertNotNull(Parent.Product);

			WhsProductInfo product = new WhsProductInfo();
			AssertNotEquals(product, Parent.Product);
			Parent.Product = product;
			AssertEquals(product, Parent.Product);
		}

		public void TestPartAttributes()
		{
			AssertNotNull(Parent.PartAttributes);

			WhsProductPartAttributesInfo partAttributes = new WhsProductPartAttributesInfo();
			AssertNotEquals(partAttributes, Parent.PartAttributes);
			Parent.PartAttributes = partAttributes;
			AssertEquals(partAttributes, Parent.PartAttributes);
		}

		#endregion

		#region Properties

		public void TestPK()
		{
			AssertEquals(Guid.Empty, Parent.PK);

			var newPK = Guid.NewGuid();
			Parent.PK = newPK;
			AssertEquals(newPK, Parent.PK);
		}

		public void TestPalletID()
		{
			AssertEquals("", Parent.PalletID);

			Parent.PalletID = "1234";
			AssertEquals("1234", Parent.PalletID);

			Parent.PalletID = "4321";
			AssertEquals("4321", Parent.PalletID);
		}

		public void TestPacks()
		{
			AssertEquals(0m, Parent.Packs);

			Parent.Packs = 10m;
			AssertEquals(10m, Parent.Packs);

			Parent.Packs = 20.20m;
			AssertEquals(20.20m, Parent.Packs);
		}

		public void TestPackUQ()
		{
			AssertEquals("", Parent.PackUQ);

			Parent.PackUQ = "1234";
			AssertEquals("1234", Parent.PackUQ);

			Parent.PackUQ = "4321";
			AssertEquals("4321", Parent.PackUQ);
		}

		public void TestQty()
		{
			AssertEquals(0m, Parent.Qty);

			Parent.Qty = 10m;
			AssertEquals(10m, Parent.Qty);

			Parent.Qty = 20.20m;
			AssertEquals(20.20m, Parent.Qty);
		}

		public void TestQtyUQ()
		{
			AssertEquals("", Parent.QtyUQ);

			Parent.QtyUQ = "1234";
			AssertEquals("1234", Parent.QtyUQ);

			Parent.QtyUQ = "4321";
			AssertEquals("4321", Parent.QtyUQ);
		}

		public void TestAttribute1()
		{
			AssertEquals("", Parent.Attribute1);

			Parent.Attribute1 = "1234";
			AssertEquals("1234", Parent.Attribute1);

			Parent.Attribute1 = "4321";
			AssertEquals("4321", Parent.Attribute1);
		}

		public void TestAttribute2()
		{
			AssertEquals("", Parent.Attribute2);

			Parent.Attribute2 = "1234";
			AssertEquals("1234", Parent.Attribute2);

			Parent.Attribute2 = "4321";
			AssertEquals("4321", Parent.Attribute2);
		}

		public void TestAttribute3()
		{
			AssertEquals("", Parent.Attribute3);

			Parent.Attribute3 = "1234";
			AssertEquals("1234", Parent.Attribute3);

			Parent.Attribute3 = "4321";
			AssertEquals("4321", Parent.Attribute3);
		}

		public void TestSerialNumber()
		{
			AssertEquals("", Parent.SerialNumber);

			Parent.SerialNumber = "1234";
			AssertEquals("1234", Parent.SerialNumber);

			Parent.SerialNumber = "4321";
			AssertEquals("4321", Parent.SerialNumber);
		}

		public void TestStatus()
		{
			AssertEquals("", Parent.InventoryStatus);

			Parent.InventoryStatus = "1234";
			AssertEquals("1234", Parent.InventoryStatus);

			Parent.InventoryStatus = "4321";
			AssertEquals("4321", Parent.InventoryStatus);
		}

		public void TestHeldCode()
		{
			AssertEquals("", Parent.InventoryHeldCode);

			Parent.InventoryHeldCode = "HEL";
			AssertEquals("HEL", Parent.InventoryHeldCode);

			Parent.InventoryHeldCode = "DAM";
			AssertEquals("DAM", Parent.InventoryHeldCode);
		}

		public void TestExpiryDate()
		{
			AssertEquals(new DateTime(), Parent.ExpiryDate);

			Parent.ExpiryDate = new DateTime(2008, 03, 12);
			AssertEquals(new DateTime(2008, 03, 12), Parent.ExpiryDate);

			Parent.ExpiryDate = new DateTime(2008, 03, 13);
			AssertEquals(new DateTime(2008, 03, 13), Parent.ExpiryDate);
		}

		public void TestPackingDate()
		{
			AssertEquals(new DateTime(), Parent.PackingDate);

			Parent.PackingDate = new DateTime(2008, 03, 12);
			AssertEquals(new DateTime(2008, 03, 12), Parent.PackingDate);

			Parent.PackingDate = new DateTime(2008, 03, 13);
			AssertEquals(new DateTime(2008, 03, 13), Parent.PackingDate);
		}

		public void TestDockDoorLocationAndPK()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-2-1");
			AssertNullOrEmpty(Parent.DockDoorLocation);
			AssertEquals(Guid.Empty, Parent.DockDoorLocationPK);

			Parent.DockDoorLocation = "A-1-1";
			Parent.DockDoorLocationPK = location1.PK.ToGuid();
			AssertEquals("A-1-1", Parent.DockDoorLocation);

			Parent.DockDoorLocation = "A-2-1";
			Parent.DockDoorLocationPK = location2.PK.ToGuid();
			AssertEquals("A-2-1", Parent.DockDoorLocation);
		}

		public void TestProductShelfLife()
		{
			AssertEquals((short)0, Parent.ProductShelfLife);

			Parent.ProductShelfLife = 10;
			AssertEquals((short)10, Parent.ProductShelfLife);

			Parent.ProductShelfLife = 20;
			AssertEquals((short)20, Parent.ProductShelfLife);
		}

		public void TestLocationFormattedCheckDigit()
		{
			AssertEquals("", Parent.LocationFormattedCheckDigit);

			Parent.LocationFormattedCheckDigit = "11";
			AssertEquals("11", Parent.LocationFormattedCheckDigit);

			Parent.LocationFormattedCheckDigit = "22";
			AssertEquals("22", Parent.LocationFormattedCheckDigit);
		}

		public void TestDestLocationFormattedCheckDigit()
		{
			AssertEquals("", Parent.DestLocationFormattedCheckDigit);

			Parent.DestLocationFormattedCheckDigit = "11";
			AssertEquals("11", Parent.DestLocationFormattedCheckDigit);

			Parent.DestLocationFormattedCheckDigit = "22";
			AssertEquals("22", Parent.DestLocationFormattedCheckDigit);
		}

		public void TestIfReceiveLineAttributes_AreNotPopulated_WhenExpiryAndPackingDateAreNotUsed()
		{
			var data = new TestDataSimpleEnvironment(Factory, 100, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10, data.Whs1.DefaultLocation);

			Factory.Save();

			var receiveLineInfo = new WhsDocketLineInfo(receiveLine);

			AssertEquals(0, receiveLineInfo.PartAttributes.ExpiryDateMaximumPastYears);
			AssertEquals(0, receiveLineInfo.PartAttributes.ExpiryDateMaximumFutureYears);

			AssertEquals(0, receiveLineInfo.PartAttributes.PackingDateMaximumPastYears);
			AssertEquals(0, receiveLineInfo.PartAttributes.PackingDateMaximumFutureYears);
		}

		public void TestIfReceiveLineAttributes_ArePopulated_WhenExpiryAndPackingDateAreUsed()
		{
			var data = new TestDataSimpleEnvironment(Factory, 100, 1);

			data.Org1.MiscServ.OM_IMUseExpiryDate = true;
			data.Org1.MiscServ.OM_IMUsePackingDate = true;

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, use: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, use: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10, data.Whs1.DefaultLocation);

			Factory.Save();

			var receiveLineInfo = new WhsDocketLineInfo(receiveLine);

			var expectedExpiryDateLimits = receiveLine.GetExpectedExpiryDateValidationRange();
			AssertEquals(expectedExpiryDateLimits.PastYearsBeforeError, receiveLineInfo.PartAttributes.ExpiryDateMaximumPastYears);
			AssertEquals(expectedExpiryDateLimits.FutureYearsBeforeError, receiveLineInfo.PartAttributes.ExpiryDateMaximumFutureYears);

			var expectedPackingDateLimits = receiveLine.GetExpectedPackingDateValidationRange();
			AssertEquals(expectedPackingDateLimits.PastYearsBeforeError, receiveLineInfo.PartAttributes.PackingDateMaximumPastYears);
			AssertEquals(expectedPackingDateLimits.FutureYearsBeforeError, receiveLineInfo.PartAttributes.PackingDateMaximumFutureYears);
		}

		public void TestIfTransferLineAttributes_AreNotPopulated_WhenExpiryAndPackingDateAreNotUsed()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-1", "", InventoryHoldCodes.Codes.Damaged);
			receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			var today = ZDateTimeOffset.Today;
			var staff = Helper.CreateGlbStaff("A.A", "AAA");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "A-2", InventoryHoldCodes.Codes.Damaged);

			transferLine.WE_GS_NKPutawayBy = staff.GS_Code;
			transferLine.WE_TransferFromPalletId = "PLT-1";
			transferLine.WE_PalletID = "PLT-2";
			transferLine.GS_NKPickedBy = staff.GS_Code;
			transferLine.PickedTime = today.AddDays(-5);

			Factory.Save();

			var transferLineInfo = new WhsDocketLineInfo(transferLine);

			AssertEquals(0, transferLineInfo.PartAttributes.ExpiryDateMaximumPastYears);
			AssertEquals(0, transferLineInfo.PartAttributes.ExpiryDateMaximumFutureYears);

			AssertEquals(0, transferLineInfo.PartAttributes.PackingDateMaximumPastYears);
			AssertEquals(0, transferLineInfo.PartAttributes.PackingDateMaximumFutureYears);
		}

		public void TestIfTransferLineAttributes_ArePopulated_WhenExpiryAndPackingDateAreUsed()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			data.Org1.MiscServ.OM_IMUseExpiryDate = true;
			data.Org1.MiscServ.OM_IMUsePackingDate = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.FindLocation("A-1"), "PLT-1", "", InventoryHoldCodes.Codes.Damaged);
			receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, use: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, use: true);

			var today = ZDateTimeOffset.Today;
			var staff = Helper.CreateGlbStaff("A.A", "AAA");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "A-2", InventoryHoldCodes.Codes.Damaged);

			transferLine.WE_GS_NKPutawayBy = staff.GS_Code;
			transferLine.WE_TransferFromPalletId = "PLT-1";
			transferLine.WE_PalletID = "PLT-2";
			transferLine.GS_NKPickedBy = staff.GS_Code;
			transferLine.PickedTime = today.AddDays(-5);

			Factory.Save();

			var transferLineInfo = new WhsDocketLineInfo(transferLine);

			var expectedExpiryDateLimits = transferLine.GetExpectedExpiryDateValidationRange();
			AssertEquals(expectedExpiryDateLimits.PastYearsBeforeError, transferLineInfo.PartAttributes.ExpiryDateMaximumPastYears);
			AssertEquals(expectedExpiryDateLimits.FutureYearsBeforeError, transferLineInfo.PartAttributes.ExpiryDateMaximumFutureYears);

			var expectedPackingDateLimits = transferLine.GetExpectedPackingDateValidationRange();
			AssertEquals(expectedPackingDateLimits.PastYearsBeforeError, transferLineInfo.PartAttributes.PackingDateMaximumPastYears);
			AssertEquals(expectedPackingDateLimits.FutureYearsBeforeError, transferLineInfo.PartAttributes.PackingDateMaximumFutureYears);
		}

		public void TestIfReceiveLineDateRanges_AreConsistent_WhenModifyingDefaultTypeValidationLimits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			data.Org1.MiscServ.OM_IMUseExpiryDate = true;
			data.Org1.MiscServ.OM_IMUsePackingDate = true;

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, use: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, use: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "PID_2226", 25m);

			Factory.Save();

			var receiveLine = inventory.InDocketLine;

			var receiveLineInfo1 = new WhsDocketLineInfo(receiveLine);
			var partAttributes1 = receiveLineInfo1.PartAttributes;

			var receiveLineInfo2 = new WhsDocketLineInfo(receiveLine);
			var partAttributes2 = receiveLineInfo2.PartAttributes;

			AssertEquals(partAttributes1.ExpiryDateMaximumPastYears, partAttributes2.ExpiryDateMaximumPastYears);
			AssertEquals(partAttributes1.ExpiryDateMaximumFutureYears, partAttributes2.ExpiryDateMaximumFutureYears);
		}

		#endregion

			#region Implementation

		protected new WhsDocketLineInfo Parent
		{
			get { return (WhsDocketLineInfo)base.Parent; }
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new WhsDocketLineInfo();
		}

		#endregion
	}
}
