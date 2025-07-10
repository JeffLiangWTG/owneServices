using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgMatchApprovalFilterBusinessObject))]
	sealed class OrgMatchApprovalFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestUnmatchedByCurrentUser()
		{
			OrgMatchApprovalFilterBusinessObject filter = new OrgMatchApprovalFilterBusinessObject();
			((ModuleFlagsFilter)filter["Status"]).Property0 = true;
			((ModuleFlagsFilter)filter["Status"]).IsActive = true;

			OrgMatchApprovalCollection collection = new OrgMatchApprovalCollection(Factory);
			collection.Load(filter.Filter);

			AssertCollectionContains(UnmatchedByCurrentUser1, collection);
			AssertCollectionContains(UnmatchedByCurrentUser2, collection);
			AssertCollectionContains(NoStatus, collection);
			AssertCollectionNotContains(OneMatchComplete, collection);
			AssertCollectionNotContains(MatchConflict, collection);
			AssertCollectionNotContains(NoMatchFound, collection);
		}

		public void TestNoStatus()
		{
			OrgMatchApprovalFilterBusinessObject filter = new OrgMatchApprovalFilterBusinessObject();
			((ModuleFlagsFilter)filter["Status"]).Property0 = false;
			((ModuleFlagsFilter)filter["Status"]).Property1 = true;
			((ModuleFlagsFilter)filter["Status"]).IsActive = true;

			OrgMatchApprovalCollection collection = new OrgMatchApprovalCollection(Factory);
			collection.Load(filter.Filter);

			AssertCollectionNotContains(UnmatchedByCurrentUser1, collection);
			AssertCollectionNotContains(UnmatchedByCurrentUser2, collection);
			AssertCollectionContains(NoStatus, collection);
			AssertCollectionNotContains(OneMatchComplete, collection);
			AssertCollectionNotContains(MatchConflict, collection);
			AssertCollectionNotContains(NoMatchFound, collection);
		}

		public void TestOneMatchComplete()
		{
			OrgMatchApprovalFilterBusinessObject filter = new OrgMatchApprovalFilterBusinessObject();
			((ModuleFlagsFilter)filter["Status"]).Property0 = false;
			((ModuleFlagsFilter)filter["Status"]).Property2 = true;
			((ModuleFlagsFilter)filter["Status"]).IsActive = true;

			OrgMatchApprovalCollection collection = new OrgMatchApprovalCollection(Factory);
			collection.Load(filter.Filter);

			AssertCollectionContains(UnmatchedByCurrentUser1, collection);
			AssertCollectionContains(UnmatchedByCurrentUser2, collection);
			AssertCollectionNotContains(NoStatus, collection);
			AssertCollectionContains(OneMatchComplete, collection);
			AssertCollectionNotContains(MatchConflict, collection);
			AssertCollectionNotContains(NoMatchFound, collection);
		}

		public void TestMatchConflict()
		{
			OrgMatchApprovalFilterBusinessObject filter = new OrgMatchApprovalFilterBusinessObject();
			((ModuleFlagsFilter)filter["Status"]).Property0 = false;
			((ModuleFlagsFilter)filter["Status"]).Property3 = true;
			((ModuleFlagsFilter)filter["Status"]).IsActive = true;

			OrgMatchApprovalCollection collection = new OrgMatchApprovalCollection(Factory);
			collection.Load(filter.Filter);

			AssertCollectionNotContains(UnmatchedByCurrentUser1, collection);
			AssertCollectionNotContains(UnmatchedByCurrentUser2, collection);
			AssertCollectionNotContains(NoStatus, collection);
			AssertCollectionNotContains(OneMatchComplete, collection);
			AssertCollectionContains(MatchConflict, collection);
			AssertCollectionContains(MatchConflictWithOneNotMatchedValue, collection);
			AssertCollectionNotContains(NoMatchFound, collection);
		}

		public void TestNoMatchFound()
		{
			OrgMatchApprovalFilterBusinessObject filter = new OrgMatchApprovalFilterBusinessObject();
			((ModuleFlagsFilter)filter["Status"]).Property0 = false;
			((ModuleFlagsFilter)filter["Status"]).Property4 = true;
			((ModuleFlagsFilter)filter["Status"]).IsActive = true;

			OrgMatchApprovalCollection collection = new OrgMatchApprovalCollection(Factory);
			collection.Load(filter.Filter);

			AssertCollectionNotContains(UnmatchedByCurrentUser1, collection);
			AssertCollectionNotContains(UnmatchedByCurrentUser2, collection);
			AssertCollectionNotContains(NoStatus, collection);
			AssertCollectionNotContains(OneMatchComplete, collection);
			AssertCollectionNotContains(MatchConflict, collection);
			AssertCollectionContains(NoMatchFound, collection);
		}

		public void TestAllStatusesAtOnce()
		{
			OrgMatchApprovalFilterBusinessObject filter = new OrgMatchApprovalFilterBusinessObject();
			((ModuleFlagsFilter)filter["Status"]).Property1 = true;
			((ModuleFlagsFilter)filter["Status"]).Property2 = true;
			((ModuleFlagsFilter)filter["Status"]).Property3 = true;
			((ModuleFlagsFilter)filter["Status"]).Property4 = true;
			((ModuleFlagsFilter)filter["Status"]).IsActive = true;

			OrgMatchApprovalCollection collection = new OrgMatchApprovalCollection(Factory);
			collection.Load(filter.Filter);

			AssertCollectionContains(UnmatchedByCurrentUser1, collection);
			AssertCollectionContains(UnmatchedByCurrentUser2, collection);
			AssertCollectionContains(NoStatus, collection);
			AssertCollectionContains(OneMatchComplete, collection);
			AssertCollectionContains(MatchConflict, collection);
			AssertCollectionContains(NoMatchFound, collection);
		}

		public void TestReferenceNumberFilter()
		{
			DummyOrgMatchApproval matchApprovalWithTrackingNumber = NewDummyOrgMatchApproval();
			matchApprovalWithTrackingNumber.P2_Reference = "SomeReferenceNumber111";

			Factory.Save();

			//these should filter no records
			OrgMatchApprovalFilterBusinessObject filter = new OrgMatchApprovalFilterBusinessObject();
			((ModuleFlagsFilter)filter["Status"]).Property1 = false;
			((ModuleFlagsFilter)filter["Status"]).Property2 = false;
			((ModuleFlagsFilter)filter["Status"]).Property3 = true;
			((ModuleFlagsFilter)filter["Status"]).Property4 = false;
			((ModuleFlagsFilter)filter["Status"]).IsActive = true;
			//but this should override the above criteria
			((ModuleTextFilter)filter["Tracking Number"]).Property = "SomeReferenceNumber111";
			((ModuleTextFilter)filter["Tracking Number"]).IsActive = true;

			OrgMatchApprovalCollection collection = new OrgMatchApprovalCollection(Factory);
			collection.Load(filter.Filter);

			AssertCollectionContains(matchApprovalWithTrackingNumber, collection);
			AssertCollectionNotContains(UnmatchedByCurrentUser1, collection);
			AssertCollectionNotContains(UnmatchedByCurrentUser2, collection);
			AssertCollectionNotContains(NoStatus, collection);
			AssertCollectionNotContains(OneMatchComplete, collection);
			AssertCollectionNotContains(MatchConflict, collection);
			AssertCollectionNotContains(NoMatchFound, collection);
		}

		public void TestMasterBillFilter()
		{
			BusinessObject cusMAWB = (BusinessObject)Factory.New<Enterprise.Integration.Customs.AU.ICusMAWB>();
			BusinessObjectCollection houseBills = (BusinessObjectCollection)cusMAWB["ChildBills"];
			BusinessObject cusHAWB = houseBills.AddNew();
			OrgMatchApproval.Loader loader = new OrgMatchApproval.Loader(Factory);
			OrgMatchApproval matchApprovalWithMasterBill = loader.LoadOrCreate(cusHAWB.PK, OrgMatchApprovalType.AirCargoConsignee);

			cusMAWB[CusMAWBSchema.CM_MAWB.Name] = "MasterBill";

			Factory.Save();

			OrgMatchApprovalFilterBusinessObject filter = new OrgMatchApprovalFilterBusinessObject();
			((ModuleFlagsFilter)filter["Status"]).Property1 = true;
			((ModuleFlagsFilter)filter["Status"]).Property2 = true;
			((ModuleFlagsFilter)filter["Status"]).Property3 = true;
			((ModuleFlagsFilter)filter["Status"]).Property4 = true;
			((ModuleFlagsFilter)filter["Status"]).IsActive = true;
			((ModuleTextFilter)filter["Master Bill"]).Property = "splaty";
			((ModuleTextFilter)filter["Master Bill"]).IsActive = true;

			OrgMatchApprovalCollection collection = new OrgMatchApprovalCollection(Factory);
			collection.Load(filter.Filter);

			AssertCollectionNotContains(matchApprovalWithMasterBill, collection);
			AssertCollectionNotContains(UnmatchedByCurrentUser1, collection);
			AssertCollectionNotContains(UnmatchedByCurrentUser2, collection);
			AssertCollectionNotContains(NoStatus, collection);
			AssertCollectionNotContains(OneMatchComplete, collection);
			AssertCollectionNotContains(MatchConflict, collection);
			AssertCollectionNotContains(NoMatchFound, collection);

			((ModuleTextFilter)filter["Master Bill"]).Property = "MasterBill";
			((ModuleTextFilter)filter["Master Bill"]).IsActive = true;

			collection = new OrgMatchApprovalCollection(Factory);
			collection.Load(filter.Filter);

			AssertCollectionContains(matchApprovalWithMasterBill, collection);
			AssertCollectionNotContains(UnmatchedByCurrentUser1, collection);
			AssertCollectionNotContains(UnmatchedByCurrentUser2, collection);
			AssertCollectionNotContains(NoStatus, collection);
			AssertCollectionNotContains(OneMatchComplete, collection);
			AssertCollectionNotContains(MatchConflict, collection);
			AssertCollectionNotContains(NoMatchFound, collection);
		}

		public void TestSetDefaultValues()
		{
			OrgMatchApprovalFilterBusinessObject filter = new OrgMatchApprovalFilterBusinessObject();
			AssertEquals(((ModuleFlagsFilter)filter["Status"]).Property0, true);
			AssertEquals(((ModuleFlagsFilter)filter["Status"]).Property1, false);
			AssertEquals(((ModuleFlagsFilter)filter["Status"]).Property2, false);
			AssertEquals(((ModuleFlagsFilter)filter["Status"]).Property3, false);
			AssertEquals(((ModuleFlagsFilter)filter["Status"]).Property4, false);
		}

		public void TestRevertFilterToUnmatchedByCurrentUser()
		{
			OrgMatchApprovalFilterBusinessObject filter = new OrgMatchApprovalFilterBusinessObject();
			((ModuleFlagsFilter)filter["Status"]).Property0 = false;
			((ModuleFlagsFilter)filter["Status"]).Property2 = false;
			((ModuleFlagsFilter)filter["Status"]).IsActive = true;
			((ModuleTextFilter)filter["Master Bill"]).Property = "splaty";
			((ModuleTextFilter)filter["Master Bill"]).IsActive = true;

			filter.RevertFilterToUnmatchedByCurrentUser();

			AssertEquals(((ModuleFlagsFilter)filter["Status"]).Property0, true);
			AssertEquals(((ModuleFlagsFilter)filter["Status"]).Property1, false);
			AssertEquals(((ModuleFlagsFilter)filter["Status"]).Property2, false);
			AssertEquals(((ModuleFlagsFilter)filter["Status"]).Property3, false);
			AssertEquals(((ModuleFlagsFilter)filter["Status"]).Property4, false);
			AssertEquals(((ModuleTextFilter)filter["Tracking Number"]).Property, ZString.Empty);
		}

		public void TestIsUnmatchForCurrentUserOnly()
		{
			OrgMatchApprovalFilterBusinessObject filter = new OrgMatchApprovalFilterBusinessObject();
			((ModuleFlagsFilter)filter["Status"]).Property0 = true;
			((ModuleFlagsFilter)filter["Status"]).Property1 = true;
			((ModuleFlagsFilter)filter["Status"]).Property2 = true;
			((ModuleFlagsFilter)filter["Status"]).Property3 = true;
			((ModuleFlagsFilter)filter["Status"]).Property4 = true;
			((ModuleFlagsFilter)filter["Status"]).IsActive = true;
			((ModuleTextFilter)filter["Master Bill"]).Property = "splaty";
			((ModuleTextFilter)filter["Master Bill"]).IsActive = true;

			AssertEquals(filter.IsUnmatchForCurrentUserOnly, false);

			filter.RevertFilterToUnmatchedByCurrentUser();

			AssertEquals(filter.IsUnmatchForCurrentUserOnly, true);
		}

		/// Fixes the following problem where loading OrgPatternMatchAddress 'misses':
		/// C1: Begin Transaction
		/// C1: Save of OrgMatchApproval
		/// C2: Load of OrgMatchApproval (with nolock) finds it
		/// C2: Load of OrgPatternMatchAddress (locking) MISS
		/// C1: Save of OrgPatternMatchAddress
		/// C1: Commit Transaction
		public void TestDontLoadOrgMatchApprovalsWithOrphanedPatternMatchAddress()
		{
			OrgMatchApproval matchApproval = NewDummyOrgMatchApproval();
			matchApproval.P2_Reference = "me";

			Factory.Save();

			OrgMatchApprovalFilterBusinessObject filter = new OrgMatchApprovalFilterBusinessObject();
			((ModuleTextFilter)filter["Tracking Number"]).Property = "me";
			((ModuleTextFilter)filter["Tracking Number"]).IsActive = true;

			OrgMatchApprovalCollection collection = new OrgMatchApprovalCollection(Factory);
			collection.Load(filter.Filter);

			AssertCollectionContains(matchApproval, collection);

			matchApproval.AddressToBeMatched.Delete();

			Factory.Save();

			collection = new OrgMatchApprovalCollection(Factory);
			collection.Load(filter.Filter);

			AssertCollectionNotContains(matchApproval, collection);
		}

		public void TestDontLoadOrgMatchApprovalsWithOrphanedParents()
		{
			OrgMatchApproval matchApproval = NewDummyOrgMatchApproval();
			matchApproval.P2_Reference = "me";

			Factory.Save();

			OrgMatchApprovalFilterBusinessObject filter = new OrgMatchApprovalFilterBusinessObject();
			((ModuleTextFilter)filter["Tracking Number"]).Property = "me";
			((ModuleTextFilter)filter["Tracking Number"]).IsActive = true;

			OrgMatchApprovalCollection collection = new OrgMatchApprovalCollection(Factory);
			collection.Load(filter.Filter);

			AssertCollectionContains(matchApproval, collection);

			matchApproval.AddressToBeMatched.P3_ParentID = ZGuid.NewZGuid();
			Factory.Save();

			collection = new OrgMatchApprovalCollection(Factory);
			collection.Load(filter.Filter);

			AssertCollectionNotContains(matchApproval, collection);
		}

		public void TestReadOnlyWhenNotUserNotSupervisor()
		{
			TestOrgMatchApprovalFilterBusinessObject.IsCurrentUserSupervisorOverride = false;
			TestOrgMatchApprovalFilterBusinessObject matchUserFilterBizObj = new TestOrgMatchApprovalFilterBusinessObject();
			AssertEquals("A normal matching user cannot change the filter", true, matchUserFilterBizObj.ReadOnly);

			TestOrgMatchApprovalFilterBusinessObject.IsCurrentUserSupervisorOverride = true;
			TestOrgMatchApprovalFilterBusinessObject supervisorFilterBizObj = new TestOrgMatchApprovalFilterBusinessObject();
			AssertEquals("Only the supervisor can change the filter", false, supervisorFilterBizObj.ReadOnly);
		}

		public void TestMatchingUserCanOnlyGetItsOwnFilter()
		{
			TestOrgMatchApprovalFilterBusinessObject filterBizObj = new TestOrgMatchApprovalFilterBusinessObject();
			((ModuleTextFilter)filterBizObj["Master Bill"]).Property = "masterbill";
			((ModuleTextFilter)filterBizObj["Master Bill"]).IsActive = true;

			filterBizObj.Factory.Save();

			TestOrgMatchApprovalFilterBusinessObject.IsCurrentUserSupervisorOverride = false;
			TestOrgMatchApprovalFilterBusinessObject loadedFilterBizObj = new TestOrgMatchApprovalFilterBusinessObject();
			AssertEquals("The filter should always be the match user default when not supervisor", true, loadedFilterBizObj.IsUnmatchForCurrentUserOnly);
		}

		#region Test Classes

		class TestOrgMatchApprovalFilterBusinessObject : OrgMatchApprovalFilterBusinessObject
		{
			public static bool IsCurrentUserSupervisorOverride;
			public override bool IsCurrentUserSupervisor
			{
				get { return IsCurrentUserSupervisorOverride; }
			}
		}

		class DummyBusinessObjectAutoLogged : DummyEnterpriseBusinessObject
		{
			public DummyBusinessObjectAutoLogged(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override EnterpriseBusinessObject.AutologState AutoLoggingState => EnterpriseBusinessObject.AutologState.AutoLogged;
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new OrgMatchApprovalFilterBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(OrgMatchApproval.Schema.TableName);
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader otherOrganisation = Factory.NewWithValidTestData<OrgHeader>();

			UnmatchedByCurrentUser1 = NewDummyOrgMatchApproval();
			UnmatchedByCurrentUser1.Match(organisation.PK, "XXX");
			UnmatchedByCurrentUser2 = NewDummyOrgMatchApproval();
			UnmatchedByCurrentUser2.NotifyNoMatchFound("XXX");

			NoStatus = NewDummyOrgMatchApproval();

			OneMatchComplete = NewDummyOrgMatchApproval();
			OneMatchComplete.Match(organisation.PK);

			MatchConflict = NewDummyOrgMatchApproval();
			MatchConflict.Match(organisation.PK, "OP1");
			MatchConflict.Match(otherOrganisation.PK, "OP2");

			NoMatchFound = NewDummyOrgMatchApproval();
			NoMatchFound.NotifyNoMatchFound("OP1");
			NoMatchFound.NotifyNoMatchFound("OP2");

			MatchConflictWithOneNotMatchedValue = NewDummyOrgMatchApproval();
			MatchConflictWithOneNotMatchedValue.Match(organisation.PK, "OP1");
			MatchConflictWithOneNotMatchedValue.NotifyNoMatchFound("OP2");

			Factory.Save();
		}

		DummyOrgMatchApproval NewDummyOrgMatchApproval()
		{
			OrgMatchApproval.Loader loader = new OrgMatchApproval.Loader(Factory);
			DummyBusinessObjectAutoLogged dummyParent = Factory.New<DummyBusinessObjectAutoLogged>();
			DummyOrgMatchApproval matchApproval = (DummyOrgMatchApproval)loader.LoadOrCreate(dummyParent.PK, OrgMatchApprovalType.DummyType);
			return matchApproval;
		}

		DummyOrgMatchApproval UnmatchedByCurrentUser1;
		DummyOrgMatchApproval UnmatchedByCurrentUser2;
		DummyOrgMatchApproval NoStatus;
		DummyOrgMatchApproval OneMatchComplete;
		DummyOrgMatchApproval MatchConflict;
		DummyOrgMatchApproval NoMatchFound;
		DummyOrgMatchApproval MatchConflictWithOneNotMatchedValue;

		#endregion
	}
}
