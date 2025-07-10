
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Integration;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public abstract class BaseCMBCalculatorGeneratorFromXSD<TCalculator, TValueObject> : RateCalculatorGeneratorFromXSD<TCalculator, TValueObject>
		where TCalculator : BaseCombinedCalculator
		where TValueObject : Xsd.BaseCMBCalculator
	{
		#region Import

		protected override void ImportFromValueObjectCore(RateLine rateLine, TCalculator calculator, TValueObject calculatorXSD, IValueObjectImportContext context)
		{
			if (ValidateRateLineItem(calculatorXSD.SimpleRate, calculatorXSD.RateItems, context))
			{
				ImportBaseOrMinimumRateOnCalculator(calculator, calculatorXSD.SimpleRate, context);
				ImportMaximumRateOnCalculator(calculator, calculatorXSD.SimpleRate, context);

				if (calculatorXSD.SimpleRate.PerUnitSpecified)
				{
					ImportPerUnitRateOnCalculator(calculator, calculatorXSD.SimpleRate, context);
				}
				else
				{
					ImportRateItemWithOperatorAndBreak(calculator, calculatorXSD.RateItems, context);
				}
			}
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateLine rateLine, TCalculator calculator, TValueObject calculatorXSD, INotifications notifications)
		{
			ExportBaseOrMinimumRateOnCalculator(calculator, calculatorXSD.SimpleRate, notifications);
			ExportMaximumRateOnCalculator(calculator, calculatorXSD.SimpleRate, notifications);

			if (!calculator.PerUnit.IsEmpty)
			{
				ExportPerUnitRateOnCalculator(calculator, calculatorXSD.SimpleRate, notifications);
			}
			else
			{
				ExportRateItemWithOperatorAndBreak(calculator, calculatorXSD.RateItems, notifications);
			}
		}

		#endregion
	}
}
