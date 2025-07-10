using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class CusCalculationRuleTypeDecider : CountrySpecificTypeDecider
	{
		public CusCalculationRuleTypeDecider()
		{
		}

		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var countryCode = GetCountryCodeForCalculationRule(row, factory);
			return GetTypeForCountryCode(countryCode);
		}

		static ZString GetCountryCodeForCalculationRule(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			var companyPK = row != null ? new ZGuid(row[AutoCusCalculationRule.Schema.CCR_GC_Company]) : ZGuid.Invalid;
			var company = companyPK.IsValid ? factory.Load<GlbCompany>(companyPK) : null;
			return company?.GC_RN_NKCountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusCalculationRule);

		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.Australia, delegate { return ObjectFactory.GetType<Integration.Customs.AU.ICusCalculationRule>(); }),
			};
	}
}
