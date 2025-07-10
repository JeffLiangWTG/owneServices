using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BondedWarehouseAutomationEndToEndTest : TestCaseWithFactory
	{
		[ExpectException(typeof(NotSupportedException))]
		public void TestCreateAndUpdateInvoicesForExBondAutomationThrowExceptionWhenNotExBond()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CreateAndUpdateInvoicesForExBondAutomation();
		}

		MergedDeclarationCreator2Line<BaseJobDeclaration> SetupDecWith2Lines(bool usePartAttributes)
		{
			var creator = new MergedDeclarationCreator2Line<BaseJobDeclaration>(Factory, DeclarationApplicationCodeList.Codes.Builtin);
			CreateVirtualWarehouse(Factory, creator.Buyer.MainAddress);

			creator.Declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			creator.Declaration.SetSupportsBondedWarehousingForTesting(true);
			creator.Declaration.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			creator.Declaration.Importer.MiscServ.OM_IMPartAttrib1Name = "VIN";
			creator.Declaration.Importer.MiscServ.OM_IMPartAttrib1Type = "NON";
			creator.Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			creator.InvoiceLine1.SetIsGoingIntoBondedWarehouseCoreForTesting(false);
			creator.InvoiceLine2.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			creator.Entry1.SetDeclarationForTesting(creator.Declaration);

			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "~~~";
			part.OP_StockKeepingUnit = "NO";
			OrgPartRelation relation = part.RelatedOrganisations.AddOrganisationIfNotExist(creator.Buyer.PK, OrgPartRelation.RelationshipTypes.Owner);
			if (usePartAttributes)
			{
				creator.Declaration.Importer.PartAttributeManager.SetProductToUseAttribute(part, 1, true);
			}

			BaseCusClassification @class = Factory.New<BaseCusClassification>();
			@class.CC_LookupCode = "TestLookup";
			@class.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			@class.CC_RN_NKCountryCode = creator.Declaration.CountryCode;
			@class.CC_TariffNum = "2605.00.00 08";
			BaseCusClassPartPivot pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_CC = @class.PK;
			pivot.CI_OP = part.PK;

			TestHelper.DisableMergeRequirementForAllDeclarationsInFactory(Factory);
			Factory.Save(); // so part gets in DB

			creator.InvoiceLine1.JI_PartNo = "~~~";
			creator.InvoiceLine1.JI_InvoiceQuantity = 100;
			creator.InvoiceLine1.JI_InvoiceUQ = "ML";

			creator.InvoiceLine2.JI_PartNo = "~~~";
			creator.InvoiceLine2.JI_InvoiceQuantity = 10;
			creator.InvoiceLine2.JI_InvoiceUQ = "NO";
			creator.InvoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.France;
			creator.InvoiceLine2.JI_CustomsUnitQty = "KG";
			creator.InvoiceLine2.JI_CustomsQuantity = 100;
			creator.InvoiceLine2.JI_LinePrice = 1000;

			creator.Declaration.WarehouseDocAddress.E2_OA_Address = creator.Buyer.MainAddress.PK;
			creator.Entry1.EntryNumber = "EA01010AE";

			return creator;
		}

		public void TestUpdateOutwardLinesWithInventoryDetails()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				/// N20
				var creator = SetupDecWith2Lines(false);
				Factory.Save();

				BatchProcessorProcessesPayMessageForEntry(creator.Entry1.PK, creator.InvoiceLine2.PK);
				var whsQuery = new ZQuery(WhsInventoryViewSchema.WI_BondedEntryKey, SQLComparisonOperator.StartsWith, "");
				whsQuery.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0);
				var inventory = Factory.LoadTop1<IWhsInventoryView>(whsQuery);
				AssertNotNull(inventory);

				/// Nature 30
				BusinessObjectFactory n30Factory = new BusinessObjectFactory();
				var declarationMock = n30Factory.New<DummyBaseJobDeclaration>();
				declarationMock.GetIsWHSUniversalXMLActiveReturns = true;
				declarationMock.IsInvoiceQuantityRequiredForBondedWarehouseReturns = true;
				declarationMock.IsBondedWhsQuantityRequiredForBondedWarehouseReturns = false;

				var n30Declaration = declarationMock;
				n30Declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
				n30Declaration.SetSupportsBondedWarehousingForTesting(true);
				n30Declaration.JE_OH_Importer = creator.Declaration.Importer.PK;
				n30Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				n30Declaration.WarehouseDocAddress.E2_OA_Address = creator.Buyer.MainAddress.PK;

				BaseJobComInvoiceLine n30Line1 = n30Declaration.FilteredInvoiceLines.AddNew();
				n30Line1.JI_PartNo = "~~~";
				n30Line1.JI_InvoiceQuantity = 5;
				n30Line1.JI_InvoiceUQ = "NO";
				n30Line1.JI_CustomsUnitQty = "KG";
				n30Line1.SetUseBondedWarehouseAutomationForTesting(true);
				n30Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(true);
				n30Declaration.UpdateOutwardLinesWithInventoryDetails();

				n30Line1 = n30Declaration.FilteredInvoiceLines[0];
				AssertEquals("~~~", n30Line1.JI_PartNo);
				AssertEquals(500m, n30Line1.JI_LinePrice);
				AssertEquals(5m, n30Line1.JI_InvoiceQuantity);
				AssertEquals(50m, n30Line1.JI_CustomsQuantity);
				AssertEquals("KG", n30Line1.JI_CustomsUnitQty);
				AssertEquals("NO", n30Line1.JI_InvoiceUQ);

				inventory = Factory.LoadTop1<IWhsInventoryView>(whsQuery);
				AssertNotNull("Should not take out of invetory", inventory);
			}
		}

		public void TestUpdateOutwardLinesWithInventoryDetailsForMultiEntry()
		{
			var helper = WhsDataTestHelper.New(Core.Constants.CountryCodes.SouthAfrica, Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				new UniversalReferenceTestDataHelper(Factory).CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
				Factory.Save();
				CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
				AssertEquals(Core.Constants.CountryCodes.SouthAfrica, helper.InwardCusProcedure.ZZ6_ZZZ_NKDataGrouping);
				AssertEquals(Core.Constants.CountryCodes.SouthAfrica, helper.OutwardCusProcedure.ZZ6_ZZZ_NKDataGrouping);
				helper.SetTariffAndSave(helper.Part, "KG");
				helper.SetTariffAndSave(helper.Part2, "KG");
				helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;
				var inwardDeclaration = helper.GetNewDeclaration(JobMessageTypeList.Codes.Import, "BZA0001232", "ENT32342", 100m);
				inwardDeclaration.JE_ApplicationCode = "BLT";
				inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
				inwardDeclaration[ZAJobDeclarationSchema.JE_AGTCode.Name] = "ASBSD";
				inwardDeclaration.JE_CustomsOffice = "JHB";
				var inwardEntryInstruction = inwardDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				inwardEntryInstruction.CEI_Style = helper.InwardCusProcedure.ZZ6_ProcedureCode;
				inwardEntryInstruction.CEI_OA_Warehouse2 = helper.WhsWarehouse.WW_OA_WarehouseAddress;
				var inwardInvoice = inwardDeclaration.Invoices[0];
				inwardInvoice.JobComInvoiceLines.RemoveAndDeleteAll();
				var inwardInvoiceLine1 = inwardInvoice.JobComInvoiceLines.AddNew();
				inwardInvoiceLine1.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				inwardInvoiceLine1.JI_CEI = inwardEntryInstruction.PK;
				inwardInvoiceLine1.JI_Procedure = "00" + helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
				inwardInvoiceLine1.JI_PartNo = helper.Part.OP_PartNum;
				inwardInvoiceLine1.JI_BondedWhsQuantity = 100m;
				inwardInvoiceLine1.JI_BondedWhsUnitQty = "PK";
				inwardInvoiceLine1.JI_InvoiceQuantity = 100m;
				inwardInvoiceLine1.JI_InvoiceUQ = "NO";
				inwardInvoiceLine1.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine1.JI_CustomsQuantity = 1000m;
				inwardInvoiceLine1.JI_LinePrice = 10000m;
				var inwardInvoiceLine2 = inwardInvoice.JobComInvoiceLines.AddNew();
				inwardInvoiceLine2.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				inwardInvoiceLine2.JI_CEI = inwardEntryInstruction.PK;
				inwardInvoiceLine2.JI_Procedure = "00" + helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
				inwardInvoiceLine2.JI_PartNo = helper.Part2.OP_PartNum;
				inwardInvoiceLine2.JI_BondedWhsQuantity = 300m;
				inwardInvoiceLine2.JI_BondedWhsUnitQty = "NO";
				inwardInvoiceLine2.JI_InvoiceQuantity = 300m;
				inwardInvoiceLine2.JI_InvoiceUQ = "PK";
				inwardInvoiceLine2.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine2.JI_CustomsQuantity = 3000m;
				inwardInvoiceLine2.JI_LinePrice = 30000m;
				inwardDeclaration.DoMerge();
				var inwardEntry = inwardInvoiceLine1.CusEntryLine.Header;
				inwardEntry.EntryNumber = "ENT32342";
				Factory.Save();
				inwardEntry.PublishShipmentForWHSInward(false);
				inwardEntry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				var inwardBondedEntryKey1 = "ENT32342-1";
				var inwardBondedEntryKey2 = "ENT32342-2";
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey1, 100m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardBondedEntryKey2, 300m);

				var outwardDeclaration = helper.GetNewDeclaration(JobMessageTypeList.Codes.ExWarehouse, "BZA0005433", "", 60m);
				outwardDeclaration.JE_ApplicationCode = "BLT";
				outwardDeclaration[ZAJobDeclarationSchema.JE_AGTCode.Name] = "ASBSD";
				outwardDeclaration.JE_CustomsOffice = "JHB";
				var outwardEntryInstruction = outwardDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				outwardEntryInstruction.CEI_Style = helper.OutwardCusProcedure.ZZ6_ProcedureCode;
				outwardEntryInstruction.CEI_OA_Warehouse = helper.WhsWarehouse.WW_OA_WarehouseAddress;
				var outwardInvoice = outwardDeclaration.Invoices[0];
				outwardInvoice.JobComInvoiceLines.RemoveAndDeleteAll();
				var outwardInvoiceLine1 = outwardInvoice.JobComInvoiceLines.AddNew();
				outwardInvoiceLine1.JI_CEI = outwardEntryInstruction.PK;
				outwardInvoiceLine1.JI_PartNo = helper.Part.OP_PartNum;
				outwardInvoiceLine1.JI_Procedure = "00" + helper.OutwardCusProcedure.ZZ6_PreviousProcedureCode;
				outwardInvoiceLine1.JI_BondedWhsQuantity = 60m;
				outwardInvoiceLine1.JI_BondedWhsUnitQty = "PK";
				outwardInvoiceLine1[ZAJobComInvoiceLineSchema.JI_CommissionNumber.Name] = "CON32";
				var outwardInvoiceLine2 = outwardDeclaration.InvoiceLines.AddNew();
				outwardInvoiceLine2.JI_CEI = outwardEntryInstruction.PK;
				outwardInvoiceLine2.JI_PartNo = helper.Part2.OP_PartNum;
				outwardInvoiceLine2.JI_BondedWhsQuantity = 150m;
				outwardInvoiceLine2.JI_BondedWhsUnitQty = "NO";
				outwardInvoiceLine2[ZAJobComInvoiceLineSchema.JI_CommissionNumber.Name] = "CON43";
				outwardDeclaration.UpdateOutwardLinesWithInventoryDetails();
				CombineAssertions(() =>
				{
					AssertZAInvoiceLine(outwardInvoiceLine1, outwardEntryInstruction.PK, helper.Part, "CON32", 60m, "PK", 60m, "PK", 600m, "KG", 6000m, "ENT32342", 1, helper.InwardCusProcedure.ZZ6_ProcedureCode);
					AssertZAInvoiceLine(outwardInvoiceLine2, outwardEntryInstruction.PK, helper.Part2, "CON43", 150m, "NO", 150m, "NO", 1500m, "KG", 15000m, "ENT32342", 2, helper.InwardCusProcedure.ZZ6_ProcedureCode);
				});
			}
		}

		void AssertZAInvoiceLine(BaseJobComInvoiceLine invoiceLine, ZGuid entryInstructionPK, OrgSupplierPart importerPart, ZString commissionNumber, ZDecimal invoiceQty, ZString invoiceUQ, ZDecimal countableQty, ZString countableUQ, ZDecimal customsQty, ZString customsUQ, ZDecimal linePrice, ZString previousEntryNumber, ZShort previousEntryLineNumber, ZString ppc)
		{
			AssertEquals("invoiceLine.JI_CEI", entryInstructionPK, invoiceLine.JI_CEI);
			AssertEquals("invoiceLine.JI_PartNo", importerPart.OP_PartNum, invoiceLine.JI_PartNo);
			AssertEquals("invoiceLine.JI_OP", importerPart.PK, invoiceLine.JI_OP);
			AssertEquals("invoiceLine.JI_CommissionNumber", commissionNumber, invoiceLine[ZAJobComInvoiceLineSchema.JI_CommissionNumber.Name]);
			AssertEquals("invoiceLine.JI_InvoiceQuantity", invoiceQty, invoiceLine.JI_InvoiceQuantity);
			AssertEquals("invoiceLine.JI_InvoiceUQ", invoiceUQ, invoiceLine.JI_InvoiceUQ);
			AssertEquals("invoiceLine.JI_BondedWhsQuantity", countableQty, invoiceLine.JI_BondedWhsQuantity);
			AssertEquals("invoiceLine.JI_BondedWhsUnitQty", countableUQ, invoiceLine.JI_BondedWhsUnitQty);
			AssertEquals("invoiceLine.JI_CustomsUnitQty", customsUQ, invoiceLine.JI_CustomsUnitQty);
			AssertEquals("invoiceLine.JI_CustomsQuantity", customsQty, invoiceLine.JI_CustomsQuantity);
			AssertEquals("invoiceLine.JI_LinePrice", linePrice, invoiceLine.JI_LinePrice);
			AssertEquals("invoiceLine.JI_PreviousEntryNumber", previousEntryNumber, invoiceLine.JI_PreviousEntryNumber);
			AssertEquals("invoiceLine.JI_PreviousEntryLineNumber", previousEntryLineNumber, invoiceLine.JI_PreviousEntryLineNumber);
			AssertEquals("invoiceLine.JI_PreviousProcedure", ppc, invoiceLine.JI_Calc_PreviousProcedure);
		}

		public void TestSimpleEndToEndByProduct()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				/// N20
				var creator = SetupDecWith2Lines(false);
				Factory.Save();

				BatchProcessorProcessesPayMessageForEntry(creator.Entry1.PK, creator.InvoiceLine2.PK);

				/// Nature 30
				var n30Factory = new BusinessObjectFactory();
				var n30Declaration = BaseJobDeclaration.New(n30Factory);
				n30Declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
				n30Declaration.SetSupportsBondedWarehousingForTesting(true);
				n30Declaration.JE_OH_Importer = creator.Declaration.Importer.PK;
				n30Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				var n30Line1 = n30Declaration.FilteredInvoiceLines.AddNew();
				n30Line1.JI_PartNo = "~~~";
				n30Line1.JI_InvoiceQuantity = 5;
				n30Line1.JI_InvoiceUQ = "NO";
				n30Line1.SetUseBondedWarehouseAutomationForTesting(true);
				n30Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(true);
				n30Declaration.CreateAndUpdateInvoicesForExBondAutomation();
				AssertEquals("No problem", null, ((SendsMessagesToCustomsShutterUpperer)n30Declaration.MessageInitiator).Warning);

				n30Line1 = n30Declaration.FilteredInvoiceLines[0];
				AssertEquals("~~~", n30Line1.JI_PartNo);
				AssertEquals(500m, n30Line1.JI_LinePrice);
				AssertEquals(5m, n30Line1.JI_InvoiceQuantity);
				AssertEquals(50m, n30Line1.JI_CustomsQuantity);
				AssertEquals("KG", n30Line1.JI_CustomsUnitQty);
				AssertEquals("KG", n30Line1.JI_CustomsUnitQty);

				AssertEquals("NO", n30Line1.JI_InvoiceUQ);
			}
		}

		public void TestSimpleEndToEndByBondID()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				/// N20
				var creator = SetupDecWith2Lines(true);
				creator.InvoiceLine2.JI_PartAttrib1 = "vin1";
				Factory.Save();

				BatchProcessorProcessesPayMessageForEntry(creator.Entry1.PK, creator.InvoiceLine2.PK);

				/// Nature 30 -1
				var n30Factory = new BusinessObjectFactory();
				var n30Declaration = BaseJobDeclaration.New(n30Factory);
				n30Declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
				n30Declaration.SetSupportsBondedWarehousingForTesting(true);
				n30Declaration.JE_OH_Importer = creator.Declaration.Importer.PK;
				n30Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				var header = n30Declaration.CustomsEntryHeaders.AddNew();
				header.EntryNumber = "N30AAAA";
				header.SetDeclarationForTesting(creator.Declaration);
				var entryLine = header.MergedLines.AddNew();
				var n30Line1 = n30Declaration.FilteredInvoiceLines.AddNew();
				var n30Line2 = n30Declaration.FilteredInvoiceLines.AddNew();
				n30Line1.JI_PartAttrib1 = "vin1";
				n30Line1.JI_CL = entryLine.PK;
				n30Line1.SetUseBondedWarehouseAutomationForTesting(true);
				n30Line2.SetUseBondedWarehouseAutomationForTesting(false);
				n30Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(true);
				n30Declaration.CreateAndUpdateInvoicesForExBondAutomation();
				var stockWarningMessage = @"Stock has been released from the bonded warehouse. However, there are some warnings/recommendations:
