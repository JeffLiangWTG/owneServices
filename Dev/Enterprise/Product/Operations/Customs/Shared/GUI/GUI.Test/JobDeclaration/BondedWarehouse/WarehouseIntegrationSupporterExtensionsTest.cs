using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using IWarehouseIntegrationSupporter = Enterprise.Customs.Business.WarehouseExtensions.IWarehouseIntegrationSupporter;

namespace Enterprise.Customs.GUI.WarehouseExtensions.Testing
{
	sealed class WarehouseIntegrationSupporterExtensionsTest : TestCaseWithFactory
	{
		public void TestCancelBondedWarehouseOutward_EndsWith_PostCancelBondedWarehouseOutwardAction_HasManualWhsUpdateChanged()
		{
			using (WhsDataTestHelper.WhsHelper.UsePutawayEngineManagerMock())
			using (WhsDataTestHelper.WhsHelper.UseAllocationEngineMock())
			{
				var intoWhsEntry = GetIntoWhsEntry("INTOWHS");
				Factory.Save();
				intoWhsEntry.UpdateBondedWarehouseInward();
				var outOfWhsEntry = GetOutOfWhsEntry("OUTWHS", "INTOWHS");
				outOfWhsEntry.UpdateBondedWarehouseOutward();
				outOfWhsEntry.CancelBondedWarehouseOutward();
				AssertEquals(true, outOfWhsEntry.IsPostCancelBondedWarehouseOutwardActionRunOnce);
				AssertEquals(false, ((IWarehouseIntegrationSupporter)outOfWhsEntry).HasManualWhsUpdate);
				Assert("Already Saved", !outOfWhsEntry.HasChanges);
			}
		}

		public void TestUpdateBondedWarehouseOutward_EndsWith_PostUpdateBondedWarehouseOutwardAction_HasManualWhsUpdateChanged()
		{
			using (WhsDataTestHelper.WhsHelper.UsePutawayEngineManagerMock())
			using (WhsDataTestHelper.WhsHelper.UseAllocationEngineMock())
			{
				var intoWhsEntry = GetIntoWhsEntry("INTOWHS");
				Factory.Save();
				intoWhsEntry.UpdateBondedWarehouseInward();
				var outOfWhsEntry = GetOutOfWhsEntry("OUTWHS", "INTOWHS");
				outOfWhsEntry.UpdateBondedWarehouseOutward();
				AssertEquals(true, outOfWhsEntry.IsPostUpdateBondedWarehouseOutwardActionRunOnce);
				AssertEquals(true, ((IWarehouseIntegrationSupporter)outOfWhsEntry).HasManualWhsUpdate);
				Assert("Already Saved", !outOfWhsEntry.HasChanges);
			}
		}

		public void TestUpdateBondedWarehouseInward_EndsWith_PostUpdateBondedWarehouseInwardAction_HasManualWhsUpdateChanged()
		{
			using (WhsDataTestHelper.WhsHelper.UsePutawayEngineManagerMock())
			{
				var intoWhsEntry = GetIntoWhsEntry("INTOWHS");
				Factory.Save();
				intoWhsEntry.UpdateBondedWarehouseInward();
				AssertEquals(true, intoWhsEntry.IsPostUpdateBondedWarehouseInwardActionRunOnce);
				AssertEquals(true, ((IWarehouseIntegrationSupporter)intoWhsEntry).HasManualWhsUpdate);
				Assert("Already Saved", !intoWhsEntry.HasChanges);
			}
		}

		public void TestUpdateBondedWarehouseOutward_NoException_WhenPublishShipmentFailed()
		{
			using (WhsDataTestHelper.WhsHelper.UsePutawayEngineManagerMock())
			using (WhsDataTestHelper.WhsHelper.UseAllocationEngineMock())
			using (GetAddJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry().SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var intoWhsEntry = GetIntoWhsEntry("INTOWHS");
				Factory.Save();
				intoWhsEntry.UpdateBondedWarehouseInward();
				var outOfWhsEntry = GetOutOfWhsEntry("OUTWHS", "INTOWHS");
				MakeDefaultDockDoorInvalid();
				AssertNoExceptionThrown("UpdateBondedWarehouseOutward should not throw exception if PublishShipmentForWHSOutward returns null", () => outOfWhsEntry.UpdateBondedWarehouseOutward());
			}
		}

