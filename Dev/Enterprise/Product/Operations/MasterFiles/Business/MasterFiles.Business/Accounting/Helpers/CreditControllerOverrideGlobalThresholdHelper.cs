using System;
using Enterprise.Registry.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	class CreditControllerOverrideGlobalThresholdHelper : AmountOrPercentageBasedRegistryHelper
	{
		public static CreditControllerOverrideGlobalThresholdHelper Instance => instance.Value;

		[ThreadSafe]
		static readonly Lazy<CreditControllerOverrideGlobalThresholdHelper> instance = new Lazy<CreditControllerOverrideGlobalThresholdHelper>(isThreadSafe: true);

		protected override AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection RegistryValue => AccountingMasterFilesRegistry.Instance.GlobalCreditControllerOverrideThreshold?.Value;
	}
}
