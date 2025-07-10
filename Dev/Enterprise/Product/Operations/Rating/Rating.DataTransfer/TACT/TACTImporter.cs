namespace Enterprise.Rating.DataTransfer.TACT
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using CargoWise.Common;
	using CargoWise.ComponentModel;
	using CargoWise.Data;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.DataTransfer.Business;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Rating.Business;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Schema;
	using static Enterprise.Core.Constants;
	using Res = Enterprise.Rating.DataTransfer.Res;
	using SqlBulkCopyOptions = CargoWise.Data.Providers.Common.SqlBulkCopyOptions;

	public class TACTImporter
	{
		const int RowsSaveBatchSize = 20 * 1000;

		// 2 threads takes about a third less time than 1 thread.
		// More threads make no measurable difference.
		const int ThreadsCount = 2;

		public int RowsDeleteBatchSize = 10 * 1000;

		const int timeoutOffset = 1200;

		public TACTImporter(Stream stream, FixedWidthFlatFileFormat format, INotifications notifications, TACTImportOptions importOptions)
		{
			Argument.NotNull(stream, "stream");
			Argument.NotNull(format, "format");
			Argument.NotNull(notifications, "notifications");
			Argument.NotNull(importOptions, "importOptions");

			options = importOptions;
			isGlobal = importOptions.CompanyPK == Guid.Empty;
			options.CompanyPK = isGlobal ? Env.CurrentCompanyPK : importOptions.CompanyPK;
			this.notifications = notifications;
			this.reader = new TACTReader(stream, format, notifications);
		}

		public event EventHandler<System.ComponentModel.ProgressChangedEventArgs> ProgressChanged = (sender, e) => { };

		public async Task<TACTImportResult> ImportAsync(ExistingRatesHandlingStrategy cleanupStrategy)
		{
			var result = new TACTImportResult();

			var clearTACTRates = cleanupStrategy.HasFlag(ExistingRatesHandlingStrategy.ClearTACTRates);
			var clearStandardRates = cleanupStrategy.HasFlag(ExistingRatesHandlingStrategy.ClearStandardRates);
			var deleteDuplicates = !cleanupStrategy.HasFlag(ExistingRatesHandlingStrategy.ClearStandardRates)
				&& !cleanupStrategy.HasFlag(ExistingRatesHandlingStrategy.ClearTACTRates);

			if (clearTACTRates || clearStandardRates)
			{
				UpdateProgress(0, Res.GetString("4e768807-977c-4231-8989-fde2b713d181", "Clearing existing rates ..."));

				var worker = Task.Run(() => RemoveExistingRates(clearTACTRates, clearStandardRates));
				await Task.WhenAll(worker).ConfigureAwait(false);
			}

			UpdateProgress(0, Res.GetString("1b9238b7-4fd3-11e7-bd37-fcaa14295823", "Starting the import..."));

			var workers = Enumerable
			.Range(0, ThreadsCount)
			.Select(i => Task.Run(() => ImportWorker(result, deleteDuplicates)));
			await Task.WhenAll(workers).ConfigureAwait(false);

			return result;
		}

		void ImportWorker(TACTImportResult result, bool deleteDuplicates)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var rowFactory = new RowFactory();
				var factory = new BusinessObjectFactory();

				while (true)
				{
					try
					{
						// PARSE
						var rows = reader.Read(result);
						if (!rows.Any())
						{
							// Finished
							break;
						}

						// CONVERT
						var rates = rows
							.GroupBy(r => r.Key)
							.Select(row => Convert(row.ToArray().ToArray(), factory, rowFactory, result))
							.WhereNotNull()
							.ToArray();

						// SAVE
						Save(rates, factory, deleteDuplicates);

						// UPDATE RESULT
						result.AddConvertedRates(rates.Sum(r => r.TACTRowsCount));
						result.SetTotalRatesCount(reader.TotalRates);

						UpdateProgress(result);
					}
					catch (SqlException ex)
					{
						// TODO: Maybe we want to try to save rates one by one instead of rejecting the whole batch
						notifications.AddError(Res.GetString("4df87c62-4ff0-11e7-b473-fcaa14295823", "An error occurred while saving processed TACT rates: {0}", ex.Message));
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						notifications.AddError(Res.GetString("65ce8ba6-4ff0-11e7-b08a-fcaa14295823", "An unexpected error occurred while processing TACT rates: {0}", ex.Message));
						throw;
					}
				}
			}
		}

		void Save(TACTRate[] rates, BusinessObjectFactory factory, bool deleteDuplicates)
		{
			var lineItems = rates.SelectMany(r => r.RateLineItems).ToArray();
			var entries = rates.Select(r => r.RateEntry).ToArray();
			var lines = rates.Select(r => r.RateLine).ToArray();

			var ratesSaver = new SaveInTransactionDelegateAction(factory, () =>
			{
				if (deleteDuplicates)
				{
					DeleteDuplicatesAndSaveRows(entries, factory);
				}
				else
				{
					SaveRows(entries, factory);
				}
				SaveRows(lines, factory);
				SaveRows(lineItems, factory);

				var tables = new[]
				{
					AutoRateLineItems.Schema.TableName,
					AutoRateEntry.Schema.TableName,
					AutoRateLines.Schema.TableName,
				};

				return new ChangedTableNames(tables.ToList());
			});

			var header = factory.Load<RatingHeader>(options.RatingHeaderPK);
			// Note, adding to Logs causes header.HasChanges to be true, which creates the auto log unless suppressed
			header.CreateAutoLogIfOnlyChildrenChanged = false;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			header.Logs.AddNew(AutoEvents.EditedARecord, ZString.Format("{0} TACT Rate Entries were added", entries.Length));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			// It is a hack which ensures that workers don't save rates in parallel. The FK_RC__TL_TI clustered index on RateLines causes
			// table locks which in their turn cause  deadlocks if we initiate multiple transactions around RateEntry, RateLines, RateLineItems
			// tables when we update these tables from multiple workers. Don't see the fast way how to fix it, so lets postpone it - it is not
			// as fast as it could be (about 2x times faster) but still should be enough.
			lock (saveSyncObject)
			{
				BusinessObjectFactory.SaveTogether(ratesSaver, factory);
			}
		}

		void SaveRows(DataRow[] rows, BusinessObjectFactory factory, string destinationTableNameOverride = null)
		{
			if (!rows.Any())
			{
				return;
			}

			var internalConnection = (IDbConnectionInternals)((IDbConnected)factory).Connection;

			using (var bulkCopy = ((IDbConnected)factory).Connection.GetSqlBulkCopy(SqlBulkCopyOptions.FireTriggers | SqlBulkCopyOptions.CheckConstraints, internalConnection.ADOTransaction))
			{
				bulkCopy.BulkCopyTimeout = 0;
				bulkCopy.BatchSize = RowsSaveBatchSize;
				bulkCopy.DestinationTableName = destinationTableNameOverride ?? rows[0].Table.TableName;

				foreach (DataColumn column in rows[0].Table.Columns)
				{
					bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
				}

				bulkCopy.WriteToServer(rows);
			}
		}

		TACTRate Convert(TACTDataDataRow[] lines, BusinessObjectFactory factory, RowFactory rowFactory, TACTImportResult result)
		{
			var expiredLines = lines.Where(l => !l.EndDate.IsEmpty && l.EndDate < ZDateTime.Today).ToArray();
			result.AddSkippedRates(expiredLines.Length, TACTImportResult.SkipReason.Expired);
			lines = lines.Except(expiredLines).ToArray();

			if (!lines.Any())
			{
				return null;
			}

			var entry = GetRateEntry(lines, factory, rowFactory, result);
			if (entry == null)
			{
				return null;
			}

			var line = GetRateLine(lines, (Guid)entry[AutoRateEntry.Schema.PK], rowFactory);
			if (line == null)
			{
				return null;
			}

			var lineItems = GetRateLineItems(lines, (Guid)line[AutoRateLines.Schema.PK], rowFactory);
			if (lineItems == null)
			{
				return null;
			}

			return new TACTRate
			{
				RateEntry = entry,
				RateLine = line,
				RateLineItems = lineItems,
				TACTRowsCount = lines.Length
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		DataRow GetRateEntry(TACTDataDataRow[] tactLines, BusinessObjectFactory factory, RowFactory rowFactory, TACTImportResult result)
		{
			OrgHeader carrier = null;
			TACTDataDataRow tactLine = tactLines[0];

			if (!tactLine.IsGeneralRate)
			{
				result.AddSkippedRates(tactLines.Length, TACTImportResult.SkipReason.UnknownCategory, tactLine.Category);
				return null;
			}

			if (!string.IsNullOrEmpty(tactLine.CarrierCode))
			{
				carrier = GetCarrier(tactLine.CarrierCode, factory);

				if (carrier == null)
				{
					result.AddSkippedRates(tactLines.Length, TACTImportResult.SkipReason.UnknownCarrier, tactLine.CarrierCode);
					return null;
				}
			}

			var rateEntry = rowFactory.New(AutoRateEntry.Schema.TableName);
			rateEntry[AutoRateEntry.Schema.PK] = Guid.NewGuid();
			rateEntry[AutoRateEntry.Schema.TI_TH] = options.RatingHeaderPK;
			rateEntry[AutoRateEntry.Schema.TI_RateCategory] = RatingConstants.RateCategory.AIR;
			rateEntry[AutoRateEntry.Schema.TI_Mode] = Core.Constants.RateMode.LSE;
			rateEntry[AutoRateEntry.Schema.TI_RX_NKCurrency] = tactLine.Currency.ToString();
			rateEntry[AutoRateEntry.Schema.TI_DestinationLRC] = GetLocationCode(tactLine.DestinationCountryCode, tactLine.DestinationCityCode, factory);
			rateEntry[AutoRateEntry.Schema.TI_OriginLRC] = GetLocationCode(tactLine.OriginCountryCode, tactLine.OriginCityCode, factory);
			rateEntry[AutoRateEntry.Schema.TI_GC_Publisher] = options.CompanyPK;
			rateEntry[AutoRateEntry.Schema.TI_IsTact] = true;
			rateEntry[AutoRateEntry.Schema.TI_IsExcludedFromAutoRating] = options.ShouldExcludeFromAutoRating;
			rateEntry[AutoRateEntry.Schema.TI_SystemCreateTimeUtc] = DateTime.UtcNow;
			rateEntry[AutoRateEntry.Schema.TI_SystemCreateUser] = GlbStaff.CurrentUser.GS_Code;
			rateEntry[AutoRateEntry.Schema.TI_SystemLastEditTimeUtc] = DateTime.UtcNow;
			rateEntry[AutoRateEntry.Schema.TI_SystemLastEditUser] = GlbStaff.CurrentUser.GS_Code;
			rateEntry[AutoRateEntry.Schema.TI_CreationSource] = RateEntryCreator.Sources.FromTACT;

			var startDate = tactLines.Max(l => l.StartDate);
			if (startDate.IsValid)
			{
				rateEntry[AutoRateEntry.Schema.TI_RateStartDate] = startDate.ToDateTime();
			}

			var endDate = tactLines.Min(l => l.EndDate);
			if (endDate.IsValid)
			{
				rateEntry[AutoRateEntry.Schema.TI_RateEndDate] = endDate.ToDateTime();
			}

			if (carrier != null)
			{
				rateEntry[AutoRateEntry.Schema.TI_OH_TransportProvider] = carrier.PK.ToGuid();

				if (!tactLine.UniqueNote.IsEmpty)
				{
					var serviceLevel = GetCarrierServiceLevel(carrier, tactLine.UniqueNote);
					if (string.IsNullOrEmpty(serviceLevel))
					{
						result.AddSkippedRates(tactLines.Length, TACTImportResult.SkipReason.UnknownCarrierServiceLevel, ZString.Format("{0} - {1}", tactLine.CarrierCode, tactLine.UniqueNote));
						return null;
					}

					rateEntry[AutoRateEntry.Schema.TI_PL_NKCarrierServiceLevel] = serviceLevel;
				}
			}

			return rateEntry;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		DataRow GetRateLine(TACTDataDataRow[] tactLines, Guid entryPK, RowFactory rowFactory)
		{
			var tactLine = tactLines[0];

			var rateLine = rowFactory.New(AutoRateLines.Schema.TableName);
			rateLine[AutoRateLines.Schema.PK] = Guid.NewGuid();
			rateLine[AutoRateLines.Schema.TL_TI] = entryPK;
			rateLine[AutoRateLines.Schema.TL_AC] = GetChargeCode();
			rateLine[AutoRateLines.Schema.TL_WeightVolume] = GetUnits(tactLine.WeightBreakUnit, tactLine.Category);
			rateLine[AutoRateLines.Schema.TL_RateCalculator] = GetRateCalculatorCode(tactLines);
			rateLine[AutoRateLines.Schema.TL_RX_NKCurrency] = tactLine.Currency.ToString();
			rateLine[AutoRateLines.Schema.TL_IsWhsJobLevelCharge] = options.IsJobLevelCharge;
			rateLine[AutoRateLines.Schema.TL_Rounding] = string.IsNullOrWhiteSpace(options.Rounding) ? RatingRoundingTypes.NoRounding : options.Rounding;
			rateLine[AutoRateLines.Schema.TL_SystemCreateTimeUtc] = DateTime.UtcNow;
			rateLine[AutoRateLines.Schema.TL_SystemCreateUser] = GlbStaff.CurrentUser.GS_Code;
			rateLine[AutoRateLines.Schema.TL_SystemLastEditTimeUtc] = DateTime.UtcNow;
			rateLine[AutoRateLines.Schema.TL_SystemLastEditUser] = GlbStaff.CurrentUser.GS_Code;

			var unit = rateLine[AutoRateLines.Schema.TL_WeightVolume];
			var conversionFactor = GetConversionFactor((string)unit);
			if (!conversionFactor.IsEmpty)
			{
				rateLine[AutoRateLines.Schema.TL_ConversionFactor] = conversionFactor.Factor;
				rateLine[AutoRateLines.Schema.TL_FactorNumerator] = conversionFactor.NumeratorUnit;
				rateLine[AutoRateLines.Schema.TL_FactorDenominator] = conversionFactor.DenominatorUnit;
			}

			return rateLine;
		}

		IEnumerable<DataRow> GetRateLineItems(TACTDataDataRow[] tactLines, Guid rateLinePK, RowFactory rowFactory)
		{
			var lineItems = new List<DataRow>();

			foreach (var tactLine in tactLines)
			{
				var lineItem = GetRateLineItem(tactLine, rateLinePK, rowFactory);
				if (lineItem == null)
				{
					return null;
				}

				lineItems.Add(lineItem);
			}

			// Fix TACT combined calculator
			var breakLines = lineItems.Where(l => l[AutoRateLineItems.Schema.TM_Type].ToString() == Calculator.Items.Operator.Plus).ToArray();

			// Convert TACT break structure:
			// +1	$10
			//
			// to CW1 per unit calculation:
			// UNT	$10
			//
			if (breakLines.Length == 1 && (decimal)breakLines[0][AutoRateLineItems.Schema.TM_Break] == 1)
			{
				breakLines[0][AutoRateLineItems.Schema.TM_Type] = Calculator.Items.Operator.UNT;
				breakLines[0][AutoRateLineItems.Schema.TM_Break] = 0;
			}

			// Convert TACT break structure:
			// +1	$10
			// +10	$20
			// +20	$30
			//
			// to CW1 break structure:
			// -10	$10
			// +10	$20
			// +20	$30
			//
			if (breakLines.Length >= 2)
			{
				breakLines[0][AutoRateLineItems.Schema.TM_Type] = Calculator.Items.Operator.Minus;
				breakLines[0][AutoRateLineItems.Schema.TM_Break] = breakLines[1][AutoRateLineItems.Schema.TM_Break];
			}

			return lineItems;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		DataRow GetRateLineItem(TACTDataDataRow tactLine, Guid rateLinePK, RowFactory rowFactory)
		{
			var lineItem = rowFactory.New(AutoRateLineItems.Schema.TableName);
			lineItem[AutoRateLineItems.Schema.PK] = Guid.NewGuid();
			lineItem[AutoRateLineItems.Schema.TM_TL] = rateLinePK;
			lineItem[AutoRateLineItems.Schema.TM_SystemCreateTimeUtc] = DateTime.UtcNow;
			lineItem[AutoRateLineItems.Schema.TM_SystemCreateUser] = GlbStaff.CurrentUser.GS_Code;
			lineItem[AutoRateLineItems.Schema.TM_SystemLastEditTimeUtc] = DateTime.UtcNow;
			lineItem[AutoRateLineItems.Schema.TM_SystemLastEditUser] = GlbStaff.CurrentUser.GS_Code;

			if (tactLine.Category == TACTDataDataRow.Constants.Category.SpecifiedMinimumCharge)
			{
				lineItem[AutoRateLineItems.Schema.TM_Type] = Calculator.Items.Operator.MIN;
				lineItem[AutoRateLineItems.Schema.TM_Value] = (decimal)tactLine.Rate;
			}
			else if (tactLine.Category == TACTDataDataRow.Constants.Category.SpecifiedBasicCharge)
			{
				lineItem[AutoRateLineItems.Schema.TM_Type] = Calculator.Items.Operator.BAS;
				lineItem[AutoRateLineItems.Schema.TM_Value] = (decimal)tactLine.Rate;
			}
			else if (TACTDataDataRow.RatesPerKilogram.Contains(tactLine.Category))
			{
				lineItem[AutoRateLineItems.Schema.TM_Type] = Calculator.Items.Operator.UNT;
				lineItem[AutoRateLineItems.Schema.TM_Value] = (decimal)tactLine.Rate;
			}
			else if (TACTDataDataRow.CargoRates.Contains(tactLine.Category))
			{
				lineItem[AutoRateLineItems.Schema.TM_Type] = Calculator.Items.Operator.Plus;
				lineItem[AutoRateLineItems.Schema.TM_Break] = (decimal)tactLine.WeightBreak;
				lineItem[AutoRateLineItems.Schema.TM_Value] = (decimal)tactLine.Rate;
			}

			return lineItem;
		}

		#region Parsing Location

		static string GetLocationCode(ZString countryCode, ZString cityCode, BusinessObjectFactory factory)
		{
			var unloco = factory.GetCachedValue("UNLOCO_" + countryCode + cityCode, () => // Just a key for cache
			{
				var iataCityCode = IATACityCode.GetValidOrDefault(factory, cityCode);
				if (iataCityCode != null)
				{
					return iataCityCode.Code.ToString();
				}

				if (!cityCode.IsEmpty)
				{
					var port = RefUNLOCO.LoadFromIATA(factory, cityCode);
					if (port != null)
					{
						return port.RL_Code.ToString();
					}
				}

				return FormattableString.Invariant($"{countryCode}{cityCode}"); // Unknown location code
			});

			return unloco;
		}

		#endregion

		#region Parsing Carrier

		static OrgHeader GetCarrier(string iataCode, BusinessObjectFactory factory)
		{
			var carrier = factory.GetCachedValue("Carrier_" + iataCode, () =>   // Just a key for cache
			{
				RefAirline airline = null;

				var airlineFilter = new ZQuery(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, SQLComparisonOperator.NotEqual, ZString.Empty);
				if (iataCode.Length == 2)
				{
					airlineFilter.AddToFilter(RefAirlineSchema.RM_TwoCharacterCode, iataCode);
					airline = factory.LoadTop1<RefAirline>(airlineFilter);
				}
				else if (iataCode.Length == 3)
				{
					airlineFilter.AddToFilter(RefAirlineSchema.RM_ThreeLetterCode, iataCode);
					airline = factory.LoadTop1<RefAirline>(airlineFilter);
				}

				if (airline != null)
				{
					var carrierMiscServ = factory.LoadTop1<OrgMiscServ>(new ZQuery(OrgMiscServSchema.OM_RM_Airline, airline.PK));
					if (carrierMiscServ != null)
					{
						return carrierMiscServ.Header;
					}
				}

				return null;
			});

			return carrier;
		}

		#endregion

		#region Parsing Carrier Service Level

		string GetCarrierServiceLevel(OrgHeader carrier, string tactServiceLevel)
		{
			// We use a Dictionary with a lock instead of a ConcurrentDictionary without a lock for better thread safety.
			// ConcurrentDictionary's GetOrAdd method is not atomic for the value factory, which can cause issues
			// when creating unique values that are saved to the database.
			//
			// Our scenario:
			// 1. We need to create a new, unique value and save it in the database.
			// 2. There's a database constraint ensuring the value's uniqueness.
			//
			// Problem with ConcurrentDictionary:
			// If multiple threads call the factory method simultaneously, the same value might be created
			// multiple times, potentially triggering a constraint violation in the database.
			//
			// Solution:
			// Using a Dictionary with a lock ensures thread-safety for the entire operation:
			// checking if the value exists, creating it if needed, and adding it to the dictionary.
			// This approach prevents race conditions and maintains data integrity.
			lock (carrierServiceLevelMappers)
			{
				var mapper = carrierServiceLevelMappers.GetOrAdd(carrier.OH_Code, () => new CarrierServiceLevelsMapper(carrier));

				var (serviceLevel, error) = mapper.GetMappedServiceLevel(carrier, tactServiceLevel);
				if (!string.IsNullOrEmpty(error))
				{
					notifications.AddWarning(error);
					return null;
				}

				return serviceLevel;
			}
		}

		readonly IDictionary<string, CarrierServiceLevelsMapper> carrierServiceLevelMappers = new Dictionary<string, CarrierServiceLevelsMapper>();

		#endregion

		#region Parsing Calculator

		static string GetRateCalculatorCode(TACTDataDataRow[] tactLines)
		{
			var breakAmountCharges = tactLines.Where(l => TACTDataDataRow.CargoRates.Contains(l.Category)).ToArray();
			var hasPerUnitCharge = tactLines.Any(l => TACTDataDataRow.RatesPerKilogram.Contains(l.Category));
			var hasMinimumCharge = tactLines.Any(l => l.Category == TACTDataDataRow.Constants.Category.SpecifiedMinimumCharge);
			var hasBasicCharge = tactLines.Any(l => l.Category == TACTDataDataRow.Constants.Category.SpecifiedBasicCharge);

			if (breakAmountCharges.Any())
			{
				if (breakAmountCharges.Length == 1 && breakAmountCharges[0].WeightBreak == 1)
				{
					return hasMinimumCharge ? MinimumOrPerUnitCalculator.Code : UnitCalculator.Code;
				}
				else
				{
					return CombinedCalculator.Code;
				}
			}
			else if (hasMinimumCharge && hasBasicCharge && hasPerUnitCharge)
			{
				return CombinedCalculator.Code;
			}
			else if (hasBasicCharge && hasPerUnitCharge)
			{
				return FlatPlusPerUnitCalculator.Code;
			}
			else if (hasMinimumCharge)
			{
				return MinimumOrPerUnitCalculator.Code;
			}
			else if (hasPerUnitCharge)
			{
				return UnitCalculator.Code;
			}
			else
			{
				return FlatCalculator.Code;
			}
		}

		#endregion

		#region Parsing Units

		static string GetUnits(string tactWeightBreakUnit, string tactCategory)
		{
			var units = tactWeightBreakUnit;

			if (string.IsNullOrEmpty(units) && TACTDataDataRow.RatesPerKilogram.Contains(tactCategory))
			{
				units = Weight.Kilograms;
			}

			return units;
		}

		#endregion

		#region Parsing Conversion Factor

		static ConversionFactor GetConversionFactor(string weightVolumeUnit)
		{
			if (string.IsNullOrEmpty(weightVolumeUnit))
			{
				return ConversionFactor.Empty;
			}
			else if (Weight.IsImperial(weightVolumeUnit))
			{
				return ConversionFactor.Standard.Imperial.Air;
			}
			else
			{
				return ConversionFactor.Standard.Metric.Air;
			}
		}

		#endregion

		#region Parsing Charge Code

		Guid? GetChargeCode()
		{
			lock (chargeCodeSyncObject)
			{
				if (!chargeCodePK.HasValue)
				{
					var factory = new BusinessObjectFactory();

					var chargeCode = factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
					if (chargeCode == null)
					{
						var query = new ZQuery(AccChargeCodeSchema.AC_Code, "FRT");     // A database value
						query.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

						chargeCode = factory.LoadTop1<AccChargeCode>(query);
					}

					chargeCodePK = chargeCode != null
						? isGlobal && chargeCode.GlobalChargeCode != null
							? chargeCode.GlobalChargeCode.PK.ToGuid()
							: chargeCode.PK.ToGuid()
						: Guid.Empty;
				}

				return chargeCodePK;
			}
		}

		Guid? chargeCodePK;
		readonly object chargeCodeSyncObject = new object();

		#endregion

		#region Clearing Rates

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void RemoveExistingRates(bool clearTACTRates, bool clearStandardRates)
		{
			try
			{
				using (Db.DisposableActionForDbConnection())
				{
					var connection = Db.Connection;
					var totalRowsToTruncate = GetExistingRateBacklog(options.RatingHeaderPK, clearTACTRates, clearStandardRates, connection);
					int backlog = totalRowsToTruncate;

					while (true)
					{
						if (backlog <= 0)
						{
							break;
						}
						UpdateProgress(GetProgressPercentage(totalRowsToTruncate, (totalRowsToTruncate - backlog)), Res.GetString("e86fa94f-8210-49a6-8b46-b04b252b37d4", "Cleared {0} TACT rates of {1} total", (totalRowsToTruncate - backlog), totalRowsToTruncate));

						var sqlText = string.Format(CultureInfo.InvariantCulture, @"EXEC DeleteExistingRates '{0}', '{1}', '{2}', '{3}'", options.RatingHeaderPK, clearTACTRates ? "Y" : "N", clearStandardRates ? "Y" : "N", RowsDeleteBatchSize); // SQL stored procedure call
						connection.Command(sqlText, timeoutOffset).ExecuteNonQuery(); // Significant perfomance problem if this is done in the standard way
						backlog = backlog - RowsDeleteBatchSize;
					}

					UpdateProgress(100, Res.GetString("7f966cf5-cc60-4b3f-8ee9-b534cbfaef46", "Cleared All Existing rates !!!"));
				}
			}
			catch (SqlException ex)
			{
				if (ex.Number == -2)
				{
					throw new TimeoutException("An error occurred while clearing existing TACT rates: SQL Execution Timeout Expired, Try running the process again." + System.Environment.NewLine + "Stack trace : ");
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL text, not translatable")]
		void DeleteDuplicatesAndSaveRows(DataRow[] rateEntries, BusinessObjectFactory factory)
		{
			if (!rateEntries.Any())
			{
				return;
			}

			var rateEntryTable = rateEntries[0].Table;
			var columnNamesSql = GetRateEntryColumnNamesSql(rateEntryTable);
			var connection = ((IDbConnected)factory).Connection;
			// Note - the temp name is also embedded in two SQL statements - makes them easier to read
			// compared to constructing them from this and other schema constants.
			const string TempRateEntryTableName = "#RateEntry";
			CreateTempRateEntryTable(connection, columnNamesSql);
			SaveRows(rateEntries, factory, TempRateEntryTableName);
			DeleteDuplicatesAndAddNewEntries(connection, columnNamesSql);
		}

		void DeleteDuplicatesAndAddNewEntries(DbConnection connection, string columnNamesSql)
		{
			string sql =
@"delete dbo.RateEntry
where TI_PK in
(
	select dupe.TI_PK
	from #RateEntry new
	cross apply dbo.GetOverlappingRateEntries(
	  new.TI_PK
	, new.TI_RateCategory
	, new.TI_Mode
	, new.TI_OriginLRC
	, new.TI_DestinationLRC
	, new.TI_ViaLRC
	, new.TI_PlannedLoadLRC
	, new.TI_PlannedDischargeLRC
	, new.TI_FirstLoadLRC
	, new.TI_LastDischargeLRC
	, new.TI_FirstRouteSetLoadPortLRC
	, new.TI_LastRouteSetDischargePortLRC
	, new.TI_RS_NKServiceLevel_NI
	, new.TI_PL_NKCarrierServiceLevel
	, new.TI_RS_NKGatewayServiceLevel
	, new.TI_RS_NKShipmentGatewayServiceLevel
	, new.TI_RH_NKCommodityCode
	, new.TI_FMCTariffID
	, new.TI_CartagePickupAddressPostCode
	, new.TI_CartageDeliveryAddressPostCode
	, new.TI_TransitTime
	, new.TI_Frequency
	, new.TI_FrequencyUnit
	, new.TI_IsCrossTrade
	, new.TI_IsTact
	, new.TI_MatchContainerRateClass
	, new.TI_TH
	, new.TI_OH_TransportProvider
	, new.TI_OH_Supplier
	, new.TI_OH_Consignor
	, new.TI_OH_Consignee
	, new.TI_OH_ControllingCustomer
	, new.TI_OA_CartagePickupAddressOverride
	, new.TI_OA_CartageDeliveryAddressOverride
	, new.TI_RateOrigin
	, new.TI_RateDestination
	, new.TI_TZ_OriginZone
	, new.TI_TZ_DestinationZone
	, new.TI_R9_FromSuburb
	, new.TI_R9_ToSuburb
	, new.TI_RC
	, new.TI_RCC_ComponentCode
	, new.TI_ContainerUnitSection
	, new.TI_RRC_RepairCode
	, new.TI_RMC_Material
    , new.TI_EstimateType
    , new.TI_REG_EquipmentGrade
    , new.TI_MNRGroup
	, new.TI_RateStartDate
	, new.TI_RateEndDate
	, new.TI_ParentID
	, new.TI_PaymentTerm
	, new.TI_GatewayAgentType
	, new.TI_ContractNumber
	, new.TI_AircraftType
	, new.TI_ShipmentConsolidationStatus
	, new.TI_HBLDeliveryMode
	, new.TI_IsNonOperatedReefer
	, new.TI_YardUnitType
	, new.TI_YardUnitLoad) dupe
)

insert dbo.RateEntry(" + columnNamesSql + @")
select " + columnNamesSql + @"
from #RateEntry

drop table #RateEntry";

			connection.ExecuteNonQuery(sql, 5 * 3600);
		}

		static string GetRateEntryColumnNamesSql(DataTable rateEntryTable)
			=> string.Join(", ", rateEntryTable.Columns.Cast<DataColumn>().Select(x => x.ColumnName));

		void CreateTempRateEntryTable(DbConnection connection, string columnNamesSql)
		{
			string sql = "if (OBJECT_ID(N'tempdb..#RateEntry', N'U') is NOT NULL) DROP TABLE #RateEntry; " +
				"\r\nselect top 0 " + columnNamesSql +
				"\r\ninto #RateEntry" +
				"\r\nfrom dbo.RateEntry";
			connection.ExecuteNonQuery(sql);
		}

		public virtual int GetExistingRateBacklog(Guid ratingHeaderPK, bool clearTACTRates, bool clearStandardRates, DbConnection connection)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"EXEC GetExistingRateBacklog '{0}', '{1}', '{2}'", ratingHeaderPK, clearTACTRates ? "Y" : "N", clearStandardRates ? "Y" : "N"); // SQL stored procedure call
			return connection.ExecuteScalar<int>(sqlText);
		}

		#endregion

		#region Progress Calculation

		void UpdateProgress(int percentage, string message)
		{
			ProgressChanged(this, new System.ComponentModel.ProgressChangedEventArgs(percentage, message));
		}

		void UpdateProgress(TACTImportResult result)
		{
			var percentage = GetProgressPercentage(result);
			var msg = Res.GetString("5a9639a4-4b3a-11e7-a1ba-fcaa14295823", "Processed {0} TACT rates of {1} total", result.ProcessedRates, result.TotalRates);

			UpdateProgress(percentage, msg);
		}

		static int GetProgressPercentage(TACTImportResult result)
		{
			return GetProgressPercentage(result.TotalRates, result.ProcessedRates);
		}

		static int GetProgressPercentage(long totalRates, long processedRates)
		{
			var percentage = totalRates > 0 && processedRates > 0
						? (int)(processedRates * 100 / totalRates)
						: 0;
			return percentage > 100 ? 100 : percentage;
		}

		#endregion

		readonly TACTReader reader;
		readonly bool isGlobal;
		readonly INotifications notifications;
		readonly object saveSyncObject = new object();
		readonly TACTImportOptions options;

		class TACTRate
		{
			public DataRow RateEntry;
			public DataRow RateLine;
			public IEnumerable<DataRow> RateLineItems;
			public int TACTRowsCount;
		}

		[Flags]
		public enum ExistingRatesHandlingStrategy
		{
			None = 0,
			ClearTACTRates = 1,
			ClearStandardRates = 2,
		}
	}
}
