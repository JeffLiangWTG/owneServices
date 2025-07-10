using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.JPReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	abstract class BaseNaccsXmlWriterTest
	{
		[Test]
		public void TestCreateUXMLFile()
		{
			TestCreateUXMLFileCore(false, null);

			if (InputErrorFilePath != null && ExpectErrorMessage != null)
			{
				TestCreateUXMLFileCore(true, ExpectErrorMessage);
			}
		}

		protected void TestCreateUXMLFileCore(bool expectHasError, string errorMessage)
		{
			var dirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			var inputPath = expectHasError ? InputErrorFilePath : InputFilePath;
			var inputFilePath = Path.Combine(dirPath, inputPath);
			var expectedFilePath = Path.Combine(dirPath, ExpectedOutputFilePath);
			var actualFilePath = Path.Combine(AppConfig.Shared.OutputDirectory, ActualOutputFilePath);

			var expectedXmlAsString = File.ReadAllText(expectedFilePath);
			var mockHttpClientHelper = new Mock<IHttpClientHelper>();
			var htmlContent = TestHelper.ReadManifestResourceContent(BasePageHtmlFilePath);

			using (var mockFileStream = new FileStream(inputFilePath, FileMode.Open))
			{
				mockHttpClientHelper.Setup(x => x.GetAsync(FileDownloadUrl)).Returns(Task.FromResult<Stream>(mockFileStream));
				mockHttpClientHelper.Setup(x => x.GetWebPageAsync(BasePageDownloadUrl)).Returns(Task.FromResult(htmlContent));

				var httpClientHelper = mockHttpClientHelper.Object;
				if (expectHasError)
				{
					RunAndAssertError(httpClientHelper, errorMessage);
				}
				else
				{
					RunAndAssert(httpClientHelper, actualFilePath, expectedXmlAsString);
				}

				File.Delete(actualFilePath);
			}
		}

		protected void RunAndAssert(IHttpClientHelper httpClientHelper, string actualFilePath, string expectedXmlAsString)
		{
			var xmlProducer = GetNaccsXmlWriter();
			xmlProducer.WriteXml(httpClientHelper);

			var actualXmlAsString = File.ReadAllText(actualFilePath);
			Assert.That(actualXmlAsString, Is.EqualTo(expectedXmlAsString));
		}

		protected virtual void RunAndAssertError(IHttpClientHelper httpClientHelper, string errorMessage)
		{
			var xmlProducer = GetNaccsXmlWriter();
			Assert.Throws<UnhandledApplicationException>(() => xmlProducer.WriteXml(httpClientHelper), errorMessage);
		}

		protected abstract string InputFilePath { get; }

		protected virtual string InputErrorFilePath => null;

		protected virtual string ExpectErrorMessage => null;

		protected virtual string BasePageDownloadUrl => AppConfig.NACCS.CodeLists.BaseUrl;

		protected virtual string BasePageHtmlFilePath => "CargoWise.RefDbRepo.JPReferenceData.Tests.TestFiles.NACCSCodeListIndexHtml.html";

		protected abstract string ExpectedOutputFilePath { get; }

		protected abstract string ActualOutputFilePath { get; }

		protected abstract string FileDownloadUrl { get; }

		protected abstract NaccsXmlWriter GetNaccsXmlWriter();
	}
}
