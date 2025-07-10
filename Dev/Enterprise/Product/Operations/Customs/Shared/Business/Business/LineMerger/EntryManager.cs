using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class EntryManager
	{
		public EntryManager(BaseJobDeclaration declaration, EntryCreationStrategy strategy)
		{
			this.declaration = declaration;
			this.strategy = strategy;
			entriesByMergeKey = new Dictionary<MergeKey, CusEntryHeader>();
			mergeKeysByEntry = new Dictionary<CusEntryHeader, MergeKey>();
			entryLinesByMergeKey = new Dictionary<MergeKey, CusEntryLine>();

			numberOfEntryLinesCreated = new Dictionary<CusEntryHeader, int>();

			unusedExistingEntries = new List<CusEntryHeader>();
			unusedExistingEntryLines = new List<CusEntryLine>();
			unusedExistingLinks = new List<AdditionalInvoiceLineEntryLineLink>();

			entryLinesBySplitLine = new Dictionary<CusEntryLine, List<CusEntryLine>>();

			PopulateUnusedExistingEntriesEntryLinesAndLinks();
		}

		readonly EntryCreationStrategy strategy;
		readonly BaseJobDeclaration declaration;

		/// <summary>
		/// If an entry cannot be mapped with the merge key any more, it is removed from this dictionary.
		/// </summary>
		readonly Dictionary<MergeKey, CusEntryHeader> entriesByMergeKey;
		readonly Dictionary<CusEntryHeader, MergeKey> mergeKeysByEntry;
		readonly Dictionary<MergeKey, CusEntryLine> entryLinesByMergeKey;
		readonly List<CusEntryHeader> unusedExistingEntries;
		readonly List<CusEntryLine> unusedExistingEntryLines;
		readonly List<AdditionalInvoiceLineEntryLineLink> unusedExistingLinks;
		readonly Dictionary<CusEntryHeader, int> numberOfEntryLinesCreated;
		readonly Dictionary<CusEntryLine, List<CusEntryLine>> entryLinesBySplitLine;

		CusEntryLine SplitEntryLine(CusEntryLine entryLine, BaseJobComInvoiceLine invLine, MergeKey headerMergeKey)
		{
			entryLine.InvoiceLines.Load();
			var mainEntryLine = entryLine;
			if (!entryLinesBySplitLine.ContainsKey(mainEntryLine))
			{
				entryLinesBySplitLine.Add(mainEntryLine, new List<CusEntryLine>());
			}
			var newEntry = GetOrCreateEntryHeader(invLine, headerMergeKey);
			var splitLines = new List<CusEntryLine>();
			entryLinesBySplitLine.TryGetValue(mainEntryLine, out splitLines);

			entryLine = strategy.GetEntryLineFromSplitIfItCanBeReused(invLine, splitLines);

			if (entryLine == null && strategy.CanCreateEntryLine(newEntry, invLine))
			{
				entryLine = newEntry.MergedLines.AddNew();
				entryLinesBySplitLine[mainEntryLine].Add(entryLine);
				UpdateNumberOfLinesCreated(entryLine, invLine);
			}
			return entryLine;
		}

		public CusEntryLine GetOrCreateEntryLine(BaseJobComInvoiceLine invoiceLine)
		{
			var mergeKey = strategy.GetKeyForLine(invoiceLine);
			var headerMergeKey = strategy.GetKeyForHeader(invoiceLine);

			CusEntryLine result = null;

			entryLinesByMergeKey.TryGetValue(mergeKey, out result);//is there an entry line with the merge key?

			if (result == null)
			{
				result = GetExistingEntryLineIfItCanBeReused(invoiceLine, headerMergeKey);
			}
			if (result == null)
			{
				result = GetRecyclableEntryLineFromUnusedEntryLines(invoiceLine, headerMergeKey);
			}
			if (result != null && strategy.EntryLineNeedsToBeSplit(result, invoiceLine))
			{
				result = SplitEntryLine(result, invoiceLine, headerMergeKey);
			}
			if (result == null)
			{
				CusEntryHeader entry = GetOrCreateEntryHeader(invoiceLine, headerMergeKey);
				if (strategy.CanCreateEntryLine(entry, invoiceLine))
				{
					result = entry.MergedLines.AddNew();

					UpdateNumberOfLinesCreated(result, invoiceLine);
				}
			}
			else
			{
				strategy.AfterCreateOrGetEntryHeader(result.Header, invoiceLine);

				if (unusedExistingEntryLines.Contains(result))
				{
					unusedExistingEntryLines.Remove(result);
					UpdateNumberOfLinesCreated(result, invoiceLine);
				}
				unusedExistingEntries.Remove(result.Header);
			}

			if (result != null)
			{
				var linesLink = strategy.LinkInvoiceLineEntryLineAndReturnPivotIfUsed(result, invoiceLine);
				if (linesLink != null)
				{
					unusedExistingLinks.Remove(linesLink);
				}

				strategy.AfterCreateOrGetEntryLine(result, invoiceLine);

				UpdateDictionaries(result, mergeKey, headerMergeKey);
			}
			return result;
		}

		#region Implementation

		internal int GetNumberOfEntryLinesCreated(CusEntryHeader entry)
		{
			int result = 0;

			numberOfEntryLinesCreated.TryGetValue(entry, out result);

			return result;
		}

		void UpdateNumberOfLinesCreated(CusEntryLine entryLine, BaseJobComInvoiceLine invoiceLine)
		{
			if (strategy.ShouldIncrementNumberOfEntryLinesForMaximumEntryLines(invoiceLine))
			{
				CusEntryHeader entry = entryLine.Header;

				int numberOfLinesCreated = GetNumberOfEntryLinesCreated(entry);
				numberOfEntryLinesCreated[entry] = ++numberOfLinesCreated;
			}
		}

		CusEntryLine GetExistingEntryLineIfItCanBeReused(BaseJobComInvoiceLine invoiceLine, MergeKey headerMergeKey)
		{
			CusEntryLine result = strategy.GetExistingEntryLine(invoiceLine);

			if (!CanBeLinkedToExistingEntryLine(result, invoiceLine) ||
				!CanBeLinkedToExistingEntry(result.Header, headerMergeKey, invoiceLine))
			{
				result = null;
			}
			return result;
		}

		CusEntryLine GetRecyclableEntryLineFromUnusedEntryLines(BaseJobComInvoiceLine invoiceLine, MergeKey headerMergeKey)
		{
			if (unusedExistingEntryLines.Count > 0)
			{
				foreach (CusEntryLine entryLine in unusedExistingEntryLines)
				{
					if (CanBeLinkedToExistingEntryLine(entryLine, invoiceLine) &&
						CanBeLinkedToExistingEntry(entryLine.Header, headerMergeKey, invoiceLine))
					{
						return entryLine;
					}
				}
			}
			return null;
		}

		bool CanBeLinkedToExistingEntryLine(CusEntryLine existingEntryLine, BaseJobComInvoiceLine invoiceLine)
		{
			bool result = existingEntryLine != null && !entryLinesByMergeKey.ContainsValue(existingEntryLine);

			//is there a non-amendable change if a customs' amendment is not total replacement for line?
			if (result && !declaration.IsCustomsLineAmendmentATotalReplacement)
			{
				string nonAmendableLineDetails = existingEntryLine.NonAmendableDetails;

				result = string.IsNullOrEmpty(nonAmendableLineDetails) || nonAmendableLineDetails == strategy.GetNonAmendableLineDetails(invoiceLine);
			}

			if (result)
			{
				result = strategy.IsEntryLineValidToBeReused(existingEntryLine, invoiceLine);
			}

			return result;
		}

		/// <summary>
		/// If an existing entry cannot be mapped and will be marked as invalid later, each country should deal with the process of deactivation.
		/// See AU for an example. It adds an error if a lodged entry is being deactivated to a field users can see(CH_BGMReference) until it is withdrawn properly.
		/// </summary>
		bool CanBeLinkedToExistingEntry(CusEntryHeader existingEntry, MergeKey headerMergeKey, BaseJobComInvoiceLine invoiceLine)
		{
			bool result = false;

			if (existingEntry != null && existingEntry.IsActive)
			{
				if (mergeKeysByEntry.ContainsKey(existingEntry))
				{
					MergeKey mergeKeyAlreadyMapped = null;

					mergeKeysByEntry.TryGetValue(existingEntry, out mergeKeyAlreadyMapped);

					result = mergeKeyAlreadyMapped == headerMergeKey; //has existingEntry been identified by this merge key?
				}
				else if (!entriesByMergeKey.ContainsKey(headerMergeKey))
				{
					result = strategy.IsEntryHeaderValidToBeReused(existingEntry, invoiceLine); //this is the first entry with this merge key
				}
			}

			if (result && strategy.CreateANewEntryIfMaximumEntryLineExceeded)
			{
				if (strategy.ShouldIncrementNumberOfEntryLinesForMaximumEntryLines(invoiceLine))
				{
					result = GetNumberOfEntryLinesCreated(existingEntry) < strategy.MaxLinesBeforeNewEntry;
				}
				else
				{
					result = GetNumberOfEntryLinesCreated(existingEntry) <= strategy.MaxLinesBeforeNewEntry;
				}
			}

			return result;
		}

		CusEntryHeader GetOrCreateEntryHeader(BaseJobComInvoiceLine invoiceLine, MergeKey mergeKey)
		{
			CusEntryHeader result = null;

			entriesByMergeKey.TryGetValue(mergeKey, out result);

			if (result == null)
			{
				result = strategy.GetExistingEntryHeader(invoiceLine);
			}

			if (result == null || !CanBeLinkedToExistingEntry(result, mergeKey, invoiceLine))
			{
				//A new entry is to be created possibly with the same merge key. The existing entry with the same merge key cannot be linked any more
				if (result != null)
				{
					entriesByMergeKey.Remove(mergeKey);
				}

				result = declaration.CustomsEntryHeaders.AddNew();
			}

			unusedExistingEntries.Remove(result);
			strategy.AfterCreateOrGetEntryHeader(result, invoiceLine);
			UpdateDictionaries(result, mergeKey);

			return result;
		}

		void PopulateUnusedExistingEntriesEntryLinesAndLinks()
		{
			unusedExistingEntries.AddRange(strategy.GetExistingEntriesCreatedThroughThisStrategy());

			foreach (CusEntryLine existingEntryLine in strategy.GetExistingEntryLinesCreatedThroughThisStrategy())
			{
				unusedExistingEntryLines.Add(existingEntryLine);
				unusedExistingLinks.AddRange(new TypedEnumerable<AdditionalInvoiceLineEntryLineLink>(existingEntryLine.AdditionalInvoiceLineLinks));
			}
		}

		internal void DiscardUnusedEntryAndEntryLines()
		{
			foreach (CusEntryHeader header in unusedExistingEntries)
			{
				if (header.ShouldDeactivateAfterMergeIsDoneAsNoInvoiceLinesLinked)
				{
					//for active entries, this is done in RefreshCollectionsForPostMergeBizObjs()
					header.AllEntryLines.Load();
					header.MergedLines.Rebuild();
					header.PendingDeletionEntryLines.Rebuild();

					header.IsActive = false;
				}
			}

			bool lineNumberBeingLessThanCH_HighestLineNumberMeansTheEntryLineIsLodged =
				!declaration.IsCustomsLineAmendmentATotalReplacement && declaration.ShouldKeepDeletedLinesOnAmendment;

			//This should happen before deleting entry lines
			foreach (AdditionalInvoiceLineEntryLineLink link in unusedExistingLinks)
			{
				if (!link.IsDeleted)
				{
					link.Delete();
				}
			}

			foreach (CusEntryLine line in unusedExistingEntryLines)
			{
				if (!line.IsDeleted)
				{
					line.MarkDeletionPendingOrDelete(lineNumberBeingLessThanCH_HighestLineNumberMeansTheEntryLineIsLodged);
				}
			}
		}

		void UpdateDictionaries(CusEntryHeader entry, MergeKey mergeKey)
		{
			if (!entriesByMergeKey.ContainsKey(mergeKey))
			{
				entriesByMergeKey.Add(mergeKey, entry);
			}

			if (!mergeKeysByEntry.ContainsKey(entry))
			{
				mergeKeysByEntry.Add(entry, mergeKey);
			}
		}

		void UpdateDictionaries(CusEntryLine entryLine, MergeKey lineMergeKey, MergeKey headerMergeKey)
		{
			if (!entryLinesByMergeKey.ContainsKey(lineMergeKey))
			{
				entryLinesByMergeKey.Add(lineMergeKey, entryLine);
			}

			UpdateDictionaries(entryLine.Header, headerMergeKey);
		}

		#endregion
	}
}
