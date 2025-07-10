using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// CalculatorToCalculatorInfoConverter
	/// </summary>
	public class CalculatorToCalculatorInfoConverter
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="factory"></param>
		public CalculatorToCalculatorInfoConverter(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		/// <summary>
		/// Convert
		/// </summary>
		/// <param name="line"></param>
		/// <param name="rateType"></param>
		/// <param name="logger"></param>
		/// <returns></returns>
		public CalculatorInfo[] Convert(IRateLine line, string rateType, ILogger logger)
		{
			var result = new List<CalculatorInfo>();

			result.Add(ConvertInternal(line, false, rateType, logger));

			var agentCalculatorInfo = ConvertInternal(line, true, rateType, logger);
			if (agentCalculatorInfo != null)
			{
				result.Add(agentCalculatorInfo);
			}

			return result.ToArray();
		}

		CalculatorInfo ConvertInternal(IRateLine line, bool isAgentRate, string rateType, ILogger logger)
		{
			if (isAgentRate && !line.ChildRateLineItems.Any(i => i.TM_AgentDeclaredRate != ZDecimal.Zero))
			{
				return null;
			}

			var result = new CalculatorInfo();
			result.CWCode = CalculatorTypeConverter.EnumToCode(line.RateCalculatorType);
			result.IsAgentRate = isAgentRate;

			var attributes = new List<CalculatorAttribute>();
			attributes.AddRange(GetCalculatorAttributes(line.Calculator, isAgentRate));

			var breaksAttribute = GetCalculatorBreaks(line.Calculator, isAgentRate);
			if (breaksAttribute != null)
			{
				attributes.Add(breaksAttribute);
			}

			var applyToChargesAttribute = GetCalculatorApplyToCharges(line.Calculator);
			if (applyToChargesAttribute != null)
			{
				attributes.Add(applyToChargesAttribute);
			}

			result.Attributes = attributes.ToArray();

			return result;
		}

		IEnumerable<CalculatorAttribute> GetCalculatorAttributes(Calculator calculator, bool isAgentRate)
		{
			var result = new List<CalculatorAttribute>();

			var relatedAttributes = calculator.GetType().GetCustomAttributes(typeof(CalculatorPropertyAttribute), true).Select(i => (CalculatorPropertyAttribute)i);

			foreach (var item in relatedAttributes)
			{
				if (!string.IsNullOrEmpty(item.RelatedTo) && !string.IsNullOrEmpty(item.MapTo))
				{
					var attributeValue = GetCalculatorPropertyValue(calculator, item, isAgentRate);

					var attribute = new CalculatorAttribute()
					{
						Name = item.RelatedTo,
						Type = attributeValue.GetType().Name,
						Value = attributeValue,
					};

					result.Add(attribute);
				}
			}

			if (calculator is FreightInclusiveCalculator)
			{
				var preCarriageOnCarriageChargeTypeItem =
					calculator
					.Line
					.ChildRateLineItems
					.FirstOrDefault(item =>
						FreightInclusiveCalculator.Items.PreCarriageOnCarriageChargeType.Equals(item.TM_Type, StringComparison.InvariantCultureIgnoreCase)
						&& !item.TM_AC.IsEmpty);

				var charge = factory.Load<AccChargeCode>(preCarriageOnCarriageChargeTypeItem?.TM_AC ?? ZGuid.Empty);

				if (charge != null)
				{
					var attribute = new CalculatorAttribute()
					{
						Name = "PreCarriageOrOnCarriageChargeCode",
						Type = (NoResString)"String",
						Value = charge.AC_Code.ToString(),
					};

					result.Add(attribute);
				}
			}

			return result.ToArray();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Attribute name")]
		CalculatorAttribute GetCalculatorBreaks(Calculator calculator, bool isAgentRate)
		{
			var breakItems = calculator.Line.ChildRateLineItems.Where(IsBreakItem).ToArray();

			if (breakItems.Length > 0)
			{
				var convertedItems = new List<CalculatorBreakItem>();
				foreach (var item in breakItems)
				{
					convertedItems.Add(ConvertBreakItem(item, isAgentRate));
				}

				var breakAttribute = new CalculatorAttribute()
				{
					Name = "Breaks",
					Value = convertedItems.ToArray(),
				};

				breakAttribute.Type = breakAttribute.Value.GetType().Name;

				return breakAttribute;
			}

			return null;
		}

		bool IsBreakItem(IRateLineItem item)
		{
			var calculator = item.Calculator();
			if
				(
					calculator is WarehousePackCalculator
					|| calculator is WarehouseLocationTypeCalculator
				)
			{
				return true;
			}

			if
				(
					calculator is AgencyCalculator
					|| calculator is FlatCalculator
					|| calculator is FlatPlusPerUnitCalculator
					|| calculator is MinimumCalculator
					|| calculator is MinimumOrPerUnitCalculator
					|| calculator is PackageCountCalculator
					|| calculator is UnitCalculator
					|| calculator is CompanyTariffOrCostBasedCalculator
					|| calculator is PercentageCalculator
					|| calculator is ProfitShareRebateCalculator
				)
			{
				return false;
			}

			if (calculator is HousebillReleaseTypeCalculator)
			{
				return item.Lookups().HousebillReleaseTypes.ContainsCode(item.TM_Type);
			}

			if (calculator is NoteCalculator)
			{
				return item.TM_Type != Calculator.Items.ShowOnBillingWithoutPrefix;
			}

			var normalBreakOperations = new[]
			{
				Calculator.Items.Operator.MIN,
				Calculator.Items.Operator.MAX,
				Calculator.Items.Operator.UNT,
				Calculator.Items.Operator.BAS,
				Calculator.Items.Operator.Minus,
				Calculator.Items.Operator.Plus
			};

			var isNormalBreakItem = normalBreakOperations.Contains(item.TM_Type.ToString());
			return isNormalBreakItem;
		}

		CalculatorBreakItem ConvertBreakItem(IRateLineItem item, bool isAgentRate)
		{
			var result = new CalculatorBreakItem()
			{
				Operator = item.TM_Type,
				Break = item.TM_Break != ZDecimal.Zero ? (decimal)new ZDecimalTypeConverter().ConvertTo(item.TM_Break, typeof(decimal)) : null,
				BreakMinimum = item.TM_BreakMinimum != ZDecimal.Zero ? (decimal)new ZDecimalTypeConverter().ConvertTo(item.TM_BreakMinimum, typeof(decimal)) : null,
				FlatAmount = item.TM_FlatAmount != ZDecimal.Zero ? (decimal)new ZDecimalTypeConverter().ConvertTo(item.TM_FlatAmount, typeof(decimal)) : null,
				UnitMultiple = item.TM_UnitMultiple != ZInt.Zero ? (int)new ZIntTypeConverter().ConvertTo(item.TM_UnitMultiple, typeof(int)) : null,
				Restricted = item.TM_CallForPricing,
				Text = item.TM_Text,
				CWTransportZone = item.TM_TZ_DomesticZone.IsValid ? factory.Load<RateTransportZone>(item.TM_TZ_DomesticZone)?.TZ_ZoneName : null,
				Units = item.TM_BreakWeightVolume,
			};

			if (!isAgentRate)
			{
				result.UnitPrice = item.TM_Value != ZDecimal.Zero ? (decimal)new ZDecimalTypeConverter().ConvertTo(item.TM_Value, typeof(decimal)) : null;
			}
			else
			{
				result.UnitPrice = item.TM_AgentDeclaredRate != ZDecimal.Zero ? (decimal)new ZDecimalTypeConverter().ConvertTo(item.TM_AgentDeclaredRate, typeof(decimal)) : null;
			}

			return result;
		}

		object GetCalculatorPropertyValue(Calculator calculator, CalculatorPropertyAttribute calculatorProperty, bool isAgentRate)
		{
			object attributeValue = calculator.GetType().GetProperty(calculatorProperty.MapTo).GetValue(calculator);

			if (isAgentRate && calculatorProperty.FieldName == RateLineItem.Schema.TM_RelevantValue)
			{
				var propertyItem = calculator.Line.ChildRateLineItems.FirstOrDefault(i => i.TM_Type == calculatorProperty.ItemType);
				if (propertyItem != null)
				{
					if (calculator.GetType().GetProperty(calculatorProperty.MapTo).PropertyType == typeof(ZInt))
					{
						attributeValue = propertyItem.TM_AgentDeclaredRate.ToZInt();
					}
					else
					{
						attributeValue = propertyItem.TM_AgentDeclaredRate;
					}
				}
			}

			if (attributeValue is ZDecimal)
			{
				return new ZDecimalTypeConverter().ConvertTo(attributeValue, typeof(decimal));
			}

			if (attributeValue is ZBool)
			{
				return new ZBoolTypeConverter().ConvertTo(attributeValue, typeof(bool));
			}

			if (attributeValue is ZInt)
			{
				return new ZIntTypeConverter().ConvertTo(attributeValue, typeof(int));
			}

			if (attributeValue is ZString)
			{
				return new ZStringTypeConverter().ConvertTo(attributeValue, typeof(string));
			}

			return attributeValue;
		}

		CalculatorAttribute GetCalculatorApplyToCharges(Calculator calculator)
		{
			var applyToItems = calculator.Line.ChildRateLineItems.Where(IsApplyToItem).ToArray();

			if (applyToItems.Length > 0)
			{
				var convertedItems = new List<CalculatorApplyToCharge>();
				foreach (var item in applyToItems)
				{
					convertedItems.Add(ConvertApplyToCharge(item));
				}

				var applyToChargesAttribute = new CalculatorAttribute()
				{
					Name = "ApplyToCharges",
					Value = convertedItems.ToArray(),
				};

				applyToChargesAttribute.Type = applyToChargesAttribute.Value.GetType().Name;

				return applyToChargesAttribute;
			}

			return null;
		}

		bool IsApplyToItem(IRateLineItem item)
		{
			if (item.Calculator() is ValueRangeCalculator)
			{
				return false;
			}

			return CalculatorConstants.Type.ApplyTo.Equals(item.TM_Type, StringComparison.InvariantCultureIgnoreCase);
		}

		CalculatorApplyToCharge ConvertApplyToCharge(IRateLineItem item)
		{
			var chargeCode = item.TM_AC.IsValid ? factory.Load<AccChargeCode>(item.TM_AC)?.AC_Code : null;

			var result = new CalculatorApplyToCharge()
			{
				Type = item.TM_Text,
				ApplyCharge = chargeCode,
				CalculationPriority = Calculator.Items.Value.CalculationOrder.Equals(item.TM_Text, StringComparison.InvariantCultureIgnoreCase) ? item.TM_Value.ToZInt() : null,
			};

			return result;
		}
	}
}
