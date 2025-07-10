using System.Globalization;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal
{
	class RateFormulaExtractionVisitor : RateFormulaBaseVisitor<bool>
	{
		public RateFormulaExtractionVisitor(UniversalRateCalcDataWrapper rateCalcData)
		{
			this.rateCalcData = Argument.NotNull(rateCalcData, "rateCalcData");
		}

		readonly UniversalRateCalcDataWrapper rateCalcData;
		FormulaErrorListener errorListener { get { return rateCalcData.errorListener; } }

		#region Variable

		public override bool VisitUomPlaceHolder(RateFormulaParser.UomPlaceHolderContext context)
		{
			var uomCode = context.UOMCode().GetText().ToUpperInvariant().Trim('[', ']');
			if (!rateCalcData.UnitOfMeasureValueList.ContainsKey(uomCode))
			{
				errorListener.Report(FormulaVisitErrorType.UOMNotFound, string.Format(CultureInfo.InvariantCulture, (NoResString)"Unit of Measure code: {0} is not specified", uomCode));
			}
			return false;
		}

		public override bool VisitVarCountrySpecificKeyword(RateFormulaParser.VarCountrySpecificKeywordContext context)
		{
			var countrySpecificValueName = context.code.Text.ToUpperInvariant();
			if (!rateCalcData.CountrySpecificValueList.ContainsKey(countrySpecificValueName))
			{
				errorListener.Report(FormulaVisitErrorType.CountrySpecificValueNotFound, string.Format(CultureInfo.InvariantCulture, (NoResString)"Country Specific Value: {0} is not specified", countrySpecificValueName));
			}
			return false;
		}

		public override bool VisitFormulaSpecificValue(RateFormulaParser.FormulaSpecificValueContext context)
		{
			var precision = 0;
			var scale = 0;

			var inputSpecification = context.formulaSpecificValueSpec();
			if (inputSpecification != null)
			{
				precision = Utils.GetIntegerNumber(inputSpecification.precision.Text, errorListener);
				scale = Utils.GetIntegerNumber(inputSpecification.scale.Text, errorListener);
			}

			var question = context.questionToAsk.Text.Trim('"').Trim();
			var questionFormatted = question.ToUpperInvariant();
			var formulaSpecificValues = rateCalcData.FormulaSpecificValueList;
			if (!formulaSpecificValues.ContainsKey(questionFormatted))
			{
				formulaSpecificValues.Add(questionFormatted, new QuestionForFormulaSpecificValue(question, precision, scale));
			}

			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Message")]
		public override bool VisitMeursingPlaceHolder(RateFormulaParser.MeursingPlaceHolderContext context)
		{
			var meursingCode = context.MEURSINGCode().GetText().ToUpperInvariant().Trim('#', '#');
			if (!rateCalcData.MeursingExpressionList.ContainsKey(meursingCode))
			{
				errorListener.Report(FormulaVisitErrorType.MeursingExpressionNotFound, string.Format(CultureInfo.InvariantCulture, "Meursing code: {0} is not specified", meursingCode));
			}
			return false;
		}

		#endregion
	}
}
