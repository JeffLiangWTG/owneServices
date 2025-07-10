using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public class SMBCalculatorGeneratorFromXSD : RateCalculatorGeneratorFromXSD<SplitMonthBillingCalculator, Xsd.SMBCalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore(RateLine rateLine, SplitMonthBillingCalculator calculator, Xsd.SMBCalculator calculatorXSD, IValueObjectImportContext context)
		{
			if (calculatorXSD.SimpleRate.MinimumSpecified)
			{
				calculator.Minimum = calculatorXSD.SimpleRate.Minimum;
			}

			ImportRateItemWithOperatorAndBreak(calculator, calculatorXSD.RateItems, context);
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateLine rateLine, SplitMonthBillingCalculator calculator, Xsd.SMBCalculator calculatorXSD, INotifications notifications)
		{
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
			get { return RatingXmlSchemaDefinitions.Instance.SMBCalculatorSchema; }
		}

		protected override ZString CalculatorType
		{
			get { return SplitMonthBillingCalculator.Code; }
		}

		#endregion
	}
}

