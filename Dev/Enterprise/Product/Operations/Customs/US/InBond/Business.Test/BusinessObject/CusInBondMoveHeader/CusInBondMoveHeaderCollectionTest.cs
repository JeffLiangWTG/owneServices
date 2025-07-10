using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondMoveHeaderCollection))]
	sealed class CusInBondMoveHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondMoveHeaderCollection>
	{
		public void TestAllowNew()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentTableCode = shipment.TablePrefix;
			var collection = new CusInBondMoveHeaderCollection(header);
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

		public void TestUnLockAllInBondNumberAllocationMutex()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader1 = header.MovementHeaders.AddNew();
			var moveHeader2 = header.MovementHeaders.AddNew();
			AssertEquals(true, moveHeader2.LockInBondNumberAllocationMutex());
			var moveHeader3 = header.MovementHeaders.AddNew();
			AssertEquals(true, moveHeader3.LockInBondNumberAllocationMutex());
			header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
			AssertEquals(false, moveHeader1.InBondNumberAllocationMutexHasLock());
			AssertEquals(false, moveHeader2.InBondNumberAllocationMutexHasLock());
			AssertEquals(false, moveHeader3.InBondNumberAllocationMutexHasLock());
		}

		public void TestUniqeInBondCarrierIDList()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeaderCollection collection = header.MovementHeaders;
			AssertEquals(0, collection.UniqeInBondCarrierIDList.Count);
			CusInBondMoveHeader moveHeader1 = collection.AddNew();
			AssertEquals(0, collection.UniqeInBondCarrierIDList.Count);
			moveHeader1.BM_InBondCarrierID = "ID1";
			AssertEquals(1, collection.UniqeInBondCarrierIDList.Count);
			AssertEquals("ID1", collection.UniqeInBondCarrierIDList[0]);
			CusInBondMoveHeader moveHeader2 = collection.AddNew();
			AssertEquals(1, collection.UniqeInBondCarrierIDList.Count);
			AssertEquals("ID1", collection.UniqeInBondCarrierIDList[0]);
			moveHeader2.BM_InBondCarrierID = "ID1";
			AssertEquals(1, collection.UniqeInBondCarrierIDList.Count);
			AssertEquals("ID1", collection.UniqeInBondCarrierIDList[0]);
			moveHeader2.BM_InBondCarrierID = "ID2";
			AssertEquals(2, collection.UniqeInBondCarrierIDList.Count);
			AssertCollectionContains("ID1", collection.UniqeInBondCarrierIDList);
			AssertCollectionContains("ID2", collection.UniqeInBondCarrierIDList);
		}

		protected override CusInBondMoveHeaderCollection GetCollectionToTest()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			return new CusInBondMoveHeaderCollection(header);
		}
	}
}
