using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseCusGuaranteeRuleTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var cusGuaranteeRule = bizO as CusGuaranteeRule;
			if (cusGuaranteeRule != null)
			{
				cusGuaranteeRule.GuaranteeHeader.CPH_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			guaranteeHeader.MainAccessCode = "MAC1";
			var rule = guaranteeHeader.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = "TAR";
			rule.CPR_ValueFrom = "1";
			rule.CPR_ValueTo = "2";
			return rule;
		}

		protected override Type BaseTypeDecidedType => typeof(CusGuaranteeRule);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Constants.CountryGuids.Latvia, ObjectFactory.GetType<Integration.Customs.EU.ICusGuaranteeRule>() },
				{ Constants.CountryGuids._TemplateCountryName_, BaseTypeDecidedType }
			};
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesForNewAndBindingTests();

		Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Constants.CountryCodes.Latvia, ObjectFactory.GetType<Integration.Customs.EU.ICusGuaranteeRule>() },
				{ Constants.CountryCodes._TemplateCountryName_, BaseTypeDecidedType }
			};
		}
	}
}
