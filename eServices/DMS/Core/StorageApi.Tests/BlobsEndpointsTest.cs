using System.Text;
using eServices.Dms.Core.ServiceDefaults.Authentication;
using eServices.Dms.Core.StorageApi.Services;
using eServices.Dms.Core.StorageRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace eServices.Dms.Core.StorageApi.Tests;

public class BlobsEndpointsTest
{
	private const string userName = "USER";
	private Mock<HttpContext> httpContext;
	private Mock<IDmsStorageRepository> storageRepository;

	[SetUp]
	public void Setup()
	{
		httpContext = new Mock<HttpContext>();
		httpContext.Setup(c => c.User.IsInRole($"{AppRoles.ReadStorage}:USC")).Returns(true);
		httpContext.SetupGet(c => c.User.Identity!.Name).Returns(userName);
		storageRepository = new Mock<IDmsStorageRepository>();
		storageRepository.Setup(x => x.TableExistsAsync("USC", "RefFiles", DmsTableTypes.Object)).ReturnsAsync(true);
	}

	[TestCase("APP", "RefFiles", "FIRMS", typeof(UnauthorizedHttpResult), null)]
	[TestCase("USC", "NotExist", "FIRMS", typeof(NotFound), null)]
	[TestCase("USC", "RefFiles", "ADCV", typeof(NotFound), null)]
	[TestCase("USC", "RefFiles", "FIRMS", typeof(FileStreamHttpResult), "ABCDEFG\r\nHIJKLMN")]
	public async Task GetObjectTests(string owner, string tableName, string key, Type resultType, string? rawObject)
	{
		var rawBytes = Encoding.UTF8.GetBytes(rawObject ?? "");
		if (rawObject is not null)
			storageRepository.Setup(x => x.GetObjectAsStreamAsync(owner, tableName, key)).ReturnsAsync(new MemoryStream(rawBytes));

		var result = await BlobsEndpoints.GetObject(storageRepository.Object, httpContext.Object, owner, tableName, key);

		Assert.Multiple(() =>
		{
			Assert.That(result.Result, Is.TypeOf(resultType));
			if (result.Result is FileStreamHttpResult streamResult)
			{
				var length = (int)streamResult.FileLength.GetValueOrDefault();
				var resultBytes = new byte[length];
				streamResult.FileStream.Read(resultBytes, 0, length);
				Assert.That(resultBytes, Is.EquivalentTo(rawBytes));
			}
		});
	}

	[TestCase("APP", "RefFiles", "FIRMS", "ABCDEFG\r\nHIJKLMN", typeof(UnauthorizedHttpResult))]
	[TestCase("USC", "NotExist", "FIRMS", "ABCDEFG\r\nHIJKLMN", typeof(NotFound))]
	[TestCase("USC", "RefFiles", "ADCV", "ABCDEFG\r\nHIJKLMN", typeof(Ok))]
	[TestCase("USC", "RefFiles", "FIRMS", "ABCDEFG\r\nHIJKLMN", typeof(Ok))]
	public async Task PutObjectTests(string owner, string tableName, string key, string rawObject, Type resultType)
	{
		var rawBytes = Encoding.UTF8.GetBytes(rawObject);
		var blob = new MemoryStream(rawBytes);
		httpContext.SetupGet(c => c.Request.Body).Returns(blob);
		storageRepository.Setup(x => x.PutObjectAsStreamAsync(owner, tableName, key, blob, userName)).ReturnsAsync(1);

		var result = await BlobsEndpoints.PutObject(storageRepository.Object, httpContext.Object, owner, tableName, key);

		Assert.That(result.Result, Is.TypeOf(resultType));
		if (result.Result is Ok)
			storageRepository.Verify(x => x.PutObjectAsStreamAsync(owner, tableName, key, blob, userName), Times.Once);
		else
			storageRepository.Verify(x => x.PutObjectAsStreamAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
	}
}
