using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CountryStatesGlbHolidayBizoCollection))]
	public class CountryStatesGlbHolidayBizoCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CountryStatesGlbHolidayBizoCollection>
	{
		protected override CountryStatesGlbHolidayBizoCollection GetCollectionToTest()
		{
			return new CountryStatesGlbHolidayBizoCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CountryStatesGlbHolidayBizo(Factory);
		}

		public void TestCollection()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			var countryState1 = Factory.NewWithValidTestData<RefCountryStates>();
			countryState1.RW_RN_NKCountryCode = country.RN_Code;
			var countryState2 = Factory.NewWithValidTestData<RefCountryStates>();
			countryState2.RW_RN_NKCountryCode = country.RN_Code;
			var holiday1 = Factory.NewWithValidTestData<GlbHoliday>();
			holiday1.GH_ParentTableCode = RefCountryStatesSchema.Constants.Prefix;
			holiday1.GH_ParentID = countryState1.PK;
			holiday1.GH_HolidayName = "Test";
			holiday1.GH_RecurrType = "DAT";
			holiday1.GH_Recurring = true;
			holiday1.GH_RecurrMonth = "";
			holiday1.GH_RecurrDay = "";
			holiday1.GH_IsWorkingDay = true;
			holiday1.GH_IsActive = true;
			holiday1.GH_Date = new CargoWise.Types.ZDateTime(2022, 1, 1, 0, 0, 0);

			var holiday2 = Factory.NewWithValidTestData<GlbHoliday>();
			holiday2.GH_ParentTableCode = RefCountryStatesSchema.Constants.Prefix;
			holiday2.GH_ParentID = countryState2.PK;
			holiday2.GH_HolidayName = "Test";
			holiday2.GH_RecurrType = "DAT";
			holiday2.GH_Recurring = true;
			holiday2.GH_RecurrMonth = "";
			holiday2.GH_RecurrDay = "";
			holiday2.GH_IsWorkingDay = true;
			holiday2.GH_IsActive = true;
			holiday2.GH_Date = new CargoWise.Types.ZDateTime(2022, 1, 1, 0, 0, 0);
			var holiday3 = Factory.NewWithValidTestData<GlbHoliday>();
			holiday3.GH_ParentTableCode = RefCountrySchema.Constants.Prefix;
			holiday3.GH_ParentID = country.PK;
			Factory.Save();

			var collection = GetCollectionToTest();
			collection.Load(new ZQuery());
			AssertEquals("Should be 2 CountryStatesGlbHolidayBizo in the collection", 2, collection.Count);
		}
	}
}
