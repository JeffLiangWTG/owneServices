using System.Text.Json.Nodes;
using eServices.Dms.Core.ServiceDefaults.Authentication;
using eServices.Dms.Core.StorageApi.Services;
using eServices.Dms.Core.StorageRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace eServices.Dms.Core.StorageApi.Tests;

public class DocsEndpointsTest
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
		storageRepository.Setup(x => x.TableExistsAsync("USC", "ViaDMS", DmsTableTypes.Document)).ReturnsAsync(true);
	}

	[TestCase("APP", "ViaDMS", "HYEDUSCMT", typeof(UnauthorizedHttpResult), null)]
	[TestCase("USC", "NotExist", "HYEDUSCMT", typeof(NotFound), null)]
	[TestCase("USC", "ViaDMS", "HYEDAUTST", typeof(NotFound), null)]
	[TestCase("USC", "ViaDMS", "HYEDUSCMT", typeof(Ok<JsonNode>), "{\"ViaDMS\":true}")]
	public async Task GetDocumentTests(string owner, string tableName, string key, Type resultType, string? document)
	{
		if (document is not null)
			storageRepository.Setup(x => x.GetDocumentAsJsonAsync("USC", "ViaDMS", "HYEDUSCMT")).ReturnsAsync(JsonNode.Parse(document));

		var result = await DocsEndpoints.GetDocument(storageRepository.Object, httpContext.Object, owner, tableName, key);

		Assert.Multiple(() =>
		{
			Assert.That(result.Result, Is.TypeOf(resultType));
			if (result.Result is IValueHttpResult<JsonNode> jsonResult)
				Assert.That(jsonResult.Value?.ToJsonString(), Is.EqualTo(document));

		});
	}

	[TestCase("APP", "ViaDMS", "HYEDUSCMT", "{\"ViaDMS\":true}", typeof(UnauthorizedHttpResult))]
	[TestCase("USC", "NotExist", "HYEDUSCMT", "{\"ViaDMS\":true}", typeof(NotFound))]
	[TestCase("USC", "ViaDMS", "HYEDAUTST", "{\"ViaDMS\":true}", typeof(Ok))]
	[TestCase("USC", "ViaDMS", "HYEDUSCMT", "{\"ViaDMS\":true}", typeof(Ok))]
	public async Task PutDocumentTests(string owner, string tableName, string key, string document, Type resultType)
	{
		var jsonDocument = JsonNode.Parse(document)!;
		storageRepository.Setup(x => x.PutDocumentAsJsonAsync(owner, tableName, key, jsonDocument, userName)).ReturnsAsync(1);

		var result = await DocsEndpoints.PutDocument(storageRepository.Object, httpContext.Object, owner, tableName, key, jsonDocument);

		Assert.That(result.Result, Is.TypeOf(resultType));
		if (result.Result is Ok)
			storageRepository.Verify(x => x.PutDocumentAsJsonAsync(owner, tableName, key, jsonDocument, userName), Times.Once);
		else
			storageRepository.Verify(x => x.PutDocumentAsJsonAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<JsonNode>(), It.IsAny<string>()), Times.Never);
	}
}
