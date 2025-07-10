using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TaxAuthoritiesRegistryItem))]
	sealed class TaxAuthoritiesRegistryItemTest : StronglyTypedRegistryItemTestCase<TaxAuthoritiesConfigurationCollection>
	{
		protected override StronglyTypedRegistryItem<TaxAuthoritiesConfigurationCollection, TaxAuthoritiesConfigurationCollection> GetNewRegistryItem()
		{
			return new TaxAuthoritiesRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
		}
	}

	[TestedType(typeof(TaxAuthoritiesRegistryDataType))]
	sealed class TaxAuthoritiesRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<TaxAuthoritiesRegistryDataType>
	{
		#region Implementation

		protected override TaxAuthoritiesRegistryDataType GetNewDataType() => new TaxAuthoritiesRegistryDataType();

		protected override string ExpectedEditorName
		{
			get { return "TaxAuthoritiesRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			TaxAuthoritiesConfigurationCollection collection1 = new TaxAuthoritiesConfigurationCollection();
			TaxAuthoritiesConfiguration taxAuthoritiesConfiguration = collection1.AddNew();
			taxAuthoritiesConfiguration.Code = "CW1TAXA1";
			taxAuthoritiesConfiguration.Name = "CW1 TAX AUTHORITY CODE1";
			taxAuthoritiesConfiguration.Country = CountryCodes.Australia;
			taxAuthoritiesConfiguration.TaxAuthorityType = AccountingMasterFilesTaxFrameworkConstants.TaxAuthorityTypeList.Municipal.Code;

			taxAuthoritiesConfiguration = collection1.AddNew();
			taxAuthoritiesConfiguration.Code = "CW1TAXA2";
			taxAuthoritiesConfiguration.Name = "CW1 TAX AUTHORITY CODE2";
			taxAuthoritiesConfiguration.Country = CountryCodes.India;
			taxAuthoritiesConfiguration.TaxAuthorityType = AccountingMasterFilesTaxFrameworkConstants.TaxAuthorityTypeList.National.Code;

			TaxAuthoritiesConfigurationCollection collection2 = new TaxAuthoritiesConfigurationCollection();
			taxAuthoritiesConfiguration = collection2.AddNew();
			taxAuthoritiesConfiguration.Code = "CW1TAXA3";
			taxAuthoritiesConfiguration.Name = "CW1 TAX AUTHORITY CODE3";
			taxAuthoritiesConfiguration.Country = CountryCodes.Argentina;
			taxAuthoritiesConfiguration.TaxAuthorityType = AccountingMasterFilesTaxFrameworkConstants.TaxAuthorityTypeList.State.Code;

			taxAuthoritiesConfiguration = collection2.AddNew();
			taxAuthoritiesConfiguration.Code = "CW1TAXA4";
			taxAuthoritiesConfiguration.Name = "CW1 TAX AUTHORITY CODE4";
			taxAuthoritiesConfiguration.Country = CountryCodes.Brazil;
			taxAuthoritiesConfiguration.TaxAuthorityType = AccountingMasterFilesTaxFrameworkConstants.TaxAuthorityTypeList.Regional.Code;

			string xml1 = @"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfTaxAuthoritiesConfiguration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
<TaxAuthoritiesConfiguration><Code>CW1TAXA1</Code><Name>CW1 TAX AUTHORITY CODE1</Name><Country>AU</Country><TaxAuthorityType>MUN</TaxAuthorityType></TaxAuthoritiesConfiguration>
<TaxAuthoritiesConfiguration><Code>CW1TAXA2</Code><Name>CW1 TAX AUTHORITY CODE2</Name><Country>IN</Country><TaxAuthorityType>NAT</TaxAuthorityType></TaxAuthoritiesConfiguration>
</ArrayOfTaxAuthoritiesConfiguration>";

			string xml2 = @"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfTaxAuthoritiesConfiguration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
<TaxAuthoritiesConfiguration><Code>CW1TAXA3</Code><Name>CW1 TAX AUTHORITY CODE3</Name><Country>AR</Country><TaxAuthorityType>STA</TaxAuthorityType></TaxAuthoritiesConfiguration>
<TaxAuthoritiesConfiguration><Code>CW1TAXA4</Code><Name>CW1 TAX AUTHORITY CODE4</Name><Country>BR</Country><TaxAuthorityType>REG</TaxAuthorityType></TaxAuthoritiesConfiguration>
</ArrayOfTaxAuthoritiesConfiguration>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, xml1),
				new ValidSampleAndBinaryValueInDB(collection2, xml2)
			};
		}

		#endregion
	}
}
