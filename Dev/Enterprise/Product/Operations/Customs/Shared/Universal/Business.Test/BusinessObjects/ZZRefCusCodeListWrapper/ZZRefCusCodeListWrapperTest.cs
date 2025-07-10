using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(ZZRefCusCodeListWrapper))]
	class ZZRefCusCodeListWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMatchesFilter()
		{
			var subLocation = GetZZRefCusCodeListWrapper();
			subLocation.Factory.Save();
			var dbOnlyQuery = new ZDBOnlyQuery(typeof(ZZRefCusCodeListCombined));
			CombineAssertions(() =>
			{
				dbOnlyQuery.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Code, "XXXX");
				AssertEquals("Parent Matches", true, subLocation.CusCodeList.MatchesFilter(dbOnlyQuery));
				AssertEquals("Wrapper Matches", true, subLocation.MatchesFilter(dbOnlyQuery));
				dbOnlyQuery.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Code, string.Empty);
				AssertEquals("Parent Not Matching", false, subLocation.CusCodeList.MatchesFilter(dbOnlyQuery));
				AssertEquals("Wrapper Not Matching", false, subLocation.MatchesFilter(dbOnlyQuery));
			});
		}

		public void TestPK()
		{
			var subLocation = GetZZRefCusCodeListWrapper();
			AssertEquals(subLocation.CusCodeList.PK, subLocation.PK);
		}

		public void TestProperties()
		{
			var subLocation = GetZZRefCusCodeListWrapper();
			AssertEquals("XXXX", subLocation.Code);
			AssertEquals("XXXX DESC", subLocation.Description);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ZZRefCusCodeListWrapper(Factory.New<ZZRefCusCodeListCombined>());
		}

		ZZRefCusCodeListWrapper GetZZRefCusCodeListWrapper()
		{
			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping;
			cusCodeList.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			cusCodeList.ZZD_Code = "XXXX";
			cusCodeList.ZZD_Description = "XXXX DESC";
			cusCodeList.ZZD_StartDate = ZDateTime.Today.AddYears(-10);
			cusCodeList.ZZD_EndDate = ZDateTime.Today.AddYears(10);
			return new ZZRefCusCodeListWrapper(cusCodeList);
		}
	}
}
