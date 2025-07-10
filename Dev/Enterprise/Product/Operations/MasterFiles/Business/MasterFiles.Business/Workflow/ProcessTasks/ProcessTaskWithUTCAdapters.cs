using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.RtfConverter;

namespace Enterprise.MasterFiles.Business
{
	[RowFetchStrategy(FetchStrategyType = typeof(ProcessTaskRowFetchStrategyWrapper))]
	public abstract class ProcessTaskWithUTCAdapters : AutoProcessTasks
	{
		public ProcessTaskWithUTCAdapters(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(P9_ActualDate), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(P9_ActualDateUtc), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(P9_OriginalScheduledDateUtc), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(P9_ScheduledDate), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(P9_ScheduledDateUtc), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(P9_SuspendedAt), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(P9_SuspendedAtUtc), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(P9_MilestoneExceptionAdded), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(P9_ExceptionAddedUtc), ConcurrencyPolicy.Ignore);
		}

		#region ActualDate

		public ZDateTimeOffset OriginalActualDateOffset
		{
			get => GetOffsetFromLocalAndUtc((ZDateTime)P9_ActualDateInfo.OriginalValue, (ZDateTime)P9_ActualDateUtcInfo.OriginalValue);
		}

		public ZDateTimeOffset P9_ActualDateOffset
		{
			get => GetOffsetFromLocalAndUtc(P9_ActualDate, P9_ActualDateUtc);
			set => (base.P9_ActualDate, base.P9_ActualDateUtc) = GetLocalAndUtcFromOffset(value);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new ZDateTime P9_ActualDate
		{
			get => base.P9_ActualDate;
			set => (base.P9_ActualDate, base.P9_ActualDateUtc) = GetLocalAndUtcFromLocal(value);
		} // Make it private so you always use dateOffset

		public override ZDateTime P9_ActualDateUtc
		{
			get => base.P9_ActualDateUtc;
			set => (base.P9_ActualDate, base.P9_ActualDateUtc) = GetLocalAndUtcFromUtc(value);
		}

		#endregion

		#region ScheduledDate

		protected ZDateTimeOffset P9_ScheduledDateOffset
		{
			get => GetOffsetFromLocalAndUtc(P9_ScheduledDate, P9_ScheduledDateUtc);
			set => (base.P9_ScheduledDate, base.P9_ScheduledDateUtc) = SanitizeSmallDateTime(GetLocalAndUtcFromOffset(value));
		}

		public override ZDateTime P9_ScheduledDate
		{
			get => base.P9_ScheduledDate;
			set => (base.P9_ScheduledDate, base.P9_ScheduledDateUtc) = SanitizeSmallDateTime(GetLocalAndUtcFromLocal(value));
		}

		public override ZDateTime P9_ScheduledDateUtc
		{
			get => base.P9_ScheduledDateUtc;
			set => (base.P9_ScheduledDate, base.P9_ScheduledDateUtc) = SanitizeSmallDateTime(GetLocalAndUtcFromUtc(value));
		}

		#endregion

		#region SuspendedAt

		public ZDateTimeOffset OriginalSuspendedAtOffset
		{
			get => GetOffsetFromLocalAndUtc((ZDateTime)P9_SuspendedAtInfo.OriginalValue, (ZDateTime)P9_SuspendedAtUtcInfo.OriginalValue);
		}

		public ZDateTimeOffset P9_SuspendedAtForBinding
		{
			get => GetOffsetFromLocalAndUtc(base.P9_SuspendedAt, base.P9_SuspendedAtUtc);
			set => (base.P9_SuspendedAt, base.P9_SuspendedAtUtc) = SanitizeSmallDateTime(GetLocalAndUtcFromOffset(value));
		}

		public ZPropertyInfo P9_SuspendedAtForBindingInfo => GetWrappedZPropertyInfo(nameof(P9_SuspendedAtForBinding), _ => P9_SuspendedAtInfo);

		public override ZDateTime P9_SuspendedAt
		{
			get => base.P9_SuspendedAt;
			set => (base.P9_SuspendedAt, base.P9_SuspendedAtUtc) = SanitizeSmallDateTime(GetLocalAndUtcFromLocal(value));
		}

		public override ZDateTime P9_SuspendedAtUtc
		{
			get => base.P9_SuspendedAtUtc;
			set => (base.P9_SuspendedAt, base.P9_SuspendedAtUtc) = SanitizeSmallDateTime(GetLocalAndUtcFromUtc(value));
		}

		#endregion

		#region P9_Notes_HTML

		public ZBlob P9_Notes_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(P9_Notes);
			}

