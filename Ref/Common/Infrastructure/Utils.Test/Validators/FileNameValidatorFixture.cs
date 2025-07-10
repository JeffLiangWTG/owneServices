using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test
{
	[TestFixture]
	class FileNameValidatorFixture
	{
		[TestCaseSource(nameof(TestFileNameData))]
		public void TestCheckFileName(string fileName, bool isValid)
		{
			Assert.That(FileNameValidator.IsValid(fileName), Is.EqualTo(isValid));
		}

		static object[] TestFileNameData => new object[]
		{
			new object[] { "../../ZATariffs.xml", false },
			new object[] { "test<h1>test", false },
			new object[] { "Click this link <a href=\"https://test.com\">Click Here</a>", false },
			new object[] { "ZATariffs.xml", true },
			new object[] { "|>test.xml", false }
		};
	}
}
