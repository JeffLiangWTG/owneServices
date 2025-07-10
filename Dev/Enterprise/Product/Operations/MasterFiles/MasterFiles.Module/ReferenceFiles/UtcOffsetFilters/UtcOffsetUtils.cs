using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Module
{
	public class UtcOffsetUtils
	{
		public UtcOffsetUtils(IEnumerable<ZShort> utcOffsetList)
		{
			UtcOffsetList = utcOffsetList;
			Min = utcOffsetList.Min();
			Max = utcOffsetList.Max();
		}

		short Min { get; }
		short Max { get; }
		IEnumerable<ZShort> UtcOffsetList { get; }

		public ZShort[] GetUtcOffsets(short from, short to)
		{
			if (from == to)
			{
				return new ZShort[] { from };
			}

			var list = new HashSet<ZShort>();

			if (from > to)
			{
				list.UnionWith(PopulateUtcOffsets(Min, to));
				list.UnionWith(PopulateUtcOffsets(from, Max));
			}
			else
			{
				list.UnionWith(PopulateUtcOffsets(from, to));
			}

			var maxUtcOffsetInFilter = list.Max();
			var minUtcOffsetInFilter = list.Min();

			if (maxUtcOffsetInFilter >= 780)
			{
				list.UnionWith(PopulateUtcOffsets(Min, maxUtcOffsetInFilter - 1440));
			}

			if (minUtcOffsetInFilter <= -600)
			{
				list.UnionWith(PopulateUtcOffsets(minUtcOffsetInFilter + 1440, Max));
			}

			return list.ToArray();
		}

		IEnumerable<ZShort> PopulateUtcOffsets(short from, short to)
		{
			return UtcOffsetList.Where(x => x >= from && x <= to);
		}
	}
}