		public void TestCancelBondedWarehouseInward_EndsWith_PostCancelBondedWarehouseInwardAction_HasManualWhsUpdateChanged()
		{
			using (WhsDataTestHelper.WhsHelper.UsePutawayEngineManagerMock())
			{
				var intoWhsEntry = GetIntoWhsEntry("INTOWHS");
				Factory.Save();
				intoWhsEntry.UpdateBondedWarehouseInward();
				intoWhsEntry.CancelBondedWarehouseInward();
				AssertEquals(true, intoWhsEntry.IsPostCancelBondedWarehouseInwardActionRunOnce);
				AssertEquals(false, ((IWarehouseIntegrationSupporter)intoWhsEntry).HasManualWhsUpdate);
				Assert("Already Saved", !intoWhsEntry.HasChanges);
			}
		}

		public void TestUpdateBondedWarehouseChangeOfOwnership_EndsWith_HasManualWhsUpdateChanged()
		{
			using (WhsDataTestHelper.WhsHelper.UsePutawayEngineManagerMock())
			using (WhsDataTestHelper.WhsHelper.UseAllocationEngineMock())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var changeOfOwnershipEntry = GetChangeOfOwnershipEntry(WhsDataTestHelper);
				changeOfOwnershipEntry.UpdateBondedWarehouseChangeOfOwnership();
				AssertEquals("changeOfOwnershipEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreated, changeOfOwnershipEntry.CH_WarehouseTransactionStatus);
				AssertEquals(true, ((IWarehouseIntegrationSupporter)changeOfOwnershipEntry).HasManualWhsUpdate);
				Assert("Already Saved", !changeOfOwnershipEntry.HasChanges);
			}
		}

		public void TestUpdateBondedWarehouseChangeOfOwnership_NoException_WhenPublishShipmentFailed()
		{
			using (WhsDataTestHelper.WhsHelper.UsePutawayEngineManagerMock())
			using (WhsDataTestHelper.WhsHelper.UseAllocationEngineMock())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			using (GetAddJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry().SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var changeOfOwnershipEntry = GetChangeOfOwnershipEntry(WhsDataTestHelper);
				MakeDefaultDockDoorInvalid();
				AssertNoExceptionThrown(() => changeOfOwnershipEntry.UpdateBondedWarehouseChangeOfOwnership());
			}
		}

		public void TestCancelBondedWarehouseChangeOfOwnership_EndsWith_HasManualWhsUpdateChanged()
		{
			using (WhsDataTestHelper.WhsHelper.UsePutawayEngineManagerMock())
			using (WhsDataTestHelper.WhsHelper.UseAllocationEngineMock())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var changeOfOwnershipEntry = GetChangeOfOwnershipEntry(WhsDataTestHelper);
				changeOfOwnershipEntry.PublishShipmentForWHSChangeOfOwnership(true, true);
				AssertEquals("changeOfOwnershipEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreatedPending, changeOfOwnershipEntry.CH_WarehouseTransactionStatus);
				AssertEquals(true, ((IWarehouseIntegrationSupporter)changeOfOwnershipEntry).HasManualWhsUpdate);
				changeOfOwnershipEntry.CancelBondedWarehouseChangeOfOwnership();
				AssertEquals("changeOfOwnershipEntry.CH_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCanceled, changeOfOwnershipEntry.CH_WarehouseTransactionStatus);
				AssertEquals(false, ((IWarehouseIntegrationSupporter)changeOfOwnershipEntry).HasManualWhsUpdate);
				Assert("Already Saved", !changeOfOwnershipEntry.HasChanges);
			}
		}

		void MakeDefaultDockDoorInvalid()
		{
			var dockDoorLocationType = Factory.LoadTop1<IWhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_LocationClass, "NOR"));
			var row = Factory.LoadTop1<IWhsRow>(new ZQuery(WhsRowSchema.WR_Name, "DOCKDOOR"));
			var location = (IWhsLocation)row.Locations[0];
			location.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Factory.Save();
		}

