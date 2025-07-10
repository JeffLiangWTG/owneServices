using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Testing
{
	sealed class DateMessageBlockAttributeTest : TestCase
	{
		public void TestLength()
		{
			AssertEquals(8, new MessageBlockDateAttribute(1, "M", "yyyyddMM").Length);
			AssertEquals(6, new MessageBlockDateAttribute(1, "M").Length);
		}

		public void TestSerialiseZDate()
		{
			AssertEquals("091871", new MessageBlockDateAttribute(1, "M").Serialise(null, ZDate.BrettsBirthday));
			AssertEquals("      ", new MessageBlockDateAttribute(1, "M").Serialise(null, ZDate.Empty));
			AssertEquals("******", new MessageBlockDateAttribute(1, "M").Serialise(null, ZDate.Invalid));
		}

		public void TestSerialiseJulianDateFormat()
		{
			AssertEquals("71261", new MessageBlockDateAttribute(1, "M", MessageBlockDateAttribute.JulianDateFormat).Serialise(null, ZDate.BrettsBirthday));
		}

		public void TestDeSerialiseEmptyDate()
		{
			AssertEquals(ZDate.Empty, new MessageBlockDateAttribute(1, "M").DeSerialise("      "));
			AssertEquals(ZDate.Empty, new MessageBlockDateAttribute(1, "M").DeSerialise("000000"));
			AssertEquals(ZDate.Empty, new MessageBlockDateAttribute(1, "M", "MMddyy").DeSerialise(" 0 0 0"));
			AssertEquals(ZDate.Empty, new MessageBlockDateAttribute(1, "M", "MMddyyyy").DeSerialise(" 0 0   0"));
		}

#if NETFRAMEWORK
		[ExpectExceptionMessage(typeof(ArgumentException), "'SA999ASD' is not a valid yyyyMMdd format\r\nParameter name: value")]
#else
		[ExpectExceptionMessage(typeof(ArgumentException), "'SA999ASD' is not a valid yyyyMMdd format (Parameter 'value')")]
#endif
		public void TestDeSerialiseInvalidDate()
		{
			AssertEquals(ZDate.Invalid, new MessageBlockDateAttribute(1, "M").DeSerialise("******"));
			new MessageBlockDateAttribute(1, "M", "yyyyMMdd").DeSerialise("SA999ASDFDS");
		}

#if NETFRAMEWORK
		[ExpectExceptionMessage(typeof(ArgumentException), "'SA999' is not a valid Julian Date format\r\nParameter name: value")]
#else
		[ExpectExceptionMessage(typeof(ArgumentException), "'SA999' is not a valid Julian Date format (Parameter 'value')")]
#endif
		public void TestDeSerialiseInvalidJulianDate()
		{
			new MessageBlockDateAttribute(1, "M", MessageBlockDateAttribute.JulianDateFormat).DeSerialise("SA999ASDFDS");
		}

		public void TestDeSerialiseMaxDate()
		{
			AssertEquals(new ZDateTime(2099, 12, 31), new MessageBlockDateAttribute(1, "M").DeSerialise("999999"));
		}

		public void TestDeSerialiseMaxJulianDate()
		{
			AssertEquals(new ZDateTime(2099, 12, 31), new MessageBlockDateAttribute(1, "M", MessageBlockDateAttribute.JulianDateFormat).DeSerialise("99999"));
		}

		public void TestDeSerialiseDate()
		{
			AssertEquals(ZDate.BrettsBirthday, new MessageBlockDateAttribute(1, "M").DeSerialise("091871"));
		}

		public void TestDeSerialiseAndSerialiseDateFormat()
		{
			var attribute = new MessageBlockDateAttribute(1, "M", "yyyyddMM");
			AssertEquals(ZDate.BrettsBirthday, attribute.DeSerialise("19711809"));
			AssertEquals("19711809", attribute.Serialise(null, ZDate.BrettsBirthday));
		}

		public void TestDeSerialiseJulianDate()
		{
			AssertEquals(ZDate.BrettsBirthday, new MessageBlockDateAttribute(1, "M", MessageBlockDateAttribute.JulianDateFormat).DeSerialise("71261"));
		}
	}
}
