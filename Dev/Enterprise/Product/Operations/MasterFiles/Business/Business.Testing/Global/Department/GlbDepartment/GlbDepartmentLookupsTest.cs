using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbDepartmentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestActivityList()
		{
			var activityList = new GlbDepartmentLookups(null).ActivityList;

			AssertEquals(10, activityList.Count);

			var expected =
				"C - " + (NoResString)"Customs" + "\n" +
				"D - " + (NoResString)"Depot CFS" + "\n" +
				"F - " + (NoResString)"Forwarding" + "\n" +
				"L - " + (NoResString)"Linehaul" + "\n" +
				"S - " + (NoResString)"Ships Agency" + "\n" +
				"T - " + (NoResString)"Port Transport" + "\n" +
				"W - " + (NoResString)"Warehouse" + "\n" +
				"G - " + (NoResString)"Gateway" + "\n" +
				"Y - " + (NoResString)"Container Yard" + "\n" +
				"M - " + (NoResString)"Miscellaneous";

			AssertMultilineASCIIEquals("", expected, activityList.ElementsAsString);
		}

		public void TestEachCodeDescriptionPairListHasItsNotesMapping()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var lookups = new GlbDepartmentLookups(department);

			AssertPairsHavePkOfType<StmNoteContextModule>(nameof(lookups.ActivityList), lookups.ActivityList);
			AssertPairsHavePkOfType<StmNoteContextDirection>(nameof(lookups.DirectionList), lookups.DirectionList);
			AssertPairsHavePkOfType<StmNoteContextFreightMode>(nameof(lookups.ShippingModeList), lookups.ShippingModeList);
			AssertPairsHavePkOfType<StmNoteContextFreightMode>(nameof(lookups.TransportModeList), lookups.TransportModeList);
		}

		void AssertPairsHavePkOfType<T>(string listName, CodeDescriptionPairList list)
		{
			CombineAssertions(() =>
			{
				foreach (ICodeDescription pair in list)
				{
					Assert($"Item with code {pair.Code} from {listName} should have a PK of type {typeof(T).Name}. If there isn't a perfect mapping, simply use 'All'", pair.PK is T);
				}
			});
		}

		public void TestTransportModeList()
		{
			var transportModeList = new GlbDepartmentLookups(null).TransportModeList;

			AssertEquals(6, transportModeList.Count);

			var expected =
				"A - " + (NoResString)"Air" + "\n" +
				"S - " + (NoResString)"Sea" + "\n" +
				"L - " + (NoResString)"Rail" + "\n" +
				"R - " + (NoResString)"Road" + "\n" +
				"P - " + (NoResString)"Post" + "\n" +
				"O - " + (NoResString)"Other";

			AssertMultilineASCIIEquals("", expected, transportModeList.ElementsAsString);
		}

		public void TestShippingModeList()
		{
			var expected =
				"C - " + (NoResString)"Containerized" + "\n" +
				"B - " + (NoResString)"B-Bulk" + "\n" +
				"V - " + (NoResString)"RORO/Vehicle" + "\n" +
				"U - " + (NoResString)"Bulk" + "\n" +
				"L - " + (NoResString)"Liquid Bulk" + "\n" +
				"D - " + (NoResString)"Detention" + "\n" +
				"A - " + (NoResString)"Voyage Accounting" + "";

			AssertMultilineASCIIEquals("", expected, new GlbDepartmentLookups(null).ShippingModeList.ElementsAsString);
		}

		public void TestDirectionList()
		{
			var directionList = new GlbDepartmentLookups(null).DirectionList;

			AssertEquals(4, directionList.Count);

			var expected =
				"E - " + (NoResString)"Export" + "\n" +
				"I - " + (NoResString)"Import" + "\n" +
				"D - " + (NoResString)"Domestic" + "\n" +
				"O - " + (NoResString)"Other";

			AssertMultilineASCIIEquals("", expected, directionList.ElementsAsString);
		}

		public void TestModeListByActivity()
		{
			GlbDepartmentLookups lookups = new GlbDepartmentLookups(null);
			string transportValues = lookups.TransportModeList.CodesAsString;
			string shippingValues = lookups.ShippingModeList.CodesAsString;

			AssertEquals("should return an empty list for undefined activities", "", lookups.ModeListByActivity("").CodesAsString);
			AssertEquals("should return the container list for ships agency", shippingValues, lookups.ModeListByActivity("S").CodesAsString);
			AssertEquals("should return the transport list for other activities", transportValues, lookups.ModeListByActivity("F").CodesAsString);
		}
	}
}
