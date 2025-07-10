using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrganisationTaxRateFileImportLine))]
	sealed class OrganisationTaxRateFileImportLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestOrganizationTaxRateFileImportLinesAttributes()
		{
			var baseTestCases = new[]
			{
				new { Resource = DataBoundResourceStrings.GetDataForProperty(orgTaxRateFileImportLine.OrganizationCodeInfo), Caption = "Organization Code", ShortCaption = "Org. Code" },
				new { Resource = DataBoundResourceStrings.GetDataForProperty(orgTaxRateFileImportLine.OrganizationNameInfo), Caption = "Organization Name", ShortCaption = "Org. Name" },
				new { Resource = DataBoundResourceStrings.GetDataForProperty(orgTaxRateFileImportLine.RegistrationCodeInfo), Caption = "Registration Code", ShortCaption = "Reg. Code" },
				new { Resource = DataBoundResourceStrings.GetDataForProperty(orgTaxRateFileImportLine.RateSourceInfo), Caption = "Rate Source", ShortCaption = "" },
				new { Resource = DataBoundResourceStrings.GetDataForProperty(orgTaxRateFileImportLine.StartDateInfo), Caption = "Start Date", ShortCaption = "" },
				new { Resource = DataBoundResourceStrings.GetDataForProperty(orgTaxRateFileImportLine.EndDateInfo), Caption = "End Date", ShortCaption = "" },
				new { Resource = DataBoundResourceStrings.GetDataForProperty(orgTaxRateFileImportLine.RateNumeratorInfo), Caption = "Rate Numerator", ShortCaption = "Rate Num." },
				new { Resource = DataBoundResourceStrings.GetDataForProperty(orgTaxRateFileImportLine.RateDenominatorInfo), Caption = "Rate Denominator", ShortCaption = "Rate Den." },
			};

			foreach (var testCase in baseTestCases)
			{
				CombineAssertions(() =>
				{
					AssertEquals("Caption", testCase.Caption, testCase.Resource.Caption);
					AssertEquals("Short Caption", testCase.ShortCaption, testCase.Resource.ShortCaption);
				});
			}
		}

		public void TestRateSource_EqualToTaxRateProperty()
		{
			var expectedCurrentRateSource = RateSourceMethods.Quarterly.Code;
			orgTaxRateFileImportLine.RateSource = expectedCurrentRateSource;
			AssertEquals(expectedCurrentRateSource, taxRate.OTR_Source);

			expectedCurrentRateSource = RateSourceMethods.Monthly.Code;
			taxRate.OTR_Source = expectedCurrentRateSource;
			AssertEquals(expectedCurrentRateSource, orgTaxRateFileImportLine.RateSource);
		}

		public void TestStartDate_EqualToTaxRateProperty()
		{
			var expectedCurrentStartDate = ZDate.Today;
			orgTaxRateFileImportLine.StartDate = expectedCurrentStartDate;
			AssertEquals(expectedCurrentStartDate, taxRate.OTR_StartDate);

			expectedCurrentStartDate = ZDate.Today.AddDays(-5);
			taxRate.OTR_StartDate = expectedCurrentStartDate;
			AssertEquals(expectedCurrentStartDate, orgTaxRateFileImportLine.StartDate);
		}

		public void TestEndDate_EqualToTaxRateProperty()
		{
			var expectedCurrentEndDate = ZDate.Today;
			orgTaxRateFileImportLine.EndDate = expectedCurrentEndDate;
			AssertEquals(expectedCurrentEndDate, taxRate.OTR_EndDate);

			expectedCurrentEndDate = ZDate.Today.AddDays(55);
			taxRate.OTR_EndDate = expectedCurrentEndDate;
			AssertEquals(expectedCurrentEndDate, orgTaxRateFileImportLine.EndDate);
		}

		public void TestRateNumerator_EqualToTaxRateProperty()
		{
			var expectedCurrentNumerator = 1;
			orgTaxRateFileImportLine.RateNumerator = expectedCurrentNumerator;
			AssertEquals(expectedCurrentNumerator, taxRate.OTR_RateNumerator);

			expectedCurrentNumerator = 99;
			taxRate.OTR_RateNumerator = expectedCurrentNumerator;
			AssertEquals(expectedCurrentNumerator, orgTaxRateFileImportLine.RateNumerator);
		}

		public void TestRateDenominator_EqualToTaxRateProperty()
		{
			var expectedCurrentDenominator = 10;
			orgTaxRateFileImportLine.RateDenominator = expectedCurrentDenominator;
			AssertEquals(expectedCurrentDenominator, taxRate.OTR_RateDenominator);

			expectedCurrentDenominator = 100;
			taxRate.OTR_RateDenominator = expectedCurrentDenominator;
			AssertEquals(expectedCurrentDenominator, orgTaxRateFileImportLine.RateDenominator);
		}

		public void TestTaxConfigurationPK_EqualToTaxRateProperty()
		{
			var expectedTaxConfigurationZGuid = ZGuid.NewZGuid();
			orgTaxRateFileImportLine.TaxConfigurationPK = expectedTaxConfigurationZGuid;
			AssertEquals(expectedTaxConfigurationZGuid, taxRate.OTR_OTC);

			expectedTaxConfigurationZGuid = ZGuid.NewZGuid();
			taxRate.OTR_OTC = expectedTaxConfigurationZGuid;
			AssertEquals(expectedTaxConfigurationZGuid, orgTaxRateFileImportLine.TaxConfigurationPK);
		}

		public void TestPropertiesOfOrgTaxRateFileImportLineIsReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertEquals("OrganizationCodeInfo should have ReadOnly property info.", true, orgTaxRateFileImportLine.OrganizationCodeInfo.ReadOnly);
				AssertEquals("OrganizationNameInfo should have ReadOnly property info.", true, orgTaxRateFileImportLine.OrganizationNameInfo.ReadOnly);
				AssertEquals("RegistrationCodeInfo should have ReadOnly property info.", true, orgTaxRateFileImportLine.RegistrationCodeInfo.ReadOnly);
				AssertEquals("RateSourceInfo should have ReadOnly property info.", true, orgTaxRateFileImportLine.RateSourceInfo.ReadOnly);
				AssertEquals("StartDateInfo should have ReadOnly property info.", true, orgTaxRateFileImportLine.StartDateInfo.ReadOnly);
				AssertEquals("EndDateInfo should have ReadOnly property info.", true, orgTaxRateFileImportLine.EndDateInfo.ReadOnly);
				AssertEquals("RateDenominatorInfo should have ReadOnly property info.", true, orgTaxRateFileImportLine.RateDenominatorInfo.ReadOnly);
				AssertEquals("RateNumeratorInfo should have ReadOnly property info.", true, orgTaxRateFileImportLine.RateNumeratorInfo.ReadOnly);
				AssertEquals("TaxConfigurationPKInfo should have ReadOnly property info.", true, orgTaxRateFileImportLine.TaxConfigurationPKInfo.ReadOnly);
			});
		}

		public void TestZWrappedPropertyInfo()
		{
			CombineAssertions(() =>
			{
				AssertEquals("RateSoureInfo should wrap Tax Rate Property Info.",
					taxRate.OTR_SourceInfo, ((ZWrappedPropertyInfo)orgTaxRateFileImportLine.RateSourceInfo).InnerInfo);
				AssertEquals("StartDateInfo should wrap Tax Rate Property Info.",
					taxRate.OTR_StartDateInfo, ((ZWrappedPropertyInfo)orgTaxRateFileImportLine.StartDateInfo).InnerInfo);
				AssertEquals("EndDateInfo should wrap Tax Rate Property Info.",
					taxRate.OTR_EndDateInfo, ((ZWrappedPropertyInfo)orgTaxRateFileImportLine.EndDateInfo).InnerInfo);
				AssertEquals("RateNumeratorInfo should wrap Tax Rate Property Info.",
					taxRate.OTR_RateNumeratorInfo, ((ZWrappedPropertyInfo)orgTaxRateFileImportLine.RateNumeratorInfo).InnerInfo);
				AssertEquals("RateDenominatorInfo should wrap Tax Rate Property Info.",
					taxRate.OTR_RateDenominatorInfo, ((ZWrappedPropertyInfo)orgTaxRateFileImportLine.RateDenominatorInfo).InnerInfo);
				AssertEquals("TaxConfigurationPKInfo should wrap Tax Rate Property Info.",
					taxRate.OTR_OTCInfo, ((ZWrappedPropertyInfo)orgTaxRateFileImportLine.TaxConfigurationPKInfo).InnerInfo);
			});
		}

		public void TestRunPreSaveValidation()
		{
			using (taxRate.GetValidationSuspender())
			{
				orgTaxRateFileImportLine.RateSource = string.Empty;
			}
			AssertNoErrors(orgTaxRateFileImportLine.RateSourceInfo);
			orgTaxRateFileImportLine.RunPreSaveValidation();
			AssertHasError(orgTaxRateFileImportLine.RateSourceInfo, "Please enter a Source.");
		}

		public void TestOrganisationTaxRateFileImportLineSaveCorrectly()
		{
			var newFactory = new BusinessObjectFactory();
			var accountingTestObjectCreator = new AccountingTestObjectCreator(newFactory);

			var companyData = newFactory.NewWithValidTestData<OrgCompanyData>();
			var taxConfiguration = accountingTestObjectCreator.CreateTaxConfiguration("AR");
			newFactory.Save();

			var accOrgTaxConfiguration = accountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfiguration, companyData, true);
			var orgTaxRateFileImportLine = new OrganisationTaxRateFileImportLine(newFactory);
			orgTaxRateFileImportLine.RateSource = RateSourceMethods.Quarterly.Code;
			orgTaxRateFileImportLine.TaxConfigurationPK = accOrgTaxConfiguration.PK;

			AssertEquals("TaxRate is not persisted.", false, orgTaxRateFileImportLine.IsOrgTaxRateInDb);

			newFactory.Save();
			AssertEquals("TaxRate now is persisted.", true, orgTaxRateFileImportLine.IsOrgTaxRateInDb);
		}

		protected override void SetUp()
		{
			base.SetUp();
			orgTaxRateFileImportLine = new OrganisationTaxRateFileImportLine(Factory);

			var taxRates = Factory.Load<AccOrgTaxRate>(new ZQuery());
			AssertEquals("Number of Tax Rates in memory", 1, taxRates.Length);
			taxRate = taxRates[0];
		}

		OrganisationTaxRateFileImportLine orgTaxRateFileImportLine;
		AccOrgTaxRate taxRate;
	}
}
