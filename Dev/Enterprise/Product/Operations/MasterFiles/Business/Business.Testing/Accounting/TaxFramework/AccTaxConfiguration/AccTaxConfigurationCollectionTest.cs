using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTaxConfigurationCollection))]
	class AccTaxConfigurationCollectionTest : ActiveBusinessObjectCollectionTestCase<AccTaxConfigurationCollection>
	{
		public void TestConstructorWithCountryCode()
		{
			var collection = new AccTaxConfigurationCollection(Factory, "AU");
			var query = collection.CompleteFilter;
			Assert("Not a NoResultQuery", !query.IsNoResultQuery);
			AssertEquals("ETC_RN_NKCountry = 'AU'", query.LiteralTextADO);

			collection = new AccTaxConfigurationCollection(Factory, ZString.Empty);
			query = collection.CompleteFilter;
			Assert("Not a NoResultQuery", !query.IsNoResultQuery);
			AssertEquals("", query.LiteralTextADO);
		}
	}
}
