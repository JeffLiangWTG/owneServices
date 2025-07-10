using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;

namespace Enterprise.Customs.Business.Testing
{
	sealed class DeclarationInventorySelectionHeaderTest : WhsDataTestHelper
	{
		public void TestImportInventories_ComponentInventory()
		{
			var declaration = Factory.New<BaseJobDeclarationForTesting>();

			using (CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var inventorySelectionHeader = new DeclarationInventorySelectionHeader(declaration);
				var whsWarehouse = GetNewWhsWarehouse(Warehouse.MainAddress.PK, true, "N10");
				var whsReceive = GetNewWhsReceive(whsWarehouse.PK, Importer.PK);
				var whsReceiveLine1 = GetNewWhsReceiveLine(whsReceive.PK, Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT3243-1", ZDateTime.Today.AddMonths(-1));
				whsReceiveLine1.Inventory.WI_AllocationKey = "D04C-3892-4353-A9FE";
				var whsReceiveLine1CustomsData = GetNewWhsBondedWarehouseAttribute(whsReceiveLine1.PK, 1000m, 50m, "KG", "AU", 100m, "NO", "", "EN00123", (ZShort)1);
				var whsInventory = whsReceiveLine1.Inventory;

				var newSelectedLine = new WhsInventoryWrapper(whsInventory, inventorySelectionHeader);
				newSelectedLine.QuantityToDraw = 22.22m;
				inventorySelectionHeader.SelectedLines.Add(newSelectedLine);
				Factory.Save();

				inventorySelectionHeader.ImportInventories();
				CombineAssertions(() =>
				{
					var importedGroupHeaders = declaration.JobComInvoiceGroupHeaders;
					AssertEquals(1, importedGroupHeaders.Count);
					var importedHeaders = importedGroupHeaders[0].JobComInvoiceHeaders;
					AssertEquals(1, importedHeaders.Count);
					var importedLines = importedHeaders[0].JobComInvoiceLines;
					AssertEquals(1, importedLines.Count);
					var inventories = importedLines[0].ComponentInventoryCollection;
					AssertEquals(1, inventories.Count);
					var inventory = inventories[0];
					AssertEquals("JIV_AllocationKey", "D04C-3892-4353-A9FE", inventory.JIV_AllocationKey);
					AssertEquals("JIV_QuantityToDraw", 22.22m, inventory.JIV_QuantityToDraw);
					AssertEquals("JIV_ClusterKey", declaration.JE_ClusterKey, inventory.JIV_ClusterKey);
				});
			}
		}

		public void TestGetSortKey_NoExceptionWhenMissingWI_ArrivalDate()
		{
			var whsWarehouse = GetNewWhsWarehouse(Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = GetNewWhsReceive(whsWarehouse.PK, Importer.PK);
			var whsReceiveLine1 = GetNewWhsReceiveLine(whsReceive.PK, Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, "ENT3243-1");
			var whsReceiveLine1CustomsData = GetNewWhsBondedWarehouseAttribute(whsReceiveLine1.PK, 1000m, 50m, "KG", "AU", 100m, "NO", "", "EN00123", (ZShort)1);
			var whsInventory = whsReceiveLine1.Inventory;
			Factory.Save();

			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Setup(m => m.IsInvoiceQuantityRequiredForBondedWarehouse).Returns(true);
			declarationMock.Setup(m => m.IsBondedWhsQuantityRequiredForBondedWarehouse).Returns(false);
			var declaration = declarationMock.Object;
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_BondedWhsQuantity = 50m;
			invoiceLine1.JI_PreviousEntryNumber = "ENT3243";
			invoiceLine1.JI_PreviousEntryLineNumber = 1;
			invoiceLine1.JI_OP = Part.PK;
			invoiceLine1.JI_PartNo = Part.OP_PartNum;

			var inventorySelectionHeader = new DeclarationInventorySelectionHeader(declaration);
			var invoiceLines = new List<BaseJobComInvoiceLine> { invoiceLine1 };

			AssertNoExceptionThrown(() =>
			{
				inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			});
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_InwardEntryNumberAndInwardEntryLineNumberExistsAsAPair()
		{
			var whsWarehouse = GetNewWhsWarehouse(Warehouse.MainAddress.PK, true, "N10");
			Factory.Save();

			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Setup(m => m.IsInvoiceQuantityRequiredForBondedWarehouse).Returns(true);
			declarationMock.Setup(m => m.IsBondedWhsQuantityRequiredForBondedWarehouse).Returns(false);
			var declaration = declarationMock.Object;
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_PreviousEntryNumber = "123";
			invoiceLine1.JI_PreviousEntryLineNumber = 0;

			var inventorySelectionHeader = new DeclarationInventorySelectionHeader(declaration);
			var invoiceLines = new List<BaseJobComInvoiceLine> { invoiceLine1 };
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "Previous Entry Line Number must be supplied if the Previous Entry Number has been specified"));

			invoiceLine1.JI_PreviousEntryNumber = "";
			invoiceLine1.JI_PreviousEntryLineNumber = 1;

			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "Previous Entry Number must be supplied if the Previous Entry Line Number has been specified"));
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_BondedEntryKeyAndProductCodeCombination()
		{
			var part3 = CreateProduct(Owner.PK, "~~3");
			part3.OP_Desc = "~~3 DESC";

			var whsWarehouse = GetNewWhsWarehouse(Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = GetNewWhsReceive(whsWarehouse.PK, Importer.PK);
			var whsReceiveLine1 = GetNewWhsReceiveLine(whsReceive.PK, Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT3243-1", ZDateTime.Today.AddMonths(-1));
			var whsReceiveLine1CustomsData = GetNewWhsBondedWarehouseAttribute(whsReceiveLine1.PK, 1000m, 50m, "KG", "AU", 100m, "NO", "", "EN00123", (ZShort)1);
			var whsInventory = whsReceiveLine1.Inventory;
			Factory.Save();

			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Setup(m => m.IsInvoiceQuantityRequiredForBondedWarehouse).Returns(true);
			declarationMock.Setup(m => m.IsBondedWhsQuantityRequiredForBondedWarehouse).Returns(false);
			var declaration = declarationMock.Object;
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_OP = part3.PK;
			invoiceLine1.JI_PartNo = part3.OP_PartNum;
			invoiceLine1.JI_BondedWhsQuantity = 50m;
			invoiceLine1.JI_PreviousEntryNumber = "ENT3333";
			invoiceLine1.JI_PreviousEntryLineNumber = 3;

			var inventorySelectionHeader = new DeclarationInventorySelectionHeader(declaration);
			var invoiceLines = new List<BaseJobComInvoiceLine> { invoiceLine1 };
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "Previous Entry Number, Previous Entry Line Number and Product Code combination cannot be matched in the Warehouse Inventory"));

			invoiceLine1.JI_PreviousEntryNumber = "ENT3243";
			invoiceLine1.JI_PreviousEntryLineNumber = 1;
			invoiceLine1.JI_OP = Part.PK;
			invoiceLine1.JI_PartNo = Part.OP_PartNum;
			invoiceLine1.ClearRowNotifications();

			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(!invoiceLine1.HasRowMessageErrors);
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_BondedEntryKey()
		{
			var whsWarehouse = GetNewWhsWarehouse(Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = GetNewWhsReceive(whsWarehouse.PK, Importer.PK);
			var whsReceiveLine1 = GetNewWhsReceiveLine(whsReceive.PK, Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT3243-1", ZDateTime.Today.AddMonths(-1));
			var whsReceiveLine1CustomsData = GetNewWhsBondedWarehouseAttribute(whsReceiveLine1.PK, 1000m, 50m, "KG", "AU", 100m, "NO", "", "ENT3243", (ZShort)1);
			var whsInventory = whsReceiveLine1.Inventory;
			Factory.Save();

			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Setup(m => m.IsInvoiceQuantityRequiredForBondedWarehouse).Returns(true);
			declarationMock.Setup(m => m.IsBondedWhsQuantityRequiredForBondedWarehouse).Returns(false);
			var declaration = declarationMock.Object;
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_BondedWhsQuantity = 50m;
			invoiceLine1.JI_PreviousEntryNumber = "ENT3333";
			invoiceLine1.JI_PreviousEntryLineNumber = 3;

			var inventorySelectionHeader = new DeclarationInventorySelectionHeader(declaration);
			var invoiceLines = new List<BaseJobComInvoiceLine> { invoiceLine1 };
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "Previous Entry Number, Previous Entry Line Number combination cannot be matched in the Warehouse Inventory"));

			invoiceLine1.JI_PreviousEntryNumber = "ENT3243";
			invoiceLine1.JI_PreviousEntryLineNumber = 1;
			invoiceLine1.ClearRowNotifications();
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.All(x => x.Message != "Previous Entry Number, Previous Entry Line Number combination cannot be matched in the Warehouse Inventory"));
			AssertEquals(Part.PK, invoiceLine1.JI_OP);
			AssertEquals("~~1", invoiceLine1.JI_PartNo);
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_ProductCode()
		{
			var part3 = CreateProduct(Owner.PK, "~~3");
			part3.OP_Desc = "~~3 DESC";

			var whsWarehouse = GetNewWhsWarehouse(Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = GetNewWhsReceive(whsWarehouse.PK, Importer.PK);
			var whsReceiveLine1 = GetNewWhsReceiveLine(whsReceive.PK, Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT3243-1", ZDateTime.Today.AddMonths(-1));
			var whsReceiveLine1CustomsData = GetNewWhsBondedWarehouseAttribute(whsReceiveLine1.PK, 1000m, 50m, "KG", "AU", 100m, "NO", "", "ENT3243", (ZShort)1);
			var whsInventory = whsReceiveLine1.Inventory;
			Factory.Save();

			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Setup(m => m.IsInvoiceQuantityRequiredForBondedWarehouse).Returns(true);
			declarationMock.Setup(m => m.IsBondedWhsQuantityRequiredForBondedWarehouse).Returns(false);
			var declaration = declarationMock.Object;
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 50m;
			invoiceLine1.JI_BondedWhsQuantity = 50m;
			invoiceLine1.JI_PreviousEntryLineNumber = 0;
			invoiceLine1.JI_PreviousEntryNumber = "";
			invoiceLine1.JI_OP = part3.PK;

			var inventorySelectionHeader = new DeclarationInventorySelectionHeader(declaration);
			var invoiceLines = new List<BaseJobComInvoiceLine> { invoiceLine1 };
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "No available warehouse inventory can be found for the requested product code"));

			invoiceLine1.JI_OP = Part.PK;
			invoiceLine1.ClearRowNotifications();
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.All(x => x.Message != "No available warehouse inventory can be found for the requested product code"));
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_Dec()
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var whsWarehouse = (IWhsWarehouse)helper.CreateWarehouse("N10", "R");
			Importer.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.NonMandatory;
			Importer.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.NonMandatory;
			Importer.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.NonMandatory;
			Importer.PartAttributeManager.SetProductToUseAttribute(Part, 1, true);
			Importer.PartAttributeManager.SetProductToUseAttribute(Part, 2, true);
			Importer.PartAttributeManager.SetProductToUseAttribute(Part, 3, true);
			Importer.PartAttributeManager.SetProductToUseAttribute(Part2, 1, true);
			Importer.PartAttributeManager.SetProductToUseAttribute(Part2, 2, true);
			Importer.PartAttributeManager.SetProductToUseAttribute(Part2, 3, true);

			Warehouse.MainAddress.OA_RN_NKCountryCode = "AU";
			whsWarehouse.WW_WarehouseName = Warehouse.MainAddress.OA_Address1;
			whsWarehouse.WW_OA_WarehouseAddress = Warehouse.MainAddress.PK;
			whsWarehouse.WW_IsVirtualWarehouse = true;
			((IWhsArea)whsWarehouse.Areas[0]).WA_AreaType = "BON";
			Factory.Save();

			var locationPK = helper.FindLocation(whsWarehouse.PK, "R").PK;
			var whsReceive1 = GetNewWhsReceive(whsWarehouse.PK, Importer.PK);
			var whsReceive2 = GetNewWhsReceive(whsWarehouse.PK, Importer.PK);
			var whsReceive3 = GetNewWhsReceive(whsWarehouse.PK, Importer.PK);
			var whsReceive4 = GetNewWhsReceive(whsWarehouse.PK, Importer.PK);
			var whsReceive5 = GetNewWhsReceive(whsWarehouse.PK, Importer.PK);
			var whsReceive6 = GetNewWhsReceive(whsWarehouse.PK, Importer.PK);
			var whsReceive7 = GetNewWhsReceive(whsWarehouse.PK, Importer.PK);
			whsReceive1.WD_ArrivalDate = ZDateTimeOffset.Today.AddMonths(-1);
			whsReceive2.WD_ArrivalDate = ZDateTimeOffset.Today.AddMonths(-3);
			whsReceive3.WD_ArrivalDate = ZDateTimeOffset.Today.AddMonths(-2);
			whsReceive4.WD_ArrivalDate = ZDateTimeOffset.Today.AddDays(-15);
			whsReceive5.WD_ArrivalDate = ZDateTimeOffset.Today.AddMonths(-2);
			whsReceive6.WD_ArrivalDate = ZDateTimeOffset.Today.AddMonths(-1);
			whsReceive7.WD_ArrivalDate = ZDateTimeOffset.Today.AddDays(-15);
			whsReceive1.WD_ExternalReference = "01";
			whsReceive2.WD_ExternalReference = "02";
			whsReceive3.WD_ExternalReference = "03";
			whsReceive4.WD_ExternalReference = "04";
			whsReceive5.WD_ExternalReference = "05";
			whsReceive6.WD_ExternalReference = "06";
			whsReceive7.WD_ExternalReference = "07";
			var whsReceiveLine1 = GetNewWhsReceiveLine(whsReceive1.PK, Part.PK, ZString.Empty, 1m, 10m, 10m, "NO", "PATT1", "PATT2", "PATT3", ZString.Empty, "EN00123-1", ZDateTime.Today.AddMonths(-1));
			var whsReceiveLine1CustomsData = GetNewWhsBondedWarehouseAttribute(whsReceiveLine1.PK, 150m, 20m, "KG", 4m, "GRM", 5m, "GRM", "AU", ZDecimal.Zero, "", "", "EN00123", (ZShort)1);
			var whsReceiveLine2 = GetNewWhsReceiveLine(whsReceive2.PK, Part2.PK, ZString.Empty, 1m, 100m, 100m, "NO", "PATT1", "PATT2", "PATT3", ZString.Empty, "EN00123-2", ZDateTime.Today.AddMonths(-3));
			var whsReceiveLine2CustomsData = GetNewWhsBondedWarehouseAttribute(whsReceiveLine2.PK, 1500m, 120m, "KG", 240m, "GRM", 250m, "GRM", "NZ", ZDecimal.Zero, "", "", "EN00123", (ZShort)2);
			var whsReceiveLine3 = GetNewWhsReceiveLine(whsReceive3.PK, Part.PK, ZString.Empty, 1m, 10m, 10m, "NO", "PATT1", "PATT2", "PATT3", ZString.Empty, "EN00123-3", ZDateTime.Today.AddMonths(-2));
			var whsReceiveLine3CustomsData = GetNewWhsBondedWarehouseAttribute(whsReceiveLine3.PK, 100m, 10m, "KG", 20m, "GRM", 30m, "GRM", "US", ZDecimal.Zero, "", "", "EN00123", (ZShort)3);
			var whsReceiveLine4 = GetNewWhsReceiveLine(whsReceive4.PK, Part.PK, ZString.Empty, 1m, 10m, 10m, "NO", "PATT1", "PATT2", "PATT3", ZString.Empty, "EN00123-4", ZDateTime.Today.AddDays(-15));
			var whsReceiveLine4CustomsData = GetNewWhsBondedWarehouseAttribute(whsReceiveLine4.PK, 300m, 40m, "KG", 80m, "GRM", 90m, "GRM", "SG", ZDecimal.Zero, "", "", "EN00123", (ZShort)4);
			var whsReceiveLine5 = GetNewWhsReceiveLine(whsReceive5.PK, Part2.PK, ZString.Empty, 1m, 100m, 100m, "NO", "PATT1", "PATT2", "PATT3", ZString.Empty, "EN00123-5", ZDateTime.Today.AddMonths(-2));
			var whsReceiveLine5CustomsData = GetNewWhsBondedWarehouseAttribute(whsReceiveLine5.PK, 2000m, 150m, "KG", 300m, "GRM", 310m, "GRM", "IT", ZDecimal.Zero, "", "", "EN00123", (ZShort)5);
			var whsReceiveLine6 = GetNewWhsReceiveLine(whsReceive6.PK, Part2.PK, ZString.Empty, 1m, 100m, 100m, "NO", "PATT1", "PATT2", "PATT3", ZString.Empty, "EN00123-6", ZDateTime.Today.AddMonths(-1));
			var whsReceiveLine6CustomsData = GetNewWhsBondedWarehouseAttribute(whsReceiveLine6.PK, 3000m, 200m, "KG", 400m, "GRM", 410m, "GRM", "DE", ZDecimal.Zero, "", "", "EN00123", (ZShort)6);
			var whsReceiveLine7 = GetNewWhsReceiveLine(whsReceive7.PK, Part.PK, ZString.Empty, 1m, 10m, 10m, "NO", "PATT1", "PATT2", "PATT3", ZString.Empty, "EN00123-7", ZDateTime.Today.AddDays(-15));
			var whsReceiveLine7CustomsData = GetNewWhsBondedWarehouseAttribute(whsReceiveLine7.PK, 3000m, 400m, "KG", 800m, "GRM", 810m, "GRM", "CA", ZDecimal.Zero, "", "", "EN00123", (ZShort)7);
			var whsInventory = whsReceiveLine1.Inventory;
			whsReceiveLine1.WE_WL = locationPK;
			whsReceiveLine2.WE_WL = locationPK;
			whsReceiveLine3.WE_WL = locationPK;
			whsReceiveLine4.WE_WL = locationPK;
			whsReceiveLine5.WE_WL = locationPK;
			whsReceiveLine6.WE_WL = locationPK;
			whsReceiveLine7.WE_WL = locationPK;
			whsReceive1.FinaliseDocketWithoutUserConfirmation();
			whsReceive2.FinaliseDocketWithoutUserConfirmation();
			whsReceive3.FinaliseDocketWithoutUserConfirmation();
			whsReceive4.FinaliseDocketWithoutUserConfirmation();
			whsReceive5.FinaliseDocketWithoutUserConfirmation();
			whsReceive6.FinaliseDocketWithoutUserConfirmation();
			whsReceive7.FinaliseDocketWithoutUserConfirmation();
			whsReceiveLine1.WE_SerialNumber = "SERIALNUM-1";
			whsReceiveLine2.WE_SerialNumber = "SERIALNUM-2";
			whsReceiveLine3.WE_SerialNumber = "SERIALNUM-3";
			whsReceiveLine4.WE_SerialNumber = "SERIALNUM-4";
			whsReceiveLine5.WE_SerialNumber = "SERIALNUM-5";
			whsReceiveLine6.WE_SerialNumber = "SERIALNUM-6";
			whsReceiveLine7.WE_SerialNumber = "SERIALNUM-7";
			Factory.Save();

			Importer.MiscServ.OM_IMUseSerialNumber = true;
			Importer.PartAttributeManager.SetProductToUseAttribute(Part, 6, true);
			Importer.PartAttributeManager.SetProductToUseAttribute(Part2, 6, true);
			var adjustment = WhsHelper.CreateWhsAdjustment(Importer.PK, whsWarehouse.PK, "AD1", null);
			adjustment[WhsDocketSchema.WD_DocketSubType] = "CUS";
			WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Part.PK, -6m, "R", "PATT1", "PATT2", "PATT3", "EN00123-1", serialNumber: "SERIALNUM-1");
			WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Part2.PK, -40m, "R", "PATT1", "PATT2", "PATT3", "EN00123-2", serialNumber: "SERIALNUM-2");
			WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Part.PK, -6m, "R", "PATT1", "PATT2", "PATT3", "EN00123-3", serialNumber: "SERIALNUM-3");
			WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Part.PK, -6m, "R", "PATT1", "PATT2", "PATT3", "EN00123-4", serialNumber: "SERIALNUM-4");
			WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Part2.PK, -50m, "R", "PATT1", "PATT2", "PATT3", "EN00123-5", serialNumber: "SERIALNUM-5");
			WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Part2.PK, -50m, "R", "PATT1", "PATT2", "PATT3", "EN00123-6", serialNumber: "SERIALNUM-6");
			WhsHelper.CreateWhsAdjustmentLine(adjustment.PK, Part.PK, -6m, "R", "PATT1", "PATT2", "PATT3", "EN00123-7", serialNumber: "SERIALNUM-7");
			WhsHelper.FinaliseDocketWithoutUserConfirmation(adjustment.PK);
			Factory.Save();

			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Protected().Setup<bool>("GetIsWHSUniversalXMLActive").Returns(true);
			declarationMock.Setup(m => m.IsInvoiceQuantityRequiredForBondedWarehouse).Returns(true);
			declarationMock.Setup(m => m.IsBondedWhsQuantityRequiredForBondedWarehouse).Returns(false);
			var declaration = declarationMock.Object;
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			var invoiceLines = new List<BaseJobComInvoiceLine>();
			var inventorySelectionHeader = new DeclarationInventorySelectionHeader(declaration);
			AssertEquals("No matching Warehouse found.", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;
			AssertEquals("At least one Invoice Line with valid Previous Entry Details or Part and Invoice Quantity is required.", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = ZString.Empty;
			var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = ZString.Empty;
			var invoice2Line1 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLines.Add(invoice1Line1);
			invoiceLines.Add(invoice2Line1);
			AssertEquals("At least one Invoice Line with valid Previous Entry Details or Part and Invoice Quantity is required.", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty);
			AssertInvoiceLine(invoice2Line1, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty);
			AssertInvoice(invoice1, ZDecimal.Zero, ZString.Empty);
			AssertInvoice(invoice2, ZDecimal.Zero, ZString.Empty);

			invoice1Line1.JI_PartNo = Part.OP_PartNum;
			invoice1Line1.JI_CustomsUnitQty = "KG";
			invoice1Line1.JI_CustomsSecondUnitQty = "GRM";
			invoice1Line1.JI_CustomsThirdUnitQty = "GRM";
			invoice2Line1.JI_InvoiceQuantity = 40m;
			AssertEquals("At least one Invoice Line with valid Previous Entry Details or Part and Invoice Quantity is required.", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, ZDecimal.Zero, "UNT", ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty);
			AssertInvoiceLine(invoice2Line1, 40m, ZString.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty);
			AssertInvoice(invoice1, ZDecimal.Zero, ZString.Empty);
			AssertInvoice(invoice2, ZDecimal.Zero, ZString.Empty);

			invoice1Line1.JI_InvoiceQuantity = 5m;
			AssertEquals("", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, 5m, "UNT", ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty);
			AssertInvoiceLine(invoice2Line1, 40m, ZString.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty);
			AssertInvoice(invoice1, ZDecimal.Zero, ZString.Empty);
			AssertInvoice(invoice2, ZDecimal.Zero, ZString.Empty);

			invoice1Line1.JI_PartAttrib1 = "PATT1";
			invoice1Line1.JI_PartAttrib2 = "PATT2";
			invoice1Line1.JI_PartAttrib3 = "PATT3";
			invoice1Line1.JI_SerialNumber = "SERIALNUM-3";
			AssertEquals("", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, 5m, "NO", 50m, 5m, 10m, 15, "US");
			AssertInvoiceLine(invoice2Line1, 40m, ZString.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty);
			AssertInvoice(invoice1, 50m, "ERN");
			AssertInvoice(invoice2, ZDecimal.Zero, ZString.Empty);

			invoice2Line1.JI_PartNo = Part2.OP_PartNum;
			invoice2Line1.JI_CustomsUnitQty = "KG";
			invoice2Line1.JI_CustomsSecondUnitQty = "GRM";
			invoice2Line1.JI_CustomsThirdUnitQty = "GRM";
			invoice2Line1.JI_PartAttrib1 = "PATT1";
			invoice2Line1.JI_PartAttrib2 = "PATT2";
			invoice2Line1.JI_PartAttrib3 = "PATT3";
			invoice2Line1.JI_SerialNumber = "SERIALNUM-2";
			AssertEquals("", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, 5m, "NO", 50m, 5m, 10m, 15m, "US");
			AssertInvoiceLine(invoice2Line1, 40m, "NO", 600m, 48m, 96m, 100m, "NZ");
			AssertInvoice(invoice1, 50m, "ERN");
			AssertInvoice(invoice2, 600m, "ERN");

			invoice1.JobComInvoiceLines.RemoveAndDeleteAll();
			invoice2.JobComInvoiceLines.RemoveAndDeleteAll();
			invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line1.JI_PartNo = Part.OP_PartNum;
			invoice1Line1.JI_PartAttrib1 = "PATT1";
			invoice1Line1.JI_PartAttrib2 = "PATT2";
			invoice1Line1.JI_PartAttrib3 = "PATT3";
			invoice1Line1.JI_SerialNumber = "SERIALNUM-3";
			invoice1Line1.JI_CustomsUnitQty = "KG";
			invoice1Line1.JI_CustomsSecondUnitQty = "GRM";
			invoice1Line1.JI_CustomsThirdUnitQty = "GRM";
			invoice1Line1.JI_InvoiceQuantity = 5m;
			invoice2Line1 = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line1.JI_PartNo = Part2.OP_PartNum;
			invoice2Line1.JI_PartAttrib1 = "PATT1";
			invoice2Line1.JI_PartAttrib2 = "PATT2";
			invoice2Line1.JI_PartAttrib3 = "PATT3";
			invoice2Line1.JI_SerialNumber = "SERIALNUM-2";
			invoice2Line1.JI_CustomsUnitQty = "KG";
			invoice2Line1.JI_CustomsSecondUnitQty = "GRM";
			invoice2Line1.JI_CustomsThirdUnitQty = "GRM";
			invoice2Line1.JI_InvoiceQuantity = 40m;
			var invoice2Line2 = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line2.JI_PartNo = Part.OP_PartNum;
			invoice2Line2.JI_PartAttrib1 = "PATT1";
			invoice2Line2.JI_PartAttrib2 = "PATT2";
			invoice2Line2.JI_PartAttrib3 = "PATT3";
			invoice2Line2.JI_SerialNumber = "SERIALNUM-1";
			invoice2Line2.JI_CustomsUnitQty = "KG";
			invoice2Line2.JI_CustomsSecondUnitQty = "GRM";
			invoice2Line2.JI_CustomsThirdUnitQty = "GRM";

			invoice2Line2.JI_InvoiceQuantity = 5m;
			var invoice1Line2 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line2.JI_PartNo = Part2.OP_PartNum;
			invoice1Line2.JI_PartAttrib1 = "PATT1";
			invoice1Line2.JI_PartAttrib2 = "PATT2";
			invoice1Line2.JI_PartAttrib3 = "PATT3";
			invoice1Line2.JI_SerialNumber = "SERIALNUM-5";
			invoice1Line2.JI_CustomsUnitQty = "KG";
			invoice1Line2.JI_CustomsSecondUnitQty = "GRM";
			invoice1Line2.JI_CustomsThirdUnitQty = "GRM";
			invoice1Line2.JI_InvoiceQuantity = 40m;
			invoiceLines.Clear();
			invoiceLines.Add(invoice1Line1);
			invoiceLines.Add(invoice2Line1);
			invoiceLines.Add(invoice1Line2);
			invoiceLines.Add(invoice2Line2);
			AssertEquals("", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, 5m, "NO", 50m, 5m, 10m, 15m, "US");
			AssertInvoiceLine(invoice2Line1, 40m, "NO", 600m, 48m, 96m, 100m, "NZ");
			AssertInvoiceLine(invoice2Line2, 5m, "NO", 75m, 10m, 2m, 2.5m, "AU");
			AssertInvoiceLine(invoice1Line2, 40m, "NO", 800m, 60m, 120m, 124m, "IT");
			AssertInvoice(invoice1, 850m, "ERN");
			AssertInvoice(invoice2, 675m, "ERN");

			declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Protected().Setup<bool>("GetIsWHSUniversalXMLActive").Returns(true);
			declarationMock.Setup(m => m.IsAllocatedQuantityRequiredForBondedWarehouse).Returns(true);
			declarationMock.Setup(m => m.IsInvoiceQuantityRequiredForBondedWarehouse).Returns(false);
			declarationMock.Setup(m => m.IsBondedWhsQuantityRequiredForBondedWarehouse).Returns(false);
			declaration = declarationMock.Object;
			invoiceLines = new List<BaseJobComInvoiceLine>();
			inventorySelectionHeader = new DeclarationInventorySelectionHeader(declaration);
			AssertEquals("No matching Warehouse found.", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;
			AssertEquals("Error when there is no valid invoice line", "At least one Invoice Line with valid Previous Entry Details or Part and Allocated Quantity is required.", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));

			invoice1 = declaration.Invoices.AddNew();
			invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
			invoice2Line1 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLines.Add(invoice1Line1);
			invoiceLines.Add(invoice2Line1);
			AssertEquals("Error when Allocated Inventory is empty for all the invoice lines", "At least one Invoice Line with valid Previous Entry Details or Part and Allocated Quantity is required.", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));

			whsInventory.WI_AllocationKey = "WI123";
			var invInventory1 = invoice1Line1.ComponentInventoryCollection.AddNew();
			invInventory1.JIV_AllocationKey = whsInventory.WI_AllocationKey;
			invInventory1.JIV_QuantityToDraw = 0;
			invoice1Line1.JI_PartNo = Part.OP_PartNum;
			var invInventory2 = invoice2Line1.ComponentInventoryCollection.AddNew();
			invInventory2.JIV_AllocationKey = whsInventory.WI_AllocationKey;
			invInventory2.JIV_QuantityToDraw = 0;
			invoice2Line1.JI_PartNo = Part2.OP_PartNum;
			AssertEquals("Error when Quantity to Draw is 0 for all the invoice lines", "At least one Invoice Line with valid Previous Entry Details or Part and Allocated Quantity is required.", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));

			invInventory1.JIV_QuantityToDraw = 0;
			invInventory2.JIV_QuantityToDraw = 2;
			AssertEquals("No error when Quantity to Draw is greater than 0 for any of the invoice lines", ZString.Empty, inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_MultipleLinesWithSameProduct()
		{
			// Setup 3 Inventories (Inventory1=Product1 100, Inventory2=Product2 50, Inventory3=Product2 150)
			// Setup 4 Invoice Lines (Line1=Product1 80, Line2=Product2 210, Line3=Product1 50, Line4=Product2 100)
			// Ensure Line1 has Inventory1
			// Ensure Line2 has Inventory2
			// Ensure Line3 has Inventory1
			// Ensure Line4 has Inventory3

			var whsWarehouse = GetNewWhsWarehouse(Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = GetNewWhsReceive(whsWarehouse.PK, Importer.PK, "RCV1", ZDateTimeOffset.Today.AddMonths(-1));
			var whsInventory1 = GetNewReceiveInventory(whsReceive, Part, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "EN00123-1", ZDateTimeOffset.Today.AddMonths(-1));
			GetNewWhsBondedWarehouseAttribute(whsInventory1.WI_WE_InDocketLine, 1000m, 50m, "KG", "AU", 100m, "NO", "", "EN00123", (ZShort)1);
			var whsInventory2 = GetNewReceiveInventory(whsReceive, Part2, ZString.Empty, 1m, 50m, 50m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "EN00123-2", ZDateTimeOffset.Today.AddMonths(-3));
			GetNewWhsBondedWarehouseAttribute(whsInventory2.WI_WE_InDocketLine, 500m, 20m, "KG", "NZ", 50m, "NO", "", "EN00123", (ZShort)2);
			var whsInventory3 = GetNewReceiveInventory(whsReceive, Part2, ZString.Empty, 1m, 150m, 150m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "EN00123-3", ZDateTimeOffset.Today.AddMonths(-2));
			GetNewWhsBondedWarehouseAttribute(whsInventory3.WI_WE_InDocketLine, 3000m, 150m, "KG", "US", 150m, "NO", "", "EN00123", (ZShort)3);
			Factory.Save();

			WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsInventory1.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory2.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory3.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();

			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Setup(m => m.IsInvoiceQuantityRequiredForBondedWarehouse).Returns(true);
			declarationMock.Setup(m => m.IsBondedWhsQuantityRequiredForBondedWarehouse).Returns(false);
			var declaration = declarationMock.Object;
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_PartNo = Part.OP_PartNum;
			invoiceLine1.JI_InvoiceQuantity = 80m;
			invoiceLine1.JI_InvoiceUQ = "NO";
			invoiceLine1.JI_CustomsUnitQty = "KG";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = Part2.OP_PartNum;
			invoiceLine2.JI_InvoiceQuantity = 210m;
			invoiceLine2.JI_InvoiceUQ = "NO";
			invoiceLine2.JI_CustomsUnitQty = "KG";
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_PartNo = Part.OP_PartNum;
			invoiceLine3.JI_InvoiceQuantity = 50m;
			invoiceLine3.JI_InvoiceUQ = "NO";
			invoiceLine3.JI_CustomsUnitQty = "KG";
			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_PartNo = Part2.OP_PartNum;
			invoiceLine4.JI_InvoiceQuantity = 100m;
			invoiceLine4.JI_InvoiceUQ = "NO";
			invoiceLine4.JI_CustomsUnitQty = "KG";

			var invoiceLines = new List<BaseJobComInvoiceLine>();
			invoiceLines.Add(invoiceLine1);
			invoiceLines.Add(invoiceLine2);
			invoiceLines.Add(invoiceLine3);
			invoiceLines.Add(invoiceLine4);
			var inventorySelectionHeader = new DeclarationInventorySelectionHeader(declaration);
			AssertEquals("", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoiceLine1, 80m, "NO", 800m, 40m, ZDecimal.Zero, ZDecimal.Zero, "AU");
			AssertInvoiceLine(invoiceLine2, 210m, "NO", 2100m, 84m, ZDecimal.Zero, ZDecimal.Zero, "NZ");
			AssertInvoiceLine(invoiceLine3, 50m, "NO", 500m, 25m, ZDecimal.Zero, ZDecimal.Zero, "AU");
			AssertInvoiceLine(invoiceLine4, 100m, "NO", 2000m, 100m, ZDecimal.Zero, ZDecimal.Zero, "US");
			AssertInvoice(invoice, 5400m, "ERN");
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_MultipleLinesWithBondedHeld()
		{
			var whsWarehouse = GetNewWhsWarehouse(Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = GetNewWhsReceive(whsWarehouse.PK, Importer.PK, "RCV1", ZDateTimeOffset.Today.AddMonths(-1));
			var whsInventory1 = GetNewReceiveInventory(whsReceive, Part, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "EN00123-1", ZDateTimeOffset.Today.AddMonths(-1));
			GetNewWhsBondedWarehouseAttribute(whsInventory1.WI_WE_InDocketLine, 1000m, 50m, "KG", "AU", 100m, "NO", "", "EN00123", (ZShort)1);
			var whsInventory2 = GetNewReceiveInventory(whsReceive, Part2, ZString.Empty, 1m, 50m, 50m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "EN00123-2", ZDateTimeOffset.Today.AddMonths(-3));
			GetNewWhsBondedWarehouseAttribute(whsInventory2.WI_WE_InDocketLine, 500m, 20m, "KG", "NZ", 50m, "NO", "", "EN00123", (ZShort)2);
			var whsInventory3 = GetNewReceiveInventory(whsReceive, Part2, ZString.Empty, 1m, 150m, 150m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "EN00123-3", ZDateTimeOffset.Today.AddMonths(-2));
			GetNewWhsBondedWarehouseAttribute(whsInventory3.WI_WE_InDocketLine, 3000m, 150m, "KG", "US", 150m, "NO", "", "EN00123", (ZShort)3);

			Factory.Save();

			WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsInventory1.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory2.WI_InventoryStatus = WhsDataTestHelper.InventoryHeldStatusCode;
			Factory.Load<IWhsReceiveLine>(whsInventory2.WI_WE_InDocketLine).WE_WHC_NKCurrentInventoryHeldCode = WhsDataTestHelper.InventoryHeldStatusCode;
			whsInventory3.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Setup(m => m.IsInvoiceQuantityRequiredForBondedWarehouse).Returns(true);
			declarationMock.Setup(m => m.IsBondedWhsQuantityRequiredForBondedWarehouse).Returns(false);
			var declaration = declarationMock.Object;
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_PartNo = Part.OP_PartNum;
			invoiceLine1.JI_InvoiceQuantity = 80m;
			invoiceLine1.JI_InvoiceUQ = "NO";
			invoiceLine1.JI_CustomsUnitQty = "KG";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = Part2.OP_PartNum;
			invoiceLine2.JI_InvoiceQuantity = 210m;
			invoiceLine2.JI_InvoiceUQ = "NO";
			invoiceLine2.JI_CustomsUnitQty = "KG";
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_PartNo = Part.OP_PartNum;
			invoiceLine3.JI_InvoiceQuantity = 50m;
			invoiceLine3.JI_InvoiceUQ = "NO";
			invoiceLine3.JI_CustomsUnitQty = "KG";
			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_PartNo = Part2.OP_PartNum;
			invoiceLine4.JI_InvoiceQuantity = 100m;
			invoiceLine4.JI_InvoiceUQ = "NO";
			invoiceLine4.JI_CustomsUnitQty = "KG";

			var invoiceLines = new List<BaseJobComInvoiceLine>();
			invoiceLines.Add(invoiceLine1);
			invoiceLines.Add(invoiceLine2);
			invoiceLines.Add(invoiceLine3);
			invoiceLines.Add(invoiceLine4);
			var inventorySelectionHeader = new DeclarationInventorySelectionHeader(declaration);
			AssertEquals("", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoiceLine1, 80m, "NO", 800m, 40m, ZDecimal.Zero, ZDecimal.Zero, "AU");
			AssertInvoiceLine(invoiceLine2, 210m, "NO", 4200m, 210m, ZDecimal.Zero, ZDecimal.Zero, "US");
			AssertInvoiceLine(invoiceLine3, 50m, "NO", 500m, 25m, ZDecimal.Zero, ZDecimal.Zero, "AU");
			AssertInvoiceLine(invoiceLine4, 100m, "NO", 2000m, 100m, ZDecimal.Zero, ZDecimal.Zero, "US");
			AssertInvoice(invoice, 7500m, "ERN");
		}

		public void TestRoundingIsDoneWhenRatioIsUsed()
		{
			var whsWarehouse = GetNewWhsWarehouse(Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = GetNewWhsReceive(whsWarehouse.PK, Importer.PK, "RCV1", ZDateTimeOffset.Today.AddMonths(-1));
			var whsInventory = GetNewReceiveInventory(whsReceive, Part, ZString.Empty, 18m, 378m, 378m, "NO", "PATT1", "PATT2", "PATT3", ZString.Empty, "EN00123-1", ZDateTimeOffset.Today.AddMonths(-1));
			GetNewWhsBondedWarehouseAttribute(whsInventory.WI_WE_InDocketLine, 150m, 21m, "KG", 7m, "GRM", "AU", ZDecimal.Zero, "", "", "EN00123", (ZShort)1);
			var whsInventory2 = GetNewReceiveInventory(whsReceive, Part2, ZString.Empty, 1m, 519m, 519m, "NO", "PATT1", "PATT2", "PATT3", ZString.Empty, "EN00123-2", ZDateTimeOffset.Today.AddMonths(-3));
			GetNewWhsBondedWarehouseAttribute(whsInventory2.WI_WE_InDocketLine, 1500m, 128m, "KG", 243m, "GRM", "NZ", ZDecimal.Zero, "", "", "EN00123", (ZShort)2);
			var whsInventory3 = GetNewReceiveInventory(whsReceive, Part2, ZString.Empty, 1m, 313m, 313m, "NO", "PATT1", "PATT2", "PATT3", ZString.Empty, "EN00123-3", ZDateTimeOffset.Today.AddMonths(-2));
			GetNewWhsBondedWarehouseAttribute(whsInventory3.WI_WE_InDocketLine, 100m, 10m, "KG", 20m, "GRM", "US", ZDecimal.Zero, "", "", "EN00123", (ZShort)3);
			Factory.Save();

			WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsInventory.WI_SerialNumber = "SERIALNUM";
			whsInventory2.WI_SerialNumber = "SERIALNUM";
			whsInventory3.WI_SerialNumber = "SERIALNUM";
			whsInventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory2.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory3.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();

			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Protected().Setup<bool>("GetIsWHSUniversalXMLActive").Returns(true);
			declarationMock.Setup(m => m.IsInvoiceQuantityRequiredForBondedWarehouse).Returns(true);
			declarationMock.Setup(m => m.IsBondedWhsQuantityRequiredForBondedWarehouse).Returns(false);
			var declaration = declarationMock.Object;
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			var invoiceLines = new List<BaseJobComInvoiceLine>();
			var inventorySelectionHeader = new DeclarationInventorySelectionHeader(declaration);
			AssertEquals("No matching Warehouse found.", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;
			AssertEquals("At least one Invoice Line with valid Previous Entry Details or Part and Invoice Quantity is required.", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = ZString.Empty;
			var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = ZString.Empty;
			var invoice2Line1 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLines.Add(invoice1Line1);
			invoiceLines.Add(invoice2Line1);
			AssertEquals("At least one Invoice Line with valid Previous Entry Details or Part and Invoice Quantity is required.", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty);
			AssertInvoiceLine(invoice2Line1, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty);
			AssertInvoice(invoice1, ZDecimal.Zero, ZString.Empty);
			AssertInvoice(invoice2, ZDecimal.Zero, ZString.Empty);

			invoice1Line1.JI_PartNo = Part.OP_PartNum;
			invoice1Line1.JI_CustomsUnitQty = "KG";
			invoice1Line1.JI_CustomsSecondUnitQty = "GRM";
			invoice2Line1.JI_InvoiceQuantity = 83m;
			AssertEquals("At least one Invoice Line with valid Previous Entry Details or Part and Invoice Quantity is required.", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, ZDecimal.Zero, "UNT", ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty);
			AssertInvoiceLine(invoice2Line1, 83m, ZString.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty);
			AssertInvoice(invoice1, ZDecimal.Zero, ZString.Empty);
			AssertInvoice(invoice2, ZDecimal.Zero, ZString.Empty);

			invoice1Line1.JI_InvoiceQuantity = 13m;
			AssertEquals("", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, 13m, "UNT", ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty);
			AssertInvoiceLine(invoice2Line1, 83m, ZString.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty);
			AssertInvoice(invoice1, ZDecimal.Zero, ZString.Empty);
			AssertInvoice(invoice2, ZDecimal.Zero, ZString.Empty);

			invoice1Line1.JI_PartAttrib1 = "PATT1";
			invoice1Line1.JI_PartAttrib2 = "PATT2";
			invoice1Line1.JI_PartAttrib3 = "PATT3";
			invoice1Line1.JI_SerialNumber = "SERIALNUM";
			AssertEquals("", inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines));
			AssertInvoiceLine(invoice1Line1, 13m, "NO", 5.16m, 0.722222m, 0.240741m, 0, "AU");
			AssertInvoiceLine(invoice2Line1, 83m, ZString.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty);
		}

		public void TestGetLowestLevelComponentsWithValueForDuty()
		{
			using (CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				WhsHelper.EnableWarehouseForBond(WhsWarehouse, true);
				WhsWarehouse.WW_IsVirtualWarehouse = true;

				var area = WhsHelper.CreateWhsArea(WhsWarehouse.PK, "IPR", "IPR");
				var row = WhsHelper.CreateRowAndGenerateLocations(WhsWarehouse, "I");
				var location = row.Locations[0] as IWhsLocation;
				location.WLV_WA_PutawayArea = area.PK;
				location.WLV_WA_PickingArea = area.PK;
				Factory.Save();

				var spoke = CreateProduct(Importer.PK, "SPOKE");
				var hub = CreateProduct(Importer.PK, "WHEEL HUB");
				var wheel = CreateProduct(Importer.PK, "WHEEL");
				var frame = CreateProduct(Importer.PK, "FRAME");
				var bike = CreateProduct(Importer.PK, "BIKE");

				WhsHelper.CreateProductBOM(wheel.PK, hub.PK, 1m);
				WhsHelper.CreateProductBOM(wheel.PK, spoke.PK, 20m);
				WhsHelper.CreateProductBOM(bike.PK, frame.PK, 1m);
				WhsHelper.CreateProductBOM(bike.PK, wheel.PK, 2m);

				var componentsReceive = GetNewWhsReceive(WhsWarehouse.PK, Importer.PK, "RCV1", ZDateTimeOffset.Today.AddMonths(-1));
				componentsReceive.WD_IsInwardsProcessingJob = true;
				var spokesReceiveLine = GetNewWhsReceiveLineWithBondedAttribute(componentsReceive.PK, spoke.PK, vfd: 10000m, qty: 10000m, location.PK, 1);
				var hubsReceiveLine = GetNewWhsReceiveLineWithBondedAttribute(componentsReceive.PK, hub.PK, vfd: 60000m, qty: 300m, location.PK, 2);
				var framesReceiveLine = GetNewWhsReceiveLineWithBondedAttribute(componentsReceive.PK, frame.PK, vfd: 50000m, qty: 500m, location.PK, 3);

				componentsReceive.FinaliseDocketWithoutUserConfirmation();
				Factory.Save();
				Assert("Precondition: Receive is finalised", !componentsReceive.WD_FinalisedDate.IsEmpty);

				CreateAndFinaliseWorkOrderWithLine(Importer.PK, WhsWarehouse.PK, wheel.PK, 200m);
				CreateAndFinaliseWorkOrderWithLine(Importer.PK, WhsWarehouse.PK, bike.PK, 10m);

				var wheelsInventory = Factory.LoadTop1<IWhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_OP, wheel.PK));
				var bikeInventory = Factory.LoadTop1<IWhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_OP, bike.PK));
				var wheelsInventoryLine = Factory.Load<IWhsDocketLine>(wheelsInventory.WI_WE_InDocketLine);
				var bikeInventoryLine = Factory.Load<IWhsDocketLine>(bikeInventory.WI_WE_InDocketLine);

				var declaration = Factory.New<BaseJobDeclarationForTesting>();
				declaration.JE_OH_Importer = Importer.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				Factory.Save();

				var inventorySelectionHeader = new DeclarationInventorySelectionHeaderForTest(declaration);

				var components = inventorySelectionHeader.GetLowestLevelComponents(bikeInventoryLine).ToDictionary<(IWhsDocketLine inv, decimal ratio), ZGuid>(x => x.inv.PK);

				CombineAssertions(() =>
				{
					AssertContainsExactElementsInAnyOrder(new[] { spokesReceiveLine.PK, hubsReceiveLine.PK, framesReceiveLine.PK }, components.Keys);
					AssertEquals("Frame ratio for 10 bikes", 10m / 500m, components[framesReceiveLine.PK].ratio); // 0.02
					AssertEquals("Hubs ratio for 10 bikes", 20m / 300m, components[hubsReceiveLine.PK].ratio); // 0.066666667
					AssertEquals("Spokes ratio for 10 bikes", 20m * 20m / 10000m, components[spokesReceiveLine.PK].ratio); // 0.04

					AssertEquals("One Wheel VFD", 220m, inventorySelectionHeader.GetValueForDutyFromComponents(wheelsInventoryLine, 1)); // 60000 / 300 + 20 x 1 = 220
					AssertEquals("VFD for 2 bikes", 1080m, inventorySelectionHeader.GetValueForDutyFromComponents(bikeInventoryLine, 2)); // (220 x 2 + 100[frame]) x 2 = 1080
				});
			}
		}

		public void TestGetLowestLevelComponentsRounding()
		{
			using (CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				WhsHelper.EnableWarehouseForBond(WhsWarehouse, true);
				WhsWarehouse.WW_IsVirtualWarehouse = true;

				var area = WhsHelper.CreateWhsArea(WhsWarehouse.PK, "IPR", "IPR");
				var row = WhsHelper.CreateRowAndGenerateLocations(WhsWarehouse, "I");
				var location = row.Locations[0] as IWhsLocation;
				location.WLV_WA_PutawayArea = area.PK;
				location.WLV_WA_PickingArea = area.PK;
				Factory.Save();

				var subpart = CreateProduct(Importer.PK, "SUBPART");
				var part = CreateProduct(Importer.PK, "PART");

				WhsHelper.CreateProductBOM(part.PK, subpart.PK, 0.6m);

				var partsReceive = GetNewWhsReceive(WhsWarehouse.PK, Importer.PK, "RCV1", ZDateTimeOffset.Today.AddMonths(-1));
				partsReceive.WD_IsInwardsProcessingJob = true;
				var subpartsReceiveLine = GetNewWhsReceiveLineWithBondedAttribute(partsReceive.PK, subpart.PK, vfd: 1000m, qty: 100m, location.PK, 1);

				partsReceive.FinaliseDocketWithoutUserConfirmation();
				Factory.Save();
				Assert("Precondition: Receive is finalised", !partsReceive.WD_FinalisedDate.IsEmpty);

				CreateAndFinaliseWorkOrderWithLine(Importer.PK, WhsWarehouse.PK, part.PK, 10m);

				var partInventory = Factory.LoadTop1<IWhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_OP, part.PK));
				var partInventoryLine = Factory.Load<IWhsDocketLine>(partInventory.WI_WE_InDocketLine);

				var declaration = Factory.New<BaseJobDeclarationForTesting>();
				declaration.JE_OH_Importer = Importer.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				Factory.Save();

				var inventorySelectionHeader = new DeclarationInventorySelectionHeaderForTest(declaration);
				var components = inventorySelectionHeader.GetLowestLevelComponents(partInventoryLine).ToDictionary<(IWhsDocketLine inv, decimal ratio), ZGuid>(x => x.inv.PK);

				CombineAssertions(() =>
				{
					// 0.6 subparts needed to make 1 part. The subpart count required to make the final number of parts should be rounded to integer before multiplying by VFD per UNIT which is 10
					AssertEquals("VFD for 1 item", 10m, inventorySelectionHeader.GetValueForDutyFromComponents(partInventoryLine, 1)); // 0.6 -> round to 1 x 10 = 1
					AssertEquals("VFD for 2 items", 10m, inventorySelectionHeader.GetValueForDutyFromComponents(partInventoryLine, 2)); // 0.6 x 2 -> round to 1 x 10 = 1
					AssertEquals("VFD for 3 items", 20m, inventorySelectionHeader.GetValueForDutyFromComponents(partInventoryLine, 3)); // 0.6 x 3 -> round to 2 x 10 = 2
				});
			}
		}

		public void TestPopulateLinePriceFromComponents()
		{
			using (CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				WhsHelper.EnableWarehouseForBond(WhsWarehouse, true);
				WhsWarehouse.WW_IsVirtualWarehouse = true;

				var area = WhsHelper.CreateWhsArea(WhsWarehouse.PK, "IPR", "IPR");
				var row = WhsHelper.CreateRowAndGenerateLocations(WhsWarehouse, "I");
				var location = row.Locations[0] as IWhsLocation;
				location.WLV_WA_PutawayArea = area.PK;
				location.WLV_WA_PickingArea = area.PK;
				Factory.Save();

				var frame = CreateProduct(Importer.PK, "FRAME");
				var bike = CreateProduct(Importer.PK, "BIKE");

				WhsHelper.CreateProductBOM(bike.PK, frame.PK, 1m);

				var componentsReceive = GetNewWhsReceive(WhsWarehouse.PK, Importer.PK, "RCV1", ZDateTimeOffset.Today.AddMonths(-1));
				componentsReceive.WD_IsInwardsProcessingJob = true;
				var framesReceiveLine = GetNewWhsReceiveLineWithBondedAttribute(componentsReceive.PK, frame.PK, vfd: 50000m, qty: 500m, location.PK, 1);

				componentsReceive.FinaliseDocketWithoutUserConfirmation();
				Factory.Save();
				Assert("Precondition: Receive is finalised", !componentsReceive.WD_FinalisedDate.IsEmpty);

				CreateAndFinaliseWorkOrderWithLine(Importer.PK, WhsWarehouse.PK, bike.PK, 4m);
				CreateAndFinaliseWorkOrderWithLine(Importer.PK, WhsWarehouse.PK, bike.PK, 6m);

				var frameInventory = Factory.LoadTop1<IWhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_OP, frame.PK));
				var bikeInventories = Factory.Load<IWhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_OP, bike.PK)).OrderBy(x => x.WI_AllocationKey).ToArray();
				AssertEquals("Precondition: 2 separate bike inventories", 2, bikeInventories.Length);

				var declaration = Factory.New<BaseJobDeclarationForTesting>();
				declaration.JE_OH_Importer = Importer.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();

				var invoiceLine1 = invoice.InvoiceLines.AddNew();
				var invInventory = invoiceLine1.ComponentInventoryCollection.AddNew();
				invInventory.JIV_AllocationKey = bikeInventories[0].WI_AllocationKey;
				invInventory.JIV_QuantityToDraw = 2;

				var invoiceLine2 = invoice.InvoiceLines.AddNew();
				var invInventory2 = invoiceLine2.ComponentInventoryCollection.AddNew();
				invInventory2.JIV_AllocationKey = bikeInventories[1].WI_AllocationKey;
				invInventory2.JIV_QuantityToDraw = 7;

				var inventorySelectionHeader = new DeclarationInventorySelectionHeaderForTest(declaration);

				CombineAssertions(() =>
				{
					inventorySelectionHeader.PopulateLinePriceFromComponents(invoiceLine1);
					AssertEquals("Line1 Price populated", 200m, invoiceLine1.JI_LinePrice);

					inventorySelectionHeader.PopulateLinePriceFromComponents(invoiceLine2);
					AssertEquals("Line2 Price populated - Multiple allocation keys", 700m, invoiceLine2.JI_LinePrice);
				});
			}
		}

		public void TestPopulateLinePriceFromComponents_SelectAndSynchronise()
		{
			using (CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				WhsHelper.EnableWarehouseForBond(WhsWarehouse, true);
				WhsWarehouse.WW_IsVirtualWarehouse = true;

				var area = WhsHelper.CreateWhsArea(WhsWarehouse.PK, "IPR", "IPR");
				var row = WhsHelper.CreateRowAndGenerateLocations(WhsWarehouse, "I");
				var location = row.Locations[0] as IWhsLocation;
				location.WLV_WA_PutawayArea = area.PK;
				location.WLV_WA_PickingArea = area.PK;
				Factory.Save();

				var frame = CreateProduct(Importer.PK, "FRAME");
				var bike = CreateProduct(Importer.PK, "BIKE");

				WhsHelper.CreateProductBOM(bike.PK, frame.PK, 1m);

				var componentsReceive = GetNewWhsReceive(WhsWarehouse.PK, Importer.PK, "RCV1", ZDateTimeOffset.Today.AddMonths(-1));
				componentsReceive.WD_IsInwardsProcessingJob = true;
				GetNewWhsReceiveLineWithBondedAttribute(componentsReceive.PK, frame.PK, vfd: 50000m, qty: 500m, location.PK, 1);

				componentsReceive.FinaliseDocketWithoutUserConfirmation();
				Factory.Save();
				Assert("Precondition: Receive is finalised", !componentsReceive.WD_FinalisedDate.IsEmpty);

				CreateAndFinaliseWorkOrderWithLine(Importer.PK, WhsWarehouse.PK, bike.PK, 10m);

				var bikeInventory = Factory.LoadTop1<IWhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_OP, bike.PK));

				var declaration = Factory.New<BaseJobDeclarationForTesting>();
				declaration.JE_OH_Importer = Importer.PK;
				declaration.WarehouseDocAddress.E2_OA_Address = WhsWarehouse.WW_OA_WarehouseAddress;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.Invoices.AddNew();

				var inventorySelectionHeader = new DeclarationInventorySelectionHeaderForTest(declaration);
				var wrapper = new WhsInventoryWrapper(bikeInventory, inventorySelectionHeader);
				wrapper.QuantityToDraw = 3;
				inventorySelectionHeader.SelectedLines.Add(wrapper);

				inventorySelectionHeader.ImportInventories();
				var invoiceLine = declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>().Single();

				AssertEquals("Price is set on Select Inventory", 300m, invoiceLine.JI_LinePrice);

				invoiceLine.JI_LinePrice = 10m;
				AssertEquals("Prerequisite: price is reset", 10m, invoiceLine.JI_LinePrice);

				AssertEquals("No Errors on Update", ZString.Empty, inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>()));
				AssertEquals("Price updated on Synchronise with Inventory", 300m, invoiceLine.JI_LinePrice);
			}
		}

		static void AssertInvoice(BaseJobComInvoiceHeader invoice, ZDecimal linePrice, ZString currencyUQ)
		{
			CombineAssertions(() =>
			{
				AssertEquals("invoice.JZ_InvoiceAmount", linePrice, invoice.JZ_InvoiceAmount);
				AssertEquals("invoice.JZ_RX_NKInvoice_Currency", currencyUQ, invoice.JZ_RX_NKInvoice_Currency);
			});
		}

		static void AssertInvoiceLine(BaseJobComInvoiceLine invoiceLine, ZDecimal invoiceQuantity, ZString invoiceUQ, ZDecimal linePrice, ZDecimal customsQuantity, ZDecimal customsSecondQuantity, ZDecimal customsThirdQuantity, ZString countryOfOrigin)
		{
			CombineAssertions(() =>
			{
				AssertEquals("invoiceLine.JI_InvoiceQuantity", invoiceQuantity, invoiceLine.JI_InvoiceQuantity);
				AssertEquals("invoiceLine.JI_InvoiceUQ", invoiceUQ, invoiceLine.JI_InvoiceUQ);
				AssertEquals("invoiceLine.JI_LinePrice", linePrice, invoiceLine.JI_LinePrice);
				AssertEquals("invoiceLine.JI_CustomsQuantity", customsQuantity, invoiceLine.JI_CustomsQuantity);
				AssertEquals("invoiceLine.JI_CustomsSecondQuantity", customsSecondQuantity, invoiceLine.JI_CustomsSecondQuantity);
				AssertEquals("invoiceLine.JI_CustomsThirdQuantity", customsThirdQuantity, invoiceLine.JI_CustomsThirdQuantity);
				AssertEquals("invoiceLine.JI_CountryOfOrigin", countryOfOrigin, invoiceLine.JI_CountryOfOrigin);
			});
		}

		class DeclarationInventorySelectionHeaderForTest : DeclarationInventorySelectionHeader
		{
			public DeclarationInventorySelectionHeaderForTest(BaseJobDeclaration declaration) : base(declaration)
			{
			}

			public new void PopulateLinePriceFromComponents(BaseJobComInvoiceLine invoiceLine) => base.PopulateLinePriceFromComponents(invoiceLine);

			public new ZDecimal GetValueForDutyFromComponents(IWhsDocketLine inventoryLine, ZDecimal quantityToDraw) => base.GetValueForDutyFromComponents(inventoryLine, quantityToDraw);
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Eritrea;
	}
}
