using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgContactStmALogFilterBusinessObject))]
	public class OrgContactStmALogFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new OrgContactStmALogFilterBusinessObject();
		}

		public void TestHasFilters()
		{
			var filterBizO = new OrgContactStmALogFilterBusinessObject();
			AssertNotNull(filterBizO[ZStmALogFilterBusinessObject.Schema.EventCode]);
			AssertNotNull(filterBizO[ZStmALogFilterBusinessObject.Schema.EventTime]);
			AssertNotNull(filterBizO[ZStmALogFilterBusinessObject.Schema.PostedTime]);
			AssertNotNull(filterBizO[ZStmALogFilterBusinessObject.Schema.Reference]);
			AssertNotNull(filterBizO[ZStmALogFilterBusinessObject.Schema.User]);
			AssertNotNull(filterBizO["Contact Table"]);
			AssertNotNull(filterBizO["Contact Table"].Visibility == FilterVisibility.AlwaysAppliedAndHidden);
		}

		public void TestPostedTime()
		{
			var filterBizO = new OrgContactStmALogFilterBusinessObject();
			var filter = filterBizO[ZStmALogFilterBusinessObject.Schema.PostedTime] as ModuleDateFilter;

			AssertNotNull(filter);
			AssertEquals(FilterVisibility.AlwaysApplied | FilterVisibility.AlwaysVisible, filter.Visibility);
			filter.Validation.ValidateAll();

			AssertHasErrors(filter.PropertySearchInfo);
			AssertNoErrors(filter.Property1Info);
			AssertNoErrors(filter.Property2Info);

			filter.PropertySearch = "Date range";
			filter.Validation.ValidateAll();
			AssertNoErrors(filter.PropertySearchInfo);
			AssertHasErrors(filter.Property1Info);
			AssertHasErrors(filter.Property2Info);

			filter.Property1 = ZDateTime.Now;
			filter.Validation.ValidateAll();
			AssertNoErrors(filter.PropertySearchInfo);
			AssertNoErrors(filter.Property1Info);
			AssertHasErrors(filter.Property2Info);

			filter.Property2 = ZDateTime.Now;
			filter.Validation.ValidateAll();
			AssertNoErrors(filter.PropertySearchInfo);
			AssertNoErrors(filter.Property1Info);
			AssertNoErrors(filter.Property2Info);

			filter.PropertySearch = "Last 7 Days";
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			filter.Validation.ValidateAll();
			AssertNoErrors(filter.PropertySearchInfo);
			AssertNoErrors(filter.Property1Info);
			AssertNoErrors(filter.Property2Info);
		}
	}
}
