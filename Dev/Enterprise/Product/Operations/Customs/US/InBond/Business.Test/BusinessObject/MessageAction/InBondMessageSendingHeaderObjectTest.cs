using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.InBond.Business.WarehouseExtensions.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(InBondMessageSendingHeaderObject))]
	sealed class InBondMessageSendingHeaderObjectTest : NonPersistentBusinessObjectTestCase
	{
		const string message = @"It is likely that your message(s) will be rejected by Customs, as they have the following message errors:

Issuer Code: You have not entered a Master Issuer Code.
Master Bill Number: You have not entered a Master Bill Number.
In-Bond Number: The Inbond Number Range from which this Job will be allocated a number has not been set up correctly.
The Inbond Number Range can be set up in the System Registry.
Maintain -> System -> Registry -> Customs -> Country or Region Specific -> United States of America -> Import -> ABI -> In-Bond Number -> Number Range.
US Port Of Destination: You have not entered an US Port Of Destination.
FIRMS: The code you have selected is not in the list.

Do you want to send the message(s) despite these errors?";
		public void TestNotificationsWhenSending()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill1 = header.Bills.AddNew();
			var moveHeader1 = header.MovementHeaders.AddNew();
			var moveDetail1 = moveHeader1.MovementDetails.AddNew(bill1.PK);
			var container1 = moveDetail1.Containers.AddNew();
			container1.BC_ContainerNum = "1";
			var bill2 = header.Bills.AddNew();
			var moveHeader2 = header.MovementHeaders.AddNew();
			var moveDetail2 = moveHeader2.MovementDetails.AddNew(bill2.PK);
			var container2 = moveDetail1.Containers.AddNew();
			var messageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			Factory.Save();
			var sendingHeaderObject = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.ContainerLevelArrival, messageInitiator);
			sendingHeaderObject.Header.RecalculateValidationModesOnHeader(InBondMessageType.ContainerLevelArrival);
			Assert(sendingHeaderObject.SendingObjects.Count == 2);
			var sendingObject = sendingHeaderObject.SendingObjects[0];
			sendingObject.US_ShouldSend = true;
			sendingObject.US_ArrivalDate = ZDateTime.Today;
			sendingObject.US_ArrivalFirmsCode = "1";
			sendingHeaderObject.SendData();
			AssertEquals(message.Replace("\r", "").Replace("\n", ""), messageInitiator.PastYesNoQuestionsAsked[0].Replace("\r", "").Replace("\n", ""));
			messageInitiator.PastYesNoQuestionsAsked.Clear();
			sendingObject = sendingHeaderObject.SendingObjects[1];
			sendingObject.US_ShouldSend = true;
			sendingObject.US_ArrivalDate = ZDateTime.Today;
			sendingObject.US_ArrivalFirmsCode = "1";
			sendingHeaderObject.SendData();
			AssertContains("Container Number: Please enter a valid container number associated with the bill of lading exactly as it physically appears on the container.", messageInitiator.PastYesNoQuestionsAsked[0].Replace("\r", "").Replace("\n", ""));
		}

		public void TestHasNotificationsOnObjectsMarkedForSending()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.InBondNumber = "INB4";
			moveHeader.LogManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingDepartureOriginal, ImportMessageStatusList.Codes.ClearDepartureOriginal);
			var moveDetail4 = moveHeader.MovementDetails.AddNew(bill.PK);
			moveDetail4.B9_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			var messageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			Factory.Save();
			var sendingHeaderObject = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DiversionRequest, messageInitiator);
			Assert(sendingHeaderObject.SendingObjects.Count == 1);
			var sendingObject = sendingHeaderObject.SendingObjects[0];
			sendingObject.US_ShouldSend = true;
			sendingObject.US_DiversionDate = ZDateTime.BrettsBirthday;
			sendingObject.US_DiversionCarrierSCAC = "^Z";
			sendingObject.US_DiversionInBondCarrierID = "10210";
			sendingObject.US_DiversionPortCode = "1101";
			sendingObject.ValidateUS_DiversionPortCode();
			Assert(sendingHeaderObject.HasNotificationsOnObjectsMarkedForSending());
			var messageErrorText = sendingObject.Notifications.GetMessageErrors().ToUniqueMessageListString();
			Assert(messageErrorText.Contains("US_DiversionInBondCarrierID: An In-Bond Carrier ID must be a valid IRS Number"));
			Assert(messageErrorText.Contains("US_DiversionCarrierSCAC: The code you have selected is not in the list"));
		}

		public void TestIsWarehouseType()
		{
			var helper = new WhsDataTestHelper(Factory);
			var importer = helper.Importer;
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FTZMove = true;
			var moveHeader = header.MovementHeaders.AddNew();
			var messageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			Factory.Save();
			foreach (var type in new[] { InBondMessageType.BondedWarehouseCancel, InBondMessageType.BondedWarehouseUpdate })
			{
				AssertEquals(type.ToString(), true, new InBondMessageSendingHeaderObject(header.PK, type, messageInitiator).IsWarehouseType);
			}

			foreach (var type in new[] { InBondMessageType.DepartureAdd, InBondMessageType.DepartureAmend, InBondMessageType.DepartureDelete })
			{
				AssertEquals(type.ToString(), false, new InBondMessageSendingHeaderObject(header.PK, type, messageInitiator).IsWarehouseType);
			}
		}

		public void TestNoMessageIsCreatedIfAMovementHasError()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
				var data = (ZArchitecture.Environment.RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
				var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (ZArchitecture.Environment.BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
				addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var declaration = helper.GetNewDeclaration(EntryTypeList.Codes.Warehouse, "B0323565", "XJ5", "ENS32423", 100m);
				Factory.Save();
				declaration.PublishShipmentForWHSInward(false);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-ENS32423-1", 100m);
				var importer = helper.Importer;
				var header = Factory.New<CusInBondHeader>();
				header.BH_OA_Importer = importer.MainAddress.PK;
				header.BH_FTZMove = true;
				var bill = header.Bills.AddNew();
				bill.B0_MasterBillNumber = "XJ5-ENS32423";
				var moveHeader1 = header.MovementHeaders.AddNew();
				moveHeader1.InBondNumber = "INB0001";
				moveHeader1.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
				var moveDetail1 = moveHeader1.MovementDetails.AddNew(bill.PK);
				var container1 = moveDetail1.Containers.AddNew();
				container1.BC_ContainerNum = "NC";
				var commodity1 = container1.Commodities.AddNew();
				commodity1.BY_PartNumber = helper.Part.OP_PartNum;
				commodity1.BY_WarehouseEntryNumber = "XJ5-ENS32423";
				commodity1.BY_WarehouseEntryLineNo = 1;
				commodity1.BY_InvoiceQuantity = 60m;
				var moveHeader2 = header.MovementHeaders.AddNew();
				moveHeader2.InBondNumber = "INB0002";
				moveHeader2.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
				var moveDetail2 = moveHeader2.MovementDetails.AddNew(bill.PK);
				var container2 = moveDetail2.Containers.AddNew();
				container2.BC_ContainerNum = "NC";
				var commodity2 = container2.Commodities.AddNew();
				commodity2.BY_PartNumber = helper.Part.OP_PartNum;
				commodity2.BY_WarehouseEntryNumber = "XJ5-ENS32423";
				commodity2.BY_WarehouseEntryLineNo = 1;
				commodity2.BY_InvoiceQuantity = 60m;
				Factory.Save();
				var messageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer()
				{ ThrowExceptionOnInvalidOperation = false };
				header.MessageInitiator = messageInitiator;
				var sendingObject = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureAdd, messageInitiator);
				AssertEquals(true, sendingObject.SendData());
				CombineAssertions(() =>
				{
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-ENS32423-1", 100m);
					moveHeader1.Messages.Load();
					AssertEquals("moveHeader1.Messages.Count", 0, moveHeader1.Messages.Count);
					moveHeader1.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalEvent, RecipientRoleType.BWR);
					AssertEquals(WarehouseTransactionStatusList.Codes.OutwardCanceled, moveHeader1.BM_WarehouseTransactionStatus);
					moveHeader2.Messages.Load();
					AssertEquals("moveHeader2.Messages.Count", 0, moveHeader2.Messages.Count);
					moveHeader2.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalShipment, RecipientRoleType.BWR);
					AssertEquals("", moveHeader2.BM_WarehouseTransactionStatus);
					sendingObject.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
					commodity1.BY_InvoiceQuantity = 40m;
					Factory.Save();
					sendingObject = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureAdd, messageInitiator);
					AssertEquals(true, sendingObject.SendData());
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-ENS32423-1", 0m);
					moveHeader1.Messages.Load();
					AssertEquals("moveHeader1.Messages.Count", 1, moveHeader1.Messages.Count);
					moveHeader1.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalShipment, RecipientRoleType.BWR);
					AssertEquals(WarehouseTransactionStatusList.Codes.OutwardCreatedPending, moveHeader1.BM_WarehouseTransactionStatus);
					moveHeader2.Messages.Load();
					AssertEquals("moveHeader2.Messages.Count", 1, moveHeader2.Messages.Count);
					moveHeader2.AssertXMLMessageWasCreated(EDIMessageSubTypeList.Codes.XmlUniversalShipment, RecipientRoleType.BWR);
					AssertEquals(WarehouseTransactionStatusList.Codes.OutwardCreatedPending, moveHeader2.BM_WarehouseTransactionStatus);
					sendingObject.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
				});
			}
		}

		public void TestSendData()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var data = (ZArchitecture.Environment.RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
				var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (ZArchitecture.Environment.BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
				addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var declaration = helper.GetNewDeclaration(EntryTypeList.Codes.Warehouse, "B0323565", "XJ5", "ENS32423", 100m);
				Factory.Save();
				declaration.PublishShipmentForWHSInward(false);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-ENS32423-1", 100m);
				var importer = helper.Importer;
				var header = Factory.New<CusInBondHeader>();
				header.BH_OA_Importer = importer.MainAddress.PK;
				header.BH_FTZMove = true;
				var moveHeader = header.MovementHeaders.AddNew();
				moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
				moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
				var bill = header.Bills.AddNew();
				bill.B0_MasterBillNumber = "XJ5-ENS32423";
				var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
				var container = moveDetail.Containers.AddNew();
				container.BC_ContainerNum = "NC";
				var commodity = container.Commodities.AddNew();
				commodity.BY_PartNumber = helper.Part.OP_PartNum;
				commodity.BY_WarehouseEntryNumber = "XJ5-ENS32423";
				commodity.BY_WarehouseEntryLineNo = 1;
				commodity.BY_InvoiceQuantity = 10m;
				Factory.Save();
				var messageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer()
				{ ThrowExceptionOnInvalidOperation = false };
				var sendingObject = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureAdd, messageInitiator);
				AssertEquals(true, sendingObject.SendData());
				AssertContains("It is likely that your message(s) will be rejected by Customs, as they have the following message errors:", messageInitiator.PastYesNoQuestionsAsked[0]);
				AssertEquals("1 message was created.", messageInitiator.SuccessfulSendText);
				moveHeader.Messages.Load();
				AssertEquals(1, moveHeader.Messages.Count);
				sendingObject.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
				messageInitiator.PastYesNoQuestionsAsked.Clear();
				messageInitiator.SuccessfulSendText = null;
				sendingObject = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.BondedWarehouseCancel, messageInitiator);
				sendingObject.SendingObjects[0].US_ShouldSend = true;
				AssertEquals(true, sendingObject.SendData());
				AssertEquals(1, messageInitiator.PastYesNoQuestionsAsked.Count);
				AssertEquals(@"Are you sure you want to cancel the Inventory stock release for the In-Bond Movement? 
If you click 'Yes', all stock of the In-Bond Movement will be uncommitted.", messageInitiator.PastYesNoQuestionsAsked[0]);
				AssertEquals("Stock Release has been canceled. (WHS Order:W00000002)", messageInitiator.SuccessfulSendText);
				AssertNull(messageInitiator.Warning);
				sendingObject.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
				messageInitiator.SuccessfulSendText = null;
				commodity.BY_InvoiceQuantity = ZDecimal.Zero;
				Factory.Save();
				messageInitiator.PastYesNoQuestionsAsked.Clear();
				messageInitiator.Warning = null;
				sendingObject = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.BondedWarehouseUpdate, messageInitiator);
				sendingObject.SendingObjects[0].US_ShouldSend = true;
				commodity.BY_InvoiceQuantity = 0m;
				Factory.Save();
				AssertEquals(false, sendingObject.SendData());
				AssertEquals(0, messageInitiator.PastYesNoQuestionsAsked.Count);
				AssertNull(messageInitiator.SuccessfulSendText);
				AssertContains("In-Bond Movement: A Bonded Warehousing commodity must have an invoice quantity; not all Bonded Warehousing commodities have an invoice quantity specified.", messageInitiator.InvalidOperationText);
				sendingObject.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
			}
		}

		public void TestSendData_AutomationDisabled()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var data = (ZArchitecture.Environment.RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
				var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (ZArchitecture.Environment.BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
				addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var declaration = helper.GetNewDeclaration(EntryTypeList.Codes.Warehouse, "B0323565", "XJ5", "ENS32423", 100m);
				Factory.Save();
				declaration.PublishShipmentForWHSInward(false);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-ENS32423-1", 100m);
				var importer = helper.Importer;
				var header = Factory.New<CusInBondHeader>();
				header.BH_OA_Importer = importer.MainAddress.PK;
				header.BH_FTZMove = true;
				var moveHeader = header.MovementHeaders.AddNew();
				moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
				moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
				var bill = header.Bills.AddNew();
				bill.B0_MasterBillNumber = "XJ5-ENS32423";
				var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
				var container = moveDetail.Containers.AddNew();
				container.BC_ContainerNum = "NC";
				var commodity = container.Commodities.AddNew();
				commodity.BY_PartNumber = helper.Part.OP_PartNum;
				commodity.BY_WarehouseEntryNumber = "XJ5-ENS32423";
				commodity.BY_WarehouseEntryLineNo = 1;
				commodity.BY_InvoiceQuantity = 10m;
				Factory.Save();
				var messageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer()
				{ ThrowExceptionOnInvalidOperation = false };
				var sendingObject = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureAdd, messageInitiator);
				AssertEquals(true, sendingObject.SendData());
				AssertEquals("moveHeader.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, moveHeader.BM_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-ENS32423-1", 100m);
				sendingObject.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
				moveHeader.BM_WarehouseTransactionStatus = ZString.Empty;
				moveHeader.BM_CustomsStatus = ZString.Empty;
				Factory.Save();
				sendingObject = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureAdd, messageInitiator);
				AssertEquals(true, sendingObject.SendData());
				AssertEquals("moveHeader.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, moveHeader.BM_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-ENS32423-1", 90m);
				sendingObject.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
				moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
				commodity.BY_InvoiceQuantity = 20m;
				moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
				Factory.Save();
				sendingObject = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureAmend, messageInitiator);
				sendingObject.SendingObjects[0].US_ShouldSend = true;
				AssertEquals(true, sendingObject.SendData());
				AssertEquals("moveHeader.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, moveHeader.BM_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-ENS32423-1", 90m);
				sendingObject.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
				moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureAmendment;
				Factory.Save();
				sendingObject = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureDelete, messageInitiator);
				sendingObject.SendingObjects[0].US_ShouldSend = true;
				AssertEquals(true, sendingObject.SendData());
				AssertEquals("moveHeader.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.AutomationIsDisabled, moveHeader.BM_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-ENS32423-1", 90m);
				sendingObject.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
				moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureWithdraw;
				Factory.Save();
				sendingObject = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.BondedWarehouseUpdate, messageInitiator);
				sendingObject.SendingObjects[0].US_ShouldSend = true;
				AssertEquals(true, sendingObject.SendData());
				AssertEquals("moveHeader.BM_WarehouseTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, moveHeader.BM_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-ENS32423-1", 80m);
				sendingObject.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
				moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
				Factory.Save();
				sendingObject = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.BondedWarehouseCancel, messageInitiator);
				AssertEquals(0, sendingObject.SendingObjects.Count);
				sendingObject.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
			}
		}

		public void TestGetMovementHeadersForSending()
		{
			var header = Factory.New<CusInBondHeader>();
			header.MovementHeaders.AddNew();
			header.MovementHeaders.AddNew();

			var sendingObject = new InBondMessageSendingHeaderObject(header, InBondMessageType.DepartureAdd, new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(2, sendingObject.GetMovementHeadersForSending().Count());
		}

		public void TestRecalculateValidationModesOnHeader()
		{
			var header = Factory.New<CusInBondHeader>();
			Factory.Save();
			header.ValidationModes = ValidationModes.Departure;
			AssertEquals(ValidationModes.Departure, header.ValidationModes);

			var sendingObject1 = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.InBondLevelArrival, new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(ValidationModes.Departure, header.ValidationModes);
			AssertEquals(ValidationModes.InBondLevelArrival, sendingObject1.Header.ValidationModes);

			var sendingObject2 = new InBondMessageSendingHeaderObject(header, InBondMessageType.InBondLevelExportation, new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(ValidationModes.InBondLevelExportation, header.ValidationModes);
			AssertEquals(ValidationModes.InBondLevelExportation, sendingObject2.Header.ValidationModes);
		}

		public void TestNotificationsOnSendingObject()
		{
			var message = @"It is likely that your message(s) will be rejected by Customs, as they have the following message errors:

Arrival Date: You have not entered an Arrival Date.
Arrival Firms Code: You have not entered an Arrival Firms Code.

Do you want to send the message(s) despite these errors?";

			var header = Factory.New<CusInBondHeader>();
			var bill1 = header.Bills.AddNew();
			var moveHeader1 = header.MovementHeaders.AddNew();
			var moveDetail1 = moveHeader1.MovementDetails.AddNew(bill1.PK);
			var container1 = moveDetail1.Containers.AddNew();
			container1.BC_ContainerNum = "1";
			var messageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			Factory.Save();
			var sendingHeaderObject = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.InBondLevelArrival, messageInitiator);
			var sendingObject = sendingHeaderObject.SendingObjects[0];
			sendingObject.US_ShouldSend = true;
			sendingHeaderObject.SendData();
			AssertEquals(message.Replace("\r", "").Replace("\n", ""), messageInitiator.PastYesNoQuestionsAsked[0].Replace("\r", "").Replace("\n", ""));
		}

		public void TestNotificationsOnSendingObject_OnAirMode()
		{
			var message = "Arrival Firms Code: You have not entered an Arrival Firms Code.";

			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			var bill1 = header.Bills.AddNew();
			var moveHeader1 = header.MovementHeaders.AddNew();
			var moveDetail1 = moveHeader1.MovementDetails.AddNew(bill1.PK);
			var container1 = moveDetail1.Containers.AddNew();
			container1.BC_ContainerNum = "1";
			var messageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			Factory.Save();
			var sendingHeaderObject = new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.InBondLevelArrival, messageInitiator);
			var sendingObject = sendingHeaderObject.SendingObjects[0];
			sendingObject.US_ShouldSend = true;
			sendingHeaderObject.SendData();
			AssertNotContains(messageInitiator.PastYesNoQuestionsAsked[0], message);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<CusInBondHeader>();
			Factory.Save();
			return new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.BondedWarehouseUpdate, new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}
	}
}
