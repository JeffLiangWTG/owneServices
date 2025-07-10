using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(InventorySelectionHeader))]
	sealed class InventorySelectionHeaderTest : NonPersistentBusinessObjectTestCase
	{
		sealed class JobDeclarationForTesting : JobDeclaration
		{
			public JobDeclarationForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool IsInvoiceQuantityRequiredForBondedWarehouseReturns { get; set; }
			public bool IsBondedWhsQuantityRequiredForBondedWarehouseReturns { get; set; }

			protected override bool IsInvoiceQuantityRequiredForBondedWarehouse => IsInvoiceQuantityRequiredForBondedWarehouseReturns;
			protected override bool IsBondedWhsQuantityRequiredForBondedWarehouse => IsBondedWhsQuantityRequiredForBondedWarehouseReturns;
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_InwardEntryNumberAndInwardEntryLineNumberExistsAsAPair_IsConsumptionFTZ()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			Factory.Save();
			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.IsInvoiceQuantityRequiredForBondedWarehouseReturns = true;
			declaration.IsBondedWhsQuantityRequiredForBondedWarehouseReturns = false;
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.US_WHSEntryNumber = "123";
			invoiceLine1.US_WHSEntryLineNo = 0;
			var inventorySelectionHeader = new InventorySelectionHeader(declaration);
			var invoiceLines = new List<BaseJobComInvoiceLine> { invoiceLine1 };
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "WHS/FTZ Line No. must be supplied if the WHS Ent./FTZ Adm No. has been specified"));
			invoiceLine1.US_WHSEntryNumber = "";
			invoiceLine1.US_WHSEntryLineNo = 1;
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "WHS Ent./FTZ Adm No. must be supplied if the WHS/FTZ Line No. has been specified"));
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_BondedEntryKeyAndProductCodeCombination_IsConsumptionFTZ()
		{
			var part3 = Helper.CreateProduct(Helper.Owner.PK, "~~3");
			part3.OP_Desc = "~~3 DESC";
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, ZString.Empty, 50m, 1000m, 1000m, "PK", bondedEntryKey: "XJJ-EN00123-1", arrivalDate: ZDateTime.Today.AddMonths(-1));
			var addInfo = GetAddInfo();
			var secondUQAddInfo = JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty.Substring(3) + "=NO*";
			var thirdUQAddInfo = JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty.Substring(3) + "=DZ*";
			var whsCustomsAttribute = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine.PK, 15000m, 100m, "KG", "NZ", ZDecimal.Zero, "", addInfo, "XJJ-EN00123", (ZShort)1);
			var whsInventory = whsReceiveLine.Inventory;
			Factory.Save();
			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.IsInvoiceQuantityRequiredForBondedWarehouseReturns = true;
			declaration.IsBondedWhsQuantityRequiredForBondedWarehouseReturns = false;
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_OP = part3.PK;
			invoiceLine1.JI_PartNo = part3.OP_PartNum;
			invoiceLine1.JI_BondedWhsQuantity = 50m;
			invoiceLine1.US_WHSEntryNumber = "XJJ-EN00123";
			invoiceLine1.US_WHSEntryLineNo = 3;
			var inventorySelectionHeader = new InventorySelectionHeader(declaration);
			var invoiceLines = new List<BaseJobComInvoiceLine> { invoiceLine1 };
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "WHS Ent./FTZ Adm No., WHS/FTZ Line No. and Product Code combination cannot be matched in the Warehouse Inventory"));
			invoiceLine1.US_WHSEntryLineNo = 1;
			invoiceLine1.JI_OP = Helper.Part.PK;
			invoiceLine1.JI_PartNo = Helper.Part.OP_PartNum;
			invoiceLine1.ClearRowNotifications();
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.All(x => x.Message != "WHS Ent./FTZ Adm No., WHS/FTZ Line No. and Product Code combination cannot be matched in the Warehouse Inventory"));
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_BondedEntryKey__IsConsumptionFTZ()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, ZString.Empty, 50m, 1000m, 1000m, "PK", bondedEntryKey: "XJJ-EN00123-1", arrivalDate: ZDateTime.Today.AddMonths(-1));
			var addInfo = GetAddInfo();
			var secondUQAddInfo = JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty.Substring(3) + "=NO*";
			var thirdUQAddInfo = JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty.Substring(3) + "=DZ*";
			var whsCustomsAttribute = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine.PK, 15000m, 100m, "KG", "NZ", ZDecimal.Zero, "", addInfo, "XJJ-EN00123", (ZShort)1);
			var whsInventory = whsReceiveLine.Inventory;
			Factory.Save();
			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.IsInvoiceQuantityRequiredForBondedWarehouseReturns = true;
			declaration.IsBondedWhsQuantityRequiredForBondedWarehouseReturns = false;
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_BondedWhsQuantity = 50m;
			invoiceLine1.US_WHSEntryNumber = "XJJ-EN00123";
			invoiceLine1.US_WHSEntryLineNo = 3;
			var inventorySelectionHeader = new InventorySelectionHeader(declaration);
			var invoiceLines = new List<BaseJobComInvoiceLine> { invoiceLine1 };
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "WHS Ent./FTZ Adm No., WHS/FTZ Line No. combination cannot be matched in the Warehouse Inventory"));
			invoiceLine1.US_WHSEntryLineNo = 1;
			invoiceLine1.ClearRowNotifications();
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.All(x => x.Message != "WHS Ent./FTZ Adm No., WHS/FTZ Line No. combination cannot be matched in the Warehouse Inventory"));
			AssertEquals(Helper.Part.PK, invoiceLine1.JI_OP);
			AssertEquals("~~1", invoiceLine1.JI_PartNo);
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_ProductCode__IsConsumptionFTZ()
		{
			var part3 = Helper.CreateProduct(helper.Owner.PK, "~~3");
			part3.OP_Desc = "~~3 DESC";
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, ZString.Empty, 50m, 1000m, 1000m, "PK", bondedEntryKey: "XJJ-EN00123-1", arrivalDate: ZDateTime.Today.AddMonths(-1));
			var addInfo = GetAddInfo();
			var secondUQAddInfo = JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty.Substring(3) + "=NO*";
			var thirdUQAddInfo = JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty.Substring(3) + "=DZ*";
			var whsCustomsAttribute = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine.PK, 15000m, 100m, "KG", "NZ", ZDecimal.Zero, "", addInfo, "XJJ-EN00123", (ZShort)1);
			var whsInventory = whsReceiveLine.Inventory;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 50m;
			invoiceLine1.JI_BondedWhsQuantity = 50m;
			invoiceLine1.US_WHSEntryNumber = "";
			invoiceLine1.US_WHSEntryLineNo = 0;
			invoiceLine1.JI_OP = part3.PK;
			var inventorySelectionHeader = new InventorySelectionHeader(declaration);
			var invoiceLines = new List<BaseJobComInvoiceLine> { invoiceLine1 };
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "No available warehouse inventory can be found for the requested product code"));
			invoiceLine1.JI_OP = Helper.Part.PK;
			invoiceLine1.ClearRowNotifications();
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.All(x => x.Message != "No available warehouse inventory can be found for the requested product code"));
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_InwardEntryNumberAndInwardEntryLineNumberExistsAsAPair_IsNonConsumptionFTZ()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			Factory.Save();
			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.IsInvoiceQuantityRequiredForBondedWarehouseReturns = true;
			declaration.IsBondedWhsQuantityRequiredForBondedWarehouseReturns = false;
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			declaration.US_WHSEntryFilerCode = "XJJ";
			declaration.US_WHSEntryNumber = "EN00123";
			invoiceLine1.US_WHSEntryLineNo = 0;
			var inventorySelectionHeader = new InventorySelectionHeader(declaration);
			var invoiceLines = new List<BaseJobComInvoiceLine> { invoiceLine1 };
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "WHS/FTZ Line No. must be supplied if the Entry No. On Declaration has been specified"));
			declaration.US_WHSEntryFilerCode = "";
			declaration.US_WHSEntryNumber = "";
			invoiceLine1.US_WHSEntryLineNo = 1;
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "Entry No. On Declaration must be supplied if the WHS/FTZ Line No. has been specified"));
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_BondedEntryKeyAndProductCodeCombination_IsNonConsumptionFTZ()
		{
			var part3 = Helper.CreateProduct(Helper.Owner.PK, "~~3");
			part3.OP_Desc = "~~3 DESC";
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, ZString.Empty, 50m, 1000m, 1000m, "PK", bondedEntryKey: "XJJ-EN00123-1", arrivalDate: ZDateTime.Today.AddMonths(-1));
			var addInfo = GetAddInfo();
			var secondUQAddInfo = JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty.Substring(3) + "=NO*";
			var thirdUQAddInfo = JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty.Substring(3) + "=DZ*";
			var whsCustomsAttribute = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine.PK, 15000m, 100m, "KG", "NZ", ZDecimal.Zero, "", addInfo, "XJJ-EN00123", (ZShort)1);
			var whsInventory = whsReceiveLine.Inventory;
			Factory.Save();
			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.IsInvoiceQuantityRequiredForBondedWarehouseReturns = true;
			declaration.IsBondedWhsQuantityRequiredForBondedWarehouseReturns = false;
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_OP = part3.PK;
			invoiceLine1.JI_PartNo = part3.OP_PartNum;
			invoiceLine1.JI_BondedWhsQuantity = 50m;
			declaration.US_WHSEntryFilerCode = "XJJ";
			declaration.US_WHSEntryNumber = "EN00123";
			invoiceLine1.US_WHSEntryLineNo = 3;
			var inventorySelectionHeader = new InventorySelectionHeader(declaration);
			var invoiceLines = new List<BaseJobComInvoiceLine> { invoiceLine1 };
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "Entry No. On Declaration, WHS/FTZ Line No. and Product Code combination cannot be matched in the Warehouse Inventory"));
			invoiceLine1.US_WHSEntryLineNo = 1;
			invoiceLine1.JI_OP = Helper.Part.PK;
			invoiceLine1.JI_PartNo = Helper.Part.OP_PartNum;
			invoiceLine1.ClearRowNotifications();
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.All(x => x.Message != "Entry No. On Declaration, WHS/FTZ Line No. and Product Code combination cannot be matched in the Warehouse Inventory"));
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_BondedEntryKey__IsNonConsumptionFTZ()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, ZString.Empty, 50m, 1000m, 1000m, "PK", bondedEntryKey: "XJJ-EN00123-1", arrivalDate: ZDateTime.Today.AddMonths(-1));
			var addInfo = GetAddInfo();
			var secondUQAddInfo = JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty.Substring(3) + "=NO*";
			var thirdUQAddInfo = JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty.Substring(3) + "=DZ*";
			var whsCustomsAttribute = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine.PK, 15000m, 100m, "KG", "NZ", ZDecimal.Zero, "", addInfo, "XJJ-EN00123", (ZShort)1);
			var whsInventory = whsReceiveLine.Inventory;
			Factory.Save();
			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.IsInvoiceQuantityRequiredForBondedWarehouseReturns = true;
			declaration.IsBondedWhsQuantityRequiredForBondedWarehouseReturns = false;
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_BondedWhsQuantity = 50m;
			declaration.US_WHSEntryFilerCode = "XJJ";
			declaration.US_WHSEntryNumber = "EN00123";
			invoiceLine1.US_WHSEntryLineNo = 3;
			var inventorySelectionHeader = new InventorySelectionHeader(declaration);
			var invoiceLines = new List<BaseJobComInvoiceLine> { invoiceLine1 };
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "Entry No. On Declaration, WHS/FTZ Line No. combination cannot be matched in the Warehouse Inventory"));
			invoiceLine1.US_WHSEntryLineNo = 1;
			invoiceLine1.ClearRowNotifications();
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.All(x => x.Message != "Entry No. On Declaration, WHS/FTZ Line No. combination cannot be matched in the Warehouse Inventory"));
			AssertEquals(Helper.Part.PK, invoiceLine1.JI_OP);
			AssertEquals("~~1", invoiceLine1.JI_PartNo);
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_ProductCode__IsNonConsumptionFTZ()
		{
			var part3 = Helper.CreateProduct(helper.Owner.PK, "~~3");
			part3.OP_Desc = "~~3 DESC";
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, ZString.Empty, 50m, 1000m, 1000m, "PK", bondedEntryKey: "XJJ-EN00123-1", arrivalDate: ZDateTime.Today.AddMonths(-1));
			var addInfo = GetAddInfo();
			var secondUQAddInfo = JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty.Substring(3) + "=NO*";
			var thirdUQAddInfo = JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty.Substring(3) + "=DZ*";
			var whsCustomsAttribute = Helper.GetNewWhsBondedWarehouseAttribute(whsReceiveLine.PK, 15000m, 100m, "KG", "NZ", ZDecimal.Zero, "", addInfo, "XJJ-EN00123", (ZShort)1);
			var whsInventory = whsReceiveLine.Inventory;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 50m;
			invoiceLine1.JI_BondedWhsQuantity = 50m;
			declaration.US_WHSEntryFilerCode = "";
			declaration.US_WHSEntryNumber = "";
			invoiceLine1.US_WHSEntryLineNo = 0;
			invoiceLine1.JI_OP = part3.PK;
			var inventorySelectionHeader = new InventorySelectionHeader(declaration);
			var invoiceLines = new List<BaseJobComInvoiceLine> { invoiceLine1 };
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "No available warehouse inventory can be found for the requested product code"));
			invoiceLine1.JI_OP = Helper.Part.PK;
			invoiceLine1.ClearRowNotifications();
			inventorySelectionHeader.UpdateOutwardLinesWithInventoryDetails(invoiceLines);
			Assert(invoiceLine1.RowMessageErrors.All(x => x.Message != "No available warehouse inventory can be found for the requested product code"));
		}

		public void TestGetFilterDefaults()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = new InventorySelectionHeader(declaration);
			var filters = header.GetFilterDefaults();
			AssertEquals(false, filters.ContainsDefaultFor("Customs Entry Key:Property"));
			declaration.US_WHSEntryFilerCode = "XJ5";
			filters = header.GetFilterDefaults();
			var filter = filters["Customs Entry Key:Property"];
			AssertEquals("XJ5-", filter.Value);
			declaration.US_WHSEntryFilerCode = ZString.Empty;
			declaration.US_WHSEntryNumber = "342342";
			filters = header.GetFilterDefaults();
			filter = filters["Customs Entry Key:Property"];
			AssertEquals("-342342", filter.Value);
			declaration.US_WHSEntryFilerCode = "XJ5";
			filters = header.GetFilterDefaults();
			filter = filters["Customs Entry Key:Property"];
			AssertEquals("XJ5-342342", filter.Value);
		}

		public void TestCannotWithdrawFromDifferentEntryNumbers()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "102130121";
			tariff.UE_Unit1 = "KG";
			tariff.UE_Unit2 = "NO";
			tariff.UE_Unit3 = "DZ";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			Helper.Classification.CC_TariffNum = tariff.UE_Tariff;
			Helper.Pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			var messageError = "Products which have different entry numbers cannot be withdrawn on entry type '31'.";
			var header = new InventorySelectionHeader(declaration);
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive1 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "R1");
			var whsInventory1 = Helper.GetNewReceiveInventory(whsReceive1, Helper.Part, ZString.Empty, 50m, 900m, 900m, "PK", bondedEntryKey: "XJJ-EN00123-1");
			var whsReceive2 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "R2");
			var whsInventory2 = Helper.GetNewReceiveInventory(whsReceive2, Helper.Part, ZString.Empty, 50m, 900m, 900m, "PK", bondedEntryKey: "XJJ-EN00124-1");
			var whsReceive3 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "R3");
			var whsInventory3 = Helper.GetNewReceiveInventory(whsReceive3, Helper.Part, ZString.Empty, 50m, 900m, 900m, "PK", bondedEntryKey: "XJJ-EN00123-2");
			Factory.Save();
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive1.PK);
			whsReceive1.FinaliseDocketWithoutUserConfirmation();
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive2.PK);
			whsReceive2.FinaliseDocketWithoutUserConfirmation();
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive3.PK);
			whsReceive3.FinaliseDocketWithoutUserConfirmation();
			whsInventory1.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory2.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory3.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();
			header.IsGroupByInventory = true;
			AssertEquals(0, header.SelectionLines.Count);
			header.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory2, whsInventory3 });
			AssertEquals(3, header.SelectionLines.Count);
			var line1 = header.SelectionLines.OfType<InventorySelectionLine>().First(x => x.US_CustomsEntryKey == "XJJ-EN00123-1");
			line1.US_ProductQtyToDraw = 100m;
			AssertNoError(line1.US_ProductQtyToDrawInfo, messageError);
			var line2 = header.SelectionLines.OfType<InventorySelectionLine>().First(x => x.US_CustomsEntryKey == "XJJ-EN00123-2");
			line2.US_ProductQtyToDraw = 100m;
			AssertNoError(line2.US_ProductQtyToDrawInfo, messageError);
			var line3 = header.SelectionLines.OfType<InventorySelectionLine>().First(x => x.US_CustomsEntryKey == "XJJ-EN00124-1");
			line3.US_ProductQtyToDraw = 100m;
			AssertHasError(line3.US_ProductQtyToDrawInfo, messageError);
		}

		public void TestUpdateQtyInWHBeforeWithdrawal()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_WHSEntryFilerCode = "XJJ";
			declaration.US_WHSEntryNumber = "EN00123";
			var header = new InventorySelectionHeader(declaration);
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive1 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "R1");
			var whsReceiveLine1 = Helper.GetNewWhsReceiveLine(whsReceive1.PK, Helper.Part.PK, ZString.Empty, 50m, 100m, 100m, "PK", bondedEntryKey: "XJJ-EN00123-1");
			var whsInventory1 = whsReceiveLine1.Inventory;
			var whsReceiveLine3 = Helper.GetNewWhsReceiveLine(whsReceive1.PK, Helper.Part.PK, ZString.Empty, 50m, 300m, 300m, "PK", bondedEntryKey: "XJJ-EN00123-2");
			var whsInventory3 = whsReceiveLine3.Inventory;
			var whsReceiveLine4 = Helper.GetNewWhsReceiveLine(whsReceive1.PK, Helper.Part.PK, ZString.Empty, 50m, 400m, 400m, "PK", bondedEntryKey: "XJJ-EN00123-3");
			var whsInventory4 = whsReceiveLine4.Inventory;
			var whsReceive2 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "R2");
			var whsReceiveLine2 = Helper.GetNewWhsReceiveLine(whsReceive2.PK, Helper.Part.PK, ZString.Empty, 50m, 200m, 200m, "PK", bondedEntryKey: "XJJ-EN00124-1");
			var whsInventory2 = whsReceiveLine2.Inventory;
			Factory.Save();
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive1.PK);
			whsReceive1.FinaliseDocketWithoutUserConfirmation();
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive2.PK);
			whsReceive2.FinaliseDocketWithoutUserConfirmation();
			whsInventory1.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory2.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			whsInventory3.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;

			Factory.Save();

			header.IsGroupByInventory = true;
			header.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory2, whsInventory3 });
			AssertEquals(3, header.SelectionLines.Count);
			var line1 = header.SelectionLines.OfType<InventorySelectionLine>().First(x => x.US_CustomsEntryKey == "XJJ-EN00123-1");
			line1.US_ProductQtyToDraw = 100m;
			var line2 = header.SelectionLines.OfType<InventorySelectionLine>().First(x => x.US_CustomsEntryKey == "XJJ-EN00123-2");
			line2.US_ProductQtyToDraw = 100m;
			var line3 = header.SelectionLines.OfType<InventorySelectionLine>().First(x => x.US_CustomsEntryKey == "XJJ-EN00124-1");
			line3.US_ProductQtyToDraw = 100m;
			AssertEquals(3, header.SelectedLines.Count);
			header.ImportInventories();
			AssertEquals("Declaration's US_QtyInWHBeforeWithdrawal should be equal to sum of qty of all inventories which are matched to the entry number", 800m, declaration.US_QtyInWHBeforeWithdrawal);
		}

		public void TestMultiClassificationPart()
		{
			using (Helper.WhsHelper.UsePutawayEngineManagerMock())
			using (Helper.WhsHelper.UseAllocationEngineMock())
			{
				var data = (ZArchitecture.Environment.RegistryItemSet)ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
				var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (ZArchitecture.Environment.BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
				addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				Helper.Warehouse.OH_RL_NKClosestPort = "USCHI";
				var tariff1 = Factory.New<USCTariff>();
				tariff1.UE_Tariff = "1010101011";
				tariff1.UE_Unit1 = "KG";
				tariff1.UE_Unit2 = "NO";
				tariff1.UE_Unit3 = "DZ";
				tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
				tariff1.UE_DateTo = ZDateTime.MaxSmallDateTime;
				var tariff2 = Factory.New<USCTariff>();
				tariff2.UE_Tariff = "2010101011";
				tariff2.UE_Unit1 = "NO";
				tariff2.UE_Unit2 = "KG";
				tariff2.UE_Unit3 = "DZ";
				tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
				tariff2.UE_DateTo = ZDateTime.MaxSmallDateTime;
				var tariff3 = Factory.New<USCTariff>();
				tariff3.UE_Tariff = "3010101011";
				tariff3.UE_Unit1 = "DZ";
				tariff3.UE_Unit2 = "NO";
				tariff3.UE_Unit3 = "KG";
				tariff3.UE_DateFrom = ZDateTime.BrettsBirthday;
				tariff3.UE_DateTo = ZDateTime.MaxSmallDateTime;
				var product1 = Factory.New<OrgSupplierPart>();
				product1.OP_PartNum = "PART1";
				product1.OP_StockKeepingUnit = "CS";
				product1.RelatedOrganisations.AddOwner(Helper.Importer);
				var pivot1 = product1.PivotsForBinding.AddNew();
				pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
				pivot1.CI_TariffNum = tariff1.UE_Tariff;
				var pivot1Child1 = pivot1.Children.AddNew();
				pivot1Child1.CI_ChildType = ClassificationChildTypeList.Codes.Related;
				pivot1Child1.CI_TariffNum = tariff2.UE_Tariff;
				pivot1Child1.CD_AMMVPerUnit = 20m;
				var pivot1Child2 = pivot1.Children.AddNew();
				pivot1Child2.CI_ChildType = ClassificationChildTypeList.Codes.Related;
				pivot1Child2.CI_TariffNum = tariff2.UE_Tariff;
				pivot1Child2.CD_AMMVPerUnit = 10m;
				var pivot1Child3 = pivot1.Children.AddNew();
				pivot1Child3.CI_ChildType = ClassificationChildTypeList.Codes.Related;
				pivot1Child3.CI_TariffNum = tariff3.UE_Tariff;
				pivot1Child3.CD_AMMVPerUnit = 20m;
				var product2 = Factory.New<OrgSupplierPart>();
				product2.OP_PartNum = "PART2";
				product2.OP_StockKeepingUnit = "CS";
				product2.RelatedOrganisations.AddOwner(Helper.Importer);
				var pivot2 = product2.PivotsForBinding.AddNew();
				pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
				pivot2.CI_TariffNum = tariff1.UE_Tariff;
				var pivot2Child1 = pivot2.Children.AddNew();
				pivot2Child1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
				pivot2Child1.CI_TariffNum = tariff2.UE_Tariff;
				pivot2Child1.CD_AMMVPerUnit = 15m;
				var pivot2Child2 = pivot2.Children.AddNew();
				pivot2Child2.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
				pivot2Child2.CI_TariffNum = tariff3.UE_Tariff;
				pivot2Child2.CD_AMMVPerUnit = 30m;
				var inwardDeclaration = Factory.New<JobDeclaration>();
				inwardDeclaration.JE_OH_Importer = Helper.Importer.PK;
				inwardDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				inwardDeclaration.US_EntryType = EntryTypeList.Codes.Warehouse;
				inwardDeclaration.US_EnableENS = ZBool.True;
				inwardDeclaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse.MainAddress.PK;
				var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
				var warehouse = (IWhsWarehouse)helper.CreateWarehouse(Helper.Warehouse.MainAddress.OA_Address1, "WHS", "BOND");
				warehouse.WW_OA_WarehouseAddress = Helper.Warehouse.MainAddress.PK;
				warehouse.WW_IsBondedWarehouse = true;
				warehouse.WW_IsVirtualWarehouse = true;
				((IWhsArea)warehouse.Areas[0]).WA_AreaType = "BON";
				warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
				warehouse.WW_AutoPrintPackingSlip = false;
				inwardDeclaration.US_EntryFilerCode = "SV9";
				inwardDeclaration.ImportEntryNumber = "ENT32432";
				var inwardInvoice = inwardDeclaration.Invoices.AddNew();
				inwardInvoice.JZ_InvoiceNumber = "INV324";
				var inwardInvoiceLine1 = inwardInvoice.JobComInvoiceLines.AddNew();
				inwardInvoiceLine1.JI_PartNo = product1.OP_PartNum;
				inwardInvoiceLine1.JI_InvoiceQuantity = 100m;
				inwardInvoiceLine1.JI_CustomsQuantity = 200m;
				inwardInvoiceLine1.JI_CustomsSecondQuantity = 300m;
				inwardInvoiceLine1.JI_CustomsThirdQuantity = 400m;
				inwardInvoiceLine1.JI_LinePrice = 10000m;
				var productRelatedLines = new List<JobComInvoiceLine>(inwardInvoiceLine1.ProductRelatedLines);
				AssertEquals(3, productRelatedLines.Count);
				var inwardInvoiceLine1Child1 = productRelatedLines[0];
				inwardInvoiceLine1Child1.JI_InvoiceQuantity = 1000m;
				inwardInvoiceLine1Child1.JI_CustomsQuantity = 2000m;
				inwardInvoiceLine1Child1.JI_CustomsSecondQuantity = 3000m;
				inwardInvoiceLine1Child1.JI_CustomsThirdQuantity = 4000m;
				inwardInvoiceLine1Child1.JI_LinePrice = 2000m;
				var inwardInvoiceLine1Child2 = productRelatedLines[1];
				inwardInvoiceLine1Child2.JI_InvoiceQuantity = 200m;
				inwardInvoiceLine1Child2.JI_CustomsQuantity = 100m;
				inwardInvoiceLine1Child2.JI_CustomsSecondQuantity = 200m;
				inwardInvoiceLine1Child2.JI_CustomsThirdQuantity = 300m;
				inwardInvoiceLine1Child2.JI_LinePrice = 5000m;
				var inwardInvoiceLine1Child3 = productRelatedLines[2];
				inwardInvoiceLine1Child3.JI_InvoiceQuantity = 200m;
				inwardInvoiceLine1Child3.JI_CustomsQuantity = 100m;
				inwardInvoiceLine1Child3.JI_CustomsSecondQuantity = 200m;
				inwardInvoiceLine1Child3.JI_CustomsThirdQuantity = 300m;
				inwardInvoiceLine1Child3.JI_LinePrice = 5000m;
				var inwardInvoiceLine1Child4 = inwardInvoiceLine1.AddProductRelatedInvoiceLine();
				inwardInvoiceLine1Child4.JI_Tariff = tariff3.UE_Tariff;
				inwardInvoiceLine1Child4.JI_InvoiceQuantity = 300m;
				inwardInvoiceLine1Child4.JI_CustomsQuantity = 1000m;
				inwardInvoiceLine1Child4.JI_CustomsSecondQuantity = 2000m;
				inwardInvoiceLine1Child4.JI_CustomsThirdQuantity = 3000m;
				inwardInvoiceLine1Child4.JI_LinePrice = 2000m;
				var inwardInvoiceLine2 = inwardInvoice.JobComInvoiceLines.AddNew();
				inwardInvoiceLine2.JI_PartNo = product2.OP_PartNum;
				inwardInvoiceLine2.JI_InvoiceQuantity = 1000m;
				inwardInvoiceLine2.JI_CustomsQuantity = 2000m;
				inwardInvoiceLine2.JI_CustomsSecondQuantity = 3000m;
				inwardInvoiceLine2.JI_CustomsThirdQuantity = 4000m;
				inwardInvoiceLine2.JI_LinePrice = 20000m;
				var childLines = new List<JobComInvoiceLine>(inwardInvoiceLine2.ChildLines);
				AssertEquals(2, childLines.Count);
				var inwardInvoiceLine2Child1 = childLines[0];
				inwardInvoiceLine2Child1.JI_InvoiceQuantity = 100m;
				inwardInvoiceLine2Child1.JI_CustomsQuantity = 200m;
				inwardInvoiceLine2Child1.JI_CustomsSecondQuantity = 300m;
				inwardInvoiceLine2Child1.JI_CustomsThirdQuantity = 400m;
				inwardInvoiceLine2Child1.JI_LinePrice = 6000m;
				var inwardInvoiceLine2Child2 = childLines[1];
				inwardInvoiceLine2Child2.JI_InvoiceQuantity = 20m;
				inwardInvoiceLine2Child2.JI_CustomsQuantity = 10m;
				inwardInvoiceLine2Child2.JI_CustomsSecondQuantity = 20m;
				inwardInvoiceLine2Child2.JI_CustomsThirdQuantity = 30m;
				inwardInvoiceLine2Child2.JI_LinePrice = 10000m;
				var inwardInvoiceLine2Child3 = inwardInvoiceLine2.AddSecondaryInvoiceLine();
				inwardInvoiceLine2Child3.JI_Tariff = tariff3.UE_Tariff;
				inwardInvoiceLine2Child3.JI_InvoiceQuantity = 30m;
				inwardInvoiceLine2Child3.JI_CustomsQuantity = 100m;
				inwardInvoiceLine2Child3.JI_CustomsSecondQuantity = 200m;
				inwardInvoiceLine2Child3.JI_CustomsThirdQuantity = 300m;
				inwardInvoiceLine2Child3.JI_LinePrice = 4000m;
				var whsPack = inwardDeclaration.WHSPacks.AddNew();
				whsPack.US_PackageQty = 10;
				var whsPackLine1 = inwardDeclaration.WHSPackLines.AddNew(inwardInvoiceLine1);
				var whsPackLine2 = inwardDeclaration.WHSPackLines.AddNew(inwardInvoiceLine2);
				inwardDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
				inwardDeclaration.DoMerge();
				Factory.Save();
				var result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsReceive = (IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				var whsReceiveLines = Factory.Load<IWhsReceiveLine>(new ZQuery(WhsDocketLineSchema.WE_WD, whsReceive.PK));
				AssertEquals(2, whsReceiveLines.Length);
				var whsInventories = Factory.Load<IWhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_WE_InDocketLine, whsReceiveLines.Select(x => x.PK)));
				AssertEquals(2, whsInventories.Length);
				var whsInventory1 = whsInventories[0];
				var whsInventory2 = whsInventories[1];
				if (whsInventory2.WI_BondedEntryKey == "SV9-ENT32432-1")
				{
					whsInventory1 = whsInventories[1];
					whsInventory2 = whsInventories[0];
				}

				AssertEquals("SV9-ENT32432-1", whsInventory1.WI_BondedEntryKey);
				AssertEquals(100m, whsInventory1.WI_TotalUnits);
				AssertEquals("SV9-ENT32432-6", whsInventory2.WI_BondedEntryKey);
				AssertEquals(1000m, whsInventory2.WI_TotalUnits);
				inwardDeclaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded();
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_OH_Importer = Helper.Importer.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
				declaration.US_EnableENS = true;
				declaration.US_WHSEntryFilerCode = "SV9";
				declaration.US_WHSEntryNumber = "ENT32432";
				var header = new InventorySelectionHeader(declaration);
				header.IsGroupByCarton = true;
				header.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory2 });
				AssertEquals(1, header.SelectionLines.Count);
				header.SelectionLines[0].US_CartonQtytoDraw = 2;
				header.ImportInventories();
				var invoiceLines = new List<JobComInvoiceLine>(declaration.InvoiceLines.OfType<JobComInvoiceLine>().OrderBy(x => x.JI_PartNo + x.JI_LineNo));
				AssertEquals(9, invoiceLines.Count);
				var invoiceLine1 = invoiceLines[0];
				ZShort part1LineNo = 1;
				ZShort part2LineNo = 6;
				if (invoiceLine1.JI_LineNo == 5)
				{
					part1LineNo = 5;
					part2LineNo = 1;
				}

				AssertInvoiceLine(invoiceLine1, part1LineNo, "PART1", "1010101011", 20m, "CS", 40m, "KG", 60m, "NO", 80m, "DZ", ZDecimal.Zero, 2000m, ZGuid.Empty, ZGuid.Empty);
				AssertInvoiceLine(invoiceLines[1], part1LineNo + 1, "PART1", "2010101011", 200m, "CS", 400m, "NO", 600m, "KG", 800m, "DZ", 20m, 400m, ZGuid.Empty, invoiceLine1.PK);
				AssertInvoiceLine(invoiceLines[2], part1LineNo + 2, "PART1", "2010101011", 40m, "CS", 20m, "NO", 40m, "KG", 60m, "DZ", 10m, 1000m, ZGuid.Empty, invoiceLine1.PK);
				AssertInvoiceLine(invoiceLines[3], part1LineNo + 3, "PART1", "3010101011", 40m, "CS", 20m, "DZ", 40m, "NO", 60m, "KG", 20m, 1000m, ZGuid.Empty, invoiceLine1.PK);
				AssertInvoiceLine(invoiceLines[4], part1LineNo + 4, "PART1", "3010101011", 60m, "", 200m, "DZ", 400m, "NO", 600m, "KG", ZDecimal.Zero, 400m, ZGuid.Empty, invoiceLine1.PK);
				var invoiceLine5 = invoiceLines[5];
				AssertInvoiceLine(invoiceLine5, part2LineNo, "PART2", "1010101011", 200m, "CS", 400m, "KG", 600m, "NO", 800m, "DZ", ZDecimal.Zero, 4000m, ZGuid.Empty, ZGuid.Empty);
				AssertInvoiceLine(invoiceLines[6], part2LineNo + 1, "PART2", "2010101011", 4m, "CS", 2m, "NO", 4m, "KG", 6m, "DZ", 15m, 2000m, invoiceLine5.PK, ZGuid.Empty);
				AssertInvoiceLine(invoiceLines[7], part2LineNo + 2, "PART2", "3010101011", 20m, "CS", 40m, "DZ", 60m, "NO", 80m, "KG", 30m, 1200m, invoiceLine5.PK, ZGuid.Empty);
				AssertInvoiceLine(invoiceLines[8], part2LineNo + 3, "PART2", "3010101011", 6m, "", 20m, "DZ", 40m, "NO", 60m, "KG", 0m, 800m, invoiceLine5.PK, ZGuid.Empty);
			}
		}

		public void TestUpdateSelectionLinesDetails()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "102130121";
			tariff.UE_Unit1 = "KG";
			tariff.UE_Unit2 = "NO";
			tariff.UE_Unit3 = "DZ";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			Helper.Classification.CC_TariffNum = tariff.UE_Tariff;
			Helper.Pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = new InventorySelectionHeader(declaration);
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsInventory = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 50m, 1000m, 1000m, "PK", bondedEntryKey: "XJJ-EN00123-1");
			var addInfo = GetAddInfo();
			var secondUQAddInfo = JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty.Substring(3) + "=NO*";
			var thirdUQAddInfo = JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty.Substring(3) + "=DZ*";
			var whsCustomsAttribute = Helper.GetNewWhsBondedWarehouseAttribute(whsInventory.WI_WE_InDocketLine, 15000m, 100m, "KG", "NZ", ZDecimal.Zero, "", addInfo, "XJJ-EN00123", (ZShort)1);
			Factory.Save();
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsInventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();
			header.IsGroupByInventory = true;
			AssertEquals(0, header.SelectionLines.Count);
			header.UpdateSelectionLinesDetails(new[] { whsInventory });
			AssertEquals(1, header.SelectionLines.Count);
			var line = header.SelectionLines[0];
			line.US_ProductQtyToDraw = 800m;
			header.ImportInventories();
			AssertEquals("declaration.WarehouseAddress", Helper.Warehouse.MainAddress, declaration.WarehouseAddress);
			AssertEquals("declaration.US_WHSEntryFilerCode", "XJJ", declaration.US_WHSEntryFilerCode);
			AssertEquals("declaration.US_WHSEntryNumber", "EN00123", declaration.US_WHSEntryNumber);
			AssertEquals(1, declaration.Invoices.Count);
			var invoice = declaration.Invoices[0];
			AssertEquals("JZ_InvoiceAmount", 11600m, invoice.JZ_InvoiceAmount);
			AssertEquals("JZ_RX_NKInvoice_Currency", Core.Constants.CurrencyCodes.UnitedStates, invoice.JZ_RX_NKInvoice_Currency);
			AssertEquals(1, declaration.InvoiceLines.Count);
			var invoiceLine = declaration.InvoiceLines[0];
			AssertInvoiceLine(invoiceLine, Helper.Part, 11600m, 800m, "PK", 80m, "KG", "NZ", 80m, 160m, 240m, 320m, 400m, 480m, 560m, 640m, 720m, 800m, 880m, 960m, 1040m, 1120m, 1200m, 1280m, 0m, 0m, 1360m);
			AssertEquals("US_SecondUQ", "NO", invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("US_ThirdUQ", "DZ", invoiceLine.JI_CustomsThirdUnitQty);
			whsCustomsAttribute.WB_CustomsSecondUnitQty = "NO";
			whsCustomsAttribute.WB_CustomsSecondQuantity = 1360m;
			Factory.Save();
			header.ImportInventories();
			AssertEquals("declaration.WarehouseAddress", Helper.Warehouse.MainAddress, declaration.WarehouseAddress);
			AssertEquals("declaration.US_WHSEntryFilerCode", "XJJ", declaration.US_WHSEntryFilerCode);
			AssertEquals("declaration.US_WHSEntryNumber", "EN00123", declaration.US_WHSEntryNumber);
			AssertEquals(1, declaration.Invoices.Count);
			AssertEquals(invoice, declaration.Invoices[0]);
			AssertEquals("JZ_InvoiceAmount", 23200m, invoice.JZ_InvoiceAmount);
			AssertEquals("JZ_RX_NKInvoice_Currency", Core.Constants.CurrencyCodes.UnitedStates, invoice.JZ_RX_NKInvoice_Currency);
			AssertEquals(2, declaration.InvoiceLines.Count);
			AssertEquals(invoiceLine, declaration.InvoiceLines[0]);
			AssertInvoiceLine(invoiceLine, Helper.Part, 11600m, 800m, "PK", 80m, "KG", "NZ", 80m, 160m, 240m, 320m, 400m, 480m, 560m, 640m, 720m, 800m, 880m, 960m, 1040m, 1120m, 1200m, 1280m, 0m, 0m, 1360m);
			AssertEquals("US_SecondUQ", "NO", invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("US_ThirdUQ", "DZ", invoiceLine.JI_CustomsThirdUnitQty);
			invoiceLine = declaration.InvoiceLines[1];
			AssertInvoiceLine(invoiceLine, Helper.Part, 11600m, 800m, "PK", 80m, "KG", "NZ", 80m, 160m, 240m, 320m, 400m, 480m, 560m, 640m, 720m, 800m, 880m, 960m, 1040m, 1120m, 1200m, 1280m, 1088m, 0m, 1360m);
			AssertEquals("US_SecondUQ", "NO", invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("US_ThirdUQ", "DZ", invoiceLine.JI_CustomsThirdUnitQty);
			whsCustomsAttribute.WB_CustomsThirdUnitQty = "DZ";
			whsCustomsAttribute.WB_CustomsThirdQuantity = 1360m;
			Factory.Save();
			header.IsGroupByCarton = true;
			header.UpdateSelectionLinesDetails(new[] { whsInventory });
			AssertEquals(1, header.SelectionLines.Count);
			AssertEquals(line, header.SelectionLines[0]);
			declaration.US_WHSEntryFilerCode = "XJ5";
			declaration.US_WHSEntryNumber = "EN00124";
			declaration.WarehouseDocAddress.E2_OA_Address = Helper.Warehouse2.MainAddress.PK;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			line.US_ProductQtyToDraw = 600m;
			header.ImportInventories();
			AssertEquals("declaration.WarehouseAddress", Helper.Warehouse2.MainAddress, declaration.WarehouseAddress);
			AssertEquals("declaration.US_WHSEntryFilerCode", "XJ5", declaration.US_WHSEntryFilerCode);
			AssertEquals("declaration.US_WHSEntryNumber", "EN00124", declaration.US_WHSEntryNumber);
			AssertEquals(1, declaration.Invoices.Count);
			AssertEquals(invoice, declaration.Invoices[0]);
			AssertEquals("JZ_InvoiceAmount", 31900m, invoice.JZ_InvoiceAmount);
			AssertEquals("JZ_RX_NKInvoice_Currency", Core.Constants.CurrencyCodes.Australia, invoice.JZ_RX_NKInvoice_Currency);
			AssertEquals(3, declaration.InvoiceLines.Count);
			invoiceLine = declaration.InvoiceLines[0];
			AssertInvoiceLine(invoiceLine, Helper.Part, 11600m, 800m, "PK", 80m, "KG", "NZ", 80m, 160m, 240m, 320m, 400m, 480m, 560m, 640m, 720m, 800m, 880m, 960m, 1040m, 1120m, 1200m, 1280m, 0m, 0m, 1360m);
			AssertEquals("US_SecondUQ", "NO", invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("US_ThirdUQ", "DZ", invoiceLine.JI_CustomsThirdUnitQty);
			invoiceLine = declaration.InvoiceLines[1];
			AssertInvoiceLine(invoiceLine, Helper.Part, 11600m, 800m, "PK", 80m, "KG", "NZ", 80m, 160m, 240m, 320m, 400m, 480m, 560m, 640m, 720m, 800m, 880m, 960m, 1040m, 1120m, 1200m, 1280m, 1088m, 0m, 1360m);
			AssertEquals("US_SecondUQ", "NO", invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("US_ThirdUQ", "DZ", invoiceLine.JI_CustomsThirdUnitQty);
			invoiceLine = declaration.InvoiceLines[2];
			AssertInvoiceLine(invoiceLine, Helper.Part, 8700m, 600m, "PK", 60m, "KG", "NZ", 60m, 120m, 180m, 240m, 300m, 360m, 420m, 480m, 540m, 600m, 660m, 720m, 780m, 840m, 900m, 960m, 816m, 816m, 1020m);
			AssertEquals("US_SecondUQ", "NO", invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals("US_ThirdUQ", "DZ", invoiceLine.JI_CustomsThirdUnitQty);
		}

		public void TestRemovePGAIndicatorFromInventoryIfIrrelevant()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "102130121";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			Helper.Classification.CC_TariffNum = tariff.UE_Tariff;
			Helper.Pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var header = new InventorySelectionHeader(declaration);
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsInventory = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 50m, 900m, 900m, "PK", bondedEntryKey: "XJ5-EN00123-1");
			var addInfo = GetPGAIndicatorsInfo();
			Helper.GetNewWhsBondedWarehouseAttribute(whsInventory.WI_WE_InDocketLine, 15000m, 100m, "KG", "NZ", ZDecimal.Zero, "", addInfo, "XJ5-EN00123", (ZShort)1);
			Factory.Save();
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsInventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			Factory.Save();
			header.IsGroupByInventory = true;
			AssertEquals(0, header.SelectionLines.Count);
			header.UpdateSelectionLinesDetails(new[] { whsInventory });
			AssertEquals(1, header.SelectionLines.Count);
			var line = header.SelectionLines[0];
			line.US_ProductQtyToDraw = 800m;
			header.ImportInventories();
			AssertEquals("declaration.US_WHSEntryFilerCode", "XJ5", declaration.US_WHSEntryFilerCode);
			AssertEquals("declaration.US_WHSEntryNumber", "EN00123", declaration.US_WHSEntryNumber);
			AssertEquals(1, declaration.Invoices.Count);
			var invoice = declaration.Invoices[0];
			AssertEquals(1, declaration.InvoiceLines.Count);
			var invoiceLine = declaration.InvoiceLines[0];
			CombineAssertions(() =>
			{
				AssertEquals("NMFS 370 Indicator should be defaulted to 'D'", OGAIndicatorList.Codes.Declared, invoiceLine.US_NMFS370Ind);
				AssertEquals("NMFS AMR Indicator should be defaulted to 'D'", OGAIndicatorList.Codes.Declared, invoiceLine.US_NMFSAMRInd);
				AssertEquals("NMFS HMS Indicator should be defaulted to 'D'", OGAIndicatorList.Codes.Declared, invoiceLine.US_NMFSHMSInd);
				AssertEquals("NMFS SIM Indicator should be defaulted to 'D'", OGAIndicatorList.Codes.Declared, invoiceLine.US_NMFSSIMPInd);
				AssertEquals("Lacey Indicator should be defaulted to 'D'", OGAIndicatorList.Codes.Declared, invoiceLine.US_LaceyIndicator);
				AssertEquals("DEA Indicator should be defaulted to 'D'", OGAIndicatorList.Codes.Declared, invoiceLine.US_DEAInd);
				AssertEquals("OMC Indicator should be defaulted to 'D'", OGAIndicatorList.Codes.Declared, invoiceLine.US_OMCInd);
				AssertEquals("TTB Indicator should be defaulted to 'D'", OGAIndicatorList.Codes.Declared, invoiceLine.US_TTBInd);
				AssertEquals("ATF Indicator SHOULD NOT be defaulted as entry type is '31'", ZString.Empty, invoiceLine.US_ATFInd);
				AssertEquals("AMS Indicator SHOULD NOT be defaulted as entry type is '31'", ZString.Empty, invoiceLine.US_AMSInd);
				AssertEquals("NOP Indicator SHOULD NOT be defaulted as entry type is '31'", ZString.Empty, invoiceLine.US_NOPInd);
				AssertEquals("APHIS Indicator SHOULD NOT be defaulted as entry type is '31'", ZString.Empty, invoiceLine.US_APHISInd);
				AssertEquals("CPSC Indicator SHOULD NOT be defaulted as entry type is '31'", ZString.Empty, invoiceLine.US_CPSCInd);
				AssertEquals("DDTC Indicator SHOULD NOT be defaulted as entry type is '31'", ZString.Empty, invoiceLine.US_DDTCInd);
				AssertEquals("FSIS Indicator SHOULD NOT be defaulted as entry type is '31'", ZString.Empty, invoiceLine.US_FSISInd);
				AssertEquals("NHTSA Indicator SHOULD NOT be defaulted as entry type is '31'", ZString.Empty, invoiceLine.US_NHTSAIndicator);
				AssertEquals("ODS Indicator SHOULD NOT be defaulted as entry type is '31'", ZString.Empty, invoiceLine.US_ODSInd);
				AssertEquals("TSCA Indicator SHOULD NOT be defaulted as entry type is '31'", ZString.Empty, invoiceLine.US_TSCAInd);
				AssertEquals("PST Indicator SHOULD NOT be defaulted as entry type is '31'", ZString.Empty, invoiceLine.US_PSTIndicator);
				AssertEquals("VNE Indicator SHOULD NOT be defaulted as entry type is '31'", ZString.Empty, invoiceLine.US_VNEInd);
				AssertEquals("FWS Indicator SHOULD NOT be defaulted as entry type is '31'", ZString.Empty, invoiceLine.US_FWSInd);
				AssertEquals("FDA Indicator SHOULD NOT be defaulted as entry type is '31'", ZString.Empty, invoiceLine.US_FDAIndicator);
			});
		}

		public void TestCalculateLinePriceFromAMMVPercentage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_EnableENS = true;
			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "PART1";
			product1.OP_StockKeepingUnit = "CS";
			product1.RelatedOrganisations.AddOwner(Helper.Importer);
			var pivot1 = product1.PivotsForBinding.AddNew();
			pivot1.CD_AMMVPercentage = 20m;
			var attr1 = pivot1.Attributes1.AddNew();
			attr1.BG_AttributeValue1 = "PATT1";
			var attr2 = pivot1.Attributes2.AddNew();
			attr2.BG_AttributeValue1 = "PATT2";
			var attr3 = pivot1.Attributes3.AddNew();
			attr3.BG_AttributeValue1 = "PATT3";
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsInventory = Helper.GetNewReceiveInventory(whsReceive, product1, ZString.Empty, 50m, 1000m, 1000m, "NO", "PATT1", "PATT2", "PATT3", "", "EN00123-1");
			Helper.GetNewWhsBondedWarehouseAttribute(whsInventory.WI_WE_InDocketLine, 15000m, 2000m, "KG", "NZ", ZDecimal.Zero, "", "", "EN00123", (ZShort)1);
			Factory.Save();
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();
			whsInventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
			var header = new InventorySelectionHeader(declaration);

			Factory.Save();

			header.UpdateSelectionLinesDetails(new[] { whsInventory });
			var line = header.SelectionLines[0];
			line.US_ProductQtyToDraw = 80m;
			header.ImportInventories();
			AssertEquals(1000m, declaration.InvoiceLines[0].JI_LinePrice);
		}

		protected override BusinessObject GetNewBusinessObject() => new InventorySelectionHeader(Factory.New<JobDeclaration>());

		void AssertInvoiceLine(JobComInvoiceLine invoiceLine, ZShort lineNo, ZString partNo, ZString tariff, ZDecimal invoiceQty, ZString invoiceUQ, ZDecimal customsQty, ZString customsUQ, ZDecimal secondQty, ZString secondUQ, ZDecimal thirdQty, ZString thirdUQ, ZDecimal ammvPerUnit, ZDecimal linePrice, ZGuid parentInvoiceLinePK, ZGuid productRelatedPK)
		{
			CombineAssertions(() =>
			{
				AssertEquals("JI_LineNo", lineNo, invoiceLine.JI_LineNo);
				AssertEquals("JI_PartNo", partNo, invoiceLine.JI_PartNo);
				AssertEquals("JI_Tariff", tariff, invoiceLine.JI_Tariff);
				AssertEquals("JI_InvoiceQuantity", invoiceQty, invoiceLine.JI_InvoiceQuantity);
				AssertEquals("JI_InvoiceUQ", invoiceUQ, invoiceLine.JI_InvoiceUQ);
				AssertEquals("JI_CustomsQuantity", customsQty, invoiceLine.JI_CustomsQuantity);
				AssertEquals("JI_CustomsUnitQty", customsUQ, invoiceLine.JI_CustomsUnitQty);
				AssertEquals("JI_CustomsSecondQuantity", secondQty, invoiceLine.JI_CustomsSecondQuantity);
				AssertEquals("JI_CustomsSecondUnitQty", secondUQ, invoiceLine.JI_CustomsSecondUnitQty);
				AssertEquals("JI_CustomsThirdQuantity", thirdQty, invoiceLine.JI_CustomsThirdQuantity);
				AssertEquals("JI_CustomsThirdUnitQty", thirdUQ, invoiceLine.JI_CustomsThirdUnitQty);
				AssertEquals("US_AMMVPerUnit", ammvPerUnit, invoiceLine.US_AMMVPerUnit);
				AssertEquals("JI_LinePrice", linePrice, invoiceLine.JI_LinePrice);
				AssertEquals("JI_ParentID", parentInvoiceLinePK, invoiceLine.JI_ParentID);
				AssertEquals("US_JI_ParentProduct", productRelatedPK, invoiceLine.US_JI_ParentProduct);
			});
		}

		ZString GetAddInfo()
		{
			var infoKeyAndValue = "{0}={1}*";
			var result = new ZStringBuilder(string.Format(infoKeyAndValue, JobComInvoiceLine.Schema.US_AgricultureLicNo.Substring(3), "ALN3"));
			result.Append(string.Format(infoKeyAndValue, JobComInvoiceLine.Schema.US_AMMVPerUnit.Substring(3), "0.5"));
			var count = 1;
			foreach (var field in new[]
				{
					JobComInvoiceLine.Schema.US_98GoodsValue,
					JobComInvoiceLine.Schema.US_GrossWeight,
					JobComInvoiceLine.Schema.US_WeightNET,
					JobComInvoiceLine.Schema.US_ADDDepositValue,
					JobComInvoiceLine.Schema.US_ADDuty,
					JobComInvoiceLine.Schema.US_ADDQty,
					JobComInvoiceLine.Schema.US_CVDDepositValue,
					JobComInvoiceLine.Schema.US_CVDQty,
					JobComInvoiceLine.Schema.US_CVDuty,
					JobComInvoiceLine.Schema.US_CustomsValue,
					JobComInvoiceLine.Schema.US_DDTCQuantity,
					JobComInvoiceLine.Schema.US_Duty,
					JobComInvoiceLine.Schema.US_SupDuty,
					JobComInvoiceLine.Schema.US_SupQty1,
					JobComInvoiceLine.Schema.US_SupQty2,
					JobComInvoiceLine.Schema.US_SupQty3,
					JobComInvoiceLine.Schema.US_VisaQty
				})
			{
				result.Append(string.Format(infoKeyAndValue, field.Substring(3), count++ * 100m));
			}
			return result.ToString();
		}

		ZString GetPGAIndicatorsInfo()
		{
			var infoKeyValue = "{0}={1}*";
			var result = new ZStringBuilder();

			foreach (var indicatorField in new[]
				{
					JobComInvoiceLine.Schema.US_ATFInd,
					JobComInvoiceLine.Schema.US_NMFS370Ind,
					JobComInvoiceLine.Schema.US_NMFSHMSInd,
					JobComInvoiceLine.Schema.US_NMFSAMRInd,
					JobComInvoiceLine.Schema.US_NMFSSIMPInd,
					JobComInvoiceLine.Schema.US_AMSInd,
					JobComInvoiceLine.Schema.US_NOPInd,
					JobComInvoiceLine.Schema.US_APHISInd,
					JobComInvoiceLine.Schema.US_CPSCInd,
					JobComInvoiceLine.Schema.US_DDTCInd,
					JobComInvoiceLine.Schema.US_DEAInd,
					JobComInvoiceLine.Schema.US_FSISInd,
					JobComInvoiceLine.Schema.US_FWSInd,
					JobComInvoiceLine.Schema.US_LaceyIndicator,
					JobComInvoiceLine.Schema.US_NHTSAIndicator,
					JobComInvoiceLine.Schema.US_ODSInd,
					JobComInvoiceLine.Schema.US_TSCAInd,
					JobComInvoiceLine.Schema.US_OMCInd,
					JobComInvoiceLine.Schema.US_PSTIndicator,
					JobComInvoiceLine.Schema.US_TTBInd,
					JobComInvoiceLine.Schema.US_VNEInd,
					JobComInvoiceLine.Schema.US_FDAIndicator
				})
			{
				result.Append(string.Format(infoKeyValue, indicatorField.Substring(3), "D"));
			}

			return result.ToString();
		}

		void AssertInvoiceLine(JobComInvoiceLine invoiceLine, Customs.Business.OrgSupplierPart part, ZDecimal linePrice, ZDecimal invoiceQuantity, ZString invoiceUQ, ZDecimal customsQuantity, ZString customsUnitQty, ZString countryOfOrigin,
			ZDecimal us98GoodsValue, ZDecimal weight, ZDecimal weightNET, ZDecimal addDepositValue, ZDecimal adduty, ZDecimal addQty, ZDecimal cvdDepositValue, ZDecimal cvdQty, ZDecimal cvduty, ZDecimal customsValue, ZDecimal ddtcQuantity, ZDecimal duty,
			ZDecimal supDuty, ZDecimal supQty1, ZDecimal supQty2, ZDecimal supQty3, ZDecimal secondQty, ZDecimal thirdQty, ZDecimal visaQty)
		{
			CombineAssertions(() =>
			{
				AssertEquals("JI_PartNo", part.OP_PartNum, invoiceLine.JI_PartNo);
				AssertEquals("JI_OP", part.PK, invoiceLine.JI_OP);
				AssertEquals("JI_LinePrice", linePrice, invoiceLine.JI_LinePrice);
				AssertEquals("JI_InvoiceQuantity", invoiceQuantity, invoiceLine.JI_InvoiceQuantity);
				AssertEquals("JI_InvoiceUQ", invoiceUQ, invoiceLine.JI_InvoiceUQ);
				AssertEquals("JI_CustomsQuantity", customsQuantity, invoiceLine.JI_CustomsQuantity);
				AssertEquals("JI_CustomsUnitQty", customsUnitQty, invoiceLine.JI_CustomsUnitQty);
				AssertEquals("JI_CountryOfOrigin", countryOfOrigin, invoiceLine.JI_CountryOfOrigin);
				AssertEquals("JI_CustomsSecondQuantity", secondQty, invoiceLine.JI_CustomsSecondQuantity);
				AssertEquals("JI_CustomsThirdQuantity", thirdQty, invoiceLine.JI_CustomsThirdQuantity);
				AssertEquals("US_AgricultureLicNo", "ALN3", invoiceLine.US_AgricultureLicNo);
				AssertEquals("US_AMMVPerUnit", 0.5m, invoiceLine.US_AMMVPerUnit);
				AssertEquals("US_98GoodsValue", us98GoodsValue, invoiceLine.US_98GoodsValue);
				AssertEquals("US_GrossWeight", weight, invoiceLine.US_GrossWeight);
				AssertEquals("US_WeightNET", weightNET, invoiceLine.US_WeightNET);
				AssertEquals("US_ADDDepositValue", addDepositValue, invoiceLine.US_ADDDepositValue);
				AssertEquals("US_ADDuty", adduty, invoiceLine.US_ADDuty);
				AssertEquals("US_ADDQty", addQty, invoiceLine.US_ADDQty);
				AssertEquals("US_CVDDepositValue", cvdDepositValue, invoiceLine.US_CVDDepositValue);
				AssertEquals("US_CVDQty", cvdQty, invoiceLine.US_CVDQty);
				AssertEquals("US_CVDuty", cvduty, invoiceLine.US_CVDuty);
				AssertEquals("US_CustomsValue", customsValue, invoiceLine.US_CustomsValue);
				AssertEquals("US_DDTCQuantity", ddtcQuantity, invoiceLine.US_DDTCQuantity);
				AssertEquals("US_Duty", duty, invoiceLine.US_Duty);
				AssertEquals("US_SupDuty", supDuty, invoiceLine.US_SupDuty);
				AssertEquals("US_SupQty1", supQty1, invoiceLine.US_SupQty1);
				AssertEquals("US_SupQty2", supQty2, invoiceLine.US_SupQty2);
				AssertEquals("US_SupQty3", supQty3, invoiceLine.US_SupQty3);
				AssertEquals("US_VisaQty", visaQty, invoiceLine.US_VisaQty);
			});
		}

		WhsDataTestHelper helper;
		WhsDataTestHelper Helper => helper ?? (helper = new WhsDataTestHelper(Factory));
	}
}