		EntryForTest GetOutOfWhsEntry(string entryNumber, string previousEntryNumber)
		{
			var entry = GetNewEntryHeader(JobMessageTypeList.Codes.Import,
				"",
				WhsDataTestHelper.OutwardCusProcedure.ZZ6_ProcedureCode,
				previousEntryNumber,
				WhsDataTestHelper.OutwardCusProcedure.ZZ6_PreviousProcedureCode,
				1000m,
				WhsDataTestHelper.OutwardCusProcedure.ZZ6_Group,
				Core.Constants.CurrencyCodes.Australia);
			entry.EntryNumber = entryNumber;
			return entry;
		}

		EntryForTest GetIntoWhsEntry(string entryNumber)
		{
			return GetNewEntryHeader(JobMessageTypeList.Codes.Import,
				"",
				WhsDataTestHelper.InwardCusProcedure.ZZ6_ProcedureCode,
				entryNumber,
				WhsDataTestHelper.InwardCusProcedure.ZZ6_PreviousProcedureCode,
				1000m,
				WhsDataTestHelper.InwardCusProcedure.ZZ6_Group,
				Core.Constants.CurrencyCodes.Australia);
		}

		WhsDataTestHelper whsDataTestHelper;
		WhsDataTestHelper WhsDataTestHelper => whsDataTestHelper ?? (whsDataTestHelper = new WhsDataTestHelper(Factory));

		EntryForTest GetNewEntryHeader(string messageType, string declarationReference, string procedureCode, string entryNumber, string previousProcedureCode, decimal quantity, string instructionStyle, string currencyCode)
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Protected().Setup<bool>("SupportMultipleWarehouseEntryCore").Returns(true);
			var declaration = declarationMock.Object;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = messageType;
			declaration.JE_OH_Importer = WhsDataTestHelper.Importer.PK;
			declaration.JE_DeclarationReference = declarationReference;
			declaration.JE_CustomsOffice = "BFN";
			declaration.JE_GS_NKCusAgent = WhsDataTestHelper.CurrentStaff.GS_Code;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_Style = instructionStyle;
			entryInstruction.CEI_Description = instructionStyle + " DESC";
			entryInstruction.CEI_OA_Warehouse = WhsDataTestHelper.WhsWarehouse.WW_OA_WarehouseAddress;
			entryInstruction.CEI_OA_Warehouse2 = WhsDataTestHelper.WhsWarehouse.WW_OA_WarehouseAddress;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = quantity * 100m;
			invoice.JZ_RX_NKInvoice_Currency = currencyCode;
			var invoiceLine = WhsDataTestHelper.AddInvoiceLine(invoice, WhsDataTestHelper.Part, quantity, entryInstruction.PK, procedureCode, previousProcedureCode, entryNumber, 0);
			var entry = Factory.New<EntryForTest>();
			entry.CH_JE = declaration.PK;
			entry.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			if (entry.IsIntoWarehouseWarehousing)
			{
				entry.EntryNumber = entryNumber;
			}

