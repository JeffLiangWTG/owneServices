using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	class RefDbRepoSafePluralizerFixture
	{
		[TestCase("RefAccTaxRate", "RefAccTaxRates")]
		[TestCase("RefCountryStates", "RefCountryStates")]
		[TestCase("RefCusQuota", "RefCusQuota")]
		[TestCase("RefUNLOCO", "RefUNLOCOs")]
		[TestCase("SystemData", "SystemData")]
		[TestCase("DataSetChangeHistory", "DataSetChangeHistories")]
		[TestCase("RefCusCodeListAttributeUserView", "RefCusCodeListAttributeUserViews")]
		public void Pluralize(string identifier, string expectedResult)
		{
			var pluralizer = new RefDbRepoSafePluralizer();
			var result = pluralizer.Pluralize(identifier);
			Assert.AreEqual(expectedResult, result);
		}

		[TestCase("RefAccTaxRates", "RefAccTaxRate")]
		[TestCase("RefCountryStates", "RefCountryStates")]
		[TestCase("RefCusQuota", "RefCusQuota")]
		[TestCase("RefUNLOCOs", "RefUNLOCO")]
		[TestCase("SystemData", "SystemData")]
		[TestCase("DataSetChangeHistories", "DataSetChangeHistory")]
		[TestCase("RefCusCodeListAttributeUserViews", "RefCusCodeListAttributeUserView")]
		public void Singularize(string identifier, string expectedResult)
		{
			var pluralizer = new RefDbRepoSafePluralizer();
			var result = pluralizer.Singularize(identifier);
			Assert.AreEqual(expectedResult, result);
		}
	}
}
