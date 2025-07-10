using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class ProcessTaskCollectionViewExtensions
	{
		public static ProcessTask GetNextTask(this BusinessObjectCollection<ProcessTask> items)
		{
			return items.Where(x => !x.IsDeleted && x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed && x.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled)
			.MinBySafe(s => s, new ScheduledDateSequenceComparer());
		}

		class ScheduledDateSequenceComparer : IComparer<ProcessTask>
		{
			public int Compare(ProcessTask x, ProcessTask y)
			{
				return ContinuationComparer(x, y,
					i => !i.P9_ScheduledDate.IsValid,
					i => i.P9_ScheduledDate.IsValid ? i.P9_ScheduledDate : ZDateTimeOffset.Empty,
					i => i.P9_Sequence,
					i => i.P9_TaskID);
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
	}
}
