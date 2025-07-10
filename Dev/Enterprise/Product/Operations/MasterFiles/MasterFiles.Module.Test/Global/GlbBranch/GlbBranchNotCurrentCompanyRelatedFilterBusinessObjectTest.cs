using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbBranchNotCurrentCompanyRelatedFilterBusinessObject))]
	sealed class GlbBranchNotCurrentCompanyRelatedFilterBusinessObjectTest : GlbBranchFilterBusinessObjectTest
	{
		public new void TestEmptyFilter()
		{
			GlbBranchCollection collection1 = new GlbBranchCollection(Factory, branchFilter.Filter);
			collection1.Load();

			AssertEquals("Collection Contains Branch1", true, collection1.Contains(branch1.PK));
			AssertEquals("Collection Contains Branch2", true, collection1.Contains(branch2.PK));
		}

		public new void TestCompanyFilter()
		{
			ModuleGuidFilter companyFilter = (ModuleGuidFilter)branchFilter["Company"];
			companyFilter.Visibility = FilterVisibility.AlwaysApplied;
			companyFilter.Property = company1.PK;
			GlbBranchCollection collection2 = new GlbBranchCollection(Factory, branchFilter.Filter);
			collection2.Load();

			AssertEquals("Collection Contains Branch1", true, collection2.Contains(branch1.PK));
			AssertEquals("Collection should not contain Branch2", false, collection2.Contains(branch2.PK));

			((ModuleGuidFilter)branchFilter["Company"]).Property = ZGuid.Empty;
			GlbBranchCollection collection3 = new GlbBranchCollection(Factory, branchFilter.Filter);
			collection3.Load();
			AssertEquals("Collection Contains Branch1", true, collection3.Contains(branch1.PK));
			AssertEquals("Collection Contains Branch2", true, collection3.Contains(branch2.PK));
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GlbBranchNotCurrentCompanyRelatedFilterBusinessObject();
		}

		#endregion
	}
}
