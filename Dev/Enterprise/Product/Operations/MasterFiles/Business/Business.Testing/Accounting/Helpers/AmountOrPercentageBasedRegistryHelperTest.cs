using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class AmountOrPercentageBasedRegistryHelperTest : TestCaseWithFactory
	{
		public void TestGetApplicableSettings_ZeroAmount_ZeroPercantage()
		{
			AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			collection.Add(CreateNewSettings(0, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly));
			SetRegistryValue(collection);

			var setting = GetInstance().GetApplicableSettings(0.01M, 1000M);
			AssertNotNull("Setting found", setting);
			AssertEquals("Setting is correct", 0M, setting.Amount);
			AssertEquals("Setting is correct", AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, setting.Range);
		}

		public void TestGetApplicableSettings()
		{
			var setting = GetInstance().GetApplicableSettings(75M, 1000M);
			AssertNull(setting);

			AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			collection.Add(CreateNewSettings(50, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired));
			collection.Add(CreateNewSettings(100, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly));
			collection.Add(CreateNewSettings(150, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly));
			collection.Add(CreateNewSettings(150, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly));
			SetRegistryValue(collection);

			setting = GetInstance().GetApplicableSettings(75M, 1000M);
			AssertNotNull(setting);
			AssertEquals("Correct setting found", 100M, setting.Amount);
			AssertEquals("Correct setting found", AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, setting.Range);

			setting = GetInstance().GetApplicableSettings(150M, 1000M);
			AssertNotNull(setting);
			AssertEquals("Correct setting found", 150M, setting.Amount);
			AssertEquals("Correct setting found", AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, setting.Range);

			setting = GetInstance().GetApplicableSettings(200M, 1000M);
			AssertNotNull(setting);
			AssertEquals("Correct setting found", 150M, setting.Amount);
			AssertEquals("Correct setting found", AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, setting.Range);

			collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			collection.Add(CreateNewSettings(0, 5, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired));
			collection.Add(CreateNewSettings(0, 10, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly));
			collection.Add(CreateNewSettings(0, 15, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly));
			collection.Add(CreateNewSettings(0, 15, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly));
			SetRegistryValue(collection);

			setting = GetInstance().GetApplicableSettings(100M, 1000);
			AssertNotNull(setting);
			AssertEquals("Correct setting found", 10M, setting.Percentage);
			AssertEquals("Correct setting found", AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, setting.Range);

			setting = GetInstance().GetApplicableSettings(149.85M, 999M);
			AssertNotNull(setting);
			AssertEquals("Correct setting found", 15M, setting.Percentage);
			AssertEquals("Correct setting found", AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, setting.Range);

			setting = GetInstance().GetApplicableSettings(149.86M, 999M);
			AssertNotNull(setting);
			AssertEquals("Correct setting found", 15M, setting.Percentage);
			AssertEquals("Correct setting found", AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, setting.Range);
		}

		protected abstract AmountOrPercentageBasedRegistryHelper GetInstance();
		protected abstract void SetRegistryValue(AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection collection);

		AmountOrPercentageBasedThreeLevelAuthorisationRequirement CreateNewSettings(ZDecimal amount, ZDecimal percentage, ZString range, ZString auth)
		{
			var settings = new AmountOrPercentageBasedThreeLevelAuthorisationRequirement();
			settings.Amount = amount;
			settings.Percentage = percentage;
			settings.Range = range;
			settings.AuthorisationRequirement = auth;

			return settings;
		}
	}
}
