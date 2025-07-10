using System;
using System.Collections.Generic;
using System.Diagnostics;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Rating.Business.AutoRating
{
	[DebuggerDisplay("{FilteredEntries.Count}")]
	public class RateEntriesRepository
	{
		public RateEntriesRepository(IEnumerable<IRateEntry> entries, ILogger logger)
		{
			this.logger = logger;
			filteredEntries = new List<IRateEntry>(entries);
		}

		readonly ILogger logger;

		void LogRemoval(IRateEntry entry, string reason)
		{
			logger?.Log(LogType.Information, $"{LogEventTypes.RateEntryFiltered} {entry.DisplayInfo()} reason: {reason}."); // Filter log message
		}

		public void FilterEntries(Func<IRateEntry, string> predicate)
		{
			FilteredEntries
				.RemoveAll(item =>
				{
					var reason = predicate(item);

					if (string.IsNullOrWhiteSpace(reason))
					{
						return false;
					}

					LogRemoval(item, reason);
					return true;
				});
		}

		public void FilterEntries(HashSet<ZGuid> itemsToRemove, string reason)
		{
			FilteredEntries
				.RemoveAll(item =>
				{
					if (itemsToRemove.Contains(item.PK))
					{
						LogRemoval(item, reason);
						return true;
					}

					return false;
				});
		}

		readonly List<IRateEntry> filteredEntries;
		public List<IRateEntry> FilteredEntries => filteredEntries;
	}
}
