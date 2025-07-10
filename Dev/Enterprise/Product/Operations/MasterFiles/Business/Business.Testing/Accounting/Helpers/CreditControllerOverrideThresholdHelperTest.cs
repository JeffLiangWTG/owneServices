using System;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CreditControllerOverrideThresholdHelperTest : AmountOrPercentageBasedRegistryHelperTest
	{
		protected override AmountOrPercentageBasedRegistryHelper GetInstance()
		{
			return CreditControllerOverrideThresholdHelper.Instance;
		}

		protected override void SetRegistryValue(AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection collection)
		{
			collection.RunPreSaveValidation();
			AssertEquals(false, collection.HasNotifications());
			CreditControllerOverrideThreshold.SetValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, collection);
		}

		AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryItem CreditControllerOverrideThreshold
		{
			get { return AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold; }
		}
	}
}
