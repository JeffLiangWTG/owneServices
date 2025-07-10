using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GroupSecurityModuleFilter))]
	sealed class GroupSecurityModuleFilterTest : StaffSecurityModuleFilterTest
	{
		public override void TestGetQuery()
		{
			AssertEquals("", Filter.Query.LiteralTextADO);
			Filter.SecurityFilterContainer.LookupKey = new CheckpointLookupKey("ABC");
			AssertEquals("", Filter.Query.LiteralTextADO);
		}

		#region Implementation

		new GroupSecurityModuleFilter Filter
		{
			get { return (GroupSecurityModuleFilter)base.Filter; }
		}

		protected override ModuleGuidsFilter GetNewModuleFilter()
		{
			return new GroupSecurityModuleFilter("moo", Branches, Departments);
		}

		#endregion
	}
}
