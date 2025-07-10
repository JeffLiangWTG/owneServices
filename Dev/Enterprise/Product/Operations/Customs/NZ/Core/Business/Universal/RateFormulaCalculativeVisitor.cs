using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NZ.Business
{
	public sealed class RateFormulaCalculativeVisitor : Universal.RateFormulaCalculativeVisitor
	{
		public RateFormulaCalculativeVisitor(UniversalRateCalcDataWrapper rateCalcData)
			: base(rateCalcData)
		{
		}
		bool omitVFDCalculation;
		bool omitUOMCalculation;
		bool saveVFDRate;

		public ZDecimal VFDRate => vfdRate;
		ZDecimal vfdRate;

		public ZDecimal UOMRate => uomRate;
		ZDecimal uomRate;

		public ZString UOMCode => uomCode;
		ZString uomCode;

		public void Reset(bool omitVFDCalculation, bool omitUOMCalculation, bool saveVFDRate = false)
		{
			this.omitVFDCalculation = omitVFDCalculation;
			this.omitUOMCalculation = omitUOMCalculation;
			this.saveVFDRate = saveVFDRate;
			vfdRate = 0m;
			uomRate = 0m;
			uomCode = ZString.Empty;
			if (errorListener.Errors.Any() && errorListener.Errors is List<ErrorInformation> errors)
			{
				errors.Clear();
			}
		}

		public override decimal VisitNumber(RateFormulaParser.NumberContext context)
		{
			var value = context.numberBody.Text;
			if (decimal.TryParse(value, out var result))
			{
				var parentExpression = context.Parent?.Parent?.GetText() ?? ZString.Empty;
				if (parentExpression.Contains("VFD"))
				{
					var originalRate = result;
					result = omitVFDCalculation ? 0m : result;
					vfdRate = saveVFDRate ? originalRate : result;
				}
				else if (parentExpression.Contains("["))
				{
					result = omitUOMCalculation ? 0m : result;
					uomRate = result;
				}
				else if (parentExpression != "/100")
				{
					errorListener.Report(FormulaVisitErrorType.SyntaxError, $"Formula structure is not correct");
				}
			}
			else
			{
				errorListener.Report(FormulaVisitErrorType.SyntaxError, $"Failed to convert \"{value}\" to Decimal");
			}

			return result;
		}

		public override decimal VisitUomPlaceHolder(RateFormulaParser.UomPlaceHolderContext context)
		{
			uomCode = context.UOMCode().GetText().ToUpperInvariant().Trim('[', ']');
			return base.VisitUomPlaceHolder(context);
		}
	}
}
