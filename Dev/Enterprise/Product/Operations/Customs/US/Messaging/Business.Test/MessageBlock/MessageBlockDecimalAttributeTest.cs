using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Testing
{
	sealed class DecimalMessageBlockAttributeTest : TestCase
	{
		public void TestFillType()
		{
			ZDecimal value = 12.3;
			MessageBlockDecimalAttribute attribute = new MessageBlockDecimalAttribute(5, 1, "M", 1);
			AssertEquals("00123", attribute.Serialise(null, value, false));
			AssertEquals("12.3", attribute.Serialise(null, value, true));
			attribute = new MessageBlockDecimalAttribute(5, 1, "M", 1, FillType.AlwaysSpaceFill);
			AssertEquals("  123", attribute.Serialise(null, value, false));
			AssertEquals("12.3", attribute.Serialise(null, value, true));
			attribute = new MessageBlockDecimalAttribute(5, 1, "M", 1, FillType.AlwaysZeroFill);
			AssertEquals("00123", attribute.Serialise(null, value, false));
			AssertEquals("12.3", attribute.Serialise(null, value, true));
			attribute = new MessageBlockDecimalAttribute(5, 1, "M", 1, FillType.ZeroFillUnlessEmpty);
			AssertEquals("00123", attribute.Serialise(null, value, false));
			AssertEquals("12.3", attribute.Serialise(null, value, true));
			value = ZDecimal.Zero;
			attribute = new MessageBlockDecimalAttribute(5, 1, "M", 1);
			AssertEquals("00000", attribute.Serialise(null, value, false));
			AssertEquals("0.0", attribute.Serialise(null, value, true));
			attribute = new MessageBlockDecimalAttribute(5, 1, "O", 1);
			AssertEquals("     ", attribute.Serialise(null, value, false));
			AssertEquals("", attribute.Serialise(null, value, true));
			attribute = new MessageBlockDecimalAttribute(5, 1, "M", 1, FillType.AlwaysSpaceFill);
			AssertEquals("    0", attribute.Serialise(null, value, false));
			AssertEquals("0.0", attribute.Serialise(null, value, true));
			attribute = new MessageBlockDecimalAttribute(5, 1, "M", 1, FillType.AlwaysZeroFill);
			AssertEquals("00000", attribute.Serialise(null, value, false));
			AssertEquals("0.0", attribute.Serialise(null, value, true));
			attribute = new MessageBlockDecimalAttribute(5, 1, "M", 1, FillType.ZeroFillUnlessEmpty);
			AssertEquals("     ", attribute.Serialise(null, value, false));
			AssertEquals("", attribute.Serialise(null, value, true));
		}

		public void TestDeserialiseEmptyDecimal()
		{
			AssertEquals(ZDecimal.Zero, new MessageBlockDecimalAttribute(5, 1, "M", 0).DeSerialise("     "));
		}

		public void TestDeserialiseDecimal()
		{
			AssertEquals(3.14159m, new MessageBlockDecimalAttribute(6, 1, "M", 5).DeSerialise("314159"));
		}

		public void TestSerialiseZDecimalNoDecimals()
		{
			AssertEquals("000120", new MessageBlockDecimalAttribute(6, 1, "M", 0).Serialise(null, new ZDecimal(120)));
		}

		public void TestDeserialiseNegativeDecimal()
		{
			AssertEquals(-3.14159m, new MessageBlockDecimalAttribute(10, 1, "M", 5, true).DeSerialise("000-314159"));
			AssertEquals(-3.14159m, new MessageBlockDecimalAttribute(10, 1, "M", 5, true).DeSerialise("-000314159"));

			CombineAssertions(() =>
			{
				var attribute = new MessageBlockDecimalAttribute(6, 1, "M", 2, true);
				AssertEquals("Case 01", -0.12m, attribute.DeSerialise("   -12"));
				AssertEquals("Case 02", -1.23m, attribute.DeSerialise("  -123"));
				AssertEquals("Case 03", -0.12m, attribute.DeSerialise("  0-12"));
				AssertEquals("Case 04", -1.23m, attribute.DeSerialise(" 0-123"));
				AssertEquals("Case 05", -0.12m, attribute.DeSerialise(" 00-12"));
				AssertEquals("Case 06", -1.23m, attribute.DeSerialise("00-123"));
				AssertEquals("Case 07", -1.23m, attribute.DeSerialise("  123-"));
				AssertEquals("Case 08", -0.12m, attribute.DeSerialise("   -12"));
				AssertEquals("Case 09", -12.31m, attribute.DeSerialise(" 123-1"));
				AssertEquals("Case 10", -11.20m, attribute.DeSerialise(" 1-120"));
			});
		}

		public void TestSerialiseDecimalTwoDecimals()
		{
			AssertEquals("000120", new MessageBlockDecimalAttribute(6, 1, "M", 2).Serialise(null, new ZDecimal(1.20)));
		}

		public void TestSerialiseDecimalTwoDecimalsMaxLength()
		{
			AssertEquals("999999", new MessageBlockDecimalAttribute(6, 1, "M", 2).Serialise(null, new ZDecimal(9999.99)));
		}

		public void TestSerialiseNegativeValue()
		{
			AssertEquals("******", new MessageBlockDecimalAttribute(6, 1, "M", 2).Serialise(null, new ZDecimal(-239.78)));
			AssertEquals("-0023978", new MessageBlockDecimalAttribute(8, 1, "M", 2, true).Serialise(null, new ZDecimal(-239.78)));
			AssertEquals("-239.78", new MessageBlockDecimalAttribute(8, 1, "M", 2, true).Serialise(null, new ZDecimal(-239.78), true));
		}

		public void TestSerialiseOutOfRangeDecimalPlaces()
		{
			AssertEquals("******", new MessageBlockDecimalAttribute(6, 1, "M", 2).Serialise(null, new ZDecimal(1.213)));
			AssertEquals("******", new MessageBlockDecimalAttribute(6, 1, "M", 2).Serialise(null, new ZDecimal(10001.20)));
			AssertEquals("******", new MessageBlockDecimalAttribute(6, 1, "M", 2).Serialise(null, new ZDecimal(-10)));
		}
	}
}
