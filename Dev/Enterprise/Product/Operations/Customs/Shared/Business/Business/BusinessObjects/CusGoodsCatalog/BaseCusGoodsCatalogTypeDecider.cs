using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class BaseCusGoodsCatalogTypeDecider : CountrySpecificTypeDecider
	{
		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var countryCode = GetGoodsCatalogCountryCode(row, factory);
			return GetTypeForCountryCode(countryCode);
		}

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new CountrySpecificType(Core.Constants.CountryCodes.Brazil, delegate { return ObjectFactory.GetType<Integration.Customs.BR.ICusGoodsCatalog>(); })
		};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(BaseCusGoodsCatalog);

		ZString GetGoodsCatalogCountryCode(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var companyPK = row != null ? new ZGuid(row[BaseCusGoodsCatalog.Schema.CGC_GC_Company]) : ZGuid.Empty;
			var company = companyPK.IsValid ? factory.Load<GlbCompany>(companyPK) : null;
			return company?.GC_RN_NKCountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
