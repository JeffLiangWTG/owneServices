using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	[DebuggerDisplay("{DebuggerDisplay}")]
	public sealed class RateLinesRepository : IDisposable
	{
		public RateLinesRepository(RatingCriteria criteria, List<IRateEntry> entries, ILogger logger)
			: this(criteria, entries, null, logger)
		{ }

		public RateLinesRepository(RatingCriteria criteria, List<IRateEntry> entries, AccChargeCode chargeCode, ILogger logger)
		{
			Argument.NotNull(criteria, nameof(criteria));

			keyChargeCode = chargeCode;
			this.additionLogger = logger.SortAndDistinct();
			this.removalLogger = logger.SortAndDistinct();
			this.afterSearchAndFilterLogger = logger.SortAndDistinct();
			this.afterSearchLogger = logger.SortAndDistinct();
			this.criteria = criteria;

			var rateEntries = entries.OfType<RateEntry>().Where(x => x != null);
			if (rateEntries.Any())
			{
				var factory = rateEntries.First().Factory;
				factory.AddFetchHint(RateLinesSchema.Instance, new ZQuery(RateLinesSchema.TL_TI, rateEntries.Select(x => x.PK)));
			}

			var filteredRateEntries = entries.Where(x => x != null);

			AddFetchHintsForRateEntries(filteredRateEntries);

			Add(true, filteredRateEntries.SelectMany(x => x.ChildRateLines).ToList());
		}

		readonly RatingCriteria criteria;
		readonly AccChargeCode keyChargeCode;
		readonly IDisposableLogger additionLogger;
		readonly IDisposableLogger removalLogger;
		readonly IDisposableLogger afterSearchAndFilterLogger;
		readonly IDisposableLogger afterSearchLogger;
		readonly Dictionary<AccChargeCode, List<FastLine>> inner = new Dictionary<AccChargeCode, List<FastLine>>(new ChargeCodeComparer());
		readonly List<RemoveResult> retainedLinesForLogging = new List<RemoveResult>();

		#region SuppressResourceStringsCheckRegion

#if DEBUG
		public void AddForTest(params IRateLine[] lines)
		{
			Add(true, lines);
		}
#endif

		void Add(bool log, IEnumerable<IRateLine> rateLines)
		{
			var chargeComparer = inner.Comparer;
			var sortedAndFilteredLines = rateLines
				.Where(x => ConvertGlobalChargeToLocal(log, x)
							&& IsChargeCodeNotNullAndMatchesKey(x.ChargeCode, chargeComparer))
				.OrderBy(x => x.ParentRateEntry.TI_RC)
				.ThenBy(x => x.ChargeCode.AC_Code);

			foreach (var line in sortedAndFilteredLines)
			{
				List<FastLine> list;
				if (!inner.TryGetValue(line.ChargeCode, out list))
				{
					list = new List<FastLine>();
					inner[line.ChargeCode] = list;
				}

				MapGlobalChargeCodeToLocalForPercentageCalculator(line, log);
				list.Add(criteria.Cache.GetOrCreateFastLine(line));

				if (log)
				{
					additionLogger.Information(ZString.Format("{0} {1}", LogEventTypes.RateLineFound, line.DisplayInfo()));
				}
			}
		}

		bool IsChargeCodeNotNullAndMatchesKey(AccChargeCode chargeCode, IEqualityComparer<AccChargeCode> chargeComparer)
			=> chargeCode != null && (keyChargeCode == null || chargeComparer.Equals(chargeCode, keyChargeCode));

		bool ConvertGlobalChargeToLocal(bool log, IRateLine line)
		{
			bool result = true;
			if (line is RateLine rateLine)
			{
				var chargeCode = rateLine.ChargeCode;
				if (chargeCode != null && chargeCode.IsGlobal)
				{
					var localChargeCode = GetLocalChargeCode(chargeCode, log, _Rating.Cost);
					if (localChargeCode != null)
					{
						rateLine.LockCalculator = true;
						rateLine.TL_AC = localChargeCode.PK;
					}
					else
					{
						result = false;
						if (log)
						{
							var info = ZString.Format("{0} {1}\treason:\tCharge Code {2} cannot be found or is not valid in the current company",
								LogEventTypes.RateLineFiltered,
								rateLine.DisplayInfo(),
								chargeCode.AC_Code);
							removalLogger.Information(info);
						}
					}
				}
			}
			return result;
		}

		void MapGlobalChargeCodeToLocalForPercentageCalculator(IRateLine line, bool log)
		{
			foreach (var rateLineItem in line.ChildRateLineItems.OfType<RateLineItem>())
			{
				var hasApplyToChargeCode = rateLineItem.TM_Text == CalculatorConstants.Text.ChargeCode
					&& !rateLineItem.TM_AC.IsEmpty
					&& rateLineItem.RateOperatorIsApplyTo();

				if (hasApplyToChargeCode)
				{
					var applyToRateLineItemChargeCode = rateLineItem.ChargeCode;
					if (applyToRateLineItemChargeCode.IsGlobal)
					{
						var localChargeCode = GetLocalChargeCode(applyToRateLineItemChargeCode, log, line.IsCostRate());
						if (localChargeCode != null)
						{
							rateLineItem.TM_AC = localChargeCode.PK;
						}
						else if (log)
						{
							var info = ZString.Format("{0} could not be applied to Charge Code {1} as it could not be found or is not valid in the current company",
								line.DisplayInfo(),
								applyToRateLineItemChargeCode.AC_Code);
							removalLogger.Information(info);
						}
					}
				}
			}
		}

		public void AddSilently(IRateLine[] lines)
		{
			Add(false, lines);
		}

		public void AddSilently(IEnumerable<FastLine> lines)
		{
			Add(false, lines.Select(x => x.Line));
		}

		public void ForceAdd(IEnumerable<IRateLine> lines)
		{
			foreach (var line in lines)
			{
				if (!inner.TryGetValue(line.ChargeCode, out var list))
				{
					list = new List<FastLine>();
					inner[line.ChargeCode] = list;
				}

				var fastLine = criteria.Cache.GetOrCreateFastLine(line);
				list.Add(fastLine);
			}
		}

		public string DebuggerDisplay
		{
			get
			{
				var sb = new ZStringBuilder();

				foreach (var pair in inner)
				{
					sb.Append(pair.Value.Count + "x" + (pair.Key != null ? pair.Key.AC_Code : (ZString)"null"));
				}

				return sb.ToStringWithDelimiterBetweenAppends(", ");
			}
		}

		public bool ContainsKey(AccChargeCode chargeCode)
		{
			return inner.ContainsKey(chargeCode);
		}

		public List<IRateLine> GetIRateLines()
		{
			return new List<IRateLine>(inner.Values.SelectMany(x => x).Select(x => x.Line));
		}

		/// <summary>
		/// Groups similar lines by Service Provider if the registry permits or otherwise into a single group.
		/// Used when deciding which rate line is the most suitable to use for autorating within each group.
		/// </summary>
		/// <param name="chargeCode">charge code of lines to group.</param>
		public IEnumerable<List<FastLine>> GetSimilarLineGroups(AccChargeCode chargeCode)
		{
			var lines = GetLines(chargeCode);

			if (chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Freight || lines.Count == 0 || !lines[0].IsCostRate())
			{
				return new List<List<FastLine>> { lines };
			}

			var transportMode = criteria.FreightMode.ToTransportMode();
			var shouldGroupByProvider = DataRegistryRating.Instance.AllowChargesWithSameChargeCodeForDifferentProvider
				.GetIsEnabled(criteria.ConsumerType?.Code ?? string.Empty, transportMode, criteria.JobDirection);

			if (!shouldGroupByProvider)
			{
				return new List<List<FastLine>> { lines };
			}

			// When we have lines for different service providers, the standard costs will be filtered out by creditor comparer.
			// Therefore it doesn't matter to which group we add standard costs, as long as they are added to any existing group
			// so they are filtered out from the repository by one of comparers (CostProviderComparer).
			var groups = lines
				.Where(l => !l.ParentRateEntry.IsStandardCostRate())
				.GroupBy(line => line.ParentRateEntry.ParentRatingHeader.TH_OH)
				.Select(l => l.ToList())
				.ToList();

			if (groups.Count == 0)
			{
				return new List<List<FastLine>> { lines };
			}

			groups[0].AddRange(lines.Where(l => l.ParentRateEntry.IsStandardCostRate()));

			return groups;
		}

		public List<FastLine> GetLines()
		{
			return new List<FastLine>(inner.Values.SelectMany(x => x));
		}

		public List<AccChargeCode> GetChargeCodes()
		{
			return new List<AccChargeCode>(inner.Keys);
		}

		public List<FastLine> GetLines(AccChargeCode chargeCode)
		{
			return new List<FastLine>(inner[chargeCode]);
		}

		void LogLineRemoved(FastLine line, string reason, bool isWarning)
		{
			var message = ZString.Format("{0} {1}\treason:\t{2}", LogEventTypes.RateLineFiltered, line.Line.DisplayInfo(), reason);
			removalLogger.Log(isWarning ? LogType.Warning : LogType.Information, message);
		}

		void LogLineRetained(FastLine line, string reason)
		{
			var message = ZString.Format("{0} {1}\treason:\t{2}", LogEventTypes.RateLineNotFiltered, line.Line.DisplayInfo(), reason);
			removalLogger.Log(LogType.Information, message);
		}

		void LogLinesRetained()
		{
			var allLines = GetLines();
			retainedLinesForLogging
				.Where(r => allLines.Contains(r.Line))
				.ForEach(r => LogLineRetained(r.Line, r.RetainedReason));
		}

		public void Remove(FastLine line, string reason, bool isWarning = false)
		{
			List<FastLine> list;
			if (inner.TryGetValue(line.Line.ChargeCode, out list))
			{
				list.Remove(line);
				LogLineRemoved(line, reason, isWarning);

				var fastLinesContainsRemovedLine = inner.SelectMany(i => i.Value).Where(fastLine => fastLine.Line.IncludedLines.Contains(line.Line));
				fastLinesContainsRemovedLine.ForEach(f => f.Line.IncludedLines.Remove(line.Line));
			}
		}

		public int Remove(Func<FastLine, bool> conditionToRemove, Func<FastLine, string> reason, bool withWarning = false)
		{
			RemoveResult removeHelper(FastLine line)
			{
				return new RemoveResult(line)
				{
					ShouldRemove = conditionToRemove(line),
					RemovalReason = reason(line),
					WithWarning = withWarning
				};
			}
			return Remove(removeHelper);
		}

		public int Remove(Func<FastLine, bool> conditionToRemove, string reason, bool withWarning = false)
		{
			RemoveResult removeHelper(FastLine line)
			{
				return new RemoveResult(line)
				{
					ShouldRemove = conditionToRemove(line),
					RemovalReason = reason,
					WithWarning = withWarning
				};
			}

			return Remove(removeHelper);
		}

		public int Remove(Func<FastLine, RemoveResult> conditionToRemove)
		{
			var removedCount = 0;
			foreach (var line in GetLines())
			{
				var result = conditionToRemove(line);
				if (result.ShouldRemove)
				{
					Remove(line, result.RemovalReason, result.WithWarning);
					removedCount++;
				}
				else if (!result.RetainedReason.IsNullOrEmpty())
				{
					retainedLinesForLogging.Add(result);
				}
			}

			return removedCount;
		}

		public bool Remove(AccChargeCode chargeCode, string reason)
		{
			if (inner.TryGetValue(chargeCode, out var list))
			{
				var wasNotEmtpy = list.Any();

				foreach (var line in list)
				{
					//If we change this to Warning, we will show this message to users in pop-up at the end of the Autorating and some customers don't like it(WI00394351)
					//Do not change this line unless confirmed with product
					LogLineRemoved(line, reason, isWarning: false);
				}

				list.Clear();

				return wasNotEmtpy;
			}

			return false;
		}

		public void Remove(Func<AccChargeCode, bool> conditionToRemove, string reason)
		{
			foreach (var chargeCode in GetChargeCodes())
			{
				if (conditionToRemove(chargeCode))
				{
					Remove(chargeCode, reason);
				}
			}
		}

		void AddFetchHintsForRateEntries(IEnumerable<IRateEntry> rateEntries)
		{
			foreach (var entry in rateEntries)
			{
				var boEntry = entry as RateEntry;
				if (boEntry != null)
				{
					boEntry.Factory.AddFetchHint(AccChargeCodeSchema.Instance, new ZQuery(AccChargeCodeSchema.PK, entry.ChildRateLines.OfType<RateLine>().Select(x => x.TL_AC)));
					boEntry.Factory.AddFetchHint(RateLineItemsSchema.Instance, new ZQuery(RateLineItemsSchema.TM_TL, entry.ChildRateLines.OfType<RateLine>().Select(x => x.PK)));
				}
			}
		}

		AccChargeCode GetLocalChargeCode(AccChargeCode globalChargeCode, bool log, bool isCost)
		{
			var ledgerType = isCost ? LedgerTypes.AccountsPayable : LedgerTypes.AccountsReceivable;
			var globalChargeCodeCacheKey = ZString.Format("{0}|{1}", globalChargeCode.PK, ledgerType);
			var localChargeCode = criteria.Factory.GetCachedValue(globalChargeCodeCacheKey, () => MapGlobalChargeCodeToLocalChargeCode(globalChargeCode, ledgerType, log));

			return localChargeCode;
		}

		AccChargeCode MapGlobalChargeCodeToLocalChargeCode(AccChargeCode globalChargeCode, string ledgerType, bool log)
		{
			var localChargeCode = AccChargeCodeExtensions.GetLocalChargeCodeFromIntercompanyChargeCodeMapping(globalChargeCode.AC_Code, ledgerType, criteria.LocalClient?.PK, criteria.Factory);
			if (localChargeCode != null)
			{
				if (log)
				{
					additionLogger.Information(ZString.Format("{0} {1} ({2} -> {3})", LogEventTypes.ChargeCodeMappingSource, ZArchitecture.Modules.ModuleIDs.GlobalChargeCodeIntercompany.Description, globalChargeCode.AC_Code, localChargeCode.AC_Code));
				}
			}
			else
			{
				localChargeCode = globalChargeCode.ChildChargeCodes.SingleOrDefault(c => c.AC_GC == Environment.Env.CurrentCompanyPK);
			}

			return localChargeCode;
		}

		public ILogger LogAfterSearchAndFilter()
		{
			return afterSearchAndFilterLogger;
		}
		public ILogger LogAfterSearch()
		{
			return afterSearchLogger;
		}

		public void Dispose()
		{
			LogLinesRetained();

			additionLogger.Dispose();
			afterSearchLogger.Dispose();
			removalLogger.Dispose();
			afterSearchAndFilterLogger.Dispose();
		}

		#endregion
	}

	public class ChargeCodeComparer : IEqualityComparer<AccChargeCode>
	{
		public bool Equals(AccChargeCode x, AccChargeCode y)
			=>
			x.AC_Code == y.AC_Code && // check the charge code
			(x.AC_GC == y.AC_GC || x.AC_GC.IsEmpty != y.AC_GC.IsEmpty); // check local/global charge code

		public int GetHashCode(AccChargeCode obj) => obj.AC_Code.GetHashCode();
	}

	public class RemoveResult
	{
		public RemoveResult(FastLine line)
		{
			Line = line;
		}

		public FastLine Line { get; }
		public bool ShouldRemove { get; set; }
		public bool WithWarning { get; set; }
		public string RemovalReason { get; set; }
		public string RetainedReason { get; set; }
	}
}

