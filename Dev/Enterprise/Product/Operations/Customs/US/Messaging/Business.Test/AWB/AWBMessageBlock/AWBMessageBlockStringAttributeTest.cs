using CargoWise.Types;
using Enterprise.Messaging.Business.AWB;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.Testing
{
	[TestedType(typeof(AWBMessageBlockStringAttribute))]
	sealed class AWBMessageBlockStringAttributeTest : AWBMessageBlockAttributeTest<AWBMessageBlockStringAttribute>
	{
		public void TestSerialiseIfMaxLengthExceeded()
		{
			try
			{
				new AWBMessageBlockStringAttribute(1, 1, 2, StatusType.Mandatory, CharType.Alpha).Serialise(new ZString("XYZ"));
				Fail("Should have thrown a MessageBlockSerialisationException");
			}
			catch (MessageBlockSerialisationException ex)
			{
				AssertEquals("Data provided exceeded allowable maximum length:\r\nMaximum Length:2\r\nActual Length:3", ex.Message);
				AssertEquals("?", ex.NewInvalidFormat);
			}

			try
			{
				AssertEquals("?", new AWBMessageBlockStringAttribute(1, 1, 2, StatusType.Mandatory, CharType.Alpha)
				{ OnLengthViolation = LengthViolationAction.SetInvalidValue }.Serialise(new ZString("XYZ")));
			}
			catch (MessageBlockSerialisationException)
			{
				Fail("Should not have thrown a MessageBlockSerialisationException");
			}

			try
			{
				AssertEquals("XY", new AWBMessageBlockStringAttribute(1, 1, 2, StatusType.Mandatory, CharType.Alpha)
				{ OnLengthViolation = LengthViolationAction.Substring }.Serialise(new ZString("XYZ")));
			}
			catch (MessageBlockSerialisationException)
			{
				Fail("Should not have thrown a MessageBlockSerialisationException");
			}
		}

		public void TestTruncateLongerMessages()
		{
			AssertEquals("ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXY", new AWBMessageBlockStringAttribute(1, 1, 51, StatusType.Mandatory, CharType.Alpha)
			{ OnLengthViolation = LengthViolationAction.Substring }.Serialise(new ZString("ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ")));
		}

		public void TestTrimInvalidCharacters()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ADDRESS", new AWBMessageBlockStringAttribute(1, 1, 20, StatusType.Mandatory, CharType.Alpha).Serialise(new ZString(@"4àd%*-d/r ês.s3")));
				AssertEquals("4ADDRESS3", new AWBMessageBlockStringAttribute(1, 1, 20, StatusType.Mandatory, CharType.AlphaNumeric).Serialise(new ZString(@"4àd%*-d/r ês.s3")));
				AssertEquals("43", new AWBMessageBlockStringAttribute(1, 1, 20, StatusType.Mandatory, CharType.Numeric).Serialise(new ZString(@"4àd%*-d/r ês.s3")));
				AssertEquals("4.3", new AWBMessageBlockStringAttribute(1, 1, 20, StatusType.Mandatory, CharType.NumericWithDecimal).Serialise(new ZString(@"4àd%*-d/r ês.s3")));
				AssertEquals(new ZString(@"4AD%*-D R ES.S3"), new AWBMessageBlockStringAttribute(1, 1, 20, StatusType.Mandatory, CharType.Special).Serialise(new ZString(@"4àd%*-d/r ês.s3")));
				AssertEquals(new ZString(@"4AD-DR ES.S3"), new AWBMessageBlockStringAttribute(1, 1, 20, StatusType.Mandatory, CharType.Text).Serialise(new ZString(@"4àd%*-d/r ês.s3")));
			});
		}

		public override void TestSerialise()
		{
			CombineAssertions(() =>
			{
				AssertEquals("XYZ", new AWBMessageBlockStringAttribute(1, 1, 5, StatusType.Mandatory, CharType.Alpha).Serialise(new ZString("XYZ")));
				AssertEquals("?", new AWBMessageBlockStringAttribute(1, 1, 5, StatusType.Mandatory, CharType.Alpha).Serialise(ZString.Empty));
				AssertEquals("?A", new AWBMessageBlockStringAttribute(1, 2, 5, StatusType.Mandatory, CharType.Alpha).Serialise(new ZString("A")));
				AssertEquals("XYZ", new AWBMessageBlockStringAttribute(1, 1, 5, StatusType.Optional, CharType.Alpha).Serialise(new ZString("XYZ")));
				AssertEquals(ZString.Empty, new AWBMessageBlockStringAttribute(1, 1, 5, StatusType.Optional, CharType.Alpha).Serialise(ZString.Empty));
				AssertEquals("?A", new AWBMessageBlockStringAttribute(1, 2, 5, StatusType.Optional, CharType.Alpha).Serialise(new ZString("A")));
				AssertEquals("XYZ", new AWBMessageBlockStringAttribute(1, 1, 5, StatusType.Conditional, CharType.Alpha).Serialise(new ZString("XYZ")));
				AssertEquals(ZString.Empty, new AWBMessageBlockStringAttribute(1, 1, 5, StatusType.Conditional, CharType.Alpha).Serialise(ZString.Empty));
				AssertEquals("?A", new AWBMessageBlockStringAttribute(1, 2, 5, StatusType.Conditional, CharType.Alpha).Serialise(new ZString("A")));
			});
		}

		public override void TestDeSerialise()
		{
			CombineAssertions(() =>
			{
				AssertEquals(new ZString("XYZ"), new AWBMessageBlockStringAttribute(1, 1, 3, StatusType.Mandatory, CharType.Alpha).DeSerialise("XY3Z"));
				AssertEquals(new ZString("XY3"), new AWBMessageBlockStringAttribute(1, 1, 3, StatusType.Mandatory, CharType.AlphaNumeric).DeSerialise("XY3Z"));
				AssertEquals(new ZString("XY3Z"), new AWBMessageBlockStringAttribute(1, 1, 4, StatusType.Mandatory, CharType.AlphaNumeric).DeSerialise("XY3Z"));
			});
		}

		protected override ZString ValidValue => "HELLO";

		protected override IZType InvalidValue => new ZString("1234567890A");

		protected override AWBMessageBlockStringAttribute CreateAttribute()
		{
			var result = new AWBMessageBlockStringAttribute(1, 1, 10, StatusType.Mandatory, CharType.AlphaNumeric);
			result.OnLengthViolation = LengthViolationAction.SetInvalidValue;
			return result;
		}
	}
}
