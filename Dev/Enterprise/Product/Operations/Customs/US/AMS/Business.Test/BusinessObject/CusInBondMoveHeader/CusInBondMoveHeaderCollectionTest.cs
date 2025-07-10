using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(CusInBondMoveHeaderCollection))]
	class CusInBondMoveHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondMoveHeaderCollection>
	{
		public void TestCollectionMembers()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentTableCode = shipment.TablePrefix;
			var collection = new CusInBondMoveHeaderCollection(header, SubApplicationCodeList.Codes.PermitToTransfer);
			IBindingList list = collection;
			AssertEquals(true, list.AllowNew);
			header.BH_OverrideFreightDefaults = ZBool.True;
			AssertEquals(true, list.AllowNew);
			AssertEquals(0, header.PTTMovements.Count);
			header.PTTMovements.AddNew();
			AssertEquals(1, header.PTTMovements.Count);
			AssertEquals(SubApplicationCodeList.Codes.PermitToTransfer, header.PTTMovements[0].BM_SubApplicationCode);
		}

		public void TestDefaultSupApplicationCode()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var moveHeader = header.InBondMovementHeaders.AddNew();
			AssertEquals("BM_SubApplicationCode", SubApplicationCodeList.Codes.SubsequentInBond, moveHeader.BM_SubApplicationCode);
			header.BH_TransitDirection = DirectionTypeList.Codes.MVOCC;
			moveHeader = header.InBondMovementHeaders.AddNew();
			AssertEquals("BM_SubApplicationCode", SubApplicationCodeList.Codes.MasterInBond, moveHeader.BM_SubApplicationCode);
		}

		public override void TestAddNew()
		{
			base.TestAddNew();
			var header = Factory.New<CusInBondHeader>();
			var collection = new CusInBondMoveHeaderCollection(header, SubApplicationCodeList.Codes.MasterInBond);
			var moveHeader = collection.AddNew();
			AssertEquals(SubApplicationCodeList.Codes.MasterInBond, moveHeader.BM_SubApplicationCode);
			collection = new CusInBondMoveHeaderCollection(header, SubApplicationCodeList.Codes.PermitToTransfer);
			moveHeader = collection.AddNew();
			AssertEquals(SubApplicationCodeList.Codes.PermitToTransfer, moveHeader.BM_SubApplicationCode);
		}

		public void TestStandaloneMasterInBondAreAttachedToInBondMovement()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterInBondIndicator = ZBool.False;
			bill1.B0_IssuerCode = "OTT1";
			bill1.B0_MasterBillNumber = "MB1";
			var bill2 = header.Bills.AddNew();
			bill2.B0_MasterInBondIndicator = ZBool.True;
			bill2.B0_IssuerCode = "OTT1";
			bill2.B0_MasterBillNumber = "MB2";
			var bill3 = header.Bills.AddNew();
			bill3.B0_MasterInBondIndicator = ZBool.True;
			bill3.B0_IssuerCode = "OTT2";
			bill3.B0_MasterBillNumber = "MB3";
			var bill4 = header.Bills.AddNew();
			bill4.B0_MasterInBondIndicator = ZBool.False;
			bill4.B0_IssuerCode = "OTT2";
			bill4.B0_MasterBillNumber = "MB4";
			var inbondMoveHeader = (CusInBondMoveHeader)((IBindingList)header.InBondMovementHeaders).AddNew();
			AssertEquals(0, inbondMoveHeader.MovementDetails.Count);
			((ICancelAddNew)header.InBondMovementHeaders).EndNew(0);
			Factory.Save();
			AssertEquals(2, inbondMoveHeader.MovementDetails.Count);
			AssertNotNull(inbondMoveHeader.MovementDetails.FindBillOfLading("OTT1", "MB2"));
			AssertNotNull(inbondMoveHeader.MovementDetails.FindBillOfLading("OTT2", "MB3"));
		}

		protected override CusInBondMoveHeaderCollection GetCollectionToTest()
		{
			var header = Factory.New<CusInBondHeader>();
			return new CusInBondMoveHeaderCollection(header, ZString.Empty);
		}
	}
}
