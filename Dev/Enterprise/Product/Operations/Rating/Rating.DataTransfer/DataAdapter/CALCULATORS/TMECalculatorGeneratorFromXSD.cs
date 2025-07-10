using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public class TMECalculatorGeneratorFromXSD : BaseCMBCalculatorGeneratorFromXSD<TimeCalculator, Xsd.TMECalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore(RateLine rateLine, TimeCalculator calculator, Xsd.TMECalculator calculatorXSD, IValueObjectImportContext context)
		{
			base.ImportFromValueObjectCore(rateLine, calculator, calculatorXSD, context);

			if (calculatorXSD.UseAccumulationSpecified)
			{
				calculator.IsAccumulated = (calculatorXSD.UseAccumulation == Xsd.TrueFalse.@true);
			}
			calculator.ExcludeHolidays = calculatorXSD.ExcludeDays;
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateLine rateLine, TimeCalculator calculator, Xsd.TMECalculator calculatorXSD, INotifications notifications)
		{
			base.ExportToValueObjectCore(rateLine, calculator, calculatorXSD, notifications);

			if (calculator.IsAccumulated)
			{
				calculatorXSD.UseAccumulation = calculator.IsAccumulated ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
			}
			calculatorXSD.ExcludeDays = calculator.ExcludeHolidays;
		}

		#endregion

		#region Implementation

		public override XmlSchema Schema
		{
			get { return RatingXmlSchemaDefinitions.Instance.TMECalculatorSchema; }
		}

		protected override ZString CalculatorType
		{
			get { return TimeCalculator.Code; }
		}

		#endregion
	}
}

