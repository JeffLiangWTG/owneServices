using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Testing
{
	sealed class ShortMessageBlockAttributeTest : TestCase
	{
		public void TestFillType()
		{
			ZShort value = 123;
			MessageBlockShortAttribute attribute = new MessageBlockShortAttribute(5, 1, "M");
			AssertEquals("00123", attribute.Serialise(null, value, false));
			AssertEquals("123", attribute.Serialise(null, value, true));
			attribute = new MessageBlockShortAttribute(5, 1, "M", FillType.AlwaysSpaceFill);
			AssertEquals("  123", attribute.Serialise(null, value, false));
			AssertEquals("123", attribute.Serialise(null, value, true));
			attribute = new MessageBlockShortAttribute(5, 1, "M", FillType.AlwaysZeroFill);
			AssertEquals("00123", attribute.Serialise(null, value, false));
			AssertEquals("123", attribute.Serialise(null, value, true));
			attribute = new MessageBlockShortAttribute(5, 1, "M", FillType.ZeroFillUnlessEmpty);
			AssertEquals("00123", attribute.Serialise(null, value, false));
			AssertEquals("123", attribute.Serialise(null, value, true));
			value = ZShort.Zero;
			attribute = new MessageBlockShortAttribute(5, 1, "M");
			AssertEquals("00000", attribute.Serialise(null, value, false));
			AssertEquals("0", attribute.Serialise(null, value, true));
			attribute = new MessageBlockShortAttribute(5, 1, "M");
			AssertEquals("00000", attribute.Serialise(null, value, false));
			AssertEquals("0", attribute.Serialise(null, value, true));
			attribute = new MessageBlockShortAttribute(5, 1, "M", FillType.AlwaysSpaceFill);
			AssertEquals("    0", attribute.Serialise(null, value, false));
			AssertEquals("0", attribute.Serialise(null, value, true));
			attribute = new MessageBlockShortAttribute(5, 1, "M", FillType.AlwaysZeroFill);
			AssertEquals("00000", attribute.Serialise(null, value, false));
			AssertEquals("0", attribute.Serialise(null, value, true));
			attribute = new MessageBlockShortAttribute(5, 1, "M", FillType.ZeroFillUnlessEmpty);
			AssertEquals("     ", attribute.Serialise(null, value, false));
			AssertEquals("", attribute.Serialise(null, value, true));
		}

		public void TestDeSerialiseEmptyShort()
		{
			AssertEquals(ZShort.Zero, new MessageBlockShortAttribute(5, 1, "M").DeSerialise("00000"));
			AssertEquals(ZShort.Zero, new MessageBlockShortAttribute(5, 1, "O").DeSerialise("     "));
			AssertEquals(ZShort.Zero, new MessageBlockShortAttribute(5, 1, "M").DeSerialise("*****"));
		}

		public void TestSerialiseEmptyZShort()
		{
			AssertEquals("00000", new MessageBlockShortAttribute(5, 1, "M").Serialise(null, ZShort.Zero));
		}

		public void TestSerialiseZShort()
		{
			AssertEquals("00123", new MessageBlockShortAttribute(5, 1, "M").Serialise(null, (ZShort)123));
		}

		public void TestSerialiseZShortHavingMoreDigits()
		{
			AssertEquals("**", new MessageBlockShortAttribute(2, 1, "M").Serialise(null, (ZShort)123));
		}
	}
}
