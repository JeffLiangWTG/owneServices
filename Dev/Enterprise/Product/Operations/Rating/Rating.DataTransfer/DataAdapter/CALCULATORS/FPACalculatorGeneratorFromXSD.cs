using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public class FPACalculatorGeneratorFromXSD : RateCalculatorGeneratorFromXSD<FirstPlusAdditionalCalculator, Xsd.FPACalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore(RateLine rateLine, FirstPlusAdditionalCalculator calculator, Xsd.FPACalculator calculatorXSD, IValueObjectImportContext context)
		{
			if (calculatorXSD.FirstItemPriceSpecified)
			{
				calculator.First = calculatorXSD.FirstItemPrice;
			}
			if (calculatorXSD.AddlItemPriceSpecified)
			{
				calculator.Additional = calculatorXSD.AddlItemPrice;
			}
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateLine rateLine, FirstPlusAdditionalCalculator calculator, Xsd.FPACalculator calculatorXSD, INotifications notifications)
		{
			if (!calculator.First.IsEmpty)
			{
				calculatorXSD.FirstItemPrice = calculator.First;
			}

			if (!calculator.Additional.IsEmpty)
			{
				calculatorXSD.AddlItemPrice = calculator.Additional;
			}
		}

		#endregion

		#region Implementation

		public override XmlSchema Schema
		{
			get { return RatingXmlSchemaDefinitions.Instance.FPACalculatorSchema; }
		}

		protected override ZString CalculatorType
		{
			get { return FirstPlusAdditionalCalculator.Code; }
		}

		#endregion
	}
}

