using System.Collections.Generic;
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
	public class PEBCalculatorGeneratorFromXSD : RateCalculatorGeneratorFromXSD<PercentageBreaksCalculator, Xsd.PEBCalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore(RateLine rateLine, PercentageBreaksCalculator calculator, Xsd.PEBCalculator calculatorXSD, IValueObjectImportContext context)
		{
			if (calculatorXSD.MinimumSpecified)
			{
				calculator.Minimum = calculatorXSD.Minimum;
			}
			if (calculatorXSD.MaximumSpecified)
			{
				calculator.Maximum = calculatorXSD.Maximum;
			}
			if (calculatorXSD.BasePriceSpecified)
			{
				calculator.BaseRate = calculatorXSD.BasePrice;
			}

			if (calculatorXSD.BreaksBasedOnValuesSpecified)
			{
				calculator.UseBreaksBasedOnValues = (calculatorXSD.BreaksBasedOnValues == Xsd.TrueFalse.@true);
			}

			foreach (Xsd.PERRateItem rateItem in calculatorXSD.ApplyToRateItems)
			{
				ZString type = rateItem.Type;

				RateLineItem item = calculator.ApplyToRateLineItems.AddNew();
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

			if (calculatorXSD.RateItems.Count > 0)
			{
				Xsd.RateItemWithOperatorAndBreakAndPercentCollection minusItems = GetSpecificItemsFromXSD(calculatorXSD.RateItems, new ZString("-"));
				Xsd.RateItemWithOperatorAndBreakAndPercentCollection plusItems = GetSpecificItemsFromXSD(calculatorXSD.RateItems, new ZString("+"));

				if (minusItems.Count > 1)
				{
					context.Notify(new ErrorNotification(RateErrorType.DataErrorPreventSave, Res.GetString("374f4076-e9b2-40b9-a316-885b45b6e04f", "You cannot have more than one '-' in same rate")));
				}
				else
				{
					RateLineItem item;
					if (minusItems.Count == 1)
					{
						item = calculator.RateLineItems.AddNew();
						ImportValuesOnRateItemWithOperatorAndBreakAndPercent(item, minusItems[0], context);
					}

					foreach (Xsd.RateItemWithOperatorAndBreakAndPercent rateItem in plusItems)
					{
						item = calculator.RateLineItems.AddNew();
						ImportValuesOnRateItemWithOperatorAndBreakAndPercent(item, rateItem, context);
					}
				}
			}
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateLine rateLine, PercentageBreaksCalculator calculator, Xsd.PEBCalculator calculatorXSD, INotifications notifications)
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

			calculatorXSD.BreaksBasedOnValues = calculator.UseBreaksBasedOnValues ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;

			foreach (RateLineItem item in calculator.ApplyToRateLineItems)
			{
				Xsd.PERRateItem rateItem = calculatorXSD.ApplyToRateItems.AddNew();

				rateItem.Type = item.TM_Text;

				if (item.TM_Text == CalculatorConstants.Text.ChargeCode && item.ChargeCode != null)
				{
					rateItem.Chrg = item.ChargeCode.AC_Code;
				}
			}

			if (calculator.RateLineItems.Count > 0)
			{
				List<RateLineItem> minusItems = GetSpecificItemsFromCalculator(calculator.RateLineItems, new ZString("-"));
				List<RateLineItem> plusItems = GetSpecificItemsFromCalculator(calculator.RateLineItems, new ZString("+"));

				if (minusItems.Count > 1)
				{
					notifications.Notify(new ErrorNotification(RateErrorType.DataErrorPreventSave, Res.GetString("374f4076-e9b2-40b9-a316-885b45b6e04f", "You cannot have more than one '-' in same rate")));
				}
				else
				{
					if (minusItems.Count == 1)
					{
						ExportValuesOnRateItemWithOperatorAndBreakAndPercent(minusItems[0], calculatorXSD.RateItems.AddNew(), notifications);
					}

					foreach (RateLineItem rateItem in plusItems)
					{
						ExportValuesOnRateItemWithOperatorAndBreakAndPercent(rateItem, calculatorXSD.RateItems.AddNew(), notifications);
					}
				}
			}
		}

		#endregion

		#region Helpers

		protected Xsd.RateItemWithOperatorAndBreakAndPercentCollection GetSpecificItemsFromXSD(Xsd.RateItemWithOperatorAndBreakAndPercentCollection rateItems, ZString rateOperator)
		{
			Xsd.RateItemWithOperatorAndBreakAndPercentCollection specificItems = new Xsd.RateItemWithOperatorAndBreakAndPercentCollection();

			foreach (Xsd.RateItemWithOperatorAndBreakAndPercent item in rateItems)
			{
				if (item.Operator == rateOperator)
				{
					specificItems.Add(item);
				}
			}

			return specificItems;
		}

		protected void ImportValuesOnRateItemWithOperatorAndBreakAndPercent(RateLineItem item, Xsd.RateItemWithOperatorAndBreakAndPercent rateItem, IValueObjectImportContext context)
		{
			item.TM_Type = rateItem.Operator;
			item.TM_BreakWeightVolume = rateItem.Units;

			if (rateItem.FlatAmountSpecified)
			{
				item.TM_FlatAmount = rateItem.FlatAmount;
			}
			if (rateItem.BreakAmountSpecified)
			{
				item.TM_Break = rateItem.BreakAmount;
			}
			if (rateItem.BreakMinimumSpecified)
			{
				item.TM_BreakMinimum = rateItem.BreakMinimum;
			}
			if (rateItem.PercentSpecified)
			{
				item.TM_BreakMinimum = rateItem.Percent;
			}
		}

		protected void ExportValuesOnRateItemWithOperatorAndBreakAndPercent(RateLineItem item, Xsd.RateItemWithOperatorAndBreakAndPercent rateItem, INotifications notifications)
		{
			rateItem.Operator = item.TM_Type;
			rateItem.Units = item.TM_BreakWeightVolume;
			rateItem.FlatAmount = item.TM_FlatAmount;
			rateItem.BreakAmount = item.TM_Break;
			rateItem.BreakMinimum = item.TM_BreakMinimum;
			rateItem.Percent = item.TM_BreakMinimum;
		}

		#endregion

		#region Implementation

		public override XmlSchema Schema
		{
			get { return RatingXmlSchemaDefinitions.Instance.PEBCalculatorSchema; }
		}

		protected override ZString CalculatorType
		{
			get { return PercentageBreaksCalculator.Code; }
		}

		#endregion
	}
}

