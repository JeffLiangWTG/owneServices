using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Core.Transforms.Helper;
using System.Diagnostics;

namespace Tests
{
	[TestClass]
	public class StringMapperTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestRemoveNewLines()
		{
			StringMapper mapper = new StringMapper();
            Assert.AreEqual("ABCD      ", mapper.RemoveNewLines("ABCD", 10));
            Assert.AreEqual("ABCD      ABCD      ", mapper.RemoveNewLines("ABCD\r\nABCD", 10));
			Assert.AreEqual("ABCD ABCD ABCD      ABCD      XYZ       ", 
                            mapper.RemoveNewLines("ABCD ABCD ABCD\r\nABCD\r\nXYZ", 10));
			Assert.AreEqual("ABCD ABCD ABCD      ABCD      XYZ       ", 
                            mapper.RemoveNewLines("ABCD ABCD ABCD\nABCD\nXYZ", 10));
			Assert.AreEqual("ABCD ABCD ABCD      ABCD      XYZ       ", 
                            mapper.RemoveNewLines("ABCD ABCD ABCD\rABCD\rXYZ", 10));
            Assert.AreEqual("ABCD ABCD ABCD      ABCD      XYZ                                                                   ", 
                            mapper.RemoveNewLines("ABCD ABCD ABCD\rABCD\rXYZ", 10, 100));
        }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestPadLeft()
		{
			StringMapper mapper = new StringMapper();
			Assert.AreEqual(" MA", mapper.PadLeft("MA", 3, " "));
			Assert.AreEqual(" MA", mapper.PadLeft("MA", 3, ""));
			Assert.AreEqual("   MA", mapper.PadLeft("MA", 5, null));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestPadLeftException()
		{
			StringMapper mapper = new StringMapper();
			Assert.AreEqual("****MA", mapper.PadLeft("MA", 6, "*%$"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestPadRight()
		{
			StringMapper mapper = new StringMapper();
			Assert.AreEqual("MA ", mapper.PadRight("MA", 3, " "));
			Assert.AreEqual("MA ", mapper.PadRight("MA", 3, ""));
			Assert.AreEqual("MA   ", mapper.PadRight("MA", 5, null));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestPadRightException()
		{
			StringMapper mapper = new StringMapper();
			Assert.AreEqual("MA****", mapper.PadRight("MA", 6, "*%$"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestReplace()
		{
			StringMapper mapper = new StringMapper();
			Assert.AreEqual("This IS only used for testing purpose", mapper.Replace("This is only used for testing purpose", " is", " IS"));
			Assert.AreEqual("30211010", mapper.Replace("30.21.1010", ".", ""));
			Assert.AreEqual("Thisisonlyusedfortestingpurpose", mapper.Replace("This is only used for testing purpose", " ", ""));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestValueMappingWithReturnValue()
		{
			StringMapper mapper = new StringMapper();
			Assert.AreEqual("trueValue", mapper.ValueMappingWithReturnValue("true", "trueValue"));
			Assert.AreEqual("", mapper.ValueMappingWithReturnValue("false", "trueValue"));
			Assert.AreEqual("trueValue", mapper.ValueMappingWithReturnValue("true", "trueValue", "falseValue"));
			Assert.AreEqual("falseValue", mapper.ValueMappingWithReturnValue("false", "trueValue", "falseValue"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestConvertToCsv()
		{
			StringMapper target = new StringMapper();

			string actual;

			string input = "";
			string expected = "";
			actual = target.ConvertToCsv(input, true);
			Assert.AreEqual(expected, actual);

			input = @"Go get one, now
they are going,fast";
			expected = @"Go get one  now they are going fast";
			actual = target.ConvertToCsv(input, true);
			Assert.AreEqual(expected, actual);

			input = "Ford";
			expected = "Ford";
			actual = target.ConvertToCsv(input, false);
			Assert.AreEqual(expected, actual);

			input = " Ford ";
			expected = " Ford ";
			actual = target.ConvertToCsv(input, false);
			Assert.AreEqual(expected, actual);

			input = "Super, luxurious truck";
			expected = "\"Super, luxurious truck\"";
			actual = target.ConvertToCsv(input, false);
			Assert.AreEqual(expected, actual);

			input = "Super, \"luxurious\" truck";
			expected = "\"Super, \"\"luxurious\"\" truck\"";
			actual = target.ConvertToCsv(input, false);
			Assert.AreEqual(expected, actual);

			input = @"Go get one now
they are going fast";
			expected = @"""Go get one now
they are going fast""";
			actual = target.ConvertToCsv(input, false);
			Assert.AreEqual(expected, actual);

			input = @"Go get one now
they are going fast";
			expected = @"""Go get one now
they are going fast""";
			actual = target.ConvertToCsv(input);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFormat()
		{
			var helper = new StringMapper();

			Assert.AreEqual("000123", helper.Format("123", "000000"));
			Assert.AreEqual("00123", helper.Format("123", "#00000"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void FormatDecimal()
		{
			var helper = new StringMapper();

			Assert.AreEqual("", helper.FormatDecimal("", "#.00"));
			Assert.AreEqual(".00", helper.FormatDecimal("", "#.00", true));
			Assert.AreEqual("00000000.000", helper.FormatDecimal("", "00000000.000", true));
			Assert.AreEqual("00000123.123", helper.FormatDecimal("123.123456", "00000000.000"));
		}
	}
}
