using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	internal abstract class SlotAllocationDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestTestingCorrectCollection()
		{
			AssertType("Expecting to test collection of type SlotAllocationDependentCollection", typeof(SlotAllocationDependentCollection), GetCollectionToTest());
		}

		public void TestCollectionIsARegisteredEditableChildObjectOfMaster()
		{
			var principal = Factory.NewWithValidTestData<OrgHeader>();
			SlotAllocationDependentCollection collection = NewCollection();
			SlotAllocation allocation = collection.GetAllocation(principal.PK);
			collection.Master.HasChanges = false;
			allocation.HasChanges = false;

			AssertEquals("Master should not be dirty", false, collection.Master.HasChanges);
			AssertEquals("Childh should not be dirty", false, allocation.HasChanges);

			allocation.E0_OverAllocationPercent = 5;

			AssertEquals("Master should be dirty", true, collection.Master.HasChanges);
		}

		public void TestGetAllocation()
		{
			SlotAllocationDependentCollection collection = NewCollection();

			AssertEquals("Collection should be empty", 0, collection.Count);

			SlotAllocation allocation1 = collection.GetAllocation(ZGuid.Empty);
			AssertEquals("Expecting E0_OH_Parent to be set.", ZGuid.Empty, allocation1.E0_OH_Principal);
			AssertEquals("Expecting the new element to not have any changes yet", false, allocation1.HasChanges);
			AssertEquals("Expecting a single element in the collection", 1, collection.Count);

			ZGuid pK = ZGuid.NewZGuid();
			SlotAllocation allocation2 = collection.GetAllocation(pK);
			AssertEquals("Expecting E0_OH_Parent to be set.", pK, allocation2.E0_OH_Principal);
			AssertEquals("Expecting the new element to not have any changes yet", false, allocation2.HasChanges);
			AssertEquals("Expecting a single element in the collection", 2, collection.Count);

			SlotAllocation allocation3 = collection.GetAllocation(pK);
			AssertSame("Expecting to get the same Allocation from the same PK", allocation2, allocation3);
			AssertEquals("Expecting no new allocations in the collection", 2, collection.Count);
		}

		public void TestRemoveAllocation()
		{
			SlotAllocationDependentCollection collection = NewCollection();

			ZGuid pK1 = ZGuid.NewZGuid();
			ZGuid pK2 = ZGuid.NewZGuid();

			SlotAllocation allocation1 = collection.GetAllocation(pK1);
			SlotAllocation allocation2 = collection.GetAllocation(pK2);

			collection.RemoveAllocation(pK1);
			AssertEquals("Allocation1 should be deleted", true, allocation1.IsDeleted);
			AssertEquals("Allocation2 should be unaffected", false, allocation2.IsDeleted);
		}

		public void TestSetRemoveCollectionRelationships()
		{
			SlotAllocationDependentCollection collection = NewCollection();
			SlotAllocation allocation = collection.AddNew();

			AssertEquals("Should have set E0_ParentID", collection.Master.PK, allocation.E0_ParentID);
			AssertEquals("Should have set E0_TableCode", TableCode, allocation.E0_ParentTableCode);

			collection.Remove(allocation);

			AssertEquals("Should have cleared E0_ParentID", ZGuid.Empty, allocation.E0_ParentID);
			AssertEquals("Should have cleared E0_TableCode", "", allocation.E0_ParentTableCode);

			SlotAllocationDependentCollection collection2 = NewCollection();
			collection2.Add(allocation);

			AssertEquals("Should have reset E0_ParentID", collection2.Master.PK, allocation.E0_ParentID);
			AssertEquals("Should have reset E0_TableCode", TableCode, allocation.E0_ParentTableCode);
		}

		public void TestDeleteChildrenWhenMasterIsDeleted()
		{
			SlotAllocationDependentCollection collection = NewCollection();
			SlotAllocation allocation = collection.AddNew();
			collection.Master.Delete();

			AssertEquals("Child should be deleted", true, allocation.IsDeleted);
		}

		public void TestSaveAndLoadInAnotherFactory()
		{
			ZGuid pK = Factory.NewWithValidTestData<OrgHeader>().PK;

			SlotAllocationDependentCollection collection = NewCollection();
			SlotAllocation allocation = collection.GetAllocation(pK);
			allocation.SetAspect("AS1", 50);

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BusinessObject master = factory2.Load(collection.Master.GetType(), collection.Master.PK);

			SlotAllocation allocation2 = factory2.Load<SlotAllocation>(allocation.PK);
			AssertNotNull(allocation2);

			SlotAllocationDependentCollection collection2 = GetCollectionFromMaster(master);
			AssertEquals("Collection should have an allocation", 1, collection2.Count);
			AssertSame("Should have the correct element in the collection", allocation2, collection2[0]);
		}

		#region Implementation

		protected abstract string TableCode { get; }

		SlotAllocationDependentCollection NewCollection()
		{
			return (SlotAllocationDependentCollection)GetCollectionToTest();
		}

		protected SlotAllocationDependentCollection GetCollectionFromMaster(BusinessObject master)
		{
			return (SlotAllocationDependentCollection)master.GetType().GetProperty("SlotAllocations").GetGetMethod().Invoke(master, null);
		}

		#endregion
	}
}
