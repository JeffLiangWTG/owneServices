using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.CLASETMessageProcessor.Test
{
	public abstract class BaseMessageProcessorFixture
	{
		protected void AssertProcess(string testFile, Action<FileInfo[], int> assertResult)
		{
			var testFileFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + @"TestData\", testFile);
			var testData = File.ReadAllText(testFileFullPath);
			AssertProcessData(testData, assertResult);
		}

		protected (Mock<IStagingRepository> stagingMock, SourceData sourceData, DirectoryInfo directoryInfo, int noOfUpdates) ProccessData(string testData)
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
			var processor = new CLASETMessageProcessor(stagingMock.Object, TestOutputFilePath);
			return (stagingMock, sourceData, directoryInfo, processor.Process());
		}

		protected void AssertProcessData(string testData, Action<FileInfo[], int> assertResult)
		{
			(var stagingMock, var sourceData, var directoryInfo, var noOfUpdates) = ProccessData(testData);
			Assert.That(sourceData.IsSourceDataMergedStatus());

			stagingMock.Verify(x => x.SaveChanges());
			Assert.That(directoryInfo.Exists, Is.EqualTo(true));

			var xmlFiles = directoryInfo.GetFiles("*.xml").OrderBy(x => x.Name).ToArray();
			assertResult(xmlFiles, noOfUpdates);
		}

		protected void AssertSameDataAsDirectory(string directoryName, FileInfo[] xmlFiles)
		{
			var testResultFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + @"XMLResult\", directoryName);
			var directoryInfo = new DirectoryInfo(testResultFullPath);

			var expectedXmlFiles = directoryInfo.GetFiles("*.xml").OrderBy(x => x.Name).ToArray();
			Assert.AreEqual(expectedXmlFiles.Length, xmlFiles.Length);

			for (int i = 0; i < expectedXmlFiles.Length; i++)
			{
				var expectedXmlFile = expectedXmlFiles[i];
				var xmlFile = xmlFiles[i];

				Assert.AreEqual(true, xmlFile.Name.StartsWith(Path.GetFileNameWithoutExtension(expectedXmlFile.Name)), xmlFile.Name + Environment.NewLine + expectedXmlFile.Name);
				Assert.AreEqual(File.ReadAllText(expectedXmlFile.FullName).TrimEnd(), File.ReadAllText(xmlFile.FullName).TrimEnd(), expectedXmlFile.Name);
			}
		}

		protected string TestOutputFilePath => AppDomain.CurrentDomain.BaseDirectory + @"SGCLASET\Output\";

		protected (Mock<IStagingRepository> stagingMock, SourceData sourceData) Setup(string contentText)
		{
			var sourceData = new SourceData
			{
				SDA_ContentText = contentText.Replace("\n", "").Replace("\r", "").Replace("\t", ""),
				SDA_Status = StatusProvider.GetQUEStatus(),
				SDA_Source = DataSourceConstants.Country.Singapore,
				SDA_SubSource = DataSourceConstants.SubSource.CLASET,
				SDA_ContentType = ContentType,
				SDA_CreatedTime = new DateTime(2019, 1, 1)
			};

			var stagingMock = new Mock<IStagingRepository>();
			stagingMock.Setup(x => x.Get<SourceData>()).Returns(new[] { sourceData }.AsQueryable());

			return (stagingMock, sourceData);
		}

		[Test]
		public void ProcessInvalidData()
		{
			ProcessInvalidDataCore("SDS", StatusProvider.GetERRStatus());
		}

		protected void ProcessInvalidDataCore(string content, string status)
		{
			(var stagingMock, var sourceData, var directoryInfo, var noOfUpdates) = ProccessData(content);
			Assert.That(sourceData.SDA_Status, Is.EqualTo(status));

			stagingMock.Verify(x => x.SaveChanges());

			Assert.That(directoryInfo.Exists, Is.EqualTo(false));
			Assert.AreEqual(0, noOfUpdates);
		}

		protected abstract string ContentType { get; }
	}
}
