using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CountryStatesGlbHolidayBizo))]
	public class CountryStatesGlbHolidayBizoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			var bizo = new CountryStatesGlbHolidayBizo(Factory);
			AssertEquals("", bizo.GHC_HolidayName);
			AssertEquals("", bizo.GHC_RecurrDay);
			AssertEquals("", bizo.GHC_RecurrType);
			AssertEquals(ZBool.False, bizo.GHC_Recurring);
			AssertEquals(ZBool.False, bizo.GHC_IsWorkingDay);
			AssertEquals(ZBool.False, bizo.GHC_IsActive);
			AssertEquals(ZDate.Empty, bizo.GHC_Date);
		}

		public void TestSave()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			var countryStates = Factory.NewWithValidTestData<RefCountryStates>();
			countryStates.RW_RN_NKCountryCode = country.RN_Code;
			var holiday = Factory.NewWithValidTestData<GlbHoliday>();
			holiday.GH_ParentID = country.PK;
			holiday.GH_ParentTableCode = country.TablePrefix;

			var bizo = new CountryStatesGlbHolidayBizo(Factory, holiday);
			AssertEquals(country.RN_Code, bizo.GHC_CountryCode);
			AssertEquals(1, bizo.GHC_CountryStates.Count);

			var stateEntryInTheList = bizo.GHC_CountryStates[0];
			AssertEquals(false, stateEntryInTheList.IsChecked);
			AssertEquals(countryStates.RW_Description, stateEntryInTheList.StateName);
			AssertEquals(holiday.PK, bizo.GHC_PK);

			stateEntryInTheList.IsChecked = true;
			Factory.Save();

			var holidayCreatedForStateLevel = Factory.LoadTop1<GlbHoliday>(new ZQuery(GlbHolidaySchema.GH_ParentID, countryStates.PK));
			AssertNotNull(holidayCreatedForStateLevel);
			AssertEquals(true, bizo.GHC_IsStateSpecific);
			AssertEquals(holidayCreatedForStateLevel.PK, bizo.GHC_PK);
			AssertEquals(holidayCreatedForStateLevel.GH_HolidayName, bizo.GHC_HolidayName);
			AssertEquals(holidayCreatedForStateLevel.GH_Date, bizo.GHC_Date);
			AssertEquals(holidayCreatedForStateLevel.GH_IsActive, bizo.GHC_IsActive);

			var holidayCountryLevel = Factory.LoadTop1<GlbHoliday>(new ZQuery(GlbHolidaySchema.GH_ParentID, country.PK));
			AssertNull(holidayCountryLevel);
		}

		public void TestDelete()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			var countryStates = Factory.NewWithValidTestData<RefCountryStates>();
			countryStates.RW_RN_NKCountryCode = country.RN_Code;
			var countryStates2 = Factory.NewWithValidTestData<RefCountryStates>();
			countryStates2.RW_RN_NKCountryCode = country.RN_Code;
			var countryStates3 = Factory.NewWithValidTestData<RefCountryStates>();
			countryStates3.RW_RN_NKCountryCode = country.RN_Code;
			var countryStates4 = Factory.NewWithValidTestData<RefCountryStates>();
			countryStates4.RW_RN_NKCountryCode = country.RN_Code;
			var holiday1 = CreateSameHoliday(countryStates.TablePrefix, countryStates.PK);
			var holiday2 = CreateSameHoliday(countryStates2.TablePrefix, countryStates2.PK);
			var holiday3 = CreateSameHoliday(countryStates3.TablePrefix, countryStates3.PK);
			var holiday4 = CreateSameHoliday(countryStates4.TablePrefix, countryStates4.PK);

			var bizo = new CountryStatesGlbHolidayBizo(Factory, holiday1);
			AssertEquals(country.RN_Code, bizo.GHC_CountryCode);
			AssertEquals(4, bizo.GHC_CountryStates.Count);
			AssertEquals(4, bizo.GHC_CountryStates.Count(x => ((CountryStatesGlbHolidayEntry)x).IsChecked));

			bizo.Delete();

			AssertNull(Factory.Load<GlbHoliday>(holiday1.PK));
			AssertNull(Factory.Load<GlbHoliday>(holiday2.PK));
			AssertNull(Factory.Load<GlbHoliday>(holiday3.PK));
			AssertNull(Factory.Load<GlbHoliday>(holiday4.PK));
		}

		public void TestSelectAllIsFalseWhenOnlyFilledCountry()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			var holiday = Factory.NewWithValidTestData<GlbHoliday>();
			holiday.GH_ParentID = country.PK;
			holiday.GH_ParentTableCode = country.TablePrefix;
			var bizo = new CountryStatesGlbHolidayBizo(Factory, holiday);
			AssertEquals(false, bizo.CountryStatesGlbHolidayEntrySelectAll);
			AssertEquals(false, bizo.GHC_IsStateSpecific);
		}

		public void TestValidateAllOnlyOnce_WhenSelectAll()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			var countryStates = Factory.NewWithValidTestData<RefCountryStates>();
			countryStates.RW_RN_NKCountryCode = country.RN_Code;
			var countryStates2 = Factory.NewWithValidTestData<RefCountryStates>();
			countryStates2.RW_RN_NKCountryCode = country.RN_Code;
			var countryStates3 = Factory.NewWithValidTestData<RefCountryStates>();
			countryStates3.RW_RN_NKCountryCode = country.RN_Code;
			var holiday = Factory.NewWithValidTestData<GlbHoliday>();
			holiday.GH_ParentID = country.PK;
			holiday.GH_ParentTableCode = country.TablePrefix;
			var glbHolidyBizo = new CountryStatesGlbHolidayBizoForTest(Factory, holiday);
			glbHolidyBizo.GHC_IsStateSpecific = true;
			glbHolidyBizo.CountryStatesGlbHolidayEntrySelectAll = true;
			Assert(glbHolidyBizo.GHC_CountryStates.All(x => ((CountryStatesGlbHolidayEntry)x).IsChecked));
			glbHolidyBizo.ValidationMock.Verify(x => x.ValidateAll(), Times.Once);
		}

		public void TestValidateAllTimesTwo_WhenNoSelectAll()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			var countryStates = Factory.NewWithValidTestData<RefCountryStates>();
			countryStates.RW_RN_NKCountryCode = country.RN_Code;
			var countryStates2 = Factory.NewWithValidTestData<RefCountryStates>();
			countryStates2.RW_RN_NKCountryCode = country.RN_Code;
			var holiday = Factory.NewWithValidTestData<GlbHoliday>();
			holiday.GH_ParentID = country.PK;
			holiday.GH_ParentTableCode = country.TablePrefix;
			var glbHolidyBizo = new CountryStatesGlbHolidayBizoForTest(Factory, holiday);
			glbHolidyBizo.GHC_IsStateSpecific = true;
			foreach (CountryStatesGlbHolidayEntry entry in glbHolidyBizo.GHC_CountryStates)
			{
				entry.IsChecked = true;
			}
			Assert(glbHolidyBizo.GHC_CountryStates.All(x => ((CountryStatesGlbHolidayEntry)x).IsChecked));
			glbHolidyBizo.ValidationMock.Verify(x => x.ValidateAll(), Times.Exactly(2));
		}

		public void TestSelectAll()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			var bizo = CreateHolidayForSelectAll(country);
			AssertEquals(4, bizo.GHC_CountryStates.Count);
			AssertEquals(0, bizo.GHC_CountryStates.Count(x => ((CountryStatesGlbHolidayEntry)x).IsChecked));
			AssertEquals(false, bizo.CountryStatesGlbHolidayEntrySelectAll);

			var stateEntryInTheList = bizo.GHC_CountryStates[0];
			bizo.GHC_IsStateSpecific = true;
			bizo.CountryStatesGlbHolidayEntrySelectAll = true;
			Factory.Save();
			AssertEquals(country.RN_Code, bizo.GHC_CountryCode);
			AssertEquals(true, stateEntryInTheList.IsChecked);
			AssertEquals(4, bizo.GHC_CountryStates.Count(x => ((CountryStatesGlbHolidayEntry)x).IsChecked));
			bizo.GHC_CountryStates.RemoveAll();
			bizo.GHC_CountryStates.Load();
			AssertEquals(true, bizo.CountryStatesGlbHolidayEntrySelectAll);
			bizo.CountryStatesGlbHolidayEntrySelectAll = false;

			Factory.Save();
			bizo.GHC_CountryStates.RemoveAll();
			bizo.GHC_CountryStates.Load();
			AssertEquals(false, bizo.GHC_CountryStates[0].IsChecked);
			AssertEquals(0, bizo.GHC_CountryStates.Count(x => ((CountryStatesGlbHolidayEntry)x).IsChecked));
			AssertEquals(true, bizo.GHC_IsStateSpecific);
		}

		public void TestSelectAllIsFalse_WhenSelectPartStates()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			var bizo = CreateHolidayForSelectAll(country);

			AssertEquals(4, bizo.GHC_CountryStates.Count);
			AssertEquals(0, bizo.GHC_CountryStates.Count(x => ((CountryStatesGlbHolidayEntry)x).IsChecked));
			AssertEquals(false, bizo.CountryStatesGlbHolidayEntrySelectAll);
			AssertEquals(false, bizo.GHC_IsStateSpecific);
			bizo.GHC_IsStateSpecific = true;
			var stateEntryInTheList = bizo.GHC_CountryStates[0];
			stateEntryInTheList.IsChecked = true;
			Factory.Save();
			AssertEquals(country.RN_Code, bizo.GHC_CountryCode);
			AssertEquals(true, stateEntryInTheList.IsChecked);
			bizo.GHC_CountryStates.RemoveAll();
			bizo.GHC_CountryStates.Load();
			AssertEquals(false, bizo.CountryStatesGlbHolidayEntrySelectAll);
			AssertEquals(1, bizo.GHC_CountryStates.Count(x => ((CountryStatesGlbHolidayEntry)x).IsChecked));
			AssertEquals(true, bizo.GHC_IsStateSpecific);
		}

		public void TestSelectAllIsTrue_WhenSelectAllStates()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			var bizo = CreateHolidayForSelectAll(country);

			AssertEquals(4, bizo.GHC_CountryStates.Count);
			AssertEquals(0, bizo.GHC_CountryStates.Count(x => ((CountryStatesGlbHolidayEntry)x).IsChecked));
			AssertEquals(false, bizo.CountryStatesGlbHolidayEntrySelectAll);
			AssertEquals(false, bizo.GHC_IsStateSpecific);
			bizo.GHC_IsStateSpecific = true;
			foreach (CountryStatesGlbHolidayEntry entry in bizo.GHC_CountryStates)
			{
				entry.IsChecked = true;
			}
			Factory.Save();
			bizo.GHC_CountryStates.RemoveAll();
			bizo.GHC_CountryStates.Load();
			AssertEquals(true, bizo.CountryStatesGlbHolidayEntrySelectAll);
			AssertEquals(4, bizo.GHC_CountryStates.Count(x => ((CountryStatesGlbHolidayEntry)x).IsChecked));
			AssertEquals(true, bizo.GHC_IsStateSpecific);
		}

		CountryStatesGlbHolidayBizo CreateHolidayForSelectAll(RefCountry country)
		{
			var countryStates = Factory.NewWithValidTestData<RefCountryStates>();
			countryStates.RW_RN_NKCountryCode = country.RN_Code;
			var countryStates2 = Factory.NewWithValidTestData<RefCountryStates>();
			countryStates2.RW_RN_NKCountryCode = country.RN_Code;
			var countryStates3 = Factory.NewWithValidTestData<RefCountryStates>();
			countryStates3.RW_RN_NKCountryCode = country.RN_Code;
			var countryStates4 = Factory.NewWithValidTestData<RefCountryStates>();
			countryStates4.RW_RN_NKCountryCode = country.RN_Code;
			var holiday = Factory.NewWithValidTestData<GlbHoliday>();
			holiday.GH_ParentID = country.PK;
			holiday.GH_ParentTableCode = country.TablePrefix;
			var bizo = new CountryStatesGlbHolidayBizo(Factory, holiday);
			return bizo;
		}

		GlbHoliday CreateSameHoliday(ZString tableCode, ZGuid parentPk)
		{
			var holiday1 = Factory.NewWithValidTestData<GlbHoliday>();
			holiday1.GH_ParentTableCode = tableCode;
			holiday1.GH_ParentID = parentPk;
			holiday1.GH_HolidayName = "Test";
			holiday1.GH_RecurrType = "DAT";
			holiday1.GH_Recurring = true;
			holiday1.GH_RecurrMonth = "";
			holiday1.GH_RecurrDay = "";
			holiday1.GH_IsWorkingDay = true;
			holiday1.GH_IsActive = true;
			holiday1.GH_Date = new ZDateTime(2022, 1, 1, 0, 0, 0);
			return holiday1;
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CountryStatesGlbHolidayBizo(Factory);
		}

		#endregion
	}

	class CountryStatesGlbHolidayBizoForTest : CountryStatesGlbHolidayBizo
	{
		public CountryStatesGlbHolidayBizoForTest(BusinessObjectFactory factory, GlbHoliday holiday) : base(factory, holiday)
		{
		}

		protected override CountryStatesGlbHolidayBizoValidation GetNewValidation()
		{
			return ValidationMock.Object;
		}

		public Mock<CountryStatesGlbHolidayBizoValidation> ValidationMock
		{
			get
			{
				if (validationMock == null)
				{
					validationMock = new Mock<CountryStatesGlbHolidayBizoValidation>(this);
				}
				return validationMock;
			}
		}

		Mock<CountryStatesGlbHolidayBizoValidation> validationMock;
	}
}
