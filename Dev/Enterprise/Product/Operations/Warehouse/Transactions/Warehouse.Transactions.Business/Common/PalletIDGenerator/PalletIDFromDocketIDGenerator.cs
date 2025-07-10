using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	class PalletIDFromDocketIDGenerator : IPalletIDFromDocketIDGenerator
	{
		public IEnumerable<GeneratedID> GenerateIDs(WhsDocket docket, int numberOfIDs, int startID = 1)
		{
			Argument.NotNull(docket, nameof(docket));
			Argument.GreaterThanZero(numberOfIDs, nameof(numberOfIDs));
			Argument.GreaterThanZero(startID, nameof(startID));

			var idPrefix = docket.WD_DocketID + IDSeparator;

			var sortedDocketLinePalletIDs = docket.Lines
				.Where(rl => IsValidID(idPrefix, rl.WE_PalletID))
				.Select(rl => ZInt.Parse(rl.WE_PalletID.Right(PadLength)))
				.OrderBy(pid => pid)
				.ToArray();

			var sortedDocketLinePalletIDsIndex = 0;

			var ids = new List<GeneratedID>(numberOfIDs);

			var id = startID;

			while (ids.Count < numberOfIDs)
			{
				id = GetSmallestUnusedIDLine(sortedDocketLinePalletIDs, id, ref sortedDocketLinePalletIDsIndex);
				if (id > MaxIDSequence)
				{ break; }

				ids.Add(new GeneratedID(GetFormattedID(idPrefix, id), id));

				id++;
			}

			return ids;
		}

		ZString GetFormattedID(ZString idPrefix, ZInt id)
		{
			return idPrefix + id.ToString().PadLeft(PadLength, '0');
		}

		int GetSmallestUnusedIDLine(ZInt[] sortedDocketLinePalletIDs, int startID, ref int sortedDocketLinePalletIDsIndex)
		{
			var result = (sortedDocketLinePalletIDsIndex == 0) ? 1 : sortedDocketLinePalletIDs[sortedDocketLinePalletIDsIndex - 1] + 1;

			for (; sortedDocketLinePalletIDsIndex < sortedDocketLinePalletIDs.Length; sortedDocketLinePalletIDsIndex++)
			{
				var id = sortedDocketLinePalletIDs[sortedDocketLinePalletIDsIndex];

				if (result < id)
				{
					if (result >= startID)
					{
						break;
					}
					if (id > startID)
					{
						result = startID;
						break;
					}
				}

				result = id + 1;
			}

			if (result < startID)
			{
				result = startID;
				sortedDocketLinePalletIDsIndex = sortedDocketLinePalletIDs.Length;
			}

			return result;
		}

		bool IsValidID(ZString idPrefix, ZString id)
		{
			var result = false;
			if (id.Length > 0)
			{
				int validIDLength = idPrefix.Length + PadLength;
				if (id.Length == validIDLength)
				{
					result = id.Left(idPrefix.Length) == idPrefix && id.Right(PadLength).IsNumbersOnlyOrEmpty;
				}
			}
			return result;
		}

		ZInt PadLength
		{
			get { return MaxIDSequence.ToString().Length; }
		}

		const string IDSeparator = "-";
		const int MaxIDSequence = 9999;
	}
}
