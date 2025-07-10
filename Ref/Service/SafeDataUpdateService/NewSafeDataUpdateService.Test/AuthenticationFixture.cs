using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.ErrorReporting;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Common.Logging;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WTG.OpenIDConnect.Token;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test;

[TestFixture]
class AuthenticationFixture
{
	[Test]
	public void TokenAuthentication()
	{
		var repo = new Mock<IReferenceDataRepository>();
		repo.Setup(x => x.Get<ClientRefDbVersionControl>()).Returns(Enumerable.Empty<ClientRefDbVersionControl>().AsQueryable());
		repo.Setup(x => x.Get<RefCusTariff>()).Returns(new[] { tariff }.AsQueryable());
		var auth = new Mock<IAuthorizationHelper>();
		auth.Setup(x => x.IsAuthorized<RefCusTariff>(It.IsAny<string>())).Returns(true);
		auth.Setup(x => x.IsAuthorized(It.IsAny<RefCusTariff>(), It.IsAny<string>())).Returns(true);
		var licenseProvider = new Mock<IClientLicenseInformationProvider>();
		licenseProvider.Setup(x => x.GetLicenceInformationInactive()).Returns(Task.FromResult(new string[0]));
		var jwtSecurityToken = new JwtSecurityToken("issuer", "audience", [new Claim("unique_name", "abc")]);
		var tokenValidationHelper = new Mock<ITokenValidationHelper>();
		tokenValidationHelper.Setup(x => x.VerifyAccessTokenAsync("abc", It.IsAny<ConcurrentDictionary<string, IConfigurationManagerWithLock>>(), It.IsAny<ILogger>(), CancellationToken.None)).Returns(Task.FromResult(jwtSecurityToken));

		using var factory = IntegrationTestHelper.WebAppFactory.WithWebHostBuilder(builder =>
		{
			builder.ConfigureTestServices(services =>
			{
				services.RemoveAll<ITokenValidationHelper>();
				services.AddScoped(sp => repo.Object)
					.AddScoped(sp => auth.Object)
					.AddScoped(sp => tokenValidationHelper.Object)
					.AddScoped(sp => licenseProvider.Object);

				services.Configure<TestServerOptions>(options =>
				{
					options.AllowSynchronousIO = true;
				});
			});
		});
		using var client = factory.CreateClient();
		foreach (var request in GetHttpRequestMessages())
		{
			HttpResponseMessage response = null;
			Assert.DoesNotThrowAsync(async () => response = await client.SendAsync(request));
		}
	}

	[Test]
	public async Task InvalidAuthenticationReturn401()
	{
		var repo = new Mock<IReferenceDataRepository>();
		var auth = new Mock<IAuthorizationHelper>();
		var tokenValidationHelper = new Mock<ITokenValidationHelper>();
		await using var factory = IntegrationTestHelper.WebAppFactory.WithWebHostBuilder(builder =>
		{
			builder.ConfigureTestServices(services =>
			{
				services.RemoveAll<ITokenValidationHelper>();
				services.AddScoped(sp => repo.Object)
					.AddScoped(sp => auth.Object)
					.AddScoped(sp => tokenValidationHelper.Object);

				services.Configure<TestServerOptions>(options =>
				{
					options.AllowSynchronousIO = true;
				});
			});
		});
		using var client = factory.CreateClient();
		foreach (var request in GetHttpRequestMessages())
		{
			HttpResponseMessage response = null;
			Assert.DoesNotThrowAsync(async () => response = await client.SendAsync(request));
			Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
			var resultContent = await response.Content.ReadAsStringAsync();
			Assert.AreEqual("Unauthorized: Invalid access token", resultContent);
		}
	}

