using NUnit.Framework;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tests
{
	[TestFixture]
	class StringExtensionsTests
	{
		[TestCase("", 1, "")]
		[TestCase("XYZ", 4, "XYZ")]
		[TestCase("ABC", 3, "ABC")]
		[TestCase("DEF", 2, "EF")]
		[TestCase("GHI", 0, "")]
		public void Right(string testString, int length, string expectedResult)
		{
			Assert.That(testString.Right(length), Is.EqualTo(expectedResult));
		}
	}
}
