using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public class PSRCalculatorGeneratorFromXSD : RateCalculatorGeneratorFromXSD<ProfitShareRebateCalculator, Xsd.PSRCalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore(RateLine rateLine, ProfitShareRebateCalculator calculator, Xsd.PSRCalculator calculatorXSD, IValueObjectImportContext context)
		{
			if (calculatorXSD.MinimumSpecified)
			{
				calculator.Minimum = calculatorXSD.Minimum;
			}

			if (calculatorXSD.BasePriceSpecified)
			{
				calculator.BaseRate = calculatorXSD.BasePrice;
			}

			if (calculatorXSD.MaximumSpecified)
			{
				calculator.Maximum = calculatorXSD.Maximum;
			}

			if (calculatorXSD.PercentageSpecified)
			{
				calculator.Percent = calculatorXSD.Percentage;
			}

			if (calculatorXSD.ZeroWhenLossSpecified)
			{
				calculator.ZeroWhenLoss = (calculatorXSD.ZeroWhenLoss == Xsd.TrueFalse.@true);
			}

			foreach (Xsd.PERRateItem rateItem in calculatorXSD.RateItems)
			{
				var type = rateItem.Type;

				var item = calculator.RateLineItems.AddNew();
				item.TM_Text = type;

				if (type == CalculatorConstants.Text.ChargeCode && !rateItem.Chrg.IsEmpty)
				{
					var charge = RateImportHelper.Instance.FindChargeCode(rateItem.Chrg, context);
					if (charge != null)
					{
						item.TM_AC = charge.PK;
					}
					else
					{
						context.Notify(new ErrorNotification(RateErrorType.InvalidChargeCode, rateItem.Chrg));
					}
				}
			}
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateLine rateLine, ProfitShareRebateCalculator calculator, Xsd.PSRCalculator calculatorXSD, INotifications notifications)
		{
			if (!calculator.Minimum.IsEmpty)
			{
				calculatorXSD.Minimum = calculator.Minimum;
			}

			if (!calculator.BaseRate.IsEmpty)
			{
				calculatorXSD.BasePrice = calculator.BaseRate;
			}

			if (!calculator.Maximum.IsEmpty)
			{
				calculatorXSD.Maximum = calculator.Maximum;
			}

			if (!calculator.Percent.IsEmpty)
			{
				calculatorXSD.Percentage = calculator.Percent;
			}

			calculatorXSD.ZeroWhenLoss = calculator.ZeroWhenLoss ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;

			foreach (RateLineItem item in calculator.RateLineItems)
			{
				var rateItem = calculatorXSD.RateItems.AddNew();
				rateItem.Type = item.TM_Text;

				if (item.TM_Text == CalculatorConstants.Text.ChargeCode && item.ChargeCode != null)
				{
					rateItem.Chrg = item.ChargeCode.AC_Code;
				}
			}
		}

		#endregion

		#region Implementation

		public override XmlSchema Schema
		{
			get { return RatingXmlSchemaDefinitions.Instance.PSRCalculatorSchema; }
		}

		protected override ZString CalculatorType
		{
			get { return ProfitShareRebateCalculator.Code; }
		}

		#endregion
	}
}

