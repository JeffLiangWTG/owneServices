using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusMiscRequestHeaderTypeDecider : CountrySpecificTypeDeciderTest
	{
		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var requestHeader = (CusMiscRequestHeader)bizO;
			requestHeader.Branch.Company.GC_RN_NKCountryCode = countryCode;
		}
		protected override Type BaseTypeDecidedType => typeof(CusMiscRequestHeader);
		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var result = Factory.NewWithValidTestData<CusMiscRequestHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			result.CMR_MessageType = "5GW";//db constraint
			return result;
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesForNewAndBindingTests();
		static Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.KoreaSouth, ObjectFactory.GetType<Integration.Customs.KR.ICusMiscRequestHeader>() },
			};
		}

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.KoreaRepublicof, ObjectFactory.GetType<Integration.Customs.KR.ICusMiscRequestHeader>() },
			};
		}
	}
}
