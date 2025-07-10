using System;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	class CreditControllerOverrideThresholdHelper : AmountOrPercentageBasedRegistryHelper
	{
		static readonly Lazy<CreditControllerOverrideThresholdHelper> instance = new Lazy<CreditControllerOverrideThresholdHelper>();

		public static CreditControllerOverrideThresholdHelper Instance
		{
			get { return instance.Value; }
		}

		protected override AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection RegistryValue
		{
			get
			{
				var registryItem = AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold;
				if (registryItem != null)
				{
					return registryItem.Value;
				}

				return null;
			}
		}
	}
}
