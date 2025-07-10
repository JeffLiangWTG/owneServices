using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Tests
{
	public class SystemIOHelperTests
	{

		[TestCase("file1.txt", "..\\..\\UXmlFiles\\file1.txt")]
		[TestCase("subdir/file2.log", "..\\..\\UXmlFiles\\subdir/file2.log")]
		public void GetOutputFilePath_Returns_Correct_Path(string fileName, string expected)
		{
			string result = SystemIOHelper.GetOutputFilePath(fileName);
			Assert.That(result, Is.EqualTo(expected));
		}

		[TestCase("config.json", "Res\\config.json")]
		[TestCase("data/sample.csv", "Res\\data/sample.csv")]
		public void GetResFilePath_Returns_Correct_Path(string fileName, string expected)
		{
			string result = SystemIOHelper.GetResFilePath(fileName);
			Assert.That(result, Is.EqualTo(expected));
		}
	}
}
