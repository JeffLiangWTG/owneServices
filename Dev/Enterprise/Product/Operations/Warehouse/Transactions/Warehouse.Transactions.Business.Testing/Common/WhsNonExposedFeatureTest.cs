using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Integration.Warehouse;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsNonExposedFeatureTest : TransactionedTestCase
	{
		public void TestAllFeaturesDisabledByDefaultForNonSupportUser()
		{
			AssertEquals("By default, feature 'None' should Not be disabled for support user.", true, ((IWhsNonExposedFeature)new WhsNonExposedFeature()).Enabled(WhsNonExposedFeatureName.None));

			GlbStaff.CurrentUser.GS_LoginName = "TestLoginName";
			foreach (var featureName in Enum.GetValues(typeof(WhsNonExposedFeatureName)))
			{
				AssertEquals($"By default, feature {featureName} should be disabled for non support user.", false, ((IWhsNonExposedFeature)new WhsNonExposedFeature()).Enabled((WhsNonExposedFeatureName)featureName));
			}
		}

		public void TestSchemaRedesignChangesFeature()
		{
			AssertEquals("By default, feature 'SchemaRedesignChanges' should be disabled.", false,
				((IWhsNonExposedFeature)new WhsNonExposedFeature()).Enabled(WhsNonExposedFeatureName.SchemaRedesignChanges));

			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("When relevant Registry is enabled, feature 'SchemaRedesignChanges' should be enabled.", true,
				((IWhsNonExposedFeature)new WhsNonExposedFeature()).Enabled(WhsNonExposedFeatureName.SchemaRedesignChanges));
			}
		}
	}
}
