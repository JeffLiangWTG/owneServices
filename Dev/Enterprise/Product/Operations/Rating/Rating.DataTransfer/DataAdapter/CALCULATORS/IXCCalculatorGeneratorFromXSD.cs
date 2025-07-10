using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public class IXCCalculatorGeneratorFromXSD : RateCalculatorGeneratorFromXSD<ValueRangeCalculator, Xsd.IXCCalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore(RateLine rateLine, ValueRangeCalculator calculator, Xsd.IXCCalculator calculatorXSD, IValueObjectImportContext context)
		{
			calculator.ApplyTo = calculatorXSD.ApplyTo;

			if (calculatorXSD.SimpleRate.MinimumSpecified)
			{
				calculator.Minimum = calculatorXSD.SimpleRate.Minimum;
			}

			ImportRateItemWithOperatorAndBreak(calculator, calculatorXSD.RateItems, context);
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateLine rateLine, ValueRangeCalculator calculator, Xsd.IXCCalculator calculatorXSD, INotifications notifications)
		{
			calculatorXSD.ApplyTo = calculator.ApplyTo;

			if (!calculator.Minimum.IsEmpty)
			{
				calculatorXSD.SimpleRate.Minimum = calculator.Minimum;
			}

			ExportRateItemWithOperatorAndBreak(calculator, calculatorXSD.RateItems, notifications);
		}

		#endregion

		#region Implementation

		public override XmlSchema Schema
		{
			get { return RatingXmlSchemaDefinitions.Instance.IXCCalculatorSchema; }
		}

		protected override ZString CalculatorType
		{
			get { return ValueRangeCalculator.Code; }
		}

		#endregion
	}
}

