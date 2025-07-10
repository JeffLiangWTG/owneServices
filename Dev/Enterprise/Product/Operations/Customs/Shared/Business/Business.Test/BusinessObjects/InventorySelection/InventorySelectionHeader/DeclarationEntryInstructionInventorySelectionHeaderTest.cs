using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(DeclarationEntryInstructionInventorySelectionHeader))]
	[CountrySpecificTest(Core.Constants.CountryCodes.Namibia)]
	[AsycudaCustomsCountries(Core.Constants.CountryCodes.Namibia)]
	sealed class DeclarationEntryInstructionInventorySelectionHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUpdateSelectionLinesDetails()
		{
			var whsReceive = (IWhsReceive)publishToUniversalResult.FindJobIfExists();
			AssertNotNull("whsReceive", whsReceive);
			var query = new ZQuery(WhsDocketLineSchema.WE_WD, whsReceive.PK);
			query.AddToFilter(WhsDocketLineSchema.WE_OP, Helper.Part.PK);
			var whsReceiveLine1 = Factory.LoadTop1<IWhsReceiveLine>(query);
			AssertNotNull("whsReceiveLine1", whsReceiveLine1);
			var whsInventory1 = whsReceiveLine1.Inventory;
			query = new ZQuery(WhsDocketLineSchema.WE_WD, whsReceive.PK);
			query.AddToFilter(WhsDocketLineSchema.WE_OP, Helper.Part2.PK);
			var whsReceiveLine2 = Factory.LoadTop1<IWhsReceiveLine>(query);
			AssertNotNull("whsReceiveLine2", whsReceiveLine2);
			var whsInventory2 = whsReceiveLine2.Inventory;

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_OH_Importer = Helper.Importer.PK;
			declaration.JE_MessageType = "EXP";
			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = Helper.OutwardCusProcedure.ZZ6_ProcedureCode;
			entryInstruction1.CEI_Description = "d";
			var entryInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = Helper.OutwardCusProcedure.ZZ6_ProcedureCode;
			entryInstruction2.CEI_Description = "C";
			var entryInstruction3 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction3.CEI_Style = "ZZ";
			entryInstruction3.CEI_Description = "C";
			var header = new DeclarationEntryInstructionInventorySelectionHeader(declaration);
			Factory.Save();

			header.IsGroupByInventory = true;
			AssertEquals(0, header.SelectionLines.Count);
			header.UpdateSelectionLinesDetails(new[] { whsInventory2 });
			AssertEquals(1, header.SelectionLines.Count);
			var line = header.SelectionLines[0];
			line.US_ProductQtyToDraw = 100m;
			header.ImportInventories();
			AssertEquals("entryInstruction1.CEI_OA_Warehouse", ZGuid.Empty, entryInstruction1.CEI_OA_Warehouse);
			AssertEquals("entryInstruction2.CEI_OA_Warehouse", Helper.Warehouse.MainAddress.PK, entryInstruction2.CEI_OA_Warehouse);
			AssertEquals("entryInstruction3.CEI_OA_Warehouse", ZGuid.Empty, entryInstruction3.CEI_OA_Warehouse);
			AssertEquals(1, declaration.Invoices.Count);
			var invoice = declaration.Invoices[0];
			AssertEquals("JZ_InvoiceAmount", 10000m, invoice.JZ_InvoiceAmount);
			AssertEquals("JZ_RX_NKInvoice_Currency", Constants.CurrencyCodes.Namibia, invoice.JZ_RX_NKInvoice_Currency);
			AssertEquals("JZ_IncoTerm", "FOB", invoice.JZ_IncoTerm);
			AssertEquals(1, declaration.InvoiceLines.Count);
			var invoiceLine = declaration.InvoiceLines[0];
			Helper.AssertInvoiceLine(invoiceLine, entryInstruction2.PK, 100m, "BX", 100m, "BX", 0, "", 0, "", 10000m, "");
			Factory.Save();

			header.ImportInventories();
			AssertEquals("entryInstruction1.CEI_OA_Warehouse", ZGuid.Empty, entryInstruction1.CEI_OA_Warehouse);
			AssertEquals("entryInstruction2.CEI_OA_Warehouse", Helper.Warehouse.MainAddress.PK, entryInstruction2.CEI_OA_Warehouse);
			AssertEquals("entryInstruction3.CEI_OA_Warehouse", ZGuid.Empty, entryInstruction3.CEI_OA_Warehouse);
			AssertEquals(1, declaration.Invoices.Count);
			AssertEquals(invoice, declaration.Invoices[0]);
			AssertEquals("JZ_InvoiceAmount", 20000m, invoice.JZ_InvoiceAmount);
			AssertEquals("JZ_RX_NKInvoice_Currency", Constants.CurrencyCodes.Namibia, invoice.JZ_RX_NKInvoice_Currency);
			AssertEquals(2, declaration.InvoiceLines.Count);
			AssertEquals(invoiceLine, declaration.InvoiceLines[0]);
			Helper.AssertInvoiceLine(invoiceLine, entryInstruction2.PK, 100m, "BX", 100m, "BX", 0, "", 0, "", 10000m, "");
			var invoiceLine2 = declaration.InvoiceLines[1];
			Helper.AssertInvoiceLine(invoiceLine, entryInstruction2.PK, 100m, "BX", 100m, "BX", 0, "", 0, "", 10000m, "");

			entryInstruction1.Delete();
			entryInstruction2.Delete();
			entryInstruction3.Delete();
			declaration.InvoiceLines.RemoveAndDeleteAll();
			entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = Helper.ChangeOfOwnershipCusProcedure.ZZ6_ProcedureCode;
			entryInstruction1.CEI_Description = "d";
			entryInstruction1.CEI_OH_Owner = Helper.Owner.PK;
			AssertEquals(Helper.Part.OP_PartNum, Helper.OwnerPart.OP_PartNum);
			AssertEquals(Helper.Part2.OP_PartNum, Helper.OwnerPart2.OP_PartNum);
			Factory.Save();

			header = new DeclarationEntryInstructionInventorySelectionHeader(declaration);
			header.IsGroupByInventory = true;
			header.UpdateSelectionLinesDetails(new[] { whsInventory1, whsInventory2 });
			AssertEquals(2, header.SelectionLines.Count);
			var line1 = header.SelectionLines[0];
			var line2 = header.SelectionLines[1];
			if (line2.US_Product == Helper.Part.OP_PartNum)
			{
				line2 = header.SelectionLines[0];
				line1 = header.SelectionLines[1];
			}
			AssertEquals(Helper.Part.OP_PartNum, line1.US_Product);
			line1.US_ProductQtyToDraw = 50m;
			AssertEquals(Helper.Part2.OP_PartNum, line2.US_Product);
			line2.US_ProductQtyToDraw = 100m;
			CombineAssertions(() =>
			{
				header.ImportInventories();
				AssertEquals("entryInstruction1.CEI_OA_Warehouse", Helper.Warehouse.MainAddress.PK, entryInstruction1.CEI_OA_Warehouse);
				AssertEquals(1, declaration.Invoices.Count);
				AssertEquals(invoice, declaration.Invoices[0]);
				AssertEquals("JZ_InvoiceAmount", 15000m, invoice.JZ_InvoiceAmount);
				AssertEquals("JZ_RX_NKInvoice_Currency", Constants.CurrencyCodes.Namibia, invoice.JZ_RX_NKInvoice_Currency);
				AssertEquals(2, declaration.InvoiceLines.Count);
				invoiceLine = declaration.InvoiceLines[0];
				invoiceLine2 = declaration.InvoiceLines[1];
				if (invoiceLine2.JI_PartNo == Helper.Part.OP_PartNum)
				{
					invoiceLine2 = declaration.InvoiceLines[0];
					invoiceLine = declaration.InvoiceLines[1];
				}
				Helper.AssertInvoiceLine(invoiceLine, entryInstruction1.PK, 50m, "NO", 50m, "NO", 0, "", 0, "", 5000m, "");
				Helper.AssertInvoiceLine(invoiceLine2, entryInstruction1.PK, 100m, "BX", 100m, "BX", 0, "", 0, "", 10000m, "");
			});
		}

		public void TestUpdateOutwardLinesWithInventoryDetailsByEntryInstruction()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = "EXP";
			declaration.JE_OH_Supplier = inwardDeclaration.JE_OH_Importer;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = Helper.OutwardCusProcedure.ZZ6_Group;
			entryInstruction.CEI_Description = "OUT DESC";
			entryInstruction.CEI_OA_Warehouse = inwardEntry.EntryInstruction.CEI_OA_Warehouse2;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_OP = Helper.Part.PK;
			invoiceLine1.JI_BondedWhsQuantity = 50m;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_OP = Helper.Part2.PK;
			invoiceLine2.JI_BondedWhsQuantity = 100m;

			new DeclarationEntryInstructionInventorySelectionHeader(entryInstruction).UpdateOutwardLinesWithInventoryDetails(entryInstruction.InvoiceLines);
			Helper.AssertInvoiceLine(invoiceLine1, entryInstruction.PK, 50m, "NO", 50m, "NO", 0m, "", 0, "", 5000m, "");
			Helper.AssertInvoiceLine(invoiceLine2, entryInstruction.PK, 100m, "BX", 100m, "BX", 0m, "", 0, "", 10000m, "");

			var part3 = Helper.CreateProduct(Helper.Owner.PK, "~~3");
			part3.OP_Desc = "~~3 DESC";
			invoiceLine1.JI_OP = part3.PK;
			new DeclarationEntryInstructionInventorySelectionHeader(entryInstruction).UpdateOutwardLinesWithInventoryDetails(entryInstruction.InvoiceLines);
			Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "No available warehouse inventory can be found for the requested product code"));
		}

		public void TestImportInventories()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var inventorySelectionHeader = new DeclarationEntryInstructionInventorySelectionHeader(declaration);
			Factory.Save();

			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine1 = Helper.GetNewWhsReceiveLine(
				whsReceive.PK,
				Helper.Part.PK,
				"PACKAGE1",
				100m,
				900m,
				900m,
				bondedEntryKey: "EN00123-1",
				arrivalDate: ZDateTime.BrettsBirthday);
			var whsInventory1 = whsReceiveLine1.Inventory;
			var newSelectedLine = new WhsInventoryWrapper(whsInventory1, inventorySelectionHeader);
			inventorySelectionHeader.SelectedLines.Add(newSelectedLine);
			Factory.Save();

			var whsBondedWarehouseAttribute = Factory.New<IWhsBondedWarehouseAttribute>();
			whsBondedWarehouseAttribute.WB_PrimaryPreference = "STADARD";
			whsBondedWarehouseAttribute.WB_ParentID = newSelectedLine.PK;
			Factory.Save();

			inventorySelectionHeader.ImportInventories();

			var importedGroupHeaders = declaration.JobComInvoiceGroupHeaders;
			AssertEquals(1, importedGroupHeaders.Count);

			var importedHeaders = importedGroupHeaders[0].JobComInvoiceHeaders;
			AssertEquals(1, importedHeaders.Count);

			var importedLines = importedHeaders[0].JobComInvoiceLines;
			AssertEquals(1, importedLines.Count);
		}

		protected override void SetUp()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				base.SetUp();

				DataRegistry.Business.CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
				Helper.UniversalTariffHelper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "CustomsUQ", Core.Constants.CountryCodes.Namibia);
				Helper.UniversalTariffHelper.CreateCusCodeList(Core.Constants.CountryCodes.Namibia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KG", new ZDateTime(2020, 01, 01), new ZDateTime(2079, 01, 01));
				Helper.SetTariffAndSave(Helper.Part, "KG");
				Helper.SetTariffAndSave(Helper.Part2, "KG");
				inwardEntry = Helper.GetNewEntryHeader("IMP", "B00001230", Helper.InwardCusProcedure.ZZ6_ProcedureCode, "ENT3243", Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m, Helper.InwardCusProcedure.ZZ6_Group, Core.Constants.CurrencyCodes.Namibia);
				inwardEntry.CH_BGMReference = "BOB";
				inwardDeclaration = inwardEntry.Declaration;
				var inwardInvoiceLine2 = Helper.AddInvoiceLine(inwardDeclaration.Invoices[0], Helper.Part2, 200m, inwardEntry.CH_CEI_Instruction, Helper.InwardCusProcedure.ZZ6_ProcedureCode, Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, "", 1);
				inwardInvoiceLine2.JI_BondedWhsUnitQty = "BX";
				inwardInvoiceLine2.InvoiceHeader.JZ_InvoiceAmount += inwardInvoiceLine2.JI_LinePrice;
				inwardDeclaration.DoMerge();
				Factory.Save();
				publishToUniversalResult = Enterprise.Customs.Business.WarehouseExtensions.JobDeclarationWarehouseExtensions.PublishShipmentForWHSInward(inwardEntry, false);
				Enterprise.Customs.Business.WarehouseExtensions.JobDeclarationWarehouseExtensions.PublishAcceptEventForWHSInwardAndSaveIfNeeded(inwardEntry, true);
				var bondedEntryKey1 = "ENT3243-1";
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey1, 100m);
				var bondedEntryKey2 = "ENT3243-2";
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 200m);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			return new DeclarationEntryInstructionInventorySelectionHeader(entryInstruction);
		}

		WhsDataTestHelper Helper => helper ?? (helper = new WhsDataTestHelper(Factory));
		WhsDataTestHelper helper;

		BaseJobDeclaration inwardDeclaration;
		CusEntryHeader inwardEntry;
		PublishToUniversalResult publishToUniversalResult;
	}
}
