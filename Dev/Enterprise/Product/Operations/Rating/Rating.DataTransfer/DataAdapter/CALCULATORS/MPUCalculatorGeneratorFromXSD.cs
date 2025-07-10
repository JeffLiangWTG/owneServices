using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public class MPUCalculatorGeneratorFromXSD : RateCalculatorGeneratorFromXSD<MinimumOrPerUnitCalculator, Xsd.MPUCalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore(RateLine rateLine, MinimumOrPerUnitCalculator calculator, Xsd.MPUCalculator calculatorXSD, IValueObjectImportContext context)
		{
			if (calculatorXSD.MinimumSpecified)
			{
				calculator.Minimum = calculatorXSD.Minimum;
			}
			if (calculatorXSD.PerUnitPriceSpecified)
			{
				calculator.PerUnit = calculatorXSD.PerUnitPrice;
			}
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateLine rateLine, MinimumOrPerUnitCalculator calculator, Xsd.MPUCalculator calculatorXSD, INotifications notifications)
		{
			if (!calculator.Minimum.IsEmpty)
			{
				calculatorXSD.Minimum = calculator.Minimum;
			}
			if (!calculator.PerUnit.IsEmpty)
			{
				calculatorXSD.PerUnitPrice = calculator.PerUnit;
			}
		}

		#endregion

		#region Implementation

		public override XmlSchema Schema
		{
			get { return RatingXmlSchemaDefinitions.Instance.MPUCalculatorSchema; }
		}

		protected override ZString CalculatorType
		{
			get { return MinimumOrPerUnitCalculator.Code; }
		}

		#endregion
	}
}

