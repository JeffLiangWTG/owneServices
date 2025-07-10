using System;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CreditControllerOverrideGlobalThresholdHelperTest : AmountOrPercentageBasedRegistryHelperTest
	{
		protected override AmountOrPercentageBasedRegistryHelper GetInstance()
		{
			return CreditControllerOverrideGlobalThresholdHelper.Instance;
		}

		protected override void SetRegistryValue(AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection collection)
		{
			collection.RunPreSaveValidation();
			AssertEquals(false, collection.HasNotifications());
			GlobalCreditControllerOverrideThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryItem GlobalCreditControllerOverrideThreshold => AccountingMasterFilesRegistry.Instance.GlobalCreditControllerOverrideThreshold;
	}
}
