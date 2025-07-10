using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public class CTGCalculatorGeneratorFromXSD : BaseCMBCalculatorGeneratorFromXSD<CartageCalculator, Xsd.CTGCalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore(RateLine rateLine, CartageCalculator calculator, Xsd.CTGCalculator calculatorXSD, IValueObjectImportContext context)
		{
			base.ImportFromValueObjectCore(rateLine, calculator, calculatorXSD, context);

			calculator.EquipmentType = calculatorXSD.Equipment;

			if (calculatorXSD.ConversionFactorSpecified)
			{
				rateLine.ConversionFactor = new ConversionFactor(calculatorXSD.ConversionFactor, calculatorXSD.ConversionFactorNumeratorUnit, calculatorXSD.ConversionFactorDenominatorUnit);
			}
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateLine rateLine, CartageCalculator calculator, Xsd.CTGCalculator calculatorXSD, INotifications notifications)
		{
			base.ExportToValueObjectCore(rateLine, calculator, calculatorXSD, notifications);

			calculatorXSD.Equipment = calculator.EquipmentType;

			if (!rateLine.ConversionFactor.IsEmpty)
			{
				calculatorXSD.ConversionFactor = rateLine.ConversionFactor.Factor;
				calculatorXSD.ConversionFactorNumeratorUnit = rateLine.ConversionFactor.NumeratorUnit;
				calculatorXSD.ConversionFactorDenominatorUnit = rateLine.ConversionFactor.DenominatorUnit;
			}
		}

		#endregion

		#region Implementation

		public override XmlSchema Schema
		{
			get { return RatingXmlSchemaDefinitions.Instance.CTGCalculatorSchema; }
		}

		protected override ZString CalculatorType
		{
			get { return CartageCalculator.Code; }
		}

		#endregion
	}
}

