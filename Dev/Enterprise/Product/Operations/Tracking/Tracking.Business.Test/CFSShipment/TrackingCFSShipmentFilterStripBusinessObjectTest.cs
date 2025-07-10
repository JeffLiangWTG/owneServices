using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingCFSShipmentFilterStripBusinessObject))]
	sealed class TrackingCFSShipmentFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			testFilterStrip = (TrackingCFSShipmentFilterStripBusinessObject)GetNewBusinessObject();
		}

		#region Test Workflow Filters

		public void TestWorkflowFilters()
		{
			AssertNotNull("Filterstrip contains workflow filters", testFilterStrip["Milestone Date"]);
			AssertNotNull("Filterstrip contains workflow filters", testFilterStrip["Milestone Completed"]);
			AssertNotNull("Filterstrip contains workflow filters", testFilterStrip["Next Milestone"]);
			AssertNotNull("Filterstrip contains workflow filters", testFilterStrip["Last Completed Milestone"]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new TrackingCFSShipmentFilterStripBusinessObject();
		}

		TrackingCFSShipmentFilterStripBusinessObject testFilterStrip;

		#endregion
	}
}
