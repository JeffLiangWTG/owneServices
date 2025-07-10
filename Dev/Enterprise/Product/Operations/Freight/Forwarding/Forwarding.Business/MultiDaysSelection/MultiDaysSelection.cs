namespace Enterprise.Freight.Forwarding.Business
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Integration;
	using CargoWise.Types;
	using Enterprise.Freight.Business;
	using Enterprise.Freight.Integration;
	using Enterprise.Integration.Freight;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Core;

	#region FlightNumberEventArgs

	public class FlightNumberEventArgs : EventArgs
	{
		public int FlightNumber { get; set; }
	}

	#endregion

	public class MultiDaysSelection : AutoMultiDaysSelection, IMultiDaysSelection
	{
		public MultiDaysSelection(JobSailingCollection sailings, BusinessObjectFactory factory)
			: base(factory)
		{
			RecurrenceEnabled = false;

			foreach (var sailing in sailings)
			{
				var newSailings = new JobSailingCollection(factory);
				newSailings.Add(sailing);
				this.sailingList.Add(newSailings);
			}

			Initialize(includeWeeklyTimetable: false, importAndCreateMawb: true);
		}

		protected MultiDaysSelection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public bool ImportAndCreateMAWB { get; private set; }

		public bool IncludeWeeklyTimetable { get; private set; }

		public string ActiveTab { get; set; }

		public const string CreateNewConsolsTabName = "CreateNewConsolTabPage";

		public const string CreateFromConsolTemplatesTabName = "CreateConsolFromTemplatesTabPage";

		public bool RecurrenceEnabled { get; protected set; }

		public void Generate(Action<ICommonConsol, IJobSailing> action = null)
		{
			if (ActiveTab == CreateNewConsolsTabName)
			{
				ConsolDetails.GenerateConsols(sailingList, action);
			}
			else
			{
				ConsolTemplateDetails.GenerateConsols(sailingList);
			}
		}

		public ZDateTime FromDateLimit { get; protected set; }
		public ZDateTime ToDateLimit { get; protected set; }

		public List<ZDateTime> DepartureDates
		{
			get
			{
				var departureDates = new List<ZDateTime>();
				CalculateRecurrence((date) =>
				{
					departureDates.Add(date);
				});

				return departureDates;
			}
		}

		#region CalculateRecurrence

		protected void CalculateRecurrence(Action<ZDateTime> action)
		{
			if (HasErrors || !RecurrenceEnabled)
			{
				return;
			}

			if (UseDailyPattern || UseMonthlyPattern)
			{
				var date = UseDailyPattern ? DailyFromDate : MonthlyFromDate;

				while (date <= RangeToDate)
				{
					if (HasOperation(date))
					{
						action?.Invoke(date);
					}

					date = UseDailyPattern ? date.AddDays(DailyRecurEvery) : date.AddMonths(MonthlyRecurEvery);
				}
			}
			else
			{
				var weekStartDate = RangeFromDate;

				while (weekStartDate <= RangeToDate)
				{
					var weekEndDate = weekStartDate.AddDays(6);

					for (var date = weekStartDate; date <= weekEndDate && date <= RangeToDate; date = date.AddDays(1))
					{
						var weekDayDescription = GetWeekDayDescription(date);

						if (HasOperation(date) && (WeeklyDaysCheckedList[weekDayDescription]?.Value ?? false))
						{
							action?.Invoke(date);
						}
					}

					weekStartDate = weekStartDate.AddDays(7 * WeeklyRecurEvery);
				}
			}
		}

		#endregion

		public ZBool UseDailyPattern
		{
			get => (UsePattern == RecurrencePattern.Daily);
			set
			{
				if (value)
				{
					UsePattern = RecurrencePattern.Daily;
					InvokeFlightNumberChangeEvent();
				}
			}
		}

		public ZBool UseWeeklyPattern
		{
			get => (UsePattern == RecurrencePattern.Weekly);
			set
			{
				if (value)
				{
					UsePattern = RecurrencePattern.Weekly;
					InvokeFlightNumberChangeEvent();
				}
			}
		}

		public ZBool UseMonthlyPattern
		{
			get => (UsePattern == RecurrencePattern.Monthly);
			set
			{
				if (value)
				{
					UsePattern = RecurrencePattern.Monthly;
					InvokeFlightNumberChangeEvent();
				}
			}
		}

		public ZBoolDescriptionPairList WeeklyDaysCheckedList { get; set; }

		public bool WeeklyDaysNotSelected => UseWeeklyPattern && WeeklyDaysCheckedList.All(x => !x.Value);

		#region ConsolTemplateDetails

		public ConsolTemplateGenerator ConsolTemplateDetails
		{
			get
			{
				if (consolTemplateDetails == null)
				{
					consolTemplateDetails = new ConsolTemplateGenerator(this);
					if (ImportAndCreateMAWB)
					{
						RegisterEditableChildObject(consolTemplateDetails);
					}
				}

				return consolTemplateDetails;
			}
		}

		ConsolTemplateGenerator consolTemplateDetails;

		#endregion

		#region FlightNumberChange Event

		public event EventHandler<FlightNumberEventArgs> OnFlightNumberChanged;

		void InvokeFlightNumberChangeEvent()
		{
			if (OnFlightNumberChanged != null)
			{
				var flightNumberEventArgs = new FlightNumberEventArgs()
				{
					FlightNumber = GetFlightNumber()
				};

				OnFlightNumberChanged(this, flightNumberEventArgs);
			}
		}

		public int GetFlightNumber()
		{
			return RecurrenceEnabled ? GetRecurrenceNumberOfFlights() : sailingList.Count;
		}

		protected virtual int GetRecurrenceNumberOfFlights() => 0;

		#endregion

		#region CreatedConsols

		public ForwardingConsolCollection CreatedConsols
		{
			get
			{
				if (createdConsols == null)
				{
					createdConsols = new ForwardingConsolCollection(Factory);
					createdConsols.SetReadOnlyIncludingChildren(true);
				}
				return createdConsols;
			}
		}
		ForwardingConsolCollection createdConsols;

		#endregion

		#region DailyFromDate

		public override ZDateTime DailyFromDate
		{
			get => base.DailyFromDate;
			set
			{
				base.DailyFromDate = value;
				InvokeFlightNumberChangeEvent();
			}
		}

		#endregion

		#region DailyRecurEvery

		public override ZInt DailyRecurEvery
		{
			get => base.DailyRecurEvery;
			set
			{
				base.DailyRecurEvery = value;
				InvokeFlightNumberChangeEvent();
			}
		}

		#endregion

		#region WeeklyRecurEvery

		public override ZInt WeeklyRecurEvery
		{
			get => base.WeeklyRecurEvery;
			set
			{
				base.WeeklyRecurEvery = value;
				InvokeFlightNumberChangeEvent();
			}
		}

		#endregion

		#region WeeklyRecurEvery

		public override ZInt MonthlyRecurEvery
		{
			get => base.MonthlyRecurEvery;
			set
			{
				base.MonthlyRecurEvery = value;
				InvokeFlightNumberChangeEvent();
			}
		}

		#endregion

		#region MonthlyFromDate

		public override ZDateTime MonthlyFromDate
		{
			get => base.MonthlyFromDate;
			set
			{
				base.MonthlyFromDate = value;
				InvokeFlightNumberChangeEvent();
			}
		}

		#endregion

		#region RangeFromDate

		public override ZDateTime RangeFromDate
		{
			get => base.RangeFromDate;
			set
			{
				base.RangeFromDate = value;
				InvokeFlightNumberChangeEvent();
			}
		}

		#endregion

		#region RangeFromDate

		public override ZDateTime RangeToDate
		{
			get => base.RangeToDate;
			set
			{
				base.RangeToDate = value;
				InvokeFlightNumberChangeEvent();
			}
		}

		#endregion

		#region Implementation

		protected void Initialize(bool includeWeeklyTimetable, bool importAndCreateMawb)
		{
			using (SuspendSettingHasChanges())
			{
				IncludeWeeklyTimetable = includeWeeklyTimetable;
				ImportAndCreateMAWB = importAndCreateMawb;

				if (!IncludeWeeklyTimetable && ToDateLimit.IsEmpty && FromDateLimit.IsValid)
				{
					ToDateLimit = FromDateLimit;
				}

				RangeFromDate = FromDateLimit;
				RangeToDate = ToDateLimit;

				DailyFromDate = RangeFromDate;
				DailyRecurEvery = 1;

				WeeklyDaysCheckedList = new ZBoolDescriptionPairList();
				WeeklyDaysCheckedList.OnPairChanged += args => InvokeFlightNumberChangeEvent();

				foreach (ICodeDescription weekDay in WeekDayCodeDescriptions)
				{
					var dayNumber = WeekDayCodeNumbers[weekDay.Code];

					if (IsWeekDayApplied(dayNumber))
					{
						WeeklyDaysCheckedList.AddNew(weekDay.Description, true);
					}
				}

				WeeklyRecurEvery = 1;

				MonthlyFromDate = RangeFromDate;
				MonthlyRecurEvery = 1;
			}

			if (ImportAndCreateMAWB)
			{
				using (ConsolDetails.SuspendSettingHasChanges())
				{
					ConsolDetails.ConsolsPerFlight = 1;
					ConsolDetails.ServiceLevel = OrgCarrierServiceLevel.StandardCode;
				}

				using (ConsolTemplateDetails.SuspendSettingHasChanges())
				{
					ConsolTemplateDetails.ServiceLevel = OrgCarrierServiceLevel.StandardCode;
				}
			}
		}

		protected virtual bool HasOperation(ZDateTime date) => false;

		public virtual bool HasMultipleCarriers
		{
			get
			{
				var carriers = sailingList
					.Select(s => s[0].JX_JV_VoyageFlight.SubstringSafe(0, AutoRoutingRequestConsolGenerator.Schema.AirlinePrefixMaxLength))
					.Distinct();
				return carriers.Count() > 1;
			}
		}

		public virtual ZString FirstCarrier =>
			sailingList.Any()
				? sailingList.First()[0].JX_JV_VoyageFlight.SubstringSafe(0, AutoRoutingRequestConsolGenerator.Schema.AirlinePrefixMaxLength)
				: ZString.Empty;

		protected virtual bool IsWeekDayApplied(int dayNumber) => false;

		protected virtual int GetDayNumber(ZDateTime date) => 0;

		string GetWeekDayDescription(ZDateTime date)
		{
			var dayNumber = GetDayNumber(date);
			var dayCode = WeekDayCodeNumbers.Where(x => x.Value == dayNumber).Select(x => x.Key).FirstOrDefault();
			return WeekDayCodeDescriptions[dayCode, StringComparison.Ordinal].Description;
		}

		enum RecurrencePattern
		{
			Daily, Weekly, Monthly
		}

		RecurrencePattern UsePattern
		{
			get { return usePattern; }
			set
			{
				if (usePattern != value)
				{
					usePattern = value;
					ValidateWhenChangingUsePattern();
				}
			}
		}
		RecurrencePattern usePattern;

		void ValidateWhenChangingUsePattern()
		{
			if (UseDailyPattern)
			{
				Validation.ValidateDailyFromDate();
				Validation.ValidateDailyRecurEvery();
			}
			else if (UseWeeklyPattern)
			{
				Validation.ValidateWeeklyRecurEvery();
			}
			else
			{
				Validation.ValidateMonthlyFromDate();
				Validation.ValidateMonthlyRecurEvery();
			}
		}

		protected override MultiDaysSelectionValidation GetNewValidation()
		{
			if (IncludeWeeklyTimetable)
			{
				return base.GetNewValidation();
			}

			return new MultiDaysSelectionValidationEmpty(this);
		}

		readonly protected List<JobSailingCollection> sailingList = new List<JobSailingCollection>();

		// This collection is for SchedulesConsolsForm to be able to display sailings as a single batch.
		public JobSailingCollection SailingCollection
		{
			get
			{
				if (sailingCollection == null)
				{
					sailingCollection = new JobSailingCollection(Factory);

					foreach (JobSailingCollection sailings in sailingList)
					{
						sailingCollection.AddRange(sailings);
					}
				}

				return sailingCollection;
			}
		}

		JobSailingCollection sailingCollection;

		readonly Dictionary<string, int> WeekDayCodeNumbers = new Dictionary<string, int>
		{
			{ DayOfWeekCodeList.Codes.Monday,    1 },
			{ DayOfWeekCodeList.Codes.Tuesday,   2 },
			{ DayOfWeekCodeList.Codes.Wednesday, 3 },
			{ DayOfWeekCodeList.Codes.Thursday,  4 },
			{ DayOfWeekCodeList.Codes.Friday,    5 },
			{ DayOfWeekCodeList.Codes.Saturday,  6 },
			{ DayOfWeekCodeList.Codes.Sunday,    7 }
		};

		readonly CodeDescriptionPairList WeekDayCodeDescriptions = new DayOfWeekCodeList();

		public RoutingRequestConsolGenerator ConsolDetails
		{
			get
			{
				if (consolDetails == null)
				{
					consolDetails = new RoutingRequestConsolGenerator(this);
					if (ImportAndCreateMAWB)
					{
						RegisterEditableChildObject(consolDetails);
					}
				}

				return consolDetails;
			}
		}

		RoutingRequestConsolGenerator consolDetails;

		#endregion
	}
}
