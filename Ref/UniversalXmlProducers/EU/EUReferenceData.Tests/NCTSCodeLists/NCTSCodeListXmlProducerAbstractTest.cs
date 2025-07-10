using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	abstract class NCTSCodeListXmlProducerAbstractTest<T> where T : NCTSCodeListXmlProducer
	{
		[Test]
		public void DownloadAndCreateRefCusCodeListXML()
		{
			var assembly = Assembly.GetExecutingAssembly();
			outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"EU\TestFiles\NCTSCodeLists");
			outputRefCusCodeListFilePath = Path.Combine(outputPath, RefCusCodeListOutputFileName);
			outputRefCusCodeTypeFilePath = Path.Combine(outputPath, RefCusCodeTypeOutputFileName);
			httpClientMock = new Mock<IHttpClientHelper>();
			var zipStream = TestHelper.ReadManifestResourceContentAsStream($"CargoWise.RefDbRepo.EUReferenceData.Tests.NCTSCodeLists.TestFiles.Input.{InputFileName}");
			httpClientMock.Setup(x => x.GetAsync(DownloadUrl)).Returns(Task.FromResult(zipStream));
			var errors = Producer.DownloadAndConvertAllRefCusCodeListXMLs(httpClientMock.Object, outputPath).Result;
			var expectedRefCusCodeListXML = TestHelper.ReadManifestResourceContentAsString($"CargoWise.RefDbRepo.EUReferenceData.Tests.NCTSCodeLists.TestFiles.Output.{RefCusCodeListOutputFileName}");
			var actualRefCusCodeListXml = File.ReadAllText(outputRefCusCodeListFilePath);
			Assert.That(actualRefCusCodeListXml, Is.EqualTo(expectedRefCusCodeListXML));

			if (!string.IsNullOrEmpty(RefCusCodeTypeOutputFileName))
			{
				var expectedRefCusCodeTypeXML = TestHelper.ReadManifestResourceContentAsString($"CargoWise.RefDbRepo.EUReferenceData.Tests.NCTSCodeLists.TestFiles.Output.{RefCusCodeTypeOutputFileName}");
				var actualRefCusCodeTypeXml = File.ReadAllText(outputRefCusCodeTypeFilePath);
				Assert.That(actualRefCusCodeTypeXml, Is.EqualTo(expectedRefCusCodeTypeXML));
			}
		}

		[TearDown]
		public void Teardown()
		{
			if (File.Exists(outputRefCusCodeListFilePath))
			{
				File.Delete(outputRefCusCodeListFilePath);
			}
			if (File.Exists(outputRefCusCodeTypeFilePath))
			{
				File.Delete(outputRefCusCodeTypeFilePath);
			}
		}

		protected abstract T Producer { get; }

		protected abstract string InputFileName { get; }

		protected abstract string RefCusCodeListOutputFileName { get; }

		protected virtual string RefCusCodeTypeOutputFileName => string.Empty;

		protected abstract string DownloadUrl { get; }

		string outputPath;

		string outputRefCusCodeListFilePath;

		string outputRefCusCodeTypeFilePath;

		Mock<IHttpClientHelper> httpClientMock;

		
	}
}
