using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(RefDataGroupingFilterStripBusinessObject))]
	public class RefDataGroupingFilterStripBusinessObjectTests : ZArchitecture.Modules.Testing.FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefDataGroupingFilterStripBusinessObject();
		}

		public void TestRefDataGroupingFilters()
		{
			var parent = Factory.New<RefDataGrouping>();
			parent.ZZZ_Description = "Mr Bumble";
			parent.ZZZ_DataGrouping = "BUM";
			var child = Factory.New<RefDataGrouping>();
			child.ZZZ_ZZZ_Grouping = parent.PK;
			child.ZZZ_DataGrouping = "DC";
			child.ZZZ_Description = "Daniel";
			var orphan = Factory.New<RefDataGrouping>();
			orphan.ZZZ_DataGrouping = "OT";
			orphan.ZZZ_Description = "Oliver Twist";
			Factory.Save();
			var destinyTheStripper = new RefDataGroupingFilterStripBusinessObject();
			var groupFilter = (ModuleTextFilter)destinyTheStripper["Group"];
			groupFilter.IsActive = true;
			groupFilter.Property = "DC";
			var filter = destinyTheStripper.Filter;
			AssertEquals(true, child.MatchesFilter(filter));
			AssertEquals(false, parent.MatchesFilter(filter));
			AssertEquals(false, orphan.MatchesFilter(filter));
			destinyTheStripper = new RefDataGroupingFilterStripBusinessObject();
			var descFilter = (ModuleTextFilter)destinyTheStripper["Description"];
			descFilter.IsActive = true;
			descFilter.Property = "Mr Bumble";
			filter = destinyTheStripper.Filter;
			AssertEquals(false, child.MatchesFilter(filter));
			AssertEquals(false, orphan.MatchesFilter(filter));
			AssertEquals(true, parent.MatchesFilter(filter));
			destinyTheStripper = new RefDataGroupingFilterStripBusinessObject();
			var parentGroupFilter = (ModuleGuidFilter)destinyTheStripper["Parent Group"];
			parentGroupFilter.IsActive = true;
			parentGroupFilter.Property = parent.PK;
			filter = destinyTheStripper.Filter;
			AssertEquals(true, child.MatchesFilter(filter));
			AssertEquals(false, parent.MatchesFilter(filter));
			AssertEquals(false, orphan.MatchesFilter(filter));
		}
	}
}
