using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using EntryTypeList = Enterprise.Customs.US.Business.EntryTypeList;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

namespace Enterprise.Customs.US.InBond.GUI.Testing
{
	[TestedType(typeof(MessagingForm))]
	sealed class MessagingFormTest : ZFormBasherTest
	{
		public void TestMessageOptionsTabPageTabVisible()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "MW323423";
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			Factory.Save();
			var messageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			var sendingObject = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DiversionRequest, messageInitiator);
			using (var form = new MessagingForm(sendingObject))
			{
				var messageOptionsTabPages = form.Controls.Find("MessageOptionsTabPage", true);
				Assert(messageOptionsTabPages.Length > 0);
				var messageOptionsTabPage = messageOptionsTabPages[0] as ZTabPage;
				Assert(messageOptionsTabPage.TabVisible);
			}

			sendingObject.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
		}

		public void TestSendButtonClick()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "MW323423";
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			Factory.Save();
			var messageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			var sendingObject = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureAdd, messageInitiator);
			messageInitiator.ThrowExceptionOnInvalidOperation = false;
			messageInitiator.AnswerToContinueWithAction = false;
			sendingObject.Header.MessageInitiator = messageInitiator;
			moveHeader = sendingObject.Factory.Load<CusInBondMoveHeader>(moveHeader.PK);
			moveDetail = sendingObject.Factory.Load<CusInBondMoveDetail>(moveDetail.PK);
			moveDetail.B9_B0 = ZGuid.Empty;
			var sendingObj = sendingObject.SendingObjects[0];
			sendingObj.US_ShouldSend = true;
			using (var form = new MessagingForm(sendingObject))
			{
				form.Show();
				var sendButton = (ZButton)form.Controls["ButtonPanel"].Controls["SendButton"];
				sendButton.PerformClick();
				AssertContains("Please fix these errors before sending any messages:\r\n", messageInitiator.InvalidOperationText);
				AssertEquals(0, moveHeader.Messages.Count);
				AssertEquals(false, messageInitiator.SuccessfulSendOccured);
				AssertEquals(0, messageInitiator.PastYesNoQuestionsAsked.Count);
				moveDetail.B9_B0 = bill.PK;
				messageInitiator.InvalidOperationText = null;
				sendButton.PerformClick();
				AssertNull(messageInitiator.InvalidOperationText);
				AssertContains("It is likely that your message(s) will be rejected by Customs, as they have the following message errors:", messageInitiator.PastYesNoQuestionsAsked[0]);
				AssertEquals(0, moveHeader.Messages.Count);
				AssertEquals(false, messageInitiator.SuccessfulSendOccured);
				messageInitiator.PastYesNoQuestionsAsked.Clear();
				messageInitiator.AnswerToContinueWithAction = true;
				sendButton.PerformClick();
				AssertNull(messageInitiator.InvalidOperationText);
				AssertContains("It is likely that your message(s) will be rejected by Customs, as they have the following message errors:", messageInitiator.PastYesNoQuestionsAsked[0]);
				AssertEquals(true, messageInitiator.SuccessfulSendOccured);
				AssertEquals("1 message was created.", messageInitiator.SuccessfulSendText);
				AssertEquals(1, moveHeader.Messages.Count);
				var message = moveHeader.Messages[0];
				AssertEquals(true, message.EM_SendWithMessageErrors);
			}

			sendingObject.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
		}

		public void TestFormDisplayForWarehouse()
		{
			var data = (ZArchitecture.Environment.RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (ZArchitecture.Environment.BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
			addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			USCustomsDataRegistry.Instance.AnInBondCommodityMustHaveAValidProduct.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var helper = new WhsDataTestHelper(Factory);
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "W#@");
			var importer = helper.Importer;
			var messageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer()
			{
				ThrowExceptionOnInvalidOperation = false
			};
			var header = Factory.New<CusInBondHeader>();
			header.MessageInitiator = messageInitiator;
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FTZMove = true;
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_WarehouseTransactionStatus = "OCA";
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "NC";
			Factory.Save();
			var sendingHeader = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.BondedWarehouseUpdate, messageInitiator);
			var sendingObj = sendingHeader.SendingObjects[0];
			sendingObj.US_ShouldSend = true;
			using (var form = new MessagingForm(sendingHeader))
			{
				form.Show();
				AssertEquals("Text", "Update Inventory", form.Text);
				var topSplitContainer = (CargoWise.Windows.UI.KSplitContainer)form.Controls["TopSplitContainer"];
				AssertEquals("topSplitContainer.Panel2Collapsed", true, topSplitContainer.Panel2Collapsed);
				var movementHeadersMessagesGrid = (ZGrid)topSplitContainer.Panel1.Controls["MovementHeadersMessagesGroupBox"].Controls["MovementHeadersMessagesGrid"];
				var columnStyles = new List<object>(movementHeadersMessagesGrid.ColumnStyles.ToArray());
				foreach (var columnName in new[] {
					InBondMessageSendingObject.Schema.US_ShouldSend,
					InBondMessageSendingObject.Schema.US_WarehouseAddressDetail,
					InBondMessageSendingObject.Schema.US_InBondNumber,
					InBondMessageSendingObject.Schema.US_InBondEntryType
				})
				{
					var columnStyle = movementHeadersMessagesGrid.GetColumnStyle(columnName);
					AssertEquals(columnName + " should be available", false, columnStyle.IsUnavailable);
					AssertEquals(columnName + " should be visible", true, columnStyle.IsVisible);
					columnStyles.Remove(columnStyle);
				}

				foreach (var columnName in new[] {
					InBondMessageSendingObject.Schema.US_InBondCarrierID,
					InBondMessageSendingObject.Schema.US_InBondCarrierSCAC,
					InBondMessageSendingObject.Schema.US_DestinationPortDCode,
					InBondMessageSendingObject.Schema.US_ForeignDestPortKCode,
					InBondMessageSendingObject.Schema.US_MonetaryValue,
					InBondMessageSendingObject.Schema.US_BTAIndicator
				})
				{
					var columnStyle = movementHeadersMessagesGrid.GetColumnStyle(columnName);
					AssertEquals(columnName + " should be available", false, columnStyle.IsUnavailable);
					AssertEquals(columnName + " should not be visible", false, columnStyle.IsVisible);
					columnStyles.Remove(columnStyle);
				}

				foreach (Core.Forms.ZGridColumnInfo columnStyle in columnStyles)
				{
					AssertEquals(columnStyle.ColumnName + " should not be available", true, columnStyle.IsUnavailable);
				}
			}

			sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
			sendingHeader = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.BondedWarehouseCancel, messageInitiator);
			sendingObj = sendingHeader.SendingObjects[0];
			sendingObj.US_ShouldSend = true;
			using (var form = new MessagingForm(sendingHeader))
			{
				form.Show();
				AssertEquals("Text", "Cancel Inventory Stock Release", form.Text);
				var topSplitContainer = (CargoWise.Windows.UI.KSplitContainer)form.Controls["TopSplitContainer"];
				AssertEquals("topSplitContainer.Panel2Collapsed", true, topSplitContainer.Panel2Collapsed);
				var movementHeadersMessagesGrid = (ZGrid)topSplitContainer.Panel1.Controls["MovementHeadersMessagesGroupBox"].Controls["MovementHeadersMessagesGrid"];
				var columnStyles = new List<object>(movementHeadersMessagesGrid.ColumnStyles.ToArray());
				foreach (var columnName in new[] {
					InBondMessageSendingObject.Schema.US_ShouldSend,
					InBondMessageSendingObject.Schema.US_WarehouseAddressDetail,
					InBondMessageSendingObject.Schema.US_InBondNumber,
					InBondMessageSendingObject.Schema.US_InBondEntryType
				})
				{
					var columnStyle = movementHeadersMessagesGrid.GetColumnStyle(columnName);
					AssertEquals(columnName + " should be available", false, columnStyle.IsUnavailable);
					AssertEquals(columnName + " should be visible", true, columnStyle.IsVisible);
					columnStyles.Remove(columnStyle);
				}

				foreach (var columnName in new[] {
					InBondMessageSendingObject.Schema.US_InBondCarrierID,
					InBondMessageSendingObject.Schema.US_InBondCarrierSCAC,
					InBondMessageSendingObject.Schema.US_DestinationPortDCode,
					InBondMessageSendingObject.Schema.US_ForeignDestPortKCode,
					InBondMessageSendingObject.Schema.US_MonetaryValue,
					InBondMessageSendingObject.Schema.US_BTAIndicator
				})
				{
					var columnStyle = movementHeadersMessagesGrid.GetColumnStyle(columnName);
					AssertEquals(columnName + " should be available", false, columnStyle.IsUnavailable);
					AssertEquals(columnName + " should not be visible", false, columnStyle.IsVisible);
					columnStyles.Remove(columnStyle);
				}

				foreach (Core.Forms.ZGridColumnInfo columnStyle in columnStyles)
				{
					AssertEquals(columnStyle.ColumnName + " should not be available", true, columnStyle.IsUnavailable);
				}
			}

			sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
			sendingHeader = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureAdd, messageInitiator);
			sendingObj = sendingHeader.SendingObjects[0];
			sendingObj.US_ShouldSend = true;
			using (var form = new MessagingForm(sendingHeader))
			{
				form.Show();
				AssertEquals("Text", "In-Bond Departure Add Messaging", form.Text);
				var topSplitContainer = (CargoWise.Windows.UI.KSplitContainer)form.Controls["TopSplitContainer"];
				AssertEquals("topSplitContainer.Panel2Collapsed", false, topSplitContainer.Panel2Collapsed);
				var movementHeadersMessagesGrid = (ZGrid)topSplitContainer.Panel1.Controls["MovementHeadersMessagesGroupBox"].Controls["MovementHeadersMessagesGrid"];
				var columnStyles = new List<object>(movementHeadersMessagesGrid.ColumnStyles.ToArray());
				foreach (var columnName in new[] { InBondMessageSendingObject.Schema.US_ShouldSend, InBondMessageSendingObject.Schema.US_InBondNumber, InBondMessageSendingObject.Schema.US_InBondEntryType })
				{
					var columnStyle = movementHeadersMessagesGrid.GetColumnStyle(columnName);
					AssertEquals(columnName + " should be available", false, columnStyle.IsUnavailable);
					AssertEquals(columnName + " should be visible", true, columnStyle.IsVisible);
					columnStyles.Remove(columnStyle);
				}

				foreach (var columnName in new[] { InBondMessageSendingObject.Schema.US_InBondCarrierID, InBondMessageSendingObject.Schema.US_InBondCarrierSCAC, InBondMessageSendingObject.Schema.US_DestinationPortDCode, InBondMessageSendingObject.Schema.US_ForeignDestPortKCode, InBondMessageSendingObject.Schema.US_MonetaryValue, InBondMessageSendingObject.Schema.US_BTAIndicator })
				{
					var columnStyle = movementHeadersMessagesGrid.GetColumnStyle(columnName);
					AssertEquals(columnName + " should be available", false, columnStyle.IsUnavailable);
					AssertEquals(columnName + " should not be visible", false, columnStyle.IsVisible);
					columnStyles.Remove(columnStyle);
				}

				foreach (Core.Forms.ZGridColumnInfo columnStyle in columnStyles)
				{
					AssertEquals(columnStyle.ColumnName + " should not be available", true, columnStyle.IsUnavailable);
				}
			}

			sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
		}

		public void TestSendDepartureAdd_WithdrawFromWarehouse()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var data = (ZArchitecture.Environment.RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
				var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (ZArchitecture.Environment.BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
				addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var importer = helper.Importer;
				var inwardDeclaration = helper.GetNewDeclaration(EntryTypeList.Codes.Warehouse, "BINW000001", "XJ5", "40000007", 100m);
				Factory.Save();
				var result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsReceive = (Warehouse.Integration.IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				var whsHelper = ObjectFactory.New<Warehouse.Integration.IWhsTransactionTestHelper>(Factory);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 100m);
				var header = Factory.New<CusInBondHeader>();
				header.BH_OA_Importer = importer.MainAddress.PK;
				header.BH_FTZMove = true;
				var moveHeader = header.MovementHeaders.AddNew();
				moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
				moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
				var bill = header.Bills.AddNew();
				bill.B0_MasterBillNumber = "XJ5-40000007";
				var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
				var container = moveDetail.Containers.AddNew();
				container.BC_ContainerNum = "NC";
				var commodity = container.Commodities.AddNew();
				commodity.BY_PartNumber = helper.Part.OP_PartNum;
				commodity.BY_WarehouseEntryNumber = "XJ5-40000007";
				commodity.BY_WarehouseEntryLineNo = 1;
				commodity.BY_InvoiceQuantity = 60m;
				Factory.Save();
				var messageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
				var sendingHeader = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureAdd, messageInitiator);
				var sendingObj = sendingHeader.SendingObjects[0];
				sendingObj.US_ShouldSend = true;
				using (var form = new MessagingForm(sendingHeader))
				{
					form.Show();
					var sendButton = (ZButton)form.Controls["ButtonPanel"].Controls["SendButton"];
					messageInitiator.AnswerToContinueWithAction = true;
					sendButton.PerformClick();
					AssertEquals("1 message was created.", messageInitiator.SuccessfulSendText);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
					AssertEquals(ImportMessageStatusList.Codes.AwaitingDepartureOriginal, moveHeader.BM_CustomsStatus);
				}

				sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
			}
		}

		public void TestSendDepartureAmend_WithdrawFromWarehouse()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var data = (ZArchitecture.Environment.RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
				var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (ZArchitecture.Environment.BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
				addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var importer = helper.Importer;
				var inwardDeclaration = helper.GetNewDeclaration(EntryTypeList.Codes.Warehouse, "BINW000001", "XJ5", "40000007", 100m);
				Factory.Save();
				var result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsReceive = (Warehouse.Integration.IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				var whsHelper = ObjectFactory.New<Warehouse.Integration.IWhsTransactionTestHelper>(Factory);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 100m);
				var header = Factory.New<CusInBondHeader>();
				header.BH_OA_Importer = importer.MainAddress.PK;
				header.BH_FTZMove = true;
				var moveHeader = header.MovementHeaders.AddNew();
				moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
				moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
				var bill = header.Bills.AddNew();
				bill.B0_MasterBillNumber = "XJ5-40000007";
				var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
				var container = moveDetail.Containers.AddNew();
				container.BC_ContainerNum = "NC";
				var commodity = container.Commodities.AddNew();
				commodity.BY_PartNumber = helper.Part.OP_PartNum;
				commodity.BY_WarehouseEntryNumber = "XJ5-40000007";
				commodity.BY_WarehouseEntryLineNo = 1;
				commodity.BY_InvoiceQuantity = 60m;
				Factory.Save();
				moveHeader.Logs.AddNew(Events.MessageStatusChange, ImportMessageStatusList.Codes.AwaitingDepartureOriginal, new ZDateTimeOffset(2010, 12, 01));
				moveHeader.Logs.AddNew(Events.MessageStatusChange, ImportMessageStatusList.Codes.ClearDepartureOriginal, new ZDateTimeOffset(2010, 12, 03));
				moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
				Factory.Save();
				var messageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
				var sendingHeader = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureAmend, messageInitiator);
				var sendingObj = sendingHeader.SendingObjects[0];
				sendingObj.US_ShouldSend = true;
				using (var form = new MessagingForm(sendingHeader))
				{
					form.Show();
					var sendButton = (ZButton)form.Controls["ButtonPanel"].Controls["SendButton"];
					messageInitiator.AnswerToContinueWithAction = true;
					sendButton.PerformClick();
					AssertEquals("2 messages were created.", messageInitiator.SuccessfulSendText);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
					AssertEquals(ImportMessageStatusList.Codes.AwaitingDepartureAmendment, moveHeader.BM_CustomsStatus);
					AssertEquals(WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, moveHeader.BM_WarehouseTransactionStatus);
				}

				sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
			}
		}

		public void TestSendDepartureDelete_CancelWarehouseWithdrawal()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var data = (ZArchitecture.Environment.RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
				var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (ZArchitecture.Environment.BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
				addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var importer = helper.Importer;
				var inwardDeclaration = helper.GetNewDeclaration(EntryTypeList.Codes.Warehouse, "BINW000001", "XJ5", "40000007", 100m);
				Factory.Save();
				var result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsReceive = (Warehouse.Integration.IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				var whsHelper = ObjectFactory.New<Warehouse.Integration.IWhsTransactionTestHelper>(Factory);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 100m);
				var header = Factory.New<CusInBondHeader>();
				header.BH_OA_Importer = importer.MainAddress.PK;
				header.BH_FTZMove = true;
				var moveHeader = header.MovementHeaders.AddNew();
				moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
				moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
				var bill = header.Bills.AddNew();
				bill.B0_MasterBillNumber = "XJ5-40000007";
				var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
				var container = moveDetail.Containers.AddNew();
				container.BC_ContainerNum = "NC";
				var commodity = container.Commodities.AddNew();
				commodity.BY_PartNumber = helper.Part.OP_PartNum;
				commodity.BY_WarehouseEntryNumber = "XJ5-40000007";
				commodity.BY_WarehouseEntryLineNo = 1;
				commodity.BY_InvoiceQuantity = 60m;
				Factory.Save();
				moveHeader.Logs.AddNew(Events.MessageStatusChange, ImportMessageStatusList.Codes.AwaitingDepartureOriginal, new ZDateTimeOffset(2010, 12, 01));
				moveHeader.Logs.AddNew(Events.MessageStatusChange, ImportMessageStatusList.Codes.ClearDepartureOriginal, new ZDateTimeOffset(2010, 12, 03));
				moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
				Factory.Save();
				moveHeader.PublishShipmentForWHSOutward(true);
				moveHeader.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				var messageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
				var sendingHeader = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureDelete, messageInitiator);
				var sendingObj = sendingHeader.SendingObjects[0];
				sendingObj.US_ShouldSend = true;
				using (var form = new MessagingForm(sendingHeader))
				{
					form.Show();
					var sendButton = (ZButton)form.Controls["ButtonPanel"].Controls["SendButton"];
					messageInitiator.AnswerToContinueWithAction = true;
					sendButton.PerformClick();
					AssertEquals("1 message was created.", messageInitiator.SuccessfulSendText);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
					AssertEquals(ImportMessageStatusList.Codes.AwaitingDepartureWithdraw, moveHeader.BM_CustomsStatus);
					AssertEquals(WarehouseTransactionStatusList.Codes.OutwardHolding, moveHeader.BM_WarehouseTransactionStatus);
				}

				sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
			}
		}

		public void TestSendBondedWarehouseUpdate_WithdrawFromWarehouse()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var data = (ZArchitecture.Environment.RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
				var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (ZArchitecture.Environment.BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
				addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var importer = helper.Importer;
				var inwardDeclaration = helper.GetNewDeclaration(EntryTypeList.Codes.Warehouse, "BINW000001", "XJ5", "40000007", 100m);
				Factory.Save();
				var result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsReceive = (Warehouse.Integration.IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				var whsHelper = ObjectFactory.New<Warehouse.Integration.IWhsTransactionTestHelper>(Factory);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 100m);
				var header = Factory.New<CusInBondHeader>();
				header.BH_OA_Importer = importer.MainAddress.PK;
				header.BH_FTZMove = true;
				var moveHeader = header.MovementHeaders.AddNew();
				moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
				moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
				var bill = header.Bills.AddNew();
				bill.B0_MasterBillNumber = "XJ5-40000007";
				var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
				var container = moveDetail.Containers.AddNew();
				container.BC_ContainerNum = "NC";
				var commodity = container.Commodities.AddNew();
				commodity.BY_PartNumber = helper.Part.OP_PartNum;
				commodity.BY_WarehouseEntryNumber = "XJ5-40000007";
				commodity.BY_WarehouseEntryLineNo = 1;
				commodity.BY_InvoiceQuantity = 60m;
				Factory.Save();
				var messageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
				var sendingHeader = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.BondedWarehouseUpdate, messageInitiator);
				var sendingObj = sendingHeader.SendingObjects[0];
				sendingObj.US_ShouldSend = true;
				using (var form = new MessagingForm(sendingHeader))
				{
					form.Show();
					var sendButton = (ZButton)form.Controls["ButtonPanel"].Controls["SendButton"];
					messageInitiator.AnswerToContinueWithAction = true;
					sendButton.PerformClick();
					AssertEquals(messageInitiator.SuccessfulSendText, true, messageInitiator.SuccessfulSendText.StartsWith("Stock Release has been updated. (WHS Order:"));
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
					AssertEquals(WarehouseTransactionStatusList.Codes.OutwardUpdated, moveHeader.BM_WarehouseTransactionStatus);
				}

				sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
			}
		}

		public void TestSendBondedWarehouseCancel_CancelWarehouseWithdrawal()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var data = (ZArchitecture.Environment.RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
				var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (ZArchitecture.Environment.BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
				addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var importer = helper.Importer;
				var inwardDeclaration = helper.GetNewDeclaration(EntryTypeList.Codes.Warehouse, "BINW000001", "XJ5", "40000007", 100m);
				Factory.Save();
				var result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsReceive = (Warehouse.Integration.IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				var whsHelper = ObjectFactory.New<Warehouse.Integration.IWhsTransactionTestHelper>(Factory);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 100m);
				var header = Factory.New<CusInBondHeader>();
				header.BH_OA_Importer = importer.MainAddress.PK;
				header.BH_FTZMove = true;
				var moveHeader = header.MovementHeaders.AddNew();
				moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
				moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
				var bill = header.Bills.AddNew();
				bill.B0_MasterBillNumber = "XJ5-40000007";
				var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
				var container = moveDetail.Containers.AddNew();
				container.BC_ContainerNum = "NC";
				var commodity = container.Commodities.AddNew();
				commodity.BY_PartNumber = helper.Part.OP_PartNum;
				commodity.BY_WarehouseEntryNumber = "XJ5-40000007";
				commodity.BY_WarehouseEntryLineNo = 1;
				commodity.BY_InvoiceQuantity = 60m;
				Factory.Save();
				moveHeader.PublishShipmentForWHSOutward(true);
				moveHeader.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				var messageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
				var sendingHeader = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.BondedWarehouseCancel, messageInitiator);
				var sendingObj = sendingHeader.SendingObjects[0];
				sendingObj.US_ShouldSend = true;
				using (var form = new MessagingForm(sendingHeader))
				{
					form.Show();
					var sendButton = (ZButton)form.Controls["ButtonPanel"].Controls["SendButton"];
					messageInitiator.AnswerToContinueWithAction = true;
					sendButton.PerformClick();
					AssertEquals(messageInitiator.SuccessfulSendText, true, messageInitiator.SuccessfulSendText.StartsWith("Stock Release has been canceled. (WHS Order:"));
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 100m);
					AssertEquals(WarehouseTransactionStatusList.Codes.OutwardCanceled, moveHeader.BM_WarehouseTransactionStatus);
				}

				sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
			}
		}

		public void TestSendAmendment()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "MW323423";
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_B0 = bill.PK;
			Factory.Save();
			moveHeader.Logs.AddNew(Events.MessageStatusChange, ImportMessageStatusList.Codes.AwaitingDepartureOriginal, new ZDateTimeOffset(2010, 12, 01));
			moveHeader.Logs.AddNew(Events.MessageStatusChange, ImportMessageStatusList.Codes.ClearDepartureOriginal, new ZDateTimeOffset(2010, 12, 03));
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			Factory.Save();
			var messageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			var sendingHeader = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureAmend, messageInitiator);
			var sendingObj = sendingHeader.SendingObjects[0];
			sendingObj.US_ShouldSend = true;
			using (var form = new MessagingForm(sendingHeader))
			{
				form.Show();
				var sendButton = (ZButton)form.Controls["ButtonPanel"].Controls["SendButton"];
				messageInitiator.AnswerToContinueWithAction = false;
				sendButton.PerformClick();
				AssertContains("It is likely that your message(s) will be rejected by Customs, as they have the following message errors:", messageInitiator.PastYesNoQuestionsAsked[0]);
				AssertEquals(0, moveHeader.Messages.Count);
				AssertEquals(false, messageInitiator.SuccessfulSendOccured);
				messageInitiator.AnswerToContinueWithAction = true;
				messageInitiator.PastYesNoQuestionsAsked.Clear();
				sendButton.PerformClick();
				AssertContains("It is likely that your message(s) will be rejected by Customs, as they have the following message errors:", messageInitiator.PastYesNoQuestionsAsked[0]);
				AssertEquals(true, messageInitiator.SuccessfulSendOccured);
				AssertEquals("Should have created a Delete message & a pending Add message", "2 messages were created.", messageInitiator.SuccessfulSendText);
				AssertEquals("Should have created a Delete message & a pending Add message", 2, moveHeader.Messages.Count);
				var deleteMessage = moveHeader.Messages[0];
				AssertEquals(true, deleteMessage.EM_SendWithMessageErrors);
				AssertEquals("Delete message EM_MessageSubType", Enterprise.Customs.US.Business.EM_MessageSubTypeList.Codes.InBondDepartureDelete, deleteMessage.EM_MessageSubType);
				AssertEquals("Delete message should be queued ready to go", EDIMessage.Status.Queued, deleteMessage.EM_Status);
				var addMessage = moveHeader.Messages[1];
				AssertEquals(true, addMessage.EM_SendWithMessageErrors);
				AssertEquals("Add message EM_MessageSubType", Enterprise.Customs.US.Business.EM_MessageSubTypeList.Codes.InBondDepartureOriginal, addMessage.EM_MessageSubType);
				AssertEquals("Add message should be in pending state waiting for response to Delete message", EDIMessage.Status.Pending, addMessage.EM_Status);
				Assert("Delete message should be added before new Add message", deleteMessage.EM_MessageNum < addMessage.EM_MessageNum);
			}

			sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
		}

		public void TestFormHeading()
		{
			var header = Factory.New<CusInBondHeader>();
			header.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			var moveHeader = header.MovementHeaders.AddNew();
			AssertFormHeading(header, InBondMessageType.DepartureAdd, "In-Bond Departure Add Messaging");
			AssertFormHeading(header, InBondMessageType.DepartureDelete, "In-Bond Departure Delete Messaging");
			AssertFormHeading(header, InBondMessageType.DepartureBillDelete, "Bill of Lading Delete Messaging");
			AssertFormHeading(header, InBondMessageType.InBondLevelArrival, "In-Bond Arrival Messaging");
			AssertFormHeading(header, InBondMessageType.InBondLevelExportation, "In-Bond Exportation Messaging");
			AssertFormHeading(header, InBondMessageType.InBondLevelTransferOfLiability, "In-Bond Transfer Of Liability Messaging");
			AssertFormHeading(header, InBondMessageType.DepartureAmend, "In-Bond Departure Amendment (Delete/Re-Add) Messaging");
			AssertFormHeading(header, InBondMessageType.AirInBondAdd, "In-Bond Departure Add Messaging");
			AssertFormHeading(header, InBondMessageType.AirInBondDelete, "In-Bond Departure Delete Messaging");
			AssertFormHeading(header, InBondMessageType.AirBillDelete, "Bill of Lading Delete Messaging");
			AssertFormHeading(header, InBondMessageType.AirEntireInBondArrival, "In-Bond Arrival Messaging");
			AssertFormHeading(header, InBondMessageType.AirEntireInBondExportation, "In-Bond Exportation Messaging");
			AssertFormHeading(header, InBondMessageType.AirInBondAmend, "In-Bond Departure Amendment (Delete/Re-Add) Messaging");
			AssertFormHeading(header, InBondMessageType.BillOfLadingLevelArrival, "Bill of Lading Arrival Messaging");
			AssertFormHeading(header, InBondMessageType.ContainerLevelArrival, "Container Arrival Messaging");
			AssertFormHeading(header, InBondMessageType.BillOfLadingLevelExportation, "Bill of Lading Exportation Messaging");
			AssertFormHeading(header, InBondMessageType.ContainerLevelExportation, "Container Exportation Messaging");
			header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
		}

		public void TestColumnsAvailablityForAirInBondAdd()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			Factory.Save();
			var sendingObject = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.AirInBondAdd, new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			using (MessagingForm form = new MessagingForm(sendingObject))
			{
				form.Show();
				ZGrid movementHeadersMessagesGrid = form.Controls.Find("MovementHeadersMessagesGrid", true)[0] as ZGrid;
				AssertNotNull("US_ShouldSend is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ShouldSend]);
				AssertNotNull("US_InBondNumber is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondNumber]);
				AssertNotNull("US_InBondEntryType is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondEntryType]);
				AssertNotNull("US_InBondCarrierID is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondCarrierID]);
				AssertNotNull("US_InBondCarrierSCAC is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondCarrierSCAC]);
				AssertNotNull("US_DestinationPortDCode is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_DestinationPortDCode]);
				AssertNotNull("US_ForeignDestPortKCode is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ForeignDestPortKCode]);
				AssertNotNull("US_MonetaryValue is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_MonetaryValue]);
			}

			sendingObject.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
		}

		public void TestColumnsAvailablityForAirInBondDelete()
		{
			var header = Factory.New<CusInBondHeader>();
			Factory.Save();
			var sendingHeader = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.AirInBondDelete, new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			using (MessagingForm form = new MessagingForm(sendingHeader))
			{
				form.Show();
				ZGrid movementHeadersMessagesGrid = form.Controls.Find("MovementHeadersMessagesGrid", true)[0] as ZGrid;
				AssertNotNull("US_ShouldSend is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ShouldSend]);
				AssertNotNull("US_InBondNumber is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondNumber]);
				AssertNotNull("US_InBondEntryType is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondEntryType]);
				AssertNotNull("US_InBondCarrierID is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondCarrierID]);
				AssertNotNull("US_InBondCarrierSCAC is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondCarrierSCAC]);
				AssertNotNull("US_DestinationPortDCode is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_DestinationPortDCode]);
				AssertNotNull("US_ForeignDestPortKCode is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ForeignDestPortKCode]);
				AssertNotNull("US_MonetaryValue is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_MonetaryValue]);
			}

			sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
		}

		public void TestColumnsAvailablityForAirInBondAmendment()
		{
			var header = Factory.New<CusInBondHeader>();
			Factory.Save();
			var sendingHeader = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.AirInBondAmend, new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			using (MessagingForm form = new MessagingForm(sendingHeader))
			{
				form.Show();
				ZGrid movementHeadersMessagesGrid = form.Controls.Find("MovementHeadersMessagesGrid", true)[0] as ZGrid;
				AssertNotNull("US_ShouldSend is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ShouldSend]);
				AssertNotNull("US_InBondNumber is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondNumber]);
				AssertNotNull("US_InBondEntryType is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondEntryType]);
				AssertNotNull("US_InBondCarrierID is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondCarrierID]);
				AssertNotNull("US_InBondCarrierSCAC is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondCarrierSCAC]);
				AssertNotNull("US_DestinationPortDCode is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_DestinationPortDCode]);
				AssertNotNull("US_ForeignDestPortKCode is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ForeignDestPortKCode]);
				AssertNotNull("US_MonetaryValue is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_MonetaryValue]);
			}

			sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
		}

		public void TestColumnsAvailablityForAirEntireInBondArrival()
		{
			var header = Factory.New<CusInBondHeader>();
			Factory.Save();
			var sendingObject = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.AirEntireInBondArrival, new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			using (MessagingForm form = new MessagingForm(sendingObject))
			{
				form.Show();
				ZGrid movementHeadersMessagesGrid = form.Controls.Find("MovementHeadersMessagesGrid", true)[0] as ZGrid;
				AssertNotNull("US_ShouldSend is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ShouldSend]);
				AssertNotNull("US_InBondNumber is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondNumber]);
				AssertNotNull("US_ArrivalDate is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ArrivalDate]);
				AssertNotNull("US_ArrivalPort is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ArrivalPort]);
				AssertNotNull("US_DestinationPortDCode is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_DestinationPortDCode]);
				AssertNotNull("US_ArrivalFirmsCode is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ArrivalFirmsCode]);
			}

			sendingObject.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
		}

		public void TestColumnsAvailablityForAirEntireInBondExportation()
		{
			var header = Factory.New<CusInBondHeader>();
			Factory.Save();
			var sendingHeader = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.AirEntireInBondExportation, new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			using (MessagingForm form = new MessagingForm(sendingHeader))
			{
				form.Show();
				ZGrid movementHeadersMessagesGrid = form.Controls.Find("MovementHeadersMessagesGrid", true)[0] as ZGrid;
				AssertNotNull("US_ShouldSend is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ShouldSend]);
				AssertNotNull("US_InBondNumber is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondNumber]);
				AssertNotNull("US_ExportDate is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ExportDate]);
				AssertNotNull("US_ExportPort is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ExportPort]);
				AssertNotNull("US_ExportTransportMode is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ExportTransportMode]);
				AssertNotNull("US_DestinationPortDCode is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_DestinationPortDCode]);
				AssertNotNull("US_ExportConveyance is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ExportConveyance]);
			}

			sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
		}

		public void TestColumnsAvailablityForDepartureDeletion()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				var header = Factory.New<CusInBondHeader>();
				header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
				Factory.Save();
				var sendingHeader = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureDelete, new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				using (MessagingForm form = new MessagingForm(sendingHeader))
				{
					form.Show();
					ZGrid movementHeadersMessagesGrid = form.Controls.Find("MovementHeadersMessagesGrid", true)[0] as ZGrid;
					AssertNotNull("US_ShouldSend is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ShouldSend]);
					AssertNotNull("US_InBondNumber is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondNumber]);
					AssertNotNull("US_InBondEntryType is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondEntryType]);
					AssertNotNull("US_InBondCarrierID is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondCarrierID]);
					AssertNotNull("US_InBondCarrierSCAC is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondCarrierSCAC]);
					AssertNotNull("US_DestinationPortDCode is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_DestinationPortDCode]);
					AssertNotNull("US_ForeignDestPortKCode is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ForeignDestPortKCode]);
					AssertNotNull("US_MonetaryValue is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_MonetaryValue]);
					AssertNotNull("US_BTAIndicator is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_BTAIndicator]);
				}

				sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
				sendingHeader = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureBillDelete, new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				using (MessagingForm form = new MessagingForm(sendingHeader))
				{
					form.Show();
					ZGrid movementHeadersMessagesGrid = form.Controls.Find("MovementHeadersMessagesGrid", true)[0] as ZGrid;
					AssertNotNull("US_ShouldSend is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ShouldSend]);
					AssertNotNull("US_InBondNumber is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondNumber]);
					AssertNotNull("US_ActionCode is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ActionCode]);
					AssertNotNull("US_ActionDescription is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ActionDescription]);
					AssertNotNull("US_MasterBillNumber is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_MasterBillNumber]);
					AssertNotNull("US_MasterBillIssuer is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_MasterBillIssuer]);
					AssertNotNull("US_HouseBillNumber is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_HouseBillNumber]);
					AssertNotNull("US_HouseBillIssuer is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_HouseBillIssuer]);
					AssertNotNull("US_InBondEntryType is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondEntryType]);
					AssertNotNull("US_InBondCarrierID is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondCarrierID]);
					AssertNotNull("US_InBondCarrierSCAC is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondCarrierSCAC]);
					AssertNotNull("US_DestinationPortDCode is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_DestinationPortDCode]);
					AssertNotNull("US_ForeignDestPortKCode is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ForeignDestPortKCode]);
					AssertNotNull("US_MonetaryValue is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_MonetaryValue]);
					AssertNotNull("US_BTAIndicator is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_BTAIndicator]);
				}

				sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
				sendingHeader = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.AirInBondDelete, new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				using (MessagingForm form = new MessagingForm(sendingHeader))
				{
					form.Show();
					ZGrid movementHeadersMessagesGrid = form.Controls.Find("MovementHeadersMessagesGrid", true)[0] as ZGrid;
					AssertNotNull("US_ShouldSend is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ShouldSend]);
					AssertNotNull("US_InBondNumber is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondNumber]);
					AssertNotNull("US_InBondEntryType is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondEntryType]);
					AssertNotNull("US_InBondCarrierID is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondCarrierID]);
					AssertNotNull("US_InBondCarrierSCAC is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondCarrierSCAC]);
					AssertNotNull("US_DestinationPortDCode is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_DestinationPortDCode]);
					AssertNotNull("US_ForeignDestPortKCode is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ForeignDestPortKCode]);
					AssertNotNull("US_MonetaryValue is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_MonetaryValue]);
				}

				sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
				sendingHeader = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.AirBillDelete, new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				using (MessagingForm form = new MessagingForm(sendingHeader))
				{
					form.Show();
					ZGrid movementHeadersMessagesGrid = form.Controls.Find("MovementHeadersMessagesGrid", true)[0] as ZGrid;
					AssertNotNull("US_ShouldSend is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ShouldSend]);
					AssertNotNull("US_InBondNumber is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondNumber]);
					AssertNotNull("US_ActionCode is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ActionCode]);
					AssertNotNull("US_ActionDescription is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ActionDescription]);
					AssertNotNull("US_MasterBillNumber is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_MasterBillNumber]);
					AssertNotNull("US_MasterBillIssuer is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_MasterBillIssuer]);
					AssertNotNull("US_InBondEntryType is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondEntryType]);
					AssertNotNull("US_InBondCarrierID is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondCarrierID]);
					AssertNotNull("US_InBondCarrierSCAC is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondCarrierSCAC]);
					AssertNotNull("US_DestinationPortDCode is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_DestinationPortDCode]);
					AssertNotNull("US_ForeignDestPortKCode is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ForeignDestPortKCode]);
					AssertNotNull("US_MonetaryValue is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_MonetaryValue]);
					AssertNotNull("US_BTAIndicator is available", movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_BTAIndicator]);
				}

				sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
			}
		}

		public void TestColumnAvailablityForArrival()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				var header = Factory.New<CusInBondHeader>();
				header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
				Factory.Save();
				var sendingHeader = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.InBondLevelArrival, new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				using (MessagingForm form = new MessagingForm(sendingHeader))
				{
					form.Show();
					ZGrid movementHeadersMessagesGrid = form.Controls.Find("MovementHeadersMessagesGrid", true)[0] as ZGrid;
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ShouldSend]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondNumber]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ActionCode]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ActionDescription]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ArrivalDate]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ArrivalPort]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ArrivalFirmsCode]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondEntryType]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondCarrierID]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondCarrierSCAC]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_DestinationPortDCode]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ForeignDestPortKCode]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_MonetaryValue]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_BTAIndicator]);
				}

				sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
				sendingHeader = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.BillOfLadingLevelArrival, new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				using (MessagingForm form = new MessagingForm(sendingHeader))
				{
					form.Show();
					ZGrid movementHeadersMessagesGrid = form.Controls.Find("MovementHeadersMessagesGrid", true)[0] as ZGrid;
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ShouldSend]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondNumber]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ActionCode]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ActionDescription]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_MasterBillNumber]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_MasterBillIssuer]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_HouseBillNumber]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_HouseBillIssuer]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ArrivalDate]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ArrivalPort]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ArrivalFirmsCode]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondEntryType]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondCarrierID]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondCarrierSCAC]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_DestinationPortDCode]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ForeignDestPortKCode]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_MonetaryValue]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_BTAIndicator]);
				}

				sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
				sendingHeader = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.ContainerLevelArrival, new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				using (MessagingForm form = new MessagingForm(sendingHeader))
				{
					form.Show();
					ZGrid movementHeadersMessagesGrid = form.Controls.Find("MovementHeadersMessagesGrid", true)[0] as ZGrid;
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ShouldSend]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondNumber]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ActionCode]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ActionDescription]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_MasterBillNumber]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_MasterBillIssuer]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ContainerNumber]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ArrivalDate]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ArrivalPort]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ArrivalFirmsCode]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondEntryType]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondCarrierID]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondCarrierSCAC]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_DestinationPortDCode]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ForeignDestPortKCode]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_MonetaryValue]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_BTAIndicator]);
				}

				sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
			}
		}

		public void TestColumnAvailablityForExportation()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				var header = Factory.New<CusInBondHeader>();
				header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
				Factory.Save();
				var sendingHeader = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.InBondLevelExportation, new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				using (MessagingForm form = new MessagingForm(sendingHeader))
				{
					form.Show();
					ZGrid movementHeadersMessagesGrid = form.Controls.Find("MovementHeadersMessagesGrid", true)[0] as ZGrid;
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ShouldSend]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondNumber]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ActionCode]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ActionDescription]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ExportDate]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ExportPort]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondEntryType]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondCarrierID]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondCarrierSCAC]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_DestinationPortDCode]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ForeignDestPortKCode]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_MonetaryValue]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_BTAIndicator]);
				}

				sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
				sendingHeader = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.BillOfLadingLevelExportation, new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				using (MessagingForm form = new MessagingForm(sendingHeader))
				{
					form.Show();
					ZGrid movementHeadersMessagesGrid = form.Controls.Find("MovementHeadersMessagesGrid", true)[0] as ZGrid;
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ShouldSend]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondNumber]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ActionCode]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ActionDescription]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_MasterBillNumber]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_MasterBillIssuer]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_HouseBillNumber]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_HouseBillIssuer]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ExportDate]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ExportPort]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondEntryType]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondCarrierID]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondCarrierSCAC]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_DestinationPortDCode]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ForeignDestPortKCode]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_MonetaryValue]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_BTAIndicator]);
				}

				sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
				sendingHeader = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.ContainerLevelExportation, new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				using (MessagingForm form = new MessagingForm(sendingHeader))
				{
					form.Show();
					ZGrid movementHeadersMessagesGrid = form.Controls.Find("MovementHeadersMessagesGrid", true)[0] as ZGrid;
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ShouldSend]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondNumber]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ActionCode]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ActionDescription]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_MasterBillNumber]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_MasterBillIssuer]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ContainerNumber]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ExportDate]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ExportPort]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondEntryType]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondCarrierID]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_InBondCarrierSCAC]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_DestinationPortDCode]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_ForeignDestPortKCode]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_MonetaryValue]);
					AssertNotNull(movementHeadersMessagesGrid.Columns[InBondMessageSendingObject.Schema.US_BTAIndicator]);
				}

				sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
			}
		}

		void AssertFormHeading(CusInBondHeader header, InBondMessageType messageType, string expectedFormCaption)
		{
			header.Factory.Save();
			var sendingObj = new InBondMessageSendingHeaderObject(header.PK, messageType, header.MessageInitiator);
			using (MessagingForm form = new MessagingForm(sendingObj))
			{
				form.Show();
				AssertEquals(expectedFormCaption, form.FormHeading);
			}

			sendingObj.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
		}

		protected override IEnumerable<Form> FormsToBash
		{
			get
			{
				yield return GetFormToBash(InBondMessageType.DepartureAdd);
				if (sendingHeader != null)
				{
					sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
				}

				yield return GetFormToBash(InBondMessageType.InBondLevelArrival);
				if (sendingHeader != null)
				{
					sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
				}

				yield return GetFormToBash(InBondMessageType.InBondLevelExportation);
				if (sendingHeader != null)
				{
					sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
				}

				yield return GetFormToBash(InBondMessageType.InBondLevelTransferOfLiability);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return GetFormToBash(InBondMessageType.DepartureAdd);
		}

		protected override void TearDown()
		{
			if (sendingHeader != null)
			{
				sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
			}

			base.TearDown();
		}

		InBondMessageSendingHeaderObject sendingHeader;
		Form GetFormToBash(InBondMessageType messageType)
		{
			var header = Factory.New<CusInBondHeader>();
			Factory.Save();
			sendingHeader = new InBondMessageSendingHeaderObject(header.PK, messageType, new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusInBondMoveHeader moveHeader = sendingHeader.Header.MovementHeaders.AddNew();
			sendingHeader.SendingObjects.Add(new InBondMessageSendingObject(moveHeader, messageType));
			((IBusinessObjectState)sendingHeader).ClearHasChangesIncludingChildren();
			return new MessagingForm(sendingHeader);
		}
	}
}
