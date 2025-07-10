using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class CountryStatesGlbHolidayBizoValidation : ZValidation
	{
		protected readonly CountryStatesGlbHolidayBizo Parent;
		readonly IValidationInternals ZValidationInternals;

		public CountryStatesGlbHolidayBizoValidation(CountryStatesGlbHolidayBizo parent)
			: base(parent)
		{
			Parent = parent;
			ZValidationInternals = this;
		}

		public override Type AutoValidationType => typeof(CountryStatesGlbHolidayBizo);

		#region GHC_CountryCode

		public void ValidateGHC_CountryCode()
		{
			ZValidationInternals.Validate(Parent.GHC_CountryCodeInfo, GetGHC_CountryCodeValidationInvoker());
		}

		RunValidationInvoker GetGHC_CountryCodeValidationInvoker()
		{
			return delegate
			{
				CheckGHC_CountryCodeIsWesternEuropean();
				CheckGHC_CountryCode();
			};
		}

		protected void CheckGHC_CountryCodeIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.GHC_CountryCodeInfo);
		}

		protected void CheckGHC_CountryCode()
		{
			MandatoryValidation.CheckEntered(Parent.GHC_CountryCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.GHC_CountryCodeInfo, Parent.Countries);
		}

		#endregion

		#region GHC_HolidayName

		public void ValidateGHC_HolidayName()
		{
			ZValidationInternals.Validate(Parent.GHC_HolidayNameInfo, GetGHC_HolidayNameValidationInvoker());
		}

		RunValidationInvoker GetGHC_HolidayNameValidationInvoker()
		{
			return delegate
			{
				CheckGHC_HolidayNameIsWesternEuropean();
				CheckGHC_HolidayName();
			};
		}

		protected void CheckGHC_HolidayNameIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.GHC_HolidayNameInfo);
		}

		protected void CheckGHC_HolidayName()
		{
			MandatoryValidation.CheckEntered(Parent.GHC_HolidayNameInfo);
			if (!Parent.GHC_HolidayNameInfo.HasErrors())
			{
				if (Parent.GHC_HolidayName.Length < 4)
				{
					Parent.GHC_HolidayNameInfo.AddError(Res.GetString("3C3A1DC4-F566-4B77-A858-5809A9D2FE5F", "The length of Holiday Name should be at least 4 characters."));
				}
				else
				{
					var holidayExistsInDb = CheckDuplicateRecordInDbByCondition(new ZQuery(GlbHolidaySchema.GH_HolidayName, Parent.GHC_HolidayName));
					if (holidayExistsInDb)
					{
						Parent.GHC_HolidayNameInfo.AddError(Res.GetString("169991CE-BE72-4E0A-B681-19AFD74FFBEB", "Description already exists"));
					}
				}
			}
		}

		#endregion

		#region GHC_Date

		public void ValidateGHC_Date()
		{
			ZValidationInternals.Validate(Parent.GHC_DateInfo, GetGHC_DateValidationInvoker());
		}

		RunValidationInvoker GetGHC_DateValidationInvoker()
		{
			return delegate
			{
				CheckGHC_DateIsValidZDateAndRange();
				CheckGHC_Date();
			};
		}

		protected void CheckGHC_DateIsValidZDateAndRange()
		{
			TypeValidation.CheckValidZDateAndRange(Parent.GHC_DateInfo);
		}

		protected void CheckGHC_Date()
		{
			MandatoryValidation.CheckEntered(Parent.GHC_DateInfo);
			var hasSameDateIndb = CheckDuplicateRecordInDbByCondition(new ZQuery(GlbHolidaySchema.GH_Date, Parent.GHC_Date));
			if (hasSameDateIndb)
			{
				Parent.GHC_DateInfo.AddError(Res.GetString("AAF6E8CB-AD09-48B1-AF9E-28A62A7346E5", "This date already exists"));
			}
			ValidateGHC_IsWorkingDay();
			ValidateGHC_Recurring();
		}

		#endregion

		#region GHC_IsStateSpecific

		public void ValidateGHC_IsStateSpecific()
		{
			ZValidationInternals.Validate(Parent.GHC_IsStateSpecificInfo, GetGHC_IsStateSpecificValidationInvoker());
		}

		RunValidationInvoker GetGHC_IsStateSpecificValidationInvoker()
		{
			return delegate
			{
				CheckGHC_IsStateSpecific();
			};
		}

		protected void CheckGHC_IsStateSpecific()
		{
			if (Parent.IsRunningPreSaveValidation && Parent.GHC_IsStateSpecific)
			{
				CheckOneStateIsSelected();
			}
			ValidateGHC_HolidayName();
			ValidateGHC_Date();
			ValidateGHC_Recurring();
			ValidateGHC_IsWorkingDay();
		}

		void CheckOneStateIsSelected()
		{
			if (!Parent.GHC_CountryStates.Any(x => ((CountryStatesGlbHolidayEntry)x).IsChecked))
			{
				Parent.GHC_IsStateSpecificInfo.AddError(Res.GetString("64876F68-CFBD-437B-8EBB-41308A407122", "At least one State must be selected OR ‘State(s) specific’ checkbox unselected"));
			}
		}

		#endregion

		#region GHC_IsWorkingDay

		public void ValidateGHC_IsWorkingDay()
		{
			ZValidationInternals.Validate(Parent.GHC_IsWorkingDayInfo, GetGHC_IsWorkingDayValidationInvoker());
		}

		RunValidationInvoker GetGHC_IsWorkingDayValidationInvoker()
		{
			return delegate
			{
				CheckGHC_IsWorkingDay();
			};
		}

		protected void CheckGHC_IsWorkingDay()
		{
			var factory = Parent.Factory;
			if (!Parent.GHC_CountryCode.IsEmpty && !Parent.GHC_Date.IsEmpty && Parent.GHC_IsWorkingDay)
			{
				var checkedStatesPks = Parent.GHC_CountryStates.Where(x => x.IsChecked).Select(x => x.StatePK);
				var dayOfWeek = GetShortDayOfWeek(Parent.GHC_Date.DayOfWeek);
				var country = new RefCountry.Loader(factory).LoadForCountry(Parent.GHC_CountryCode);
				if (!checkedStatesPks.Any())
				{
					if (country != null && !country.Weekends.Any(x => x.GH_RecurrDay == dayOfWeek && !x.GH_IsWorkingDay))
					{
						Parent.GHC_IsWorkingDayInfo.AddError(Res.GetString("8EDA9F85-DDA8-4565-BD2B-001CA0E12809", "The selected date is not a weekend on the country"));
					}
				}
				else
				{
					var query = new ZQuery(GlbHolidaySchema.GH_ParentID, checkedStatesPks);
					query.AddToFilter(GlbHolidaySchema.GH_ParentTableCode, RefCountryStatesSchema.Constants.Prefix);
					query.AddToFilter(GlbHolidaySchema.GH_RecurrType, GlbHolidayRecurTypeCodeList.Codes.Weekly);
					query.AddToFilter(GlbHolidaySchema.GH_RecurrDay, dayOfWeek);

					var recordsInDb = factory.Load<GlbHoliday>(query);
					var weekendsInDb = recordsInDb.Where(x => !x.GH_IsWorkingDay);
					if (!recordsInDb.Any())
					{
						//no records at all means this state does not override country
						if (country != null && !country.Weekends.Any(x => x.GH_RecurrDay == dayOfWeek && !x.GH_IsWorkingDay))
						{
							Parent.GHC_IsWorkingDayInfo.AddError(Res.GetString("4E9FE04F-9992-4ADC-8B83-52A218999F03", "The selected date is not a weekend on the country/states"));
						}
					}
					else if (recordsInDb.Any() && !weekendsInDb.Any())
					{
						//there is records, but none of them are weekends, then it should complain.
						var statesWithOverrides = recordsInDb.Select(x => x.GH_ParentID);
						var stateNames = Parent.GHC_CountryStates.Where(x => statesWithOverrides.Contains(x.StatePK)).Select(x => x.StateName);
						Parent.GHC_IsWorkingDayInfo.AddError(Res.GetString("0794EFFF-7340-4D26-8ABF-F3428F7CC77B", "The selected date is not a weekend on the selected state(s): {0}", string.Join(", ", stateNames)));
					}
					else if (weekendsInDb.Count() < checkedStatesPks.Count())
					{
						//there is weekends, but some of the checked states don't comply with it
						var diffRecords = checkedStatesPks.Except(recordsInDb.Select(x => x.GH_ParentID));
						var stateNames = Parent.GHC_CountryStates.Where(x => diffRecords.Contains(x.StatePK)).Select(x => x.StateName);
						Parent.GHC_IsWorkingDayInfo.AddError(Res.GetString("B50B1F81-709F-462D-BA85-3457896341E2", "The selected date is not a weekend on the selected state(s): {0}", string.Join(", ", stateNames)));
					}
				}
			}
		}

		string GetShortDayOfWeek(DayOfWeek dayOfWeek)
		{
			switch (dayOfWeek)
			{
				case DayOfWeek.Monday:
					return DayOfWeekCodeList.LocalizedCodes.Monday;
				case DayOfWeek.Tuesday:
					return DayOfWeekCodeList.LocalizedCodes.Tuesday;
				case DayOfWeek.Wednesday:
					return DayOfWeekCodeList.LocalizedCodes.Wednesday;
				case DayOfWeek.Thursday:
					return DayOfWeekCodeList.LocalizedCodes.Thursday;
				case DayOfWeek.Friday:
					return DayOfWeekCodeList.LocalizedCodes.Friday;
				case DayOfWeek.Saturday:
					return DayOfWeekCodeList.LocalizedCodes.Saturday;
				case DayOfWeek.Sunday:
					return DayOfWeekCodeList.LocalizedCodes.Sunday;
			}
			return string.Empty;
		}

		#endregion

		#region GHC_Recurring

		public void ValidateGHC_Recurring()
		{
			ZValidationInternals.Validate(Parent.GHC_RecurringInfo, GetGHC_RecurringValidationInvoker());
		}

		RunValidationInvoker GetGHC_RecurringValidationInvoker()
		{
			return delegate
			{
				CheckGHC_Recurring();
			};
		}

		protected void CheckGHC_Recurring()
		{
			if (!Parent.GHC_CountryCode.IsEmpty && !Parent.GHC_Date.IsEmpty)
			{
				var errorMessage = Res.GetString("47770B2B-79BD-44AD-94CE-87643D39A2E0", "There is an existent holiday with the selected recurring day/month");
				if (CheckDuplicateRecordInDb(new ZQuery(GlbHolidaySchema.GH_Recurring, 1), isCheckGHCRecurring: true))
				{
					Parent.GHC_RecurringInfo.AddError(errorMessage);
				}
			}
		}

		#endregion

		#region GHC_RecurrDay

		public void ValidateGHC_RecurrDay()
		{
			ZValidationInternals.Validate(Parent.GHC_RecurrDayInfo, GetGHC_RecurrDayValidationInvoker());
		}

		RunValidationInvoker GetGHC_RecurrDayValidationInvoker()
		{
			return delegate
			{
				CheckGHC_RecurrDayIsWesternEuropean();
				CheckGHC_RecurrDay();
			};
		}

		protected void CheckGHC_RecurrDayIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.GHC_RecurrDayInfo);
		}

		protected void CheckGHC_RecurrDay()
		{
		}

		#endregion

		#region GHC_RecurrType

		public void ValidateGHC_RecurrType()
		{
			ZValidationInternals.Validate(Parent.GHC_RecurrTypeInfo, GetGHC_RecurrTypeValidationInvoker());
		}

		RunValidationInvoker GetGHC_RecurrTypeValidationInvoker()
		{
			return delegate
			{
				CheckGHC_RecurrTypeIsWesternEuropean();
				CheckGHC_RecurrType();
			};
		}

		protected void CheckGHC_RecurrTypeIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.GHC_RecurrTypeInfo);
		}

		protected void CheckGHC_RecurrType()
		{
		}

		#endregion

		#region GHC_IsActive

		public void ValidateGHC_IsActive()
		{
			ZValidationInternals.Validate(Parent.GHC_IsActiveInfo, GetGHC_IsActiveValidationInvoker());
		}

		RunValidationInvoker GetGHC_IsActiveValidationInvoker()
		{
			return delegate
			{
				CheckGHC_IsActive();
			};
		}

		protected virtual void CheckGHC_IsActive()
		{
		}

		#endregion

		bool CheckDuplicateRecordInDbByCondition(ZQuery condition)
		{
			if (!Parent.GHC_CountryCode.IsEmpty)
			{
				return CheckDuplicateRecordInDb(condition, isCheckGHCRecurring: false);
			}
			return false;
		}

		bool CheckDuplicateRecordInDb(ZQuery condition, bool isCheckGHCRecurring)
		{
			var factory = Parent.Factory;
			var day = Parent.GHC_Date.Day;
			var month = Parent.GHC_Date.Month;
			var country = new RefCountry.Loader(factory).LoadForCountry(Parent.GHC_CountryCode);
			if (country != null)
			{
				var checkedStatesPks = Parent.GHC_CountryStates.Where(x => x.IsChecked).Select(x => x.StatePK);
				var holidayPksUnderCheckedStates = Parent.GHC_CountryStates.Where(x => x.IsChecked && x.HolidayPK != null).Select(x => x.HolidayPK);
				var statePksUnderCurrentCountry = Parent.GHC_CountryStates.Select(x => x.StatePK).ToArray();
				var currentCountryAndStatesPks = new ZGuid[] { country.PK }.Union(statePksUnderCurrentCountry);

				var queryHolidaysInCurrentCountry = new ZQuery(condition);
				queryHolidaysInCurrentCountry.AddToFilter(GlbHolidaySchema.GH_ParentID, SQLComparisonOperator.Equal, currentCountryAndStatesPks);
				var currentCountryHoliday = factory.Load<GlbHoliday>(queryHolidaysInCurrentCountry);
				if (isCheckGHCRecurring)
				{
					currentCountryHoliday = currentCountryHoliday.Where(x => !x.GH_Date.IsEmpty && x.GH_Date.Month == month && x.GH_Date.Day == day).ToArray();
				}
				if (!checkedStatesPks.Any())
				{
					var isExistCountryHoliday = currentCountryHoliday.Any(x => x.GH_ParentTableCode.Equals(RefCountrySchema.Constants.Prefix) && x.GH_ParentID.Equals(country.PK) && !x.PK.Equals(Parent.GHC_PK));
					if (isExistCountryHoliday)
					{
						return true;
					}
					// If the current Holiday does not check state, then it passes the validation.
					if (!isCheckGHCRecurring && (statePksUnderCurrentCountry == null || statePksUnderCurrentCountry.Length == 0))
					{
						return false;
					}
					// If the current is edit form, then Parent.GHC_PK should already be an existing PK.
					var stateHolidayPks = currentCountryHoliday.Where(x => x.GH_ParentTableCode.Equals(RefCountryStatesSchema.Constants.Prefix) && statePksUnderCurrentCountry.Contains(x.GH_ParentID)).Select(x => x.PK);
					if (stateHolidayPks.Contains(Parent.GHC_PK))
					{
						return false;
					}
					var isExistStateHolidayForCountry = currentCountryHoliday.Any(x => x.GH_ParentTableCode.Equals(RefCountryStatesSchema.Constants.Prefix) && statePksUnderCurrentCountry.Contains(x.GH_ParentID) && !holidayPksUnderCheckedStates.Contains(x.PK));
					if (isExistStateHolidayForCountry)
					{
						return true;
					}
				}
				else
				{
					var isExistStateHoliday = currentCountryHoliday.Any(x => x.GH_ParentTableCode.Equals(RefCountryStatesSchema.Constants.Prefix) && checkedStatesPks.Contains(x.GH_ParentID) && !holidayPksUnderCheckedStates.Contains(x.PK));
					if (isExistStateHoliday)
					{
						return true;
					}
					var isExistCountryHolidayForState = currentCountryHoliday.Any(x => x.GH_ParentTableCode.Equals(RefCountrySchema.Constants.Prefix) && x.GH_ParentID.Equals(country.PK) && !x.PK.Equals(Parent.GHC_PK));
					if (isExistCountryHolidayForState)
					{
						return true;
					}
				}
			}
			return false;
		}

		#region ValidateAll

		public override void ValidateAll()
		{
			ValidateGHC_CountryCode();
			ValidateGHC_HolidayName();
			ValidateGHC_Date();
			ValidateGHC_IsStateSpecific();
			ValidateGHC_IsWorkingDay();
			ValidateGHC_Recurring();
			ValidateGHC_RecurrDay();
			ValidateGHC_RecurrType();
			ValidateGHC_IsActive();
		}

		#endregion
	}
}
