using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(WorkflowModuleTextFilterWithRoutingSupport))]
	sealed class WorkflowModuleTextFilterWithRoutingSupportTest : ModuleFilterTestCase<WorkflowModuleTextFilterWithRoutingSupport>
	{
		#region Test Xml Serialise

		public void TestDeserializePropertiesFromToXml()
		{
			DummyFilterStripBusinessObject filterStripBizO = new DummyFilterStripBusinessObject();

			filterStripBizO.AddModuleFilterForTest(Filter);

			FilterStrip strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = Filter.Description;

			Filter.Origin = "moon";
			Filter.Destination = "mars";

			var savedLayout = new DataGridLayoutManager().SavePreconfiguredLayout(filterStripBizO, "layoutName", false, false, SaveColumnLayout.Ignore);

			strip.FilterDescription = "";
			strip.Delete();

			WorkflowModuleTextFilterWithRoutingSupport loadedFilter = (WorkflowModuleTextFilterWithRoutingSupport)filterStripBizO[Filter.Description];

			filterStripBizO.LoadLayout(savedLayout);

			AssertEquals("moon", loadedFilter.Origin);
			AssertEquals("mars", loadedFilter.Destination);
		}

		#endregion

		#region TestIsExpensiveQuery

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		#endregion

		#region TestClearResetsToDefaults

		public void TestClearResetsToDefaults()
		{
			Filter.Origin = "ABC";
			Filter.Destination = "ABC";

			Filter.Clear();
			AssertEquals("", Filter.Origin);
			AssertEquals("", Filter.Destination);
		}

		#endregion

		#region TestIsEmpty

		public void TestIsEmpty()
		{
			Filter.Origin = "ABC";
			Filter.Destination = "ABC";
			AssertEquals(false, Filter.IsEmpty);

			Filter.Clear();
			AssertEquals(true, Filter.IsEmpty);
		}

		#endregion

		#region Properties

		public void TestOriginDestInfoReadOnly()
		{
			Filter.MilestoneEvent = Events.ExWorks.Code;
			AssertEquals(true, Filter.OriginInfo.ReadOnly);
			AssertEquals(true, Filter.DestinationInfo.ReadOnly);

			Filter.MilestoneEvent = "";
			AssertEquals(false, Filter.OriginInfo.ReadOnly);
			AssertEquals(false, Filter.DestinationInfo.ReadOnly);

			Filter.MilestoneEvent = Events.Arrival.Code;
			AssertEquals(false, Filter.OriginInfo.ReadOnly);
			AssertEquals(false, Filter.DestinationInfo.ReadOnly);

			Filter.MilestoneEvent = Events.Authorised.Code;
			AssertEquals(true, Filter.OriginInfo.ReadOnly);
			AssertEquals(true, Filter.DestinationInfo.ReadOnly);

			Filter.MilestoneEvent = Events.Departure.Code;
			AssertEquals(false, Filter.OriginInfo.ReadOnly);
			AssertEquals(false, Filter.DestinationInfo.ReadOnly);
		}

		#endregion

		#region Implementation

		protected override WorkflowModuleTextFilterWithRoutingSupport GetNewModuleFilter()
		{
			return new WorkflowModuleTextFilterWithRoutingSupport("moo", GetMilestoneTextFilter, new CodeDescriptionPairList(), typeof(DummyBusinessObject));
		}

		static ZQuery GetMilestoneTextFilter(ZString value)
		{
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.P9_ParentID);

			if (value == "rightvalue")
			{
				subQuery.AddToFilter(ProcessTasksSchema.P9_ActualDate, SQLComparisonOperator.NotEqual, ZDateTime.Empty);
			}
			else if (value == "wrongvalue")
			{
				subQuery.AddToFilter(ProcessTasksSchema.P9_ActualDate, SQLComparisonOperator.Equal, ZDateTime.Empty);
			}
			return subQuery;
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.TextSearch; }
		}

		#endregion
	}
}
