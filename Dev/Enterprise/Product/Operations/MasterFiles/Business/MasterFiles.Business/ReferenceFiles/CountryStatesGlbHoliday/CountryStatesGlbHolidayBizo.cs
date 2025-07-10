using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class CountryStatesGlbHolidayBizo : NonPersistentBusinessObject, IIdentified, IWrapPersistentBizO
	{
		public CountryStatesGlbHolidayBizo(BusinessObjectFactory factory)
			: base(factory, GetNewRow(factory))
		{
			GHC_CountryStates.SetReadOnlyIncludingChildren(true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007:Customizable Data Translation Rule", Justification = "<Pending>")]
		public CountryStatesGlbHolidayBizo(BusinessObjectFactory factory, GlbHoliday holiday)
			: base(factory, GetNewRow(factory))
		{
			using (SuspendSettingHasChangesIncludingChildren())
			{
				GHC_PK = holiday.PK;
				GHC_Date = holiday.GH_Date.Date;
				GHC_HolidayName = holiday.GH_HolidayName;
				GHC_IsActive = holiday.GH_IsActive;
				GHC_RecurrDay = holiday.GH_RecurrDay;
				GHC_Recurring = holiday.GH_Recurring;
				GHC_IsWorkingDay = holiday.GH_IsWorkingDay;
				GHC_RecurrType = holiday.GH_RecurrType;
				GHC_CountryCode = holiday.Country.RN_Code;
				GHC_CountryStates.SetReadOnlyIncludingChildren(!GHC_IsStateSpecific);
			}
		}

		#region IWrapPersistentBizO

		public BusinessObject Parent { get; private set; }

		public override SchemaGuidColumn PKSchemaColumn
		{
			get
			{
				return CountryStatesGlbHolidaySchema.PK;
			}
		}

		#endregion

		#region Identifier

		ZGuid IIdentified.Identifier => GHC_PK;
		protected override ZString HumanReadableNameCore => GHC_HolidayName;

		#endregion

		#region Schema

		public static class Schema
		{
			public const string PK = CountryStatesGlbHolidaySchema.Constants.PK;
			public const string TableName = CountryStatesGlbHolidaySchema.Constants.TableName;

			public const string GHC_HolidayName = CountryStatesGlbHolidaySchema.Constants.GHC_HolidayName;
			public const string GHC_RecurrType = CountryStatesGlbHolidaySchema.Constants.GHC_RecurrType;
			public const string GHC_Recurring = CountryStatesGlbHolidaySchema.Constants.GHC_Recurring;
			public const string GHC_IsActive = CountryStatesGlbHolidaySchema.Constants.GHC_IsActive;
			public const string GHC_IsWorkingDay = CountryStatesGlbHolidaySchema.Constants.GHC_IsWorkingDay;
			public const string GHC_RecurrDay = CountryStatesGlbHolidaySchema.Constants.GHC_RecurrDay;
			public const string GHC_Date = CountryStatesGlbHolidaySchema.Constants.GHC_Date;
			public const string GHC_CountryCode = CountryStatesGlbHolidaySchema.Constants.GHC_CountryCode;

			public const int GHC_HolidayNameMaxLength = GlbHoliday.Schema.GH_HolidayNameMaxLength;
			public const int GHC_RecurrDayMaxLength = GlbHoliday.Schema.GH_RecurrDayMaxLength;
			public const int GHC_RecurrTypeMaxLength = GlbHoliday.Schema.GH_RecurrTypeMaxLength;
			public const int GHC_CountryCodeMaxLength = RefCountry.Schema.RN_CodeMaxLength;
		}

		#endregion

		#region Properties

		#region GHC_PK

		public ZGuid GHC_PK
		{
			get { return gHC_PK; }
			set
			{
				gHC_PK = value;
				Parent = Factory.Load<GlbHoliday>(value);
			}
		}

		ZGuid gHC_PK = ZGuid.NewZGuid();

		#endregion

		#region GHC_HolidayName

		[MaxLength(Schema.GHC_HolidayNameMaxLength)]
		public ZString GHC_HolidayName
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(GHC_HolidayNameInfo)); }
			set
			{
				value = value.TrimEndSpaceTab();
				CheckMaximumLength(GHC_HolidayNameInfo, value);
				SetPropertyValue(GHC_HolidayNameInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateGHC_HolidayName();
				}
			}
		}

		public ZPropertyInfo GHC_HolidayNameInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.GHC_HolidayName); }
		}

		#endregion

		#region GHC_RecurrDay

		[MaxLength(Schema.GHC_RecurrDayMaxLength)]
		public ZString GHC_RecurrDay
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(GHC_RecurrDayInfo)); }
			set
			{
				value = value.TrimEndSpaceTab();
				CheckMaximumLength(GHC_RecurrDayInfo, value);
				SetPropertyValue(GHC_RecurrDayInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateGHC_RecurrDay();
				}
			}
		}

		public ZPropertyInfo GHC_RecurrDayInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.GHC_RecurrDay); }
		}

		#endregion

		#region GHC_RecurrType

		[MaxLength(Schema.GHC_RecurrTypeMaxLength)]
		public ZString GHC_RecurrType
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(GHC_RecurrTypeInfo)); }
			set
			{
				value = value.TrimEndSpaceTab();
				CheckMaximumLength(GHC_RecurrTypeInfo, value);
				SetPropertyValue(GHC_RecurrTypeInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateGHC_RecurrType();
				}
			}
		}

		public ZPropertyInfo GHC_RecurrTypeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.GHC_RecurrType); }
		}

		#endregion

		#region GHC_CountryStatesApplicability

		public ZString GHC_CountryStatesApplicability
		{
			get
			{
				if (string.IsNullOrEmpty(countryStatesApplicability))
				{
					countryStatesApplicability = Factory.Load<GlbHoliday>(GHC_PK).CountryStatesApplicability;
				}
				return countryStatesApplicability;
			}
		}

		string countryStatesApplicability = string.Empty;

		#endregion

		#region GHC_Recurring
		[ReadOnlyMember(nameof(GHC_IsWorkingDay))]

		public ZBool GHC_Recurring
		{
			get { return new ZBool(GetValueFromRowSafely(CountryStatesGlbHolidaySchema.GHC_Recurring)); }
			set
			{
				SetPropertyValue(GHC_RecurringInfo, value);
				GHC_IsWorkingDayInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateGHC_Recurring();
				}
			}
		}

		public ZPropertyInfo GHC_RecurringInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.GHC_Recurring); }
		}

		#endregion

		#region GHC_IsActive

		public ZBool GHC_IsActive
		{
			get { return new ZBool(GetValueFromRowSafely(CountryStatesGlbHolidaySchema.GHC_IsActive)); }
			set
			{
				SetPropertyValue(GHC_IsActiveInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateGHC_IsActive();
				}
			}
		}

		public ZPropertyInfo GHC_IsActiveInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.GHC_IsActive); }
		}

		#endregion

		#region GHC_IsWorkingDay

		[ReadOnlyMember(nameof(GHC_Recurring))]
		public ZBool GHC_IsWorkingDay
		{
			get { return new ZBool(GetValueFromRowSafely(CountryStatesGlbHolidaySchema.GHC_IsWorkingDay)); }
			set
			{
				SetPropertyValue(GHC_IsWorkingDayInfo, value);
				GHC_RecurringInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateGHC_IsWorkingDay();
				}
			}
		}

		public ZPropertyInfo GHC_IsWorkingDayInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.GHC_IsWorkingDay); }
		}

		#endregion

		#region GHC_CountryCode

		[List(nameof(Countries))]
		[MaxLength(Schema.GHC_CountryCodeMaxLength)]
		public ZString GHC_CountryCode
		{
			get
			{
				return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(GHC_CountryCodeInfo));
			}
			set
			{
				value = value.TrimEndSpaceTab();
				value = value.ToUpperInvariant();
				CheckMaximumLength(GHC_CountryCodeInfo, value);
				SetPropertyValue(GHC_CountryCodeInfo, value);
				if (fCollectionCountryCode != value)
				{
					GHC_CountryStates.RemoveAndDeleteAll();
					if (!value.IsEmpty)
					{
						fCollectionCountryCode = value;
						GHC_CountryStates.CountryCode = value;
						GHC_CountryStates.Load();
					}
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateGHC_CountryCode();
				}
			}
		}
		public ZPropertyInfo GHC_CountryCodeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.GHC_CountryCode); }
		}

		#endregion

		#region GHC_CountryStates

		[ChildEditable(true)]
		public CountryStatesGlbHolidayEntryCollection GHC_CountryStates
		{
			get
			{
				if (fCountryStatesCollection == null)
				{
					fCountryStatesCollection = new CountryStatesGlbHolidayEntryCollection(this, Factory);
					if (!GHC_CountryCode.IsEmpty)
					{
						fCollectionCountryCode = GHC_CountryCode;
						fCountryStatesCollection.CountryCode = GHC_CountryCode;
						fCountryStatesCollection.Load();
						fCountryStatesCollection.Sort(nameof(CountryStatesGlbHolidayEntry.StateName));
					}

					RegisterEditableChildObject(fCountryStatesCollection);
				}

				return fCountryStatesCollection;
			}
		}

		CountryStatesGlbHolidayEntryCollection fCountryStatesCollection;
		ZString fCollectionCountryCode = ZString.Empty;

		#endregion

		#region GHC_IsStateSpecific

		public ZBool IsStateSpecific_ReadOnly => GHC_CountryCode.IsEmpty;

		[ReadOnlyMember(nameof(IsStateSpecific_ReadOnly))]
		public ZBool GHC_IsStateSpecific
		{
			get
			{
				return isStateSpecificValue || GHC_CountryStates.Any(x => ((CountryStatesGlbHolidayEntry)x).IsChecked);
			}
			set
			{
				if (!value && GHC_CountryStates.Any(x => ((CountryStatesGlbHolidayEntry)x).IsChecked))
				{
					foreach (CountryStatesGlbHolidayEntry entry in GHC_CountryStates.Where(x => x.IsChecked))
					{
						entry.IsChecked = false;
					}
				}
				isStateSpecificValue = value;
				GHC_CountryStates.SetReadOnlyIncludingChildren(!value);
				GHC_IsStateSpecificInfo.RefreshBinding();
			}
		}
		ZBool isStateSpecificValue = false;

		public ZPropertyInfo GHC_IsStateSpecificInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(GHC_IsStateSpecific)); }
		}

		#endregion

		#region GHC_Date

		public ZDate GHC_Date
		{
			get
			{
				var date = new ZDate(GetValueFromRowSafely(CountryStatesGlbHolidaySchema.GHC_Date));
				return date.IsValid ? date : ZDate.Empty;
			}
			set
			{
				SetPropertyValue(GHC_DateInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateGHC_Date();
				}
			}
		}

		public ZPropertyInfo GHC_DateInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.GHC_Date); }
		}

		public ZBool isSelectAll = false;

		[BusinessObjectTestExclude]
		public ZBool CountryStatesGlbHolidayEntrySelectAll
		{
			get
			{
				return GHC_CountryStates.Any() && GHC_CountryStates.All(x => ((CountryStatesGlbHolidayEntry)x).IsChecked);
			}
			set
			{
				if (GHC_IsStateSpecific)
				{
					isStateSpecificValue = true;
					isSelectAll = true;
					foreach (CountryStatesGlbHolidayEntry entry in GHC_CountryStates)
					{
						entry.IsChecked = value;
					}
					this.Validation.ValidateAll();
					isSelectAll = false;
				}
				CountryStatesGlbHolidayEntrySelectAllInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CountryStatesGlbHolidayEntrySelectAllInfo
		{
			get { return GetZPropertyInfo(nameof(CountryStatesGlbHolidayEntrySelectAll)); }
		}

		#endregion

		#region Lookups

		public RefCountryCollection Countries
		{
			get { return new RefCountryCollection(Factory); }
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			try
			{
				IsRunningPreSaveValidation = true;
				Validation.ValidateAll();
				base.RunPreSaveValidationCore();
			}
			finally
			{
				IsRunningPreSaveValidation = false;
			}
		}

		internal bool IsRunningPreSaveValidation;

		public CountryStatesGlbHolidayBizoValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual CountryStatesGlbHolidayBizoValidation GetNewValidation()
		{
			return new CountryStatesGlbHolidayBizoValidation(this);
		}

		#endregion

		#endregion

		public const string RecurrTypeDate = "DAT";
		public const string RecurrTypeWeekly = "WKL";

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			DataRow row = ((IBusinessObjectInternals)this).Row;
			row[CountryStatesGlbHolidayBizo.Schema.GHC_HolidayName] = "";
			row[CountryStatesGlbHolidayBizo.Schema.GHC_RecurrDay] = "";
			row[CountryStatesGlbHolidayBizo.Schema.GHC_RecurrType] = "";
			row[CountryStatesGlbHolidayBizo.Schema.GHC_Recurring] = false;
			row[CountryStatesGlbHolidayBizo.Schema.GHC_IsWorkingDay] = false;
			row[CountryStatesGlbHolidayBizo.Schema.GHC_IsActive] = false;
			row[CountryStatesGlbHolidayBizo.Schema.GHC_Date] = DateTime.MinValue.Date;
			row[CountryStatesGlbHolidayBizo.Schema.GHC_CountryCode] = "";
		}

		static DataRow GetNewRow(BusinessObjectFactory factory)
		{
			DataTable table = ((INeedDataSet)factory).Data.Tables[CountryStatesGlbHolidayBizo.Schema.TableName];
			if (table == null)
			{
				table = new NonPersistentDataTable();
				((INeedDataSet)factory).Data.Tables.Add(table);
			}
			return table.NewRow();
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (HasChanges)
			{
				var anyStateSavedPk = ZGuid.Empty;
				foreach (CountryStatesGlbHolidayEntry entry in GHC_CountryStates)
				{
					GlbHoliday holiday = null;
					//existing holiday
					if (entry.HolidayPK.HasValue)
					{
						holiday = Factory.Load<GlbHoliday>(entry.HolidayPK.Value);
						if (!entry.IsChecked)
						{
							//delete if unchecked and it exists
							holiday?.Delete();
						}
					}

					if (entry.IsChecked)
					{
						if (holiday == null)
						{
							holiday = Factory.New<GlbHoliday>();
						}
						holiday.GH_Date = GHC_Date;
						holiday.GH_RecurrType = RecurrTypeDate;
						holiday.GH_Recurring = GHC_Recurring;
						holiday.GH_ParentID = entry.StatePK;
						holiday.GH_ParentTableCode = RefCountryStatesSchema.Constants.Prefix;
						holiday.GH_IsWorkingDay = GHC_IsWorkingDay;
						holiday.GH_HolidayName = GHC_HolidayName;
						entry.HolidayPK = holiday.PK;
						anyStateSavedPk = holiday.PK;
					}
				}

				if (!GHC_CountryStates.Any(x => ((CountryStatesGlbHolidayEntry)x).IsChecked))
				{
					if (!GHC_CountryCode.IsEmpty)
					{
						var country = new RefCountry.Loader(Factory).LoadForCountry(GHC_CountryCode);
						if (country != null)
						{
							//save for country
							var holidayMatch = Factory.Load<GlbHoliday>(GHC_PK)
								?? Factory.New<GlbHoliday>();
							holidayMatch.GH_Date = GHC_Date;
							holidayMatch.GH_RecurrType = RecurrTypeDate;
							holidayMatch.GH_Recurring = GHC_Recurring;
							holidayMatch.GH_ParentID = country.PK;
							holidayMatch.GH_ParentTableCode = RefCountrySchema.Constants.Prefix;
							holidayMatch.GH_IsWorkingDay = GHC_IsWorkingDay;
							holidayMatch.GH_HolidayName = GHC_HolidayName;
							GHC_PK = holidayMatch.PK;
						}
					}
				}
				else
				{
					//check whether originally GHC_PK was a country holiday
					//then delete holiday
					var holidayMatch = Factory.Load<GlbHoliday>(GHC_PK);
					if (holidayMatch != null && holidayMatch.GH_ParentTableCode == RefCountrySchema.Constants.Prefix)
					{
						holidayMatch.Delete();
					}
				}

				if (!anyStateSavedPk.IsEmpty)
				{
					GHC_PK = anyStateSavedPk;
				}
			}
		}

		public override void Delete()
		{
			foreach (CountryStatesGlbHolidayEntry entry in GHC_CountryStates.Where(x => x.IsChecked))
			{
				var holiday = Factory.Load<GlbHoliday>(entry.HolidayPK.Value);
				holiday?.Delete();
			}

			if (!GHC_CountryStates.Any(x => ((CountryStatesGlbHolidayEntry)x).IsChecked))
			{
				var holiday = Factory.Load<GlbHoliday>(GHC_PK);
				holiday?.Delete();
			}
			base.Delete();
		}

		class NonPersistentDataTable : ZDataTable
		{
			public NonPersistentDataTable()
				: base(CountryStatesGlbHolidayBizo.Schema.TableName)
			{
				DataColumn column;
				column = Columns.Add(CountryStatesGlbHolidayBizo.Schema.PK, typeof(Guid));
				column.AllowDBNull = false;

				column = Columns.Add(CountryStatesGlbHolidayBizo.Schema.GHC_HolidayName, typeof(System.String));
				column.MaxLength = GlbHoliday.Schema.GH_HolidayNameMaxLength;
				column.AllowDBNull = false;

				column = Columns.Add(CountryStatesGlbHolidayBizo.Schema.GHC_RecurrDay, typeof(System.String));
				column.MaxLength = GlbHoliday.Schema.GH_RecurrDayMaxLength;
				column.AllowDBNull = false;

				column = Columns.Add(CountryStatesGlbHolidayBizo.Schema.GHC_RecurrType, typeof(System.String));
				column.MaxLength = GlbHoliday.Schema.GH_RecurrTypeMaxLength;
				column.AllowDBNull = false;

				column = Columns.Add(CountryStatesGlbHolidayBizo.Schema.GHC_Recurring, typeof(System.Boolean));
				column.AllowDBNull = false;

				column = Columns.Add(CountryStatesGlbHolidayBizo.Schema.GHC_IsActive, typeof(System.Boolean));
				column.AllowDBNull = false;

				column = Columns.Add(CountryStatesGlbHolidayBizo.Schema.GHC_IsWorkingDay, typeof(System.Boolean));
				column.AllowDBNull = false;

				column = Columns.Add(CountryStatesGlbHolidayBizo.Schema.GHC_Date, typeof(DateTime));
				column.AllowDBNull = false;

				column = Columns.Add(CountryStatesGlbHolidayBizo.Schema.GHC_CountryCode, typeof(System.String));
				column.AllowDBNull = true;
			}
		}
	}
}
