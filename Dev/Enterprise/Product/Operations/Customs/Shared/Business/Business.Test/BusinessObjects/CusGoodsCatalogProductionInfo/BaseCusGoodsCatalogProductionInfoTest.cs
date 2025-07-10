using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseCusGoodsCatalogProductionInfo))]
	public class BaseCusGoodsCatalogProductionInfoTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var cusGoodsCatalog = Factory.NewWithValidTestData<BaseCusGoodsCatalog>();
			var cusGoogCatalogProd = Factory.New<BaseCusGoodsCatalogProductionInfo>();
			cusGoogCatalogProd.CGI_Reference = "1";
			cusGoogCatalogProd.CGI_Type = "FOR";
			cusGoogCatalogProd.CGI_CGC_Catalog = cusGoodsCatalog.PK;
			return cusGoogCatalogProd;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var cusGoodsCatalog = Factory.NewWithValidTestData<BaseCusGoodsCatalog>();
			var cusGoogCatalogProd = Factory.New<BaseCusGoodsCatalogProductionInfo>();
			cusGoogCatalogProd.CGI_Reference = "1";
			cusGoogCatalogProd.CGI_Type = "FOR";
			cusGoogCatalogProd.CGI_CGC_Catalog = cusGoodsCatalog.PK;
			return cusGoogCatalogProd;
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			var cusGoodsCatalog = Factory.NewWithValidTestData<BaseCusGoodsCatalog>();
			var bo = Factory.New<BaseCusGoodsCatalogProductionInfo>();
			bo.CGI_Reference = "1";
			bo.CGI_Type = "FOR";
			bo.CGI_CGC_Catalog = cusGoodsCatalog.PK;

			AssertNoExceptionThrown(() => Factory.Save());
			Assert(bo.IsInDatabase);

			AssertNoExceptionThrown(() => bo.Delete());
			Assert(bo.IsDeleted);
		}
	}
}
