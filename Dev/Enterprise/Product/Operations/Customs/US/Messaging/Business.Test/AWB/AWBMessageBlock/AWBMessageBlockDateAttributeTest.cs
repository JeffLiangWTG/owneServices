using System;
using CargoWise.Types;
using Enterprise.Messaging.Business.AWB;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.Testing
{
	[TestedType(typeof(AWBMessageBlockDateAttribute))]
	sealed class AWBMessageBlockDateAttributeTest : AWBMessageBlockAttributeTest<AWBMessageBlockDateAttribute>
	{
		public override void TestSerialise()
		{
			CombineAssertions(() =>
			{
				AssertEquals("091971", new AWBMessageBlockDateAttribute(1, StatusType.Mandatory, CharType.Numeric, "MMyyyy").Serialise(ZDate.BrettsBirthday));
				AssertEquals("??????", new AWBMessageBlockDateAttribute(1, StatusType.Mandatory, CharType.Numeric, "MMyyyy").Serialise(ZDate.Empty));
				AssertEquals("??????", new AWBMessageBlockDateAttribute(1, StatusType.Mandatory, CharType.Numeric, "MMyyyy").Serialise(ZDate.Invalid));
				AssertEquals("091971", new AWBMessageBlockDateAttribute(1, StatusType.Optional, CharType.Numeric, "MMyyyy").Serialise(ZDate.BrettsBirthday));
				AssertEquals("", new AWBMessageBlockDateAttribute(1, StatusType.Optional, CharType.Numeric, "MMyyyy").Serialise(ZDate.Empty));
				AssertEquals("??????", new AWBMessageBlockDateAttribute(1, StatusType.Optional, CharType.Numeric, "MMyyyy").Serialise(ZDate.Invalid));
				AssertEquals("091971", new AWBMessageBlockDateAttribute(1, StatusType.Conditional, CharType.Numeric, "MMyyyy").Serialise(ZDate.BrettsBirthday));
				AssertEquals("", new AWBMessageBlockDateAttribute(1, StatusType.Conditional, CharType.Numeric, "MMyyyy").Serialise(ZDate.Empty));
				AssertEquals("??????", new AWBMessageBlockDateAttribute(1, StatusType.Conditional, CharType.Numeric, "MMyyyy").Serialise(ZDate.Invalid));
				AssertEquals("18SEP", new AWBMessageBlockDateAttribute(1, StatusType.Mandatory, CharType.Numeric, "ddMMM").Serialise(ZDate.BrettsBirthday));
				AssertEquals("091871", new AWBMessageBlockDateAttribute(1, StatusType.Mandatory, CharType.Numeric).Serialise(ZDate.BrettsBirthday));
			});
		}

		public void TestDeSerialiseEmptyDate()
		{
			CombineAssertions(() =>
			{
				AssertEquals(ZDate.Empty, new AWBMessageBlockDateAttribute(1, StatusType.Mandatory, CharType.Numeric).DeSerialise("      "));
				AssertEquals(ZDate.Empty, new AWBMessageBlockDateAttribute(1, StatusType.Mandatory, CharType.Numeric).DeSerialise("000000"));
				AssertEquals(ZDate.Empty, new AWBMessageBlockDateAttribute(1, StatusType.Mandatory, CharType.Numeric, "MMddyy").DeSerialise(" 0 0 0"));
				AssertEquals(ZDate.Empty, new AWBMessageBlockDateAttribute(1, StatusType.Mandatory, CharType.Numeric, "MMddyyyy").DeSerialise(" 0 0   0"));
			});
		}

#if NETFRAMEWORK
		[ExpectExceptionMessage(typeof(ArgumentException), "'SA999ASDFDS' is not a valid yyyyMMdd format\r\nParameter name: value")]
#else
		[ExpectExceptionMessage(typeof(ArgumentException), "'SA999ASDFDS' is not a valid yyyyMMdd format (Parameter 'value')")]
#endif
		public void TestDeSerialiseInvalidDate()
		{
			AssertEquals(ZDate.Invalid, new AWBMessageBlockDateAttribute(1, StatusType.Mandatory, CharType.Numeric).DeSerialise("??????"));
			new AWBMessageBlockDateAttribute(1, StatusType.Mandatory, CharType.Numeric, "yyyyMMdd").DeSerialise("SA999ASDFDS");
		}

		public void TestDeSerialiseMaxDate()
		{
			AssertEquals(new ZDateTime(2099, 12, 31), new AWBMessageBlockDateAttribute(1, StatusType.Mandatory, CharType.Numeric).DeSerialise("999999"));
		}

		public override void TestDeSerialise()
		{
			AssertEquals(ZDate.BrettsBirthday, new AWBMessageBlockDateAttribute(1, StatusType.Mandatory, CharType.Numeric).DeSerialise("091871"));
		}

		public void TestDeSerialiseAndSerialiseDateFormat()
		{
			var attribute = new AWBMessageBlockDateAttribute(1, StatusType.Mandatory, CharType.Numeric, "yyyyddMM");
			AssertEquals(ZDate.BrettsBirthday, attribute.DeSerialise("19711809"));
			AssertEquals("19711809", attribute.Serialise(ZDate.BrettsBirthday));
		}

		protected override AWBMessageBlockDateAttribute CreateAttribute() => new AWBMessageBlockDateAttribute(1, StatusType.Mandatory, CharType.Numeric, "yyyyddMM");

		protected override ZString ValidValue => "19711809";

		protected override IZType InvalidValue => ZDate.Invalid;
	}
}
