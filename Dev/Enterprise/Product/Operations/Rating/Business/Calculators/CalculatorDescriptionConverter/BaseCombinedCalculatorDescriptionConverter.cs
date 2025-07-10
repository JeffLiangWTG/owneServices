using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public class BaseCombinedCalculatorDescriptionConverter<T> : ICalculatorDescriptionConverter
		where T : BaseCombinedCalculator
	{
		protected T calculator;

		public BaseCombinedCalculatorDescriptionConverter(T calculator)
		{
			this.calculator = calculator;
		}

		public virtual string Convert()
		{
			var basOperatorLinesDescription = CreateBasOperatorLinesDescription();
			var minusPlusOperatorLinesDescription = CreateMinusPlusOperatorLinesDescription();
			var nonBasMinusPlusOperatorLinesDescription = CreateNonBasMinusPlusOperatorLinesDescription();

			if (!string.IsNullOrEmpty(basOperatorLinesDescription) && (!string.IsNullOrEmpty(minusPlusOperatorLinesDescription) || !string.IsNullOrEmpty(nonBasMinusPlusOperatorLinesDescription)))
			{
				basOperatorLinesDescription += " +";
			}
			if (!string.IsNullOrEmpty(nonBasMinusPlusOperatorLinesDescription))
			{
				nonBasMinusPlusOperatorLinesDescription += ".";
			}

			return string.Join(System.Environment.NewLine, new[] { basOperatorLinesDescription, minusPlusOperatorLinesDescription, nonBasMinusPlusOperatorLinesDescription }.Where(x => !string.IsNullOrEmpty(x)));
		}

		#region BAS lines description

		string CreateBasOperatorLinesDescription()
		{
			return CreateLineDescriptionFromBasicOperator(Calculator.Items.Operator.BAS);
		}

		#endregion

		#region Minus Plus lines description

		string CreateMinusPlusOperatorLinesDescription()
		{
			var sortedBreakLines = GetSortedBreaks(calculator.Line.ChildRateLineItems).ToList();

			if (sortedBreakLines.Count == 0)
			{
				return string.Empty;
			}

			var descriptionLines = new List<string>();

			if (sortedBreakLines[0].TM_Type == Calculator.Items.Operator.Plus)
			{
				if (sortedBreakLines.Count == 1)
				{
					// When there is only 1 Plus operator line without Minus operator line, convert it without any break description
					return CreateMinusPlusOperatorLineDescription(sortedBreakLines[0], false);
				}

				// When there are multiple Plus operator lines without Minus operator line, insert a minus description at the beginning
				descriptionLines.Add(CreateMinusOperatorLineDescriptionFromPlusOperatorLine(sortedBreakLines[0]));
			}

			descriptionLines.AddRange(
				sortedBreakLines
					.Select(line => CreateMinusPlusOperatorLineDescription(line))
					.Where(description => !string.IsNullOrEmpty(description))
					.ToList());

			return string.Join(System.Environment.NewLine, descriptionLines);
		}

		string CreateMinusOperatorLineDescriptionFromPlusOperatorLine(IRateLineItem line)
		{
			return $"{CreateBreakDescription(Calculator.Items.Operator.Minus, line.TM_Break)} {CreateFlatAmountWithRateDescription(line)}";
		}

		string CreateMinusPlusOperatorLineDescription(IRateLineItem line, bool withBreak = true)
		{
			return withBreak ? $"{CreateBreakDescription(line.TM_Type, line.TM_Break)} {CreateFlatAmountWithRateDescription(line)}" : $"{CreateFlatAmountWithRateDescription(line)}";
		}

		// ie. "Unit portion <= 5 Day(s)"
		string CreateBreakDescription(ZString type, ZDecimal breakValue) => $"{CreateCumulativeDescription()} {CreateOperatorDescription(type)} {breakValue.Normalize()} {calculator.UnitDescriptionInternal(calculator.BreakUnit, addPlural: true)}";

		// ie. "Base USD 20 + USD 10/Day"
		string CreateFlatAmountWithRateDescription(IRateLineItem line) => string.Join(" + ", new[] { CreateFlatAmountDescription(line), CreateRateDescription(line) }.Where(x => !string.IsNullOrEmpty(x)));

		// ie. "Base USD 20"
		string CreateFlatAmountDescription(IRateLineItem line) => line.TM_FlatAmount != 0 ? Res.GetString("98bd6723-a7b0-4b73-a263-aced8e26b62e", "Base {0} {1}", line.ParentRateLine.TL_RX_NKCurrency, line.TM_FlatAmount.Normalize()) : string.Empty; 

		// ie. "USD 10/Day"
		string CreateRateDescription(IRateLineItem line) => line.TM_RelevantValue != 0 ? $"{line.ParentRateLine.TL_RX_NKCurrency} {line.TM_RelevantValue.Normalize()}/{GetUnitDescription(line)}" : string.Empty;

		string CreateCumulativeDescription() => calculator.IsAccumulated ? Res.GetString("5354a7fd-4cd8-472b-a4c3-088b69c2974d", "Unit portion") : Res.GetString("1779dc6d-77cd-4d18-9955-e0cd6008f5d2", "Unit total");

		#endregion

		#region Non Bas Minus Plus lines description

		protected virtual string CreateNonBasMinusPlusOperatorLinesDescription()
		{
			var nonBasMinusPlusOperatorLinesDescriptions = new List<string>();

			var untOperatorLineDescription = CreateLineDescriptionFromBasicOperator(Calculator.Items.Operator.UNT);
			if (!string.IsNullOrEmpty(untOperatorLineDescription))
			{
				nonBasMinusPlusOperatorLinesDescriptions.Add(untOperatorLineDescription);
			}

			var minOperatorLineDescription = CreateLineDescriptionFromBasicOperator(Calculator.Items.Operator.MIN);
			if (!string.IsNullOrEmpty(minOperatorLineDescription))
			{
				nonBasMinusPlusOperatorLinesDescriptions.Add(minOperatorLineDescription);
			}

			var maxOperatorLineDescription = CreateLineDescriptionFromBasicOperator(Calculator.Items.Operator.MAX);
			if (!string.IsNullOrEmpty(maxOperatorLineDescription))
			{
				nonBasMinusPlusOperatorLinesDescriptions.Add(maxOperatorLineDescription);
			}

			var higherBreakLowerRateDescription = CreateHigherBreakLowerRateDescription();
			if (!string.IsNullOrEmpty(higherBreakLowerRateDescription))
			{
				nonBasMinusPlusOperatorLinesDescriptions.Add(higherBreakLowerRateDescription);
			}

			return string.Join(", ", nonBasMinusPlusOperatorLinesDescriptions);
		}

		string CreateLineDescriptionFromBasicOperator(ZString lineOperator)
		{
			var line = calculator.Line.FindRateLineItem(lineOperator);
			if (line == null)
			{
				return string.Empty;
			}

			switch (lineOperator)
			{
				case Calculator.Items.Operator.MIN:
				case Calculator.Items.Operator.MAX:
					if (line.TM_Break > 0)
					{
						return CreateLineDescriptionFromBreak(line);
					}
					if (line.TM_RelevantValue > 0)
					{
						return CreateLineDescriptionFromRelevantValue(line);
					}
					break;
				case Calculator.Items.Operator.BAS:
					if (line.TM_RelevantValue > 0)
					{
						return CreateLineDescriptionFromRelevantValue(line);
					}
					break;
				case Calculator.Items.Operator.UNT:
					if (line.TM_RelevantValue > 0)
					{
						return CreateUntLineDescriptionFromRelevantValue(line);
					}
					break;
			}

			return string.Empty;
		}

		// ie. "MIN. 5 Day(s)"
		string CreateLineDescriptionFromBreak(IRateLineItem line) => $"{CreateOperatorDescription(line.TM_Type)} {line.TM_Break.Normalize()} {calculator.UnitDescriptionInternal(calculator.BreakUnit, addPlural: true)}";

		// ie. "MIN. Rate USD 100"
		string CreateLineDescriptionFromRelevantValue(IRateLineItem line) => Res.GetString("8e00ea41-89fc-4310-865e-5aa56b500f83", "{0} Rate {1} {2}", CreateOperatorDescription(line.TM_Type), line.ParentRateLine.TL_RX_NKCurrency, line.TM_RelevantValue.Normalize());

		// ie. "USD 3/Day"
		string CreateUntLineDescriptionFromRelevantValue(IRateLineItem line) => $"{line.ParentRateLine.TL_RX_NKCurrency} {line.TM_RelevantValue.Normalize()}/{GetUnitDescription(line)}";

		string CreateHigherBreakLowerRateDescription() => calculator.UseHigherChargeableLowerRateRule ? Res.GetString("97dfb69f-fd84-4464-b09a-5800ddc7bb0e", "Higher Break Lower Rate is applied") : string.Empty;

		string GetUnitDescription(IRateLineItem line) => (calculator.BreakUnit != line.ParentRateLine.TL_WeightVolume) ? $"{calculator.UnitDescriptionInternal(calculator.BreakUnit, addPlural: false)}/{calculator.UnitDescriptionInternal(line.ParentRateLine.TL_WeightVolume, addPlural: false)}" : $"{calculator.UnitDescriptionInternal(calculator.BreakUnit, addPlural: false)}";

		#endregion

		string CreateOperatorDescription(ZString lineOperator)
		{
			switch (lineOperator)
			{
				case Calculator.Items.Operator.MIN:
					return "MIN.";
				case Calculator.Items.Operator.MAX:
					return "MAX.";
				case Calculator.Items.Operator.BAS:
					return "BAS.";
				case Calculator.Items.Operator.Minus:
					return calculator.UseInclusiveBreaks ? "<=" : "<";
				case Calculator.Items.Operator.Plus:
					return calculator.UseInclusiveBreaks ? ">" : ">=";
				default:
					return string.Empty;
			}
		}

		IEnumerable<IRateLineItem> GetSortedBreaks(IEnumerable<IRateLineItem> items)
		{
			var result = items
				.Where(item => item.RateOperatorIsMinus() || item.RateOperatorIsPlus() && (item.TM_RelevantValue != 0 || item.TM_FlatAmount != 0))
				.OrderBy(x => x.RateOperatorIsPlus())
				.ThenBy(x => x.TM_Break);

			return result;
		}
	}
}
