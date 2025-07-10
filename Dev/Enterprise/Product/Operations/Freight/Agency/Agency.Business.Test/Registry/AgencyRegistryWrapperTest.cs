using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Freight;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyRegistryWrapperTest : TestCaseWithFactory
	{
		public void TestPostBothPrepaidAndCollectShipmentRevenueCharges()
		{
			AgencyRegistry.Instance.PostAllShipmentRevenueCharges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, Registry.PostBothPrepaidAndCollectShipmentRevenueCharges);
			AgencyRegistry.Instance.PostAllShipmentRevenueCharges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, Registry.PostBothPrepaidAndCollectShipmentRevenueCharges);
		}

		public void TestPostBothPrepaidAndCollectShipmentCostCharges()
		{
			AgencyRegistry.Instance.PostAllShipmentCostCharges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, Registry.PostBothPrepaidAndCollectShipmentCostCharges);
			AgencyRegistry.Instance.PostAllShipmentCostCharges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, Registry.PostBothPrepaidAndCollectShipmentCostCharges);
		}

		public void TestElectronicBookingAndShippingInstructions()
		{
			AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, Registry.ElectronicBookingAndShippingInstructions);
			AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, Registry.ElectronicBookingAndShippingInstructions);
		}

		public void TestUpdateEmptyReturnByWhenAvailabilityDatesChange()
		{
			AgencyRegistry.Instance.UpdateEmptyReturnByWhenAvailabilityDatesChange.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, Registry.UpdateEmptyReturnByWhenAvailabilityDatesChange.Value);
			AgencyRegistry.Instance.UpdateEmptyReturnByWhenAvailabilityDatesChange.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, Registry.UpdateEmptyReturnByWhenAvailabilityDatesChange.Value);
		}

		#region Implementation
		IAgencyRegistry Registry
		{
			get
			{
				return registry ?? (registry = new AgencyRegistryWrapper());
			}
		}

		IAgencyRegistry registry;
		#endregion
	}
}
