using System;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators.Testing
{
	public class AGYCalculatorGeneratorFromXSDTest : RateCalculatorGeneratorFromXSDTest<AgencyCalculator, Xsd.AGYCalculator>
	{
		#region Override ImportCore Method

		protected override void ImportFromValueObjectCore()
		{
			Xsd.AGYCalculator calculatorXSD = CalculatorXSDForTest(false);
			Buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Buffer);
			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator is an Agency Caluclator", typeof(AgencyCalculator), RateLineForCalculatorXSDTest.Calculator.GetType());

			AssertEquals("Calculator 's Agency Fee Type", "ENT", CalculatorGenerated.AgencyFeeType);
			AssertEquals("Calculator 's Agency Line Type", "TRE", CalculatorGenerated.AgencyLineType);
			AssertEquals("Calculator 's Hide Fee Line Type On Quote", false, CalculatorGenerated.HideFeeLineTypeOnQuote);
			AssertEquals("Calculator 's Message Type", "IMP", CalculatorGenerated.MessageType);
			AssertEquals("Calculator 's Message Sub Type", "SAC", CalculatorGenerated.MessageSubType);
			AssertEquals("Calculator 's Hide Message Type On Quote", false, CalculatorGenerated.HideMessageTypeOnQuote);
			AssertEquals("Calculator 's Agency Rate", (ZDecimal)0, CalculatorGenerated.AgencyRate);
			AssertEquals("Calculator 's Included Headers", 1, CalculatorGenerated.IncludedHeaders);
			AssertEquals("Calculator 's Additional Rate", (ZDecimal)0, CalculatorGenerated.AdditionalRate);
			AssertEquals("Calculator 's Included Lines", (ZInt)0, CalculatorGenerated.IncludedLines);
			AssertEquals("Calculator 's Per Additional Line", (ZDecimal)0, CalculatorGenerated.PerAdditionalLine);
			AssertEquals("Calculator 's Maximum Lines", (ZInt)0, CalculatorGenerated.MaximumLines);
			AssertEquals("Calculator 's Maximum", (ZDecimal)0, CalculatorGenerated.Maximum);

			calculatorXSD = CalculatorXSDForTest(true);
			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator 's Agency Fee Type", "ENT", CalculatorGenerated.AgencyFeeType);
			AssertEquals("Calculator 's Agency Line Type", "TRE", CalculatorGenerated.AgencyLineType);
			AssertEquals("Calculator 's Hide Fee Line Type On Quote", true, CalculatorGenerated.HideFeeLineTypeOnQuote);
			AssertEquals("Calculator 's Message Type", "IMP", CalculatorGenerated.MessageType);
			AssertEquals("Calculator 's Message Sub Type", "SAC", CalculatorGenerated.MessageSubType);
			AssertEquals("Calculator 's Hide Message Type On Quote", true, CalculatorGenerated.HideMessageTypeOnQuote);
			AssertEquals("Calculator 's Agency Rate", 12.5m, CalculatorGenerated.AgencyRate);
			AssertEquals("Calculator 's Included Headers", 2, CalculatorGenerated.IncludedHeaders);
			AssertEquals("Calculator 's Additional Rate", 8.5m, CalculatorGenerated.AdditionalRate);
			AssertEquals("Calculator 's Included Lines", 22, CalculatorGenerated.IncludedLines);
			AssertEquals("Calculator 's Per Additional Line", 32.5m, CalculatorGenerated.PerAdditionalLine);
			AssertEquals("Calculator 's Maximum Lines", 10, CalculatorGenerated.MaximumLines);
			AssertEquals("Calculator 's Maximum", 20.5m, CalculatorGenerated.Maximum);
		}

		#endregion

		#region Override ExportCore Method

		protected override void ExportToValueObjectCore()
		{
			Xsd.AGYCalculator calculatorXSD = new Xsd.AGYCalculator();

			CalculatorGenerator.ExportToValueObject(RateLineForCalculatorTest, calculatorXSD, new NotificationBuffer());

			AssertEquals("Calculator 's Agency Fee Type", "ENT", calculatorXSD.FeeType);
			AssertEquals("Calculator 's Agency Line Type", "TRE", calculatorXSD.LineType);
			AssertEquals("Calculator 's Hide Fee Line Type On Quote", Xsd.TrueFalse.@true, calculatorXSD.HideFeeAndLineTypeOnQuotation);
			AssertEquals("Calculator 's Hide Fee Line Type On Quote Specified", true, calculatorXSD.HideFeeAndLineTypeOnQuotationSpecified);
			AssertEquals("Calculator 's Message Type", "IMP", calculatorXSD.Type);
			AssertEquals("Calculator 's Message Sub Type", "SAC", calculatorXSD.Style);
			AssertEquals("Calculator 's Hide Message Type On Quote", Xsd.TrueFalse.@true, calculatorXSD.HideTypeAndStyleOnQuotation);
			AssertEquals("Calculator 's Hide Message Type On Quote Specified", true, calculatorXSD.HideTypeAndStyleOnQuotationSpecified);
			AssertEquals("Calculator 's Agency Rate", 12.5m, calculatorXSD.Rate);
			AssertEquals("Calculator 's Agency Rate Specified", true, calculatorXSD.RateSpecified);
			AssertEquals("Calculator 's Included Headers", 2, calculatorXSD.IncludedHeaders);
			AssertEquals("Calculator 's Included Headers Specified", true, calculatorXSD.IncludedHeadersSpecified);
			AssertEquals("Calculator 's Additional Rate", 8.5m, calculatorXSD.RatePerAddl);
			AssertEquals("Calculator 's Additional Rate Specified", true, calculatorXSD.RatePerAddlSpecified);
			AssertEquals("Calculator 's Included Lines", 22, calculatorXSD.IncludedLines);
			AssertEquals("Calculator 's Included Lines Specified", true, calculatorXSD.IncludedLinesSpecified);
			AssertEquals("Calculator 's Per Additional Line", 32.5m, calculatorXSD.RatePerAddLine);
			AssertEquals("Calculator 's Per Additional Line Specified", true, calculatorXSD.RatePerAddLineSpecified);
			AssertEquals("Calculator 's Maximum Lines", 10, calculatorXSD.MaximumLines);
			AssertEquals("Calculator 's Maximum Lines Specified", true, calculatorXSD.MaximumLinesSpecified);
			AssertEquals("Calculator 's Maximum", 20.5m, calculatorXSD.Maximum);
			AssertEquals("Calculator 's Maximum Specified", true, calculatorXSD.MaximumSpecified);
		}

		#endregion

		#region Override Members

		protected override ZString CalculatorType
		{
			get { return AgencyCalculator.Code; }
		}

		protected override Type ExpectedGeneratorType
		{
			get { return typeof(AGYCalculatorGeneratorFromXSD); }
		}

		protected override RateCalculatorGeneratorFromXSD<AgencyCalculator, Xsd.AGYCalculator> CalculatorGenerator
		{
			get { return new AGYCalculatorGeneratorFromXSD(); }
		}

		#endregion

		#region Data For Test

		protected override Xsd.AGYCalculator CalculatorXSDForTest(bool specified)
		{
			calculatorXSD = new Xsd.AGYCalculator();

			calculatorXSD.FeeType = "ENT";
			calculatorXSD.LineType = RateLineTypeList.Codes.PerTariffLinePerEntry;
			calculatorXSD.Type = "IMP";
			calculatorXSD.Style = "SAC";
			if (specified)
			{
				calculatorXSD.HideFeeAndLineTypeOnQuotation = Xsd.TrueFalse.@true;
				calculatorXSD.HideTypeAndStyleOnQuotation = Xsd.TrueFalse.@true;
				calculatorXSD.Rate = 12.5m;
				calculatorXSD.IncludedHeaders = 2;
				calculatorXSD.RatePerAddl = 8.5m;
				calculatorXSD.IncludedLines = 22;
				calculatorXSD.RatePerAddLine = 32.5m;
				calculatorXSD.MaximumLines = 10;
				calculatorXSD.Maximum = 20.5m;
			}

			return calculatorXSD;
		}

		protected override void SetTestDataCalculatorForTest(AgencyCalculator calculator)
		{
			calculator.AgencyFeeType = "ENT";
			calculator.AgencyLineType = RateLineTypeList.Codes.PerTariffLinePerEntry;
			calculator.HideFeeLineTypeOnQuote = true;
			calculator.MessageType = "IMP";
			calculator.MessageSubType = "SAC";
			calculator.HideMessageTypeOnQuote = true;
			calculator.AgencyRate = 12.5m;
			calculator.IncludedHeaders = 2;
			calculator.AdditionalRate = 8.5m;
			calculator.IncludedLines = 22;
			calculator.PerAdditionalLine = 32.5m;
			calculator.MaximumLines = 10;
			calculator.Maximum = 20.5m;
		}

		#endregion
	}
}
