using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Universal.Testing
{
	public class ZZRefCusRulingCombinedTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		#region Overrides
		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var ruling = bizO as ZZRefCusRulingCombined;
			if (ruling != null)
			{
				RefDataHelper.CreateNewOrGetExistingDataGrouping(countryCode);
				ruling.ZZX_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			RefDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Canada);
			var cusRuling = Factory.New<ZZRefCusRulingCombined>();
			cusRuling.ZZX_Description = "11111 Des";
			cusRuling.ZZX_RulingNumber = "11111";
			cusRuling.ZZX_RulingType = RefCusRulingConfigValuesForAcceptType.Codes.X;
			cusRuling.ZZX_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			return cusRuling;
		}

		protected override Type BaseTypeDecidedType => typeof(ZZRefCusRulingCombined);
		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type> { { Core.Constants.CountryGuids.Canada, ObjectFactory.GetType<Integration.Customs.CA.ICusRuling>() }, { Core.Constants.CountryGuids.SouthAfrica, typeof(ZZRefCusRulingCombined) } };
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
			return new Dictionary<string, Type> { { Core.Constants.CountryCodes.Canada, ObjectFactory.GetType<Integration.Customs.CA.ICusRuling>() }, { Core.Constants.CountryCodes.SouthAfrica, typeof(ZZRefCusRulingCombined) } };
		}

		#endregion
		UniversalReferenceTestDataHelper RefDataHelper => refDataHelper ?? (refDataHelper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper refDataHelper;
	}
}
