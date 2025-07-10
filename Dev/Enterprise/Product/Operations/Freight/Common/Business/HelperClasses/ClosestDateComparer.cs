using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Freight.Common.Business
{
	public class ClosestDateComparer : IComparer<ZDateTime>
	{
		public ClosestDateComparer(ZDateTime referenceDate)
		{
			this.referenceDate = referenceDate;
		}

		readonly ZDateTime referenceDate;

		public int Compare(ZDateTime dateTime1, ZDateTime dateTime2)
		{
			if (!referenceDate.IsValid)
			{
				return 0;
			}

			var voyage1Diff = dateTime1.IsValid ? Math.Abs(dateTime1.Ticks - referenceDate.Ticks) : long.MinValue;
			var voyage2Diff = dateTime2.IsValid ? Math.Abs(dateTime2.Ticks - referenceDate.Ticks) : long.MinValue;

			return voyage1Diff.CompareTo(voyage2Diff);
		}
	}
}
