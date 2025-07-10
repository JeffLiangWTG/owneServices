using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public class MINCalculatorGeneratorFromXSD : RateCalculatorGeneratorFromXSD<MinimumCalculator, Xsd.MINCalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore(RateLine rateLine, MinimumCalculator calculator, Xsd.MINCalculator calculatorXSD, IValueObjectImportContext context)
		{
			if (calculatorXSD.MinimumValueSpecified)
			{
				calculator.MinimumValue = calculatorXSD.MinimumValue;
			}
			if (calculatorXSD.IsChargeCodeMinimumSpecified)
			{
				calculator.IsChargeCodeMinimum = (calculatorXSD.IsChargeCodeMinimum == Xsd.TrueFalse.@true);
			}
			if (calculatorXSD.IsJobMinimumSpecified)
			{
				calculator.IsJobMinimum = (calculatorXSD.IsJobMinimum == Xsd.TrueFalse.@true);
			}
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateLine rateLine, MinimumCalculator calculator, Xsd.MINCalculator calculatorXSD, INotifications notifications)
		{
			if (!calculator.MinimumValue.IsEmpty)
			{
				calculatorXSD.MinimumValue = calculator.MinimumValue;
			}
			if (!calculator.IsChargeCodeMinimum.IsEmpty)
			{
				calculatorXSD.IsChargeCodeMinimum = calculator.IsChargeCodeMinimum ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
			}
			if (!calculator.IsJobMinimum.IsEmpty)
			{
				calculatorXSD.IsJobMinimum = calculator.IsJobMinimum ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
			}
		}

		#endregion

		#region Implementation

		public override XmlSchema Schema
		{
			get { return RatingXmlSchemaDefinitions.Instance.MINCalculatorSchema; }
		}

		protected override ZString CalculatorType
		{
			get { return MinimumCalculator.Code; }
		}

		#endregion
	}
}

