
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public class VEDCalculatorGeneratorFromXSD : RateCalculatorGeneratorFromXSD<EqualizationCalculator, Xsd.VEDCalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore(RateLine rateLine, EqualizationCalculator calculator, Xsd.VEDCalculator calculatorXSD, IValueObjectImportContext context)
		{
			ImportRateItemWithOperatorAndBreak(calculator, calculatorXSD.RateItems, context);

			if (calculatorXSD.InclusiveBreaksSpecified)
			{
				calculator.UseInclusiveBreaks = (calculatorXSD.InclusiveBreaks == Xsd.TrueFalse.@true);
			}
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateLine rateLine, EqualizationCalculator calculator, Xsd.VEDCalculator calculatorXSD, INotifications notifications)
		{
			ExportRateItemWithOperatorAndBreak(calculator, calculatorXSD.RateItems, notifications);
			calculatorXSD.InclusiveBreaks = calculator.UseInclusiveBreaks ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
		}

		#endregion

		#region Implementation

		public override XmlSchema Schema
		{
			get { return RatingXmlSchemaDefinitions.Instance.CMBCalculatorSchema; }
		}

		protected override ZString CalculatorType
		{
			get { return CombinedCalculator.Code; }
		}

		#endregion
	}
}
