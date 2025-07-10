using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TaxSystemsRegistryItem))]
	sealed class TaxSystemsRegistryItemTest : StronglyTypedRegistryItemTestCase<TaxSystemsConfigurationCollection>
	{
		protected override StronglyTypedRegistryItem<TaxSystemsConfigurationCollection, TaxSystemsConfigurationCollection> GetNewRegistryItem()
		{
			return new TaxSystemsRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
		}
	}

	[TestedType(typeof(TaxSystemsRegistryDataType))]
	sealed class TaxSystemsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<TaxSystemsRegistryDataType>
	{
		#region Implementation

		protected override TaxSystemsRegistryDataType GetNewDataType() => new TaxSystemsRegistryDataType();

		protected override string ExpectedEditorName
		{
			get { return "TaxSystemsRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			TaxSystemsConfigurationCollection collection1 = new TaxSystemsConfigurationCollection();
			TaxSystemsConfiguration taxSystemsConfiguration = collection1.AddNew();
			taxSystemsConfiguration.Code = "CW1TAXA1";
			taxSystemsConfiguration.Name = "CW1 TAX SYSTEM CODE1";
			taxSystemsConfiguration.Country = CountryCodes.Australia;
			taxSystemsConfiguration.TaxAuthorityType = AccountingMasterFilesTaxFrameworkConstants.TaxAuthorityTypeList.Municipal.Code;
			taxSystemsConfiguration.TaxSuperType = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.Perceptions.Code;
			taxSystemsConfiguration.RegistrationLevel = AccountingMasterFilesTaxFrameworkConstants.TaxSystemRegistrationLevels.Company.Code;
			taxSystemsConfiguration.IncludeInInvoceTotal = true;
			taxSystemsConfiguration.AdjustmentSign = AccountingMasterFilesTaxFrameworkConstants.TaxCalculationAdjustmentSigns.Positive.Code;
			taxSystemsConfiguration.TaxBaseCalculationMethod = AccountingMasterFilesTaxFrameworkConstants.TaxBaseCalculationMethods.InvoiceLineAmount.Code;
			taxSystemsConfiguration.TaxAmountCalculationMethod = AccountingMasterFilesTaxFrameworkConstants.TaxAmountCalculationMethods.BaseTimesRate.Code;
			taxSystemsConfiguration.ThresholdRule = AccountingMasterFilesTaxFrameworkConstants.ThresholdRulesForTaxCalculation.NoThreshold.Code;
			taxSystemsConfiguration.TaxRateSource = AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.OrganisationOnly.Code;

			taxSystemsConfiguration = collection1.AddNew();
			taxSystemsConfiguration.Code = "CW1TAXA2";
			taxSystemsConfiguration.Name = "CW1 TAX SYSTEM CODE2";
			taxSystemsConfiguration.Country = CountryCodes.India;
			taxSystemsConfiguration.TaxAuthorityType = AccountingMasterFilesTaxFrameworkConstants.TaxAuthorityTypeList.State.Code;
			taxSystemsConfiguration.TaxSuperType = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.TurnoverTax.Code;
			taxSystemsConfiguration.RegistrationLevel = AccountingMasterFilesTaxFrameworkConstants.TaxSystemRegistrationLevels.Branch.Code;
			taxSystemsConfiguration.IncludeInInvoceTotal = false;
			taxSystemsConfiguration.AdjustmentSign = AccountingMasterFilesTaxFrameworkConstants.TaxCalculationAdjustmentSigns.Negative.Code;
			taxSystemsConfiguration.TaxBaseCalculationMethod = AccountingMasterFilesTaxFrameworkConstants.TaxBaseCalculationMethods.InvoiceLineAmount.Code;
			taxSystemsConfiguration.TaxAmountCalculationMethod = AccountingMasterFilesTaxFrameworkConstants.TaxAmountCalculationMethods.BaseTimesRate.Code;
			taxSystemsConfiguration.ThresholdRule = AccountingMasterFilesTaxFrameworkConstants.ThresholdRulesForTaxCalculation.NoThreshold.Code;
			taxSystemsConfiguration.TaxRateSource = AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.TaxGroupOnly.Code;

			TaxSystemsConfigurationCollection collection2 = new TaxSystemsConfigurationCollection();
			taxSystemsConfiguration = collection2.AddNew();
			taxSystemsConfiguration.Code = "CW1TAXA3";
			taxSystemsConfiguration.Name = "CW1 TAX SYSTEM CODE3";
			taxSystemsConfiguration.Country = CountryCodes.Argentina;
			taxSystemsConfiguration.TaxAuthorityType = AccountingMasterFilesTaxFrameworkConstants.TaxAuthorityTypeList.National.Code;
			taxSystemsConfiguration.TaxSuperType = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.SalesTax.Code;
			taxSystemsConfiguration.RegistrationLevel = AccountingMasterFilesTaxFrameworkConstants.TaxSystemRegistrationLevels.Company.Code;
			taxSystemsConfiguration.IncludeInInvoceTotal = true;
			taxSystemsConfiguration.AdjustmentSign = AccountingMasterFilesTaxFrameworkConstants.TaxCalculationAdjustmentSigns.Positive.Code;
			taxSystemsConfiguration.TaxBaseCalculationMethod = AccountingMasterFilesTaxFrameworkConstants.TaxBaseCalculationMethods.InvoiceLineAmount.Code;
			taxSystemsConfiguration.TaxAmountCalculationMethod = AccountingMasterFilesTaxFrameworkConstants.TaxAmountCalculationMethods.BaseTimesRate.Code;
			taxSystemsConfiguration.ThresholdRule = AccountingMasterFilesTaxFrameworkConstants.ThresholdRulesForTaxCalculation.NoThreshold.Code;
			taxSystemsConfiguration.TaxRateSource = AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.OrganisationFallbackToTaxGroup.Code;

			taxSystemsConfiguration = collection2.AddNew();
			taxSystemsConfiguration.Code = "CW1TAXA4";
			taxSystemsConfiguration.Name = "CW1 TAX SYSTEM CODE4";
			taxSystemsConfiguration.Country = CountryCodes.Brazil;
			taxSystemsConfiguration.TaxAuthorityType = AccountingMasterFilesTaxFrameworkConstants.TaxAuthorityTypeList.Regional.Code;
			taxSystemsConfiguration.TaxSuperType = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.RetentionInInvoice.Code;
			taxSystemsConfiguration.RegistrationLevel = AccountingMasterFilesTaxFrameworkConstants.TaxSystemRegistrationLevels.Branch.Code;
			taxSystemsConfiguration.IncludeInInvoceTotal = false;
			taxSystemsConfiguration.AdjustmentSign = AccountingMasterFilesTaxFrameworkConstants.TaxCalculationAdjustmentSigns.Negative.Code;
			taxSystemsConfiguration.TaxBaseCalculationMethod = AccountingMasterFilesTaxFrameworkConstants.TaxBaseCalculationMethods.InvoiceLineAmount.Code;
			taxSystemsConfiguration.TaxAmountCalculationMethod = AccountingMasterFilesTaxFrameworkConstants.TaxAmountCalculationMethods.BaseTimesRate.Code;
			taxSystemsConfiguration.ThresholdRule = AccountingMasterFilesTaxFrameworkConstants.ThresholdRulesForTaxCalculation.NoThreshold.Code;
			taxSystemsConfiguration.TaxRateSource = AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.TaxIDOnly.Code;

			string xml1 = @"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfTaxSystemsConfiguration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
<TaxSystemsConfiguration><Code>CW1TAXA1</Code><Name>CW1 TAX SYSTEM CODE1</Name><Country>AU</Country><TaxAuthorityType>MUN</TaxAuthorityType><TaxSuperType>PER</TaxSuperType><RegistrationLevel>CMP</RegistrationLevel><IncludeInInvoceTotal>Y</IncludeInInvoceTotal><AdjustmentSign>POS</AdjustmentSign><TaxBaseCalculationMethod>ILA</TaxBaseCalculationMethod><TaxAmountCalculationMethod>BTR</TaxAmountCalculationMethod><ThresholdRule>NTR</ThresholdRule><TaxRateSource>ORG</TaxRateSource></TaxSystemsConfiguration>
<TaxSystemsConfiguration><Code>CW1TAXA2</Code><Name>CW1 TAX SYSTEM CODE2</Name><Country>IN</Country><TaxAuthorityType>STA</TaxAuthorityType><TaxSuperType>TRX</TaxSuperType><RegistrationLevel>BRN</RegistrationLevel><IncludeInInvoceTotal>N</IncludeInInvoceTotal><AdjustmentSign>NEG</AdjustmentSign><TaxBaseCalculationMethod>ILA</TaxBaseCalculationMethod><TaxAmountCalculationMethod>BTR</TaxAmountCalculationMethod><ThresholdRule>NTR</ThresholdRule><TaxRateSource>TGR</TaxRateSource></TaxSystemsConfiguration>
</ArrayOfTaxSystemsConfiguration>";

			string xml2 = @"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfTaxSystemsConfiguration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
<TaxSystemsConfiguration><Code>CW1TAXA3</Code><Name>CW1 TAX SYSTEM CODE3</Name><Country>AR</Country><TaxAuthorityType>NAT</TaxAuthorityType><TaxSuperType>SLX</TaxSuperType><RegistrationLevel>CMP</RegistrationLevel><IncludeInInvoceTotal>Y</IncludeInInvoceTotal><AdjustmentSign>POS</AdjustmentSign><TaxBaseCalculationMethod>ILA</TaxBaseCalculationMethod><TaxAmountCalculationMethod>BTR</TaxAmountCalculationMethod><ThresholdRule>NTR</ThresholdRule><TaxRateSource>OFT</TaxRateSource></TaxSystemsConfiguration>
<TaxSystemsConfiguration><Code>CW1TAXA4</Code><Name>CW1 TAX SYSTEM CODE4</Name><Country>BR</Country><TaxAuthorityType>REG</TaxAuthorityType><TaxSuperType>RII</TaxSuperType><RegistrationLevel>BRN</RegistrationLevel><IncludeInInvoceTotal>N</IncludeInInvoceTotal><AdjustmentSign>NEG</AdjustmentSign><TaxBaseCalculationMethod>ILA</TaxBaseCalculationMethod><TaxAmountCalculationMethod>BTR</TaxAmountCalculationMethod><ThresholdRule>NTR</ThresholdRule><TaxRateSource>TID</TaxRateSource></TaxSystemsConfiguration>
</ArrayOfTaxSystemsConfiguration>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, xml1),
				new ValidSampleAndBinaryValueInDB(collection2, xml2)
			};
		}

		#endregion
	}
}
