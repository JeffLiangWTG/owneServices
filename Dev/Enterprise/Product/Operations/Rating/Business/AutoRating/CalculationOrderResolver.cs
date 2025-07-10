using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.Business
{
	internal class CalculationOrderResolver
	{
		/// <summary>
		/// Determines the order the lines in parameters.LinesToCalculate should be calculated.
		/// Calculators that need the output of other calculators come later.
		/// The list in the given parameters can optionally be modified to be in order.
		/// </summary>
		/// <param name="sort">if true parameters.LinesToCalculate is sorted into order</param>
		public CalculationOrderResolver(AutoRatingCalculatorParametersWithoutFilter parameters, bool sort)
		{
			if (parameters != null)
			{
				criteria = parameters.Criteria;
				var dependentLines = new List<IRateLine>();
				foreach (var line in parameters.LinesToCalculate)
				{
					var baseLine = GetBaseLine(parameters, line);
					if (IsDependent(baseLine))
					{
						dependentLines.Add(baseLine);
					}
				}
				ProcessLines(dependentLines);
				CreateCalculationOrder();

				if (sort)
				{
					int CompareLines(FastLine xFast, FastLine yFast)
						=> CompareRateLines(parameters, xFast, yFast);

					parameters.LinesToCalculate.Sort(CompareLines);
				}
			}
		}

		public CalculationOrderResolver(IEnumerable<IRateLine> lines)
		{
			lines.AddFetchHintsToLoadAllRateLineItems();

			var dependentLines = lines.Where(x => !(x.UsesCompanyTariffOrCostBasedCalculator())).Where(IsDependent).ToList();
			ProcessLines(dependentLines);
		}

		public void ProcessLines(List<IRateLine> dependentLines)
		{
			FillDictionaries(dependentLines, false);
			var conflictingLines = conflictsUnresolvedPriorToSequenceUse.Keys.ToList();
			FillDictionaries(conflictingLines, true);
		}

		public bool AreInConflict(IRateLineItem applyToItem, AccChargeCode chargeCode)
		{
			var result = false;

			List<IRateLine> conflictingLines;
			if (conflictsImpossibleToResolve.TryGetValue(applyToItem.ParentRateLine, out conflictingLines))
			{
				result = conflictingLines.Any(line => line.TL_AC == chargeCode.PK);
			}

			if (!result && conflictsUnresolvedPriorToSequenceUse.TryGetValue(applyToItem.ParentRateLine, out conflictingLines))
			{
				var conflictingLine = conflictingLines.FirstOrDefault(line => line.TL_AC == chargeCode.PK);
				result = conflictingLine != null && CompareBySequence(applyToItem.ParentRateLine, conflictingLine) < 0;
			}

			return result;
		}

		public bool IsItemApplicableToChargeCode(IRateLineItem applyToItem, AccChargeCode chargeCode)
		{
			if (chargeCode != null && applyToItem != null && (applyToItem.RateOperatorIsApplyToOrMNT() || applyToItem.RateOperatorIsRatePickRule()))
			{
				switch (applyToItem.TM_Text)
				{
					case HighestRateCalculator.Items.AsFreightedHighestRateWhenMin:
					case HighestRateCalculator.Items.AsFreightedDontApplyWhenMin:
						return chargeCode.PK == Env.Registry.FreightChargeCode;

					case CalculatorConstants.Text.AllCharges:
					case CalculatorConstants.Text.MIN_Job:
						return true;

					case CalculatorConstants.Text.MIN_ChargeCode:
						return applyToItem.ParentRateLine.ChargeCode.PK == chargeCode.PK;

					case CalculatorConstants.Text.ChargeCode:
						return applyToItem.TM_AC == chargeCode.PK;

					case CalculatorConstants.Text.OriginCharges:
						return chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Origin;

					case CalculatorConstants.Text.FreightCharges:
						return chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Freight;

					case CalculatorConstants.Text.DestinationCharges:
						return chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Destination;

					case CalculatorConstants.Text.LoadingCharges:
						return chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Loading;

					case CalculatorConstants.Text.OriginCustomsBrokerageCharges:
						return chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.OriginBrokerage;

					case CalculatorConstants.Text.CustomsBrokerageCharges:
						return chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Brokerage;

					case CalculatorConstants.Text.UnloadingCharges:
						return chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Unloading;

					case Calculator.Items.Value.DisbursementApplyToTypes.Disbursements:

						if (criteria == null)
						{
							return (chargeCode.GetChargeType(null, Directions.Import).AN_ChargeType == Constants.ChargeType.Disbursement || chargeCode.GetChargeType(null, Directions.Export).AN_ChargeType == Constants.ChargeType.Disbursement);
						}

						return chargeCode.GetChargeType(criteria.ConsumerType, criteria.JobDirection).AN_ChargeType == Constants.ChargeType.Disbursement;

					case Calculator.Items.Value.DisbursementApplyToTypes.CustomsDisbursement:
						return chargeCode.PK == RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value || chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.CustomsDuty;
				}
			}

			return false;
		}

		public bool IsItemApplicableToInfo(IRateLineItem applyToItem, AutoRateInfo info)
		{
			return info != null && IsItemApplicableToChargeCode(applyToItem, info.ChargeCode);
		}

		public bool IsItemApplicableToLine(IRateLineItem applyToItem, IRateLine line)
		{
			return line != null && IsItemApplicableToChargeCode(applyToItem, line.ChargeCode);
		}

		public bool TryGetConflicts(IRateLine line, out List<IRateLine> conflicts)
		{
			return conflictsImpossibleToResolve.TryGetValue(line, out conflicts);
		}

		readonly Dictionary<IRateLine, List<IRateLine>> conflictsUnresolvedPriorToSequenceUse = new Dictionary<IRateLine, List<IRateLine>>();
		readonly Dictionary<IRateLine, List<IRateLine>> conflictsImpossibleToResolve = new Dictionary<IRateLine, List<IRateLine>>();
		readonly Dictionary<IRateLine, List<IRateLine>> safeDictionary = new Dictionary<IRateLine, List<IRateLine>>();
		readonly Dictionary<ZGuid, int> calculationOrder = new Dictionary<ZGuid, int>();
		readonly RatingCriteria criteria;

		void FillDictionaries(List<IRateLine> dependentLines, bool useSequence)
		{
			var dependencyDictionary = new Dictionary<IRateLine, List<IRateLine>>();

			foreach (var line in dependentLines)
			{
				if (!dependencyDictionary.ContainsKey(line))
				{
					dependencyDictionary.Add(line, GetLinesNeededForCalculation(line, dependentLines, useSequence));
					linesAlreadyChecked.Clear();
				}
			}

			var conflictsDictionary = useSequence ? conflictsImpossibleToResolve : conflictsUnresolvedPriorToSequenceUse;

			foreach (var pair in dependencyDictionary)
			{
				if (pair.Value.Count == 0)
				{
					if (!safeDictionary.ContainsKey(pair.Key))
					{
						safeDictionary.Add(pair.Key, new List<IRateLine>());
					}
				}
				else
				{
					foreach (var line in pair.Value)
					{
						var isConflict = dependencyDictionary[line].Contains(pair.Key);
						var dictionary = isConflict ? conflictsDictionary : safeDictionary;

						List<IRateLine> list;
						if (!dictionary.TryGetValue(pair.Key, out list))
						{
							dictionary.Add(pair.Key, (list = new List<IRateLine>()));
						}

						list.Add(line);
					}
				}
			}
		}

		bool IsDependent(IRateLine baseLine)
		{
			return baseLine.ChildRateLineItems.Any(item =>
				item.RateOperatorIsApplyToOrMNT()
				&& (item.TM_Text == CalculatorConstants.Text.ChargeCode
					|| item.TM_Text == CalculatorConstants.Text.OriginCharges
					|| item.TM_Text == CalculatorConstants.Text.FreightCharges
					|| item.TM_Text == CalculatorConstants.Text.DestinationCharges
					|| item.TM_Text == CalculatorConstants.Text.LoadingCharges
					|| item.TM_Text == CalculatorConstants.Text.OriginCustomsBrokerageCharges
					|| item.TM_Text == CalculatorConstants.Text.CustomsBrokerageCharges
					|| item.TM_Text == CalculatorConstants.Text.UnloadingCharges
					|| item.TM_Text == Calculator.Items.Value.DisbursementApplyToTypes.Disbursements
					|| item.TM_Text == Calculator.Items.Value.DisbursementApplyToTypes.CustomsDisbursement
					|| item.TM_Text == CalculatorConstants.Text.MIN_Job
					|| item.TM_Text == CalculatorConstants.Text.AllCharges)
				|| item.RateOperatorIsRatePickRule()
				&& (item.TM_Text == HighestRateCalculator.Items.AsFreightedHighestRateWhenMin
					|| item.TM_Text == HighestRateCalculator.Items.AsFreightedDontApplyWhenMin))
				|| baseLine.Calculator is MinimumCalculator;
		}

		bool IsJobMinimum(IRateLine line)
		{
			var calc = line.Calculator as MinimumCalculator;
			return calc != null && calc.IsJobMinimum;
		}

		List<IRateLine> GetLinesNeededForCalculation(IRateLine line, IEnumerable<IRateLine> lines, bool useSequence, int sequence = 0)
		{
			sequence = Math.Max(sequence, line.GetCalculationOrder());

			var result = new List<IRateLine>();
			linesAlreadyChecked.Add(line);

			foreach (var lineToCheck in lines.Where(x => x != line && x.TL_AC != line.TL_AC))
			{
				if (IsDependentFrom(line, lineToCheck, useSequence, sequence))
				{
					result.Add(lineToCheck);
					if (!linesAlreadyChecked.Contains(lineToCheck))
					{
						foreach (var lineToAdd in GetLinesNeededForCalculation(lineToCheck, lines, useSequence, sequence))
						{
							if (line.TL_AC != lineToAdd.TL_AC)
							{
								result.Add(lineToAdd);
							}
						}
					}
				}
			}

			return result;
		}

		readonly List<IRateLine> linesAlreadyChecked = new List<IRateLine>();

		bool IsDependentFrom(IRateLine dependant, IRateLine independant, bool useSequence, int sequence)
		{
			if (dependant.IsCostRate() && !independant.IsCostRate())
			{
				return false;
			}

			if (!dependant.IsCostRate() && independant.IsCostRate())
			{
				return true;
			}

			if (IsJobMinimum(dependant) && !IsJobMinimum(independant))
			{
				return true;
			}

			if (!IsJobMinimum(dependant) && IsJobMinimum(independant))
			{
				return false;
			}

			if (useSequence)
			{
				var dependantSequence = Math.Max(sequence, dependant.GetCalculationOrder());

				if (dependantSequence > 0)
				{
					if (independant.GetCalculationOrder() == 0 || dependantSequence < independant.GetCalculationOrder())
					{
						return false;
					}
				}
			}

			foreach (var item in dependant.ChildRateLineItems)
			{
				if (IsItemApplicableToLine(item, independant))
				{
					return true;
				}
			}

			return false;
		}

		int CompareRateLines(AutoRatingCalculatorParameters parameters, FastLine xFast, FastLine yFast)
		{
			var x = xFast.Line;
			var y = yFast.Line;
			var baseX = GetBaseLine(parameters, xFast);
			var baseY = GetBaseLine(parameters, yFast);

			if (baseX.IsInclusive() != baseY.IsInclusive())
			{
				return baseY.IsInclusive().CompareTo(baseX.IsInclusive());
			}

			if (baseX.IsCostRate() != baseY.IsCostRate())
			{
				return baseY.IsCostRate().CompareTo(baseX.IsCostRate());
			}

			if (IsJobMinimum(x) != IsJobMinimum(y))
			{
				return IsJobMinimum(x).CompareTo(IsJobMinimum(y));
			}

			var xOrder = GetOrder(baseX);
			var yOrder = GetOrder(baseY);

			if (xOrder > 1 && yOrder > 1)
			{
				var sequenceComparison = CompareBySequence(baseX, baseY);

				if (sequenceComparison != 0)
				{
					return sequenceComparison;
				}
			}

			if (xOrder == yOrder && baseX.Calculator is MinimumCalculator != baseY.Calculator is MinimumCalculator)
			{
				return (baseX.Calculator is MinimumCalculator).CompareTo(baseY.Calculator is MinimumCalculator);
			}

			return xOrder - yOrder;
		}

		int CompareBySequence(IRateLine x, IRateLine y)
		{
			List<IRateLine> list;

			if ((x.GetCalculationOrder() != 0 || y.GetCalculationOrder() != 0) && conflictsUnresolvedPriorToSequenceUse.TryGetValue(x, out list) && list.Contains(y))
			{
				if (x.GetCalculationOrder() == 0)
				{
					return 1;
				}

				if (y.GetCalculationOrder() == 0)
				{
					return -1;
				}

				return x.GetCalculationOrder() - y.GetCalculationOrder();
			}

			return 0;
		}

		int GetOrder(IRateLine baseLine)
		{
			if (calculationOrder.ContainsKey(baseLine.TL_AC))
			{
				return calculationOrder[baseLine.TL_AC];
			}

			if (conflictsImpossibleToResolve.Keys.FirstOrDefault(conflictingLine => conflictingLine.TL_AC == baseLine.TL_AC) != null)
			{
				return 1;
			}

			return 0;
		}

		void CreateCalculationOrder()
		{
			var loopAgain = true;

			while (loopAgain)
			{
				loopAgain = false;

				foreach (var pair in safeDictionary)
				{
					if (calculationOrder.ContainsKey(pair.Key.TL_AC))
					{
						continue;
					}

					var allLinesNeededForThisAlreadyOrdered = pair.Value.Count == 0 || pair.Value.All(line => calculationOrder.ContainsKey(line.TL_AC));

					if (allLinesNeededForThisAlreadyOrdered)
					{
						calculationOrder.Add(pair.Key.TL_AC, calculationOrder.Count + 2);
						loopAgain = true;
					}
				}
			}
		}

		static IRateLine GetBaseLine(AutoRatingCalculatorParameters parameters, FastLine line)
		{
			return line.Calculator.GetBaseCalculator(parameters).Line;
		}
	}
}

