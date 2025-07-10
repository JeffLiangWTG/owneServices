using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.Business.WarehouseExtensions.Testing
{
	sealed class JobDeclarationWarehouseExtensionsTest_WhenSupportModificationState : WhsDataTestHelper
	{
		IWhsWarehouse PrepareWarehouseData()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				helper.Importer.MainAddress.OA_PostCode = "1234";
				helper.Importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "FRTVA1234");

				var whsWarehouse1 = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "N10");
				whsWarehouse1.WW_IsVirtualWarehouse = false;
				var whsRowA = helper.WhsHelper.CreateRowAndGenerateLocations(whsWarehouse1, "A");
				Factory.Save();

				var locationAPK = helper.WhsHelper.FindLocation(whsWarehouse1.PK, "A").PK;
				var receive = helper.GetNewWhsReceive(whsWarehouse1.PK, helper.Importer.PK);
				receive.WD_CustomsParentReference = "2-B0893654455-EDIDATEDI";
				receive.WD_TotalUnits = 100m;
				var receiveLine = helper.GetNewWhsReceiveLine(receive.PK, helper.Part.PK, ZString.Empty, 1m, 100m, 100m, "NO", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "ENT1234-1", ZDateTime.Today.AddMonths(-1));
				receiveLine.WE_WL = locationAPK;
				var whsInventory = receiveLine.Inventory;
				Factory.Save();

				return whsWarehouse1;
			}
		}

		public void TestIsInwardOrOutwardFirstTryPending_WhenInward()
		{
			var whsWarehouse1 = PrepareWarehouseData();
			var inwardDec = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B0893654455", "ENT865", 110m);
			var entry = inwardDec.ActiveEntryHeaders[0];
			entry.CH_BGMReference = "2-B0893654455";
			var instruction = inwardDec.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			instruction.CEI_OA_Warehouse2 = whsWarehouse1.WW_OA_WarehouseAddress;
			Factory.Save();

			AssertEquals("Status is not ICH", false, entry.IsInwardOrOutwardFirstTryPending());

			entry.PublishShipmentForWHSInward(true);
			AssertEquals("Status is ICH and has hold note.", true, entry.IsInwardOrOutwardFirstTryPending());

			entry.PublishShipmentForWHSInward(true);
			AssertEquals("Status is ICH and has hold note, but also has modification note.", false, entry.IsInwardOrOutwardFirstTryPending());
		}

		public void TestIsInwardOrOutwardFirstTryPending_WhenOutward()
		{
			var entry = Factory.New<CusEntryHeader>();
			AssertEquals("Status is not OCP", false, entry.IsInwardOrOutwardFirstTryPending());

			entry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreatedPending;
			entry.Notes.AddNew(false, WarehouseConstants.UniversalHoldShipmentNoteDescription, "");
			AssertEquals("Status is OCP and has hold note.", true, entry.IsInwardOrOutwardFirstTryPending());

			entry.Notes.AddNew(false, WarehouseConstants.UniversalModificationShipmentNoteDescription, "");
			AssertEquals("Status is OCP and has hold note, but also has modification note.", false, entry.IsInwardOrOutwardFirstTryPending());
		}

		public void TestIsInwardSecondTryPending()
		{
			var whsWarehouse1 = PrepareWarehouseData();
			var inwardDec = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B0893654455", "ENT865", 110m);
			var entry = inwardDec.ActiveEntryHeaders[0];
			entry.CH_BGMReference = "2-B0893654455";
			var instruction = inwardDec.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			instruction.CEI_OA_Warehouse2 = whsWarehouse1.WW_OA_WarehouseAddress;
			Factory.Save();

			AssertEquals("Status is not ICH", false, entry.IsInwardOrOutwardSecondTryPending());

			entry.PublishShipmentForWHSInward(true);
			AssertEquals("Status is ICH and has hold note, but has no modification note.", false, entry.IsInwardOrOutwardSecondTryPending());

			entry.PublishShipmentForWHSInward(true);
			AssertEquals("Status is ICH and has hold note, and has modification note.", true, entry.IsInwardOrOutwardSecondTryPending());
		}

		public void TestDeleteUniversalHoldShipmentNotes()
		{
			var whsWarehouse1 = PrepareWarehouseData();
			var inwardDec = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B0893654455", "ENT865", 110m);
			var entry = inwardDec.ActiveEntryHeaders[0];
			entry.CH_BGMReference = "2-B0893654455";
			var instruction = inwardDec.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			instruction.CEI_OA_Warehouse2 = whsWarehouse1.WW_OA_WarehouseAddress;
			Factory.Save();
			entry.PublishShipmentForWHSInward(true);
			entry.PublishShipmentForWHSInward(true);
			AssertEquals("Original note exists.", true, entry.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Any());
			AssertEquals("Modification note exists.", true, entry.Notes.FindByDescription(WarehouseConstants.UniversalModificationShipmentNoteDescription).Any());

			entry.DeleteUniversalShipmentNotes();
			AssertEquals("Original note was deleted.", false, entry.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Any());
			AssertEquals("Modification note was deleted.", false, entry.Notes.FindByDescription(WarehouseConstants.UniversalModificationShipmentNoteDescription).Any());
		}

		public void TestElevateModificationToHold()
		{
			var whsWarehouse1 = PrepareWarehouseData();
			var inwardDec = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B0893654455", "ENT865", 110m);
			var entry = inwardDec.ActiveEntryHeaders[0];
			entry.CH_BGMReference = "2-B0893654455";
			var instruction = inwardDec.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			instruction.CEI_OA_Warehouse2 = whsWarehouse1.WW_OA_WarehouseAddress;
			var invoiceLine = entry.InvoiceLines.Single();
			Factory.Save();

			invoiceLine.JI_PartNo = "APPLE";
			entry.PublishShipmentForWHSInward(true);
			var holdNote = entry.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Single();
			AssertXMLContains("<PartNo>APPLE</PartNo>", Compressor.UncompressAsString(holdNote.ST_NoteData));

			invoiceLine.JI_PartNo = "BANANA";
			entry.PublishShipmentForWHSInward(true);
			var modificationNote = entry.Notes.FindByDescription(WarehouseConstants.UniversalModificationShipmentNoteDescription).Single();
			AssertXMLContains("<PartNo>BANANA</PartNo>", Compressor.UncompressAsString(modificationNote.ST_NoteData));

			entry.ElevateModificationToHold();
			var updatedHoldNote = entry.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Single();
			AssertXMLContains("<PartNo>BANANA</PartNo>", Compressor.UncompressAsString(updatedHoldNote.ST_NoteData));
			AssertEquals("Modification note was deleted (elevated to hold in fact).", false, entry.Notes.FindByDescription(WarehouseConstants.UniversalModificationShipmentNoteDescription).Any());
		}

		public void TestDeleteModificationNotes()
		{
			var whsWarehouse1 = PrepareWarehouseData();
			var inwardDec = GetNewDeclaration(JobMessageTypeList.Codes.Import, "B0893654455", "ENT865", 110m);
			var entry = inwardDec.ActiveEntryHeaders[0];
			entry.CH_BGMReference = "2-B0893654455";
			var instruction = inwardDec.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			instruction.CEI_OA_Warehouse2 = whsWarehouse1.WW_OA_WarehouseAddress;
			var invoiceLine = entry.InvoiceLines.Single();
			Factory.Save();

			invoiceLine.JI_PartNo = "APPLE";
			entry.PublishShipmentForWHSInward(true);
			var holdNote = entry.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Single();
			AssertXMLContains("<PartNo>APPLE</PartNo>", Compressor.UncompressAsString(holdNote.ST_NoteData));

			invoiceLine.JI_PartNo = "BANANA";
			entry.PublishShipmentForWHSInward(true);
			var modificationNote = entry.Notes.FindByDescription(WarehouseConstants.UniversalModificationShipmentNoteDescription).Single();
			AssertXMLContains("<PartNo>BANANA</PartNo>", Compressor.UncompressAsString(modificationNote.ST_NoteData));

			entry.DeleteModificationNotes();
			var updatedHoldNote = entry.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Single();
			AssertXMLContains("<PartNo>APPLE</PartNo>", Compressor.UncompressAsString(updatedHoldNote.ST_NoteData));
			AssertEquals("Modification note was deleted (truly).", false, entry.Notes.FindByDescription(WarehouseConstants.UniversalModificationShipmentNoteDescription).Any());
		}

		public void TestCancelPublishShipmentForWHSOutward_ToCancelTheFirstPublish()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = GetInwardDeclaration("B00002377", "ENT123", 1000m);
				var inwardEntry = inwardDeclaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Single();
				inwardEntry.PublishShipmentForWHSInward(false);
				inwardEntry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);

				var outwardDeclaration = GetOutwardDeclaration("B00002378", "ENT123", 10m);
				var outwardEntry = outwardDeclaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Single();
				Factory.Save();

				// publish for the first time.
				var publishResult = outwardEntry.PublishShipmentForWHSOutward();
				Factory.Save();
				AssertNotEquals(UniversalResult.HadErrors, publishResult.ResultType);
				AssertEquals("A hold note is created after publish.", true, outwardEntry.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Any());

				outwardEntry.CancelPublishShipmentForWHSOutward();
				Factory.Save();
				var outwardEntryInADifferentFactory = new BusinessObjectFactory().Load<CusEntryHeader>(outwardEntry.PK);
				AssertEquals("The hold note should be deleted if we cancel the publish.", false, outwardEntryInADifferentFactory.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Any());
			}
		}

		public void TestCancelPublishShipmentForWHSOutward_ToCancelTheSecondPublish()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = GetInwardDeclaration("B00002377", "ENT123", 1000m);
				var inwardEntry = inwardDeclaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Single();
				inwardEntry.PublishShipmentForWHSInward(false);
				inwardEntry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);

				var outwardDeclaration = GetOutwardDeclaration("B00002378", "ENT123", 10m);
				var outwardEntry = outwardDeclaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Single();
				Factory.Save();

				// publish for the first time.
				var publishResult = outwardEntry.PublishShipmentForWHSOutward();
				Factory.Save();
				AssertNotEquals(UniversalResult.HadErrors, publishResult.ResultType);
				AssertEquals("A hold note is created after publish.", true, outwardEntry.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Any());

				// publish for the second time.
				var publishAgainResult = outwardEntry.PublishShipmentForWHSOutward();
				Factory.Save();
				AssertNotEquals(UniversalResult.HadErrors, publishAgainResult.ResultType);
				AssertEquals("The hold note still exists.", true, outwardEntry.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Any());
				AssertEquals("Another modification note is created after second publish.", true, outwardEntry.Notes.FindByDescription(WarehouseConstants.UniversalModificationShipmentNoteDescription).Any());

				outwardEntry.CancelPublishShipmentForWHSOutward();
				Factory.Save();
				var outwardEntryFromADifferentFactory = new BusinessObjectFactory().Load<CusEntryHeader>(outwardEntry.PK);
				AssertEquals("The hold note still exists.", true, outwardEntryFromADifferentFactory.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Any());
				AssertEquals("The modification note should be deleted if we cancel the publish.", false, outwardEntryFromADifferentFactory.Notes.FindByDescription(WarehouseConstants.UniversalModificationShipmentNoteDescription).Any());
			}
		}

		public void TestGetLastModificationUniversalShipment()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = GetInwardDeclaration("B00002377", "ENT123", 1000m);
				var inwardEntry = inwardDeclaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Single();
				inwardEntry.PublishShipmentForWHSInward(false);
				inwardEntry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);

				var outwardDeclaration = GetOutwardDeclaration("B00002378", "ENT123", 10m);
				var outwardEntry = outwardDeclaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Single();
				Factory.Save();

				// publish for the first time.
				var publishResult = outwardEntry.PublishShipmentForWHSOutward();
				Factory.Save();
				AssertNotEquals(UniversalResult.HadErrors, publishResult.ResultType);
				AssertEquals("A hold note is created after publish.", true, outwardEntry.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Any());

				outwardEntry.InvoiceLines.Single().JI_BondedWhsQuantity = 100m;
				// publish for the second time.
				var publishAgainResult = outwardEntry.PublishShipmentForWHSOutward();
				Factory.Save();
				AssertNotEquals(UniversalResult.HadErrors, publishAgainResult.ResultType);
				AssertEquals("The hold note still exists.", true, outwardEntry.Notes.FindByDescription(WarehouseConstants.UniversalHoldShipmentNoteDescription).Any());
				AssertEquals("Another modification note is created after second publish.", true, outwardEntry.Notes.FindByDescription(WarehouseConstants.UniversalModificationShipmentNoteDescription).Any());

				var holdShipment = outwardEntry.GetLastHoldUniversalShipment(RecipientRoleType.BWR);
				AssertEquals(10m, holdShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity);
				var modificationShipment = outwardEntry.GetLastModificationUniversalShipment(RecipientRoleType.BWR);
				AssertEquals(100m, modificationShipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			temporarilySetCountry = GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France); // FR is the only one who supports modification state.
		}

		protected override void TearDown()
		{
			base.TearDown();
			temporarilySetCountry.Dispose();
		}

		BaseJobDeclaration GetInwardDeclaration(ZString declarationReference, ZString entryNumber, ZDecimal quantity)
		{
			var helper = new WhsDataTestHelper(Factory);
			var inwardDeclaration = helper.GetNewDeclaration(JobMessageTypeList.Codes.Import, declarationReference, entryNumber, quantity);
			inwardDeclaration.SetSupportsBondedWarehousingForTesting(true);

			var inwardEntryInstruction = inwardDeclaration.CustomsEntryInstructions.AddNew();
			inwardEntryInstruction.CEI_OA_Warehouse2 = inwardDeclaration.WarehouseDocAddress.E2_OA_Address;

			var inwardEntry = inwardDeclaration.CustomsEntryHeaders.Single();
			inwardEntry.CH_CEI_Instruction = inwardEntryInstruction.PK;

			var inwardInvoiceLine = inwardEntry.InvoiceLines.Single();
			inwardInvoiceLine.JI_CEI = inwardEntryInstruction.PK;
			var inwardProcedure = CreateInwardCusProcedure();
			inwardInvoiceLine.JI_Procedure = inwardProcedure.ZZ6_ProcedureCode + inwardProcedure.ZZ6_PreviousProcedureCode + inwardProcedure.ZZ6_Concession;
			inwardInvoiceLine.JI_BondedWhsQuantity = quantity;
			return inwardDeclaration;
		}

		BaseJobDeclaration GetOutwardDeclaration(ZString declarationReference, ZString entryNumber, ZDecimal quantity)
		{
			var helper = new WhsDataTestHelper(Factory);
			var outwardDeclaration = helper.GetNewDeclaration(JobMessageTypeList.Codes.Import, declarationReference, entryNumber, quantity);
			var outwardMessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			outwardDeclaration.MessageInitiator = outwardMessageInitiator;
			outwardDeclaration.SetSupportsBondedWarehousingForTesting(true);

			var outwardEntryInstruction = outwardDeclaration.CustomsEntryInstructions.AddNew();
			outwardEntryInstruction.CEI_OA_Warehouse = outwardDeclaration.WarehouseDocAddress.E2_OA_Address;

			var outwardEntry = outwardDeclaration.CustomsEntryHeaders.Single();
			outwardEntry.CH_CEI_Instruction = outwardEntryInstruction.PK;

			var outwardInvoiceLine = outwardEntry.InvoiceLines.Single();
			outwardInvoiceLine.JI_CEI = outwardEntryInstruction.PK;
			var outwardProcedure = CreateOutwardCusProcedure();
			outwardInvoiceLine.JI_Procedure = outwardProcedure.ZZ6_ProcedureCode + outwardProcedure.ZZ6_PreviousProcedureCode + outwardProcedure.ZZ6_Concession;
			outwardInvoiceLine.JI_BondedWhsQuantity = quantity;
			return outwardDeclaration;
		}

		RefCusProcedure CreateInwardCusProcedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "IM", "10", "71", "F61", "", "IMP", "10P");
			procedure.ZZ6_IntoWarehouse = "Y";
			procedure.ZZ6_OutOfWarehouse = "N";
			Factory.Save();
			return procedure;
		}

		RefCusProcedure CreateOutwardCusProcedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "IM", "42", "71", "C33", "", "IMP", "42P");
			procedure.ZZ6_IntoWarehouse = "N";
			procedure.ZZ6_OutOfWarehouse = "Y";
			Factory.Save();
			return procedure;
		}

		IDisposable temporarilySetCountry;
	}
}
