using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.IFTRINMessageProcessor.Test
{
	public abstract class BaseMessageProcessorFixture
	{
		[Test]
		public void ProcessInvalidData()
		{
			(var stagingMock, var sourceData, var directoryInfo, var noOfUpdates) = ProccessData("SDS");
			Assert.That(sourceData.IsSourceDataErrorStatus());
			stagingMock.Verify(x => x.SaveChanges());
			Assert.That(directoryInfo.Exists, Is.EqualTo(false));
		}

		protected abstract string CountryCode { get; }

		protected abstract string ContentType { get; }

		protected abstract IFTRINMessageProcessor CreateProcessor(IStagingRepository staging, string outputPath);

		protected void AssertProcess(string testFile, Action<FileInfo[], int> assertResult)
		{
			var testData = ReadAllTextFromManifestResource("CargoWise.RefDbRepo.Staging.IFTRINMessageProcessor.Test.TestFiles.Input." + testFile);
			AssertProcessData(testData, assertResult);
		}

		(Mock<IStagingRepository> stagingMock, SourceData sourceData, DirectoryInfo directoryInfo, int noOfUpdates) ProccessData(string testData)
		{
			var directoryInfo = new DirectoryInfo(TestOutputFilePath);
			try
			{
				directoryInfo.Delete(true);
			}
			catch (IOException)
			{
				// do nothing
			}
			(var stagingMock, var sourceData) = Setup(testData);
			var processor = CreateProcessor(stagingMock.Object, TestOutputFilePath);
			return (stagingMock, sourceData, directoryInfo, processor.Process());
		}

		void AssertProcessData(string testData, Action<FileInfo[], int> assertResult)
		{
			(var stagingMock, var sourceData, var directoryInfo, var noOfUpdates) = ProccessData(testData);
			Assert.That(sourceData.IsSourceDataMergedStatus());

			stagingMock.Verify(x => x.SaveChanges());
			Assert.That(directoryInfo.Exists, Is.EqualTo(true));

			var xmlFiles = directoryInfo.GetFiles("*.xml").OrderBy(x => x.Name).ToArray();
			assertResult(xmlFiles, noOfUpdates);
		}

		protected void AssertSameDataAsDirectory(string directoryName, FileInfo[] actualOutputFiles)
		{
			foreach (var outputFile in actualOutputFiles)
			{
				var expectedXmlContent = ReadAllTextFromManifestResource($"CargoWise.RefDbRepo.Staging.IFTRINMessageProcessor.Test.TestFiles.Output.{directoryName}.{outputFile.Name}");
				var actualXmlContent = File.ReadAllText(outputFile.FullName);
				var regex = new Regex(@"<AppProgramArgs>(.*?)</AppProgramArgs>");
				var match = regex.Match(actualXmlContent);
				if (match.Success)
				{
					actualXmlContent = actualXmlContent.Replace(match.Value, "<AppProgramArgs></AppProgramArgs>");
				}
				Assert.AreEqual(expectedXmlContent.TrimEnd(), actualXmlContent.TrimEnd());
			}
		}

		string TestOutputFilePath => AppDomain.CurrentDomain.BaseDirectory + CountryCode + @"IFTRIN\Output\";

		(Mock<IStagingRepository> stagingMock, SourceData sourceData) Setup(string contentText)
		{
			var sourceData = new SourceData
			{
				SDA_ContentText = contentText.Replace("\n", "").Replace("\r", "").Replace("\t", ""),
				SDA_Status = StatusProvider.GetQUEStatus(),
				SDA_Source = CountryCode,
				SDA_SubSource = DataSourceConstants.SubSource.IFTRIN,
				SDA_ContentType = ContentType,
				SDA_CreatedTime = new DateTime(2019, 1, 1)
			};

			var stagingMock = new Mock<IStagingRepository>();
			stagingMock.Setup(x => x.Get<SourceData>()).Returns(new[] { sourceData }.AsQueryable());

			return (stagingMock, sourceData);
		}

		string ReadAllTextFromManifestResource(string resourceName)
		{
			using (var reader = new StreamReader(GetType().Assembly.GetManifestResourceStream(resourceName), Encoding.UTF8))
			{
				return reader.ReadToEnd();
			}
		}
	}
}
