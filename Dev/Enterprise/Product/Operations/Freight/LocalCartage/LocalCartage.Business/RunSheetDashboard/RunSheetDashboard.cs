using System;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class RunSheetDashboard : AutoRunSheetDashboard
	{
		public RunSheetDashboard(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public CommonWorkSheetCollection RunSheets
		{
			get
			{
				if (runSheets == null)
				{
					runSheets = new CommonWorkSheetCollection(Factory);
					RunSheets.ApplySort(CommonWorkSheet.Schema.Heading, System.ComponentModel.ListSortDirection.Ascending);
					runSheets.AdditionalFilter = ZQuery.NoResultQuery;
				}
				return runSheets;
			}
		}

		CommonWorkSheetCollection runSheets;

		public event EventHandler FilterChanged;

		void NotifyFilterChanged()
		{
			var filterChanged = this.FilterChanged;

			if (filterChanged != null)
			{
				filterChanged(this, EventArgs.Empty);
			}
		}

		[List("Weekdays")]
		public override ZString DateRangeFilter
		{
			get { return base.DateRangeFilter; }
			set
			{
				base.DateRangeFilter = value;
				UpdateFilter();
			}
		}

		public MultilingualString DateRangeFilterMultilingualString
		{
			get
			{
				MultilingualString result = (NoResString)"";
				if (!string.IsNullOrEmpty(DateRangeFilter))
				{
					result = Weekdays.ContainsCode(DateRangeFilter) ? ((CodeDescriptionPair)Weekdays[DateRangeFilter, StringComparison.OrdinalIgnoreCase]).MultilingualCode : (NoResString)DateRangeFilter;
				}
				return result;
			}
		}

		public override ZDateTime DateRangeFrom
		{
			get { return base.DateRangeFrom; }
			set
			{
				base.DateRangeFrom = value;
				UpdateFilter();
			}
		}

		public override ZDateTime DateRangeTo
		{
			get { return base.DateRangeTo; }
			set
			{
				base.DateRangeTo = value;
				UpdateFilter();
			}
		}

		public bool IsDateRange
		{
			get { return DateRangeFilter == Days.DateRange.GetUnresolvedString(); }
		}

		[List("Branches")]
		public override ZGuid Branch
		{
			get { return base.Branch; }
			set
			{
				base.Branch = value;
				UpdateFilter();
			}
		}

		public ZString BranchName
		{
			get
			{
				var branch = Factory.Load<GlbBranch>(Branch);
				return branch != null ? branch.GB_BranchName : ZString.Empty;
			}
		}

		ZGuid OrgProxy
		{
			get
			{
				var branch = Factory.Load<GlbBranch>(Branch);
				return branch != null ? branch.GB_OH_OrgProxy : ZGuid.Empty;
			}
		}

		public bool HasRunSheets
		{
			get { return runSheets.Count > 0; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Result limit checking")]
		public void UpdateFilter()
		{
			if (IsFilterUpdateSuspended)
			{
				return;
			}

			var query = GetWeekDayQuery();

			if (!Branch.IsEmpty && !query.IsNoResultQuery)
			{
				query.AddToFilter(GetBranchQuery());
			}
			else if (!Branch.IsEmpty && (!IsDateRange || IsDateRange && DateRangeFrom.IsValid && DateRangeTo.IsValid))
			{
				query = GetBranchQuery();
			}

			maxResultLimitReached = !query.IsNoResultQuery && Factory.GetDatabaseCount(typeof(CommonWorkSheet), query) > MaxResults;

			if (!query.IsNoResultQuery)
			{
				query.MaximumRows = MaxResults;
			}

			RunSheets.AdditionalFilter = query;

			NotifyFilterChanged();
		}

		public ZBool MaxResultLimitReached
		{
			get { return maxResultLimitReached; }
		}

		ZBool maxResultLimitReached = false;
		public const int MaxResults = 50;

		ZQuery GetWeekDayQuery()
		{
			var query = ZQuery.NoResultQuery;
			ZDateTime from;
			ZDateTime to;

			if (Weekdays.ContainsCode(DateRangeFilter) && !IsDateRange || (IsDateRange && DateRangeFrom.IsValid && DateRangeTo.IsValid))
			{
				if (IsDateRange)
				{
					from = DateRangeFrom;
					to = DateRangeTo.AddDays(1);
				}
				else
				{
					var i = ZInt.ParseSafe(Weekdays.GetDescriptionFromCode(DateRangeFilter), ZInt.Zero);
					from = ZDateTime.Today.AddDays(i);
					to = ZDateTime.Today.AddDays(i + 1);
				}
				query = new ZQuery(JobCartageRunSheetSchema.EY_EndTime, SQLComparisonOperator.GreaterThan, from);
				query.AddToFilter(JobCartageRunSheetSchema.EY_StartTime, SQLComparisonOperator.LessThan, to);
			}
			return query;
		}

		ZQuery GetBranchQuery()
		{
			var result = new ZDBOnlyQuery(typeof(CommonWorkSheet));

			var staffQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_Code);
			var staffBranchQuery = new ZQuery(GlbStaffSchema.GS_GB_HomeBranch, Branch);
			staffQuery.AddToFilter(staffBranchQuery);

			var truckQuery = new ZDBOnlySubQuery(typeof(RefEquipment), RefEquipmentSchema.PK);
			var truckBranchQuery = new ZQuery(RefEquipmentSchema.RQ_OH_Owner, OrgProxy);
			truckQuery.AddToFilter(truckBranchQuery);

			result.AddSubQuery(JobCartageRunSheetSchema.EY_GS_NKTruckDriver, staffQuery, JoinCondition.Or);
			result.AddSubQuery(JobCartageRunSheetSchema.EY_RQ_Truck, truckQuery, JoinCondition.Or);

			return result;
		}

		public GlbBranchCollection Branches
		{
			get { return branches ?? (branches = new GlbBranchCollection(Factory)); }
		}

		GlbBranchCollection branches;

		public CodeDescriptionPairList Weekdays
		{
			get
			{
				var result = new CodeDescriptionPairList();
				for (int i = -1; i < 7; i++)
				{
					result.AddPair(GetWeekdayText(i), i.ToString(CultureInfo.InvariantCulture));
				}

				result.AddPair(Days.DateRange, Days.DateRange);

				return result;
			}
		}

		MultilingualString GetWeekdayText(int i)
		{
			switch (i)
			{
				case -1:
					return Days.Yesterday;
				case 0:
					return Days.Today;
				case 1:
					return Days.Tomorrow;
				default:
					return ((IMultilingualDescription)new DayOfWeekCodeList()[(int)ZDateTime.Today.AddDays(i).DayOfWeek]).MultilingualDescription;
			}
		}

		static class Days
		{
			public static MultilingualString Yesterday
			{
				get { return ResString.GetMultilingualString("3b31c344-5a6f-4012-8e67-4581e73d6a0d", "Yesterday"); }
			}
			public static MultilingualString Today
			{
				get { return ResString.GetMultilingualString("1db37e10-d40b-4466-9bad-0a0111a12a63", "Today"); }
			}
			public static MultilingualString Tomorrow
			{
				get { return ResString.GetMultilingualString("a4f8a972-876f-483b-8765-2966dfaf75d4", "Tomorrow"); }
			}
			public static MultilingualString DateRange
			{
				get { return ResString.GetMultilingualString("1f37dad4-f22e-49ed-a3d0-5d0afaee5a26", "Date range"); }
			}
		}

		public override ZString ErrorMessage
		{
			get
			{
				if (RunSheets.Count > 0)
				{
					return string.Empty;
				}

				if (BranchInfo.HasErrors())
				{
					return Res.GetString("e42b2284-083d-472d-8612-8d8b6f3f731d", "Please enter valid branch");
				}

				bool hasDateRangeFilter = !string.IsNullOrEmpty(DateRangeFilter);
				bool hasBranchFilter = !Branch.IsEmpty;

				var dateRangeFilterMessage = DateRangeFilterMultilingualString;

				if (IsDateRange && !DateRangeFromInfo.HasErrors() && !DateRangeToInfo.HasErrors())
				{
					dateRangeFilterMessage = ResString.GetMultilingualString("b9fdadb0-fae6-4f02-9cb6-cf83c8402b0c", "this date range");
				}
				else if (IsDateRange && (DateRangeFrom.IsEmpty || DateRangeTo.IsEmpty))
				{
					return Res.GetString("85e3b5c7-28cb-417c-b4fa-a40af83a8d24", "Start and end dates are mandatory");
				}
				else if (IsDateRange && (DateRangeFromInfo.HasErrors() || DateRangeToInfo.HasErrors()))
				{
					return Res.GetString("5164c21b-bad6-4078-babe-b4d2b260bbec", "Please enter valid date range");
				}

				if (hasDateRangeFilter && hasBranchFilter)
				{
					return Res.GetString("1f28f618-2a0f-4d09-b20c-82a899913fd6", "No matching run sheets for {0} and {1} can be found", dateRangeFilterMessage, BranchName);
				}
				else if (hasDateRangeFilter && !hasBranchFilter)
				{
					return Res.GetString("d578d21b-9b1c-4094-bb81-cf77ef78c3d1", "No matching run sheets for {0} can be found", dateRangeFilterMessage);
				}
				else if (!hasDateRangeFilter && hasBranchFilter)
				{
					return Res.GetString("d578d21b-9b1c-4094-bb81-cf77ef78c3d1", "No matching run sheets for {0} can be found", BranchName);
				}
				else
				{
					return Res.GetString("2b426ec0-2142-495d-b122-a142b8e3c766", "Please select date range and/or branch");
				}
			}
		}

		public override bool HasChanges
		{
			get { return false; }
			set { base.HasChanges = value; }
		}

		public override void CopyTransientProperties(BusinessObject copy)
		{
			var newDashboard = (RunSheetDashboard)copy;
			using (newDashboard.SuspendFilterUpdate())
			{
				newDashboard.Branch = Branch;
				newDashboard.DateRangeFilter = DateRangeFilter;
				newDashboard.DateRangeFrom = DateRangeFrom;
				newDashboard.DateRangeTo = DateRangeTo;
			}
		}

		public IDisposable SuspendFilterUpdate()
		{
			return new FilterUpdateSuspender(this);
		}

		ZBool IsFilterUpdateSuspended
		{
			get { return FilterUpdateSuspenderLevel > 0; }
		}

		int FilterUpdateSuspenderLevel;

		sealed class FilterUpdateSuspender : IDisposable
		{
			public FilterUpdateSuspender(RunSheetDashboard dashboard)
			{
				if (dashboard == null)
				{
					throw new ArgumentNullException(nameof(dashboard));
				}

				Dashboard = dashboard;
				Dashboard.FilterUpdateSuspenderLevel++;
			}
			readonly RunSheetDashboard Dashboard;

			public void Dispose()
			{
				if (!disposed)
				{
					disposed = true;
					Dashboard.FilterUpdateSuspenderLevel--;
				}
			}

			bool disposed;
		}
	}
}
