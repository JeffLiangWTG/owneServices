using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondCargoDescCollection))]
	sealed class CusInBondCargoDescCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondCargoDescCollection>
	{
		public void TestAllowNew()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentTableCode = shipment.TablePrefix;
			header.BH_FTZMove = true;
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			var collection = new CusInBondCargoDescCollection(container);
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
			header.Delete();
			AssertEquals(false, list.AllowNew);
		}

		public void TestTotalPieceCount()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			CusInBondContainer container = moveDetail.Containers.AddNew();
			CusInBondCargoDesc commodity1 = container.Commodities.AddNew();
			commodity1.BY_PieceCount = 10;
			AssertEquals(10, container.Commodities.TotalPieceCount);
			CusInBondCargoDesc commodity2 = container.Commodities.AddNew();
			commodity2.BY_PieceCount = 35;
			AssertEquals(45, container.Commodities.TotalPieceCount);
		}

		public void TestSetDefaultsForNewElementCore()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentTableCode = shipment.TablePrefix;
			header.BH_FTZMove = true;
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			bill.B0_WeightUQ = Core.Constants.Weight.Pounds;
			bill.B0_ManifestUQ = InBondManifestUQList.Codes.BDL;
			bill.B0_MasterBillNumber = "0012456";
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			var collection = new CusInBondCargoDescCollection(container);
			var cargo = collection.AddNew();
			AssertEquals(Core.Constants.Weight.Pounds, cargo.BY_GrossWeightUnit);
			AssertEquals(InBondManifestUQList.Codes.BDL, cargo.BY_ManifestUnitCode);
			AssertEquals(cargo.BY_Description, bill.B0_MasterBillNumber);
		}

		protected override CusInBondCargoDescCollection GetCollectionToTest()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			CusInBondContainer container = moveDetail.Containers.AddNew();
			return new CusInBondCargoDescCollection(container);
		}
	}
}
