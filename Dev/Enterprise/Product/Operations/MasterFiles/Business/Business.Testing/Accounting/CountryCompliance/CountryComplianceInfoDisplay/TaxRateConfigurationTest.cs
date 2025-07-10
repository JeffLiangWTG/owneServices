using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.CountryComplianceInfoDisplay.Testing
{
	[TestedType(typeof(TaxRateConfiguration))]
	sealed class TaxRateConfigurationTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
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
