using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(VoyagePort))]
	class VoyagePortTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMessageHost()
		{
			var voyagePort = new VoyagePort(voyage, "AUMEL");
			voyagePort.Messages.Add(Factory.New<StowPlanMessage>());
			voyagePort.Origin.Messages.Load();
			AssertEquals(1, voyagePort.Origin.Messages.Count);
			voyagePort = new VoyagePort(voyage, "NZABY");
			voyagePort.Messages.Add(Factory.New<StowPlanMessage>());
			voyagePort.Destination.Messages.Load();
			AssertEquals(1, voyagePort.Destination.Messages.Count);
			voyagePort = new VoyagePort(voyage, "USLAX");
			voyagePort.Messages.Add(Factory.New<StowPlanMessage>());
			voyagePort.Origin.Messages.Load();
			voyagePort.Destination.Messages.Load();
			AssertEquals(1, voyagePort.Origin.Messages.Count);
			AssertEquals(0, voyagePort.Destination.Messages.Count);
			voyagePort.Origin.Messages.RemoveAll();
			voyagePort.Destination.Messages.Add(Factory.New<StowPlanMessage>());
			voyagePort.Messages.Add(Factory.New<StowPlanMessage>());
			voyagePort.Origin.Messages.Load();
			voyagePort.Destination.Messages.Load();
			AssertEquals(0, voyagePort.Origin.Messages.Count);
			AssertEquals(2, voyagePort.Destination.Messages.Count);
		}

		public void TestVoyagePortArrivalAndDeparture()
		{
			var voyagePort = new VoyagePort(voyage, "AUMEL");
			AssertEquals("AUMEL", voyagePort.Port);
			AssertEquals(ZDateTime.Today.AddDays(1), voyagePort.ArrivalTime);
			AssertEquals(ZDateTime.Today.AddDays(1), voyagePort.DepartureTime);
			voyagePort = new VoyagePort(voyage, "USLAX");
			AssertEquals("USLAX", voyagePort.Port);
			AssertEquals(ZDateTime.Today.AddDays(5), voyagePort.ArrivalTime);
			AssertEquals(ZDateTime.Today.AddDays(6), voyagePort.DepartureTime);
			voyagePort = new VoyagePort(voyage, "NZABY");
			AssertEquals(ZDateTime.Today.AddDays(10), voyagePort.ArrivalTime);
			AssertEquals(ZDateTime.Today.AddDays(10), voyagePort.DepartureTime);
		}

		JobVoyage voyage;
		protected override void SetUp()
		{
			base.SetUp();
			voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUMEL";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(1);
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			destination.JB_E_ARV = ZDateTime.Today.AddDays(5);
			origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "USLAX";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(6);
			destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZABY";
			destination.JB_E_ARV = ZDateTime.Today.AddDays(10);
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			return new VoyagePort(voyage, "USLAX");
		}
	}
}
