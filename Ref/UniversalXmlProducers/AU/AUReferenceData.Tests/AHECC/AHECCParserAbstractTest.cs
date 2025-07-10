using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.AHECC
{
	[TestFixture]
	[SetCulture("en-AU")]
	abstract class AHECCParserAbstractTest
	{
		protected abstract string ManifestResourcePathBase { get; }

		protected abstract string ManifestResourceFileName { get; }

		protected abstract string DownloadFileName { get; }

		protected abstract string ProducedFileName { get; }

		protected abstract string ExpectOutputFileName { get; }

		protected abstract BaseAHECCParser AHECCParser { get; }

		[Test, Timeout(10000),]
		public void TestReadAHECC()
		{
			var parser = AHECCParser;
			parser.HttpClientHelper = CreateHttpClientHelper();
			parser.Parse();
			using (var expectedStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ManifestResourcePathBase + "." + ExpectOutputFileName))
			using (var producedStream = new FileStream(Path.Combine(ApplicationConfig.OutputPath, ProducedFileName), FileMode.Open))
			using (var expectedReader = new StreamReader(expectedStream))
			using (var producedReader = new StreamReader(producedStream))
			{
				Assert.AreEqual(expectedReader.ReadToEnd(), producedReader.ReadToEnd());
			}
		}

		protected IHttpClientHelper CreateHttpClientHelper()
		{
			var mockHttpClientHelper = new Mock<IHttpClientHelper>();
			mockHttpClientHelper.Setup(x => x.GetWebPageAsync(It.Is<string>(uri => !uri.EndsWith(".txt")))).Returns<string>(x =>
			{
				var resourcePath = string.Join(".", ManifestResourcePathBase, ManifestResourceFileName);
				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath))
				using (var reader = new StreamReader(stream))
				{
					return Task.FromResult(reader.ReadToEnd());
				}
			});
			mockHttpClientHelper.Setup(x => x.GetWebPageAsync(It.Is<string>(uri => uri.EndsWith(".txt")))).Returns<string>(x =>
			{
				var resourcePath = string.Join(".", ManifestResourcePathBase, DownloadFileName);
				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath))
				using (var reader = new StreamReader(stream))
				{
					return Task.FromResult(reader.ReadToEnd());
				}
			});

			return mockHttpClientHelper.Object;
		}

		[TearDown]
		public virtual void TearDown()
		{
			File.Delete(Path.Combine(ApplicationConfig.OutputPath, ProducedFileName));
		}
	}
}
