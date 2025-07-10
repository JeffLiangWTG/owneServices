using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseJobDeclarationCollection))]
	public class BaseJobDeclarationBizoCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetCountryQuery()
		{
			var query = BaseJobDeclarationCollection.GetCountryQuery(Core.Constants.CountryCodes.Australia).LiteralTextADO;
			Assert("Query containers AU", query.Contains("'AU%'"));
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new BaseJobDeclarationCollection(Factory);
	}
}
