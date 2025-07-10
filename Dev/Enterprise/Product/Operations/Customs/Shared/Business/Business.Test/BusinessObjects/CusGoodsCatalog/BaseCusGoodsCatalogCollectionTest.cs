using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseCusGoodsCatalogCollection<BaseCusGoodsCatalog>))]
	class BaseCusGoodsCatalogCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetCompanyQuery()
		{
			var company = Factory.New<GlbCompany>();
			var goodsCatalog1 = Factory.New<BaseCusGoodsCatalog>();
			var goodsCatalog2 = Factory.New<BaseCusGoodsCatalog>();
			goodsCatalog2.CGC_GC_Company = company.PK;

			var collection = new BaseCusGoodsCatalogCollection<BaseCusGoodsCatalog>(Factory);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(goodsCatalog1, collection);

			collection = new BaseCusGoodsCatalogCollection<BaseCusGoodsCatalog>(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(goodsCatalog1, collection);

			collection = new BaseCusGoodsCatalogCollection<BaseCusGoodsCatalog>(Factory, company.PK);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(goodsCatalog2, collection);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new BaseCusGoodsCatalogCollection<BaseCusGoodsCatalog>(Factory);
	}
}
