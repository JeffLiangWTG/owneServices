using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Tracking.Business.Testing
{
	public class ContainerHelperTest : TestCaseWithFactory
	{
		public void TestList()
		{
			var seaContainer = Factory.New<RefContainer>();
			seaContainer.RC_Code = "TST1";
			seaContainer.RC_ShippingMode = Constants.TransportModes.Sea;

			var airContainer = Factory.New<RefContainer>();
			airContainer.RC_Code = "TST2";
			airContainer.RC_ShippingMode = Constants.TransportModes.Air;

			var roadContainer = Factory.New<RefContainer>();
			roadContainer.RC_Code = "TST3";
			roadContainer.RC_ShippingMode = Constants.TransportModes.Road;

			var inactiveRoadContainer = Factory.New<RefContainer>();
			inactiveRoadContainer.RC_Code = "TST4";
			inactiveRoadContainer.RC_ShippingMode = Constants.TransportModes.Road;
			inactiveRoadContainer.RC_IsActive = false;

			var testHelper = new ContainerHelper(Factory);
			var testResults = testHelper.List(Constants.TransportModes.Sea);
			AssertNotEquals(0, testResults.Count);
			AssertCollectionContains(seaContainer, testResults);
			AssertCollectionNotContains(airContainer, testResults);
			AssertCollectionNotContains(roadContainer, testResults);
			AssertCollectionNotContains(inactiveRoadContainer, testResults);
			AssertContainersMode(testResults, Constants.TransportModes.Sea);

			testResults = testHelper.List(Constants.TransportModes.Air);
			AssertNotEquals(0, testResults.Count);
			AssertCollectionContains(airContainer, testResults);
			AssertCollectionNotContains(seaContainer, testResults);
			AssertCollectionNotContains(roadContainer, testResults);
			AssertCollectionNotContains(inactiveRoadContainer, testResults);
			AssertContainersMode(testResults, Constants.TransportModes.Air);

			testResults = testHelper.List(Constants.TransportModes.Road);
			AssertNotEquals(0, testResults.Count);
			AssertCollectionContains(roadContainer, testResults);
			AssertCollectionNotContains(inactiveRoadContainer, testResults);
			AssertCollectionNotContains(seaContainer, testResults);
			AssertCollectionNotContains(airContainer, testResults);
			AssertContainersMode(testResults, Constants.TransportModes.Road);

			testResults = testHelper.List(Constants.TransportModes.Rail);
			AssertNotEquals(0, testResults.Count);
			AssertCollectionContains(seaContainer, testResults);
			AssertCollectionNotContains(airContainer, testResults);
			AssertCollectionNotContains(roadContainer, testResults);
			AssertCollectionNotContains(inactiveRoadContainer, testResults);
			AssertContainersMode(testResults, Constants.TransportModes.Sea);
		}

		protected void AssertContainersMode(RefContainerCollection collection, ZString expectedMode)
		{
			foreach (RefContainer container in collection)
			{
				AssertEquals(expectedMode, container.RC_ShippingMode);
			}
		}
	}
}
