using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.CountryComplianceInfoDisplay.Testing
{
	[TestedType(typeof(TaxRateConfigurationCollection))]
	sealed class TaxRateConfigurationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TaxRateConfigurationCollection>
	{
		protected override TaxRateConfigurationCollection GetCollectionToTest() => new TaxRateConfigurationCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TaxRateConfiguration(
				"AU",
				"TAX",
				"TYPE",
				"Desc",
				"EXTYPE",
				false,
				false,
				false,
				false,
				false,
				0,
				"REF",
				"EXREF",
				"",
				"");
		}
	}
}
