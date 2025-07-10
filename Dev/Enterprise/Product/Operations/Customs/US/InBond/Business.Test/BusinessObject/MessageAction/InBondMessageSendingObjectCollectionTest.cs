using System;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(InBondMessageSendingObjectCollection))]
	sealed class InBondMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<InBondMessageSendingObjectCollection>
	{
		public void TestConstructFromCusInBondHeader()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "bill1";
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.InBondNumber = "INB1";
			var moveDetail1 = moveHeader1.MovementDetails.AddNew(bill.PK);
			var moveHeader2 = header.MovementHeaders.AddNew();
			moveHeader2.InBondNumber = "INB2";
			var moveDetail2 = moveHeader2.MovementDetails.AddNew(bill.PK);
			moveDetail2.B9_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureWithdraw;
			var moveHeader3 = header.MovementHeaders.AddNew();
			moveHeader3.InBondNumber = "INB3";
			moveHeader3.LogManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingDepartureOriginal, ImportMessageStatusList.Codes.ClearDepartureOriginal);
			var moveDetail3 = moveHeader3.MovementDetails.AddNew(bill.PK);
			moveDetail3.B9_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			var moveHeader4 = header.MovementHeaders.AddNew();
			moveHeader4.InBondNumber = "INB4";
			var moveDetail4 = moveHeader4.MovementDetails.AddNew(bill.PK);
			moveHeader4.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
			var moveHeader5 = header.MovementHeaders.AddNew();
			moveHeader5.InBondNumber = "INB5";
			var moveDetail5 = moveHeader5.MovementDetails.AddNew(bill.PK);
			moveHeader5.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureWithdraw;
			var moveHeader6 = header.MovementHeaders.AddNew();
			moveHeader6.InBondNumber = "INB6";
			var moveDetail6 = moveHeader6.MovementDetails.AddNew(bill.PK);
			moveHeader6.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureAmendment;
			Factory.Save();
			var collection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureAdd, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			AssertEquals(2, collection.Count);
			var sendingObj1 = collection[0];
			var sendingObj2 = collection[1];
			var inbondNo1 = "INB1".PadRight(Customs.US.Business.MQEDIMessage.InBondNumberPlaceHolder.Length);
			var inbondNo2 = "INB2".PadRight(Customs.US.Business.MQEDIMessage.InBondNumberPlaceHolder.Length);
			if (sendingObj1.US_InBondNumber == inbondNo2)
			{
				sendingObj1 = collection[1];
				sendingObj2 = collection[0];
			}

			AssertEquals(inbondNo1, sendingObj1.US_InBondNumber);
			AssertEquals(inbondNo2, sendingObj2.US_InBondNumber);
			AssertNoExceptionThrown(delegate
			{
				collection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureBillDelete, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			});
			var bill2 = header.Bills.AddNew();
			bill2.B0_MasterBillNumber = "bill2";
			var moveHeader7 = header.MovementHeaders.AddNew();
			var moveDetail7 = moveHeader7.MovementDetails.AddNew(bill2.PK);
			var container1 = moveDetail7.Containers.AddNew();
			container1.BC_ContainerNum = "AC";
			var container2 = moveDetail7.Containers.AddNew();
			container2.BC_ContainerNum = "NC";
			Factory.Save();
			collection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.BillOfLadingLevelArrival, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			AssertEquals(1, collection.Count);
			sendingObj1 = collection[0];
			AssertEquals("bill2", sendingObj1.US_MasterBillNumber);
			bill2.B0_MessageStatus = "AAV";
			Factory.Save();
			collection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.BillOfLadingLevelArrival, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			AssertEquals(0, collection.Count);
			collection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.ContainerLevelArrival, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			AssertEquals(1, collection.Count);
			sendingObj1 = collection[0];
			AssertEquals("AC", sendingObj1.US_ContainerNumber);
		}

		public void TestConstructFromCusInBondHeaderForDepartureDelete()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.InBondNumber = "";
			var moveHeader2 = header.MovementHeaders.AddNew();
			moveHeader2.InBondNumber = "INB2";
			var moveHeader3 = header.MovementHeaders.AddNew();
			moveHeader3.InBondNumber = "INB3";
			moveHeader3.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureAmendment;
			var moveHeader4 = header.MovementHeaders.AddNew();
			moveHeader4.InBondNumber = "INB4";
			moveHeader4.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			moveHeader4.LogManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingDepartureOriginal, ImportMessageStatusList.Codes.ClearDepartureOriginal);
			var moveDetail4 = moveHeader4.MovementDetails.AddNew(bill.PK);
			moveDetail4.B9_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			Factory.Save();
			var collection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureDelete, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			AssertEquals(2, collection.Count);
			AssertEquals("INB2".PadRight(Customs.US.Business.MQEDIMessage.InBondNumberPlaceHolder.Length), collection[0].US_InBondNumber);
			AssertEquals("INB4".PadRight(Customs.US.Business.MQEDIMessage.InBondNumberPlaceHolder.Length), collection[1].US_InBondNumber);
			collection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DiversionRequest, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			AssertEquals("INB2".PadRight(Customs.US.Business.MQEDIMessage.InBondNumberPlaceHolder.Length), collection[0].US_InBondNumber);
			AssertEquals("INB4".PadRight(Customs.US.Business.MQEDIMessage.InBondNumberPlaceHolder.Length), collection[1].US_InBondNumber);
			collection[1].Send();
			moveHeader4.Reload();
			AssertEquals(moveHeader4.BM_CustomsStatus, ImportMessageStatusList.Codes.ClearDepartureOriginal);
		}

		public void TestAddingClearLogCancellsOthersIfNecessary()
		{
			var logManager = new StatusLogManager(MoveHeader.Logs);
			var log1 = MoveHeader.Logs.AddNew(Events.MessageStatusChange, ImportMessageStatusList.Codes.AwaitingDepartureOriginal);
			AssertEquals(false, log1.SL_IsCancelled);
			var log2 = MoveHeader.Logs.AddNew(Events.MessageStatusChange, ImportMessageStatusList.Codes.ClearDepartureOriginal);
			logManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingDepartureOriginal, ImportMessageStatusList.Codes.ClearDepartureOriginal);
			var log3 = MoveHeader.Logs.AddNew(Events.MessageStatusChange, ImportMessageStatusList.Codes.AwaitingDepartureWithdraw);
			var log4 = MoveHeader.Logs.AddNew(Events.MessageStatusChange, ImportMessageStatusList.Codes.ClearDepartureWithdraw);
			logManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingDepartureWithdraw, ImportMessageStatusList.Codes.ClearDepartureWithdraw);
			AssertEquals("Original departure clear should be cancelled by Delete clear message", true, log2.SL_IsCancelled);
		}

		public void TestHasAtLeastOneMarkedForSending()
		{
			var header = Factory.New<CusInBondHeader>();
			Factory.Save();
			var collection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureAdd, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			var obj1 = new InBondMessageSendingObject(collection.Header.MovementHeaders.AddNew(), InBondMessageType.DepartureAdd);
			collection.Add(obj1);
			obj1.US_ShouldSend = false;
			var obj2 = new InBondMessageSendingObject(collection.Header.MovementHeaders.AddNew(), InBondMessageType.DepartureAdd);
			obj2.US_ShouldSend = false;
			collection.Add(obj2);
			var obj3 = new InBondMessageSendingObject(collection.Header.MovementHeaders.AddNew(), InBondMessageType.DepartureAdd);
			obj3.US_ShouldSend = false;
			collection.Add(obj3);
			AssertEquals(false, collection.HasAtLeastOneMarkedForSending);
			obj2.US_ShouldSend = true;
			AssertEquals(true, collection.HasAtLeastOneMarkedForSending);
			collection.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
		}

		public void TestCanSendOriginalAgainAfterOriginalDeleted()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			moveHeader.InBondNumber = "INB1";
			header.Logs.AddNew(Events.MessageStatusChange, ImportMessageStatusList.Codes.ClearDepartureOriginal);
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			Factory.Save();
			InBondMessageSendingObjectCollection collection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureAdd, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			AssertEquals("Pre-condition - cannot add as Original already added", 0, collection.Count);
			Thread.Sleep(1000);
			header.Logs.AddNew(Events.MessageStatusChange, ImportMessageStatusList.Codes.AwaitingDepartureWithdraw);
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureWithdraw;
			header.Logs.AddNew(Events.MessageStatusChange, ImportMessageStatusList.Codes.ClearDepartureWithdraw);
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureWithdraw;
			Factory.Save();
			collection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureAdd, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			AssertEquals("Original cancelled, should be able to add again", 1, collection.Count);
		}

		public void TestWarehouseType()
		{
			var helper = new WhsDataTestHelper(Factory);
			var importer = helper.Importer;
			helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "HO2");
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FTZMove = true;
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.InBondNumber = "INB0001";
			moveHeader1.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			var moveHeader2 = header.MovementHeaders.AddNew();
			moveHeader2.InBondNumber = "INB0002";
			moveHeader2.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			var moveHeader3 = header.MovementHeaders.AddNew();
			moveHeader3.InBondNumber = "INB0003";
			Factory.Save();
			var sendingObjectCollection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.BondedWarehouseUpdate, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			AssertEquals(2, sendingObjectCollection.Count);
			var sendingObject1 = sendingObjectCollection[0];
			var sendingObject2 = sendingObjectCollection[1];
			AssertEquals(moveHeader1.PK, ((CusInBondMoveHeader)sendingObject1.moveHeader).PK);
			AssertEquals(moveHeader2.PK, ((CusInBondMoveHeader)sendingObject2.moveHeader).PK);
			sendingObjectCollection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.BondedWarehouseCancel, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			AssertEquals(1, sendingObjectCollection.Count);
			AssertEquals(moveHeader2.PK, ((CusInBondMoveHeader)sendingObjectCollection[0].moveHeader).PK);
		}

		public void TestAirInBondAdd()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader1 = header.MovementHeaders.AddNew();
			var moveHeader2 = header.MovementHeaders.AddNew();
			var moveHeader3 = header.MovementHeaders.AddNew();
			moveHeader3.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			moveHeader2.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
			Factory.Save();
			var sendingObjectCollection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.AirInBondAdd, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			AssertEquals("There is 1 message to send", 1, sendingObjectCollection.Count);
			AssertEquals("Should be moveHeader1 as it's not awaiting response and it's not cleared", moveHeader1.PK, ((CusInBondMoveHeader)sendingObjectCollection[0].moveHeader).PK);
		}

		public void TestAirInBondDelete()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader1 = header.MovementHeaders.AddNew();
			var moveHeader2 = header.MovementHeaders.AddNew();
			var moveHeader3 = header.MovementHeaders.AddNew();
			var moveHeader4 = header.MovementHeaders.AddNew();
			moveHeader1.InBondNumber = "INB0001";
			moveHeader2.InBondNumber = "INB0002";
			moveHeader3.InBondNumber = "INB0003";
			moveHeader1.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			moveHeader2.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureWithdraw;
			moveHeader3.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDeparturePartialOriginal;
			moveHeader4.BM_CustomsStatus = ImportMessageStatusList.Codes.NotSent;
			Factory.Save();
			var sendingObjectCollection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.AirInBondDelete, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			AssertEquals("There is 2 message to send", 2, sendingObjectCollection.Count);
			AssertEquals("Should be moveHeader1 as it's not awaiting response and is cleared", moveHeader1.PK, ((CusInBondMoveHeader)sendingObjectCollection[0].moveHeader).PK);
			AssertEquals("Should be moveHeader3 as it's not awaiting response and is partially cleared", moveHeader3.PK, ((CusInBondMoveHeader)sendingObjectCollection[1].moveHeader).PK);
			sendingObjectCollection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.AirInBondAmend, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			AssertEquals("There is 2 message to send", 2, sendingObjectCollection.Count);
			AssertEquals("Should be moveHeader1 as it's not awaiting response and is cleared", moveHeader1.PK, ((CusInBondMoveHeader)sendingObjectCollection[0].moveHeader).PK);
			AssertEquals("Should be moveHeader3 as it's not awaiting response and is partially cleared", moveHeader3.PK, ((CusInBondMoveHeader)sendingObjectCollection[1].moveHeader).PK);
		}

		public void TestAirEntireInBondArrival()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader1 = header.MovementHeaders.AddNew();
			var moveHeader2 = header.MovementHeaders.AddNew();
			moveHeader2.BM_MessageStatus = ImportMessageStatusList.Codes.AwaitingArrival;
			Factory.Save();
			var sendingObjectCollection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.AirEntireInBondArrival, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			AssertEquals("There is 1 message to send", 1, sendingObjectCollection.Count);
			AssertEquals("Should be moveHeader1 as it's not awaiting response", moveHeader1.PK, ((CusInBondMoveHeader)sendingObjectCollection[0].moveHeader).PK);
		}

		public void TestAirEntireInBondExportation()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader1 = header.MovementHeaders.AddNew();
			var moveHeader2 = header.MovementHeaders.AddNew();
			var moveHeader3 = header.MovementHeaders.AddNew();
			var moveHeader4 = header.MovementHeaders.AddNew();
			var moveHeader5 = header.MovementHeaders.AddNew();
			moveHeader1.InBondNumber = "INB0001";
			moveHeader2.InBondNumber = "INB0002";
			moveHeader3.InBondNumber = "INB0003";
			moveHeader4.InBondNumber = "INB0004";
			moveHeader5.InBondNumber = "INB0005";
			moveHeader1.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			moveHeader2.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			moveHeader3.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			moveHeader4.BM_InBondEntryType = InbondCommonTypeList.Codes._3ImmediateExport;
			moveHeader5.BM_InBondEntryType = InbondCommonTypeList.Codes._3ImmediateExport;
			moveHeader3.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingExportation;
			moveHeader5.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingExportation;
			Factory.Save();
			var sendingObjectCollection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.AirEntireInBondExportation, new Customs.Business.SendsMessagesToCustomsShutterUpperer())).Cast<InBondMessageSendingObject>();
			AssertEquals("There is 2 messages to send", 2, sendingObjectCollection.Count());
			var element1 = sendingObjectCollection.FirstOrDefault(x => x.US_InBondNumber.StartsWith("INB0002")).moveHeader as CusInBondMoveHeader;
			var element2 = sendingObjectCollection.FirstOrDefault(x => x.US_InBondNumber.StartsWith("INB0004")).moveHeader as CusInBondMoveHeader;
			AssertEquals("Should be moveHeader2 as it's not awaiting response and it's Transport Export", moveHeader2.PK, element1.PK);
			AssertEquals("Should be moveHeader4 as it's not awaiting response and it's Immediate Export", moveHeader4.PK, element2.PK);
		}

		public void TestReleaseAllInBondNumberMutex()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			Factory.Save();
			var sendingObjectCollection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.AirEntireInBondExportation, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			AssertEquals(3, sendingObjectCollection.Count);
			IInBondMessagingHeader moveHeader1 = sendingObjectCollection[0].moveHeader;
			IInBondMessagingHeader moveHeader2 = sendingObjectCollection[1].moveHeader;
			IInBondMessagingHeader moveHeader3 = sendingObjectCollection[2].moveHeader;
			moveHeader1.LockInBondNumberAllocationMutex();
			AssertEquals(true, moveHeader1.InBondNumberAllocationMutexHasLock());
			AssertEquals(false, moveHeader2.InBondNumberAllocationMutexHasLock());
			moveHeader3.LockInBondNumberAllocationMutex();
			AssertEquals(true, moveHeader3.InBondNumberAllocationMutexHasLock());
			sendingObjectCollection.ReleaseAllInBondNumberMutex();
			AssertEquals(false, moveHeader1.InBondNumberAllocationMutexHasLock());
			AssertEquals(false, moveHeader2.InBondNumberAllocationMutexHasLock());
			AssertEquals(false, moveHeader3.InBondNumberAllocationMutexHasLock());
		}

		public void TestCollectionCountWhenDeleteBillOfLading()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader01 = header.MovementHeaders.AddNew();
			moveHeader01.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var bill3 = header.Bills.AddNew();
			var bill4 = header.Bills.AddNew();
			var moveDetail1 = header.MovementDetails.AddNew();
			var moveDetail2 = header.MovementDetails.AddNew();
			var moveDetail3 = header.MovementDetails.AddNew();
			var moveDetail4 = header.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill1.PK;
			moveDetail1.B9_BM = moveHeader01.PK;
			moveDetail2.B9_B0 = bill2.PK;
			moveDetail2.B9_BM = moveHeader01.PK;
			moveDetail3.B9_B0 = bill3.PK;
			moveDetail3.B9_BM = moveHeader01.PK;
			moveDetail4.B9_B0 = bill4.PK;
			moveDetail4.B9_BM = moveHeader01.PK;
			bill1.B0_MessageStatus = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
			bill2.B0_MessageStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			bill2.LogManager.AddAClearLogIfNecessary(ZString.Empty, ImportMessageStatusList.Codes.ClearDepartureOriginal);
			bill3.B0_MessageStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			bill3.LogManager.AddAClearLogIfNecessary(ZString.Empty, ImportMessageStatusList.Codes.ClearDepartureOriginal);
			bill4.B0_MessageStatus = ZString.Empty;
			Factory.Save();
			var sendingObjectCollection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureBillDelete, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			AssertEquals(3, sendingObjectCollection.Count);
			var moveHeader02 = header.MovementHeaders.AddNew();
			var moveDetail5 = header.MovementDetails.AddNew();
			moveDetail5.B9_B0 = bill2.PK;
			moveDetail5.B9_BM = moveHeader02.PK;
			Factory.Save();
			sendingObjectCollection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureBillDelete, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			var list = sendingObjectCollection.ToList().Cast<InBondMessageSendingObject>();
			AssertNull("bill2 should not populated", list.FirstOrDefault(x => x.Bill.PK == bill2.PK));
		}

		public void TestSendDepartureBillDelete()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			header.BH_CarrierSCAC = "ABC";
			header.BH_ImportConveyanceName = "APL VESSEL";
			header.BH_VoyageNumber = "1234A";
			header.BH_ETA = new ZDateTime(2012, 10, 02, 18, 20, 10);
			header.BH_FTZMove = ZBool.False;
			header.BH_PortUnladingDCode = "5687";
			header.BH_FIRMS = "FOD3";
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			moveHeader1.InBondNumber = "123456789";
			moveHeader1.BM_InBondCarrierSCAC = "OTT1";
			moveHeader1.BM_DestinationPortCode = "1234";
			moveHeader1.BM_ForeignDestPortKCode = "12345";
			moveHeader1.BM_MonetaryValue = 323.25m;
			moveHeader1.BM_InBondCarrierID = "123456789012";
			moveHeader1.BM_BTAIndicator = US.Business.YesNoDefaultList.Codes.Yes;
			var moveHeader2 = header.MovementHeaders.AddNew();
			moveHeader2.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			moveHeader2.InBondNumber = "234567890";
			moveHeader2.BM_InBondCarrierSCAC = "OTT2";
			moveHeader2.BM_DestinationPortCode = "2345";
			moveHeader2.BM_ForeignDestPortKCode = "23456";
			moveHeader2.BM_MonetaryValue = 434.36m;
			moveHeader2.BM_InBondCarrierID = "234567890123";
			moveHeader1.BM_BTAIndicator = US.Business.YesNoDefaultList.Codes.Yes;
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "12345678901";
			bill1.B0_HouseBillNumber = "10987654321";
			bill1.B0_PortOfLadingKCode = "35987";
			bill1.B0_ManifestQty = 436;
			bill1.B0_ManifestUQ = ShippingOrPackingingUnitList.Codes.Aerosol;
			bill1.B0_Weight = 974.64m;
			bill1.B0_WeightUQ = Core.Constants.Weight.Pounds;
			bill1.B0_Volume = 3.54m;
			bill1.B0_VolumeUQ = VolumeUnitList.Codes.Cord;
			bill1.B0_PlaceOfReceiptDCode = "9853";
			bill1.B0_MessageStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			var bill2 = header.Bills.AddNew();
			bill2.B0_MasterBillNumber = "23456789012";
			bill2.B0_HouseBillNumber = "23412354652";
			bill2.B0_PortOfLadingKCode = "09876";
			bill2.B0_ManifestQty = 213;
			bill2.B0_ManifestUQ = ShippingOrPackingingUnitList.Codes.Aerosol;
			bill2.B0_Weight = 911.64m;
			bill2.B0_WeightUQ = Core.Constants.Weight.Pounds;
			bill2.B0_Volume = 3.24m;
			bill2.B0_VolumeUQ = VolumeUnitList.Codes.Cord;
			bill2.B0_PlaceOfReceiptDCode = "1543";
			bill2.B0_MessageStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			bill2.LogManager.AddAClearLogIfNecessary(ZString.Empty, ImportMessageStatusList.Codes.ClearDepartureOriginal);
			var bill3 = header.Bills.AddNew();
			bill3.B0_MasterBillNumber = "34567890123";
			bill3.B0_HouseBillNumber = "23412354123";
			bill3.B0_PortOfLadingKCode = "09123";
			bill3.B0_ManifestQty = 654;
			bill3.B0_ManifestUQ = ShippingOrPackingingUnitList.Codes.Aerosol;
			bill3.B0_Weight = 111.64m;
			bill3.B0_WeightUQ = Core.Constants.Weight.Pounds;
			bill3.B0_Volume = 2.24m;
			bill3.B0_VolumeUQ = VolumeUnitList.Codes.Cord;
			bill3.B0_PlaceOfReceiptDCode = "8769";
			bill3.B0_MessageStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			bill3.LogManager.AddAClearLogIfNecessary(ZString.Empty, ImportMessageStatusList.Codes.ClearDepartureOriginal);
			var moveDetail1 = moveHeader1.MovementDetails.AddNew(bill1.PK);
			moveDetail1.B9_InBoundQty = 124;
			var moveDetail2 = moveHeader1.MovementDetails.AddNew(bill2.PK);
			moveDetail2.B9_InBoundQty = 122;
			var moveDetail3 = moveHeader1.MovementDetails.AddNew(bill3.PK);
			moveDetail3.B9_InBoundQty = 132;
			var moveDetail4 = moveHeader2.MovementDetails.AddNew(bill1.PK);
			moveDetail4.B9_InBoundQty = 231;
			Factory.Save();
			var sendingObjectCollection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureBillDelete, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			var list = sendingObjectCollection.ToList().Cast<InBondMessageSendingObject>();
			var object1 = list.FirstOrDefault(x => x.Bill.PK == bill1.PK);
			var object2 = list.FirstOrDefault(x => x.Bill.PK == bill2.PK);
			var object3 = list.FirstOrDefault(x => x.Bill.PK == bill3.PK);
			var message2 = object2.Send();
			var message3 = object3.Send();
			var expectedMessage2 = @"B         QP                                               <<MSGNO PLACEHOLDER>>
