using System;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Test
{
	[TestFixture]
	public class CFRParserHelperFixture
	{
		[TestCase("1", ExpectedResult = "1")]
		[TestCase("123456", ExpectedResult = "1234")]
		[TestCase("", ExpectedResult = "")]
		public string ParsePrimaryClass(string rawString)
		{
			return CFRParserHelper.ParsePrimaryClass(rawString);
		}

		[TestCase("1", ExpectedResult = "1")]
		[TestCase("123", ExpectedResult = "1")]
		[TestCase("", ExpectedResult = "")]
		public string ParseTertiaryClass(string rawString)
		{
			return CFRParserHelper.ParseTertiaryClass(rawString);
		}

		[TestCase("see", ExpectedResult = false)]
		[TestCase("0", ExpectedResult = false)]
		public bool ParseLimitedQuantityPermittedShouldReturnFalseForZeroValue(string rawString)
		{
			return CFRParserHelper.ParseLimitedQuantityPermitted(rawString);
		}

		[TestCase("see", ExpectedResult = "0")]
		[TestCase("", ExpectedResult = "0")]
		[TestCase("12 kg", ExpectedResult = "12")]
		[TestCase("0", ExpectedResult = "0")]
		public string ParseLimitedQuantityValue(string rawString)
		{
			return CFRParserHelper.ParseLimitedQuantityValue(rawString);
		}

		[TestCase("see", ExpectedResult = "kg")]
		[TestCase("", ExpectedResult = "kg")]
		[TestCase("12 kg", ExpectedResult = "kg")]
		[TestCase("100 L", ExpectedResult = "L")]
		public string ParseLimitedQuantityUnit(string rawString)
		{
			return CFRParserHelper.ParseLimitedQuantityUnit(rawString);
		}

		[TestCase("5000 (2270)", ExpectedResult = "5000")]
		[TestCase("1000 (454)", ExpectedResult = "1000")]
		[TestCase("100 (45.4)", ExpectedResult = "100")]
		[TestCase("10 (4.54)", ExpectedResult = "10")]
		public string ParseReportableQuantityValueReturnsStringOfFirstValue(string rawString)
		{
			return CFRParserHelper.ParseReportableQuantityValue(rawString);
		}

		[Test]
		public void ParseReportableQuantityValueThrowsExceptionForInvalidFormat()
		{
			Assert.Throws<ApplicationException>(() => CFRParserHelper.ParseReportableQuantityValue("234"));
			Assert.Throws<ApplicationException>(() => CFRParserHelper.ParseReportableQuantityValue("111 123 234"));
		}

		[TestCase("+ * A", ExpectedResult = true)]
		[TestCase("* A", ExpectedResult = false)]
		[TestCase("+ *", ExpectedResult = true)]
		[TestCase("+ A", ExpectedResult = true)]
		[TestCase("", ExpectedResult = false)]
		public bool ParseIsPSNFixed(string rawString)
		{
			return CFRParserHelper.ParseIsPSNFixed(rawString);
		}

		[TestCase("+ * A", ExpectedResult = true)]
		[TestCase("* A", ExpectedResult = true)]
		[TestCase("+ *", ExpectedResult = false)]
		[TestCase("+ A", ExpectedResult = true)]
		[TestCase("", ExpectedResult = false)]
		public bool ParseAppliesForAirTransport(string rawString)
		{
			return CFRParserHelper.ParseAppliesForAirTransport(rawString);
		}

		[TestCase("+ * D", ExpectedResult = true)]
		[TestCase("* D", ExpectedResult = true)]
		[TestCase("+ *", ExpectedResult = false)]
		[TestCase("+ D", ExpectedResult = true)]
		[TestCase("", ExpectedResult = false)]
		public bool ParseAppliesForDomesticTransport(string rawString)
		{
			return CFRParserHelper.ParseAppliesForDomesticTransport(rawString);
		}

		[TestCase("+ * I", ExpectedResult = true)]
		[TestCase("* I", ExpectedResult = true)]
		[TestCase("+ *", ExpectedResult = false)]
		[TestCase("+ I", ExpectedResult = true)]
		[TestCase("", ExpectedResult = false)]
		public bool ParseAppliesForInternationalTransport(string rawString)
		{
			return CFRParserHelper.ParseAppliesForInternationalTransport(rawString);
		}

		[TestCase("+ * W", ExpectedResult = true)]
		[TestCase("* W", ExpectedResult = true)]
		[TestCase("+ *", ExpectedResult = false)]
		[TestCase("+ W", ExpectedResult = true)]
		[TestCase("", ExpectedResult = false)]
		public bool ParseAppliesForVesselTransport(string rawString)
		{
			return CFRParserHelper.ParseAppliesForVesselTransport(rawString);
		}

		[TestCase("+ * G", ExpectedResult = true)]
		[TestCase("* G", ExpectedResult = true)]
		[TestCase("+ *", ExpectedResult = false)]
		[TestCase("+ G", ExpectedResult = true)]
		[TestCase("", ExpectedResult = false)]
		public bool ParseRequiresTechnicalNameInParenthesis(string rawString)
		{
			return CFRParserHelper.ParseRequiresTechnicalNameInParenthesis(rawString);
		}

		[TestCase("Forbidden", ExpectedResult = true)]
		[TestCase("10 kg", ExpectedResult = false)]
		[TestCase("", ExpectedResult = false)]
		public bool ParseIsPAXAirRailForbidden(string rawString)
		{
			return CFRParserHelper.ParseIsPAXAirRailForbidden(rawString);
		}

		[TestCase("Forbidden", ExpectedResult = true)]
		[TestCase("10 kg", ExpectedResult = false)]
		[TestCase("", ExpectedResult = false)]
		public bool ParseIsCargoAirRailForbidden(string rawString)
		{
			return CFRParserHelper.ParseIsPAXAirRailForbidden(rawString);
		}

		[TestCase("No limit", ExpectedResult = "kg")]
		[TestCase("Forbidden", ExpectedResult = "kg")]
		[TestCase("10 kg", ExpectedResult = "kg")]
		[TestCase("20 L", ExpectedResult = "L")]
		[TestCase("10 kg or 20 L", ExpectedResult = "kg")]
		[TestCase("20 L or 10 kg", ExpectedResult = "L")]
		[TestCase("", ExpectedResult = "kg")]
		public string ParsePAXAirRailLimitUnit(string rawString)
		{
			return CFRParserHelper.ParsePAXAirRailLimitUnit(rawString, 0);
		}

		[TestCase("Forbidden", ExpectedResult = "kg")]
		[TestCase("10 kg", ExpectedResult = "kg")]
		[TestCase("20 L", ExpectedResult = "L")]
		[TestCase("10 kg or 20 L", ExpectedResult = "kg")]
		[TestCase("20 L or 10 kg", ExpectedResult = "L")]
		[TestCase("", ExpectedResult = "kg")]
		public string ParseCargoAirRailLimitUnit(string rawString)
		{
			return CFRParserHelper.ParseCargoAirRailLimitUnit(rawString, 0);
		}

		[TestCase("No limit", ExpectedResult = "kg")]
		[TestCase("Forbidden", ExpectedResult = "kg")]
		[TestCase("10 kg", ExpectedResult = "kg")]
		[TestCase("20 L", ExpectedResult = "kg")]
		[TestCase("10 kg or 20 L", ExpectedResult = "L")]
		[TestCase("20 L or 10 kg", ExpectedResult = "kg")]
		[TestCase("", ExpectedResult = "kg")]
		public string ParseSecondaryPAXAirRailLimitUnit(string rawString)
		{
			return CFRParserHelper.ParsePAXAirRailLimitUnit(rawString, 1);
		}

		[TestCase("No limit", ExpectedResult = "kg")]
		[TestCase("Forbidden", ExpectedResult = "kg")]
		[TestCase("10 kg", ExpectedResult = "kg")]
		[TestCase("20 L", ExpectedResult = "kg")]
		[TestCase("10 kg or 20 L", ExpectedResult = "L")]
		[TestCase("20 L or 10 kg", ExpectedResult = "kg")]
		[TestCase("", ExpectedResult = "kg")]
		public string ParseSecondaryCargoAirRailLimitUnit(string rawString)
		{
			return CFRParserHelper.ParseCargoAirRailLimitUnit(rawString, 1);
		}

		[TestCase("Forbidden", ExpectedResult = "0")]
		[TestCase("10 kg", ExpectedResult = "10")]
		[TestCase("20 L", ExpectedResult = "20")]
		[TestCase("10 kg or 20 L", ExpectedResult = "10")]
		[TestCase("20 L or 10 kg", ExpectedResult = "20")]
		[TestCase("", ExpectedResult = "0")]
		public string ParsePAXAirRailLimitValue(string rawString)
		{
			return CFRParserHelper.ParsePAXAirRailLimitValue(rawString, 0);
		}

		[TestCase("Forbidden", ExpectedResult = "0")]
		[TestCase("10 kg", ExpectedResult = "10")]
		[TestCase("20 L", ExpectedResult = "20")]
		[TestCase("10 kg or 20 L", ExpectedResult = "10")]
		[TestCase("20 L or 10 kg", ExpectedResult = "20")]
		[TestCase("", ExpectedResult = "0")]
		public string ParseCargoAirRailLimitValue(string rawString)
		{
			return CFRParserHelper.ParseCargoAirRailLimitValue(rawString, 0);
		}

		[TestCase("Forbidden", ExpectedResult = "0")]
		[TestCase("10 kg", ExpectedResult = "0")]
		[TestCase("20 L", ExpectedResult = "0")]
		[TestCase("10 kg or 20 L", ExpectedResult = "20")]
		[TestCase("20 L or 10 kg", ExpectedResult = "10")]
		[TestCase("", ExpectedResult = "0")]
		public string ParseSecondaryPAXAirRailLimitValue(string rawString)
		{
			return CFRParserHelper.ParsePAXAirRailLimitValue(rawString, 1);
		}

		[TestCase("Forbidden", ExpectedResult = "0")]
		[TestCase("10 kg", ExpectedResult = "0")]
		[TestCase("20 L", ExpectedResult = "0")]
		[TestCase("10 kg or 20 L", ExpectedResult = "20")]
		[TestCase("20 L or 10 kg", ExpectedResult = "10")]
		[TestCase("", ExpectedResult = "0")]
		public string ParseSecondaryCargoAirRailLimitValue(string rawString)
		{
			return CFRParserHelper.ParseCargoAirRailLimitValue(rawString, 1);
		}

		[TestCase("See A105", ExpectedResult = "")]
		[TestCase("10 L gross", ExpectedResult = "GLM")]
		[TestCase("Forbidden", ExpectedResult = "FOB")]
		[TestCase("No limit", ExpectedResult = "NLT")]
		[TestCase("10 kg", ExpectedResult = "NLM")]
		public string ParsePAXAirRailLimitType(string rawString)
		{
			return CFRParserHelper.ParsePAXAirRailLimitType(rawString);
		}

		[TestCase("See A105", ExpectedResult = "")]
		[TestCase("Forbidden", ExpectedResult = "FOB")]
		[TestCase("No limit", ExpectedResult = "NLT")]
		[TestCase("10 kg", ExpectedResult = "NLM")]
		[TestCase("10 kg gross", ExpectedResult = "GLM")]
		public string ParseCargoAirRailLimitType(string rawString)
		{
			return CFRParserHelper.ParseCargoAirRailLimitType(rawString);
		}
	}
}
