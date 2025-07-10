using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.CN.Testing
{
	[TestedType(typeof(ETerminalReleaseMessageConsolCollection))]
	sealed class ETerminalReleaseMessageConsolCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ETerminalReleaseMessageConsolCollection>
	{
		public void TestLoad()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var consol3 = Factory.New<ForwardingConsol>();
			consol3.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var consol4 = Factory.New<ForwardingConsol>();
			consol4.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var voyage1 = CreateVoyage("CNNBO", "AUBNE");
			var voyage2 = CreateVoyage("CNNBG", "AUSYD");
			var voyage3 = CreateVoyage("CNNGB", "AUSYD");

			var transport1 = consol1.Transports.AddNew();
			var transport2 = consol2.Transports.AddNew();
			var transport3 = consol3.Transports.AddNew();
			var transport4 = consol4.Transports.AddNew();

			transport1.JW_IsLinked = true;
			transport2.JW_IsLinked = true;
			transport3.JW_IsLinked = true;
			transport4.JW_IsLinked = true;

			transport1.JW_JX = voyage1.Sailings[0].PK;
			transport2.JW_JX = voyage2.Sailings[0].PK;
			transport3.JW_JX = voyage3.Sailings[0].PK;
			transport4.JW_JX = voyage3.Sailings[0].PK;

			Factory.Save();

			var collection = new ETerminalReleaseMessageConsolCollection(voyage1);
			collection.Load();

			AssertContainsExactElementsInAnyOrder(new[] { consol1.PK }, collection.Cast<ETerminalReleaseMessageConsol>().Select(x => x.Consol.PK));

			collection = new ETerminalReleaseMessageConsolCollection(voyage2);
			collection.Load();

			AssertContainsExactElementsInAnyOrder(new[] { consol2.PK }, collection.Cast<ETerminalReleaseMessageConsol>().Select(x => x.Consol.PK));

			collection = new ETerminalReleaseMessageConsolCollection(voyage3);
			collection.Load();

			AssertContainsExactElementsInAnyOrder(new[] { consol3.PK, consol4.PK }, collection.Cast<ETerminalReleaseMessageConsol>().Select(x => x.Consol.PK));
		}

		JobVoyage CreateVoyage(string origin, string destination)
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "DI56";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = origin;
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = destination;
			voyage.GenerateSailings();

			return voyage;
		}

		protected override ETerminalReleaseMessageConsolCollection GetCollectionToTest()
		{
			return new ETerminalReleaseMessageConsolCollection(Factory.New<JobVoyage>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ETerminalReleaseMessageConsol(Factory.New<ForwardingConsol>());
		}
	}
}
