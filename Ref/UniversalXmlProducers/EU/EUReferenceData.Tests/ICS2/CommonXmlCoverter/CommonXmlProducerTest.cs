using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	abstract class CommonXmlProducerTest<T> where T : CommonXMLProducer
	{
		[Test]
		public void TestDownloadAndConvertToXML()
		{
			httpClientMock.Setup(x => x.GetAsync(DownloadUrl)).Returns(Task.FromResult(ZipFileForTest));

			var producer = (T)Activator.CreateInstance(typeof(T));
			var errors = producer.DownloadAndConvertToXML(httpClientMock.Object, outputPath, DownloadUrl);
			Assert.That(errors, Is.Empty);
			var actualXml = File.ReadAllText(outputFile);
			Assert.That(actualXml, Is.EqualTo(ExpectedXML));

			if (!string.IsNullOrEmpty(RefCusCodeTypeOutputFileName))
			{
				var actualRefCusCodeTypeXml = File.ReadAllText(outputRefCusCodeTypeFilePath);
				Assert.That(actualRefCusCodeTypeXml, Is.EqualTo(ExpectedRefCusCodeTypeXML));
			}
		}

		[SetUp]
		public void SetUp()
		{
			var assembly = Assembly.GetExecutingAssembly();
			outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"EU\TestFiles\ICS2");
			outputFile = Path.Combine(outputPath, OutputFileName);
			if (!string.IsNullOrEmpty(RefCusCodeTypeOutputFileName))
			{
				outputRefCusCodeTypeFilePath = Path.Combine(outputPath, RefCusCodeTypeOutputFileName);
			}
			httpClientMock = new Mock<IHttpClientHelper>();
		}

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			ManifestResourceHelper.ExtractAdditionalTranslationsResources();
		}

		string outputPath;
		string outputFile;
		string outputRefCusCodeTypeFilePath;
		Mock<IHttpClientHelper> httpClientMock;

		[TearDown]
		public void Teardown()
		{
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}
			if (File.Exists(outputRefCusCodeTypeFilePath))
			{
				File.Delete(outputRefCusCodeTypeFilePath);
			}
		}

		protected abstract string DownloadUrl { get; }

		protected abstract Stream ZipFileForTest { get; }

		protected abstract string OutputFileName { get; }

		protected abstract string ExpectedXML { get; }

		protected virtual string RefCusCodeTypeOutputFileName { get; }

		protected virtual string ExpectedRefCusCodeTypeXML { get; }
	}
}
