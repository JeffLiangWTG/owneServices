using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class MilestonePositionHelper
	{
		public MilestonePositionHelper(IEnumerable<ProcessTask> items)
		{
			this.items = items;
		}

		public ProcessTask LastMilestone
		{
			get
			{
				return GetLastMilestone(false);
			}
		}

		public ProcessTask CurrentCompanyLastMilestone
		{
			get
			{
				return GetLastMilestone(true);
			}
		}

		public ProcessTask NextMilestone
		{
			get
			{
				return GetNextMilestone(false);
			}
		}

		public ProcessTask CurrentCompanyNextMilestone
		{
			get
			{
				return GetNextMilestone(true);
			}
		}

		#region Implementation

		readonly IEnumerable<ProcessTask> items;

		ProcessTask GetLastMilestone(bool forCurrentCompany)
		{
			var result = items
						.Where(x => !x.IsDeleted
									&& (!forCurrentCompany || x.P9_GC == GlbCompany.CurrentCompany.PK)
									&& x.P9_ActualDate.IsValid)
						.MaxBySafe(x => x.P9_ActualDate);

			return result;
		}

		ProcessTask GetNextMilestone(bool forCurrentCompany)
		{
			return items.Where(x => !x.IsDeleted && !x.P9_ActualDate.IsValid && (!forCurrentCompany || x.P9_GC == GlbCompany.CurrentCompany.PK))
				.MinBySafe(s => s, new ScheduledDateSequenceComparer());
		}

		class ScheduledDateSequenceComparer : IComparer<ProcessTask>
		{
			public int Compare(ProcessTask x, ProcessTask y)
			{
				return ContinuationComparer(x, y,
					i => !i.P9_ScheduledDate.IsValid,
					i => i.P9_ScheduledDateForBinding.IsValid ? i.P9_ScheduledDateForBinding : ZDateTimeOffset.Empty,
					i => i.P9_Sequence);
			}

			int ContinuationComparer<T>(T x, T y, params Func<T, IComparable>[] getters)
			{
				foreach (var getter in getters)
				{
					var result = getter(x).CompareTo(getter(y));
					if (result != 0)
					{
						return result;
					}
				}

				return 0;
			}
		}

		#endregion
	}
}
