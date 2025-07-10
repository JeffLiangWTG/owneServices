using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.AutoRating;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public class FreightAutoRater
	{
		public FreightAutoRater(IRatingContext context)
		{
			RatingContext = Argument.NotNull(context, nameof(context));
			RevenueRatesLoader = new RevenueRatesLoader(context.Factory, context.Logger);
			RevenueRatesLoader.OneOffQuoteSelected += (sender, e) => OneOffQuoteSelected?.Invoke(sender, e);
		}

		public IRatingContext RatingContext { get; }

		public event EventHandler<RevenueRatesLoader.QuoteEventArgs> OneOffQuoteSelected;

		internal RevenueRatesLoader RevenueRatesLoader { get; }

		#region AutoRating Execution

		public AutoRateResult AutoRate(RatingCriteria mainCriteria, CostSell costOrSell, bool searchForAdditionalRates = false)
		{
			try
			{
				var result = searchForAdditionalRates && costOrSell == CostSell.Revenue
					? SearchForAdditionalRevenueRates(mainCriteria)
					: new AutoRateResult(Factory, costOrSell);

				var allCriteria = new List<RatingCriteria>();
				allCriteria.AddRange(SetupNonContainerisedCriteria(mainCriteria));
				allCriteria.Add(mainCriteria);

				Calculate(allCriteria, costOrSell, result);

				if (!result.UserCancelledAutoRating && !RatingContext.SearchForRatesMode)
				{
					UpdateHost(mainCriteria, allCriteria, result.RateInfoCollection, costOrSell == CostSell.Cost);
				}

				return result;
			}
			catch (AutoRater.RatingCancelledException ex)
			{
				var result = new AutoRateResult(Factory, costOrSell);
				result.CancelAutoRating(ex.CancelReason ?? AutoRatingCancellation.UndefinedReason);

				return result;
			}
			catch (Exception ex)
			{
				// To help diagnosing exceptions during AutoRating we are collecting full AutoRating object.
				//instead of throw, we are using ExceptionDispatchInfo, so we don't loose real exception trace.
				ExceptionDispatchInfo.Capture(ex.Report(mainCriteria)).Throw();
				return null;
			}
		}

		void Calculate(IEnumerable<RatingCriteria> allCriteria, CostSell costOrSell, AutoRateResult result)
		{
			foreach (var criteria in allCriteria)
			{
				//should be not the full criteria passed as parameter but a ratesQuery sent here so that we could cache results by ratesQuery
				//in addition similarity/amountByLine and rebate checks should happed after the SearchForRates. Because similarity uses measures. While rebate is the reason we want to chache in the first place
				//ideally ratesQuery should have a structure identical to Rate and Charge classes from Rates Service + non supported on Rates Service fields like Warehouse data

				foreach (var charges in SearchForRates(costOrSell, criteria))
				{
					if (!AddCharges(result, charges, criteria.MergeCharges))
					{
						return;
					}
				}
			}
		}

		bool AddCharges(AutoRateResult result, AutoRateInfoCollection charges, MergeChargeOptions options)
		{
			if (charges.UserCancelledAutoRating)
			{
				result.CancelAutoRating(charges.Cancellation);
				return false;
			}

			if (options == MergeChargeOptions.WithinAdapter)
			{
				charges.SumUpSameCharges();
			}

			result.RateInfoCollection.AddRange(charges);
			return true;
		}

		IEnumerable<AutoRateInfoCollection> SearchForRates(CostSell costOrSell, RatingCriteria criteria)
		{
			var isCosting = costOrSell == CostSell.Cost;
			if (IsManualCostSelect(isCosting, criteria))
			{
				return SelectCostsInteractively(criteria) ?? SearchForAutoRates(costOrSell, criteria);
			}
			else
			{
				return SearchForAutoRates(costOrSell, criteria);
			}
		}

		IEnumerable<AutoRateInfoCollection> SearchForAutoRates(CostSell costOrSell, RatingCriteria criteria)
		{
			ReconfigureLoggers();

			var isCosting = costOrSell == CostSell.Cost;
			var entries = FindRateEntries(criteria, isCosting);
			var results = CalculateResultsForBestMatches(costOrSell, criteria, entries);
			return new[] { results };
		}

		void ReconfigureLoggers()
		{
			RatingContext.RatesProvider?.ReconfigureLogger(RatingContext.Logger);
		}

		/// <summary>
		/// Calculate results given a set of entries.
		/// The best matching rate lines will be used to calculate charges and return results.
		/// Rates that aren't the best match are ignored. The log will list the reason for ignoring.
		/// </summary>
		public AutoRateInfoCollection CalculateResultsForBestMatches(CostSell costOrSell, RatingCriteria criteria, IEnumerable<IRateEntry> entries, NotApplicableRateLineRemover.FilterOptions filterOptions = default)
		{
			try
			{
				var isCosting = costOrSell == CostSell.Cost;
				AutoRatingCalculatorParametersWithoutFilter parameters;

				using (var rateLinesRepository = new RateLinesRepository(criteria, entries.ToList(), RatingContext.Logger))
				{
					parameters = new AutoRatingCalculatorParametersWithoutFilter(criteria, this, isCosting);

					filterOptions.IsCosting = isCosting;
					filterOptions.IsRebateCalculationMode = RatingContext.IsInRebateCalculationMode;

					if (criteria.CarrierContractNumbers.Any())
					{
						filterOptions.RemoveFreightCharge = true;
					}

					var remover = new NotApplicableRateLineRemover();
					remover.RemoveNotApplicable(parameters, rateLinesRepository, filterOptions, RatingContext.DialogService);
					parameters.RemoveSimilarChargesAndBuildLineMeasureMatches(rateLinesRepository);

					var freightLeg1Entry = GetFreightEntry(rateLinesRepository, criteria, 1);
					var freightLeg2Entry = GetFreightEntry(rateLinesRepository, criteria, 2);
					parameters.SetFreightEntries(freightLeg1Entry, freightLeg2Entry);

					parameters.SetLinesToCalculate(rateLinesRepository);
				}

				CalculateAllLines(parameters);
				return parameters.Results;
			}
			catch (Exception ex)
			{
				// To help diagnosing exceptions during CalculateResultsForBestMatches we are collecting full AutoRating object.
				//instead of throw, we are using ExceptionDispatchInfo, so we don't loose real exception trace.
				ExceptionDispatchInfo.Capture(ex.Report(criteria)).Throw();
				return null;
			}
		}

		bool IsManualCostSelect(bool isCosting, RatingCriteria criteria)
		{
			var carrierConnectOnlySupportSeaAirFreight = RatingFeatureHelper.CarrierConnect.IsEnabledForRateSelection() && !(criteria.IsAirFreight || criteria.IsSeaFreight);
			return isCosting && RatingContext.IsManualCostSelectMode && criteria.SupportsManualRateSelection && !carrierConnectOnlySupportSeaAirFreight;
		}

		IEnumerable<AutoRateInfoCollection> SelectCostsInteractively(RatingCriteria criteria)
		{
			var charges = _Rating.Interactor.SelectRate(RatingContext, criteria);
			if (charges == null)
			{
				// User skipped rate selection
				return null;
			}

			var collection = new AutoRateInfoCollection(Factory);
			collection.AddRange(charges);

			IsManualCostSelected = true;
			return new[] { collection };
		}
		bool IsManualCostSelected { get; set; }

		AutoRateResult SearchForAdditionalRevenueRates(RatingCriteria mainCriteria)
		{
			var result = new AutoRateResult(Factory, CostSell.Revenue);

			var allCriteria = new List<RatingCriteria>();
			allCriteria.AddRange(SetupNonContainerisedCriteria(mainCriteria));
			allCriteria.Add(mainCriteria);

			result.PossibleMatchesWrapper.Criteria = mainCriteria;
			possibleMatchesWrapper = result.PossibleMatchesWrapper;

			// We create a new instance of rates loader instance of using the existing one (on a class level) because
			// we want to use a dummy logger, i.e. we don't want to log anything during the search for additional rates.
			var revenueLoader = new RevenueRatesLoader(Factory, new DummyLogger());

			foreach (var criteria in allCriteria)
			{
				var entries = revenueLoader.Load(criteria, RevenueLoadOptions.ClientRates | RevenueLoadOptions.Tariffs);
				var expiringEntries = RateEntryFilter.Filter(criteria, false, entries, Factory, new DummyLogger(), RatesToFindEnum.RatesGoingToExpire);
				var expiredEntries = RateEntryFilter.Filter(criteria, false, entries, Factory, new DummyLogger(), RatesToFindEnum.JustExpiredRates);

				result.RatesGoingToExpire.AddRange(expiringEntries);
				result.JustExpiredRates.AddRange(expiredEntries);
			}

			var unacceptedQuotes = revenueLoader.Load(mainCriteria, RevenueLoadOptions.UnacceptedQuotes);
			var filteredUnacceptedQuotes = RateEntryFilter.Filter(mainCriteria, false, unacceptedQuotes, Factory, new DummyLogger());

			result.UnacceptedQuotes.AddRange(filteredUnacceptedQuotes.Select(q => q.ParentRatingHeader).Distinct());
			return result;
		}

		#endregion

		#region Result Processing

		void UpdateHost(RatingCriteria criteria, List<RatingCriteria> criterias, AutoRateInfoCollection results, bool isCosting)
		{
			if (!UpdateContractNumber(criteria, results, isCosting))
			{
				results.CancelAutoRating(new AutoRatingCancellation(AutoRatingCancellation.Reasons.ContractNumbersPrompt, string.Empty));
				return;
			}

			if (criterias.Count == 1 && (criteria.StandardFreightCost?.Enabled ?? false) && isCosting)
			{
				SetStandardFreightCost(criteria, criterias[0], results);
			}

			UpdateChargeableAmount(criteria, results);
		}

		static void UpdateChargeableAmount(RatingCriteria criteria, AutoRateInfoCollection results)
		{
			var jobUpdate = criteria.GetRatingAdapter() as IJobDataUpdater;
			var invoicingSupporter = criteria.InvoicingSupporter;

			#region For Test Reason
#if DEBUG
			// Normally should come from criteria.InvoicingSupporter but those incorrectly setup mocked objects in tests require fall back
			var ratingProxy = criteria.AutoRating as AutoRatingProxy;
			if (invoicingSupporter == null)
			{
				invoicingSupporter = (ratingProxy?.AutoRating as IJobInvoicingPlugIn)?.InvoicingSupporter;
			}

			if (jobUpdate == null)
			{
				jobUpdate = ratingProxy?.AutoRating as IJobDataUpdater;
			}
#endif
			#endregion

			if (jobUpdate != null && invoicingSupporter != null && !invoicingSupporter.ActualChargeableUnit.IsEmpty)
			{
				var freightInfos = results.Where(r => r.ChargeCode.PK == Env.Registry.FreightChargeCode &&
					r.OperationalJobRef == invoicingSupporter.OperationalJobRef &&
					!r.Entry.IsSpotEntry);

				if (results.Any(r => r.Line.TL_Rounding == RatingRoundingTypes.Chargeable))
				{
					return;
				}

				var chargeable = 0m;
				foreach (var freightInfo in freightInfos)
				{
					if (freightInfo.CalculationLogs.Logs.Any(x => !x.ChargeableUnit.IsEmpty))
					{
						chargeable += freightInfo.CalculationLogs.Logs.Where(x => !x.ChargeableUnit.IsEmpty)
							.Max(x => new Quantity(x.Chargeable, x.ChargeableUnit).AmountFor(invoicingSupporter.ActualChargeableUnit).Amount);
					}
				}

				if (chargeable != 0m)
				{
					jobUpdate.UpdateChargeable(chargeable);
				}
			}
		}

		/// <summary>
		/// Update job's contract numbers after getting rates.
		/// </summary>
		/// <returns>False to indicate cancellation and True if updating is good</returns>
		bool UpdateContractNumber(RatingCriteria criteria, AutoRateInfoCollection results, bool isCosting)
		{
			return isCosting
				? UpdateContractNumberForCosting(criteria, results)
				: UpdateContractNumberForRevenue(criteria, results);
		}

		bool UpdateContractNumberForCosting(RatingCriteria criteria, AutoRateInfoCollection results)
		{
			if (IsManualCostSelected)
			{
				// Contract number like other job properties are updated in rate selectors, so, no need to update them twice.
				return true;
			}

			var rateContractNumbers = results
				.FilterByContractNumber(isCosting: true)
				.Where(x => x.IsCost)
				.Select(x => x.RateContractNumber.ToString());
			var checkResult = criteria.CanUpdateCarrierContractNumber(rateContractNumbers, RatingContext.DialogService, isManualCostSelected: false);
			if (!checkResult.CanUpdate)
			{
				// Autorating without RSL does not force CON to be updated so it's fine (don't stop) if user cancels the popup.
				return true;
			}

			if (checkResult.ConfirmationMessageForOverridingJobContractNumber != null
				&& _Rating.IsOn // some unit tests run rating without a session so there is no Interactor. Treat it as confirmed.
				&& _Rating.Interactor != null
				&& !_Rating.Interactor.YesNoWarning(checkResult.ConfirmationMessageForOverridingJobContractNumber))
			{
				return false;
			}

			return criteria.UpdateCarrierContractNumber(checkResult.Token) != DataUpdateResult.Cancelled;
		}

		static bool UpdateContractNumberForRevenue(RatingCriteria criteria, AutoRateInfoCollection results)
		{
			var clientContractNumbersInResult = results
				.FilterByContractNumber(isCosting: false)
				.Where(x => !x.IsCost && !(x.Entry?.IsSpotEntry ?? false))
				.Select(x => x.RateContractNumber.ToString())
				.Distinct()
				.ToArray();

			if (!clientContractNumbersInResult.Any())
			{
				return true;
			}

			var existingNumbers = criteria.ClientContractNumbers.ToArray();
			if (existingNumbers.Length > 1 && !criteria.IsMultipleClientContractNumberSupported)
			{
				_Rating.Interactor.Error(MultipleCLCsMessage);
				return false;
			}

			var updateResult = criteria.UpdateClientContractNumber(clientContractNumbersInResult);
			return updateResult != DataUpdateResult.Cancelled;
		}

		static string MultipleCLCsMessage => Res.GetString("3cd7c978-0874-47ca-a67c-c6f124287608", "Only one Client Contract Number, regardless same or different Country/Region of Issue, can be used for Autorating Revenue.");

		#endregion

		#region Setup Rating Criteria

		IEnumerable<RatingCriteria> SetupNonContainerisedCriteria(RatingCriteria mainCriteria)
		{
			var autoRatingInfo = mainCriteria.AutoRating as AutoRatingProxy;
			if (autoRatingInfo != null && ((autoRatingInfo.FreightMode & FreightMode.Containerised) != 0))
			{
				var lclMeasures = mainCriteria.JobMeasures.SplitOutLCLMeasuresIfPresent(autoRatingInfo.RateableMeasures);
				if (lclMeasures != null)
				{
					var freightType = (autoRatingInfo.FreightMode & FreightMode.FreightTypeMask);

					var lclCriteria = new RatingCriteria(autoRatingInfo, Factory) { ValuesCanBeSet = true };
					lclCriteria.FreightMode = freightType | FreightMode.NonContainerised;
					lclCriteria.SetOverrideMeasures(lclMeasures);
					lclCriteria.ValuesCanBeSet = false;
					yield return lclCriteria;

					// ROA and RAI have an additional NonContainerised mode - FullLoad.
					if (freightType == FreightMode.ROA || freightType == FreightMode.RAI)
					{
						var fullLoadCriteria = new RatingCriteria(autoRatingInfo, Factory) { ValuesCanBeSet = true };
						fullLoadCriteria.FreightMode = freightType | FreightMode.NonContainerised | FreightMode.FullLoad;
						fullLoadCriteria.SetOverrideMeasures(lclMeasures);
						fullLoadCriteria.ValuesCanBeSet = false;
						yield return fullLoadCriteria;
					}
				}
			}
		}

		#endregion

		#region Retrieve Rate Entries

		PossibleMatchesWrapper possibleMatchesWrapper;

		IEnumerable<IRateEntry> FindRateEntries(RatingCriteria criteria, bool isCosting)
		{
			IEnumerable<IRateEntry> entries;

			entries = isCosting
				? RatesProvider.GetCostRateEntries(criteria)
				: GetRevenueRateEntries(criteria);

			var repository = new RateEntriesRepository(entries, RatingContext.Logger);
			repository.FilterEntries(ValidateEntry);

			return repository.FilteredEntries;
		}

		IEnumerable<IRateEntry> GetRevenueRateEntries(RatingCriteria criteria)
		{
			var ratesLoadOptions = RevenueLoadOptions.Default;

			if (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.Value && criteria.GatewayConfiguration.IsGateway)
			{
				if (!criteria.GatewayConfiguration.ContinueAutoratingRevenueFromIntercompanyTariff)
				{
					return Array.Empty<IRateEntry>();
				}

				ratesLoadOptions = RevenueLoadOptions.IntercopanyTariffs;
			}

			// Standard rates
			var watch = Stopwatch.StartNew();

			var unfilteredStandardRates = RevenueRatesLoader.Load(criteria, ratesLoadOptions);
			var filteredStandardRates = RateEntryFilter.Filter(criteria, false, unfilteredStandardRates, Factory, RatingContext.Logger);
			
			watch.Stop();
						
			RevenueRatesLoader.ReportRatesLoadedStats(new RatesLoadedStats
			{
				LoadedRatesCount = unfilteredStandardRates.Count(),
				FilteredRatesCount = filteredStandardRates.Count()
			}, watch.ElapsedMilliseconds);

			if (possibleMatchesWrapper != null)
			{
				var headers = RevenueRatesLoader.LoadHeaders(criteria);
				AddToPossibleMatches(criteria, possibleMatchesWrapper, headers, unfilteredStandardRates);
			}

			// Spot rates
			var spotRates = RevenueRatesLoader.LoadSpotRates(criteria);

			// All rates
			var allRates = filteredStandardRates.Concat(spotRates).ToList();
			return allRates;
		}

		/// <summary>
		/// Prepares a collection of RateEntries that do not quite-match the
		/// criteria but possibly could. This list is later presented to the user
		/// in the PossibleMatches form for them to be informed but unable to
		/// really do anything with.
		/// </summary>
		static void AddToPossibleMatches(RatingCriteria criteria, PossibleMatchesWrapper possibleMatchesWrapper, IEnumerable<IRatingHeader> headers, IEnumerable<IRateEntry> rateEntries)
		{
			if (ShouldFindSimilarMatches(criteria, possibleMatchesWrapper))
			{
				var freightRates = rateEntries.Where(e => e.IsFreightEntry()).ToList();
				var freightMatcher = new FreightRateEntryFilter(criteria, false, criteria.Factory, null);
				freightMatcher.ShouldLogEntryRemovalReason = false;
				possibleMatchesWrapper.PossibleMatches.AddRange(freightMatcher.FindSimilarMatches(freightRates, headers));
			}
		}

		static bool ShouldFindSimilarMatches(RatingCriteria criteria, PossibleMatchesWrapper possibleMatchesWrapper)
		{
			if (DataRegistryRating.Instance.ProposeSimilarRates.Value
				&& criteria.SellSpotRateInfo.AutoratedMode != Constants.FreightRateAutoratingModes.Code.AllInRate
				&& !criteria.GatewayConfiguration.IsGateway)
			{
				var isForwardingOrShipping = criteria.RateTypeToUse == RateType.Forwarding || criteria.RateTypeToUse == RateType.Shipping;
				if (isForwardingOrShipping && possibleMatchesWrapper.PossibleMatches.Count == 0)
				{
					return true;
				}
			}

			return false;
		}

		string ValidateEntry(IRateEntry entry)
		{
			var errors = new List<string>();

			foreach (var line in entry.ChildRateLines)
			{
				var error = ValidateLine(line);
				if (error != null)
				{
					errors.Add(Res.GetString("47bec59e-50b1-4f5f-b7c0-4e0e25759dd6", "• {0} - {1}", line.DisplayInfo(), error));
				}
			}

			if (errors.Any())
			{
				var errorsSt = string.Join(System.Environment.NewLine, errors);
				return Res.GetString("1dc1c800-09da-4493-9882-da8dd9024d4f", "Is invalid due to the following errors:{0}{1}", System.Environment.NewLine, errorsSt);
			}

			return null;
		}

		string ValidateLine(IRateLine line)
		{
			if (!(line.Calculator is FreightInclusiveCalculator))
			{
				var currencies = Factory.GetCachedValue((NoResString)"Currencies", () =>     // it is a cache key, not going to be displayed anywhere
				{
					return Factory.Load<RefCurrency>(new ZQuery()).Select(c => c.RX_Code).ToHashSet();
				});

				if (!currencies.Contains(line.TL_RX_NKCurrency))
				{
					return Res.GetString("826209C7-91BC-4690-8777-CA6050B3D4A7", "Invalid currency: '{0}'", line.TL_RX_NKCurrency);
				}
			}

			return null;
		}

		IRatesProvider RatesProvider => RatingContext.RatesProvider;

		#endregion

		#region Expired and Expiring Rates

		public enum RatesToFindEnum
		{
			ActiveRates,
			RatesGoingToExpire,
			JustExpiredRates,
		}

		#endregion

		#region Calculate

		void LogPostRateLineCalculation(IRateLine line, AutoRateInfo info) =>
			RatingContext.Logger.Debug(LogMessages.CalculatedRateLine(line.DisplayInfo(), info?.Currency, info?.Amount.ToString(), info?.AgentAmount.ToString()));

		void CalculateAllLines(AutoRatingCalculatorParametersWithoutFilter parameters)
		{
			try
			{
				foreach (var fastLine in parameters.LinesToCalculate)
				{
					var rateLine = fastLine.Line;

					if (rateLine.TL_WeightVolume == Constants.Volume.TeaChest)
					{
						UpdatePackageVolumesForRounding(parameters.Criteria, rateLine);
					}

					CalculateSingleLine(parameters, fastLine);
				}
			}
			catch (ZArchitecture.Business.UnitConversionException ex)
			{
				throw new AutoRaterException(ex.Message);
			}
		}

		void UpdatePackageVolumesForRounding(RatingCriteria criteria, IRateLine line)
		{
			if (line.TL_Rounding == RatingRoundingTypes.UpTo1IfLessThanOne)
			{
				foreach (var packInfo in criteria.PackageInformation)
				{
					var individualVol = packInfo.Volume / packInfo.Count;

					if (Constants.Volume.Convert(individualVol, packInfo.VolumeUnit, Constants.Volume.TeaChest) < 1)
					{
						packInfo.Volume = Constants.Volume.Convert(packInfo.Count, Constants.Volume.TeaChest, packInfo.VolumeUnit);
					}
				}
			}
		}

		void CalculateSingleLine(AutoRatingCalculatorParametersWithoutFilter parameters, FastLine fastLine)
		{
			(var isSplit, var splitParameterList) = parameters.CreatedFilteredParametersIfNeeded(fastLine);
			var line = fastLine.Line;
			if (isSplit)
			{
				foreach (var splitParams in splitParameterList)
				{
					Calculate(splitParams, line);
				}
			}
			else
			{
				bool isCalculated = false;
				// Note, this is assuming the ServiceRater is not enabled if there are split parameters.
				// This is just a temporary limitation of ServiceRater that it doesn't support Warehouse/Local transport services yet.
				if (parameters.ServiceRater.IsEnabled && !fastLine.ChargeCode.AC_ChargeSubGroup.IsEmpty)
				{
					var services = parameters.ServiceRater.GetAllServicesForLine(fastLine);
					var serviceCreditorGroups = services.GroupBy(x => x.IsContractorCreditor ? (x.Contractor, x.ServiceId) :  (null, x.ServiceId));
					foreach (var creditorGroup in serviceCreditorGroups)
					{
						try
						{
							parameters.ServiceRater.SetServicesToCalculate(fastLine, creditorGroup);
							Calculate(parameters, line);
							isCalculated = true;
						}
						finally
						{
							parameters.ServiceRater.SetServicesToCalculate(null, null);
						}
					}
				}

				if (!isCalculated)
				{
					Calculate(parameters, line);
				}
			}
		}

		void Calculate(AutoRatingCalculatorParameters parameters, IRateLine line)
		{
			var oldValue = line.ViewAgentRates;

			line.SetViewAgentRatesWithoutRefreshBinding(false);
			var (calculationResult, calculationError) = line.Calculator.Calculate(parameters);

			line.SetViewAgentRatesWithoutRefreshBinding(true);
			var (agentCalculationResult, agentCalculationError) = line.Calculator.Calculate(parameters);

			line.SetViewAgentRatesWithoutRefreshBinding(oldValue);

			if (!string.IsNullOrEmpty(calculationError))
			{
				var info = new AutoRateInfo(calculationError, line, parameters, parameters.Factory);
				parameters.Results.Add(info);

				LogPostRateLineCalculation(line, info);
			}
			else
			{
				var infos = CreateAutoRateInfos(calculationResult, agentCalculationResult, parameters);
				foreach (var info in infos)
				{
					parameters.Results.Add(info);

					LogPostRateLineCalculation(line, info);
				}
			}
		}

		// Creating AutoRateInfos from CalculationResults normally works for common calculators but not for CST calculator
		// producing multiple calculation results.
		//
		// The problem is that we calculate non-agent rates and agent rates separately and we get two separate collections
		// of calculation results. If these collection contain 1 result each (case for normal calculators), we just match them
		// 1 to 1 and create 1 AutoRateInfo. If these collections contain multiple results each (an edge case for CST calculator)
		// there is no way to properly merge them under the same AutoRateInfo. Perhaps we need to refacotr calculators to return
		// a single CalculationResult for both - non-agent and agent calculations.
		//
		// This method is a temporary workaround where we try to manually match non-agent and agent calculation results.
		// It should work in most cases but sometimes it may not.
		static IEnumerable<AutoRateInfo> CreateAutoRateInfos(IEnumerable<CalculationResult> results, IEnumerable<CalculationResult> agentResults, AutoRatingCalculatorParameters parameters)
		{
			var infos = new List<AutoRateInfo>();

			if (results.Count() == 1)
			{
				var result = results.FirstOrDefault(x => x.PaymentBases.Any());
				var agentResult = agentResults.FirstOrDefault(x => x.PaymentBases.Any());

				if (result != null)
				{
					infos.Add(new AutoRateInfo(result, agentResult, parameters, parameters.Results.Factory));
				}
			}
			else
			{
				// This is a hack as explained in the comment above. There is no other way, we can merge them with non-agent rates.
				// In the most cases there will be one CalculationResult per ChargePK, so, it should be fine, for other cases,
				// we will have this hack for the time being.

				var agentCalculationResultList = agentResults.ToList();

				foreach (var result in results)
				{
					if (!result.PaymentBases.Any())
					{
						continue;
					}

					var agentResult = agentCalculationResultList.FirstOrDefault(r => r.ChargePK == result.ChargePK);
					if (agentResult != null)
					{
						agentCalculationResultList.Remove(agentResult);
					}

					infos.Add(new AutoRateInfo(result, agentResult, parameters, parameters.Results.Factory));
				}
			}

			return infos;
		}

		#region Leg Management

		IRateEntry GetFreightEntry(RateLinesRepository linesRepository, RatingCriteria criteria, int leg)
		{
			return linesRepository.GetLines()
				.Where(x => x.Line.TL_AC == Env.Registry.GetFreightChargeCode(criteria.Company.PK.ToGuid()))
				.Select(x => x.ParentRateEntry)
				.FirstOrDefault(x => criteria.GetFreightLeg(x) == leg);
		}

		#endregion

		#endregion

		#region Standard Freight Cost

		void SetStandardFreightCost(AutoRatingProxy autoRatingProxy, RatingCriteria criteria, AutoRateInfoCollection results)
		{
			var result = Money.Invalid;

			var freightResults = results
				.Where(i => i.ChargeCode.PK.ToGuid() == Env.Registry.FreightChargeCode && i.ProviderPK == criteria.Carrier?.PK)
				.ToList();

			var carriersCostChargeableWeights = freightResults
				.SelectMany(i => i.Bases.Select(b => b.Chargeable))
				.Where(c => c.IsWeight())
				.ToArray();

			decimal? originalWeight = null;

			MeasureAnalyser.IMeasureChanger measureChanger = null;
			try
			{
				// Most favourable weight break for Consolidated Weight
				ZDecimal[] originalWeights;
				if (carriersCostChargeableWeights.Length == 1
					&& criteria.AdapterType == AdapterType.Consolidation
					&& (originalWeights = GetOriginalWeights(freightResults)).Length > 0
					&& originalWeights.AllSame())
				{
					var measures = criteria.JobMeasures;
					var potentiallyUpdatedWeight = carriersCostChargeableWeights[0];

					measureChanger = measures.BeginTemporaryChanges();

					if (measures.HasMeasureType(MeasureType.Weight))
					{
						originalWeight = originalWeights[0];
						measureChanger.SetTemporaryQuantity(MeasureType.Weight, potentiallyUpdatedWeight);
					}

					if (measures.HasMeasureType(MeasureType.JobWeight))
					{
						measureChanger.SetTemporaryQuantity(MeasureType.JobWeight, potentiallyUpdatedWeight);
					}

					if (measures.HasMeasureType(MeasureType.Chargeable) && Constants.Weight.ContainsCode(measures.GetUnit(MeasureType.Chargeable)))
					{
						measureChanger.SetTemporaryQuantity(MeasureType.Chargeable, potentiallyUpdatedWeight);
					}
				}

				var parameters = new AutoRatingCalculatorParametersWithoutFilter(criteria, this, true);

				var logsWrapper = RatingContext.RateCalculationLogWrapper;

				var standardFreightCostLines = FindStandardFreightCostLines(parameters);
				if (standardFreightCostLines != null && standardFreightCostLines.Count > 0)
				{
					foreach (var freightLine in standardFreightCostLines)
					{
						var (calculationResults, error) = freightLine.Calculator.Calculate(parameters);
						if (!string.IsNullOrEmpty(error))
						{
							continue;
						}

						foreach (var calculationResult in calculationResults)
						{
							var calcLog = calculationResult.FreightChargeCodeCalculationLog;
							if (calcLog != null && !calcLog.IsEmpty)
							{
								if (originalWeight.HasValue)
								{
									calcLog.Weight = originalWeight.Value;
								}
								logsWrapper.Logs.Add(calcLog);
							}

							// Normally there has to be just 1 calculation result. Multiple are possible for Cost Based Calculator which normally
							// is not used with standard freight costs.
							if (calculationResults.Count() == 1)
							{
								var perUnit = calculationResult.PaymentBases.FirstOrDefault(p => p.RateInfo.Type == RateInfo.RateInfoType.UNT);
								if (perUnit.RateInfo.PerUnitRate.HasValue)
								{
									result = new Money(perUnit.RateInfo.PerUnitRate.Value, freightLine.Line.Currency);
								}
							}
						}
					}
				}

				SaveCalculationLogs(autoRatingProxy.StandardFreightCost, logsWrapper);
				autoRatingProxy.StandardFreightCost.Set(result);
			}
			finally
			{
				measureChanger?.Dispose();
			}
		}

		static ZDecimal[] GetOriginalWeights(List<AutoRateInfo> freightResults)
		{
			return freightResults
				.SelectMany(r => r.CalculationLogs.Logs)
				.Where(l => l.Weight > 0)
				.Select(l => l.Weight)
				.ToArray();
		}

		List<FastLine> FindStandardFreightCostLines(AutoRatingCalculatorParametersWithoutFilter parameters)
		{
			var criteria = parameters.Criteria;
			var logPrefix = (NoResString)"(Searching for Freight Cost to update the job)"; // log message, subject to change, more for support people as of now

			var entries =
				RatingContext.ProviderCW1Rates.GetCostRateEntries(criteria)
				.Where(entry => entry.ParentRatingHeader.IsStandardCostRate())
				.ToList();

			using (var linesRepository = new RateLinesRepository(criteria, entries, Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode), RatingContext.Logger.WithPrefix(logPrefix + " ")))
			{
				var options = new NotApplicableRateLineRemover.FilterOptions
				{
					IsCosting = true,
					DisableExcludedFromAutoRatingFilter = true,
					DisableIrrelevantTariffsFilter = true,
					IsRebateCalculationMode = RatingContext.IsInRebateCalculationMode
				};

				var remover = new NotApplicableRateLineRemover();
				remover.RemoveNotApplicable(parameters, linesRepository, options, RatingContext.DialogService);
				parameters.RemoveSimilarChargesAndBuildLineMeasureMatches(linesRepository);

				return linesRepository.GetLines();
			}
		}

		void SaveCalculationLogs(IAutoRatingStandardFreightCost standardFreightCost, CalculationLogsWrapper logsWrapper)
		{
			var bizo = standardFreightCost.GetHost();
			if (bizo != null && !logsWrapper.IsEmpty)
			{
				CalculationLogsLoader.Save(bizo, logsWrapper);
			}
			else
			{
				DisableCalculationLogs(standardFreightCost);
			}
		}

		void DisableCalculationLogs(IAutoRatingStandardFreightCost standardFreightCost)
		{
			var bizo = standardFreightCost.GetHost();
			if (bizo != null)
			{
				CalculationLogsLoader.Disable(bizo);
			}
		}

		#endregion

		#region Factory

		internal BusinessObjectFactory Factory => RatingContext.Factory;

		#endregion
	}
}

#pragma warning restore 0618
