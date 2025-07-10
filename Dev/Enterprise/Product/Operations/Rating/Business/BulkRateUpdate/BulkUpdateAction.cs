using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public abstract class BulkUpdateAction
	{
		protected BulkUpdateAction(BulkRateUpdater updater)
		{
			Updater = updater;
		}

		protected BulkRateUpdater Updater { get; }

		protected RateLine[] GetRateLinesWithActionLineChargeCode(RateEntry entry)
		{
			var chargeCode = Updater.ActionsLine.TL_AC;
			var result = !chargeCode.IsEmpty
				? entry.RateLines.Cast<RateLine>().Where(rateLine => rateLine.TL_AC == chargeCode).ToArray()
				: System.Array.Empty<RateLine>();

			return result;
		}

		protected virtual ZDBOnlySubQuery IncludeInActionSubQuery
		{
			get
			{
				var result = new ZDBOnlySubQuery(typeof(RateLine), RateLinesSchema.TL_TI);
				if (Updater.ActionsLine.ChargeCode != null)
				{
					result.AddToFilter(RateLinesSchema.TL_AC, Updater.ActionsLine.ChargeCode.PK);
				}
				else
				{
					result.IsNoResultQuery = true;
				}

				return result;
			}
		}

		protected virtual bool IsEntryNotUpdatable(RateEntry rateEntry) => rateEntry == null
				|| !rateEntry.IncludeInUpdate
				|| (Updater.CreateNewEntry && rateEntry.IsQuote());

		void AddFetchHints(SimpleRateEntryCollection entryCollection, BusinessObjectFactory saveFactory)
		{
			foreach (RateEntry rateEntry in entryCollection)
			{
				saveFactory.AddFetchHint(RateLinesSchema.TL_TI, rateEntry.PK);
				saveFactory.AddFetchHint(ProcessTasksSchema.P9_ParentID, rateEntry.PK);

				var rateLine = GetRateLinesWithActionLineChargeCode(rateEntry).FirstOrDefault();
				if (rateLine != null)
				{
					saveFactory.AddFetchHint(RateLineItemsSchema.TM_TL, rateLine.PK);
					saveFactory.AddFetchHint(StmNoteSchema.ST_ParentID, rateLine.PK);

					foreach (RateLineItem item in rateLine.RateLineItems)
					{
						saveFactory.AddFetchHint(StmNoteSchema.ST_ParentID, item.PK);
					}
				}
			}
		}

		internal SimpleRateEntryCollection GetRateEntriesToUpdate(BusinessObjectFactory saveFactory)
		{
			var result = new SimpleRateEntryCollection(saveFactory);
			Updater.LoadEntries(result, IncludeInActionSubQuery);

			for (var i = result.Count - 1; i >= 0; i--)
			{
				if (IsEntryNotUpdatable((RateEntry)Updater.Entries.FindByPK(result[i].PK)))
				{
					result.Remove(result[i]);
				}
			}

			return result;
		}

		internal SimpleRateEntryCollection Update(SimpleRateEntryCollection remainingEntriesToUpdate, BusinessObjectFactory saveFactory, int batchSize)
		{
			var updatedEntries = new SimpleRateEntryCollection(saveFactory);

			for (var i = remainingEntriesToUpdate.Count - 1; i >= 0; i--)
			{
				if (updatedEntries.Count < batchSize)
				{
					var entryToUpdate = saveFactory.ImportFromAnotherFactorySafe(remainingEntriesToUpdate[i]);
					if (TryUpdateEntry(updatedEntries, entryToUpdate))
					{
						updatedEntries.Add(entryToUpdate);
					}
					remainingEntriesToUpdate.Remove(entryToUpdate.PK);
				}
				else
				{
					break;
				}
			}

			AddFetchHints(updatedEntries, saveFactory);

			return updatedEntries;
		}

		bool TryUpdateEntry(SimpleRateEntryCollection entryCollection, RateEntry entry)
		{
			if (Updater.CreateNewEntry)
			{
				entry = CreateNewEntry(entryCollection, entry);
				if (entry == null)
				{
					return false;
				}
			}

			ApplyBulkUpdateActionCore(entry);
			entry.Parent.HasChanges = true;

			return true;
		}

		RateEntry CreateNewEntry(SimpleRateEntryCollection entryCollection, RateEntry entry)
		{
			RateEntry result;

			var newStartDate = Updater.NewEntryStartDate;
			var newEndDate = Updater.NewEntryEndDate;
			var entryStartDate = entry.TI_RateStartDate;
			var entryEndDate = entry.TI_RateEndDate;

			if (newStartDate <= entryStartDate && (newEndDate >= entryEndDate || newEndDate.IsEmpty))
			{
				// Old     |==========|
				// New |------------------|
				result = entry;
			}
			else if (!entryEndDate.IsEmpty && entryEndDate < newStartDate)
			{
				// Old |========|
				// New             |---------|
				result = null;
			}
			else if (!newEndDate.IsEmpty && newEndDate < entryStartDate)
			{
				// Old             |========|
				// New |---------|
				result = null;
			}
			else if (newStartDate > entryStartDate && (newEndDate >= entryEndDate || newEndDate.IsEmpty))
			{
				// Old |========|
				// New       |---------|
				entry.TI_RateEndDate = newStartDate.AddDays(-1);
				result = Duplicate(entryCollection, entry, newStartDate, entryEndDate);
				RateLineHelper.UpdateAllRateLinesEndDate(entry.RateLines.Cast<RateLine>(), entry.TI_RateEndDate);
			}
			else if (newStartDate <= entryStartDate && (newEndDate < entryEndDate || entryEndDate.IsEmpty))
			{
				// Old        |========|
				// New |---------|
				entry.TI_RateStartDate = newEndDate.AddDays(1);
				result = Duplicate(entryCollection, entry, entryStartDate, newEndDate);
				RateLineHelper.UpdateAllRateLinesStartDate(entry.RateLines.Cast<RateLine>(), entry.TI_RateStartDate);
			}
			else if (newStartDate > entryStartDate && (newEndDate < entryEndDate || entryEndDate.IsEmpty))
			{
				// Old |==================|
				// New    |-----------|
				entry.TI_RateEndDate = newStartDate.AddDays(-1);
				result = Duplicate(entryCollection, entry, newStartDate, newEndDate);
				Duplicate(entryCollection, entry, newEndDate.AddDays(1), entryEndDate);
				RateLineHelper.UpdateAllRateLinesEndDate(entry.RateLines.Cast<RateLine>(), entry.TI_RateEndDate);
			}
			else
			{
				result = null;
			}

			return result;
		}

		RateEntry Duplicate(SimpleRateEntryCollection entryCollection, RateEntry entry, ZDate startDate, ZDate endDate)
		{
			var result = entry.Clone(entryCollection);

			if (result == null)
			{
				return null;
			}

			using (result.GetValidationSuspender())
			{
				result.TI_RateStartDate = startDate;
				result.TI_RateEndDate = endDate;

				foreach (RateLine line in entry.RateLines)
				{
					var newLine = line.Clone(result.RateLines);
					newLine.RateLineItems.Clone(line);
				}
			}

			result.Validation.ValidateTI_Mode();
			return result;
		}

		protected abstract void ApplyBulkUpdateActionCore(RateEntry entry);
	}
}

