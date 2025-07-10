using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccComplianceSequenceFilterBusinessObject))]
	sealed class AccComplianceSequenceFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Filters

		[TestDate(2012, 09, 09)]
		public void TestEmptyFilter()
		{
			AccComplianceSequenceCollection collection = new AccComplianceSequenceCollection(Factory, filter.Filter);
			AssertCollectionContains(sequence1, collection);
			AssertCollectionContains(sequence2, collection);
		}

		[TestDate(2012, 09, 09)]
		public void TestSequenceClassFilter()
		{
			((ModuleTextFilter)filter["Sub type"]).Property = "TXI";
			((ModuleTextFilter)filter["Sub type"]).IsActive = true;
			AccComplianceSequenceCollection collection = new AccComplianceSequenceCollection(Factory, filter.Filter);
			AssertCollectionContains(sequence1, collection);
			AssertCollectionNotContains(sequence2, collection);
		}

		[TestDate(2012, 09, 09)]
		public void TestCodeFilter()
		{
			((ModuleTextFilter)filter["Code"]).Property = "LM2";
			((ModuleTextFilter)filter["Code"]).IsActive = true;
			AccComplianceSequenceCollection collection = new AccComplianceSequenceCollection(Factory, filter.Filter);

			AssertCollectionNotContains(sequence1, collection);
			AssertCollectionContains(sequence2, collection);
		}

		[TestDate(2012, 09, 09)]
		public void TestAllocationLevelFilter()
		{
			sequence1.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Company;
			sequence2.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.BranchDepartment;
			Factory.Save();

			var targetFilter = (ModuleTextFilter)filter["Allocation Level"];
			targetFilter.Property = Core.Constants.ComplianceBookAllocationLevel.Company;
			targetFilter.IsActive = true;
			var collection = new AccComplianceSequenceCollection(Factory, filter.Filter);

			AssertCollectionContains(sequence1, collection);
			AssertCollectionNotContains(sequence2, collection);
		}

		[TestDate(2012, 09, 09)]
		public void TestBranchFilter()
		{
			((ModuleGuidFilter)filter["Branch"]).Property = branch1.PK;
			((ModuleGuidFilter)filter["Branch"]).IsActive = true;
			AccComplianceSequenceCollection collection = new AccComplianceSequenceCollection(Factory, filter.Filter);
			AssertCollectionContains(sequence1, collection);
			AssertCollectionNotContains(sequence2, collection);
		}

		[TestDate(2012, 09, 09)]
		public void TestDepartmentFilter()
		{
			sequence1.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.BranchDepartment;
			sequence1.XD_GE_Department = department1.PK;
			sequence2.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.BranchDepartment;
			sequence2.XD_GE_Department = department2.PK;
			Factory.Save();

			var targetFilter = (ModuleGuidFilter)filter["Department"];
			targetFilter.Property = department1.PK;
			targetFilter.IsActive = true;
			var collection = new AccComplianceSequenceCollection(Factory, filter.Filter);

			AssertCollectionContains(sequence1, collection);
			AssertCollectionNotContains(sequence2, collection);
		}

		[TestDate(2012, 09, 09)]
		public void TestLockByFilter()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			sequence1.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Counter;
			sequence1.XD_LockBy = staff1.PK;
			sequence2.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Counter;
			sequence2.XD_LockBy = staff2.PK;
			Factory.Save();

			var targetFilter = (ModuleGuidFilter)filter["Lock By"];
			targetFilter.Property = staff1.PK;
			targetFilter.IsActive = true;
			var collection = new AccComplianceSequenceCollection(Factory, filter.Filter);

			AssertCollectionContains(sequence1, collection);
			AssertCollectionNotContains(sequence2, collection);
		}

		[TestDate(2012, 09, 09)]
		public void TestValidDateFilter()
		{
			((ModuleDateFilter)filter["Valid From"]).Property1 = ZDate.Empty;
			((ModuleDateFilter)filter["Valid From"]).Property2 = ZDate.Empty;
			((ModuleDateFilter)filter["Valid From"]).IsActive = true;
			((ModuleDateFilter)filter["Valid From"]).PropertySearch = "Date range";
			AccComplianceSequenceCollection collection = new AccComplianceSequenceCollection(Factory, filter.Filter);
			AssertCollectionContains(sequence1, collection);
			AssertCollectionContains(sequence2, collection);

			((ModuleDateFilter)filter["Valid From"]).Property1 = new ZDate(2012, 06, 01);
			((ModuleDateFilter)filter["Valid From"]).Property2 = new ZDate(2012, 12, 01);
			collection = new AccComplianceSequenceCollection(Factory, filter.Filter);
			AssertCollectionContains(sequence1, collection);
			AssertCollectionNotContains(sequence2, collection);

			((ModuleDateFilter)filter["Valid From"]).Property1 = new ZDate(2011, 01, 01);
			((ModuleDateFilter)filter["Valid From"]).Property2 = new ZDate(2012, 05, 01);
			collection = new AccComplianceSequenceCollection(Factory, filter.Filter);
			AssertCollectionNotContains(sequence1, collection);
			AssertCollectionContains(sequence2, collection);
		}

		[TestDate(2012, 09, 09)]
		public void TestExpiryDateFilter()
		{
			((ModuleDateFilter)filter["Expiry Date"]).Property1 = ZDateTime.Empty;
			((ModuleDateFilter)filter["Expiry Date"]).Property2 = ZDateTime.Empty;
			((ModuleDateFilter)filter["Expiry Date"]).IsActive = true;
			((ModuleDateFilter)filter["Expiry Date"]).PropertySearch = "Date range";
			AccComplianceSequenceCollection collection = new AccComplianceSequenceCollection(Factory, filter.Filter);
			AssertCollectionContains(sequence1, collection);
			AssertCollectionContains(sequence2, collection);

			((ModuleDateFilter)filter["Expiry Date"]).Property1 = new ZDateTime(2014, 01, 01);
			((ModuleDateFilter)filter["Expiry Date"]).Property2 = new ZDateTime(2015, 01, 01);
			collection = new AccComplianceSequenceCollection(Factory, filter.Filter);
			AssertCollectionContains(sequence1, collection);
			AssertCollectionNotContains(sequence2, collection);

			((ModuleDateFilter)filter["Expiry Date"]).Property1 = new ZDateTime(2012, 01, 01);
			((ModuleDateFilter)filter["Expiry Date"]).Property2 = new ZDateTime(2012, 12, 31);
			collection = new AccComplianceSequenceCollection(Factory, filter.Filter);
			AssertCollectionNotContains(sequence1, collection);
			AssertCollectionContains(sequence2, collection);
		}

		#endregion

		#region Implementation

		AccComplianceSequenceFilterBusinessObject filter;
		AccComplianceSequence sequence1, sequence2;
		GlbDepartment department1, department2;
		GlbBranch branch1, branch2;

		protected override void SetUp()
		{
			base.SetUp();
			branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch2 = Factory.NewWithValidTestData<GlbBranch>();
			department1 = Factory.NewWithValidTestData<GlbDepartment>();
			department2 = Factory.NewWithValidTestData<GlbDepartment>();
			sequence1 = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence1.XD_GC_Company = Env.CurrentCompany.PK;
			sequence1.XD_GB_BranchOwner = branch1.PK;
			sequence1.XD_SequenceClass = "TXI";
			sequence1.XD_Code = "LM1";
			sequence1.XD_Description = "this is TXI - LM1";
			sequence1.XD_StartDate = new ZDate(2012, 09, 01);
			sequence1.XD_ExpiryDate = new ZDateTime(2014, 12, 12);

			sequence2 = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence2.XD_GC_Company = Env.CurrentCompany.PK;
			sequence2.XD_GB_BranchOwner = branch2.PK;
			sequence2.XD_SequenceClass = "TCR";
			sequence2.XD_Code = "LM2";
			sequence2.XD_Description = "this is TCR - LM2";
			sequence2.XD_StartDate = new ZDate(2012, 01, 01);
			sequence2.XD_ExpiryDate = new ZDateTime(2012, 06, 06);

			Factory.Save();

			filter = (AccComplianceSequenceFilterBusinessObject)GetNewFilterStripBusinessObject();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AccComplianceSequenceFilterBusinessObject();
		}

		#endregion
	}
}
