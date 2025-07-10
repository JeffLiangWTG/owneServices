using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class OrgTaxConfigurationModuleFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateTaxConfiguration_MandatoryValidation()
		{
			var taxConfig1 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfig1.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var filter = GetNewFilter();
			filter.ConfigurationStatus = "AAA";
			filter.TaxConfiguration = taxConfig1.PK;
			AssertNoErrors(filter.TaxConfigurationInfo);

			filter.TaxConfiguration = ZGuid.Empty;
			AssertHasError(filter.TaxConfigurationInfo, "Please enter a Tax Configuration.");

			using (filter.GetValidationSuspender())
			{
				filter.TaxConfiguration = taxConfig1.PK;
				AssertHasErrors("Precondition: Validation in setter is suspended", filter.TaxConfigurationInfo);
			}
			filter.Validation.ValidateAll();
			AssertNoErrors("The validation is called from ValidateAll", filter.TaxConfigurationInfo);
		}

		public void TestValidateTaxConfiguration_MandatoryValidationDependsOnConfigurationStatus()
		{
			var filter = GetNewFilter();
			filter.ConfigurationStatus = "AAA";
			AssertHasError(filter.TaxConfigurationInfo, "Please enter a Tax Configuration.");

			filter.ConfigurationStatus = "";
			AssertNoErrors(filter.TaxConfigurationInfo);
		}

		public void TestValidateTaxConfiguration()
		{
			var taxConfig1 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfig1.ETC_ParentId = GlbCompany.CurrentCompany.PK;

			var taxConfig2 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfig2.ETC_ParentId = Factory.NewWithValidTestData<GlbCompany>().PK;

			Factory.Save();

			var filter = GetNewFilter();
			filter.TaxConfiguration = taxConfig1.PK;
			AssertNoErrors(filter.TaxConfigurationInfo);

			filter.TaxConfiguration = taxConfig2.PK;
			AssertHasError(filter.TaxConfigurationInfo, "Enter a valid Tax Configuration.");

			using (filter.GetValidationSuspender())
			{
				filter.TaxConfiguration = taxConfig1.PK;
				AssertHasErrors("Precondition: Validation in setter is suspended", filter.TaxConfigurationInfo);
			}
			filter.Validation.ValidateAll();
			AssertNoErrors("The validation is called from ValidateAll", filter.TaxConfigurationInfo);
		}

		public void TestValidateConfigurationStatus()
		{
			var filter = GetNewFilter();
			filter.ConfigurationStatus = OrgTaxConfigurationModuleFilter.StatusActive;
			AssertNoErrors(filter.ConfigurationStatusInfo);

			filter.ConfigurationStatus = "XXX";
			AssertHasError(filter.ConfigurationStatusInfo, "Enter a valid Status.");

			using (filter.GetValidationSuspender())
			{
				filter.ConfigurationStatus = OrgTaxConfigurationModuleFilter.StatusNotConfigured;
				AssertHasErrors("Precondition: Validation in setter is suspended", filter.ConfigurationStatusInfo);
			}
			filter.Validation.ValidateAll();
			AssertNoErrors("The validation is called from ValidateAll", filter.ConfigurationStatusInfo);
		}

		OrgTaxConfigurationModuleFilter GetNewFilter()
		{
			return new OrgTaxConfigurationModuleFilter("Test");
		}
	}
}
