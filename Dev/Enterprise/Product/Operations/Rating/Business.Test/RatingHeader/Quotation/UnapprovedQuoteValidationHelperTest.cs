using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Rating.Business.Testing
{
	public class UnapprovedQuoteValidationHelperTest : LevelAuthorizationSecurityHelperTest<PaymentThreeLevelAuthorisationSettingsRegistryItem, PaymentThreeLevelAuthorisationSettingsCollection, PaymentThreeLevelAuthorisationSettings>
	{
		public override PaymentThreeLevelAuthorisationSettingsRegistryItem GetRegistryItem()
		{
			return DataRegistryRating.Instance.SpotQuoteApprovalSettings;
		}

		public override PaymentThreeLevelAuthorisationSettingsCollection PrepareAuthorisationSettingsCollection()
		{
			var valuesForTest = new PaymentThreeLevelAuthorisationSettingsCollection();
			upTo1000 = GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 1000, AuthorisationCodes.NoApprovalRequired);
			GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 2000, AuthorisationCodes.FirstApprovalRequiredOnly);
			GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 3000, AuthorisationCodes.SecondApprovalRequiredOnly);
			GetNewAuthorisationSetting(valuesForTest, RangeCodes.Above, 3000, AuthorisationCodes.ThirdApprovalRequiredOnly);
			return valuesForTest;
		}

		public override void TestGetSecurityCheckPoint()
		{
			var testAmount = upTo1000.Amount;

			var testCheckpoint = new UnapprovedQuoteValidationHelper().GetSecurityCheckPoint(-1000);
			AssertEquals(Env.Security.OneOffQuoteApproveOneOffQuotes.Code, testCheckpoint.Code);

			testCheckpoint = new UnapprovedQuoteValidationHelper().GetSecurityCheckPoint(0);
			AssertEquals(Env.Security.OneOffQuoteApproveOneOffQuotes.Code, testCheckpoint.Code);

			testCheckpoint = new UnapprovedQuoteValidationHelper().GetSecurityCheckPoint(testAmount - 1);
			AssertEquals(Env.Security.None.Code, testCheckpoint.Code);

			testCheckpoint = new UnapprovedQuoteValidationHelper().GetSecurityCheckPoint(testAmount);
			AssertEquals(Env.Security.None.Code, testCheckpoint.Code);

			testCheckpoint = new UnapprovedQuoteValidationHelper().GetSecurityCheckPoint(testAmount + 1);
			AssertEquals(Env.Security.OneOffQuoteFirstLevelApproval.Code, testCheckpoint.Code);

			testCheckpoint = new UnapprovedQuoteValidationHelper().GetSecurityCheckPoint(testAmount + 1000);
			AssertEquals(Env.Security.OneOffQuoteFirstLevelApproval.Code, testCheckpoint.Code);

			testCheckpoint = new UnapprovedQuoteValidationHelper().GetSecurityCheckPoint(testAmount + 1001);
			AssertEquals(Env.Security.OneOffQuoteSecondLevelApproval.Code, testCheckpoint.Code);

			testCheckpoint = new UnapprovedQuoteValidationHelper().GetSecurityCheckPoint(testAmount + 2000);
			AssertEquals(Env.Security.OneOffQuoteSecondLevelApproval.Code, testCheckpoint.Code);

			testCheckpoint = new UnapprovedQuoteValidationHelper().GetSecurityCheckPoint(testAmount + 2001);
			AssertEquals(Env.Security.OneOffQuoteThirdLevelApproval.Code, testCheckpoint.Code);

			testCheckpoint = new UnapprovedQuoteValidationHelper().GetSecurityCheckPoint(testAmount + 5000);
			AssertEquals(Env.Security.OneOffQuoteThirdLevelApproval.Code, testCheckpoint.Code);
		}

		AmountBasedMultiLevelAuthorisationRequirement upTo1000;
	}
}
