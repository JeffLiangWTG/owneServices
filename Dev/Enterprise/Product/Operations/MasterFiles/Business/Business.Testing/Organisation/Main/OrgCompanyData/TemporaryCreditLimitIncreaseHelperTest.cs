using System;
using CargoWise.Application;
using Enterprise.Integration.Accounting;
using Enterprise.Registry.Business;
using AuthorisationRequirementCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;
using SettingNotFoundReason = Enterprise.MasterFiles.Business.TemporaryCreditLimitIncreaseHelper.SettingNotFoundReason;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class TemporaryCreditLimitIncreaseHelperTest : AmountOrPercentageBasedRegistryHelperTest
	{
		public void TestGetSettingForProposedIncrease()
		{
			SettingNotFoundReason reason;

			TemporaryCreditLimitIncreaseHelper helper = new TemporaryCreditLimitIncreaseHelper();
			var setting = helper.GetSettingForProposedIncrease(1000M, 75M, out reason);
			AssertNull(setting);
			AssertEquals(SettingNotFoundReason.RegistryNotConfigured, reason);

			var collection = new CreditTemporaryIncreaseAuthorisationSettingsCollection();
			collection.CreateCreditTemporaryIncreaseRequirement(50, 0, RangeCodes.UpTo, AuthorisationRequirementCodes.NoApprovalRequired, 5);
			collection.CreateCreditTemporaryIncreaseRequirement(100, 0, RangeCodes.UpTo, AuthorisationRequirementCodes.FirstApprovalRequiredOnly, 10);
			collection.CreateCreditTemporaryIncreaseRequirement(150, 0, RangeCodes.UpTo, AuthorisationRequirementCodes.SecondApprovalRequiredOnly, 15);
			collection.CreateCreditTemporaryIncreaseRequirement(150, 0, RangeCodes.Above, AuthorisationRequirementCodes.ThirdApprovalRequiredOnly, 20);
			collection.RunPreSaveValidation();
			AssertEquals(false, collection.HasNotifications());
			CreditLimitCheckTemporaryCreditLimitIncreaseThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			setting = helper.GetSettingForProposedIncrease(1000M, 75M);
			AssertNotNull(setting);
			AssertEquals("Correct setting found", 100M, setting.Amount);
			AssertEquals("Correct setting found", RangeCodes.UpTo, setting.Range);

			setting = helper.GetSettingForProposedIncrease(1000M, 150M);
			AssertNotNull(setting);
			AssertEquals("Correct setting found", 150M, setting.Amount);
			AssertEquals("Correct setting found", RangeCodes.UpTo, setting.Range);

			setting = helper.GetSettingForProposedIncrease(1000M, 200M);
			AssertNotNull(setting);
			AssertEquals("Correct setting found", 150M, setting.Amount);
			AssertEquals("Correct setting found", RangeCodes.Above, setting.Range);

			setting = helper.GetSettingForProposedIncrease(1000M, 0M, out reason);
			AssertNull(setting);
			AssertEquals(SettingNotFoundReason.IncreaseIsNegativeOrZero, reason);

			setting = helper.GetSettingForProposedIncrease(1000M, -1M, out reason);
			AssertNull(setting);
			AssertEquals(SettingNotFoundReason.IncreaseIsNegativeOrZero, reason);

			collection = new CreditTemporaryIncreaseAuthorisationSettingsCollection();
			collection.CreateCreditTemporaryIncreaseRequirement(0, 5, RangeCodes.UpTo, AuthorisationRequirementCodes.NoApprovalRequired, 5);
			collection.CreateCreditTemporaryIncreaseRequirement(0, 10, RangeCodes.UpTo, AuthorisationRequirementCodes.FirstApprovalRequiredOnly, 10);
			collection.CreateCreditTemporaryIncreaseRequirement(0, 15, RangeCodes.UpTo, AuthorisationRequirementCodes.SecondApprovalRequiredOnly, 15);
			collection.CreateCreditTemporaryIncreaseRequirement(0, 15, RangeCodes.Above, AuthorisationRequirementCodes.ThirdApprovalRequiredOnly, 20);
			collection.RunPreSaveValidation();
			AssertEquals(false, collection.HasNotifications());
			CreditLimitCheckTemporaryCreditLimitIncreaseThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			setting = helper.GetSettingForProposedIncrease(1000, 100);
			AssertNotNull(setting);
			AssertEquals("Correct setting found", 10M, setting.Percentage);
			AssertEquals("Correct setting found", RangeCodes.UpTo, setting.Range);

			setting = helper.GetSettingForProposedIncrease(999, 149.85);
			AssertNotNull(setting);
			AssertEquals("Correct setting found", 15M, setting.Percentage);
			AssertEquals("Correct setting found", RangeCodes.UpTo, setting.Range);

			setting = helper.GetSettingForProposedIncrease(999, 149.86);
			AssertNotNull(setting);
			AssertEquals("Correct setting found", 15M, setting.Percentage);
			AssertEquals("Correct setting found", RangeCodes.Above, setting.Range);

			setting = helper.GetSettingForProposedIncrease(1000M, 0M, out reason);
			AssertNull(setting);
			AssertEquals(SettingNotFoundReason.IncreaseIsNegativeOrZero, reason);

			setting = helper.GetSettingForProposedIncrease(1000M, -1M, out reason);
			AssertNull(setting);
			AssertEquals(SettingNotFoundReason.IncreaseIsNegativeOrZero, reason);
		}

		protected override void SetRegistryValue(AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection collection)
		{
			collection.RunPreSaveValidation();
			AssertEquals(false, collection.HasNotifications());

			CreditTemporaryIncreaseAuthorisationSettingsCollection creditTemporaryIncreaseAuthorisationSettingsCollection = new CreditTemporaryIncreaseAuthorisationSettingsCollection();
			foreach (AmountOrPercentageBasedThreeLevelAuthorisationRequirement item in collection)
			{
				creditTemporaryIncreaseAuthorisationSettingsCollection.CreateCreditTemporaryIncreaseRequirement(item.Amount, item.Percentage, item.Range, item.AuthorisationRequirement, 1);
			}

			CreditLimitCheckTemporaryCreditLimitIncreaseThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, creditTemporaryIncreaseAuthorisationSettingsCollection);
		}

		protected override AmountOrPercentageBasedRegistryHelper GetInstance()
		{
			return TemporaryCreditLimitIncreaseHelper.Instance;
		}

		CreditTemporaryIncreaseAuthorisationSettingsRegistryItem CreditLimitCheckTemporaryCreditLimitIncreaseThreshold
		{
			get { return ObjectFactory.Get<IAccounting>().Registry.CreditLimitCheckTemporaryCreditLimitIncreaseThreshold as CreditTemporaryIncreaseAuthorisationSettingsRegistryItem; }
		}
	}
}
