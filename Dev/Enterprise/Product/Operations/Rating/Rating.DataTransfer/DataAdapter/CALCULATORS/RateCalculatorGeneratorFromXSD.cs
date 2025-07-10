using System;
using System.Collections.Generic;
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
	public abstract class RateCalculatorGeneratorFromXSDBase<TCalculator, TValueObject> : IRateCalculatorGenerator
		where TCalculator : Calculator
		where TValueObject : Xsd.RateCalculator
	{
		#region IRateCalculatorGenerator Members

		public void ImportFromValueObject(RateLine rateLine, IValueObject calculatorXSD, IValueObjectImportContext context)
		{
			ImportFromValueObjectCore(rateLine, (TCalculator)rateLine.Calculator, (TValueObject)calculatorXSD, context);
		}

		public void ExportToValueObject(RateLine rateLine, IValueObject calculatorXSD, INotifications notifications)
		{
			ExportToValueObjectCore(rateLine, (TCalculator)rateLine.Calculator, (TValueObject)calculatorXSD, notifications);
		}

		ZString IRateCalculatorGenerator.CalculatorType
		{
			get { return CalculatorType; }
		}

		Type IRateCalculatorGenerator.CalculatorSchemaType
		{
			get { return typeof(TValueObject); }
		}

		#endregion

		#region Abstract Members

		protected abstract void ImportFromValueObjectCore(RateLine rateLine, TCalculator calculator, TValueObject calculatorXSD, IValueObjectImportContext context);

		protected abstract void ExportToValueObjectCore(RateLine rateLine, TCalculator calculator, TValueObject calculatorXSD, INotifications notifications);

		protected abstract ZString CalculatorType { get; }

		#endregion
	}

	public abstract class RateCalculatorGeneratorFromXSD<TCalculator, TValueObject> : RateCalculatorGeneratorFromXSDBase<TCalculator, TValueObject>
		where TCalculator : Calculator
		where TValueObject : Xsd.RateCalculator
	{
		#region ReadOnly Members

		readonly ZString InvalidRateItemErrorMesg = Res.GetString("f24a5183-b36c-4446-91b7-acefcb34b059", "You cannot have both 'UNT' and '-' in the same rate");

		#endregion

		#region Abstract Members

		public abstract XmlSchema Schema { get; }

		#endregion

		#region Helpers

		protected Xsd.RateItemWithOperatorAndBreakCollection GetSpecificItemsFromXSD(Xsd.RateItemWithOperatorAndBreakCollection rateItems, ZString rateOperator)
		{
			Xsd.RateItemWithOperatorAndBreakCollection specificItems = new Xsd.RateItemWithOperatorAndBreakCollection();

			foreach (Xsd.RateItemWithOperatorAndBreak item in rateItems)
			{
				if (item.Operator == rateOperator)
				{
					specificItems.Add(item);
				}
			}

			return specificItems;
		}

		protected List<RateLineItem> GetSpecificItemsFromCalculator(RateLineItemsView rateItems, ZString rateOperator)
		{
			List<RateLineItem> specificItems = new List<RateLineItem>();

			foreach (RateLineItem item in rateItems)
			{
				if (item.TM_Type == rateOperator)
				{
					specificItems.Add(item);
				}
			}

			return specificItems;
		}

		#endregion

		#region Import

		protected void ImportRateItemWithOperatorAndBreak(TCalculator calculator, Xsd.RateItemWithOperatorAndBreakCollection rateItems, IValueObjectImportContext context)
		{
			ImportRateItemWithOperatorAndBreak(calculator, rateItems, null, context);
		}

		protected void ImportRateItemWithOperatorAndBreak(TCalculator calculator, Xsd.RateItemWithOperatorAndBreakCollection rateItems, CartageZone zone, IValueObjectImportContext context)
		{
			if (rateItems.Count == 0)
			{
				return;
			}

			Xsd.RateItemWithOperatorAndBreakCollection minusItems = GetSpecificItemsFromXSD(rateItems, new ZString("-"));
			Xsd.RateItemWithOperatorAndBreakCollection plusItems = GetSpecificItemsFromXSD(rateItems, new ZString("+"));

			if (minusItems.Count > 1)
			{
				context.Notify(new ErrorNotification(RateErrorType.DataErrorPreventSave, Res.GetString("6f2ff899-caec-4706-af02-b958df512488", "You cannot have more than one '-' in same rate")));
			}

			if (plusItems.Count > 0)
			{
				RateLineItem item;
				if (minusItems.Count == 1)
				{
					item = calculator.RateLineItems.AddNew();
					ImportValuesOnRateItemWithOperatorAndBreak(item, minusItems[0], zone, context);
				}

				foreach (Xsd.RateItemWithOperatorAndBreak rateItem in plusItems)
				{
					item = calculator.RateLineItems.AddNew();
					ImportValuesOnRateItemWithOperatorAndBreak(item, rateItem, zone, context);
				}
			}
		}

		protected void ImportRateItemWithDescAmtOrRate(TCalculator calculator, Xsd.RateItemWithDescAmtOrRateCollection rateItemsXSD, bool takeValueFromRate, IValueObjectImportContext context)
		{
			foreach (Xsd.RateItemWithDescAmtOrRate rateItemXSD in rateItemsXSD)
			{
				RateLineItem rateItem = calculator.RateLineItems.AddNew();
				context.SetPropertyInfoValueWithinMaxLengthAndWarn(rateItem.TM_TypeInfo, rateItemXSD.Code, rateItemXSD.IsSpecified);
				context.SetPropertyInfoValueWithinMaxLengthAndWarn(rateItem.TM_TextInfo, rateItemXSD.Description, rateItemXSD.IsSpecified);
				context.SetPropertyInfoValueWithinMaxLengthAndWarn(rateItem.TM_BreakWeightVolumeInfo, rateItemXSD.Units, rateItemXSD.IsSpecified);

				if ((takeValueFromRate && rateItemXSD.RateSpecified) || (!takeValueFromRate && rateItemXSD.AmountSpecified))
				{
					rateItem.TM_RelevantValue = (takeValueFromRate) ? rateItemXSD.Rate : rateItemXSD.Amount;
				}
			}
		}

		protected bool ValidateRateLineItem(Xsd.CalculatorSimpleRate baseRate, Xsd.RateItemWithOperatorAndBreakCollection rateItemsXSD, IValueObjectImportContext context)
		{
			bool result = true;

			if (baseRate.PerUnitSpecified && rateItemsXSD.Count > 0)
			{
				context.Notify(new ErrorNotification(RateErrorType.DataErrorPreventSave, InvalidRateItemErrorMesg));
				result = false;
			}

			return result;
		}

		protected void ImportValuesOnRateItemWithOperatorAndBreak(RateLineItem item, Xsd.RateItemWithOperatorAndBreak rateItem, CartageZone zone, IValueObjectImportContext context)
		{
			SetZoneOnRateItem(item, zone);

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

			if (rateItem.PerUnitSpecified)
			{
				item.TM_RelevantValue = rateItem.PerUnit;
			}
		}

		protected static void SetZoneOnRateItem(RateLineItem lineItem, CartageZone zone)
		{
			if (zone != null)
			{
				if (zone.IsACIZone)
				{
					lineItem.TM_F1Zone = zone.ZoneName;
				}
				else
				{
					lineItem.TM_TZ_DomesticZone = zone.ZonePK;
				}
			}
		}

		protected void ImportBaseOrMinimumRateOnCalculator(TCalculator calculator, Xsd.CalculatorSimpleRate baseRate, IValueObjectImportContext context)
		{
			if (baseRate.BaseRateSpecified)
			{
				calculator[Calculator.Items.Operator.BAS] = baseRate.BaseRate;
			}

			if (baseRate.MinimumSpecified)
			{
				calculator[Calculator.Items.Operator.MIN] = baseRate.Minimum;
			}
		}

		protected void ImportMaximumRateOnCalculator(TCalculator calculator, Xsd.CalculatorSimpleRate baseRate, IValueObjectImportContext context)
		{
			if (baseRate.MaximumSpecified)
			{
				calculator[Calculator.Items.Operator.MAX] = baseRate.Maximum;
			}
		}

		protected void ImportPerUnitRateOnCalculator(TCalculator calculator, Xsd.CalculatorSimpleRate baseRate, IValueObjectImportContext context)
		{
			if (baseRate.PerUnitSpecified)
			{
				calculator[Calculator.Items.Operator.UNT] = baseRate.PerUnit;
			}
		}

		#endregion

		#region Export

		protected void ExportRateItemWithOperatorAndBreak(TCalculator calculator, Xsd.RateItemWithOperatorAndBreakCollection rateItems, INotifications notifications)
		{
			ExportRateItemWithOperatorAndBreak(calculator, rateItems, null, notifications);
		}

		protected void ExportRateItemWithOperatorAndBreak(TCalculator calculator, Xsd.RateItemWithOperatorAndBreakCollection rateItems, CartageZone zone, INotifications notifications)
		{
			var lineItems = GetRateLineItems(calculator, zone);
			if (lineItems.Count == 0)
			{
				return;
			}

			List<RateLineItem> minusItems = GetSpecificItemsFromCalculator(lineItems, new ZString("-"));
			List<RateLineItem> plusItems = GetSpecificItemsFromCalculator(lineItems, new ZString("+"));

			if (minusItems.Count > 1)
			{
				notifications.Notify(new ErrorNotification(RateErrorType.DataErrorPreventSave, Res.GetString("6f2ff899-caec-4706-af02-b958df512488", "You cannot have more than one '-' in same rate")));
			}
			else
			{
				if (minusItems.Count == 1)
				{
					ExportValuesOnRateItemWithOperatorAndBreak(minusItems[0], rateItems.AddNew(), notifications);
				}

				foreach (RateLineItem rateItem in plusItems)
				{
					ExportValuesOnRateItemWithOperatorAndBreak(rateItem, rateItems.AddNew(), notifications);
				}
			}
		}

		protected virtual RateLineItemsView GetRateLineItems(TCalculator calculator, CartageZone zone)
		{
			return calculator.RateLineItems;
		}

		protected void ExportRateItemWithDescAmtOrRate(TCalculator calculator, Xsd.RateItemWithDescAmtOrRateCollection rateItemsXSD, bool setValueToRate, INotifications notifications)
		{
			var lineItems = GetRateLineItems(calculator, null);
			foreach (RateLineItem rateItem in lineItems)
			{
				Xsd.RateItemWithDescAmtOrRate rateItemXSD = rateItemsXSD.AddNew();
				rateItemXSD.Description = rateItem.TM_Text;

				if (setValueToRate)
				{
					rateItemXSD.Rate = rateItem.TM_RelevantValue;
				}
				else
				{
					rateItemXSD.Amount = rateItem.TM_RelevantValue;
				}

				rateItemXSD.Code = rateItem.TM_Type;
				rateItemXSD.Units = rateItem.TM_BreakWeightVolume;
			}
		}

		protected void ExportValuesOnRateItemWithOperatorAndBreak(RateLineItem item, Xsd.RateItemWithOperatorAndBreak rateItem, INotifications notifications)
		{
			rateItem.Operator = item.TM_Type;
			rateItem.Units = item.TM_BreakWeightVolume;
			rateItem.FlatAmount = RoundToAvoidTrailingZeroDecimals(item.TM_FlatAmount);
			rateItem.BreakAmount = RoundToAvoidTrailingZeroDecimals(item.TM_Break);
			rateItem.BreakMinimum = RoundToAvoidTrailingZeroDecimals(item.TM_BreakMinimum);
			rateItem.PerUnit = RoundToAvoidTrailingZeroDecimals(item.TM_RelevantValue);
		}

		ZDecimal RoundToAvoidTrailingZeroDecimals(ZDecimal value)
		{
			return value.Round(value.DecimalPlaces);
		}

		protected void ExportBaseOrMinimumRateOnCalculator(TCalculator calculator, Xsd.CalculatorSimpleRate baseRate, INotifications notifications)
		{
			if (!((ZDecimal)calculator[Calculator.Items.Operator.BAS]).IsEmpty)
			{
				baseRate.BaseRate = (ZDecimal)calculator[Calculator.Items.Operator.BAS];
			}

			if (!((ZDecimal)calculator[Calculator.Items.Operator.MIN]).IsEmpty)
			{
				baseRate.Minimum = (ZDecimal)calculator[Calculator.Items.Operator.MIN];
			}
		}

		protected void ExportMaximumRateOnCalculator(TCalculator calculator, Xsd.CalculatorSimpleRate baseRate, INotifications notifications)
		{
			if (!((ZDecimal)calculator[Calculator.Items.Operator.MAX]).IsEmpty)
			{
				baseRate.Maximum = (ZDecimal)calculator[Calculator.Items.Operator.MAX];
			}
		}

		protected void ExportPerUnitRateOnCalculator(TCalculator calculator, Xsd.CalculatorSimpleRate baseRate, INotifications notifications)
		{
			if (!((ZDecimal)calculator[Calculator.Items.Operator.UNT]).IsEmpty)
			{
				baseRate.PerUnit = (ZDecimal)calculator[Calculator.Items.Operator.UNT];
			}
		}

		#endregion
	}
}

