using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests.NCTSTradeGroups
{
	abstract class NctsTradeGroupXMLProducerAbstractTest
	{
		[Test]
		public void DownloadAndCreateRefCusCodeListXML()
		{
			var zipStream = TestHelper.ReadManifestResourceContentAsStream($"CargoWise.RefDbRepo.EUReferenceData.Tests.NCTSTradeGroups.TestFiles.Input.{InputFileName}");
			httpClientMock.Setup(x => x.GetAsync(DownloadUrl)).Returns(Task.FromResult(zipStream));

			var errors = Producer.DownloadAndConvertToRefCusTradeGroupXML(httpClientMock.Object, outputPath);
			var expectedXML = TestHelper.ReadManifestResourceContentAsString($"CargoWise.RefDbRepo.EUReferenceData.Tests.NCTSTradeGroups.TestFiles.Output.{OutputFileName}");
			var actualXml = File.ReadAllText(outputFile);
			Assert.That(actualXml, Is.EqualTo(expectedXML));
		}

		[SetUp]
		public void SetUp()
		{
			var assembly = Assembly.GetExecutingAssembly();
			outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"EU\TestFiles\NCTSTradeGroups");
			outputFile = Path.Combine(outputPath, OutputFileName);
			httpClientMock = new Mock<IHttpClientHelper>();
		}
		string outputPath;
		string outputFile;
		Mock<IHttpClientHelper> httpClientMock;

		[TearDown]
		public void Teardown()
		{
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}
		}

		protected abstract NctsTradeGroupXMLProducer Producer { get; }

		protected abstract string DownloadUrl { get; }

		protected abstract string InputFileName { get; }

		protected abstract string OutputFileName { get; }
	}
}