10B62123456789   OTT112341234500000323123456789012 Y                            
30D 0001    23456789012     23412354652                                         
Y         QP";
			var expectedMessage3 = @"B         QP                                               <<MSGNO PLACEHOLDER>>
10B62123456789   OTT112341234500000323123456789012 Y                            
30D 0001    34567890123     23412354123                                         
Y         QP";
			CombineAssertions(() =>
			{
				AssertNull(object1);
				AssertEquals(2, sendingObjectCollection.Count);
				AssertEquals(expectedMessage2, message2.EM_FormattedMessageText);
				AssertEquals(expectedMessage3, message3.EM_FormattedMessageText);
				AssertEquals(Enterprise.Customs.US.Business.EM_MessageSubTypeList.Codes.InBondDepartureDelete, message2.EM_MessageSubType);
				AssertEquals(Enterprise.Customs.US.Business.EM_MessageSubTypeList.Codes.InBondDepartureDelete, message3.EM_MessageSubType);
				AssertEquals(CusInBondBill.Schema.TableName, message2.EM_LinkTable);
				AssertEquals(CusInBondBill.Schema.TableName, message3.EM_LinkTable);
				AssertEquals(bill2.PK, message2.EM_LinkUniqueID);
				AssertEquals(bill3.PK, message3.EM_LinkUniqueID);
			});
		}

		protected override Type GetExpectedCollectionType() => typeof(InBondMessageSendingObjectCollection);

		protected override InBondMessageSendingObjectCollection GetCollectionToTest()
		{
			var moveHeader = MoveHeader;
			Factory.Save();
			var result = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(moveHeader.BM_BH, InBondMessageType.DepartureAdd, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			result.HasChanges = false;
			return result;
		}

		protected override void TearDown()
		{
			header?.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
			Collection.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
			base.TearDown();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => new InBondMessageSendingObject(Header.MovementHeaders.AddNew(), InBondMessageType.DepartureAdd);

		CusInBondMoveHeader moveHeader;
		CusInBondMoveHeader MoveHeader => moveHeader ?? (moveHeader = Header.MovementHeaders.AddNew());

		CusInBondHeader header;
		CusInBondHeader Header => header ?? (header = Factory.New<CusInBondHeader>());
	}
}
