using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobSailingCollectionTest : TestCaseWithFactory
	{
		public void TestGetSailingFromLoadAndDischarge()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin o1 = Factory.New<VoyageOrigin>();
			o1.JA_RL_NKPortOfLoading = "AUSYD";
			o1.JA_E_DEP = ZDateTime.Today;
			voyage.Origins.Add(o1);
			VoyageDestination d1 = Factory.New<VoyageDestination>();
			d1.JB_RL_NKPortOfDischarge = "USLAX";
			d1.JB_E_ARV = ZDateTime.Today.AddDays(5);
			voyage.Destinations.Add(d1);

			AssertEquals("Precondition: expecting 1 sailing.", 1, voyage.Sailings.Count);

			AssertNotNull("Expecting to find SYD-LAX sailing.", voyage.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "USLAX"));
			AssertNull("Not expecting to find BNE-LAX sailing.", voyage.Sailings.GetSailingFromLoadAndDischarge("AUBNE", "USLAX"));
		}
	}
}
