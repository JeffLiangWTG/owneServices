using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(WorkflowModuleFilterWithRoutingSupport))]
	sealed class WorkflowModuleFilterWithRoutingSupportTest : ModuleFilterTestCase<WorkflowModuleFilterWithRoutingSupport>
	{
		#region Test Xml Serialise

		public void TestDeserializePropertiesFromToXml()
		{
			DummyFilterStripBusinessObject filterStripBizO = new DummyFilterStripBusinessObject();

			filterStripBizO.AddModuleFilterForTest(filterDate);

			FilterStrip strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filterDate.Description;

			filterDate.Origin = "moon";
			filterDate.Destination = "mars";

			var savedLayout = new DataGridLayoutManager().SavePreconfiguredLayout(filterStripBizO, "layoutName", false, false, SaveColumnLayout.Ignore);

			strip.FilterDescription = "";
			strip.Delete();

			WorkflowModuleFilterWithRoutingSupport loadedFilter = (WorkflowModuleFilterWithRoutingSupport)filterStripBizO[filterDate.Description];

			filterStripBizO.LoadLayout(savedLayout);

			AssertEquals("moon", loadedFilter.Origin);
			AssertEquals("mars", loadedFilter.Destination);
		}

		#endregion

		#region TestIsExpensiveQuery

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, filterDate.IsExpensiveQuery);
		}

		#endregion

		#region TestClearResetsToDefaults

		public void TestClearResetsToDefaults()
		{
			filterDate.Origin = "ABC";
			filterDate.Destination = "ABC";

			filterDate.Clear();
			AssertEquals("", filterDate.Origin);
			AssertEquals("", filterDate.Destination);
		}

		#endregion

		#region TestIsEmpty

		public void TestIsEmpty()
		{
			filterDate.Origin = "ABC";
			filterDate.Destination = "ABC";
			AssertEquals(false, filterDate.IsEmpty);

			filterDate.Clear();
			AssertEquals(true, filterDate.IsEmpty);
		}

		#endregion

		#region Properties

		public void TestOriginDestInfoReadOnly()
		{
			filterDate.MilestoneEvent = Events.ExWorks.Code;
			AssertEquals(true, filterDate.OriginInfo.ReadOnly);
			AssertEquals(true, filterDate.DestinationInfo.ReadOnly);

			filterDate.MilestoneEvent = "";
			AssertEquals(false, filterDate.OriginInfo.ReadOnly);
			AssertEquals(false, filterDate.DestinationInfo.ReadOnly);

			filterDate.MilestoneEvent = Events.Arrival.Code;
			AssertEquals(false, filterDate.OriginInfo.ReadOnly);
			AssertEquals(false, filterDate.DestinationInfo.ReadOnly);

			filterDate.MilestoneEvent = Events.Authorised.Code;
			AssertEquals(true, filterDate.OriginInfo.ReadOnly);
			AssertEquals(true, filterDate.DestinationInfo.ReadOnly);

			filterDate.MilestoneEvent = Events.Departure.Code;
			AssertEquals(false, filterDate.OriginInfo.ReadOnly);
			AssertEquals(false, filterDate.DestinationInfo.ReadOnly);
		}

		#endregion

		#region Implementation

		#region filter Getters

		static WorkflowModuleFilterWithRoutingSupport filterDate
		{
			get { return fFilterDate ?? (fFilterDate = new WorkflowModuleFilterWithRoutingSupport("date", typeof(DummyBusinessObject), WorkflowModuleFilterTypes.MilestoneDate, "", null, "")); }
		}

		static WorkflowModuleFilterWithRoutingSupport fFilterDate;

		#endregion

		protected override WorkflowModuleFilterWithRoutingSupport GetNewModuleFilter()
		{
			return new WorkflowModuleFilterWithRoutingSupport("moo", typeof(DummyBusinessObject), WorkflowModuleFilterTypes.MilestoneDate, "", null, "");
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Dates; }
		}

		#endregion
	}
}
