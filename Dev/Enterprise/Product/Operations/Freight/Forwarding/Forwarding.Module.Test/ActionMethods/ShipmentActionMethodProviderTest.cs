using System.Linq;
using CargoWise.Data.Testing;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(ShipmentActionMethodProvider))]
	public class ShipmentActionMethodProviderTest : OperationalActionMethodProviderTest
	{
		[UseSnapshotProtection(true)]
		public void TestNewMethods()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			{
				var methodTypes = Provider
					.NewMethods(new ForwardingShipmentSupporter())
					.Select(method => method.GetType());

				AssertEquals(3, methodTypes.Count());
				AssertCollectionContains(typeof(CalculateGreenhouseGasEmissionsActionMethod), methodTypes);
				AssertCollectionContains(typeof(UpdateCTStatusActionMethod), methodTypes);
				AssertCollectionContains(typeof(UpdateLastKnownTransitWarehouseActionMethod), methodTypes);
			}
		}

		[UseSnapshotProtection(true)]
		public void TestNewMethods_WhenGHGIsDisabled()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(false))
			{
				var methodTypes = Provider
					.NewMethods(new ForwardingShipmentSupporter())
					.Select(method => method.GetType());

				AssertEquals("Expecting 2 Shipment action methods when GHG registry value is false.", 2, methodTypes.Count());
				AssertCollectionNotContains("CalculateGreenhouseGasEmissionsActionMethod shouldn't be created when GHG calculation registry value is false.", typeof(CalculateGreenhouseGasEmissionsActionMethod), methodTypes);
			}
		}

		#region Implementation

		protected override ActionMethodProviderID ID
		{
			get { return ActionMethodProviderIDs.Shipment; }
		}

		#endregion
	}
}
