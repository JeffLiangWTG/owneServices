using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusGoodsCatalogProductionInfoTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var brCompany = Factory.NewWithValidTestData<GlbCompany>();
			brCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			var caCompany = Factory.NewWithValidTestData<GlbCompany>();
			caCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

			var catalog1 = Factory.NewWithValidTestData<BaseCusGoodsCatalog>();
			catalog1.CGC_GC_Company = brCompany.PK;
			var catalogProductionInfo1 = Factory.New<BaseCusGoodsCatalogProductionInfo>();
			catalogProductionInfo1.CGI_CGC_Catalog = catalog1.PK;
			catalogProductionInfo1.CGI_Type = "FOR";
			catalogProductionInfo1.CGI_Reference = "BR";

			var catalog2 = Factory.NewWithValidTestData<BaseCusGoodsCatalog>();
			catalog2.CGC_GC_Company = caCompany.PK;
			var catalogProductionInfo2 = Factory.New<BaseCusGoodsCatalogProductionInfo>();
			catalogProductionInfo2.CGI_CGC_Catalog = catalog2.PK;
			catalogProductionInfo2.CGI_Type = "FOR";
			catalogProductionInfo2.CGI_Reference = "AU";

			var catalog3 = Factory.NewWithValidTestData<BaseCusGoodsCatalog>();
			catalog3.CGC_GC_Company = brCompany.PK;
			var catalogProductionInfo3 = Factory.New<BaseCusGoodsCatalogProductionInfo>();
			catalogProductionInfo3.CGI_CGC_Catalog = catalog3.PK;
			catalogProductionInfo3.CGI_Type = "LPN";
			catalogProductionInfo3.CGI_Reference = "UY";

			var catalog4 = Factory.NewWithValidTestData<BaseCusGoodsCatalog>();
			catalog4.CGC_GC_Company = caCompany.PK;
			var catalogProductionInfo4 = Factory.New<BaseCusGoodsCatalogProductionInfo>();
			catalogProductionInfo4.CGI_CGC_Catalog = catalog4.PK;
			catalogProductionInfo4.CGI_Type = "LPN";
			catalogProductionInfo4.CGI_Reference = "DE";

			Factory.Save();

			var factory = new BusinessObjectFactory();
			var typeDecider = new CusGoodsCatalogProductionInfoTypeDecider();

			void AssertGetTypeForLoad(string expectedTypeFullName, BaseCusGoodsCatalogProductionInfo catalogProductionInfo)
			{
				AssertEquals(expectedTypeFullName, typeDecider.GetTypeForLoad(((INeedRow)catalogProductionInfo).Row, factory).FullName);
				AssertEquals(expectedTypeFullName, factory.Load<BaseCusGoodsCatalogProductionInfo>(catalogProductionInfo.PK).GetType().FullName);
			}

			AssertGetTypeForLoad("Enterprise.Customs.BR.Business.ForeignOperator", catalogProductionInfo1);
			AssertGetTypeForLoad("Enterprise.Customs.Business.BaseCusGoodsCatalogProductionInfo", catalogProductionInfo2);
			AssertGetTypeForLoad("Enterprise.Customs.BR.Business.LocalPartNumber", catalogProductionInfo3);
			AssertGetTypeForLoad("Enterprise.Customs.Business.BaseCusGoodsCatalogProductionInfo", catalogProductionInfo4);
		}

		public void TestGetTypeForNew()
		{
			AssertEquals(typeof(BaseCusGoodsCatalogProductionInfo), new CusGoodsCatalogProductionInfoTypeDecider().GetTypeForNew());
			AssertEquals(typeof(BaseCusGoodsCatalogProductionInfo), new BusinessObjectFactory().New<BaseCusGoodsCatalogProductionInfo>().GetType());
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(BaseCusGoodsCatalogProductionInfo), new CusGoodsCatalogProductionInfoTypeDecider().GetTypeForBinding());
		}
	}
}