One or more lines do not have a Warehouse Quantity entered. Please enter a Warehouse Quantity on each line with a warning and then Synchronize again.";
				AssertEquals("warning", stockWarningMessage, ((SendsMessagesToCustomsShutterUpperer)n30Declaration.MessageInitiator).Warning);

				n30Line1 = n30Declaration.FilteredInvoiceLines[0] == n30Line2 ? n30Declaration.FilteredInvoiceLines[1] : n30Declaration.FilteredInvoiceLines[0];
				n30Line1.SetUseBondedWarehouseAutomationForTesting(true); // this should be implemented to happen automatically in real countries with addinfo
				AssertEquals("~~~", n30Line1.JI_PartNo);
				AssertEquals(1000m, n30Line1.JI_LinePrice);
				AssertEquals(10m, n30Line1.JI_InvoiceQuantity);
				AssertEquals("NO", n30Line1.JI_InvoiceUQ);
				AssertEquals(100m, n30Line1.JI_CustomsQuantity);
				AssertEquals("KG", n30Line1.JI_CustomsUnitQty);

				AssertEquals("NO", n30Line1.JI_InvoiceUQ);
				n30Declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
				n30Factory.Save();

				/// Nature 30 -- 2
				var n30Factory2 = new BusinessObjectFactory();
				var n30Declaration2 = BaseJobDeclaration.New(n30Factory);
				n30Declaration2.SetSupportsBondedWarehousingForTesting(true);
				n30Declaration2.JE_OH_Importer = creator.Declaration.Importer.PK;
				n30Declaration2.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				var n30_2_Line1 = n30Declaration2.FilteredInvoiceLines.AddNew();
				n30_2_Line1.JI_PartAttrib1 = "vin1";
				n30_2_Line1.JI_CL = entryLine.PK;
				n30_2_Line1.JI_InvoiceQuantity = 10;
				n30_2_Line1.SetUseBondedWarehouseAutomationForTesting(true);
				n30Declaration2.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(true);
				n30Declaration2.CreateAndUpdateInvoicesForExBondAutomation();
				AssertNotNull("Has problem", ((SendsMessagesToCustomsShutterUpperer)n30Declaration2.MessageInitiator).Warning);

				/// Nature 30 -- 1
				AssertEquals("Automation should be enabled", true, n30Line1.UseBondedWarehouseAutomation);
				n30Declaration.CancelBondedWarehouseIntegration();
				AssertEquals("Automation should be disabled", false, n30Line1.UseBondedWarehouseAutomation);
				AssertEquals("Automation still disabled", false, n30Line2.UseBondedWarehouseAutomation);
				n30Factory.Save();

				/// Nature 30 -- 2
				n30Declaration2.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(true);
				n30Declaration2.CreateAndUpdateInvoicesForExBondAutomation();
				AssertEquals("No problem", null, ((SendsMessagesToCustomsShutterUpperer)n30Declaration2.MessageInitiator).Warning);
				n30_2_Line1 = n30Declaration2.FilteredInvoiceLines[0];
				AssertEquals("~~~", n30_2_Line1.JI_PartNo);
				AssertEquals(1000m, n30_2_Line1.JI_LinePrice);
				AssertEquals(10m, n30_2_Line1.JI_InvoiceQuantity);
				AssertEquals(100m, n30_2_Line1.JI_CustomsQuantity);
				AssertEquals("KG", n30_2_Line1.JI_CustomsUnitQty);
				AssertEquals("NO", n30_2_Line1.JI_InvoiceUQ);
			}
		}

		void BatchProcessorProcessesPayMessageForEntry(ZGuid entryPK, ZGuid invoiceLinePK)
		{
			BusinessObjectFactory batchProcessorFactory = new BusinessObjectFactory();
			CusEntryHeader batchProcessorEntry = batchProcessorFactory.Load<CusEntryHeader>(entryPK);
			batchProcessorEntry.Declaration.SetSupportsBondedWarehousingForTesting(true);
			batchProcessorEntry.OverrideIsStatusChangingToClearedForTesting = true;
			batchProcessorEntry.IsStatusChangingToClearedForTesting = true; // pretend service task has processed customs clear message for pay
			batchProcessorEntry.HasChanges = true;
			BaseJobComInvoiceLine line = batchProcessorFactory.Load<BaseJobComInvoiceLine>(invoiceLinePK);
			line.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			batchProcessorFactory.Save();

			ZQuery filter = new ZQuery(WhsDocketSchema.WD_ExternalReference, SQLComparisonOperator.StartsWith, batchProcessorEntry.EntryNumber + "-INW W0000000");
			filter.AddToFilter(WhsDocketSchema.WD_DocketType, "INW");
			BusinessObject receiveDocket = (BusinessObject)new BusinessObjectFactory().LoadTop1<IWhsReceive>(filter);

			AssertNotNull("Receive docket should be created", receiveDocket);
		}

		IDisposable setupWHSUniversalXMLForTesting;
		protected override void SetUp()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddYears(1).ToDateTime());
			setupWHSUniversalXMLForTesting = BaseJobDeclaration.SetupWHSUniversalXMLForTesting();
			base.SetUp();
		}

		protected override void TearDown()
		{
			if (setupWHSUniversalXMLForTesting != null)
			{
				setupWHSUniversalXMLForTesting.Dispose();
				setupWHSUniversalXMLForTesting = null;
			}
			base.TearDown();
		}

		public static IWhsWarehouse CreateVirtualWarehouse(BusinessObjectFactory factory, OrgAddress address)
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(factory);
			var warehouse = factory.New<IWhsWarehouse>();

			var locationType = factory.LoadTop1<IWhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "RNO"));
			warehouse.WW_WLT_DefaultLocationType = locationType.PK;
			helper.SetUpBondedWarehouse(warehouse.PK, address.PK);
			warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
			return warehouse;
		}

		sealed class DummyBaseJobDeclaration : BaseJobDeclaration
		{
			public DummyBaseJobDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool GetIsWHSUniversalXMLActiveReturns { get; set; }
			public bool IsInvoiceQuantityRequiredForBondedWarehouseReturns { get; set; }
			public bool IsBondedWhsQuantityRequiredForBondedWarehouseReturns { get; set; }

			protected override bool GetIsWHSUniversalXMLActive() => GetIsWHSUniversalXMLActiveReturns;
			protected internal override bool IsInvoiceQuantityRequiredForBondedWarehouse => IsInvoiceQuantityRequiredForBondedWarehouseReturns;
			protected internal override bool IsBondedWhsQuantityRequiredForBondedWarehouse => IsBondedWhsQuantityRequiredForBondedWarehouseReturns;
		}
	}
}