			set
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				base.P9_Notes = ZBlob.FromUTF8(htmlToRtfConverter.Convert(value.ToUTF8()));
			}
		}

		#endregion

		#region MilestoneExceptionAdded

		public override ZDateTime P9_MilestoneExceptionAdded
		{
			get => base.P9_MilestoneExceptionAdded;
			set => (base.P9_MilestoneExceptionAdded, base.P9_ExceptionAddedUtc) = SanitizeSmallDateTime(GetLocalAndUtcFromLocal(value));
		}

		public override ZDateTime P9_ExceptionAddedUtc
		{
			get => base.P9_ExceptionAddedUtc;
			set => (base.P9_MilestoneExceptionAdded, base.P9_ExceptionAddedUtc) = SanitizeSmallDateTime(GetLocalAndUtcFromUtc(value));
		}

		public ZDateTimeOffset P9_ExceptionAddedForBinding
		{
			get => GetOffsetFromLocalAndUtc(P9_MilestoneExceptionAdded, P9_ExceptionAddedUtc);
			set => (base.P9_MilestoneExceptionAdded, base.P9_ExceptionAddedUtc) = SanitizeSmallDateTime(GetLocalAndUtcFromOffset(value));
		}

		public ZPropertyInfo P9_ExceptionAddedForBindingInfo => GetWrappedZPropertyInfo(nameof(P9_ExceptionAddedForBinding), _ => P9_MilestoneExceptionAddedInfo);

		#endregion

		protected static ZDateTimeOffset GetOffsetFromLocalAndUtc(ZDateTime local, ZDateTime utc)
		{
			if (local.IsValid)
			{
				if (utc.IsValid)
				{
					var timeSpan = local - utc;
					var offset = new TimeSpan(timeSpan.Hours, timeSpan.Minutes, 0);
					if (ZDateTimeOffset.IsValidOffset(offset))
					{
						return new ZDateTimeOffset(local, offset);
					}
					else
					{
						return utc.UtcToDateTimeOffset();
					}
				}
				else
				{
					return local.ToDateTimeOffset(null);
				}
			}
			else if (utc.IsValid)
			{
				return utc.UtcToDateTimeOffset();
			}
			else if (local.IsEmpty)
			{
				return ZDateTimeOffset.Empty;
			}
			else
			{
				return ZDateTimeOffset.Invalid;
			}
		}

		(ZDateTime smallLocal, ZDateTime smallUtc) SanitizeSmallDateTime((ZDateTime date, ZDateTime utc) dates)
		{
			if (dates.date.IsValid && (!dates.date.IsValidSmallDateTime || !dates.utc.IsValidSmallDateTime))
			{
				if (supressDateChangeErrorReport <= 0)
				{
					ErrorReporter.ReportOnce("Property is being set to an invalid time (not a valid small date time).", FormattableString.Invariant($"Date: {dates.date} Utc: {dates.utc} outside of range 01-Jan-1900 - 06-Jun-2079."));
				}
				return (dates.date.ToSmallDateTime(), dates.utc.ToSmallDateTime());
			}
			return dates;
		}

		protected static (ZDateTime local, ZDateTime utc) GetLocalAndUtcFromOffset(ZDateTimeOffset offset)
		{
			return (offset.ToZDateTime(), offset.ToUtcZDateTime());
		}

		protected static (ZDateTime local, ZDateTime utc) GetLocalAndUtcFromLocal(ZDateTime local)
		{
			return (local, local.ToUniversalBranchTime());
		}

		protected static (ZDateTime local, ZDateTime utc) GetLocalAndUtcFromUtc(ZDateTime utc)
		{
			return (utc.ToLocalBranchTime(), utc);
		}

		internal ZDateTimeOffset GetFromUtcDateIfApplicable(ZPropertyInfo utcInfo, ZDateTime baseValue)
		{
			var utfOffset = (ZDateTime)utcInfo.Value;
			if (baseValue.IsValid && utfOffset.IsValid)
			{
				var timeSpan = baseValue - utfOffset;
				var offset = new TimeSpan(timeSpan.Hours, timeSpan.Minutes, 0);
				if (ZDateTimeOffset.IsValidOffset(offset))
				{
					return new ZDateTimeOffset(baseValue, offset);
				}
			}

			return new ZDateTimeOffset(baseValue, DateTimeKind.Unspecified);
		}

		#region OffsetForUtcDates

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "offsetForUtcDates is reAssigned in the callback of delegate Action.")]
		TimeSpan? offsetForUtcDates;

		public IDisposable SetOffsetForUtcDates(TimeSpan offset)
		{
			offsetForUtcDates = offset;
			return new DisposableAction(() => offsetForUtcDates = null);
		}

		#endregion

		int supressDateChangeErrorReport;

		internal IDisposable SupressScheduledDateChangeErrorReport()
		{
			supressDateChangeErrorReport++;
			return new DisposableAction(() => supressDateChangeErrorReport--);
		}

#if DEBUG

		public void SetLocalDateWithoutSettingUTCForTest(string propertyName, ZDateTime value)
		{
			switch (propertyName)
			{
				case ProcessTasksSchema.Constants.P9_ActualDate:
					base.P9_ActualDate = value;
					break;

				case ProcessTasksSchema.Constants.P9_MilestoneExceptionAdded:
					base.P9_MilestoneExceptionAdded = value;
					break;

				case ProcessTasksSchema.Constants.P9_ScheduledDate:
					base.P9_ScheduledDate = value;
					break;

				case ProcessTasksSchema.Constants.P9_SuspendedAt:
					base.P9_SuspendedAt = value;
					break;
			}
		}

#endif
	}
}
