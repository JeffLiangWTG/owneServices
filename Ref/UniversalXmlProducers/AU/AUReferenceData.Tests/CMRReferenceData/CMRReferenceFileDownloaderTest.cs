using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	[TestFixture]
	sealed class CMRReferenceFileDownloaderTest
	{
		[Test]
		public void TestDownload()
		{
			var manifestResourcePathBase = string.Join(".", executingAssembly.GetName().Name, "CMRReferenceData", "TestFiles");

			using (var htmlStream = executingAssembly.GetManifestResourceStream(string.Join(".", manifestResourcePathBase, "MainPage.html")))
			using (var htmlReader = new StreamReader(htmlStream))
			{
				var expectedContent = "Expected downloaded content";
				var mockHttpClientHelper = new Mock<IHttpClientHelper>();
				mockHttpClientHelper.Setup(x => x.GetWebPageAsync(It.Is<string>(uri => !uri.EndsWith(".txt")))).Returns<string>(x => Task.FromResult(htmlReader.ReadToEnd()));
				mockHttpClientHelper.Setup(x => x.GetWebPageAsync(It.Is<string>(uri => uri.EndsWith(".txt")))).Returns<string>(x => Task.FromResult(expectedContent));

				(var content, var publishedDate) = CMRReferenceFileDownloader.Download(mockHttpClientHelper.Object, ApplicationConfig.AQISCommodityCodesFilePrefix, ApplicationConfig.AUReferenceFilesDirectory);
				Assert.AreEqual(expectedContent, content);
				Assert.AreEqual(new DateTime(2022, 03, 02, 03, 03, 00), publishedDate);
			}
		}

		[SetUp]
		public void Setup()
		{
			executingAssembly = Assembly.GetExecutingAssembly();
		}
		Assembly executingAssembly;
	}
}