			return entry;
		}

		CusEntryHeader GetChangeOfOwnershipEntry(WhsDataTestHelper helper)
		{
			var ownerPart2 = helper.CreateProduct(helper.Owner.PK, helper.Part2.OP_PartNum);
			var inwardDeclaration = helper.GetNewDeclaration(JobMessageTypeList.Codes.Import, "BZA0001232", "ENT32342", 100m);
			inwardDeclaration.JE_ApplicationCode = "BLT";
			inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			inwardDeclaration[ZAJobDeclarationSchema.JE_AGTCode.Name] = "ASBSD";
			inwardDeclaration.JE_CustomsOffice = "JHB";
			var inwardEntryInstruction = Factory.New<CusEntryInstruction>();
			inwardEntryInstruction.CEI_JE = inwardDeclaration.PK;
			inwardEntryInstruction.CEI_Style = helper.InwardCusProcedure.ZZ6_ProcedureCode;
			inwardEntryInstruction.CEI_OA_Warehouse2 = helper.WhsWarehouse.WW_OA_WarehouseAddress;
			var inwardEntry = inwardDeclaration.CustomsEntryHeaders[0];
			inwardEntry.CH_CEI_Instruction = inwardEntryInstruction.PK;
			var inwardInvoiceLine = inwardDeclaration.InvoiceLines[0];
			inwardInvoiceLine.JI_CEI = inwardEntryInstruction.PK;
			inwardInvoiceLine.JI_Procedure = helper.InwardCusProcedure.ZZ6_ProcedureCode + helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
			inwardInvoiceLine.JI_PartNo = helper.Part2.OP_PartNum;
			inwardInvoiceLine.JI_BondedWhsQuantity = 300m;
			inwardInvoiceLine.JI_BondedWhsUnitQty = "NO";
			inwardDeclaration.DoMerge();
			Factory.Save();
			inwardEntry.PublishShipmentForWHSInward(false);
			inwardEntry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
			var changeOfOwnershipDeclaration = helper.GetNewDeclaration(JobMessageTypeList.Codes.ExWarehouse, "BZA0005433", "", 60m);
			changeOfOwnershipDeclaration.JE_ApplicationCode = "BLT";
			changeOfOwnershipDeclaration[ZAJobDeclarationSchema.JE_AGTCode.Name] = "ASBSD";
			var changeOfOwnershipEntryInstruction = Factory.New<CusEntryInstruction>();
			changeOfOwnershipEntryInstruction.CEI_JE = changeOfOwnershipDeclaration.PK;
			changeOfOwnershipEntryInstruction.CEI_Style = helper.ChangeOfOwnershipCusProcedure.ZZ6_ProcedureCode;
			changeOfOwnershipEntryInstruction.CEI_OH_Owner = helper.Owner.PK;
			changeOfOwnershipEntryInstruction.CEI_OA_Warehouse = helper.WhsWarehouse.WW_OA_WarehouseAddress;
			changeOfOwnershipEntryInstruction.CEI_OA_Warehouse2 = helper.WhsWarehouse.WW_OA_WarehouseAddress;
			var changeOfOwnershipEntry = changeOfOwnershipDeclaration.CustomsEntryHeaders[0];
			changeOfOwnershipEntry.CH_CEI_Instruction = changeOfOwnershipEntryInstruction.PK;
			var changeOfOwnershipInvoiceLine = changeOfOwnershipDeclaration.InvoiceLines[0];
			changeOfOwnershipInvoiceLine.JI_CEI = changeOfOwnershipEntryInstruction.PK;
			changeOfOwnershipInvoiceLine.JI_Procedure = helper.ChangeOfOwnershipCusProcedure.ZZ6_ProcedureCode + helper.ChangeOfOwnershipCusProcedure.ZZ6_PreviousProcedureCode;
			changeOfOwnershipInvoiceLine.JI_PreviousEntryNumber = "ENT32342";
			changeOfOwnershipInvoiceLine.JI_PreviousEntryLineNumber = 2;
			changeOfOwnershipInvoiceLine.JI_PartNo = helper.Part2.OP_PartNum;
			changeOfOwnershipInvoiceLine.JI_BondedWhsQuantity = 150m;
			changeOfOwnershipInvoiceLine.JI_BondedWhsUnitQty = "NO";
			changeOfOwnershipInvoiceLine[ZAJobComInvoiceLineSchema.JI_NewOwnerPartNo.Name] = ownerPart2.OP_PartNum;
			changeOfOwnershipDeclaration.DoMerge();
			Factory.Save();
			return changeOfOwnershipEntry;
		}

		BooleanRegistryItem GetAddJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry()
		{
			var accountingConfigurationRegistry = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			return (BooleanRegistryItem)accountingConfigurationRegistry.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
		}

		sealed class EntryForTest : CusEntryHeader, IWarehouseIntegrationSupporter
		{
			public EntryForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool IsPostUpdateBondedWarehouseInwardActionRunOnce => cnt1 == 1;
			public bool IsPostUpdateBondedWarehouseOutwardActionRunOnce => cnt2 == 1;
			public bool IsPostCancelBondedWarehouseInwardActionRunOnce => cnt3 == 1;
			public bool IsPostCancelBondedWarehouseOutwardActionRunOnce => cnt4 == 1;
			void IWarehouseIntegrationSupporter.PostUpdateBondedWarehouseInwardAction() => cnt1++;
			void IWarehouseIntegrationSupporter.PostUpdateBondedWarehouseOutwardAction() => cnt2++;
			void IWarehouseIntegrationSupporter.PostCancelBondedWarehouseInwardAction() => cnt3++;
			void IWarehouseIntegrationSupporter.PostCancelBondedWarehouseOutwardAction() => cnt4++;
			int cnt1;
			int cnt2;
			int cnt3;
			int cnt4;
		}
	}
}
