using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	public class CustomsEntryInfoExtensionsTest : TestCase
	{
		public void TestGetFormattedCustomsEntryKeyWithLineNo()
		{
			AssertEquals("", ((CustomsEntryInfo)null).GetFormattedCustomsEntryKeyWithLineNo());
			AssertEquals("", new CustomsEntryInfo().GetFormattedCustomsEntryKeyWithLineNo());
			AssertEquals("", new CustomsEntryInfo { EntryLineNumber = 7 }.GetFormattedCustomsEntryKeyWithLineNo());
			AssertEquals("ABC", new CustomsEntryInfo { EntryKey = "abc" }.GetFormattedCustomsEntryKeyWithLineNo());
			AssertEquals("ABC-7", new CustomsEntryInfo { EntryKey = "abc", EntryLineNumber = 7 }.GetFormattedCustomsEntryKeyWithLineNo());
		}

		public void TestGetFormattedInwardsCustomsEntryKeyWithLineNoForOrderLine()
		{
			AssertEquals("", ((CustomsEntryInfo)null).GetFormattedInwardsCustomsEntryKeyWithLineNoForOrderLine());
			AssertEquals("", new CustomsEntryInfo().GetFormattedInwardsCustomsEntryKeyWithLineNoForOrderLine());
			AssertEquals("", new CustomsEntryInfo { EntryLineNumber = 7 }.GetFormattedInwardsCustomsEntryKeyWithLineNoForOrderLine());
			AssertEquals("", new CustomsEntryInfo { EntryKey = "abc" }.GetFormattedInwardsCustomsEntryKeyWithLineNoForOrderLine());
			AssertEquals("", new CustomsEntryInfo { EntryKey = "abc", EntryLineNumber = 7 }.GetFormattedInwardsCustomsEntryKeyWithLineNoForOrderLine());

			AssertEquals("", new CustomsEntryInfo { EntryKey = "abc", EntryLineNumber = 7, InwardsEntryLineNumber = 9 }.GetFormattedInwardsCustomsEntryKeyWithLineNoForOrderLine());
			AssertEquals("DEF", new CustomsEntryInfo { EntryKey = "abc", EntryLineNumber = 7, InwardsEntryKey = "def" }.GetFormattedInwardsCustomsEntryKeyWithLineNoForOrderLine());
			AssertEquals("DEF-9", new CustomsEntryInfo { EntryKey = "abc", EntryLineNumber = 7, InwardsEntryKey = "def", InwardsEntryLineNumber = 9 }.GetFormattedInwardsCustomsEntryKeyWithLineNoForOrderLine());
		}
	}
}