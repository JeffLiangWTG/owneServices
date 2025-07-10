using System;
using CargoWise.Types;
using Enterprise.Messaging.Business.AWB;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.Testing
{
	[TestedType(typeof(AWBMessageBlockDecimalAttribute))]
	sealed class AWBMessageBlockDecimalAttributeTest : AWBMessageBlockAttributeTest<AWBMessageBlockDecimalAttribute>
	{
		public void TestInvalidCharType()
		{
#if NETFRAMEWORK
			AssertExceptionThrown<ArgumentOutOfRangeException>("Alpha", "charType must be either Numeric or NumericWithDecimal.\r\nParameter name: charType", () => new AWBMessageBlockDecimalAttribute(1, 1, 3, StatusType.Mandatory, CharType.Alpha));
			AssertExceptionThrown<ArgumentOutOfRangeException>("AlphaNumeric", "charType must be either Numeric or NumericWithDecimal.\r\nParameter name: charType", () => new AWBMessageBlockDecimalAttribute(1, 1, 3, StatusType.Mandatory, CharType.AlphaNumeric));
			AssertExceptionThrown<ArgumentOutOfRangeException>("Special", "charType must be either Numeric or NumericWithDecimal.\r\nParameter name: charType", () => new AWBMessageBlockDecimalAttribute(1, 1, 3, StatusType.Mandatory, CharType.Special));
			AssertExceptionThrown<ArgumentOutOfRangeException>("Text", "charType must be either Numeric or NumericWithDecimal.\r\nParameter name: charType", () => new AWBMessageBlockDecimalAttribute(1, 1, 3, StatusType.Mandatory, CharType.Text));
#else
			AssertExceptionThrown<ArgumentOutOfRangeException>("Alpha", "charType must be either Numeric or NumericWithDecimal. (Parameter 'charType')", () => new AWBMessageBlockDecimalAttribute(1, 1, 3, StatusType.Mandatory, CharType.Alpha));
			AssertExceptionThrown<ArgumentOutOfRangeException>("AlphaNumeric", "charType must be either Numeric or NumericWithDecimal. (Parameter 'charType')", () => new AWBMessageBlockDecimalAttribute(1, 1, 3, StatusType.Mandatory, CharType.AlphaNumeric));
			AssertExceptionThrown<ArgumentOutOfRangeException>("Special", "charType must be either Numeric or NumericWithDecimal. (Parameter 'charType')", () => new AWBMessageBlockDecimalAttribute(1, 1, 3, StatusType.Mandatory, CharType.Special));
			AssertExceptionThrown<ArgumentOutOfRangeException>("Text", "charType must be either Numeric or NumericWithDecimal. (Parameter 'charType')", () => new AWBMessageBlockDecimalAttribute(1, 1, 3, StatusType.Mandatory, CharType.Text));
#endif
		}

		public void TestDeserialiseEmptyDecimal()
		{
			AssertEquals(ZDecimal.Zero, new AWBMessageBlockDecimalAttribute(1, 1, 5, StatusType.Mandatory, CharType.Numeric).DeSerialise("     "));
		}

		public void TestSerialiseNegativeValue()
		{
			AssertEquals("??", new AWBMessageBlockDecimalAttribute(1, 2, 6, StatusType.Mandatory, CharType.Numeric).Serialise(new ZDecimal(-239.78)));
		}

		public void TestSerialiseOutOfRangeDecimalPlaces()
		{
			CombineAssertions(() =>
			{
				AssertEquals("?", new AWBMessageBlockDecimalAttribute(1, 1, 3, StatusType.Mandatory, CharType.NumericWithDecimal).Serialise(new ZDecimal(1.21)));
				AssertEquals("?", new AWBMessageBlockDecimalAttribute(1, 1, 7, StatusType.Mandatory, CharType.NumericWithDecimal).Serialise(new ZDecimal(10001.23)));
				AssertEquals("?", new AWBMessageBlockDecimalAttribute(1, 1, 2, StatusType.Mandatory, CharType.Numeric).Serialise(new ZDecimal(-10)));
				AssertEquals("?1", new AWBMessageBlockDecimalAttribute(1, 2, 3, StatusType.Mandatory, CharType.Numeric).Serialise(new ZDecimal(1)));
			});
		}

		public override void TestSerialise()
		{
			CombineAssertions(() =>
			{
				AssertEquals("23.4", new AWBMessageBlockDecimalAttribute(1, 1, 4, StatusType.Mandatory, CharType.NumericWithDecimal).Serialise(new ZDecimal(23.4)));
				AssertEquals("0", new AWBMessageBlockDecimalAttribute(1, 1, 4, StatusType.Mandatory, CharType.NumericWithDecimal).Serialise(ZDecimal.Zero));
				AssertEquals("?", new AWBMessageBlockDecimalAttribute(1, 1, 4, StatusType.Mandatory, CharType.NumericWithDecimal).Serialise(new ZDecimal(-10)));
				AssertEquals("?5", new AWBMessageBlockDecimalAttribute(1, 2, 4, StatusType.Mandatory, CharType.Numeric).Serialise(new ZDecimal(5)));
				AssertEquals("?0", new AWBMessageBlockDecimalAttribute(1, 2, 4, StatusType.Mandatory, CharType.Numeric).Serialise(ZDecimal.Zero));
				AssertEquals("23.4", new AWBMessageBlockDecimalAttribute(1, 1, 4, StatusType.Optional, CharType.NumericWithDecimal).Serialise(new ZDecimal(23.4)));
				AssertEquals("", new AWBMessageBlockDecimalAttribute(1, 1, 4, StatusType.Optional, CharType.NumericWithDecimal).Serialise(ZDecimal.Zero));
				AssertEquals("?", new AWBMessageBlockDecimalAttribute(1, 1, 4, StatusType.Optional, CharType.NumericWithDecimal).Serialise(new ZDecimal(-10)));
				AssertEquals("?5", new AWBMessageBlockDecimalAttribute(1, 2, 4, StatusType.Optional, CharType.Numeric).Serialise(new ZDecimal(5)));
				AssertEquals("", new AWBMessageBlockDecimalAttribute(1, 2, 4, StatusType.Optional, CharType.Numeric).Serialise(ZDecimal.Zero));
				AssertEquals("23.4", new AWBMessageBlockDecimalAttribute(1, 1, 4, StatusType.Conditional, CharType.NumericWithDecimal).Serialise(new ZDecimal(23.4)));
				AssertEquals("", new AWBMessageBlockDecimalAttribute(1, 1, 4, StatusType.Conditional, CharType.NumericWithDecimal).Serialise(ZDecimal.Zero));
				AssertEquals("?", new AWBMessageBlockDecimalAttribute(1, 1, 4, StatusType.Conditional, CharType.NumericWithDecimal).Serialise(new ZDecimal(-10)));
				AssertEquals("?5", new AWBMessageBlockDecimalAttribute(1, 2, 4, StatusType.Conditional, CharType.Numeric).Serialise(new ZDecimal(5)));
				AssertEquals("", new AWBMessageBlockDecimalAttribute(1, 2, 4, StatusType.Conditional, CharType.Numeric).Serialise(ZDecimal.Zero));
				AssertEquals("?", new AWBMessageBlockDecimalAttribute(1, 1, 4, StatusType.Mandatory, CharType.Numeric).Serialise(new ZDecimal(23.4)));
				AssertEquals("0", new AWBMessageBlockDecimalAttribute(1, 1, 4, StatusType.Mandatory, CharType.Numeric).Serialise(ZDecimal.Zero));
				AssertEquals("?", new AWBMessageBlockDecimalAttribute(1, 1, 4, StatusType.Mandatory, CharType.Numeric).Serialise(new ZDecimal(-10)));
				AssertEquals("23", new AWBMessageBlockDecimalAttribute(1, 1, 4, StatusType.Mandatory, CharType.Numeric).Serialise(new ZDecimal(23)));
			});
		}

		public override void TestDeSerialise()
		{
			AssertEquals(314159m, new AWBMessageBlockDecimalAttribute(1, 1, 6, StatusType.Mandatory, CharType.Numeric).DeSerialise("314159"));
		}

		protected override ZString ValidValue => "1234.56";

		protected override IZType InvalidValue => new ZDecimal(-232);

		protected override AWBMessageBlockDecimalAttribute CreateAttribute() => new AWBMessageBlockDecimalAttribute(1, 1, 7, StatusType.Mandatory, CharType.NumericWithDecimal);
	}
}
