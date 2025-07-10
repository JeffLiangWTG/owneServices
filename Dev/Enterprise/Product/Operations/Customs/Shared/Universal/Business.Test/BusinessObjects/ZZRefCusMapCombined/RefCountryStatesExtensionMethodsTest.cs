using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal.Testing
{
	public class RefCountryStatesExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestGetCustomsCodeFor()
		{
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.EXPSTA, "OUT", "Export State Mapping", true);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.IMPSTA, "OUT", "Import State Mapping", true);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EXPSTA, "MXAGU", "AG", startDate, endDate, Core.Constants.CountryCodes.UnitedStates);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EXPSTA, "MXBCN", "BN", startDate, endDate, Core.Constants.CountryCodes.UnitedStates);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EXPSTA, "CNZAC", "ZA", startDate, endDate, Core.Constants.CountryCodes.UnitedStates);
			helper.CreateCusMap(RefCusMapTypeList.Codes.IMPSTA, "MXCMX", "DIF", startDate, endDate, Core.Constants.CountryCodes.UnitedStates);
			var refCountryStates1 = CreateTestDataIfNotExist("MX", "AGU");
			var refCountryStates2 = CreateTestDataIfNotExist("MX", "BCN");
			var refCountryStates3 = CreateTestDataIfNotExist("CN", "ZAC");
			var refCountryStates4 = CreateTestDataIfNotExist("CA", "XXX");
			var refCountryStates5 = CreateTestDataIfNotExist("MX", "CMX");
			Factory.Save();
			AssertEquals("AG", refCountryStates1.GetCustomsCodeFor(RefCusMapTypeList.Codes.EXPSTA, Core.Constants.CountryCodes.UnitedStates));
			AssertEquals("BN", refCountryStates2.GetCustomsCodeFor(RefCusMapTypeList.Codes.EXPSTA, Core.Constants.CountryCodes.UnitedStates));
			AssertEquals("ZA", refCountryStates3.GetCustomsCodeFor(RefCusMapTypeList.Codes.EXPSTA, Core.Constants.CountryCodes.UnitedStates));
			AssertEquals("XXX", refCountryStates4.GetCustomsCodeFor(RefCusMapTypeList.Codes.EXPSTA, Core.Constants.CountryCodes.UnitedStates));
			AssertEquals("DIF", refCountryStates5.GetCustomsCodeFor(RefCusMapTypeList.Codes.IMPSTA, Core.Constants.CountryCodes.UnitedStates));
		}

		RefCountryStates CreateTestDataIfNotExist(ZString countryCode, ZString stateCode)
		{
			var query = new ZQuery();
			if (!countryCode.IsEmpty)
			{
				query.AddToFilter(RefCountryStatesSchema.RW_RN_NKCountryCode, countryCode);
			}

			query.AddToFilter(RefCountryStatesSchema.RW_Code, stateCode);
			var result = Factory.LoadTop1<RefCountryStates>(query);
			if (result == null)
			{
				result = Factory.NewWithValidTestData<RefCountryStates>();
				result.RW_RN_NKCountryCode = countryCode;
				result.RW_Code = stateCode;
			}

			return result;
		}
	}
}
