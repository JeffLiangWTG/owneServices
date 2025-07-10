using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public class UNTCalculatorGeneratorFromXSD : RateCalculatorGeneratorFromXSD<UnitCalculator, Xsd.UNTCalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore(RateLine rateLine, UnitCalculator calculator, Xsd.UNTCalculator calculatorXSD, IValueObjectImportContext context)
		{
			if (calculatorXSD.PerUnitPriceSpecified)
			{
				calculator.PerUnit = calculatorXSD.PerUnitPrice;
			}
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateLine rateLine, UnitCalculator calculator, Xsd.UNTCalculator calculatorXSD, INotifications notifications)
		{
			if (!calculator.PerUnit.IsEmpty)
			{
				calculatorXSD.PerUnitPrice = calculator.PerUnit;
			}
		}

		#endregion

		#region Implementation

		public override XmlSchema Schema
		{
			get { return RatingXmlSchemaDefinitions.Instance.UNTCalculatorSchema; }
		}

		protected override ZString CalculatorType
		{
			get { return UnitCalculator.Code; }
		}

		#endregion
	}
}

