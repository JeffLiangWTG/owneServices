using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public class PERCalculatorGeneratorFromXSD : RateCalculatorGeneratorFromXSD<PercentageCalculator, Xsd.PERCalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore(RateLine rateLine, PercentageCalculator calculator, Xsd.PERCalculator calculatorXSD, IValueObjectImportContext context)
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

			if (calculatorXSD.TakeHighestChargeSpecified)
			{
				calculator.GreaterCharge = (calculatorXSD.TakeHighestCharge == Xsd.TrueFalse.@true);
			}

			if (calculatorXSD.ExcludeGSTSpecified)
			{
				calculator.IncludeGST = (calculatorXSD.ExcludeGST == Xsd.TrueFalse.@true);
			}

			if (calculatorXSD.PartThereOf.RateSpecified || calculatorXSD.PartThereOf.ValueOrPartThereOfSpecified)
			{
				calculator.IsPartThereof = true;
				if (calculatorXSD.PartThereOf.RateSpecified)
				{
					calculator.Rate = calculatorXSD.PartThereOf.Rate;
				}
				if (calculatorXSD.PartThereOf.ValueOrPartThereOfSpecified)
				{
					calculator.ValueOrPartThereOf = calculatorXSD.PartThereOf.ValueOrPartThereOf;
				}
			}
			else
			{
				if (calculatorXSD.PercentageSpecified)
				{
					calculator.Percent = calculatorXSD.Percentage;
				}
			}

			foreach (Xsd.PERRateItem rateItem in calculatorXSD.RateItems)
			{
				ZString type = rateItem.Type;

				RateLineItem item = calculator.RateLineItems.AddNew();
				item.TM_Text = type;

				if (type == CalculatorConstants.Text.ChargeCode && !rateItem.Chrg.IsEmpty)
				{
					AccChargeCode charge = RateImportHelper.Instance.FindChargeCode(rateItem.Chrg, context);

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

		protected override void ExportToValueObjectCore(RateLine rateLine, PercentageCalculator calculator, Xsd.PERCalculator calculatorXSD, INotifications notifications)
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

			if (calculator.GreaterCharge)
			{
				calculatorXSD.TakeHighestCharge = calculator.GreaterCharge ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
			}
			if (calculator.IncludeGST)
			{
				calculatorXSD.ExcludeGST = calculator.IncludeGST ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
			}

			if (calculator.IsPartThereof)
			{
				if (!calculator.Rate.IsEmpty)
				{
					calculatorXSD.PartThereOf.Rate = calculator.Rate;
				}
				if (!calculator.ValueOrPartThereOf.IsEmpty)
				{
					calculatorXSD.PartThereOf.ValueOrPartThereOf = calculator.ValueOrPartThereOf;
				}
			}
			else
			{
				if (!calculator.Percent.IsEmpty)
				{
					calculatorXSD.Percentage = calculator.Percent;
				}
			}

			foreach (RateLineItem item in calculator.RateLineItems)
			{
				Xsd.PERRateItem rateItem = calculatorXSD.RateItems.AddNew();

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
			get { return RatingXmlSchemaDefinitions.Instance.PERCalculatorSchema; }
		}

		protected override ZString CalculatorType
		{
			get { return PercentageCalculator.Code; }
		}

		#endregion
	}
}

