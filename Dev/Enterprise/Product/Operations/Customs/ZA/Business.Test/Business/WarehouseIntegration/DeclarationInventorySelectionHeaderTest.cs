using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(DeclarationInventorySelectionHeader))]
	sealed class DeclarationInventorySelectionHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUpdateSelectionLinesDetails()
		{
			using (Helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				Helper.SetTariffAndSave(Helper.Part, "KG", "CF", "BAG");
				Helper.SetTariffAndSave(Helper.Part2, "KG", "CF", "BAG");
				var inwardEntry = Helper.GetNewEntryHeader(Common.ZA.ZAJobMessageTypeList.Codes.Import, "BZA00001230", Helper.InwardCusProcedure.ZZ6_ProcedureCode, "ENT3243", Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
				inwardEntry.CH_BGMReference = "BOB";
				var inwardDeclaration = inwardEntry.Declaration;
				var inwardInvoiceLine2 = inwardDeclaration.InvoiceLines.AddNew();
				inwardInvoiceLine2.JI_CEI = inwardEntry.CH_CEI_Instruction;
				inwardInvoiceLine2.JI_PartNo = Helper.Part2.OP_PartNum;
				inwardInvoiceLine2.JI_Procedure = Helper.InwardCusProcedure.ZZ6_ProcedureCode + Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
				inwardInvoiceLine2.JI_InvoiceQuantity = 400m;
				inwardInvoiceLine2.JI_InvoiceUQ = "NO";
				inwardInvoiceLine2.JI_BondedWhsQuantity = 200m;
				inwardInvoiceLine2.JI_BondedWhsUnitQty = "BX";
				inwardInvoiceLine2.JI_CustomsQuantity = 20m;
				inwardInvoiceLine2.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine2.JI_LinePrice = 5000m;
				inwardInvoiceLine2.JI_EngineNumber = "EGN12";
				inwardInvoiceLine2.JI_PrimaryPreference = "EU";
				inwardInvoiceLine2.JI_ROOCert = "ROO32342";
				inwardInvoiceLine2.JI_VIN = "VIN4353";
				inwardInvoiceLine2.InvoiceHeader.JZ_InvoiceAmount += 5000m;
				inwardInvoiceLine2.JI_NewUsed = "S";
				inwardInvoiceLine2.JI_CustomsThirdQuantity = 4m;
				inwardInvoiceLine2.JI_CustomsThirdUnitQty = "BAG";
				inwardDeclaration.DoMerge();
				Factory.Save();
				var result = inwardEntry.PublishShipmentForWHSInward(false);
				inwardEntry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsReceive = (IWhsReceive)result.FindJobIfExists();
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
				var bondedEntryKey1 = "ENT3243-1";
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey1, 100m);
				var bondedEntryKey2 = "ENT3243-2";
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 200m);
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_OH_Importer = Helper.Importer.PK;
				declaration.JE_MessageType = Common.ZA.ZAJobMessageTypeList.Codes.ExBond;
				var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction1.CEI_Style = Helper.OutwardCusProcedure.ZZ6_ProcedureCode;
				entryInstruction1.CEI_Description = "d";
				var entryInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_Style = Helper.OutwardCusProcedure.ZZ6_ProcedureCode;
				entryInstruction2.CEI_Description = "C";
				var entryInstruction3 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction3.CEI_Style = "ZZ";
				entryInstruction3.CEI_Description = "C";
				var header = new DeclarationInventorySelectionHeader(declaration);
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
				AssertEquals("JZ_InvoiceAmount", 2500m, invoice.JZ_InvoiceAmount);
				AssertEquals("JZ_RX_NKInvoice_Currency", Constants.CurrencyCodes.SouthAfrica, invoice.JZ_RX_NKInvoice_Currency);
				AssertEquals("JZ_IncoTerm", "FOB", invoice.JZ_IncoTerm);
				AssertEquals(1, declaration.InvoiceLines.Count);
				var invoiceLine = declaration.InvoiceLines[0];
				AssertInvoiceLine(invoiceLine, entryInstruction2.PK, 100m, "BX", 100m, "BX", 10m, "KG", 2m, "BAG", 2500m, "EGN12", "VIN4353", "ROO32342", "S", Helper.InwardCusProcedure.ZZ6_ProcedureCode);
				Factory.Save();
				header.ImportInventories();
				AssertEquals("entryInstruction1.CEI_OA_Warehouse", ZGuid.Empty, entryInstruction1.CEI_OA_Warehouse);
				AssertEquals("entryInstruction2.CEI_OA_Warehouse", Helper.Warehouse.MainAddress.PK, entryInstruction2.CEI_OA_Warehouse);
				AssertEquals("entryInstruction3.CEI_OA_Warehouse", ZGuid.Empty, entryInstruction3.CEI_OA_Warehouse);
				AssertEquals(1, declaration.Invoices.Count);
				AssertEquals(invoice, declaration.Invoices[0]);
				AssertEquals("JZ_InvoiceAmount", 5000m, invoice.JZ_InvoiceAmount);
				AssertEquals("JZ_RX_NKInvoice_Currency", Constants.CurrencyCodes.SouthAfrica, invoice.JZ_RX_NKInvoice_Currency);
				AssertEquals(2, declaration.InvoiceLines.Count);
				AssertEquals(invoiceLine, declaration.InvoiceLines[0]);
				AssertInvoiceLine(invoiceLine, entryInstruction2.PK, 100m, "BX", 100m, "BX", 10m, "KG", 2m, "BAG", 2500m, "EGN12", "VIN4353", "ROO32342", "S", Helper.InwardCusProcedure.ZZ6_ProcedureCode);
				var invoiceLine2 = declaration.InvoiceLines[1];
				AssertInvoiceLine(invoiceLine2, entryInstruction2.PK, 100m, "BX", 100m, "BX", 10m, "KG", 2m, "BAG", 2500m, "EGN12", "VIN4353", "ROO32342", "S", Helper.InwardCusProcedure.ZZ6_ProcedureCode);
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
				header = new DeclarationInventorySelectionHeader(declaration);
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
					AssertEquals("entryInstruction1.CEI_OA_Warehouse2", Helper.Warehouse.MainAddress.PK, entryInstruction1.CEI_OA_Warehouse2);
					AssertEquals(1, declaration.Invoices.Count);
					AssertEquals(invoice, declaration.Invoices[0]);
					AssertEquals("JZ_InvoiceAmount", 7500m, invoice.JZ_InvoiceAmount);
					AssertEquals("JZ_RX_NKInvoice_Currency", Constants.CurrencyCodes.SouthAfrica, invoice.JZ_RX_NKInvoice_Currency);
					AssertEquals(2, declaration.InvoiceLines.Count);
					invoiceLine = declaration.InvoiceLines[0];
					invoiceLine2 = declaration.InvoiceLines[1];
					if (invoiceLine2.JI_PartNo == Helper.Part.OP_PartNum)
					{
						invoiceLine2 = declaration.InvoiceLines[0];
						invoiceLine = declaration.InvoiceLines[1];
					}

					AssertInvoiceLine(invoiceLine, entryInstruction1.PK, 50m, "NO", 50m, "NO", 500m, "KG", 0m, "BAG", 5000m, "", "", "", "N", Helper.InwardCusProcedure.ZZ6_ProcedureCode);
					AssertEquals("invoiceLine.JI_NewOwnerPartNo", Helper.OwnerPart.OP_PartNum, invoiceLine.JI_NewOwnerPartNo);
					AssertEquals("invoiceLine.JI_OP_NewOwnerProduct", Helper.OwnerPart.PK, invoiceLine.JI_OP_NewOwnerProduct);
					AssertInvoiceLine(invoiceLine2, entryInstruction1.PK, 100m, "BX", 100m, "BX", 10m, "KG", 2m, "BAG", 2500m, "EGN12", "VIN4353", "ROO32342", "S", Helper.InwardCusProcedure.ZZ6_ProcedureCode);
					AssertEquals("invoiceLine2.JI_NewOwnerPartNo", Helper.OwnerPart2.OP_PartNum, invoiceLine2.JI_NewOwnerPartNo);
					AssertEquals("invoiceLine2.JI_OP_NewOwnerProduct", Helper.OwnerPart2.PK, invoiceLine2.JI_OP_NewOwnerProduct);
				});
			}
		}

		public void TestUpdateOutwardLinesWithInventoryDetails()
		{
			using (Helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				Helper.SetTariffAndSave(Helper.Part, "KG");
				Helper.SetTariffAndSave(Helper.Part2, "KG");
				var inwardEntry = Helper.GetNewEntryHeader(Common.ZA.ZAJobMessageTypeList.Codes.Import, "BZA00001230", Helper.InwardCusProcedure.ZZ6_ProcedureCode, "ENT3243", Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
				inwardEntry.CH_BGMReference = "BOB";
				var inwardDeclaration = inwardEntry.Declaration;
				var inwardInvoiceLine2 = inwardDeclaration.InvoiceLines.AddNew();
				inwardInvoiceLine2.JI_CEI = inwardEntry.CH_CEI_Instruction;
				inwardInvoiceLine2.JI_PartNo = Helper.Part2.OP_PartNum;
				inwardInvoiceLine2.JI_Procedure = Helper.InwardCusProcedure.ZZ6_ProcedureCode + Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
				inwardInvoiceLine2.JI_InvoiceQuantity = 400m;
				inwardInvoiceLine2.JI_InvoiceUQ = "NO";
				inwardInvoiceLine2.JI_BondedWhsQuantity = 200m;
				inwardInvoiceLine2.JI_BondedWhsUnitQty = "BX";
				inwardInvoiceLine2.JI_CustomsQuantity = 20m;
				inwardInvoiceLine2.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine2.JI_LinePrice = 5000m;
				inwardInvoiceLine2.JI_EngineNumber = "EGN12";
				inwardInvoiceLine2.JI_PrimaryPreference = "EU";
				inwardInvoiceLine2.JI_ROOCert = "ROO32342";
				inwardInvoiceLine2.JI_VIN = "VIN4353";
				inwardInvoiceLine2.JI_NewUsed = "S";
				inwardInvoiceLine2.JI_CustomsThirdQuantity = 4m;
				inwardInvoiceLine2.JI_CustomsThirdUnitQty = "BAG";
				inwardInvoiceLine2.InvoiceHeader.JZ_InvoiceAmount += 5000m;
				inwardDeclaration.DoMerge();
				Factory.Save();
				inwardEntry.PublishShipmentForWHSInward(false);
				inwardEntry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				var bondedEntryKey1 = "ENT3243-1";
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey1, 100m);
				var bondedEntryKey2 = "ENT3243-2";
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 200m);
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_MessageType = Common.ZA.ZAJobMessageTypeList.Codes.ExBond;
				declaration.JE_OH_Importer = inwardDeclaration.JE_OH_Importer;
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = Helper.OutwardCusProcedure.ZZ6_ProcedureCode;
				entryInstruction.CEI_Description = "OUT DESC";
				entryInstruction.CEI_OA_Warehouse = inwardEntry.EntryInstruction.CEI_OA_Warehouse2;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;
				invoiceLine1.JI_PartNo = Helper.Part.OP_PartNum;
				invoiceLine1.JI_BondedWhsQuantity = 50m;
				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction.PK;
				invoiceLine2.JI_PartNo = Helper.Part2.OP_PartNum;
				invoiceLine2.JI_BondedWhsQuantity = 100m;
				invoiceLine2.JI_CommissionNumber = "CM32";
				new DeclarationInventorySelectionHeader(entryInstruction).UpdateOutwardLinesWithInventoryDetails(entryInstruction.InvoiceLines);
				AssertInvoiceLine(invoiceLine1, entryInstruction.PK, 50m, "NO", 50m, "NO", 500m, "KG", 0m, "", 5000m, "", "", "", "N", Helper.InwardCusProcedure.ZZ6_ProcedureCode);
				AssertInvoiceLine(invoiceLine2, entryInstruction.PK, 100m, "BX", 100m, "BX", 10m, "KG", 0m, "", 2500m, "EGN12", "VIN4353", "ROO32342", "S", Helper.InwardCusProcedure.ZZ6_ProcedureCode);
				AssertEquals("invoiceLine2.JI_CommissionNumber", "CM32", invoiceLine2.JI_CommissionNumber);
			}
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_MRNAndMRLExistsAsAPair()
		{
			using (Helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				var inwardEntry = Helper.GetNewEntryHeader(Common.ZA.ZAJobMessageTypeList.Codes.Import, "BZA00001230", Helper.InwardCusProcedure.ZZ6_ProcedureCode, "ENT3243", Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
				inwardEntry.CH_BGMReference = "BOB";
				var inwardDeclaration = inwardEntry.Declaration;
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_MessageType = Common.ZA.ZAJobMessageTypeList.Codes.ExBond;
				declaration.JE_OH_Importer = inwardDeclaration.JE_OH_Importer;
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = Helper.OutwardCusProcedure.ZZ6_ProcedureCode;
				entryInstruction.CEI_Description = "OUT DESC";
				entryInstruction.CEI_OA_Warehouse = inwardEntry.EntryInstruction.CEI_OA_Warehouse2;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;
				invoiceLine1.JI_PartNo = Helper.Part.OP_PartNum;
				invoiceLine1.JI_BondedWhsQuantity = 50m;
				invoiceLine1.JI_PreviousEntryNumber = "123";
				invoiceLine1.JI_PreviousEntryLineNumber = 0;
				new DeclarationInventorySelectionHeader(entryInstruction).UpdateOutwardLinesWithInventoryDetails(entryInstruction.InvoiceLines);
				Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "MRL must be supplied if the MRN has been specified"));
				invoiceLine1.JI_PreviousEntryNumber = "";
				invoiceLine1.JI_PreviousEntryLineNumber = 1;
				new DeclarationInventorySelectionHeader(entryInstruction).UpdateOutwardLinesWithInventoryDetails(entryInstruction.InvoiceLines);
				Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "MRN must be supplied if the MRL has been specified"));
			}
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_MRNAndMRLAndProductCodeCombination()
		{
			using (Helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				var part3 = Helper.CreateProduct(Helper.Owner.PK, "~~3");
				part3.OP_Desc = "~~3 DESC";
				Helper.SetTariffAndSave(Helper.Part, "KG");
				Helper.SetTariffAndSave(Helper.Part2, "KG");
				var inwardEntry = Helper.GetNewEntryHeader(Common.ZA.ZAJobMessageTypeList.Codes.Import, "BZA00001230", Helper.InwardCusProcedure.ZZ6_ProcedureCode, "ENT3243", Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
				inwardEntry.CH_BGMReference = "BOB";
				var inwardDeclaration = inwardEntry.Declaration;
				var inwardInvoiceLine2 = inwardDeclaration.InvoiceLines.AddNew();
				inwardInvoiceLine2.JI_CEI = inwardEntry.CH_CEI_Instruction;
				inwardInvoiceLine2.JI_PartNo = Helper.Part2.OP_PartNum;
				inwardInvoiceLine2.JI_Procedure = Helper.InwardCusProcedure.ZZ6_ProcedureCode + Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
				inwardInvoiceLine2.JI_InvoiceQuantity = 400m;
				inwardInvoiceLine2.JI_InvoiceUQ = "NO";
				inwardInvoiceLine2.JI_BondedWhsQuantity = 200m;
				inwardInvoiceLine2.JI_BondedWhsUnitQty = "BX";
				inwardInvoiceLine2.JI_CustomsQuantity = 20m;
				inwardInvoiceLine2.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine2.JI_LinePrice = 5000m;
				inwardInvoiceLine2.JI_EngineNumber = "EGN12";
				inwardInvoiceLine2.JI_PrimaryPreference = "EU";
				inwardInvoiceLine2.JI_ROOCert = "ROO32342";
				inwardInvoiceLine2.JI_VIN = "VIN4353";
				inwardInvoiceLine2.JI_NewUsed = "S";
				inwardInvoiceLine2.JI_CustomsThirdQuantity = 4m;
				inwardInvoiceLine2.JI_CustomsThirdUnitQty = "BAG";
				inwardInvoiceLine2.InvoiceHeader.JZ_InvoiceAmount += 5000m;
				inwardDeclaration.DoMerge();
				Factory.Save();
				inwardEntry.PublishShipmentForWHSInward(false);
				inwardEntry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				var bondedEntryKey1 = "ENT3243-1";
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey1, 100m);
				var bondedEntryKey2 = "ENT3243-2";
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 200m);
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_MessageType = Common.ZA.ZAJobMessageTypeList.Codes.ExBond;
				declaration.JE_OH_Importer = inwardDeclaration.JE_OH_Importer;
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = Helper.OutwardCusProcedure.ZZ6_ProcedureCode;
				entryInstruction.CEI_Description = "OUT DESC";
				entryInstruction.CEI_OA_Warehouse = inwardEntry.EntryInstruction.CEI_OA_Warehouse2;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;
				invoiceLine1.JI_OP = part3.PK;
				invoiceLine1.JI_PartNo = part3.OP_PartNum;
				invoiceLine1.JI_BondedWhsQuantity = 50m;
				invoiceLine1.JI_PreviousEntryNumber = "ENT3333";
				invoiceLine1.JI_PreviousEntryLineNumber = 3;
				new DeclarationInventorySelectionHeader(entryInstruction).UpdateOutwardLinesWithInventoryDetails(entryInstruction.InvoiceLines);
				Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "MRN, MRL and Product Code combination cannot be matched in the Warehouse Inventory"));
				invoiceLine1.JI_PreviousEntryNumber = "ENT3243";
				invoiceLine1.JI_PreviousEntryLineNumber = 1;
				invoiceLine1.JI_OP = Helper.Part.PK;
				invoiceLine1.JI_PartNo = Helper.Part.OP_PartNum;
				invoiceLine1.ClearRowNotifications();
				new DeclarationInventorySelectionHeader(entryInstruction).UpdateOutwardLinesWithInventoryDetails(entryInstruction.InvoiceLines);
				Assert(invoiceLine1.RowMessageErrors.All(x => x.Message != "MRN, MRL and Product Code combination cannot be matched in the Warehouse Inventory"));
			}
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_MRNAndMRLCombination()
		{
			using (Helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				Helper.SetTariffAndSave(Helper.Part, "KG");
				Helper.SetTariffAndSave(Helper.Part2, "KG");
				var inwardEntry = Helper.GetNewEntryHeader(Common.ZA.ZAJobMessageTypeList.Codes.Import, "BZA00001230", Helper.InwardCusProcedure.ZZ6_ProcedureCode, "ENT3243", Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
				inwardEntry.CH_BGMReference = "BOB";
				var inwardDeclaration = inwardEntry.Declaration;
				var inwardInvoiceLine2 = inwardDeclaration.InvoiceLines.AddNew();
				inwardInvoiceLine2.JI_CEI = inwardEntry.CH_CEI_Instruction;
				inwardInvoiceLine2.JI_PartNo = Helper.Part2.OP_PartNum;
				inwardInvoiceLine2.JI_Procedure = Helper.InwardCusProcedure.ZZ6_ProcedureCode + Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
				inwardInvoiceLine2.JI_InvoiceQuantity = 400m;
				inwardInvoiceLine2.JI_InvoiceUQ = "NO";
				inwardInvoiceLine2.JI_BondedWhsQuantity = 200m;
				inwardInvoiceLine2.JI_BondedWhsUnitQty = "BX";
				inwardInvoiceLine2.JI_CustomsQuantity = 20m;
				inwardInvoiceLine2.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine2.JI_LinePrice = 5000m;
				inwardInvoiceLine2.JI_EngineNumber = "EGN12";
				inwardInvoiceLine2.JI_PrimaryPreference = "EU";
				inwardInvoiceLine2.JI_ROOCert = "ROO32342";
				inwardInvoiceLine2.JI_VIN = "VIN4353";
				inwardInvoiceLine2.JI_NewUsed = "S";
				inwardInvoiceLine2.JI_CustomsThirdQuantity = 4m;
				inwardInvoiceLine2.JI_CustomsThirdUnitQty = "BAG";
				inwardInvoiceLine2.InvoiceHeader.JZ_InvoiceAmount += 5000m;
				inwardDeclaration.DoMerge();
				Factory.Save();
				inwardEntry.PublishShipmentForWHSInward(false);
				inwardEntry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				var bondedEntryKey1 = "ENT3243-1";
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey1, 100m);
				var bondedEntryKey2 = "ENT3243-2";
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 200m);
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_MessageType = Common.ZA.ZAJobMessageTypeList.Codes.ExBond;
				declaration.JE_OH_Importer = inwardDeclaration.JE_OH_Importer;
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = Helper.OutwardCusProcedure.ZZ6_ProcedureCode;
				entryInstruction.CEI_Description = "OUT DESC";
				entryInstruction.CEI_OA_Warehouse = inwardEntry.EntryInstruction.CEI_OA_Warehouse2;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;
				invoiceLine1.JI_BondedWhsQuantity = 50m;
				invoiceLine1.JI_PreviousEntryNumber = "ENT3333";
				invoiceLine1.JI_PreviousEntryLineNumber = 3;
				new DeclarationInventorySelectionHeader(entryInstruction).UpdateOutwardLinesWithInventoryDetails(entryInstruction.InvoiceLines);
				Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "MRN, MRL combination cannot be matched in the Warehouse Inventory"));
				invoiceLine1.JI_PreviousEntryNumber = "ENT3243";
				invoiceLine1.JI_PreviousEntryLineNumber = 1;
				invoiceLine1.ClearRowNotifications();
				new DeclarationInventorySelectionHeader(entryInstruction).UpdateOutwardLinesWithInventoryDetails(entryInstruction.InvoiceLines);
				Assert(invoiceLine1.RowMessageErrors.All(x => x.Message != "MRN, MRL combination cannot be matched in the Warehouse Inventory"));
				AssertEquals(Helper.Part.PK, invoiceLine1.JI_OP);
				AssertEquals("~~1", invoiceLine1.JI_PartNo);
			}
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_ProductCodeAndVINCombination()
		{
			using (Helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				var part3 = Helper.CreateProduct(Helper.Owner.PK, "~~3");
				part3.OP_Desc = "~~3 DESC";
				Helper.SetTariffAndSave(Helper.Part, "KG");
				Helper.SetTariffAndSave(Helper.Part2, "KG");
				var inwardEntry = Helper.GetNewEntryHeader(Common.ZA.ZAJobMessageTypeList.Codes.Import, "BZA00001230", Helper.InwardCusProcedure.ZZ6_ProcedureCode, "ENT3243", Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
				inwardEntry.CH_BGMReference = "BOB";
				var inwardDeclaration = inwardEntry.Declaration;
				var inwardInvoiceLine2 = inwardDeclaration.InvoiceLines.AddNew();
				inwardInvoiceLine2.JI_CEI = inwardEntry.CH_CEI_Instruction;
				inwardInvoiceLine2.JI_PartNo = Helper.Part2.OP_PartNum;
				inwardInvoiceLine2.JI_Procedure = Helper.InwardCusProcedure.ZZ6_ProcedureCode + Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
				inwardInvoiceLine2.JI_InvoiceQuantity = 400m;
				inwardInvoiceLine2.JI_InvoiceUQ = "NO";
				inwardInvoiceLine2.JI_BondedWhsQuantity = 200m;
				inwardInvoiceLine2.JI_BondedWhsUnitQty = "BX";
				inwardInvoiceLine2.JI_CustomsQuantity = 20m;
				inwardInvoiceLine2.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine2.JI_LinePrice = 5000m;
				inwardInvoiceLine2.JI_EngineNumber = "EGN12";
				inwardInvoiceLine2.JI_PrimaryPreference = "EU";
				inwardInvoiceLine2.JI_ROOCert = "ROO32342";
				inwardInvoiceLine2.JI_VIN = "VIN4353";
				inwardInvoiceLine2.JI_NewUsed = "S";
				inwardInvoiceLine2.JI_CustomsThirdQuantity = 4m;
				inwardInvoiceLine2.JI_CustomsThirdUnitQty = "BAG";
				inwardInvoiceLine2.InvoiceHeader.JZ_InvoiceAmount += 5000m;
				inwardDeclaration.DoMerge();
				Factory.Save();
				inwardEntry.PublishShipmentForWHSInward(false);
				inwardEntry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				var bondedEntryKey1 = "ENT3243-1";
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey1, 100m);
				var bondedEntryKey2 = "ENT3243-2";
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 200m);
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_MessageType = Common.ZA.ZAJobMessageTypeList.Codes.ExBond;
				declaration.JE_OH_Importer = inwardDeclaration.JE_OH_Importer;
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = Helper.OutwardCusProcedure.ZZ6_ProcedureCode;
				entryInstruction.CEI_Description = "OUT DESC";
				entryInstruction.CEI_OA_Warehouse = inwardEntry.EntryInstruction.CEI_OA_Warehouse2;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;
				invoiceLine1.JI_BondedWhsQuantity = 50m;
				invoiceLine1.JI_PreviousEntryNumber = "";
				invoiceLine1.JI_PreviousEntryLineNumber = 0;
				invoiceLine1.JI_VIN = "VIN4444";
				invoiceLine1.JI_OP = part3.PK;
				new DeclarationInventorySelectionHeader(entryInstruction).UpdateOutwardLinesWithInventoryDetails(entryInstruction.InvoiceLines);
				Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "Product Code and VIN combination cannot be matched in the Warehouse Inventory"));
				invoiceLine1.JI_VIN = "VIN4353";
				invoiceLine1.JI_OP = Helper.Part2.PK;
				invoiceLine1.ClearRowNotifications();
				new DeclarationInventorySelectionHeader(entryInstruction).UpdateOutwardLinesWithInventoryDetails(entryInstruction.InvoiceLines);
				Assert(invoiceLine1.RowMessageErrors.All(x => x.Message != "Product Code and VIN combination cannot be matched in the Warehouse Inventory"));
				AssertEquals("ENT3243", invoiceLine1.JI_PreviousEntryNumber);
				AssertEquals((ZShort)2, invoiceLine1.JI_PreviousEntryLineNumber);
			}
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_VIN()
		{
			using (Helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				Helper.SetTariffAndSave(Helper.Part, "KG");
				Helper.SetTariffAndSave(Helper.Part2, "KG");
				var inwardEntry = Helper.GetNewEntryHeader(Common.ZA.ZAJobMessageTypeList.Codes.Import, "BZA00001230", Helper.InwardCusProcedure.ZZ6_ProcedureCode, "ENT3243", Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
				inwardEntry.CH_BGMReference = "BOB";
				var inwardDeclaration = inwardEntry.Declaration;
				var inwardInvoiceLine2 = inwardDeclaration.InvoiceLines.AddNew();
				inwardInvoiceLine2.JI_CEI = inwardEntry.CH_CEI_Instruction;
				inwardInvoiceLine2.JI_PartNo = Helper.Part2.OP_PartNum;
				inwardInvoiceLine2.JI_Procedure = Helper.InwardCusProcedure.ZZ6_ProcedureCode + Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
				inwardInvoiceLine2.JI_InvoiceQuantity = 400m;
				inwardInvoiceLine2.JI_InvoiceUQ = "NO";
				inwardInvoiceLine2.JI_BondedWhsQuantity = 200m;
				inwardInvoiceLine2.JI_BondedWhsUnitQty = "BX";
				inwardInvoiceLine2.JI_CustomsQuantity = 20m;
				inwardInvoiceLine2.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine2.JI_LinePrice = 5000m;
				inwardInvoiceLine2.JI_EngineNumber = "EGN12";
				inwardInvoiceLine2.JI_PrimaryPreference = "EU";
				inwardInvoiceLine2.JI_ROOCert = "ROO32342";
				inwardInvoiceLine2.JI_VIN = "VIN4353";
				inwardInvoiceLine2.JI_NewUsed = "S";
				inwardInvoiceLine2.JI_CustomsThirdQuantity = 4m;
				inwardInvoiceLine2.JI_CustomsThirdUnitQty = "BAG";
				inwardInvoiceLine2.InvoiceHeader.JZ_InvoiceAmount += 5000m;
				inwardDeclaration.DoMerge();
				Factory.Save();
				inwardEntry.PublishShipmentForWHSInward(false);
				inwardEntry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				var bondedEntryKey1 = "ENT3243-1";
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey1, 100m);
				var bondedEntryKey2 = "ENT3243-2";
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 200m);
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_MessageType = Common.ZA.ZAJobMessageTypeList.Codes.ExBond;
				declaration.JE_OH_Importer = inwardDeclaration.JE_OH_Importer;
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = Helper.OutwardCusProcedure.ZZ6_ProcedureCode;
				entryInstruction.CEI_Description = "OUT DESC";
				entryInstruction.CEI_OA_Warehouse = inwardEntry.EntryInstruction.CEI_OA_Warehouse2;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;
				invoiceLine1.JI_BondedWhsQuantity = 50m;
				invoiceLine1.JI_VIN = "VIN4444";
				new DeclarationInventorySelectionHeader(entryInstruction).UpdateOutwardLinesWithInventoryDetails(entryInstruction.InvoiceLines);
				Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "VIN cannot be matched in the Warehouse Inventory"));
				invoiceLine1.JI_VIN = "VIN4353";
				invoiceLine1.ClearRowNotifications();
				new DeclarationInventorySelectionHeader(entryInstruction).UpdateOutwardLinesWithInventoryDetails(entryInstruction.InvoiceLines);
				Assert(invoiceLine1.RowMessageErrors.All(x => x.Message != "VIN cannot be matched in the Warehouse Inventory"));
				AssertEquals("ENT3243", invoiceLine1.JI_PreviousEntryNumber);
				AssertEquals((ZShort)2, invoiceLine1.JI_PreviousEntryLineNumber);
				AssertEquals(Helper.Part2.PK, invoiceLine1.JI_OP);
				AssertEquals("~~2", invoiceLine1.JI_PartNo);
			}
		}

		public void TestUpdateOutwardLinesWithInventoryDetails_ProductCode()
		{
			using (Helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				var part3 = Helper.CreateProduct(Helper.Owner.PK, "~~3");
				part3.OP_Desc = "~~3 DESC";
				Helper.SetTariffAndSave(Helper.Part, "KG");
				Helper.SetTariffAndSave(Helper.Part2, "KG");
				var inwardEntry = Helper.GetNewEntryHeader(Common.ZA.ZAJobMessageTypeList.Codes.Import, "BZA00001230", Helper.InwardCusProcedure.ZZ6_ProcedureCode, "ENT3243", Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
				inwardEntry.CH_BGMReference = "BOB";
				var inwardDeclaration = inwardEntry.Declaration;
				var inwardInvoiceLine2 = inwardDeclaration.InvoiceLines.AddNew();
				inwardInvoiceLine2.JI_CEI = inwardEntry.CH_CEI_Instruction;
				inwardInvoiceLine2.JI_PartNo = Helper.Part2.OP_PartNum;
				inwardInvoiceLine2.JI_Procedure = Helper.InwardCusProcedure.ZZ6_ProcedureCode + Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
				inwardInvoiceLine2.JI_InvoiceQuantity = 400m;
				inwardInvoiceLine2.JI_InvoiceUQ = "NO";
				inwardInvoiceLine2.JI_BondedWhsQuantity = 200m;
				inwardInvoiceLine2.JI_BondedWhsUnitQty = "BX";
				inwardInvoiceLine2.JI_CustomsQuantity = 20m;
				inwardInvoiceLine2.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine2.JI_LinePrice = 5000m;
				inwardInvoiceLine2.JI_EngineNumber = "EGN12";
				inwardInvoiceLine2.JI_PrimaryPreference = "EU";
				inwardInvoiceLine2.JI_ROOCert = "ROO32342";
				inwardInvoiceLine2.JI_VIN = "VIN4353";
				inwardInvoiceLine2.JI_NewUsed = "S";
				inwardInvoiceLine2.JI_CustomsThirdQuantity = 4m;
				inwardInvoiceLine2.JI_CustomsThirdUnitQty = "BAG";
				inwardInvoiceLine2.InvoiceHeader.JZ_InvoiceAmount += 5000m;
				inwardDeclaration.DoMerge();
				Factory.Save();
				inwardEntry.PublishShipmentForWHSInward(false);
				inwardEntry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				var bondedEntryKey1 = "ENT3243-1";
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey1, 100m);
				var bondedEntryKey2 = "ENT3243-2";
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 200m);
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_MessageType = Common.ZA.ZAJobMessageTypeList.Codes.ExBond;
				declaration.JE_OH_Importer = inwardDeclaration.JE_OH_Importer;
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = Helper.OutwardCusProcedure.ZZ6_ProcedureCode;
				entryInstruction.CEI_Description = "OUT DESC";
				entryInstruction.CEI_OA_Warehouse = inwardEntry.EntryInstruction.CEI_OA_Warehouse2;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;
				invoiceLine1.JI_BondedWhsQuantity = 50m;
				invoiceLine1.JI_VIN = "";
				invoiceLine1.JI_PreviousEntryLineNumber = 0;
				invoiceLine1.JI_PreviousEntryNumber = "";
				invoiceLine1.JI_OP = part3.PK;
				new DeclarationInventorySelectionHeader(entryInstruction).UpdateOutwardLinesWithInventoryDetails(entryInstruction.InvoiceLines);
				Assert(invoiceLine1.RowMessageErrors.Any(x => x.Message == "No available warehouse inventory can be found for the requested product code"));
				invoiceLine1.JI_OP = Helper.Part.PK;
				invoiceLine1.ClearRowNotifications();
				new DeclarationInventorySelectionHeader(entryInstruction).UpdateOutwardLinesWithInventoryDetails(entryInstruction.InvoiceLines);
				Assert(invoiceLine1.RowMessageErrors.All(x => x.Message != "No available warehouse inventory can be found for the requested product code"));
			}
		}

		public void TestImportInventories()
		{
			var declaration = Factory.New<JobDeclaration>();
			var inventorySelectionHeader = new DeclarationInventorySelectionHeader(declaration);
			Factory.Save();
			SetupWarehouse(inventorySelectionHeader);
			inventorySelectionHeader.ImportInventories();
			var importedGroupHeaders = declaration.JobComInvoiceGroupHeaders;
			AssertEquals(1, importedGroupHeaders.Count);
			var importedHeaders = importedGroupHeaders[0].JobComInvoiceHeaders;
			AssertEquals(1, importedHeaders.Count);
			var importedLines = importedHeaders[0].JobComInvoiceLines;
			AssertEquals(1, importedLines.Count);
		}

		public void TestImportInventories_PopuldateJI_Procedure_EmptyWB_InwardProcedure()
		{
			var declaration = Factory.New<JobDeclaration>();
			var inventorySelectionHeader = new DeclarationInventorySelectionHeader(declaration);
			Factory.Save();
			SetupWarehouse(inventorySelectionHeader);
			CombineAssertions(() =>
			{
				inventorySelectionHeader.ImportInventories();
				AssertInvoiceLineImportedAndSetJI_Procedure(declaration, string.Empty, "Empty WB_InwardProcedure");
			});
		}

		public void TestImportInventories_PopuldateJI_Procedure_NoEntryInstruction()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			testHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "ZA", "11", "00", "", "", "IMP,EXW", "");
			testHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "ZA", "11", "41", "", "", "IMP,EXW", "");
			var declaration = Factory.New<JobDeclaration>();
			var inventorySelectionHeader = new DeclarationInventorySelectionHeader(declaration);
			Factory.Save();
			SetupWarehouse(inventorySelectionHeader, "4140");
			CombineAssertions(() =>
			{
				inventorySelectionHeader.ImportInventories();
				AssertInvoiceLineImportedAndSetJI_Procedure(declaration, string.Empty, "Null Entry Instruction");
			});
		}

		public void TestImportInventories_PopuldateJI_Procedure_HasEntryInstruction()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			testHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "ZA", "11", "00", "", "", "IMP,EXW", "");
			testHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "ZA", "11", "41", "", "", "IMP,EXW", "");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var inventorySelectionHeader = new DeclarationInventorySelectionHeader(declaration);
			Factory.Save();
			SetupWarehouse(inventorySelectionHeader, "4140");
			CombineAssertions(() =>
			{
				inventorySelectionHeader.ImportInventories();
				AssertInvoiceLineImportedAndSetJI_Procedure(declaration, string.Empty, "Empty CEI_Style");
				declaration.InvoiceLines.DeleteAll();
				entryInstruction.CEI_Style = "11";
				inventorySelectionHeader.ImportInventories();
				AssertInvoiceLineImportedAndSetJI_Procedure(declaration, "1141", "CEI_Style+WB_InwardProcedure");
				declaration.InvoiceLines.DeleteAll();
				entryInstruction.CEI_Style = "XX";
				inventorySelectionHeader.ImportInventories();
				AssertInvoiceLineImportedAndSetJI_Procedure(declaration, string.Empty, "The code not in procedure list");
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.ZA.ZAJobMessageTypeList.Codes.ExBond;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			return new DeclarationInventorySelectionHeader(entryInstruction);
		}

		void AssertInvoiceLineImportedAndSetJI_Procedure(JobDeclaration declaration, string expectedProcedure, string message)
		{
			var importedLines = declaration.InvoiceLines;
			AssertEquals("Count", 1, importedLines.Count);
			var invoiceLine = declaration.InvoiceLines[0];
			AssertEquals($"JI_Procedure: {message}", expectedProcedure, invoiceLine.JI_Procedure);
		}

		void SetupWarehouse(DeclarationInventorySelectionHeader inventorySelectionHeader, string inwardProcedure = "")
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsReceiveLine1 = Helper.GetNewWhsReceiveLine(whsReceive.PK, Helper.Part.PK, "PACKAGE1", 100m, 900m, 900m, bondedEntryKey: "EN00123-1", arrivalDate: ZDateTime.BrettsBirthday);
			var whsInventory1 = whsReceiveLine1.Inventory;
			var newSelectedLine = new WhsInventoryWrapper(whsInventory1, inventorySelectionHeader);
			inventorySelectionHeader.SelectedLines.Add(newSelectedLine);
			Factory.Save();
			if (!string.IsNullOrEmpty(inwardProcedure))
			{
				var attributeQuery = new ZQuery(WhsBondedWarehouseAttributeSchema.WB_ParentID, newSelectedLine.ReceiveLine.PK);
				attributeQuery.AddToFilter(WhsBondedWarehouseAttributeSchema.WB_ParentTableCode, WhsDocketLineSchema.Constants.Prefix);
				var whsBondedWarehouseAttribute = Factory.LoadTop1<IWhsBondedWarehouseAttribute>(attributeQuery);
				whsBondedWarehouseAttribute.WB_InwardProcedure = inwardProcedure;
				Factory.Save();
			}
		}

		void AssertInvoiceLine(JobComInvoiceLine invoiceLine, ZGuid entryInstructionPK, ZDecimal invoiceQty, ZString invoiceUQ, ZDecimal countableQty, ZString countableUQ, ZDecimal customsQty, ZString customsUQ, ZDecimal customsThirdQty, ZString customsThirdUQ, ZDecimal linePrice, ZString engineNumber, ZString vin, ZString rooCert, ZString newUsed, ZString ppc)
		{
			AssertEquals("invoiceLine.JI_CEI", entryInstructionPK, invoiceLine.JI_CEI);
			AssertEquals("invoiceLine.JI_InvoiceQuantity", invoiceQty, invoiceLine.JI_InvoiceQuantity);
			AssertEquals("invoiceLine.JI_InvoiceUQ", invoiceUQ, invoiceLine.JI_InvoiceUQ);
			AssertEquals("invoiceLine.JI_BondedWhsQuantity", countableQty, invoiceLine.JI_BondedWhsQuantity);
			AssertEquals("invoiceLine.JI_BondedWhsUnitQty", countableUQ, invoiceLine.JI_BondedWhsUnitQty);
			AssertEquals("invoiceLine.JI_CustomsUnitQty", customsUQ, invoiceLine.JI_CustomsUnitQty);
			AssertEquals("invoiceLine.JI_CustomsQuantity", customsQty, invoiceLine.JI_CustomsQuantity);
			AssertEquals("invoiceLine.JI_CustomsThirdQuantity", customsThirdQty, invoiceLine.JI_CustomsThirdQuantity);
			AssertEquals("invoiceLine.JI_CustomsThirdUnitQty", customsThirdUQ, invoiceLine.JI_CustomsThirdUnitQty);
			AssertEquals("invoiceLine.JI_LinePrice", linePrice, invoiceLine.JI_LinePrice);
			AssertEquals("invoiceLine.JI_EngineNumber", engineNumber, invoiceLine.JI_EngineNumber);
			AssertEquals("invoiceLine.JI_VIN", vin, invoiceLine.JI_VIN);
			AssertEquals("invoiceLine.JI_ROOCert", rooCert, invoiceLine.JI_ROOCert);
			AssertEquals("invoiceLine.JI_NewUsed", newUsed, invoiceLine.JI_NewUsed);
			AssertEquals("invoiceLine.JI_Calc_PreviousProcedure", ppc, invoiceLine.JI_Calc_PreviousProcedure);
		}

		ZAWhsDataTestHelper helper;
		ZAWhsDataTestHelper Helper => helper ?? (helper = new ZAWhsDataTestHelper(Factory));
	}
}
