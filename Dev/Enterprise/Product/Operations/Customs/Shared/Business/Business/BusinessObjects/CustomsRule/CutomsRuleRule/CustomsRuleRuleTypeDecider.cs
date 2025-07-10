using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class CustomsRuleRuleTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
			{
				new CountrySpecificType(Core.Constants.CountryCodes.Canada, () => ObjectFactory.GetType<Integration.Customs.CA.ICustomsRuleRule>()),
				new CountrySpecificType(Core.Constants.CountryCodes.UnitedStates, () => ObjectFactory.GetType<Integration.Customs.US.ICustomsRuleRule>())
			};

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CustomsRuleRule);

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			return GetTypeForCountryCode(GetCustomsRuleCountryCode(row, factory));
		}

		ZString GetCustomsRuleCountryCode(DataRow row, BusinessObjectFactory factory)
		{
			var ruleHeaderPK = row != null ? new ZGuid(row[CustomsRuleRule.Schema.CPR_CPH_PermitHeader]) : ZGuid.Invalid;
			var ruleHeader = ruleHeaderPK.IsValid ? factory.Load<CustomsRule>(ruleHeaderPK) : null;
			var countryCode = ruleHeader?.CPH_RN_NKCountryCode ?? ZString.Empty;
			return countryCode.IsEmpty ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : countryCode;
		}
	}
}
