using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Testing
{
	sealed class IntMessageBlockAttributeTest : TestCase
	{
		public void TestFillType()
		{
			ZInt value = 123;
			MessageBlockIntAttribute attribute = new MessageBlockIntAttribute(5, 1, "M");
			AssertEquals("00123", attribute.Serialise(null, value, false));
			AssertEquals("123", attribute.Serialise(null, value, true));
			attribute = new MessageBlockIntAttribute(5, 1, "M", FillType.AlwaysSpaceFill);
			AssertEquals("  123", attribute.Serialise(null, value, false));
			AssertEquals("123", attribute.Serialise(null, value, true));
			attribute = new MessageBlockIntAttribute(5, 1, "M", FillType.AlwaysZeroFill);
			AssertEquals("00123", attribute.Serialise(null, value, false));
			AssertEquals("123", attribute.Serialise(null, value, true));
			attribute = new MessageBlockIntAttribute(5, 1, "M", FillType.ZeroFillUnlessEmpty);
			AssertEquals("00123", attribute.Serialise(null, value, false));
			AssertEquals("123", attribute.Serialise(null, value, true));
			value = ZInt.Zero;
			attribute = new MessageBlockIntAttribute(5, 1, "M");
			AssertEquals("00000", attribute.Serialise(null, value, false));
			AssertEquals("0", attribute.Serialise(null, value, true));
			attribute = new MessageBlockIntAttribute(5, 1, "M");
			AssertEquals("00000", attribute.Serialise(null, value, false));
			AssertEquals("0", attribute.Serialise(null, value, true));
			attribute = new MessageBlockIntAttribute(5, 1, "M", FillType.AlwaysSpaceFill);
			AssertEquals("    0", attribute.Serialise(null, value, false));
			AssertEquals("0", attribute.Serialise(null, value, true));
			attribute = new MessageBlockIntAttribute(5, 1, "M", FillType.AlwaysZeroFill);
			AssertEquals("00000", attribute.Serialise(null, value, false));
			AssertEquals("0", attribute.Serialise(null, value, true));
			attribute = new MessageBlockIntAttribute(5, 1, "M", FillType.ZeroFillUnlessEmpty);
			AssertEquals("     ", attribute.Serialise(null, value, false));
			AssertEquals("", attribute.Serialise(null, value, true));
		}

		public void TestDeSerialiseEmptyInt()
		{
			AssertEquals(ZInt.Zero, new MessageBlockIntAttribute(5, 1, "M").DeSerialise("00000"));
			AssertEquals(ZInt.Zero, new MessageBlockIntAttribute(5, 1, "O").DeSerialise("     "));
			AssertEquals(ZInt.Zero, new MessageBlockIntAttribute(5, 1, "M").DeSerialise("*****"));
		}

		public void TestSerialiseEmptyZInt()
		{
			AssertEquals("00000", new MessageBlockIntAttribute(5, 1, "M").Serialise(null, ZInt.Zero));
		}

		public void TestSerialiseZInt()
		{
			AssertEquals("00123", new MessageBlockIntAttribute(5, 1, "M").Serialise(null, (ZInt)123));
		}

		public void TestSerialiseZIntHavingMoreDigitsThanAllowed()
		{
			AssertEquals("**", new MessageBlockIntAttribute(2, 1, "M").Serialise(null, (ZInt)123));
			AssertEquals("**", new MessageBlockIntAttribute(2, 1, "M").Serialise(null, new ZInt(-1)));
		}
	}
}
