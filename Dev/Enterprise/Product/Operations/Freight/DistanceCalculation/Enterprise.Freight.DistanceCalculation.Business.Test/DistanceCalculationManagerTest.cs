using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Freight.DistanceCalculation.Integration;

namespace Enterprise.Freight.DistanceCalculation.Business.Testing
{
	sealed class DistanceCalculationManagerTest : TestCaseWithFactory
	{
		#region TestCalculate

		public void TestCalculate()
		{
			DistanceCalculationManager manager = new DistanceCalculationManager();
			DistanceCalculationResult result = manager.Calculate(Env.Security.RoadDistanceCalculationServiceForwarding, Guid.NewGuid(), DistanceCalculationConfig, OriginAddress, DestinationAddress);

			AssertEquals("Distance", 66.0, result.Distance); // sum of the letters in both addresses
			AssertEquals("DistanceUnits", DistanceCalculationConstants.UnitsForCalculation.Kilometres, result.DistanceUnit);
			AssertEquals("Travel time", 0.0, result.TravelTime);
			AssertEquals("Status message", "", result.StatusMessage);
		}

		#endregion

		#region TestNoSecurity

		public void TestNoSecurity()
		{
			var manager = new DistanceCalculationManager();
			var result = manager.Calculate(Env.Security.RoadDistanceCalculationServiceForwarding, Guid.NewGuid(), DistanceCalculationConfig, OriginAddress, DestinationAddress);
			AssertEquals("No errors", "", result.StatusMessage);

			Env.Security.RoadDistanceCalculationServiceForwarding.IsAllowed = false;
			result = manager.Calculate(Env.Security.RoadDistanceCalculationServiceForwarding, Guid.NewGuid(), DistanceCalculationConfig, OriginAddress, DestinationAddress);

			string errorSecurityMessage = string.Format(@"
You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{0}", Env.Security.RoadDistanceCalculationServiceForwarding.DisplayTextPathToSecurityRight).Trim();

			AssertEquals("Security check error", errorSecurityMessage, result.StatusMessage);
		}

		#endregion

		#region Implementation

		DistanceCalculationConfiguration DistanceCalculationConfig;
		DistanceCalculationAddress OriginAddress;
		DistanceCalculationAddress DestinationAddress;

		protected override void SetUp()
		{
			DistanceCalculationConfig = new DistanceCalculationConfiguration();
			DistanceCalculationConfig.UnitsForCalculation = DistanceCalculationConstants.UnitsForCalculation.Kilometres;

			OriginAddress = new DistanceCalculationAddress("Nowhere street", "", "Neverville", "AA", "1234", "Neverland");
			DestinationAddress = new DistanceCalculationAddress("Elm street", "", "City", "BB", "Code", "Country");

			base.SetUp();
		}

		#endregion
	}
}
