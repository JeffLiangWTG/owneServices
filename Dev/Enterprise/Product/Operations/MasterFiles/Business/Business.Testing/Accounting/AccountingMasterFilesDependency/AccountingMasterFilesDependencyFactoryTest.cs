using CargoWise.Application;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Accounting.Testing
{
	sealed class AccountingMasterFilesDependencyFactoryTest : TestCaseWithFactory
	{
		public void TestType()
		{
			AssertType<AccountingMasterFilesDependencyFactory>(ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>());
		}

		public void TestGetTaxFrameworkConfigurationHelperDoesNotReturnNull()
		{
			AssertNotNull(ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper());
		}

		public void TestGetAccountingLogHelperDoesNotReturnNull()
		{
			AssertNotNull(ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetAccountingLogHelper());
		}

		public void TestGetChargeCodeOverrideRulesRankerDoesNotReturnNull()
		{
			AssertNotNull(ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetChargeCodeOverrideRulesRanker());
		}

		public void TestGetChargeComplianceDescriptionPostingHelperDoesNotReturnNull()
		{
			AssertNotNull(ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetChargeComplianceDescriptionPostingHelper());
		}
	}
}
