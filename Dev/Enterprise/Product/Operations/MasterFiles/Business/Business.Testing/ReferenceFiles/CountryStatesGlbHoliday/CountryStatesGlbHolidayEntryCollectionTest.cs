using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CountryStatesGlbHolidayEntryCollection))]
	public class CountryStatesGlbHolidayEntryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CountryStatesGlbHolidayEntryCollection>
	{
		protected override CountryStatesGlbHolidayEntryCollection GetCollectionToTest()
		{
			var parentBizo = new CountryStatesGlbHolidayBizo(Factory);
			parentBizo.GHC_CountryCode = "AU";
			var collection = new CountryStatesGlbHolidayEntryCollection(parentBizo, Factory);
			collection.CountryCode = "AU";
			return collection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CountryStatesGlbHolidayEntry();
		}

		public void TestLoadCollection()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			var countryState1 = Factory.NewWithValidTestData<RefCountryStates>();
			countryState1.RW_RN_NKCountryCode = country.RN_Code;
			var countryState2 = Factory.NewWithValidTestData<RefCountryStates>();
			countryState2.RW_RN_NKCountryCode = country.RN_Code;
			var countryState3 = Factory.NewWithValidTestData<RefCountryStates>();
			countryState3.RW_RN_NKCountryCode = country.RN_Code;
			var countryState4 = Factory.NewWithValidTestData<RefCountryStates>();
			countryState4.RW_RN_NKCountryCode = country.RN_Code;
			var countryState5 = Factory.NewWithValidTestData<RefCountryStates>();
			countryState5.RW_RN_NKCountryCode = country.RN_Code;
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

			var parentBizo = new CountryStatesGlbHolidayBizo(Factory, holiday1);
			var collection = new CountryStatesGlbHolidayEntryCollection(parentBizo, Factory);
			collection.CountryCode = country.RN_Code;
			collection.Load();
			AssertEquals("Should be 5 CountryStatesGlbHolidayEntry in the collection", 5, collection.Count);
			AssertEquals("Should be 2 CountryStatesGlbHolidayEntry.IsChecked=true in the collection", 2, collection.Count(x => ((CountryStatesGlbHolidayEntry)x).IsChecked));
			AssertEquals("Should be 3 CountryStatesGlbHolidayEntry.IsChecked=false in the collection", 3, collection.Count(x => !((CountryStatesGlbHolidayEntry)x).IsChecked));
		}
	}
}
