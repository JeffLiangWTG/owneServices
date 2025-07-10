using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Validates Rate Entries for overlap with others in the same header, and for missing Rate Lines.
	/// Avoids loading entries into memory where possible since there can be millions for a Standard Cost Rate (TACT) or global tariff.
	/// Note, local Rate Entries can be published/unpublished which moves them between the local, login company specific rating header
	/// and the global rating header.
	///
	/// 1. Validation for Entries without any lines
	/// This causes a warning, not an error, and the entry can still be saved.
	/// Since this isn't such an important warning, only entries that have been loaded into memory and their RateLines property already
	/// initialized are checked, to save time and avoid DB hits.
	///
	/// 2. Validation for Entries with overlap
	/// Entries in the database can be assumed to not overlap because trigger TG_CheckNoRateEntryOverlaps would have prevented them.
	/// So entries in memory only need to be checked if they have been modified.
	/// It is not practical to make the check by loading every entry into memory.
	/// Instead:
	/// - modified entries in memory are compared against all entries in memory (for the same header)
	/// - any that pass the in-memory check are checked using SQL against entries in the DB.
	/// </summary>
	public class RateEntryCollectionValidator
	{
		public RateEntryCollectionValidator(RatingHeader header)
		{
			this.header = header;
		}

		readonly RatingHeader header;
		public void Validate() => ValidateLocalAndGlobal(header);

		/// <summary>
		/// Validate any changed entries for the given header.
		/// If it is a local header and there is a global header, also validate any newly published global entries.
		/// This method is called before the normal BizO validation so be careful with the values in the BizO as some may not be valid.
		/// </summary>
		/// <returns>entries with errors</returns>
		public static HashSet<RateEntry> ValidateLocalAndGlobal(RatingHeader ratingHeader)
		{
			var entriesWithError = new HashSet<RateEntry>();
			ValidateOneHeader(ratingHeader, entriesWithError);
			ValidateOneHeader(ratingHeader.GlobalRatingHeader, entriesWithError);
			return entriesWithError;
		}

		static void ValidateOneHeader(RatingHeader ratingHeader, HashSet<RateEntry> entriesWithError)
		{
			if (ratingHeader == null)
			{
				return;
			}

			var entriesLoaded = ratingHeader.FetchAllEntriesFromLocalCache();
			ClearRowNotifications(entriesLoaded);

			var (entriesHasErrors, entriesNoError) = SplitLoadedEntriesByHasErrors(entriesLoaded);
			entriesHasErrors?.ForEach(x => entriesWithError.Add(x));

			var entriesLoadedNoErrors = entriesNoError?.ToArray() ?? Array.Empty<RateEntry>();
			CheckEntriesWithRateLinesLoadedHaveRateLines(entriesLoadedNoErrors);
			CheckNoDuplicateEntries(ratingHeader, entriesLoadedNoErrors, entriesWithError);
		}

		static (IEnumerable<RateEntry> HasErrors, IEnumerable<RateEntry> NoError) SplitLoadedEntriesByHasErrors(IEnumerable<RateEntry> entriesLoaded)
		{
			var entryGroups = entriesLoaded.GroupBy(x => x.HasErrorsNotIncludingChildren).ToArray();
			return (entryGroups.FirstOrDefault(x => x.Key), entryGroups.FirstOrDefault(x => !x.Key));
		}

		static void CheckEntriesWithRateLinesLoadedHaveRateLines(IEnumerable<RateEntry> entries)
		{
			foreach (var entry in entries)
			{
				if (entry.IsRateLinesLoaded && entry.RateLines.Count == 0)
				{
					entry.AddRowWarning(ErrorMessages.EntryHasNoLines);
				}
			}
		}

		/// <summary>
		/// Check for duplicates in the given entries and add any to entriesWithError and give them a row error.
		/// Entries must all be for the same header (so can check if all entries are in memory by inspecting the AllEntries property).
		/// Only checks modified entries, i.e., HasChanges is true.
		/// This function assumes that the entries have no existing errors.
		/// </summary>
		/// <param name="entriesWithError">entries with duplicates will be added to this, if it's not null</param>
		/// <returns>true if no duplicates</returns>
		static bool CheckNoDuplicateEntries(RatingHeader ratingHeader, RateEntry[] entries, HashSet<RateEntry> entriesWithError)
		{
			var entriesModified = entries.Where(x => x.HasChanges);
			if (!entriesModified.Any())
			{
				return true;
			}

			// The InMemory check is much slower than the InDb one below due to BusinessObject overhead. For example,
			// for 500k entries it takes 15 seconds, while for InDb one it takes only 3 seconds. For most clients it is not a problem,
			// as having that number of entries under the same header is quite rare case, but for clients like UPS this is the case.
			// In memory duplicacy check is required to check modified rates against each other. The in db check excludes this, and only
			// checks modified rate entries against persistent rate entries that have not been modified
			var entriesNotModified = entries.Where(x => !x.HasChanges);
			var result = CheckNoDuplicatesInMemory(entriesModified, entriesNotModified, entriesWithError);

			// Container Yard has special rules for duplicates, when two rate entries are all the same except for the matching
			// container rate class setting (one is true and the other is false), a duplicate error should be shown. This is because
			// otherwise the two rate entries will both be applicable when the container type is matched.
			if (entriesModified.Any(e => e.IsContainerYard() || e.IsContainerYardTPU()))
			{
				result &= CheckNoDuplicatesInMemoryForCYD(entriesModified, entriesNotModified, entriesWithError);
			}

			// Any modified entries that don't yet have an overlap
			// are checked against entries in the database, if there are any not in memory
			var entriesToCheck = entriesModified.Where(x => !x.HasRowErrors);
			if (entriesToCheck.Any() && !ratingHeader.AllEntriesIsLoaded)
			{
				result &= CheckNoDuplicatesInDB(ratingHeader.Factory, entriesToCheck, entriesWithError);
			}

			return result;
		}

		/// <summary>
		/// Check only given entries for duplicates and adds a row error if found.
		/// Doesn't do any checks against DB.
		/// </summary>
		/// <param name="entriesToCheck">entries to check, will usually just be the modified entries</param>
		/// <param name="otherEntriesToCompareWith">other entries to compare with, usually the unmodified entries already in memory</param>
		/// <param name="entriesWithError">entries with duplicates will be added to this, if it's not null</param>
		/// <returns>true if no duplicates</returns>
		static bool CheckNoDuplicatesInMemory(
			IEnumerable<RateEntry> entriesToCheck,
			IEnumerable<RateEntry> otherEntriesToCompareWith,
			HashSet<RateEntry> entriesWithError)
		{
			foreach (var entry in entriesToCheck)
			{
				// Matching locations needs to be loaded into rate entry when they are still in memory
				entry.RateEntryLocations.PopulateBackToRateEntry();
			}

			return CheckNoDuplicateEntriesCore(entriesToCheck, otherEntriesToCompareWith, entriesWithError, excludingContainerClass: false);
		}

		static bool CheckNoDuplicatesInMemoryForCYD(
			IEnumerable<RateEntry> entriesToCheck,
			IEnumerable<RateEntry> otherEntriesToCompareWith,
			HashSet<RateEntry> entriesWithError)
		{
			return CheckNoDuplicateEntriesCore(
				entriesToCheck.Where(e => e.IsContainerYard() || e.IsContainerYardTPU()),
				otherEntriesToCompareWith.Where(e => e.IsContainerYard() || e.IsContainerYardTPU()),
				entriesWithError,
				excludingContainerClass: true
			);
		}

		static bool CheckNoDuplicateEntriesCore(
			IEnumerable<RateEntry> entriesToCheck,
			IEnumerable<RateEntry> otherEntriesToCompareWith,
			HashSet<RateEntry> entriesWithError,
			bool excludingContainerClass)
		{
			// Build dictionary for modified entries
			var keyToDuplicateEntries = new Dictionary<string, List<RateEntry>>();
			var uniqueEntryPks = new HashSet<Guid>();
			foreach (var entry in entriesToCheck)
			{
				if (uniqueEntryPks.Add(entry.PK.ToGuid()))
				{
					var key = entry.GetKeyForDuplicateSearch(excludingContainerClass);
					if (!keyToDuplicateEntries.TryGetValue(key, out var list))
					{
						list = new List<RateEntry>();
						keyToDuplicateEntries.Add(key, list);
					}
					list.Add(entry);
				}
			}

			// Find duplicates in the other entries
			foreach (var entry in otherEntriesToCompareWith)
			{
				if (uniqueEntryPks.Add(entry.PK.ToGuid()))
				{
					var key = entry.GetKeyForDuplicateSearch(excludingContainerClass);
					if (keyToDuplicateEntries.TryGetValue(key, out var list))
					{
						list.Add(entry);
					}
				}
			}

			bool result = true;
			foreach (var keyEntryListPair in keyToDuplicateEntries)
			{
				var orderedRates = keyEntryListPair.Value;
				if (orderedRates.Count > 1)
				{
					orderedRates.Sort(delegate(RateEntry x, RateEntry y)
					{
						return x.TI_RateStartDate.CompareTo(y.TI_RateStartDate);
					});

					result &= CheckNoOverlaps(orderedRates, entriesWithError);
				}
			}
			return result;
		}

		/// <summary>
		/// Check there are no date overlaps in the given rate entries.
		/// Entries must be ordered by start date and already verified as duplicates in the key fields.
		/// Entries with overlaps are given a row error.
		/// </summary>
		/// <param name="entriesWithError">entries with duplicates will be added to this, if it's not null</param>
		/// <returns>true if there no overlaps, false otherwise</returns>
		static bool CheckNoOverlaps(List<RateEntry> orderedRates, HashSet<RateEntry> entriesWithError)
		{
			bool hasNoOverlaps = true;
			var i = 0;
			var j = 1;

			while (i < orderedRates.Count && j < orderedRates.Count)
			{
				if (!orderedRates[i].TI_RateEndDate.IsEmpty && orderedRates[j].TI_RateStartDate > orderedRates[i].TI_RateEndDate)
				{
					i = i + 1;
					j = i == j ? j + 1 : j;

					continue;
				}

				AddOverlapError(orderedRates[i]);
				AddOverlapError(orderedRates[j]);
				if (entriesWithError != null)
				{
					entriesWithError.Add(orderedRates[i]);
					entriesWithError.Add(orderedRates[j]);
				}
				hasNoOverlaps = false;

				j++;
			}

			return hasNoOverlaps;
		}

		static void AddOverlapError(RateEntry entry)
		{
			if (!entry.HasRowErrors)
			{
				entry.AddRowError(ErrorMessages.OverlappingDatesOnRateEntry);
			}
		}

		/// <summary>
		/// Check for duplicates in DB and load/reload them.
		/// Given entries do not all have to be for the same header.
		/// Duplicate entries are given a row error.
		/// </summary>
		/// <param name="factory">factory used to load any duplicates. If the entry has already been loaded and not modified is is reloaded to ensure it has the latest values.</param>
		/// <param name="entries">entries to check</param>
		/// <param name="entriesWithError">entries with duplicates will be added to this, if it's not null</param>
		/// <returns>true if no duplicates found</returns>
		internal static bool CheckNoDuplicatesInDB(BusinessObjectFactory factory, IEnumerable<RateEntry> entries, HashSet<RateEntry> entriesWithError)
		{
			// Do in batches rather than one at a time.
			// GetOverlappingRateEntries has 49 parameters
			// SQL server has a limit of 2100 parameters.
			// 2100 / 41 = 51
			// Use a bit smaller batches for plenty of safety margin.
			// Future: use table value parameter, but that requires a schema change

			const int batchSize = 32;

			return entries
				.Batch(batchSize)
				.All((batch) => CheckNoDuplicatesInDBForBatch(factory, batch, entriesWithError));
		}

		/// <summary>
		/// Check for overlaps for a single batch of entries with one SQL command
		/// </summary>
		/// <returns>true if no duplicates found</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static bool CheckNoDuplicatesInDBForBatch(BusinessObjectFactory factory, IEnumerable<RateEntry> batch, HashSet<RateEntry> entriesWithError)
		{
			bool result = true;

			/*
			 * Used to map other rate entries with potentially overlapping entries in the current batch. Then later once
			 * each other rate entry is loaded, we determine if it has been modified/deleted, if not, now we add an
			 * overlap error to each rate entry in this batch that overlaps with it.
			 * 
			 * This addresses a case where a rate entry is modified or deleted in the new state, 
			 * but the old state (from the database) could still incorrectly trigger overlap errors.
			 * 
			 * e.g
			 * Old State: (whats in the db)
			 * 
			 * Entry1: day 1 - 10
			 * 
			 * New State:
			 * 
			 * Entry1: deleted
			 * Entry2: day 1 - 10 <- Previously marked as overlapping with old state Entry 1.
			 * 
			 * This avoids this case by checking that Entry1 is not modified/deleted before marking entry2 as overlapping. 
			 */
			var entryPksToLoad = new Dictionary<Guid, List<RateEntry>>();
			using (var cmd = Db.Connection.Command(string.Empty)) // Doing operation directly in SQL for efficiency
			{
				var builder = new StringBuilder(batch.Count() * 250);
				int index = 1;
				foreach (var entry in batch)
				{
					BuildGetOverlapSql(cmd, builder, entry, index);
					++index;
				}
				cmd.CommandText = builder.ToString();

				using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
				{
					foreach (var entry in batch)
					{
						while (reader.Read())
						{
							var guid = reader.GetGuid(0);
							if (!entryPksToLoad.ContainsKey(guid))
							{
								entryPksToLoad[guid] = new List<RateEntry>();
							}
							entryPksToLoad[guid].Add(entry);
						}

						reader.NextResult();
					}
				}
			}

			if (entryPksToLoad.Count > 0)
			{
				// Reload entries that are already loaded and not modified to be sure we have that the latest values that actually cause the overlap.
				var alreadyLoaded = GetEntriesFromLocalCache(factory, entryPksToLoad.Keys);
				foreach (var entry in alreadyLoaded)
				{
					if (!entry.HasChanges)
					{
						if (entry.IsInDatabase)
						{
							entry.Reload();
						}

						if (!entry.IsDeleted)
						{
							foreach (var relatedEntry in entryPksToLoad[entry.PK.ToGuid()])
							{
								AddOverlapError(relatedEntry);
							}
						}

						if (entriesWithError.Add(entry))
						{
							AddOverlapError(entry);
							result = false;
						}
					}
					entryPksToLoad.Remove(entry.PK.ToGuid());
				}

				// Load entries that are not yet loaded.
				if (entryPksToLoad.Count > 0)
				{
					var query = new ZDBOnlyQuery(typeof(RateEntry));
					query.AddToFilter(RateEntrySchema.PK, entryPksToLoad.Keys.ToList());
					var loaded = factory.Load<RateEntry>(query);
					foreach (var entry in loaded)
					{
						if (!entry.IsDeleted && !entry.HasChanges)
						{
							foreach (var relatedEntry in entryPksToLoad[entry.PK.ToGuid()])
							{
								AddOverlapError(relatedEntry);
							}
						}

						AddOverlapError(entry);
						entriesWithError.Add(entry);
						result = false;
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Build the SQL to check for overlaps for a single entry.
		/// </summary>
		/// <param name="cmd">command to add parameters to</param>
		/// <param name="builder">SQL text</param>
		/// <param name="entry">entry</param>
		/// <param name="index">index of entry in a batch. Used as parameter suffix to make them unique to this entry</param>
		static void BuildGetOverlapSql(DbCommand cmd, StringBuilder builder, RateEntry entry, int index)
		{
			var n = index.ToString(CultureInfo.InvariantCulture);
			builder.Append($@"select TI_PK from dbo.GetOverlappingRateEntries(");
			AddParam(builder, cmd, "@PK", n, entry.PK.ToSqlParameter(), RateEntrySchema.PK, false);
			AddParam(builder, cmd, "@RateCategory", n, entry.TI_RateCategory.ToString(), RateEntrySchema.TI_RateCategory);
			AddParam(builder, cmd, "@Mode", n, entry.TI_Mode.ToString(), RateEntrySchema.TI_Mode);
			AddParam(builder, cmd, "@OriginLRC", n, entry.TI_OriginLRC.ToString(), RateEntrySchema.TI_OriginLRC);
			AddParam(builder, cmd, "@DestinationLRC", n, entry.TI_DestinationLRC.ToString(), RateEntrySchema.TI_DestinationLRC);
			AddParam(builder, cmd, "@ViaLRC", n, entry.TI_ViaLRC.ToString(), RateEntrySchema.TI_ViaLRC);
			AddParam(builder, cmd, "@PlannedLoadLRC", n, entry.TI_PlannedLoadLRC.ToString(), RateEntrySchema.TI_PlannedLoadLRC);
			AddParam(builder, cmd, "@PlannedDischargeLRC", n, entry.TI_PlannedDischargeLRC.ToString(), RateEntrySchema.TI_PlannedDischargeLRC);
			AddParam(builder, cmd, "@FirstLoadLRC", n, entry.TI_FirstLoadLRC.ToString(), RateEntrySchema.TI_FirstLoadLRC);
			AddParam(builder, cmd, "@LastDischargeLRC", n, entry.TI_LastDischargeLRC.ToString(), RateEntrySchema.TI_LastDischargeLRC);
			AddParam(builder, cmd, "@FirstRouteSetLoadPortLRC", n, entry.TI_FirstRouteSetLoadPortLRC.ToString(), RateEntrySchema.TI_FirstRouteSetLoadPortLRC);
			AddParam(builder, cmd, "@LastRouteSetDischargePortLRC", n, entry.TI_LastRouteSetDischargePortLRC.ToString(), RateEntrySchema.TI_LastRouteSetDischargePortLRC);
			AddParam(builder, cmd, "@RS_NKServiceLevel_NI", n, entry.TI_RS_NKServiceLevel_NI.ToString(), RateEntrySchema.TI_RS_NKServiceLevel_NI);
			AddParam(builder, cmd, "@PL_NKCarrierServiceLevel", n, entry.TI_PL_NKCarrierServiceLevel.ToString(), RateEntrySchema.TI_PL_NKCarrierServiceLevel);
			AddParam(builder, cmd, "@RS_NKGatewayServiceLevel", n, entry.TI_RS_NKGatewayServiceLevel.ToString(), RateEntrySchema.TI_RS_NKGatewayServiceLevel);
			AddParam(builder, cmd, "@RS_NKShipmentGatewayServiceLevel", n, entry.TI_RS_NKShipmentGatewayServiceLevel.ToString(), RateEntrySchema.TI_RS_NKShipmentGatewayServiceLevel);
			AddParam(builder, cmd, "@RH_NKCommodityCode", n, entry.TI_RH_NKCommodityCode.ToString(), RateEntrySchema.TI_RH_NKCommodityCode);
			AddParam(builder, cmd, "@FMCTariffID", n, entry.TI_FMCTariffID.ToString(), RateEntrySchema.TI_FMCTariffID);
			AddParam(builder, cmd, "@CartagePickupAddressPostCode", n, entry.TI_CartagePickupAddressPostCode.ToString(), RateEntrySchema.TI_CartagePickupAddressPostCode);
			AddParam(builder, cmd, "@CartageDeliveryAddressPostCode", n, entry.TI_CartageDeliveryAddressPostCode.ToString(), RateEntrySchema.TI_CartageDeliveryAddressPostCode);
			AddParam(builder, cmd, "@TransitTime", n, entry.TI_TransitTime.ToString(), RateEntrySchema.TI_TransitTime);
			AddParam(builder, cmd, "@Frequency", n, (int)entry.TI_Frequency, RateEntrySchema.TI_Frequency);
			AddParam(builder, cmd, "@FrequencyUnit", n, entry.TI_FrequencyUnit.ToString(), RateEntrySchema.TI_FrequencyUnit);
			AddParam(builder, cmd, "@IsCrossTrade", n, (bool)entry.TI_IsCrossTrade, RateEntrySchema.TI_IsCrossTrade);
			AddParam(builder, cmd, "@IsTact", n, (bool)entry.TI_IsTact, RateEntrySchema.TI_IsTact);
			AddParam(builder, cmd, "@MatchContainerRateClass", n, (bool)entry.TI_MatchContainerRateClass, RateEntrySchema.TI_MatchContainerRateClass);
			AddParam(builder, cmd, "@TH", n, entry.TI_TH.ToSqlParameter(), RateEntrySchema.TI_TH);
			AddParam(builder, cmd, "@OH_TransportProvider", n, entry.TI_OH_TransportProvider.ToSqlParameter(), RateEntrySchema.TI_OH_TransportProvider);
			AddParam(builder, cmd, "@OH_Supplier", n, entry.TI_OH_Supplier.ToSqlParameter(), RateEntrySchema.TI_OH_Supplier);
			AddParam(builder, cmd, "@OH_Consignor", n, entry.TI_OH_Consignor.ToSqlParameter(), RateEntrySchema.TI_OH_Consignor);
			AddParam(builder, cmd, "@OH_Consignee", n, entry.TI_OH_Consignee.ToSqlParameter(), RateEntrySchema.TI_OH_Consignee);
			AddParam(builder, cmd, "@OH_ControllingCustomer", n, entry.TI_OH_ControllingCustomer.ToSqlParameter(), RateEntrySchema.TI_OH_ControllingCustomer);
			AddParam(builder, cmd, "@OA_CartagePickupAddressOverride", n, entry.TI_OA_CartageDeliveryAddressOverride.ToSqlParameter(), RateEntrySchema.TI_OA_CartageDeliveryAddressOverride);
			AddParam(builder, cmd, "@OA_CartageDeliveryAddressOverride", n, entry.TI_OA_CartageDeliveryAddressOverride.ToSqlParameter(), RateEntrySchema.TI_OA_CartageDeliveryAddressOverride);
			AddParam(builder, cmd, "@RateOrigin", n, entry.TI_RateOrigin.ToString(), RateEntrySchema.TI_RateOrigin);
			AddParam(builder, cmd, "@RateDestination", n, entry.TI_RateDestination.ToString(), RateEntrySchema.TI_RateDestination);
			AddParam(builder, cmd, "@TZ_OriginZone", n, entry.TI_TZ_OriginZone.ToSqlParameter(), RateEntrySchema.TI_TZ_OriginZone);
			AddParam(builder, cmd, "@TZ_DestinationZone", n, entry.TI_TZ_DestinationZone.ToSqlParameter(), RateEntrySchema.TI_TZ_DestinationZone);
			AddParam(builder, cmd, "@R9_FromSuburb", n, entry.TI_R9_FromSuburb.ToSqlParameter(), RateEntrySchema.TI_R9_FromSuburb);
			AddParam(builder, cmd, "@R9_ToSuburb", n, entry.TI_R9_ToSuburb.ToSqlParameter(), RateEntrySchema.TI_R9_ToSuburb);
			AddParam(builder, cmd, "@RC", n, entry.TI_RC.ToSqlParameter(), RateEntrySchema.TI_RC);

			AddParam(builder, cmd, "@TI_RCC_ComponentCode", n, entry.TI_RCC_ComponentCode.ToSqlParameter(), RateEntrySchema.TI_RCC_ComponentCode);
			AddParam(builder, cmd, "@TI_ContainerUnitSection", n, entry.TI_ContainerUnitSection.ToString(), RateEntrySchema.TI_ContainerUnitSection);
			AddParam(builder, cmd, "@TI_RRC_RepairCode", n, entry.TI_RRC_RepairCode.ToSqlParameter(), RateEntrySchema.TI_RRC_RepairCode);
			AddParam(builder, cmd, "@TI_RMC_Material", n, entry.TI_RMC_Material.ToSqlParameter(), RateEntrySchema.TI_RMC_Material);
			AddParam(builder, cmd, "@TI_EstimateType", n, entry.TI_EstimateType.ToString(), RateEntrySchema.TI_EstimateType);
			AddParam(builder, cmd, "@TI_REG_EquipmentGrade", n, entry.TI_REG_EquipmentGrade.ToSqlParameter(), RateEntrySchema.TI_REG_EquipmentGrade);
			AddParam(builder, cmd, "@TI_MNRGroup", n, entry.TI_MNRGroup.ToString(), RateEntrySchema.TI_MNRGroup);

			// TI_RateStartDate is not allowed to be empty but TI_RateEndDate is.
			// We need another check for TI_RateEndDate for this reason.
			// Validations on the fields checked valid entered dates.
			AddParam(builder, cmd, "@RateStartDate", n, entry.TI_RateStartDate.ToDateTime(), RateEntrySchema.TI_RateStartDate);
			AddParam(builder, cmd, "@RateEndDate", n, entry.TI_RateEndDate.IsValid ? entry.TI_RateEndDate.ToDateTime() : DBNull.Value, RateEntrySchema.TI_RateEndDate);

			AddParam(builder, cmd, "@ParentID", n, entry.TI_ParentID.ToSqlParameter(), RateEntrySchema.TI_ParentID);
			AddParam(builder, cmd, "@PaymentTerm", n, entry.TI_PaymentTerm.ToString(), RateEntrySchema.TI_PaymentTerm);
			AddParam(builder, cmd, "@GatewayAgentType", n, entry.TI_PaymentTerm.ToString(), RateEntrySchema.TI_GatewayAgentType);
			AddParam(builder, cmd, "@ContractNumber", n, entry.TI_ContractNumber.ToString(), RateEntrySchema.TI_ContractNumber);
			AddParam(builder, cmd, "@AircraftType", n, entry.TI_AircraftType.ToString(), RateEntrySchema.TI_AircraftType);
			AddParam(builder, cmd, "@TI_ShipmentConsolidationStatus", n, entry.TI_ShipmentConsolidationStatus.ToString(), RateEntrySchema.TI_ShipmentConsolidationStatus);
			AddParam(builder, cmd, "@TI_HBLDeliveryMode", n, entry.TI_HBLDeliveryMode.ToString(), RateEntrySchema.TI_HBLDeliveryMode);
			AddParam(builder, cmd, "@IsNonOperatedReefer", n, entry.TI_IsNonOperatedReefer.ToString(), RateEntrySchema.TI_IsNonOperatedReefer);
			AddParam(builder, cmd, "@TI_YardUnitType", n, entry.TI_YardUnitType.ToString(), RateEntrySchema.TI_YardUnitType);
			AddParam(builder, cmd, "@TI_YardUnitLoad", n, entry.TI_YardUnitLoad.ToString(), RateEntrySchema.TI_YardUnitLoad);
			builder.AppendLine(");");
		}

		static void AddParam(StringBuilder builder, DbCommand cmd, string name, string suffix, object paramValue, ISchemaColumn col, bool addComma = true)
		{
			if (addComma)
			{
				builder.Append(", ");
			}
			var fullName = name + suffix;
			builder.Append(fullName);
			cmd.AddParameterBasedOnDbColumn(fullName, paramValue, col);
		}

		internal static void ClearRowNotifications(IEnumerable<RateEntry> entries)
		{
			foreach (var entry in entries)
			{
				if (!entry.IsValidationSuspended)
				{
					entry.ClearRowNotifications();
				}
			}
		}

		/// <summary>
		/// Get the entries with given PKs from the factory cache.
		/// </summary>
		internal static RateEntry[] GetEntriesFromLocalCache(BusinessObjectFactory factory, IEnumerable<Guid> entryPKs)
		{
			var query = new ZQuery(RateEntrySchema.PK, entryPKs);
			query.FetchOnlyFromLocalCache = true;
			return factory.Load<RateEntry>(query);
		}

		public static string HandleOverlapExceptionForBulkRateUpdater(Exception ex, SimpleRateEntryCollection previewEntries)
			=> RateEntryOverlapSaveExceptionHandler.HandleOverlapExceptionForBulkRateUpdater(ex, previewEntries);

		public static string HandleOverlapException(RatingHeader header, Exception ex)
			=> RateEntryOverlapSaveExceptionHandler.HandleOverlapException(header.Factory, ex);

		/// <summary>
		/// Class to encapsulate logic to handle the exception from the trigger TG_CheckNoRateEntryOverlaps.
		/// Note, unit tests that call the HandleOverlap methods will need to run non-transactioned with snapshot protection
		/// since the handler will try and use the DB connection, so the connection must be in a usable state.
		/// It won't be in a usable state if the test is in an outer transaction, since the inner transaction will have been rolled-back.
		/// Trying to use it will cause an exception from DbCommand.CheckAppTransactionRolledBackInDbServer.
		/// </summary>
		static class RateEntryOverlapSaveExceptionHandler
		{
			/// <summary>
			/// Handle save exception for BulkRateUpdater.
			/// If the exception is from the overlap trigger then this returns an error message, and adds a row error
			/// to those entries in the given collection that were reported by the trigger.
			/// It also looks for further overlaps between the given entries and the trigger reported entries.
			/// Note, BulkRateUpdater does not run RateEntryCollectionValidator before saving.
			/// Also, Bulk Rate Updater saves in batches in separate factories.
			/// The given entries are just the first batch of maximum size 50.
			/// If the trigger fired for a later batch then the PKs from the trigger won't be found and no entry will get a row error.
			/// </summary>
			/// <param name="previewEntries">first batch of entries being updated and shown in the updater preview</param>
			/// <returns>error string if the exception was from the overlap trigger, null otherwise</returns>
			public static string HandleOverlapExceptionForBulkRateUpdater(Exception ex, SimpleRateEntryCollection previewEntries)
			{
				var duplicateEntryPKs = GetDuplicateEntryPKsOrHandleException(ex);
				if (duplicateEntryPKs == null)
				{
					return null;
				}

				var rateEntries = previewEntries.Cast<RateEntry>();

				foreach (var insertedRateEntry in rateEntries.Where(r => duplicateEntryPKs.Contains(r.PK.ToGuid())))
				{
					AddOverlapError(insertedRateEntry);
					var duplicateEntries = rateEntries
						.Where(r => r.PK != insertedRateEntry.PK && HasOverlappingDateRanges(r, insertedRateEntry) && r.IsDuplicate(insertedRateEntry))
						.ToArray();

					foreach (var duplicateEntry in duplicateEntries)
					{
						AddOverlapError(duplicateEntry);
					}
				}

				return ErrorMessages.OverlappingRatesCreatedByBulkRateUpdater;
			}

			static bool HasOverlappingDateRanges(RateEntry r1, RateEntry r2)
			{
				return (r1.TI_RateEndDate.IsEmpty || r2.TI_RateStartDate <= r1.TI_RateEndDate)
					&& (r2.TI_RateEndDate.IsEmpty || r1.TI_RateStartDate <= r2.TI_RateEndDate);
			}

			/// <summary>
			/// Handles save exception for entries for a single RatingHeader (and the linked Global header if any).
			/// If it's not the overlap exception, uses the default exception handling.
			/// For an overlap exception, adds row errors to overlapping entries.
			/// If validation was already run, any overlaps got missed by the validation,
			/// which can happen if another login changed the rates concurrently.
			/// Generates an issue report if the cause was not concurrency.
			/// </summary>
			/// <returns>the error message for the overlap, or null if this wasn't an overlap</returns>
			public static string HandleOverlapException(BusinessObjectFactory factory, Exception ex)
			{
				var duplicateEntryPKs = GetDuplicateEntryPKsOrHandleException(ex);
				if (duplicateEntryPKs == null)
				{
					return null;
				}

				var duplicateEntries = RateEntryCollectionValidator.GetEntriesFromLocalCache(factory, duplicateEntryPKs);
				foreach (var entry in duplicateEntries)
				{
					AddOverlapError(entry);
				}

				// We know the duplicate entries have overlaps according to the trigger.
				// The only known reason that validation didn't prevent the save in the first place is that another user changed the rates.
				// So find the overlap rates in the DB, reload them and add a row error.
				var entriesWithError = new HashSet<RateEntry>();
				RateEntryCollectionValidator.CheckNoDuplicatesInDB(factory, duplicateEntries, entriesWithError);

				// Redo the validation overlap check. If it finds nothing then that's strange so send an issue.
				var exceptionIsDueToConcurrency = !RateEntryCollectionValidator.CheckNoDuplicatesInMemory(duplicateEntries, entriesWithError, null);

				return exceptionIsDueToConcurrency
					? ErrorMessages.OverlappingRatesCreatedByConcurrency
					: ErrorMessages.OverlappingRatesButPassesInDatabase;
			}

			/// <summary>
			/// Returns duplicate entry PKs if the exception is from the overlap trigger and PKs could be parsed from it.
			/// Otherwise performs default exception handling.
			/// </summary>
			static List<Guid> GetDuplicateEntryPKsOrHandleException(Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw ex;
				}

				var sqlException = ex.Find<SqlException>();
				List<Guid> duplicateEntryPKs;
				if (sqlException == null ||
					sqlException.Number != RatesOverlapErrorNumber ||
					(duplicateEntryPKs = ParseDuplicateRateEntryPKs(sqlException.Message)).Count == 0)
				{
					ZExceptionReporting.HandleSaveException(ex);
					return null;
				}

				return duplicateEntryPKs;
			}

			/// <summary>
			/// Parse the message from the trigger TG_CheckNoRateEntryOverlaps.
			/// It contains the top 55 PKs of failed records as a comma separated string.
			/// </summary>
			/// <param name="exceptionMessage">The message thrown by the database when the trigger fails</param>
			static List<Guid> ParseDuplicateRateEntryPKs(string exceptionMessage)
			{
				var list = new List<Guid>();

				if (!string.IsNullOrEmpty(exceptionMessage))
				{
					var guidMatchingRegex = new Regex(@"[\da-f]{8}\-[\da-f]{4}\-[\da-f]{4}\-[\da-f]{4}\-[\da-f]{12}", RegexOptions.IgnoreCase);
					foreach (Match match in guidMatchingRegex.Matches(exceptionMessage))
					{
						if (ZGuid.TryParse(match.Value, out ZGuid result))
						{
							list.Add(result.ToGuid());
						}
					}
				}

				return list;
			}

			/// <summary>
			///	A custom error code for rates overlap error.
			///	The number should be in sync with the one thrown in TG_CheckNoRateEntryOverlaps.sql
			/// </summary>
			const int RatesOverlapErrorNumber = 58008;
		}
	}
}
