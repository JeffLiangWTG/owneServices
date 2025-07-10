using System.Text;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceReportConfigurationRegistryDataType))]
	sealed class ComplianceReportConfigurationDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ComplianceReportConfigurationRegistryDataType>
	{
		public void TestReportingDateDefaultedforOldSavedValues()
		{
			var xmlValueWithoutReportingDate = @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfComplianceReportConfiguration xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><ComplianceReportConfiguration><Country>AU</Country><ReportCode>TST</ReportCode><ReportTitle>Test</ReportTitle><ReportBaseTablePrefix>AH</ReportBaseTablePrefix><ReportPeriodicity>PER</ReportPeriodicity><TaxRegistrationType>ABN</TaxRegistrationType><ReportLineGrouping /><GoodsServiceType /><ReportAmountsRoundingTruncating>0</ReportAmountsRoundingTruncating><AmountThresholdLevel /><ExTaxAmountThreshold>0</ExTaxAmountThreshold><TaxAmountThreshold>0</TaxAmountThreshold><ArrayOfComplianceReportConfigurationSetting><ComplianceReportConfigurationSetting><Country>AU</Country><ComplianceSubType /><LedgerType>AR</LedgerType><InvoiceType>INV</InvoiceType><TaxInvoiceRule>TID</TaxInvoiceRule><DisbursementRule>NDB</DisbursementRule><OriginalRule>ALL</OriginalRule><OrganisationLocation>AU</OrganisationLocation><TaxRegistrationType /></ComplianceReportConfigurationSetting></ArrayOfComplianceReportConfigurationSetting></ComplianceReportConfiguration></ArrayOfComplianceReportConfiguration>";
			var byteArrayValueWithoutReportingDate = Encoding.Unicode.GetBytes(xmlValueWithoutReportingDate);

			var collection = GetNewDataType().Deserialise(byteArrayValueWithoutReportingDate);
			AssertNotNull("collection", collection);

			AssertEquals("collection.Count", 1, collection.Count);
			var configuration = collection[0];
			AssertEquals("Country", "AU", configuration.Country);
			AssertEquals("ReportCode", "TST", configuration.ReportCode);
			AssertEquals("ReportTitle", "Test", configuration.ReportTitle);
			AssertEquals("TaxRegistrationType", "ABN", configuration.TaxRegistrationType);
			AssertEquals("ReportBaseTablePrefix", "AH", configuration.ReportBaseTablePrefix);
			AssertEquals("ReportPeriodicity", "PER", configuration.ReportPeriodicity);

			AssertEquals("Settings.Count", 1, configuration.Settings.Count);
			var setting = configuration.Settings[0];
			AssertEquals("LedgerType", "AR", setting.LedgerType);
			AssertEquals("InvoiceType", "INV", setting.InvoiceType);
			AssertEquals("OriginalRule", "ALL", setting.OriginalRule);
			AssertEquals("DisbursementRule", "NDB", setting.DisbursementRule);
			AssertEquals("TaxInvoiceRule", "TID", setting.TaxInvoiceRule);
			AssertEquals("OrganisationLocation", "AU", setting.OrganisationLocation);
			AssertEquals("ReportingDate was defaulted from Ledger setter and left unchanged", "POS", setting.ReportingDate);
		}

		#region Implementation

		protected override ComplianceReportConfigurationRegistryDataType GetNewDataType()
		{
			return new ComplianceReportConfigurationRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "ComplianceReportConfigurationRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new ComplianceReportConfigurationCollection(new BusinessObjectFactory());
			var complianceReportConfiguration = collection.AddNew();
			complianceReportConfiguration.Country = "AU";
			complianceReportConfiguration.ReportCode = "TST";
			complianceReportConfiguration.ReportTitle = "Test";
			complianceReportConfiguration.TaxRegistrationType = "ABN";
			complianceReportConfiguration.ReportBaseTablePrefix = "AH";
			complianceReportConfiguration.ReportPeriodicity = "PER";
			var setting = complianceReportConfiguration.Settings.AddNew();
			setting.LedgerType = "AR";
			setting.InvoiceType = "INV";
			setting.OriginalRule = "ALL";
			setting.DisbursementRule = "NDB";
			setting.TaxInvoiceRule = "TID";
			setting.OrganisationLocation = "AU";
			setting.ReportingDate = "INV";

			var xmlValue = @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfComplianceReportConfiguration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><ComplianceReportConfiguration><Country>AU</Country><ReportCode>TST</ReportCode><ReportTitle>Test</ReportTitle><ReportBaseTablePrefix>AH</ReportBaseTablePrefix><ReportPeriodicity>PER</ReportPeriodicity><TaxRegistrationType>ABN</TaxRegistrationType><ReportLineGrouping /><ReportLineOrdering /><GoodsServiceType /><ReportAmountsRoundingType /><ReportAmountsRoundingTruncating>0</ReportAmountsRoundingTruncating><AmountThresholdLevel /><ExTaxAmountThreshold>0</ExTaxAmountThreshold><TaxAmountThreshold>0</TaxAmountThreshold><RecipientOrgPK>00000000-0000-0000-0000-000000000000</RecipientOrgPK><RepCountryRegistrationCode /><IncludeQueuedForPreviousPeriod>N</IncludeQueuedForPreviousPeriod><ArrayOfComplianceReportConfigurationSetting><ComplianceReportConfigurationSetting><Country>AU</Country><ComplianceSubType /><LedgerType>AR</LedgerType><InvoiceType>INV</InvoiceType><TaxInvoiceRule>TID</TaxInvoiceRule><DisbursementRule>NDB</DisbursementRule><OriginalRule>ALL</OriginalRule><OrganisationLocation>AU</OrganisationLocation><TaxRegistrationType /><ReportingDate>INV</ReportingDate></ComplianceReportConfigurationSetting></ArrayOfComplianceReportConfigurationSetting></ComplianceReportConfiguration></ArrayOfComplianceReportConfiguration>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, xmlValue)
			};
		}

		#endregion
	}
}
