using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TaxAuthoritiesConfiguration))]
	class TaxAuthoritiesConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateCode()
		{
			var config = new TaxAuthoritiesConfiguration();
			AssertEquals("Max length", 10, config.CodeInfo.MaxLength);

			config.RunPreSaveValidation();
			AssertHasError(config.CodeInfo, "Please enter a Code.");

			config.Code = "C.1";
			config.RunPreSaveValidation();
			AssertHasError(config.CodeInfo, "Please enter alphanumeric characters only.");

			config.Code = "CḈ1";
			config.RunPreSaveValidation();
			AssertHasError(config.CodeInfo, "Code only accepts Western European languages characters.");

			config.Code = "Cw1Code";
			config.RunPreSaveValidation();
			AssertNoErrors(config.CodeInfo);
			AssertEquals("Assignment in uppercase", "CW1CODE", config.Code);
		}

		public void TestName()
		{
			var config = new TaxAuthoritiesConfiguration();
			AssertEquals("Max length", 80, config.NameInfo.MaxLength);

			config.Name = ZString.Empty;
			config.RunPreSaveValidation();
			AssertNoErrors(config.NameInfo);

			config.Name = "C.1";
			config.RunPreSaveValidation();
			AssertNoErrors(config.NameInfo);
		}

		public void TestValidateCountry()
		{
			var config = new TaxAuthoritiesConfiguration();
			AssertEquals("Max length", 2, config.CountryInfo.MaxLength);

			config.Name = ZString.Empty;
			config.RunPreSaveValidation();
			AssertHasError(config.CountryInfo, "Please enter a Country/Region.");

			config.Country = "AB";
			config.RunPreSaveValidation();
			AssertHasError(config.CountryInfo, "Enter a valid Country/Region.");

			config.Country = CountryCodes.Australia;
			config.RunPreSaveValidation();
			AssertNoErrors(config.CountryInfo);
		}

		public void TestValidateTaxAuthorityType()
		{
			var config = new TaxAuthoritiesConfiguration();
			AssertEquals("Max length", 3, config.TaxAuthorityTypeInfo.MaxLength);

			config.TaxAuthorityType = ZString.Empty;
			config.RunPreSaveValidation();
			AssertHasError(config.TaxAuthorityTypeInfo, "Please enter a Tax Authority Type.");

			config.TaxAuthorityType = "AB";
			config.RunPreSaveValidation();
			AssertHasError(config.TaxAuthorityTypeInfo, "Enter a valid Tax Authority Type.");

			config.TaxAuthorityType = AccountingMasterFilesTaxFrameworkConstants.TaxAuthorityTypeList.National.Code;
			config.RunPreSaveValidation();
			AssertNoErrors(config.TaxAuthorityTypeInfo);

			config.TaxAuthorityType = AccountingMasterFilesTaxFrameworkConstants.TaxAuthorityTypeList.Regional.Code;
			config.RunPreSaveValidation();
			AssertNoErrors(config.TaxAuthorityTypeInfo);

			config.TaxAuthorityType = AccountingMasterFilesTaxFrameworkConstants.TaxAuthorityTypeList.State.Code;
			config.RunPreSaveValidation();
			AssertNoErrors(config.TaxAuthorityTypeInfo);

			config.TaxAuthorityType = AccountingMasterFilesTaxFrameworkConstants.TaxAuthorityTypeList.Municipal.Code;
			config.RunPreSaveValidation();
			AssertNoErrors(config.TaxAuthorityTypeInfo);
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override BusinessObject GetNewBusinessObject() => new TaxAuthoritiesConfiguration();

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone() => (TaxAuthoritiesConfiguration)GetNewBusinessObject();

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise() => (TaxAuthoritiesConfiguration)GetNewBusinessObject();

		protected new TaxAuthoritiesConfiguration BizObj => (TaxAuthoritiesConfiguration)base.BizObj;

		#endregion
	}
}