	[Test]
	public void BatchRequestThrowExceptionWithInvalidAuthentication()
	{
		var log = new Mock<ILog>();
		var logWrapper = new Mock<ILogWrapper>();
		var errorRportWrapper = new Mock<ErrorReportingClientWrapper>(null);
		logWrapper.Setup(x => x.GetLog(nameof(AuthenticationLogger))).Returns(log.Object);

		var repo = new Mock<IReferenceDataRepository>();
		var auth = new Mock<IAuthorizationHelper>();
		var tokenValidationHelper = new Mock<ITokenValidationHelper>();
		using var factory = IntegrationTestHelper.WebAppFactory.WithWebHostBuilder(builder =>
		{
			builder.ConfigureTestServices(services =>
			{
				services.AddSingleton(logWrapper.Object)
					.AddSingleton(errorRportWrapper.Object);
				services.RemoveAll<ITokenValidationHelper>();
				services.AddScoped(sp => repo.Object)
					.AddScoped(sp => auth.Object)
					.AddScoped(sp => tokenValidationHelper.Object);

				services.Configure<TestServerOptions>(options =>
				{
					options.AllowSynchronousIO = true;
				});
			});
		});
		using var client = factory.CreateClient();
		using var batchRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost/odata/$batch");
		batchRequest.Content = CreateBatchRequestContent();
		var ex = Assert.ThrowsAsync<HttpRequestException>(async () => await client.SendAsync(batchRequest));
		Assert.That(ex?.Message, Does.Contain("Error while copying content to a stream."));
		log.Verify(x => x.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
	}

	[Test]
	public void BatchRequestNotThrowExceptionWithValidAuthentication()
	{
		var log = new Mock<ILog>();
		var logWrapper = new Mock<ILogWrapper>();
		var errorRportWrapper = new Mock<ErrorReportingClientWrapper>(null);
		logWrapper.Setup(x => x.GetLog(nameof(AuthenticationLogger))).Returns(log.Object);

		var repo = new Mock<IReferenceDataRepository>();
		repo.Setup(x => x.Get<RefCusTariff>()).Returns(new[] { tariff }.AsQueryable());
		var auth = new Mock<IAuthorizationHelper>();
		auth.Setup(x => x.IsAuthorized<RefCusTariff>(It.IsAny<string>())).Returns(true);
		auth.Setup(x => x.IsAuthorized(It.IsAny<RefCusTariff>(), It.IsAny<string>())).Returns(true);
		var tokenValidationHelper = new Mock<ITokenValidationHelper>();
		tokenValidationHelper.Setup(x => x.VerifyAccessTokenAsync(It.IsAny<string>(), It.IsAny<ConcurrentDictionary<string, IConfigurationManagerWithLock>>(), It.IsAny<ILogger>(), CancellationToken.None))
			.Returns(Task.FromResult(new JwtSecurityToken("issuer", "audience", new[] { new Claim(AuthClaimType.UniqueName, "abc") })));
		tokenValidationHelper.Setup(x => x.ShouldHandle(It.IsAny<string>())).Returns(true);

		using var factory = IntegrationTestHelper.WebAppFactory.WithWebHostBuilder(builder =>
		{
			builder.ConfigureTestServices(services =>
			{
				services.AddSingleton(logWrapper.Object)
					.AddSingleton(errorRportWrapper.Object);
				services.RemoveAll<ITokenValidationHelper>();
				services.AddScoped(sp => repo.Object)
					.AddScoped(sp => auth.Object)
					.AddScoped(sp => tokenValidationHelper.Object);

				services.Configure<TestServerOptions>(options =>
				{
					options.AllowSynchronousIO = true;
				});
			});
		});
		using var client = factory.CreateClient();
		using var batchRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost/odata/$batch");
		HttpResponseMessage response = null;
		batchRequest.Content = CreateBatchRequestContent();
		batchRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "abc");
		Assert.DoesNotThrowAsync(async () => response = await client.SendAsync(batchRequest));
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		log.Verify(x => x.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "used in test")]
	HttpRequestMessage[] GetHttpRequestMessages()
	{
		var removeInactiveClientsRequest = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/ClientDataSetVersionSummary/RemoveInactiveClients");
		var postRequest = CreateHttpRequestMessage(HttpMethod.Post, "http://localhost/odata/RefCusTariffUpdate");
		var putRequest = CreateHttpRequestMessage(HttpMethod.Put, "http://localhost/odata/RefCusTariffUpdate(9B79A9DB-3BCB-4757-9338-B0DD5EF2CAF2)");
		var patchRequest = CreateHttpRequestMessage(HttpMethod.Patch, "http://localhost/odata/RefCusTariffUpdate(9B79A9DB-3BCB-4757-9338-B0DD5EF2CAF2)");
		var deleteRequest = CreateHttpRequestMessage(HttpMethod.Delete, "http://localhost/odata/RefCusTariffUpdate(9B79A9DB-3BCB-4757-9338-B0DD5EF2CAF2)");
		var forceDeleteRequest = CreateHttpRequestMessage(HttpMethod.Post, "http://localhost/odata/RefCusTariffUpdate/Default.ForceDelete");
		var batchDeleteRequest = CreateHttpRequestMessage(HttpMethod.Post, "http://localhost/odata/RefCusTariffUpdate/Default.BatchDelete");
		var batchExpireRequest = CreateHttpRequestMessage(HttpMethod.Post, "http://localhost/odata/RefCusTariffUpdate/Default.BatchExpire");
		var batchInActiveRequest = CreateHttpRequestMessage(HttpMethod.Post, "http://localhost/odata/RefCusTariffUpdate/Default.BatchInActive");
		var cloneRecordRequest = CreateHttpRequestMessage(HttpMethod.Post, "http://localhost/odata/RefCusTariffUpdate/Default.CloneExistingRecordChildrenIntoNewRecord");

		return [removeInactiveClientsRequest, postRequest, putRequest, patchRequest, deleteRequest, forceDeleteRequest, batchDeleteRequest, batchInActiveRequest, batchExpireRequest, cloneRecordRequest];
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "used in test")]
	HttpContent CreateBatchRequestContent()
	{
		var changesetContent = new MultipartContent("mixed", "changeset_825d-598d-f1dc");
		var request1 = CreateHttpRequestMessage(HttpMethod.Post, "http://localhost/odata/RefCusTariffUpdate");
		var content1 = new HttpMessageContent(request1);
		content1.Headers.ContentType = MediaTypeHeaderValue.Parse("application/http");
		content1.Headers.Add("Content-Transfer-Encoding", "binary");
		changesetContent.Add(content1);

		var request2 = CreateHttpRequestMessage(HttpMethod.Put, "http://localhost/odata/RefCusTariffUpdate(9B79A9DB-3BCB-4757-9338-B0DD5EF2CAF2)");
		var content2 = new HttpMessageContent(request2);
		content2.Headers.ContentType = MediaTypeHeaderValue.Parse("application/http");
		content2.Headers.Add("Content-Transfer-Encoding", "binary");
		changesetContent.Add(content2);

		return changesetContent;
	}

	HttpRequestMessage CreateHttpRequestMessage(HttpMethod method, string url)
	{
		var request = new HttpRequestMessage(method, new Uri(url));
		request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json", 0.9D));
		request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.1D));
		request.Content = new StringContent(JsonSerializer.Serialize(tariff), Encoding.UTF8, "application/json");
		request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
		return request;
	}

	readonly RefCusTariff tariff = new RefCusTariff
	{
		ZZ1_PK = Guid.Parse("9B79A9DB-3BCB-4757-9338-B0DD5EF2CAF2"),
		ZZ1_ZZI_TariffType = Guid.Empty,
		ZZ1_TariffCode = "001001",
		ZZ1_Description = "Description",
		ZZ1_ZZZ_NKDataGrouping = "ZZ",
		ZZ1_StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
		ZZ1_EndDate = new DateTime(2024, 12, 31, 0, 0, 0, DateTimeKind.Utc)
	};
}
