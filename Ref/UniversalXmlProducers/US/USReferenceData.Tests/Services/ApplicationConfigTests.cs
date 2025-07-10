using CargoWise.RefDbRepo.USReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	class ApplicationConfigTests
	{
		[Test]
		public void OutputDirectory()
		{
			Assert.That(@"..\..\UXmlFiles", Is.EqualTo(ApplicationConfig.InstanceForTest.OutputPath));
		}

		[Test]
		public void CensusEndPoint()
		{
			Assert.That(@"https://www.census.gov", Is.EqualTo(ApplicationConfig.InstanceForTest.CensusEndPoint));
		}

		[Test]
		public void CensusTradeDocumentReferenceLibraryEndPoint()
		{
			Assert.That(@"https://www.census.gov/foreign-trade/aes/documentlibrary/index.html#concordance", Is.EqualTo(ApplicationConfig.InstanceForTest.CensusTradeDocumentReferenceLibraryEndPoint));
		}

		[Test]
		public void InputDirectory()
		{
			Assert.That("Input", Is.EqualTo(ApplicationConfig.InstanceForTest.InputPath));
		}

		[SetUp]
		public void SetUp()
		{
			ApplicationConfig.SetConfigFileForTest("CargoWise.RefDbRepo.USReferenceData.Tests.config.json");
		}

		[TearDown]
		public void TearDown()
		{
			ApplicationConfig.SetConfigFileForTest("CargoWise.RefDbRepo.USReferenceData.CmdLine.config.json");
		}
	}
}
