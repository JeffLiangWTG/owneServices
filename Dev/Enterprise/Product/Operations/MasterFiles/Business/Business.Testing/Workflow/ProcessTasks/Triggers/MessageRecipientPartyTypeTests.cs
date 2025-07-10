using System;
using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class MessageRecipientPartyTypeTests : TestCase
	{
		public void TestFlagDeclarationAfterInitialization()
		{
			var ex = AssertExceptionThrown<InvalidOperationException>(() => MessageRecipientPartyType.DeclareFlagForTest(MessageRecipientPartyType.BitFlagForTest(1)));
			AssertEquals("New flags should not be added to MessageRecipientPartyType outside of static initializer.", ex.Message);
		}

		public void TestBitFlag()
		{
			MessageRecipientPartyType val;

			val = MessageRecipientPartyType.BitFlagForTest(0);
			AssertEquals(0x0000000000000001ul, val.Value0);
			AssertEquals(0x0000000000000000ul, val.Value1);

			val = MessageRecipientPartyType.BitFlagForTest(63);
			AssertEquals(0x8000000000000000ul, val.Value0);
			AssertEquals(0x0000000000000000ul, val.Value1);

			val = MessageRecipientPartyType.BitFlagForTest(64);
			AssertEquals(0x0000000000000000ul, val.Value0);
			AssertEquals(0x0000000000000001ul, val.Value1);

			val = MessageRecipientPartyType.BitFlagForTest(127);
			AssertEquals(0x0000000000000000ul, val.Value0);
			AssertEquals(0x8000000000000000ul, val.Value1);

			AssertExceptionThrown<ArgumentException>(() => MessageRecipientPartyType.BitFlagForTest(-1000));
			AssertExceptionThrown<ArgumentException>(() => MessageRecipientPartyType.BitFlagForTest(-1));
			AssertExceptionThrown<ArgumentException>(() => MessageRecipientPartyType.BitFlagForTest(128));
			AssertExceptionThrown<ArgumentException>(() => MessageRecipientPartyType.BitFlagForTest(2000));
		}

		public void TestGetValues()
		{
			var values1 = MessageRecipientPartyType.GetValues();
			var values2 = MessageRecipientPartyType.GetValues();

			AssertNotEquals("GetValues should not return same array. That way one consumer would not be able to affect other consumers by changing values.", values1, values2);
			AssertArrayEqualsByElements("GetValues always returns same values", values1, values2);

			Assert(values1.Contains(MessageRecipientPartyType.None));

			Assert(values1.Contains(MessageRecipientPartyType.Consignee));
			Assert(values1.Contains(MessageRecipientPartyType.Consignor));
			Assert(values1.Contains(MessageRecipientPartyType.NVOCC));
		}

		public void TestOr()
		{
			MessageRecipientPartyType val;

			val = MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000000ul) | MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000000ul);
			AssertEquals(0x0000000000000000ul, val.Value0);
			AssertEquals(0x0000000000000000ul, val.Value1);

			val = MessageRecipientPartyType.CreateForTest(0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul) | MessageRecipientPartyType.CreateForTest(0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul);
			AssertEquals(0xFFFFFFFFFFFFFFFFul, val.Value0);
			AssertEquals(0xFFFFFFFFFFFFFFFFul, val.Value1);

			val = MessageRecipientPartyType.CreateForTest(0x0000FFFF0000FFFFul, 0xFFFF0000FFFF0000ul) | MessageRecipientPartyType.CreateForTest(0x0000FFFF0000FFFFul, 0xFFFF0000FFFF0000ul);
			AssertEquals(0x0000FFFF0000FFFFul, val.Value0);
			AssertEquals(0xFFFF0000FFFF0000ul, val.Value1);

			val = MessageRecipientPartyType.CreateForTest(0xFFFF0000FFFF0000ul, 0x0000FFFF0000FFFFul) | MessageRecipientPartyType.CreateForTest(0xFFFF0000FFFF0000ul, 0x0000FFFF0000FFFFul);
			AssertEquals(0xFFFF0000FFFF0000ul, val.Value0);
			AssertEquals(0x0000FFFF0000FFFFul, val.Value1);

			val = MessageRecipientPartyType.CreateForTest(0x0000FFFF0000FFFFul, 0xFFFF0000FFFF0000ul) | MessageRecipientPartyType.CreateForTest(0xFFFF0000FFFF0000ul, 0x0000FFFF0000FFFFul);
			AssertEquals(0xFFFFFFFFFFFFFFFFul, val.Value0);
			AssertEquals(0xFFFFFFFFFFFFFFFFul, val.Value1);

			val = MessageRecipientPartyType.CreateForTest(0x1234567890ABCDEFul, 0x23456789ABCDEF01ul) | MessageRecipientPartyType.CreateForTest(0x3456789ABCDEF012ul, 0x56789ABCDEF01234ul);
			AssertEquals(0x36767EFABCFFFDFFul, val.Value0);
			AssertEquals(0x777DFFBDFFFDFF35ul, val.Value1);

			val = MessageRecipientPartyType.CreateForTest(0x1234567890ABCDEFul, 0x23456789ABCDEF01ul) | MessageRecipientPartyType.CreateForTest(0x89ABCDEF01234567ul, 0xABCDEF0123456789ul);
			AssertEquals(0x9BBFDFFF91ABCDEFul, val.Value0);
			AssertEquals(0xABCDEF89ABCDEF89ul, val.Value1);
		}

		public void TestAnd()
		{
			MessageRecipientPartyType val;

			val = MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000000ul) & MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000000ul);
			AssertEquals(0x0000000000000000ul, val.Value0);
			AssertEquals(0x0000000000000000ul, val.Value1);

			val = MessageRecipientPartyType.CreateForTest(0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul) & MessageRecipientPartyType.CreateForTest(0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul);
			AssertEquals(0xFFFFFFFFFFFFFFFFul, val.Value0);
			AssertEquals(0xFFFFFFFFFFFFFFFFul, val.Value1);

			val = MessageRecipientPartyType.CreateForTest(0x0000FFFF0000FFFFul, 0xFFFF0000FFFF0000ul) & MessageRecipientPartyType.CreateForTest(0x0000FFFF0000FFFFul, 0xFFFF0000FFFF0000ul);
			AssertEquals(0x0000FFFF0000FFFFul, val.Value0);
			AssertEquals(0xFFFF0000FFFF0000ul, val.Value1);

			val = MessageRecipientPartyType.CreateForTest(0xFFFF0000FFFF0000ul, 0x0000FFFF0000FFFFul) & MessageRecipientPartyType.CreateForTest(0xFFFF0000FFFF0000ul, 0x0000FFFF0000FFFFul);
			AssertEquals(0xFFFF0000FFFF0000ul, val.Value0);
			AssertEquals(0x0000FFFF0000FFFFul, val.Value1);

			val = MessageRecipientPartyType.CreateForTest(0x0000FFFF0000FFFFul, 0xFFFF0000FFFF0000ul) & MessageRecipientPartyType.CreateForTest(0xFFFF0000FFFF0000ul, 0x0000FFFF0000FFFFul);
			AssertEquals(0x0000000000000000ul, val.Value0);
			AssertEquals(0x0000000000000000ul, val.Value1);

			val = MessageRecipientPartyType.CreateForTest(0x1234567890ABCDEFul, 0x23456789ABCDEF01ul) & MessageRecipientPartyType.CreateForTest(0x3456789ABCDEF012ul, 0x56789ABCDEF01234ul);
			AssertEquals(0x10145018908AC002ul, val.Value0);
			AssertEquals(0x024002888AC00200ul, val.Value1);

			val = MessageRecipientPartyType.CreateForTest(0x89ABCDEF01234567ul, 0x9ABCDEF012345678ul) & MessageRecipientPartyType.CreateForTest(0xABCDEF0123456789ul, 0xCDEF0123456789ABul);
			AssertEquals(0x8989CD0101014501ul, val.Value0);
			AssertEquals(0x88AC002000240028ul, val.Value1);
		}

		public void TestComplement()
		{
			MessageRecipientPartyType val;

			val = ~MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000000ul);
			AssertEquals(0xFFFFFFFFFFFFFFFFul, val.Value0);
			AssertEquals(0xFFFFFFFFFFFFFFFFul, val.Value1);

			val = ~MessageRecipientPartyType.CreateForTest(0x0000FFFF0000FFFFul, 0xFFFF0000FFFF0000ul);
			AssertEquals(0xFFFF0000FFFF0000ul, val.Value0);
			AssertEquals(0x0000FFFF0000FFFFul, val.Value1);

			val = ~MessageRecipientPartyType.CreateForTest(0xFFFF0000FFFF0000ul, 0x0000FFFF0000FFFFul);
			AssertEquals(0x0000FFFF0000FFFFul, val.Value0);
			AssertEquals(0xFFFF0000FFFF0000ul, val.Value1);

			val = ~MessageRecipientPartyType.CreateForTest(0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul);
			AssertEquals(0x0000000000000000ul, val.Value0);
			AssertEquals(0x0000000000000000ul, val.Value1);

			val = ~MessageRecipientPartyType.CreateForTest(0x1234567890ABCDEFul, 0x89ABCDEF01234567ul);
			AssertEquals(0xEDCBA9876F543210ul, val.Value0);
			AssertEquals(0x76543210FEDCBA98ul, val.Value1);
		}

		public void TestHasFlag2()
		{
			MessageRecipientPartyType none = MessageRecipientPartyType.None;
			MessageRecipientPartyType all = ~MessageRecipientPartyType.None;

			MessageRecipientPartyType bit0 = MessageRecipientPartyType.BitFlagForTest(0);
			MessageRecipientPartyType bit63 = MessageRecipientPartyType.BitFlagForTest(63);
			MessageRecipientPartyType bit64 = MessageRecipientPartyType.BitFlagForTest(64);
			MessageRecipientPartyType bit127 = MessageRecipientPartyType.BitFlagForTest(127);

			MessageRecipientPartyType bits0and63 = bit0 | bit63;
			MessageRecipientPartyType bits63and64 = bit63 | bit64;
			MessageRecipientPartyType bits64and127 = bit64 | bit127;

			Assert(none.HasFlag(none));
			Assert(!none.HasFlag(all));
			Assert(!none.HasFlag(bit0));
			Assert(!none.HasFlag(bit63));
			Assert(!none.HasFlag(bit64));
			Assert(!none.HasFlag(bit127));
			Assert(!none.HasFlag(bits0and63));
			Assert(!none.HasFlag(bits63and64));
			Assert(!none.HasFlag(bits64and127));

			Assert(all.HasFlag(none));
			Assert(all.HasFlag(all));
			Assert(all.HasFlag(bit0));
			Assert(all.HasFlag(bit63));
			Assert(all.HasFlag(bit64));
			Assert(all.HasFlag(bit127));
			Assert(all.HasFlag(bits0and63));
			Assert(all.HasFlag(bits63and64));
			Assert(all.HasFlag(bits64and127));

			Assert(bit0.HasFlag(none));
			Assert(!bit0.HasFlag(all));
			Assert(bit0.HasFlag(bit0));
			Assert(!bit0.HasFlag(bit63));
			Assert(!bit0.HasFlag(bit64));
			Assert(!bit0.HasFlag(bit127));
			Assert(!bit0.HasFlag(bits0and63));
			Assert(!bit0.HasFlag(bits63and64));
			Assert(!bit0.HasFlag(bits64and127));

			Assert(bit63.HasFlag(none));
			Assert(!bit63.HasFlag(all));
			Assert(!bit63.HasFlag(bit0));
			Assert(bit63.HasFlag(bit63));
			Assert(!bit63.HasFlag(bit64));
			Assert(!bit63.HasFlag(bit127));
			Assert(!bit63.HasFlag(bits0and63));
			Assert(!bit63.HasFlag(bits63and64));
			Assert(!bit63.HasFlag(bits64and127));

			Assert(bit64.HasFlag(none));
			Assert(!bit64.HasFlag(all));
			Assert(!bit64.HasFlag(bit0));
			Assert(!bit64.HasFlag(bit63));
			Assert(bit64.HasFlag(bit64));
			Assert(!bit64.HasFlag(bit127));
			Assert(!bit64.HasFlag(bits0and63));
			Assert(!bit64.HasFlag(bits63and64));
			Assert(!bit64.HasFlag(bits64and127));

			Assert(bit127.HasFlag(none));
			Assert(!bit127.HasFlag(all));
			Assert(!bit127.HasFlag(bit0));
			Assert(!bit127.HasFlag(bit63));
			Assert(!bit127.HasFlag(bit64));
			Assert(bit127.HasFlag(bit127));
			Assert(!bit127.HasFlag(bits0and63));
			Assert(!bit127.HasFlag(bits63and64));
			Assert(!bit127.HasFlag(bits64and127));

			Assert(bits0and63.HasFlag(none));
			Assert(!bits0and63.HasFlag(all));
			Assert(bits0and63.HasFlag(bit0));
			Assert(bits0and63.HasFlag(bit63));
			Assert(!bits0and63.HasFlag(bit64));
			Assert(!bits0and63.HasFlag(bit127));
			Assert(bits0and63.HasFlag(bits0and63));
			Assert(!bits0and63.HasFlag(bits63and64));
			Assert(!bits0and63.HasFlag(bits64and127));

			Assert(bits63and64.HasFlag(none));
			Assert(!bits63and64.HasFlag(all));
			Assert(!bits63and64.HasFlag(bit0));
			Assert(bits63and64.HasFlag(bit63));
			Assert(bits63and64.HasFlag(bit64));
			Assert(!bits63and64.HasFlag(bit127));
			Assert(!bits63and64.HasFlag(bits0and63));
			Assert(bits63and64.HasFlag(bits63and64));
			Assert(!bits63and64.HasFlag(bits64and127));

			Assert(bits64and127.HasFlag(none));
			Assert(!bits64and127.HasFlag(all));
			Assert(!bits64and127.HasFlag(bit0));
			Assert(!bits64and127.HasFlag(bit63));
			Assert(bits64and127.HasFlag(bit64));
			Assert(bits64and127.HasFlag(bit127));
			Assert(!bits64and127.HasFlag(bits0and63));
			Assert(!bits64and127.HasFlag(bits63and64));
			Assert(bits64and127.HasFlag(bits64and127));
		}

		public void TestEqualityMembers()
		{
			var val1Copy1 = MessageRecipientPartyType.CreateForTest(0x1234567890ABCDEFul, 0x23456789ABCDEF01ul);
			var val1Copy2 = MessageRecipientPartyType.CreateForTest(0x1234567890ABCDEFul, 0x23456789ABCDEF01ul);

			var val2 = MessageRecipientPartyType.CreateForTest(0x23456789ABCDEF01ul, 0x34567890ABCDEF12ul);

			AssertEquals(val1Copy1, val1Copy2);
			Assert(val1Copy1 == val1Copy2);
			AssertEquals(val1Copy1.GetHashCode(), val1Copy2.GetHashCode());

			AssertNotEquals(val1Copy1, val2);
			Assert(val1Copy1 != val2);

			// note: Just checking that hashcode has non-trivial implementation. Hash-code inequality is not required by hash-equals contract.
			AssertNotEquals(MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000000ul).GetHashCode(), MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000001ul).GetHashCode());
			AssertNotEquals(MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000000ul).GetHashCode(), MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x8000000000000000ul).GetHashCode());
			AssertNotEquals(MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000000ul).GetHashCode(), MessageRecipientPartyType.CreateForTest(0x0000000000000001ul, 0x0000000000000000ul).GetHashCode());
			AssertNotEquals(MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000000ul).GetHashCode(), MessageRecipientPartyType.CreateForTest(0x8000000000000000ul, 0x0000000000000000ul).GetHashCode());

			AssertNotEquals(MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000000ul), MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000001ul));
			AssertNotEquals(MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000000ul), MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000080000000ul));
			AssertNotEquals(MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000000ul), MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000100000000ul));
			AssertNotEquals(MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000000ul), MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x8000000000000000ul));
			AssertNotEquals(MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000000ul), MessageRecipientPartyType.CreateForTest(0x0000000000000001ul, 0x0000000000000000ul));
			AssertNotEquals(MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000000ul), MessageRecipientPartyType.CreateForTest(0x0000000080000000ul, 0x0000000000000000ul));
			AssertNotEquals(MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000000ul), MessageRecipientPartyType.CreateForTest(0x0000000100000000ul, 0x0000000000000000ul));
			AssertNotEquals(MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000000ul), MessageRecipientPartyType.CreateForTest(0x8000000000000000ul, 0x0000000000000000ul));

			AssertNotEquals(MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000001ul), MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000000ul));
			AssertNotEquals(MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000080000000ul), MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000000ul));
			AssertNotEquals(MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000100000000ul), MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000000ul));
			AssertNotEquals(MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x8000000000000000ul), MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000000ul));
			AssertNotEquals(MessageRecipientPartyType.CreateForTest(0x0000000000000001ul, 0x0000000000000000ul), MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000000ul));
			AssertNotEquals(MessageRecipientPartyType.CreateForTest(0x0000000080000000ul, 0x0000000000000000ul), MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000000ul));
			AssertNotEquals(MessageRecipientPartyType.CreateForTest(0x0000000100000000ul, 0x0000000000000000ul), MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000000ul));
			AssertNotEquals(MessageRecipientPartyType.CreateForTest(0x8000000000000000ul, 0x0000000000000000ul), MessageRecipientPartyType.CreateForTest(0x0000000000000000ul, 0x0000000000000000ul));
		}
	}
}
