using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CountryStatesGlbHolidayBizoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckGHC_HolidayName()
		{
			var glbHolidayBizo = new CountryStatesGlbHolidayBizo(Factory, NewGlbHoliday(RefCountry.TablePrefix, RefCountry.PK));
			glbHolidayBizo.GHC_HolidayName = "NAME1";
			Factory.Save();

			var glbHoliday2 = new CountryStatesGlbHolidayBizo(Factory, NewGlbHoliday(RefCountry.TablePrefix, RefCountry.PK));
			glbHoliday2.GHC_HolidayName = "NAM";
			Assert("GHC_HolidayName has error", glbHoliday2.GHC_HolidayNameInfo.HasError("The length of Holiday Name should be at least 4 characters."));
		}

		public void TestCheckGHC_Date()
		{
			var glbHolidayBizo = new CountryStatesGlbHolidayBizo(Factory, NewGlbHoliday(RefCountry.TablePrefix, RefCountry.PK));
			glbHolidayBizo.GHC_HolidayName = "NAME1";
			glbHolidayBizo.GHC_Date = new ZDate(2022, 10, 21);
			Factory.Save();

			var glbHoliday2 = new CountryStatesGlbHolidayBizo(Factory, NewGlbHoliday(RefCountry.TablePrefix, RefCountry.PK));
			glbHoliday2.GHC_HolidayName = "NAME2";
			glbHoliday2.GHC_Date = new ZDate(2022, 10, 21);
			Assert("GHC_Date has error", glbHoliday2.GHC_DateInfo.HasError("This date already exists"));
		}

		public void TestCheckGHC_DateWhenExistCountryThenAddStateLevel()
		{
			var glbHolidayBizo = new CountryStatesGlbHolidayBizo(Factory, NewGlbHoliday(RefCountry.TablePrefix, RefCountry.PK));
			glbHolidayBizo.GHC_HolidayName = "NAME1";
			glbHolidayBizo.GHC_Date = new ZDate(2022, 10, 21);
			Factory.Save();

			var glbHoliday2 = new CountryStatesGlbHolidayBizo(Factory, NewGlbHoliday(RefCountryStates1.TablePrefix, RefCountryStates1.PK));
			glbHoliday2.GHC_HolidayName = "NAME2";
			glbHoliday2.GHC_Date = new ZDate(2022, 10, 21);
			Assert("GHC_Date has error", glbHoliday2.GHC_DateInfo.HasError("This date already exists"));
		}

		public void TestCheckGHC_DateWhenExistStateLevelThenAddCountryLevel()
		{
			var glbHolidayBizo = new CountryStatesGlbHolidayBizo(Factory, NewGlbHoliday(RefCountryStates1.TablePrefix, RefCountryStates1.PK));
			glbHolidayBizo.GHC_HolidayName = "NAME1";
			glbHolidayBizo.GHC_Date = new ZDate(2022, 10, 21);
			Factory.Save();

			var glbHoliday2 = new CountryStatesGlbHolidayBizo(Factory, NewGlbHoliday(RefCountry.TablePrefix, RefCountry.PK));
			glbHoliday2.GHC_HolidayName = "NAME2";
			glbHoliday2.GHC_Date = new ZDate(2022, 10, 21);
			Assert("GHC_Date has error", glbHoliday2.GHC_DateInfo.HasError("This date already exists"));
		}

		public void TestCheckGHC_IsWorkingDayAndGHC_Recurring()
		{
			var glbHolidayBizo = new CountryStatesGlbHolidayBizo(Factory, NewGlbHoliday(RefCountry.TablePrefix, RefCountry.PK));
			glbHolidayBizo.GHC_IsWorkingDay = true;
			Assert("Recurring is read only", glbHolidayBizo.GHC_RecurringInfo.ReadOnly);
			glbHolidayBizo.GHC_Recurring = true;
			Assert("Is Working Day is read only", glbHolidayBizo.GHC_IsWorkingDayInfo.ReadOnly);
		}

		public void TestStateIsSelectedIfStateSpecific()
		{
			var glbHolidayBizo = new CountryStatesGlbHolidayBizo(Factory, NewGlbHoliday(RefCountry.TablePrefix, RefCountry.PK));
			glbHolidayBizo.GHC_IsStateSpecific = true;
			glbHolidayBizo.IsRunningPreSaveValidation = true;
			glbHolidayBizo.RunPreSaveValidation();
			Assert("State Specific has error", glbHolidayBizo.GHC_IsStateSpecificInfo.HasError("At least one State must be selected OR ‘State(s) specific’ checkbox unselected"));

			CountryStatesGlbHolidayEntry firstState = glbHolidayBizo.GHC_CountryStates.FirstOrDefault() as CountryStatesGlbHolidayEntry;
			firstState.IsChecked = true;
			Assert("State Specific has no error", !glbHolidayBizo.GHC_IsStateSpecificInfo.HasError("At least one State must be selected OR ‘State(s) specific’ checkbox unselected"));
		}

		public void TestCheckGHC_IsWorkingDay()
		{
			// country related
			var glbWeekendBizo = NewGlbHoliday(RefCountry.TablePrefix, RefCountry.PK);
			glbWeekendBizo.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;
			glbWeekendBizo.GH_RecurrDay = DayOfWeekCodeList.LocalizedCodes.Saturday;
			glbWeekendBizo.GH_IsWorkingDay = false;
			Factory.Save();

			var glbHolidayBizo = new CountryStatesGlbHolidayBizo(Factory, NewGlbHoliday(RefCountry.TablePrefix, RefCountry.PK));
			glbHolidayBizo.GHC_Date = new ZDate(2022, 10, 28); //Friday
			glbHolidayBizo.GHC_IsWorkingDay = true;

			Assert("Is Working Day has error", glbHolidayBizo.GHC_IsWorkingDayInfo.HasError("The selected date is not a weekend on the country"));

			glbHolidayBizo.GHC_Date = new ZDate(2022, 10, 29); //Saturday
			Assert("Is Working Day has no error", !glbHolidayBizo.GHC_IsWorkingDayInfo.HasError("The selected date is not a weekend on the country"));

			//state related
			//Friday is weekend
			var glbWeekendOnStateBizo = NewGlbHoliday(RefCountryStates1.TablePrefix, RefCountryStates1.PK);
			glbWeekendOnStateBizo.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;
			glbWeekendOnStateBizo.GH_RecurrDay = DayOfWeekCodeList.LocalizedCodes.Friday;
			glbWeekendOnStateBizo.GH_IsWorkingDay = false;
			Factory.Save();

			glbHolidayBizo.GHC_Date = new ZDate(2022, 10, 28);
			Assert("Is Working Day has error", glbHolidayBizo.GHC_IsWorkingDayInfo.HasError("The selected date is not a weekend on the country"));

			glbHolidayBizo.GHC_IsStateSpecific = true;
			CountryStatesGlbHolidayEntry firstState = glbHolidayBizo.GHC_CountryStates.FirstOrDefault(x => ((CountryStatesGlbHolidayEntry)x).StatePK == RefCountryStates1.PK) as CountryStatesGlbHolidayEntry;
			firstState.IsChecked = true;
			Assert("Is Working Day has no error", !glbHolidayBizo.GHC_IsWorkingDayInfo.HasError("The selected date is not a weekend on the country"));

			CountryStatesGlbHolidayEntry anyOtherState = glbHolidayBizo.GHC_CountryStates.FirstOrDefault(x => ((CountryStatesGlbHolidayEntry)x).StatePK != RefCountryStates1.PK) as CountryStatesGlbHolidayEntry;
			anyOtherState.IsChecked = true;

			Assert("Is Working Day has error", glbHolidayBizo.GHC_IsWorkingDayInfo.HasError($"The selected date is not a weekend on the selected state(s): {anyOtherState.StateName}"));
		}

		public void TestCheckGHC_IsWorkingDayStateOverrided()
		{
			// country related
			var glbWeekendBizo = NewGlbHoliday(RefCountry.TablePrefix, RefCountry.PK);
			glbWeekendBizo.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;
			glbWeekendBizo.GH_RecurrDay = DayOfWeekCodeList.LocalizedCodes.Saturday;
			glbWeekendBizo.GH_IsWorkingDay = false;

			var glbWeekendOnCountryOverride = NewGlbHoliday(RefCountryStates1.TablePrefix, RefCountryStates1.PK);
			glbWeekendOnCountryOverride.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;
			glbWeekendOnCountryOverride.GH_RecurrDay = DayOfWeekCodeList.LocalizedCodes.Saturday;
			glbWeekendOnCountryOverride.GH_IsWorkingDay = true;
			Factory.Save();

			var glbHolidayBizo = new CountryStatesGlbHolidayBizo(Factory, NewGlbHoliday(RefCountry.TablePrefix, RefCountry.PK));
			glbHolidayBizo.GHC_Date = new ZDate(2022, 10, 29); //Saturday
			glbHolidayBizo.GHC_IsWorkingDay = true;

			Assert("Is Working Day has no error", !glbHolidayBizo.GHC_IsWorkingDayInfo.HasError("The selected date is not a weekend on the country"));

			glbHolidayBizo.GHC_IsStateSpecific = true;

			CountryStatesGlbHolidayEntry stateWithOverrideWeekend = glbHolidayBizo.GHC_CountryStates.FirstOrDefault(x => ((CountryStatesGlbHolidayEntry)x).StatePK == RefCountryStates1.PK) as CountryStatesGlbHolidayEntry;
			stateWithOverrideWeekend.IsChecked = true;
			Assert("Is Working Day has error", glbHolidayBizo.GHC_IsWorkingDayInfo.HasError($"The selected date is not a weekend on the selected state(s): {stateWithOverrideWeekend.StateName}"));

			//check another state, not included in the error because it follows country weekends
			CountryStatesGlbHolidayEntry anyOtherState = glbHolidayBizo.GHC_CountryStates.FirstOrDefault(x => ((CountryStatesGlbHolidayEntry)x).StatePK != RefCountryStates1.PK) as CountryStatesGlbHolidayEntry;
			anyOtherState.IsChecked = true;

			Assert("Is Working Day has error", glbHolidayBizo.GHC_IsWorkingDayInfo.HasError($"The selected date is not a weekend on the selected state(s): {stateWithOverrideWeekend.StateName}"));
		}

		public void TestCheckGHC_Recurring()
		{
			var glbHoliday = NewGlbHoliday(RefCountry.TablePrefix, RefCountry.PK);
			glbHoliday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;
			glbHoliday.GH_Date = new ZDate(2022, 12, 25);
			glbHoliday.GH_Recurring = true;
			Factory.Save();

			var glbHolidayBizo = new CountryStatesGlbHolidayBizo(Factory, NewGlbHoliday(RefCountry.TablePrefix, RefCountry.PK));
			glbHolidayBizo.GHC_Date = new ZDate(2000, 12, 25); //Any year, non recurring holiday
			Assert("Recurring has error", glbHolidayBizo.GHC_RecurringInfo.HasError("There is an existent holiday with the selected recurring day/month"));

			glbHolidayBizo.GHC_Date = new ZDate(2000, 12, 26);
			Assert("Recurring has no error", !glbHolidayBizo.GHC_RecurringInfo.HasError("There is an existent holiday with the selected recurring day/month"));

			var glbRecurringOnStateBizo = NewGlbHoliday(RefCountryStates1.TablePrefix, RefCountryStates1.PK);
			glbRecurringOnStateBizo.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;
			glbRecurringOnStateBizo.GH_Date = new ZDate(2022, 12, 20);
			glbRecurringOnStateBizo.GH_Recurring = true;
			Factory.Save();

			glbHolidayBizo.GHC_IsStateSpecific = true;
			CountryStatesGlbHolidayEntry firstState = glbHolidayBizo.GHC_CountryStates.FirstOrDefault(x => ((CountryStatesGlbHolidayEntry)x).StatePK == RefCountryStates1.PK) as CountryStatesGlbHolidayEntry;
			firstState.IsChecked = true;

			glbHolidayBizo.GHC_Date = new ZDate(2000, 12, 25);
			Assert("State Specific Recurring has error, matching with Country recurring holiday", glbHolidayBizo.GHC_RecurringInfo.HasError("There is an existent holiday with the selected recurring day/month"));

			glbHolidayBizo.GHC_Date = new ZDate(2000, 12, 20);
			Assert("State Specific Recurring has error, matching with state recurring holiday", glbHolidayBizo.GHC_RecurringInfo.HasError("There is an existent holiday with the selected recurring day/month"));
		}

		public void TestCheckGHC_RecurringWhenExistCountryThenAddStateLevel()
		{
			var glbHoliday = NewGlbHoliday(RefCountry.TablePrefix, RefCountry.PK);
			glbHoliday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;
			glbHoliday.GH_Date = new ZDate(2022, 12, 25);
			glbHoliday.GH_Recurring = true;
			Factory.Save();

			var glbHolidayBizo = new CountryStatesGlbHolidayBizo(Factory, NewGlbHoliday(RefCountryStates1.TablePrefix, RefCountryStates1.PK));
			glbHolidayBizo.GHC_Date = new ZDate(2000, 12, 25);
			Assert("Recurring has error", glbHolidayBizo.GHC_RecurringInfo.HasError("There is an existent holiday with the selected recurring day/month"));

			glbHolidayBizo.GHC_Date = new ZDate(2000, 12, 26);
			Assert("Recurring has no error", !glbHolidayBizo.GHC_RecurringInfo.HasError("There is an existent holiday with the selected recurring day/month"));
		}

		public void TestCheckGHC_RecurringWhenExistStateLevelThenAddCountryLevel()
		{
			var glbHoliday = NewGlbHoliday(RefCountryStates1.TablePrefix, RefCountryStates1.PK);
			glbHoliday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;
			glbHoliday.GH_Date = new ZDate(2022, 12, 25);
			glbHoliday.GH_Recurring = true;
			Factory.Save();

			var glbHolidayBizo = new CountryStatesGlbHolidayBizo(Factory, NewGlbHoliday(RefCountry.TablePrefix, RefCountry.PK));
			glbHolidayBizo.GHC_Date = new ZDate(2000, 12, 25);
			Assert("Recurring has error", glbHolidayBizo.GHC_RecurringInfo.HasError("There is an existent holiday with the selected recurring day/month"));

			glbHolidayBizo.GHC_Date = new ZDate(2000, 12, 26);
			Assert("Recurring has no error", !glbHolidayBizo.GHC_RecurringInfo.HasError("There is an existent holiday with the selected recurring day/month"));
		}

		GlbHoliday NewGlbHoliday(ZString parentTableCode, ZGuid parentPk)
		{
			var glbHoliday = Factory.NewWithValidTestData<GlbHoliday>();
			glbHoliday.GH_ParentID = parentPk;
			glbHoliday.GH_ParentTableCode = parentTableCode;
			Factory.Save();
			return glbHoliday;
		}

		RefCountry RefCountry;
		RefCountryStates RefCountryStates1;
		RefCountryStates RefCountryStates2;
		RefCountryStates RefCountryStates3;
		RefCountryStates RefCountryStates4;

		protected override void SetUp()
		{
			base.SetUp();
			RefCountry = Factory.NewWithValidTestData<RefCountry>();
			RefCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			RefCountryStates1.RW_RN_NKCountryCode = RefCountry.RN_Code;
			RefCountryStates2 = Factory.NewWithValidTestData<RefCountryStates>();
			RefCountryStates2.RW_RN_NKCountryCode = RefCountry.RN_Code;
			RefCountryStates3 = Factory.NewWithValidTestData<RefCountryStates>();
			RefCountryStates3.RW_RN_NKCountryCode = RefCountry.RN_Code;
			RefCountryStates4 = Factory.NewWithValidTestData<RefCountryStates>();
			RefCountryStates4.RW_RN_NKCountryCode = RefCountry.RN_Code;
			Factory.Save();
		}
	}
}
