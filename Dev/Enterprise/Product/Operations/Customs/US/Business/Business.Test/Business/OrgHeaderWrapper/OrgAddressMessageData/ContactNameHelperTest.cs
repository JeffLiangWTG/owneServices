using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	public class ContactNameHelperTest : TestCaseWithFactory
	{
		public void TestGetFormattedName()
		{
			var contactName = "Donaldaaaabbbbbbbbbbccccccccccddddddddddeeeee MiddleNameaaaaabbbbbcccccdddddeeeeefffffggggg Trumpaaaaabbbbbbbbbbccccccccccddddddddddeeeee";
			AssertEquals("Trumpaaaaabbbbbbbbbbccccccccccdddddddddd, Donaldaaaabbbbbbbbbbccccccccccdddddddddd, M", ContactNameHelper.GetFormattedName(contactName));
			AssertEquals("Trumpaaaaabbbbbbbbbbccccccccccdddddddddd, Donaldaaaabbbbbbbbbbccccccccccdddddddddd, MiddleNameaaaaabbbbbcccccdddddeeeeefffff", ContactNameHelper.GetFormattedName(contactName, true));
		}

		public void TestGetWarningMessageIfInvalidFormat()
		{
			var errorMsg = "Format of name should be Last Name(max 40 characters), First Name(max 40 characters), Middle Initial(optional, max 18 characters)";
			var contactName = "Donald";
			AssertEquals(errorMsg, ContactNameHelper.GetWarningMessageIfInvalidFormat(contactName));

			contactName = "Trumpaaaaabbbbbbbbbbccccccccccddddddddddeeeee, Donald";
			AssertEquals(errorMsg, ContactNameHelper.GetWarningMessageIfInvalidFormat(contactName));

			contactName = "Trump, Donald";
			AssertEquals(ZString.Empty, ContactNameHelper.GetWarningMessageIfInvalidFormat(contactName));

			contactName = "Trump, Donald, M";
			AssertEquals(ZString.Empty, ContactNameHelper.GetWarningMessageIfInvalidFormat(contactName));

			contactName = "Trump, Donald, M, M";
			AssertEquals(errorMsg, ContactNameHelper.GetWarningMessageIfInvalidFormat(contactName));
		}

		public void TestGetSplitContactName()
		{
			var contactName = "12BO-2B3 T3H3E -29BUI-ilD3er3";
			(var firstName, var middleInitial, var lastName) = ContactNameHelper.GetSplitContactName(contactName);
			AssertEquals("BOB", firstName);
			AssertEquals("T", middleInitial);
			AssertEquals("BUIILDER", lastName);
			contactName = "(Wed323dy) Des32Troye3r";
			(firstName, middleInitial, lastName) = ContactNameHelper.GetSplitContactName(contactName);
			AssertEquals("WEDDY", firstName);
			AssertEquals("", middleInitial);
			AssertEquals("DESTROYER", lastName);
		}
	}
}
