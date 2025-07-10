using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseCusGoodsCatalogProductionInfoCollection<BaseCusGoodsCatalogProductionInfo>))]
	internal sealed class BaseCusGoodsCatalogProductionInfoCollectionTest : ActiveBusinessObjectCollectionTestCase<BaseCusGoodsCatalogProductionInfoCollection<BaseCusGoodsCatalogProductionInfo>>
	{
		public void TestCollection()
		{
			var goodsCatalog1 = Factory.NewWithValidTestData<BaseCusGoodsCatalog>();
			var productionInfo1 = Factory.New<BaseCusGoodsCatalogProductionInfo>();
			productionInfo1.CGI_CGC_Catalog = goodsCatalog1.PK;
			productionInfo1.CGI_Type = "FOR";
			productionInfo1.CGI_Reference = "AU";

			var goodsCatalog2 = Factory.NewWithValidTestData<BaseCusGoodsCatalog>();
			var productionInfo2 = Factory.New<BaseCusGoodsCatalogProductionInfo>();
			productionInfo2.CGI_CGC_Catalog = goodsCatalog2.PK;
			productionInfo2.CGI_Reference = "AU";
			productionInfo2.CGI_Type = "FOR";

			var productionInfo3 = Factory.New<BaseCusGoodsCatalogProductionInfo>();
			productionInfo3.CGI_CGC_Catalog = goodsCatalog1.PK;
			productionInfo3.CGI_Type = "LPN";
			productionInfo3.CGI_Reference = "AU";

			Factory.Save();

			var collection = new BaseCusGoodsCatalogProductionInfoCollection<BaseCusGoodsCatalogProductionInfo>(goodsCatalog1, "FOR");
			AssertContainsExactElementsInAnyOrder("Load BaseCusGoodsCatalogProductionInfoCollection by CGI_CGC_Catalog", new[] { productionInfo1 }, collection);

			collection = new BaseCusGoodsCatalogProductionInfoCollection<BaseCusGoodsCatalogProductionInfo>(goodsCatalog2, "FOR");
			AssertContainsExactElementsInAnyOrder("Load BaseCusGoodsCatalogProductionInfoCollection by CGI_CGC_Catalog", new[] { productionInfo2 }, collection);

			collection = new BaseCusGoodsCatalogProductionInfoCollection<BaseCusGoodsCatalogProductionInfo>(goodsCatalog1, "LPN");
			AssertContainsExactElementsInAnyOrder("Load BaseCusGoodsCatalogProductionInfoCollection by CGI_CGC_Catalog", new[] { productionInfo3 }, collection);
		}

		public void TestSetDefaultsForNewChild()
		{
			var cusGoodsCatalog = Factory.New<BaseCusGoodsCatalog>();
			var catalogProductionInfo = Factory.New<BaseCusGoodsCatalogProductionInfo>();
			catalogProductionInfo.CGI_CGC_Catalog = cusGoodsCatalog.PK;
			AssertEquals(cusGoodsCatalog.PK, catalogProductionInfo.CGI_CGC_Catalog);
		}

		protected override BaseCusGoodsCatalogProductionInfoCollection<BaseCusGoodsCatalogProductionInfo> GetCollectionToTest()
		{
			var catalog = Factory.NewWithValidTestData<BaseCusGoodsCatalog>();
			return new BaseCusGoodsCatalogProductionInfoCollection<BaseCusGoodsCatalogProductionInfo>(catalog, "");
		}
	}
}

