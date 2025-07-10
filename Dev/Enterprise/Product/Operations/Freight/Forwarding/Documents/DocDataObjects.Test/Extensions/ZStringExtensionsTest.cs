using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	[TestFixture]
	sealed class ZStringExtensionsTest : TestCaseWithFactory
	{
		const int MaxLines = 4;

		[Test]
		public void GetFirstNLines_ShouldReturnEmpty_WhenInputIsEmpty()
		{
			ZString input = string.Empty;

			var result = input.GetFirstNLines(MaxLines);

			AssertEquals(ZString.Empty, result);
		}

		[Test]
		public void GetFirstNLines_ShouldReturnEmpty_WhenInputIsNull()
		{
			ZString input = null;

			var result = input.GetFirstNLines(MaxLines);

			AssertEquals(ZString.Empty, result);
		}

		[Test]
		public void GetFirstNLines_ShouldReturnAllLines_WhenLineCountIsLessThanMaxLines()
		{
			ZString input = "Line1\nLine2\nLine3";

			var result = input.GetFirstNLines(MaxLines);

			AssertEquals(input, result);
		}

		[Test]
		public void GetFirstNLines_ShouldReturnFirstNLines_WhenLineCountExceedsMaxLines()
		{
			ZString input = "Line1\nLine2\nLine3\nLine4\nLine5";

			var result = input.GetFirstNLines(MaxLines);

			var expected = "Line1\nLine2\nLine3\nLine4";
			AssertEquals(expected, result);
		}

		[Test]
		public void GetFirstNLines_ShouldHandleWindowsLineEndings()
		{
			ZString input = "Line1\r\nLine2\r\nLine3\r\nLine4\r\nLine5";

			var result = input.GetFirstNLines(MaxLines);

			var expected = "Line1\r\nLine2\r\nLine3\r\nLine4";
			AssertEquals(expected, result);
		}
	}
}
