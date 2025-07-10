using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public class CMBCalculatorGeneratorFromXSD : BaseCMBCalculatorGeneratorFromXSD<CombinedCalculator, Xsd.CMBCalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore(RateLine rateLine, CombinedCalculator calculator, Xsd.CMBCalculator calculatorXSD, IValueObjectImportContext context)
		{
			base.ImportFromValueObjectCore(rateLine, calculator, calculatorXSD, context);

			if (calculatorXSD.AccumulationSpecified)
			{
				calculator.IsAccumulated = (calculatorXSD.Accumulation == Xsd.TrueFalse.@true);
			}

			if (calculatorXSD.HigherWeightOrUnitLowerRateSpecified)
			{
				calculator.UseHigherChargeableLowerRateRule = (calculatorXSD.HigherWeightOrUnitLowerRate == Xsd.TrueFalse.@true);
			}

			if (calculatorXSD.InclusiveBreaksSpecified)
			{
				calculator.UseInclusiveBreaks = (calculatorXSD.InclusiveBreaks == Xsd.TrueFalse.@true);
			}
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateLine rateLine, CombinedCalculator calculator, Xsd.CMBCalculator calculatorXSD, INotifications notifications)
		{
			base.ExportToValueObjectCore(rateLine, calculator, calculatorXSD, notifications);

			calculatorXSD.Accumulation = calculator.IsAccumulated ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
			calculatorXSD.HigherWeightOrUnitLowerRate = calculator.UseHigherChargeableLowerRateRule ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
			calculatorXSD.InclusiveBreaks = calculator.UseInclusiveBreaks ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;

			ExportBaseOrMinimumRateOnCalculator(calculator, calculatorXSD.SimpleRate, notifications);
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

