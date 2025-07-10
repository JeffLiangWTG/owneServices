using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusEntryInstruction))]
	[AsycudaCustomsCountries(Core.Constants.CountryCodes.Namibia)]
	sealed class CusEntryInstructionBaseOnlyTest : CusEntryInstructionAbstractTest
	{
		public void TestCEI_DataModel_SetOnSaving()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			AssertEquals("Not set", ZString.Empty, instruction.CEI_DataModel);
			Factory.Save();
			AssertEquals("set", "ER", instruction.CEI_DataModel);
		}

		public void TestCEI_DataModel_ReportErrorWhenUpdated() =>
			DataModelTestHelper.RunDataModelTest_ReportErrorWhenUpdated<CusEntryInstruction>(Factory);

		public void TestCEI_DataModel_CanSaveTwice() =>
			DataModelTestHelper.RunDataModelTest_CanSaveTwice<CusEntryInstruction>(Factory);

		public void TestIDataModelSupporter()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			var cusSupportingInfoParent = (IDataModelSupporter)instruction;
			AssertEquals(ZString.Empty, cusSupportingInfoParent.DataModel);
			cusSupportingInfoParent.PopulateDataModelIfNeeded();
			AssertEquals("ER", cusSupportingInfoParent.DataModel);
		}

		public void TestIsOutOfOutwardProcessing()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "AA", "BB", ZString.Empty, "description", "EXP");
			procedure1.ZZ6_OutofOutwardProcessing = "Y";
			var procedure2 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "CC", "DD", ZString.Empty, "description2", "EXP");
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var cusInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = cusInstruction.PK;
			invoiceLine.JI_Procedure = "AABB";

			AssertEquals(true, cusInstruction.IsOutOfOutwardProcessing);

			invoiceLine.JI_Procedure = "CCDD";
			AssertEquals(false, cusInstruction.IsOutOfOutwardProcessing);
		}

		public void TestIsIntoOutwardProcessing()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "AA", "BB", ZString.Empty, "description", "EXP");
			procedure1.ZZ6_IntoOutwardProcessing = "Y";
			var procedure2 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "CC", "DD", ZString.Empty, "description2", "EXP");
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var cusInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = cusInstruction.PK;
			invoiceLine.JI_Procedure = "AABB";

			AssertEquals(true, cusInstruction.IsIntoOutwardProcessing);

			invoiceLine.JI_Procedure = "CCDD";
			AssertEquals(false, cusInstruction.IsIntoOutwardProcessing);
		}

		public void TestIsOutOfInwardProcessing()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "AA", "BB", ZString.Empty, "description", "EXP");
			procedure1.ZZ6_OutOfInwardProcessing = "Y";
			var procedure2 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "CC", "DD", ZString.Empty, "description2", "EXP");
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var cusInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = cusInstruction.PK;
			invoiceLine.JI_Procedure = "AABB";

			AssertEquals(true, cusInstruction.IsOutOfInwardProcessing);

			invoiceLine.JI_Procedure = "CCDD";
			AssertEquals(false, cusInstruction.IsOutOfInwardProcessing);
		}

		public void TestIsIntoInwardProcessing()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "AA", "BB", ZString.Empty, "description", "EXP");
			procedure1.ZZ6_IntoInwardProcessing = "Y";
			var procedure2 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "CC", "DD", ZString.Empty, "description2", "EXP");
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var cusInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = cusInstruction.PK;
			invoiceLine.JI_Procedure = "AABB";

			AssertEquals(true, cusInstruction.IsIntoInwardProcessing);

			invoiceLine.JI_Procedure = "CCDD";
			AssertEquals(false, cusInstruction.IsIntoInwardProcessing);
		}

		public void TestWarehouseIsOutwardProcessing()
		{
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_OA_Warehouse = warehouse.MainAddress.PK;
			AssertEquals(false, entryInstruction.WarehouseIsOutwardProcessing);
			AssertEquals(false, entryInstruction.WarehouseIsInventoryManagementOn);

			var entryInstruction2 = Factory.New<CusEntryInstruction>();
			entryInstruction2.CEI_OA_Warehouse = warehouse.MainAddress.PK;
			warehouse.CompanyData.OB_CusInventoryForOutwardProcessing = true;
			AssertEquals(false, entryInstruction2.WarehouseIsOutwardProcessing);
			AssertEquals(false, entryInstruction2.WarehouseIsInventoryManagementOn);
		}

		public void TestWarehouseIsInwardProcessing()
		{
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_OA_Warehouse = warehouse.MainAddress.PK;
			AssertEquals(false, entryInstruction.WarehouseIsInwardProcessing);
			AssertEquals(false, entryInstruction.WarehouseIsInventoryManagementOn);

			var entryInstruction2 = Factory.New<CusEntryInstruction>();
			entryInstruction2.CEI_OA_Warehouse = warehouse.MainAddress.PK;
			warehouse.CompanyData.OB_CusInventoryForInwardProcessing = true;
			AssertEquals(true, entryInstruction2.WarehouseIsInwardProcessing);
			AssertEquals(true, entryInstruction2.WarehouseIsInventoryManagementOn);
		}

		public void TestWarehouse2IsOutwardProcessing()
		{
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;
			AssertEquals(false, entryInstruction.Warehouse2IsOutwardProcessing);
			AssertEquals(false, entryInstruction.Warehouse2IsInventoryManagementOn);

			var entryInstruction2 = Factory.New<CusEntryInstruction>();
			entryInstruction2.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;
			warehouse.CompanyData.OB_CusInventoryForOutwardProcessing = true;
			AssertEquals(false, entryInstruction2.Warehouse2IsOutwardProcessing);
			AssertEquals(false, entryInstruction2.Warehouse2IsInventoryManagementOn);
		}

		public void TestWarehouse2IsInwardProcessing()
		{
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;
			AssertEquals(false, entryInstruction.Warehouse2IsInwardProcessing);
			AssertEquals(false, entryInstruction.Warehouse2IsInventoryManagementOn);

			var entryInstruction2 = Factory.New<CusEntryInstruction>();
			entryInstruction2.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;
			warehouse.CompanyData.OB_CusInventoryForInwardProcessing = true;
			AssertEquals(true, entryInstruction2.Warehouse2IsInwardProcessing);
			AssertEquals(true, entryInstruction2.Warehouse2IsInventoryManagementOn);
		}

		public void TestClientIsOutwardProcessing()
		{
			var clent = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			declaration.JE_OH_Supplier = clent.PK;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(false, entryInstruction.ClientIsOutwardProcessing);
			AssertEquals(false, entryInstruction.ClientIsInventoryManagementOn);

			var declaration2 = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			declaration2.JE_OH_Supplier = clent.PK;
			clent.CompanyData.OB_CusInventoryForOutwardProcessing = true;
			var entryInstruction2 = declaration2.CustomsEntryInstructions.AddNew();
			AssertEquals(false, entryInstruction2.ClientIsOutwardProcessing);
			AssertEquals(false, entryInstruction2.ClientIsInventoryManagementOn);
		}

		public void TestClientIsInwardProcessing()
		{
			var clent = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			declaration.JE_OH_Supplier = clent.PK;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(false, entryInstruction.ClientIsInwardProcessing);
			AssertEquals(false, entryInstruction.ClientIsInventoryManagementOn);

			var declaration2 = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			declaration2.JE_OH_Supplier = clent.PK;
			clent.CompanyData.OB_CusInventoryForInwardProcessing = true;
			var entryInstruction2 = declaration2.CustomsEntryInstructions.AddNew();
			AssertEquals(true, entryInstruction2.ClientIsInwardProcessing);
			AssertEquals(true, entryInstruction2.ClientIsInventoryManagementOn);
		}

		public void TestOwnerIsOutwardProcessing()
		{
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_OH_Owner = owner.PK;
			AssertEquals(false, entryInstruction.OwnerIsOutwardProcessing);
			AssertEquals(false, entryInstruction.OwnerIsInventoryManagementOn);

			var entryInstruction2 = Factory.New<CusEntryInstruction>();
			entryInstruction2.CEI_OH_Owner = owner.PK;
			owner.CompanyData.OB_CusInventoryForOutwardProcessing = true;
			AssertEquals(false, entryInstruction2.OwnerIsOutwardProcessing);
			AssertEquals(false, entryInstruction2.OwnerIsInventoryManagementOn);
		}

		public void TestOwnerIsInwardProcessing()
		{
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_OH_Owner = owner.PK;
			AssertEquals(false, entryInstruction.OwnerIsInwardProcessing);
			AssertEquals(false, entryInstruction.OwnerIsInventoryManagementOn);

			var entryInstruction2 = Factory.New<CusEntryInstruction>();
			entryInstruction2.CEI_OH_Owner = owner.PK;
			owner.CompanyData.OB_CusInventoryForInwardProcessing = true;
			AssertEquals(true, entryInstruction2.OwnerIsInwardProcessing);
			AssertEquals(true, entryInstruction2.OwnerIsInventoryManagementOn);
		}

		public void TestHasOutOfRegimeProcedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "AA", "BB", ZString.Empty, "OP DESC1", "EXW", outOfWarehouse: true);
			var procedure2 = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "CC", "DD", ZString.Empty, "OP DESC2", "EXW");
			procedure2.ZZ6_OutOfInwardProcessing = "Y";
			var procedure3 = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "EE", "FF", ZString.Empty, "OP DESC3", "EXW");
			procedure3.ZZ6_OutofOutwardProcessing = "Y";
			helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "HH", "II", ZString.Empty, "OP DESC4", "EXW");
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;

			invoiceLine.JI_Procedure = "AABB";
			Assert(entryInstruction.HasOutOfRegimeProcedure);

			invoiceLine.JI_Procedure = "CCDD";
			Assert(entryInstruction.HasOutOfRegimeProcedure);

			invoiceLine.JI_Procedure = "EEFF";
			Assert(entryInstruction.HasOutOfRegimeProcedure);

			invoiceLine.JI_Procedure = "GGHH";
			Assert(!entryInstruction.HasOutOfRegimeProcedure);
		}

		public void TestHasIntoRegimeProcedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "AA", "BB", ZString.Empty, "OP DESC1", "EXW", intoWarehouse: true);
			var procedure2 = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "CC", "DD", ZString.Empty, "OP DESC2", "EXW");
			procedure2.ZZ6_IntoInwardProcessing = "Y";
			var procedure3 = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "EE", "FF", ZString.Empty, "OP DESC3", "EXW");
			procedure3.ZZ6_IntoOutwardProcessing = "Y";
			helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "HH", "II", ZString.Empty, "OP DESC4", "EXW");
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;

			invoiceLine.JI_Procedure = "AABB";
			Assert(entryInstruction.HasIntoRegimeProcedure);

			invoiceLine.JI_Procedure = "CCDD";
			Assert(entryInstruction.HasIntoRegimeProcedure);

			invoiceLine.JI_Procedure = "EEFF";
			Assert(entryInstruction.HasIntoRegimeProcedure);

			invoiceLine.JI_Procedure = "GGHH";
			Assert(!entryInstruction.HasIntoRegimeProcedure);
		}

		public void TestWarehouseAddressDefaultValue()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ONEADDRESS";
			var address1 = org1.MainAddress;
			address1.OA_Address1 = "Address1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "TWOADDRESS";
			var address2 = org2.Addresses.AddNew();
			address2.OA_Address1 = "Address2";
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "WHSADDRESS";
			var address4 = org3.Addresses.AddNew();
			address4.OA_Address1 = "Address4";
			Helper.GetNewWhsWarehouse(address4.PK, false, "WHS");
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;

			CombineAssertions(() =>
			{
				entryInstruction.CEI_OA_Warehouse_ZAddress.OrgPK = org1.PK;
				AssertEquals("Default Address: Only one address", address1.PK, entryInstruction.CEI_OA_Warehouse);
				entryInstruction.CEI_OA_Warehouse_ZAddress.OrgPK = org2.PK;
				AssertEquals("Default Address: empty", ZGuid.Empty, entryInstruction.CEI_OA_Warehouse);
				entryInstruction.CEI_OA_Warehouse_ZAddress.OrgPK = org3.PK;
				AssertEquals("Default Address: more than one address and has only one warehouse address", address4.PK, entryInstruction.CEI_OA_Warehouse);

				entryInstruction.CEI_OA_Warehouse2_ZAddress.OrgPK = org1.PK;
				AssertEquals("Default Address: Only one address", address1.PK, entryInstruction.CEI_OA_Warehouse2);
				entryInstruction.CEI_OA_Warehouse2_ZAddress.OrgPK = org2.PK;
				AssertEquals("Default Address: empty", ZGuid.Empty, entryInstruction.CEI_OA_Warehouse2);
				entryInstruction.CEI_OA_Warehouse2_ZAddress.OrgPK = org3.PK;
				AssertEquals("Default Address: more than one address and has only one warehouse address", address4.PK, entryInstruction.CEI_OA_Warehouse2);
			});
		}

		public void TestHasAnEntryWithEntryStatus()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var entryInstruction = Factory.New<CusEntryInstruction>();
			AssertEquals("Null declaration", false, entryInstruction.HasAnEntryWithEntryStatus);
			entryInstruction.CEI_JE = declaration.PK;
			AssertEquals("No entries", false, entryInstruction.HasAnEntryWithEntryStatus);
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_CEI_Instruction = entryInstruction.PK;
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_CEI_Instruction = entryInstruction.PK;
			var entry3 = declaration.CustomsEntryHeaders.AddNew();
			entry3.CH_CEI_Instruction = entryInstruction.PK;
			AssertEquals("Empty entries", false, entryInstruction.HasAnEntryWithEntryStatus);

			entry2.CH_EntryStatus = "CLR";
			AssertEquals("At least one entry has value", true, entryInstruction.HasAnEntryWithEntryStatus);
		}

		public void TestUpdateOutwardLinesWithInventoryDetails()
		{
			using (Helper.WhsHelper.UsePutawayEngineManagerMock())
			using (Helper.WhsHelper.UseAllocationEngineMock())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Namibia)) // Declaration.SupportMultipleWarehouseEntry = true for NA
			{
				DataRegistry.Business.CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
				Helper.UniversalTariffHelper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "CustomsUQ", Core.Constants.CountryCodes.Namibia);
				Helper.UniversalTariffHelper.CreateCusCodeList(Core.Constants.CountryCodes.Namibia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KG", new ZDateTime(2020, 01, 01), new ZDateTime(2079, 01, 01));
				Helper.SetTariffAndSave(Helper.Part, "KG");
				Helper.SetTariffAndSave(Helper.Part2, "KG");
				var inwardEntry = Helper.GetNewEntryHeader("IMP", "B00001230", Helper.InwardCusProcedure.ZZ6_ProcedureCode, "ENT3243", Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m, Helper.InwardCusProcedure.ZZ6_Group, Core.Constants.CurrencyCodes.Namibia);
				inwardEntry.CH_BGMReference = "BOB";
				var inwardDeclaration = inwardEntry.Declaration;
				var inwardInvoiceLine2 = Helper.AddInvoiceLine(inwardDeclaration.Invoices[0], Helper.Part2, 200m, inwardEntry.CH_CEI_Instruction, Helper.InwardCusProcedure.ZZ6_ProcedureCode, Helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, "", 1);
				inwardInvoiceLine2.JI_BondedWhsUnitQty = "BX";
				inwardInvoiceLine2.InvoiceHeader.JZ_InvoiceAmount += inwardInvoiceLine2.JI_LinePrice;
				inwardDeclaration.DoMerge();
				Factory.Save();
				WarehouseExtensions.JobDeclarationWarehouseExtensions.PublishShipmentForWHSInward(inwardEntry, false);
				WarehouseExtensions.JobDeclarationWarehouseExtensions.PublishAcceptEventForWHSInwardAndSaveIfNeeded(inwardEntry, true);
				var bondedEntryKey1 = "ENT3243-1";
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey1, 100m);
				var bondedEntryKey2 = "ENT3243-2";
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 200m);

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = "EXW";
				declaration.JE_OH_Importer = inwardDeclaration.JE_OH_Importer;
				var entryInstruction = Factory.New<CusEntryInstruction>();
				entryInstruction.CEI_JE = declaration.PK;
				entryInstruction.CEI_Style = Helper.OutwardCusProcedure.ZZ6_ProcedureCode;
				entryInstruction.CEI_Description = "OUT DESC";
				entryInstruction.CEI_OA_Warehouse = inwardEntry.EntryInstruction.CEI_OA_Warehouse2;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;
				invoiceLine1.JI_PartNo = Helper.Part.OP_PartNum;
				invoiceLine1.JI_BondedWhsQuantity = 50m;
				invoiceLine1.JI_CustomsUnitQty = "KG";
				invoiceLine1.JI_Procedure = Helper.OutwardCusProcedure.ZZ6_ProcedureCode + Helper.OutwardCusProcedure.ZZ6_PreviousProcedureCode;
				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction.PK;
				invoiceLine2.JI_PartNo = Helper.Part2.OP_PartNum;
				invoiceLine2.JI_BondedWhsQuantity = 100m;
				invoiceLine2.JI_CustomsUnitQty = "KG";
				invoiceLine2.JI_Procedure = Helper.OutwardCusProcedure.ZZ6_ProcedureCode + Helper.OutwardCusProcedure.ZZ6_PreviousProcedureCode;

				entryInstruction.UpdateOutwardLinesWithInventoryDetails();
				Helper.AssertInvoiceLine(invoiceLine1, entryInstruction.PK, 50m, "NO", 50m, "NO", 500m, "KG", 0, "", 5000m, Helper.InwardCusProcedure.ZZ6_ProcedureCode);
				Helper.AssertInvoiceLine(invoiceLine2, entryInstruction.PK, 100m, "BX", 100m, "BX", 1000m, "KG", 0, "", 10000m, Helper.InwardCusProcedure.ZZ6_ProcedureCode);

				Factory.ClearCachedValue<bool>();
				var entryInstruction2 = Factory.New<CusEntryInstruction>();
				entryInstruction2.CEI_JE = declaration.PK;
				AssertEquals("No matching Warehouse found.", entryInstruction2.UpdateOutwardLinesWithInventoryDetails());
				entryInstruction2.CEI_OA_Warehouse = inwardEntry.EntryInstruction.CEI_OA_Warehouse2;
				AssertEquals("At least one Invoice Line with valid Previous Entry Details or Part and Countable Quantity is required.", entryInstruction2.UpdateOutwardLinesWithInventoryDetails());
			}
		}

		public void TestIsInventorySelectionEnabled()
		{
			DataRegistry.Business.CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
			Helper.UniversalTariffHelper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "AA", "BB", ZString.Empty, "OP DESC", "EXW", outOfWarehouse: true);
			var procedure2 = Helper.UniversalTariffHelper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "CC", "DD", ZString.Empty, "OP DESC", "EXW", outOfWarehouse: true);
			procedure2.ZZ6_OutOfInwardProcessing = "Y";
			var procedure3 = Helper.UniversalTariffHelper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "EE", "FF", ZString.Empty, "OP DESC", "EXW", outOfWarehouse: true);
			procedure3.ZZ6_OutofOutwardProcessing = "Y";
			Helper.UniversalTariffHelper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "YY", "ZZ", ZString.Empty, "IP DESC", "IMP", outOfWarehouse: false);

			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			invoiceLine.JI_Procedure = "AABB";
			Assert("IsInventorySelectionEnabled true", entryInstruction.IsInventorySelectionEnabled);

			invoiceLine.JI_Procedure = "CCDD";
			Assert("IsInventorySelectionEnabled true", entryInstruction.IsInventorySelectionEnabled);

			invoiceLine.JI_Procedure = "EEFF";
			Assert("IsInventorySelectionEnabled true", entryInstruction.IsInventorySelectionEnabled);

			invoiceLine.JI_Procedure = "YYZZ";
			Assert("IsInventorySelectionEnabled false", !entryInstruction.IsInventorySelectionEnabled);

			var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Procedure = "AABB";
			Assert("IsInventorySelectionEnabled true", entryInstruction.IsInventorySelectionEnabled);
		}

		WhsDataTestHelper Helper => helper ?? (helper = new WhsDataTestHelper(Factory));
		WhsDataTestHelper helper;

		public void TestFromWarehouseCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var warehouse = Factory.New<OrgHeader>();
				var warehouseAddress1 = warehouse.Addresses.AddNew();
				var cusCode1 = warehouse.CustomsCodes.AddNew();
				cusCode1.OK_CustomsRegNo = "DBNSOS78901";
				cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
				cusCode1.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
				cusCode1.OK_OA_PremisesAddress = warehouseAddress1.PK;
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				instruction.CEI_OA_Warehouse_ZAddress.OrgPK = warehouse.PK;
				AssertEquals(warehouseAddress1.PK, instruction.CEI_OA_Warehouse);
				AssertEquals("DBNSOS78901", instruction.FromWarehouseCode);

				instruction.CEI_OA_Warehouse = ZGuid.Empty;
				instruction.CEI_OA_Warehouse_ZAddress.OrgPK = ZGuid.Empty;
				var warehouseAddress2 = warehouse.Addresses.AddNew();
				var cusCode2 = warehouse.CustomsCodes.AddNew();
				cusCode2.OK_CustomsRegNo = "DBNSOS78902";
				cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
				cusCode2.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
				cusCode2.OK_OA_PremisesAddress = warehouseAddress2.PK;
				instruction.CEI_OA_Warehouse_ZAddress.OrgPK = warehouse.PK;
				AssertEquals(ZGuid.Empty, instruction.CEI_OA_Warehouse);
				AssertEquals(ZString.Empty, instruction.FromWarehouseCode);
			}
		}

		public void TestOrgCusCodeTypeForWarehouse()
		{
			var entryInstruction = Factory.New<CusEntryInstructionForTest>();
			AssertEquals(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, entryInstruction.OrgCusCodeTypeForWarehouseExposed);
		}

		public void TestToWarehouseZAddress()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var warehouse = Factory.New<OrgHeader>();
				var warehouseAddress1 = warehouse.Addresses.AddNew();
				var cusCode1 = warehouse.CustomsCodes.AddNew();
				cusCode1.OK_CustomsRegNo = "DBNSOS78901";
				cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
				cusCode1.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
				cusCode1.OK_OA_PremisesAddress = warehouseAddress1.PK;
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				instruction.CEI_OA_Warehouse2_ZAddress.OrgPK = warehouse.PK;
				AssertEquals(warehouseAddress1.PK, instruction.CEI_OA_Warehouse2);
				AssertEquals("DBNSOS78901", instruction.ToWarehouseCode);

				instruction.CEI_OA_Warehouse2 = ZGuid.Empty;
				instruction.CEI_OA_Warehouse2_ZAddress.OrgPK = ZGuid.Empty;
				var warehouseAddress2 = warehouse.Addresses.AddNew();
				var cusCode2 = warehouse.CustomsCodes.AddNew();
				cusCode2.OK_CustomsRegNo = "DBNSOS78902";
				cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
				cusCode2.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
				cusCode2.OK_OA_PremisesAddress = warehouseAddress2.PK;
				instruction.CEI_OA_Warehouse2_ZAddress.OrgPK = warehouse.PK;
				AssertEquals(ZGuid.Empty, instruction.CEI_OA_Warehouse2);
				AssertEquals(ZString.Empty, instruction.ToWarehouseCode);
			}
		}

		public void TestEntryHeader()
		{
			var testDeclaration = Factory.NewWithValidTestData<BaseJobDeclarationWithEntryInstructions>();
			var testInstruction1 = testDeclaration.CustomsEntryInstructions.AddNew();
			var testInstruction2 = testDeclaration.CustomsEntryInstructions.AddNew();

			AssertEquals(testInstruction1.EntryHeader, null);

			var testInvHeader = testDeclaration.Invoices.AddNew();

			var testLine1 = testInvHeader.InvoiceLines.AddNew();
			testLine1.JI_CEI = testInstruction1.PK;
			var testLine2 = testInvHeader.InvoiceLines.AddNew();
			testLine2.JI_CEI = testInstruction2.PK;

			AssertEquals(testInstruction1.EntryHeader, null);

			var testEntryHeader1 = testDeclaration.CustomsEntryHeaders.AddNew();
			var testEntryLine1 = testEntryHeader1.AllEntryLines.AddNew();

			AssertEquals(testInstruction1.EntryHeader, null);

			testLine1.JI_CL = testEntryLine1.PK;

			AssertEquals(testInstruction1.EntryHeader, testEntryHeader1);

			var testEntryHeader2 = testDeclaration.CustomsEntryHeaders.AddNew();
			var testEntryLine2 = testEntryHeader2.AllEntryLines.AddNew();
			testLine2.JI_CL = testEntryLine2.PK;

			AssertEquals(testInstruction1.EntryHeader, testEntryHeader1);
			AssertEquals(testInstruction2.EntryHeader, testEntryHeader2);

			var testInstruction3 = testDeclaration.CustomsEntryInstructions.AddNew();
			var testEntryHeader3 = testDeclaration.CustomsEntryHeaders.AddNew();
			testEntryHeader3.CH_CEI_Instruction = testInstruction3.PK;
			AssertEquals(testInstruction1.EntryHeader, testEntryHeader1);
			AssertEquals(testInstruction2.EntryHeader, testEntryHeader2);
			AssertEquals(testInstruction3.EntryHeader, testEntryHeader3);
		}

		public void TestIsChangeOfRegimeWarehousing()
		{
			var helper = new WhsDataTestHelper(Factory);
			var data = helper.CreateChangeOfRegimeEntryData();
			var instruction = data.Instruction;
			var invoiceLine = data.InvoiceLine;
			AssertEquals("PreCondiion: invoiceLine.IsChangeOfRegimeWarehousing", true, invoiceLine.IsChangeOfRegimeWarehousing);
			AssertEquals("PreCondiion: instruction.IsChangeOfRegimeWarehousingEnabled", true, instruction.IsChangeOfRegimeWarehousingEnabled);
			AssertEquals("IsChangeOfRegimeWarehousing", true, instruction.IsChangeOfRegimeWarehousing);

			invoiceLine.JI_Procedure = ZString.Empty;
			AssertEquals("invoiceLine.IsChangeOfRegimeWarehousing", false, invoiceLine.IsChangeOfRegimeWarehousing);
			AssertEquals("IsChangeOfRegimeWarehousing", false, instruction.IsChangeOfRegimeWarehousing);

			var procedure = helper.ChangeOfOwnershipCusProcedure;
			invoiceLine.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode;
			AssertEquals("invoiceLine.IsChangeOfRegimeWarehousing", true, invoiceLine.IsChangeOfRegimeWarehousing);
			AssertEquals("IsChangeOfRegimeWarehousing", true, instruction.IsChangeOfRegimeWarehousing);

			instruction.CEI_OH_Owner = helper.Owner.PK;
			AssertEquals("instruction.IsChangeOfRegimeWarehousingEnabled", false, instruction.IsChangeOfRegimeWarehousingEnabled);
			AssertEquals("IsChangeOfRegimeWarehousing", false, instruction.IsChangeOfRegimeWarehousing);
		}

		public void TestHasChangeOfRegimeWarehousing()
		{
			var helper = new WhsDataTestHelper(Factory);
			var data = helper.CreateChangeOfRegimeEntryData();
			var instruction = data.Instruction;
			AssertEquals("OB_CusInventoryForInwardProcessing", true, instruction.HasChangeOfRegimeWarehousing);
			var warehouse2CompanyData = helper.Warehouse2.CompanyData;
			warehouse2CompanyData.OB_IMUsedBondedWhs = true;
			AssertEquals("warehouse2 is InwardProcessing and warehouse is BondWarehouse", true, instruction.HasChangeOfRegimeWarehousing);

			warehouse2CompanyData.OB_CusInventoryForInwardProcessing = false;
			AssertEquals("warehouse2 is not InwardProcessing and warehouse is BondWarehouse", false, instruction.HasChangeOfRegimeWarehousing);

			var warehouseCompanyData = helper.Warehouse.CompanyData;
			warehouseCompanyData.OB_CusInventoryForInwardProcessing = true;
			AssertEquals("warehouse is InwardProcessing and warehouse2 is BondWarehouse", true, instruction.HasChangeOfRegimeWarehousing);

			warehouseCompanyData.OB_IMUsedBondedWhs = false;
			AssertEquals("warehouse is InwardProcessing and warehouse2 is BondWarehouse", true, instruction.HasChangeOfRegimeWarehousing);

			// this is optional check as at this stage we expect warehouse client to have both flagged
			var importerCompanyData = helper.Importer.CompanyData;
			importerCompanyData.OB_IMUsedBondedWhs = false;
			AssertEquals("client is not BondWarehouse", false, instruction.HasChangeOfRegimeWarehousing);

			importerCompanyData.OB_IMUsedBondedWhs = true;
			importerCompanyData.OB_CusInventoryForInwardProcessing = false;
			AssertEquals("client is not InwardProcessing", false, instruction.HasChangeOfRegimeWarehousing);
		}

		public void TestIsChangeOfRegimeWarehousingEnabled()
		{
			var helper = new WhsDataTestHelper(Factory);
			var data = helper.CreateChangeOfRegimeEntryData();
			var instruction = data.Instruction;
			AssertEquals("IsChangeOfRegimeWarehousingEnabled", true, instruction.IsChangeOfRegimeWarehousingEnabled);

			instruction.CEI_OH_Owner = helper.Owner.PK;
			AssertEquals("IsChangeOfRegimeWarehousingEnabled - Owner is not null", false, instruction.IsChangeOfRegimeWarehousingEnabled);

			instruction.CEI_OH_Owner = ZGuid.Empty;
			data.InstructionMock.Protected().Setup<bool>("IsChangeOfRegimeWarehousingEnabledCore").Returns(false);
			AssertEquals("IsChangeOfRegimeWarehousingEnabled is disable", false, instruction.IsChangeOfRegimeWarehousingEnabled);
		}

		public void TestWarehousingProperties()
		{
			var cusProcedureAB11OutOfWhs = Factory.New<RefCusProcedure>();
			cusProcedureAB11OutOfWhs.ZZ6_ProcedureCode = "AB";
			cusProcedureAB11OutOfWhs.ZZ6_PreviousProcedureCode = "11";
			cusProcedureAB11OutOfWhs.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			cusProcedureAB11OutOfWhs.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			cusProcedureAB11OutOfWhs.ZZ6_Description = "AB DESC";
			var cusProcedureAB22IntoWhs = Factory.New<RefCusProcedure>();
			cusProcedureAB22IntoWhs.ZZ6_ProcedureCode = "AB";
			cusProcedureAB22IntoWhs.ZZ6_PreviousProcedureCode = "22";
			cusProcedureAB22IntoWhs.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			cusProcedureAB22IntoWhs.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			cusProcedureAB22IntoWhs.ZZ6_Description = "AB DESC";
			var cusProcedureAB33OutOfInwardWhs = Factory.New<RefCusProcedure>();
			cusProcedureAB33OutOfInwardWhs.ZZ6_ProcedureCode = "AB";
			cusProcedureAB33OutOfInwardWhs.ZZ6_PreviousProcedureCode = "33";
			cusProcedureAB33OutOfInwardWhs.ZZ6_OutOfInwardProcessing = WarehouseMoveStatus.Codes.Yes;
			cusProcedureAB33OutOfInwardWhs.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			cusProcedureAB33OutOfInwardWhs.ZZ6_Description = "AB DESC";
			var cusProcedureAB44IntoInwardWhs = Factory.New<RefCusProcedure>();
			cusProcedureAB44IntoInwardWhs.ZZ6_ProcedureCode = "AB";
			cusProcedureAB44IntoInwardWhs.ZZ6_PreviousProcedureCode = "44";
			cusProcedureAB44IntoInwardWhs.ZZ6_IntoInwardProcessing = WarehouseMoveStatus.Codes.Yes;
			cusProcedureAB44IntoInwardWhs.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			cusProcedureAB44IntoInwardWhs.ZZ6_Description = "AB DESC";
			var cusProcedureAB55OutOfOutwardWhs = Factory.New<RefCusProcedure>();
			cusProcedureAB55OutOfOutwardWhs.ZZ6_ProcedureCode = "AB";
			cusProcedureAB55OutOfOutwardWhs.ZZ6_PreviousProcedureCode = "55";
			cusProcedureAB55OutOfOutwardWhs.ZZ6_OutofOutwardProcessing = WarehouseMoveStatus.Codes.Yes;
			cusProcedureAB55OutOfOutwardWhs.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			cusProcedureAB55OutOfOutwardWhs.ZZ6_Description = "AB DESC";
			var cusProcedureAB66IntoOutwardWhs = Factory.New<RefCusProcedure>();
			cusProcedureAB66IntoOutwardWhs.ZZ6_ProcedureCode = "AB";
			cusProcedureAB66IntoOutwardWhs.ZZ6_PreviousProcedureCode = "66";
			cusProcedureAB66IntoOutwardWhs.ZZ6_IntoOutwardProcessing = WarehouseMoveStatus.Codes.Yes;
			cusProcedureAB66IntoOutwardWhs.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			cusProcedureAB66IntoOutwardWhs.ZZ6_Description = "AB DESC";
			var cusProcedureAB88IntoAndOutOfWhs = Factory.New<RefCusProcedure>();
			cusProcedureAB88IntoAndOutOfWhs.ZZ6_ProcedureCode = "AB";
			cusProcedureAB88IntoAndOutOfWhs.ZZ6_PreviousProcedureCode = "88";
			cusProcedureAB88IntoAndOutOfWhs.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			cusProcedureAB88IntoAndOutOfWhs.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			cusProcedureAB88IntoAndOutOfWhs.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			cusProcedureAB88IntoAndOutOfWhs.ZZ6_Description = "AB DESC";
			var helper = new WhsDataTestHelper(Factory);
			helper.Importer.CompanyData.OB_IMUsedBondedWhs = false;
			helper.Owner.CompanyData.OB_IMUsedBondedWhs = false;
			helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;
			helper.Warehouse2.CompanyData.OB_IMUsedBondedWhs = false;
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = helper.Importer.PK;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "AB";
			instruction.CEI_OH_Owner = helper.Owner.PK;
			instruction.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
			instruction.CEI_OA_Warehouse2 = helper.Warehouse2.MainAddress.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;

			AssertEquals("HasBothOutOfAndIntoRegimeProcedure", false, instruction.HasBothOutOfAndIntoRegimeProcedure);
			AssertEquals("HasIntoWarehouseProcedure", false, instruction.HasIntoWarehouseProcedure);
			AssertEquals("HasOutOfWarehouseProcedure", false, instruction.HasOutOfWarehouseProcedure);
			AssertEquals("IsOutOfWarehouseWarehousing", false, instruction.IsOutOfWarehouseWarehousing);
			AssertEquals("IsIntoWarehouseWarehousing", false, instruction.IsIntoWarehouseWarehousing);
			AssertEquals("IsNonWarehousing", true, instruction.IsNonWarehousing);
			AssertEquals("IsChangeOfOwnershipWarehousing", false, instruction.IsChangeOfOwnershipWarehousing);
			AssertEquals("OwnerIsBondedWarehousing", false, instruction.OwnerIsBondedWarehousing);
			AssertEquals("ClientIsBondedWarehousing", false, instruction.ClientIsBondedWarehousing);
			AssertEquals("WarehouseIsBondedWarehousing", true, instruction.WarehouseIsBondedWarehousing);
			AssertEquals("Warehouse2IsBondedWarehousing", false, instruction.Warehouse2IsBondedWarehousing);

			invoiceLine1.JI_Procedure = invoiceLine1.EntryInstruction.CEI_Style + "11";
			AssertEquals("HasBothOutOfAndIntoRegimeProcedure", false, instruction.HasBothOutOfAndIntoRegimeProcedure);
			AssertEquals("HasIntoWarehouseProcedure", false, instruction.HasIntoWarehouseProcedure);
			AssertEquals("HasOutOfWarehouseProcedure", true, instruction.HasOutOfWarehouseProcedure);
			AssertEquals("IsOutOfWarehouseWarehousing", true, instruction.IsOutOfWarehouseWarehousing);
			AssertEquals("IsIntoWarehouseWarehousing", false, instruction.IsIntoWarehouseWarehousing);
			AssertEquals("IsNonWarehousing", true, instruction.IsNonWarehousing);
			AssertEquals("IsChangeOfOwnershipWarehousing", false, instruction.IsChangeOfOwnershipWarehousing);
			AssertEquals("OwnerIsBondedWarehousing", false, instruction.OwnerIsBondedWarehousing);
			AssertEquals("ClientIsBondedWarehousing", false, instruction.ClientIsBondedWarehousing);
			AssertEquals("WarehouseIsBondedWarehousing", true, instruction.WarehouseIsBondedWarehousing);
			AssertEquals("Warehouse2IsBondedWarehousing", false, instruction.Warehouse2IsBondedWarehousing);

			invoiceLine1.JI_Procedure = invoiceLine1.EntryInstruction.CEI_Style + "33";
			AssertEquals("HasBothOutOfAndIntoRegimeProcedure", false, instruction.HasBothOutOfAndIntoRegimeProcedure);
			AssertEquals("HasIntoWarehouseProcedure", false, instruction.HasIntoWarehouseProcedure);
			AssertEquals("HasOutOfWarehouseProcedure", false, instruction.HasOutOfWarehouseProcedure);
			AssertEquals("IsOutOfWarehouseWarehousing", false, instruction.IsOutOfWarehouseWarehousing);
			AssertEquals("IsIntoWarehouseWarehousing", false, instruction.IsIntoWarehouseWarehousing);
			AssertEquals("IsNonWarehousing", true, instruction.IsNonWarehousing);
			AssertEquals("IsChangeOfOwnershipWarehousing", false, instruction.IsChangeOfOwnershipWarehousing);
			AssertEquals("OwnerIsBondedWarehousing", false, instruction.OwnerIsBondedWarehousing);
			AssertEquals("ClientIsBondedWarehousing", false, instruction.ClientIsBondedWarehousing);
			AssertEquals("WarehouseIsBondedWarehousing", true, instruction.WarehouseIsBondedWarehousing);
			AssertEquals("Warehouse2IsBondedWarehousing", false, instruction.Warehouse2IsBondedWarehousing);

			invoiceLine1.JI_Procedure = invoiceLine1.EntryInstruction.CEI_Style + "55";
			AssertEquals("HasBothOutOfAndIntoRegimeProcedure", false, instruction.HasBothOutOfAndIntoRegimeProcedure);
			AssertEquals("HasIntoWarehouseProcedure", false, instruction.HasIntoWarehouseProcedure);
			AssertEquals("HasOutOfWarehouseProcedure", false, instruction.HasOutOfWarehouseProcedure);
			AssertEquals("IsOutOfWarehouseWarehousing", false, instruction.IsOutOfWarehouseWarehousing);
			AssertEquals("IsIntoWarehouseWarehousing", false, instruction.IsIntoWarehouseWarehousing);
			AssertEquals("IsNonWarehousing", true, instruction.IsNonWarehousing);
			AssertEquals("IsChangeOfOwnershipWarehousing", false, instruction.IsChangeOfOwnershipWarehousing);
			AssertEquals("OwnerIsBondedWarehousing", false, instruction.OwnerIsBondedWarehousing);
			AssertEquals("ClientIsBondedWarehousing", false, instruction.ClientIsBondedWarehousing);
			AssertEquals("WarehouseIsBondedWarehousing", true, instruction.WarehouseIsBondedWarehousing);
			AssertEquals("Warehouse2IsBondedWarehousing", false, instruction.Warehouse2IsBondedWarehousing);

			invoiceLine2.JI_Procedure = invoiceLine2.EntryInstruction.CEI_Style + "22";
			AssertEquals("HasBothOutOfAndIntoRegimeProcedure", false, instruction.HasBothOutOfAndIntoRegimeProcedure);
			AssertEquals("HasIntoWarehouseProcedure", true, instruction.HasIntoWarehouseProcedure);
			AssertEquals("HasOutOfWarehouseProcedure", false, instruction.HasOutOfWarehouseProcedure);
			AssertEquals("IsOutOfWarehouseWarehousing", false, instruction.IsOutOfWarehouseWarehousing);
			AssertEquals("IsIntoWarehouseWarehousing", true, instruction.IsIntoWarehouseWarehousing);
			AssertEquals("IsNonWarehousing", false, instruction.IsNonWarehousing);
			AssertEquals("IsChangeOfOwnershipWarehousing", false, instruction.IsChangeOfOwnershipWarehousing);
			AssertEquals("OwnerIsBondedWarehousing", false, instruction.OwnerIsBondedWarehousing);
			AssertEquals("ClientIsBondedWarehousing", false, instruction.ClientIsBondedWarehousing);
			AssertEquals("WarehouseIsBondedWarehousing", true, instruction.WarehouseIsBondedWarehousing);
			AssertEquals("Warehouse2IsBondedWarehousing", false, instruction.Warehouse2IsBondedWarehousing);

			invoiceLine2.JI_Procedure = invoiceLine2.EntryInstruction.CEI_Style + "44";
			AssertEquals("HasBothOutOfAndIntoRegimeProcedure", false, instruction.HasBothOutOfAndIntoRegimeProcedure);
			AssertEquals("HasIntoWarehouseProcedure", false, instruction.HasIntoWarehouseProcedure);
			AssertEquals("HasOutOfWarehouseProcedure", false, instruction.HasOutOfWarehouseProcedure);
			AssertEquals("IsOutOfWarehouseWarehousing", false, instruction.IsOutOfWarehouseWarehousing);
			AssertEquals("IsIntoWarehouseWarehousing", false, instruction.IsIntoWarehouseWarehousing);
			AssertEquals("IsNonWarehousing", false, instruction.IsNonWarehousing);
			AssertEquals("IsChangeOfOwnershipWarehousing", false, instruction.IsChangeOfOwnershipWarehousing);
			AssertEquals("OwnerIsBondedWarehousing", false, instruction.OwnerIsBondedWarehousing);
			AssertEquals("ClientIsBondedWarehousing", false, instruction.ClientIsBondedWarehousing);
			AssertEquals("WarehouseIsBondedWarehousing", true, instruction.WarehouseIsBondedWarehousing);
			AssertEquals("Warehouse2IsBondedWarehousing", false, instruction.Warehouse2IsBondedWarehousing);

			invoiceLine2.JI_Procedure = invoiceLine2.EntryInstruction.CEI_Style + "66";
			AssertEquals("HasBothOutOfAndIntoRegimeProcedure", false, instruction.HasBothOutOfAndIntoRegimeProcedure);
			AssertEquals("HasIntoWarehouseProcedure", false, instruction.HasIntoWarehouseProcedure);
			AssertEquals("HasOutOfWarehouseProcedure", false, instruction.HasOutOfWarehouseProcedure);
			AssertEquals("IsOutOfWarehouseWarehousing", false, instruction.IsOutOfWarehouseWarehousing);
			AssertEquals("IsIntoWarehouseWarehousing", false, instruction.IsIntoWarehouseWarehousing);
			AssertEquals("IsNonWarehousing", false, instruction.IsNonWarehousing);
			AssertEquals("IsChangeOfOwnershipWarehousing", false, instruction.IsChangeOfOwnershipWarehousing);
			AssertEquals("OwnerIsBondedWarehousing", false, instruction.OwnerIsBondedWarehousing);
			AssertEquals("ClientIsBondedWarehousing", false, instruction.ClientIsBondedWarehousing);
			AssertEquals("WarehouseIsBondedWarehousing", true, instruction.WarehouseIsBondedWarehousing);
			AssertEquals("Warehouse2IsBondedWarehousing", false, instruction.Warehouse2IsBondedWarehousing);

			instruction.CEI_Style = "CD";
			invoiceLine1.JI_Procedure = "CD12";
			invoiceLine2.JI_Procedure = "CD34";
			AssertEquals("HasBothOutOfAndIntoRegimeProcedure", false, instruction.HasBothOutOfAndIntoRegimeProcedure);
			AssertEquals("HasIntoWarehouseProcedure", false, instruction.HasIntoWarehouseProcedure);
			AssertEquals("HasOutOfWarehouseProcedure", false, instruction.HasOutOfWarehouseProcedure);
			AssertEquals("IsOutOfWarehouseWarehousing", false, instruction.IsOutOfWarehouseWarehousing);
			AssertEquals("IsIntoWarehouseWarehousing", false, instruction.IsIntoWarehouseWarehousing);
			AssertEquals("IsNonWarehousing", true, instruction.IsNonWarehousing);
			AssertEquals("IsChangeOfOwnershipWarehousing", false, instruction.IsChangeOfOwnershipWarehousing);
			AssertEquals("OwnerIsBondedWarehousing", false, instruction.OwnerIsBondedWarehousing);
			AssertEquals("ClientIsBondedWarehousing", false, instruction.ClientIsBondedWarehousing);
			AssertEquals("WarehouseIsBondedWarehousing", true, instruction.WarehouseIsBondedWarehousing);
			AssertEquals("Warehouse2IsBondedWarehousing", false, instruction.Warehouse2IsBondedWarehousing);

			instruction.CEI_Style = "AB";
			invoiceLine1.JI_Procedure = "AB11";
			invoiceLine2.JI_Procedure = "AB22";
			AssertEquals("HasBothOutOfAndIntoRegimeProcedure", false, instruction.HasBothOutOfAndIntoRegimeProcedure);
			AssertEquals("HasIntoWarehouseProcedure", true, instruction.HasIntoWarehouseProcedure);
			AssertEquals("HasOutOfWarehouseProcedure", true, instruction.HasOutOfWarehouseProcedure);
			AssertEquals("IsOutOfWarehouseWarehousing", true, instruction.IsOutOfWarehouseWarehousing);
			AssertEquals("IsIntoWarehouseWarehousing", true, instruction.IsIntoWarehouseWarehousing);
			AssertEquals("IsNonWarehousing", false, instruction.IsNonWarehousing);
			AssertEquals("IsChangeOfOwnershipWarehousing", false, instruction.IsChangeOfOwnershipWarehousing);
			AssertEquals("OwnerIsBondedWarehousing", false, instruction.OwnerIsBondedWarehousing);
			AssertEquals("ClientIsBondedWarehousing", false, instruction.ClientIsBondedWarehousing);
			AssertEquals("WarehouseIsBondedWarehousing", true, instruction.WarehouseIsBondedWarehousing);
			AssertEquals("Warehouse2IsBondedWarehousing", false, instruction.Warehouse2IsBondedWarehousing);

			invoiceLine1.JI_CEI = ZGuid.Empty;
			AssertEquals("HasBothOutOfAndIntoRegimeProcedure", false, instruction.HasBothOutOfAndIntoRegimeProcedure);
			AssertEquals("HasIntoWarehouseProcedure", true, instruction.HasIntoWarehouseProcedure);
			AssertEquals("HasOutOfWarehouseProcedure", false, instruction.HasOutOfWarehouseProcedure);
			AssertEquals("IsOutOfWarehouseWarehousing", false, instruction.IsOutOfWarehouseWarehousing);
			AssertEquals("IsIntoWarehouseWarehousing", true, instruction.IsIntoWarehouseWarehousing);
			AssertEquals("IsNonWarehousing", false, instruction.IsNonWarehousing);
			AssertEquals("IsChangeOfOwnershipWarehousing", false, instruction.IsChangeOfOwnershipWarehousing);
			AssertEquals("OwnerIsBondedWarehousing", false, instruction.OwnerIsBondedWarehousing);
			AssertEquals("ClientIsBondedWarehousing", false, instruction.ClientIsBondedWarehousing);
			AssertEquals("WarehouseIsBondedWarehousing", true, instruction.WarehouseIsBondedWarehousing);
			AssertEquals("Warehouse2IsBondedWarehousing", false, instruction.Warehouse2IsBondedWarehousing);

			invoiceLine2.JI_Procedure = "AB88";
			AssertEquals("HasBothOutOfAndIntoRegimeProcedure", true, instruction.HasBothOutOfAndIntoRegimeProcedure);
			AssertEquals("HasIntoWarehouseProcedure", true, instruction.HasIntoWarehouseProcedure);
			AssertEquals("HasOutOfWarehouseProcedure", true, instruction.HasOutOfWarehouseProcedure);
			AssertEquals("IsOutOfWarehouseWarehousing", false, instruction.IsOutOfWarehouseWarehousing);
			AssertEquals("IsIntoWarehouseWarehousing", false, instruction.IsIntoWarehouseWarehousing);
			AssertEquals("IsNonWarehousing", false, instruction.IsNonWarehousing);
			AssertEquals("IsChangeOfOwnershipWarehousing", true, instruction.IsChangeOfOwnershipWarehousing);
			AssertEquals("OwnerIsBondedWarehousing", false, instruction.OwnerIsBondedWarehousing);
			AssertEquals("ClientIsBondedWarehousing", false, instruction.ClientIsBondedWarehousing);
			AssertEquals("WarehouseIsBondedWarehousing", true, instruction.WarehouseIsBondedWarehousing);
			AssertEquals("Warehouse2IsBondedWarehousing", false, instruction.Warehouse2IsBondedWarehousing);

			helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = false;
			AssertEquals("HasBothOutOfAndIntoRegimeProcedure", true, instruction.HasBothOutOfAndIntoRegimeProcedure);
			AssertEquals("HasIntoWarehouseProcedure", true, instruction.HasIntoWarehouseProcedure);
			AssertEquals("HasOutOfWarehouseProcedure", true, instruction.HasOutOfWarehouseProcedure);
			AssertEquals("IsOutOfWarehouseWarehousing", false, instruction.IsOutOfWarehouseWarehousing);
			AssertEquals("IsIntoWarehouseWarehousing", false, instruction.IsIntoWarehouseWarehousing);
			AssertEquals("IsNonWarehousing", false, instruction.IsNonWarehousing);
			AssertEquals("IsChangeOfOwnershipWarehousing", false, instruction.IsChangeOfOwnershipWarehousing);
			AssertEquals("OwnerIsBondedWarehousing", false, instruction.OwnerIsBondedWarehousing);
			AssertEquals("ClientIsBondedWarehousing", false, instruction.ClientIsBondedWarehousing);
			AssertEquals("WarehouseIsBondedWarehousing", false, instruction.WarehouseIsBondedWarehousing);
			AssertEquals("Warehouse2IsBondedWarehousing", false, instruction.Warehouse2IsBondedWarehousing);

			helper.Owner.CompanyData.OB_IMUsedBondedWhs = true;
			AssertEquals("HasBothOutOfAndIntoRegimeProcedure", true, instruction.HasBothOutOfAndIntoRegimeProcedure);
			AssertEquals("HasIntoWarehouseProcedure", true, instruction.HasIntoWarehouseProcedure);
			AssertEquals("HasOutOfWarehouseProcedure", true, instruction.HasOutOfWarehouseProcedure);
			AssertEquals("IsOutOfWarehouseWarehousing", false, instruction.IsOutOfWarehouseWarehousing);
			AssertEquals("IsIntoWarehouseWarehousing", true, instruction.IsIntoWarehouseWarehousing);
			AssertEquals("IsNonWarehousing", false, instruction.IsNonWarehousing);
			AssertEquals("IsChangeOfOwnershipWarehousing", false, instruction.IsChangeOfOwnershipWarehousing);
			AssertEquals("OwnerIsBondedWarehousing", true, instruction.OwnerIsBondedWarehousing);
			AssertEquals("ClientIsBondedWarehousing", false, instruction.ClientIsBondedWarehousing);
			AssertEquals("WarehouseIsBondedWarehousing", false, instruction.WarehouseIsBondedWarehousing);
			AssertEquals("Warehouse2IsBondedWarehousing", false, instruction.Warehouse2IsBondedWarehousing);

			helper.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			AssertEquals("HasBothOutOfAndIntoRegimeProcedure", true, instruction.HasBothOutOfAndIntoRegimeProcedure);
			AssertEquals("HasIntoWarehouseProcedure", true, instruction.HasIntoWarehouseProcedure);
			AssertEquals("HasOutOfWarehouseProcedure", true, instruction.HasOutOfWarehouseProcedure);
			AssertEquals("IsOutOfWarehouseWarehousing", false, instruction.IsOutOfWarehouseWarehousing);
			AssertEquals("IsIntoWarehouseWarehousing", false, instruction.IsIntoWarehouseWarehousing);
			AssertEquals("IsNonWarehousing", false, instruction.IsNonWarehousing);
			AssertEquals("IsChangeOfOwnershipWarehousing", true, instruction.IsChangeOfOwnershipWarehousing);
			AssertEquals("OwnerIsBondedWarehousing", true, instruction.OwnerIsBondedWarehousing);
			AssertEquals("ClientIsBondedWarehousing", true, instruction.ClientIsBondedWarehousing);
			AssertEquals("WarehouseIsBondedWarehousing", false, instruction.WarehouseIsBondedWarehousing);
			AssertEquals("Warehouse2IsBondedWarehousing", false, instruction.Warehouse2IsBondedWarehousing);

			helper.Owner.CompanyData.OB_IMUsedBondedWhs = false;
			AssertEquals("HasBothOutOfAndIntoRegimeProcedure", true, instruction.HasBothOutOfAndIntoRegimeProcedure);
			AssertEquals("HasIntoWarehouseProcedure", true, instruction.HasIntoWarehouseProcedure);
			AssertEquals("HasOutOfWarehouseProcedure", true, instruction.HasOutOfWarehouseProcedure);
			AssertEquals("IsOutOfWarehouseWarehousing", true, instruction.IsOutOfWarehouseWarehousing);
			AssertEquals("IsIntoWarehouseWarehousing", false, instruction.IsIntoWarehouseWarehousing);
			AssertEquals("IsNonWarehousing", false, instruction.IsNonWarehousing);
			AssertEquals("IsChangeOfOwnershipWarehousing", false, instruction.IsChangeOfOwnershipWarehousing);
			AssertEquals("OwnerIsBondedWarehousing", false, instruction.OwnerIsBondedWarehousing);
			AssertEquals("ClientIsBondedWarehousing", true, instruction.ClientIsBondedWarehousing);
			AssertEquals("WarehouseIsBondedWarehousing", false, instruction.WarehouseIsBondedWarehousing);
			AssertEquals("Warehouse2IsBondedWarehousing", false, instruction.Warehouse2IsBondedWarehousing);

			helper.Warehouse2.CompanyData.OB_IMUsedBondedWhs = true;
			AssertEquals("HasBothOutOfAndIntoRegimeProcedure", true, instruction.HasBothOutOfAndIntoRegimeProcedure);
			AssertEquals("HasIntoWarehouseProcedure", true, instruction.HasIntoWarehouseProcedure);
			AssertEquals("HasOutOfWarehouseProcedure", true, instruction.HasOutOfWarehouseProcedure);
			AssertEquals("IsOutOfWarehouseWarehousing", false, instruction.IsOutOfWarehouseWarehousing);
			AssertEquals("IsIntoWarehouseWarehousing", false, instruction.IsIntoWarehouseWarehousing);
			AssertEquals("IsNonWarehousing", false, instruction.IsNonWarehousing);
			AssertEquals("IsChangeOfOwnershipWarehousing", true, instruction.IsChangeOfOwnershipWarehousing);
			AssertEquals("OwnerIsBondedWarehousing", false, instruction.OwnerIsBondedWarehousing);
			AssertEquals("ClientIsBondedWarehousing", true, instruction.ClientIsBondedWarehousing);
			AssertEquals("WarehouseIsBondedWarehousing", false, instruction.WarehouseIsBondedWarehousing);
			AssertEquals("Warehouse2IsBondedWarehousing", true, instruction.Warehouse2IsBondedWarehousing);

			invoiceLine2.JI_Procedure = "**##";
			AssertEquals("HasBothOutOfAndIntoRegimeProcedure", false, instruction.HasBothOutOfAndIntoRegimeProcedure);
			AssertEquals("HasIntoWarehouseProcedure", false, instruction.HasIntoWarehouseProcedure);
			AssertEquals("HasOutOfWarehouseProcedure", false, instruction.HasOutOfWarehouseProcedure);
			AssertEquals("IsOutOfWarehouseWarehousing", false, instruction.IsOutOfWarehouseWarehousing);
			AssertEquals("IsIntoWarehouseWarehousing", false, instruction.IsIntoWarehouseWarehousing);
			AssertEquals("IsNonWarehousing", true, instruction.IsNonWarehousing);
			AssertEquals("IsChangeOfOwnershipWarehousing", false, instruction.IsChangeOfOwnershipWarehousing);
			AssertEquals("OwnerIsBondedWarehousing", false, instruction.OwnerIsBondedWarehousing);
			AssertEquals("ClientIsBondedWarehousing", true, instruction.ClientIsBondedWarehousing);
			AssertEquals("WarehouseIsBondedWarehousing", false, instruction.WarehouseIsBondedWarehousing);
			AssertEquals("Warehouse2IsBondedWarehousing", true, instruction.Warehouse2IsBondedWarehousing);
		}

		public void TestProcedureCodeOnInvoiceLine()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var testDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var testInstruction1 = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				var testInstruction2 = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInstruction1.CEI_Style = "11";
				testInstruction2.CEI_Style = "13";

				var testInvHeader = testDeclaration.Invoices.AddNew();
				var testLine1 = testInvHeader.InvoiceLines.AddNew();

				AssertEquals("Line 1 Instruction PK", ZGuid.Empty, testLine1.JI_CEI);
				var line1Lookup = testLine1.Lookups.CustomsProcedureCodes;
				AssertEquals("Line 1 Lookup", 2, line1Lookup.Count);
				Assert("Line lookup content - 11", line1Lookup.ContainsCode("11"));
				Assert("Line lookup content - 13", line1Lookup.ContainsCode("13"));

				testLine1.JI_CEI = testInstruction2.PK;
				AssertEquals("Change to Valid Code, Line 1 Instruction PK", testInstruction2.PK, testLine1.JI_CEI);

				var testLine2 = testInvHeader.InvoiceLines.AddNew();
				AssertEquals("Line 2 Instruction PK", testInstruction2.PK, testLine2.JI_CEI);

				testInstruction2.CEI_Style = "60";
				AssertEquals("Change Instruction Code, Line 1 Instruction PK", testInstruction2.PK, testLine1.JI_CEI);
				AssertEquals("Change Instruction Code, Line 2 Instruction PK", testInstruction2.PK, testLine2.JI_CEI);
				line1Lookup = testLine1.Lookups.CustomsProcedureCodes;
				AssertEquals("Line 1 Lookup", 2, line1Lookup.Count);
				Assert("Line lookup content - 11", line1Lookup.ContainsCode("11"));
				Assert("Line lookup content - 60", line1Lookup.ContainsCode("60"));

				testLine1.JI_CEI = testInstruction1.PK;
				AssertEquals("Line 1 Instruction PK", testInstruction1.PK, testLine1.JI_CEI);
				AssertEquals("Line 2 Instruction PK", testInstruction2.PK, testLine2.JI_CEI);

				var testInstruction3 = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInstruction3.CEI_Style = "50";
				line1Lookup = testLine1.Lookups.CustomsProcedureCodes;
				AssertEquals("Line 1 Lookup", 3, line1Lookup.Count);
				Assert("Line lookup content - 11", line1Lookup.ContainsCode("11"));
				Assert("Line lookup content - 50", line1Lookup.ContainsCode("50"));
				Assert("Line lookup content - 60", line1Lookup.ContainsCode("60"));

				testInstruction1.Delete();
				line1Lookup = testLine1.Lookups.CustomsProcedureCodes;
				AssertEquals("Line 1 Lookup", 2, line1Lookup.Count);
				Assert("Line lookup content - 50", line1Lookup.ContainsCode("50"));
				Assert("Line lookup content - 60", line1Lookup.ContainsCode("60"));
				AssertEquals("Delete Instruction: Line 1 Instruction PK", ZGuid.Empty, testLine1.JI_CEI);
				AssertEquals("Delete Instruction: Line 2 Instruction PK", testInstruction2.PK, testLine2.JI_CEI);
			}
		}

		public void TestSortedEntryInstructionListIsUpdatedCorrectly()
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var instruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = "11";
			instruction1.CEI_Description = "11 DESC";
			instruction2.CEI_Style = "12";
			instruction2.CEI_Description = "12 DESC";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();

			AssertEquals("Line 1 Instruction PK", ZGuid.Empty, invoiceLine1.JI_CEI);
			var line1Lookup = invoiceLine1.Lookups.CustomsProcedureCodes;
			AssertEquals("Line 1 Lookup", 2, line1Lookup.Count);
			AssertEquals("Line lookup content - 11", "11 DESC", line1Lookup.GetDescriptionFromCode("11"));
			AssertEquals("Line lookup content - 12", "12 DESC", line1Lookup.GetDescriptionFromCode("12"));

			invoiceLine1.JI_CEI = instruction2.PK;
			AssertEquals("Change to Valid Code, Line 1 Instruction PK", instruction2.PK, invoiceLine1.JI_CEI);

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			AssertEquals("Line 2 Instruction PK", instruction2.PK, invoiceLine2.JI_CEI);

			instruction2.CEI_Style = "60";
			AssertEquals("Change Instruction Code, Line 1 Instruction PK", instruction2.PK, invoiceLine1.JI_CEI);
			AssertEquals("Change Instruction Code, Line 2 Instruction PK", instruction2.PK, invoiceLine2.JI_CEI);
			line1Lookup = invoiceLine1.Lookups.CustomsProcedureCodes;
			AssertEquals("Line 1 Lookup", 2, line1Lookup.Count);
			AssertEquals("Line lookup content - 11", "11 DESC", line1Lookup.GetDescriptionFromCode("11"));
			AssertEquals("Line lookup content - 60", "12 DESC", line1Lookup.GetDescriptionFromCode("60"));

			invoiceLine1.JI_CEI = instruction1.PK;
			AssertEquals("Line 1 Instruction PK", instruction1.PK, invoiceLine1.JI_CEI);
			AssertEquals("Line 2 Instruction PK", instruction2.PK, invoiceLine2.JI_CEI);

			var instruction3 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction3.CEI_Style = "50";
			instruction3.CEI_Description = "50 DESC";

			line1Lookup = invoiceLine1.Lookups.CustomsProcedureCodes;
			AssertEquals("Line 1 Lookup", 3, line1Lookup.Count);
			AssertEquals("Line lookup content - 11", "11 DESC", line1Lookup.GetDescriptionFromCode("11"));
			AssertEquals("Line lookup content - 50", "50 DESC", line1Lookup.GetDescriptionFromCode("50"));
			AssertEquals("Line lookup content - 60", "12 DESC", line1Lookup.GetDescriptionFromCode("60"));

			instruction1.Delete();
			line1Lookup = invoiceLine1.Lookups.CustomsProcedureCodes;
			AssertEquals("Line 1 Lookup", 2, line1Lookup.Count);
			AssertEquals("Line lookup content - 50", "50 DESC", line1Lookup.GetDescriptionFromCode("50"));
			AssertEquals("Line lookup content - 60", "12 DESC", line1Lookup.GetDescriptionFromCode("60"));
			AssertEquals("Delete Instruction: Line 1 Instruction PK", ZGuid.Empty, invoiceLine1.JI_CEI);
			AssertEquals("Delete Instruction: Line 2 Instruction PK", instruction2.PK, invoiceLine2.JI_CEI);

			instruction2.CEI_Description = "60 DESC";
			line1Lookup = invoiceLine1.Lookups.CustomsProcedureCodes;
			AssertEquals("Line 1 Lookup", 2, line1Lookup.Count);
			AssertEquals("Line lookup content - 50", "50 DESC", line1Lookup.GetDescriptionFromCode("50"));
			AssertEquals("Line lookup content - 60", "60 DESC", line1Lookup.GetDescriptionFromCode("60"));
		}

		public void TestHasInvoiceLineWithPPC()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclarationWithEntryInstructions>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			var invoice = declaration.Invoices.AddNew();
			var line1 = invoice.InvoiceLines.AddNew();
			line1.JI_CEI = instruction.PK;
			line1.JI_Procedure = "0020";
			var line2 = invoice.InvoiceLines.AddNew();
			line2.JI_CEI = instruction.PK;
			line2.JI_Procedure = "0041";

			AssertEquals("No Procedure", false, instruction.HasInvoiceLineWithPPC("00"));
			AssertEquals("Has 20", true, instruction.HasInvoiceLineWithPPC("20"));
			AssertEquals("Has 41", true, instruction.HasInvoiceLineWithPPC("41"));
		}

		public void TestInvoiceLinesShouldBeClearedIfEntryInstructionIsDeleted()
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			AssertEquals("Precondition", instruction.PK, invoiceLine.JI_CEI);

			instruction.Delete();
			AssertEquals("JI_CEI of invoice line should be set as empty.", ZGuid.Empty, invoiceLine.JI_CEI);
		}

		public void TestInvoiceLinesShouldBeClearedIfEntryInstructionIsDeletedFromAnotherFactory()
		{
			var factory1 = new BusinessObjectFactory();
			var declaration = factory1.New<BaseJobDeclarationWithEntryInstructions>();
			var instruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = "11";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction1.PK;
			factory1.Save();
			AssertEquals("Precondition", instruction1.PK, invoiceLine.JI_CEI);

			var factory2 = new BusinessObjectFactory();
			var instruction2 = factory2.Load<CusEntryInstruction>(instruction1.PK);
			instruction2.Delete();
			factory2.Save();

			AssertEquals("JI_CEI of invoice line should be set as empty.", ZGuid.Empty, invoiceLine.JI_CEI);
		}

		public void TestSetterSuspender_CEI_OA_Warehouse()
		{
			var guid1 = ZGuid.NewZGuid();
			var guid2 = ZGuid.NewZGuid();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_OA_Warehouse = guid1;
			using (instruction.SetterSuspender.SuspendSetting(CusEntryInstruction.Schema.CEI_OA_Warehouse))
			{
				instruction.CEI_OA_Warehouse = guid2;
				AssertEquals(guid1, instruction.CEI_OA_Warehouse);
			}
			AssertEquals(guid1, instruction.CEI_OA_Warehouse);
			instruction.CEI_OA_Warehouse = guid2;
			AssertEquals(guid2, instruction.CEI_OA_Warehouse);
		}

		public void TestSetterSuspender_CEI_OA_Warehouse2()
		{
			var guid1 = ZGuid.NewZGuid();
			var guid2 = ZGuid.NewZGuid();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_OA_Warehouse2 = guid1;
			using (instruction.SetterSuspender.SuspendSetting(CusEntryInstruction.Schema.CEI_OA_Warehouse2))
			{
				instruction.CEI_OA_Warehouse2 = guid2;
				AssertEquals(guid1, instruction.CEI_OA_Warehouse2);
			}
			AssertEquals(guid1, instruction.CEI_OA_Warehouse2);
			instruction.CEI_OA_Warehouse2 = guid2;
			AssertEquals(guid2, instruction.CEI_OA_Warehouse2);
		}

		public void TestIsDescriptionDefaultedFromStyle()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var zaImport = helper.CreateRefCusProcedure("ZA", "A", "10", "", "", "Import", "IMP");
			Factory.Save();

			var ceiDescriptionDefaulted = Factory.New<CEI_WithDescriptionDefaultedFromStyle>();
			ceiDescriptionDefaulted.CEI_Style = ZString.Empty;
			ceiDescriptionDefaulted.CEI_Description = ZString.Empty;
			Assert("This overridable bool should be set to false in base.", !ceiDescriptionDefaulted.baseIsDescriptionDefaultedFromStyle);

			var ceiDescriptionNotDefaulted = Factory.New<CusEntryInstruction>();
			ceiDescriptionNotDefaulted.CEI_Style = ZString.Empty;
			ceiDescriptionNotDefaulted.CEI_Description = ZString.Empty;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var lookups = ceiDescriptionDefaulted.Lookups.StyleList;
				Assert(!lookups.GetAllCodesZString().IsNullOrEmpty());
				var code = lookups.GetAllCodesZString().FirstOrDefault();
				var desc = lookups.GetDescriptionFromCode(code);

				ceiDescriptionDefaulted.CEI_Style = code;
				AssertEquals(ceiDescriptionDefaulted.CEI_Description, desc);

				ceiDescriptionNotDefaulted.CEI_Style = code;
				AssertEquals(ceiDescriptionNotDefaulted.CEI_Description, ZString.Empty);

				var ceiDescriptionDefaulted2 = Factory.New<CEI_WithDescriptionDefaultedFromStyle>();
				ceiDescriptionDefaulted2.CEI_Style = ZString.Empty;
				ceiDescriptionDefaulted2.CEI_Description = ZString.Empty;

				using (ceiDescriptionDefaulted2.SetterSuspender.SuspendSetting(CusEntryInstruction.Schema.CEI_Style))
				{
					ceiDescriptionDefaulted2.CEI_Style = code;
					AssertEquals("", ceiDescriptionDefaulted2.CEI_Description);
				}
			}
		}

		public void TestAllowedToBeLinkedToMultipleEntryHeaders()
		{
			CombineAssertions("When Declaration == null", () =>
			{
				var entryInstruction = Factory.New<CusEntryInstruction>();
				AssertEquals("AllowedToBeLinkedToMultipleEntryHeaders should be defaulted to false", false, entryInstruction.AllowedToBeLinkedToMultipleEntryHeaders);
			});

			CombineAssertions("When Declaration != null", () =>
			{
				var declaration = Factory.New<BaseJobDeclarationForTest>();
				declaration.AreMultipleEntryInstructionsAllowedExposed = false;
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				AssertEquals("AllowedToBeLinkedToMultipleEntryHeaders should be defaulted to true", true, entryInstruction.AllowedToBeLinkedToMultipleEntryHeaders);

				declaration.AreMultipleEntryInstructionsAllowedExposed = true;
				AssertEquals("AllowedToBeLinkedToMultipleEntryHeaders should be defaulted to false", false, entryInstruction.AllowedToBeLinkedToMultipleEntryHeaders);
			});
		}

		public void TestAddBlueRowMessageErrorForMultipleLinkedEntriesInsteadOfRedError()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			AssertEquals(false, entryInstruction.AddBlueRowMessageErrorForMultipleLinkedEntriesInsteadOfRedError);
		}

		public void TestDocAddressesRelatedProperties()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var instruction = Factory.New<CusEntryInstructionForTest>();
			instruction.CEI_JE = declaration.PK;

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_ParentTableCode = instruction.TablePrefix;
			docAddress.E2_ParentID = instruction.PK;
			Factory.Save();

			AssertEquals("Should have 1 item for DocAddresses.", 1, instruction.DocAddresses.Count);
			AssertSame("Should load the correcte item.", instruction.DocAddresses[0], docAddress);

			var iDocAddresses = instruction as IDocAddresses;
			Assert("GetSupportedAddressTypes", iDocAddresses.SupportedAddressTypes.Count == 1 && iDocAddresses.SupportedAddressTypes[0] == DocAddressType.InspectionWitness);
			AssertEquals("GetCanOverrideCheckpoint", Env.Security.None, iDocAddresses.GetCanOverrideCheckpoint(docAddress));
			AssertNotNull("OrgHeaderCollection", iDocAddresses.GetOrgHeaderList(DocAddressType.InspectionWitness));
		}

		public void TestICusGoodsLocationTypeSupporter()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			AssertEquals(typeof(CusGoodsLocation), (entryInstruction as ICusGoodsLocationTypeSupporter).GoodsLocationType);
		}

		public void TestITypeDeciderContext()
		{
			var nzCompany = Factory.New<GlbCompany>();
			nzCompany.GC_Code = "CNZ";
			nzCompany.GC_Name = "NZ Company";
			nzCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var nzBranch = nzCompany.Branches.AddNew();
			nzBranch.GB_Code = "BNZ";

			CombineAssertions(() =>
			{
				AssertEquals("From CurrentCompany", "ER", (Factory.New<CusEntryInstruction>() as ITypeDeciderContext).Country);

				var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.NZ.IJobDeclaration>();
				declaration.JE_GB = nzBranch.PK;
				var instruction = Factory.New<CusEntryInstructionForTest>();
				instruction.CEI_JE = declaration.PK;
				AssertEquals("From Declaration", "NZ", (instruction as ITypeDeciderContext).Country);
			});
		}

		public void TestPivotContainerToInstructionIsDeleted()
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			declaration.SupportContainerEntryInstructionPivotCoreForTesting = true;
			var container = declaration.CusContainers.AddNew();
			var entry = declaration.CustomsEntryInstructions.AddNew();
			var pivot = Factory.New<CusContainerEntryInstructionPivot>();
			pivot.CEP_CEI_EntryInstruction = entry.PK;
			pivot.CEP_CO_Container = container.PK;
			Factory.Save();

			entry.Delete();
			AssertNoExceptionThrown(Factory.Save);
			AssertEquals("pivot.IsDeleted", true, pivot.IsDeleted);
		}

		public void TestDeleteIncludeAllPivots()
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var result1 = CreateNewCusContainerOnEntryInstruction(declaration, instruction);
			var result2 = CreateNewCusContainerOnEntryInstruction(declaration, instruction);

			CombineAssertions(() =>
			{
				AssertEquals(2, instruction.ContainersPivot.Count);

				result1.Container.Delete();
				AssertEquals(1, instruction.ContainersPivot.Count);

				result2.IsForEntry = false;
				AssertEquals(0, instruction.ContainersPivot.Count);
			});
		}

		public void TestPivotsToContainersIsNotLoad()
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var container = declaration.CusContainers.AddNew();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var result = CreateNewCusContainerOnEntryInstruction(declaration, instruction);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDecMock = newFactory.LoadMoq<BaseJobDeclarationWithEntryInstructions>(declaration.PK);
			loadedDecMock.Setup(m => m.SupportContainerEntryInstructionPivot).Returns(false);

			declaration = loadedDecMock.Object;
			instruction = declaration.CustomsEntryInstructions.Single();
			instruction.LoadChildEditableObjects();
			AssertEquals("No db hits", 0, newFactory.GetTableHitCount(CusContainerEntryInstructionPivot.Schema.TableName));
			AssertEquals(0, instruction.ContainersPivot.Count);
			AssertEquals("No db hits", 0, newFactory.GetTableHitCount(CusContainerEntryInstructionPivot.Schema.TableName));

			newFactory = new BusinessObjectFactory();

			var loadedDecMock2 = newFactory.LoadMoq<BaseJobDeclarationWithEntryInstructions>(declaration.PK);
			loadedDecMock2.Setup(m => m.SupportContainerEntryInstructionPivot).Returns(true);

			declaration = loadedDecMock2.Object;
			instruction = declaration.CustomsEntryInstructions.Single();
			instruction.LoadChildEditableObjects();
			AssertEquals("db hits", 1, newFactory.GetTableHitCount(CusContainerEntryInstructionPivot.Schema.TableName));
			AssertEquals(1, instruction.ContainersPivot.Count);
		}

		public void TestContainersForInstructionForBindingOnly()
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			declaration.SupportContainerEntryInstructionPivotCoreForTesting = true;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var result1 = CreateNewCusContainerOnEntryInstruction(declaration, instruction);
			result1.Container.CO_ContainerNumber = "TEST1";
			var result2 = CreateNewCusContainerOnEntryInstruction(declaration, instruction);
			result2.Container.CO_ContainerNumber = "TEST2";

			CombineAssertions(() =>
			{
				AssertEquals(2, instruction.ContainersForInstructionForBindingOnly.Count);
				var containerNumbers = instruction.ContainersForInstructionForBindingOnly.Select(c => c.ContainerNumber).ToArray();
				AssertContainsExactElementsInAnyOrder(new[] { "TEST1", "TEST2" }, containerNumbers);

				result1.Container.Delete();
				containerNumbers = instruction.ContainersForInstructionForBindingOnly.Select(c => c.ContainerNumber).ToArray();
				AssertEquals(1, instruction.ContainersForInstructionForBindingOnly.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "TEST2" }, containerNumbers);

				result2.IsForEntry = false;
				containerNumbers = instruction.ContainersForInstructionForBindingOnly.Select(c => c.ContainerNumber).ToArray();
				AssertEquals(1, instruction.ContainersForInstructionForBindingOnly.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "TEST2" }, containerNumbers);
			});
		}

		public void TestContainersForInstructionForBindingOnlyType()
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			AssertType(typeof(CusContainerOnEntryInstructionCollection<CusContainerOnEntryInstruction>), instruction.ContainersForInstructionForBindingOnly);
		}

		public void TestToggleLinkageWithContainer()
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var container = declaration.CusContainers.AddNew();
			var result = new CusContainerOnEntryInstruction(instruction);
			result.Container = container;
			result.IsForEntry = true;
			CombineAssertions(() =>
			{
				instruction.ToggleLinkageWithContainer(container, false);
				AssertEquals(0, instruction.ContainersPivot.Count);
				instruction.ToggleLinkageWithContainer(container, true);
				AssertEquals(1, instruction.ContainersPivot.Count);
				var pivot = instruction.ContainersPivot.Cast<CusContainerEntryInstructionPivot>().Single();
				AssertEquals(pivot.CEP_CEI_EntryInstruction, instruction.PK);
				AssertEquals(pivot.CEP_CO_Container, container.PK);
			});
		}

		CusContainerOnEntryInstruction CreateNewCusContainerOnEntryInstruction(BaseJobDeclarationWithEntryInstructions declaration, CusEntryInstruction instruction)
		{
			var container = declaration.CusContainers.AddNew();
			var result = new CusContainerOnEntryInstruction(instruction);
			result.Container = container;
			result.IsForEntry = true;
			return result;
		}

		sealed class CEI_WithDescriptionDefaultedFromStyle : CusEntryInstruction
		{
			public CEI_WithDescriptionDefaultedFromStyle(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override bool IsDescriptionDefaultedFromStyle => true;

			public bool baseIsDescriptionDefaultedFromStyle => base.IsDescriptionDefaultedFromStyle;
		}

		sealed class BaseJobDeclarationForTest : BaseJobDeclarationWithEntryInstructions
		{
			public BaseJobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ZBool AreMultipleEntryInstructionsAllowedExposed { get; set; }

			public override ZBool AreMultipleEntryInstructionsAllowed => AreMultipleEntryInstructionsAllowedExposed;
		}

		sealed class CusEntryInstructionForTest : CusEntryInstruction
		{
			public CusEntryInstructionForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ZString OrgCusCodeTypeForWarehouseExposed => OrgCusCodeTypeForWarehouse;

			#region JobDocAddress

			protected override ZString HumanReadableNameCore => "InstructionForTest";

			protected override DocAddressType[] GetSupportedAddressTypesCore()
			{
				return new DocAddressType[] { DocAddressType.InspectionWitness };
			}

			protected override SecurityCheckpoint GetCanOverrideCheckpointCore(JobDocAddress docAddress)
			{
				return Env.Security.None;
			}

			protected override OrgHeaderCollection OrgHeaderCollectionCore(DocAddressType addressType)
			{
				return new OrgHeaderCollection(Factory);
			}

			#endregion
		}
	}
}
