namespace Enterprise.Freight.Business.Testing
{
	sealed class VoyageDestinationDependentCollectionTest : VoyagePortsCollectionTest
	{
		protected override bool IsOriginPortsCollection
		{
			get { return false; }
		}

		public void TestGetDestinationFromDischarge()
		{
			var voyage = Factory.New<JobVoyage>();

			var destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "AUSYD";

			var destination2 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "NZAKL";

			AssertEquals(destination1, voyage.Destinations.GetDestinationFromDischarge("AUSYD"));
			AssertEquals(destination2, voyage.Destinations.GetDestinationFromDischarge("NZAKL"));
			AssertEquals(null, voyage.Destinations.GetDestinationFromDischarge("USLAX"));
		}
	}
}
