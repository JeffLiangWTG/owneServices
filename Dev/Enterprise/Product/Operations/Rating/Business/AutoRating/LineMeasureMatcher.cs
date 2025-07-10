using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Handles matching of a rate line to the job based on its similarity to the job measure attributes.
	/// Removes lines that are not the best match.
	/// Retains matching information in IAmountByLine for later use by calculators.
	///
	/// Job measure attributes are container type, commodity, etc.
	/// They differ from other job attributes like Origin Port in that they can have multiple values.
	/// Each part is matched separately. For example, each container is matched.
	/// Containers with different commodities may match different lines.
	///
	/// When more than one line matches a part and they have the same charge code and the same leg, the best line(s) are determined.
	/// Note, leg is applicable only to freight rates and is determined by origin, via and destination ports (leg 1 is the leg starting at the origin, etc).
	/// The best line is:
	/// - the higher match according to OverriddenRateLineRemover <see cref="OverriddenRateLineRemover"/>
	/// - the highest similarity to the part (for lines that compare as equal in the Remover)
	/// For example, if a global line and a local line are similar to a part then the local line is considered the best
	/// since local is better than global in the Remover. The global line may have higher similarity
	/// but the local/global comparison takes precedence.
	/// Lines that are equal in the Remover and equal similarity may be duplicates or in conflict and can get removed.
	///
	/// There are different rules depending on the calculator.
	/// 1) Rate lines with calculators that depend on measures are matched against each part with the same measure.
	/// E.g. the unit calculator needs the quantity of the configured unit, such as the weights if the line has a weight unit.
	/// The best matches for each part are kept. Any line that is not the best match for anything is removed.
	/// The match from line to part is stored for later.
	/// This is how, for example, a rate for a specific container type is made to apply to only those containers with the same type
	/// even if the job has containers with other types.
	///
	/// 2) Rate lines with calculators that don't depend on measures have two possible rules:
	/// 2.1) If the calculator has been deemed as not needing a similarity check it always matches.
	/// <see cref="IsLineMeasurableAgainstSimilarCharges"/> for the calculators.
	/// However, it still takes part in picking the best match, and can cause other lines to be removed.
	/// For example, a line with the Inclusive calculator and the BAF charge code is not measurable,
	/// but if there is another BAF line with a per-container unit calculator that is a lesser match
	/// for all container parts then that other line will be removed.
	/// If the other line was a better match then neither line would be removed, which seems inconsistent, but that's how it currently works.
	///
	/// Note, if all the lines fall under the above rule 2.1, then this class has nothing to do. No line will be removed.
	///
	/// 2.2) If the calculator does need a similarity check it uses the same similarity logic as in rule 1,
	/// but the rate attributes are compared against all the job attributes, rather than part by part.
	/// For example, two rates with flat calculator, one with container type of 20GP and one with container type empty,
	/// when compared with a job with just 20GP containers will pick the 20GP rate as most similar to the job.
	/// If the job was a mix of 20GP and 40GP containers then the rate with no container type is considered most similar.
	///
	/// Mixing calculators with different rules
	/// =======================================
	/// Things get complicated when lines with the same charge code have different calculators or different units.
	/// If the lines apply to different parts then they can both apply.
	/// If one line applies to parts, but another line applies to the job then it can be that only the best applies.
	/// For example, for a per-unit calculator and a flat calculator typically only one will apply.
	/// The rule to decide what happens has yet to be precisely defined.
	///
	/// MeasureType.Unidentified
	/// ========================
	/// Reference attributes for calculators that don't depend on measures are taken from the MeasureType.Unidentified measure.
	/// For example, to pass the warehouse docket reference to the flat calculator, set the docket reference on the MeasureType.Unidentified part.
	/// Warehouse jobs are filtered on docket reference and the filter gets its values from the parts that matched the rate line.
	/// This class will match non-measure calculators with the MeasureType.Unidentified part so the filter gets the right value.
	/// Should really have a more obvious interface for this logic though.
	///
	/// Weight, Volume and Chargeable
	/// =============================
	/// Calculating by weight can actually use volume quantities via standard conversion factors, and vice-versa.
	/// It can also use the chargeable amount if the TL_Rounding is Chargeable.
	/// So matching by weight must also trigger matching by the volume and chargeable. Similarly for matching by volume.
	/// As of Oct 2021, Chargeable is only used for shipment packlines and consols.
	/// Note, the Chargeable commodity can differ from a pack line weight/volume commodity.
	/// Lines will never actually have a MeasureType of Chargeable though since no unit maps to Chargeable.
	///
	/// Conflicts and Duplicates
	/// ========================
	/// For all calculators, there is a conflict check that can also remove the rate.
	/// If two or more rates are the best match (for the same part if they use rule 1, or the job for rule 2)
	/// then each pair is checked for a conflict.
	/// First, the rates must not be compatible for simultaneous usage in at least one attribute.
	/// Second, if they don't have exactly the same calculator type and settings
	///	then they conflict - a conflict error rate is generated and becomes the result rate for that matching step,
	///	otherwise they are the same charge and one is removed as a duplicate.
	///	Rates are considered compatible for simultaneous usage in an attribute if they are both similar or both not similar to each job value.
	///	For example, for the container type attribute, if two rates had the same container type they would be compatible.
	///	A typical case of conflict is two rates with different container types and the flat calculator, and the job has both container types.
	///
	///	It is quite difficult for rates that do depend on measures to be in conflict since they would both need to be the best
	///	match for one part, but there would need to be another part that matches one rate and not the other.
	///	For the conflict to be in the container type, they would both have to match on class for one container,
	///	but on some other container only one would match. It would be an odd setup.
	///
	/// </summary>
	public class LineMeasureMatcher
	{
		public LineMeasureMatcher(RatingCriteria criteria)
		{
			Criteria = criteria;
			CreateComparers();
		}

		public RatingCriteria Criteria { get; }

		/// <summary>
		/// Remove lines where the measures they require have errors or do not exist.
		///
		/// Historically this logic was in NotApplicableRateLineRemover class (originally FreightAutoRater)
		/// but it has common logic to this class's RemoveSimilarCharges method, so they are kept together.
		/// </summary>
		public static void RemoveChargesThatDependOnInvalidOrMissingRequiredMeasures(AutoRatingCalculatorParameters parameters, RateLinesRepository rateLinesRepository, RatingCriteria criteria)
		{
			foreach (var chargeCode in rateLinesRepository.GetChargeCodes())
			{
				var linesList = rateLinesRepository.GetLines(chargeCode);
				if (linesList.Count > 0)
				{
					Dictionary<MeasureType, string> errorMeasureTypeToErrorMessage = null;
					foreach (var line in linesList)
					{
						if (IsLineRequiringMeasures(line, parameters))
						{
							var measures = criteria.JobMeasures;
							var measureTypes = parameters.GetMeasureTypes(line);
							var wasRemoved = RemoveLineWithMeasureError(rateLinesRepository, line, measures, measureTypes, ref errorMeasureTypeToErrorMessage);
							if (wasRemoved)
							{
								continue;
							}

							if (measureTypes.All(x => !measures.ContainsKey(x)))
							{
								var reason = (NoResString)"no " + string.Join((NoResString)",", measureTypes) + (NoResString)" measure on the job."; // log message, no translation
								rateLinesRepository.Remove(line, reason);
							}
						}
						else if (criteria.ShouldDiscardPerJobCharges)
						{
							rateLinesRepository.Remove(line, (NoResString)"the charge is per job, only charges per measures are calculated by this adapter");
						}
					}
				}
			}
		}

		static bool RemoveLineWithMeasureError(RateLinesRepository rateLinesRepository, FastLine line, MeasureAnalyser measures, IEnumerable<MeasureType> measureTypes, ref Dictionary<MeasureType, string> errorMeasureTypeToErrorMessage)
		{
			bool wasRemoved = false;
			foreach (var measureType in measureTypes)
			{
				var errors = measures.GetMeasureErrors(measureType);
				if (errors.Any())
				{
					if (errorMeasureTypeToErrorMessage == null)
					{
						errorMeasureTypeToErrorMessage = new Dictionary<MeasureType, string>();
					}
					var reason = errorMeasureTypeToErrorMessage.GetOrAdd(measureType, () => errors.ToStringWithNewLineBetweenStrings());
					rateLinesRepository.Remove(line, reason, isWarning: true);
					wasRemoved = true;
					break;
				}
			}

			return wasRemoved;
		}

		#region Similar Charges

		/// <summary>
		/// Remove less similar charges from the given lines.
		/// </summary>
		/// <param name="parameters">AutoRatingCalculatorParameters needed for some calculators to determine units</param>
		/// <param name="rateLinesRepository"></param>
		/// <param name="amountByLine">AmountByLineTable to store match between amounts (measures) and lines. Pass null if the matches need not be stored.</param>
		public void RemoveSimilarCharges(AutoRatingCalculatorParameters parameters, RateLinesRepository rateLinesRepository, IAmountByLine amountByLine)
		{
			var shouldAddToLog = false;
			var logBuilder = new ZStringBuilder();
			logBuilder.AppendLine((NoResString)"Chargeable was added for");// log message, subject to change
			foreach (var chargeCode in rateLinesRepository.GetChargeCodes())
			{
				var groups = rateLinesRepository.GetSimilarLineGroups(chargeCode);

				foreach (var group in groups)
				{
					// Autorating service charges is different to autorating other charges. It doesn't use the AmountByLine
					// structure (populated by RemoveSimilarChargesForChargeCode), where we store details about which line calculates
					// which measure part so that the calculator knows which amount to use during calculation of the line.
					//
					// Instead, it stores relations between services and lines (populated by RemoveSimilarServiceCharges),
					// so that autorating knows which line should calculate which service.
					//
					// So, services charges are treated separately.

					var serviceCharges = group.Where(parameters.ServiceRater.CanAutoRate).ToList();
					var nonServiceCharges = group.Except(serviceCharges).ToList();

					if (serviceCharges.Any())
					{
						RemoveSimilarServiceCharges(parameters, serviceCharges, rateLinesRepository, amountByLine);
					}

					if (nonServiceCharges.Any())
					{
						// Some service charges are autorated by generic autorater too. It happens when a line for a service charge
						// is per weight, container or any other non-service unit. Currently, only service occurrence and service time
						// are considered as service units and are autorated with service autorater.
						//
						// Check parameters.ServiceRater.CanAutoRate for more details
						shouldAddToLog |= RemoveSimilarChargesForChargeCode(parameters, rateLinesRepository, amountByLine, nonServiceCharges, logBuilder);
					}
				}
			}

			if (shouldAddToLog)
			{
				rateLinesRepository.LogAfterSearchAndFilter().Information(logBuilder.ToString());
			}
		}

		/// <summary>
		/// Analyses and builds information about the given list of lines with the same charge code.
		/// </summary>
		class LineListInfo
		{
			internal LineListInfo(LineMeasureMatcher matcher, AutoRatingCalculatorParameters parameters, List<FastLine> linesWithSameChargeCode)
			{
				int lineCount = linesWithSameChargeCode.Count;
				LinesToMatchParts = new List<LineWithMeasures>(lineCount);
				LinesToMatchJob = new List<FastLine>(lineCount);
				LinesNotMeasurableAgainstSimilarCharges = new List<FastLine>(lineCount);
				PartListToMeasureTypes = new Dictionary<IRateablePartList, List<MeasureType>>();

				bool lineMeasuresHaveWeight = false;
				bool lineMeasuresHaveVolume = false;
				bool lineMeasuresHaveChargeable = false;

				foreach (var line in linesWithSameChargeCode)
				{
					if (IsLineMeasurableAgainstSimilarCharges(line))
					{
						bool shouldMatchParts = false;
						var measureableLine = new LineWithMeasures(line, parameters);
						foreach (var measureType in measureableLine.MeasureTypes)
						{
							var partList = parameters.Criteria.RateableMeasures.GetPartList(measureType);
							// Note, RemoveChargesThatDependOnInvalidOrMissingRequiredMeasures will
							// have removed lines that have all their measures missing.
							// So for the typical case of a calculator with one measure type the partList will not be null.
							// However the Highest Rate Calculator has two measure types, so it is possible that
							// it has just one measure type existing on the job. In practice, this is unlikely.
							if (ShouldMatchPerPart(partList, matcher))
							{
								shouldMatchParts = true;
								AddMeasureTypeToMatch(partList, measureType);
								if (measureType == MeasureType.Weight)
								{
									lineMeasuresHaveWeight = true;
								}
								else if (measureType == MeasureType.Volume)
								{
									lineMeasuresHaveVolume = true;
								}
								else if (measureType == MeasureType.Chargeable)
								{
									lineMeasuresHaveChargeable = true;
								}
							}
						}
						// Allocate a line to either match on parts or match on the entire job.
						// In principle, a calculator could match on both, if it has two measure types (highest rate calculator).
						// However, the job match can still be done by matching on parts because of the way
						// part matching falls back to job measures. It would just be less efficient.
						// So even if there was such a setup it should be OK to just match both measures on parts.
						// In practice, this would be an unusual job though and probably never occurs.
						if (shouldMatchParts)
						{
							LinesToMatchParts.Add(measureableLine);
						}
						// If a line is being rated by a service, it should not rate the job.
						else
						{
							LinesToMatchJob.Add(line);
						}
					}
					else
					{
						LinesNotMeasurableAgainstSimilarCharges.Add(line);
					}
				}

				// Add weight, volume and chargeable if needed and they are valid for matching
				if (lineMeasuresHaveWeight || lineMeasuresHaveVolume)
				{
					if (!lineMeasuresHaveWeight)
					{
						AddExtraMeasureTypeToMatchIfValid(parameters, MeasureType.Weight, matcher);
					}

					if (!lineMeasuresHaveVolume)
					{
						AddExtraMeasureTypeToMatchIfValid(parameters, MeasureType.Volume, matcher);
					}

					if (!lineMeasuresHaveChargeable)
					{
						AddExtraMeasureTypeToMatchIfValid(parameters, MeasureType.Chargeable, matcher);
					}
				}
			}

			/// <summary>
			/// Determine if a part list should be matched on each part.
			/// Will be true if the part has rate dimensions, or has a package type.
			/// The check on package type is to compensate for an inconsistency in AutoRatingCalculatorParameters.SumAmountFromParts.
			/// SumAmountFromParts has special logic for package units that only applies to parts that matched a line.
			/// So we need to ensure a partList with HasPackageType is matched to parts.
			/// Further investigation is needed to remove the inconsistency. Ideally SumAmountFromParts should have the same logic for all cases.
			/// </summary>
			static bool ShouldMatchPerPart(IRateablePartList partList, LineMeasureMatcher matcher)
				=> partList != null && (matcher.PartsHaveRateDimensions(partList) || partList.HasPackageType);

			void AddMeasureTypeToMatch(IRateablePartList partList, MeasureType measureType)
			{
				if (!PartListToMeasureTypes.TryGetValue(partList, out var measureTypes))
				{
					measureTypes = new List<MeasureType>();
					PartListToMeasureTypes.Add(partList, measureTypes);
				}

				if (!measureTypes.Contains(measureType))
				{
					measureTypes.Add(measureType);
				}
			}

			void AddExtraMeasureTypeToMatchIfValid(AutoRatingCalculatorParameters parameters, MeasureType measureType, LineMeasureMatcher matcher)
			{
				var partList = parameters.Criteria.RateableMeasures.GetPartList(measureType);
				if (ShouldMatchPerPart(partList, matcher))
				{
					AddMeasureTypeToMatch(partList, measureType);
					if (extraMeasureTypes == null)
					{
						extraMeasureTypes = new List<MeasureType>();
					}
					extraMeasureTypes.Add(measureType);
				}
			}

			internal List<LineWithMeasures> LinesToMatchParts { get; }
			internal List<FastLine> LinesToMatchJob { get; }
			internal List<FastLine> LinesNotMeasurableAgainstSimilarCharges { get; }
			internal IEnumerable<FastLine> LinesMeasurableAgainstSimilarCharges => LinesToMatchParts.Select(x => x.Line).Concat(LinesToMatchJob);
			internal Dictionary<IRateablePartList, List<MeasureType>> PartListToMeasureTypes;

			List<MeasureType> extraMeasureTypes;

			/// <summary>
			/// Extra measure types do not occur on any line, but they need to be matched and stored in AmountByLine.
			/// E.g., weight if lines only have volume.
			/// </summary>
			internal bool IsExtraMeasureType(MeasureType measureType)
				=> extraMeasureTypes != null && extraMeasureTypes.Contains(measureType);
		}

		/// <summary>
		/// Remove similar charges from the given lines, all with the same charge code.
		/// The number of lines will usually be less than 10.
		/// However, the worst case would be something like
		/// - a warehouse with hundreds of products and a rate line for each product
		/// - a consol with many container types and commodities and a rate line for each combination
		/// It needs to be able to scale to hundreds of lines.
		/// </summary>
		bool RemoveSimilarChargesForChargeCode(
			AutoRatingCalculatorParameters parameters,
			RateLinesRepository rateLinesRepository,
			IAmountByLine amountByLine,
			List<FastLine> linesWithSameChargeCode,
			ZStringBuilder logBuilder)
		{
			var info = new LineListInfo(this, parameters, linesWithSameChargeCode);
			var pointMatchingLog = new PointMatchingLog(parameters.Factory);

			var allBestMatches = new HashSet<FastLine>(linesWithSameChargeCode.Count);
			AddBestMatchesForParts(parameters, amountByLine, info, pointMatchingLog, allBestMatches);
			if (info.LinesToMatchJob.Count > 0)
			{
				AddBestMatchesForJob(parameters, linesWithSameChargeCode, allBestMatches);
			}

			// Remove lines that didn't make it into the best matches, and are measureable against similar charges.
			foreach (var line in info.LinesMeasurableAgainstSimilarCharges)
			{
				if (!allBestMatches.Contains(line))
				{
					// If a charge is used to autorate a service, we should not remove it from calculations.
					rateLinesRepository.Remove(line, (NoResString)"failed similarity check"); // log message, subject to change, more for support people as of now
				}
			}

			// add any new lines, i.e., errors due to conflicting rates
			var bestMatchesToAdd = allBestMatches.Where(x => !linesWithSameChargeCode.Contains(x));
			rateLinesRepository.AddSilently(bestMatchesToAdd);

			bool shouldAddToLog = pointMatchingLog.Any();
			if (shouldAddToLog)
			{
				logBuilder.AppendLine(pointMatchingLog.GetLog());
			}

			return shouldAddToLog;
		}

		/// <summary>
		/// Find the best matches for parts that
		/// - have a measure type from any of the lines
		/// - and have matchable attributes.
		/// </summary>
		/// <param name="allBestMatches">stores results</param>
		void AddBestMatchesForParts(AutoRatingCalculatorParameters parameters, IAmountByLine amountByLine, LineListInfo info, PointMatchingLog pointMatchingLog, HashSet<FastLine> allBestMatches)
		{
			foreach (var partListMeasureTypePair in info.PartListToMeasureTypes)
			{
				var partList = partListMeasureTypePair.Key;
				var partListMeasureTypes = partListMeasureTypePair.Value;

				var linesToMatch = new List<FastLine>(info.LinesToMatchParts.Count);
				foreach (var line in info.LinesToMatchParts)
				{
					if (HasCompatibleMeasureTypes(line, partListMeasureTypes))
					{
						linesToMatch.Add(line.Line);
					}
				}

				if (linesToMatch.Count > 0)
				{
					bool addToBestMatches = partListMeasureTypes.Any(x => !info.IsExtraMeasureType(x));

					if (addToBestMatches)
					{
						// Add in lines that don't match parts since they may be better and can cause lines that do match parts to be removed.
						linesToMatch.AddRange(info.LinesToMatchJob);
						linesToMatch.AddRange(info.LinesNotMeasurableAgainstSimilarCharges);
					}

					var measureInfos = partListMeasureTypes.Select(x => PartMeasureInfoProvider.Instance.GetPartMeasureInfo(x)).ToList();
					foreach (var part in partList)
					{
						pointMatchingLog.CurrentMeasureType = partListMeasureTypes.First();
						var partBestMatches = FindBestMatches(parameters, linesToMatch, partList, part, pointMatchingLog, false);
						foreach (var bestMatch in partBestMatches)
						{
							if (bestMatch != null)
							{
								if (addToBestMatches)
								{
									allBestMatches.Add(bestMatch);
								}
								var lineInfo = bestMatch.Line.DisplayInfo();
								var partName = pointMatchingLog.GetPartName(partList, part);
								for (int i = 0; i < partListMeasureTypes.Count; ++i)
								{
									// Note, it seems unnecessary for lines that don't depend on measures
									// to be added to AmountByLine, but it would need further investigation to remove it.
									amountByLine?.AddAmount(partListMeasureTypes[i], bestMatch.Line, part);
									pointMatchingLog.CurrentMeasureType = partListMeasureTypes[i];
									pointMatchingLog.LogPartMatchedLine(lineInfo, partName, measureInfos[i].GetActualValue(part).ToString());
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Determines if the line should be matched to the part list based on its measure types.
		/// This has large impact when multiple lines match, since it means they will be ranked and possibly some removed.
		/// For example, if this method says a line with MeasureType.JobUnit should be matched to a part list with MeasureType.Unit
		/// then another line with MeasureType.Unit may end up being removed.
		/// However, if JobUnit should not be matched then neither line will be removed and both rates will apply.
		/// The logic for determining which lines should be compared and which should not does not seem to be defined in any spec.
		/// Originally it seems the assumption was that if rates have the same charge code then they should always be compared.
		/// However, there were exceptions to this assumption such as MeasureType.ContainerCount is not compatible with MeasureType.Unit.
		/// It seems most customers generally used different charge codes for rates they want to apply simultaneously so the customer impact
		/// of this rule is small.
		/// </summary>
		bool HasCompatibleMeasureTypes(LineWithMeasures line, List<MeasureType> partListMeasureTypes)
		{
			foreach (var measureType in line.MeasureTypes)
			{
				if (partListMeasureTypes.Contains(measureType))
				{
					return true;
				}
				else if (measureType == MeasureType.Unit)
				{
					if (partListMeasureTypes.Any(x => IsIncompatibleWithMeasureTypeUnit(x)))
					{
						return false;
					}
				}
				else if (IsIncompatibleWithMeasureTypeUnit(measureType))
				{
					if (partListMeasureTypes.Contains(MeasureType.Unit))
					{
						return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		///		Returns true if the <paramref name="measureType"/> is incompatible with MeasureType.Unit.
		/// </summary>
		/// <remarks>
		///		Unidentified here is a warehouse special case. They want to add docket reference to a charge code
		///		description. It is populated from charge attribute which in its turn is populated from measures part
		///		attributes being calculated. It works for per unit charges and they match unit related measure parts,
		///		but in warehouse case they want to add docket reference to Flat calculation as well which is normally
		///		calculated per job rather than per concrete measure part. So, they create a fake measure part with
		///		Unidentified type so that a Flat line could match it and populate attached docket reference attribute.
		///
		///		Also, seems like those Unidentified measure has something to do with warehouse service count calculation.
		///
		///		We exclude it from Unit comparision so that it doesn't conflict with pure Per Unit calculations. Without
		///		this, it causes some ugly side-effects when there are 2 charges with the same charge code - one is Per Unit
		///		and another one Flat.
		/// </remarks>
		static bool IsIncompatibleWithMeasureTypeUnit(MeasureType measureType)
			=> measureType == MeasureType.ContainerCount
			   || measureType == MeasureType.LocationPallet
			   || measureType == MeasureType.PalletID
			   || measureType == MeasureType.Unidentified;

		/// <summary>
		/// Add best matches for lines that that have MeasureTypes/parts with no matcheable attributes.
		/// They, and all other lines with the same charge code, are matched on all other job measure attributes.
		/// This allows for example, flat calculators for jobs that don't set MeasureType.Unidentified
		/// to be matched, compared, and potentially removed.
		/// Note, we have to include all lines, including those that match parts, since
		/// we allow a line that matched a part to override a line that that had no measures.
		/// E.g., a unit calculator line can override a flat calculator line (and vice-versa).
		/// </summary>
		/// <param name="allBestMatches">stores results</param>
		void AddBestMatchesForJob(AutoRatingCalculatorParameters parameters, List<FastLine> lines, HashSet<FastLine> allBestMatches)
		{
			allBestMatches.UnionWith(FindBestMatches(parameters, lines, null, null, null, false));
		}

		/// <summary>
		/// A line and its measure types
		/// </summary>
		class LineWithMeasures
		{
			public LineWithMeasures(FastLine line, AutoRatingCalculatorParameters parameters)
			{
				Line = line;
				MeasureTypes = parameters.GetMeasureTypes(line);
				foreach (var measureType in MeasureTypes)
				{
					if (measureType == MeasureType.Weight)
					{
						HasWeight = true;
					}
					else if (measureType == MeasureType.Volume)
					{
						HasVolume = true;
					}
				}
			}

			public FastLine Line { get; }
			public IEnumerable<MeasureType> MeasureTypes { get; }
			public bool HasVolume { get; }
			public bool HasWeight { get; }
		}

		void RemoveSimilarServiceCharges(AutoRatingCalculatorParameters parameters, IEnumerable<FastLine> similarCharges, RateLinesRepository rateLinesRepository, IAmountByLine amountByLine)
		{
			var chargeCode = similarCharges.First().ChargeCode;
			var services = parameters.ServiceRater.JobServices.FindServices(chargeCode)
				.Where(x => x.IsEnabled);

			if (!services.Any())
			{
				return;
			}

			var serviceRater = parameters.ServiceRater;
			var allBestMatches = new HashSet<FastLine>();
			var allMeasurableLines = new HashSet<FastLine>();
			var containersWithDimensions = parameters.Criteria.RateableMeasures.GetPartList(MeasureType.ContainerCount);

			var servicesWithoutContainer = new List<JobServiceInfo>();
			var linesInRepository = new HashSet<FastLine>(similarCharges);

			foreach (var service in services)
			{
				if (service.Container.IsEmpty)
				{
					servicesWithoutContainer.Add(service);
				}
				else
				{
					var allLinesForService = serviceRater.GetLinesForService(service);
					var lookupInRepository = allLinesForService.ToLookup(x => linesInRepository.Contains(x));
					var repositoryLinesForService = lookupInRepository[true];
					var measurableLinesList = repositoryLinesForService.Where(IsLineMeasurableAgainstSimilarCharges).ToList();
					if (measurableLinesList.Count > 0)
					{
						allMeasurableLines.UnionWith(measurableLinesList);
						// pick the lines that match the container of the service (ContainerType, commodity, etc)
						if (containersWithDimensions != null)
						{
							var containerForService = containersWithDimensions
								.OfType<IRateableContainer>()
								.FirstOrDefault(pt => pt.ContainerPK == service.Container);
							if (containerForService != null)
							{
								var bestMatches = FindBestMatches(parameters, repositoryLinesForService, containersWithDimensions, containerForService, null, true);
								allBestMatches.UnionWith(bestMatches);

								var otherLinesForService = lookupInRepository[false];
								bestMatches.AddRange(otherLinesForService);
								serviceRater.SetBestLinesForService(service, bestMatches);
							}
							else
							{
								// No container matched the service - probably this is an LCL job
								servicesWithoutContainer.Add(service);
							}
						}
						else
						{
							// Job doesn't have any containers - should be impossible if the service has one
							servicesWithoutContainer.Add(service);
						}
					}
				}
			}

			if (servicesWithoutContainer.Any())
			{
				foreach (var serviceGroup in servicesWithoutContainer.GroupBy(x => x.IsContractorCreditor ? (x.Contractor.PK, x.ServiceId) :  (ZGuid.Empty, x.ServiceId)))
				{
					var repositoryLinesForThisGroup = serviceGroup.SelectMany(service => serviceRater.GetLinesForService(service))
						.Distinct()
						.Where(x => linesInRepository.Contains(x))
						.ToList();
					var measurableLinesList = repositoryLinesForThisGroup.Where(IsLineMeasurableAgainstSimilarCharges);
					allMeasurableLines.UnionWith(measurableLinesList);

					allBestMatches.UnionWith(FindBestMatches(parameters, repositoryLinesForThisGroup, null, null, null, true));

					foreach (var service in serviceGroup)
					{
						var serviceLines = serviceRater.GetLinesForService(service);
						serviceRater.SetBestLinesForService(service, serviceLines.Where(x => !allMeasurableLines.Contains(x) || allBestMatches.Contains(x)));
					}
				}
			}

			rateLinesRepository.Remove(x => x.Line.TL_AC == chargeCode.PK
				&& allMeasurableLines.Contains(x)
				&& !allBestMatches.Contains(x), (NoResString)"failed similarity check"); // log message, subject to change, more for support people as of now
			var bestMatchesToAdd = allBestMatches.Where(x => rateLinesRepository.GetLines().All(y => y.Line.PK != x.Line.PK)).ToArray();
			rateLinesRepository.AddSilently(bestMatchesToAdd);
		}

		bool PartsHaveRateDimensions(IRateablePartList partList)
			=> comparers.Any(x => x.HasDimension(partList));

		/// <summary>
		/// Returns lines that best match the part (have similar attributes).
		/// If there is more than one, they are checked to see if they conflict (e.g., two flat rates with different amounts).
		/// Conflicting rate lines result in a error message rate line in the output.
		/// </summary>
		List<FastLine> FindBestMatches(AutoRatingCalculatorParameters parameters,
			IEnumerable<FastLine> linesList,
			IRateablePartList partList,
			IRateablePart part,
			PointMatchingLog pointMatchingLog,
			bool isServiceAutoRating)
		{
			var result = new List<FastLine>();
			int lineCount = linesList.Count();
			if (lineCount > 0)
			{
				var criteria = parameters.Criteria;
				var isCosting = linesList.First().IsCostRate();
				var legResults = new List<FastLine>(lineCount);
				var similarityList = new List<long>(lineCount);
				var remover = new OverriddenRateLineRemover(criteria, isCosting);
				for (var leg = 0; leg < 3; leg++)
				{
					var bestMatchSimilarity = long.MinValue;
					legResults.Clear();
					similarityList.Clear();
					foreach (var legLine in linesList.Where(x => x.GetFreightLeg() == leg))
					{
						var similarity = GetSimilarity(legLine, partList, part, pointMatchingLog);
						if (similarity >= 0)
						{
							// Future optimization - lines that are OverriddenRateLineRemover.IsComparable have already been compared with result == 0 (in RemoveOverriddenRates)
							// so no need to compare them again.
							var firstCompareResult = legResults.Count > 0
								? remover.Compare(legLine, legResults[0], out _)
								: 0;
							if (firstCompareResult > 0 || firstCompareResult == 0 && similarity >= bestMatchSimilarity)
							{
								RemoveExistingLowerMatches(remover, legResults, similarityList, legLine, similarity);
								if (legResults.Count > 0 && (firstCompareResult > 0 || similarity > bestMatchSimilarity))
								{
									// The new line is better than the first line which needs removing
									legResults.RemoveAt(0);
									similarityList.RemoveAt(0);
								}
								legResults.Add(legLine);
								similarityList.Add(similarity);
								bestMatchSimilarity = similarity;
							}
						}
					}

					ManageConflictingRates(parameters, legResults, partList, isServiceAutoRating);
					result.AddRange(legResults);
				}
			}

			return result;
		}

		void ManageConflictingRates(AutoRatingCalculatorParameters parameters, List<FastLine> legResults, IRateablePartList partList, bool isServiceAutoRating)
		{
			for (var i = 0; i < legResults.Count - 1; i++)
			{
				for (var j = legResults.Count - 1; j > i; j--)
				{
					if (!AreRatelinesCompatibleForSimultaneousUsage(legResults[i], legResults[j], partList, isServiceAutoRating))
					{
						if (LinesHaveDifferentCalculators(parameters, legResults[i].Line, legResults[j].Line))
						{
#if DEBUG
							legResults.Sort((x, y) => (x.ParentRateEntry.Container != null ? x.ParentRateEntry.Container.RC_Code : ZString.Empty).CompareTo((y.ParentRateEntry.Container != null ? y.ParentRateEntry.Container.RC_Code : ZString.Empty))); //to stop intermittent test failures
#endif
							var messageRateLine = CreateMessageRateLine(legResults[0].Line,
								Res.GetString("a047b455-4503-492c-a9d8-d5f1e89d89c7", "Charge cannot be calculated due to conflicting rates found."));
							var newFastLine = parameters.Criteria.Cache.GetOrCreateFastLine(messageRateLine);
							legResults.Clear();
							legResults.Add(newFastLine);
							return;
						}
						else
						{
							legResults.RemoveAt(j);
						}
					}
				}
			}
		}

		static bool LinesHaveDifferentCalculators(AutoRatingCalculatorParameters parameters, IRateLine line1, IRateLine line2)
		{
			return !(line1.Calculator.Equals(line2.Calculator) && line1.Calculator.GetBaseCalculator(parameters).Equals(line2.Calculator.GetBaseCalculator(parameters)));
		}

		static IRateLine CreateMessageRateLine(IRateLine parentLine, string message)
		{
			if (parentLine.ParentRateEntry.ParentRatingHeader.IsWiseCostRate())
			{
				var parentWiseLine = parentLine as WiseLine;
				var cloneRateLine = (WiseLine)parentWiseLine.Clone();

				cloneRateLine.TL_RateCalculator = NoteCalculator.Code;

				// Note Calculator requires 'SHW', otherwise Calculator.FindOrAddRateLineItem would throw NullReferenceException.
				// For RateLine, it is automatically created when setting calculator
				cloneRateLine.ChildRateLineItems = new[]
				{
					new WiseLineItem(cloneRateLine, Calculator.Items.ShowOnBillingWithoutPrefix, ZString.Empty, 0m, ZString.Empty, 0m, 0m, false),
					new WiseLineItem(cloneRateLine, ZString.Empty, message, 0m, ZString.Empty, 0m, 0m, false)
				};

				return cloneRateLine;
			}
			else
			{
				var parentRateLine = parentLine as RateLine;

				var notSavedFactory = new ReadOnlyBusinessObjectFactory { NameForDebugging = "Rating Message Line Not Saved Factory" }; // Factory name for debugging

				notSavedFactory.ImportFromAnotherFactorySafe(parentRateLine.Parent.Parent);
				var entry = notSavedFactory.ImportFromAnotherFactorySafe(parentRateLine.Parent);

				var cloneRateLine = parentRateLine.Clone(entry.RateLines);

				if (parentRateLine.Parent.Container != null)
				{
					notSavedFactory.ImportFromAnotherFactorySafe(parentRateLine.Parent.Container);
				}

				if (parentRateLine.Parent.Parent.Header != null)
				{
					notSavedFactory.ImportFromAnotherFactorySafe(parentRateLine.Parent.Parent.Header);
				}

				cloneRateLine.TL_RateCalculator = NoteCalculator.Code;
				cloneRateLine.RateLineItems.AddNew().TM_Text = message;

				return cloneRateLine;
			}
		}

		/// <summary>
		/// The given line matches the job and is at least as good a match as the first line that previously matched.
		/// Remove any other lines after the first that are less of a match.
		/// A line is less of a match if it compares lower (via the remover) or compares the same and has lower similarity.
		/// </summary>
		/// <param name="remover"></param>
		/// <param name="legResults">lines that previously matched</param>
		/// <param name="similarityList">similarity for legResults line</param>
		/// <param name="line">new line that matched</param>
		/// <param name="similarity">similarity of new line</param>
		static void RemoveExistingLowerMatches(OverriddenRateLineRemover remover,
			List<FastLine> legResults,
			List<long> similarityList,
			FastLine line, long similarity)
		{
			for (var i = legResults.Count - 1; i >= 1; i--)
			{
				var compareResult = remover.Compare(line, legResults[i], out _);

				if (compareResult > 0 || (compareResult == 0 && similarityList[i] < similarity))
				{
					legResults.RemoveAt(i);
					similarityList.RemoveAt(i);
				}
			}
		}

		/// <summary>
		/// Determines if the given line requires measures to calculate charges.
		/// </summary>
		static bool IsLineRequiringMeasures(FastLine line, AutoRatingCalculatorParameters parameters)
		{
			return line.ChargeCode.AC_ChargeSubGroup.IsEmpty
				&& line.Calculator.GetBaseCalculator(parameters).IsMeasureTypeMatchApplicable;
		}

		/// <summary>
		/// Lines measureable against similar charges can be removed based on their similarity with the job.
		/// Lines not measureable are not removed.
		///
		/// For example, percentage calculators are not measurable against similar charges.
		/// So if there are two percentage calculator rates, one with a 40GP container type and one with a 40XX container type (in the same class as 40GP)
		/// they will both apply to a job with only a 40GP container. The 40XX rate is less similar since it is not an exact match, but it is not removed.
		///
		/// The concept is very close to IsLineRequiringMeasures.
		/// It is expected that any calculator that is not measurable against similar charges
		/// also does not require measures. However the reverse is not true.
		/// There are calculators that don't require measures, like the flat calculator, that are measurable against similar charges.
		/// For the case above of 40GP and 40XX, the 40XX rate with a flat calculator would be removed.
		///
		/// The actual product level rule deciding whether a calculator that requires measures is measurable or not against similar charges is yet to be described.
		/// It seems to be case by case so far.
		/// </summary>
		static bool IsLineMeasurableAgainstSimilarCharges(FastLine fastLine)
		{
			var line = fastLine.Line;
			var result = line.RateCalculatorType != CalculatorType.Minimum;
			result = result && !line.IsContainerSpotRate();
			result = result && !line.ParentRateEntry.IsSpotEntry;
			result = result && !line.IsInclusive();

			return result;
		}

		#region RateLine Comparing/Similarity

		bool AreRatelinesCompatibleForSimultaneousUsage(FastLine rateLine1, FastLine rateLine2, IRateablePartList partList, bool isServiceAutoRating)
		{
			if (isServiceAutoRating)
			{
				// Some special case exclusive for services autorating
				var hasDifferentServices = rateLine1.Line.TL_AC != rateLine2.Line.TL_AC;
				var hasDifferentContainerTypesPerService = rateLine1.ParentRateEntry.TI_RC != rateLine2.ParentRateEntry.TI_RC;

				return hasDifferentServices || hasDifferentContainerTypesPerService;
			}

			// Percentage calculator normally depends on other charges rather than measurable value, so, if we find a
			// conflict between two percentage lines, we keep both. Then, on calculation stage they will or won't calculate
			// based on matching charges.
			if (rateLine1.Calculator is PercentageCalculator percentage1 &&
				rateLine2.Calculator is PercentageCalculator percentage2 &&
				CalculatorConstants.Text.ApplyTo.Charges.All.Any(v => v == percentage1.ValueApplyTo || percentage1.ValueApplyTo == Calculator.Items.Value.Multiple) &&
				CalculatorConstants.Text.ApplyTo.Charges.All.Any(v => v == percentage2.ValueApplyTo || percentage2.ValueApplyTo == Calculator.Items.Value.Multiple))
			{
				return true;
			}

			bool hasAnyDimensions = partList != null && comparers.Any(x => x.HasDimension(partList));

			var measures = Criteria.RateableMeasures;
			foreach (var comparer in comparers)
			{
				if (!hasAnyDimensions || comparer.HasDimension(partList))
				{
					if (!comparer.AreRatelinesCompatibleForSimultaneousUsage(measures, rateLine1, rateLine2))
					{
						return false;
					}
				}
			}

			return true;
		}

		internal long GetSimilarity(FastLine line, IRateablePartList partList, IRateablePart part, PointMatchingLog pointMatchingLog = null)
		{
			var measures = Criteria.RateableMeasures;

			long totalSimilarity = 0;
			foreach (var comparer in comparers)
			{
				var similarity = comparer.GetSimilarity(line, measures, partList, part, pointMatchingLog);
				if (similarity == Similarity.None)
				{
					return (long)Similarity.None;
				}

				totalSimilarity = totalSimilarity * SimilarityBase + (long)similarity;
			}

			return totalSimilarity;
		}

		/// <summary>
		/// Base multiplier so separate similarity numbers can be combined like digits into a bigger number.
		/// E.g., if warehouse similarity is 9 and container type is 0, then the overall similarity is 90 (9 * Base + 0).
		/// This combined number can be compared with other combined numbers to pick the most similar with a single comparison.
		/// </summary>
		public const int SimilarityBase = (int)Similarity.Exact + 1;

		void CreateComparers()
		{
			// Comparers in order of importance of dimension.
			// If a rate exactly matches a job on a higher rank comparer (say warehouse), but is only a generic match on a lower rank (say commodity)
			// then it is a better match than a rate that is a generic match on warehouse and an exact match on commodity
			// since exact matches on warehouse are more important than exact matches on commodity.

			comparers = new List<IDimensionComparer>
			{
				new WarehouseComparer(),
				new ContainerTypeComparer(),
			};

			if (!Criteria.IsLooseRateSearchForCarrierConnect)
			{
				comparers.Add(new ContainerIsNonOperatingReeferComparer());
			}

			comparers.AddRange(new IDimensionComparer[]
			{
				new ProductComparer(),
				new CommodityComparer(),
				new PalletizedComparer(),
				new ContainerOwnershipComparer(),
				new ChargeGroupToUseComparer(),
				new YardUnitTypeComparer(),
				new YardUnitLoadComparer(),
				new YardUnitClientComparer(),
			});
		}

		List<IDimensionComparer> comparers;

		#endregion

		#if DEBUG
		// Ranks here are zero based and must match the order of the comparers.
		public const long WarehouseSimilarityRank = 10;
		public const long ContainerTypeSimilarityRank = 9;
		public const long ContainerIsNonOperatingReeferComparer = 8;
		public const long ProductSimilarityRank = 7;
		public const long CommoditySimilarityRank = 6;
		public const long PalletizedSimilarityRank = 5;
		public const long ContainerOwnershipSimilarityRank = 4;
		public const long ChargeGroupToUseSimilarityRank = 3;
		public const long YardUnitTypeSimilarityRank = 2;
		public const long YardUnitLoadSimilarityRank = 1;
		public const long YardUnitClientSimilarityRank = 0;
		#endif

		#endregion
	}
}
