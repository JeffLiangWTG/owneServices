using Moq;
using NUnit.Framework;
using System.IO;
using CargoWise.RefDbRepo.NZReferenceData.Business;

namespace CargoWise.RefDbRepo.NZReferenceData.Tests
{
	public class CodeDescriptionTest
	{
		[Test]
		public void TestGetCodeDescriptionsFromJsonPath_ReturnsEmptyDictionary_WhenPathIsNullOrWhitespace()
		{
			var loggerMock = new Mock<ILogger>();
			Assert.IsEmpty(CodeDescription.GetCodeDescriptionsFromJsonPath(null, loggerMock.Object));
			Assert.IsEmpty(CodeDescription.GetCodeDescriptionsFromJsonPath("", loggerMock.Object));
			Assert.IsEmpty(CodeDescription.GetCodeDescriptionsFromJsonPath("   ", loggerMock.Object));
			loggerMock.VerifyNoOtherCalls();
		}

		[Test]
		public void TestGetCodeDescriptionsFromJsonPath_LogsErrorAndReturnsEmpty_WhenFileDoesNotExist()
		{
			var loggerMock = new Mock<ILogger>();
			var result = CodeDescription.GetCodeDescriptionsFromJsonPath("nonexistent.json", loggerMock.Object);
			Assert.IsEmpty(result);
			loggerMock.Verify(l => l.LogError(It.Is<string>(msg => msg.Contains("Could not open lookup item in provided path"))), Times.Once);
		}

		[Test]
		public void TestGetCodeDescriptionsFromJsonPath_LogsErrorAndReturnsEmpty_WhenJsonIsInvalid()
		{
			var loggerMock = new Mock<ILogger>();
			var path = Path.GetTempFileName();
			File.WriteAllText(path, "{ invalid json }");

			try
			{
				var result = CodeDescription.GetCodeDescriptionsFromJsonPath(path, loggerMock.Object);
				Assert.IsEmpty(result);
				loggerMock.Verify(l => l.LogError(It.Is<string>(msg => msg.Contains("Invalid JSON"))), Times.Once);
			}
			finally
			{
				File.Delete(path);
			}
		}

		[Test]
		public void TestGetCodeDescriptionsFromJsonPath_ReturnsDictionary_WhenJsonIsValid()
		{
			var loggerMock = new Mock<ILogger>();
			var path = Path.GetTempFileName();
			const string json = "[{\"Code\":\"A1\",\"Description\":\"Desc1\"},{\"Code\":\"B2\",\"Description\":\"Desc2\"}]";

			File.WriteAllText(path, json);

			try
			{
				var result = CodeDescription.GetCodeDescriptionsFromJsonPath(path, loggerMock.Object);
				Assert.AreEqual(2, result.Count);
				Assert.AreEqual("Desc1", result["A1"].Description);
				Assert.AreEqual("Desc2", result["B2"].Description);
				loggerMock.VerifyNoOtherCalls();
			}
			finally
			{
				File.Delete(path);
			}
		}
	}
}
