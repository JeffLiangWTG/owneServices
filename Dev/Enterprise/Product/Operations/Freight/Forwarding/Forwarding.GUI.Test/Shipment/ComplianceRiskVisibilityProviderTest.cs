using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ComplianceRiskVisibilityProviderTest : TestCaseWithFactory
	{
		public void TestVisible()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var provider = new ShipmentDpsVisibilityProvider(() => true);
				AssertEquals(false, provider.Visible);

				provider = new ShipmentDpsVisibilityProvider(() => false);
				AssertEquals(false, provider.Visible);
			}

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var provider = new ShipmentDpsVisibilityProvider(() => true);
				AssertEquals(true, provider.Visible);

				provider = new ShipmentDpsVisibilityProvider(() => false);
				AssertEquals(false, provider.Visible);
			}
		}
	}
}
