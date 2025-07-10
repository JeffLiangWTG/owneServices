using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobDecRefsTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var lineRefs = bizO as JobDecRefs;
			if (lineRefs != null)
			{
				lineRefs.Declaration.Branch.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			return declaration.DeclarationRefs.AddNew();
		}

		protected override Type BaseTypeDecidedType => typeof(JobDecRefs);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Brazil, BaseTypeDecidedType },
				{ Core.Constants.CountryGuids._TemplateCountryName_, BaseTypeDecidedType }
			};
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding()
		{
			return GetTestCountryCodesForNewAndBindingTests();
		}

		Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Brazil, BaseTypeDecidedType },
				{ Core.Constants.CountryCodes.Japan, BaseTypeDecidedType },
				{ Core.Constants.CountryCodes.India, BaseTypeDecidedType },
				{ Core.Constants.CountryCodes._TemplateCountryName_, BaseTypeDecidedType }
			};
		}
	}
}
