using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.DistanceCalculation.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DistanceCalculationListsTest : TestCaseWithFactory
	{
		public void TestProviders()
		{
			AssertEquals(2, DistanceCalculationLists.Instance.Providers.Count);
			AssertEquals("Contains CWS", true, DistanceCalculationLists.Instance.Providers.ContainsCode(DistanceCalculationConstants.Providers.CargoWise));
			AssertEquals("Contains PCM", true, DistanceCalculationLists.Instance.Providers.ContainsCode(DistanceCalculationConstants.Providers.PCMiler));
		}

		public void TestPCMilerVersions()
		{
			AssertEquals(8, DistanceCalculationLists.Instance.PCMilerVersions.Count);
		}

		public void TestPCMilerCalculationMethods()
		{
			AssertEquals(2, DistanceCalculationLists.Instance.PCMilerCalculationMethods.Count);
			AssertEquals("Contains practical", true, DistanceCalculationLists.Instance.PCMilerCalculationMethods.ContainsCode(DistanceCalculationConstants.CalculationMethods.PCMiler.Practical));
			AssertEquals("Contains shortest", true, DistanceCalculationLists.Instance.PCMilerCalculationMethods.ContainsCode(DistanceCalculationConstants.CalculationMethods.PCMiler.Shortest));
		}

		public void TestVersions()
		{
			AssertEquals(DistanceCalculationLists.Instance.PCMilerVersions.Count, DistanceCalculationLists.Instance.Versions(DistanceCalculationConstants.Providers.PCMiler).Count);
			AssertEquals(0, DistanceCalculationLists.Instance.Versions(DistanceCalculationConstants.Providers.CargoWise).Count);
		}

		public void TestMethods()
		{
			AssertEquals(DistanceCalculationLists.Instance.PCMilerCalculationMethods.Count, DistanceCalculationLists.Instance.CalculationMethods(DistanceCalculationConstants.Providers.PCMiler).Count);
			AssertEquals(0, DistanceCalculationLists.Instance.CalculationMethods(DistanceCalculationConstants.Providers.CargoWise).Count);
		}
	}
}
