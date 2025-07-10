using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public class HRCCalculatorGeneratorFromXSD : RateCalculatorGeneratorFromXSD<HighestRateCalculator, Xsd.HRCCalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore(RateLine rateLine, HighestRateCalculator calculator, Xsd.HRCCalculator calculatorXSD, IValueObjectImportContext context)
		{
			foreach (Xsd.RateItemWithOperatorAndBreak rateItemXSD in calculatorXSD.RateItems)
			{
				RateLineItem item = calculator.RateLineItems.AddNew();
				item.TM_Type = rateItemXSD.Operator;
				if (item.RateOperatorIsUNT())
				{
					item.TM_BreakWeightVolume = rateItemXSD.Units;
				}
				if (rateItemXSD.FlatAmountSpecified)
				{
					item.TM_FlatAmount = rateItemXSD.FlatAmount;
				}
				if (rateItemXSD.PerUnitSpecified)
				{
					item.TM_RelevantValue = rateItemXSD.PerUnit;
				}
			}
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateLine rateLine, HighestRateCalculator calculator, Xsd.HRCCalculator calculatorXSD, INotifications notifications)
		{
			foreach (RateLineItem item in calculator.RateLineItems)
			{
				Xsd.RateItemWithOperatorAndBreak rateItemXSD = calculatorXSD.RateItems.AddNew();

				rateItemXSD.Operator = item.TM_Type;
				rateItemXSD.Units = item.TM_BreakWeightVolume;
				rateItemXSD.FlatAmount = item.TM_FlatAmount;
				rateItemXSD.PerUnit = item.TM_RelevantValue;
			}
		}

		#endregion

		#region Implementation

		public override XmlSchema Schema
		{
			get { return RatingXmlSchemaDefinitions.Instance.HRCCalculatorSchema; }
		}

		protected override ZString CalculatorType
		{
			get { return HighestRateCalculator.Code; }
		}

		#endregion
	}
}

