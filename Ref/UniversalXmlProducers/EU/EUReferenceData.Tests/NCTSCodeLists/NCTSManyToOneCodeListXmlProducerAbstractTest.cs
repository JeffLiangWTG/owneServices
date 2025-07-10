using System.Collections.Generic;
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
	abstract class NctsManyToOneCodeListXmlProducerAbstractTest<T> where T : NctsManyToOneCodeListXMLProducer
	{
		[Test]
		public void DownloadAndCreateRefCusCodeListXML()
		{
			foreach (var codeListDetail in CodeListDetails)
			{
				var zipStream = TestHelper.ReadManifestResourceContentAsStream($"CargoWise.RefDbRepo.EUReferenceData.Tests.NCTSCodeLists.TestFiles.Input.{codeListDetail.InputFileName}");
				httpClientMock.Setup(x => x.GetAsync(codeListDetail.DownloadURL)).Returns(Task.FromResult(zipStream));
			}
			var errors = Producer.DownloadAndConvertAllRefCusCodeListXMLs(httpClientMock.Object, outputPath).Result;
			var expectedXML = TestHelper.ReadManifestResourceContentAsString($"CargoWise.RefDbRepo.EUReferenceData.Tests.NCTSCodeLists.TestFiles.Output.{OutputFileName}");
			var actualXml = File.ReadAllText(outputFile);
			Assert.That(actualXml, Is.EqualTo(expectedXML));		
		}

		[SetUp]
		public void SetUp()
		{
			var assembly = Assembly.GetExecutingAssembly();
			outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"EU\TestFiles\NCTSCodeLists");
			outputFile = Path.Combine(outputPath, OutputFileName);
			httpClientMock = new Mock<IHttpClientHelper>();
		}

		[TearDown]
		public void Teardown()
		{
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}
		}

		protected abstract T Producer { get; }

		protected abstract string OutputFileName { get; }

		protected abstract IEnumerable<CodeListInformation> CodeListDetails { get; }

		internal class CodeListInformation
		{
			internal string InputFileName;

			internal string DownloadURL;
		}

		string outputPath;

		string outputFile;

		Mock<IHttpClientHelper> httpClientMock;
	}
}
