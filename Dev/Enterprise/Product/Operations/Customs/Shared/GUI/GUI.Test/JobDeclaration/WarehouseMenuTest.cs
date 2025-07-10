using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Business.WarehouseExtensions.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using EventConstants = Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class WarehouseMenuTest : TestCaseWithFactory
	{
		public void TestCannotUpdateBondedWarehouseWhenSameEntryDetailsIsUsedAgaintsDifferntProduct()
		{
			setupWHSUniversalXMLForTesting = BaseJobDeclaration.SetupWHSUniversalXMLForTesting();
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			dummy = DummyBusinessObject.New(Factory);
			formMock = new Mock<ZForm>(dummy);
			formMock.CallBase = true;
			form = formMock.Object;
			menu = new EDIMenu();
			form.Menu.MenuItems.Add(menu);
			declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.WarehouseDocAddress.E2_OA_Address = WarehouseAddress.MainAddress.PK;
			invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.SetSupportsBondedWarehousingForTesting(true);
			menu.Declaration = declaration;
			using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(declaration))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			}
			Factory.Save();

			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());

			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var importer = Buyer;
				importer.OH_RL_NKClosestPort = "AUSYD";
				var warehouse = WarehouseAddress;
				warehouse.OH_RL_NKClosestPort = "AUSYD";
				var part1 = CreatePart(Factory, importer.PK);
				var part2 = Factory.New<Business.OrgSupplierPart>();
				part2.OP_PartNum = part1.OP_PartNum + "@";
				part2.OP_StockKeepingUnit = "NO";
				var relation = part2.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
				var pivot = part2.PivotsForBinding.AddNew();
				pivot.CI_CC = part1.PivotsForBinding[0].CI_CC;

				var inwardDeclaration = Factory.New<BaseJobDeclaration>();
				inwardDeclaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
				inwardDeclaration.JE_TransportMode = inwardDeclaration.TransportModeAirCodeForTesting;
				inwardDeclaration.JE_OH_Importer = importer.PK;
				inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
				inwardDeclaration.JE_DeclarationReference = "B00000123";
				inwardDeclaration.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;
				inwardDeclaration.SetSupportsBondedWarehousingForTesting(true);

				var inwardEntry = inwardDeclaration.CustomsEntryHeaders.AddNew();
				inwardEntry.CH_MessageType = JobMessageTypeList.Codes.Import;
				var inwardEntryLine = inwardEntry.MergedLines.AddNew();
				inwardEntryLine.CL_LineNumber = 1;
				var inwardInvoice = inwardDeclaration.Invoices.AddNew();
				inwardInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				inwardInvoice.JZ_RX_NKInvoice_Currency = inwardDeclaration.LocalCurrencyCode;
				inwardInvoice.JZ_InvoiceAmount = 1000m;

				var inwardInvoiceLine1 = inwardInvoice.JobComInvoiceLines.AddNew();
				inwardInvoiceLine1.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				inwardInvoiceLine1.JI_PartNo = part1.OP_PartNum;
				inwardInvoiceLine1.JI_InvoiceQuantity = 100m;
				inwardInvoiceLine1.JI_InvoiceUQ = "NO";
				inwardInvoiceLine1.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine1.JI_CustomsQuantity = 10m;
				inwardInvoiceLine1.JI_LinePrice = 1000m;
				inwardInvoiceLine1.JI_CL = inwardEntryLine.PK;
				inwardInvoiceLine1.JI_AddInfo = "WRQ=100*WUV=NO*WRN=ENT2343*WRL=1";

				var inwardInvoiceLine2 = inwardInvoice.JobComInvoiceLines.AddNew();
				inwardInvoiceLine2.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				inwardInvoiceLine2.JI_PartNo = part2.OP_PartNum;
				inwardInvoiceLine2.JI_InvoiceQuantity = 100m;
				inwardInvoiceLine2.JI_InvoiceUQ = "NO";
				inwardInvoiceLine2.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine2.JI_CustomsQuantity = 10m;
				inwardInvoiceLine2.JI_LinePrice = 1000m;
				inwardInvoiceLine2.JI_CL = inwardEntryLine.PK;
				inwardInvoiceLine2.JI_AddInfo = "WRQ=100*WUV=NO*WRN=ENT2343*WRL=1";
				var job = new JobHeader.Loader(inwardDeclaration).TryCreate();
				job.JH_GE = Department.PK;
				Factory.Save();
				using (var form = new BaseJobDeclarationFormForTest(inwardDeclaration))
				{
					var menu = form.EDIMenu;
					menu.Declaration = inwardDeclaration;
					menu.RefreshMenu();
					form.FireSaveButton();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
					AssertEquals(true, bondedWarehouseMenuItem.Visible);
					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					var updateBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("Update Inventory");
					AssertEquals(true, updateBondedWarehouseMenuItem.Visible);
					updateBondedWarehouseMenuItem.PerformClick();
					AssertEquals("Unable to proceed due to some critical errors; please fix all errors before trying again.", UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 0m);

					inwardInvoiceLine2.JI_PartNo = part1.OP_PartNum;
					form.FireSaveButton();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
					AssertEquals(true, bondedWarehouseMenuItem.Visible);
					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					updateBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("Update Inventory");
					AssertEquals(true, updateBondedWarehouseMenuItem.Visible);
					updateBondedWarehouseMenuItem.PerformClick();
					AssertNotEquals("Unable to proceed due to some critical errors; please fix all errors before trying again.", UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 200m);
				}
			}
		}

		public void TestUpdateBondedWarehouseInwardWithZeroQuantity()
		{
			setupWHSUniversalXMLForTesting = BaseJobDeclaration.SetupWHSUniversalXMLForTesting();
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			dummy = DummyBusinessObject.New(Factory);
			formMock = new Mock<ZForm>(dummy);
			formMock.CallBase = true;
			form = formMock.Object;
			menu = new EDIMenu();
			form.Menu.MenuItems.Add(menu);
			declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.WarehouseDocAddress.E2_OA_Address = WarehouseAddress.MainAddress.PK;
			invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.SetSupportsBondedWarehousingForTesting(true);
			menu.Declaration = declaration;
			using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(declaration))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			}
			Factory.Save();

			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());

			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var importer = Buyer;
				importer.OH_RL_NKClosestPort = "AUSYD";
				var warehouse = WarehouseAddress;
				warehouse.OH_RL_NKClosestPort = "AUSYD";
				var part1 = CreatePart(Factory, importer.PK);
				var part2 = Factory.New<Business.OrgSupplierPart>();
				part2.OP_PartNum = part1.OP_PartNum + "@";
				part2.OP_StockKeepingUnit = "NO";
				var relation = part2.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
				var pivot = part2.PivotsForBinding.AddNew();
				pivot.CI_CC = part1.PivotsForBinding[0].CI_CC;

				var inwardDeclaration = Factory.New<BaseJobDeclaration>();
				inwardDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				inwardDeclaration.JE_TransportMode = inwardDeclaration.TransportModeAirCodeForTesting;
				inwardDeclaration.JE_OH_Importer = importer.PK;
				inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
				inwardDeclaration.JE_DeclarationReference = "B00000123";
				inwardDeclaration.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;
				inwardDeclaration.SetSupportsBondedWarehousingForTesting(true);

				var inwardEntry = inwardDeclaration.CustomsEntryHeaders.AddNew();
				inwardEntry.CH_MessageType = JobMessageTypeList.Codes.Import;
				inwardEntry.EntryNumber = "ENT2343";
				var inwardEntryLine = inwardEntry.MergedLines.AddNew();
				inwardEntryLine.CL_LineNumber = 1;
				var inwardInvoice = inwardDeclaration.Invoices.AddNew();
				inwardInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				inwardInvoice.JZ_RX_NKInvoice_Currency = inwardDeclaration.LocalCurrencyCode;
				inwardInvoice.JZ_InvoiceAmount = 1000m;

				var inwardInvoiceLine1 = inwardInvoice.JobComInvoiceLines.AddNew();
				inwardInvoiceLine1.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				inwardInvoiceLine1.JI_PartNo = part1.OP_PartNum;
				inwardInvoiceLine1.JI_InvoiceQuantity = 0m;
				inwardInvoiceLine1.JI_InvoiceUQ = "NO";
				inwardInvoiceLine1.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine1.JI_CustomsQuantity = 10m;
				inwardInvoiceLine1.JI_LinePrice = 1000m;
				inwardInvoiceLine1.JI_CL = inwardEntryLine.PK;
				inwardInvoiceLine1.JI_AddInfo = "WRQ=100*WUV=NO*WRN=ENT2343*WRL=1";
				var job = new JobHeader.Loader(inwardDeclaration).TryCreate();
				job.JH_GE = Department.PK;
				Factory.Save();
				using (var form = new BaseJobDeclarationFormForTest(inwardDeclaration))
				{
					var menu = form.EDIMenu;
					menu.Declaration = inwardDeclaration;
					menu.RefreshMenu();
					form.FireSaveButton();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
					AssertEquals(true, bondedWarehouseMenuItem.Visible);
					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					var updateBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("Update Inventory");
					AssertEquals(true, updateBondedWarehouseMenuItem.Visible);
					updateBondedWarehouseMenuItem.PerformClick();
					AssertEquals(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(2, UnitTestUserNotification.Instance.PreviousMessages.Length);
					AssertEquals(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), UnitTestUserNotification.Instance.PreviousMessages[0].Text);
					AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages[1].WasNone);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 0m);

					inwardInvoiceLine1.JI_InvoiceQuantity = 100m;
					form.FireSaveButton();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
					AssertEquals(true, bondedWarehouseMenuItem.Visible);
					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					updateBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("Update Inventory");
					AssertEquals(true, updateBondedWarehouseMenuItem.Visible);
					updateBondedWarehouseMenuItem.PerformClick();
					AssertEquals("Stock Levels have been updated. (WHS Receipt:W00000001)", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(2, UnitTestUserNotification.Instance.PreviousMessages.Length);
					AssertEquals("Stock Levels have been updated. (WHS Receipt:W00000001)", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
					AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages[1].WasNone);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 100m);

					inwardInvoiceLine1.JI_InvoiceQuantity = 0m;
					form.FireSaveButton();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
					AssertEquals(true, bondedWarehouseMenuItem.Visible);
					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					updateBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("Update Inventory");
					AssertEquals(true, updateBondedWarehouseMenuItem.Visible);
					updateBondedWarehouseMenuItem.PerformClick();
					AssertEquals(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(2, UnitTestUserNotification.Instance.PreviousMessages.Length);
					AssertEquals(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), UnitTestUserNotification.Instance.PreviousMessages[0].Text);
					AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages[1].WasNone);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 100m);
				}
			}
		}

		public void TestUpdateBondedWarehouseOutwardWithZeroQuantity()
		{
			setupWHSUniversalXMLForTesting = BaseJobDeclaration.SetupWHSUniversalXMLForTesting();
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			dummy = DummyBusinessObject.New(Factory);
			formMock = new Mock<ZForm>(dummy);
			formMock.CallBase = true;
			form = formMock.Object;
			menu = new EDIMenu();
			form.Menu.MenuItems.Add(menu);
			declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.WarehouseDocAddress.E2_OA_Address = WarehouseAddress.MainAddress.PK;
			invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.SetSupportsBondedWarehousingForTesting(true);
			menu.Declaration = declaration;
			using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(declaration))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			}
			Factory.Save();

			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());

			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var importer = Buyer;
				importer.OH_RL_NKClosestPort = "AUSYD";
				var warehouse = WarehouseAddress;
				warehouse.OH_RL_NKClosestPort = "AUSYD";
				var part1 = CreatePart(Factory, importer.PK);
				var part2 = Factory.New<Business.OrgSupplierPart>();
				part2.OP_PartNum = part1.OP_PartNum + "@";
				part2.OP_StockKeepingUnit = "NO";
				var relation = part2.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
				var pivot = part2.PivotsForBinding.AddNew();
				pivot.CI_CC = part1.PivotsForBinding[0].CI_CC;

				var inwardDeclaration = Factory.New<BaseJobDeclaration>();
				inwardDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				inwardDeclaration.JE_TransportMode = inwardDeclaration.TransportModeAirCodeForTesting;
				inwardDeclaration.JE_OH_Importer = importer.PK;
				inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
				inwardDeclaration.JE_DeclarationReference = "B00000123";
				inwardDeclaration.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;
				inwardDeclaration.SetSupportsBondedWarehousingForTesting(true);

				var inwardEntry = inwardDeclaration.CustomsEntryHeaders.AddNew();
				inwardEntry.CH_MessageType = JobMessageTypeList.Codes.Import;
				inwardEntry.EntryNumber = "ENT2343";
				var inwardEntryLine = inwardEntry.MergedLines.AddNew();
				inwardEntryLine.CL_LineNumber = 1;
				var inwardInvoice = inwardDeclaration.Invoices.AddNew();
				inwardInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				inwardInvoice.JZ_RX_NKInvoice_Currency = inwardDeclaration.LocalCurrencyCode;
				inwardInvoice.JZ_InvoiceAmount = 1000m;

				var inwardInvoiceLine1 = inwardInvoice.JobComInvoiceLines.AddNew();
				inwardInvoiceLine1.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				inwardInvoiceLine1.JI_PartNo = part1.OP_PartNum;
				inwardInvoiceLine1.JI_InvoiceQuantity = 100m;
				inwardInvoiceLine1.JI_InvoiceUQ = "NO";
				inwardInvoiceLine1.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine1.JI_CustomsQuantity = 10m;
				inwardInvoiceLine1.JI_LinePrice = 1000m;
				inwardInvoiceLine1.JI_CL = inwardEntryLine.PK;
				inwardInvoiceLine1.JI_AddInfo = "WRQ=100*WUV=NO*WRN=ENT2343*WRL=1";
				var job = new JobHeader.Loader(inwardDeclaration).TryCreate();
				job.JH_GE = Department.PK;

				var outwardDeclaration = Factory.New<BaseJobDeclaration>();
				outwardDeclaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				outwardDeclaration.JE_TransportMode = outwardDeclaration.TransportModeAirCodeForTesting;
				outwardDeclaration.JE_OH_Importer = importer.PK;
				outwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
				outwardDeclaration.JE_DeclarationReference = "B00000124";
				outwardDeclaration.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;
				outwardDeclaration.SetSupportsBondedWarehousingForTesting(true);

				var outwardEntry = outwardDeclaration.CustomsEntryHeaders.AddNew();
				outwardEntry.CH_MessageType = JobMessageTypeList.Codes.Import;
				outwardEntry.EntryNumber = "ENT2344";
				var outwardEntryLine = outwardEntry.MergedLines.AddNew();
				outwardEntryLine.CL_LineNumber = 1;
				outwardDeclaration.Invoices.DeleteAll();
				var outwardInvoice = outwardDeclaration.Invoices.AddNew();
				outwardInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				outwardInvoice.JZ_RX_NKInvoice_Currency = outwardDeclaration.LocalCurrencyCode;
				outwardInvoice.JZ_InvoiceAmount = 1000m;

				var outwardInvoiceLine1 = outwardInvoice.JobComInvoiceLines.AddNew();
				outwardInvoiceLine1.SetUseBondedWarehouseAutomationForTesting(true);
				outwardInvoiceLine1.JI_PartNo = part1.OP_PartNum;
				outwardInvoiceLine1.JI_InvoiceQuantity = 0m;
				outwardInvoiceLine1.JI_InvoiceUQ = "NO";
				outwardInvoiceLine1.JI_CustomsUnitQty = "KG";
				outwardInvoiceLine1.JI_CustomsQuantity = 10m;
				outwardInvoiceLine1.JI_LinePrice = 1000m;
				outwardInvoiceLine1.JI_CL = outwardEntryLine.PK;
				outwardInvoiceLine1.JI_AddInfo = "WRN=ENT2343*WRL=1";
				job = new JobHeader.Loader(outwardDeclaration).TryCreate();
				job.JH_GE = Department.PK;
				Factory.Save();
				inwardDeclaration.PublishShipmentForWHSInward(false);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 100m);

				using (var form = new BaseJobDeclarationFormForTest(outwardDeclaration))
				{
					var menu = form.EDIMenu;
					menu.Declaration = outwardDeclaration;
					menu.RefreshMenu();
					form.FireSaveButton();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
					AssertEquals(true, bondedWarehouseMenuItem.Visible);
					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					var updateBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("Update Inventory");
					AssertEquals(true, updateBondedWarehouseMenuItem.Visible);
					updateBondedWarehouseMenuItem.PerformClick();
					AssertEquals(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(2, UnitTestUserNotification.Instance.PreviousMessages.Length);
					AssertEquals(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), UnitTestUserNotification.Instance.PreviousMessages[0].Text);
					AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages[1].WasNone);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 100m);

					outwardInvoiceLine1.JI_InvoiceQuantity = 60m;
					form.FireSaveButton();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
					AssertEquals(true, bondedWarehouseMenuItem.Visible);
					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					updateBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("Update Inventory");
					AssertEquals(true, updateBondedWarehouseMenuItem.Visible);
					updateBondedWarehouseMenuItem.PerformClick();
					AssertEquals("Stock Release has been updated. (WHS Order:W00000002)", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(2, UnitTestUserNotification.Instance.PreviousMessages.Length);
					AssertEquals("Stock Release has been updated. (WHS Order:W00000002)", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
					AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages[1].WasNone);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 40m);

					outwardInvoiceLine1.JI_InvoiceQuantity = 0m;
					form.FireSaveButton();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
					AssertEquals(true, bondedWarehouseMenuItem.Visible);
					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					updateBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("Update Inventory");
					AssertEquals(true, updateBondedWarehouseMenuItem.Visible);
					updateBondedWarehouseMenuItem.PerformClick();
					AssertEquals(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(2, UnitTestUserNotification.Instance.PreviousMessages.Length);
					AssertEquals(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), UnitTestUserNotification.Instance.PreviousMessages[0].Text);
					AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages[1].WasNone);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 40m);
				}
			}
		}

		public void TestUpdateBondedWarehouseRequiresAllLineToHaveAProductButNotCancel()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());

			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var importer = Buyer;
				importer.OH_RL_NKClosestPort = "AUSYD";
				var warehouse = WarehouseAddress;
				warehouse.OH_RL_NKClosestPort = "AUSYD";
				var part = CreatePart(Factory, importer.PK);

				var inwardDeclaration = Factory.New<BaseJobDeclaration>();
				inwardDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				inwardDeclaration.JE_TransportMode = inwardDeclaration.TransportModeAirCodeForTesting;
				inwardDeclaration.JE_OH_Importer = importer.PK;
				inwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
				inwardDeclaration.JE_DeclarationReference = "B00000123";
				inwardDeclaration.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;
				inwardDeclaration.SetSupportsBondedWarehousingForTesting(true);

				var inwardEntry = inwardDeclaration.CustomsEntryHeaders.AddNew();
				inwardEntry.CH_MessageType = JobMessageTypeList.Codes.Import;
				inwardEntry.EntryNumber = "ENT2343";
				var inwardEntryLine = inwardEntry.MergedLines.AddNew();
				inwardEntryLine.CL_LineNumber = 1;
				var inwardInvoice = inwardDeclaration.Invoices.AddNew();
				inwardInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				inwardInvoice.JZ_RX_NKInvoice_Currency = inwardDeclaration.LocalCurrencyCode;
				inwardInvoice.JZ_InvoiceAmount = 1000m;

				var inwardInvoiceLine1 = inwardInvoice.JobComInvoiceLines.AddNew();
				inwardInvoiceLine1.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				inwardInvoiceLine1.JI_PartNo = part.OP_PartNum;
				inwardInvoiceLine1.JI_InvoiceQuantity = 100m;
				inwardInvoiceLine1.JI_InvoiceUQ = "NO";
				inwardInvoiceLine1.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine1.JI_CustomsQuantity = 10m;
				inwardInvoiceLine1.JI_LinePrice = 1000m;
				inwardInvoiceLine1.JI_CL = inwardEntryLine.PK;
				inwardInvoiceLine1.JI_AddInfo = "WRQ=100*WUV=NO";

				var inwardInvoiceLine2 = inwardInvoice.JobComInvoiceLines.AddNew();
				inwardInvoiceLine2.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				inwardInvoiceLine2.JI_PartNo = ZString.Empty;
				inwardInvoiceLine2.JI_Tariff = inwardInvoiceLine1.JI_Tariff;
				inwardInvoiceLine2.JI_InvoiceQuantity = 100m;
				inwardInvoiceLine2.JI_InvoiceUQ = "NO";
				inwardInvoiceLine2.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine2.JI_CustomsQuantity = 10m;
				inwardInvoiceLine2.JI_LinePrice = 1000m;
				inwardInvoiceLine2.JI_CL = inwardEntryLine.PK;
				inwardInvoiceLine2.JI_AddInfo = "WRQ=100*WUV=NO";
				var job = new JobHeader.Loader(inwardDeclaration).TryCreate();
				job.JH_GE = Department.PK;

				using (var form = new BaseJobDeclarationFormForTest(inwardDeclaration))
				{
					var menu = form.EDIMenu;
					menu.Declaration = inwardDeclaration;
					menu.RefreshMenu();
					form.FireSaveButton();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var bondedWarehouseMenuItem = menu.MenuItems.FindByName("bondedWarehouseMenuItem", true);
					AssertEquals(true, bondedWarehouseMenuItem.Visible);
					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					var updateBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByName("updateBondedWarehouseMenuItem", true);
					AssertEquals(true, updateBondedWarehouseMenuItem.Visible);
					updateBondedWarehouseMenuItem.PerformClick();
					AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 0m);

					inwardInvoiceLine2.JI_PartNo = part.OP_PartNum;
					form.FireSaveButton();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					bondedWarehouseMenuItem = menu.MenuItems.FindByName("bondedWarehouseMenuItem", true);
					AssertEquals(true, bondedWarehouseMenuItem.Visible);
					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					updateBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByName("updateBondedWarehouseMenuItem", true);
					AssertEquals(true, updateBondedWarehouseMenuItem.Visible);
					updateBondedWarehouseMenuItem.PerformClick();
					AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 200m);

					inwardInvoiceLine2.JI_PartNo = ZString.Empty;
					inwardInvoiceLine2.JI_Tariff = inwardInvoiceLine1.JI_Tariff;
					form.FireSaveButton();
					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					var cancelBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByName("cancelUpdateBondedWarehouseInwardMenuItem", true);
					AssertEquals(true, cancelBondedWarehouseMenuItem.Visible);
					cancelBondedWarehouseMenuItem.PerformClick();
					AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 0m);

					inwardInvoiceLine2.JI_PartNo = part.OP_PartNum;
					form.FireSaveButton();
					updateBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByName("updateBondedWarehouseMenuItem", true);
					AssertEquals(true, updateBondedWarehouseMenuItem.Visible);
					updateBondedWarehouseMenuItem.PerformClick();
					AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 200m);
				}

				var outwardDeclaration = Factory.New<BaseJobDeclaration>();
				outwardDeclaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				outwardDeclaration.JE_TransportMode = inwardDeclaration.TransportModeAirCodeForTesting;
				outwardDeclaration.JE_OH_Importer = importer.PK;
				outwardDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
				outwardDeclaration.JE_DeclarationReference = "B00000125";
				outwardDeclaration.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;
				outwardDeclaration.SetSupportsBondedWarehousingForTesting(true);

				var outwardEntry = outwardDeclaration.CustomsEntryHeaders.AddNew();
				outwardEntry.CH_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				outwardDeclaration.Invoices.DeleteAll();
				var outwardInvoice = outwardDeclaration.Invoices.AddNew();
				outwardInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				outwardInvoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
				outwardInvoice.JZ_InvoiceAmount = 600m;

				var outwardInvoiceLine1 = outwardInvoice.JobComInvoiceLines.AddNew();
				outwardInvoiceLine1.JI_PartNo = part.OP_PartNum;
				outwardInvoiceLine1.SetUseBondedWarehouseAutomationForTesting(true);
				outwardInvoiceLine1.JI_InvoiceQuantity = 60m;
				outwardInvoiceLine1.JI_AddInfo = "WRN=ENT2343*WRL=1";

				var outwardInvoiceLine2 = outwardInvoice.JobComInvoiceLines.AddNew();
				outwardInvoiceLine2.JI_PartNo = ZString.Empty;
				outwardInvoiceLine2.SetUseBondedWarehouseAutomationForTesting(true);
				outwardInvoiceLine2.JI_InvoiceQuantity = 50m;
				outwardInvoiceLine2.JI_AddInfo = "WRN=ENT2343*WRL=1";
				job = new JobHeader.Loader(outwardDeclaration).TryCreate();
				job.JH_GE = Department.PK;
				using (var form = new BaseJobDeclarationFormForTest(outwardDeclaration))
				{
					var menu = form.EDIMenu;
					menu.Declaration = outwardDeclaration;
					menu.RefreshMenu();
					form.FireSaveButton();
					var bondedWarehouseMenuItem = menu.MenuItems.FindByName("bondedWarehouseMenuItem", true);
					AssertEquals(true, bondedWarehouseMenuItem.Visible);
					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					var updateBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByName("updateBondedWarehouseMenuItem", true);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					updateBondedWarehouseMenuItem.PerformClick();
					AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 200m);

					outwardInvoiceLine2.JI_PartNo = part.OP_PartNum;
					outwardInvoiceLine2.SetUseBondedWarehouseAutomationForTesting(true);
					outwardInvoiceLine2.JI_InvoiceQuantity = 50m;
					outwardInvoiceLine2.JI_AddInfo = "WRN=ENT2343*WRL=1";
					form.FireSaveButton();
					bondedWarehouseMenuItem = menu.MenuItems.FindByName("bondedWarehouseMenuItem", true);
					AssertEquals(true, bondedWarehouseMenuItem.Visible);
					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					updateBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByName("updateBondedWarehouseMenuItem", true);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					updateBondedWarehouseMenuItem.PerformClick();
					AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 90m);

					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					var cancelBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByName("cancelBondedWarehouseMenuItem", true);
					AssertEquals(true, cancelBondedWarehouseMenuItem.Visible);
					outwardInvoiceLine2.JI_PartNo = ZString.Empty;
					form.FireSaveButton();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Yes to cancel
					cancelBondedWarehouseMenuItem.PerformClick();
					AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 200m);
				}
			}
		}

		public void TestCanAmendInwardWhenOutwardIsCancelled()
		{
			setupWHSUniversalXMLForTesting = BaseJobDeclaration.SetupWHSUniversalXMLForTesting();
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			dummy = DummyBusinessObject.New(Factory);
			formMock = new Mock<ZForm>(dummy);
			formMock.CallBase = true;
			form = formMock.Object;
			menu = new EDIMenu();
			form.Menu.MenuItems.Add(menu);
			declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.WarehouseDocAddress.E2_OA_Address = WarehouseAddress.MainAddress.PK;
			invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.SetSupportsBondedWarehousingForTesting(true);
			menu.Declaration = declaration;
			using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(declaration))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			}
			Factory.Save();

			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());

			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var importer = Buyer;
				importer.OH_RL_NKClosestPort = "AUSYD";
				var warehouse = WarehouseAddress;
				warehouse.OH_RL_NKClosestPort = "AUSYD";
				var part = CreatePart(Factory, importer.PK);

				var inwardDeclaration = Factory.New<BaseJobDeclaration>();
				inwardDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				inwardDeclaration.JE_TransportMode = inwardDeclaration.TransportModeAirCodeForTesting;
				inwardDeclaration.JE_OH_Importer = importer.PK;
				inwardDeclaration.JE_DeclarationReference = "B00000123";
				inwardDeclaration.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;
				inwardDeclaration.SetSupportsBondedWarehousingForTesting(true);

				var inwardEntry = inwardDeclaration.CustomsEntryHeaders.AddNew();
				inwardEntry.CH_MessageType = JobMessageTypeList.Codes.Import;
				inwardEntry.EntryNumber = "ENT2343";
				var inwardEntryLine = inwardEntry.MergedLines.AddNew();
				inwardEntryLine.CL_LineNumber = 1;
				var inwardInvoice = inwardDeclaration.Invoices.AddNew();
				inwardInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				inwardInvoice.JZ_RX_NKInvoice_Currency = inwardDeclaration.LocalCurrencyCode;
				inwardInvoice.JZ_InvoiceAmount = 1000m;

				var inwardInvoiceLine = inwardInvoice.JobComInvoiceLines.AddNew();
				inwardInvoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				inwardInvoiceLine.JI_PartNo = part.OP_PartNum;
				inwardInvoiceLine.JI_InvoiceQuantity = 100m;
				inwardInvoiceLine.JI_InvoiceUQ = "NO";
				inwardInvoiceLine.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine.JI_CustomsQuantity = 10m;
				inwardInvoiceLine.JI_LinePrice = 1000m;
				inwardInvoiceLine.JI_CL = inwardEntryLine.PK;
				inwardInvoiceLine.JI_AddInfo = "WRQ=100*WUV=NO";
				var job = new JobHeader.Loader(inwardDeclaration).TryCreate();
				job.JH_GE = Department.PK;
				inwardDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

				var outwardDeclaration = Factory.New<BaseJobDeclaration>();
				outwardDeclaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				outwardDeclaration.JE_TransportMode = inwardDeclaration.TransportModeAirCodeForTesting;
				outwardDeclaration.JE_OH_Importer = importer.PK;
				outwardDeclaration.JE_DeclarationReference = "B00000125";
				outwardDeclaration.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;
				outwardDeclaration.SetSupportsBondedWarehousingForTesting(true);

				var outwardEntry = outwardDeclaration.CustomsEntryHeaders.AddNew();
				outwardEntry.CH_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				outwardDeclaration.Invoices.DeleteAll();
				var outwardInvoice = outwardDeclaration.Invoices.AddNew();
				outwardInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				outwardInvoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
				outwardInvoice.JZ_InvoiceAmount = 600m;

				var outwardInvoiceLine = outwardInvoice.JobComInvoiceLines.AddNew();
				outwardInvoiceLine.JI_PartNo = part.OP_PartNum;
				outwardInvoiceLine.JI_InvoiceQuantity = 60m;
				outwardInvoiceLine.JI_AddInfo = "WRN=ENT2343*WRL=1";
				outwardInvoiceLine.SetUseBondedWarehouseAutomationForTesting(true);
				job = new JobHeader.Loader(outwardDeclaration).TryCreate();
				job.JH_GE = Department.PK;
				outwardDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

				using (var inwardForm = new BaseJobDeclarationFormForTest(inwardDeclaration))
				using (var outwardForm = new BaseJobDeclarationFormForTest(outwardDeclaration))
				{
					var inwardMenu = inwardForm.EDIMenu;
					inwardMenu.Declaration = inwardDeclaration;
					inwardMenu.RefreshMenu();
					inwardForm.FireSaveButton();
					var inwardBondedWarehouseMenuItem = inwardMenu.MenuItems.FindByText("Inventory Management");
					AssertEquals(true, inwardBondedWarehouseMenuItem.Visible);
					inwardBondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					var inwardUpdateBondedWarehouseMenuItem = inwardBondedWarehouseMenuItem.MenuItems.FindByText("Update Inventory");
					AssertEquals(true, inwardUpdateBondedWarehouseMenuItem.Visible);
					inwardUpdateBondedWarehouseMenuItem.PerformClick();
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 100m);

					var outwardMenu = outwardForm.EDIMenu;
					outwardMenu.Declaration = outwardDeclaration;
					outwardMenu.RefreshMenu();
					outwardForm.FireSaveButton();
					var outwardBondedWarehouseMenuItem = outwardMenu.MenuItems.FindByText("Inventory Management");
					AssertEquals(true, outwardBondedWarehouseMenuItem.Visible);
					outwardBondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					var outwardUpdateBondedWarehouseMenuItem = outwardBondedWarehouseMenuItem.MenuItems.FindByText("Update Inventory");
					AssertEquals(true, outwardUpdateBondedWarehouseMenuItem.Visible);
					outwardUpdateBondedWarehouseMenuItem.PerformClick();
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 40m);

					inwardInvoiceLine.JI_InvoiceQuantity = 200m;
					inwardInvoiceLine.JI_CustomsQuantity = 20m;
					inwardForm.FireSaveButton();
					inwardBondedWarehouseMenuItem = inwardMenu.MenuItems.FindByText("Inventory Management");
					AssertEquals(true, inwardBondedWarehouseMenuItem.Visible);
					inwardBondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					inwardUpdateBondedWarehouseMenuItem = inwardBondedWarehouseMenuItem.MenuItems.FindByText("Update Inventory");
					AssertEquals(true, inwardUpdateBondedWarehouseMenuItem.Visible);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					inwardUpdateBondedWarehouseMenuItem.PerformClick();
					var message = "Stock Levels have been updated. (WHS Receipt:W00000001)";
					AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 140m);

					outwardBondedWarehouseMenuItem = outwardMenu.MenuItems.FindByText("Inventory Management");
					AssertEquals(true, outwardBondedWarehouseMenuItem.Visible);
					outwardBondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					var cancelBondedWarehouseMenuItem = outwardBondedWarehouseMenuItem.MenuItems.FindByText("&Cancel Inventory Stock Release");
					AssertEquals(true, cancelBondedWarehouseMenuItem.Visible);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Yes to continue with cancel
					cancelBondedWarehouseMenuItem.PerformClick();
					AssertEquals("Stock Release has been canceled. (WHS Order:W00000002)", UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 200m);

					inwardBondedWarehouseMenuItem = inwardMenu.MenuItems.FindByText("Inventory Management");
					AssertEquals(true, inwardBondedWarehouseMenuItem.Visible);
					inwardBondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					inwardUpdateBondedWarehouseMenuItem = inwardBondedWarehouseMenuItem.MenuItems.FindByText("Update Inventory");
					AssertEquals(true, inwardUpdateBondedWarehouseMenuItem.Visible);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					inwardUpdateBondedWarehouseMenuItem.PerformClick();
					AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 200m);
				}
			}
		}

		public void TestWarehouseAutomationEndToEnd()
		{
			setupWHSUniversalXMLForTesting = BaseJobDeclaration.SetupWHSUniversalXMLForTesting();
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			dummy = DummyBusinessObject.New(Factory);
			formMock = new Mock<ZForm>(dummy);
			formMock.CallBase = true;
			form = formMock.Object;
			menu = new EDIMenu();
			form.Menu.MenuItems.Add(menu);
			declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.WarehouseDocAddress.E2_OA_Address = WarehouseAddress.MainAddress.PK;
			invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.SetSupportsBondedWarehousingForTesting(true);
			menu.Declaration = declaration;
			using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(declaration))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			}
			Factory.Save();

			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());

			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var importer = Buyer;
				importer.OH_RL_NKClosestPort = "AUSYD";
				var warehouse = WarehouseAddress;
				warehouse.OH_RL_NKClosestPort = "AUSYD";
				var part = CreatePart(Factory, importer.PK);

				var inwardDeclaration = Factory.New<BaseJobDeclaration>();
				inwardDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				inwardDeclaration.JE_TransportMode = inwardDeclaration.TransportModeAirCodeForTesting;
				inwardDeclaration.JE_OH_Importer = importer.PK;
				inwardDeclaration.JE_DeclarationReference = "B00000123";
				inwardDeclaration.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;
				inwardDeclaration.SetSupportsBondedWarehousingForTesting(true);

				var inwardEntry = inwardDeclaration.CustomsEntryHeaders.AddNew();
				inwardEntry.CH_MessageType = JobMessageTypeList.Codes.Import;
				inwardEntry.EntryNumber = "ENT2343";
				var inwardEntryLine = inwardEntry.MergedLines.AddNew();
				inwardEntryLine.CL_LineNumber = 1;
				var inwardInvoice = inwardDeclaration.Invoices.AddNew();
				inwardInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				inwardInvoice.JZ_RX_NKInvoice_Currency = inwardDeclaration.LocalCurrencyCode;
				inwardInvoice.JZ_InvoiceAmount = 1000m;

				var inwardInvoiceLine = inwardInvoice.JobComInvoiceLines.AddNew();
				inwardInvoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				inwardInvoiceLine.JI_PartNo = part.OP_PartNum;
				inwardInvoiceLine.JI_InvoiceQuantity = 100m;
				inwardInvoiceLine.JI_InvoiceUQ = "NO";
				inwardInvoiceLine.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine.JI_CustomsQuantity = 10m;
				inwardInvoiceLine.JI_LinePrice = 1000m;
				inwardInvoiceLine.JI_CL = inwardEntryLine.PK;
				inwardInvoiceLine.JI_AddInfo = "WRQ=100*WUV=NO";
				var job = new JobHeader.Loader(inwardDeclaration).TryCreate();
				job.JH_GE = Department.PK;
				Assert("CH_HasManualWhsUpdate", !inwardEntry.CH_HasManualWhsUpdate);

				using (var form = new BaseJobDeclarationFormForTest(inwardDeclaration))
				{
					var menu = form.EDIMenu;
					menu.Declaration = inwardDeclaration;
					menu.RefreshMenu();
					form.FireSaveButton();
					var bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
					var updateBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("Update Inventory");
					updateBondedWarehouseMenuItem.PerformClick();
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 100m);
					Assert("CH_HasManualWhsUpdate changed", inwardEntry.CH_HasManualWhsUpdate);

					var cancelBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("Cancel Inventory");
					cancelBondedWarehouseMenuItem.PerformClick();
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2343-1", 0m);
					Assert("CH_HasManualWhsUpdate changed", !inwardEntry.CH_HasManualWhsUpdate);

					inwardDeclaration.JE_DeclarationReference = "B00000124";
					inwardEntry.EntryNumber = "ENT2344";
					form.FireSaveButton();
					updateBondedWarehouseMenuItem.PerformClick();
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2344-1", 100m);
				}

				var outwardDeclaration = Factory.New<BaseJobDeclaration>();
				outwardDeclaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				outwardDeclaration.JE_TransportMode = inwardDeclaration.TransportModeAirCodeForTesting;
				outwardDeclaration.JE_OH_Importer = importer.PK;
				outwardDeclaration.JE_DeclarationReference = "B00000125";
				outwardDeclaration.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;
				outwardDeclaration.SetSupportsBondedWarehousingForTesting(true);

				var outwardEntry = outwardDeclaration.CustomsEntryHeaders.AddNew();
				outwardEntry.CH_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				outwardEntry.EntryNumber = "ENT2344";
				var outwardEntryLine = outwardEntry.MergedLines.AddNew();
				outwardEntryLine.CL_LineNumber = 1;
				outwardDeclaration.Invoices.DeleteAll();
				var outwardInvoice = outwardDeclaration.Invoices.AddNew();
				outwardInvoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				outwardInvoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
				outwardInvoice.JZ_InvoiceAmount = 600m;

				var outwardInvoiceLine = outwardInvoice.JobComInvoiceLines.AddNew();
				outwardInvoiceLine.JI_PartNo = part.OP_PartNum;
				outwardInvoiceLine.JI_InvoiceQuantity = 60m;
				outwardInvoiceLine.JI_CL = outwardEntryLine.PK;
				outwardInvoiceLine.JI_AddInfo = "WRN=ENT2344*WRL=1";
				job = new JobHeader.Loader(outwardDeclaration).TryCreate();
				job.JH_GE = Department.PK;
				Assert("CH_HasManualWhsUpdate", !outwardEntry.CH_HasManualWhsUpdate);
				using (var form = new BaseJobDeclarationFormForTest(outwardDeclaration))
				{
					var menu = form.EDIMenu;
					menu.Declaration = outwardDeclaration;
					menu.RefreshMenu();
					form.FireSaveButton();

					var bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
					var synchronizeBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("&Synchronize with Inventory");
					synchronizeBondedWarehouseMenuItem.PerformClick();
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2344-1", 100m);
					outwardInvoiceLine = outwardInvoice.JobComInvoiceLines[0];
					AssertEquals(600m, outwardInvoiceLine.JI_LinePrice);
					outwardInvoiceLine.JI_InvoiceQuantity = 60m;
					form.FireSaveButton();

					var updateBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("Update Inventory");
					updateBondedWarehouseMenuItem.PerformClick();
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2344-1", 40m);
					Assert("CH_HasManualWhsUpdate changed", outwardEntry.CH_HasManualWhsUpdate);

					var cancelBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("&Cancel Inventory Stock Release");
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Yes to cancel
					cancelBondedWarehouseMenuItem.PerformClick();
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT2344-1", 100m);
				}
			}
		}

		public void TestCancelBondedWarehouseIntegrationMenuItem_Click()
		{
			setupWHSUniversalXMLForTesting = BaseJobDeclaration.SetupWHSUniversalXMLForTesting();
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			dummy = DummyBusinessObject.New(Factory);
			formMock = new Mock<ZForm>(dummy) { CallBase = true };
			form = formMock.Object;
			menu = new EDIMenu();
			form.Menu.MenuItems.Add(menu);
			declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.WarehouseDocAddress.E2_OA_Address = WarehouseAddress.MainAddress.PK;
			invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.SetSupportsBondedWarehousingForTesting(true);
			menu.Declaration = declaration;
			using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(declaration))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			}
			Factory.Save();

			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());

			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var inwardDeclaration = Factory.New<BaseJobDeclaration>();
				inwardDeclaration.JE_DeclarationReference = "B0234233443";
				inwardDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				inwardDeclaration.JE_OH_Importer = Buyer.PK;
				Buyer.CompanyData.OB_IMUsedBondedWhs = true;
				Buyer.OH_RL_NKClosestPort = "AUSYD";

				var warehouse = WarehouseAddress;
				warehouse.OH_RL_NKClosestPort = "AUSYD";
				inwardDeclaration.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;
				var inwardInvoice = inwardDeclaration.Invoices.AddNew();
				inwardInvoice.JZ_RX_NKInvoice_Currency = inwardDeclaration.LocalCurrencyCode;
				var inwardInvoiceLine = inwardInvoice.JobComInvoiceLines.AddNew();
				inwardInvoiceLine.JI_PartNo = Part.OP_PartNum;
				inwardInvoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				inwardInvoiceLine.JI_InvoiceQuantity = 1;
				inwardInvoiceLine.JI_InvoiceUQ = "NO";
				inwardInvoiceLine.JI_LinePrice = 1500m;
				inwardInvoiceLine.JI_CustomsQuantity = 12;
				inwardInvoiceLine.JI_CustomsUnitQty = "PK";
				inwardInvoiceLine.JI_AddInfo = "WRQ=1*WUV=NO";
				var inwardEntryHeader = inwardDeclaration.CustomsEntryHeaders.AddNew();
				inwardEntryHeader.EntryNumber = "ENT12312";
				var inwardEntryLine = inwardEntryHeader.MergedLines.AddNew();
				inwardEntryLine.CL_LineNumber = 1;
				inwardInvoiceLine.JI_CL = inwardEntryLine.PK;
				Factory.Save();
				inwardDeclaration.PublishShipmentForWHSInward(false);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT12312-1", 1m);
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-2);
				declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
				invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				declaration.SetSupportsBondedWarehousingForTesting(true);
				declaration.JE_OH_Importer = Buyer.PK;
				declaration.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;
				menu.Declaration = declaration;
				var outwardEntryHeader = declaration.CustomsEntryHeaders.AddNew();
				outwardEntryHeader.EntryNumber = "ENT12312";
				var outwardEntryLine = outwardEntryHeader.MergedLines.AddNew();
				outwardEntryLine.CL_LineNumber = 1;
				invoiceLine.JI_CL = outwardEntryLine.PK;
				invoiceLine.JI_PartNo = Part.OP_PartNum;
				invoiceLine.JI_InvoiceQuantity = 1;
				invoiceLine.JI_InvoiceUQ = "NO";
				invoiceLine.SetUseBondedWarehouseAutomationForTesting(true);
				invoiceLine.JI_AddInfo = "WRN=ENT12312*WRL=1";
				Factory.Save();
				declaration.PublishShipmentForWHSOutward(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT12312-1", 0m);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No); // No for confirmation
				var cancelBondedWarehouseMenuItem = menu.MenuItems.FindByName("cancelBondedWarehouseMenuItem", true);
				cancelBondedWarehouseMenuItem.PerformClick();
				AssertEquals("Should not call CancelBondedWarehouseIntegration", true, invoiceLine.UseBondedWarehouseAutomation);
				AssertEquals(EDIMenu.ConfirmCancelBondedWarehouseOutwardAutomationMessageOld("Inventory Management"), UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("DecHasChanges", false, declaration.HasChanges);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT12312-1", 0m);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				cancelBondedWarehouseMenuItem.PerformClick();
				AssertEquals("Should call CancelBondedWarehouseIntegration", false, invoiceLine.UseBondedWarehouseAutomation);
				AssertEquals(EDIMenu.ConfirmCancelBondedWarehouseOutwardAutomationMessageOld("Inventory Management"), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("DecHasChanges", true, declaration.HasChanges);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT12312-1", 0m);

				declaration.JE_SystemCreateTimeUtc = ZDateTime.Today;
				cancelBondedWarehouseMenuItem.PerformClick();
				AssertEquals(EDIMenu.SaveDeclarationFirstMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No); // No for confirmation
				invoiceLine.UseBondedWarehouseAutomation = true;
				cancelBondedWarehouseMenuItem.PerformClick();
				AssertEquals("Should not call CancelBondedWarehouseIntegration", true, invoiceLine.UseBondedWarehouseAutomation);
				AssertEquals(EDIMenu.ConfirmCancelBondedWarehouseOutwardAutomationMessageNew("Inventory Management"), UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("DecHasChanges", false, declaration.HasChanges);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT12312-1", 0m);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				cancelBondedWarehouseMenuItem.PerformClick();
				AssertEquals("Stock Release has been canceled. (WHS Order:W00000002)", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("DecHasChanges", false, declaration.HasChanges);
				AssertEquals("UseBondedWarehouseAutomation is not used via Universal XML so there is not need to clear it out", true, invoiceLine.UseBondedWarehouseAutomation);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT12312-1", 1m);
			}
		}

		public void TestSynchronizeWithOrdersMenuItem()
		{
			SetUpMoq();
			menu.BondedWarehouseMenuItem_Popup(null, null);
			Assert(!menu.synchronizeWithOrdersMenuItem.Visible);

			declarationMock.Protected().Setup<bool>("IsBondedWarehousePermitEnabledCore").Returns(true);
			menu.BondedWarehouseMenuItem_Popup(null, null);
			Assert(menu.synchronizeWithOrdersMenuItem.Visible);
		}

		public void TestSynchronizeWithBondedWarehouseMenuItem_Click()
		{
			SetUpMoq();
			declarationMock.Verify(m => m.CreateAndUpdateInvoicesForExBondAutomation(), Times.Never);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			declaration.Invoices.DeleteAll();
			menu.SynchronizeWithBondedWarehouseMenuItem_Click(null, null);
			AssertEquals("Please enter at least one invoice line with Inventory Management integration enabled.", UnitTestUserNotification.Instance.LastMessage.Text);

			BaseJobComInvoiceLine line = declaration.FilteredInvoiceLines.AddNew();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			menu.SynchronizeWithBondedWarehouseMenuItem_Click(null, null);
			AssertEquals("Please enter at least one invoice line with Inventory Management integration enabled.", UnitTestUserNotification.Instance.LastMessage.Text);

			declarationMock.Setup(m => m.CreateAndUpdateInvoicesForExBondAutomation());
			line.SetUseBondedWarehouseAutomationForTesting(true);
			Factory.InvalidateCachedProperties();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			menu.SynchronizeWithBondedWarehouseMenuItem_Click(null, null);
			Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
			declarationMock.VerifyAll();
		}

		public void TestSynchronizeWithBondedWarehouseMenuItem_Click_SelectInvoiceLinesTabPage_InvoiceLinesHaveRowErrors()
		{
			SetUpMoq();
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			invoiceLine.AddRowError("Find Me!");
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				var menu = new EDIMenu { Declaration = declaration };
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.SynchronizeWithBondedWarehouseMenuItem_Click(null, null);
				var customsBrokerageUserControl = form.CustomsBrokerageUserControl;
				AssertEquals(customsBrokerageUserControl.InvoiceLinesTabPage, customsBrokerageUserControl.MainTabControl.SelectedTab);
			}
		}

		public void TestSynchronizeWithBondedWarehouseMenuItem_Click_SelectInvoiceLinesTabPage_WhenThereIsAnyError()
		{
			SetUpMoq();
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			invoiceLine.JI_BondedWhsQuantity = 0m;
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				var menu = new EDIMenu { Declaration = declaration };
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.SynchronizeWithBondedWarehouseMenuItem_Click(null, null);
				var customsBrokerageUserControl = form.CustomsBrokerageUserControl;
				AssertEquals(customsBrokerageUserControl.InvoiceLinesTabPage, customsBrokerageUserControl.MainTabControl.SelectedTab);
			}
		}

		public void TestInventoriesSelectionFromBondedWarehouseMenuItem_Click()
		{
			setupWHSUniversalXMLForTesting = BaseJobDeclaration.SetupWHSUniversalXMLForTesting();
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			dummy = DummyBusinessObject.New(Factory);
			formMock = new Mock<ZForm>(dummy);
			formMock.CallBase = true;
			form = formMock.Object;
			menu = new EDIMenu();
			form.Menu.MenuItems.Add(menu);
			declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.WarehouseDocAddress.E2_OA_Address = WarehouseAddress.MainAddress.PK;
			invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.SetSupportsBondedWarehousingForTesting(true);
			menu.Declaration = declaration;
			using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(declaration))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			}
			Factory.Save();

			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			using (var form = new BaseJobDeclarationForm(declaration))
			{
				var menu = new EDIMenu();
				menu.Declaration = declaration;
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.BondedWarehouseMenuItem_Popup(null, null);
				AssertEquals(true, menu.bondedWarehouseMenuItem.Visible);
				AssertEquals(true, menu.inventoriesSelectionFromBondedWarehouseMenuItem.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
				menu.inventoriesSelectionFromBondedWarehouseMenuItem.PerformClick();
				AssertEquals(typeof(InventorySelectionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertNotEquals(form.CustomsBrokerageUserControl.InvoiceLinesTabPage, form.CustomsBrokerageUserControl.MainTabControl.SelectedTab);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				menu.inventoriesSelectionFromBondedWarehouseMenuItem.PerformClick();
				AssertEquals(typeof(InventorySelectionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals(form.CustomsBrokerageUserControl.InvoiceLinesTabPage, form.CustomsBrokerageUserControl.MainTabControl.SelectedTab);
			}
			var shipment = Factory.New<Freight.Forwarding.Business.ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new Freight.Forwarding.GUI.ShipmentForm(shipment))
			{
				var menu = new EDIMenu();
				menu.Declaration = declaration;
				shipmentForm.Menu.MenuItems.Add(menu);
				shipmentForm.Show();
				var plugin = (BaseBrokeragePlugIn)shipmentForm.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.Customs.JobDeclaration);
				plugin.OnGUIShown();
				var userControl = (BaseCustomsBrokerageUserControl)plugin.UserControl;

				menu.BondedWarehouseMenuItem_Popup(null, null);
				AssertEquals(true, menu.bondedWarehouseMenuItem.Visible);
				AssertEquals(true, menu.inventoriesSelectionFromBondedWarehouseMenuItem.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
				menu.inventoriesSelectionFromBondedWarehouseMenuItem.PerformClick();
				AssertEquals(typeof(InventorySelectionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertNotEquals(userControl.InvoiceLinesTabPage, userControl.MainTabControl.SelectedTab);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				menu.inventoriesSelectionFromBondedWarehouseMenuItem.PerformClick();
				AssertEquals(typeof(InventorySelectionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals(userControl.InvoiceLinesTabPage, userControl.MainTabControl.SelectedTab);
			}
		}

		public void TestBondedWarehouseMenuItemSecurity()
		{
			setupWHSUniversalXMLForTesting = BaseJobDeclaration.SetupWHSUniversalXMLForTesting();
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			dummy = DummyBusinessObject.New(Factory);
			formMock = new Mock<ZForm>(dummy);
			formMock.CallBase = true;
			form = formMock.Object;
			menu = new EDIMenu();
			form.Menu.MenuItems.Add(menu);
			declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.WarehouseDocAddress.E2_OA_Address = WarehouseAddress.MainAddress.PK;
			invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.SetSupportsBondedWarehousingForTesting(true);
			menu.Declaration = declaration;
			using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(declaration))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			}
			Factory.Save();

			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var helper = new WhsDataTestHelper(Factory);
				var declaration = helper.GetNewDeclaration(JobMessageTypeList.Codes.Import, "BIMP000001", "EN012312", 100m);
				var job = new JobHeader.Loader(declaration).TryCreate();
				job.JH_GE = Department.PK;
				Factory.Save();
				var entry = declaration.ActiveEntryHeaders[0];
				Assert("CH_HasManualWhsUpdate", !entry.CH_HasManualWhsUpdate);

				using (helper.WhsHelper.UsePutawayEngineManagerMock())
				using (helper.WhsHelper.UseAllocationEngineMock())
				using (var form = new BaseJobDeclarationForm(declaration))
				{
					var menu = new EDIMenu();
					menu.Declaration = declaration;
					form.Menu.MenuItems.Add(menu);
					form.Show();
					Env.Security.CustomsBondedWhsUpdate.IsAllowed = false;
					Env.Security.CustomsBondedWhsCancel.IsAllowed = false;
					Env.Security.CustomsBondedWhsDisable.IsAllowed = false;
					menu.BondedWarehouseMenuItem_Popup(null, null);
					AssertEquals(true, menu.bondedWarehouseMenuItem.Visible);
					AssertEquals(true, menu.updateBondedWarehouseMenuItem.Visible);
					AssertEquals(false, menu.cancelBondedWarehouseMenuItem.Visible);
					AssertEquals(false, menu.cancelUpdateBondedWarehouseInwardMenuItem.Visible);
					AssertEquals(true, menu.disableBondedWarehouseIntegrationMenuItem.Visible);
					menu.updateBondedWarehouseMenuItem.PerformClick();
					AssertEquals("WarehouseTransactionStatus", "", declaration.WarehouseTransactionStatus);
					AssertContains("No permission to Update", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("EN012312-1", 0m);
					Assert("CH_HasManualWhsUpdate no change", !entry.CH_HasManualWhsUpdate);

					Env.Security.CustomsBondedWhsUpdate.IsAllowed = true;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menu.updateBondedWarehouseMenuItem.PerformClick();
					Assert("CH_HasManualWhsUpdate changed", entry.CH_HasManualWhsUpdate);
					AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);
					AssertNotContains("Has permission to Update", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("EN012312-1", 100m);

					menu.BondedWarehouseMenuItem_Popup(null, null);
					AssertEquals(true, menu.bondedWarehouseMenuItem.Visible);
					AssertEquals(true, menu.updateBondedWarehouseMenuItem.Visible);
					AssertEquals(false, menu.cancelBondedWarehouseMenuItem.Visible);
					AssertEquals(true, menu.cancelUpdateBondedWarehouseInwardMenuItem.Visible);
					AssertEquals(false, menu.disableBondedWarehouseIntegrationMenuItem.Visible);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menu.cancelUpdateBondedWarehouseInwardMenuItem.PerformClick();
					AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);
					AssertContains("No permission to Cancel", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("EN012312-1", 100m);
					Assert("CH_HasManualWhsUpdate no change", entry.CH_HasManualWhsUpdate);

					Env.Security.CustomsBondedWhsCancel.IsAllowed = true;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menu.cancelUpdateBondedWarehouseInwardMenuItem.PerformClick();
					Assert("CH_HasManualWhsUpdate changed", !entry.CH_HasManualWhsUpdate);
					AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCanceled, declaration.WarehouseTransactionStatus);
					AssertNotContains("Has permission to Cancel", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("EN012312-1", 0m);

					menu.updateBondedWarehouseMenuItem.PerformClick();
					Assert("CH_HasManualWhsUpdate changed", entry.CH_HasManualWhsUpdate);
					AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("EN012312-1", 100m);

					menu.BondedWarehouseMenuItem_Popup(null, null);
					AssertEquals(true, menu.bondedWarehouseMenuItem.Visible);
					AssertEquals(true, menu.updateBondedWarehouseMenuItem.Visible);
					AssertEquals(false, menu.cancelBondedWarehouseMenuItem.Visible);
					AssertEquals(true, menu.cancelUpdateBondedWarehouseInwardMenuItem.Visible);
					AssertEquals(false, menu.disableBondedWarehouseIntegrationMenuItem.Visible);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menu.disableBondedWarehouseIntegrationMenuItem.PerformClick();
					AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);
					AssertContains("No permission to Disable", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("EN012312-1", 100m);

					Env.Security.CustomsBondedWhsDisable.IsAllowed = true;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menu.disableBondedWarehouseIntegrationMenuItem.PerformClick();
					AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, declaration.WarehouseTransactionStatus);
					AssertEquals("Inventory Management Integration has been disabled.", UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("EN012312-1", 100m);

					menu.BondedWarehouseMenuItem_Popup(null, null);
					AssertEquals(true, menu.bondedWarehouseMenuItem.Visible);
					AssertEquals(true, menu.updateBondedWarehouseMenuItem.Visible);
					AssertEquals(false, menu.cancelBondedWarehouseMenuItem.Visible);
					AssertEquals(false, menu.cancelUpdateBondedWarehouseInwardMenuItem.Visible);
					AssertEquals(false, menu.disableBondedWarehouseIntegrationMenuItem.Visible);
				}

				declaration = helper.GetNewDeclaration(JobMessageTypeList.Codes.ExWarehouse, "BEXW000001", "EN012312", 40m);
				var outwardInvoiceLine = declaration.InvoiceLines[0];
				outwardInvoiceLine.SetUseBondedWarehouseAutomationForTesting(true);
				outwardInvoiceLine.JI_AddInfo = "WRQ=40*WUV=NO*WRN=EN012312*WRL=1";
				job = new JobHeader.Loader(declaration).TryCreate();
				job.JH_GE = Department.PK;
				Factory.Save();
				using (var form = new BaseJobDeclarationForm(declaration))
				using (helper.WhsHelper.UseAllocationEngineMock())
				{
					var menu = new EDIMenu();
					menu.Declaration = declaration;
					form.Menu.MenuItems.Add(menu);
					form.Show();
					Env.Security.CustomsBondedWhsUpdate.IsAllowed = false;
					Env.Security.CustomsBondedWhsCancel.IsAllowed = false;
					Env.Security.CustomsBondedWhsDisable.IsAllowed = false;
					menu.BondedWarehouseMenuItem_Popup(null, null);
					AssertEquals(true, menu.bondedWarehouseMenuItem.Visible);
					AssertEquals(true, menu.updateBondedWarehouseMenuItem.Visible);
					AssertEquals(false, menu.cancelBondedWarehouseMenuItem.Visible);
					AssertEquals(false, menu.cancelUpdateBondedWarehouseInwardMenuItem.Visible);
					AssertEquals(true, menu.disableBondedWarehouseIntegrationMenuItem.Visible);
					menu.updateBondedWarehouseMenuItem.PerformClick();
					AssertEquals("WarehouseTransactionStatus", "", declaration.WarehouseTransactionStatus);
					AssertContains("No permission to Update", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("EN012312-1", 100m);

					Env.Security.CustomsBondedWhsUpdate.IsAllowed = true;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menu.updateBondedWarehouseMenuItem.PerformClick();
					AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, declaration.WarehouseTransactionStatus);
					AssertNotContains("Has permission to Update", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("EN012312-1", 60m);

					menu.BondedWarehouseMenuItem_Popup(null, null);
					AssertEquals(true, menu.bondedWarehouseMenuItem.Visible);
					AssertEquals(true, menu.updateBondedWarehouseMenuItem.Visible);
					AssertEquals(true, menu.cancelBondedWarehouseMenuItem.Visible);
					AssertEquals(false, menu.cancelUpdateBondedWarehouseInwardMenuItem.Visible);
					AssertEquals(false, menu.disableBondedWarehouseIntegrationMenuItem.Visible);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					menu.cancelBondedWarehouseMenuItem.PerformClick();
					AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, declaration.WarehouseTransactionStatus);
					AssertContains("No permission to Cancel", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("EN012312-1", 60m);

					Env.Security.CustomsBondedWhsCancel.IsAllowed = true;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					menu.cancelBondedWarehouseMenuItem.PerformClick();
					AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCanceled, declaration.WarehouseTransactionStatus);
					AssertNotContains("Has permission to Cancel", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("EN012312-1", 100m);

					menu.updateBondedWarehouseMenuItem.PerformClick();
					form.FireSaveButton();
					AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, declaration.WarehouseTransactionStatus);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("EN012312-1", 60m);

					menu.BondedWarehouseMenuItem_Popup(null, null);
					AssertEquals(true, menu.bondedWarehouseMenuItem.Visible);
					AssertEquals(true, menu.updateBondedWarehouseMenuItem.Visible);
					AssertEquals(true, menu.cancelBondedWarehouseMenuItem.Visible);
					AssertEquals(false, menu.cancelUpdateBondedWarehouseInwardMenuItem.Visible);
					AssertEquals(false, menu.disableBondedWarehouseIntegrationMenuItem.Visible);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menu.disableBondedWarehouseIntegrationMenuItem.PerformClick();
					AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, declaration.WarehouseTransactionStatus);
					AssertContains("No permission to Disable", SecurityCore.SecurityErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("EN012312-1", 60m);

					Env.Security.CustomsBondedWhsDisable.IsAllowed = true;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menu.disableBondedWarehouseIntegrationMenuItem.PerformClick();
					AssertEquals("WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, declaration.WarehouseTransactionStatus);
					AssertEquals("Inventory Management Integration has been disabled.", UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("EN012312-1", 60m);

					menu.BondedWarehouseMenuItem_Popup(null, null);
					AssertEquals(true, menu.bondedWarehouseMenuItem.Visible);
					AssertEquals(true, menu.updateBondedWarehouseMenuItem.Visible);
					AssertEquals(false, menu.cancelBondedWarehouseMenuItem.Visible);
					AssertEquals(false, menu.cancelUpdateBondedWarehouseInwardMenuItem.Visible);
					AssertEquals(false, menu.disableBondedWarehouseIntegrationMenuItem.Visible);
				}
			}
		}

		public void TestWhsAutomationViaUniversalXML()
		{
			setupWHSUniversalXMLForTesting = BaseJobDeclaration.SetupWHSUniversalXMLForTesting();
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			dummy = DummyBusinessObject.New(Factory);
			formMock = new Mock<ZForm>(dummy);
			formMock.CallBase = true;
			form = formMock.Object;
			menu = new EDIMenu();
			form.Menu.MenuItems.Add(menu);
			declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.WarehouseDocAddress.E2_OA_Address = WarehouseAddress.MainAddress.PK;
			invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.SetSupportsBondedWarehousingForTesting(true);
			menu.Declaration = declaration;
			using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(declaration))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			}
			Factory.Save();

			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());

			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var auDeclaration = Factory.New<BaseJobDeclaration>();
				declaration = Factory.New<BaseJobDeclaration>();
				// Inward Testing
				declaration.JE_DeclarationReference = "B0234233443";
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_OH_Importer = Buyer.PK;
				Buyer.OH_RL_NKClosestPort = "AUSYD";

				var warehouse = WarehouseAddress;
				warehouse.OH_RL_NKClosestPort = "AUSYD";
				declaration.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;
				invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				declaration.SetSupportsBondedWarehousingForTesting(true);
				menu.Declaration = declaration;

				var invoice = declaration.Invoices[0];
				invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
				invoiceLine.JI_PartNo = Part.OP_PartNum;
				invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				invoiceLine.JI_InvoiceQuantity = 1;
				invoiceLine.JI_InvoiceUQ = "NO";
				invoiceLine.JI_LinePrice = 1500m;
				invoiceLine.JI_CustomsQuantity = 12;
				invoiceLine.JI_CustomsUnitQty = "PK";
				invoiceLine.JI_AddInfo = "WRQ=1*WUV=NO";
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.EntryNumber = "ENT12312";
				var entryLine = entryHeader.MergedLines.AddNew();
				entryLine.CL_LineNumber = 1;
				invoiceLine.JI_CL = entryLine.PK;
				menu.BondedWarehouseMenuItem_Popup(null, null);
				AssertEquals(false, menu.bondedWarehouseMenuItem.Visible);
				AssertEquals(false, menu.cancelUpdateBondedWarehouseInwardMenuItem.Visible);
				AssertEquals(false, menu.cancelBondedWarehouseMenuItem.Visible);
				AssertEquals(false, menu.synchronizeWithBondedWarehouseMenuItem.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.UpdateBondedWarehouseMenuItem_Click(null, null);
				AssertEquals(EDIMenu.SaveDeclarationFirstMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(declaration.Logs.MostRecentLogByEventTime(EventConstants.Events.DataExport));

				Factory.Save();
				menu.UpdateBondedWarehouseMenuItem_Click(null, null);
				AssertEquals("Stock Levels have been updated. (WHS Receipt:W00000001)", UnitTestUserNotification.Instance.LastMessage.Text);
				var exportLogs = declaration.GetDataExportLogsInPostedOrder();
				AssertEquals(2, exportLogs.Length);
				var exportLog1 = exportLogs[0];
				var exportLog2 = exportLogs[1];
				exportLog1.AssertLogHasXMLMessage(declaration.JE_DeclarationReference, EDIMessageSubTypeList.Codes.XmlUniversalShipment, RecipientRoleType.BWI);
				exportLog2.AssertLogHasXMLMessage(declaration.JE_DeclarationReference, EDIMessageSubTypeList.Codes.XmlUniversalEvent, RecipientRoleType.BWI);
				menu.BondedWarehouseMenuItem_Popup(null, null);
				menu.RefreshMenu();
				AssertEquals(true, menu.bondedWarehouseMenuItem.Visible);
				AssertEquals(true, menu.cancelUpdateBondedWarehouseInwardMenuItem.Visible);
				AssertEquals(false, menu.cancelBondedWarehouseMenuItem.Visible);
				AssertEquals(false, menu.synchronizeWithBondedWarehouseMenuItem.Visible);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.CancelUpdateBondedWarehouseInwardMenuItem_Click(null, null);
				AssertEquals("Stock Levels have been canceled. (WHS Receipt:W00000001)", UnitTestUserNotification.Instance.LastMessage.Text);
				exportLogs = declaration.GetDataExportLogsInPostedOrder();
				AssertEquals(3, exportLogs.Length);
				AssertEquals(exportLog1, exportLogs[0]);
				AssertEquals(exportLog2, exportLogs[1]);
				var exportLog3 = exportLogs[2];
				exportLog3.AssertLogHasXMLMessage(declaration.JE_DeclarationReference, EDIMessageSubTypeList.Codes.XmlUniversalEvent, RecipientRoleType.BWI, string.Format("<EventType>{0}</EventType>", EventConstants.Events.CancelTheWarehouseJobCode));

				invoiceLine.JI_InvoiceQuantity = 10;
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.UpdateBondedWarehouseMenuItem_Click(null, null);
				AssertEquals("Stock Levels have been updated. (WHS Receipt:W00000003)", UnitTestUserNotification.Instance.LastMessage.Text);
				exportLogs = declaration.GetDataExportLogsInPostedOrder();
				AssertEquals(5, exportLogs.Length);
				AssertEquals(exportLog1, exportLogs[0]);
				AssertEquals(exportLog2, exportLogs[1]);
				AssertEquals(exportLog3, exportLogs[2]);
				var exportLog4 = exportLogs[3];
				var exportLog5 = exportLogs[4];
				exportLog4.AssertLogHasXMLMessage(declaration.JE_DeclarationReference, EDIMessageSubTypeList.Codes.XmlUniversalShipment, RecipientRoleType.BWI);
				exportLog5.AssertLogHasXMLMessage(declaration.JE_DeclarationReference, EDIMessageSubTypeList.Codes.XmlUniversalEvent, RecipientRoleType.BWI);

				// Outward Testing
				declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				declaration.JE_OH_Importer = Buyer.PK;
				declaration.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				declaration.SetSupportsBondedWarehousingForTesting(true);
				menu.Declaration = declaration;
				menu.BondedWarehouseMenuItem_Popup(null, null);
				menu.RefreshMenu();
				AssertEquals(true, menu.bondedWarehouseMenuItem.Visible);
				AssertEquals(false, menu.cancelUpdateBondedWarehouseInwardMenuItem.Visible);
				AssertEquals(false, menu.cancelBondedWarehouseMenuItem.Visible);
				AssertEquals(true, menu.synchronizeWithBondedWarehouseMenuItem.Visible);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.SynchronizeWithBondedWarehouseMenuItem_Click(null, null);
				AssertEquals("At least one invoice line is required in order for Synchronization to work.", UnitTestUserNotification.Instance.LastMessage.Text);

				menu.SynchronizeWithOrdersMenuItem_ClickInternal(null, null);
				AssertEquals("System will replace existing invoice data with the data from Orders. Do you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertNull(declaration.Logs.MostRecentLogByEventTime(EventConstants.Events.DataExport));

				invoiceLine = declaration.FilteredInvoiceLines.AddNew();
				invoiceLine.JI_PartNo = Part.OP_PartNum;
				invoiceLine.JI_InvoiceQuantity = 0;
				invoiceLine.JI_InvoiceUQ = "NO";
				invoiceLine.JI_CustomsUnitQty = "PK";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.SynchronizeWithBondedWarehouseMenuItem_Click(null, null);
				AssertEquals("At least one Invoice Line with valid Previous Entry Details or Part and Invoice Quantity is required.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(declaration.Logs.MostRecentLogByEventTime(EventConstants.Events.DataExport));

				invoiceLine.JI_InvoiceQuantity = 5;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.SynchronizeWithBondedWarehouseMenuItem_Click(null, null);
				AssertNull(EDIMenu.SaveDeclarationFirstMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(declaration.Logs.MostRecentLogByEventTime(EventConstants.Events.DataExport));
				AssertEquals(1, declaration.FilteredInvoiceLines.Count);
				invoiceLine = declaration.FilteredInvoiceLines[0];
				AssertEquals(5m, invoiceLine.JI_InvoiceQuantity);
				AssertEquals("NO", invoiceLine.JI_InvoiceUQ);
				AssertEquals(6m, invoiceLine.JI_CustomsQuantity);
				AssertEquals("PK", invoiceLine.JI_CustomsUnitQty);
				AssertEquals(750m, invoiceLine.JI_LinePrice);
				invoiceLine.JI_InvoiceQuantity = 5m;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.UpdateBondedWarehouseMenuItem_Click(null, null);
				exportLogs = declaration.GetDataExportLogsInPostedOrder();
				AssertEquals(2, exportLogs.Length);
				exportLog1 = exportLogs[0];
				exportLog1.AssertLogHasXMLMessage(declaration.JE_DeclarationReference, EDIMessageSubTypeList.Codes.XmlUniversalShipment, RecipientRoleType.BWR);
				exportLog2 = exportLogs[1];
				exportLog2.AssertLogHasXMLMessage(declaration.JE_DeclarationReference, EDIMessageSubTypeList.Codes.XmlUniversalEvent, RecipientRoleType.BWR, string.Format("<EventType>{0}</EventType>", EventConstants.Events.WarehouseJobCanNowBeFinalisedCode));
				AssertNotNull(declaration.Logs.MostRecentLogByEventTime(EventConstants.Events.WarehouseJobCanNowBeFinalised));
				AssertEquals("Stock Release has been updated. (WHS Order:W00000004)", UnitTestUserNotification.Instance.LastMessage.Text);
				menu.BondedWarehouseMenuItem_Popup(null, null);
				menu.RefreshMenu();
				AssertEquals(true, menu.bondedWarehouseMenuItem.Visible);
				AssertEquals(false, menu.cancelUpdateBondedWarehouseInwardMenuItem.Visible);
				AssertEquals(true, menu.cancelBondedWarehouseMenuItem.Visible);
				AssertEquals(true, menu.synchronizeWithBondedWarehouseMenuItem.Visible);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Yes to cancel
				var cancelBondedWarehouseMenuItem = menu.MenuItems.FindByName("cancelBondedWarehouseMenuItem", true);
				cancelBondedWarehouseMenuItem.PerformClick();
				exportLogs = declaration.GetDataExportLogsInPostedOrder();
				AssertEquals(3, exportLogs.Length);
				AssertEquals(exportLog1, exportLogs[0]);
				AssertEquals(exportLog2, exportLogs[1]);
				exportLog3 = exportLogs[2];
				exportLog3.AssertLogHasXMLMessage(declaration.JE_DeclarationReference, EDIMessageSubTypeList.Codes.XmlUniversalEvent, RecipientRoleType.BWR, string.Format("<EventType>{0}</EventType>", EventConstants.Events.CancelTheWarehouseJobCode));
				AssertNotNull(declaration.Logs.MostRecentLogByEventTime(EventConstants.Events.CancelTheWarehouseJob));
				AssertEquals("Stock Release has been canceled. (WHS Order:W00000004)", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCannotUpdateBondedWarehouseIfHasChangesNonWEA()
		{
			SetUpMoq();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			declaration.HasChanges = true;
			menu.UpdateBondedWarehouseMenuItem_Click(null, null);
			AssertEquals(EDIMenu.SaveDeclarationFirstMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			declaration.HasChanges = false;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			menu.UpdateBondedWarehouseMenuItem_Click(null, null);
			AssertNotEquals("No force save message", EDIMenu.SaveDeclarationFirstMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestFinaliseWithBondedWarehouse()
		{
			SetUpMoq();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.HasChanges = false;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var mock = Factory.NewMoq<CusEntryHeader>();
			var entryHeader = mock.Object;
			entryHeader.SetDeclarationForTesting(declaration);
			declaration.CustomsEntryHeaders.Add(entryHeader);
			declarationMock.Verify(m => m.NotifyBondedWarehouseThatExWarehouseEntryHasCleared(), Times.Never);

			menu.FinaliseStockWithBondedWarehouseMenuItem_Click(null, null);
			AssertEquals(EDIMenu.SaveDeclarationFirstMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			Factory.Save();
			mock.Setup(m => m.HaveAmendmentsBeenMadeAndNotYetClearedByCustoms).Returns(true);
			menu.FinaliseStockWithBondedWarehouseMenuItem_Click(null, null);
			AssertEquals("Sorry, you cannot finalize this declaration in the Inventory Management as it has amendments outstanding which have not yet been cleared by Customs.", UnitTestUserNotification.Instance.LastMessage.Text);

			mock.Setup(m => m.HaveAmendmentsBeenMadeAndNotYetClearedByCustoms).Returns(false);
			declarationMock.Setup(m => m.NotifyBondedWarehouseThatExWarehouseEntryHasCleared());
			menu.FinaliseStockWithBondedWarehouseMenuItem_Click(null, null);
			AssertEquals("The stock has been successfully finalized in the Inventory Management system.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCannotUpdateBondedWarehouseIfAmendmentsAreOutstandingNonWEA()
		{
			setupWHSUniversalXMLForTesting = BaseJobDeclaration.SetupWHSUniversalXMLForTesting();
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			dummy = DummyBusinessObject.New(Factory);
			formMock = new Mock<ZForm>(dummy);
			formMock.CallBase = true;
			form = formMock.Object;
			menu = new EDIMenu();
			form.Menu.MenuItems.Add(menu);
			declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.WarehouseDocAddress.E2_OA_Address = WarehouseAddress.MainAddress.PK;
			invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.SetSupportsBondedWarehousingForTesting(true);
			menu.Declaration = declaration;
			using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(declaration))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			}
			Factory.Save();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.SetHaveAmendmentsBeenMadeAndNotYetClearedByCustomsForTesting(true);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // OK to save
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			menu.UpdateBondedWarehouseMenuItem_Click(null, null);
			AssertEquals("Updating Inventory Management stock levels when there is a change which requires Customs amendment messaging will cause the data to be out of sync.\r\nDo you still wish to continue?", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // OK to save
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			menu.UpdateBondedWarehouseMenuItem_Click(null, null);
			Assert("No amendment message", "Updating Inventory Management stock levels when there is a change which requires Customs amendment messaging will cause the data to be out of sync.\r\nDo you still wish to continue?" != UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCancelUpdateBondedWarehouseInwardMenuItemVisible()
		{
			SetUpMoq();
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "ENT2423";
			declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
			var line = declaration.FilteredInvoiceLines.AddNew();
			line.SetIsGoingIntoBondedWarehouseCoreForTesting(true);

			menu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(false, menu.cancelUpdateBondedWarehouseInwardMenuItem.Visible);

			declaration.JE_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(2);
			menu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(true, menu.cancelUpdateBondedWarehouseInwardMenuItem.Visible);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			menu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(false, menu.cancelUpdateBondedWarehouseInwardMenuItem.Visible);

			declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			menu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(true, menu.cancelUpdateBondedWarehouseInwardMenuItem.Visible);

			declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCanceled;
			menu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(false, menu.cancelUpdateBondedWarehouseInwardMenuItem.Visible);
		}

		public void TestUpdateBondedWarehouseMenuItemVisible()
		{
			SetUpMoq();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			menu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(false, menu.updateBondedWarehouseMenuItem.Visible);

			declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			menu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(true, menu.updateBondedWarehouseMenuItem.Visible);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			menu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(false, menu.updateBondedWarehouseMenuItem.Visible);

			declaration.CustomsEntryHeaders.AddNew();
			BaseJobComInvoiceLine line = declaration.FilteredInvoiceLines.AddNew();
			line.SetDeclarationForTesting(declaration);
			line.SetIsGoingIntoBondedWarehouseCoreForTesting(false);
			menu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(false, menu.updateBondedWarehouseMenuItem.Visible);

			declaration.CustomsEntryHeaders[0].EntryNumber = "EntryNum";
			line.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			menu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(true, menu.updateBondedWarehouseMenuItem.Visible);

			declaration.SetSupportsBondedWarehousingForTesting(false);
			menu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(false, menu.updateBondedWarehouseMenuItem.Visible);

			declaration.JE_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(2);
			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			declaration.FilteredInvoiceLines.RemoveAndDeleteAll();
			declaration.SetSupportsBondedWarehousingForTesting(true);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			menu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(true, menu.updateBondedWarehouseMenuItem.Visible);

			declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			menu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(true, menu.updateBondedWarehouseMenuItem.Visible);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			menu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(false, menu.updateBondedWarehouseMenuItem.Visible);

			declaration.CustomsEntryHeaders.AddNew();
			line = declaration.FilteredInvoiceLines.AddNew();
			line.SetDeclarationForTesting(declaration);
			line.SetIsGoingIntoBondedWarehouseCoreForTesting(false);
			menu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(false, menu.updateBondedWarehouseMenuItem.Visible);

			declaration.CustomsEntryHeaders[0].EntryNumber = "EntryNum";
			line.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			menu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(true, menu.updateBondedWarehouseMenuItem.Visible);

			declaration.SetSupportsBondedWarehousingForTesting(false);
			menu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(false, menu.updateBondedWarehouseMenuItem.Visible);
		}

		public void TestDisableBondedWarehouseIntegrationMenuItemVisible()
		{
			SetUpMoq();
			using (EDIMenu menu = new EDIMenu())
			{
				var declarationMock = Factory.NewMoq<BaseJobDeclarationWithEntryInstructions>();
				var declarationMockProtected = declarationMock.Protected();
				declarationMockProtected.Setup<bool>("SupportMultipleWarehouseEntryCore").Returns(true);
				declarationMockProtected.Setup<bool>("GetIsWHSUniversalXMLActive").Returns(true);
				var declaration = declarationMock.Object;
				var helperMock = new Mock<BondedWarehousingHelper>(declaration) { CallBase = true };
				declarationMockProtected.Setup<BondedWarehousingHelper>("GetNewBondedWarehousingHelper").Returns(helperMock.Object);
				declaration.SetSupportsBondedWarehousingForTesting(true);
				var helper = new WhsDataTestHelper(Factory);
				var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction1.CEI_Style = helper.InwardCusProcedure.ZZ6_ProcedureCode;
				entryInstruction1.CEI_Description = "DESC 1";
				entryInstruction1.CEI_OA_Warehouse2 = helper.Warehouse2.MainAddress.PK;

				helper.Warehouse2.CompanyData.OB_IMUsedBondedWhs = true;
				var entryInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_Style = helper.OutwardCusProcedure.ZZ6_ProcedureCode;
				entryInstruction2.CEI_Description = "DESC 2";
				entryInstruction2.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
				helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;

				var entry1Mock = Factory.NewMoq<CusEntryHeader>();
				var entry1 = entry1Mock.Object;
				declaration.CustomsEntryHeaders.Add(entry1);
				entry1.CH_CEI_Instruction = entryInstruction1.PK;
				entry1.CH_BGMReference = "BGM0001";
				entry1.EntryNumber = "ENT0001";
				entry1.CH_HasManualWhsUpdate = true;
				var entry1Line = entry1.MergedLines.AddNew();

				var entry2Mock = Factory.NewMoq<CusEntryHeader>();
				var entry2 = entry2Mock.Object;
				declaration.CustomsEntryHeaders.Add(entry2);
				entry2.CH_CEI_Instruction = entryInstruction2.PK;
				entry2.CH_BGMReference = "BGM0002";
				entry2.EntryNumber = "ENT0002";
				var entry2Line = entry2.MergedLines.AddNew();

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1Mock = Factory.NewMoq<BaseJobComInvoiceLine>();
				invoiceLine1Mock.Protected().Setup<bool>("SupportsBondedWarehousingCore").Returns(declaration.SupportsBondedWarehousing);

				var invoiceLine1 = invoiceLine1Mock.Object;
				invoiceLine1.JI_JZ = invoice.PK;
				declaration.InvoiceLines.Add(invoiceLine1);
				invoiceLine1.JI_CEI = entryInstruction1.PK;
				invoiceLine1.JI_CL = entry1Line.PK;
				invoiceLine1.JI_Procedure = helper.InwardCusProcedure.ZZ6_ProcedureCode + helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;

				var invoiceLine2Mock = Factory.NewMoq<BaseJobComInvoiceLine>();
				invoiceLine2Mock.Protected().Setup<bool>("SupportsBondedWarehousingCore").Returns(declaration.SupportsBondedWarehousing);
				var invoiceLine2 = invoiceLine2Mock.Object;

				invoiceLine2.JI_JZ = invoice.PK;
				declaration.InvoiceLines.Add(invoiceLine2);
				invoiceLine2.JI_CEI = entryInstruction2.PK;
				invoiceLine2.JI_CL = entry2Line.PK;
				invoiceLine2.JI_Procedure = helper.OutwardCusProcedure.ZZ6_ProcedureCode + helper.OutwardCusProcedure.ZZ6_PreviousProcedureCode;

				menu.Declaration = declaration;
				menu.bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
				CombineAssertions(() =>
				{
					var entryMenuItem1 = menu.bondedWarehouseMenuItem.MenuItems[0];
					entryMenuItem1.OnPopup(EventArgs.Empty);
					AssertEquals("Disable Integration is invisible", false, entryMenuItem1.MenuItems.FindByText("&Disable Integration").Visible);

					var entryMenuItem2 = menu.bondedWarehouseMenuItem.MenuItems[1];
					entryMenuItem2.OnPopup(EventArgs.Empty);
					AssertEquals("Disable Integration is visible", true, entryMenuItem2.MenuItems.FindByText("&Disable Integration").Visible);
				});
			}
		}

		public void TestFinaliseStockWithBondedWarehouseMenuItemVisible()
		{
			SetUpMoq();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			menu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(false, menu.finaliseStockWithBondedWarehouseMenuItem.Visible);

			declaration.CustomsEntryHeaders.AddNew();
			BaseJobComInvoiceLine line = declaration.FilteredInvoiceLines.AddNew();
			line.SetDeclarationForTesting(declaration);
			line.SetUseBondedWarehouseAutomationForTesting(false);
			menu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(false, menu.finaliseStockWithBondedWarehouseMenuItem.Visible);

			declaration.CustomsEntryHeaders[0].EntryNumber = "EntryNum";
			line.SetUseBondedWarehouseAutomationForTesting(true);
			menu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(true, menu.finaliseStockWithBondedWarehouseMenuItem.Visible);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			menu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(false, menu.finaliseStockWithBondedWarehouseMenuItem.Visible);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;

			declaration.SetSupportsBondedWarehousingForTesting(false);
			menu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(false, menu.finaliseStockWithBondedWarehouseMenuItem.Visible);

			declaration.JE_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(2);
			declaration.SetSupportsBondedWarehousingForTesting(true);
			menu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(false, menu.finaliseStockWithBondedWarehouseMenuItem.Visible);
		}

		public void TestInventoriesSelectionFromBondedWarehouseMenuItemVisible()
		{
			SetUpMoq();
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			CheckN30WarehouseMenuVisible(menu.inventoriesSelectionFromBondedWarehouseMenuItem, menu);
		}

		public void TestSynchronizeWithBondedWarehouseMenuItemVisible()
		{
			SetUpMoq();
			CheckN30WarehouseMenuVisible(menu.synchronizeWithBondedWarehouseMenuItem, menu);
		}

		public void TestCancelBondedWarehouseMenuItemVisible()
		{
			SetUpMoq();
			CheckN30WarehouseMenuVisible(menu.cancelBondedWarehouseMenuItem, menu);
		}

		[NUnit.Framework.ExpectNoExceptions()]
		public void TestBondedWarehouseMenuItemWithNullDeclaration()
		{
			SetUpMoq();
			using (EDIMenu menu = new EDIMenu())
			{
				menu.BondedWarehouseMenuItem_Popup(null, null);
			}
		}

		public void TestErrorsStopWEAProcess()
		{
			setupWHSUniversalXMLForTesting = BaseJobDeclaration.SetupWHSUniversalXMLForTesting();
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			dummy = DummyBusinessObject.New(Factory);
			formMock = new Mock<ZForm>(dummy);
			formMock.CallBase = true;
			form = formMock.Object;
			menu = new EDIMenu();
			form.Menu.MenuItems.Add(menu);
			declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.WarehouseDocAddress.E2_OA_Address = WarehouseAddress.MainAddress.PK;
			invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.SetSupportsBondedWarehousingForTesting(true);
			menu.Declaration = declaration;
			using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(declaration))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			}
			Factory.Save();

			declaration.JE_OH_Importer = Buyer.PK;
			invoiceLine.JI_PartNo = Part.OP_PartNum;
			invoiceLine.JI_InvoiceQuantity = 1m;
			declaration.DoMerge();
			Factory.Save();
			dummy.AddRowError("Error!");
			menu.UpdateBondedWarehouseMenuItem_Click(null, null);
			AssertEquals("Please fix all errors and message errors on the form before updating the Inventory Management.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestMessageErrorsStopWEAProcess()
		{
			setupWHSUniversalXMLForTesting = BaseJobDeclaration.SetupWHSUniversalXMLForTesting();
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			dummy = DummyBusinessObject.New(Factory);
			formMock = new Mock<ZForm>(dummy);
			formMock.CallBase = true;
			form = formMock.Object;
			menu = new EDIMenu();
			form.Menu.MenuItems.Add(menu);
			declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.WarehouseDocAddress.E2_OA_Address = WarehouseAddress.MainAddress.PK;
			invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.SetSupportsBondedWarehousingForTesting(true);
			menu.Declaration = declaration;
			using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(declaration))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			}
			Factory.Save();

			dummy.AddRowMessageError("Error error!");
			menu.UpdateBondedWarehouseMenuItem_Click(null, null);
			AssertEquals("Please fix all errors and message errors on the form before updating the Inventory Management.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestMergeDuringWEA()
		{
			SetUpMoq();
			invoiceLine.JI_PartNo = Part.OP_PartNum;
			AssertEquals("No entry headers", 0, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Doesn't require merge", false, declaration.MergeManager.RequiresMerge);
			declarationMock.Protected().Setup<bool>("DoMergeCore", declaration.MessageInitiator).Returns(false);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // OK to save
			menu.UpdateBondedWarehouseMenuItem_Click(null, null);
			declarationMock.VerifyAll();
			declarationMock.Reset();

			declaration.JE_OwnerRef = "ASDF";
			declarationMock.Protected().Verify("DoMergeCore", Times.Never(), declaration.MessageInitiator);
			AssertEquals("Doesn't require merge", false, declaration.MergeManager.RequiresMerge);
			menu.UpdateBondedWarehouseMenuItem_Click(null, null);
			AssertEquals(EDIMenu.SaveDeclarationFirstMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			declarationMock.VerifyAll();

			Factory.Save();
			AssertEquals("Requires merge", 0, declaration.CustomsEntryHeaders.Count);
			declarationMock.Reset();
			formMock.Setup(m => m.FireSaveButton(null)).Returns(ContinueWithSave.Yes);
			declarationMock.Setup(m => m.CreateOrUpdateBondedWarehouseInward());
			menu.UpdateBondedWarehouseMenuItem_Click(null, null);
			declarationMock.VerifyAll();
			AssertEquals("Merged Done", 1, declaration.CustomsEntryHeaders.Count);
		}

		public void TestSuccessMessageAndSave()
		{
			setupWHSUniversalXMLForTesting = BaseJobDeclaration.SetupWHSUniversalXMLForTesting();
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			dummy = DummyBusinessObject.New(Factory);
			var formMock = new Mock<ZForm>(dummy) { CallBase = true };
			form = formMock.Object;
			menu = new EDIMenu();
			form.Menu.MenuItems.Add(menu);
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declaration = declarationMock.Object;
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.WarehouseDocAddress.E2_OA_Address = WarehouseAddress.MainAddress.PK;
			invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.SetSupportsBondedWarehousingForTesting(true);
			menu.Declaration = declaration;
			using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(declaration))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			}
			Factory.Save();

			declaration.JE_OH_Importer = Buyer.PK;

			invoiceLine.JI_PartNo = Part.OP_PartNum;
			invoiceLine.JI_InvoiceQuantity = 1m;
			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var declarationMockProtected = declarationMock.Protected();
			declarationMockProtected.Setup<bool>("DoMergeCore", ItExpr.IsAny<ISendsMessagesToCustoms>()).Returns(true);
			declarationMock.Setup(x => x.CreateOrUpdateBondedWarehouseInward());
			formMock.Setup(m => m.FireSaveButton(It.IsAny<object>())).Returns(ContinueWithSave.Yes);
			menu.UpdateBondedWarehouseMenuItem_Click(null, null);
			formMock.Verify(m => m.FireSaveButton(It.IsAny<object>()), Times.AtLeastOnce);
			AssertEquals("Inventory Management stock levels have been updated.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			formMock.Setup(m => m.FireSaveButton(It.IsAny<object>())).Returns(ContinueWithSave.No);
			declarationMockProtected.Setup<bool>("DoMergeCore", ItExpr.IsAny<ISendsMessagesToCustoms>()).Returns(true);
			declarationMock.Setup(x => x.CreateOrUpdateBondedWarehouseInward());
			menu.UpdateBondedWarehouseMenuItem_Click(null, null);
			formMock.Verify(m => m.FireSaveButton(It.IsAny<object>()), Times.AtLeastOnce);
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCannotUpdateStockExceptionIsTurnedIntoMessageToUser()
		{
			setupWHSUniversalXMLForTesting = BaseJobDeclaration.SetupWHSUniversalXMLForTesting();
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			dummy = DummyBusinessObject.New(Factory);
			formMock = new Mock<ZForm>(dummy) { CallBase = true };
			form = formMock.Object;
			menu = new EDIMenu();
			form.Menu.MenuItems.Add(menu);
			declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.WarehouseDocAddress.E2_OA_Address = WarehouseAddress.MainAddress.PK;
			invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.SetSupportsBondedWarehousingForTesting(true);
			menu.Declaration = declaration;
			using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(declaration))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			}
			Factory.Save();
			var declarationMockDNM = Factory.NewMoq<BaseJobDeclaration>();
			var declarationDNM = declarationMockDNM.Object;
			declarationDNM.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declarationDNM.WarehouseDocAddress.E2_OA_Address = WarehouseAddress.MainAddress.PK;
			invoiceLine = declarationDNM.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declarationDNM.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declarationDNM.SetSupportsBondedWarehousingForTesting(true);
			menu.Declaration = declarationDNM;
			using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(declarationDNM))
			{
				declarationDNM.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			}
			Factory.Save();

			invoiceLine.JI_PartNo = Part.OP_PartNum;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var declarationMockDNMProtected = declarationMockDNM.Protected();
			declarationMockDNMProtected.Setup<bool>("DoMergeCore", ItExpr.IsAny<ISendsMessagesToCustoms>()).Returns(true);
			declarationMockDNM.Setup(x => x.CreateOrUpdateBondedWarehouseInward()).Throws(() => new CannotUpdateStockException("DetailedMessage"));
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // OK to save
			menu.UpdateBondedWarehouseMenuItem_Click(null, null);
			AssertEquals("DetailedMessage", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestMissingDataException()
		{
			SetUpMoq();
			CusEntryHeader header = declaration.CustomsEntryHeaders.AddNew();
			header.SetDeclarationForTesting(declaration);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CusEntryLine entryLine = header.MergedLines.AddNew();
			BaseJobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.SetDeclarationForTesting(declaration);
			invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			invoiceLine.SetPartForTesting(Factory.New<Business.OrgSupplierPart>());
			invoiceLine.JI_InvoiceQuantity = 10;
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.HasChanges = false;
			declaration.HasChanges = false;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			declarationMock.Protected().Setup<bool>("DoMergeCore", ItExpr.IsAny<ISendsMessagesToCustoms>()).Returns(true);
			menu.UpdateBondedWarehouseMenuItem_Click(null, null);
			AssertContains("Has correct error message", "Sorry, could not update Inventory Management as the following information is missing", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		Mock<BaseJobDeclaration> declarationMock;
		EDIMenu menu;
		BaseJobDeclaration declaration;
		BaseJobComInvoiceLine invoiceLine;
		ZForm form;
		Mock<ZForm> formMock;
		DummyBusinessObject dummy;
		IDisposable setupWHSUniversalXMLForTesting;

		void SetUpMoq()
		{
			setupWHSUniversalXMLForTesting = BaseJobDeclaration.SetupWHSUniversalXMLForTesting();
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			dummy = DummyBusinessObject.New(Factory);
			formMock = new Mock<ZForm>(dummy);
			formMock.CallBase = true;
			form = formMock.Object;
			menu = new EDIMenu();
			form.Menu.MenuItems.Add(menu);
			declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declaration = declarationMock.Object;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.WarehouseDocAddress.E2_OA_Address = WarehouseAddress.MainAddress.PK;
			invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.SetSupportsBondedWarehousingForTesting(true);
			menu.Declaration = declaration;
			using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(declaration))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			}
			Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();
			form?.Dispose();
			setupWHSUniversalXMLForTesting?.Dispose();
		}

		OrgHeader Buyer => buyer ?? (buyer = CreateImporter(Factory));
		OrgHeader buyer;

		OrgHeader WarehouseAddress => warehouseAddress ?? (warehouseAddress = CreateWarehouse(Factory));
		OrgHeader warehouseAddress;

		GlbDepartment Department => department ?? (department = CreateDepartment(Factory));
		GlbDepartment department;

		Business.OrgSupplierPart Part => part ?? (part = CreatePart(Factory, Buyer.PK));
		Business.OrgSupplierPart part;

		OrgHeader CreateWarehouse(BusinessObjectFactory factory)
		{
			var warehouseOrg = factory.New<OrgHeader>();
			warehouseOrg.OH_Code = "W1!";
			warehouseOrg.MainAddress.OA_Address1 = "ADDRESS 1";
			warehouseOrg.MainAddress.LocalControlledPremisesID = "23423";
			warehouseOrg.OH_RL_NKClosestPort = factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).RL_Code;

			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse(warehouseOrg.MainAddress.OA_Address1, "WHS", "BOND");
			warehouse.WW_OA_WarehouseAddress = warehouseOrg.MainAddress.PK;
			warehouse.WW_IsBondedWarehouse = true;
			warehouse.WW_IsVirtualWarehouse = true;
			((IWhsArea)warehouse.Areas[0]).WA_AreaType = "BON";
			warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
			warehouse.WW_AutoPrintPackingSlip = false;
			return warehouseOrg;
		}

		OrgHeader CreateImporter(BusinessObjectFactory factory)
		{
			var importer = factory.New<OrgHeader>();
			importer.OH_Code = "IMP234";
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			importer.MainAddress.OA_Address1 = "ADDRESS 1";
			importer.OH_IsWarehouseClient = true;
			return importer;
		}

		GlbDepartment CreateDepartment(BusinessObjectFactory factory)
		{
			var department = factory.New<GlbDepartment>();
			department.GE_Code = "234";
			department.GE_Warehouse = true;
			department.GE_CustomsBrokerage = true;
			return department;
		}

		Business.OrgSupplierPart CreatePart(BusinessObjectFactory factory, ZGuid importerPK)
		{
			var part = factory.New<Business.OrgSupplierPart>();
			part.OP_PartNum = "~~~";
			part.OP_StockKeepingUnit = "NO";
			var relation = part.RelatedOrganisations.AddOrganisationIfNotExist(importerPK, OrgPartRelation.RelationshipTypes.Owner);
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_LookupCode = "TestLookup";
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			classification.CC_TariffNum = "2605.00.00 08";
			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_CC = classification.PK;
			pivot.CI_OP = part.PK;
			return part;
		}

		void CheckN30WarehouseMenuVisible(MenuItem menu, EDIMenu mainMenu)
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			mainMenu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(false, menu.Visible);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			mainMenu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(false, menu.Visible);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			mainMenu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(true, menu.Visible);

			declaration.SetSupportsBondedWarehousingForTesting(false);
			mainMenu.BondedWarehouseMenuItem_Popup(null, null);
			AssertEquals(false, menu.Visible);
		}
	}
}
