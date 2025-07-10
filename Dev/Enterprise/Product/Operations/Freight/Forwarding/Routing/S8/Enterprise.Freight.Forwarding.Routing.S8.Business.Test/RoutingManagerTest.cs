using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	[TestedType(typeof(RoutingManager))]
	public class RoutingManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCloneSelectedSchedules()
		{
			var testMessageLine = "100 SYD HKG 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  7437    332     0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 HKG 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);

			var manager = (RoutingManager)GetNewBusinessObject();
			var newManager = manager.CloneSelectedSchedules(new[] { header });

			AssertNotEquals(manager.Factory, newManager.Factory);
			AssertEquals(1, newManager.Routings.Count);

			var newHeader = newManager.Routings[0];
			AssertEquals("SYD", newHeader.Origin);
			AssertEquals("Sydney", newHeader.OriginDescription);
			AssertEquals("HKG", newHeader.Destination);
			AssertEquals("Hong Kong", newHeader.DestinationDescription);
		}

		public void TestCreateEnterpriseVoyages()
		{
			var query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "BA9437");
			var voyages = Factory.Load<IJobVoyage>(query);
			AssertEquals("Pre condition: the voyage with flight number BA9437 should not exist.", 0, voyages.Length);

			var testMessageLine = "100 SYD HKG 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  9437    332     0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 HKG 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);

			var manager = (RoutingManager)GetNewBusinessObject();
			var newManager = manager.CloneSelectedSchedules(new[] { header });

			AssertNotEquals(manager.Factory, newManager.Factory);
			AssertEquals(1, newManager.Routings.Count);

			var departureDate = new ZDateTime(2016, 10, 4);
			newManager.TryCreateEnterpriseVoyages(new List<ZDateTime>() { departureDate });

			var expectedMessage = @"
Sydney (SYD) - Hong Kong (HKG) 04-Oct-16 07:00 - 04-Oct-16 15:15
    Sydney (SYD) - Melbourne (MEL) BA9437 04-Oct-16 07:00 - 04-Oct-16 08:35
    Melbourne (MEL) - Hong Kong (HKG) BA4138 04-Oct-16 08:50 - 04-Oct-16 15:15
";
			AssertEquals(expectedMessage, string.Concat(newManager.RouteCreationLogs));

			newManager.Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newQuery = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "BA9437");
			var newVoyages = newFactory.Load<IJobVoyage>(newQuery);
			AssertEquals(1, newVoyages.Length);
		}

		public void TestRouteCreationLogs()
		{
			var testMessageLine1 = "000 SYD WUH 10:55   11:20   20:15 MU           2018/06/28 2018/10/06 1..4.6. <SYD 1 WUH     11:20   20:15 MU   750    332     0  5063                                        1..4.6. 18/06/28 18/10/06 J> ";
			var header1 = new RoutingResponseHeader(testMessageLine1, Factory);

			var testMessageLine2 = "000 SYD WUH 10:55   11:20   20:15 MU           2018/06/28 2018/10/06 1..4.6. <SYD 1 WUH     11:20   20:15 QF  5003    332     0  5063                                        1..4.6. 18/06/28 18/10/06 J> ";
			var header2 = new RoutingResponseHeader(testMessageLine2, Factory);

			var manager = (RoutingManager)GetNewBusinessObject();
			var newManager = manager.CloneSelectedSchedules(new[] { header1, header2 });

			AssertNotEquals(manager.Factory, newManager.Factory);
			AssertEquals(2, newManager.Routings.Count);

			var departureDate = new ZDateTime(2018, 7, 9);
			newManager.TryCreateEnterpriseVoyages(new List<ZDateTime>() { departureDate });

			var expectedMessage = @"
Sydney (SYD) - Wuhan Tianhe International Apt (WUH) 09-Jul-18 11:20 - 09-Jul-18 20:15
    Sydney (SYD) - Wuhan Tianhe International Apt (WUH) MU750 09-Jul-18 11:20 - 09-Jul-18 20:15

Sydney (SYD) - Wuhan Tianhe International Apt (WUH) 09-Jul-18 11:20 - 09-Jul-18 20:15
    Sydney (SYD) - Wuhan Tianhe International Apt (WUH) QF5003 09-Jul-18 11:20 - 09-Jul-18 20:15
";
			AssertEquals(expectedMessage, string.Concat(newManager.RouteCreationLogs));
		}

		public void TestNotCreateEnterpriseVoyagesIfDepartureDateIsEmpty()
		{
			var query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "BA9437");
			var voyages = Factory.Load<IJobVoyage>(query);
			AssertEquals("Pre condition: the voyage with flight number BA9437 should not exist.", 0, voyages.Length);

			var testMessageLine = "100 SYD HKG 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  9437    332     0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 HKG 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var header = new RoutingResponseHeader(testMessageLine, Factory);

			var manager = (RoutingManager)GetNewBusinessObject();
			var newManager = manager.CloneSelectedSchedules(new[] { header });

			AssertNotEquals(manager.Factory, newManager.Factory);
			AssertEquals(1, newManager.Routings.Count);

			newManager.TryCreateEnterpriseVoyages(new List<ZDateTime>() { ZDateTime.Empty });
			AssertEquals("", string.Concat(newManager.RouteCreationLogs));
			newManager.Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newQuery = new ZQuery(JobVoyageSchema.JV_VoyageFlight, "BA9437");
			var newVoyages = newFactory.Load<IJobVoyage>(newQuery);
			AssertEquals("No voyage is created when departure date is empty.", 0, newVoyages.Length);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RoutingManager(Factory);
		}

		#endregion
	}
}
