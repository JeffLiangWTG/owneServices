namespace Enterprise.Freight.Business.Testing
{
	sealed class VoyageOriginDependentCollectionTest : VoyagePortsCollectionTest
	{
		protected override bool IsOriginPortsCollection
		{
			get { return true; }
		}

		public void TestGetOriginFromLoading()
		{
			var voyage = Factory.New<JobVoyage>();

			var origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUSYD";

			var origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "NZAKL";

			AssertEquals(origin1, voyage.Origins.GetOriginFromLoading("AUSYD"));
			AssertEquals(origin2, voyage.Origins.GetOriginFromLoading("NZAKL"));
			AssertEquals(null, voyage.Origins.GetOriginFromLoading("USLAX"));
		}
	}
}
