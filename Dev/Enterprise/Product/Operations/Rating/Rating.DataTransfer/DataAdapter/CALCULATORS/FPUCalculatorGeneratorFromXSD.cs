using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public class FPUCalculatorGeneratorFromXSD : RateCalculatorGeneratorFromXSD<FlatPlusPerUnitCalculator, Xsd.FPUCalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore(RateLine rateLine, FlatPlusPerUnitCalculator calculator, Xsd.FPUCalculator calculatorXSD, IValueObjectImportContext context)
		{
			if (calculatorXSD.BasePriceSpecified)
			{
				calculator.BaseRate = calculatorXSD.BasePrice;
			}
			if (calculatorXSD.PerUnitPriceSpecified)
			{
				calculator.PerUnit = calculatorXSD.PerUnitPrice;
			}
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateLine rateLine, FlatPlusPerUnitCalculator calculator, Xsd.FPUCalculator calculatorXSD, INotifications notifications)
		{
			if (!calculator.BaseRate.IsEmpty)
			{
				calculatorXSD.BasePrice = calculator.BaseRate;
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
			get { return RatingXmlSchemaDefinitions.Instance.FPUCalculatorSchema; }
		}

		protected override ZString CalculatorType
		{
			get { return FlatPlusPerUnitCalculator.Code; }
		}

		#endregion
	}
}

