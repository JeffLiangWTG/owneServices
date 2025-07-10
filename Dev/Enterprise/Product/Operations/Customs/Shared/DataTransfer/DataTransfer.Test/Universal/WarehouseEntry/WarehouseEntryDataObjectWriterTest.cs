using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class WarehouseEntryDataObjectWriterTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestIntoWarehouseAddressIsUsedForInward()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var warehouse1 = Factory.New<OrgHeader>();
				warehouse1.OH_FullName = "WAREHOUSE 1";
				warehouse1.OH_Code = "WAR1";
				warehouse1.OH_RL_NKClosestPort = "ZAJHB";
				warehouse1.MainAddress.OA_Address1 = "ADDRESS 1";
				warehouse1.CompanyData.OB_IMUsedBondedWhs = true;
				var warehouse2 = Factory.New<OrgHeader>();
				warehouse2.OH_FullName = "WAREHOUSE 2";
				warehouse2.OH_Code = "WAR2";
				warehouse2.OH_RL_NKClosestPort = "ZAJHB";
				warehouse2.MainAddress.OA_Address1 = "ADDRESS 2";
				warehouse2.CompanyData.OB_IMUsedBondedWhs = true;
				var warehouse3 = Factory.New<OrgHeader>();
				warehouse3.OH_FullName = "WAREHOUSE 3";
				warehouse3.OH_Code = "WAR3";
				warehouse3.OH_RL_NKClosestPort = "ZAJHB";
				warehouse3.MainAddress.OA_Address1 = "ADDRESS 3";
				warehouse3.CompanyData.OB_IMUsedBondedWhs = true;
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.WarehouseDocAddress.E2_OA_Address = warehouse1.MainAddress.PK;
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = "40";
				entryInstruction.CEI_OA_Warehouse = warehouse2.MainAddress.PK;
				entryInstruction.CEI_OA_Warehouse2 = warehouse3.MainAddress.PK;
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_CEI_Instruction = entryInstruction.PK;
				var entryLine = entry.MergedLines.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;

				var writer = new WarehouseEntryDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, entry)));
				var shipmentDat = writer.GetDataObject(entry);
				var warehouseAddressData = shipmentDat.OrganizationAddressCollection.First(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.CustomsWarehouseAddress));
				AssertEquals("warehouseAddressData.CompanyName", "WAREHOUSE 3", warehouseAddressData.CompanyName);

				writer = new WarehouseEntryDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWR, entry)));
				shipmentDat = writer.GetDataObject(entry);
				warehouseAddressData = shipmentDat.OrganizationAddressCollection.First(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.CustomsWarehouseAddress));
				AssertEquals("warehouseAddressData.CompanyName", "WAREHOUSE 2", warehouseAddressData.CompanyName);
			}
		}

		public void TestCorrectWarehouseClientIsSent()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var helper = new WhsDataTestHelper(Factory.BOFactory);
				helper.Importer.CompanyData.OB_IMUsedBondedWhs = false;
				helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;
				helper.Owner.CompanyData.OB_IMUsedBondedWhs = false;
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_OH_Importer = helper.Importer.PK;
				declaration.JE_OH_Supplier = helper.Supplier.PK;
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = helper.ChangeOfOwnershipCusProcedure.ZZ6_ProcedureCode;
				entryInstruction.CEI_OH_Owner = helper.Owner.PK;
				entryInstruction.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
				entryInstruction.CEI_OA_Warehouse2 = entryInstruction.CEI_OA_Warehouse;
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_CEI_Instruction = entryInstruction.PK;
				var entryLine = entry.MergedLines.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_CEI = entryInstruction.PK;
				invoiceLine.JI_Procedure = helper.ChangeOfOwnershipCusProcedure.ZZ6_ProcedureCode + helper.ChangeOfOwnershipCusProcedure.ZZ6_PreviousProcedureCode;

				var writer = new WarehouseEntryDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, entry)));
				var shipmentDat = writer.GetDataObject(entry);
				var warehouseClient = shipmentDat.OrganizationAddressCollection.First(x => x.AddressType.GetValueOrDefault() == AddressTypes.WarehouseClient);
				AssertEquals("warehouseClient.CompanyName", helper.Owner.OH_FullName, warehouseClient.CompanyName);

				writer = new WarehouseEntryDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWR, entry)));
				shipmentDat = writer.GetDataObject(entry);
				warehouseClient = shipmentDat.OrganizationAddressCollection.First(x => x.AddressType.GetValueOrDefault() == AddressTypes.WarehouseClient);
				AssertEquals("warehouseClient.CompanyName", helper.Importer.OH_FullName, warehouseClient.CompanyName);

				writer = new WarehouseEntryDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BCO, entry)));
				shipmentDat = writer.GetDataObject(entry);
				AssertNull("Warehouse Client should not be specified for Change of Ownership as system should use be using interface", shipmentDat.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == AddressTypes.WarehouseClient));

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				writer = new WarehouseEntryDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWI, entry)));
				shipmentDat = writer.GetDataObject(entry);
				warehouseClient = shipmentDat.OrganizationAddressCollection.First(x => x.AddressType.GetValueOrDefault() == AddressTypes.WarehouseClient);
				AssertEquals("warehouseClient.CompanyName", helper.Owner.OH_FullName, warehouseClient.CompanyName);

				writer = new WarehouseEntryDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BWR, entry)));
				shipmentDat = writer.GetDataObject(entry);
				warehouseClient = shipmentDat.OrganizationAddressCollection.First(x => x.AddressType.GetValueOrDefault() == AddressTypes.WarehouseClient);
				AssertEquals("warehouseClient.CompanyName", helper.Supplier.OH_FullName, warehouseClient.CompanyName);

				writer = new WarehouseEntryDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BCO, entry)));
				shipmentDat = writer.GetDataObject(entry);
				AssertNull("Warehouse Client should not be specified for Change of Ownership as system should use be using interface", shipmentDat.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == AddressTypes.WarehouseClient));
			}
		}

		public void TestCusEntryHeaderMappingsForWarehouse()
		{
			// Setup Declaration with 2 entries
			// Generate the DataObject for one entry
			// Assert that only data related to that entry is included

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "MB2342";
			declaration.JE_HouseBill = "HB543453";

			var group1 = declaration.TopGroupInvoice.JobComInvoiceGroupHeaders.AddNew();
			group1.JZ_InvoiceNumber = "GROUP1";
			var group1Invoice = group1.JobComInvoiceHeaders.AddNew();
			group1Invoice.JZ_InvoiceNumber = "GROUP1INV";
			var group1InvoiceLine = group1Invoice.JobComInvoiceLines.AddNew();
			group1InvoiceLine.JI_Description = "GROUP1INVLINE";

			var group2 = declaration.TopGroupInvoice.JobComInvoiceGroupHeaders.AddNew();
			group2.JZ_InvoiceNumber = "GROUP2";
			var group2Invoice1 = group2.JobComInvoiceHeaders.AddNew();
			group2Invoice1.JZ_InvoiceNumber = "GROUP2INV1";
			var group2Invoice1Line1 = group2Invoice1.JobComInvoiceLines.AddNew();
			group2Invoice1Line1.JI_Description = "GROUP2INVLINE1";
			var group2Invoice2 = group2.JobComInvoiceHeaders.AddNew();
			group2Invoice2.JZ_InvoiceNumber = "GROUP2INV2";
			var group2Invoice2Line1 = group2Invoice2.JobComInvoiceLines.AddNew();
			group2Invoice2Line1.JI_Description = "GROUP2INV2LINE1";
			var group2Invoice2Line2 = group2Invoice2.JobComInvoiceLines.AddNew();
			group2Invoice2Line2.JI_Description = "GROUP2INV2LINE2";
			var group2Sub = group2.JobComInvoiceGroupHeaders.AddNew();
			group2Sub.JZ_InvoiceNumber = "GROUP2SUB";
			var group2SubInvoice = group2Sub.JobComInvoiceHeaders.AddNew();
			group2SubInvoice.JZ_InvoiceNumber = "GROUP2SUBINV";
			var group2SubInvoiceLine = group2SubInvoice.JobComInvoiceLines.AddNew();
			group2SubInvoiceLine.JI_Description = "GROUP2SUBINVLINE";

			var group3 = declaration.TopGroupInvoice.JobComInvoiceGroupHeaders.AddNew();
			group3.JZ_InvoiceNumber = "GROUP3";
			var group3Invoice = group3.JobComInvoiceHeaders.AddNew();
			group3Invoice.JZ_InvoiceNumber = "GROUP3INV";
			var group3InvoiceLine = group3Invoice.JobComInvoiceLines.AddNew();
			group3InvoiceLine.JI_Description = "GROUP3INVLINE";

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_BGMReference = "ENTRY1";
			var entry1Line = entry1.MergedLines.AddNew();
			group1InvoiceLine.JI_CL = entry1Line.PK;

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_BGMReference = "ENTRY2";
			var entry2Line1 = entry2.MergedLines.AddNew();
			group2Invoice2Line2.JI_CL = entry2Line1.PK;
			var entry2Line2 = entry2.MergedLines.AddNew();
			group3InvoiceLine.JI_CL = entry2Line2.PK;
			var entry2Line3 = entry2.MergedLines.AddNew();
			group2SubInvoiceLine.JI_CL = entry2Line3.PK;

			Factory.SaveForTesting();

			var writer = new WarehouseEntryDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, entry2)));
			var entryData = writer.GetDataObject(entry2);
			AssertEquals("entryData.WayBillNumber", "HB543453", entryData.WayBillNumber);
			AssertEquals("entryData.WayBillType.Code", WayBillTypeList.Codes.House, entryData.WayBillType.GetCodeAsUpperCase());

			AssertEquals("entryData.AdditionalBillCollection.Count", 2, entryData.AdditionalBillCollection.Count);
			var masterBillData = entryData.AdditionalBillCollection.FirstOrDefault(x => x.BillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.Master);
			AssertEquals("masterBillData.BillNumber", "MB2342", masterBillData.BillNumber);
			AssertNull("masterBillData.ParentBillNumber", masterBillData.ParentBillNumber);
			var houseBillData = entryData.AdditionalBillCollection.FirstOrDefault(x => x.BillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.House);
			AssertEquals("houseBillData.BillNumber", "HB543453", houseBillData.BillNumber);
			AssertEquals("houseBillData.ParentBillNumber", "MB2342", houseBillData.ParentBillNumber);

			AssertEquals("entryData.CommercialInfo.SubGroupCollection.Count", 2, entryData.CommercialInfo.SubGroupCollection.Count);
			var group2Data = entryData.CommercialInfo.SubGroupCollection.FirstOrDefault(x => x.Name.Value == "GROUP2");
			AssertEquals("group2Data.SubGroupCollection.Count", 1, group2Data.SubGroupCollection.Count);
			var group2SubData = group2Data.SubGroupCollection.FirstOrDefault(x => x.Name.Value == "GROUP2SUB");
			AssertNull("group2SubData.SubGroupCollection", group2SubData.SubGroupCollection);
			AssertEquals("group2SubData.CommercialInvoiceCollection.Count", 1, group2SubData.CommercialInvoiceCollection.Count);
			var group2SubInvData = group2SubData.CommercialInvoiceCollection.FirstOrDefault(x => x.InvoiceNumber.Value == "GROUP2SUBINV");
			AssertEquals("group2SubInvData.CommercialInvoiceLineCollection.Count", 1, group2SubInvData.CommercialInvoiceLineCollection.Count);
			var group2SubInvLineData = group2SubInvData.CommercialInvoiceLineCollection[0];
			AssertEquals("group2SubInvLineData.Description", "GROUP2SUBINVLINE", group2SubInvLineData.Description);

			AssertEquals("group2Data.CommercialInvoiceCollection.Count", 1, group2Data.CommercialInvoiceCollection.Count);
			var group2Inv2Data = group2Data.CommercialInvoiceCollection.FirstOrDefault(x => x.InvoiceNumber.Value == "GROUP2INV2");
			AssertEquals("group2Inv2Data.CommercialInvoiceLineCollection.Count", 1, group2Inv2Data.CommercialInvoiceLineCollection.Count);
			var group2Inv2Line2Data = group2Inv2Data.CommercialInvoiceLineCollection[0];
			AssertEquals("group2Inv2Line2Data.Description", "GROUP2INV2LINE2", group2Inv2Line2Data.Description);

			var group3Data = entryData.CommercialInfo.SubGroupCollection.FirstOrDefault(x => x.Name.Value == "GROUP3");
			AssertNull("group3Data.SubGroupCollection", group3Data.SubGroupCollection);
			AssertEquals("group3Data.CommercialInvoiceCollection.Count", 1, group3Data.CommercialInvoiceCollection.Count);
			var group3InvData = group3Data.CommercialInvoiceCollection.FirstOrDefault(x => x.InvoiceNumber.Value == "GROUP3INV");
			AssertEquals("group3InvData.CommercialInvoiceLineCollection.Count", 1, group3InvData.CommercialInvoiceLineCollection.Count);
			var group3InvLineData = group3InvData.CommercialInvoiceLineCollection[0];
			AssertEquals("group3InvLineData.Description", "GROUP3INVLINE", group3InvLineData.Description);
		}

		protected override void SetUp()
		{
			setupCreator = ((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
			base.SetUp();
		}
		IDisposable setupCreator;

		protected override void TearDown()
		{
			base.TearDown();
			if (setupCreator != null)
			{
				setupCreator.Dispose();
				setupCreator = null;
			}
		}
	}
}
