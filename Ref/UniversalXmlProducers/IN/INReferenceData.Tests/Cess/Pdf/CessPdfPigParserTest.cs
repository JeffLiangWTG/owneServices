using CargoWise.RefDbRepo.INReferenceData.Business;
using CargoWise.RefDbRepo.INReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Tests
{
	public class CessPdfPigParserTests
	{
		[Test]
		public void TestParseFileAsJsonReturnsNullWhenPdfFileDoesNotExist()
		{
			var result = parser.ParseFileAsJson("nonexistent.pdf", "someFolder");
			Assert.IsNull(result);

			loggerMock.Verify(
				l => l.Log(LogType.ReviewRequired, It.Is<string>(s => s.Contains("Invalid file path")), It.IsAny<object>()),
				Times.Once
			);
		}

		[SetUp]
		public void Setup()
		{
			loggerMock = new Mock<ILogger>();
			parser = new CessPdfPigParser(loggerMock.Object);
		}

		Mock<ILogger> loggerMock;
		CessPdfPigParser parser;
	}
}
