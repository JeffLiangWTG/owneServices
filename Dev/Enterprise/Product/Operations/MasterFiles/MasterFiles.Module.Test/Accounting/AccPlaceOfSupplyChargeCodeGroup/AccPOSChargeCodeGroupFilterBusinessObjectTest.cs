using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccPOSChargeCodeGroupFilterBusinessObject))]
	sealed class AccPOSChargeCodeGroupFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AccPOSChargeCodeGroupFilterBusinessObject();
		}

		ModuleTextFilter GetModuleTextFilter(ZString description)
		{
			return (ModuleTextFilter)(GetNewFilterStripBusinessObject()[description]);
		}

		public void TestCodeFilter()
		{
			var groups = new AccPOSChargeCodeGroupCollection(Factory.Load<GlbCompany>(Env.CurrentCompanyPK));
			groups.Load();
			groups.RemoveAndDeleteAll();

			var group1 = Factory.New<AccPOSChargeCodeGroup>();
			group1.GRO_Code = "AB";

			var group2 = Factory.New<AccPOSChargeCodeGroup>();
			group2.GRO_Code = "BA";

			ModuleTextFilter filter = GetModuleTextFilter("Code");
			filter.IsActive = true;

			groups = new AccPOSChargeCodeGroupCollection(Factory.Load<GlbCompany>(Env.CurrentCompanyPK));
			filter.Property = "A";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			groups.Load(filter.Query);
			AssertEquals("groups.Count", 2, groups.Count);
			AssertEquals("Should contain group1", true, groups.Contains(group1));
			AssertEquals("Should contain group2", true, groups.Contains(group2));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			groups.Load(filter.Query);
			AssertEquals("groups.Count", 1, groups.Count);
			AssertEquals("Should contain group1", true, groups.Contains(group1));
			AssertEquals("Should not contain group2", false, groups.Contains(group2));
		}

		public void TestDescriptionFilter()
		{
			var groups = new AccPOSChargeCodeGroupCollection(Factory.Load<GlbCompany>(Env.CurrentCompanyPK));
			groups.Load();
			groups.RemoveAndDeleteAll();

			var group1 = Factory.New<AccPOSChargeCodeGroup>();
			group1.GRO_Description = "AB";

			var group2 = Factory.New<AccPOSChargeCodeGroup>();
			group2.GRO_Description = "BA";

			ModuleTextFilter filter = GetModuleTextFilter("Description");
			filter.IsActive = true;

			groups = new AccPOSChargeCodeGroupCollection(Factory.Load<GlbCompany>(Env.CurrentCompanyPK));
			filter.Property = "A";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			groups.Load(filter.Query);
			AssertEquals("groups.Count", 2, groups.Count);
			AssertEquals("Should contain group1", true, groups.Contains(group1));
			AssertEquals("Should contain group2", true, groups.Contains(group2));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			groups.Load(filter.Query);
			AssertEquals("groups.Count", 1, groups.Count);
			AssertEquals("Should contain group1", true, groups.Contains(group1));
			AssertEquals("Should not contain group2", false, groups.Contains(group2));
		}

		#endregion
	}
}
