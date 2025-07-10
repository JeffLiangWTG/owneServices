using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbDepartmentAttributesForReportsTest : BusinessObjectLookupsTestCase
	{
		public void TestDepartmentAttributesChangesForReports()
		{
			//READ THIS BEFORE CHANGES
			//This test function is used to verify that the attribute definition remains aligned for reports.
			//Change this test function only after correcting the SQL DecodeDepartmentAttributes function.
			//The following lists must reflect what is configured in the SQL function DecodeDepartmentAttributes.
			var reportsActivityList =
				"C - Customs\n" +
				"D - Depot CFS\n" +
				"F - Forwarding\n" +
				"L - Linehaul\n" +
				"S - Ships Agency\n" +
				"T - Port Transport\n" +
				"W - Warehouse\n" +
				"G - Gateway\n" +
				"Y - Container Yard\n" +
				"M - Miscellaneous";

			var reportsTransportModeList =
				"A - Air\n" +
				"S - Sea\n" +
				"L - Rail\n" +
				"R - Road\n" +
				"P - Post\n" +
				"O - Other";

			var reportsShippingModeList =
				"C - Containerized\n" +
				"B - B-Bulk\n" +
				"V - RORO/Vehicle\n" +
				"U - Bulk\n" +
				"L - Liquid Bulk\n" +
				"D - Detention\n" +
				"A - Voyage Accounting";

			var reportsDirectionList =
				"E - Export\n" +
				"I - Import\n" +
				"D - Domestic\n" +
				"O - Other";

			var activityList = new GlbDepartmentLookups(null).ActivityList.ElementsAsString;
			var transportModeList = new GlbDepartmentLookups(null).TransportModeList.ElementsAsString;
			var shippingModeList = new GlbDepartmentLookups(null).ShippingModeList.ElementsAsString;
			var directionList = new GlbDepartmentLookups(null).DirectionList.ElementsAsString;

			var expected = $"{reportsActivityList}\n{reportsTransportModeList}\n{reportsShippingModeList}\n{reportsDirectionList}";
			var actual = $"{activityList}\n{transportModeList}\n{shippingModeList}\n{directionList}";

			AssertMultilineASCIIEquals("The definition of the department attributes has changed and does not match the one defined in the SQL function DecodeDepartmentAttributes./n The DecodeDepartmentAttributes function must be updated and aligned with the new attribute configuration.", expected, actual);
		}
	}
}
