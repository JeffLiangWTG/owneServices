using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public class BaseCusGoodsCatalogTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var goodsCatalog = bizO as BaseCusGoodsCatalog;
			if (goodsCatalog != null)
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_RN_NKCountryCode = countryCode;

				goodsCatalog.CGC_GC_Company = company.PK;
				goodsCatalog.CGC_Type = GoodsCatalogTypeList.Codes.Import;
				goodsCatalog.CGC_CatalogCode = "TST_GOODS_CATALOG";
				goodsCatalog.CGC_Description = "TEST GOODS CATALOG";
				goodsCatalog.CGC_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			}
		}

		protected override Type BaseTypeDecidedType => typeof(BaseCusGoodsCatalog);

		protected override BusinessObject GetNewBusinessObjectForLoadTest() => (BaseCusGoodsCatalog)Factory.New(BaseTypeDecidedType);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Brazil, ObjectFactory.GetType<Integration.Customs.BR.ICusGoodsCatalog>() },
			};
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesForNewAndBindingTests();

		Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Brazil, ObjectFactory.GetType<Integration.Customs.BR.ICusGoodsCatalog>() },
			};
		}
	}
}
