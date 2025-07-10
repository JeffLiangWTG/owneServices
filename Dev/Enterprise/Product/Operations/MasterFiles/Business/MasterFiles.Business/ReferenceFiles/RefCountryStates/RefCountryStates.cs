using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefCountryStates : AutoRefCountryStates, IRefCountryStates, ILocation
	{
		public RefCountryStates(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new partial class Schema : AutoRefCountryStates.Schema
		{
			public const string RW_DescriptionMultilingual = "RW_DescriptionMultilingual";
		}

		#region Properties

		public override ZBool RW_IsSystem
		{
			get { return base.RW_IsSystem; }
			set
			{
				if (base.RW_IsSystem != value)
				{
					base.RW_IsSystem = value;
					RW_CodeInfo.RefreshBinding();
					RW_DescriptionInfo.RefreshBinding();
					RW_RN_NKCountryCodeInfo.RefreshBinding();
					RW_RegionNameInfo.RefreshBinding();
				}
			}
		}

		[ReadOnlyMember(RefCountryStatesSchema.Constants.RW_IsSystem)]
		public override ZString RW_Code
		{
			get { return base.RW_Code; }
			set { base.RW_Code = value; }
		}

		[BusinessObjectTestExclude]
		[ReadOnlyMember(RefCountryStatesSchema.Constants.RW_IsSystem)]
		[RefCountryStatesTranslatableDataField(Schema.TableName, Schema.RW_Description, DataXmlFilePaths.RefCountry, MaxLength = 200, Type = typeof(RefCountryStates), Asmid = ResString.AssemblyId)]
		public override ZString RW_Description
		{
			get { return base.RW_Description; }
			set { base.RW_Description = value; }
		}

		public MultilingualString RW_DescriptionMultilingual
		{
			get { return new MultilingualLanguageText(PK.IsEmpty ? null : PK.ToString(), RefCountryStatesSchema.Constants.Prefix, Schema.RW_Description, RW_Description, Factory); }
		}

		[ReadOnlyMember(RefCountryStatesSchema.Constants.RW_IsSystem)]
		public override ZString RW_RN_NKCountryCode
		{
			get { return base.RW_RN_NKCountryCode; }
			set { base.RW_RN_NKCountryCode = value; }
		}

		[ReadOnlyMember(RefCountryStatesSchema.Constants.RW_IsSystem)]
		public override ZString RW_RegionName
		{
			get { return base.RW_RegionName; }
			set { base.RW_RegionName = value; }
		}

		public override bool CanDelete => base.CanDelete && !RW_IsSystem;

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (RW_IsSystem)
				{
					return ResString.GetMultilingualString("1A141A82-1B8B-4F97-AD7C-3FCAFF810456", "Cannot delete system defined states.");
				}
				return base.ReasonForNotAbleToDelete;
			}
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			RW_IsSystem = false;
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public RefCountryStates LoadRefCountryStatesFromCodeOrDesc(ZString codeOrDesc, ZString countryCode)
			{
				var stateQuery = new ZQuery(RefCountryStatesSchema.RW_Code, codeOrDesc);
				stateQuery.AddToFilter(JoinCondition.Or, RefCountryStatesSchema.RW_Description, codeOrDesc);
				stateQuery.AddToFilter(RefCountryStatesSchema.RW_RN_NKCountryCode, countryCode);
				return Factory.LoadTop1<RefCountryStates>(stateQuery);
			}

			public RefCountryStates LoadRefCountryStatesFromCode(ZString code, ZString countryCode)
			{
				var stateQuery = new ZQuery(RefCountryStatesSchema.RW_Code, code);
				stateQuery.AddToFilter(RefCountryStatesSchema.RW_RN_NKCountryCode, countryCode);
				return Factory.LoadTop1<RefCountryStates>(stateQuery);
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(RefCountryStates);
			}
		}

		#endregion

		#region ILocation Members

		ZString ILocation.Code => RW_Code;

		ZString ILocation.Description => RW_DescriptionMultilingual;

		ZBool ILocation.IsActive => RW_IsActive;

		RefCityTown ILocation.CityTown => null;

		RefCountry ILocation.Country => Country;

		RefCountryStates ILocation.State => this;

		RefUNLOCO ILocation.UNLOCO => null;

		IATACityCode ILocation.IATACityCode => null;

		RefZoneHeader[] ILocation.Zones => Array.Empty<RefZoneHeader>();

		#endregion

		#region Holidays
		public DayOfWeekCodeList WeekDays => Factory.GetCachedValue<DayOfWeekCodeList>();

		[ChildEditable(true)]
		public GlbHolidayCountryStatesCollection Holidays
		{
			get
			{
				if (fHolidays == null)
				{
					fHolidays = new GlbHolidayCountryStatesCollection(this, Factory, GlbHolidayCountryStateCollectionTypes.Holiday);
					RegisterEditableChildObject(fHolidays);
				}
				return fHolidays;
			}
		}
		GlbHolidayCountryStatesCollection fHolidays;

		[ChildEditable(true)]
		public GlbHolidayCountryStatesCollection Weekends
		{
			get
			{
				if (fWeekends == null)
				{
					fWeekends = new GlbHolidayCountryStatesCollection(this, Factory, GlbHolidayCountryStateCollectionTypes.Weekend);
					RegisterEditableChildObject(fWeekends);
				}
				return fWeekends;
			}
		}
		GlbHolidayCountryStatesCollection fWeekends;

		void AddOrEditWeekendDay(string dayOfWeek, bool isNonWorkingDay)
		{
			var dbRecord = Weekends.FirstOrDefault(x => x.GH_RecurrDay == dayOfWeek);
			if (dbRecord == null)
			{
				var holiday = Weekends.AddNew();
				holiday.GH_RecurrDay = dayOfWeek;
				holiday.GH_ParentID = PK;
				holiday.GH_ParentTableCode = TablePrefix;
				holiday.GH_Recurring = true;
				holiday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;
				holiday.GH_IsWorkingDay = !isNonWorkingDay;
				holiday.GH_HolidayName = WeekDays.GetDescriptionFromCode(dayOfWeek);
			}
			else if (dbRecord != null)
			{
				dbRecord.GH_IsWorkingDay = !isNonWorkingDay;
			}
		}

		ZBool GetIsNonWorkingDayByWeekday(string weekDay)
		{
			if (IsNonWorkingDaysOverrided)
			{
				var weekend = Weekends.FirstOrDefault(x => x.GH_RecurrDay == weekDay);
				if (weekend == null)
				{
					return GetIsNonWorkingDayByWeekdayOnCountry(weekDay);
				}
				return weekend != null && !weekend.GH_IsWorkingDay;
			}

			return GetIsNonWorkingDayByWeekdayOnCountry(weekDay);
		}

		ZBool GetIsNonWorkingDayByWeekdayOnCountry(string weekDay)
		{
			if (Country == null)
			{
				return false;
			}
			var countryWeekend = Factory.Load<GlbHoliday>(new ZQuery(GlbHolidaySchema.GH_ParentTableCode, Country.TablePrefix)
					.AddToFilter(GlbHolidaySchema.GH_ParentID, Country.PK)
					.AddToFilter(GlbHolidaySchema.GH_RecurrType, GlbHolidayRecurTypeCodeList.Codes.Weekly)
					.AddToFilter(GlbHolidaySchema.GH_RecurrDay, weekDay)).SingleOrDefault();

			return countryWeekend != null && !countryWeekend.GH_IsWorkingDay;
		}

		public bool IsNonWorkingDaysReadOnly => !IsNonWorkingDaysOverrided;

		public ZBool IsNonWorkingDaysOverrided
		{
			get
			{
				return Weekends?.Any() ?? false;
			}
			set
			{
				if (Weekends.Any() && !value)
				{
					Weekends.DeleteAll();
				}
				else if (!Weekends.Any() && value)
				{
					if (Country != null)
					{
						var countryWeekends = Factory.Load<GlbHoliday>(new ZQuery(GlbHolidaySchema.GH_ParentTableCode, Country.TablePrefix)
						.AddToFilter(GlbHolidaySchema.GH_ParentID, Country.PK)
						.AddToFilter(GlbHolidaySchema.GH_RecurrType, GlbHolidayRecurTypeCodeList.Codes.Weekly));
						foreach (var dayOfWeek in WeekDays.GetAllCodes())
						{
							AddOrEditWeekendDay(dayOfWeek, countryWeekends.Any(x => x.GH_RecurrDay == dayOfWeek && !x.GH_IsWorkingDay));
						}
					}
				}

				IsNonWorkingDaysOverridedInfo.RefreshBinding();
				fIsMondayNonWorkingDay = null;
				IsMondayNonWorkingDayInfo.RefreshBinding();
				fIsTuesdayNonWorkingDay = null;
				IsTuesdayNonWorkingDayInfo.RefreshBinding();
				fIsWednesdayNonWorkingDay = null;
				IsWednesdayNonWorkingDayInfo.RefreshBinding();
				fIsThursdayNonWorkingDay = null;
				IsThursdayNonWorkingDayInfo.RefreshBinding();
				fIsFridayNonWorkingDay = null;
				IsFridayNonWorkingDayInfo.RefreshBinding();
				fIsSaturdayNonWorkingDay = null;
				IsSaturdayNonWorkingDayInfo.RefreshBinding();
				fIsSundayNonWorkingDay = null;
				IsSundayNonWorkingDayInfo.RefreshBinding();
			}
		}

		ZPropertyInfo IsNonWorkingDaysOverridedInfo
		{
			get { return GetZPropertyInfo(nameof(IsNonWorkingDaysOverrided)); }
		}

		[ReadOnlyMember(nameof(IsNonWorkingDaysReadOnly))]
		public ZBool IsMondayNonWorkingDay
		{
			get
			{
				if (!fIsMondayNonWorkingDay.HasValue)
				{
					fIsMondayNonWorkingDay = GetIsNonWorkingDayByWeekday(AutoDayOfWeekCodeList.Codes.Monday);
				}

				return fIsMondayNonWorkingDay.Value;
			}
			set
			{
				if (fIsMondayNonWorkingDay != value)
				{
					fIsMondayNonWorkingDay = value;
					AddOrEditWeekendDay(AutoDayOfWeekCodeList.Codes.Monday, value);
					SetPropertyValue(IsMondayNonWorkingDayInfo, value);
				}
			}
		}
		ZBool? fIsMondayNonWorkingDay;
		ZPropertyInfo IsMondayNonWorkingDayInfo
		{
			get { return GetZPropertyInfo(nameof(IsMondayNonWorkingDay)); }
		}

		[ReadOnlyMember(nameof(IsNonWorkingDaysReadOnly))]
		public ZBool IsTuesdayNonWorkingDay
		{
			get
			{
				if (!fIsTuesdayNonWorkingDay.HasValue)
				{
					fIsTuesdayNonWorkingDay = GetIsNonWorkingDayByWeekday(AutoDayOfWeekCodeList.Codes.Tuesday);
				}

				return fIsTuesdayNonWorkingDay.Value;
			}
			set
			{
				if (fIsTuesdayNonWorkingDay != value)
				{
					fIsTuesdayNonWorkingDay = value;
					AddOrEditWeekendDay(AutoDayOfWeekCodeList.Codes.Tuesday, value);
					SetPropertyValue(IsTuesdayNonWorkingDayInfo, value);
				}
			}
		}
		ZBool? fIsTuesdayNonWorkingDay;
		ZPropertyInfo IsTuesdayNonWorkingDayInfo
		{
			get { return GetZPropertyInfo(nameof(IsTuesdayNonWorkingDay)); }
		}

		[ReadOnlyMember(nameof(IsNonWorkingDaysReadOnly))]
		public ZBool IsWednesdayNonWorkingDay
		{
			get
			{
				if (!fIsWednesdayNonWorkingDay.HasValue)
				{
					fIsWednesdayNonWorkingDay = GetIsNonWorkingDayByWeekday(AutoDayOfWeekCodeList.Codes.Wednesday);
				}

				return fIsWednesdayNonWorkingDay.Value;
			}
			set
			{
				if (fIsWednesdayNonWorkingDay != value)
				{
					fIsWednesdayNonWorkingDay = value;
					AddOrEditWeekendDay(AutoDayOfWeekCodeList.Codes.Wednesday, value);
					SetPropertyValue(IsWednesdayNonWorkingDayInfo, value);
				}
			}
		}
		ZBool? fIsWednesdayNonWorkingDay;
		ZPropertyInfo IsWednesdayNonWorkingDayInfo
		{
			get { return GetZPropertyInfo(nameof(IsWednesdayNonWorkingDay)); }
		}

		[ReadOnlyMember(nameof(IsNonWorkingDaysReadOnly))]
		public ZBool IsThursdayNonWorkingDay
		{
			get
			{
				if (!fIsThursdayNonWorkingDay.HasValue)
				{
					fIsThursdayNonWorkingDay = GetIsNonWorkingDayByWeekday(AutoDayOfWeekCodeList.Codes.Thursday);
				}

				return fIsThursdayNonWorkingDay.Value;
			}
			set
			{
				if (fIsThursdayNonWorkingDay != value)
				{
					fIsThursdayNonWorkingDay = value;
					AddOrEditWeekendDay(AutoDayOfWeekCodeList.Codes.Thursday, value);
					SetPropertyValue(IsThursdayNonWorkingDayInfo, value);
				}
			}
		}
		ZBool? fIsThursdayNonWorkingDay;
		ZPropertyInfo IsThursdayNonWorkingDayInfo
		{
			get { return GetZPropertyInfo(nameof(IsThursdayNonWorkingDay)); }
		}

		[ReadOnlyMember(nameof(IsNonWorkingDaysReadOnly))]
		public ZBool IsFridayNonWorkingDay
		{
			get
			{
				if (!fIsFridayNonWorkingDay.HasValue)
				{
					fIsFridayNonWorkingDay = GetIsNonWorkingDayByWeekday(AutoDayOfWeekCodeList.Codes.Friday);
				}
				return fIsFridayNonWorkingDay.Value;
			}
			set
			{
				if (fIsFridayNonWorkingDay != value)
				{
					fIsFridayNonWorkingDay = value;
					AddOrEditWeekendDay(AutoDayOfWeekCodeList.Codes.Friday, value);
					SetPropertyValue(IsFridayNonWorkingDayInfo, value);
				}
			}
		}
		ZBool? fIsFridayNonWorkingDay;
		ZPropertyInfo IsFridayNonWorkingDayInfo
		{
			get { return GetZPropertyInfo(nameof(IsFridayNonWorkingDay)); }
		}

		[ReadOnlyMember(nameof(IsNonWorkingDaysReadOnly))]
		public ZBool IsSaturdayNonWorkingDay
		{
			get
			{
				if (!fIsSaturdayNonWorkingDay.HasValue)
				{
					fIsSaturdayNonWorkingDay = GetIsNonWorkingDayByWeekday(AutoDayOfWeekCodeList.Codes.Saturday);
				}

				return fIsSaturdayNonWorkingDay.Value;
			}
			set
			{
				if (fIsSaturdayNonWorkingDay != value)
				{
					fIsSaturdayNonWorkingDay = value;
					AddOrEditWeekendDay(AutoDayOfWeekCodeList.Codes.Saturday, value);
					SetPropertyValue(IsSaturdayNonWorkingDayInfo, value);
				}
			}
		}
		ZBool? fIsSaturdayNonWorkingDay;
		ZPropertyInfo IsSaturdayNonWorkingDayInfo
		{
			get { return GetZPropertyInfo(nameof(IsSaturdayNonWorkingDay)); }
		}

		[ReadOnlyMember(nameof(IsNonWorkingDaysReadOnly))]
		public ZBool IsSundayNonWorkingDay
		{
			get
			{
				if (!fIsSundayNonWorkingDay.HasValue)
				{
					fIsSundayNonWorkingDay = GetIsNonWorkingDayByWeekday(AutoDayOfWeekCodeList.Codes.Sunday);
				}

				return fIsSundayNonWorkingDay.Value;
			}
			set
			{
				if (fIsSundayNonWorkingDay != value)
				{
					fIsSundayNonWorkingDay = value;
					AddOrEditWeekendDay(AutoDayOfWeekCodeList.Codes.Sunday, value);
					SetPropertyValue(IsSundayNonWorkingDayInfo, value);
				}
			}
		}
		ZBool? fIsSundayNonWorkingDay;
		ZPropertyInfo IsSundayNonWorkingDayInfo
		{
			get { return GetZPropertyInfo(nameof(IsSundayNonWorkingDay)); }
		}
		#endregion

		public override void Delete()
		{
			countryCodeToRefreshStateList = RW_RN_NKCountryCode;
			base.Delete();
			Factory.ClearStateList(countryCodeToRefreshStateList);
		}

		public override void OnSaving()
		{
			countryCodeToRefreshStateList = (ZString)RW_RN_NKCountryCodeInfo.OriginalValue;
			base.OnSaving();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				RefreshCountryStateList();
			}
			else
			{
				countryCodeToRefreshStateList = ZString.Empty;
			}
		}

		protected override void DeleteForDataRefresh()
		{
			countryCodeToRefreshStateList = RW_RN_NKCountryCode;
			base.DeleteForDataRefresh();
			Factory.ClearStateList(countryCodeToRefreshStateList);
			countryCodeToRefreshStateList = ZString.Empty;
		}

		protected override void OnBeforeUpdatedByDataRefresh()
		{
			countryCodeToRefreshStateList = RW_RN_NKCountryCode;
			base.OnBeforeUpdatedByDataRefresh();
		}
		ZString countryCodeToRefreshStateList;

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();
			RefreshCountryStateList();
		}

		void RefreshCountryStateList()
		{
			Factory.ClearStateList(countryCodeToRefreshStateList);
			if (RW_RN_NKCountryCode != countryCodeToRefreshStateList)
			{
				Factory.ClearStateList(RW_RN_NKCountryCode);
			}
			countryCodeToRefreshStateList = ZString.Empty;
		}

		bool ILocationReference.IsLocalInRelationTo(ZString code)
		{
			var result = false;
			if (code.Length == 5)
			{
				var unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, code);
				result = (unloco?.RL_RW ?? ZGuid.Empty) == PK;
			}
			return result;
		}
	}
}
