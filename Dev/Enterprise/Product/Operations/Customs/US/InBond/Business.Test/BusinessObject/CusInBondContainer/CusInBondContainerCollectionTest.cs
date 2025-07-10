using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondContainerCollection))]
	sealed class CusInBondContainerCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondContainerCollection>
	{
		public void TestAllowNew()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentTableCode = shipment.TablePrefix;
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var collection = new CusInBondContainerCollection(moveDetail);
			IBindingList list = collection;
			AssertEquals(true, list.AllowNew);
			header.BH_ParentID = shipment.PK;
			AssertEquals(false, list.AllowNew);
			header.BH_OverrideFreightDefaults = ZBool.True;
			AssertEquals(true, list.AllowNew);
			header.BH_OverrideFreightDefaults = ZBool.False;
			AssertEquals(false, list.AllowNew);
			var message = Factory.New<US.Business.MQEDIMessage>();
			header.MovementHeader.Messages.Add(message);
			AssertEquals(true, list.AllowNew);
		}

		public void TestResetPieceCountInContainerCollection()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var containers = moveDetail.Containers;
			var container1 = containers.AddNew();
			container1.BC_ContainerNum = "CONT1";
			container1.BC_PieceCount = 211;
			var container2 = containers.AddNew();
			container2.BC_ContainerNum = "CONT2";
			container2.BC_PieceCount = 212;
			containers.ResetPieceCount();
			AssertEquals(0, container1.BC_PieceCount);
			AssertEquals(0, container2.BC_PieceCount);
		}

		public void TestIndexerContainerNum()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var containers = moveDetail.Containers;
			var container1 = containers.AddNew();
			container1.BC_ContainerNum = "CONT1";
			var container2 = containers.AddNew();
			container2.BC_ContainerNum = "CONT2";
			var container3 = containers.AddNew();
			container3.BC_ContainerNum = "CONT3";
			AssertEquals(container2, containers["CONT2"]);
			AssertEquals(container1, containers["CONT1"]);
			AssertEquals(container3, containers["CONT3"]);
			AssertNull(containers["CONT4"]);
			AssertNull(containers[""]);
			container1.Delete();
			AssertNull(containers["CONT1"]);
		}

		public void TestTotalPieceCount()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var container1 = moveDetail.Containers.AddNew();
			var commodity1 = container1.Commodities.AddNew();
			commodity1.BY_PieceCount = 10;
			AssertEquals(10, moveDetail.Containers.TotalPieceCount);
			var commodity2 = container1.Commodities.AddNew();
			commodity2.BY_PieceCount = 35;
			AssertEquals(45, moveDetail.Containers.TotalPieceCount);
			var container2 = moveDetail.Containers.AddNew();
			var commodity3 = container2.Commodities.AddNew();
			commodity3.BY_PieceCount = 22;
			AssertEquals(67, moveDetail.Containers.TotalPieceCount);
		}

		public void TestContainerTotalPieceCount()
		{
			var importer = Factory.New<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FTZMove = true;
			var moveHeader = header.MovementHeaders.AddNew();
			AssertEquals(true, moveHeader.SupportsBondedWarehousing);
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var container1 = moveDetail.Containers.AddNew();
			var commodity1 = container1.Commodities.AddNew();
			commodity1.BY_PieceCount = 10;
			AssertEquals(10, moveDetail.Containers.TotalPieceCount);
			var commodity2 = container1.Commodities.AddNew();
			commodity2.BY_PieceCount = 35;
			AssertEquals(45, moveDetail.Containers.TotalPieceCount);
			var container2 = moveDetail.Containers.AddNew();
			var commodity3 = container2.Commodities.AddNew();
			commodity3.BY_PieceCount = 22;
			AssertEquals(67, moveDetail.Containers.TotalPieceCount);
			header.BH_FTZMove = false;
			container1.BC_PieceCount = 100;
			container2.BC_PieceCount = 300;
			AssertEquals(400, moveDetail.Containers.TotalPieceCount);
		}

		public void TestSetDefaultsForNewElement()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var container1 = moveDetail.Containers.AddNew();
			AssertEquals("", container1.BC_ContainerNum);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			moveDetail.Containers.DeleteAll();
			container1 = moveDetail.Containers.AddNew();
			AssertEquals(ZString.Empty, container1.BC_ContainerNum);
			var container2 = moveDetail.Containers.AddNew();
			AssertEquals(ZString.Empty, container2.BC_ContainerNum);
		}

		protected override CusInBondContainerCollection GetCollectionToTest()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			return new CusInBondContainerCollection(moveDetail);
		}
	}
}
