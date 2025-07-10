using System.Linq;
using System.Text;
using CargoWise.DataTransfer.Ratings;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public class HighestChargeCalculator : Calculator, IDependentCalculator
	{
		public HighestChargeCalculator(IRateLine master) : base(master)
		{
		}

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
			var parameters = calcOutput.Parameters;

			var applicableItemChargePks = Line.ChildRateLineItems
				.Where(rli => rli.RateOperatorIsApplyToOrMNT())
				.Select(rli => rli.TM_AC)
				.ToList();

			var result = this.GetValueCalculatorAppliesTo(parameters, out var highestChargeItem, includeGST: false, useGreaterCharge: true, showFullSource: false);

			if (result.Amount <= 0)
			{
				return;
			}

			LogLineRemoval(parameters, Line, (NoResString)"placeholder used for HCC calculator");

			// Remove all but the highest of the charges this calculator applies to.
			var resultsToRemove = parameters.Results
				.Where(ari => ari.ChargeCode.PK != highestChargeItem.TM_AC && applicableItemChargePks.Contains(ari.ChargeCode.PK))
				.ToList();
			var highestChargeResult = parameters.Results.FirstOrDefault(ari => ari.ChargeCode.PK == highestChargeItem.TM_AC);

			if (resultsToRemove.Count > 0 && highestChargeResult != null)
			{
				var newCalculationDescription = new StringBuilder();
				newCalculationDescription.AppendLine(highestChargeResult.CalculationDescription);
				newCalculationDescription.AppendLine(Res.GetString("50f37506-3dca-4d24-aa7d-6948af25e73c", "HCC Over Charges: {0}", string.Join(", ", resultsToRemove.Select(r => r.ChargeCode.AC_Code))));
				highestChargeResult.CalculationDescription = newCalculationDescription.ToString();

				resultsToRemove.ForEach(res =>
				{
					LogLineRemoval(parameters, res.Line, $"overridden by {highestChargeResult.Line.DisplayInfo()} by HCC calculator");
					parameters.Results.Remove(res);
				});
			}
		}

		static void LogLineRemoval(AutoRatingCalculatorParameters parameters, IRateLine line, string reason)
		{
			parameters.RatingContext.Logger.Log(LogType.Information, ZString.Format("{0} {1}\treason:\t{2}", LogEventTypes.RateLineFiltered, line.DisplayInfo(), reason));
		}

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => false;

		public const string Code = RatingCalculatorCodes.HighestCharge;

		public override bool IsMeasureTypeMatchApplicable => false;

		IRateLine IDependentCalculator.Master => Line;
	}
}
