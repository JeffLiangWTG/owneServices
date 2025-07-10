using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AccountingAssertionHelperTest : TestCaseWithFactory
	{
		public void TestAssertContainsInOrderNewLineSensitive()
		{
			AccountingAssertionHelper.AssertContainsInOrderNewLineSensitive("",
@"X
	12Test1123Test2123
123",
			"Test1", "Test2");

			AssertExceptionThrown<AssertionFailedError>(() => AccountingAssertionHelper.AssertContainsInOrderNewLineSensitive("Fail because new line between elements.",
@"Test1
Test2",
			"Test1", "Test2"));

			AssertExceptionThrown<AssertionFailedError>(() => AccountingAssertionHelper.AssertContainsInOrderNewLineSensitive("Fail because Test2 element was split.",
@"Test1Tes
t2",
			"Test1", "Test2"));

			AccountingAssertionHelper.AssertContainsInOrderNewLineSensitive("",
@"XXX
			AAAATest1
	Test212
	XXX",
			"Test1\r\n", "Test2");

			AssertExceptionThrown<AssertionFailedError>(() => AccountingAssertionHelper.AssertContainsInOrderNewLineSensitive("Fail because there are 2 new lines between elements, while the element specifies only 1 new line.",
@"Test1

Test2",
			"Test1\r\n", "Test2"));

			AccountingAssertionHelper.AssertContainsInOrderNewLineSensitive("",
@"Test1

Test2",
			"Test1\r\n\r\n", "Test2");

			AccountingAssertionHelper.AssertContainsInOrderNewLineSensitive("",
@"Test1

Test2",
			"Test1\r\n", "\r\nTest2");

			AssertExceptionThrown<AssertionFailedError>(() => AccountingAssertionHelper.AssertContainsInOrderNewLineSensitive("Fail because there are NO new line between elements, while the element specifies 1 new line.",
@"Test1 Test2",
			"Test1\r\n", "Test2"));

			var mixedScenario = @"
Test1
Test2
Test3
Test1
Test1 Test2 Test3
Test2 Test3
Test2
Test1 Test4 Test2
Test4
Test3";

			AccountingAssertionHelper.AssertContainsInOrderNewLineSensitive("Assert Message", mixedScenario, "Test1", "Test3");
			AccountingAssertionHelper.AssertContainsInOrderNewLineSensitive("Assert Message", mixedScenario, "Test2", "Test3");
			AccountingAssertionHelper.AssertContainsInOrderNewLineSensitive("Assert Message", mixedScenario, "Test1", "Test2", "Test3");
			AssertExceptionThrown<AssertionFailedError>(() => AccountingAssertionHelper.AssertContainsInOrderNewLineSensitive("Fail because there is no match in sequence, in one line.", mixedScenario, "Test3", "Test2"));
			AssertExceptionThrown<AssertionFailedError>(() => AccountingAssertionHelper.AssertContainsInOrderNewLineSensitive("Fail because there is no match in sequence, in one line.", mixedScenario, "Test2", "Test1", "Test3"));

			AccountingAssertionHelper.AssertContainsInOrderNewLineSensitive("Assert Message", mixedScenario, "Test1\r\n", "Test2");
			AccountingAssertionHelper.AssertContainsInOrderNewLineSensitive("Assert Message", mixedScenario, "Test1\r\n", "Test2\r\n", "Test3");
			AssertExceptionThrown<AssertionFailedError>(() => AccountingAssertionHelper.AssertContainsInOrderNewLineSensitive("Fail because there is no match in sequence, among two lines.", mixedScenario, "Test3\r\n", "Test4"));
		}
	}
}
