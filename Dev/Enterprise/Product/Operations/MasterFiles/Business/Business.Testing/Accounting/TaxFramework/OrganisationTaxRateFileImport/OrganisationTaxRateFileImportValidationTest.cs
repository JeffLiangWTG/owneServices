using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrganisationTaxRateFileImport))]
	sealed class OrganisationTaxRateFileImportValidationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLoadTaxConfiguration()
		{
			var orgTaxRate = GetNewBusinessObject() as OrganisationTaxRateFileImport;
			AssertEquals("default", ZGuid.Empty, orgTaxRate.TaxConfiguration);

			orgTaxRate.TaxConfiguration = ZGuid.Invalid;
			AssertHasError(orgTaxRate.TaxConfigurationInfo, "Enter a valid Tax Configuration.");

			using (orgTaxRate.GetValidationSuspender())
			{
				orgTaxRate.TaxConfiguration = ZGuid.Empty;
			}
			AssertHasError(orgTaxRate.TaxConfigurationInfo, "Enter a valid Tax Configuration.");
			orgTaxRate.RunPreSaveValidation();
			AssertHasError(orgTaxRate.TaxConfigurationInfo, "Please enter a Tax Configuration.");

			var taxTestHelper = new AccountingTestObjectCreator(Factory);
			taxTestHelper.ConfigureTaxFrameworkAtCompanyLevel(GlbCompany.CurrentCompany, taxRateSource: TaxRateSources.OrganisationOnly.Code);

			orgTaxRate = GetNewBusinessObject() as OrganisationTaxRateFileImport;
			var collection = orgTaxRate.TaxConfigurations;
			orgTaxRate.TaxConfiguration = collection[0].PK;

			AssertNoErrors(orgTaxRate.TaxConfigurationInfo);
		}

		public void TestLoadRateSource()
		{
			var orgTaxRate = GetNewBusinessObject() as OrganisationTaxRateFileImport;
			AssertEquals("default", "", orgTaxRate.RateSource);

			orgTaxRate.RateSource = "---";
			AssertHasError(orgTaxRate.RateSourceInfo, "Enter a valid Rate Source.");

			using (orgTaxRate.GetValidationSuspender())
			{
				orgTaxRate.RateSource = "";
			}
			AssertHasError(orgTaxRate.RateSourceInfo, "Enter a valid Rate Source.");
			orgTaxRate.RunPreSaveValidation();
			AssertHasError(orgTaxRate.RateSourceInfo, "Please enter a Rate Source.");

			orgTaxRate = GetNewBusinessObject() as OrganisationTaxRateFileImport;
			orgTaxRate.RateSource = AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.ManualOverride.Code;
			AssertNoErrors(orgTaxRate.RateSourceInfo);
		}

		protected override BusinessObject GetNewBusinessObject() => new OrganisationTaxRateFileImport();
	}
}
