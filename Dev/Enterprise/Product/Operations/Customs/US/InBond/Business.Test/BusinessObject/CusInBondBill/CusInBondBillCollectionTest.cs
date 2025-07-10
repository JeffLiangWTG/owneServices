using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondBillCollection))]
	sealed class CusInBondBillCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondBillCollection>
	{
		public void TestSetDefaultsForNewElementCore()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_FIRMS = "W235";
			header.BH_FTZMove = true;
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.BM_InBondCarrierSCAC = "ABCD";
			var bill1 = header.Bills.AddNew();
			AssertEquals(bill1.B0_IssuerCode, "ABCD");
			AssertEquals(bill1.B0_PortOfLadingKCode, "99999");
			bill1.B0_PortOfLadingKCode = "88888";
			var bill2 = header.Bills.AddNew();
			AssertEquals(bill2.B0_IssuerCode, "ABCD");
			AssertEquals(bill2.B0_PortOfLadingKCode, "88888");
			bill2.B0_IssuerCode = "";
			bill2.B0_PortOfLadingKCode = "";
			header.BH_FTZMove = false;
			var bill3 = header.Bills.AddNew();
			AssertEquals(bill3.B0_IssuerCode, "");
			AssertEquals(bill3.B0_PortOfLadingKCode, "");
		}

		public void TestAllowNew()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentTableCode = shipment.TablePrefix;
			var collection = new CusInBondBillCollection(header);
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

		protected override CusInBondBillCollection GetCollectionToTest()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			return new CusInBondBillCollection(header);
		}
	}
}
