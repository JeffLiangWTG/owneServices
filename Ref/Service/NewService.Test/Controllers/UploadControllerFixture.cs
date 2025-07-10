using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.NewService.UploadDownloadService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test.Controllers
{
	[TestFixture]
	public class UploadControllerFixture
	{
		[Test]
		public async Task UploadWontWorkWithWrongParameters()
		{
			var result = await controller.Send(RequiredParamSource, "wrongparam", RequiredParamContentType, "anyemail@email.com");
			Assert.AreEqual(StatusCodes.Status412PreconditionFailed, ((StatusCodeResult)result).StatusCode);
		}

		[Test]
		public async Task Upload()
		{
			var bytes = new byte[100];
			using (var stream = new MemoryStream(bytes))
			{
				request.SetupGet(x => x.Body).Returns(stream);
				controller.ControllerContext = controllerContext;
				var contracts = "anyemail@email.com";
				await controller.Send(RequiredParamSource, RequiredParamFileType, RequiredParamContentType, contracts);
				sourceDataProvider.Verify(o => o.UploadFileToServer(RequiredParamSource, RequiredParamFileType, RequiredParamContentType, contracts, It.IsAny<string>(), It.IsAny<FileMultipartSection>()), Times.Never);
			}
		}

		[Test]
		public async Task UploadFileToServer_ReturnPreconditionFailed_WhenFileNameIsInvalid()
		{
			var testSourceDataProvider = new SourceDataProvider("test staging connection string");
			var testController = new UploadController(testSourceDataProvider);
			request.Setup(r => r.ContentType).Returns("multipart/form-data; boundary=----WebKitFormBoundaryTest");
			request.Setup(r => r.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(@"
------WebKitFormBoundaryTest
Content-Disposition: form-data; name=""uploadedFile""; filename=""<a href=https://test.com>click here</a>""
Content-Type: application/zip

File Content
------WebKitFormBoundaryTest—-
")));
			const string contentDispositionHeader = "form-data; name=\"file\"; filename=\"<a href=https://test.com>click here</a>\"";
			request.Setup(r => r.Headers[HeaderNames.ContentDisposition]).Returns(contentDispositionHeader);
			testController.ControllerContext = controllerContext;
			var result = await testController.Send(RequiredParamSource, "XML", RequiredParamContentType, "anyemail@email.com");
			Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(StatusCodes.Status412PreconditionFailed));
			Assert.That(((ObjectResult)result).Value, Is.EqualTo("file name is invalid."));
		}

		[Test]
		public async Task GetFile()
		{
			var sourceDataPK = Guid.NewGuid();
			var fileName = "testfile.xml";
			var fileContent = "<abcd 1234 />";
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent)))
			{
				sourceDataProvider.Setup(x => x.GetFileFromServer(sourceDataPK)).ReturnsAsync((fileName, stream));
				var result = (FileStreamResult)await controller.GetFile(sourceDataPK);
				Assert.AreEqual(fileName, result.FileDownloadName);
				using (var streamReader = new StreamReader(result.FileStream))
				{
					Assert.AreEqual(fileContent, streamReader.ReadToEnd());
				}
			}
		}

		[Test]
		public async Task GetFileReturnsNull()
		{
			var result = await controller.GetFile(Guid.NewGuid());
			Assert.True(result is NotFoundResult);
		}

		UploadController controller;
		ControllerContext controllerContext;
		Mock<HttpContext> httpContext;
		Mock<HttpRequest> request;
		Mock<ISourceDataProvider> sourceDataProvider;
		const string RequiredParamSource = "UPL";
		const string RequiredParamFileType = "COM";
		const string RequiredParamContentType = "URD";

		[SetUp]
		public void SetUp()
		{
			sourceDataProvider = new Mock<ISourceDataProvider>();
			controller = new UploadController(sourceDataProvider.Object);
			controllerContext = new ControllerContext();
			httpContext = new Mock<HttpContext>();
			request = new Mock<HttpRequest>();
			httpContext.SetupGet(x => x.Request).Returns(request.Object);
			controllerContext.HttpContext = httpContext.Object;
		}
	}
}
