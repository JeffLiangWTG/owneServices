using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccSurchargeApplicationValidationTest : BusinessObjectValidationTestCase
	{
		#region TestCheckASP_AT

		public void TestCheckASP_AT_ListValidation_VATTaxForCurrentCountry()
		{
			var taxID = AccSurchargeApplicationLookupsTest.CreateTaxIDForTaxRatesLookup(AccountingTestObjectCreator);

			var surchargeApplication = AccSurchargeApplicationLookupsTest.CreateAccSurchargeApplication(Factory);
			surchargeApplication.ASP_AT = taxID.PK;

			AssertNoErrors(surchargeApplication.ASP_ATInfo);
		}

		public void TestCheckASP_AT_ListValidation_VATTaxForNonCurrentCountry()
		{
			var taxID = AccSurchargeApplicationLookupsTest.CreateTaxIDForTaxRatesLookup(AccountingTestObjectCreator);
			taxID.AT_RN_NKCountry = CountryCodes.NewZealand;
			AssertNotEquals("Precondition: taxRate country", AccSurchargeApplicationLookupsTest.GetDefaultCountryCode(), taxID.AT_RN_NKCountry);

			var surchargeApplication = AccSurchargeApplicationLookupsTest.CreateAccSurchargeApplication(Factory);
			surchargeApplication.ASP_AT = taxID.PK;

			AssertHasError(surchargeApplication.ASP_ATInfo, "Enter a valid Tax ID.");
		}

		public void TestCheckASP_AT_ListValidation_TaxFrameworkTaxID()
		{
			var taxID = AccSurchargeApplicationLookupsTest.CreateTaxIDForTaxRatesLookup(AccountingTestObjectCreator);
			taxID.AT_TaxSystemCode = "OTHERTAX";
			AssertNotEquals("Precondition: taxRate is Tax Framework tax ID", ZString.Empty, taxID.AT_TaxSystemCode);

			var surchargeApplication = AccSurchargeApplicationLookupsTest.CreateAccSurchargeApplication(Factory);
			surchargeApplication.ASP_AT = taxID.PK;

			AssertHasError(surchargeApplication.ASP_ATInfo, "Enter a valid Tax ID.");
		}

		public void TestCheckASP_AT_AllowsEmptyValue()
		{
			var surchargeApplication = CreateAccSurchargeApplicationForCurrentCompany();
			surchargeApplication.ASP_AT = ZGuid.Empty;
			AssertNoErrors(surchargeApplication.ASP_ATInfo);
		}

		#endregion

		#region ASP_JobType
		public void TestCheckASP_JobType()
		{
			var surchargeApplication = CreateAccSurchargeApplicationForCurrentCompany();
			surchargeApplication.ASP_JobType = ZString.Empty;
			AssertHasError(surchargeApplication.ASP_JobTypeInfo, "Please enter a Job Type.");

			surchargeApplication.ASP_JobType = "ALL";
			AssertNoErrors(surchargeApplication.ASP_JobTypeInfo);

			surchargeApplication.ASP_JobType = "A&^";
			AssertHasError(surchargeApplication.ASP_JobTypeInfo, "Enter a valid Job Type.");
		}
		#endregion

		#region ASP_SupplyType

		public void TestCheckASP_SupplyType()
		{
			var surchargeApplication = CreateAccSurchargeApplicationForCurrentCompany();
			surchargeApplication.ASP_SupplyType = ZString.Empty;
			AssertNoErrors(surchargeApplication.ASP_SupplyTypeInfo);

			surchargeApplication.ASP_SupplyType = "LOC";
			AssertNoErrors(surchargeApplication.ASP_SupplyTypeInfo);

			surchargeApplication.ASP_SupplyType = "A&^";
			AssertHasError(surchargeApplication.ASP_SupplyTypeInfo, "Enter a valid Supply Type.");
		}
		#endregion

		#region ASP_HomeCountryOrZone

		public void TestCheckASP_HomeCountryOrZone()
		{
			var surchargeApplication = CreateAccSurchargeApplicationForCurrentCompany();
			surchargeApplication.ASP_HomeCountryOrZone = ZString.Empty;
			AssertHasError(surchargeApplication.ASP_HomeCountryOrZoneInfo, "Please enter an Organization Country or Zone.");

			surchargeApplication.ASP_HomeCountryOrZone = "EUX";
			AssertNoErrors(surchargeApplication.ASP_HomeCountryOrZoneInfo);

			surchargeApplication.ASP_HomeCountryOrZone = "A&^";
			AssertHasError(surchargeApplication.ASP_HomeCountryOrZoneInfo, "Enter a valid Organization Country or Zone.");
		}
		#endregion

		#region ASP_OrganizationCategory
		public void TestCheckASP_OrganizationCategory()
		{
			var surchargeApplication = CreateAccSurchargeApplicationForCurrentCompany();
			surchargeApplication.ASP_OrganizationCategory = ZString.Empty;
			AssertHasError(surchargeApplication.ASP_OrganizationCategoryInfo, "Please enter an Organization Category.");

			surchargeApplication.ASP_OrganizationCategory = "ALL";
			AssertNoErrors(surchargeApplication.ASP_OrganizationCategoryInfo);

			surchargeApplication.ASP_OrganizationCategory = "A&^";
			AssertHasError(surchargeApplication.ASP_OrganizationCategoryInfo, "Enter a valid Organization Category.");
		}
		#endregion

		#region ASP_PlaceOfSupply
		public void TestCheckASP_PlaceOfSupply()
		{
			var surchargeApplication = CreateAccSurchargeApplicationForCurrentCompany();
			surchargeApplication.ASP_PlaceOfSupply = ZString.Empty;
			AssertNoErrors(surchargeApplication.ASP_PlaceOfSupplyInfo);

			surchargeApplication.ASP_PlaceOfSupply = "EUX";
			AssertNoErrors(surchargeApplication.ASP_PlaceOfSupplyInfo);

			surchargeApplication.ASP_PlaceOfSupply = "A&^";
			AssertHasError(surchargeApplication.ASP_PlaceOfSupplyInfo, "Enter a valid Sell Fixed Place of Supply.");
		}
		#endregion

		#region ASP_ASC_NKSurchargeCode
		public void TestCheckASP_ASC_NKSurchargeCode()
		{
			var newFacotry = new BusinessObjectFactory();
			var surcharge1 = newFacotry.NewWithValidTestData<AccSurchargeConfiguration>();
			surcharge1.ASC_Code = "AAA";
			surcharge1.ASC_GC_Company = GlbCompany.CurrentCompany.PK;
			newFacotry.Save();

			var surchargeApplication = CreateAccSurchargeApplicationForCurrentCompany();
			AssertEquals("Precondition: We should have a surcharge configuration in same company.", surcharge1.ASC_GC_Company, surchargeApplication.ASP_GC_Company);

			surchargeApplication.ASP_ASC_NKSurchargeCode = ZString.Empty;
			AssertHasError(surchargeApplication.ASP_ASC_NKSurchargeCodeInfo, "Please enter a Surcharge Code.");

			surchargeApplication.ASP_ASC_NKSurchargeCode = "AAA";
			AssertNoErrors(surchargeApplication.ASP_ASC_NKSurchargeCodeInfo);

			surchargeApplication.ASP_ASC_NKSurchargeCode = "A&^";
			AssertHasError(surchargeApplication.ASP_ASC_NKSurchargeCodeInfo, "Enter a valid Surcharge Code.");
		}
		#endregion

		#region DuplicateCheck
		public void TestCheckItIsNotDuplicate()
		{
			var newFactory = new BusinessObjectFactory();
			var surcharge1 = newFactory.NewWithValidTestData<AccSurchargeConfiguration>();
			surcharge1.ASC_Code = "AAA";
			surcharge1.ASC_GC_Company = GlbCompany.CurrentCompany.PK;

			var surcharge2 = newFactory.NewWithValidTestData<AccSurchargeConfiguration>();
			surcharge2.ASC_Code = "BBB";
			surcharge2.ASC_GC_Company = GlbCompany.CurrentCompany.PK;
			newFactory.Save();

			var surchargeApplication1 = newFactory.NewWithValidTestData<AccSurchargeApplication>();
			var surchargeApplication2 = newFactory.NewWithValidTestData<AccSurchargeApplication>();

			surchargeApplication1.ASP_JobType = "FCN";
			surchargeApplication2.ASP_JobType = "ALL";
			surchargeApplication2.ASP_SupplyType = surchargeApplication1.ASP_SupplyType = "LOC";
			surchargeApplication2.ASP_HomeCountryOrZone = surchargeApplication1.ASP_HomeCountryOrZone = "ALL";
			surchargeApplication2.ASP_OrganizationCategory = surchargeApplication1.ASP_OrganizationCategory = "ALL";
			surchargeApplication2.ASP_PlaceOfSupply = surchargeApplication1.ASP_PlaceOfSupply = "AU";
			surchargeApplication2.ASP_ASC_NKSurchargeCode = surchargeApplication1.ASP_ASC_NKSurchargeCode = "AAA";
			surchargeApplication2.ASP_GC_Company = surchargeApplication1.ASP_GC_Company = GlbCompany.CurrentCompany.PK;

			newFactory.Save();

			var duplicateErrorString = "There should not be more than one row with the same conditions. (i.e. Job Type + Supply Type + Organization Country/Region Or Zone + Organization Category + SELL FPOS + Surcharge Code.)";

			surchargeApplication2.ASP_JobType = "SHP";
			AssertNoRowError(surchargeApplication2, duplicateErrorString);

			surchargeApplication1.ASP_JobType = "SHP";
			AssertHasRowError(surchargeApplication1, duplicateErrorString);

			surchargeApplication2.ASP_SupplyType = "LOX";
			AssertNoRowError(surchargeApplication2, duplicateErrorString);

			surchargeApplication1.ASP_SupplyType = "LOX";
			AssertHasRowError(surchargeApplication1, duplicateErrorString);

			surchargeApplication2.ASP_HomeCountryOrZone = "EUX";
			AssertNoRowError(surchargeApplication2, duplicateErrorString);

			surchargeApplication1.ASP_HomeCountryOrZone = "EUX";
			AssertHasRowError(surchargeApplication1, duplicateErrorString);

			surchargeApplication2.ASP_OrganizationCategory = "BUS";
			AssertNoRowError(surchargeApplication2, duplicateErrorString);

			surchargeApplication1.ASP_OrganizationCategory = "BUS";
			AssertHasRowError(surchargeApplication1, duplicateErrorString);

			surchargeApplication2.ASP_PlaceOfSupply = "CN";
			AssertNoRowError(surchargeApplication2, duplicateErrorString);

			surchargeApplication1.ASP_PlaceOfSupply = "CN";
			AssertHasRowError(surchargeApplication1, duplicateErrorString);

			surchargeApplication2.ASP_ASC_NKSurchargeCode = "BBB";
			AssertNoRowError(surchargeApplication2, duplicateErrorString);

			surchargeApplication1.ASP_ASC_NKSurchargeCode = "BBB";
			AssertHasRowError(surchargeApplication1, duplicateErrorString);
		}
		#endregion

		#region Implementation

		AccSurchargeApplication CreateAccSurchargeApplicationForCurrentCompany()
		{
			var surchargeApplication = Factory.New<AccSurchargeApplication>();
			surchargeApplication.ASP_GC_Company = GlbCompany.CurrentCompany.PK;

			return surchargeApplication;
		}

		AccountingTestObjectCreator AccountingTestObjectCreator => accountingTestObjectCreator ?? (accountingTestObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator accountingTestObjectCreator;

		#endregion
	}
}
