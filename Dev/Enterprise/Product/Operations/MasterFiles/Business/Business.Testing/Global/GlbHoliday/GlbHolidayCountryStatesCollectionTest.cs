using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbHolidayCountryStatesCollection))]
	public class GlbHolidayCountryStatesCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbHolidayCountryStatesCollection>
	{
		public void TestGetCountryStateHolidays()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			var countryState = Factory.NewWithValidTestData<RefCountryStates>();
			countryState.RW_RN_NKCountryCode = country.RN_Code;
			var holiday1 = Factory.NewWithValidTestData<GlbHoliday>();
			holiday1.GH_ParentTableCode = RefCountryStatesSchema.Constants.Prefix;
			holiday1.GH_ParentID = countryState.PK;

			var holiday2 = Factory.NewWithValidTestData<GlbHoliday>();
			holiday2.GH_ParentTableCode = RefCountryStatesSchema.Constants.Prefix;
			holiday2.GH_ParentID = countryState.PK;

			var collection = new GlbHolidayCountryStatesCollection(countryState, Factory, GlbHolidayCountryStateCollectionTypes.Holiday);
			AssertEquals(2, collection.Count);
			AssertEquals(holiday1.PK, collection[0].PK);
			AssertEquals(holiday2.PK, collection[1].PK);
		}

		public void TestGetCountryStateWeekends()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			var countryState = Factory.NewWithValidTestData<RefCountryStates>();
			countryState.RW_RN_NKCountryCode = country.RN_Code;
			var holiday = Factory.NewWithValidTestData<GlbHoliday>();
			holiday.GH_ParentTableCode = RefCountryStatesSchema.Constants.Prefix;
			holiday.GH_ParentID = countryState.PK;

			var weekendHoliday = Factory.NewWithValidTestData<GlbHoliday>();
			weekendHoliday.GH_ParentTableCode = RefCountryStatesSchema.Constants.Prefix;
			weekendHoliday.GH_ParentID = countryState.PK;
			weekendHoliday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;

			var weekendHoliday2 = Factory.NewWithValidTestData<GlbHoliday>();
			weekendHoliday2.GH_ParentTableCode = RefCountryStatesSchema.Constants.Prefix;
			weekendHoliday2.GH_ParentID = countryState.PK;
			weekendHoliday2.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;

			var collection = new GlbHolidayCountryStatesCollection(countryState, Factory, GlbHolidayCountryStateCollectionTypes.Holiday);
			AssertEquals(1, collection.Count);

			var collectionWithWeeklyHolidays = new GlbHolidayCountryStatesCollection(countryState, Factory, GlbHolidayCountryStateCollectionTypes.Weekend);
			AssertEquals(2, collectionWithWeeklyHolidays.Count);
		}

		public void TestGetCountryHolidays()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			var holiday = Factory.NewWithValidTestData<GlbHoliday>();
			holiday.GH_ParentTableCode = RefCountrySchema.Constants.Prefix;
			holiday.GH_ParentID = country.PK;

			var collection = new GlbHolidayCountryStatesCollection(country, Factory, GlbHolidayCountryStateCollectionTypes.Holiday);
			AssertEquals(1, collection.Count);
		}

		public void TestGetCountryWeekends()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			var holiday = Factory.NewWithValidTestData<GlbHoliday>();
			holiday.GH_ParentTableCode = RefCountrySchema.Constants.Prefix;
			holiday.GH_ParentID = country.PK;

			var weeklyHoliday = Factory.NewWithValidTestData<GlbHoliday>();
			weeklyHoliday.GH_ParentTableCode = RefCountrySchema.Constants.Prefix;
			weeklyHoliday.GH_ParentID = country.PK;
			weeklyHoliday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;

			var weekendHoliday2 = Factory.NewWithValidTestData<GlbHoliday>();
			weekendHoliday2.GH_ParentTableCode = RefCountrySchema.Constants.Prefix;
			weekendHoliday2.GH_ParentID = country.PK;
			weekendHoliday2.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;

			var collection = new GlbHolidayCountryStatesCollection(country, Factory, GlbHolidayCountryStateCollectionTypes.Holiday);
			AssertEquals(1, collection.Count);

			var collectionWithWeeklyHolidays = new GlbHolidayCountryStatesCollection(country, Factory, GlbHolidayCountryStateCollectionTypes.Weekend);
			AssertEquals(2, collectionWithWeeklyHolidays.Count);
		}

		protected override GlbHolidayCountryStatesCollection GetCollectionToTest()
		{
			var countryState = Factory.NewWithValidTestData<RefCountryStates>();
			var country = Factory.NewWithValidTestData<RefCountry>();
			countryState.RW_RN_NKCountryCode = country.RN_Code;
			return new GlbHolidayCountryStatesCollection(countryState, Factory, GlbHolidayCountryStateCollectionTypes.Holiday);
		}
	}
}
