using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	class TemporaryCreditLimitIncreaseHelper : AmountOrPercentageBasedRegistryHelper
	{
		public enum SettingNotFoundReason
		{
			NotApplicable,
			IncreaseIsNegativeOrZero,
			RegistryNotConfigured
		}

		static readonly Lazy<TemporaryCreditLimitIncreaseHelper> instance = new Lazy<TemporaryCreditLimitIncreaseHelper>();

		public static TemporaryCreditLimitIncreaseHelper Instance
		{
			get { return instance.Value; }
		}

		protected override AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection RegistryValue
		{
			get
			{
				var registryItem = ObjectFactory.Get<IAccounting>().Registry.CreditLimitCheckTemporaryCreditLimitIncreaseThreshold as CreditTemporaryIncreaseAuthorisationSettingsRegistryItem;
				if (registryItem != null)
				{
					return registryItem.Value;
				}

				return null;
			}
		}

		public CreditTemporaryIncreaseAuthorisationSettings GetSettingForProposedIncrease(ZDecimal creditLimit, ZDecimal proposedIncreaseAsCurrency)
		{
			SettingNotFoundReason settingNotFoundReason;
			return GetSettingForProposedIncrease(creditLimit, proposedIncreaseAsCurrency, out settingNotFoundReason);
		}

		public CreditTemporaryIncreaseAuthorisationSettings GetSettingForProposedIncrease(ZDecimal creditLimit, ZDecimal proposedIncreaseAsCurrency, out SettingNotFoundReason settingNotFoundReason)
		{
			settingNotFoundReason = SettingNotFoundReason.NotApplicable;

			if (proposedIncreaseAsCurrency <= 0)
			{
				settingNotFoundReason = SettingNotFoundReason.IncreaseIsNegativeOrZero;
				return null;
			}
			else if (RegistryValue == null || RegistryValue.Count == 0)
			{
				settingNotFoundReason = SettingNotFoundReason.RegistryNotConfigured;
				return null;
			}
			else
			{
				return (CreditTemporaryIncreaseAuthorisationSettings)GetApplicableSettings(proposedIncreaseAsCurrency, creditLimit);
			}
		}
	}
}
