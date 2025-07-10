using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public class CTBCalculatorGeneratorFromXSD : RateCalculatorGeneratorFromXSD<CompanyTariffOrCostBasedCalculator, Xsd.CTBCalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore(RateLine rateLine, CompanyTariffOrCostBasedCalculator calculator, Xsd.CTBCalculator calculatorXSD, IValueObjectImportContext context)
		{
			if (ValidateRateLineItem(calculatorXSD.SimpleRate, calculatorXSD.RateItems, context))
			{
				ImportBaseOrMinimumRateOnCalculator(calculator, calculatorXSD.SimpleRate, context);
				if (calculatorXSD.SimpleRate.PerUnitSpecified)
				{
					ImportPerUnitRateOnCalculator(calculator, calculatorXSD.SimpleRate, context);
				}
				else
				{
					ImportRateItemWithOperatorAndBreak(calculator, calculatorXSD.RateItems, context);
				}
			}

			calculator.CalculationOrder = calculatorXSD.CalculationOrder;
			calculator.EquipmentType = calculatorXSD.EquipmentType;
			calculator.MessageType = calculatorXSD.Type;
			calculator.MessageSubType = calculatorXSD.Style;

			if (calculatorXSD.PercentSpecified && calculatorXSD.RateItems.Count == 0)
			{
				calculator.Percent = calculatorXSD.Percent;
			}
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateLine rateLine, CompanyTariffOrCostBasedCalculator calculator, Xsd.CTBCalculator calculatorXSD, INotifications notifications)
		{
			ExportBaseOrMinimumRateOnCalculator(calculator, calculatorXSD.SimpleRate, notifications);
			if (!calculator.PerUnit.IsEmpty)
			{
				ExportPerUnitRateOnCalculator(calculator, calculatorXSD.SimpleRate, notifications);
			}
			else
			{
				ExportRateItemWithOperatorAndBreak(calculator, calculatorXSD.RateItems, notifications);
			}

			calculatorXSD.CalculationOrder = calculator.CalculationOrder;
			calculatorXSD.EquipmentType = calculator.EquipmentType;
			calculatorXSD.Type = calculator.MessageType;
			calculatorXSD.Style = calculator.MessageSubType;

			if (!calculator.Percent.IsEmpty && calculator.RateLineItems.Count == 0)
			{
				calculatorXSD.Percent = calculator.Percent;
			}
		}

		#endregion

		#region Implementation

		public override XmlSchema Schema
		{
			get { return RatingXmlSchemaDefinitions.Instance.CTBCalculatorSchema; }
		}

		protected override ZString CalculatorType
		{
			get { return CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode; }
		}

		#endregion
	}
}

