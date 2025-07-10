using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public class AGYCalculatorGeneratorFromXSD : RateCalculatorGeneratorFromXSD<AgencyCalculator, Xsd.AGYCalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore(RateLine rateLine, AgencyCalculator calculator, Xsd.AGYCalculator calculatorXSD, IValueObjectImportContext context)
		{
			calculator.AgencyFeeType = calculatorXSD.FeeType;
			calculator.AgencyLineType = calculatorXSD.LineType;

			if (calculatorXSD.HideFeeAndLineTypeOnQuotationSpecified)
			{
				calculator.HideFeeLineTypeOnQuote = (calculatorXSD.HideFeeAndLineTypeOnQuotation == Xsd.TrueFalse.@true);
			}

			calculator.MessageType = calculatorXSD.Type;
			calculator.MessageSubType = calculatorXSD.Style;

			if (calculatorXSD.HideTypeAndStyleOnQuotationSpecified)
			{
				calculator.HideMessageTypeOnQuote = (calculatorXSD.HideTypeAndStyleOnQuotation == Xsd.TrueFalse.@true);
			}

			if (calculatorXSD.RateSpecified)
			{
				calculator.AgencyRate = calculatorXSD.Rate;
			}

			if (calculatorXSD.IncludedHeadersSpecified)
			{
				calculator.IncludedHeaders = calculatorXSD.IncludedHeaders;
			}

			if (calculatorXSD.RatePerAddlSpecified)
			{
				calculator.AdditionalRate = calculatorXSD.RatePerAddl;
			}
			if (calculatorXSD.IncludedLinesSpecified)
			{
				calculator.IncludedLines = calculatorXSD.IncludedLines;
			}
			if (calculatorXSD.RatePerAddLineSpecified)
			{
				calculator.PerAdditionalLine = calculatorXSD.RatePerAddLine;
			}
			if (calculatorXSD.MaximumLinesSpecified)
			{
				calculator.MaximumLines = calculatorXSD.MaximumLines;
			}
			if (calculatorXSD.MaximumSpecified)
			{
				calculator.Maximum = calculatorXSD.Maximum;
			}
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateLine rateLine, AgencyCalculator calculator, Xsd.AGYCalculator calculatorXSD, INotifications notifications)
		{
			calculatorXSD.FeeType = calculator.AgencyFeeType;
			calculatorXSD.LineType = calculator.AgencyLineType;
			calculatorXSD.HideFeeAndLineTypeOnQuotation = calculator.HideFeeLineTypeOnQuote ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
			calculatorXSD.Type = calculator.MessageType;
			calculatorXSD.Style = calculator.MessageSubType;
			calculatorXSD.HideTypeAndStyleOnQuotation = calculator.HideMessageTypeOnQuote ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
			calculatorXSD.Rate = calculator.AgencyRate;
			calculatorXSD.IncludedHeaders = calculator.IncludedHeaders;
			calculatorXSD.RatePerAddl = calculator.AdditionalRate;
			calculatorXSD.IncludedLines = calculator.IncludedLines;
			calculatorXSD.RatePerAddLine = calculator.PerAdditionalLine;
			calculatorXSD.MaximumLines = calculator.MaximumLines;
			calculatorXSD.Maximum = calculator.Maximum;
		}

		#endregion

		#region Implementation

		public override XmlSchema Schema
		{
			get { return RatingXmlSchemaDefinitions.Instance.AGYCalculatorSchema; }
		}

		protected override ZString CalculatorType
		{
			get { return AgencyCalculator.Code; }
		}

		#endregion
	}
}

