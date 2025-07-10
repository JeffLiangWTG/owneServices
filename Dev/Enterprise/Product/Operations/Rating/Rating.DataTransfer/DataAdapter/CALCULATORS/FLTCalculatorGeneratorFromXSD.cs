using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public class FLTCalculatorGeneratorFromXSD : RateCalculatorGeneratorFromXSD<FlatCalculator, Xsd.FLTCalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore(RateLine rateLine, FlatCalculator calculator, Xsd.FLTCalculator calculatorXSD, IValueObjectImportContext context)
		{
			if (calculatorXSD.BasePriceSpecified)
			{
				calculator.BaseRate = calculatorXSD.BasePrice;
			}
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateLine rateLine, FlatCalculator calculator, Xsd.FLTCalculator calculatorXSD, INotifications notifications)
		{
			if (!calculator.BaseRate.IsEmpty)
			{
				calculatorXSD.BasePrice = calculator.BaseRate;
			}
		}

		#endregion

		#region Implementation

		public override XmlSchema Schema
		{
			get { return RatingXmlSchemaDefinitions.Instance.FLTCalculatorSchema; }
		}

		protected override ZString CalculatorType
		{
			get { return FlatCalculator.Code; }
		}

		#endregion
	}
}

