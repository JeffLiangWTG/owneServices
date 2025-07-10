using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public enum OrgTimeTableRangeType
	{
		Default = 0,
		Weekday = 1,
		Advanced = 2,
		NotApplicable = 3
	}

	public class OrgTimetableCollection : ActiveBusinessObjectCollection<OrgTimetable>
	{
		public OrgTimetableCollection(OrgAddress parent)
			: base(parent.Factory, parent, null, OrgTimetableSchema.OTT_OA)
		{
			parentAddress = parent;
		}

		void SetNotApplicable(OrgTimetable timetable)
		{
			timetable.OTT_Monday = false;
			timetable.OTT_Tuesday = false;
			timetable.OTT_Wednesday = false;
			timetable.OTT_Thursday = false;
			timetable.OTT_Friday = false;
			timetable.OTT_Saturday = false;
			timetable.OTT_Sunday = false;
			timetable.OTT_IsForAllWeekDays = false;
		}

		bool defaultRange;
		public bool DefaultRange
		{
			get => defaultRange;
			set
			{
				if (value)
				{
					SetRangeType(OrgTimeTableRangeType.Default);
				}
			}
		}

		bool weekdayRange;
		public bool WeekdayRange
		{
			get => weekdayRange;
			set
			{
				if (value)
				{
					SetRangeType(OrgTimeTableRangeType.Weekday);
				}
			}
		}

		bool advancedRange;
		public bool AdvancedRange
		{
			get => advancedRange;
			set
			{
				if (value)
				{
					SetRangeType(OrgTimeTableRangeType.Advanced);
				}
			}
		}

		bool notApplicableRange;
		public bool NotApplicable
		{
			get => notApplicableRange;
			set
			{
				if (value)
				{
					SetRangeType(OrgTimeTableRangeType.NotApplicable);
				}
			}
		}

		internal ZString CountryCode
		{
			get
			{
				return parentAddress?.Country?.Code ?? ZString.Empty;
			}
		}

		internal OrgTimeTableRangeType RangeType { get; set; }

		OrgTimeTableRangeType CalculateRangeType()
		{
			if (Count == 0)
			{
				return OrgTimeTableRangeType.Default;
			}

			var item = this[0];
			if (item.OTT_IsForAllWeekDays)
			{
				return OrgTimeTableRangeType.Weekday;
			}

			if (!item.OTT_Monday && !item.OTT_Tuesday && !item.OTT_Wednesday && !item.OTT_Thursday && !item.OTT_Friday && !item.OTT_Saturday && !item.OTT_Sunday)
			{
				return OrgTimeTableRangeType.NotApplicable;
			}

			return OrgTimeTableRangeType.Advanced;
		}

		void ResetRangeType()
		{
			advancedRange = false;
			notApplicableRange = false;
			defaultRange = false;
			weekdayRange = false;
			var rangeType = CalculateRangeType();
			if (rangeType == OrgTimeTableRangeType.Default)
			{
				defaultRange = true;
				RangeType = OrgTimeTableRangeType.Default;
				return;
			}

			if (rangeType == OrgTimeTableRangeType.Weekday)
			{
				weekdayRange = true;
				RangeType = OrgTimeTableRangeType.Weekday;
				return;
			}

			if (rangeType == OrgTimeTableRangeType.NotApplicable)
			{
				notApplicableRange = true;
				RangeType = OrgTimeTableRangeType.NotApplicable;
				return;
			}

			advancedRange = true;
			RangeType = OrgTimeTableRangeType.Advanced;
		}

		internal void InitializeRangeType()
		{
			ResetRangeType();
			if (defaultRange)
			{
				DefaultRange = true;
			}

			BackupPreviousData();
		}

		public void ReInitializeRangeType()
		{
			ResetRangeType();
		}

		internal void SetRangeType(OrgTimeTableRangeType rangeType)
		{
			RangeType = rangeType;
			defaultRange = RangeType == OrgTimeTableRangeType.Default;
			weekdayRange = RangeType == OrgTimeTableRangeType.Weekday;
			advancedRange = RangeType == OrgTimeTableRangeType.Advanced;
			notApplicableRange = RangeType == OrgTimeTableRangeType.NotApplicable;

			if (defaultRange)
			{
				DeleteAllAndIgnoreDeleteEvent();
				CreateDefaultRowsForDefaultRange();
			}
			else if (notApplicableRange)
			{
				DeleteAllAndIgnoreDeleteEvent();
				CreateDefaultRows();
			}
			else if (weekdayRange)
			{
				DeleteAllAndIgnoreDeleteEvent();
				CreateWeekdayRows();
			}
			else
			{
				this.ForEach(x =>
				{
					x.IsDefaultFromRegistry = false;
				});
			}

			BackupPreviousData();
		}

		void BackupPreviousData()
		{
			previousRangeType = RangeType;
			previousTimetables.Clear();
			factoryForBackupData = factoryForBackupData ?? new BusinessObjectFactory() { NameForDebugging = "OrgTimetableCollection_BackupPreviousData" };
			this.ForEach(x =>
			{
				var timetable = factoryForBackupData.New<OrgTimetable>();
				using (timetable.GetValidationSuspender())
				{
					timetable.OTT_OA = x.OTT_OA;
					timetable.OTT_Type = x.OTT_Type;
					timetable.OTT_TimeTo = x.OTT_TimeTo;
					timetable.OTT_TimeFrom = x.OTT_TimeFrom;
					timetable.OTT_Monday = x.OTT_Monday;
					timetable.OTT_Tuesday = x.OTT_Tuesday;
					timetable.OTT_Wednesday = x.OTT_Wednesday;
					timetable.OTT_Thursday = x.OTT_Thursday;
					timetable.OTT_Friday = x.OTT_Friday;
					timetable.OTT_Saturday = x.OTT_Saturday;
					timetable.OTT_Sunday = x.OTT_Sunday;
					timetable.OTT_IsForAllWeekDays = x.OTT_IsForAllWeekDays;
				}
				previousTimetables.Add(timetable);
			});
		}
		OrgTimeTableRangeType previousRangeType;
		readonly List<OrgTimetable> previousTimetables = new List<OrgTimetable>();
		BusinessObjectFactory factoryForBackupData;

		void CreateDefaultRowsForDefaultRange()
		{
			if (Relationship is not NoResultRelationship)
			{
				var defaultTimetables = OrganisationRegistry.Instance.DefaultOrgTimetable.Value.DefaultOrgTimetablesForCountry(CountryCode);
				defaultTimetables?.ForEach(defaultOrgTimetable =>
				{
					var defaultTimeTableRowFromRegistry = (DefaultOrgTimetable)defaultOrgTimetable;
					var defaultTimeTableRow = Factory.New<OrgTimetable>();
					using (defaultTimeTableRow.GetValidationSuspender())
					{
						defaultTimeTableRow.DayOfWeek = defaultTimeTableRowFromRegistry.Day;
						defaultTimeTableRow.OTT_Type = defaultTimeTableRowFromRegistry.Type;
						defaultTimeTableRow.OTT_TimeFrom = defaultTimeTableRowFromRegistry.From;
						defaultTimeTableRow.OTT_TimeTo = defaultTimeTableRowFromRegistry.To;
						defaultTimeTableRow.OTT_OA = parentAddress.PK;
						defaultTimeTableRow.OTT_CutOffTime = defaultTimeTableRowFromRegistry.CutOffTime;
						defaultTimeTableRow.OTT_ProcessingTimeInMinutes = defaultTimeTableRowFromRegistry.ProcessingTime;
						defaultTimeTableRow.IsDefaultFromRegistry = true;
					}
					Add(defaultTimeTableRow);
				});
			}
		}
		readonly OrgAddress parentAddress;

		void CreateDefaultRows()
		{
			if (Relationship is not NoResultRelationship)
			{
				Add(DefaultRow(OrgTimetableType.Codes.Pickup));
				Add(DefaultRow(OrgTimetableType.Codes.Deliver));
			}
		}

		void CreateWeekdayRows()
		{
			if (Relationship is not NoResultRelationship)
			{
				Add(WeekdayRow(OrgTimetableType.Codes.Pickup));
				Add(WeekdayRow(OrgTimetableType.Codes.Deliver));
			}
		}

		OrgTimetable DefaultRow(string orgTimetableType)
		{
			var timetable = Factory.New<OrgTimetable>();
			timetable.OTT_Type = orgTimetableType;
			if (notApplicableRange)
			{
				SetNotApplicable(timetable);
			}

			return timetable;
		}

		OrgTimetable WeekdayRow(string orgTimetableType)
		{
			var timetable = Factory.New<OrgTimetable>();
			if (parentAddress != null)
			{
				timetable.OTT_OA = parentAddress.PK;
			}

			timetable.OTT_Type = orgTimetableType;
			timetable.OTT_IsForAllWeekDays = true;
			return timetable;
		}

		internal bool HasChanges
		{
			get
			{
				bool hasChanges = true;
				if (DefaultRange || NotApplicable || this.All(x => x.IsInDatabase && !x.HasChanges))
				{
					BackupPreviousData();
					hasChanges = false;
				}
				else
				{
					if (Count == previousTimetables.Count && IsSameTimeTable(this.ToList(), previousTimetables) && IsSameTimeTable(previousTimetables, this.ToList()))
					{
						hasChanges = false;
					}
				}

				return hasChanges;
			}
		}

		bool IsSameTimeTable(List<OrgTimetable> sourceTimetales, List<OrgTimetable> targetTimetales)
		{
			bool isSame = true;
			sourceTimetales.ForEach(x =>
			{
				if (!targetTimetales.Any(y =>
					y.OTT_Type == x.OTT_Type &&
					y.OTT_TimeFrom == x.OTT_TimeFrom &&
					y.OTT_TimeTo == x.OTT_TimeTo &&
					y.OTT_Monday == x.OTT_Monday &&
					y.OTT_Tuesday == x.OTT_Tuesday &&
					y.OTT_Wednesday == x.OTT_Wednesday &&
					y.OTT_Thursday == x.OTT_Thursday &&
					y.OTT_Friday == x.OTT_Friday &&
					y.OTT_Saturday == x.OTT_Saturday &&
					y.OTT_Sunday == x.OTT_Sunday &&
					y.OTT_IsForAllWeekDays == x.OTT_IsForAllWeekDays))
				{
					isSame = false;
				}
			});

			return isSame;
		}

		public event EventHandler OnTriedToDeleteLastTimeTable;

		public override void Delete(OrgTimetable address)
		{
			if (address.IsDeleted)
			{
				return;
			}

			if (Count > 1 || allowDeleteLastTimeTable)
			{
				base.Delete(address);
			}
			else
			{
				OnTriedToDeleteLastTimeTable?.Invoke(this, EventArgs.Empty);
			}
		}

		public bool allowDeleteLastTimeTable;

		internal void DeleteAllAndIgnoreDeleteEvent()
		{
			allowDeleteLastTimeTable = true;
			DeleteAll();
			allowDeleteLastTimeTable = false;
		}

		protected override object[] GetCollectionState()
		{
			return new object[] { defaultRange, weekdayRange, advancedRange, notApplicableRange, allowDeleteLastTimeTable, RangeType, previousRangeType, previousTimetables, parentAddress };
		}

		protected override IComparer GetSortComparerForProperty(System.ComponentModel.PropertyDescriptor property, System.ComponentModel.ListSortDirection direction)
		{
			if (property.Name == "DayOfWeek")
			{
				return new OrgTimetableDayOfWeekComparer(property, direction);
			}
			return base.GetSortComparerForProperty(property, direction);
		}

		protected override void OnAdded(OrgTimetable item)
		{
			base.OnAdded(item);
			if (CalculateRangeType() == OrgTimeTableRangeType.Weekday)
			{
				item.OTT_IsForAllWeekDays = true;
			}
		}

		class OrgTimetableDayOfWeekComparer : PropertyComparer
		{
			internal OrgTimetableDayOfWeekComparer(System.ComponentModel.PropertyDescriptor property, System.ComponentModel.ListSortDirection direction)
				: base(property, direction)
			{
			}

			public override int Compare(BusinessObject x, BusinessObject y)
			{
				OrgTimetable data1 = (OrgTimetable)x;
				OrgTimetable data2 = (OrgTimetable)y;

				int result = new OrgTimetableComparer().Compare(data1, data2);

				return Direction == System.ComponentModel.ListSortDirection.Ascending
					? result
					: -result;
			}
		}
	}

	public class OrgTimetableComparer : IComparer<OrgTimetable>, IComparer
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Override is required")]
		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Override is required")]
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public int Compare(OrgTimetable timetable1, OrgTimetable timetable2)
		{
			int result = 0;
			DayOfWeek day1, day2;
			if (DayOfWeekCodeList.Mappping.TryGetValue(timetable1.DayOfWeek.ToString(), out day1) && DayOfWeekCodeList.Mappping.TryGetValue(timetable2.DayOfWeek.ToString(), out day2))
			{
				result = day1.CompareTo(day2);
			}
			if (result == 0)
			{
				result = timetable1.OTT_Type.CompareTo(timetable2.OTT_Type);
			}
			if (result == 0)
			{
				result = timetable1.OTT_TimeFrom.CompareTo(timetable2.OTT_TimeFrom);
			}
			return result;
		}

		int IComparer.Compare(object x, object y)
		{
			var timeTable1 = (OrgTimetable)x;
			var timeTable2 = (OrgTimetable)y;
			return Compare(timeTable1, timeTable2);
		}
	}
}
