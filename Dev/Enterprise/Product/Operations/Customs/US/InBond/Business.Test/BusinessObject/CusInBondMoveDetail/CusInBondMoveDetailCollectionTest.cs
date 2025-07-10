using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondMoveDetailCollection))]
	sealed class CusInBondMoveDetailCollectionTest : US.Business.Testing.CusInBondMoveDetailCollectionTest<CusInBondMoveDetailCollection>
	{
		public void TestSetDefaultsForNewElement()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselContainer;
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail1 = moveHeader.MovementDetails.AddNew();
			AssertEquals(0, moveDetail1.Containers.Count);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			CusInBondMoveDetail moveDetail2 = moveHeader.MovementDetails.AddNew();
			AssertEquals(1, moveDetail2.Containers.Count);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			CusInBondMoveDetail moveDetail3 = moveHeader.MovementDetails.AddNew();
			AssertEquals(0, moveDetail3.Containers.Count);
		}

		public void TestAllowNew()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentTableCode = shipment.TablePrefix;
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeader;
			var collection1 = new CusInBondMoveDetailCollection(bill);
			IBindingList list1 = collection1;
			var collection2 = new CusInBondMoveDetailCollection(moveHeader);
			IBindingList list2 = collection1;
			AssertEquals(true, list1.AllowNew);
			AssertEquals(true, list2.AllowNew);
			header.BH_ParentID = shipment.PK;
			AssertEquals(false, list1.AllowNew);
			AssertEquals(false, list2.AllowNew);
			header.BH_OverrideFreightDefaults = ZBool.True;
			AssertEquals(true, list1.AllowNew);
			AssertEquals(true, list2.AllowNew);
			header.BH_OverrideFreightDefaults = ZBool.False;
			AssertEquals(false, list1.AllowNew);
			AssertEquals(false, list2.AllowNew);
			var message = Factory.New<MQEDIMessage>();
			header.MovementHeader.Messages.Add(message);
			AssertEquals(true, list1.AllowNew);
			AssertEquals(true, list2.AllowNew);
		}

		public void TestAllowNewWithDeletedMoveHeader()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var collection = new CusInBondMoveDetailCollection(moveHeader);
			IBindingList list = collection;
			moveHeader.Delete();
			AssertNoExceptionThrown(() =>
			{
				var isAllow = list.AllowNew;
			});
			var bill = header.Bills.AddNew();
			collection = new CusInBondMoveDetailCollection(bill);
			list = collection;
			bill.Delete();
			AssertNoExceptionThrown(() =>
			{
				var isAllow = list.AllowNew;
			});
		}

		public void TestIsWaitingForResponseOrHasBeenReportedToCustoms()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondBill bill = header.Bills.AddNew();
			AssertEquals(false, bill.MoveDetails.IsWaitingForResponseOrHasBeenReportedToCustoms);
			CusInBondMoveHeader moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
			CusInBondMoveDetail moveDetail1 = moveHeader1.MovementDetails.AddNew();
			CusInBondMoveDetail moveDetail2 = moveHeader1.MovementDetails.AddNew();
			moveDetail2.B9_B0 = bill.PK;
			CusInBondMoveHeader moveHeader2 = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail3 = moveHeader2.MovementDetails.AddNew();
			moveDetail3.B9_B0 = bill.PK;
			AssertEquals(true, bill.MoveDetails.IsWaitingForResponseOrHasBeenReportedToCustoms);
			AssertEquals(true, moveHeader1.MovementDetails.IsWaitingForResponseOrHasBeenReportedToCustoms);
			AssertEquals(false, moveHeader2.MovementDetails.IsWaitingForResponseOrHasBeenReportedToCustoms);
			moveHeader1.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			AssertEquals(false, bill.MoveDetails.IsWaitingForResponseOrHasBeenReportedToCustoms);
			AssertEquals(false, moveHeader1.MovementDetails.IsWaitingForResponseOrHasBeenReportedToCustoms);
			AssertEquals(false, moveHeader2.MovementDetails.IsWaitingForResponseOrHasBeenReportedToCustoms);
			moveDetail3.B9_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			AssertEquals(true, bill.MoveDetails.IsWaitingForResponseOrHasBeenReportedToCustoms);
			AssertEquals(false, moveHeader1.MovementDetails.IsWaitingForResponseOrHasBeenReportedToCustoms);
			AssertEquals(true, moveHeader2.MovementDetails.IsWaitingForResponseOrHasBeenReportedToCustoms);
			moveDetail3.B9_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureWithdraw;
			AssertEquals(true, bill.MoveDetails.IsWaitingForResponseOrHasBeenReportedToCustoms);
			AssertEquals(false, moveHeader1.MovementDetails.IsWaitingForResponseOrHasBeenReportedToCustoms);
			AssertEquals(true, moveHeader2.MovementDetails.IsWaitingForResponseOrHasBeenReportedToCustoms);
			moveDetail3.B9_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureWithdraw;
			AssertEquals(false, bill.MoveDetails.IsWaitingForResponseOrHasBeenReportedToCustoms);
			AssertEquals(false, moveHeader1.MovementDetails.IsWaitingForResponseOrHasBeenReportedToCustoms);
			AssertEquals(false, moveHeader2.MovementDetails.IsWaitingForResponseOrHasBeenReportedToCustoms);
		}

		public void TestCanDeleteAll()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			AssertEquals(true, bill.MoveDetails.CanDeleteAll);
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
			var moveDetail1 = moveHeader1.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill2.PK;
			moveDetail1.B9_SeqNo = "";
			var moveDetail2 = moveHeader1.MovementDetails.AddNew();
			moveDetail2.B9_B0 = bill.PK;
			moveDetail2.B9_SeqNo = "1";
			var moveHeader2 = header.MovementHeaders.AddNew();
			var moveDetail3 = moveHeader2.MovementDetails.AddNew();
			moveDetail3.B9_B0 = bill.PK;
			moveDetail3.B9_SeqNo = "1";
			AssertEquals(false, bill.MoveDetails.CanDeleteAll);
			AssertEquals(true, bill2.MoveDetails.CanDeleteAll);
			AssertEquals(false, moveHeader1.MovementDetails.CanDeleteAll);
			AssertEquals(true, moveHeader2.MovementDetails.CanDeleteAll);
			Factory.Save();
			AssertEquals(false, bill.MoveDetails.CanDeleteAll);
			AssertEquals(true, bill2.MoveDetails.CanDeleteAll);
			AssertEquals(false, moveHeader1.MovementDetails.CanDeleteAll);
			AssertEquals(true, moveHeader2.MovementDetails.CanDeleteAll);
			moveHeader1.BM_CustomsStatus = ImportMessageStatusList.Codes.ErrorDepartureOriginal;
			AssertEquals(true, bill.MoveDetails.CanDeleteAll);
			AssertEquals(true, bill2.MoveDetails.CanDeleteAll);
			AssertEquals(true, moveHeader1.MovementDetails.CanDeleteAll);
			AssertEquals(true, moveHeader2.MovementDetails.CanDeleteAll);
			moveHeader1.MovementDetails.SetAllCustomsStatus(ImportMessageStatusList.Codes.ClearDepartureOriginal);
			AssertEquals(false, bill.MoveDetails.CanDeleteAll);
			AssertEquals(true, bill2.MoveDetails.CanDeleteAll);
			AssertEquals(false, moveHeader1.MovementDetails.CanDeleteAll);
			AssertEquals(true, moveHeader2.MovementDetails.CanDeleteAll);
			moveHeader1.MovementDetails.SetAllCustomsStatus(ImportMessageStatusList.Codes.AwaitingDepartureWithdraw);
			AssertEquals(false, bill.MoveDetails.CanDeleteAll);
			AssertEquals(true, bill2.MoveDetails.CanDeleteAll);
			AssertEquals(false, moveHeader1.MovementDetails.CanDeleteAll);
			AssertEquals(true, moveHeader2.MovementDetails.CanDeleteAll);
			moveHeader1.MovementDetails.SetAllCustomsStatus(ImportMessageStatusList.Codes.ClearDepartureWithdraw);
			AssertEquals(true, bill.MoveDetails.CanDeleteAll);
			AssertEquals(true, bill2.MoveDetails.CanDeleteAll);
			AssertEquals(true, moveHeader1.MovementDetails.CanDeleteAll);
			AssertEquals(true, moveHeader2.MovementDetails.CanDeleteAll);
			moveDetail3.B9_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			AssertEquals(false, bill.MoveDetails.CanDeleteAll);
			AssertEquals(true, bill2.MoveDetails.CanDeleteAll);
			AssertEquals(true, moveHeader1.MovementDetails.CanDeleteAll);
			AssertEquals(false, moveHeader2.MovementDetails.CanDeleteAll);
			moveDetail3.B9_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureWithdraw;
			AssertEquals(false, bill.MoveDetails.CanDeleteAll);
			AssertEquals(true, bill2.MoveDetails.CanDeleteAll);
			AssertEquals(true, moveHeader1.MovementDetails.CanDeleteAll);
			AssertEquals(false, moveHeader2.MovementDetails.CanDeleteAll);
			moveDetail3.B9_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureWithdraw;
			AssertEquals(true, bill.MoveDetails.CanDeleteAll);
			AssertEquals(true, bill2.MoveDetails.CanDeleteAll);
			AssertEquals(true, moveHeader1.MovementDetails.CanDeleteAll);
			AssertEquals(true, moveHeader2.MovementDetails.CanDeleteAll);
		}

		public void TestFindBillOfLading()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var bill1 = header.Bills.AddNew();
			bill1.B0_IssuerCode = "OTT1";
			bill1.B0_MasterBillNumber = "MB1";
			var moveDetail1 = moveHeader.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill1.PK;
			var bill2 = header.Bills.AddNew();
			bill2.B0_IssuerCode = "OTT1";
			bill2.B0_MasterBillNumber = "MB2";
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			moveDetail2.B9_B0 = bill2.PK;
			var bill3 = header.Bills.AddNew();
			bill3.B0_IssuerCode = "OTT2";
			bill3.B0_MasterBillNumber = "MB2";
			var moveDetail3 = moveHeader.MovementDetails.AddNew();
			moveDetail3.B9_B0 = bill3.PK;
			Factory.Save();
			var collection = moveHeader.MovementDetails;
			AssertEquals(moveDetail2, collection.FindBillOfLading("OTT1", "MB2"));
			AssertEquals(moveDetail1, collection.FindBillOfLading("OTT1", "MB1"));
			AssertEquals(moveDetail3, collection.FindBillOfLading("OTT2", "MB2"));
			AssertEquals(moveDetail1, collection.FindBillOfLading("", "MB1"));
			AssertNull(collection.FindBillOfLading("OTT1", "MB3"));
			AssertNull(collection.FindBillOfLading("OTT1", ""));
		}

		protected override US.Business.CusInBondMoveHeader CreateNewCusInBondMoveHeader(Customs.Business.CusInBondHeader header) => ((CusInBondHeader)header).MovementHeaders.AddNew();

		protected override Customs.Business.CusInBondHeader CreateNewCusInBondHeader() => Factory.New<CusInBondHeader>();

		protected override Customs.Business.CusInBondBill CreateNewCusInBondBill(Customs.Business.CusInBondHeader header) => ((CusInBondHeader)header).Bills.AddNew();

		protected override US.Business.CusInBondMoveDetail CreateNewCusInBondMoveDetail(US.Business.CusInBondMoveHeader moveHeader) => ((CusInBondMoveHeader)moveHeader).MovementDetails.AddNew();

		protected override CusInBondMoveDetailCollection GetCollectionToTest()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			return new CusInBondMoveDetailCollection(moveHeader);
		}
	}
}
